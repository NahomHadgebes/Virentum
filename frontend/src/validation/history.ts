/**
 * Client-side mirror of the query-parameter bounds on
 * backend/src/Virentum.Api/Controllers/InspectionController.cs:
 *
 *   GetHistory: [Range(1, 100)] int limit = 20
 *   GetSummary: [Range(1, 90)]  int days  = 7
 *
 * A value outside these comes back as ValidationProblemDetails, so the point of
 * having them here is to offer only reachable choices — not to add rules of our
 * own.
 */

export const HISTORY_LIMIT_MIN = 1;
export const HISTORY_LIMIT_MAX = 100;
export const HISTORY_LIMIT_DEFAULT = 20;

/** Sizes offered in the UI. Every one is inside the server's range. */
export const HISTORY_LIMIT_CHOICES = [20, 50, 100] as const;

export const SUMMARY_DAYS_MIN = 1;
export const SUMMARY_DAYS_MAX = 90;
export const SUMMARY_DAYS_DEFAULT = 7;

export function isHistoryLimitInRange(limit: number): boolean {
  return Number.isInteger(limit) && limit >= HISTORY_LIMIT_MIN && limit <= HISTORY_LIMIT_MAX;
}

export function isSummaryDaysInRange(days: number): boolean {
  return Number.isInteger(days) && days >= SUMMARY_DAYS_MIN && days <= SUMMARY_DAYS_MAX;
}
