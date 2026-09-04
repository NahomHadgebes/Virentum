import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { MantineProvider } from '@mantine/core';
import { FruitSelect } from './FruitSelect';
import type { SupportedFruit } from '../../types/enums';

function renderSelect(fruits: readonly SupportedFruit[], value: SupportedFruit | null = null) {
  const onChange = vi.fn();

  render(
    <MantineProvider>
      <FruitSelect
        fruits={fruits}
        value={value}
        onChange={onChange}
        disabled={false}
        loading={false}
      />
    </MantineProvider>,
  );

  return onChange;
}

function open() {
  fireEvent.click(screen.getByRole('combobox', { name: 'Fruit' }));
}

/**
 * The point of these: an operator must never be offered a fruit the API cannot
 * inspect. Before the options came from GET /api/fruits, a frontend ahead of
 * its API let you pick one, attach photos and press analyse — and only then
 * returned "the value 'Pear' is not valid for FruitType".
 */
describe('FruitSelect', () => {
  it('offers exactly the fruits the API reported', () => {
    renderSelect(['Banana', 'Avocado']);
    open();

    expect(screen.getByRole('option', { name: 'Banana' })).toBeTruthy();
    expect(screen.getByRole('option', { name: 'Avocado' })).toBeTruthy();
    expect(screen.queryByRole('option', { name: 'Pear' })).toBeNull();
    expect(screen.queryByRole('option', { name: 'Mango' })).toBeNull();
  });

  it('offers a fruit the API added without any change here', () => {
    renderSelect(['Banana', 'Avocado', 'Pear', 'Mango']);
    open();

    expect(screen.getByRole('option', { name: 'Pear' })).toBeTruthy();
    expect(screen.getByRole('option', { name: 'Mango' })).toBeTruthy();
  });

  it('reports the chosen fruit to its caller', () => {
    const onChange = renderSelect(['Banana', 'Pear']);
    open();
    fireEvent.click(screen.getByRole('option', { name: 'Pear' }));

    expect(onChange).toHaveBeenCalledWith('Pear');
  });

  it('cannot be used when the API reported nothing', () => {
    renderSelect([]);

    expect(screen.getByRole('combobox', { name: 'Fruit' })).toHaveProperty('disabled', true);
  });
});
