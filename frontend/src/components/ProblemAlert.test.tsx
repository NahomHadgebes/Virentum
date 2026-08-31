import { render, screen } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { describe, expect, it } from 'vitest';
import { ApiError, toApiError } from '../api/problemDetails';
import { ProblemAlert } from './ProblemAlert';

function show(error: ApiError, handledFields?: readonly string[]) {
  render(
    <MantineProvider>
      <ProblemAlert error={error} {...(handledFields === undefined ? {} : { handledFields })} />
    </MantineProvider>,
  );
}

function problem(status: number, body: unknown): Promise<ApiError> {
  return toApiError(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/problem+json' },
    }),
  );
}

describe('ProblemAlert', () => {
  it('shows the title, the detail and the traceId the API sent', async () => {
    show(
      await problem(401, {
        title: 'Authentication failed',
        detail: 'Invalid store id or password.',
        traceId: '00-trace-01',
      }),
    );

    expect(screen.getByText('Authentication failed')).toBeDefined();
    expect(screen.getByText('Invalid store id or password.')).toBeDefined();
    expect(screen.getByText('00-trace-01')).toBeDefined();
    expect(screen.getByRole('alert')).toBeDefined();
  });

  it('says a trace id is missing rather than leaving a blank', async () => {
    show(await toApiError(new Response(null, { status: 401 })));

    expect(screen.getByText(/no trace id in this response/)).toBeDefined();
    expect(screen.getByText(/opaque-response/)).toBeDefined();
  });

  /**
   * The requirement this component exists for: a field the form does not render
   * must still reach the operator instead of disappearing.
   */
  it('lists field errors the surrounding form does not handle', async () => {
    show(
      await problem(400, {
        title: 'One or more validation errors occurred.',
        errors: { StoreId: ['Required.'], Captcha: ['Missing token.'] },
      }),
      ['storeId'],
    );

    expect(screen.getByText(/Missing token\./)).toBeDefined();
    expect(screen.queryByText(/Required\./)).toBeNull();
  });

  it('lists every field error when the form handles none', async () => {
    show(await problem(400, { title: 'Bad', errors: { StoreId: ['Required.'] } }));

    expect(screen.getByText(/Required\./)).toBeDefined();
  });

  it('renders the status alongside the trace id', async () => {
    show(await problem(502, { title: 'Vision analysis unavailable', traceId: 'trace-502' }));

    expect(screen.getByText(/HTTP 502/)).toBeDefined();
    expect(screen.getByText('trace-502')).toBeDefined();
  });

  it('omits the detail line entirely when the API sent none', async () => {
    show(await problem(500, { title: 'An unexpected error occurred' }));

    expect(screen.getByText('An unexpected error occurred')).toBeDefined();
    expect(screen.queryByText(/undefined|null/)).toBeNull();
  });
});
