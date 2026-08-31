/**
 * @vitest-environment jsdom
 *
 * jsdom rather than the project-wide happy-dom: these tests drive a real HTTP
 * server, and that needs Node's own fetch, which the jsdom environment leaves
 * in place. happy-dom substitutes its own client and never reaches the socket.
 */
import { createServer } from 'node:http';
import type { IncomingMessage, Server } from 'node:http';
import { afterAll, beforeAll, beforeEach, describe, expect, it } from 'vitest';
import { ApiError } from './problemDetails';
import { post } from './client';
import * as tokenStorage from '../auth/tokenStorage';

/**
 * A real HTTP server rather than a replaced fetch. The point of these tests is
 * what actually goes over the wire — the Authorization header, the multipart
 * boundary, the status handling — and a stubbed fetch would only prove that the
 * stub was called.
 *
 * The port matches test.env.VITE_API_BASE_URL in vite.config.ts, which
 * api/client.ts reads once at module load.
 */
const PORT = 5099;

interface Received {
  method: string;
  url: string;
  headers: IncomingMessage['headers'];
  body: string;
}

let server: Server;
let received: Received | null = null;
let respond: (path: string) => { status: number; type?: string; body?: string };

beforeAll(
  () =>
    new Promise<void>((resolve) => {
      server = createServer((req, res) => {
        const chunks: Buffer[] = [];
        req.on('data', (chunk: Buffer) => chunks.push(chunk));
        req.on('end', () => {
          received = {
            method: req.method ?? '',
            url: req.url ?? '',
            headers: req.headers,
            body: Buffer.concat(chunks).toString(),
          };

          const reply = respond(req.url ?? '');
          res.writeHead(reply.status, reply.type === undefined ? {} : { 'Content-Type': reply.type });
          res.end(reply.body);
        });
      });
      server.listen(PORT, '127.0.0.1', resolve);
    }),
);

afterAll(
  () =>
    new Promise<void>((resolve) => {
      server.close(() => {
        resolve();
      });
    }),
);

beforeEach(() => {
  received = null;
  window.sessionStorage.clear();
  tokenStorage.clear();
  respond = () => ({ status: 200, type: 'application/json', body: '{"ok":true}' });
});

const SESSION = {
  token: 'test.jwt.token',
  user: { storeId: 'demo-store', displayName: 'Store Associate', station: 'Station #4' },
};

describe('sending a request', () => {
  it('posts JSON to the configured base URL', async () => {
    await post({ path: '/api/auth/login', body: { storeId: 'demo-store' }, authenticated: false });

    expect(received?.method).toBe('POST');
    expect(received?.url).toBe('/api/auth/login');
    expect(received?.headers['content-type']).toBe('application/json');
    expect(received?.body).toBe('{"storeId":"demo-store"}');
  });

  it('sends no Authorization header on an unauthenticated call', async () => {
    tokenStorage.write(SESSION);

    await post({ path: '/api/auth/login', body: {}, authenticated: false });

    expect(received?.headers.authorization).toBeUndefined();
  });

  it('attaches the stored token as a bearer on an authenticated call', async () => {
    tokenStorage.write(SESSION);

    await post({ path: '/api/inspection/scan', body: {}, authenticated: true });

    expect(received?.headers.authorization).toBe('Bearer test.jwt.token');
  });

  /**
   * Only string parts here. jsdom and undici cannot exchange a File in either
   * direction — a jsdom File hangs undici's fetch, and jsdom's FormData
   * stringifies a node:buffer File to "[object File]". What this test pins down
   * is client.ts's own behaviour: it must not set Content-Type itself, or the
   * boundary would be lost. The file part travelling correctly is covered
   * end to end in the browser, against a real multipart parser.
   */
  it('lets the platform set the multipart Content-Type so the boundary survives', async () => {
    tokenStorage.write(SESSION);
    const form = new FormData();
    form.append('FruitType', 'Banana');

    await post({ path: '/api/inspection/scan', body: form, authenticated: true });

    expect(received?.headers['content-type']).toMatch(/^multipart\/form-data; boundary=/);
    expect(received?.body).toContain('name="FruitType"');
    expect(received?.body).toContain('Banana');
  });

  it('returns the parsed response body', async () => {
    respond = () => ({ status: 200, type: 'application/json', body: '{"token":"abc"}' });

    const result = await post<{ token: string }>({ path: '/x', body: {}, authenticated: false });

    expect(result.token).toBe('abc');
  });
});

describe('authentication guards', () => {
  it('fails without sending when an authenticated call has no token', async () => {
    const error = await post({ path: '/api/inspection/scan', body: {}, authenticated: true }).catch(
      (cause: unknown) => cause,
    );

    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).status).toBe(401);
    expect(received).toBeNull();
  });

  it('clears the session when an authenticated call is rejected as 401', async () => {
    tokenStorage.write(SESSION);
    respond = () => ({ status: 401 });

    await post({ path: '/api/inspection/scan', body: {}, authenticated: true }).catch(() => null);

    expect(tokenStorage.getSnapshot()).toBeNull();
  });

  /**
   * The distinction that makes a mistyped password different from an expired
   * token: /api/auth/login answers 401 for bad credentials, and that must not
   * look like a session ending.
   */
  it('keeps the session when an unauthenticated call is rejected as 401', async () => {
    tokenStorage.write(SESSION);
    respond = () => ({ status: 401 });

    await post({ path: '/api/auth/login', body: {}, authenticated: false }).catch(() => null);

    expect(tokenStorage.getSnapshot()).toEqual(SESSION);
  });
});

describe('failure translation', () => {
  it('turns a problem+json error into an ApiError with its traceId', async () => {
    respond = () => ({
      status: 502,
      type: 'application/problem+json',
      body: JSON.stringify({ title: 'Vision analysis unavailable', traceId: 'trace-1' }),
    });

    const error = (await post({ path: '/x', body: {}, authenticated: false }).catch(
      (cause: unknown) => cause,
    )) as ApiError;

    expect(error.title).toBe('Vision analysis unavailable');
    expect(error.traceId).toBe('trace-1');
  });

  it('reports a success response whose body is not JSON', async () => {
    respond = () => ({ status: 200, type: 'application/json', body: 'not json at all' });

    const error = (await post({ path: '/x', body: {}, authenticated: false }).catch(
      (cause: unknown) => cause,
    )) as ApiError;

    expect(error.title).toBe('Unreadable response');
    expect(error.status).toBe(200);
  });
});
