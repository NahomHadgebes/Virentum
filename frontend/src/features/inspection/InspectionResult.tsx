import { Alert, Badge, Card, Group, List, Stack, Text, Title } from '@mantine/core';
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

  // An API without this field sends nothing rather than a value, and a missing
  // object must not read as "reliable" — that would be the app inventing
  // confidence the server never expressed. Absent concerns mean nothing to show;
  // absent evidence means nothing to claim either way.
  const concerns = result.evidence?.concerns ?? [];
  const unreliable = result.evidence !== undefined && !result.evidence.isReliable;

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
        {unreliable && concerns.length > 0 && (
          <Alert
            color="yellow"
            variant="light"
            title="This reading may not be trustworthy"
            role="alert"
          >
            <List size="sm" spacing={4} withPadding>
              {concerns.map((concern) => (
                <List.Item key={concern}>{concern}</List.Item>
              ))}
            </List>
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
