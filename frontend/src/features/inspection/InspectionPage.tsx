import { Alert, Container, Stack, Text, Title } from '@mantine/core';
import { useAuth } from '../../auth/useAuth';

/**
 * Placeholder for the scan screen. Step 5 replaces the body with the fruit
 * selector, the image dropzone and the result card; the route, the guard and
 * the session around it are already real.
 */
export function InspectionPage() {
  const { session } = useAuth();

  return (
    <Container size="sm">
      <Stack gap="md">
        <Title order={2}>Inspection</Title>

        <Alert variant="light" title="Signed in against the live API">
          <Text size="sm">
            The token below came from POST /api/auth/login and is attached to every
            authenticated request. Scanning arrives in the next step.
          </Text>
        </Alert>

        <Text size="sm" c="dimmed">
          Operator {session?.user.displayName} at {session?.user.station}.
        </Text>
      </Stack>
    </Container>
  );
}
