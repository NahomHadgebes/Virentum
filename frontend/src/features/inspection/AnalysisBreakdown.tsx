import { Box, Group, Stack, Text } from '@mantine/core';
import type { AnalysisFactorResponse } from '../../types/contracts';
import type { Shot } from './ImageUploader';
import classes from './AnalysisBreakdown.module.css';

/** The colour a bucket is drawn in, so the bar looks like what it measured. */
const SWATCHES: Record<string, string> = {
  green: '#4b8a3f',
  yellow: '#e8b52c',
  'brown or dark': '#5a4632',
};

interface AnalysisBreakdownProps {
  factors: readonly AnalysisFactorResponse[];
  imageCount: number;
  shots: readonly Shot[];
}

/**
 * Why the reading came out where it did.
 *
 * This is the difference between a verdict and an assertion: the reader can hold
 * their own photograph next to the bar and check whether "just over half yellow"
 * is a fair description. If it isn't, they know not to trust the number — which
 * is exactly the judgement the app cannot make for them.
 */
export function AnalysisBreakdown({ factors, imageCount, shots }: AnalysisBreakdownProps) {
  return (
    <Stack gap="sm">
      <Text fz="xs" fw={700} tt="uppercase" c="dimmed" style={{ letterSpacing: '0.08em' }}>
        What the analysis saw
      </Text>

      {shots.length > 0 && (
        <Group gap="xs">
          {shots.map((shot, index) => (
            <img
              key={shot.id}
              src={shot.url}
              alt={`Photograph ${String(index + 1)} used for this reading`}
              className={classes.thumb}
            />
          ))}
        </Group>
      )}

      {factors.length === 0 ? (
        <Text fz="sm" c="dimmed">
          No produce-like colour could be measured in{' '}
          {imageCount === 1 ? 'this photograph' : 'these photographs'}.
        </Text>
      ) : (
        <>
          <div className={classes.bar} role="presentation">
            {factors.map((factor) => (
              <div
                key={factor.label}
                className={classes.segment}
                style={{
                  width: `${String(factor.share * 100)}%`,
                  background: SWATCHES[factor.label] ?? 'var(--mantine-color-sand-4)',
                }}
              />
            ))}
          </div>

          <Stack gap={8}>
            {factors.map((factor) => (
              <Group key={factor.label} gap="sm" wrap="nowrap" align="flex-start">
                <Box
                  className={classes.dot}
                  style={{ background: SWATCHES[factor.label] ?? 'var(--mantine-color-sand-4)' }}
                  aria-hidden
                />
                {/* Wide enough that "brown or dark" stays on one line, so the
                    meanings beside it line up as a column. */}
                <Text fz="sm" w={132} fw={600} className="tabular" style={{ flexShrink: 0 }}>
                  {Math.round(factor.share * 100)}% {factor.label}
                </Text>
                <Text fz="sm" c="dimmed" style={{ minWidth: 0 }}>
                  {factor.meaning}
                </Text>
              </Group>
            ))}
          </Stack>
        </>
      )}
    </Stack>
  );
}
