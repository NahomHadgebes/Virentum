/**
 * The RFC 7807 layer.
 *
 * The API answers failures in three distinguishable shapes, and this module
 * keeps them distinguishable instead of collapsing them into one vague error:
 *
 *  1. `ProblemDetails` written by Middleware/GlobalExceptionHandler.cs, always
 *     carrying a `traceId` extension.
 *  2. `ValidationProblemDetails` produced automatically by [ApiController] when
 *     model binding or DataAnnotations fail — same shape plus an `errors` map.
 *  3. A body-less error response. Program.cs never calls UseStatusCodePages, so
 *     the JWT bearer challenge returns 401 with an empty body and no traceId.
 *
 * A fourth case never reaches the server at all: the fetch itself failing.
 */

/** Shape of a problem+json body as written by the API. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
}

/** ProblemDetails plus the ModelState errors from [ApiController]. */
export interface ValidationProblemDetails extends ProblemDetails {
  errors: Record<string, string[]>;
}

/** Where an ApiError's information came from — never guessed. */
export type ApiErrorSource =
  | 'problem-details'
  | 'opaque-response'
  | 'network'
  | 'client';

/**
 * The single error type the UI deals with. Every field that the server did not
 * actually provide stays `null`; nothing is substituted with placeholder text.
 */
export class ApiError extends Error {
  readonly source: ApiErrorSource;
  /** HTTP status, or null when the request never got a response. */
  readonly status: number | null;
  readonly title: string;
  readonly detail: string | null;
  readonly traceId: string | null;
  readonly fieldErrors: Record<string, string[]> | null;

  constructor(init: {
    source: ApiErrorSource;
    status: number | null;
    title: string;
    detail: string | null;
    traceId: string | null;
    fieldErrors: Record<string, string[]> | null;
  }) {
    super(init.detail ?? init.title);
    this.name = 'ApiError';
    this.source = init.source;
    this.status = init.status;
    this.title = init.title;
    this.detail = init.detail;
    this.traceId = init.traceId;
    this.fieldErrors = init.fieldErrors;
  }

  /**
   * Validation messages for one client-side field name.
   *
   * ModelState keys arrive PascalCased (`StoreId`) and JSON binding failures
   * arrive as JSON paths (`$.storeId`), so the match is case-insensitive on the
   * final path segment.
   */
  errorsFor(field: string): string[] {
    if (this.fieldErrors === null) {
      return [];
    }

    const wanted = field.toLowerCase();

    return Object.entries(this.fieldErrors)
      .filter(([key]) => lastSegment(key).toLowerCase() === wanted)
      .flatMap(([, messages]) => messages);
  }

  /**
   * Validation messages for fields the caller is NOT rendering itself.
   *
   * A form binds the keys it knows about to its own inputs; this returns
   * everything left over, so an unexpected ModelState key is still shown
   * somewhere instead of disappearing.
   */
  fieldErrorsExcept(handled: readonly string[]): { field: string; messages: string[] }[] {
    if (this.fieldErrors === null) {
      return [];
    }

    const handledSet = new Set(handled.map((field) => field.toLowerCase()));

    return Object.entries(this.fieldErrors)
      .filter(([key]) => !handledSet.has(lastSegment(key).toLowerCase()))
      .map(([field, messages]) => ({ field, messages }));
  }
}

function lastSegment(key: string): string {
  const segments = key.split('.');
  return segments[segments.length - 1] ?? key;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

/** True when the parsed body carries at least one Problem Details member. */
function isProblemDetails(value: unknown): value is ProblemDetails {
  if (!isRecord(value)) {
    return false;
  }

  return (
    typeof value['title'] === 'string' ||
    typeof value['detail'] === 'string' ||
    typeof value['status'] === 'number'
  );
}

/** Reads `errors` only when it really is a map of string arrays. */
function readFieldErrors(value: Record<string, unknown>): Record<string, string[]> | null {
  const errors = value['errors'];
  if (!isRecord(errors)) {
    return null;
  }

  const entries = Object.entries(errors).filter(
    (entry): entry is [string, string[]] =>
      Array.isArray(entry[1]) && entry[1].every((message) => typeof message === 'string'),
  );

  return entries.length > 0 ? Object.fromEntries(entries) : null;
}

/**
 * Turns a failed response into an ApiError, reading the body only when the
 * server said it is problem+json (or plain json) and it actually parses.
 */
export async function toApiError(response: Response): Promise<ApiError> {
  const body = await readJsonBody(response);

  if (isProblemDetails(body)) {
    return new ApiError({
      source: 'problem-details',
      status: response.status,
      title: body.title ?? describeStatus(response.status),
      detail: body.detail ?? null,
      traceId: typeof body.traceId === 'string' ? body.traceId : null,
      fieldErrors: readFieldErrors(body as Record<string, unknown>),
    });
  }

  return new ApiError({
    source: 'opaque-response',
    status: response.status,
    title: describeStatus(response.status),
    detail: opaqueDetail(response.status),
    traceId: null,
    fieldErrors: null,
  });
}

/** A fetch rejection: DNS, TLS, CORS or the API not running. */
export function toNetworkError(cause: unknown): ApiError {
  return new ApiError({
    source: 'network',
    status: null,
    title: 'Could not reach the Virentum API',
    detail:
      cause instanceof Error
        ? `The request failed before the server answered: ${cause.message}`
        : 'The request failed before the server answered.',
    traceId: null,
    fieldErrors: null,
  });
}

/**
 * A failure raised inside the browser app rather than by the API. Labelled as
 * such so the UI never presents a frontend bug as if the server had reported it.
 */
export function toClientError(cause: unknown): ApiError {
  return new ApiError({
    source: 'client',
    status: null,
    title: 'Unexpected error in the Virentum client',
    detail: cause instanceof Error ? cause.message : String(cause),
    traceId: null,
    fieldErrors: null,
  });
}

/** Narrows an unknown throw to the single error type the UI renders. */
export function asApiError(cause: unknown): ApiError {
  return cause instanceof ApiError ? cause : toClientError(cause);
}

async function readJsonBody(response: Response): Promise<unknown> {
  const contentType = response.headers.get('content-type') ?? '';
  if (!/\bapplication\/(problem\+)?json\b/i.test(contentType)) {
    return null;
  }

  try {
    return await response.json();
  } catch {
    // A malformed body is not worth surfacing as its own failure mode; the
    // caller falls back to describing the status code.
    return null;
  }
}

/** A factual name for a status code, used only when the body gave no title. */
function describeStatus(status: number): string {
  switch (status) {
    case 400:
      return 'Invalid request';
    case 401:
      return 'Not authenticated';
    case 403:
      return 'Not authorised';
    case 404:
      return 'Endpoint not found';
    case 413:
      return 'Payload too large';
    case 422:
      return 'Request could not be processed';
    case 502:
      return 'Upstream service unavailable';
    default:
      return `Request failed with HTTP ${String(status)}`;
  }
}

/**
 * What we can honestly say about an error response that carried no body. The
 * 401 case is the one the API actually produces, from the bearer challenge.
 */
function opaqueDetail(status: number): string {
  return status === 401
    ? 'The API rejected the request without a response body. The access token is missing, expired or invalid.'
    : 'The API returned an error without a response body, so no trace id is available.';
}
