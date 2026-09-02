import { AppShell, Box, Group, Menu, Stack, Text, UnstyledButton } from '@mantine/core';
import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';
import { useAudience } from '../audience/useAudience';
import { AppNav } from './AppNav';
import { ColorSchemeToggle } from './ColorSchemeToggle';
import classes from './AppLayout.module.css';

/**
 * Chrome for signed-in routes.
 *
 * Deliberately block flow rather than a flex Stack around the page: a flex item
 * defaults to min-width:auto, so a page containing anything wide — the history
 * table, at 620px — would refuse to shrink and push the layout past the
 * viewport.
 */
export function AppLayout({ children }: { children: ReactNode }) {
  const { session, signOut } = useAuth();
  const { audience, choose } = useAudience();

  return (
    <AppShell header={{ height: 62 }} padding={0}>
      <AppShell.Header className={classes.header}>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap" gap="sm">
          <Link to="/scan" className={classes.brand}>
            <Group gap={8} wrap="nowrap">
              <Box className={classes.mark} aria-hidden />
              <Text fw={700} fz="lg" ff="Fraunces, Georgia, serif" visibleFrom="xs">
                Virentum
              </Text>
            </Group>
          </Link>

          <Group gap="xs" wrap="nowrap">
            <Menu position="bottom-end" width={220} radius="md">
              <Menu.Target>
                <UnstyledButton className={classes.audience}>
                  <Text fz="xs" fw={600}>
                    {audience === 'Business' ? 'For business' : 'At home'}
                  </Text>
                  <Text fz="xs" c="dimmed" aria-hidden>
                    ▾
                  </Text>
                </UnstyledButton>
              </Menu.Target>
              <Menu.Dropdown>
                <Menu.Label>Answers are written for</Menu.Label>
                <Menu.Item
                  onClick={() => {
                    choose('Consumer');
                  }}
                >
                  At home — can I eat this?
                </Menu.Item>
                <Menu.Item
                  onClick={() => {
                    choose('Business');
                  }}
                >
                  For business — what do we do with it?
                </Menu.Item>
              </Menu.Dropdown>
            </Menu>

            <ColorSchemeToggle />

            {session !== null && (
              <Menu position="bottom-end" width={230} radius="md">
                <Menu.Target>
                  <UnstyledButton className={classes.avatar} aria-label="Account">
                    {initials(session.user.displayName)}
                  </UnstyledButton>
                </Menu.Target>
                <Menu.Dropdown>
                  <Menu.Label>
                    {session.user.displayName} · {session.user.station}
                  </Menu.Label>
                  <Menu.Item onClick={signOut}>Sign out</Menu.Item>
                </Menu.Dropdown>
              </Menu>
            )}
          </Group>
        </Group>
      </AppShell.Header>

      <AppShell.Main>
        <Box className={classes.navRail}>
          <Box className={classes.shell}>
            <AppNav />
          </Box>
        </Box>

        <Box className={classes.shell} py="xl">
          <Stack gap="xl">{children}</Stack>
        </Box>
      </AppShell.Main>
    </AppShell>
  );
}

/** Two letters is enough to recognise yourself, and it needs no avatar upload. */
function initials(name: string): string {
  return name
    .split(/\s+/)
    .filter((part) => part.length > 0)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('');
}
