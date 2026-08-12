import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright E2E — frontend DSA Visual.
 *
 * - testDir: ./tests/e2e (auth / simulator / ladder / code-runner).
 * - Chỉ chromium desktop: mobile (<768px) ngoài phạm vi docs/TEST_PLAN.md §7.1.
 * - Backend KHÔNG bắt buộc: mọi spec dùng route-mock /api/v1 (tests/e2e/helpers/mockApi.ts)
 *   — xem README.md trong cùng thư mục.
 * - PORT RIÊNG 5174 (strictPort): port 5173 trong môi trường này đang bị chiếm bởi
 *   relay Docker/WSL phục vụ bản STALE của app (route mới không tồn tại) → không dùng.
 *   webServer tự chạy `npm run dev -- --port 5174` (reuseExistingServer nếu đã chạy).
 */
export default defineConfig({
  testDir: './tests/e2e',
  timeout: 30_000,
  expect: {
    timeout: 10_000,
  },
  // Dev: retries 0 — fail hiện ngay, không che giấu mock sai shape
  retries: 0,
  // 1 worker — tránh đụng port / dev server
  workers: 1,
  reporter: [['list']],
  use: {
    baseURL: 'http://localhost:5174',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: {
    command: 'npm run dev -- --port 5174 --strictPort',
    url: 'http://localhost:5174',
    reuseExistingServer: true,
    timeout: 60_000,
  },
});
