import { authApi } from "@/api";
import type { DualPathLoginRequest } from "@/api/auth";
import { useAuthStore } from "@/state/authStore";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useCallback } from "react";

export function useAuth() {
  const queryClient = useQueryClient();
  const { user, setUser, clearUser, isAuthenticated } = useAuthStore();

  // Fetch current user on mount if token exists
  const { isLoading: isCheckingAuth } = useQuery({
    queryKey: ["auth", "me"],
    queryFn: async () => {
      const user = await authApi.getCurrentUser();
      setUser(user);
      return user;
    },
    enabled: !!localStorage.getItem("accessToken") && !user,
    retry: false,
  });

  const loginMutation = useMutation({
    mutationFn: (data: DualPathLoginRequest) => authApi.login(data),
    onSuccess: (response) => {
      setUser(response.user);
      queryClient.invalidateQueries();
    },
  });

  const entraLoginMutation = useMutation({
    mutationFn: (token: string) => authApi.loginWithEntra(token),
    onSuccess: (response) => {
      setUser(response.user);
      queryClient.invalidateQueries();
    },
  });

  const magicLinkRequestMutation = useMutation({
    mutationFn: (email: string) => authApi.requestMagicLink(email),
  });

  const magicLinkLoginMutation = useMutation({
    mutationFn: (email: string) => authApi.loginWithMagicLink(email),
    onSuccess: (response) => {
      setUser(response.user);
      queryClient.invalidateQueries();
    },
  });

  const logoutMutation = useMutation({
    mutationFn: () => authApi.logout(),
    onSuccess: () => {
      clearUser();
      queryClient.clear();
    },
  });

  const login = useCallback(
    async (data: DualPathLoginRequest) => {
      await loginMutation.mutateAsync(data);
    },
    [loginMutation]
  );

  const loginWithEntra = useCallback(
    async (token: string) => {
      await entraLoginMutation.mutateAsync(token);
    },
    [entraLoginMutation]
  );

  const requestMagicLink = useCallback(
    async (email: string) => {
      await magicLinkRequestMutation.mutateAsync(email);
    },
    [magicLinkRequestMutation]
  );

  const loginWithMagicLink = useCallback(
    async (email: string) => {
      await magicLinkLoginMutation.mutateAsync(email);
    },
    [magicLinkLoginMutation]
  );

  const logout = useCallback(async () => {
    await logoutMutation.mutateAsync();
  }, [logoutMutation]);

  return {
    user,
    isAuthenticated,
    isCheckingAuth,
    login,
    loginWithEntra,
    requestMagicLink,
    loginWithMagicLink,
    logout,
    isLoggingIn:
      loginMutation.isPending ||
      entraLoginMutation.isPending ||
      magicLinkLoginMutation.isPending,
    isLoggingOut: logoutMutation.isPending,
    loginError:
      loginMutation.error ||
      entraLoginMutation.error ||
      magicLinkLoginMutation.error,
    isRequestingMagicLink: magicLinkRequestMutation.isPending,
    magicLinkRequestError: magicLinkRequestMutation.error,
  };
}
