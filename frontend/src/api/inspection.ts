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

  // Field names must match Contracts/Requests/ScanRequest.cs. Form binding
  // resolves FruitType from the enum member name.
  form.append('Image', request.image);
  form.append('FruitType', request.fruitType);

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
