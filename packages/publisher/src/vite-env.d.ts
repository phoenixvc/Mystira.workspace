/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
  readonly VITE_PUBLIC_API_URL: string;
  readonly VITE_GRPC_ENDPOINT: string;
  readonly VITE_ENABLE_MOCK_API: string;
  readonly VITE_ANALYTICS_ID?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
