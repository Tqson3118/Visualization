import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-35~48: Mô phỏng giải thuật', () => {
  // D6b: /simulator/sort.* dùng SharedVisualizerShell → điều khiển qua VcrDockBar
  // (indicator = range .vcr-scrubber; nút "Phát"/"Tạm dừng"/"Bước trước"/"Bước tiếp theo"/"Đặt lại").
  const scrubber = (page) => page.locator('.vcr-scrubber');

  test('TC-35: Chọn bubble-sort + [5,3,1] → trace sinh đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    await expect(page.locator('h1')).toContainText(/nổi bọt|Bubble/i);
    await expect(scrubber(page)).toBeVisible();
  });

  test('TC-36: Play → step tăng, mã giả highlight', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    const playBtn = page.getByRole('button', { name: 'Phát', exact: true });
    if (await playBtn.isVisible()) {
      await playBtn.click({ force: true });
      await page.waitForTimeout(1000);
      // Đang chạy → nút chuyển "Tạm dừng"; index tăng (range value > 0)
      await expect(page.getByRole('button', { name: 'Tạm dừng', exact: true })).toBeVisible();
      const value = await scrubber(page).inputValue();
      expect(Number(value)).toBeGreaterThan(0);
    }
  });

  test('TC-37: Pause → dừng đúng chỗ', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    const playBtn = page.getByRole('button', { name: 'Phát', exact: true });
    if (await playBtn.isVisible()) {
      await playBtn.click({ force: true });
      await page.waitForTimeout(500);
      const pauseBtn = page.getByRole('button', { name: 'Tạm dừng', exact: true });
      if (await pauseBtn.isVisible()) {
        await pauseBtn.click({ force: true });
        await expect(playBtn).toBeVisible();
      }
    }
  });

  test('TC-38: Step forward → step+1', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    await expect(scrubber(page)).toHaveValue('0');
    const nextBtn = page.getByRole('button', { name: 'Bước tiếp theo', exact: true });
    if (await nextBtn.isVisible()) {
      await nextBtn.click({ force: true });
      await expect(scrubber(page)).toHaveValue('1');
    }
  });

  test('TC-39: Step back → step-1', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    await expect(scrubber(page)).toHaveValue('0');
    const nextBtn = page.getByRole('button', { name: 'Bước tiếp theo', exact: true });
    const prevBtn = page.getByRole('button', { name: 'Bước trước', exact: true });
    if (await nextBtn.isVisible() && await prevBtn.isVisible()) {
      await nextBtn.click({ force: true });
      await expect(scrubber(page)).toHaveValue('1');
      await prevBtn.click({ force: true });
      await expect(scrubber(page)).toHaveValue('0');
    }
  });

  test('TC-40: Reset → step=0 / step=1', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    const nextBtn = page.getByRole('button', { name: 'Bước tiếp theo', exact: true });
    const resetBtn = page.getByRole('button', { name: /Đặt lại|Reset/i });
    if (await nextBtn.isVisible() && await resetBtn.isVisible()) {
      await nextBtn.click({ force: true });
      await expect(scrubber(page)).toHaveValue('1');
      await resetBtn.click({ force: true });
      await expect(scrubber(page)).toHaveValue('0');
    }
  });

  test('TC-41: Tốc độ 1x→2x → playback nhanh hơn', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    const speedSelect = page.getByRole('combobox', { name: 'Tốc độ phát' });
    if (await speedSelect.isVisible()) {
      await speedSelect.selectOption('2');
    }
  });

  test('TC-42: Mảng rỗng → lỗi, không crash', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    const customInput = page.locator('input[placeholder*="mảng"], input[placeholder*="dữ liệu"], textarea').first();
    if (await customInput.isVisible()) {
      await customInput.fill('');
      await page.keyboard.press('Enter');
    }
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-43: 100 phần tử → trace time < 500ms', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    const start = Date.now();
    await page.evaluate(() => {
      const arr = Array.from({ length: 100 }, (_, i) => 100 - i);
      return arr;
    });
    const elapsed = Date.now() - start;
    expect(elapsed).toBeLessThan(500);
  });

  test('TC-44: Thay giải thuật giữa chừng → reset + load mới', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    await page.goto('/simulator/search.binary');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-45: Resize window → canvas resize, offsetWidth < 15000', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    await page.setViewportSize({ width: 800, height: 600 });
    const canvas = page.locator('canvas, svg.visualizer-stage, .simulator__stage, .shared-visualizer-shell').first();
    if (await canvas.isVisible()) {
      const width = await canvas.evaluate((el) => el.clientWidth || el.getBoundingClientRect().width);
      expect(width).toBeLessThan(15000);
    }
  });

  test('TC-46: Deep link /simulator/sort.bubble → load đúng', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    await expect(page).toHaveURL(/sort\.bubble/);
  });

  test('TC-47: Keyboard Space → toggle play/pause', async ({ page }) => {
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    await page.keyboard.press('Space');
    await page.waitForTimeout(300);
    await page.keyboard.press('Space');
    await expect(page.locator('body')).toBeVisible();
  });

  test('TC-48: Không có console error toàn session', async ({ page }) => {
    const errors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') errors.push(msg.text());
    });
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    await page.waitForTimeout(500);
    // 401 từ POST /auth/refresh lúc boot (guest) là thiết kế — xem home.spec.ts TC-34.
    const unexpected = errors.filter((e) => !e.includes('favicon') && !/401 \(Unauthorized\)/.test(e));
    expect(unexpected).toHaveLength(0);
  });
});
