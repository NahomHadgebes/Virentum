/**
 * Client-side mirror of the DataAnnotations on
 * backend/src/Virentum.Api/Contracts/Requests/LoginRequest.cs:
 *
 *   [Required(AllowEmptyStrings = false)] string StoreId
 *   [Required(AllowEmptyStrings = false)] [MinLength(6)] string Password
 *
 * No rule here exists that the backend does not enforce. The wording is ours;
 * the rules are not. Validating here only saves a round trip — the server
 * remains the authority, and its ValidationProblemDetails are surfaced either
 * way.
 */

/** From [MinLength(6)] on Password. */
export const PASSWORD_MIN_LENGTH = 6;

/**
 * RequiredAttribute trims before testing when AllowEmptyStrings is false, so a
 * whitespace-only value fails on the server too.
 */
function isBlank(value: string): boolean {
  return value.trim().length === 0;
}

export function validateStoreId(value: string): string | null {
  return isBlank(value) ? 'Store id is required.' : null;
}

export function validatePassword(value: string): string | null {
  if (isBlank(value)) {
    return 'Password is required.';
  }

  // MinLength counts the raw string; unlike Required it does not trim.
  if (value.length < PASSWORD_MIN_LENGTH) {
    return `Password must be at least ${String(PASSWORD_MIN_LENGTH)} characters.`;
  }

  return null;
}
