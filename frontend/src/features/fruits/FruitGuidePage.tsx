import { useCallback } from 'react';
import { Card, Container, Skeleton, Stack, Text, Title } from '@mantine/core';
import { useDocumentTitle } from '@mantine/hooks';
import { getFruits } from '../../api/fruits';
import { useApiResource } from '../../api/useApiResource';
import { ProblemAlert } from '../../components/ProblemAlert';
import { FruitBands } from './FruitBands';

export function FruitGuidePage() {
  useDocumentTitle('Fruit guide · Virentum');

  const load = useCallback(() => getFruits(), []);
  const { data, error, loading } = useApiResource(load);

  return (
    <Container size="md">
      <Stack gap="lg">
        <div>
          <Title order={2}>Fruit guide</Title>
          <Text c="dimmed" size="sm">
            The ripeness bands each fruit is judged against. These come from the
            API, so they are the same thresholds a scan is actually assessed with.
          </Text>
        </div>

        {error !== null && <ProblemAlert error={error} />}

        {loading && <Skeleton height={280} radius="md" />}

        {!loading && error === null && data !== null && (
          <Stack gap="lg">
            {data.map((profile) => (
              <Card key={profile.fruitType} withBorder padding="lg" radius="md">
                <Stack gap="md">
                  <Title order={3}>{profile.fruitType}</Title>
                  <FruitBands bands={profile.bands} />
                </Stack>
              </Card>
            ))}
          </Stack>
        )}
      </Stack>
    </Container>
  );
}
