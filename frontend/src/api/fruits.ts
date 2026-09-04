/** GET /api/fruits — Controllers/FruitsController.cs */
import type { FruitProfileResponse, RipenessBandResponse } from '../types/contracts';
import { COMMERCIAL_STATUSES, EDIBILITY_VERDICTS, SUPPORTED_FRUITS } from '../types/enums';
import { ContractReader } from './contract';
import { get } from './client';

const read = new ContractReader('GET /api/fruits');

export async function getFruits(): Promise<FruitProfileResponse[]> {
  return parseProfiles(await get<unknown>('/api/fruits'));
}

/**
 * The guide draws every field of every band — the swatch, the stage name, the
 * appearance, the audience-specific advice. A body missing any of them is
 * checked here rather than allowed to fail while rendering.
 */
export function parseProfiles(body: unknown): FruitProfileResponse[] {
  return read.array(body, '$').map((entry, index) => {
    const path = `$[${String(index)}]`;
    const profile = read.object(entry, path);

    return {
      fruitType: read.member(profile, 'fruitType', SUPPORTED_FRUITS, path),
      bands: read
        .array(profile['bands'], `${path}.bands`)
        .map((band, bandIndex) => parseBand(band, `${path}.bands[${String(bandIndex)}]`)),
    };
  });
}

function parseBand(entry: unknown, path: string): RipenessBandResponse {
  const band = read.object(entry, path);

  return {
    minPercent: read.number(band, 'minPercent', path),
    maxPercent: read.number(band, 'maxPercent', path),
    stageName: read.string(band, 'stageName', path),
    appearance: read.string(band, 'appearance', path),
    swatchHex: read.string(band, 'swatchHex', path),
    commercialStatus: read.member(band, 'commercialStatus', COMMERCIAL_STATUSES, path),
    edibility: read.member(band, 'edibility', EDIBILITY_VERDICTS, path),
    businessGuidance: read.string(band, 'businessGuidance', path),
    consumerGuidance: read.string(band, 'consumerGuidance', path),
  };
}
