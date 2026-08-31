import { Alert, Code, List, Stack, Text } from '@mantine/core';
import type { ApiError } from '../api/problemDetails';

interface ProblemAlertProps {
  error: ApiError;
  /**
   * Field names the surrounding form renders on its own inputs. Anything the
   * API reported outside this list is listed here instead of vanishing.
   */
  handledFields?: readonly string[];
}

export function ProblemAlert({ error, handledFields = [] }: ProblemAlertProps) {
  const unhandled = error.fieldErrorsExcept(handledFields);

  return (
    <Alert color="red" variant="light" title={error.title} role="alert">
      <Stack gap="xs">
        {error.detail !== null && <Text size="sm">{error.detail}</Text>}

        {unhandled.length > 0 && (
          <List size="sm" withPadding>
            {unhandled.map(({ field, messages }) => (
              <List.Item key={field}>
                <Text span fw={600} size="sm">
                  {field}:
                </Text>{' '}
                {messages.join(' ')}
              </List.Item>
            ))}
          </List>
        )}

        <Text size="xs" c="dimmed">
          {error.status !== null && <>HTTP {error.status} · </>}
          {error.traceId !== null ? (
            <>
              trace id <Code>{error.traceId}</Code>
            </>
          ) : (
            <>no trace id in this response ({error.source})</>
          )}
        </Text>
      </Stack>
    </Alert>
  );
}
