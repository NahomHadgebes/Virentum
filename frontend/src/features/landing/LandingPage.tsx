import { Badge, Box, Container, Group, Stack, Text, Title, UnstyledButton } from '@mantine/core';
import { useDocumentTitle } from '@mantine/hooks';
import { useNavigate } from 'react-router-dom';
import { useAudience } from '../../audience/useAudience';
import type { Audience } from '../../types/enums';
import { FruitGlyph } from '../../components/produce/FruitGlyph';
import { ColorSchemeToggle } from '../../components/ColorSchemeToggle';
import classes from './LandingPage.module.css';

interface Choice {
  audience: Audience;
  eyebrow: string;
  title: string;
  body: string;
  examples: string[];
  swatch: string;
  fruit: 'Banana' | 'Avocado';
  ripeness: number;
}

/**
 * The two readers are asking genuinely different questions, and the same
 * measurement answers both. Making that the first screen sets the expectation
 * that the app has a point of view, rather than being a form with a file input.
 */
const CHOICES: readonly Choice[] = [
  {
    audience: 'Consumer',
    eyebrow: 'At home',
    title: 'Is this still good?',
    body: 'Point your camera at the fruit and get a straight answer, in plain language, with what to do next.',
    examples: ['Is this avocado still edible?', 'Will these bananas last the week?'],
    swatch: '#ffd54f',
    fruit: 'Banana',
    ripeness: 60,
  },
  {
    audience: 'Business',
    eyebrow: 'On the shelf',
    title: 'What do we do with this stock?',
    body: 'Ripeness, shelf status and the action to take, recorded per store so you can see the week at a glance.',
    examples: ['Discount or pull from display?', 'How much did we lose this week?'],
    swatch: '#4b6b35',
    fruit: 'Avocado',
    ripeness: 55,
  },
];

export function LandingPage() {
  useDocumentTitle('Virentum · Know when produce is ready');

  const { choose } = useAudience();
  const navigate = useNavigate();

  const pick = (audience: Audience) => {
    choose(audience);
    void navigate('/scan');
  };

  return (
    <Box className={classes.page}>
      <Group justify="space-between" px="lg" py="md" className={classes.topBar}>
        <Group gap="xs">
          <Box className={classes.mark} aria-hidden />
          <Text fw={700} fz="lg" ff="Fraunces, Georgia, serif">
            Virentum
          </Text>
        </Group>
        <ColorSchemeToggle />
      </Group>

      <Container size="lg" py={{ base: 'xl', sm: 60 }}>
        <Stack gap={48}>
          <Stack gap="lg" maw={620} className="rise">
            <Badge variant="light" color="amber" size="lg" radius="sm" w="fit-content">
              Produce, read by colour
            </Badge>
            <Title order={1}>
              Know when produce is ready —{' '}
              <Text span inherit c="virentum.7">
                and when it isn&apos;t.
              </Text>
            </Title>
            {/* Deliberately not a list of fruits: which ones can be scanned is
                the API's answer, and a landing page that enumerates them goes
                stale the moment one is added. */}
            <Text fz="lg" c="dimmed" maw={540}>
              Photograph the fruit. Virentum reads the colour, places it on that
              fruit&apos;s ripeness scale, and tells you what it means — including when the picture
              wasn&apos;t good enough to say.
            </Text>
          </Stack>

          <Stack gap="md">
            <Text tt="uppercase" fz="xs" fw={700} c="dimmed" style={{ letterSpacing: '0.08em' }}>
              Who is asking?
            </Text>

            <div className={classes.choices}>
              {CHOICES.map((choice, index) => (
                <UnstyledButton
                  key={choice.audience}
                  className={`${classes.choice} rise`}
                  style={{ animationDelay: `${String(index * 90)}ms` }}
                  onClick={() => {
                    pick(choice.audience);
                  }}
                >
                  <Stack gap="md" h="100%">
                    <Group justify="space-between" align="flex-start" wrap="nowrap">
                      <Stack gap={4}>
                        <Text fz="xs" fw={700} tt="uppercase" c="dimmed" style={{ letterSpacing: '0.08em' }}>
                          {choice.eyebrow}
                        </Text>
                        <Title order={3}>{choice.title}</Title>
                      </Stack>
                      <Box className={classes.glyph}>
                        <FruitGlyph
                          fruit={choice.fruit}
                          color={choice.swatch}
                          ripeness={choice.ripeness}
                          size={56}
                        />
                      </Box>
                    </Group>

                    <Text c="dimmed" fz="sm" style={{ flex: 1 }}>
                      {choice.body}
                    </Text>

                    <Stack gap={6}>
                      {choice.examples.map((example) => (
                        <Text key={example} fz="sm" c="dimmed" className={classes.example}>
                          {example}
                        </Text>
                      ))}
                    </Stack>

                    <Group gap={6} className={classes.cta}>
                      <Text fz="sm" fw={600}>
                        Continue as {choice.audience.toLowerCase()}
                      </Text>
                      <Text fz="sm" fw={600} className={classes.arrow} aria-hidden>
                        &rarr;
                      </Text>
                    </Group>
                  </Stack>
                </UnstyledButton>
              ))}
            </div>

            <Text fz="xs" c="dimmed">
              You can switch at any time from the header.
            </Text>
          </Stack>
        </Stack>
      </Container>
    </Box>
  );
}
