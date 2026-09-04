import { Component } from 'react';
import type { ErrorInfo, ReactNode } from 'react';
import { Button, Group, Stack } from '@mantine/core';
import { asApiError } from '../api/problemDetails';
import type { ApiError } from '../api/problemDetails';
import { ProblemAlert } from './ProblemAlert';

interface ErrorBoundaryProps {
  children: ReactNode;
}

interface ErrorBoundaryState {
  error: ApiError | null;
}

/**
 * The last line of the app's rule that failures must be visible.
 *
 * React unmounts the entire tree when a render throws, which leaves a blank
 * white page — the one failure mode that tells the operator nothing at all and
 * cannot be distinguished from a broken build. This catches the throw and shows
 * it in the same alert the API errors use, labelled as coming from the client
 * rather than the server.
 */
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  override state: ErrorBoundaryState = { error: null };

  static getDerivedStateFromError(cause: unknown): ErrorBoundaryState {
    return { error: asApiError(cause) };
  }

  override componentDidCatch(cause: unknown, info: ErrorInfo) {
    // The alert carries the message; the console keeps the component stack,
    // which is what actually locates the bug.
    console.error('Render failed:', cause, info.componentStack);
  }

  private readonly retry = () => {
    this.setState({ error: null });
  };

  override render() {
    const { error } = this.state;

    if (error === null) {
      return this.props.children;
    }

    return (
      <Stack gap="md">
        <ProblemAlert error={error} />
        <Group>
          <Button variant="default" onClick={this.retry}>
            Try again
          </Button>
          <Button
            variant="subtle"
            onClick={() => {
              window.location.reload();
            }}
          >
            Reload the app
          </Button>
        </Group>
      </Stack>
    );
  }
}
