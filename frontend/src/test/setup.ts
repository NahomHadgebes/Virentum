import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

/**
 * Testing Library only registers its own cleanup when Vitest runs with globals
 * enabled. This project imports its test functions explicitly, so unmounting
 * between tests has to be wired up here — without it, rendered trees accumulate
 * and a query that should match one element finds several.
 */
afterEach(cleanup);
