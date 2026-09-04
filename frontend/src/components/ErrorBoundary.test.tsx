import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { ErrorBoundary } from './ErrorBoundary';

function Exploding(): React.ReactNode {
  throw new TypeError("Cannot read properties of undefined (reading 'split')");
}

describe('ErrorBoundary', () => {
  beforeEach(() => {
    // React logs the caught error itself; the boundary adds the component
    // stack. Neither is a test failure, but both would drown the report.
    vi.spyOn(console, 'error').mockImplementation(() => undefined);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('shows a failing render instead of leaving the page blank', () => {
    render(
      <MantineProvider>
        <ErrorBoundary>
          <Exploding />
        </ErrorBoundary>
      </MantineProvider>,
    );

    const alert = screen.getByRole('alert');

    expect(alert.textContent).toContain('Unexpected error in the Virentum client');
    expect(alert.textContent).toContain("reading 'split'");
    expect(screen.getByRole('button', { name: 'Try again' })).toBeTruthy();
  });

  it('renders its children untouched when nothing throws', () => {
    render(
      <MantineProvider>
        <ErrorBoundary>
          <p>All good</p>
        </ErrorBoundary>
      </MantineProvider>,
    );

    expect(screen.getByText('All good')).toBeTruthy();
    expect(screen.queryByRole('alert')).toBeNull();
  });
});
