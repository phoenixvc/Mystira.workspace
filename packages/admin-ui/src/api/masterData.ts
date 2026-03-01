import { apiClient } from "./client";

export interface AgeGroup {
  id: string;
  name: string;
  description?: string;
  minAge?: number;
  maxAge?: number;
}

export interface Archetype {
  id: string;
  name: string;
  description?: string;
}

export interface CompassAxis {
  id: string;
  name: string;
  description?: string;
  positiveLabel?: string;
  negativeLabel?: string;
}

export interface EchoType {
  id: string;
  name: string;
  description?: string;
}

export interface FantasyTheme {
  id: string;
  name: string;
  description?: string;
}

export interface MasterDataQueryRequest {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

export interface MasterDataQueryResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const ageGroupsApi = {
  getAgeGroups: async (
    request?: MasterDataQueryRequest
  ): Promise<MasterDataQueryResponse<AgeGroup>> => {
    const response = await apiClient.get<MasterDataQueryResponse<AgeGroup>>(
      "/api/admin/agegroups",
      { params: request }
    );
    return response.data;
  },

  getAgeGroup: async (id: string): Promise<AgeGroup> => {
    const response = await apiClient.get<AgeGroup>(
      `/api/admin/agegroups/${id}`
    );
    return response.data;
  },

  createAgeGroup: async (ageGroup: Partial<AgeGroup>): Promise<AgeGroup> => {
    const response = await apiClient.post<AgeGroup>(
      "/api/admin/agegroups",
      ageGroup
    );
    return response.data;
  },

  updateAgeGroup: async (
    id: string,
    ageGroup: Partial<AgeGroup>
  ): Promise<AgeGroup> => {
    const response = await apiClient.put<AgeGroup>(
      `/api/admin/agegroups/${id}`,
      ageGroup
    );
    return response.data;
  },

  deleteAgeGroup: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/admin/agegroups/${id}`);
  },
};

export const archetypesApi = {
  getArchetypes: async (
    request?: MasterDataQueryRequest
  ): Promise<MasterDataQueryResponse<Archetype>> => {
    const response = await apiClient.get<MasterDataQueryResponse<Archetype>>(
      "/api/admin/archetypes",
      { params: request }
    );
    return response.data;
  },

  getArchetype: async (id: string): Promise<Archetype> => {
    const response = await apiClient.get<Archetype>(
      `/api/admin/archetypes/${id}`
    );
    return response.data;
  },

  createArchetype: async (
    archetype: Partial<Archetype>
  ): Promise<Archetype> => {
    const response = await apiClient.post<Archetype>(
      "/api/admin/archetypes",
      archetype
    );
    return response.data;
  },

  updateArchetype: async (
    id: string,
    archetype: Partial<Archetype>
  ): Promise<Archetype> => {
    const response = await apiClient.put<Archetype>(
      `/api/admin/archetypes/${id}`,
      archetype
    );
    return response.data;
  },

  deleteArchetype: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/admin/archetypes/${id}`);
  },
};

export const compassAxesApi = {
  getCompassAxes: async (
    request?: MasterDataQueryRequest
  ): Promise<MasterDataQueryResponse<CompassAxis>> => {
    const response = await apiClient.get<MasterDataQueryResponse<CompassAxis>>(
      "/api/admin/compassaxes",
      { params: request }
    );
    return response.data;
  },

  getCompassAxis: async (id: string): Promise<CompassAxis> => {
    const response = await apiClient.get<CompassAxis>(
      `/api/admin/compassaxes/${id}`
    );
    return response.data;
  },

  createCompassAxis: async (
    compassAxis: Partial<CompassAxis>
  ): Promise<CompassAxis> => {
    const response = await apiClient.post<CompassAxis>(
      "/api/admin/compassaxes",
      compassAxis
    );
    return response.data;
  },

  updateCompassAxis: async (
    id: string,
    compassAxis: Partial<CompassAxis>
  ): Promise<CompassAxis> => {
    const response = await apiClient.put<CompassAxis>(
      `/api/admin/compassaxes/${id}`,
      compassAxis
    );
    return response.data;
  },

  deleteCompassAxis: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/admin/compassaxes/${id}`);
  },
};

export const echoTypesApi = {
  getEchoTypes: async (
    request?: MasterDataQueryRequest
  ): Promise<MasterDataQueryResponse<EchoType>> => {
    const response = await apiClient.get<MasterDataQueryResponse<EchoType>>(
      "/api/admin/echotypes",
      { params: request }
    );
    return response.data;
  },

  getEchoType: async (id: string): Promise<EchoType> => {
    const response = await apiClient.get<EchoType>(
      `/api/admin/echotypes/${id}`
    );
    return response.data;
  },

  createEchoType: async (echoType: Partial<EchoType>): Promise<EchoType> => {
    const response = await apiClient.post<EchoType>(
      "/api/admin/echotypes",
      echoType
    );
    return response.data;
  },

  updateEchoType: async (
    id: string,
    echoType: Partial<EchoType>
  ): Promise<EchoType> => {
    const response = await apiClient.put<EchoType>(
      `/api/admin/echotypes/${id}`,
      echoType
    );
    return response.data;
  },

  deleteEchoType: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/admin/echotypes/${id}`);
  },
};

export const fantasyThemesApi = {
  getFantasyThemes: async (
    request?: MasterDataQueryRequest
  ): Promise<MasterDataQueryResponse<FantasyTheme>> => {
    const response = await apiClient.get<MasterDataQueryResponse<FantasyTheme>>(
      "/api/admin/fantasythemes",
      { params: request }
    );
    return response.data;
  },

  getFantasyTheme: async (id: string): Promise<FantasyTheme> => {
    const response = await apiClient.get<FantasyTheme>(
      `/api/admin/fantasythemes/${id}`
    );
    return response.data;
  },

  createFantasyTheme: async (
    fantasyTheme: Partial<FantasyTheme>
  ): Promise<FantasyTheme> => {
    const response = await apiClient.post<FantasyTheme>(
      "/api/admin/fantasythemes",
      fantasyTheme
    );
    return response.data;
  },

  updateFantasyTheme: async (
    id: string,
    fantasyTheme: Partial<FantasyTheme>
  ): Promise<FantasyTheme> => {
    const response = await apiClient.put<FantasyTheme>(
      `/api/admin/fantasythemes/${id}`,
      fantasyTheme
    );
    return response.data;
  },

  deleteFantasyTheme: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/admin/fantasythemes/${id}`);
  },
};
