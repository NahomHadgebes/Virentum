import { useContext } from 'react';
import { AudienceContext } from './AudienceContext';
import type { AudienceContextValue } from './AudienceContext';

export function useAudience(): AudienceContextValue {
  const value = useContext(AudienceContext);

  if (value === null) {
    throw new Error('useAudience must be used inside <AudienceProvider>.');
  }

  return value;
}
