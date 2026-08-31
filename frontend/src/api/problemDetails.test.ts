import { describe, expect, it } from 'vitest';
import { ApiError, asApiError, toApiError, toClientError, toNetworkError } from './problemDetails';

function problemResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  });
}

describe('toApiError — RFC 7807 bodies from GlobalExceptionHandler', () => {
  it('carries title, detail and traceId through unchanged', async () => {
    const error = await toApiError(
      problemResponse(401, {
        title: 'Authentication failed',
        status: 401,
        detail: 'Invalid store id or password.',
        instance: '/api/auth/login',
        traceId: '00-abc-def-00',
      }),
    );

    expect(error.source).toBe('problem-details');
    expect(error.status).toBe(401);
    expect(error.title).toBe('Authentication failed');
    expect(error.detail).toBe('Invalid store id or password.');
    expect(error.traceId).toBe('00-abc-def-00');
    expect(error.fieldErrors).toBeNull();
  });

  it('keeps detail null rather than inventing text when the body omits it', async () => {
    const error = await toApiError(problemResponse(502, { title: 'Vision analysis unavailable' }));

    expect(error.detail).toBeNull();
    expect(error.title).toBe('Vision analysis unavailable');
  });

  it('falls back to a status description only when the body has no title', async () => {
    const error = await toApiError(problemResponse(422, { status: 422, detail: 'No processor.' }));

    expect(error.title).toBe('Request could not be processed');
    expect(error.detail).toBe('No processor.');
  });
});

describe('toApiError — ValidationProblemDetails from [ApiController]', () => {
  const validation = () =>
    problemResponse(400, {
      title: 'One or more validation errors occurred.',
      status: 400,
      traceId: 'trace-400',
      errors: {
        StoreId: ['The StoreId field is required.'],
        Password: ['Too short.', 'Also bad.'],
      },
    });

  it('exposes the ModelState errors', async () => {
    const error = await toApiError(validation());

    expect(error.fieldErrors).toEqual({
      StoreId: ['The StoreId field is required.'],
      Password: ['Too short.', 'Also bad.'],
    });
  });

  it('matches PascalCase server keys to camelCase client fields', async () => {
    const error = await toApiError(validation());

    expect(error.errorsFor('storeId')).toEqual(['The StoreId field is required.']);
    expect(error.errorsFor('password')).toEqual(['Too short.', 'Also bad.']);
  });

  it('matches JSON path keys on their final segment', async () => {
    const error = await toApiError(
      problemResponse(400, { title: 'Bad', errors: { '$.storeId': ['Invalid.'] } }),
    );

    expect(error.errorsFor('storeId')).toEqual(['Invalid.']);
  });

  it('returns nothing for a field the server did not flag', async () => {
    const error = await toApiError(validation());

    expect(error.errorsFor('station')).toEqual([]);
  });

  it('ignores an errors value that is not a map of string arrays', async () => {
    const error = await toApiError(problemResponse(400, { title: 'Bad', errors: 'nope' }));

    expect(error.fieldErrors).toBeNull();
  });
});

describe('fieldErrorsExcept — nothing the server reported gets hidden', () => {
  it('returns the keys the form does not render itself', async () => {
    const error = await toApiError(
      problemResponse(400, {
        title: 'Bad',
        errors: { StoreId: ['Required.'], Captcha: ['Missing.'] },
      }),
    );

    expect(error.fieldErrorsExcept(['storeId'])).toEqual([
      { field: 'Captcha', messages: ['Missing.'] },
    ]);
  });

  it('returns everything when the form handles nothing', async () => {
    const error = await toApiError(
      problemResponse(400, { title: 'Bad', errors: { StoreId: ['Required.'] } }),
    );

    expect(error.fieldErrorsExcept([])).toHaveLength(1);
  });

  it('is empty when there were no field errors at all', async () => {
    const error = await toApiError(problemResponse(500, { title: 'Boom' }));

    expect(error.fieldErrorsExcept([])).toEqual([]);
  });
});

describe('toApiError — responses with no usable body', () => {
  /**
   * Program.cs never calls UseStatusCodePages, so the JWT bearer challenge
   * returns 401 with an empty body and no traceId. Reading it blindly as JSON
   * would throw.
   */
  it('describes the body-less 401 challenge without claiming a traceId', async () => {
    const error = await toApiError(new Response(null, { status: 401 }));

    expect(error.source).toBe('opaque-response');
    expect(error.status).toBe(401);
    expect(error.title).toBe('Not authenticated');
    expect(error.traceId).toBeNull();
    expect(error.detail).toContain('without a response body');
  });

  it('does not parse a body the server did not label as json', async () => {
    const error = await toApiError(
      new Response('<html>502 Bad Gateway</html>', {
        status: 502,
        headers: { 'Content-Type': 'text/html' },
      }),
    );

    expect(error.source).toBe('opaque-response');
    expect(error.title).toBe('Upstream service unavailable');
  });

  it('survives a malformed body that claims to be json', async () => {
    const error = await toApiError(
      new Response('{ not json', {
        status: 500,
        headers: { 'Content-Type': 'application/json' },
      }),
    );

    expect(error.source).toBe('opaque-response');
    expect(error.title).toBe('Request failed with HTTP 500');
  });

  it('names an unmapped status rather than guessing at its meaning', async () => {
    const error = await toApiError(new Response(null, { status: 418 }));

    expect(error.title).toBe('Request failed with HTTP 418');
  });
});

describe('failures that never reached the server', () => {
  it('marks a fetch rejection as a network failure with no status', () => {
    const error = toNetworkError(new TypeError('Failed to fetch'));

    expect(error.source).toBe('network');
    expect(error.status).toBeNull();
    expect(error.detail).toContain('Failed to fetch');
  });

  it('labels an error thrown inside the client as a client fault', () => {
    const error = toClientError(new Error('render blew up'));

    expect(error.source).toBe('client');
    expect(error.title).toBe('Unexpected error in the Virentum client');
    expect(error.detail).toBe('render blew up');
  });

  it('asApiError passes an ApiError through untouched', () => {
    const original = toNetworkError(new Error('down'));

    expect(asApiError(original)).toBe(original);
  });

  it('asApiError wraps anything else so the UI has one type to render', () => {
    expect(asApiError('a bare string')).toBeInstanceOf(ApiError);
    expect(asApiError('a bare string').source).toBe('client');
  });
});

describe('ApiError as an Error', () => {
  it('uses detail as the message when there is one', () => {
    const error = toClientError(new Error('boom'));

    expect(error.message).toBe('boom');
    expect(error).toBeInstanceOf(Error);
  });

  it('falls back to the title when there is no detail', async () => {
    const error = await toApiError(problemResponse(500, { title: 'Server exploded' }));

    expect(error.message).toBe('Server exploded');
  });
});
