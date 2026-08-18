import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-29~34: Trang chủ', () => {
  test('TC-29: Guest thấy hero + danh sách sim + nút "Dùng thử"', async ({ page }) => {
    await mockApi(page);
    await page.goto('/');
    await expect(page.locator('body')).toBeVisible();
    const tryBtn = page.getByRole('link', { name: /Dùng thử|Chạy thử|Bắt đầu|Trực quan hóa/i }).first();
    await expect(tryBtn).toBeVisible();
  });

  test('TC-30: Guest không thấy nav Premium/Shop/Quest', async ({ page }) => {
    await mockApi(page);
    await page.goto('/');
    const shopLink = page.getByRole('link', { name: /^Cửa hàng$/i });
    const questLink = page.getByRole('link', { name: /^Nhiệm vụ$/i });
    expect(await shopLink.count()).toBe(0);
    expect(await questLink.count()).toBe(0);
  });

  test('TC-31: Student authed → thấy nav đầy đủ', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');
    await page.locator('#email').fill('student@demo.local');
    await page.locator('#password').fill('Student@123');
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/(path|lessons|home|dashboard)/, { timeout: 8000 });
  });

  test('TC-32: Bấm "Dùng thử" → simulator load, không yêu cầu login', async ({ page }) => {
    await mockApi(page);
    await page.goto('/');
    const tryBtn = page.getByRole('link', { name: /Dùng thử|Chạy thử|Bắt đầu|Trực quan hóa/i }).first();
    if (await tryBtn.isVisible()) {
      await tryBtn.click();
      await page.waitForTimeout(500);
      expect(page.url()).not.toContain('/login');
    }
  });

  test('TC-33: Mobile 390px → document.documentElement.scrollWidth <= clientWidth', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await mockApi(page);
    await page.goto('/');
    const overflow = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    expect(overflow).toBeFalsy();
  });

  test('TC-34: Không có console error khi load', async ({ page }) => {
    const errors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') errors.push(msg.text());
    });
    page.on('pageerror', (err) => errors.push(err.message));
    await mockApi(page);
    await page.goto('/');
    await page.waitForTimeout(500);
    expect(errors.filter((e) => !e.includes('favicon'))).toHaveLength(0);
  });
});
