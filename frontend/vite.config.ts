import { fileURLToPath, URL } from 'node:url';

import tailwindcss from '@tailwindcss/vite';
import vue from '@vitejs/plugin-vue';
import { defineConfig, configDefaults } from 'vitest/config';

// Cấu hình theo SDD §3.9
export default defineConfig({
  plugins: [vue(), tailwindcss()],
  resolve: {
    // alias dạng array: hỗ trợ find/RegExp (B3 tree-shake pixi) + alias đơn giản (`@`)
    alias: [
      { find: '@', replacement: fileURLToPath(new URL('./src', import.meta.url)) },
      // B3 — Tree-shake PixiJS: alias root 'pixi.js' sang pixi-entry.mjs (deep-import chỉ
      // Application/Container/Graphics/Text/TextStyle + các init cần thiết). Regex để
      // KHÔNG khớp 'pixi.js/app' hay subpath khác.
      { find: /^pixi[.]js$/, replacement: fileURLToPath(new URL('./src/lib/pixi-entry.mjs', import.meta.url)) },
    ],
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
        // manualChunks theo SDD §3.9 — Vite 8/Rolldown chỉ hỗ trợ dạng hàm
        // engine tách 2: renderers (pixi+canvas+painter ~400KB) vs core (generators+catalog ~430KB)
        // để cả 2 chunk < 500KB (fix Vite warning)
        manualChunks(id: string) {
          // Normalize Windows path (\ -> /): trên Windows id là 'D:\FPT\...\src\engines\...'
          // nên includes('/src/engines/') không khớp -> mọi module rơi về chunk mặc định.
          const nid = id.replace(/\\/g, '/');
          if (nid.includes('/src/engines/renderers/')) return 'engine-renderers';
          if (nid.includes('/src/engines/')) return 'engine-core';
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
    // E2E Playwright (tests/e2e/*.spec.ts) KHÔNG phải vitest — tránh vitest chạy nhầm
    exclude: [...configDefaults.exclude, 'tests/e2e/**'],
  },
});
