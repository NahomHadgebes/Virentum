/**
 * Client-side mirror of the checks in
 * backend/src/Virentum.Api/Services/Inspection/InspectionService.cs
 * (ReadAndValidateImageAsync):
 *
 *   Image is null or Length == 0        -> "An image file is required."
 *   Length > 8 MB                        -> "Image exceeds the 8 MB limit."
 *   ContentType not in the allow list    -> "Unsupported image content type '...'."
 *
 * The allow list is narrower than Mantine's IMAGE_MIME_TYPE, which also covers
 * gif, svg, avif, heic and heif — all of which the API rejects. Using the
 * backend's list is the whole point; do not widen it here.
 */

/** MaxImageBytes in InspectionService. */
export const MAX_IMAGE_BYTES = 8 * 1024 * 1024;

/** AllowedContentTypes in InspectionService, in the same order. */
export const ALLOWED_IMAGE_CONTENT_TYPES = [
  'image/jpeg',
  'image/jpg',
  'image/png',
  'image/webp',
] as const;

/**
 * The Content-Type the browser will actually put on the multipart part. A file
 * the OS could not type reaches the server as application/octet-stream, so that
 * is what the message should name.
 */
function contentTypeOf(file: File): string {
  return file.type === '' ? 'application/octet-stream' : file.type;
}

export function validateImage(file: File): string | null {
  if (file.size === 0) {
    return 'An image file is required.';
  }

  if (file.size > MAX_IMAGE_BYTES) {
    return `Image exceeds the ${String(MAX_IMAGE_BYTES / (1024 * 1024))} MB limit.`;
  }

  const contentType = contentTypeOf(file);
  if (!ALLOWED_IMAGE_CONTENT_TYPES.some((allowed) => allowed === contentType)) {
    return `Unsupported image content type '${contentType}'.`;
  }

  return null;
}
