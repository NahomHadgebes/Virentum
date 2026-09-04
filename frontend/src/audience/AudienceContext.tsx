import { createContext, useCallback, useMemo, useSyncExternalStore } from 'react';
import type { ReactNode } from 'react';
import type { Audience } from '../types/enums';
import * as audienceStorage from './audienceStorage';

export interface AudienceContextValue {
  /** null until the visitor has chosen. */
  audience: Audience | null;
  choose: (audience: Audience) => void;
  reset: () => void;
}

export const AudienceContext = createContext<AudienceContextValue | null>(null);

export function AudienceProvider({ children }: { children: ReactNode }) {
  const audience = useSyncExternalStore(
    audienceStorage.subscribe,
    audienceStorage.getSnapshot,
  );

  const choose = useCallback((next: Audience) => {
    audienceStorage.write(next);
  }, []);

  const reset = useCallback(() => {
    audienceStorage.clear();
  }, []);

  const value = useMemo<AudienceContextValue>(
    () => ({ audience, choose, reset }),
    [audience, choose, reset],
  );

  return <AudienceContext.Provider value={value}>{children}</AudienceContext.Provider>;
}
