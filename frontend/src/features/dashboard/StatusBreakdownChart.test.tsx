import { render, screen } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { describe, expect, it } from 'vitest';
import type { StatusCount } from '../../types/contracts';
import { StatusBreakdownChart } from './StatusBreakdownChart';

const ALL: StatusCount[] = [
  { commercialStatus: 'Underripe', count: 3 },
  { commercialStatus: 'ReadyForSale', count: 12 },
  { commercialStatus: 'ActionRequired', count: 5 },
  { commercialStatus: 'Expired', count: 0 },
];

function show(byStatus: StatusCount[], totalScans: number) {
  render(
    <MantineProvider>
      <StatusBreakdownChart byStatus={byStatus} totalScans={totalScans} />
    </MantineProvider>,
  );
}

describe('StatusBreakdownChart', () => {
  /**
   * Identity must never rest on colour alone — that is what makes the reserved
   * status steps safe to use for adjacent bars.
   */
  it('names every status in text', () => {
    show(ALL, 20);

    for (const label of ['Underripe', 'Ready for sale', 'Action required', 'Expired']) {
      expect(screen.getByText(label)).toBeDefined();
    }
  });

  it('prints the count beside every bar', () => {
    show(ALL, 20);

    expect(screen.getByText('12')).toBeDefined();
    expect(screen.getByText('5')).toBeDefined();
  });

  /** A zero-filled row is information: nothing expired this week. */
  it('keeps a status with no scans as a labelled row', () => {
    show(ALL, 20);

    expect(screen.getByText('Expired')).toBeDefined();
    expect(screen.getByText('0')).toBeDefined();
  });

  it('renders one row per status even when the window is empty', () => {
    show(
      ALL.map((entry) => ({ ...entry, count: 0 })),
      0,
    );

    expect(screen.getByText('Ready for sale')).toBeDefined();
    expect(screen.getAllByText('0').length).toBe(4);
  });
});
