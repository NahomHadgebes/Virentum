import { createContext, useCallback, useMemo, useSyncExternalStore } from 'react';
import type { ReactNode } from 'react';
import { login } from '../api/auth';
import type { LoginRequest } from '../types/contracts';
import * as tokenStorage from './tokenStorage';
import type { Session } from './tokenStorage';

export interface AuthContextValue {
  /** null when signed out. */
  session: Session | null;
  /** Throws ApiError when the API rejects the credentials. */
  signIn: (credentials: LoginRequest) => Promise<void>;
  signOut: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

/**
 * Reads the session straight from tokenStorage rather than keeping a second
 * copy in React state. That matters because api/client.ts clears the session
 * on a 401 from outside the component tree — useSyncExternalStore is what makes
 * the UI notice.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const session = useSyncExternalStore(tokenStorage.subscribe, tokenStorage.getSnapshot);

  const signIn = useCallback(async (credentials: LoginRequest) => {
    const response = await login(credentials);
    tokenStorage.write({ token: response.token, user: response.user });
  }, []);

  const signOut = useCallback(() => {
    tokenStorage.clear();
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ session, signIn, signOut }),
    [session, signIn, signOut],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
