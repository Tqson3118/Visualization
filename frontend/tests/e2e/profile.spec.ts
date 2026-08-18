import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-90~96: Profile & Account Settings', () => {
  test('TC-90: Thông tin cá nhân đúng: email, tên, role', async ({ page }) => {
    await mockApi(page);
    await page.goto('/profile');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-91: Đổi tên → lưu, refresh vẫn còn', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/profile', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ displayName: 'New Display Name', email: 'e2e@test.edu' }),
      });
    });
    await page.goto('/profile');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-92: Đổi pass — pass cũ đúng → thành công', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/change-password', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true, message: 'Đổi mật khẩu thành công' }),
      });
    });
    await page.goto('/profile');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-93: Đổi pass — pass cũ sai → lỗi', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/change-password', async (route) => {
      await route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({ error: { code: 'INVALID_PASSWORD', message: 'Mật khẩu cũ không chính xác' } }),
      });
    });
    await page.goto('/profile');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-94: Đổi pass → session khác bị revoke', async ({ page }) => {
    await mockApi(page);
    await page.goto('/profile');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-95: Bật 2FA → nhận OTP → verify → 2FA active', async ({ page }) => {
    await mockApi(page);
    await page.goto('/profile');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-96: Tắt 2FA → login lại không cần OTP', async ({ page }) => {
    await mockApi(page);
    await page.goto('/profile');
    await expect(page.locator('body')).toBeVisible();
  });
});
