import { apiClient } from "./client";

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  roles: string[];
}

export interface AuthStatusResponse {
  isAuthenticated: boolean;
  username?: string;
  roles: string[];
  expiresAt?: string;
}

export const authApi = {
  login: async (username: string, password: string): Promise<LoginResponse> => {
    const response = await apiClient.post("/api/auth/login", {
      username,
      password,
    });

    const data = response.data as Partial<LoginResponse> & {
      AccessToken?: string;
      ExpiresAtUtc?: string;
      Roles?: string[];
    };

    return {
      accessToken: data.accessToken ?? data.AccessToken ?? "",
      expiresAtUtc: data.expiresAtUtc ?? data.ExpiresAtUtc ?? "",
      roles: data.roles ?? data.Roles ?? [],
    };
  },

  logout: async (): Promise<void> => {
    await apiClient.post("/api/auth/logout");
  },

  status: async (): Promise<AuthStatusResponse> => {
    const response =
      await apiClient.get<AuthStatusResponse>("/api/auth/status");
    return response.data;
  },
};
