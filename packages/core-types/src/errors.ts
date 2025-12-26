/**
 * Error types for Mystira platform.
 * Mirrors Mystira.Shared.Exceptions in C#.
 */

/**
 * Standard error codes used across the platform.
 */
export type ErrorCode =
  | 'VALIDATION'
  | 'NOT_FOUND'
  | 'CONFLICT'
  | 'UNAUTHORIZED'
  | 'FORBIDDEN'
  | 'RATE_LIMITED'
  | 'INTERNAL'
  | 'SERVICE_UNAVAILABLE'
  | 'BAD_REQUEST';

/**
 * Base error interface for all Mystira errors.
 * Mirrors C# MystiraException.
 */
export interface MystiraError {
  /** Error code for programmatic handling */
  code: ErrorCode;
  /** Human-readable error message */
  message: string;
  /** Optional error details */
  details?: Record<string, unknown>;
  /** Optional correlation ID for tracing */
  correlationId?: string;
  /** Optional timestamp */
  timestamp?: string;
}

/**
 * Standard API error response format.
 * Mirrors C# ErrorResponse and RFC 7807 Problem Details.
 */
export interface ErrorResponse {
  /** HTTP status code */
  status: number;
  /** Error type URI */
  type?: string;
  /** Short error title */
  title: string;
  /** Detailed error message */
  detail?: string;
  /** Request instance identifier */
  instance?: string;
  /** Error code for programmatic handling */
  code: ErrorCode;
  /** Correlation ID for tracing */
  correlationId?: string;
  /** Validation errors by field */
  errors?: Record<string, string[]>;
  /** Additional metadata */
  metadata?: Record<string, unknown>;
}

/**
 * Create a validation error.
 */
export function validationError(
  message: string,
  errors?: Record<string, string[]>
): MystiraError {
  return {
    code: 'VALIDATION',
    message,
    details: errors ? { errors } : undefined,
  };
}

/**
 * Create a not found error.
 */
export function notFoundError(resource: string, id?: string): MystiraError {
  return {
    code: 'NOT_FOUND',
    message: id ? `${resource} with ID '${id}' not found` : `${resource} not found`,
    details: { resource, id },
  };
}

/**
 * Create a conflict error.
 */
export function conflictError(message: string): MystiraError {
  return {
    code: 'CONFLICT',
    message,
  };
}

/**
 * Create an unauthorized error.
 */
export function unauthorizedError(message = 'Authentication required'): MystiraError {
  return {
    code: 'UNAUTHORIZED',
    message,
  };
}

/**
 * Create a forbidden error.
 */
export function forbiddenError(message = 'Access denied'): MystiraError {
  return {
    code: 'FORBIDDEN',
    message,
  };
}

/**
 * Create a rate limit error.
 */
export function rateLimitError(retryAfterSeconds?: number): MystiraError {
  return {
    code: 'RATE_LIMITED',
    message: 'Rate limit exceeded',
    details: retryAfterSeconds ? { retryAfterSeconds } : undefined,
  };
}

/**
 * Create an internal error.
 */
export function internalError(message = 'An unexpected error occurred'): MystiraError {
  return {
    code: 'INTERNAL',
    message,
  };
}

/**
 * Convert a MystiraError to an ErrorResponse.
 */
export function toErrorResponse(error: MystiraError, status?: number): ErrorResponse {
  const statusCode = status ?? getDefaultStatus(error.code);
  return {
    status: statusCode,
    title: error.code,
    detail: error.message,
    code: error.code,
    correlationId: error.correlationId,
    errors: error.details?.errors as Record<string, string[]> | undefined,
    metadata: error.details,
  };
}

function getDefaultStatus(code: ErrorCode): number {
  switch (code) {
    case 'VALIDATION':
    case 'BAD_REQUEST':
      return 400;
    case 'UNAUTHORIZED':
      return 401;
    case 'FORBIDDEN':
      return 403;
    case 'NOT_FOUND':
      return 404;
    case 'CONFLICT':
      return 409;
    case 'RATE_LIMITED':
      return 429;
    case 'SERVICE_UNAVAILABLE':
      return 503;
    case 'INTERNAL':
    default:
      return 500;
  }
}
