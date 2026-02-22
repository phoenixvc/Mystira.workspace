import { apiClient } from "./client";

export interface LoginRequest {
  Username: string;
  Password: string;
}

export interface LoginResponse {
  Message: string;
}

export const authApi = {
  login: async (username: string, password: string): Promise<{ Message: string }> => {
    const response = await apiClient.post<{ Message: string }>("/api/auth/login", {
      Username: username,
      Password: password,
    });
    return response.data;
  },

  logout: async (): Promise<void> => {
    await apiClient.post("/api/auth/logout");
  },
};
