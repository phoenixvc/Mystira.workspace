import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { App } from './App';
import '@/styles/index.css';

// Initialize React Query client
import { QUERY_STALE_TIME, QUERY_GC_TIME, QUERY_RETRY } from '@/constants';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: QUERY_STALE_TIME,
      gcTime: QUERY_GC_TIME,
      retry: QUERY_RETRY,
      refetchOnWindowFocus: false,
    },
  },
});

// Enable MSW in development when mock API is enabled
async function enableMocking() {
  if (import.meta.env.VITE_ENABLE_MOCK_API !== 'true') {
    return;
  }

  const { worker } = await import('./tests/mocks/browser');
  return worker.start({
    onUnhandledRequest: 'bypass',
  });
}

enableMocking().then(() => {
  ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <App />
        </BrowserRouter>
      </QueryClientProvider>
    </React.StrictMode>
  );
});
