// Validation utilities

export function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

export function isValidSplits(splits: number[]): { valid: boolean; total: number; error?: string } {
  const total = splits.reduce((sum, split) => sum + split, 0);

  if (splits.some(split => split < 0)) {
    return { valid: false, total, error: 'Splits cannot be negative' };
  }

  if (splits.some(split => split > 100)) {
    return { valid: false, total, error: 'Individual splits cannot exceed 100%' };
  }

  if (total !== 100) {
    return {
      valid: false,
      total,
      error: total < 100 ? `${100 - total}% remaining` : `${total - 100}% over allocation`,
    };
  }

  return { valid: true, total };
}

export function isNotEmpty(value: unknown): boolean {
  if (value === null || value === undefined) return false;
  if (typeof value === 'string') return value.length > 0;
  if (Array.isArray(value)) return value.length > 0;
  if (typeof value === 'object') return Object.keys(value).length > 0;
  return true;
}

export function isValidLength(value: string, min: number, max: number): boolean {
  const length = value.trim().length;
  return length >= min && length <= max;
}

export function hasMinContributors(contributors: unknown[], min: number = 1): boolean {
  return contributors.length >= min;
}

export function allApproved(approvalStatuses: string[]): boolean {
  return approvalStatuses.every(status => status === 'approved' || status === 'overridden');
}
