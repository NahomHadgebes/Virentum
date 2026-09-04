import { useCallback, useState } from 'react';
import { Card, Group, Select, Skeleton, Stack, Text, Title } from '@mantine/core';
import { useDocumentTitle } from '@mantine/hooks';
import { getSummary } from '../../api/inspection';
import { useApiResource } from '../../api/useApiResource';
import { ProblemAlert } from '../../components/ProblemAlert';
import { SUMMARY_DAYS_DEFAULT } from '../../validation/history';
import { StatTiles } from './StatTiles';
import { StatusBreakdownChart } from './StatusBreakdownChart';
import { FruitSplit } from './FruitSplit';

/** Every option is inside the server's [Range(1, 90)]. */
const WINDOW_CHOICES = [7, 30, 90] as const;

export function DashboardPage() {
  useDocumentTitle('Dashboard · Virentum');

  const [days, setDays] = useState<number>(SUMMARY_DAYS_DEFAULT);
  const load = useCallback(() => getSummary(days), [days]);
  const { data, error, loading } = useApiResource(load);

  return (
      <Stack gap="lg">
        <Group justify="space-between" align="flex-end" wrap="wrap">
          <div>
            <Title order={2}>Dashboard</Title>
            <Text c="dimmed" size="sm">
              Inspection activity recorded at this store.
            </Text>
          </div>
          <Select
            label="Window"
            w={140}
            value={String(days)}
            data={WINDOW_CHOICES.map((value) => ({
              value: String(value),
              label: `Last ${String(value)} days`,
            }))}
            onChange={(selected) => {
              const match = WINDOW_CHOICES.find((choice) => String(choice) === selected);
              if (match !== undefined) {
                setDays(match);
              }
            }}
            allowDeselect={false}
            disabled={loading}
          />
        </Group>

        {error !== null && <ProblemAlert error={error} />}

        {loading && <Skeleton height={320} radius="md" />}

        {!loading && error === null && data !== null && (
          <Stack gap="lg">
            <StatTiles summary={data} />

            <Card withBorder padding="lg" radius="md">
              <Stack gap="md">
                <div>
                  <Title order={4}>Scans by status</Title>
                  <Text size="xs" c="dimmed">
                    {data.totalScans === 0
                      ? 'Nothing scanned in this window.'
                      : `${String(data.totalScans)} scans since ${new Date(data.since).toLocaleDateString()}.`}
                  </Text>
                </div>
                <StatusBreakdownChart byStatus={data.byStatus} totalScans={data.totalScans} />
              </Stack>
            </Card>

            <Card withBorder padding="lg" radius="md">
              <Stack gap="xs">
                <Title order={4}>By fruit</Title>
                <FruitSplit byFruit={data.byFruit} />
              </Stack>
            </Card>
          </Stack>
        )}
      </Stack>
  );
}
