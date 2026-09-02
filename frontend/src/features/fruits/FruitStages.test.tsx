import { render, screen } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { describe, expect, it } from 'vitest';
import type { FruitProfileResponse } from '../../types/contracts';
import { FruitStages } from './FruitStages';

const PROFILE: FruitProfileResponse = {
  fruitType: 'Banana',
  bands: [
    {
      minPercent: 43,
      maxPercent: 75,
      stageName: 'Prime',
      appearance: 'Even yellow, firm but with a slight give.',
      swatchHex: '#ffd54f',
      commercialStatus: 'ReadyForSale',
      edibility: 'Good',
      businessGuidance: 'Perfect condition. Suitable for prime display at the front shelf.',
      consumerGuidance: 'Ready to eat right now. Sweet, firm and at its best.',
    },
    {
      minPercent: 76,
      maxPercent: 88,
      stageName: 'Spotted',
      appearance: 'Yellow with brown freckles, noticeably softer.',
      swatchHex: '#c98a3a',
      commercialStatus: 'ActionRequired',
      edibility: 'EatSoon',
      businessGuidance: 'This batch is {0}% ripe. Print a 50% discount label immediately.',
      consumerGuidance: 'Very sweet and soft. Eat it today, or freeze it for baking.',
    },
  ],
};

function show(audience: 'Consumer' | 'Business') {
  render(
    <MantineProvider>
      <FruitStages profile={PROFILE} audience={audience} />
    </MantineProvider>,
  );
}

describe('FruitStages', () => {
  it('names every stage with the range it covers', () => {
    show('Consumer');

    expect(screen.getByText('Prime')).toBeDefined();
    expect(screen.getByText('43–75%')).toBeDefined();
    expect(screen.getByText('Spotted')).toBeDefined();
    expect(screen.getByText('76–88%')).toBeDefined();
  });

  it('describes how the fruit looks at each stage', () => {
    show('Consumer');

    expect(screen.getByText(/Even yellow, firm but with a slight give\./)).toBeDefined();
  });

  /**
   * A shopper and a shop are answering different questions, and the guide has to
   * answer the one the reader actually has.
   */
  it('shows consumer guidance and edibility at home', () => {
    show('Consumer');

    expect(screen.getByText(/Ready to eat right now/)).toBeDefined();
    expect(screen.getByText('Good to eat')).toBeDefined();
    expect(screen.queryByText(/prime display at the front shelf/)).toBeNull();
  });

  it('shows business guidance and shelf status for a store', () => {
    show('Business');

    expect(screen.getByText(/prime display at the front shelf/)).toBeDefined();
    expect(screen.getByText('Ready for sale')).toBeDefined();
    expect(screen.queryByText(/Ready to eat right now/)).toBeNull();
  });

  /** The guide describes a stage, so it must not state a measurement. */
  it('never prints the raw placeholder', () => {
    show('Business');

    expect(screen.queryByText(/\{0\}/)).toBeNull();
    expect(screen.getByText('[ripeness]')).toBeDefined();
  });

  it('draws each stage with a title a screen reader can use', () => {
    show('Consumer');

    expect(screen.getByLabelText('Banana at the Prime stage')).toBeDefined();
  });
});
