// Environment variable validation and configuration

interface EnvConfig {
  apiBaseUrl: string;
  adminApiBaseUrl: string;
  grpcEndpoint: string;
  enableMockApi: boolean;
  useFakeAuth: boolean;
}

// Determine environment prefix
const getEnvPrefix = (): string => {
  const env = import.meta.env.MODE;
  if (env === 'development') return 'dev.';
  if (env === 'staging') return 'staging.';
  return '';
};

// Validate and get environment variables
function getEnvVar(name: string, defaultValue?: string): string {
  const value = import.meta.env[name];
  if (!value && !defaultValue) {
    throw new Error(`Missing required environment variable: ${name}`);
  }
  return value || defaultValue || '';
}

// Environment configuration
export const env: EnvConfig = {
  apiBaseUrl:
    getEnvVar('VITE_API_BASE_URL') || `https://${getEnvPrefix()}api.mystira.app/api`,
  adminApiBaseUrl:
    getEnvVar('VITE_ADMIN_API_BASE_URL') || `https://${getEnvPrefix()}admin.mystira.app/api`,
  grpcEndpoint:
    getEnvVar('VITE_GRPC_ENDPOINT') || `https://${getEnvPrefix()}chain.mystira.app`,
  enableMockApi: import.meta.env.VITE_ENABLE_MOCK_API === 'true',
  useFakeAuth: import.meta.env.DEV || import.meta.env.VITE_USE_FAKE_AUTH === 'true',
};

// Validate critical configuration on import
if (!env.apiBaseUrl) {
  throw new Error('API base URL is required');
}

