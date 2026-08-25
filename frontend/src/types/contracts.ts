/**
 * Mirrors backend/src/Virentum.Api/Contracts/ field for field.
 *
 * Property names are camelCased by the backend's default JSON policy; no field
 * is added, renamed or reshaped here. Anything the API does not send does not
 * belong in this file.
 */
import type { CommercialStatus, SupportedFruit } from './enums';

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
 * the names `Image` and `FruitType`; see api/inspection.ts.
 */
export interface ScanRequest {
  image: File;
  fruitType: SupportedFruit;
}

/** Contracts/Responses/InspectionResponse.cs */
export interface InspectionResponse {
  fruitType: SupportedFruit;
  /** Whole percent, 0–100. */
  ripenessPercent: number;
  commercialStatus: CommercialStatus;
  recommendation: string;
  /** DateTimeOffset, ISO 8601 with offset. */
  scannedAt: string;
}
