/**
 * Where the session lives between renders and reloads.
 *
 * sessionStorage rather than localStorage: both are readable by injected
 * script, but a scanning station is a shared device and the session should not
 * outlive the tab. The dev token is valid for 120 minutes either way.
 *
 * This module is deliberately React-free so that api/client.ts can clear the
 * session on a 401 without importing anything from the component tree. The
 * subscribe/getSnapshot pair is what lets AuthContext observe that through
 * useSyncExternalStore.
 */
import type { UserDto } from '../types/contracts';

const STORAGE_KEY = 'virentum.session';

export interface Session {
  token: string;
  user: UserDto;
}

type Listener = () => void;

const listeners = new Set<Listener>();

/**
 * useSyncExternalStore compares snapshots by identity, so parsing storage on
 * every call would loop forever. The parsed session is cached and replaced only
 * when it actually changes.
 */
let cached: Session | null = null;
let cacheLoaded = false;

export function subscribe(listener: Listener): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

/** The current session — a stable reference until write() or clear() runs. */
export function getSnapshot(): Session | null {
  if (!cacheLoaded) {
    cached = readFromStorage();
    cacheLoaded = true;
  }

  return cached;
}

export function write(session: Session): void {
  window.sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  cached = session;
  cacheLoaded = true;
  notify();
}

export function clear(): void {
  try {
    window.sessionStorage.removeItem(STORAGE_KEY);
  } finally {
    cached = null;
    cacheLoaded = true;
    notify();
  }
}

/** The token for the Authorization header, or null when signed out. */
export function readToken(): string | null {
  return getSnapshot()?.token ?? null;
}

function notify(): void {
  for (const listener of listeners) {
    listener();
  }
}

function readFromStorage(): Session | null {
  let raw: string | null;
  try {
    raw = window.sessionStorage.getItem(STORAGE_KEY);
  } catch {
    // Storage disabled by the browser. There is no session to be had; that is
    // a true answer, not a swallowed failure.
    return null;
  }

  if (raw === null) {
    return null;
  }

  try {
    return parseSession(JSON.parse(raw));
  } catch {
    // Corrupt or stale entry from an older shape. Drop it directly rather than
    // through clear(), which would notify subscribers mid-render.
    discardSilently();
    return null;
  }
}

function discardSilently(): void {
  try {
    window.sessionStorage.removeItem(STORAGE_KEY);
  } catch {
    // Nothing further to do; the parse already failed and we return null.
  }
}

/** Validates the stored shape instead of casting; throws on anything else. */
function parseSession(value: unknown): Session {
  if (typeof value !== 'object' || value === null) {
    throw new Error('Stored session is not an object.');
  }

  const candidate = value as Record<string, unknown>;
  const user = candidate['user'];

  if (typeof candidate['token'] !== 'string' || typeof user !== 'object' || user === null) {
    throw new Error('Stored session is missing token or user.');
  }

  const userFields = user as Record<string, unknown>;
  if (
    typeof userFields['storeId'] !== 'string' ||
    typeof userFields['displayName'] !== 'string' ||
    typeof userFields['station'] !== 'string'
  ) {
    throw new Error('Stored user does not match UserDto.');
  }

  return {
    token: candidate['token'],
    user: {
      storeId: userFields['storeId'],
      displayName: userFields['displayName'],
      station: userFields['station'],
    },
  };
}
