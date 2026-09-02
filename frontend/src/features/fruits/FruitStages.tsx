import { Badge, Box, Group, Stack, Text, Title } from '@mantine/core';
import type { FruitProfileResponse, RipenessBandResponse } from '../../types/contracts';
import type { Audience } from '../../types/enums';
import { presentStatus } from '../../status/statusPresentation';
import { presentEdibility } from '../../status/edibilityPresentation';
import { FruitGlyph } from '../../components/produce/FruitGlyph';
import classes from './FruitStages.module.css';

/**
 * One fruit, every stage, laid out along its own scale.
 *
 * The bar across the top is the whole 0–100 range with each stage taking its
 * real share of the width, so a reader can see at a glance that an avocado is
 * "ready" for nearly half its life while a banana's prime is a narrower window.
 * A list of four cards cannot show that; a proportional strip can.
 */
export function FruitStages({
  profile,
  audience,
}: {
  profile: FruitProfileResponse;
  audience: Audience;
}) {
  return (
    <Stack gap="md">
      <Group gap="sm" align="center">
        {/* Drawn at the fruit's prime stage, so the heading shows it at its
            best rather than in a neutral grey that reads as disabled. */}
        <FruitGlyph
          fruit={profile.fruitType}
          color={primeSwatch(profile)}
          ripeness={40}
          size={30}
        />
        <Title order={3}>{profile.fruitType}</Title>
      </Group>

      <div className={classes.strip} aria-hidden>
        {profile.bands.map((band) => (
          <div
            key={band.minPercent}
            className={classes.stripSegment}
            style={{
              width: `${String(((band.maxPercent - band.minPercent + 1) / 101) * 100)}%`,
              background: band.swatchHex,
            }}
          />
        ))}
      </div>

      <Stack gap="sm">
        {profile.bands.map((band) => (
          <Stage key={band.minPercent} band={band} fruit={profile.fruitType} audience={audience} />
        ))}
      </Stack>
    </Stack>
  );
}

/** The colour of the stage a shop would call ready for sale. */
function primeSwatch(profile: FruitProfileResponse): string {
  const prime = profile.bands.find((band) => band.commercialStatus === 'ReadyForSale');
  return prime?.swatchHex ?? profile.bands[0]?.swatchHex ?? '#8a8f7a';
}

function Stage({
  band,
  fruit,
  audience,
}: {
  band: RipenessBandResponse;
  fruit: FruitProfileResponse['fruitType'];
  audience: Audience;
}) {
  const isConsumer = audience === 'Consumer';
  const badge = isConsumer
    ? presentEdibility(band.edibility)
    : presentStatus(band.commercialStatus);
  const guidance = isConsumer ? band.consumerGuidance : band.businessGuidance;

  return (
    <Box className={classes.stage}>
      <div className={classes.swatchColumn}>
        <div className={classes.swatch} style={{ background: band.swatchHex }}>
          <FruitGlyph
            fruit={fruit}
            color={band.swatchHex}
            ripeness={band.maxPercent}
            size={44}
            title={`${fruit} at the ${band.stageName} stage`}
          />
        </div>
        <Text fz="xs" c="dimmed" className="tabular" ta="center">
          {band.minPercent}–{band.maxPercent}%
        </Text>
      </div>

      <Stack gap={6} style={{ minWidth: 0 }}>
        <Group gap="sm" wrap="wrap">
          <Text fw={600}>{band.stageName}</Text>
          <Badge color={badge.color} variant="light" radius="sm" size="sm">
            {badge.label}
          </Badge>
        </Group>

        <Text fz="sm" c="dimmed">
          {band.appearance}
        </Text>

        <Text fz="sm">{renderGuidance(guidance)}</Text>
      </Stack>
    </Box>
  );
}

/**
 * The API sends guidance as a template: where the advice quotes the measured
 * value it carries a {0} placeholder. Printing it raw would look broken, and
 * substituting a number would state a measurement that never happened — the
 * guide describes a stage, not a scan. The sentence is written around a number,
 * so no phrase reads well in its place either; it is shown as a visible slot.
 */
function renderGuidance(template: string): React.ReactNode {
  const parts = template.split('{0}');

  if (parts.length === 1) {
    return template;
  }

  return parts.map((part, index) => (
    // Parts are positional and the template is fixed, so the index is stable.
    <span key={index}>
      {part}
      {index < parts.length - 1 && (
        <Text span c="dimmed" ff="monospace" fz="xs">
          [ripeness]
        </Text>
      )}
    </span>
  ));
}
