import { describe, expect, it } from 'vitest';
import { render } from '@testing-library/react';
import { FruitGlyph } from './FruitGlyph';
import { SUPPORTED_FRUITS } from '../../types/enums';

/**
 * The lookup in FruitGlyph is typed as a total Record, so a missing fruit is a
 * compile error rather than a blank box. These tests cover what the type cannot:
 * that each entry actually draws something, and that no two fruits share an
 * outline — a copied path would type-check perfectly and ship a mango shaped
 * like an avocado.
 */
describe('FruitGlyph', () => {
  function outlineOf(fruit: (typeof SUPPORTED_FRUITS)[number]): string {
    const { container } = render(
      <FruitGlyph fruit={fruit} color="#8a8f7a" ripeness={50} title={`${fruit} sample`} />,
    );
    const svg = container.querySelector('svg');

    expect(svg).not.toBeNull();
    return svg?.innerHTML ?? '';
  }

  it.each(SUPPORTED_FRUITS)('draws %s', (fruit) => {
    const outline = outlineOf(fruit);

    expect(outline).toContain('#8a8f7a');
    expect(outline.length).toBeGreaterThan(120);
  });

  it('gives every fruit its own outline', () => {
    const outlines = SUPPORTED_FRUITS.map(outlineOf);

    expect(new Set(outlines).size).toBe(SUPPORTED_FRUITS.length);
  });

  it('adds spotting only once a fruit is past its prime', () => {
    const { container: fresh } = render(
      <FruitGlyph fruit="Pear" color="#c2cd5c" ripeness={40} title="fresh" />,
    );
    const { container: old } = render(
      <FruitGlyph fruit="Pear" color="#6b4b2a" ripeness={95} title="old" />,
    );

    expect(fresh.querySelectorAll('ellipse[opacity="0.5"]')).toHaveLength(0);
    expect(old.querySelectorAll('ellipse[opacity="0.5"]').length).toBeGreaterThan(0);
  });
});
