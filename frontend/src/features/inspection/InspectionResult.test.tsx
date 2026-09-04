import { render, screen } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { describe, expect, it } from 'vitest';
import type { InspectionResponse } from '../../types/contracts';
import { InspectionResult } from './InspectionResult';

const BASE: InspectionResponse = {
  fruitType: 'Avocado',
  audience: 'Consumer',
  ripenessPercent: 68,
  stageName: 'Ready',
  appearance: 'Darker green, yields slightly to gentle pressure.',
  commercialStatus: 'ReadyForSale',
  edibility: 'Good',
  recommendation: 'Good to eat. Press gently with your whole hand.',
  factors: [
    { label: 'green', share: 0.62, meaning: 'firm and under-ripe' },
    { label: 'brown or dark', share: 0.38, meaning: 'ripe to over-ripe' },
  ],
  imageCount: 2,
  scannedAt: '2026-09-02T13:00:00+00:00',
  evidence: { isReliable: true, concerns: [] },
};

function show(result: Partial<InspectionResponse>) {
  render(
    <MantineProvider>
      <InspectionResult result={{ ...BASE, ...result }} shots={[]} />
    </MantineProvider>,
  );
}

describe('InspectionResult', () => {
  /**
   * The headline has to answer the question the reader actually asked. A shopper
   * asking whether to eat something is not helped by shelf language.
   */
  it('leads with edibility for a consumer', () => {
    show({ audience: 'Consumer' });

    expect(screen.getByRole('heading', { name: 'Good to eat' })).toBeDefined();
    expect(screen.queryByText('Ready for sale')).toBeNull();
  });

  it('leads with the shelf decision for a business, and still states edibility', () => {
    show({ audience: 'Business' });

    expect(screen.getByRole('heading', { name: 'Ready for sale' })).toBeDefined();
    expect(screen.getByText('Good to eat')).toBeDefined();
  });

  it('shows the ripeness as a meter a screen reader can read', () => {
    show({});

    const meter = screen.getByRole('meter');
    expect(meter.getAttribute('aria-valuenow')).toBe('68');
    expect(screen.getByText('68')).toBeDefined();
  });

  it('renders the stage and how the fruit should look', () => {
    show({});

    expect(screen.getByText('Ready')).toBeDefined();
    expect(screen.getByText(/yields slightly to gentle pressure/)).toBeDefined();
  });

  /** A verdict a reader cannot audit is an assertion. */
  it('shows what the analysis saw, with a meaning per colour', () => {
    show({});

    expect(screen.getByText('62% green')).toBeDefined();
    expect(screen.getByText('firm and under-ripe')).toBeDefined();
    expect(screen.getByText('38% brown or dark')).toBeDefined();
  });

  it('says how many photographs the reading pooled', () => {
    show({ imageCount: 2 });

    expect(screen.getByText(/read from 2 photographs/)).toBeDefined();
  });

  it('confirms plainly when nothing limited the reading', () => {
    show({});

    expect(screen.getByText(/Nothing limited this reading/)).toBeDefined();
    expect(screen.queryByRole('alert')).toBeNull();
  });

  it('surfaces every concern the API raised', () => {
    show({
      evidence: {
        isReliable: false,
        concerns: [
          'Only 8% of the picture held produce-like colour.',
          '70% of what we could see reads as yellow.',
        ],
      },
    });

    expect(screen.getByRole('alert')).toBeDefined();
    expect(screen.getByText(/Only 8% of the picture/)).toBeDefined();
    expect(screen.getByText(/reads as yellow/)).toBeDefined();
    expect(screen.getByText('Take this reading with caution')).toBeDefined();
  });

  /**
   * A missing evidence object must not read as "reliable" — that would be the
   * app inventing confidence the server never expressed.
   */
  it('claims nothing either way when the API omitted the evidence', () => {
    const withoutField = { ...BASE } as Partial<InspectionResponse>;
    delete withoutField.evidence;

    render(
      <MantineProvider>
        <InspectionResult result={withoutField as InspectionResponse} shots={[]} />
      </MantineProvider>,
    );

    expect(screen.queryByRole('alert')).toBeNull();
    expect(screen.queryByText(/Nothing limited this reading/)).toBeNull();
    expect(screen.getByText('68')).toBeDefined();
  });

  it('still shows the assessment alongside a warning', () => {
    show({ evidence: { isReliable: false, concerns: ['Something is off.'] } });

    expect(screen.getByText('68')).toBeDefined();
    expect(screen.getByText(/Press gently with your whole hand/)).toBeDefined();
  });

  it('says so when no produce-like colour could be measured', () => {
    show({ factors: [] });

    expect(screen.getByText(/No produce-like colour could be measured/)).toBeDefined();
  });
});
