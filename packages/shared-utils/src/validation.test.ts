import { describe, it, expect } from 'vitest';
import {
  isDefined,
  isNonEmptyString,
  isPositiveNumber,
  isValidEmail,
  isValidUrl,
  createValidationResult,
  combineValidationResults,
  createValidationError,
  validateRequired,
} from './validation';

describe('isDefined', () => {
  it('should return true for defined values', () => {
    expect(isDefined(0)).toBe(true);
    expect(isDefined('')).toBe(true);
    expect(isDefined(false)).toBe(true);
    expect(isDefined({})).toBe(true);
    expect(isDefined([])).toBe(true);
  });

  it('should return false for null and undefined', () => {
    expect(isDefined(null)).toBe(false);
    expect(isDefined(undefined)).toBe(false);
  });
});

describe('isNonEmptyString', () => {
  it('should return true for non-empty strings', () => {
    expect(isNonEmptyString('hello')).toBe(true);
    expect(isNonEmptyString('  hello  ')).toBe(true);
  });

  it('should return false for empty or whitespace strings', () => {
    expect(isNonEmptyString('')).toBe(false);
    expect(isNonEmptyString('   ')).toBe(false);
  });

  it('should return false for non-strings', () => {
    expect(isNonEmptyString(123)).toBe(false);
    expect(isNonEmptyString(null)).toBe(false);
    expect(isNonEmptyString(undefined)).toBe(false);
    expect(isNonEmptyString({})).toBe(false);
  });
});

describe('isPositiveNumber', () => {
  it('should return true for positive numbers', () => {
    expect(isPositiveNumber(1)).toBe(true);
    expect(isPositiveNumber(0.5)).toBe(true);
    expect(isPositiveNumber(1000)).toBe(true);
  });

  it('should return false for zero and negative numbers', () => {
    expect(isPositiveNumber(0)).toBe(false);
    expect(isPositiveNumber(-1)).toBe(false);
    expect(isPositiveNumber(-0.5)).toBe(false);
  });

  it('should return false for non-numbers', () => {
    expect(isPositiveNumber('123')).toBe(false);
    expect(isPositiveNumber(NaN)).toBe(false);
    expect(isPositiveNumber(null)).toBe(false);
  });
});

describe('isValidEmail', () => {
  it('should return true for valid emails', () => {
    expect(isValidEmail('test@example.com')).toBe(true);
    expect(isValidEmail('user.name@domain.org')).toBe(true);
    expect(isValidEmail('user+tag@example.co.uk')).toBe(true);
  });

  it('should return false for invalid emails', () => {
    expect(isValidEmail('invalid')).toBe(false);
    expect(isValidEmail('invalid@')).toBe(false);
    expect(isValidEmail('@domain.com')).toBe(false);
    expect(isValidEmail('user@domain')).toBe(false);
    expect(isValidEmail('')).toBe(false);
  });

  it('should return false for non-strings', () => {
    expect(isValidEmail(123)).toBe(false);
    expect(isValidEmail(null)).toBe(false);
  });
});

describe('isValidUrl', () => {
  it('should return true for valid URLs', () => {
    expect(isValidUrl('https://example.com')).toBe(true);
    expect(isValidUrl('http://localhost:3000')).toBe(true);
    expect(isValidUrl('https://example.com/path?query=value')).toBe(true);
  });

  it('should return false for invalid URLs', () => {
    expect(isValidUrl('not a url')).toBe(false);
    expect(isValidUrl('example.com')).toBe(false);
    expect(isValidUrl('')).toBe(false);
  });

  it('should return false for non-strings', () => {
    expect(isValidUrl(123)).toBe(false);
    expect(isValidUrl(null)).toBe(false);
  });
});

describe('createValidationResult', () => {
  it('should create valid result when no errors', () => {
    const result = createValidationResult();
    expect(result.valid).toBe(true);
    expect(result.errors).toEqual([]);
  });

  it('should create invalid result when errors provided', () => {
    const errors = [createValidationError('field', 'error message', 'CODE')];
    const result = createValidationResult(errors);
    expect(result.valid).toBe(false);
    expect(result.errors).toHaveLength(1);
  });
});

describe('combineValidationResults', () => {
  it('should combine multiple results', () => {
    const result1 = createValidationResult([
      createValidationError('field1', 'error 1', 'CODE1'),
    ]);
    const result2 = createValidationResult([
      createValidationError('field2', 'error 2', 'CODE2'),
    ]);
    const result3 = createValidationResult();

    const combined = combineValidationResults(result1, result2, result3);

    expect(combined.valid).toBe(false);
    expect(combined.errors).toHaveLength(2);
  });

  it('should return valid when all results are valid', () => {
    const result1 = createValidationResult();
    const result2 = createValidationResult();

    const combined = combineValidationResults(result1, result2);

    expect(combined.valid).toBe(true);
    expect(combined.errors).toHaveLength(0);
  });
});

describe('validateRequired', () => {
  it('should return valid when all fields present', () => {
    const obj = { name: 'John', email: 'john@example.com' };
    const result = validateRequired(obj, ['name', 'email']);

    expect(result.valid).toBe(true);
    expect(result.errors).toHaveLength(0);
  });

  it('should return errors for missing fields', () => {
    const obj = { name: 'John' };
    const result = validateRequired(obj, ['name', 'email', 'age']);

    expect(result.valid).toBe(false);
    expect(result.errors).toHaveLength(2);
    expect(result.errors[0].path).toBe('email');
    expect(result.errors[1].path).toBe('age');
  });

  it('should treat null and undefined as missing', () => {
    const obj = { name: null, email: undefined };
    const result = validateRequired(obj, ['name', 'email']);

    expect(result.valid).toBe(false);
    expect(result.errors).toHaveLength(2);
  });

  it('should treat empty string and zero as present', () => {
    const obj = { name: '', count: 0 };
    const result = validateRequired(obj, ['name', 'count']);

    expect(result.valid).toBe(true);
  });
});
