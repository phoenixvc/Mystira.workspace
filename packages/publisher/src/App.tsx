import { lazy, Suspense } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { ErrorBoundary, Spinner, ToastContainer } from '@/components';
import { useUIStore } from '@/state/uiStore';
import { Layout } from './Layout';
import { ProtectedRoute } from './ProtectedRoute';

// Lazy load pages for code splitting
const HomePage = lazy(() => import('@/pages/HomePage').then(m => ({ default: m.HomePage })));
const LoginPage = lazy(() => import('@/pages/LoginPage').then(m => ({ default: m.LoginPage })));
const DashboardPage = lazy(() => import('@/pages/DashboardPage').then(m => ({ default: m.DashboardPage })));
const StoriesPage = lazy(() => import('@/pages/StoriesPage').then(m => ({ default: m.StoriesPage })));
const StoryDetailPage = lazy(() => import('@/pages/StoryDetailPage').then(m => ({ default: m.StoryDetailPage })));
const RegisterPage = lazy(() => import('@/pages/RegisterPage').then(m => ({ default: m.RegisterPage })));
const AuditPage = lazy(() => import('@/pages/AuditPage').then(m => ({ default: m.AuditPage })));
const OpenRolesPage = lazy(() => import('@/pages/OpenRolesPage').then(m => ({ default: m.OpenRolesPage })));
const RoleRequestsPage = lazy(() => import('@/pages/RoleRequestsPage').then(m => ({ default: m.RoleRequestsPage })));
const NotFoundPage = lazy(() => import('@/pages/NotFoundPage').then(m => ({ default: m.NotFoundPage })));

function AppContent() {
  const notifications = useUIStore(state => state.notifications);
  const removeNotification = useUIStore(state => state.removeNotification);

  // Convert UI notifications to toast format
  const toasts = notifications.map(n => ({
    id: n.id,
    variant: n.type as 'success' | 'error' | 'warning' | 'info',
    title: n.title,
    message: n.message,
    duration: n.duration,
  }));

  return (
    <>
      <Suspense fallback={
        <div className="page page--loading">
          <Spinner size="lg" />
        </div>
      }>
        <Routes>
          {/* Public routes */}
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />

          {/* Protected routes with layout */}
          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/stories" element={<StoriesPage />} />
              <Route path="/stories/:id" element={<StoryDetailPage />} />
              <Route path="/open-roles" element={<OpenRolesPage />} />
              <Route path="/role-requests" element={<RoleRequestsPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/audit" element={<AuditPage />} />
            </Route>
          </Route>

          {/* Catch-all */}
          <Route path="/404" element={<NotFoundPage />} />
          <Route path="*" element={<Navigate to="/404" replace />} />
        </Routes>
      </Suspense>
      <ToastContainer toasts={toasts} onRemove={removeNotification} />
    </>
  );
}

export function App() {
  return (
    <ErrorBoundary>
      <AppContent />
    </ErrorBoundary>
  );
}
