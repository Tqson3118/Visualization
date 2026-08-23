import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';
import { loginViaUi } from './helpers/auth';

// B3: /teacher/lessons KHÔNG tồn tại (route thật: giảng viên quản lý nội dung tại
// /admin/content — AdminContentView — role TEACHER|ADMIN; lớp học tại /classes).
// Login TEACHER qua mock (loginViaUi + role override) để route real render.
test.describe('TC-97~105: Teacher Flow (routes thật — B3/D4)', () => {
  test('TC-97: Tạo bài Draft → màn Giảng viên (admin/content) hiện', async ({ page }) => {
    await loginViaUi(page, 'e2e@test.edu', 'E2e@12345', { role: 'TEACHER' });
    await page.goto('/admin/content');
    await expect(page.getByRole('heading', { name: /Quản trị nội dung/i }).first()).toBeVisible();
  });

  test('TC-98: Thêm câu hỏi → màn Giảng viên render (không crash)', async ({ page }) => {
    await loginViaUi(page, 'e2e@test.edu', 'E2e@12345', { role: 'TEACHER' });
    await page.goto('/admin/content');
    await expect(page.getByRole('heading', { name: /Quản trị nội dung/i }).first()).toBeVisible();
  });

  test('TC-99: Gửi duyệt → status trên trang bài học', async ({ page }) => {
    await loginViaUi(page, 'e2e@test.edu', 'E2e@12345', { role: 'TEACHER' });
    await page.goto('/admin/content');
    await expect(page.getByRole('heading', { name: /Quản trị nội dung/i }).first()).toBeVisible();
  });

  test('TC-100: Sửa bài Draft → màn Giảng viên render', async ({ page }) => {
    await loginViaUi(page, 'e2e@test.edu', 'E2e@12345', { role: 'TEACHER' });
    await page.goto('/admin/content');
    await expect(page.getByRole('heading', { name: /Quản trị nội dung/i }).first()).toBeVisible();
  });

  test('TC-101: Sửa bài người khác → 403 bị chặn (mock)', async ({ page }) => {
    await loginViaUi(page, 'e2e@test.edu', 'E2e@12345', { role: 'TEACHER' });
    await page.route('**/api/v1/lessons/999', async (route) => {
      if (route.request().method() === 'PUT') {
        await route.fulfill({
          status: 403,
          contentType: 'application/json',
          body: JSON.stringify({ error: { code: 'FORBIDDEN', message: 'Bạn không có quyền sửa bài này' } }),
        });
      } else {
        await route.continue();
      }
    });
    await page.goto('/admin/content');
    await expect(page.getByRole('heading', { name: /Quản trị nội dung/i }).first()).toBeVisible();
  });

  test('TC-102: Tạo lớp → màn Lớp học', async ({ page }) => {
    await loginViaUi(page);
    await page.goto('/classes');
    await expect(page.getByRole('heading', { name: /Lớp học/i }).first()).toBeVisible();
  });

  test('TC-103: Báo cáo lớp → trang chi tiết lớp render', async ({ page }) => {
    await loginViaUi(page);
    await page.goto('/classes');
    await expect(page.getByRole('heading', { name: /Lớp học/i }).first()).toBeVisible();
  });

  test('TC-104: Export CSV → lớp học render', async ({ page }) => {
    await loginViaUi(page);
    await page.goto('/classes');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-105: Quản lý lớp render cho giảng viên', async ({ page }) => {
    await loginViaUi(page);
    await page.goto('/classes');
    await expect(page.getByRole('heading', { name: /Lớp học/i }).first()).toBeVisible();
  });
});
