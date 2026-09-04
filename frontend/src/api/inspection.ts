/** POST /api/inspection/scan — Controllers/InspectionController.cs */
import type {
  AnalysisFactorResponse,
  InspectionHistoryItem,
  InspectionResponse,
  InspectionSummaryResponse,
  ScanRequest,
} from '../types/contracts';
import {
  AUDIENCES,
  COMMERCIAL_STATUSES,
  EDIBILITY_VERDICTS,
  SUPPORTED_FRUITS,
} from '../types/enums';
import { ContractReader } from './contract';
import { get, post } from './client';

const read = new ContractReader('POST /api/inspection/scan');

export async function scan(request: ScanRequest): Promise<InspectionResponse> {
  const form = new FormData();

  // Field names must match Contracts/Requests/ScanRequest.cs. Repeating the
  // Images key is how a form binds to IFormFileCollection; the enums bind from
  // their member names.
  for (const image of request.images) {
    form.append('Images', image);
  }
  form.append('FruitType', request.fruitType);
  form.append('Audience', request.audience);

  const body = await post<unknown>({
    path: '/api/inspection/scan',
    body: form,
    authenticated: true,
  });

  return parseInspection(body);
}

/**
 * The verdict card renders the stage, the reasoning and the evidence panel from
 * this body. Checking it here means a contract mismatch is reported as a failed
 * scan, with the offending field named, instead of crashing the result card.
 */
export function parseInspection(body: unknown): InspectionResponse {
  const result = read.object(body, '$');
  const evidence = read.object(result['evidence'], '$.evidence');

  return {
    fruitType: read.member(result, 'fruitType', SUPPORTED_FRUITS, '$'),
    audience: read.member(result, 'audience', AUDIENCES, '$'),
    ripenessPercent: read.number(result, 'ripenessPercent', '$'),
    stageName: read.string(result, 'stageName', '$'),
    appearance: read.string(result, 'appearance', '$'),
    commercialStatus: read.member(result, 'commercialStatus', COMMERCIAL_STATUSES, '$'),
    edibility: read.member(result, 'edibility', EDIBILITY_VERDICTS, '$'),
    recommendation: read.string(result, 'recommendation', '$'),
    factors: read
      .array(result['factors'], '$.factors')
      .map((factor, index) => parseFactor(factor, `$.factors[${String(index)}]`)),
    imageCount: read.number(result, 'imageCount', '$'),
    scannedAt: read.string(result, 'scannedAt', '$'),
    evidence: {
      isReliable: read.boolean(evidence, 'isReliable', '$.evidence'),
      concerns: read.strings(evidence, 'concerns', '$.evidence'),
    },
  };
}

function parseFactor(entry: unknown, path: string): AnalysisFactorResponse {
  const factor = read.object(entry, path);

  return {
    label: read.string(factor, 'label', path),
    share: read.number(factor, 'share', path),
    meaning: read.string(factor, 'meaning', path),
  };
}

/** GET /api/inspection/history — Controllers/InspectionController.cs */
export function getHistory(limit: number): Promise<InspectionHistoryItem[]> {
  const query = new URLSearchParams({ limit: String(limit) });
  return get<InspectionHistoryItem[]>(`/api/inspection/history?${query.toString()}`);
}

/** GET /api/inspection/summary — Controllers/InspectionController.cs */
export function getSummary(days: number): Promise<InspectionSummaryResponse> {
  const query = new URLSearchParams({ days: String(days) });
  return get<InspectionSummaryResponse>(`/api/inspection/summary?${query.toString()}`);
}
