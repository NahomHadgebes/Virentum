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
