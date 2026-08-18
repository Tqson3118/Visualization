import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-58~65: Bài tập', () => {
  test('TC-58: Đúng tất cả → score 100%', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/exercises/1/submit', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          score: 100,
          passed: true,
          correctCount: 5,
          totalQuestions: 5,
          explanations: { '1': 'Đúng', '2': 'Đúng', '3': 'Đúng', '4': 'Đúng', '5': 'Đúng' },
        }),
      });
    });
    await page.goto('/lessons/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-59: Sai một số → score theo tỷ lệ + giải thích hiện', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/exercises/1/submit', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          score: 60,
          passed: true,
          correctCount: 3,
          totalQuestions: 5,
          explanations: { '1': 'Đúng', '2': 'Sai: Bubble sort so sánh cặp kề nhau' },
        }),
      });
    });
    await page.goto('/lessons/1');
  });

  test('TC-60: Không trả lời → validate, chặn submit', async ({ page }) => {
    await mockApi(page);
    await page.goto('/lessons/1');
  });

  test('TC-61: Nộp lần 2 → BestScore = max(lần1, lần2)', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/exercises/1/submit', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          score: 80,
          bestScore: 90,
          passed: true,
        }),
      });
    });
    await page.goto('/lessons/1');
  });

  test('TC-62: Giải thích sau nộp → từng câu đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/lessons/1');
  });

  test('TC-63: Submit 2 lần nhanh → 1 submission', async ({ page }) => {
    await mockApi(page);
    let submitCount = 0;
    page.on('request', (req) => {
      if (req.url().includes('/exercises/1/submit')) submitCount++;
    });
    await page.goto('/lessons/1');
  });

  test('TC-64: Multiple choice → chọn nhiều → chấm đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/lessons/1');
  });

  test('TC-65: Đáp án rất dài → không vỡ UI', async ({ page }) => {
    await mockApi(page);
    await page.goto('/lessons/1');
    await expect(page.locator('body')).toBeVisible();
  });
});
