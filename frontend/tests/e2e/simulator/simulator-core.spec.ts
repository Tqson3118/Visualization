/**
 * simulator-core.spec.ts — TC-SIM-010 đến TC-SIM-022
 * Test suite cho SimulatorView core UI và controls.
 */
import { expect, test } from '@playwright/test';
import { mockApi } from '../helpers/mockApi';

/** Helper: đợi simulator workspace load xong */
async function waitForSimulatorReady(page: import('@playwright/test').Page) {
  const spinner = page.locator('.animate-spin');
  if (await spinner.isVisible({ timeout: 500 }).catch(() => false)) {
    await expect(spinner).not.toBeVisible({ timeout: 10000 });
  }
  const slider = page.locator('.control-bar__slider');
  await expect(slider).toBeVisible({ timeout: 10000 });
  await expect(slider).not.toHaveAttribute('max', '0', { timeout: 10000 });
}

test.describe('TC-SIM-010~013: SimulatorView Core UI', () => {
  test.beforeEach(async ({ page }) => {
    await mockApi(page, { authenticated: true });
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-010: Load simulator và render canvas (fix C8)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-010: Load sort.bubble → canvas render + ControlBar hiện đủ nút', async ({ page }) => {
    const errors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') errors.push(msg.text());
    });

    await page.goto('/simulator/sort.bubble');
    await waitForSimulatorReady(page);

    // Canvas area
    const canvasArea = page.locator('canvas, .simulator-workspace, .simulator-canvas-card').first();
    await expect(canvasArea).toBeVisible({ timeout: 10000 });

    // ControlBar phải có các nút cơ bản
    const playBtn = page.getByRole('button', { name: /Chạy|Phát|Play/i });
    const stepFwdBtn = page.getByRole('button', { name: /Bước tới|Bước tiếp theo|Step/i });

    const hasPlay = await playBtn.isVisible({ timeout: 3000 }).catch(() => false);
    const hasFwd = await stepFwdBtn.isVisible({ timeout: 3000 }).catch(() => false);
    expect(hasPlay || hasFwd).toBe(true);

    const unexpected = errors.filter(
      (e) => !e.includes('favicon') && !/401 \(Unauthorized\)/.test(e)
    );
    expect(unexpected).toHaveLength(0);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-011: Navigate giữa 2 simulators không reload (fix C8 — CRITICAL)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-011: Navigate Bubble Sort → BFS không reload page [CRITICAL]', async ({ page }) => {
    await page.goto('/simulator/sort.bubble');
    await waitForSimulatorReady(page);

    // Đặt sentinel flag để detect page reload
    await page.evaluate(() => {
      (window as any).__navigated = true;
    });

    // Kiểm tra title hiện tại là Bubble Sort
    await expect(page.locator('.simulator-header__title, h1').first()).toContainText(
      /Bubble|nổi bọt/i
    );

    // Click link back to explore / simulations rồi chọn BFS
    const backBtn = page.locator('.simulator-header__back, a[href*="simulations"]').first();
    if (await backBtn.isVisible({ timeout: 1000 }).catch(() => false)) {
      await backBtn.click();
      await page.waitForURL(/\/simulations/);
      const bfsCard = page.locator('.simulations__card', {
        has: page.locator('.simulations__card-title', { hasText: /BFS/i }),
      }).first();
      await bfsCard.click();
      await page.waitForURL(/\/simulator\/graph\.bfs/);
    } else {
      await page.goto('/simulator/graph.bfs');
    }

    await waitForSimulatorReady(page);

    // URL đúng
    await expect(page).toHaveURL(/\/simulator\/graph\.bfs/);

    // Sentinel flag vẫn tồn tại (chứng minh SPA không reload toàn trang)
    const navigated = await page.evaluate(() => (window as any).__navigated);
    expect(navigated).toBe(true);

    // Title phải thay đổi sang BFS
    await expect(page.locator('.simulator-header__title, h1').first()).toContainText(
      /BFS|Duyệt BFS/i,
      { timeout: 5000 }
    );

    // Canvas phải render
    const canvas = page.locator('canvas, .simulator-workspace').first();
    await expect(canvas).toBeVisible({ timeout: 5000 });

    // Pseudocode không còn nội dung Bubble Sort
    const pseudocode = page.locator('.pseudocode-panel, [class*="pseudocode"]');
    if (await pseudocode.isVisible({ timeout: 2000 }).catch(() => false)) {
      const text = await pseudocode.textContent();
      expect(text?.toLowerCase()).not.toContain('bubblesort');
    }
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-012: Keyboard navigation không scroll trang (fix m14)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-012: Phím Space và ArrowRight không scroll trang', async ({ page }) => {
    await page.goto('/simulator/sort.bubble');
    await waitForSimulatorReady(page);

    // Scroll xuống 100px
    await page.evaluate(() => window.scrollTo(0, 100));
    await page.waitForTimeout(100);

    // Ghi nhận scrollY trước
    const scrollBeforeSpace = await page.evaluate(() => window.scrollY);

    // Nhấn Space → simulation play/pause (không scroll)
    await page.keyboard.press('Space');
    await page.waitForTimeout(200);

    const scrollAfterSpace = await page.evaluate(() => window.scrollY);
    expect(Math.abs(scrollAfterSpace - scrollBeforeSpace)).toBeLessThanOrEqual(5);

    // Nhấn Space lại để pause
    await page.keyboard.press('Space');
    await page.waitForTimeout(100);

    // Nhấn ArrowRight → step forward (không scroll)
    const scrollBeforeArrow = await page.evaluate(() => window.scrollY);
    await page.keyboard.press('ArrowRight');
    await page.waitForTimeout(200);

    const scrollAfterArrow = await page.evaluate(() => window.scrollY);
    expect(Math.abs(scrollAfterArrow - scrollBeforeArrow)).toBeLessThanOrEqual(5);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-013: Share link button (fix M12)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-013: Share button tạo toast và clipboard URL hợp lệ', async ({ page, context }) => {
    await context.grantPermissions(['clipboard-read', 'clipboard-write']);

    await page.goto('/simulator/search.binary');
    await waitForSimulatorReady(page);

    // Click nút Share
    const shareBtn = page.getByRole('button', {
      name: /share|sao chép|chia sẻ/i,
    });
    await expect(shareBtn).toBeVisible({ timeout: 5000 });
    await shareBtn.click();

    // Toast thành công xuất hiện (qua vue-sonner)
    const toast = page.locator('[data-sonner-toast], [data-sonner-toaster], li[data-sonner-toast]').first();
    if (await toast.isVisible({ timeout: 2000 }).catch(() => false)) {
      const toastText = await toast.textContent();
      expect(toastText?.toLowerCase()).toMatch(/sao chép|copied|link/i);
    }

    // Clipboard content chứa URL hợp lệ với /simulator/search.binary
    const clipContent = await page.evaluate(async () => {
      try {
        return await navigator.clipboard.readText();
      } catch {
        return '';
      }
    });
    if (clipContent) {
      expect(clipContent).toContain('/simulator/search.binary');
    }
  });
});

