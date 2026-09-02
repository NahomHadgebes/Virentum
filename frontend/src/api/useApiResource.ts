import { useCallback, useEffect, useMemo, useState } from 'react';
import { asApiError } from './problemDetails';
import type { ApiError } from './problemDetails';

interface ApiResource<T> {
  data: T | null;
  error: ApiError | null;
  loading: boolean;
  reload: () => void;
}

/** The outcome of one request, tagged with the request it answers. */
interface Settled<T> {
  token: object;
  data: T | null;
  error: ApiError | null;
}

/**
 * Loads one API resource and exposes the three states a caller has to render:
 * in flight, failed, and loaded. Keeping them in one place is what stops a view
 * from quietly treating a failure as "no data".
 *
 * `load` must be memoised by the caller — it is the dependency that decides when
 * to refetch, so an inline arrow would refetch on every render.
 *
 * Loading is derived rather than stored: a result carries the token of the
 * request that produced it, so anything that does not match the current token is
 * by definition still in flight. That also means a slow response for an
 * abandoned filter can never replace a newer one.
 */
export function useApiResource<T>(load: () => Promise<T>): ApiResource<T> {
  const [attempt, setAttempt] = useState(0);
  const [settled, setSettled] = useState<Settled<T> | null>(null);

  const token = useMemo(() => ({ load, attempt }), [load, attempt]);

  const reload = useCallback(() => {
    setAttempt((previous) => previous + 1);
  }, []);

  useEffect(() => {
    let current = true;

    load().then(
      (data) => {
        if (current) {
          setSettled({ token, data, error: null });
        }
      },
      (cause: unknown) => {
        if (current) {
          setSettled({ token, data: null, error: asApiError(cause) });
        }
      },
    );

    return () => {
      current = false;
    };
  }, [token, load]);

  const fresh = settled?.token === token ? settled : null;

  return {
    data: fresh?.data ?? null,
    error: fresh?.error ?? null,
    loading: fresh === null,
    reload,
  };
}
