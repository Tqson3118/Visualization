import { defineConfig } from "vitest/config";
import vue from "@vitejs/plugin-vue";
import path from "path";

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src")
    },
  },
  server: {
    port: 5173,
    strictPort: true,
  },
  worker: {
    format: 'es',
  },
  optimizeDeps: {
    exclude: ['*.wasm'],
  },
  assetsInclude: ['**/*.wasm'],
  build: {
    chunkSizeWarningLimit: 800,
    cssCodeSplit: true,
    rollupOptions: {
      output: {
        manualChunks(id) {
          // Monaco Editor — isolate all monaco chunks together
          if (id.includes('monaco-editor') || id.includes('monaco-languageclient')) {
            return 'monaco-vendor';
          }
          // Monaco workers — separate chunk so they load on demand
          if (id.includes('monaco') && (id.includes('worker') || id.includes('Worker'))) {
            return 'monaco-workers';
          }
          // Vue core ecosystem — Vue, Pinia, Vue-Router
          if (
            id.includes('/node_modules/vue/') ||
            id.includes('/node_modules/@vue/') ||
            id.includes('/node_modules/pinia') ||
            id.includes('/node_modules/vue-router')
          ) {
            return 'vue-core';
          }
          // SignalR — real-time comms
          if (id.includes('@microsoft/signalr')) {
            return 'signalr-vendor';
          }
          // XLSX / ExcelJS — heavy spreadsheet libs
          if (id.includes('xlsx') || id.includes('exceljs')) {
            return 'xlsx-vendor';
          }
          // yjs / CRDT
          if (id.includes('yjs') || id.includes('lib0')) {
            return 'crdt-vendor';
          }
          // Mermaid — very large diagram library
          if (id.includes('mermaid')) {
            return 'mermaid-vendor';
          }
          // Chart.js + vue-chartjs
          if (id.includes('chart.js') || id.includes('vue-chartjs')) {
            return 'chart-vendor';
          }
          // GSAP — animation library
          if (id.includes('gsap')) {
            return 'gsap-vendor';
          }
          // Lottie-web
          if (id.includes('lottie-web')) {
            return 'lottie-vendor';
          }
          // Shiki — syntax highlighting
          if (id.includes('shiki')) {
            return 'shiki-vendor';
          }
          // DnD Kit
          if (id.includes('@dnd-kit')) {
            return 'dnd-vendor';
          }
          // VueUse
          if (id.includes('@vueuse')) {
            return 'vueuse-vendor';
          }
          // Marked + escodegen + acorn (markdown/code parsing)
          if (id.includes('marked') || id.includes('escodegen') || id.includes('acorn')) {
            return 'markdown-vendor';
          }
          // Splitpanes
          if (id.includes('splitpanes')) {
            return 'splitpanes-vendor';
          }
          // QRCode
          if (id.includes('qrcode')) {
            return 'qrcode-vendor';
          }
          // LZ-string
          if (id.includes('lz-string')) {
            return 'lzstring-vendor';
          }
          // canvas-confetti, animejs — small animation libs
          if (
            id.includes('canvas-confetti') ||
            id.includes('animejs')
          ) {
            return 'ui-vendor';
          }
          // Axios
          if (id.includes('axios')) {
            return 'axios-vendor';
          }
          // Everything else goes into vendor
          if (id.includes('node_modules')) {
            return 'vendor';
          }
        },
      },
    },
  },
  test: {
    globals: true,
    environment: "node",
    setupFiles: ["./vitest.setup.ts"],
  },
});
