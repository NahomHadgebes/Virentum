import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from './useAuth';

/**
 * Gate for authenticated routes. The attempted location travels along in the
 * navigation state so LoginPage can return the operator to where they were
 * heading — including after api/client.ts clears an expired session mid-scan.
 */
export function RequireAuth({ children }: { children: ReactNode }) {
  const { session } = useAuth();
  const location = useLocation();

  if (session === null) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  return children;
}
