import { Box, Group, List, Stack, Text } from '@mantine/core';
import type { InspectionEvidenceResponse } from '../../types/contracts';
import classes from './EvidencePanel.module.css';

/**
 * How much the reading can carry.
 *
 * A missing evidence object must not read as "reliable" — that would be the app
 * inventing confidence the server never expressed. Absence says nothing, and
 * saying nothing is the honest rendering of it.
 */
export function EvidencePanel({ evidence }: { evidence: InspectionEvidenceResponse | undefined }) {
  if (evidence === undefined) {
    return null;
  }

  if (evidence.isReliable) {
    return (
      <Group gap={8} wrap="nowrap">
        <Box className={classes.dotGood} aria-hidden />
        <Text fz="sm" c="dimmed">
          Nothing limited this reading — the photographs gave the analysis enough to work with.
        </Text>
      </Group>
    );
  }

  if (evidence.concerns.length === 0) {
    return null;
  }

  return (
    <Box className={classes.panel} role="alert">
      <Stack gap={8}>
        <Group gap={8} wrap="nowrap">
          <Box className={classes.dotWarn} aria-hidden />
          <Text fz="sm" fw={600}>
            Take this reading with caution
          </Text>
        </Group>
        <List size="sm" spacing={6} withPadding className={classes.list}>
          {evidence.concerns.map((concern) => (
            <List.Item key={concern}>{concern}</List.Item>
          ))}
        </List>
      </Stack>
    </Box>
  );
}
