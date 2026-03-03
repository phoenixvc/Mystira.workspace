import { apiClient } from "./client";

export interface MediaAsset {
  id: string;
  fileName: string;
  contentType: string;
  size: number;
  uploadedAt?: string;
  metadata?: Record<string, unknown>;
}

export interface MediaQueryRequest {
  page?: number;
  pageSize?: number;
  type?: string;
  searchTerm?: string;
}

export interface MediaQueryResponse {
  media: MediaAsset[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const mediaApi = {
  getMedia: async (request?: MediaQueryRequest): Promise<MediaQueryResponse> => {
    const response = await apiClient.get<MediaQueryResponse>("/api/admin/media", {
      params: request,
    });
    return response.data;
  },

  getMediaFile: async (mediaId: string): Promise<Blob> => {
    const response = await apiClient.get(`/api/admin/media/${mediaId}`, {
      responseType: "blob",
    });
    return response.data;
  },

  uploadMedia: async (file: File, metadata?: Record<string, unknown>): Promise<MediaAsset> => {
    const formData = new FormData();
    formData.append("file", file);
    if (metadata) {
      formData.append("metadata", JSON.stringify(metadata));
    }

    const response = await apiClient.post<MediaAsset>("/api/admin/media/upload", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return response.data;
  },

  uploadMediaZip: async (
    file: File,
    overwriteMetadata = false,
    overwriteMedia = false
  ): Promise<{
    success: boolean;
    message: string;
    processedFiles: number;
    errors: string[];
  }> => {
    const formData = new FormData();
    formData.append("zipFile", file);
    formData.append("overwriteMetadata", overwriteMetadata.toString());
    formData.append("overwriteMedia", overwriteMedia.toString());

    const response = await apiClient.post<{
      success: boolean;
      message: string;
      processedFiles: number;
      errors: string[];
    }>("/api/admin/media/upload-zip", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return response.data;
  },

  deleteMedia: async (mediaId: string): Promise<void> => {
    await apiClient.delete(`/api/admin/media/${mediaId}`);
  },
};
