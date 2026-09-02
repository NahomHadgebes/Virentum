import { Box, Group, Text } from '@mantine/core';
import classes from './RipenessScale.module.css';

interface RipenessScaleProps {
  /** Whole percent, 0–100. */
  percent: number;
  color: string;
  stageName: string;
}

/**
 * Where this reading sits on the fruit's whole scale.
 *
 * A bare percentage is hard to place — is 68% a lot? A track with a marker says
 * "this far along, out of all the way", which is the question the number is
 * really answering. The number stays visible for anyone who wants the value.
 */
export function RipenessScale({ percent, color, stageName }: RipenessScaleProps) {
  return (
    <Box>
      <Group justify="space-between" align="flex-end" mb={8}>
        <Text fz="xs" fw={700} tt="uppercase" c="dimmed" style={{ letterSpacing: '0.08em' }}>
          Ripeness
        </Text>
        <Group gap={8} align="baseline">
          <Text fz={30} fw={700} lh={1} className="tabular" ff="Fraunces, Georgia, serif">
            {percent}
            <Text span fz={16} fw={600} c="dimmed">
              %
            </Text>
          </Text>
        </Group>
      </Group>

      <div
        className={classes.track}
        role="meter"
        aria-valuenow={percent}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={`Ripeness ${String(percent)} percent, ${stageName}`}
      >
        <div className={classes.fill} style={{ width: `${String(percent)}%`, background: color }} />
        <div className={classes.marker} style={{ left: `${String(percent)}%`, borderColor: color }} />
      </div>

      <Group justify="space-between" mt={6}>
        <Text fz="xs" c="dimmed">
          Unripe
        </Text>
        <Text fz="xs" c="dimmed">
          Overripe
        </Text>
      </Group>
    </Box>
  );
}
