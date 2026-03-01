import { env } from "@/config/env";
import { logger } from "@/utils/logger";
import { apiClient, request } from "./client";
import type {
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
  User,
} from "./types";

const AUTH_PATH = "/auth";

export interface EntraLoginRequest {
  provider: "entra";
  token: string; // JWT token from Entra
}

export interface MagicLinkRequest {
  provider: "magic";
  email: string;
}

export interface DualPathLoginRequest extends LoginRequest {
  provider?: "credentials" | "entra" | "magic";
  entraToken?: string;
}

export const authApi = {
  // Dual-path login - supports credentials, Entra, and Magic Link
  login: async (data: DualPathLoginRequest): Promise<LoginResponse> => {
    // Handle Entra login
    if (data.provider === "entra" && data.entraToken) {
      return await authApi.loginWithEntra(data.entraToken);
    }

    // Handle Magic Link login
    if (data.provider === "magic" && data.email) {
      return await authApi.loginWithMagicLink(data.email);
    }

    // Handle fake login for development
    const useFakeLogin = env.useFakeAuth;

    if (useFakeLogin) {
      // Create fake user from email
      const fakeUser: LoginResponse = {
        user: {
          id: "fake-user-" + Date.now(),
          name: data.email.split("@")[0] || "User",
          email: data.email,
          roles: ["author"] as const,
          createdAt: new Date().toISOString(),
        },
        accessToken: "fake-access-token-" + Date.now(),
        refreshToken: "fake-refresh-token-" + Date.now(),
        expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
      };

      // Store fake tokens
      localStorage.setItem("accessToken", fakeUser.accessToken);
      localStorage.setItem("refreshToken", fakeUser.refreshToken);

      // Simulate network delay
      await new Promise((resolve) => setTimeout(resolve, 500));

      return fakeUser;
    }

    const response = await request<LoginResponse>({
      method: "POST",
      url: `${AUTH_PATH}/login`,
      data,
    });
    // Store tokens
    localStorage.setItem("accessToken", response.accessToken);
    localStorage.setItem("refreshToken", response.refreshToken);
    return response;
  },

  // Login with Entra JWT token
  loginWithEntra: async (token: string): Promise<LoginResponse> => {
    const useFakeLogin = env.useFakeAuth;

    if (useFakeLogin) {
      // Parse JWT token (fake validation for development)
      let fakeUser: LoginResponse;

      try {
        const tokenParts = token.split(".");
        if (tokenParts.length !== 3 || !tokenParts[1]) {
          throw new Error("Invalid JWT token format");
        }

        const payload = JSON.parse(atob(tokenParts[1]));
        fakeUser = {
          user: {
            id: payload.sub || "entra-user-" + Date.now(),
            name: payload.name || payload.email?.split("@")[0] || "Entra User",
            email: payload.email || "user@entra.com",
            roles: ["author"] as const,
            createdAt: new Date().toISOString(),
          },
          accessToken: token, // Use the provided JWT token
          refreshToken: "fake-refresh-token-" + Date.now(),
          expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
        };
      } catch (parseError) {
        console.error("Error parsing JWT token in fake login:", parseError);
        // Fallback to safe defaults
        fakeUser = {
          user: {
            id: "entra-user-" + Date.now(),
            name: "Entra User",
            email: "user@entra.com",
            roles: ["author"] as const,
            createdAt: new Date().toISOString(),
          },
          accessToken: token,
          refreshToken: "fake-refresh-token-" + Date.now(),
          expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
        };
      }

      localStorage.setItem("accessToken", fakeUser.accessToken);
      localStorage.setItem("refreshToken", fakeUser.refreshToken);

      return fakeUser;
    }

    const response = await request<LoginResponse>({
      method: "POST",
      url: `${AUTH_PATH}/entra-login`,
      data: { token },
    });

    localStorage.setItem("accessToken", response.accessToken);
    localStorage.setItem("refreshToken", response.refreshToken);
    return response;
  },

  // Request Magic Link
  requestMagicLink: async (email: string): Promise<void> => {
    const useFakeLogin = env.useFakeAuth;

    if (useFakeLogin) {
      // Simulate sending magic link
      const maskedEmail =
        email.length > 0
          ? `${email[0]}***${email.substring(email.lastIndexOf("@"))}`
          : "[no-email]";
      logger.debug(`Fake magic link sent to ${maskedEmail}`);
      await new Promise((resolve) => setTimeout(resolve, 1000));
      return;
    }

    await request<void>({
      method: "POST",
      url: `${AUTH_PATH}/magic-link`,
      data: { email },
    });
  },

  // Login with Magic Link
  loginWithMagicLink: async (email: string): Promise<LoginResponse> => {
    const useFakeLogin = env.useFakeAuth;

    if (useFakeLogin) {
      const fakeUser: LoginResponse = {
        user: {
          id: "magic-user-" + Date.now(),
          name: email.split("@")[0] || "User",
          email: email,
          roles: ["author"] as const,
          createdAt: new Date().toISOString(),
        },
        accessToken: "fake-magic-token-" + Date.now(),
        refreshToken: "fake-refresh-token-" + Date.now(),
        expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
      };

      localStorage.setItem("accessToken", fakeUser.accessToken);
      localStorage.setItem("refreshToken", fakeUser.refreshToken);

      return fakeUser;
    }

    const response = await request<LoginResponse>({
      method: "POST",
      url: `${AUTH_PATH}/magic-login`,
      data: { email },
    });

    localStorage.setItem("accessToken", response.accessToken);
    localStorage.setItem("refreshToken", response.refreshToken);
    return response;
  },

  // Logout
  logout: async (): Promise<void> => {
    const useFakeLogin = env.useFakeAuth;

    if (useFakeLogin) {
      // Just clear local storage for fake login
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
      return;
    }

    try {
      await request<void>({
        method: "POST",
        url: `${AUTH_PATH}/logout`,
      });
    } finally {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
    }
  },

  // Refresh access token
  refreshToken: async (data: RefreshTokenRequest): Promise<LoginResponse> => {
    // Handle fake auth in development
    if (env.useFakeAuth) {
      const refreshToken = localStorage.getItem("refreshToken");
      if (refreshToken?.startsWith("fake-refresh-token")) {
        // Return new fake tokens
        const fakeResponse: LoginResponse = {
          user: {
            id: "fake-user-refreshed",
            name: "Demo User",
            email: "demo@example.com",
            roles: ["author"] as const,
            createdAt: new Date().toISOString(),
          },
          accessToken: "fake-access-token-" + Date.now(),
          refreshToken: "fake-refresh-token-" + Date.now(),
          expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
        };
        localStorage.setItem("accessToken", fakeResponse.accessToken);
        localStorage.setItem("refreshToken", fakeResponse.refreshToken);
        return fakeResponse;
      }
    }

    const response = await apiClient.post<{ data: LoginResponse }>(
      `${AUTH_PATH}/refresh`,
      data
    );
    localStorage.setItem("accessToken", response.data.data.accessToken);
    localStorage.setItem("refreshToken", response.data.data.refreshToken);
    logger.debug("Token refreshed successfully");
    return response.data.data;
  },

  // Get current user (with fake user fallback)
  getCurrentUser: async (): Promise<User> => {
    const useFakeLogin = env.useFakeAuth;
    const accessToken = localStorage.getItem("accessToken");

    if (useFakeLogin && accessToken?.startsWith("fake-access-token")) {
      // Return fake user from stored data
      const storedUser = localStorage.getItem("auth-storage");
      if (storedUser) {
        try {
          const parsed = JSON.parse(storedUser);
          if (parsed.state?.user) {
            return parsed.state.user;
          }
        } catch {
          // Fall through to create new fake user
        }
      }

      // Create default fake user
      return {
        id: "fake-user-default",
        name: "Demo User",
        email: "demo@example.com",
        roles: ["author"] as const,
        createdAt: new Date().toISOString(),
      };
    }

    return request<User>({
      method: "GET",
      url: `${AUTH_PATH}/me`,
    });
  },
};
