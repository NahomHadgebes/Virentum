import { useCallback, useMemo, useState } from 'react';
import { Button, Container, Group, Skeleton, Stack, Text, Title } from '@mantine/core';
import { useDocumentTitle } from '@mantine/hooks';
import { getHistory } from '../../api/inspection';
import { useApiResource } from '../../api/useApiResource';
import { HISTORY_LIMIT_DEFAULT } from '../../validation/history';
import { ProblemAlert } from '../../components/ProblemAlert';
import { ANY, HistoryFilters } from './HistoryFilters';
import type { FruitFilter, StatusFilter } from './HistoryFilters';
import { HistoryTable } from './HistoryTable';

export function HistoryPage() {
  useDocumentTitle('History · Virentum');

  const [limit, setLimit] = useState(HISTORY_LIMIT_DEFAULT);
  const [fruit, setFruit] = useState<FruitFilter>(ANY);
  const [status, setStatus] = useState<StatusFilter>(ANY);

  // Memoised on limit alone: fruit and status narrow what was fetched, so
  // changing them must not trigger a request.
  const load = useCallback(() => getHistory(limit), [limit]);
  const { data, error, loading, reload } = useApiResource(load);

  const visible = useMemo(
    () =>
      (data ?? []).filter(
        (item) =>
          (fruit === ANY || item.fruitType === fruit) &&
          (status === ANY || item.commercialStatus === status),
      ),
    [data, fruit, status],
  );

  return (
    <Container size="md">
      <Stack gap="lg">
        <Group justify="space-between" align="flex-end" wrap="wrap">
          <div>
            <Title order={2}>History</Title>
            <Text c="dimmed" size="sm">
              The most recent inspections recorded at this store.
            </Text>
          </div>
          <Button variant="default" onClick={reload} loading={loading}>
            Refresh
          </Button>
        </Group>

        <HistoryFilters
          fruit={fruit}
          status={status}
          limit={limit}
          onFruitChange={setFruit}
          onStatusChange={setStatus}
          onLimitChange={setLimit}
          disabled={loading}
        />

        {error !== null && <ProblemAlert error={error} />}

        {loading && <Skeleton height={220} radius="md" />}

        {!loading && error === null && data !== null && <Results total={data.length} visible={visible} />}
      </Stack>
    </Container>
  );
}

function Results({
  total,
  visible,
}: {
  total: number;
  visible: React.ComponentProps<typeof HistoryTable>['items'];
}) {
  if (total === 0) {
    return (
      <Text c="dimmed">
        No inspections recorded yet. Run a scan and it will appear here.
      </Text>
    );
  }

  if (visible.length === 0) {
    return (
      <Text c="dimmed">
        None of the {total} loaded scans match these filters.
      </Text>
    );
  }

  return (
    <Stack gap="xs">
      <Text size="sm" c="dimmed">
        {/* Filters narrow the loaded window, so both numbers matter. */}
        Showing {visible.length} of {total} loaded scans.
      </Text>
      <HistoryTable items={visible} />
    </Stack>
  );
}
