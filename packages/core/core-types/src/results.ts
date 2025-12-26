/**
 * Core Result types for TypeScript.
 * These mirror the C# Result pattern in Mystira.Core.
 */

/**
 * Represents an error with a code and message.
 */
export interface Error {
  readonly code: string;
  readonly message: string;
  readonly metadata?: Readonly<Record<string, unknown>>;
}

/**
 * Represents a successful result.
 */
export interface Success<T> {
  readonly success: true;
  readonly value: T;
}

/**
 * Represents a failed result.
 */
export interface Failure<E = Error> {
  readonly success: false;
  readonly error: E;
}

/**
 * Represents the result of an operation that can succeed or fail.
 */
export type Result<T, E = Error> = Success<T> | Failure<E>;

/**
 * Result factory and utility functions.
 */
export const Result = {
  /**
   * Creates a successful result.
   */
  success: <T>(value: T): Success<T> => ({ success: true, value }),

  /**
   * Creates a failed result.
   */
  failure: <E = Error>(error: E): Failure<E> => ({ success: false, error }),

  /**
   * Checks if a result is successful.
   */
  isSuccess: <T, E>(result: Result<T, E>): result is Success<T> => result.success,

  /**
   * Checks if a result is a failure.
   */
  isFailure: <T, E>(result: Result<T, E>): result is Failure<E> => !result.success,

  /**
   * Maps a successful result to a new value.
   */
  map: <T, U, E>(result: Result<T, E>, fn: (value: T) => U): Result<U, E> =>
    result.success ? Result.success(fn(result.value)) : result,

  /**
   * Maps a successful result to a new result (flatMap/bind).
   */
  flatMap: <T, U, E>(
    result: Result<T, E>,
    fn: (value: T) => Result<U, E>
  ): Result<U, E> => (result.success ? fn(result.value) : result),

  /**
   * Maps the error of a failed result.
   */
  mapError: <T, E, F>(result: Result<T, E>, fn: (error: E) => F): Result<T, F> =>
    result.success ? result : Result.failure(fn(result.error)),

  /**
   * Pattern matches on the result.
   */
  match: <T, E, U>(
    result: Result<T, E>,
    handlers: {
      onSuccess: (value: T) => U;
      onFailure: (error: E) => U;
    }
  ): U =>
    result.success
      ? handlers.onSuccess(result.value)
      : handlers.onFailure(result.error),

  /**
   * Gets the value or a default if failed.
   */
  getOrElse: <T, E>(result: Result<T, E>, defaultValue: T): T =>
    result.success ? result.value : defaultValue,

  /**
   * Gets the value or computes a default if failed.
   */
  getOrElseWith: <T, E>(result: Result<T, E>, fn: (error: E) => T): T =>
    result.success ? result.value : fn(result.error),

  /**
   * Combines multiple results into a single result.
   * Returns the first error if any fail.
   */
  combine: <T, E>(results: readonly Result<T, E>[]): Result<readonly T[], E> => {
    const values: T[] = [];
    for (const result of results) {
      if (!result.success) return result;
      values.push(result.value);
    }
    return Result.success(values);
  },

  /**
   * Converts a nullable value to a Result.
   */
  fromNullable: <T>(value: T | null | undefined, error: Error): Result<T> =>
    value != null ? Result.success(value) : Result.failure(error),

  /**
   * Wraps an async function that may throw in a Result.
   */
  tryCatch: async <T>(fn: () => Promise<T>): Promise<Result<T>> => {
    try {
      return Result.success(await fn());
    } catch (e) {
      return Result.failure(Errors.fromException(e));
    }
  },

  /**
   * Wraps a sync function that may throw in a Result.
   */
  tryCatchSync: <T>(fn: () => T): Result<T> => {
    try {
      return Result.success(fn());
    } catch (e) {
      return Result.failure(Errors.fromException(e));
    }
  },
} as const;

/**
 * Error factory functions.
 */
export const Errors = {
  /**
   * Creates a "not found" error.
   */
  notFound: (resource: string, id?: string): Error => ({
    code: 'NOT_FOUND',
    message: id ? `${resource} '${id}' not found` : `${resource} not found`,
  }),

  /**
   * Creates a validation error.
   */
  validation: (message: string, field?: string): Error => ({
    code: 'VALIDATION',
    message,
    metadata: field ? { field } : undefined,
  }),

  /**
   * Creates an "unauthorized" error.
   */
  unauthorized: (reason?: string): Error => ({
    code: 'UNAUTHORIZED',
    message: reason ?? 'Authentication required',
  }),

  /**
   * Creates a "forbidden" error.
   */
  forbidden: (permission?: string): Error => ({
    code: 'FORBIDDEN',
    message: permission ? `Missing permission: ${permission}` : 'Access denied',
  }),

  /**
   * Creates a "conflict" error.
   */
  conflict: (message: string): Error => ({
    code: 'CONFLICT',
    message,
  }),

  /**
   * Creates an "internal" error.
   */
  internal: (message: string): Error => ({
    code: 'INTERNAL',
    message,
  }),

  /**
   * Creates an error from an exception.
   */
  fromException: (e: unknown): Error => ({
    code: 'EXCEPTION',
    message: e instanceof globalThis.Error ? e.message : String(e),
  }),
} as const;
