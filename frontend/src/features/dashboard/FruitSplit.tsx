import { Group, Text } from '@mantine/core';
import type { FruitCount } from '../../types/contracts';

/**
 * Two numbers are not a chart. Colouring one bar per fruit would spend the
 * identity channel re-encoding what the number already says, so this is a plain
 * readout.
 */
export function FruitSplit({ byFruit }: { byFruit: readonly FruitCount[] }) {
  return (
    <Group gap="lg" wrap="wrap">
      {byFruit.map((entry) => (
        <Group key={entry.fruitType} gap={6}>
          <Text size="sm" c="dimmed">
            {entry.fruitType}
          </Text>
          <Text size="sm" fw={700}>
            {entry.count}
          </Text>
        </Group>
      ))}
    </Group>
  );
}
