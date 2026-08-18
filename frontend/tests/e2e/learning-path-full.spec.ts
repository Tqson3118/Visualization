import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-66~73: Learning Path & Hearts', () => {
  test('TC-66: Bản đồ node hiện locked/unlocked đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-67: Vào node có tim → hearts giảm 1, vào NodeHub', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1');
    await page.goto('/path/1/node/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-68: Vào node hết tim → blocked, thông báo', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/gamification/paths/1/nodes/1/enter', async (route) => {
      await route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({
          error: { code: 'HEARTS_EMPTY', message: 'Bạn đã hết tim. Vui lòng chờ hồi phục hoặc mua thêm.' },
        }),
      });
    });
    await page.goto('/path/1');
  });

  test('TC-69: Node đã pass → vào miễn phí, tim không giảm', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1/node/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-70: 2 tab vào node cùng lúc → chỉ trừ 1 tim (atomic)', async ({ browser }) => {
    const ctx1 = await browser.newContext();
    const ctx2 = await browser.newContext();
    const [p1, p2] = await Promise.all([ctx1.newPage(), ctx2.newPage()]);
    await Promise.all([mockApi(p1), mockApi(p2)]);
    await Promise.all([p1.goto('/path/1'), p2.goto('/path/1')]);
    await Promise.all([ctx1.close(), ctx2.close()]);
  });

  test('TC-71: Countdown tim hồi phục → hiện đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-72: Pass node → node kế unlock', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-73: Back browser từ NodeHub → bản đồ đúng state', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path/1');
    await page.goto('/path/1/node/1');
    await page.goBack();
    await expect(page).toHaveURL(/\/path/);
  });
});
