import { request, apiClient } from './client';
import type {
  User,
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
} from './types';
import { env } from '@/config/env';
import { logger } from '@/utils/logger';

const AUTH_PATH = '/auth';

export const authApi = {
  // Login with credentials (with fake login fallback for development)
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    // Fake login for development - accept any credentials
    const useFakeLogin = env.useFakeAuth;
    
    if (useFakeLogin) {
      // Create fake user from email
      const fakeUser: LoginResponse = {
        user: {
          id: 'fake-user-' + Date.now(),
          name: data.email.split('@')[0] || 'User',
          email: data.email,
          roles: ['author'] as const,
          createdAt: new Date().toISOString(),
        },
        accessToken: 'fake-access-token-' + Date.now(),
        refreshToken: 'fake-refresh-token-' + Date.now(),
        expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
      };
      
      // Store fake tokens
      localStorage.setItem('accessToken', fakeUser.accessToken);
      localStorage.setItem('refreshToken', fakeUser.refreshToken);
      
      // Simulate network delay
      await new Promise(resolve => setTimeout(resolve, 500));
      
      return fakeUser;
    }
    
    const response = await request<LoginResponse>({
      method: 'POST',
      url: `${AUTH_PATH}/login`,
      data,
    });
    // Store tokens
    localStorage.setItem('accessToken', response.accessToken);
    localStorage.setItem('refreshToken', response.refreshToken);
    return response;
  },

  // Logout
  logout: async (): Promise<void> => {
    const useFakeLogin = env.useFakeAuth;
    
    if (useFakeLogin) {
      // Just clear local storage for fake login
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      return;
    }
    
    try {
      await request<void>({
        method: 'POST',
        url: `${AUTH_PATH}/logout`,
      });
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
    }
  },

  // Refresh access token
  refreshToken: async (data: RefreshTokenRequest): Promise<LoginResponse> => {
    // Handle fake auth in development
    if (env.useFakeAuth) {
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken?.startsWith('fake-refresh-token')) {
        // Return new fake tokens
        const fakeResponse: LoginResponse = {
          user: {
            id: 'fake-user-refreshed',
            name: 'Demo User',
            email: 'demo@example.com',
            roles: ['author'] as const,
            createdAt: new Date().toISOString(),
          },
          accessToken: 'fake-access-token-' + Date.now(),
          refreshToken: 'fake-refresh-token-' + Date.now(),
          expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
        };
        localStorage.setItem('accessToken', fakeResponse.accessToken);
        localStorage.setItem('refreshToken', fakeResponse.refreshToken);
        return fakeResponse;
      }
    }

    const response = await apiClient.post<{ data: LoginResponse }>(
      `${AUTH_PATH}/refresh`,
      data
    );
    localStorage.setItem('accessToken', response.data.data.accessToken);
    localStorage.setItem('refreshToken', response.data.data.refreshToken);
    logger.debug('Token refreshed successfully');
    return response.data.data;
  },

  // Get current user (with fake user fallback)
  getCurrentUser: async (): Promise<User> => {
    const useFakeLogin = env.useFakeAuth;
    const accessToken = localStorage.getItem('accessToken');
    
    if (useFakeLogin && accessToken?.startsWith('fake-access-token')) {
      // Return fake user from stored data
      const storedUser = localStorage.getItem('auth-storage');
      if (storedUser) {
        try {
          const parsed = JSON.parse(storedUser);
          if (parsed.state?.user) {
            return parsed.state.user;
          }
        } catch {
          // Fall through to create new fake user
        }
      }
      
      // Create default fake user
      return {
        id: 'fake-user-default',
        name: 'Demo User',
        email: 'demo@example.com',
        roles: ['author'] as const,
        createdAt: new Date().toISOString(),
      };
    }
    
    return request<User>({
      method: 'GET',
      url: `${AUTH_PATH}/me`,
    });
  },
};
