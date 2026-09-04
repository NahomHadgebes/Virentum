import { describe, expect, it } from 'vitest';
import { parseProfiles } from './fruits';
import { parseInspection } from './inspection';
import { ApiError } from './problemDetails';

/**
 * The payloads below are the ones an API built before the multi-image rebuild
 * actually sends. Running the current frontend against that API is the exact
 * situation these checks exist for, so the fixtures are the old shapes rather
 * than invented ones.
 */
const legacyProfile = {
  fruitType: 'Banana',
  bands: [{ minPercent: 0, maxPercent: 25, commercialStatus: 'Underripe', guidanceTemplate: 'x' }],
};

const legacyInspection = {
  fruitType: 'Banana',
  ripenessPercent: 40,
  commercialStatus: 'ReadyForSale',
  recommendation: 'Sell now.',
  scannedAt: '2026-09-02T10:00:00+00:00',
};

function reject(run: () => unknown): ApiError {
  try {
    run();
  } catch (cause) {
    expect(cause).toBeInstanceOf(ApiError);
    return cause as ApiError;
  }

  throw new Error('Expected the parser to reject this body.');
}

describe('parseProfiles', () => {
  it('names the first field an older API omits', () => {
    const error = reject(() => parseProfiles([legacyProfile]));

    expect(error.source).toBe('client');
    expect(error.detail).toContain('$[0].bands[0].stageName');
    expect(error.detail).toContain('missing');
    expect(error.detail).toContain('GET /api/fruits');
  });

  it('rejects an enum member this build does not know', () => {
    const unknownStatus = {
      fruitType: 'Banana',
      bands: [
        {
          minPercent: 0,
          maxPercent: 25,
          stageName: 'Green',
          appearance: 'Firm and green.',
          swatchHex: '#3f7d2f',
          commercialStatus: 'Composted',
          edibility: 'NotReadyYet',
          businessGuidance: 'Hold.',
          consumerGuidance: 'Wait.',
        },
      ],
    };

    const error = reject(() => parseProfiles([unknownStatus]));

    expect(error.detail).toContain('$[0].bands[0].commercialStatus');
    expect(error.detail).toContain('Underripe, ReadyForSale, ActionRequired, Expired');
  });

  it('accepts the body the current API sends', () => {
    const current = {
      fruitType: 'Avocado',
      bands: [
        {
          minPercent: 0,
          maxPercent: 30,
          stageName: 'Hard',
          appearance: 'Bright green and rock hard.',
          swatchHex: '#3f7d2f',
          commercialStatus: 'Underripe',
          edibility: 'NotReadyYet',
          businessGuidance: 'Hold at {0}%.',
          consumerGuidance: 'Give it days.',
        },
      ],
    };

    const [profile] = parseProfiles([current]);

    expect(profile?.fruitType).toBe('Avocado');
    expect(profile?.bands[0]?.swatchHex).toBe('#3f7d2f');
  });
});

describe('parseInspection', () => {
  it('names what an older scan response is missing', () => {
    const error = reject(() => parseInspection(legacyInspection));

    expect(error.source).toBe('client');
    expect(error.detail).toContain('$.evidence');
    expect(error.detail).toContain('POST /api/inspection/scan');
  });

  it('accepts the body the current API sends', () => {
    const result = parseInspection({
      fruitType: 'Banana',
      audience: 'Consumer',
      ripenessPercent: 40,
      stageName: 'Prime',
      appearance: 'Even yellow.',
      commercialStatus: 'ReadyForSale',
      edibility: 'Good',
      recommendation: 'Eat it today.',
      factors: [{ label: 'Yellow', share: 0.8, meaning: 'Ripe flesh.' }],
      imageCount: 2,
      scannedAt: '2026-09-02T10:00:00+00:00',
      evidence: { isReliable: true, concerns: [] },
    });

    expect(result.imageCount).toBe(2);
    expect(result.factors[0]?.label).toBe('Yellow');
    expect(result.evidence.concerns).toEqual([]);
  });
});
