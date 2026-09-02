import { describe, expect, it } from 'vitest';
import {
  HISTORY_LIMIT_CHOICES,
  HISTORY_LIMIT_DEFAULT,
  HISTORY_LIMIT_MAX,
  HISTORY_LIMIT_MIN,
  SUMMARY_DAYS_DEFAULT,
  SUMMARY_DAYS_MAX,
  isHistoryLimitInRange,
  isSummaryDaysInRange,
} from './history';

/** These encode [Range(1, 100)] and [Range(1, 90)] on InspectionController. */
describe('history limit — [Range(1, 100)]', () => {
  it.each([HISTORY_LIMIT_MIN, HISTORY_LIMIT_DEFAULT, HISTORY_LIMIT_MAX])(
    'accepts %i',
    (limit) => {
      expect(isHistoryLimitInRange(limit)).toBe(true);
    },
  );

  it.each([0, -1, HISTORY_LIMIT_MAX + 1])('rejects %i', (limit) => {
    expect(isHistoryLimitInRange(limit)).toBe(false);
  });

  it('rejects a fractional limit, which the int parameter could not bind', () => {
    expect(isHistoryLimitInRange(20.5)).toBe(false);
  });

  /** Offering a choice the server would reject is a bug the UI can prevent. */
  it('only offers sizes the server accepts', () => {
    for (const choice of HISTORY_LIMIT_CHOICES) {
      expect(isHistoryLimitInRange(choice)).toBe(true);
    }
  });

  it('uses the same default the server does', () => {
    expect(HISTORY_LIMIT_DEFAULT).toBe(20);
  });
});

describe('summary window — [Range(1, 90)]', () => {
  it.each([1, SUMMARY_DAYS_DEFAULT, SUMMARY_DAYS_MAX])('accepts %i', (days) => {
    expect(isSummaryDaysInRange(days)).toBe(true);
  });

  it.each([0, SUMMARY_DAYS_MAX + 1])('rejects %i', (days) => {
    expect(isSummaryDaysInRange(days)).toBe(false);
  });

  it('uses the same default the server does', () => {
    expect(SUMMARY_DAYS_DEFAULT).toBe(7);
  });
});
