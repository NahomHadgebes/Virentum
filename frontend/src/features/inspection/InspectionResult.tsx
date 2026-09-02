import { Badge, Box, Divider, Group, Stack, Text, Title } from '@mantine/core';
import type { InspectionResponse } from '../../types/contracts';
import type { Shot } from './ImageUploader';
import { presentStatus } from '../../status/statusPresentation';
import { presentEdibility } from '../../status/edibilityPresentation';
import { FruitGlyph } from '../../components/produce/FruitGlyph';
import { RipenessScale } from './RipenessScale';
import { EvidencePanel } from './EvidencePanel';
import { AnalysisBreakdown } from './AnalysisBreakdown';
import classes from './InspectionResult.module.css';

const TIMESTAMP_FORMAT = new Intl.DateTimeFormat(undefined, {
  dateStyle: 'medium',
  timeStyle: 'short',
});

interface InspectionResultProps {
  result: InspectionResponse;
  /** The photographs this reading was taken from, so the reader can check it. */
  shots: readonly Shot[];
}

/**
 * The verdict, and everything needed to disagree with it.
 *
 * The headline answers the question the reader actually asked — a shopper is
 * told whether to eat it, a store what to do with the stock — and the rest of
 * the card is the working: where it sits on the scale, what colours it saw, and
 * what limits the reading. A verdict a reader cannot audit is just an assertion.
 */
export function InspectionResult({ result, shots }: InspectionResultProps) {
  const isConsumer = result.audience === 'Consumer';
  const status = presentStatus(result.commercialStatus);
  const edible = presentEdibility(result.edibility);
  const headline = isConsumer ? edible : { label: status.label, color: status.color };

  return (
    <Box className={`${classes.card} rise`} style={{ '--accent': headline.color }}>
      <div className={classes.accentBar} aria-hidden />

      <Stack gap="lg" p={{ base: 'lg', sm: 'xl' }}>
        <Group justify="space-between" align="flex-start" wrap="nowrap" gap="md">
          <Stack gap={6}>
            <Group gap={8}>
              <Text fz="xs" fw={700} tt="uppercase" c="dimmed" style={{ letterSpacing: '0.08em' }}>
                {result.fruitType}
              </Text>
              <Text fz="xs" c="dimmed">
                ·
              </Text>
              <Text fz="xs" c="dimmed">
                {result.stageName}
              </Text>
            </Group>

            <Title order={2} className={classes.headline}>
              {headline.label}
            </Title>

            <Text c="dimmed" fz="sm" maw={460}>
              {result.appearance}
            </Text>
          </Stack>

          <Box className={classes.glyph}>
            <FruitGlyph
              fruit={result.fruitType}
              color={headline.color}
              ripeness={result.ripenessPercent}
              size={64}
            />
          </Box>
        </Group>

        <RipenessScale
          percent={result.ripenessPercent}
          color={headline.color}
          stageName={result.stageName}
        />

        <Box className={classes.advice}>
          <Text fw={500} className={classes.adviceText}>
            {result.recommendation}
          </Text>
        </Box>

        {/* A shopper is not shown shelf language, but a shop is shown both:
            what to do with the stock, and whether it is still edible. */}
        {!isConsumer && (
          <Group gap="xs" wrap="wrap">
            <Text fz="sm" c="dimmed">
              Still edible:
            </Text>
            <Badge variant="light" color={edible.color} radius="sm">
              {edible.label}
            </Badge>
          </Group>
        )}

        <Divider color="var(--app-hairline)" />

        <AnalysisBreakdown factors={result.factors} imageCount={result.imageCount} shots={shots} />

        <EvidencePanel evidence={result.evidence} />

        <Text fz="xs" c="dimmed">
          Scanned {TIMESTAMP_FORMAT.format(new Date(result.scannedAt))} · read from{' '}
          {result.imageCount === 1 ? 'one photograph' : `${String(result.imageCount)} photographs`}
        </Text>
      </Stack>
    </Box>
  );
}
