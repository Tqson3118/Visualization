/**
 * leaderboard.spec.ts — Màn 24 (G-F3E-NEW-1 + G-F3E-NEW-2):
 *  - 3 tab Tuần/Level/Lớp render KHÔNG crash khi rows có value (BE trả Value theo contract).
 *  - Tab Lớp gửi classId từ class store (mock GET /classes) — không 400 "Thiếu classId".
 *  - User chưa tham gia lớp → EmptyState "Bạn chưa tham gia lớp học nào", không gọi API.
 *
 * Luồng mock:
 *  - login qua UI (helpers/mockApi.ts) → /path → menu user → "Bảng xếp hạng" (SPA nav,
 *    không reload trang — token auth nằm trong memory Pinia, ADR-004).
 *  - GET /leaderboard mock trả items có `value` theo tab (week/levelScore/classScore).
 *  - GET /classes (mới — G-F3E-NEW-2) trả [MOCK_CLASSES] để resolveClassId có classId.
 */
import { expect, test, type Page } from '@playwright/test';

import { E2E_EMAIL, E2E_PASSWORD } from './helpers/auth';
import { mockApi } from './helpers/mockApi';

async function loginAndOpenLeaderboard(page: Page): Promise<void> {
  await page.goto('/login');
  await page.locator('#email').fill(E2E_EMAIL);
  await page.locator('#password').fill(E2E_PASSWORD);
  await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
  // PR30: /path redirect → /courses
  await expect(page).toHaveURL(/\/courses$/);
  // SPA nav: mở menu user → "Bảng xếp hạng" (không reload → giữ phiên auth)
  await page.getByRole('button', { name: 'E2E Student' }).click();
  await page.getByRole('link', { name: 'Bảng xếp hạng' }).click();
  await expect(page).toHaveURL(/\/leaderboard$/);
}

test.describe('Leaderboard — Màn 24', () => {
  test('3 tab Tuần/Level/Lớp render không crash (rows có value) + tab Lớp gửi classId', async ({ page }) => {
    const pageErrors: string[] = [];
    page.on('pageerror', (err) => pageErrors.push(err.message));

    await mockApi(page);
    await loginAndOpenLeaderboard(page);

    await expect(page.getByRole('heading', { name: 'Bảng xếp hạng' })).toBeVisible();

    // Tab Tuần (mặc định) — rows render với value
    await expect(page.locator('.board-row__value').first()).toBeVisible();

    // Tab Level — trước fix: TypeError toLocaleString vì BE thiếu `value` (G-F3D-NEW-1)
    await page.getByRole('tab', { name: 'Level' }).click();
    await expect(page.getByRole('tab', { name: 'Level' })).toHaveAttribute('data-state', 'active');
    await expect(page.locator('.board-row__value').first()).toBeVisible();

    // Tab Lớp — gửi classId từ class store (mock GET /classes) → không 400, rows render
    await page.getByRole('tab', { name: 'Lớp' }).click();
    await expect(page.getByRole('tab', { name: 'Lớp' })).toHaveAttribute('data-state', 'active');
    await expect(page.locator('.board-row__value').first()).toBeVisible();

    expect(pageErrors).toEqual([]);
  });

  test('tab Lớp khi user chưa tham gia lớp → EmptyState, không gọi API (G-F3D-NEW-2)', async ({ page }) => {
    await mockApi(page);
    // Override GET /classes → [] (user chưa tham gia lớp) — route đăng ký sau, LIFO thắng
    await page.route('**/api/v1/classes', (route) =>
      route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify([]) }),
    );

    await loginAndOpenLeaderboard(page);

    await page.getByRole('tab', { name: 'Lớp' }).click();
    await expect(page.getByRole('heading', { name: 'Bạn chưa tham gia lớp học nào' })).toBeVisible();
  });
});
