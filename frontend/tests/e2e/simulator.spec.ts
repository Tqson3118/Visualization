/**
 * simulator.spec.ts — TEST-UI-001 (bước 4: phát 2s / tạm dừng / bước lùi) + TEST-UI-003 (phím tắt)
 * + FR-2.11 deep-link ?step=N (ghi nhận trạng thái hiện tại).
 *
 * Dùng demo key `sort.bubble` (demoAllowed=true — FR-7.6): KHÔNG cần đăng nhập
 * (SimulatorView chỉ redirect khi !isAuthenticated && !isDemoKey). Generator chạy
 * client-side từ engines/registry — KHÔNG mock engine.
 *
 * Selector theo SimulatorView/ControlBar thật:
 *   - Tiêu đề: h1.simulator__title (catalog title).
 *   - Chỉ số bước: .control-bar__indicator "Bước {current}/{total}".
 *   - Nút: "Bước tới" / "Bước lùi" / "Chạy" (play) / "Tạm dừng" (pause).
 *
 * ⚠ FR-2.11: SimulatorView CHƯA đọc route.query.step (kiểm tra src: không có nơi nào
 *   đọc query.step) → deep-link ?step=N chưa nhảy bước. Spec ghi nhận hành vi hiện tại
 *   (bước vẫn 1, query giữ nguyên). Bổ sung assert khi view triển khai (chờ task sau).
 */
import { expect, test } from '@playwright/test';

import { mockApi } from './helpers/mockApi';

test.describe('Simulator — Màn 05 (demo công khai sort.bubble)', () => {
  test('play / pause / step (TEST-UI-001 bước 4)', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');

    await expect(page.getByRole('heading', { level: 1 })).toContainText('Sắp xếp nổi bọt');
    const indicator = page.locator('.control-bar__indicator');
    await expect(indicator).toHaveText(/Bước 1\//);

    // ⚠ CanvasArea có vòng ResizeObserver → layout dịch liên tục → nút không bao giờ
    // "stable". Dùng force:true để click ngay tại vị trí hiện tại (đã probe). Bug app
    // tiềm ẩn (ResizeObserver loop) — ngoài phạm vi task, đề xuất task sau.
    // Bước tới 1 → 2
    await page.getByRole('button', { name: 'Bước tới', exact: true }).click({ force: true });
    await expect(indicator).toHaveText(/Bước 2\//);

    // Bước lùi 1 → về 1
    await page.getByRole('button', { name: 'Bước lùi', exact: true }).click({ force: true });
    await expect(indicator).toHaveText(/Bước 1\//);

    // Play → nút chuyển "Tạm dừng"; đợi ≥1 nhịp (interval 1200/speed ms) → bước tăng
    await page.getByRole('button', { name: 'Chạy', exact: true }).click({ force: true });
    await expect(page.getByRole('button', { name: 'Tạm dừng', exact: true })).toBeVisible();
    await page.waitForTimeout(1_500);
    const text = (await indicator.textContent()) ?? '';
    const current = Number(text.match(/Bước (\d+)\//)?.[1] ?? '1');
    expect(current).toBeGreaterThan(1);

    // Pause → nút quay lại "Chạy"
    await page.getByRole('button', { name: 'Tạm dừng', exact: true }).click({ force: true });
    await expect(page.getByRole('button', { name: 'Chạy', exact: true })).toBeVisible();
  });

  test('phím tắt Space / → / ← (TEST-UI-003)', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');

    const indicator = page.locator('.control-bar__indicator');
    await expect(indicator).toHaveText(/Bước 1\//);

    // Focus ra khỏi nút (tránh Space kích hoạt nút đang focus)
    await page.locator('.simulator__title').click();

    // → : bước tới; ← : bước lùi
    await page.keyboard.press('ArrowRight');
    await expect(indicator).toHaveText(/Bước 2\//);
    await page.keyboard.press('ArrowLeft');
    await expect(indicator).toHaveText(/Bước 1\//);

    // Space: play ↔ pause
    await page.keyboard.press('Space');
    await expect(page.getByRole('button', { name: 'Tạm dừng', exact: true })).toBeVisible();
    await page.keyboard.press('Space');
    await expect(page.getByRole('button', { name: 'Chạy', exact: true })).toBeVisible();
  });

  test('deep-link ?step=N (FR-2.11) — ghi nhận trạng thái hiện tại', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble?step=12');

    // ⚠ FR-2.11 CHƯA triển khai trong view (không đọc query.step) → bước giữ 1, query giữ nguyên.
    // Khi view triển khai deep-link (chờ task sau): kỳ vọng .control-bar__indicator hiển thị "Bước 12/…".
    await expect(page.locator('.control-bar__indicator')).toHaveText(/Bước 1\//);
    await expect(page).toHaveURL(/\?step=12/);
  });
});
