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

  // An API without this field sends nothing rather than null, and
  // `undefined !== null` is true — which rendered a warning box with no message
  // in it. Only an actual message counts as a mismatch.
  const mismatch = result.colourMismatch?.trim() ?? '';

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
        {mismatch !== '' && (
          <Alert color="yellow" variant="light" title="Check the selected fruit" role="alert">
            <Text size="sm">{mismatch}</Text>
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
