import { Select } from '@mantine/core';
import { SUPPORTED_FRUITS } from '../../types/enums';
import type { SupportedFruit } from '../../types/enums';

interface FruitSelectProps {
  value: SupportedFruit;
  onChange: (fruit: SupportedFruit) => void;
  disabled: boolean;
}

/**
 * The options come from SUPPORTED_FRUITS, so adding a fruit to the backend enum
 * and to types/enums.ts is enough — there is no second list to forget.
 */
export function FruitSelect({ value, onChange, disabled }: FruitSelectProps) {
  return (
    <Select
      label="Fruit"
      data={[...SUPPORTED_FRUITS]}
      value={value}
      onChange={(selected) => {
        // Narrow against the contract rather than asserting: a value that is
        // not a SupportedFruit must never reach the request.
        const fruit = SUPPORTED_FRUITS.find((candidate) => candidate === selected);
        if (fruit !== undefined) {
          onChange(fruit);
        }
      }}
      allowDeselect={false}
      disabled={disabled}
    />
  );
}
