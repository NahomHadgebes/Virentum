import { Group, Select } from '@mantine/core';
import { COMMERCIAL_STATUSES, SUPPORTED_FRUITS } from '../../types/enums';
import type { CommercialStatus, SupportedFruit } from '../../types/enums';
import { HISTORY_LIMIT_CHOICES } from '../../validation/history';
import { presentStatus } from '../inspection/statusPresentation';

/** Sentinel for "no filter". Not a value the API knows about. */
export const ANY = 'any';

export type FruitFilter = SupportedFruit | typeof ANY;
export type StatusFilter = CommercialStatus | typeof ANY;

interface HistoryFiltersProps {
  fruit: FruitFilter;
  status: StatusFilter;
  limit: number;
  onFruitChange: (fruit: FruitFilter) => void;
  onStatusChange: (status: StatusFilter) => void;
  onLimitChange: (limit: number) => void;
  disabled: boolean;
}

/**
 * Fruit and status narrow the rows already fetched; the size picker changes what
 * is fetched. That difference is why the page states how many scans it is
 * looking at — filtering a window is not the same as searching everything.
 */
export function HistoryFilters({
  fruit,
  status,
  limit,
  onFruitChange,
  onStatusChange,
  onLimitChange,
  disabled,
}: HistoryFiltersProps) {
  return (
    <Group gap="sm" wrap="wrap">
      <Select
        label="Fruit"
        w={160}
        value={fruit}
        data={[
          { value: ANY, label: 'All fruit' },
          ...SUPPORTED_FRUITS.map((value) => ({ value, label: value })),
        ]}
        onChange={(selected) => {
          const match = SUPPORTED_FRUITS.find((candidate) => candidate === selected);
          onFruitChange(match ?? ANY);
        }}
        allowDeselect={false}
        disabled={disabled}
      />

      <Select
        label="Status"
        w={180}
        value={status}
        data={[
          { value: ANY, label: 'All statuses' },
          ...COMMERCIAL_STATUSES.map((value) => ({
            value,
            label: presentStatus(value).label,
          })),
        ]}
        onChange={(selected) => {
          const match = COMMERCIAL_STATUSES.find((candidate) => candidate === selected);
          onStatusChange(match ?? ANY);
        }}
        allowDeselect={false}
        disabled={disabled}
      />

      <Select
        label="Scans to load"
        w={140}
        value={String(limit)}
        data={HISTORY_LIMIT_CHOICES.map((value) => ({
          value: String(value),
          label: `Last ${String(value)}`,
        }))}
        onChange={(selected) => {
          const match = HISTORY_LIMIT_CHOICES.find(
            (candidate) => String(candidate) === selected,
          );
          if (match !== undefined) {
            onLimitChange(match);
          }
        }}
        allowDeselect={false}
        disabled={disabled}
      />
    </Group>
  );
}
