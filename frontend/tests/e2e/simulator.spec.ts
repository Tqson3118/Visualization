/**
 * simulator.spec.ts — TEST-UI-001 (bước 4: phát 2s / tạm dừng / bước lùi) + TEST-UI-003 (phím tắt)
 * + FR-2.11 deep-link ?step=N (ghi nhận trạng thái hiện tại).
 *
 * Dùng demo key sort.bubble (demoAllowed=true — FR-7.6): KHÔNG cần đăng nhập.
 * D6b: /simulator/sort.* dùng SharedVisualizerShell → điều khiển qua VcrDockBar:
 *   - Chỉ số bước: input range .vcr-scrubber (value 0-based).
 *   - Nút: "Bước trước" / "Bước tiếp theo" / "Phát" (play) / "Tạm dừng" (pause).
 *
 * ⚠ FR-2.11: SimulatorView CHƯA đọc route.query.step → deep-link ?step=N chưa nhảy bước.
 *   Spec ghi nhận hành vi hiện tại (bước vẫn 0, query giữ nguyên).
 */
import { expect, test } from '@playwright/test';

import { mockApi } from './helpers/mockApi';

test.describe('Simulator — Màn 05 (demo công khai sort.bubble)', () => {
  test('play / pause / step (TEST-UI-001 bước 4)', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');

    await expect(page.getByRole('heading', { level: 1 })).toContainText('Sắp xếp nổi bọt');
    const scrubber = page.locator('.vcr-scrubber');
    await expect(scrubber).toHaveValue('0');

    // Bước tiếp theo 0 → 1
    await page.getByRole('button', { name: 'Bước tiếp theo', exact: true }).click({ force: true });
    await expect(scrubber).toHaveValue('1');

    // Bước lùi 1 → 0
    await page.getByRole('button', { name: 'Bước trước', exact: true }).click({ force: true });
    await expect(scrubber).toHaveValue('0');

    // Play → nút chuyển "Tạm dừng"; đợi ≥1 nhịp → index > 0
    await page.getByRole('button', { name: 'Phát', exact: true }).click({ force: true });
    await expect(page.getByRole('button', { name: 'Tạm dừng', exact: true })).toBeVisible();
    await page.waitForTimeout(1_500);
    const value = await scrubber.inputValue();
    expect(Number(value)).toBeGreaterThan(0);

    // Pause → nút quay lại "Phát"
    await page.getByRole('button', { name: 'Tạm dừng', exact: true }).click({ force: true });
    await expect(page.getByRole('button', { name: 'Phát', exact: true })).toBeVisible();
  });

  test('phím tắt Space / → / ← (TEST-UI-003)', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');

    const scrubber = page.locator('.vcr-scrubber');
    await expect(scrubber).toHaveValue('0');

    // Focus ra khỏi nút (tránh Space kích hoạt nút đang focus)
    await page.locator('.simulator__title').click();

    // → : bước tới; ← : bước lùi (keydown của SimulatorView điều khiển vcrStore)
    await page.keyboard.press('ArrowRight');
    await expect(scrubber).toHaveValue('1');
    await page.keyboard.press('ArrowLeft');
    await expect(scrubber).toHaveValue('0');

    // Space: play ↔ pause
    await page.keyboard.press('Space');
    await expect(page.getByRole('button', { name: 'Tạm dừng', exact: true })).toBeVisible();
    await page.keyboard.press('Space');
    await expect(page.getByRole('button', { name: 'Phát', exact: true })).toBeVisible();
  });

  test('deep-link ?step=N (FR-2.11) — ghi nhận trạng thái hiện tại', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble?step=12');

    // ⚠ FR-2.11 CHƯA triển khai → bước giữ 0, query giữ nguyên.
    await expect(page.locator('.vcr-scrubber')).toHaveValue('0');
    await expect(page).toHaveURL(/\?step=12/);
  });
});
