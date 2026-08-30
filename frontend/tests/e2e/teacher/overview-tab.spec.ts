import { test, expect } from '../fixtures/auth';

test.describe('Test Suite 6 — StudioOverviewTab', () => {
  test('TC-TF-050: Overview hiển thị KPI stats đúng', async ({ teacherPage }) => {
    await teacherPage.goto('/admin/content?tab=overview');

    // Hero banner hiển thị
    await expect(teacherPage.getByRole('heading', { name: /Xin chào/i }).first()).toBeVisible();

    // 4 KPI cards
    await expect(teacherPage.getByText(/Tổng bài học/i).first()).toBeVisible();
    await expect(teacherPage.getByText(/Chương \/ Topic/i).first()).toBeVisible();
    await expect(teacherPage.getByText(/Giáo trình \(Course\)/i).first()).toBeVisible();
    await expect(teacherPage.getByText(/Lớp học/i).first()).toBeVisible();

    // Bảng bài học gần đây
    await expect(teacherPage.getByRole('heading', { name: /Bài học cập nhật gần đây/i }).first()).toBeVisible();
  });

  test('TC-TF-051: Quick action button navigate sang tab đúng', async ({ teacherPage }) => {
    await teacherPage.goto('/admin/content?tab=overview');

    // Bấm nút Mở Studio biên soạn trong card Giáo trình
    const openStudioBtn = teacherPage.getByRole('button', { name: /Mở Studio biên soạn/i }).first();
    await expect(openStudioBtn).toBeVisible();
    await openStudioBtn.click();

    // URL cập nhật thành ?tab=curriculum
    await expect(teacherPage).toHaveURL(/tab=curriculum/);
    await expect(teacherPage.getByTestId('tab-curriculum')).toHaveAttribute('aria-selected', 'true');
  });
});
