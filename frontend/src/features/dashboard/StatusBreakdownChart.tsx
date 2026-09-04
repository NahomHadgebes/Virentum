import { Box, Group, Stack, Text, Tooltip } from '@mantine/core';
import type { StatusCount } from '../../types/contracts';
import { presentStatus } from '../../status/statusPresentation';

/** Mark spec: bars cap at 24px so the row keeps air around them. */
const BAR_HEIGHT = 20;

interface StatusBreakdownChartProps {
  byStatus: readonly StatusCount[];
  totalScans: number;
}

/**
 * Scans per commercial status, as horizontal bars.
 *
 * Horizontal because the category names are long. Every bar is named and its
 * count printed, so identity never rests on colour alone — which is what makes
 * the reserved status steps safe to use here despite two of them sitting close
 * together under simulated colour-vision deficiency.
 *
 * The API zero-fills every status, so the axis is the same four rows whether the
 * week was busy or quiet. A row at zero is information, not an absence.
 */
export function StatusBreakdownChart({ byStatus, totalScans }: StatusBreakdownChartProps) {
  // Scale to the largest bar, not to the total: with four buckets a share-of-
  // total scale would leave every bar short and hard to compare.
  const largest = Math.max(...byStatus.map((entry) => entry.count), 1);

  return (
    <Stack gap="xs" role="group" aria-label="Scans by commercial status">
      {byStatus.map((entry) => {
        const status = presentStatus(entry.commercialStatus);
        const share = totalScans === 0 ? 0 : Math.round((entry.count / totalScans) * 100);

        return (
          <Group key={entry.commercialStatus} gap="sm" wrap="nowrap" align="center">
            <Text size="sm" w={130} style={{ flexShrink: 0 }}>
              {status.label}
            </Text>

            <Tooltip
              label={`${status.label}: ${String(entry.count)} of ${String(totalScans)} scans (${String(share)}%)`}
              withArrow
            >
              {/* The track is the plot area; the fill grows from its left baseline. */}
              <Box
                style={{
                  flex: 1,
                  minWidth: 0,
                  height: BAR_HEIGHT,
                  borderRadius: 2,
                  background: 'var(--mantine-color-default-hover)',
                }}
              >
                <Box
                  style={{
                    width: `${String((entry.count / largest) * 100)}%`,
                    height: '100%',
                    background: status.color,
                    // Square at the baseline, rounded at the data end.
                    borderRadius: '2px 4px 4px 2px',
                    transition: 'width 150ms ease',
                  }}
                />
              </Box>
            </Tooltip>

            <Text size="sm" fw={600} w={36} ta="right" style={{ flexShrink: 0 }}>
              {entry.count}
            </Text>
          </Group>
        );
      })}
    </Stack>
  );
}
