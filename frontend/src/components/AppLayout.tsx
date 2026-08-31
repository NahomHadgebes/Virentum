import { AppShell, Button, Group, Stack, Text, Title } from '@mantine/core';
import type { ReactNode } from 'react';
import { useAuth } from '../auth/useAuth';
import { ColorSchemeToggle } from './ColorSchemeToggle';

/** Chrome for authenticated routes: who is signed in, and a way out. */
export function AppLayout({ children }: { children: ReactNode }) {
  const { session, signOut } = useAuth();

  return (
    <AppShell header={{ height: 64 }} padding="md">
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap" gap="sm">
          <Title order={3} style={{ flexShrink: 0 }}>
            Virentum
          </Title>

          <Group gap="sm" wrap="nowrap" style={{ minWidth: 0 }}>
            {session !== null && (
              // minWidth:0 lets the flex child shrink so truncate can engage;
              // without it the button gets squeezed instead of the text.
              <Stack gap={0} align="flex-end" style={{ minWidth: 0 }} visibleFrom="xs">
                <Text size="sm" fw={600} lh={1.2} truncate="end" maw={180}>
                  {session.user.displayName}
                </Text>
                <Text size="xs" c="dimmed" lh={1.2} truncate="end" maw={180}>
                  {session.user.station} · {session.user.storeId}
                </Text>
              </Stack>
            )}

            <ColorSchemeToggle />

            {session !== null && (
              <Button variant="default" size="xs" onClick={signOut} style={{ flexShrink: 0 }}>
                Sign out
              </Button>
            )}
          </Group>
        </Group>
      </AppShell.Header>

      <AppShell.Main>
        {/* The header hides the operator below xs; it still matters on a shared
            station, so it reappears here. */}
        {session !== null && (
          <Text size="xs" c="dimmed" hiddenFrom="xs" mb="sm">
            {session.user.displayName} · {session.user.station} · {session.user.storeId}
          </Text>
        )}
        {children}
      </AppShell.Main>
    </AppShell>
  );
}
