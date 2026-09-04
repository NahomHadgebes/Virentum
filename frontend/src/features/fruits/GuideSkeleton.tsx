import { Skeleton, Stack } from '@mantine/core';

/**
 * Shaped like the content it stands in for, so the page does not jump when the
 * real stages arrive.
 */
export function GuideSkeleton() {
  return (
    <Stack gap={40}>
      {[0, 1].map((fruit) => (
        <Stack key={fruit} gap="md">
          <Skeleton height={26} width={160} radius="sm" />
          <Stack gap="sm">
            {[0, 1, 2, 3].map((stage) => (
              <Skeleton key={stage} height={96} radius="lg" />
            ))}
          </Stack>
        </Stack>
      ))}
    </Stack>
  );
}
