import axios, { AxiosError, AxiosInstance, AxiosRequestConfig, InternalAxiosRequestConfig } from 'axios';
import type { ApiError, ApiResponse } from './types';
import { env } from '@/config/env';
import { authApi } from './auth';
import { logger } from '@/utils/logger';
import { API_TIMEOUT } from '@/constants';

// API base URL from environment config
const API_BASE_URL = env.apiBaseUrl;

// Admin API URL - for admin-specific endpoints
export const ADMIN_API_BASE_URL = env.adminApiBaseUrl;

// Create axios instance with default config
export const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for adding auth token
apiClient.interceptors.request.use(
  config => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => Promise.reject(error)
);

// Response interceptor for handling errors and token refresh
apiClient.interceptors.response.use(
  response => response,
  async (error: AxiosError<ApiResponse<unknown>>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // Prevent infinite loop - don't refresh if already on refresh endpoint
    const isRefreshEndpoint = originalRequest?.url?.includes('/auth/refresh');
    
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry && !isRefreshEndpoint) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) {
          // No refresh token, logout user
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          window.location.href = '/login';
          return Promise.reject(error);
        }

        logger.debug('Attempting to refresh access token');
        const response = await authApi.refreshToken({ refreshToken });
        
        // Update stored tokens
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
        
        // Retry original request with new token
        originalRequest.headers.Authorization = `Bearer ${response.accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        logger.error('Token refresh failed:', refreshError);
        // Refresh failed, logout user
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    // Log other errors in development
    if (import.meta.env.DEV && error.response) {
      logger.error('API Error:', {
        status: error.response.status,
        data: error.response.data,
        url: originalRequest?.url,
      });
    }

    return Promise.reject(error);
  }
);

// Generic request wrapper with type safety
export async function request<T>(config: AxiosRequestConfig): Promise<T> {
  try {
    const response = await apiClient.request<ApiResponse<T>>(config);
    return response.data.data;
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.data) {
      const apiError = error.response.data as ApiResponse<unknown>;
      throw new ApiRequestError(
        apiError.message || 'An error occurred',
        apiError.errors || []
      );
    }
    throw error;
  }
}

export class ApiRequestError extends Error {
  constructor(
    message: string,
    public errors: ApiError[]
  ) {
    super(message);
    this.name = 'ApiRequestError';
  }
}
