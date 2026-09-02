import { Alert, Badge, Card, Group, Stack, Text, Title } from '@mantine/core';
import type { InspectionResponse } from '../../types/contracts';
import { presentStatus } from '../../status/statusPresentation';
import { RipenessBar } from './RipenessBar';

/** Browser-local formatting of the API's DateTimeOffset. */
const TIMESTAMP_FORMAT = new Intl.DateTimeFormat(undefined, {
  dateStyle: 'medium',
  timeStyle: 'short',
});

export function InspectionResult({ result }: { result: InspectionResponse }) {
  const status = presentStatus(result.commercialStatus);

  return (
    <Card withBorder padding="lg" radius="md">
      <Stack gap="md">
        <Group justify="space-between" align="flex-start" wrap="nowrap">
          <Title order={3}>{result.fruitType}</Title>
          <Badge color={status.color} size="lg" variant="filled">
            {status.label}
          </Badge>
        </Group>

        {/* Shown above the advice: if the wrong fruit was selected, the advice
            below it is answering the wrong question. */}
        {result.colourMismatch !== null && (
          <Alert color="yellow" variant="light" title="Check the selected fruit" role="alert">
            <Text size="sm">{result.colourMismatch}</Text>
          </Alert>
        )}

        <RipenessBar percent={result.ripenessPercent} color={status.color} />

        {/* Verbatim from the fruit processor; the thresholds and wording are
            the backend's, not ours. */}
        <Text>{result.recommendation}</Text>

        <Text size="xs" c="dimmed">
          Scanned {TIMESTAMP_FORMAT.format(new Date(result.scannedAt))}
        </Text>
      </Stack>
    </Card>
  );
}
