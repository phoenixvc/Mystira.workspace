import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { contributorsApi } from '@/api';
import type { AddContributorRequest, Attribution } from '@/api/types';

export function useContributors(storyId: string) {
  const queryClient = useQueryClient();

  const { data: contributors, isLoading, error } = useQuery({
    queryKey: ['contributors', storyId],
    queryFn: () => contributorsApi.getByStory(storyId),
    enabled: !!storyId,
  });

  const addMutation = useMutation({
    mutationFn: (data: AddContributorRequest) => contributorsApi.add(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contributors', storyId] });
      queryClient.invalidateQueries({ queryKey: ['stories'] });
    },
  });

  const removeMutation = useMutation({
    mutationFn: (contributorId: string) => contributorsApi.remove(contributorId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contributors', storyId] });
      queryClient.invalidateQueries({ queryKey: ['stories'] });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Attribution> }) =>
      contributorsApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contributors', storyId] });
    },
  });

  return {
    contributors,
    isLoading,
    error,
    addContributor: addMutation.mutateAsync,
    isAdding: addMutation.isPending,
    addError: addMutation.error,
    removeContributor: removeMutation.mutate,
    isRemoving: removeMutation.isPending,
    updateContributor: updateMutation.mutateAsync,
    isUpdating: updateMutation.isPending,
  };
}
