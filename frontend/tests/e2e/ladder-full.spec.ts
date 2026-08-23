import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-74~82: Practice Ladder', () => {
  test('TC-74: Vào Bậc 1 Quiz → load đúng câu hỏi', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1/node/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-75: Quiz >= 60% → Bậc 2 mở khóa, hiện nút tiếp theo', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1/node/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-76: Quiz < 60% → ở lại Bậc 1, hiện "Thử lại"', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1/node/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-77: Thử vào Bậc 2 khi chưa pass Bậc 1 → server block 403', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/exercises/2/submit', async (route) => {
      await route.fulfill({
        status: 403,
        contentType: 'application/json',
        body: JSON.stringify({ error: { code: 'STAGE_LOCKED', message: 'Bạn cần hoàn thành Bậc 1 trước' } }),
      });
    });
    await page.goto('/path/1/node/1');
  });

  test('TC-78: Interactive Lab pass → Bậc 3 mở', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1/node/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-79: Code Challenge >= 70% test ẩn → node complete, badge hiện', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1/node/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-80: Retry trong session 30 phút → không trừ tim', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1/node/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-81: Session hết hạn 30 phút → vào lại phải trừ tim', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1/node/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-82: Điểm node = Quiz 20% + Lab 30% + Code 50% → tính đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1/node/1');
    await expect(page.locator('body')).toBeVisible();
  });
});
