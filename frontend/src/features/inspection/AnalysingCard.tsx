import { Box, Stack, Text } from '@mantine/core';
import classes from './AnalysingCard.module.css';

/**
 * The waiting state, written as the steps the server actually performs rather
 * than a spinner. A reader who knows what is happening tolerates the wait; a
 * spinner only says "something".
 */
const STEPS = [
  'Reading pixels',
  'Sorting colour',
  'Placing on the ripeness scale',
  'Weighing the evidence',
] as const;

export function AnalysingCard({ imageCount }: { imageCount: number }) {
  return (
    <Box className={`${classes.card} rise`} aria-live="polite" aria-busy="true">
      <Stack gap="md" p="xl">
        <Text fz="sm" fw={600}>
          Analysing {imageCount === 1 ? 'your photo' : `${String(imageCount)} photos`}
        </Text>

        <Stack gap={10}>
          {STEPS.map((step, index) => (
            <div
              key={step}
              className={classes.step}
              style={{ animationDelay: `${String(index * 260)}ms` }}
            >
              <span className={classes.stepDot} aria-hidden />
              <Text fz="sm" c="dimmed">
                {step}
              </Text>
            </div>
          ))}
        </Stack>

        <div className={classes.sheen} aria-hidden />
      </Stack>
    </Box>
  );
}
