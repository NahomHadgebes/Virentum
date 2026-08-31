import { describe, expect, it } from 'vitest';
import { PASSWORD_MIN_LENGTH, validatePassword, validateStoreId } from './credentials';

/**
 * These assertions encode LoginRequest.cs. If the backend's attributes change,
 * these tests should fail before a user ever sees a mismatch.
 */
describe('validateStoreId — [Required(AllowEmptyStrings = false)]', () => {
  it('accepts a normal store id', () => {
    expect(validateStoreId('demo-store')).toBeNull();
  });

  it('rejects an empty string', () => {
    expect(validateStoreId('')).toBe('Store id is required.');
  });

  it('rejects whitespace only, because RequiredAttribute trims first', () => {
    expect(validateStoreId('   ')).toBe('Store id is required.');
  });

  it('imposes no length rule of its own', () => {
    expect(validateStoreId('a')).toBeNull();
  });
});

describe('validatePassword — [Required] + [MinLength(6)]', () => {
  it('accepts a password at exactly the minimum length', () => {
    expect(validatePassword('a'.repeat(PASSWORD_MIN_LENGTH))).toBeNull();
  });

  it('rejects one character below the minimum', () => {
    expect(validatePassword('a'.repeat(PASSWORD_MIN_LENGTH - 1))).toBe(
      'Password must be at least 6 characters.',
    );
  });

  it('rejects an empty string as required, not as too short', () => {
    expect(validatePassword('')).toBe('Password is required.');
  });

  /**
   * The asymmetry worth pinning down: Required trims before testing, MinLength
   * does not. Six spaces are long enough for MinLength but still fail Required,
   * on the server and here.
   */
  it('rejects six spaces as required rather than accepting them on length', () => {
    expect(validatePassword('      ')).toBe('Password is required.');
  });

  it('counts the raw string, so leading spaces do count toward the minimum', () => {
    expect(validatePassword('  abcd')).toBeNull();
  });
});
