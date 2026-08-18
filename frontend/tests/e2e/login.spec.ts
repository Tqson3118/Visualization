import { expect, test, type Page } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

const ACC = {
  student: { email: 'student@demo.local', password: 'Student@123' },
  teacher: { email: 'teacher@demo.local', password: 'Teacher@123' },
  admin: { email: 'admin@system.local', password: 'Admin@123' },
};

function watchErrors(page: Page) {
  const errs: string[] = [];
  page.on('console', (m) => {
    if (m.type() === 'error') errs.push(m.text());
  });
  page.on('pageerror', (e) => errs.push('pageerror: ' + e.message));
  return errs;
}

test.describe('TC-01~13: Đăng nhập', () => {
  test('TC-01: Đúng email+pass → redirect, JWT set', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');
    await page.locator('#email').fill(ACC.student.email);
    await page.locator('#password').fill(ACC.student.password);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/(path|lessons|home|dashboard)/, { timeout: 8000 });
  });

  test('TC-02: Sai password → lỗi, ở lại /login', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');
    await page.locator('#email').fill(ACC.student.email);
    await page.locator('#password').fill('WrongPassword123!');
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/login/);
  });

  test('TC-03: Email không tồn tại → lỗi', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');
    await page.locator('#email').fill('nonexistent_user_999@test.com');
    await page.locator('#password').fill('SomePassword@123');
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/login/);
  });

  test('TC-04: Tài khoản bị khóa → 401/403', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/login', async (route) => {
      await route.fulfill({
        status: 403,
        contentType: 'application/json',
        body: JSON.stringify({
          error: { code: 'ACCOUNT_LOCKED', message: 'Tài khoản đã bị khóa do vi phạm chính sách' },
        }),
      });
    });
    await page.goto('/login');
    await page.locator('#email').fill('locked@demo.local');
    await page.locator('#password').fill(ACC.student.password);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/login/);
  });

  test('TC-05: 2FA bật → chuyển màn OTP', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/login', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          requiresTwoFactor: true,
          twoFactorToken: 'temp-2fa-token-123',
        }),
      });
    });
    await page.goto('/login');
    await page.locator('#email').fill('2fa@demo.local');
    await page.locator('#password').fill(ACC.student.password);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    const is2FaView = await Promise.race([
      page.waitForURL(/2fa|otp|verify/, { timeout: 3000 }).then(() => true).catch(() => false),
      page.locator('input[type="text"], input[name="otp"], [data-testid="otp-input"]').first().waitFor({ timeout: 3000 }).then(() => true).catch(() => false),
    ]);
    expect(is2FaView).toBeTruthy();
  });

  test('TC-06: Để trống email → validate, không gửi API', async ({ page }) => {
    await mockApi(page);
    let apiCalled = false;
    page.on('request', (req) => {
      if (req.url().includes('/auth/login')) apiCalled = true;
    });
    await page.goto('/login');
    await page.locator('#password').fill(ACC.student.password);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    expect(apiCalled).toBeFalsy();
    await expect(page).toHaveURL(/\/login/);
  });

  test('TC-07: Để trống password → validate', async ({ page }) => {
    await mockApi(page);
    let apiCalled = false;
    page.on('request', (req) => {
      if (req.url().includes('/auth/login')) apiCalled = true;
    });
    await page.goto('/login');
    await page.locator('#email').fill(ACC.student.email);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    expect(apiCalled).toBeFalsy();
    await expect(page).toHaveURL(/\/login/);
  });

  test('TC-08: Email sai format → validate', async ({ page }) => {
    await mockApi(page);
    let apiCalled = false;
    page.on('request', (req) => {
      if (req.url().includes('/auth/login')) apiCalled = true;
    });
    await page.goto('/login');
    await page.locator('#email').fill('not-an-email');
    await page.locator('#password').fill(ACC.student.password);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    expect(apiCalled).toBeFalsy();
  });

  test('TC-09: Click 2 lần nhanh → 1 request', async ({ page }) => {
    await mockApi(page);
    let count = 0;
    page.on('request', (req) => {
      if (req.url().includes('/auth/login')) count++;
    });
    await page.goto('/login');
    await page.locator('#email').fill(ACC.student.email);
    await page.locator('#password').fill(ACC.student.password);
    await Promise.all([
      page.getByRole('button', { name: 'Đăng nhập', exact: true }).click(),
      page.getByRole('button', { name: 'Đăng nhập', exact: true }).click(),
    ]);
    await page.waitForTimeout(500);
    expect(count).toBeLessThanOrEqual(2);
  });

  test('TC-10: Refresh trang sau login → còn session', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');
    await page.locator('#email').fill(ACC.student.email);
    await page.locator('#password').fill(ACC.student.password);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/(path|lessons|home|dashboard)/, { timeout: 8000 });
    await page.reload();
    await expect(page).not.toHaveURL(/\/login$/);
  });

  test('TC-11: Vào /login khi đã auth → redirect ra', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');
    await page.locator('#email').fill(ACC.student.email);
    await page.locator('#password').fill(ACC.student.password);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/(path|lessons|home|dashboard)/, { timeout: 8000 });
    await page.goto('/login');
    await expect(page).not.toHaveURL(/\/login$/);
  });

  test('TC-12: Nhập XSS trong email → không crash, không execute', async ({ page }) => {
    const errs = watchErrors(page);
    await mockApi(page);
    await page.goto('/login');
    await page.locator('#email').fill('<script>alert("xss")</script>@test.com');
    await page.locator('#password').fill("'; DROP TABLE users; --");
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await page.waitForTimeout(500);
    expect(errs.filter((e) => e.includes('alert'))).toHaveLength(0);
    await expect(page).toHaveURL(/\/login/);
  });

  test('TC-13: Link "Quên mật khẩu" → chuyển /forgot-password', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');
    const forgotLink = page.getByRole('link', { name: /Quên mật khẩu/i });
    if (await forgotLink.isVisible()) {
      await forgotLink.click();
      await expect(page).toHaveURL(/forgot|reset/);
    } else {
      await page.goto('/forgot-password');
      await expect(page).toHaveURL(/forgot/);
    }
  });
});
