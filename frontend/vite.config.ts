import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    // Must stay in the backend's Cors:AllowedOrigins list.
    port: 5173,
    strictPort: true,
  },
});
