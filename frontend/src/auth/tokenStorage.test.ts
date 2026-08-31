import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { Session } from './tokenStorage';

const SESSION: Session = {
  token: 'header.payload.signature',
  user: { storeId: 'demo-store', displayName: 'Store Associate', station: 'Station #4' },
};

/** A fresh module instance, as if the tab had just loaded. */
async function load() {
  vi.resetModules();
  return import('./tokenStorage');
}

beforeEach(() => {
  window.sessionStorage.clear();
});

describe('reading and writing', () => {
  it('reports no session when storage is empty', async () => {
    const storage = await load();

    expect(storage.getSnapshot()).toBeNull();
    expect(storage.readToken()).toBeNull();
  });

  it('returns what was written', async () => {
    const storage = await load();
    storage.write(SESSION);

    expect(storage.getSnapshot()).toEqual(SESSION);
    expect(storage.readToken()).toBe('header.payload.signature');
  });

  /**
   * useSyncExternalStore compares snapshots by identity. A fresh object per
   * call would re-render forever, so this is load-bearing, not a detail.
   */
  it('returns a stable reference until the session changes', async () => {
    const storage = await load();
    storage.write(SESSION);

    expect(storage.getSnapshot()).toBe(storage.getSnapshot());
  });

  it('survives a reload, which is why sessionStorage is used at all', async () => {
    const first = await load();
    first.write(SESSION);

    const second = await load();
    expect(second.getSnapshot()).toEqual(SESSION);
  });

  it('forgets the session after clear', async () => {
    const storage = await load();
    storage.write(SESSION);
    storage.clear();

    expect(storage.getSnapshot()).toBeNull();
    expect(window.sessionStorage.getItem('virentum.session')).toBeNull();
  });
});

describe('subscribers', () => {
  it('notifies on write and on clear', async () => {
    const storage = await load();
    const listener = vi.fn();
    storage.subscribe(listener);

    storage.write(SESSION);
    storage.clear();

    expect(listener).toHaveBeenCalledTimes(2);
  });

  it('stops notifying after unsubscribe', async () => {
    const storage = await load();
    const listener = vi.fn();
    const unsubscribe = storage.subscribe(listener);

    unsubscribe();
    storage.write(SESSION);

    expect(listener).not.toHaveBeenCalled();
  });
});

describe('untrusted stored data', () => {
  it('discards a corrupt entry instead of returning it', async () => {
    window.sessionStorage.setItem('virentum.session', '{ not json');
    const storage = await load();

    expect(storage.getSnapshot()).toBeNull();
    expect(window.sessionStorage.getItem('virentum.session')).toBeNull();
  });

  it('rejects a session whose user does not match UserDto', async () => {
    window.sessionStorage.setItem(
      'virentum.session',
      JSON.stringify({ token: 'abc', user: { storeId: 'demo-store' } }),
    );
    const storage = await load();

    expect(storage.getSnapshot()).toBeNull();
  });

  it('rejects an entry with no token', async () => {
    window.sessionStorage.setItem('virentum.session', JSON.stringify({ user: SESSION.user }));
    const storage = await load();

    expect(storage.getSnapshot()).toBeNull();
  });
});
