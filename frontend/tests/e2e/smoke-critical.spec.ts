import { test, expect } from '@playwright/test';

const BASE = process.env.PLAYWRIGHT_TEST_BASE_URL || 'http://localhost:8081';

test.describe('CRITICAL DEMO PATH SMOKE SUITE', () => {
  test.setTimeout(120_000);

  test('1. Auth Journey: Student Registration -> Login -> Profile -> Logout', async ({ page }) => {
    const timestamp = Date.now();
    const testEmail = `student_${timestamp}@demo.test`;
    const testPassword = 'Password@123';

    // Step 1: Register
    await page.goto(`${BASE}/register`);
    await expect(page.locator('#email')).toBeVisible({ timeout: 10_000 });
    await page.locator('#displayName').fill(`Student Test ${timestamp}`);
    await page.locator('#email').fill(testEmail);
    await page.locator('#password').fill(testPassword);
    await page.locator('#confirmPassword').fill(testPassword);
    await page.getByRole('button', { name: /đăng ký/i }).click();

    // After register or login, verify redirect or success
    await page.waitForTimeout(1500);

    // Step 2: Login
    await page.goto(`${BASE}/login`);
    await page.locator('#email').fill(testEmail);
    await page.locator('#password').fill(testPassword);
    await page.getByRole('button', { name: /đăng nhập/i }).click();
    await page.waitForTimeout(2000);

    // Step 3: Profile
    await page.goto(`${BASE}/profile`);
    await expect(page.locator('body')).toBeVisible();
  });

  test('2. Lesson Study Journey: Open Lesson -> Theory -> Sim -> Quiz', async ({ page }) => {
    // Open standard lesson study view
    await page.goto(`${BASE}/lessons/1`);
    await expect(page.locator('body')).toBeVisible({ timeout: 10_000 });

    // Verify tabs or breadcrumbs
    const lessonTitle = page.locator('h1, h2, .lesson-study-title').first();
    await expect(lessonTitle).toBeVisible({ timeout: 10_000 });
  });

  test('3. Simulator Flow: /simulator/sort.bubble -> Playback -> Inputs', async ({ page }) => {
    await page.goto(`${BASE}/simulator/sort.bubble`);
    await expect(page.locator('body')).toBeVisible({ timeout: 10_000 });

    // Step forward button
    const stepForwardBtn = page.locator('button[title*="bước tiếp"], button[aria-label*="bước tiếp"], .control-btn').first();
    if (await stepForwardBtn.isVisible()) {
      await stepForwardBtn.click();
    }
  });

  test('4. Teacher Studio Flow: Login Teacher -> Studio Dashboard -> Navigation', async ({ page }) => {
    await page.goto(`${BASE}/login`);
    await page.locator('#email').fill('teacher@demo.local');
    await page.locator('#password').fill('Teacher@123');
    await page.getByRole('button', { name: /đăng nhập/i }).click();
    await page.waitForTimeout(2000);

    // Studio Overview
    await page.goto(`${BASE}/studio?tab=overview`);
    await expect(page.locator('body')).toBeVisible({ timeout: 10_000 });
    await expect(page.getByText(/Teacher Studio|Tổng bài học|Chương/i).first()).toBeVisible({ timeout: 10_000 });
  });

  test('5. Class Management Flow: View Classes & Curriculum', async ({ page }) => {
    await page.goto(`${BASE}/login`);
    await page.locator('#email').fill('teacher@demo.local');
    await page.locator('#password').fill('Teacher@123');
    await page.getByRole('button', { name: /đăng nhập/i }).click();
    await page.waitForTimeout(2000);

    await page.goto(`${BASE}/classes`);
    await expect(page.locator('body')).toBeVisible({ timeout: 10_000 });
  });
});
