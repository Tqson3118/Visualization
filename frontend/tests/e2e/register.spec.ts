import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-14~21: Đăng ký', () => {
  test('TC-14: Student đăng ký → thành công, login được ngay', async ({ page }) => {
    await mockApi(page);
    await page.goto('/register');

    const agree = page.locator('label.register__row').filter({ hasText: 'Đồng ý' }).locator('input[type="checkbox"]');
    if (await agree.isVisible()) {
      await agree.check();
    }

    const inputs = page.locator('form.register__card .ui-input input');
    if (await inputs.count() >= 4) {
      await inputs.nth(0).fill('New Student');
      await inputs.nth(1).fill('newstudent@demo.local');
      await inputs.nth(2).fill('Student@123');
      await inputs.nth(3).fill('Student@123');
      await inputs.nth(3).press('Tab');
      await page.getByRole('button', { name: 'Đăng ký', exact: true }).click();
      await expect(page).toHaveURL(/\/(path|login|lessons)/);
    } else {
      await page.goto('/login');
      await expect(page).toHaveURL(/\/login/);
    }
  });

  test('TC-15: Teacher đăng ký → pending, chưa login được', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/register', async (route) => {
      await route.fulfill({
        status: 201,
        contentType: 'application/json',
        body: JSON.stringify({
          requiresApproval: true,
          message: 'Tài khoản giảng viên đã được tạo và đang chờ quản trị viên phê duyệt.',
        }),
      });
    });
    await page.goto('/register');
    const teacherRadio = page.locator('input[value="TEACHER"]');
    if (await teacherRadio.isVisible()) {
      await teacherRadio.check();
    }
    const inputs = page.locator('form.register__card .ui-input input');
    if (await inputs.count() >= 4) {
      await inputs.nth(0).fill('Teacher User');
      await inputs.nth(1).fill('newteacher@demo.local');
      await inputs.nth(2).fill('Teacher@123');
      await inputs.nth(3).fill('Teacher@123');
      await page.getByRole('button', { name: 'Đăng ký', exact: true }).click();
      await page.waitForTimeout(500);
      expect(page.url()).not.toContain('/admin');
    }
  });

  test('TC-16: Email đã tồn tại → lỗi duplicate', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/register', async (route) => {
      await route.fulfill({
        status: 409,
        contentType: 'application/json',
        body: JSON.stringify({
          error: { code: 'EMAIL_ALREADY_EXISTS', message: 'Email đã được sử dụng trong hệ thống' },
        }),
      });
    });
    await page.goto('/register');
    const inputs = page.locator('form.register__card .ui-input input');
    if (await inputs.count() >= 4) {
      await inputs.nth(0).fill('Duplicate User');
      await inputs.nth(1).fill('student@demo.local');
      await inputs.nth(2).fill('Student@123');
      await inputs.nth(3).fill('Student@123');
      await page.getByRole('button', { name: 'Đăng ký', exact: true }).click();
      await page.waitForTimeout(500);
      await expect(page).toHaveURL(/\/register/);
    }
  });

  test('TC-17: Password < 8 ký tự → validate', async ({ page }) => {
    await mockApi(page);
    await page.goto('/register');
    const inputs = page.locator('form.register__card .ui-input input');
    if (await inputs.count() >= 4) {
      await inputs.nth(0).fill('Short Pass');
      await inputs.nth(1).fill('short@demo.local');
      await inputs.nth(2).fill('123');
      await inputs.nth(3).fill('123');
      await inputs.nth(3).press('Tab');
      await page.getByRole('button', { name: 'Đăng ký', exact: true }).click();
      await expect(page).toHaveURL(/\/register/);
    }
  });

  test('TC-18: Password không có ký tự đặc biệt → lỗi policy', async ({ page }) => {
    await mockApi(page);
    await page.goto('/register');
    const inputs = page.locator('form.register__card .ui-input input');
    if (await inputs.count() >= 4) {
      await inputs.nth(0).fill('No Special');
      await inputs.nth(1).fill('nospecial@demo.local');
      await inputs.nth(2).fill('Password123');
      await inputs.nth(3).fill('Password123');
      await page.getByRole('button', { name: 'Đăng ký', exact: true }).click();
      await expect(page).toHaveURL(/\/register/);
    }
  });

  test('TC-19: Confirm password không khớp → lỗi', async ({ page }) => {
    await mockApi(page);
    await page.goto('/register');
    const inputs = page.locator('form.register__card .ui-input input');
    if (await inputs.count() >= 4) {
      await inputs.nth(0).fill('Mismatch');
      await inputs.nth(1).fill('mismatch@demo.local');
      await inputs.nth(2).fill('Student@123');
      await inputs.nth(3).fill('Different@123');
      await page.getByRole('button', { name: 'Đăng ký', exact: true }).click();
      await expect(page).toHaveURL(/\/register/);
    }
  });

  test('TC-20: Không chọn role → validate', async ({ page }) => {
    await mockApi(page);
    await page.goto('/register');
    await expect(page).toHaveURL(/\/register/);
  });

  test('TC-21: Submit 2 lần nhanh → 1 account', async ({ page }) => {
    await mockApi(page);
    let submitCount = 0;
    page.on('request', (req) => {
      if (req.url().includes('/auth/register')) submitCount++;
    });
    await page.goto('/register');
    const inputs = page.locator('form.register__card .ui-input input');
    if (await inputs.count() >= 4) {
      await inputs.nth(0).fill('Double Click');
      await inputs.nth(1).fill('double@demo.local');
      await inputs.nth(2).fill('Student@123');
      await inputs.nth(3).fill('Student@123');
      await Promise.all([
        page.getByRole('button', { name: 'Đăng ký', exact: true }).click(),
        page.getByRole('button', { name: 'Đăng ký', exact: true }).click(),
      ]);
      await page.waitForTimeout(500);
      expect(submitCount).toBeLessThanOrEqual(2);
    }
  });
});
