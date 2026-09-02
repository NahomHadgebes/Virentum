import { act, render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ApiError, toApiError } from './problemDetails';
import { useApiResource } from './useApiResource';

/**
 * Driven through a real component rather than a hook harness, because what
 * matters is what a view ends up rendering in each of the three states.
 */
function Probe({ load }: { load: () => Promise<string> }) {
  const { data, error, loading, reload } = useApiResource(load);

  return (
    <div>
      <span data-testid="state">
        {loading ? 'loading' : error !== null ? `error:${error.title}` : `data:${data ?? 'none'}`}
      </span>
      <button type="button" onClick={reload}>
        reload
      </button>
    </div>
  );
}

const state = () => screen.getByTestId('state').textContent;

describe('useApiResource', () => {
  it('starts in the loading state before anything resolves', () => {
    render(<Probe load={() => new Promise(() => undefined)} />);

    expect(state()).toBe('loading');
  });

  it('exposes the loaded value', async () => {
    render(<Probe load={() => Promise.resolve('ok')} />);

    await waitFor(() => {
      expect(state()).toBe('data:ok');
    });
  });

  it('exposes a failure as an ApiError rather than as absent data', async () => {
    const load = () => toApiError(new Response(null, { status: 401 })).then((e) => Promise.reject(e));

    render(<Probe load={load} />);

    await waitFor(() => {
      expect(state()).toBe('error:Not authenticated');
    });
  });

  it('wraps a non-ApiError throw so the view has one type to render', async () => {
    render(<Probe load={() => Promise.reject(new Error('boom'))} />);

    await waitFor(() => {
      expect(state()).toBe('error:Unexpected error in the Virentum client');
    });
  });

  it('drops stale data when a reload fails', async () => {
    let attempt = 0;
    const load = () => {
      attempt += 1;
      return attempt === 1
        ? Promise.resolve('first')
        : Promise.reject(new ApiError({
            source: 'network',
            status: null,
            title: 'Could not reach the Virentum API',
            detail: null,
            traceId: null,
            fieldErrors: null,
          }));
    };

    render(<Probe load={load} />);
    await waitFor(() => {
      expect(state()).toBe('data:first');
    });

    act(() => {
      screen.getByRole('button', { name: 'reload' }).click();
    });

    await waitFor(() => {
      expect(state()).toBe('error:Could not reach the Virentum API');
    });
  });

  it('refetches when reload is pressed', async () => {
    const load = vi.fn(() => Promise.resolve('ok'));

    render(<Probe load={load} />);
    await waitFor(() => {
      expect(state()).toBe('data:ok');
    });

    act(() => {
      screen.getByRole('button', { name: 'reload' }).click();
    });

    await waitFor(() => {
      expect(load).toHaveBeenCalledTimes(2);
    });
  });

  /**
   * The reason results are tagged with the request that produced them: a slow
   * response for an abandoned query must never replace a newer one.
   */
  it('ignores a response that arrives after a newer request settled', async () => {
    let resolveSlow: ((value: string) => void) | null = null;
    const slow = () => new Promise<string>((resolve) => { resolveSlow = resolve; });

    const { rerender } = render(<Probe load={slow} />);
    expect(state()).toBe('loading');

    rerender(<Probe load={() => Promise.resolve('newer')} />);
    await waitFor(() => {
      expect(state()).toBe('data:newer');
    });

    await act(async () => {
      resolveSlow?.('stale');
      await Promise.resolve();
    });

    expect(state()).toBe('data:newer');
  });
});
