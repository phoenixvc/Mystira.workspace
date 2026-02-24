import axios, { InternalAxiosRequestConfig } from "axios";
import { tokenRequest } from "../auth";
import { BYPASS_AUTH } from "../auth/msalConfig";
import { msalInstance } from "../auth/msalInstance";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Request interceptor to add MSAL access token
apiClient.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    // If bypassing auth, add a dev token header or skip authentication
    if (BYPASS_AUTH) {
      if (config.headers) {
        config.headers.Authorization = "Bearer dev-token";
      }
      return config;
    }

    const account = msalInstance.getActiveAccount();

    if (account) {
      try {
        const response = await msalInstance.acquireTokenSilent({
          ...tokenRequest,
          account,
        });

        if (config.headers) {
          config.headers.Authorization = `Bearer ${response.accessToken}`;
        }
      } catch (error) {
        console.error("Token acquisition failed:", error);
        // Let the request proceed without token - backend will return 401 if needed
      }
    }

    return config;
  },
  error => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      // Unauthorized - redirect to login
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);
