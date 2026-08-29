import { fileURLToPath, URL } from 'node:url';

import tailwindcss from '@tailwindcss/vite';
import vue from '@vitejs/plugin-vue';
import { defineConfig, configDefaults } from 'vitest/config';

// Cấu hình theo SDD §3.9
export default defineConfig({
  plugins: [vue(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  // Web Worker engine (engines/worker/compiler.worker.ts) — bê từ VisualizationDSA3 (V3):
  // worker chạy dạng ES module (compileWorker tạo Worker { type: 'module' })
  worker: {
    format: 'es',
  },
  build: {
    target: 'es2020',
    rollupOptions: {
      output: {
        // manualChunks theo SDD §3.9 (engine + vendor) — Vite 8/Rolldown chỉ hỗ trợ dạng hàm
        manualChunks(id: string) {
          if (id.includes('/src/engines/')) return 'engine';
          if (
            id.includes('/node_modules/vue/') ||
            id.includes('/node_modules/@vue/') ||
            id.includes('/node_modules/pinia') ||
            id.includes('/node_modules/vue-router')
          ) {
            return 'vendor';
          }
          return undefined;
        },
      },
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./vitest.setup.ts'],
    testTimeout: 20000,
    // E2E Playwright (tests/e2e/*.spec.ts) KHÔNG phải vitest — tránh vitest chạy nhầm
    exclude: [...configDefaults.exclude, 'tests/e2e/**'],
  },
});
