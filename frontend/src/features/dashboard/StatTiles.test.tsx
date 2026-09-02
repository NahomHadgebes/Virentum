import { render, screen } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { describe, expect, it } from 'vitest';
import type { InspectionSummaryResponse } from '../../types/contracts';
import { StatTiles } from './StatTiles';

const EMPTY: InspectionSummaryResponse = {
  windowDays: 7,
  since: '2026-08-25T00:00:00+00:00',
  totalScans: 0,
  byStatus: [],
  byFruit: [],
  averageRipenessPercent: null,
  lastScanAt: null,
};

function show(summary: Partial<InspectionSummaryResponse>) {
  render(
    <MantineProvider>
      <StatTiles summary={{ ...EMPTY, ...summary }} />
    </MantineProvider>,
  );
}

describe('StatTiles', () => {
  it('shows the scan count for the window', () => {
    show({ totalScans: 34 });

    expect(screen.getByText('34')).toBeDefined();
    expect(screen.getByText('Last 7 days')).toBeDefined();
  });

  it('rounds the average ripeness the API sent', () => {
    show({ totalScans: 3, averageRipenessPercent: 64.6667 });

    expect(screen.getByText('65%')).toBeDefined();
  });

  /**
   * The API sends null rather than zero for a window with no scans, because zero
   * would read as "everything is completely unripe". The UI has to keep that
   * distinction visible.
   */
  it('shows no average at all when nothing was scanned', () => {
    show({ totalScans: 0, averageRipenessPercent: null });

    expect(screen.queryByText('0%')).toBeNull();
    expect(screen.getAllByText('—').length).toBe(2);
    expect(screen.getAllByText('No scans in this window').length).toBe(2);
  });

  it('shows no last-scan time when nothing was scanned', () => {
    show({ lastScanAt: null });

    expect(screen.getAllByText('—').length).toBe(2);
  });
});
