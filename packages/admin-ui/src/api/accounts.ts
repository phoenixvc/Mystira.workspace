import { apiClient } from "./client";

export interface Account {
  id: string;
  email?: string;
  displayName?: string;
  createdAt?: string;
  lastLoginAt?: string;
  isActive?: boolean;
}

export interface UserProfile {
  id: string;
  accountId: string;
  displayName?: string;
  avatarId?: string;
  ageGroup?: string;
  preferences?: Record<string, unknown>;
  createdAt?: string;
  updatedAt?: string;
}

export interface AccountQueryRequest {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

export interface AccountQueryResponse {
  accounts: Account[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ProfileQueryRequest {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

export interface ProfileQueryResponse {
  profiles: UserProfile[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Read-only API for user administration
export const accountsApi = {
  getAccounts: async (request?: AccountQueryRequest): Promise<AccountQueryResponse> => {
    const response = await apiClient.get<AccountQueryResponse>("/api/admin/accounts", {
      params: request,
    });
    return response.data;
  },

  getAccount: async (id: string): Promise<Account> => {
    const response = await apiClient.get<Account>(`/api/admin/accounts/${id}`);
    return response.data;
  },
};

export const profilesApi = {
  getProfiles: async (request?: ProfileQueryRequest): Promise<ProfileQueryResponse> => {
    const response = await apiClient.get<ProfileQueryResponse>("/api/admin/profiles", {
      params: request,
    });
    return response.data;
  },

  getProfile: async (id: string): Promise<UserProfile> => {
    const response = await apiClient.get<UserProfile>(`/api/admin/profiles/${id}`);
    return response.data;
  },
};
