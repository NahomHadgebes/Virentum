import { Badge, Progress, Table, Text } from '@mantine/core';
import type { InspectionHistoryItem } from '../../types/contracts';
import { presentStatus } from '../inspection/statusPresentation';

const TIMESTAMP_FORMAT = new Intl.DateTimeFormat(undefined, {
  dateStyle: 'medium',
  timeStyle: 'short',
});

export function HistoryTable({ items }: { items: readonly InspectionHistoryItem[] }) {
  return (
    <Table.ScrollContainer minWidth={620}>
      <Table striped highlightOnHover verticalSpacing="sm">
        <Table.Thead>
          <Table.Tr>
            <Table.Th>Scanned</Table.Th>
            <Table.Th>Fruit</Table.Th>
            <Table.Th>Ripeness</Table.Th>
            <Table.Th>Status</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {items.map((item) => {
            const status = presentStatus(item.commercialStatus);

            return (
              <Table.Tr key={item.id}>
                <Table.Td>
                  <Text size="sm">{TIMESTAMP_FORMAT.format(new Date(item.scannedAt))}</Text>
                </Table.Td>
                <Table.Td>{item.fruitType}</Table.Td>
                <Table.Td w={180}>
                  <Text size="sm" fw={600} mb={4}>
                    {item.ripenessPercent}%
                  </Text>
                  <Progress
                    value={item.ripenessPercent}
                    color={status.color}
                    size="sm"
                    aria-label={`Ripeness ${String(item.ripenessPercent)} percent`}
                  />
                </Table.Td>
                <Table.Td>
                  <Badge color={status.color} variant="filled">
                    {status.label}
                  </Badge>
                </Table.Td>
              </Table.Tr>
            );
          })}
        </Table.Tbody>
      </Table>
    </Table.ScrollContainer>
  );
}
