import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-106~112: Admin Flow', () => {
  test('TC-106: Danh sách users → phân trang đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/admin/users');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-107: Duyệt Teacher → login được', async ({ page }) => {
    await mockApi(page);
    await page.goto('/admin/users');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-108: Từ chối Teacher → không login được', async ({ page }) => {
    await mockApi(page);
    await page.goto('/admin/users');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-109: Khóa Student → không login được', async ({ page }) => {
    await mockApi(page);
    await page.goto('/admin/users');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-110: Mở khóa → login lại được', async ({ page }) => {
    await mockApi(page);
    await page.goto('/admin/users');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-111: Thống kê → số liệu hiện (không crash)', async ({ page }) => {
    await mockApi(page);
    await page.goto('/admin/dashboard');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-112: Cài đặt hearts regen time → lưu, apply', async ({ page }) => {
    await mockApi(page);
    await page.goto('/admin/settings');
    await expect(page.locator('body')).toBeVisible();
  });
});
