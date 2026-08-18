/**
 * playwright.demo.config.ts — DEMO CHAIN (Trục 14) chạy với BACKEND THẬT :5000 + FE :8081.
 * KHÔNG mock — yêu cầu BE/FE đang chạy và DB đã seed. Không đụng config mặc định
 * (playwright.config.ts — suite route-mock giữ nguyên).
 */
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  testMatch: /(demo-chain|live-smoke-5-journeys)\.spec\.ts/,
  timeout: 240_000,
  expect: { timeout: 15_000 },
  retries: 0,
  workers: 1,
  reporter: [['list']],
  use: {
    baseURL: 'http://localhost:8081',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
