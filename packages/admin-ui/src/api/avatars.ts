import { apiClient } from "./client";

export interface AvatarConfiguration {
  ageGroup: string;
  avatarMediaIds: string[];
}

export interface AvatarConfigurationFile {
  avatars: AvatarConfiguration[];
}

export const avatarsApi = {
  getAllAvatars: async (): Promise<AvatarConfigurationFile> => {
    const response = await apiClient.get<AvatarConfigurationFile>("/api/admin/avatars");
    return response.data;
  },

  getAvatarsForAgeGroup: async (ageGroup: string): Promise<string[]> => {
    const response = await apiClient.get<string[]>(`/api/admin/avatars/${ageGroup}`);
    return response.data;
  },

  setAvatarsForAgeGroup: async (
    ageGroup: string,
    mediaIds: string[]
  ): Promise<AvatarConfigurationFile> => {
    const response = await apiClient.post<AvatarConfigurationFile>(
      `/api/admin/avatars/${ageGroup}`,
      mediaIds
    );
    return response.data;
  },

  addAvatarToAgeGroup: async (
    ageGroup: string,
    mediaId: string
  ): Promise<AvatarConfigurationFile> => {
    const response = await apiClient.post<AvatarConfigurationFile>(
      `/api/admin/avatars/${ageGroup}/add`,
      JSON.stringify(mediaId),
      {
        headers: {
          "Content-Type": "application/json",
        },
      }
    );
    return response.data;
  },

  removeAvatarFromAgeGroup: async (
    ageGroup: string,
    mediaId: string
  ): Promise<AvatarConfigurationFile> => {
    const response = await apiClient.delete<AvatarConfigurationFile>(
      `/api/admin/avatars/${ageGroup}/remove/${mediaId}`
    );
    return response.data;
  },
};
