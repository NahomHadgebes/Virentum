import { createTheme } from '@mantine/core';

/** Virentum: produce inspection. Green carries the brand; the rest is Mantine's. */
export const theme = createTheme({
  primaryColor: 'green',
  defaultRadius: 'md',

  // Pick black or white text per background luminance instead of always white.
  // Without it the filled orange used for ActionRequired renders white-on-orange,
  // which measures around 2.5:1.
  autoContrast: true,

  // Mantine's default threshold of 0.3 leaves the status "good" green (relative
  // luminance 0.264) just inside the white-text band, where its badge measures
  // 3.35:1 — below the 4.5:1 that 13px bold text needs. At 0.2 it takes dark
  // text instead and measures 6.29:1; no other status colour changes side.
  luminanceThreshold: 0.2,
});
