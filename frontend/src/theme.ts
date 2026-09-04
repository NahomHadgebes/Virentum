import { createTheme, rem } from '@mantine/core';
import type { MantineColorsTuple } from '@mantine/core';

/**
 * Virentum's design tokens.
 *
 * The palette is built from produce rather than from a UI kit: a botanical green
 * for the brand, warm sand for surfaces instead of the usual cold grey, and an
 * amber accent borrowed from ripe fruit. Warm neutrals are the single biggest
 * reason the app reads as considered rather than generated — a default grey
 * scale is the tell.
 *
 * Typography pairs Fraunces, an optical-size serif, for anything that carries a
 * verdict, with Inter for everything a reader scans rather than reads.
 */

/** Botanical green. Darker and less saturated than a stock green. */
const virentum: MantineColorsTuple = [
  '#eef7f0',
  '#dcefe0',
  '#b7ddc0',
  '#8fca9e',
  '#6db881',
  '#55ac6d',
  '#3f9459',
  '#33804a',
  '#286a3c',
  '#1c542e',
];

/** Warm sand, used for every surface and border. */
const sand: MantineColorsTuple = [
  '#faf8f4',
  '#f2eee6',
  '#e6dfd2',
  '#d6ccba',
  '#c4b7a1',
  '#b3a48d',
  '#9c8b73',
  '#7d6e5a',
  '#5c5044',
  '#3a332c',
];

/** Ripe amber, for accents and highlights. */
const amber: MantineColorsTuple = [
  '#fff8e6',
  '#fdefd0',
  '#f9dda1',
  '#f5ca6d',
  '#f1ba43',
  '#efb029',
  '#eeab1a',
  '#d3950e',
  '#bb8405',
  '#a17100',
];

export const theme = createTheme({
  colors: { virentum, sand, amber },
  primaryColor: 'virentum',
  primaryShade: { light: 7, dark: 5 },

  fontFamily:
    'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif',
  fontFamilyMonospace:
    'ui-monospace, SFMono-Regular, "SF Mono", Menlo, Consolas, "Liberation Mono", monospace',

  headings: {
    fontFamily: 'Fraunces, Georgia, "Times New Roman", serif',
    fontWeight: '600',
    sizes: {
      h1: { fontSize: rem(46), lineHeight: '1.08', fontWeight: '600' },
      h2: { fontSize: rem(32), lineHeight: '1.15', fontWeight: '600' },
      h3: { fontSize: rem(23), lineHeight: '1.25', fontWeight: '600' },
      h4: { fontSize: rem(18), lineHeight: '1.3', fontWeight: '600' },
    },
  },

  defaultRadius: 'lg',
  radius: { sm: rem(8), md: rem(12), lg: rem(16), xl: rem(24) },

  shadows: {
    xs: '0 1px 2px rgba(58, 51, 44, 0.06)',
    sm: '0 2px 8px rgba(58, 51, 44, 0.07)',
    md: '0 8px 24px -8px rgba(58, 51, 44, 0.16)',
    lg: '0 18px 44px -14px rgba(58, 51, 44, 0.22)',
  },

  // Pick black or white text per background luminance instead of always white.
  autoContrast: true,
  // Mantine's default of 0.3 leaves the status "good" green just inside the
  // white-text band, where its badge measures 3.35:1 — below the 4.5:1 that
  // small bold text needs. At 0.2 it takes dark text and measures 6.29:1.
  luminanceThreshold: 0.2,

  other: {
    /** Duration for the app's own transitions, kept in one place. */
    motionFast: '140ms',
    motionBase: '260ms',
    easing: 'cubic-bezier(0.22, 1, 0.36, 1)',
  },
});
