import { useCallback } from 'react';
import { Box, Container, Group, SegmentedControl, Stack, Text, Title } from '@mantine/core';
import { useDocumentTitle } from '@mantine/hooks';
import { getFruits } from '../../api/fruits';
import { useApiResource } from '../../api/useApiResource';
import { useAudience } from '../../audience/useAudience';
import { ProblemAlert } from '../../components/ProblemAlert';
import { FruitStages } from './FruitStages';
import { GuideSkeleton } from './GuideSkeleton';
import type { Audience } from '../../types/enums';

/**
 * The stages a fruit passes through, drawn from the same bands a scan is judged
 * against. The audience toggle is here as well as in the header because the
 * guide is the one page a visitor might open before deciding which reader they
 * are — and the advice genuinely differs.
 */
export function FruitGuidePage() {
  useDocumentTitle('Fruit guide · Virentum');

  const { audience, choose } = useAudience();
  const active: Audience = audience ?? 'Consumer';

  const load = useCallback(() => getFruits(), []);
  const { data, error, loading } = useApiResource(load);

  return (
    <Container size={860} px={0}>
      <Stack gap="xl">
        <Group justify="space-between" align="flex-end" wrap="wrap" gap="md">
          <Stack gap={6} maw={520}>
            <Title order={2}>Fruit guide</Title>
            <Text c="dimmed">
              Every stage each fruit passes through, what it looks like, and what to do about it.
              These are the exact thresholds a scan is measured against — not a separate article
              that can drift out of date.
            </Text>
          </Stack>

          <SegmentedControl
            value={active}
            onChange={(value) => {
              if (value === 'Consumer' || value === 'Business') {
                choose(value);
              }
            }}
            data={[
              { label: 'At home', value: 'Consumer' },
              { label: 'For business', value: 'Business' },
            ]}
            radius="md"
          />
        </Group>

        {error !== null && <ProblemAlert error={error} />}

        {loading && <GuideSkeleton />}

        {!loading && error === null && data !== null && (
          <Stack gap={40}>
            {data.map((profile) => (
              <Box key={profile.fruitType} className="rise">
                <FruitStages profile={profile} audience={active} />
              </Box>
            ))}
          </Stack>
        )}
      </Stack>
    </Container>
  );
}
