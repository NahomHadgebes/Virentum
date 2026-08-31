/// <reference types="vitest/config" />
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    // Must stay in the backend's Cors:AllowedOrigins list.
    port: 5173,
    strictPort: true,
  },
  test: {
    environment: 'happy-dom',
    include: ['src/**/*.test.{ts,tsx}'],
    env: {
      // api/client.ts resolves this once at module load. The client test serves
      // a real HTTP server on this port rather than replacing fetch.
      VITE_API_BASE_URL: 'http://127.0.0.1:5099',
    },
  },
});
