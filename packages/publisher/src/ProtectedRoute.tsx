import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '@/hooks';
import { Spinner } from '@/components';

export function ProtectedRoute() {
  const { isAuthenticated, isCheckingAuth } = useAuth();
  const location = useLocation();

  // Show loading while checking authentication
  if (isCheckingAuth) {
    return (
      <div className="page page--loading">
        <Spinner size="lg" />
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <Outlet />;
}
