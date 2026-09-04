/// <reference types="vitest/config" />
import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ command, mode }) => {
  // Vite inlines this at build time, so a missing value does not fail the
  // build — it ships a bundle that throws in the visitor's browser instead. A
  // hosted deploy is exactly where nobody would notice until it is live, so the
  // build refuses to produce one.
  if (command === 'build' && loadEnv(mode, process.cwd(), 'VITE_').VITE_API_BASE_URL === undefined) {
    throw new Error(
      'VITE_API_BASE_URL is not set, so this build would produce a bundle with no API to ' +
        "call. Set it in the host's build environment (on Netlify: Site configuration → " +
        'Environment variables).',
    );
  }

  return {
    plugins: [react()],
    server: {
      // Must stay in the backend's Cors:AllowedOrigins list.
      port: 5173,
      strictPort: true,
    },
    test: {
      environment: 'happy-dom',
      setupFiles: ['./src/test/setup.ts'],
      include: ['src/**/*.test.{ts,tsx}'],
      env: {
        // api/client.ts resolves this once at module load. The client test serves
        // a real HTTP server on this port rather than replacing fetch.
        VITE_API_BASE_URL: 'http://127.0.0.1:5099',
      },
    },
  };
});
