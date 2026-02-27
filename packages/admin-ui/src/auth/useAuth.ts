import { useCallback, useEffect, useMemo, useState } from "react";
import { authApi } from "../api/auth";
import { useAuthStore } from "../state/authStore";

interface AuthUser {
  username: string | null;
  roles: string[];
}

export function useAuth() {
  const token = useAuthStore((state) => state.token);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const setLoggedIn = useAuthStore((state) => state.login);
  const clearAuth = useAuthStore((state) => state.logout);

  const [isLoading, setIsLoading] = useState(true);
  const [user, setUser] = useState<AuthUser>({ username: null, roles: [] });

  useEffect(() => {
    let isMounted = true;

    const hydrateAuthState = async () => {
      if (!token) {
        if (isMounted) {
          setUser({ username: null, roles: [] });
          setIsLoading(false);
        }
        return;
      }

      try {
        const status = await authApi.status();
        if (status.isAuthenticated) {
          if (isMounted) {
            setUser({
              username: status.username ?? null,
              roles: status.roles ?? [],
            });
          }
        } else {
          clearAuth();
          if (isMounted) {
            setUser({ username: null, roles: [] });
          }
        }
      } catch {
        clearAuth();
        if (isMounted) {
          setUser({ username: null, roles: [] });
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    hydrateAuthState();

    return () => {
      isMounted = false;
    };
  }, [token, clearAuth]);

  const login = useCallback(
    async (username: string, password: string) => {
      const result = await authApi.login(username, password);
      if (!result.accessToken) {
        throw new Error(
          "Authentication succeeded but access token is missing."
        );
      }

      setLoggedIn(result.accessToken);
      setUser({ username, roles: result.roles ?? [] });
    },
    [setLoggedIn]
  );

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } finally {
      clearAuth();
      setUser({ username: null, roles: [] });
    }
  }, [clearAuth]);

  const getAccessToken = useCallback(async () => token, [token]);

  return useMemo(
    () => ({
      isAuthenticated,
      isLoading,
      user,
      login,
      logout,
      getAccessToken,
    }),
    [isAuthenticated, isLoading, user, login, logout, getAccessToken]
  );
}
