import { useMutation, UseMutationOptions, UseMutationResult } from '@tanstack/react-query';
import { useErrorHandler } from './useErrorHandler';
import { useToast } from './useToast';

interface MutationWithErrorHandlingOptions<TData, TError, TVariables, TContext> extends Omit<
  UseMutationOptions<TData, TError, TVariables, TContext>,
  'onError' | 'onSuccess'
> {
  successMessage?: string;
  errorContext?: string;
  onError?: (error: TError, variables: TVariables, context: TContext | undefined) => void;
  onSuccess?: (data: TData, variables: TVariables, context: TContext | undefined) => void;
}

export function useMutationWithErrorHandling<
  TData = unknown,
  TError = unknown,
  TVariables = void,
  TContext = unknown,
>(
  options: MutationWithErrorHandlingOptions<TData, TError, TVariables, TContext>
): UseMutationResult<TData, TError, TVariables, TContext> {
  const { handleApiError } = useErrorHandler();
  const { success } = useToast();

  const { successMessage, errorContext, onError, onSuccess, ...mutationOptions } = options;

  return useMutation({
    ...mutationOptions,
    onSuccess: (data, variables, context) => {
      if (successMessage) {
        success(successMessage);
      }
      onSuccess?.(data, variables, context);
    },
    onError: (error, variables, context) => {
      handleApiError(error, errorContext);
      onError?.(error, variables, context);
    },
  });
}
