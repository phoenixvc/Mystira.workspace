import { describe, it, expect } from 'vitest';
import { formatDate, formatDateTime, formatRelativeTime, formatRole } from '@/utils/format';

describe('format utilities', () => {
  describe('formatDate', () => {
    it('formats a date string', () => {
      const date = '2024-01-15T10:30:00Z';
      const formatted = formatDate(date);
      expect(formatted).toMatch(/Jan/);
      expect(formatted).toMatch(/2024/);
    });

    it('formats a Date object', () => {
      const date = new Date('2024-01-15T10:30:00Z');
      const formatted = formatDate(date);
      expect(formatted).toMatch(/Jan/);
    });
  });

  describe('formatDateTime', () => {
    it('formats date and time', () => {
      const date = '2024-01-15T10:30:00Z';
      const formatted = formatDateTime(date);
      expect(formatted).toMatch(/Jan/);
      expect(formatted).toMatch(/2024/);
      expect(formatted).toMatch(/\d{1,2}:\d{2}/); // Matches time format HH:MM
    });
  });

  describe('formatRelativeTime', () => {
    it('formats recent time as "Just now"', () => {
      const date = new Date();
      const formatted = formatRelativeTime(date);
      expect(formatted).toBe('Just now');
    });

    it('formats time as minutes ago', () => {
      const date = new Date(Date.now() - 5 * 60 * 1000);
      const formatted = formatRelativeTime(date);
      expect(formatted).toMatch(/minute/);
    });

    it('formats time as hours ago', () => {
      const date = new Date(Date.now() - 2 * 60 * 60 * 1000);
      const formatted = formatRelativeTime(date);
      expect(formatted).toMatch(/hour/);
    });
  });

  describe('formatRole', () => {
    it('formats snake_case role to Title Case', () => {
      expect(formatRole('primary_author')).toBe('Primary Author');
      expect(formatRole('co_author')).toBe('Co Author');
      expect(formatRole('illustrator')).toBe('Illustrator');
    });
  });
});

