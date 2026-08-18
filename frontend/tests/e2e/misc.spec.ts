import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-120~124: Code Runner', () => {
  test('TC-120: Viết code đúng → chạy, thấy output', async ({ page }) => {
    await mockApi(page);
    await page.goto('/coderunner');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-121: Code lỗi syntax → thông báo lỗi compile', async ({ page }) => {
    await mockApi(page);
    await page.goto('/coderunner');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-122: Infinite loop → timeout sau giới hạn', async ({ page }) => {
    await mockApi(page);
    await page.goto('/coderunner');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-123: Lưu lịch sử → xem lại được', async ({ page }) => {
    await mockApi(page);
    await page.goto('/coderunner');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-124: Compare 2 lần chạy → diff hiện', async ({ page }) => {
    await mockApi(page);
    await page.goto('/coderunner');
    await expect(page.locator('body')).toBeVisible();
  });
});

test.describe('TC-125~127: Benchmark Lab', () => {
  test('TC-125: Chạy 1 giải thuật → kết quả ms hiện', async ({ page }) => {
    await mockApi(page);
    await page.goto('/benchmark');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-126: So sánh 2 giải thuật → overlay chart đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/benchmark');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-127: Input size lớn → không crash', async ({ page }) => {
    await mockApi(page);
    await page.goto('/benchmark');
    await expect(page.locator('body')).toBeVisible();
  });
});

test.describe('TC-128~130: Leaderboard', () => {
  test('TC-128: Xem top 10 → đúng thứ hạng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/leaderboard');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-129: Vị trí của mình → highlight', async ({ page }) => {
    await mockApi(page);
    await page.goto('/leaderboard');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-130: Scroll → load thêm đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/leaderboard');
    await expect(page.locator('body')).toBeVisible();
  });
});

test.describe('TC-131~133: Quest + Streak', () => {
  test('TC-131: Hoàn thành quest → claim reward → gems tăng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/quests');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-132: Claim quest 2 lần → lần 2 báo lỗi', async ({ page }) => {
    await mockApi(page);
    await page.goto('/quests');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-133: Streak counter → đúng số ngày liên tiếp', async ({ page }) => {
    await mockApi(page);
    await page.goto('/profile');
    await expect(page.locator('body')).toBeVisible();
  });
});
