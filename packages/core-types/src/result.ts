/**
 * Result pattern for handling success/failure without exceptions.
 * Mirrors Mystira.Shared.Exceptions.Result<T> in C#.
 */

import { MystiraError } from './errors';

/**
 * Represents a result that can be either success or failure.
 */
export type Result<T, E = MystiraError> =
  | { success: true; value: T }
  | { success: false; error: E };

/**
 * Create a successful result.
 */
export function ok<T>(value: T): Result<T, never> {
  return { success: true, value };
}

/**
 * Create a failed result.
 */
export function err<E = MystiraError>(error: E): Result<never, E> {
  return { success: false, error };
}

/**
 * Check if a result is successful.
 */
export function isOk<T, E>(result: Result<T, E>): result is { success: true; value: T } {
  return result.success;
}

/**
 * Check if a result is a failure.
 */
export function isErr<T, E>(result: Result<T, E>): result is { success: false; error: E } {
  return !result.success;
}

/**
 * Unwrap a result, throwing if it's an error.
 */
export function unwrap<T, E>(result: Result<T, E>): T {
  if (result.success) {
    return result.value;
  }
  throw result.error;
}

/**
 * Unwrap a result with a default value.
 */
export function unwrapOr<T, E>(result: Result<T, E>, defaultValue: T): T {
  return result.success ? result.value : defaultValue;
}

/**
 * Map a successful result to a new value.
 */
export function map<T, U, E>(result: Result<T, E>, fn: (value: T) => U): Result<U, E> {
  return result.success ? ok(fn(result.value)) : result;
}

/**
 * Map a failed result to a new error.
 */
export function mapErr<T, E, F>(result: Result<T, E>, fn: (error: E) => F): Result<T, F> {
  return result.success ? result : err(fn(result.error));
}

/**
 * Flat map a successful result.
 */
export function flatMap<T, U, E>(
  result: Result<T, E>,
  fn: (value: T) => Result<U, E>
): Result<U, E> {
  return result.success ? fn(result.value) : result;
}
