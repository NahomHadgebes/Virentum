import { Badge, Box, Group, Stack, Text } from '@mantine/core';
import type { RipenessBandResponse } from '../../types/contracts';
import { presentStatus } from '../../status/statusPresentation';

/**
 * The API sends guidance as a template: where the advice quotes the measured
 * value it carries a {0} placeholder. Printing that raw would look broken, and
 * substituting an invented number would state a measurement that never
 * happened. The sentence is written around a number, so no phrase reads well in
 * its place either — it is shown as a visible slot instead, which is what it is.
 */
function renderGuidance(template: string): React.ReactNode {
  const parts = template.split('{0}');

  if (parts.length === 1) {
    return template;
  }

  return parts.map((part, index) => (
    // Parts are positional and the template is fixed, so the index is stable.
    <span key={index}>
      {part}
      {index < parts.length - 1 && (
        <Text span c="dimmed" ff="monospace" fz="xs">
          [ripeness]
        </Text>
      )}
    </span>
  ));
}

export function FruitBands({ bands }: { bands: readonly RipenessBandResponse[] }) {
  return (
    <Stack gap="sm">
      {bands.map((band) => {
        const status = presentStatus(band.commercialStatus);

        return (
          <Group key={band.minPercent} gap="md" wrap="nowrap" align="flex-start">
            <Box w={92} style={{ flexShrink: 0 }}>
              <Text size="sm" fw={600} ff="monospace">
                {band.minPercent}–{band.maxPercent}%
              </Text>
            </Box>

            <Badge color={status.color} variant="filled" w={130} style={{ flexShrink: 0 }}>
              {status.label}
            </Badge>

            <Text size="sm" style={{ minWidth: 0 }}>
              {renderGuidance(band.guidanceTemplate)}
            </Text>
          </Group>
        );
      })}
    </Stack>
  );
}
