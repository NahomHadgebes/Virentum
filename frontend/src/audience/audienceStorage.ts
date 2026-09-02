import type { Audience } from '../types/enums';
import { AUDIENCES } from '../types/enums';

/**
 * The chosen audience, remembered between visits.
 *
 * localStorage rather than sessionStorage, unlike the access token: this is a
 * preference, not a credential, and a shopper who picked "Consumer" last week
 * should not be asked again. React-free for the same reason tokenStorage is —
 * so it can be read outside the component tree.
 */
const STORAGE_KEY = 'virentum.audience';

type Listener = () => void;

const listeners = new Set<Listener>();

let cached: Audience | null = null;
let cacheLoaded = false;

export function subscribe(listener: Listener): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

/** Stable reference until write() or clear() runs, for useSyncExternalStore. */
export function getSnapshot(): Audience | null {
  if (!cacheLoaded) {
    cached = read();
    cacheLoaded = true;
  }

  return cached;
}

export function write(audience: Audience): void {
  try {
    window.localStorage.setItem(STORAGE_KEY, audience);
  } catch {
    // A browser refusing storage should not stop the app; the choice simply
    // lasts for this visit instead of beyond it.
  }

  cached = audience;
  cacheLoaded = true;
  notify();
}

export function clear(): void {
  try {
    window.localStorage.removeItem(STORAGE_KEY);
  } finally {
    cached = null;
    cacheLoaded = true;
    notify();
  }
}

function notify(): void {
  for (const listener of listeners) {
    listener();
  }
}

function read(): Audience | null {
  let raw: string | null;
  try {
    raw = window.localStorage.getItem(STORAGE_KEY);
  } catch {
    return null;
  }

  // Validate rather than cast: the value is user-writable storage, and an
  // unrecognised one should send the visitor back to the choice.
  return AUDIENCES.find((candidate) => candidate === raw) ?? null;
}
