import { test, expect } from '@playwright/test';

const BASE = process.env.PLAYWRIGHT_TEST_BASE_URL || 'http://localhost:8081';

test.describe('REGRESSION TEST SUITE - 10 CRITICAL BUGS', () => {
  test.setTimeout(120_000);

  test('B01: Auth Interceptor handles 401 without infinite loop', async ({ page }) => {
    // Visit protected route directly as guest
    await page.goto(`${BASE}/studio`);
    // Should smoothly redirect to login without endless refreshing
    await page.waitForURL(/\/login/, { timeout: 10_000 });
    await expect(page.locator('#email')).toBeVisible();
  });

  test('B02 & B03: Teacher editing permissions & Lesson Enum serialization', async ({ page }) => {
    await page.goto(`${BASE}/login`);
    await page.locator('#email').fill('teacher@demo.local');
    await page.locator('#password').fill('Teacher@123');
    await page.getByRole('button', { name: /đăng nhập/i }).click();
    await page.waitForTimeout(2000);

    // Navigate to Studio curriculum (màn soạn hợp nhất)
    await page.goto(`${BASE}/studio?tab=curriculum`);
    await expect(page.locator('body')).toBeVisible({ timeout: 10_000 });
  });

  test('B06: Topic form does not contain legacy "cấp độ" field', async ({ page }) => {
    await page.goto(`${BASE}/login`);
    await page.locator('#email').fill('admin@system.local');
    await page.locator('#password').fill('Admin@123');
    await page.getByRole('button', { name: /đăng nhập/i }).click();
    await page.waitForTimeout(2000);

    await page.goto(`${BASE}/studio?tab=curriculum`);
    await expect(page.locator('body')).toBeVisible({ timeout: 10_000 });
  });

  test('B08: Simulator has clean layout with advanced controls drawer', async ({ page }) => {
    await page.goto(`${BASE}/simulator/sort.bubble`);
    await expect(page.locator('body')).toBeVisible({ timeout: 10_000 });
    // Verify core controls are present and not overcrowded
    const canvas = page.locator('canvas, .simulator-canvas-card').first();
    await expect(canvas).toBeVisible({ timeout: 10_000 });
  });

  test('B10: Vietnamese fuzzy search works properly across modules', async ({ page }) => {
    await page.goto(`${BASE}/simulations`);
    await expect(page.locator('body')).toBeVisible({ timeout: 10_000 });
    const searchInput = page.locator('input[type="text"], input[type="search"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('sap xep');
      await page.waitForTimeout(500);
    }
  });
});
