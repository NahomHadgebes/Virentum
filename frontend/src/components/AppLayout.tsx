import { AppShell, Button, Group, Stack, Text, Title } from '@mantine/core';
import type { ReactNode } from 'react';
import { useAuth } from '../auth/useAuth';

/** Chrome for authenticated routes: who is signed in, and a way out. */
export function AppLayout({ children }: { children: ReactNode }) {
  const { session, signOut } = useAuth();

  return (
    <AppShell header={{ height: 64 }} padding="md">
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap">
          <Title order={3}>Virentum</Title>

          {session !== null && (
            <Group gap="md" wrap="nowrap">
              <Stack gap={0} align="flex-end">
                <Text size="sm" fw={600} lh={1.2}>
                  {session.user.displayName}
                </Text>
                <Text size="xs" c="dimmed" lh={1.2}>
                  {session.user.station} · {session.user.storeId}
                </Text>
              </Stack>
              <Button variant="default" size="xs" onClick={signOut}>
                Sign out
              </Button>
            </Group>
          )}
        </Group>
      </AppShell.Header>

      <AppShell.Main>{children}</AppShell.Main>
    </AppShell>
  );
}
