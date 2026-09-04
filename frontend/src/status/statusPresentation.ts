import type { CommercialStatus } from '../types/enums';

/**
 * How each CommercialStatus is shown, everywhere in the app: badges, ripeness
 * bars, and the dashboard chart all read from here so one status is one colour.
 *
 * These are the reserved status steps — good / warning / serious / critical —
 * not categorical series colours, because a commercial status genuinely means
 * good-or-bad rather than "series 4". They are mode-invariant by design and all
 * clear 3:1 on the dark surface; on light, serious sits at 2.57:1, which is why
 * every place that uses a status colour also shows its name. Colour is never the
 * only channel.
 *
 * The four roles map to merchandising urgency rather than to ripeness order —
 * the middle of the ripeness scale is the good state, so the colours are not
 * monotonic and should not be.
 */
interface StatusPresentation {
  /** The enum name written for a human. */
  label: string;
  /** Reserved status step. */
  color: string;
}

const PRESENTATION: Record<CommercialStatus, StatusPresentation> = {
  // warning: not sellable today, but nothing is going wrong.
  Underripe: { label: 'Underripe', color: '#fab219' },
  // good.
  ReadyForSale: { label: 'Ready for sale', color: '#0ca30c' },
  // serious: sellable but degrading, discount now.
  ActionRequired: { label: 'Action required', color: '#ec835a' },
  // critical: pull from display.
  Expired: { label: 'Expired', color: '#d03b3b' },
};

export function presentStatus(status: CommercialStatus): StatusPresentation {
  return PRESENTATION[status];
}
