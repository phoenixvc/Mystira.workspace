import { useMutation, useQueryClient } from '@tanstack/react-query';
import { contributorsApi } from '@/api';
import type { ApprovalRequest, OverrideRequest } from '@/api/types';

export function useApproval() {
  const queryClient = useQueryClient();

  const approvalMutation = useMutation({
    mutationFn: (data: ApprovalRequest) => contributorsApi.submitApproval(data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['contributors', variables.storyId] });
      queryClient.invalidateQueries({ queryKey: ['stories'] });
    },
  });

  const overrideMutation = useMutation({
    mutationFn: (data: OverrideRequest) => contributorsApi.override(data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['contributors', variables.storyId] });
      queryClient.invalidateQueries({ queryKey: ['stories'] });
    },
  });

  return {
    submitApproval: approvalMutation.mutate,
    isSubmitting: approvalMutation.isPending,
    approvalError: approvalMutation.error,
    override: overrideMutation.mutate,
    isOverriding: overrideMutation.isPending,
    overrideError: overrideMutation.error,
  };
}
