import { apiClient } from "./client";

export interface Badge {
  id: string;
  name: string;
  description?: string;
  imageId?: string;
  requirements?: Record<string, unknown>;
}

export interface BadgeQueryRequest {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

export interface BadgeQueryResponse {
  badges: Badge[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const badgesApi = {
  getBadges: async (request?: BadgeQueryRequest): Promise<BadgeQueryResponse> => {
    const response = await apiClient.get<BadgeQueryResponse>("/api/admin/badges", {
      params: request,
    });
    return response.data;
  },

  getBadge: async (id: string): Promise<Badge> => {
    const response = await apiClient.get<Badge>(`/api/admin/badges/${id}`);
    return response.data;
  },

  createBadge: async (badge: Partial<Badge>): Promise<Badge> => {
    const response = await apiClient.post<Badge>("/api/admin/badges", badge);
    return response.data;
  },

  updateBadge: async (id: string, badge: Partial<Badge>): Promise<Badge> => {
    const response = await apiClient.put<Badge>(`/api/admin/badges/${id}`, badge);
    return response.data;
  },

  deleteBadge: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/admin/badges/${id}`);
  },
};
