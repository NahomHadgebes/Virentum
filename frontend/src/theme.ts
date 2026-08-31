import { createTheme } from '@mantine/core';

/** Virentum: produce inspection. Green carries the brand; the rest is Mantine's. */
export const theme = createTheme({
  primaryColor: 'green',
  defaultRadius: 'md',

  // Pick black or white text per background luminance instead of always white.
  // Without it the filled orange used for ActionRequired renders white-on-orange,
  // which measures around 2.5:1.
  autoContrast: true,
});
