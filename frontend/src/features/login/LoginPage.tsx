import { Card, Center, Stack, Text, Title } from '@mantine/core';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { LoginForm } from './LoginForm';

/** Where RequireAuth stashes the route the operator was trying to reach. */
interface LocationState {
  from?: string;
}

export function LoginPage() {
  const { session } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const state = location.state as LocationState | null;
  const destination = state?.from ?? '/';

  if (session !== null) {
    return <Navigate to={destination} replace />;
  }

  return (
    <Center mih="100vh" p="md">
      <Stack gap="lg" w="100%" maw={400}>
        <Stack gap={4}>
          <Title order={1}>Virentum</Title>
          <Text c="dimmed" size="sm">
            Sign in to run a produce inspection.
          </Text>
        </Stack>

        <Card withBorder padding="lg" radius="md">
          <LoginForm
            onSuccess={() => {
              void navigate(destination, { replace: true });
            }}
          />
        </Card>
      </Stack>
    </Center>
  );
}
