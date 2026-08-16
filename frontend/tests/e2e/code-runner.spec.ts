/**
 * code-runner.spec.ts — Màn 16 (Module I): guard, render editor (textarea), chạy code,
 * FR-9.6 giới hạn sandbox (ghi nhận trạng thái hiện tại).
 *
 * CodeRunnerView thật:
 *   - Editor là textarea (aria-label "Trình soạn mã {key}") — KHÔNG Monaco.
 *   - "Chạy" chạy sandbox CLIENT (engines/core/stepExecutor.runCode — ADR-012), sau đó
 *     lưu vết POST /code-runs (mock; lỗi mạng bị bỏ qua trong store) → status
 *     "Thành công · {durationMs}ms" (role=status) khi pass.
 *   - Template code mẫu (sort.bubble) nạp sẵn khi mở trang.
 *
 * ⚠ FR-9.6 (chặn > 200 dòng / 10s / 64MB với thông báo rõ): CodeRunnerView + codeRunner
 *   store CHƯA có kiểm tra giới hạn dòng → KHÔNG có thông báo chặn nào. Spec ghi nhận
 *   hành vi hiện tại (không có message "200 dòng"). Bổ sung assert khi view triển khai
 *   (chờ task sau) — KHÔNG tự thêm feature vào view.
 */
import { expect, test } from '@playwright/test';

import { E2E_EMAIL, E2E_PASSWORD } from './helpers/auth';
import { mockApi } from './helpers/mockApi';

test.describe('Code Runner — Màn 16 (sort.bubble)', () => {
  test('guard chưa đăng nhập → /login?redirect=... → sau login hiển thị editor với code mẫu', async ({ page }) => {
    await mockApi(page);
    await page.goto('/code/sort.bubble');

    // Guard requiresAuth → login kèm redirect
    await expect(page).toHaveURL(/\/login\?redirect=/);

    // Login ngay → quay lại /code/sort.bubble
    await page.locator('#email').fill(E2E_EMAIL);
    await page.locator('#password').fill(E2E_PASSWORD);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/code\/sort\.bubble$/);

    // CodeRunnerView: kicker "CODE CHALLENGE · key" là <p> — heading thật là tên bài (h1)
    await expect(page.getByRole('heading', { name: /Sắp xếp nổi bọt/ })).toBeVisible();

    // Editor = textarea, chứa code mẫu bubbleSort (loadTemplate từ store)
    const editor = page.getByRole('textbox', { name: 'Trình soạn mã sort.bubble' });
    await expect(editor).toBeVisible();
    await expect(editor).toHaveValue(/bubbleSort/);
  });

  test('chạy code mẫu → thông báo Thành công (sandbox client + mock /code-runs)', async ({ page }) => {
    await mockApi(page);
    await page.goto('/code/sort.bubble');
    await expect(page).toHaveURL(/\/login\?redirect=/);
    await page.locator('#email').fill(E2E_EMAIL);
    await page.locator('#password').fill(E2E_PASSWORD);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/code\/sort\.bubble$/);

    // Chạy code mẫu (đã nạp sẵn) → status success (sandbox client, không cần backend)
    // KHÔNG force: click thường chờ element stable; force click trong suite có thể rơi trượt
    // (canvas ResizeObserver/PixiJS làm layout dịch giữa chừng) → status không cập nhật.
    await page.getByRole('button', { name: /Chạy/ }).click();
    await expect(page.getByText(/Thành công · \d+ms/)).toBeVisible();
  });

  test('FR-9.6 giới hạn 200 dòng — ghi nhận trạng thái hiện tại (chưa có thông báo chặn)', async ({ page }) => {
    await mockApi(page);
    await page.goto('/code/sort.bubble');
    await expect(page).toHaveURL(/\/login\?redirect=/);
    await page.locator('#email').fill(E2E_EMAIL);
    await page.locator('#password').fill(E2E_PASSWORD);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/code\/sort\.bubble$/);

    const editor = page.getByRole('textbox', { name: 'Trình soạn mã sort.bubble' });

    // 203 dòng comment + 1 hàm hợp lệ → vượt giới hạn 200 dòng FR-9.6
    const longCode =
      Array.from({ length: 203 }, (_, i) => `// filler line ${i + 1}`).join('\n') +
      '\nfunction solve() { return; }\n';
    await editor.fill(longCode);
    await page.getByRole('button', { name: /Chạy/ }).click();

    // ⚠ UI hiện CHƯA chặn > 200 dòng → không xuất hiện thông báo nào chứa "200 dòng".
    // Khi view triển khai giới hạn (chờ task sau): kỳ vọng thông báo chặn rõ ràng hiển thị.
    await expect(page.getByText(/200 dòng/)).toHaveCount(0);
  });
});
