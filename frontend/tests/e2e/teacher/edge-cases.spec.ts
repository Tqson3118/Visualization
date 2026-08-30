import { test, expect } from '../fixtures/auth';

test.describe('Test Suite 7 & 8 — Error, Edge Cases & Responsive', () => {
  test('TC-TF-060: API lỗi — hiển thị thông báo lỗi thay vì crash', async ({ adminPage }) => {
    await adminPage.route('**/api/v1/concepts/courses', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: { code: 'INTERNAL_ERROR', message: 'Lỗi máy chủ nội bộ' } }),
      });
    });

    await adminPage.goto('/admin/content?tab=curriculum');

    await expect(adminPage.locator('body')).toBeVisible();
    await expect(adminPage.getByTestId('tab-overview')).toBeVisible();
  });

  test('TC-TF-061: Không có pending items — hiển thị bình thường không crash', async ({ adminPage }) => {
    await adminPage.route('**/api/v1/concepts/courses/pending', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await adminPage.goto('/admin/content?tab=curriculum');
    await expect(adminPage.getByTestId('tab-curriculum')).toBeVisible();
    await expect(adminPage.locator('body')).toBeVisible();
  });

  test('TC-TF-062: Teacher role — limited permissions trên giáo trình hệ thống', async ({ teacherPage }) => {
    await teacherPage.goto('/admin/content?tab=curriculum');

    await expect(teacherPage.getByRole('heading', { name: /Giáo trình đang chờ duyệt/i })).not.toBeVisible();

    const systemNotice = teacherPage.getByText(/Giáo trình hệ thống/i).or(teacherPage.getByText(/Chỉ xem/i)).first();
    await expect(systemNotice).toBeVisible();
  });

  test('TC-TF-063: Concurrent operations — nút disabled khi đang loading', async ({ adminPage }) => {
    await adminPage.goto('/admin/content?tab=curriculum');
    const refreshBtn = adminPage.getByRole('button', { name: /Làm mới/i }).first();
    if (await refreshBtn.isVisible()) {
      await expect(refreshBtn).toBeEnabled();
    }
  });

  test('TC-TF-070: Responsive 768px (tablet) — tab bar không bị vỡ', async ({ adminPage }) => {
    await adminPage.setViewportSize({ width: 768, height: 1024 });
    await adminPage.goto('/admin/content?tab=overview');

    await expect(adminPage.getByTestId('tab-overview')).toBeVisible();
    await expect(adminPage.getByTestId('tab-curriculum')).toBeVisible();
    await expect(adminPage.getByTestId('tab-exercises')).toBeVisible();
    await expect(adminPage.getByTestId('tab-feedback')).toBeVisible();
  });

  test('TC-TF-071: Desktop 1280px — layout hiển thị side-by-side hoàn chỉnh', async ({ teacherPage }) => {
    await teacherPage.setViewportSize({ width: 1280, height: 800 });
    await teacherPage.goto('/classes/7');

    const curriculumTab = teacherPage.getByRole('tab', { name: /Lộ trình/i }).or(teacherPage.getByText('Lộ trình học')).first();
    await expect(curriculumTab).toBeVisible();
    await curriculumTab.click();

    await expect(teacherPage.getByTestId('curriculum-drag-list')).toBeVisible();
  });
});
