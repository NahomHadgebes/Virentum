import { render, screen } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { describe, expect, it } from 'vitest';
import type { RipenessBandResponse } from '../../types/contracts';
import { FruitBands } from './FruitBands';

function show(bands: RipenessBandResponse[]) {
  render(
    <MantineProvider>
      <FruitBands bands={bands} />
    </MantineProvider>,
  );
}

const plain: RipenessBandResponse = {
  minPercent: 43,
  maxPercent: 75,
  commercialStatus: 'ReadyForSale',
  guidanceTemplate: 'Perfect condition. Suitable for prime display at the front shelf.',
};

const templated: RipenessBandResponse = {
  minPercent: 76,
  maxPercent: 88,
  commercialStatus: 'ActionRequired',
  guidanceTemplate: 'This batch is {0}% ripe. Print a 50% discount label immediately.',
};

describe('FruitBands', () => {
  it('shows the band range as sent by the API', () => {
    show([plain]);

    expect(screen.getByText('43–75%')).toBeDefined();
  });

  it('names the commercial status rather than relying on the colour', () => {
    show([plain]);

    expect(screen.getByText('Ready for sale')).toBeDefined();
  });

  it('renders guidance without a placeholder unchanged', () => {
    show([plain]);

    expect(
      screen.getByText(/Perfect condition\. Suitable for prime display at the front shelf\./),
    ).toBeDefined();
  });

  /**
   * The guidance is a template, not finished copy. Printing {0} raw would look
   * broken; substituting a number would state a measurement that never happened.
   */
  it('never prints the raw placeholder', () => {
    show([templated]);

    expect(screen.queryByText(/\{0\}/)).toBeNull();
  });

  it('shows the placeholder as a visible slot, keeping the sentence intact', () => {
    show([templated]);

    expect(screen.getByText('[ripeness]')).toBeDefined();
    expect(screen.getByText(/This batch is/)).toBeDefined();
    expect(screen.getByText(/% ripe\. Print a 50% discount label immediately\./)).toBeDefined();
  });

  it('renders every band it is given', () => {
    show([plain, templated]);

    expect(screen.getByText('43–75%')).toBeDefined();
    expect(screen.getByText('76–88%')).toBeDefined();
  });
});
