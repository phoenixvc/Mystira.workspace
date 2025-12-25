import { describe, it, expect, vi } from 'vitest';
import {
  sleep,
  calculateBackoffDelay,
  withRetry,
  createRetryable,
} from './retry';

describe('sleep', () => {
  it('should resolve after specified time', async () => {
    const start = Date.now();
    await sleep(50);
    const elapsed = Date.now() - start;
    expect(elapsed).toBeGreaterThanOrEqual(45); // Allow some timing variance
  });
});

describe('calculateBackoffDelay', () => {
  it('should calculate exponential delay', () => {
    // Mock Math.random to return 0.5 (no jitter effect)
    vi.spyOn(Math, 'random').mockReturnValue(0.5);

    expect(calculateBackoffDelay(1, 1000, 30000, 2)).toBe(1000);
    expect(calculateBackoffDelay(2, 1000, 30000, 2)).toBe(2000);
    expect(calculateBackoffDelay(3, 1000, 30000, 2)).toBe(4000);
    expect(calculateBackoffDelay(4, 1000, 30000, 2)).toBe(8000);

    vi.restoreAllMocks();
  });

  it('should respect max delay', () => {
    vi.spyOn(Math, 'random').mockReturnValue(0.5);

    expect(calculateBackoffDelay(10, 1000, 5000, 2)).toBe(5000);

    vi.restoreAllMocks();
  });

  it('should add jitter', () => {
    // With random = 0, jitter = -10%
    vi.spyOn(Math, 'random').mockReturnValue(0);
    expect(calculateBackoffDelay(1, 1000, 30000, 2)).toBe(900);

    // With random = 1, jitter = +10%
    vi.spyOn(Math, 'random').mockReturnValue(1);
    expect(calculateBackoffDelay(1, 1000, 30000, 2)).toBe(1100);

    vi.restoreAllMocks();
  });
});

describe('withRetry', () => {
  it('should return result on first success', async () => {
    const fn = vi.fn().mockResolvedValue('success');

    const result = await withRetry(fn);

    expect(result).toBe('success');
    expect(fn).toHaveBeenCalledTimes(1);
  });

  it('should retry on failure and succeed', async () => {
    const fn = vi
      .fn()
      .mockRejectedValueOnce(new Error('fail 1'))
      .mockRejectedValueOnce(new Error('fail 2'))
      .mockResolvedValue('success');

    const result = await withRetry(fn, {
      maxAttempts: 3,
      initialDelayMs: 10,
    });

    expect(result).toBe('success');
    expect(fn).toHaveBeenCalledTimes(3);
  });

  it('should throw after max attempts', async () => {
    const fn = vi.fn().mockRejectedValue(new Error('always fails'));

    await expect(
      withRetry(fn, { maxAttempts: 3, initialDelayMs: 10 })
    ).rejects.toThrow('always fails');

    expect(fn).toHaveBeenCalledTimes(3);
  });

  it('should respect isRetryable option', async () => {
    const retryableError = new Error('retryable');
    const nonRetryableError = new Error('non-retryable');

    const fn = vi
      .fn()
      .mockRejectedValueOnce(retryableError)
      .mockRejectedValueOnce(nonRetryableError);

    await expect(
      withRetry(fn, {
        maxAttempts: 5,
        initialDelayMs: 10,
        isRetryable: (error) => (error as Error).message === 'retryable',
      })
    ).rejects.toThrow('non-retryable');

    expect(fn).toHaveBeenCalledTimes(2);
  });

  it('should call onRetry callback', async () => {
    const onRetry = vi.fn();
    const fn = vi
      .fn()
      .mockRejectedValueOnce(new Error('fail'))
      .mockResolvedValue('success');

    await withRetry(fn, {
      maxAttempts: 3,
      initialDelayMs: 10,
      onRetry,
    });

    expect(onRetry).toHaveBeenCalledTimes(1);
    expect(onRetry).toHaveBeenCalledWith(1, expect.any(Error), expect.any(Number));
  });
});

describe('createRetryable', () => {
  it('should create a retryable wrapper', async () => {
    const fn = vi.fn().mockResolvedValue('result');
    const retryableFn = createRetryable(fn, { maxAttempts: 3 });

    const result = await retryableFn();

    expect(result).toBe('result');
    expect(fn).toHaveBeenCalledTimes(1);
  });

  it('should pass arguments through', async () => {
    const fn = vi.fn().mockImplementation((a: number, b: string) =>
      Promise.resolve(`${a}-${b}`)
    );
    const retryableFn = createRetryable(fn, { maxAttempts: 3 });

    const result = await retryableFn(42, 'test');

    expect(result).toBe('42-test');
    expect(fn).toHaveBeenCalledWith(42, 'test');
  });
});
