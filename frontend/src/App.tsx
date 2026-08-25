import { Code, Container, Stack, Text, Title } from '@mantine/core';

/**
 * Placeholder shell. Step 4 replaces this with the router, the auth context
 * and the login / inspection routes.
 */
export function App() {
  return (
    <Container size="sm" py="xl">
      <Stack gap="xs">
        <Title order={1}>Virentum</Title>
        <Text c="dimmed">
          API contract types and transport layer are in place. No screens yet.
        </Text>
        <Text size="sm">
          Configured API: <Code>{import.meta.env.VITE_API_BASE_URL}</Code>
        </Text>
      </Stack>
    </Container>
  );
}
