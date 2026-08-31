import type { CommercialStatus } from '../../types/enums';

/**
 * How each CommercialStatus is shown. This is presentation only — the wording
 * of the advice itself comes from the API's `recommendation` and is rendered
 * verbatim, because the thresholds and copy live in the fruit processors.
 */
interface StatusPresentation {
  /** The enum name written for a human. */
  label: string;
  /** Mantine colour, distinct at a glance across the four states. */
  color: string;
}

const PRESENTATION: Record<CommercialStatus, StatusPresentation> = {
  Underripe: { label: 'Underripe', color: 'blue' },
  ReadyForSale: { label: 'Ready for sale', color: 'green' },
  ActionRequired: { label: 'Action required', color: 'orange' },
  Expired: { label: 'Expired', color: 'red' },
};

export function presentStatus(status: CommercialStatus): StatusPresentation {
  return PRESENTATION[status];
}
