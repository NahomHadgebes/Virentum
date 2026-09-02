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
  colourMismatch: null,
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

  it('shows no warning when the image is consistent with the selection', () => {
    show({});

    expect(screen.queryByRole('alert')).toBeNull();
  });

  /**
   * Virentum measures colour and does not identify produce, so the operator's
   * selection can be wrong. When it looks wrong, the advice below is answering
   * the wrong question — and the reader has to be told.
   */
  it('surfaces a colour mismatch reported by the API', () => {
    show({
      colourMismatch:
        '80% of this image reads as yellow, which is unusual for Avocado. ' +
        'Virentum measures colour, not fruit identity - check that the right fruit is selected.',
    });

    expect(screen.getByRole('alert')).toBeDefined();
    expect(screen.getByText(/80% of this image reads as yellow/)).toBeDefined();
    expect(screen.getByText('Check the selected fruit')).toBeDefined();
  });

  it('still shows the assessment alongside the warning', () => {
    show({ colourMismatch: 'Something is off.' });

    expect(screen.getByText('68%')).toBeDefined();
    expect(screen.getByText(/Firm and ready/)).toBeDefined();
  });
});
