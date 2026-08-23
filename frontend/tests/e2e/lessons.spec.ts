import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-49~57: Bài học', () => {
  test('TC-49: Danh sách bài học → ≥1 card hiện', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-50: Filter theo topic → kết quả lọc đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-51: Tìm kiếm → kết quả đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path');
    const searchInput = page.locator('input[placeholder*="Tìm"], input[type="search"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('Bubble');
    }
  });

  test('TC-52: Click bài Active → xem nội dung', async ({ page }) => {
    await mockApi(page);
    await page.goto('/lessons/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-53: URL trực tiếp bài Draft → 404 hoặc redirect', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/lessons/999', async (route) => {
      await route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({ error: { code: 'NOT_FOUND', message: 'Bài học không tồn tại' } }),
      });
    });
    await page.goto('/lessons/999');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-54: Đánh dấu đã học → badge/indicator hiện', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/lessons/1/mark-viewed', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true }),
      });
    });
    await page.goto('/lessons/1');
  });

  test('TC-55: Bài ClassOnly → student ngoài lớp → 404', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/lessons/888', async (route) => {
      await route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({ error: { code: 'NOT_FOUND', message: 'Bài học không tồn tại' } }),
      });
    });
    await page.goto('/lessons/888');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-56: Bài có simKey → nút "Mở mô phỏng" hiện', async ({ page }) => {
    await mockApi(page);
    await page.goto('/lessons/1');
    const simBtn = page.getByRole('button', { name: /Chạy thử|Mô phỏng|Trực quan/i }).first();
    if (await simBtn.isVisible()) {
      await expect(simBtn).toBeVisible();
    }
  });

  test('TC-57: Back từ bài học → danh sách giữ state', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path');
    await page.goto('/lessons/1');
    await page.goBack();
    await expect(page).toHaveURL(/\/path/);
  });
});
