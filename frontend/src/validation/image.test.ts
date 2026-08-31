import { describe, expect, it } from 'vitest';
import {
  ALLOWED_IMAGE_CONTENT_TYPES,
  MAX_IMAGE_BYTES,
  validateImage,
} from './image';

function fileOf(bytes: number, type: string, name = 'produce.png'): File {
  return new File([new Uint8Array(bytes)], name, { type });
}

/** These assertions encode ReadAndValidateImageAsync in InspectionService.cs. */
describe('validateImage', () => {
  it('accepts every content type the API allows', () => {
    for (const type of ALLOWED_IMAGE_CONTENT_TYPES) {
      expect(validateImage(fileOf(1024, type))).toBeNull();
    }
  });

  it('rejects an empty file as missing, matching the API wording', () => {
    expect(validateImage(fileOf(0, 'image/png'))).toBe('An image file is required.');
  });

  it('accepts a file at exactly the 8 MB ceiling', () => {
    expect(validateImage(fileOf(MAX_IMAGE_BYTES, 'image/png'))).toBeNull();
  });

  it('rejects one byte over the ceiling', () => {
    expect(validateImage(fileOf(MAX_IMAGE_BYTES + 1, 'image/png'))).toBe(
      'Image exceeds the 8 MB limit.',
    );
  });

  it('names the offending content type, as the API does', () => {
    expect(validateImage(fileOf(1024, 'application/pdf', 'sheet.pdf'))).toBe(
      "Unsupported image content type 'application/pdf'.",
    );
  });

  /**
   * Mantine's IMAGE_MIME_TYPE covers these; the API does not. Widening the
   * client list would let files through that the server then rejects.
   */
  it.each(['image/gif', 'image/svg+xml', 'image/avif', 'image/heic'])(
    'rejects %s, which the API does not accept',
    (type) => {
      expect(validateImage(fileOf(1024, type))).toContain('Unsupported image content type');
    },
  );

  it('reports an untyped file as application/octet-stream, which is what the browser sends', () => {
    expect(validateImage(fileOf(1024, '', 'mystery'))).toBe(
      "Unsupported image content type 'application/octet-stream'.",
    );
  });

  it('checks size before content type, so an empty pdf reads as missing', () => {
    expect(validateImage(fileOf(0, 'application/pdf'))).toBe('An image file is required.');
  });
});
