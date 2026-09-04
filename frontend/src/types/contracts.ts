/**
 * Mirrors backend/src/Virentum.Api/Contracts/ field for field.
 *
 * Property names are camelCased by the backend's default JSON policy; no field
 * is added, renamed or reshaped here. Anything the API does not send does not
 * belong in this file.
 */
import type { Audience, CommercialStatus, EdibilityVerdict, SupportedFruit } from './enums';

/** Contracts/Requests/LoginRequest.cs */
export interface LoginRequest {
  storeId: string;
  password: string;
}

/** Contracts/Responses/UserDto.cs */
export interface UserDto {
  storeId: string;
  displayName: string;
  station: string;
}

/** Contracts/Responses/LoginResponse.cs */
export interface LoginResponse {
  token: string;
  user: UserDto;
}

/**
 * Contracts/Requests/ScanRequest.cs
 *
 * Sent as multipart/form-data, not JSON. The backend binds the form fields by
 * the names `Images`, `FruitType` and `Audience`; see api/inspection.ts.
 */
export interface ScanRequest {
  /** One to three photographs of the same item; evidence is pooled across them. */
  images: File[];
  fruitType: SupportedFruit;
  audience: Audience;
}

/** Contracts/Responses/InspectionResponse.cs — AnalysisFactorResponse */
export interface AnalysisFactorResponse {
  /** The colour, written for a human. */
  label: string;
  /** Its share of everything the analysis could classify, 0–1. */
  share: number;
  /** What that colour indicates for this particular fruit. */
  meaning: string;
}

/** Contracts/Responses/InspectionResponse.cs — InspectionEvidenceResponse */
export interface InspectionEvidenceResponse {
  isReliable: boolean;
  /** Plain statements of what limits the reading. Empty when nothing does. */
  concerns: string[];
}

/**
 * Contracts/Responses/InspectionResponse.cs
 *
 * Both readings of the same measurement travel together: what a store should do
 * with the stock, and whether a person can still eat it.
 */
export interface InspectionResponse {
  /** The fruit the operator selected, echoed back — never derived from the image. */
  fruitType: SupportedFruit;
  audience: Audience;
  /** Whole percent, 0–100. */
  ripenessPercent: number;
  stageName: string;
  appearance: string;
  commercialStatus: CommercialStatus;
  edibility: EdibilityVerdict;
  recommendation: string;
  /** What the measurement rested on, largest first. */
  factors: AnalysisFactorResponse[];
  imageCount: number;
  /** DateTimeOffset, ISO 8601 with offset. */
  scannedAt: string;
  evidence: InspectionEvidenceResponse;
}

/** Contracts/Responses/InspectionHistoryItem.cs */
export interface InspectionHistoryItem {
  id: string;
  fruitType: SupportedFruit;
  /** Whole percent, 0–100. */
  ripenessPercent: number;
  commercialStatus: CommercialStatus;
  recommendation: string;
  /** DateTimeOffset, ISO 8601 with offset. */
  scannedAt: string;
}

/** Contracts/Responses/InspectionSummaryResponse.cs — StatusCount */
export interface StatusCount {
  commercialStatus: CommercialStatus;
  count: number;
}

/** Contracts/Responses/InspectionSummaryResponse.cs — FruitCount */
export interface FruitCount {
  fruitType: SupportedFruit;
  count: number;
}

/**
 * Contracts/Responses/InspectionSummaryResponse.cs
 *
 * byStatus and byFruit always list every enum member in declaration order,
 * zero-filled, so a chart has a stable set of categories even for a quiet week.
 */
export interface InspectionSummaryResponse {
  windowDays: number;
  since: string;
  totalScans: number;
  byStatus: StatusCount[];
  byFruit: FruitCount[];
  /** Null when nothing was scanned — not zero, which would read as "unripe". */
  averageRipenessPercent: number | null;
  lastScanAt: string | null;
}

/**
 * Contracts/Responses/FruitProfileResponse.cs — RipenessBandResponse
 *
 * businessGuidance and consumerGuidance are templates, not finished copy: where
 * the advice quotes the measured value it contains a `{0}` placeholder. A view
 * rendering the catalogue has to present that deliberately rather than raw.
 */
export interface RipenessBandResponse {
  minPercent: number;
  maxPercent: number;
  stageName: string;
  appearance: string;
  /** Representative colour of the fruit at this stage. */
  swatchHex: string;
  commercialStatus: CommercialStatus;
  edibility: EdibilityVerdict;
  businessGuidance: string;
  consumerGuidance: string;
}

/** Contracts/Responses/FruitProfileResponse.cs */
export interface FruitProfileResponse {
  fruitType: SupportedFruit;
  bands: RipenessBandResponse[];
}
