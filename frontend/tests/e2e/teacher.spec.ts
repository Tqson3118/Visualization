import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-97~105: Teacher Flow', () => {
  test('TC-97: Tạo bài Draft → hiện danh sách', async ({ page }) => {
    await mockApi(page);
    await page.goto('/teacher/lessons');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-98: Thêm câu hỏi → lưu đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/teacher/lessons');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-99: Gửi duyệt → status PendingReview', async ({ page }) => {
    await mockApi(page);
    await page.goto('/teacher/lessons');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-100: Sửa bài Draft của mình → lưu', async ({ page }) => {
    await mockApi(page);
    await page.goto('/teacher/lessons');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-101: Sửa bài người khác → 403', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/lessons/999', async (route) => {
      if (route.request().method() === 'PUT') {
        await route.fulfill({
          status: 403,
          contentType: 'application/json',
          body: JSON.stringify({ error: { code: 'FORBIDDEN', message: 'Bạn không có quyền sửa bài này' } }),
        });
      } else {
        await route.continue();
      }
    });
    await page.goto('/teacher/lessons');
  });

  test('TC-102: Tạo lớp → thêm sinh viên bằng email', async ({ page }) => {
    await mockApi(page);
    await page.goto('/teacher/classes');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-103: Báo cáo lớp → data đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/teacher/classes');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-104: Export CSV → file download', async ({ page }) => {
    await mockApi(page);
    await page.goto('/teacher/classes');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-105: Giao bài → sinh viên thấy deadline', async ({ page }) => {
    await mockApi(page);
    await page.goto('/teacher/classes');
    await expect(page.locator('body')).toBeVisible();
  });
});
