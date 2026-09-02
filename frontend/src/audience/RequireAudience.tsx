import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAudience } from './useAudience';

/**
 * Every answer in the app is worded for one reader or the other, so there is no
 * sensible default to fall back on — a visitor who has not chosen is sent to the
 * choice rather than quietly treated as a shopper.
 */
export function RequireAudience({ children }: { children: ReactNode }) {
  const { audience } = useAudience();

  if (audience === null) {
    return <Navigate to="/" replace />;
  }

  return children;
}
