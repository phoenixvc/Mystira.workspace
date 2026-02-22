import { apiClient } from "./client";

export interface Bundle {
  id: string;
  name: string;
  description?: string;
  version?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface BundleQueryRequest {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

export interface BundleQueryResponse {
  bundles: Bundle[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const bundlesApi = {
  getBundles: async (request?: BundleQueryRequest): Promise<BundleQueryResponse> => {
    const response = await apiClient.get<BundleQueryResponse>("/api/admin/bundles", {
      params: request,
    });
    return response.data;
  },

  getBundle: async (id: string): Promise<Bundle> => {
    const response = await apiClient.get<Bundle>(`/api/admin/bundles/${id}`);
    return response.data;
  },

  validateBundle: async (file: File): Promise<{ success: boolean; result: unknown }> => {
    const formData = new FormData();
    formData.append("bundleFile", file);

    const response = await apiClient.post<{
      success: boolean;
      result: unknown;
    }>("/api/admin/bundles/validate", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return response.data;
  },

  uploadBundle: async (
    file: File,
    validateReferences = true,
    overwriteExisting = false
  ): Promise<{ success: boolean; result: unknown }> => {
    const formData = new FormData();
    formData.append("bundleFile", file);
    formData.append("validateReferences", validateReferences.toString());
    formData.append("overwriteExisting", overwriteExisting.toString());

    const response = await apiClient.post<{
      success: boolean;
      result: unknown;
    }>("/api/admin/bundles/upload", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return response.data;
  },

  createBundle: async (bundle: Omit<Bundle, "id" | "createdAt" | "updatedAt">): Promise<Bundle> => {
    const response = await apiClient.post<Bundle>("/api/admin/bundles", bundle);
    return response.data;
  },

  updateBundle: async (
    id: string,
    bundle: Omit<Bundle, "id" | "createdAt" | "updatedAt">
  ): Promise<Bundle> => {
    const response = await apiClient.put<Bundle>(`/api/admin/bundles/${id}`, bundle);
    return response.data;
  },

  deleteBundle: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/admin/bundles/${id}`);
  },
};