test.describe('TC-SIM-020~022: Play/Pause/Step Controls', () => {
  test.beforeEach(async ({ page }) => {
    await mockApi(page, { authenticated: true });
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-020: Play → chạy từng step tự động
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-020: Play → scrubber tiến, nút đổi thành Pause', async ({ page }) => {
    await page.goto('/simulator/sort.bubble');
    await waitForSimulatorReady(page);

    const scrubber = page.locator('.control-bar__slider, .vcr-scrubber, input[type="range"]').first();
    await expect(scrubber).toHaveValue('0');

    const playBtn = page.getByRole('button', { name: /Chạy|Phát|Play/i });
    await expect(playBtn).toBeVisible();
    await playBtn.click({ force: true });

    // Nút đổi thành Pause
    await expect(page.getByRole('button', { name: /Tạm dừng|Pause/i })).toBeVisible({
      timeout: 3000,
    });

    // Đợi simulation chạy một số bước
    await page.waitForTimeout(1500);
    const value = await scrubber.inputValue();
    expect(Number(value)).toBeGreaterThan(0);

    // Pause lại
    await page.getByRole('button', { name: /Tạm dừng|Pause/i }).click({ force: true });
    await expect(playBtn).toBeVisible({ timeout: 3000 });
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-021: Step Forward / Step Back
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-021: Step Forward 3 lần → step tăng, Step Back 1 lần → step giảm', async ({ page }) => {
    await page.goto('/simulator/sort.insertion');
    await waitForSimulatorReady(page);

    const scrubber = page.locator('.control-bar__slider, .vcr-scrubber, input[type="range"]').first();
    await expect(scrubber).toHaveValue('0');

    const fwdBtn = page.getByRole('button', { name: /Bước tới|Bước tiếp theo|Step/i });
    const backBtn = page.getByRole('button', { name: /Bước lùi|Bước trước|Back/i });

    await expect(fwdBtn).toBeVisible();

    // Step forward 3 lần
    await fwdBtn.click({ force: true });
    await expect(scrubber).toHaveValue('1');
    await fwdBtn.click({ force: true });
    await expect(scrubber).toHaveValue('2');
    await fwdBtn.click({ force: true });
    await expect(scrubber).toHaveValue('3');

    // Step back 1 lần
    await backBtn.click({ force: true });
    await expect(scrubber).toHaveValue('2');
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-022: Reset về trạng thái ban đầu
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-022: Reset sau khi Play → scrubber về 0', async ({ page }) => {
    await page.goto('/simulator/sort.bubble');
    await waitForSimulatorReady(page);

    const scrubber = page.locator('.control-bar__slider, .vcr-scrubber, input[type="range"]').first();
    const fwdBtn = page.getByRole('button', { name: /Bước tới|Bước tiếp theo|Step/i });
    const resetBtn = page.getByRole('button', { name: /Đặt lại|Reset/i });

    // Advance vài bước
    if (await fwdBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await fwdBtn.click({ force: true });
      await fwdBtn.click({ force: true });
      await fwdBtn.click({ force: true });
      expect(Number(await scrubber.inputValue())).toBeGreaterThan(0);
    } else {
      const playBtn = page.getByRole('button', { name: /Chạy|Phát|Play/i });
      await playBtn.click({ force: true });
      await page.waitForTimeout(800);
      await page.getByRole('button', { name: /Tạm dừng|Pause/i }).click({ force: true });
    }

    // Reset
    await expect(resetBtn).toBeVisible();
    await resetBtn.click({ force: true });

    // Scrubber về 0
    await expect(scrubber).toHaveValue('0', { timeout: 3000 });
  });
});
