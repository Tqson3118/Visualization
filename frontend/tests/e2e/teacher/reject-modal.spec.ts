import { test, expect } from '../fixtures/auth';
import { assertNoWindowPrompt, waitForToast } from '../fixtures/helpers';

test.describe('Test Suite 5 — RejectReasonModal (thay thế window.prompt)', () => {
  test.beforeEach(async ({ adminPage }) => {
    await adminPage.goto('/admin/content?tab=curriculum');
    await expect(adminPage.locator('.curriculum-tree, .chapter-card, [data-testid="teacher-glossary-banner"]').first()).toBeVisible();
  });

  test('TC-TF-040: Approve lesson không gọi window.prompt', async ({ adminPage }) => {
    await assertNoWindowPrompt(adminPage, async () => {
      // Tìm bài học hoặc giáo trình chờ duyệt
      const approveBtn = adminPage.getByRole('button', { name: /Duyệt giáo trình|Duyệt bài/i }).first();
      if (await approveBtn.isVisible()) {
        await approveBtn.click();
        // Xác nhận trong confirm modal (nếu có)
        const confirmBtn = adminPage.getByRole('button', { name: /Phê duyệt|Duyệt/i }).last();
        if (await confirmBtn.isVisible()) {
          await confirmBtn.click();
        }
      }
    });
  });

  test('TC-TF-041: Reject mở RejectReasonModal (QUAN TRỌNG: KHÔNG dùng window.prompt)', async ({ adminPage }) => {
    await assertNoWindowPrompt(adminPage, async () => {
      // Bấm nút Từ chối trên Pending Course
      const rejectBtn = adminPage.getByRole('button', { name: /Từ chối/i }).first();
      await expect(rejectBtn).toBeVisible();
      await rejectBtn.click();

      // Modal xuất hiện (KHÔNG phải window.prompt())
      const modal = adminPage.getByTestId('reject-modal');
      await expect(modal).toBeVisible();
      await expect(modal).toContainText('Lý do từ chối');

      const textarea = adminPage.getByTestId('reject-reason-input');
      await expect(textarea).toBeVisible();
    });
  });

  test('TC-TF-042: Submit reject với lý do chi tiết', async ({ adminPage }) => {
    // Mở reject modal
    const rejectBtn = adminPage.getByRole('button', { name: /Từ chối/i }).first();
    await rejectBtn.click();

    const textarea = adminPage.getByTestId('reject-reason-input');
    await textarea.fill('Bài học chưa đủ chi tiết, cần bổ sung ví dụ minh họa');

    const confirmBtn = adminPage.getByTestId('btn-confirm-reject');
    await expect(confirmBtn).toBeEnabled();
    await confirmBtn.click();

    // Modal đóng
    await expect(adminPage.getByTestId('reject-modal')).not.toBeVisible();
  });

  test('TC-TF-043: Hủy reject — đóng modal mà không submit', async ({ adminPage }) => {
    const rejectBtn = adminPage.getByRole('button', { name: /Từ chối/i }).first();
    await rejectBtn.click();

    const modal = adminPage.getByTestId('reject-modal');
    await expect(modal).toBeVisible();

    const cancelBtn = adminPage.getByTestId('btn-cancel-reject');
    await cancelBtn.click();

    await expect(modal).not.toBeVisible();
  });

  test('TC-TF-044: Reject với lý do rỗng — nút submit bị disabled', async ({ adminPage }) => {
    const rejectBtn = adminPage.getByRole('button', { name: /Từ chối/i }).first();
    await rejectBtn.click();

    const textarea = adminPage.getByTestId('reject-reason-input');
    await textarea.fill('');

    const confirmBtn = adminPage.getByTestId('btn-confirm-reject');
    await expect(confirmBtn).toBeDisabled();

    // Đóng modal
    await adminPage.getByTestId('btn-cancel-reject').click();
  });

  test('TC-TF-045: Reject course (Giáo trình) dùng chung RejectReasonModal pattern', async ({ adminPage }) => {
    const rejectCourseBtn = adminPage.getByRole('button', { name: /Từ chối/i }).first();
    await expect(rejectCourseBtn).toBeVisible();
    await rejectCourseBtn.click();

    const modal = adminPage.getByTestId('reject-modal');
    await expect(modal).toBeVisible();
    await expect(modal).toContainText('Lý do từ chối');

    await adminPage.getByTestId('btn-cancel-reject').click();
  });

  test('TC-TF-046: Modal accessible — phím Escape đóng modal', async ({ adminPage }) => {
    const rejectBtn = adminPage.getByRole('button', { name: /Từ chối/i }).first();
    await rejectBtn.click();

    const modal = adminPage.getByTestId('reject-modal');
    await expect(modal).toBeVisible();

    // Nhấn Escape
    await adminPage.keyboard.press('Escape');
    await expect(modal).not.toBeVisible();
  });
});
