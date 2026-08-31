import { Group, Progress, Text } from '@mantine/core';

interface RipenessBarProps {
  /** InspectionResponse.ripenessPercent — a whole percent, 0–100. */
  percent: number;
  color: string;
}

export function RipenessBar({ percent, color }: RipenessBarProps) {
  return (
    <div>
      <Group justify="space-between" mb={4}>
        <Text size="sm" c="dimmed">
          Ripeness
        </Text>
        <Text size="sm" fw={600}>
          {percent}%
        </Text>
      </Group>
      <Progress
        value={percent}
        color={color}
        size="lg"
        radius="sm"
        aria-label={`Ripeness ${String(percent)} percent`}
      />
    </div>
  );
}
