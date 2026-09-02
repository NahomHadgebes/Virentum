/**
 * The only place in the app that calls fetch.
 *
 * Responsibilities, and nothing else: resolve the base URL, attach the bearer
 * token, translate every failure into an ApiError, and drop the session when
 * the API says the token is no longer good.
 */
import * as tokenStorage from '../auth/tokenStorage';
import { ApiError, toApiError, toNetworkError } from './problemDetails';

const baseUrl = resolveBaseUrl();

/**
 * Read once, at module load. A missing base URL is a configuration bug and the
 * app must fail loudly instead of quietly defaulting to some plausible host.
 */
function resolveBaseUrl(): string {
  const configured = import.meta.env.VITE_API_BASE_URL;

  if (typeof configured !== 'string' || configured.trim() === '') {
    throw new Error(
      'VITE_API_BASE_URL is not set. Copy .env.example to .env and point it at the Virentum API.',
    );
  }

  return configured.trim().replace(/\/+$/, '');
}

interface PostOptions {
  path: string;
  /** JSON is serialised; FormData is passed through untouched. */
  body: unknown;
  /** When true the request carries the bearer token and a 401 ends the session. */
  authenticated: boolean;
}

interface RequestOptions extends PostOptions {
  method: 'GET' | 'POST';
}

export function post<TResponse>(options: PostOptions): Promise<TResponse> {
  return request<TResponse>({ ...options, method: 'POST' });
}

/**
 * Every GET in this API is behind the bearer token, so there is no
 * unauthenticated variant to choose between.
 */
export function get<TResponse>(path: string): Promise<TResponse> {
  return request<TResponse>({ method: 'GET', path, body: null, authenticated: true });
}

async function request<TResponse>(options: RequestOptions): Promise<TResponse> {
  const headers = new Headers();
  let payload: BodyInit | undefined;

  if (options.method === 'GET') {
    // A GET carries no body, and no Content-Type describing one.
    payload = undefined;
  } else if (options.body instanceof FormData) {
    // Content-Type is intentionally unset: the browser must add the multipart
    // boundary itself.
    payload = options.body;
  } else {
    headers.set('Content-Type', 'application/json');
    payload = JSON.stringify(options.body);
  }

  if (options.authenticated) {
    const token = tokenStorage.readToken();

    if (token === null) {
      throw new ApiError({
        source: 'opaque-response',
        status: 401,
        title: 'Not authenticated',
        detail: 'No access token is stored, so the request was not sent.',
        traceId: null,
        fieldErrors: null,
      });
    }

    headers.set('Authorization', `Bearer ${token}`);
  }

  let response: Response;
  try {
    response = await fetch(`${baseUrl}${options.path}`, {
      method: options.method,
      headers,
      ...(payload === undefined ? {} : { body: payload }),
    });
  } catch (cause) {
    throw toNetworkError(cause);
  }

  if (!response.ok) {
    // Only an authenticated call can have its token rejected. A 401 from
    // /api/auth/login means the credentials were wrong, not that a session
    // expired, so it must not clear anything.
    if (response.status === 401 && options.authenticated) {
      tokenStorage.clear();
    }

    throw await toApiError(response);
  }

  return await readJson<TResponse>(response);
}

/** A success response that is not readable JSON is a contract violation. */
async function readJson<TResponse>(response: Response): Promise<TResponse> {
  try {
    return (await response.json()) as TResponse;
  } catch (cause) {
    throw new ApiError({
      source: 'opaque-response',
      status: response.status,
      title: 'Unreadable response',
      detail:
        cause instanceof Error
          ? `The API returned HTTP ${String(response.status)} but the body could not be parsed as JSON: ${cause.message}`
          : `The API returned HTTP ${String(response.status)} with an unreadable body.`,
      traceId: null,
      fieldErrors: null,
    });
  }
}
