import { useCallback } from 'react';
import { useToast } from './useToast';
import { logger } from '@/utils/logger';
import { ApiRequestError } from '@/api/client';

interface ErrorHandlerOptions {
  showToast?: boolean;
  logError?: boolean;
  fallbackMessage?: string;
}

export function useErrorHandler() {
  const { error: showErrorToast } = useToast();

  const handleError = useCallback(
    (error: unknown, options: ErrorHandlerOptions = {}) => {
      const {
        showToast = true,
        logError = true,
        fallbackMessage = 'An unexpected error occurred',
      } = options;

      let errorMessage = fallbackMessage;
      let errorDetails: string[] = [];

      // Extract error message based on error type
      if (error instanceof ApiRequestError) {
        errorMessage = error.message || fallbackMessage;
        errorDetails = error.errors.map(e => e.message || e.field || 'Unknown error');
      } else if (error instanceof Error) {
        errorMessage = error.message || fallbackMessage;
      } else if (typeof error === 'string') {
        errorMessage = error;
      }

      // Log error
      if (logError) {
        logger.error('Error handled:', {
          message: errorMessage,
          details: errorDetails,
          error,
        });
      }

      // Show toast notification
      if (showToast) {
        showErrorToast(
          errorMessage,
          errorDetails.length > 0 ? errorDetails.join(', ') : undefined
        );
      }

      return { message: errorMessage, details: errorDetails };
    },
    [showErrorToast]
  );

  const handleApiError = useCallback(
    (error: unknown, context?: string) => {
      return handleError(error, {
        showToast: true,
        logError: true,
        fallbackMessage: context
          ? `Failed to ${context}. Please try again.`
          : 'An error occurred while processing your request.',
      });
    },
    [handleError]
  );

  const handleValidationError = useCallback(
    (error: unknown) => {
      return handleError(error, {
        showToast: true,
        logError: false,
        fallbackMessage: 'Please check your input and try again.',
      });
    },
    [handleError]
  );

  const handleNetworkError = useCallback(
    (error: unknown) => {
      // Detect network errors
      const isNetworkError =
        (error instanceof Error &&
          (error.message.includes('Network Error') ||
            error.message.includes('timeout') ||
            error.message.includes('Failed to fetch'))) ||
        (typeof navigator !== 'undefined' && !navigator.onLine);

      return handleError(error, {
        showToast: true,
        logError: true,
        fallbackMessage: isNetworkError
          ? 'Network error. Please check your connection and try again.'
          : 'An error occurred while processing your request.',
      });
    },
    [handleError]
  );

  return {
    handleError,
    handleApiError,
    handleValidationError,
    handleNetworkError,
  };
}

