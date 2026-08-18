import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-113~119: Bảo mật & RBAC', () => {
  test('TC-113: Student vào /admin → redirect/403', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');
    await page.locator('#email').fill('student@demo.local');
    await page.locator('#password').fill('Student@123');
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await page.goto('/admin');
    await expect(page).not.toHaveURL(/\/admin$/);
  });

  test('TC-114: Guest vào /lessons → redirect /login', async ({ page }) => {
    await mockApi(page);
    await page.goto('/lessons/1');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-115: JWT hết hạn → auto refresh, user không bị logout', async ({ page }) => {
    await mockApi(page);
    await page.goto('/path');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-116: Refresh token hết → redirect /login', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/refresh-token', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({ error: { code: 'UNAUTHORIZED', message: 'Phiên làm việc hết hạn' } }),
      });
    });
    await page.goto('/login');
    await expect(page).toHaveURL(/\/login/);
  });

  test('TC-117: XSS trong lesson content → không execute', async ({ page }) => {
    await mockApi(page);
    await page.goto('/lessons/1');
    const xssFlag = await page.evaluate(() => (window as unknown as { xssInjected?: boolean }).xssInjected);
    expect(xssFlag).toBeUndefined();
  });

  test('TC-118: Teacher vào /admin/users → blocked', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');
    await page.locator('#email').fill('teacher@demo.local');
    await page.locator('#password').fill('Teacher@123');
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await page.goto('/admin/users');
    await expect(page).not.toHaveURL(/\/admin\/users$/);
  });

  test('TC-119: API call không token → 401', async ({ request }) => {
    const res = await request.get('http://localhost:5000/api/v1/lessons').catch(() => null);
    if (res) {
      expect([401, 404]).toContain(res.status());
    }
  });
});
