/**
 * Simple validation utilities
 */

export interface ValidationResult {
  valid: boolean;
  errors: ValidationError[];
}

export interface ValidationError {
  path: string;
  message: string;
  code: string;
}

export type Validator<T> = (value: unknown) => ValidationResult & { value?: T };

/**
 * Check if a value is defined (not null or undefined)
 */
export function isDefined<T>(value: T | null | undefined): value is T {
  return value !== null && value !== undefined;
}

/**
 * Check if a value is a non-empty string
 */
export function isNonEmptyString(value: unknown): value is string {
  return typeof value === 'string' && value.trim().length > 0;
}

/**
 * Check if a value is a positive number
 */
export function isPositiveNumber(value: unknown): value is number {
  return typeof value === 'number' && !isNaN(value) && value > 0;
}

/**
 * Check if a value is a valid email format
 */
export function isValidEmail(value: unknown): value is string {
  if (typeof value !== 'string') return false;
  // Simple email regex - not comprehensive but catches most cases
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(value);
}

/**
 * Check if a value is a valid URL
 */
export function isValidUrl(value: unknown): value is string {
  if (typeof value !== 'string') return false;
  try {
    new URL(value);
    return true;
  } catch {
    return false;
  }
}

/**
 * Create a validation result
 */
export function createValidationResult(errors: ValidationError[] = []): ValidationResult {
  return {
    valid: errors.length === 0,
    errors,
  };
}

/**
 * Combine multiple validation results
 */
export function combineValidationResults(...results: ValidationResult[]): ValidationResult {
  const errors = results.flatMap((r) => r.errors);
  return createValidationResult(errors);
}

/**
 * Create a validation error
 */
export function createValidationError(
  path: string,
  message: string,
  code: string = 'VALIDATION_ERROR'
): ValidationError {
  return { path, message, code };
}

/**
 * Validate required fields on an object
 *
 * @example
 * ```typescript
 * const result = validateRequired(data, ['name', 'email']);
 * if (!result.valid) {
 *   console.log(result.errors);
 * }
 * ```
 */
export function validateRequired(
  obj: Record<string, unknown>,
  fields: string[]
): ValidationResult {
  const errors: ValidationError[] = [];

  for (const field of fields) {
    if (!isDefined(obj[field])) {
      errors.push(createValidationError(field, `${field} is required`, 'REQUIRED'));
    }
  }

  return createValidationResult(errors);
}
