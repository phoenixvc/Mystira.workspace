import { describe, it, expect } from 'vitest';
import { isValidSplits, isNotEmpty, isValidLength } from '@/utils/validation';

describe('validation utilities', () => {
  describe('isValidSplits', () => {
    it('returns valid for splits that sum to 100', () => {
      const result = isValidSplits([50, 30, 20]);
      expect(result.valid).toBe(true);
      expect(result.total).toBe(100);
    });

    it('returns invalid for splits that do not sum to 100', () => {
      const result = isValidSplits([50, 30, 10]);
      expect(result.valid).toBe(false);
      expect(result.total).toBe(90);
      expect(result.error).toBeDefined();
    });

    it('handles empty array', () => {
      const result = isValidSplits([]);
      expect(result.valid).toBe(false);
      expect(result.total).toBe(0);
    });
  });

  describe('isNotEmpty', () => {
    it('returns true for non-empty strings', () => {
      expect(isNotEmpty('hello')).toBe(true);
      expect(isNotEmpty('world')).toBe(true);
    });

    it('returns true for strings with whitespace only', () => {
      expect(isNotEmpty(' ')).toBe(true);
      expect(isNotEmpty('   ')).toBe(true);
    });

    it('returns false for empty strings', () => {
      expect(isNotEmpty('')).toBe(false);
    });

    it('returns false for null/undefined', () => {
      expect(isNotEmpty(null)).toBe(false);
      expect(isNotEmpty(undefined)).toBe(false);
    });
  });

  describe('isValidLength', () => {
    it('returns true for strings within range', () => {
      expect(isValidLength('hello', 1, 10)).toBe(true);
      expect(isValidLength('hello', 5, 5)).toBe(true);
    });

    it('returns false for strings outside range', () => {
      expect(isValidLength('hello', 10, 20)).toBe(false);
      expect(isValidLength('hello', 1, 3)).toBe(false);
    });
  });
});

