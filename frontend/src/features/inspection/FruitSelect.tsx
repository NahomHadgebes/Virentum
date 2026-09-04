import { Select } from '@mantine/core';
import { SUPPORTED_FRUITS } from '../../types/enums';
import type { SupportedFruit } from '../../types/enums';

interface FruitSelectProps {
  /** What the API reported it can inspect, in the order it reported them. */
  fruits: readonly SupportedFruit[];
  value: SupportedFruit | null;
  onChange: (fruit: SupportedFruit) => void;
  disabled: boolean;
  loading: boolean;
}

/**
 * The options come from GET /api/fruits, not from the frontend's own enum.
 *
 * The enum says which names are legal in the contract; only the API knows which
 * of them the build that is actually answering can inspect. Offering the union
 * meant a frontend ahead of its API let an operator pick a fruit, choose photos
 * and press analyse, only to be told the value was not valid for FruitType —
 * after the upload. A fruit that cannot be scanned is now never offered.
 */
export function FruitSelect({ fruits, value, onChange, disabled, loading }: FruitSelectProps) {
  return (
    <Select
      label="Fruit"
      data={[...fruits]}
      value={value}
      placeholder={loading ? 'Loading the fruits this API supports…' : 'Select a fruit'}
      onChange={(selected) => {
        // Narrow against the contract rather than asserting: a value that is
        // not a SupportedFruit must never reach the request.
        const fruit = SUPPORTED_FRUITS.find((candidate) => candidate === selected);
        if (fruit !== undefined) {
          onChange(fruit);
        }
      }}
      allowDeselect={false}
      disabled={disabled || loading || fruits.length === 0}
    />
  );
}
