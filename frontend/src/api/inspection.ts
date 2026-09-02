/** POST /api/inspection/scan — Controllers/InspectionController.cs */
import type {
  InspectionHistoryItem,
  InspectionResponse,
  InspectionSummaryResponse,
  ScanRequest,
} from '../types/contracts';
import { get, post } from './client';

export function scan(request: ScanRequest): Promise<InspectionResponse> {
  const form = new FormData();

  // Field names must match Contracts/Requests/ScanRequest.cs. Repeating the
  // Images key is how a form binds to IFormFileCollection; the enums bind from
  // their member names.
  for (const image of request.images) {
    form.append('Images', image);
  }
  form.append('FruitType', request.fruitType);
  form.append('Audience', request.audience);

  return post<InspectionResponse>({
    path: '/api/inspection/scan',
    body: form,
    authenticated: true,
  });
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
