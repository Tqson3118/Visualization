/**
 * auth.spec.ts — TEST-UI-001 (bước 1-2: register → login → /path) + TEST-UI-005 (guard).
 *
 * Backend mock 100% (helpers/mockApi.ts) — KHÔNG phụ thuộc backend thật.
 * Selector theo LoginView/RegisterView/PathRedirectView thật:
 *   - LoginView: input #email / #password (label for=...), nút submit "Đăng nhập".
 *   - RegisterView: 4 Input (.ui-input input theo thứ tự Họ tên/Email/Mật khẩu/Xác nhận),
 *     checkbox "Đồng ý...", nút submit "Đăng ký".
 *   - PathRedirectView: thẻ topic là RouterLink → role=link "Sắp xếp & Tìm kiếm" (mock GET /topics).
 */
import { expect, test } from '@playwright/test';

import { E2E_EMAIL, E2E_PASSWORD } from './helpers/auth';
import { mockApi } from './helpers/mockApi';

test.describe('Auth — TEST-UI-001 / TEST-UI-005', () => {
  test('đăng ký tài khoản học viên thành công → vào /path', async ({ page }) => {
    await mockApi(page);
    await page.goto('/register');

    // Đồng ý chính sách TRƯỚC khi fill: click checkbox lúc không có input text đang focus
    // tránh race blur→validate→re-render nuốt click (xem ghi chú cuối file).
    const agree = page
      .locator('label.register__row')
      .filter({ hasText: 'Đồng ý' })
      .locator('input[type="checkbox"]');
    await agree.check();
    await expect(agree).toBeChecked();

    // Form hiển thị đủ 4 trường Input (thứ tự template RegisterView)
    const inputs = page.locator('form.register__card .ui-input input');
    await expect(inputs).toHaveCount(4);
    await inputs.nth(0).fill('E2E Student'); // Họ tên
    await inputs.nth(1).fill(E2E_EMAIL); // Email
    await inputs.nth(2).fill(E2E_PASSWORD); // Mật khẩu
    await inputs.nth(3).fill(E2E_PASSWORD); // Xác nhận mật khẩu

    // Blur input cuối (Tab) để validate xong + DOM ổn định TRƯỚC khi bấm nút —
    // nếu không, re-render giữa mousedown/mouseup làm click bị nuốt (đã probe).
    await inputs.nth(3).press('Tab');
    await page.waitForTimeout(200);

    // Submit → mock POST /auth/register 201 → student → router.replace('/path')
    await page.getByRole('button', { name: 'Đăng ký', exact: true }).click();
    await expect(page).toHaveURL(/\/path$/);

    // PathRedirectView hiển thị topic từ mock GET /topics
    await expect(page.getByRole('link', { name: /Sắp xếp & Tìm kiếm/ })).toBeVisible();
  });

  test('đăng nhập đúng → vào /path', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');

    await page.locator('#email').fill(E2E_EMAIL);
    await page.locator('#password').fill(E2E_PASSWORD);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();

    // Mock POST /auth/login → token + user → LoginView router.replace('/path')
    await expect(page).toHaveURL(/\/path$/);
    await expect(page.getByRole('link', { name: /Sắp xếp & Tìm kiếm/ })).toBeVisible();
  });

  test('guard: /profile chưa đăng nhập → /login?redirect=... → sau login quay lại /profile', async ({ page }) => {
    await mockApi(page);
    await page.goto('/profile');

    // Router guard (requiresAuth) → login kèm redirect (SDD §3.4)
    await expect(page).toHaveURL(/\/login\?redirect=/);
    await expect(page.getByRole('heading', { name: 'Đăng nhập' })).toBeVisible();

    // Login ngay trên trang redirect này → tự quay lại /profile (KHÔNG reload — giữ token memory)
    await page.locator('#email').fill(E2E_EMAIL);
    await page.locator('#password').fill(E2E_PASSWORD);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();

    await expect(page).toHaveURL(/\/profile$/);
    // ProfileView hiển thị displayName từ user mock (GET /auth/me + gamification/progress mock)
    await expect(page.getByRole('heading', { name: 'E2E Student' })).toBeVisible();
  });
});

/**
 * GHI CHÚ đã probe (12/08/2026) — RegisterView:
 * 1. Click checkbox/submit khi một input text đang focus sẽ bị NUỐT: mousedown → blur input
 *    → validate() → re-render (error hiện/biến mất đổi layout) → mouseup rơi sai vị trí.
 *    Workaround: checkbox check TRƯỚC khi fill; blur input cuối (Tab) trước khi bấm submit.
 * 2. validate() dùng Object.assign(fieldErrors, errors) với errors={} KHÔNG xóa key cũ
 *    (stale error hiển thị tới khi có key mới ghi đè) — bug UI tiềm ẩn, ngoài phạm vi task
 *    (KHÔNG sửa view production) — đề xuất task sau.
 */
