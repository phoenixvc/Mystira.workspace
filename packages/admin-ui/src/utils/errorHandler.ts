import { AxiosError } from "axios";
import { errorReportingService } from "../services/errorReporting";
import { showToast } from "./toast";

export interface ApiError {
  message: string;
  status?: number;
  code?: string;
  details?: unknown;
}

/**
 * Extracts error information from various error types
 */
export function parseError(error: unknown): ApiError {
  // Axios error
  if (error instanceof AxiosError) {
    const status = error.response?.status;
    const data = error.response?.data;

    return {
      message: data?.message || error.message || "An error occurred",
      status,
      code: data?.code || error.code,
      details: data,
    };
  }

  // Standard Error
  if (error instanceof Error) {
    return {
      message: error.message,
      details: error.stack,
    };
  }

  // String error
  if (typeof error === "string") {
    return {
      message: error,
    };
  }

  // Unknown error type
  return {
    message: "An unexpected error occurred",
    details: error,
  };
}

/**
 * Formats error message for display
 */
export function formatErrorMessage(error: ApiError): string {
  if (error.status) {
    return `Error ${error.status}: ${error.message}`;
  }
  return error.message;
}

/**
 * Handles API errors with toast notifications
 */
export function handleApiError(error: unknown, customMessage?: string): void {
  const apiError = parseError(error);
  const message = customMessage || formatErrorMessage(apiError);

  showToast.error(message);

  // Report to error reporting service
  errorReportingService.reportApiError(apiError.message, apiError.status, apiError.code, {
    source: "handleApiError",
    extra: { details: apiError.details },
  });
}

/**
 * Gets user-friendly error message based on status code
 */
export function getErrorMessageByStatus(status: number): string {
  const messages: Record<number, string> = {
    400: "Bad request. Please check your input and try again.",
    401: "Unauthorized. Please log in and try again.",
    403: "Forbidden. You don't have permission to perform this action.",
    404: "Not found. The requested resource doesn't exist.",
    409: "Conflict. The resource already exists or there's a conflict.",
    422: "Validation error. Please check your input.",
    429: "Too many requests. Please try again later.",
    500: "Internal server error. Please try again later.",
    502: "Bad gateway. The server is temporarily unavailable.",
    503: "Service unavailable. Please try again later.",
    504: "Gateway timeout. The request took too long.",
  };

  return messages[status] || `Error ${status}: An error occurred`;
}

/**
 * Checks if error is a network error
 */
export function isNetworkError(error: unknown): boolean {
  if (error instanceof AxiosError) {
    return !error.response && !!error.request;
  }
  return false;
}

/**
 * Checks if error is an authentication error
 */
export function isAuthError(error: unknown): boolean {
  const apiError = parseError(error);
  return apiError.status === 401 || apiError.status === 403;
}

/**
 * Checks if error is a validation error
 */
export function isValidationError(error: unknown): boolean {
  const apiError = parseError(error);
  return apiError.status === 400 || apiError.status === 422;
}

/**
 * Retry helper for failed requests
 */
export async function retryWithBackoff<T>(
  fn: () => Promise<T>,
  maxRetries = 3,
  baseDelay = 1000
): Promise<T> {
  // Handle edge case: if maxRetries is 0 or negative, execute once without retry
  if (maxRetries <= 0) {
    return await fn();
  }

  let lastError: unknown;

  for (let i = 0; i < maxRetries; i++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error;

      // Don't retry on client errors (4xx)
      const apiError = parseError(error);
      if (apiError.status && apiError.status >= 400 && apiError.status < 500) {
        throw error;
      }

      // Wait before retrying (exponential backoff)
      if (i < maxRetries - 1) {
        const delay = baseDelay * Math.pow(2, i);
        await new Promise(resolve => setTimeout(resolve, delay));
      }
    }
  }

  throw lastError;
}
