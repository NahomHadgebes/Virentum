/**
 * Mirrors backend/src/Virentum.Api/Domain/Enums/.
 *
 * The backend serialises enums by name (global JsonStringEnumConverter), and
 * both enum files state that the member names are part of the public contract.
 * These are therefore string literal unions, not numeric enums.
 */

/** Domain/Enums/SupportedFruit.cs */
export const SUPPORTED_FRUITS = ['Banana', 'Avocado'] as const;

export type SupportedFruit = (typeof SUPPORTED_FRUITS)[number];

/** Domain/Enums/CommercialStatus.cs */
export const COMMERCIAL_STATUSES = [
  'Underripe',
  'ReadyForSale',
  'ActionRequired',
  'Expired',
] as const;

export type CommercialStatus = (typeof COMMERCIAL_STATUSES)[number];

/** Domain/Enums/Audience.cs */
export const AUDIENCES = ['Consumer', 'Business'] as const;

export type Audience = (typeof AUDIENCES)[number];

/** Domain/Enums/EdibilityVerdict.cs */
export const EDIBILITY_VERDICTS = [
  'NotReadyYet',
  'Good',
  'EatSoon',
  'DoNotEat',
] as const;

export type EdibilityVerdict = (typeof EDIBILITY_VERDICTS)[number];
