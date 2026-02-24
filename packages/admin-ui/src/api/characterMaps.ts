import { apiClient } from "./client";

export interface CharacterMap {
  id: string;
  name: string;
  description?: string;
  imageId?: string;
  metadata?: Record<string, unknown>;
}

export interface CharacterMapQueryRequest {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

export interface CharacterMapQueryResponse {
  characterMaps: CharacterMap[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const characterMapsApi = {
  getCharacterMaps: async (
    request?: CharacterMapQueryRequest
  ): Promise<CharacterMapQueryResponse> => {
    const response = await apiClient.get<CharacterMapQueryResponse>("/api/admin/charactermaps", {
      params: request,
    });
    return response.data;
  },

  getCharacterMap: async (id: string): Promise<CharacterMap> => {
    const response = await apiClient.get<CharacterMap>(`/api/admin/charactermaps/${id}`);
    return response.data;
  },

  createCharacterMap: async (characterMap: Partial<CharacterMap>): Promise<CharacterMap> => {
    const response = await apiClient.post<CharacterMap>("/api/admin/charactermaps", characterMap);
    return response.data;
  },

  updateCharacterMap: async (
    id: string,
    characterMap: Partial<CharacterMap>
  ): Promise<CharacterMap> => {
    const response = await apiClient.put<CharacterMap>(
      `/api/admin/charactermaps/${id}`,
      characterMap
    );
    return response.data;
  },

  deleteCharacterMap: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/admin/charactermaps/${id}`);
  },

  uploadCharacterMap: async (file: File): Promise<{ success: boolean; message?: string }> => {
    const formData = new FormData();
    formData.append("file", file);

    const response = await apiClient.post<{ success: boolean; message?: string }>(
      "/api/admin/charactermaps/upload",
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      }
    );
    return response.data;
  },
};
