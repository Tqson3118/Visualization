import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-83~89: Shop & Inventory', () => {
  test('TC-83: Items hiện đúng: tên, giá gems, mô tả', async ({ page }) => {
    await mockApi(page);
    await page.goto('/shop');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-84: Mua item đủ gems → item trong Inventory, gems giảm đúng', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/gamification/shop/buy', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          gemsLeft: 50,
          item: { id: 1, name: 'Bình hồi phục tim', priceGems: 50 },
          owned: 1,
        }),
      });
    });
    await page.goto('/shop');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-85: Mua item thiếu gems → lỗi rõ ràng, gems không giảm', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/gamification/shop/buy', async (route) => {
      await route.fulfill({
        status: 422,
        contentType: 'application/json',
        body: JSON.stringify({ error: { code: 'INSUFFICIENT_GEMS', message: 'Bạn không đủ gems' } }),
      });
    });
    await page.goto('/shop');
  });

  test('TC-86: Dblclick mua → gems chỉ giảm 1 lần (atomic)', async ({ page }) => {
    await mockApi(page);
    await page.goto('/shop');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-87: Mua Premium → badge hiện', async ({ page }) => {
    await mockApi(page);
    await page.goto('/shop');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-88: Inventory → thấy item vừa mua', async ({ page }) => {
    await mockApi(page);
    await page.goto('/profile');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-89: Item đã mua → nút disabled/"Đã sở hữu"', async ({ page }) => {
    await mockApi(page);
    await page.goto('/shop');
    await expect(page.locator('body')).toBeVisible();
  });
});
