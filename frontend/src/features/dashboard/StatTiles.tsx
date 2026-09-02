import { Card, Group, Stack, Text } from '@mantine/core';
import type { InspectionSummaryResponse } from '../../types/contracts';

const TIMESTAMP_FORMAT = new Intl.DateTimeFormat(undefined, {
  dateStyle: 'medium',
  timeStyle: 'short',
});

/**
 * Headline numbers belong in stat tiles, not in a chart. Three values with no
 * shared scale have nothing to compare against each other.
 */
export function StatTiles({ summary }: { summary: InspectionSummaryResponse }) {
  return (
    <Group grow align="stretch" wrap="wrap">
      <Tile label="Scans" value={String(summary.totalScans)} note={`Last ${String(summary.windowDays)} days`} />
      <Tile
        label="Average ripeness"
        // Null means nothing was scanned. Rendering 0% would read as "everything
        // is completely unripe", which is a different claim entirely.
        value={summary.averageRipenessPercent === null
          ? '—'
          : `${String(Math.round(summary.averageRipenessPercent))}%`}
        note={summary.averageRipenessPercent === null ? 'No scans in this window' : 'Across the window'}
      />
      <Tile
        label="Last scan"
        value={summary.lastScanAt === null ? '—' : TIMESTAMP_FORMAT.format(new Date(summary.lastScanAt))}
        note={summary.lastScanAt === null ? 'No scans in this window' : 'Most recent inspection'}
      />
    </Group>
  );
}

function Tile({ label, value, note }: { label: string; value: string; note: string }) {
  return (
    <Card withBorder padding="md" radius="md" miw={180}>
      <Stack gap={2}>
        <Text size="xs" c="dimmed" tt="uppercase" fw={600}>
          {label}
        </Text>
        <Text fz={28} fw={700} lh={1.2}>
          {value}
        </Text>
        <Text size="xs" c="dimmed">
          {note}
        </Text>
      </Stack>
    </Card>
  );
}
