import type { EdibilityVerdict } from '../types/enums';

/**
 * How each EdibilityVerdict is shown to a person deciding whether to eat
 * something.
 *
 * Deliberately not the same scale as CommercialStatus: produce a shop must pull
 * from display is often perfectly good at home, and reusing the shop's colours
 * here would tell a shopper to bin edible food. The reserved status steps still
 * apply, but the mapping is its own.
 */
interface EdibilityPresentation {
  /** The verdict as a person would say it. */
  label: string;
  /** Reserved status step. */
  color: string;
}

const PRESENTATION: Record<EdibilityVerdict, EdibilityPresentation> = {
  NotReadyYet: { label: 'Not ready yet', color: '#fab219' },
  Good: { label: 'Good to eat', color: '#0ca30c' },
  EatSoon: { label: 'Eat it today', color: '#ec835a' },
  DoNotEat: { label: 'Do not eat', color: '#d03b3b' },
};

export function presentEdibility(verdict: EdibilityVerdict): EdibilityPresentation {
  return PRESENTATION[verdict];
}
