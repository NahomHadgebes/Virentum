/**
 * Runtime checks at the API boundary.
 *
 * types/contracts.ts states what the API sends, and client.ts casts the parsed
 * body to it without looking. When the two disagree — an older API still
 * running, a field renamed on one side only — the mismatch surfaces much later,
 * deep inside a component, as "cannot read properties of undefined". That names
 * neither the field nor the endpoint, and it takes the whole page down with it.
 *
 * These helpers raise an ApiError at the moment the response is read, naming
 * the path that did not match. Nothing is defaulted and nothing is filled in: a
 * body that does not match the contract is a failure, not a thinner success.
 */
import { ApiError } from './problemDetails';

/** Where a mismatch was found, e.g. `/api/fruits $[0].bands[2].stageName`. */
function violation(endpoint: string, path: string, expected: string, actual: unknown): ApiError {
  return new ApiError({
    source: 'client',
    status: null,
    title: 'The API sent a response this app cannot read',
    detail:
      `${endpoint} returned a body that does not match the expected contract: ` +
      `${path} should be ${expected} but was ${describe(actual)}. ` +
      'The frontend and the API are most likely built from different versions.',
    traceId: null,
    fieldErrors: null,
  });
}

function describe(value: unknown): string {
  if (value === undefined) {
    return 'missing';
  }
  if (value === null) {
    return 'null';
  }
  if (Array.isArray(value)) {
    return 'an array';
  }
  return typeof value === 'string' ? `the string ${JSON.stringify(value)}` : typeof value;
}

/**
 * A reader bound to one endpoint, so every message it raises says which call
 * produced the body.
 */
export class ContractReader {
  private readonly endpoint: string;

  constructor(endpoint: string) {
    this.endpoint = endpoint;
  }

  array(value: unknown, path: string): unknown[] {
    if (!Array.isArray(value)) {
      throw violation(this.endpoint, path, 'an array', value);
    }
    return value;
  }

  object(value: unknown, path: string): Record<string, unknown> {
    if (typeof value !== 'object' || value === null || Array.isArray(value)) {
      throw violation(this.endpoint, path, 'an object', value);
    }
    return value as Record<string, unknown>;
  }

  string(source: Record<string, unknown>, field: string, path: string): string {
    const value = source[field];
    if (typeof value !== 'string') {
      throw violation(this.endpoint, `${path}.${field}`, 'a string', value);
    }
    return value;
  }

  number(source: Record<string, unknown>, field: string, path: string): number {
    const value = source[field];
    if (typeof value !== 'number' || !Number.isFinite(value)) {
      throw violation(this.endpoint, `${path}.${field}`, 'a number', value);
    }
    return value;
  }

  boolean(source: Record<string, unknown>, field: string, path: string): boolean {
    const value = source[field];
    if (typeof value !== 'boolean') {
      throw violation(this.endpoint, `${path}.${field}`, 'a boolean', value);
    }
    return value;
  }

  /** A number the API may legitimately omit by sending null. */
  nullableNumber(source: Record<string, unknown>, field: string, path: string): number | null {
    const value = source[field];
    if (value === null) {
      return null;
    }
    return this.number(source, field, path);
  }

  /**
   * One member of a string enum. The backend serialises enums by name, so an
   * unknown name means the API knows a member this build does not — which the
   * UI cannot colour, word or sort, and must not silently drop.
   */
  member<T extends string>(
    source: Record<string, unknown>,
    field: string,
    allowed: readonly T[],
    path: string,
  ): T {
    const value = source[field];
    if (typeof value !== 'string' || !(allowed as readonly string[]).includes(value)) {
      throw violation(this.endpoint, `${path}.${field}`, `one of ${allowed.join(', ')}`, value);
    }
    return value as T;
  }

  strings(source: Record<string, unknown>, field: string, path: string): string[] {
    const value = this.array(source[field], `${path}.${field}`);
    return value.map((entry, index) => {
      if (typeof entry !== 'string') {
        throw violation(this.endpoint, `${path}.${field}[${String(index)}]`, 'a string', entry);
      }
      return entry;
    });
  }
}
