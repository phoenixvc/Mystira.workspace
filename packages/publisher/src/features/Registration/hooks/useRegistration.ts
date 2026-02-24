import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { storiesApi, chainApi, type RegistrationResponse } from '@/api';

export function useRegistration() {
  const queryClient = useQueryClient();
  const [registrationResult, setRegistrationResult] = useState<RegistrationResponse | null>(null);

  const submitMutation = useMutation({
    mutationFn: async (storyId: string) => {
      // First submit for approval via admin API
      const story = await storiesApi.submitForRegistration(storyId);

      // Then trigger on-chain registration
      const result = await chainApi.registerStory({
        storyId: story.id,
        metadata: {
          title: story.title,
          summary: story.summary,
          createdAt: story.createdAt,
        },
        contributors: story.contributors.map(c => ({
          userId: c.userId,
          role: c.role,
          splitPercentage: c.split,
        })),
      });

      return result;
    },
    onSuccess: result => {
      setRegistrationResult(result);
      // Invalidate queries to refresh story data
      queryClient.invalidateQueries({ queryKey: ['stories'] });
    },
  });

  return {
    register: submitMutation.mutateAsync,
    isRegistering: submitMutation.isPending,
    registrationError: submitMutation.error,
    registrationResult,
    reset: () => {
      submitMutation.reset();
      setRegistrationResult(null);
    },
  };
}
