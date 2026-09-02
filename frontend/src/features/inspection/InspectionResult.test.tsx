import { render, screen } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { describe, expect, it } from 'vitest';
import type { InspectionResponse } from '../../types/contracts';
import { InspectionResult } from './InspectionResult';

const BASE: InspectionResponse = {
  fruitType: 'Avocado',
  ripenessPercent: 68,
  commercialStatus: 'ReadyForSale',
  recommendation: 'Firm and ready for the premium produce section.',
  scannedAt: '2026-09-02T13:00:00+00:00',
  evidence: { isReliable: true, concerns: [] },
};

function show(result: Partial<InspectionResponse>) {
  render(
    <MantineProvider>
      <InspectionResult result={{ ...BASE, ...result }} />
    </MantineProvider>,
  );
}

describe('InspectionResult', () => {
  it('renders the assessment the API returned', () => {
    show({});

    expect(screen.getByText('Avocado')).toBeDefined();
    expect(screen.getByText('68%')).toBeDefined();
    expect(screen.getByText(/Firm and ready for the premium produce section\./)).toBeDefined();
  });

  it('shows no warning when nothing limits the reading', () => {
    show({});

    expect(screen.queryByRole('alert')).toBeNull();
  });

  /**
   * Virentum measures colour and does not identify produce, so the operator's
   * selection can be wrong. When it looks wrong, the advice below is answering
   * the wrong question — and the reader has to be told.
   */
  it('surfaces every concern the API raised', () => {
    show({
      evidence: {
        isReliable: false,
        concerns: [
          'Only 8% of this image held produce-like colour; the rest read as background.',
          '70% of this image reads as yellow, which carries no ripeness meaning for Avocado.',
        ],
      },
    });

    expect(screen.getByRole('alert')).toBeDefined();
    expect(screen.getByText(/Only 8% of this image/)).toBeDefined();
    expect(screen.getByText(/no ripeness meaning for Avocado/)).toBeDefined();
    expect(screen.getByText('This reading may not be trustworthy')).toBeDefined();
  });

  /**
   * An API that predates the field omits it entirely. `undefined !== null` is
   * true, so a strict null check rendered a warning box with nothing in it —
   * worse than no warning, because it implies a problem it cannot name.
   */
  it('shows no warning when the API omitted the field entirely', () => {
    const withoutField = { ...BASE } as Partial<InspectionResponse>;
    delete withoutField.evidence;

    render(
      <MantineProvider>
        <InspectionResult result={withoutField as InspectionResponse} />
      </MantineProvider>,
    );

    expect(screen.queryByRole('alert')).toBeNull();
    expect(screen.getByText('68%')).toBeDefined();
  });

  /** An unreliable verdict with no stated reason has nothing to show. */
  it('shows no warning when the reading is unreliable but unexplained', () => {
    show({ evidence: { isReliable: false, concerns: [] } });

    expect(screen.queryByRole('alert')).toBeNull();
  });

  it('still shows the assessment alongside the warning', () => {
    show({ evidence: { isReliable: false, concerns: ['Something is off.'] } });

    expect(screen.getByText('68%')).toBeDefined();
    expect(screen.getByText(/Firm and ready/)).toBeDefined();
  });
});
