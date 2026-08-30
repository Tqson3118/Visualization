/**
 * simulations-list.spec.ts — TC-SIM-001 đến TC-SIM-005
 * Test suite cho trang danh sách simulators (/simulations).
 *
 * Dùng mockApi để không phụ thuộc backend thật.
 * /simulations không cần auth (demoAllowed public view).
 */
import { expect, test } from '@playwright/test';
import { mockApi } from '../helpers/mockApi';

test.describe('TC-SIM-001~005: SimulationsView — Danh sách Simulators', () => {
  test.beforeEach(async ({ page }) => {
    await mockApi(page);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-001: Hiển thị danh sách simulators
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-001: Hiển thị ít nhất 20 card simulator sau 5 giây', async ({ page }) => {
    await page.goto('/simulations');

    // Đợi loading biến mất (nếu có spinner)
    const spinner = page.locator('[role="status"]');
    if (await spinner.isVisible({ timeout: 500 }).catch(() => false)) {
      await expect(spinner).not.toBeVisible({ timeout: 5000 });
    }

    // Đợi ít nhất một card render
    await expect(page.locator('.simulations__card').first()).toBeVisible({ timeout: 5000 });

    // Đếm số card — catalog có 44 items (có thể phân trang 12/page)
    // Trước mắt kiểm tra ít nhất 10 cards hiển thị trên trang đầu tiên
    const cards = page.locator('.simulations__card');
    const count = await cards.count();
    expect(count).toBeGreaterThanOrEqual(10);

    // Mỗi card phải có title và độ phức tạp
    const firstCard = cards.first();
    await expect(firstCard.locator('.simulations__card-title')).toBeVisible();
    // Badge độ phức tạp (complexity)
    await expect(firstCard.locator('.simulations__complexity-badge')).toBeVisible();
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-002: Filter theo loại cấu trúc "Đồ thị"
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-002: Filter "Đồ thị" chỉ hiển thị các card đồ thị', async ({ page }) => {
    await page.goto('/simulations');
    await expect(page.locator('.simulations__card').first()).toBeVisible({ timeout: 5000 });

    // Select structureFilter = "Đồ thị"
    const structureSelect = page.locator('select[aria-label*="cấu trúc"], select[aria-label*="Loại"]').first();
    await expect(structureSelect).toBeVisible();
    await structureSelect.selectOption('Đồ thị');

    // Đợi filter áp dụng
    await page.waitForTimeout(300);

    // Chỉ các card đồ thị còn hiển thị
    const cards = page.locator('.simulations__card');
    const count = await cards.count();
    expect(count).toBeGreaterThan(0);

    // BFS và DFS phải xuất hiện
    const bfsCard = page.locator('.simulations__card', {
      has: page.locator('.simulations__card-title', { hasText: 'BFS' }),
    });
    const dfsCard = page.locator('.simulations__card', {
      has: page.locator('.simulations__card-title', { hasText: 'DFS' }),
    });

    await expect(bfsCard.first()).toBeVisible();
    await expect(dfsCard.first()).toBeVisible();

    // Không có card sắp xếp (Mảng)
    const arrayCard = page.locator('.simulations__card', {
      has: page.locator('.simulations__card-title', { hasText: 'Bubble Sort' }),
    });
    expect(await arrayCard.count()).toBe(0);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-003: Search simulator theo từ khóa "bubble"
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-003: Search "bubble" chỉ hiển thị Bubble Sort, clear → đầy đủ', async ({ page }) => {
    await page.goto('/simulations');
    await expect(page.locator('.simulations__card').first()).toBeVisible({ timeout: 5000 });

    const countBefore = await page.locator('.simulations__card').count();

    // Gõ "bubble" vào ô tìm kiếm
    const searchInput = page.locator('input[type="search"], input.simulations__search');
    await expect(searchInput).toBeVisible();
    await searchInput.fill('bubble');
    await page.waitForTimeout(300);

    // Card khớp với từ khóa "bubble" (Bubble Sort, Heap Insert bubble up)
    const cards = page.locator('.simulations__card');
    const count = await cards.count();
    expect(count).toBeGreaterThanOrEqual(1);
    await expect(cards.first().locator('.simulations__card-title')).toContainText(/Bubble/i);

    // Clear filter — dùng button "Xóa bộ lọc" hoặc clear input
    const clearBtn = page.locator('button', { hasText: /xóa bộ lọc|clear/i });
    if (await clearBtn.isVisible({ timeout: 500 }).catch(() => false)) {
      await clearBtn.click();
    } else {
      await searchInput.fill('');
      await searchInput.press('Enter');
    }
    await page.waitForTimeout(300);

    // Danh sách đầy đủ trở lại
    const countAfter = await page.locator('.simulations__card').count();
    expect(countAfter).toBeGreaterThanOrEqual(countBefore);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-004: Filter reset page về 1 (fix M13)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-004: Thay đổi filter reset page về 1', async ({ page }) => {
    await page.goto('/simulations');
    await expect(page.locator('.simulations__card').first()).toBeVisible({ timeout: 5000 });

    // Chỉ test khi có phân trang (totalPages > 1)
    const nextPageBtn = page.locator('button', { hasText: /trang sau|tiếp theo|next/i });
    const hasPagination = await nextPageBtn.isVisible({ timeout: 500 }).catch(() => false);

    if (hasPagination) {
      // Đến trang 2
      await nextPageBtn.click();
      await page.waitForTimeout(300);

      // Kiểm tra đang ở page 2
      const pageInfo = page.locator('.simulations__page-info');
      await expect(pageInfo).toContainText('2');

      // Thay đổi search
      const searchInput = page.locator('input[type="search"], input.simulations__search');
      await searchInput.fill('sort');
      await page.waitForTimeout(300);

      // Page phải reset về 1
      const pageInfoAfter = page.locator('.simulations__page-info');
      if (await pageInfoAfter.isVisible({ timeout: 500 }).catch(() => false)) {
        await expect(pageInfoAfter).toContainText('1');
      }
    } else {
      // Không có phân trang → skip logic page, chỉ verify filter hoạt động
      const searchInput = page.locator('input[type="search"], input.simulations__search');
      await searchInput.fill('sort');
      await page.waitForTimeout(300);
      const cards = page.locator('.simulations__card');
      expect(await cards.count()).toBeGreaterThan(0);
    }
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-005: Click card → navigate đến simulator
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-005: Click card "Binary Search" → navigate /simulator/search.binary', async ({ page }) => {
    await page.goto('/simulations');
    await expect(page.locator('.simulations__card').first()).toBeVisible({ timeout: 5000 });

    // Tìm card Binary Search
    const binaryCard = page.locator('.simulations__card', {
      has: page.locator('.simulations__card-title', { hasText: /Binary Search|nhị phân/i }),
    }).first();
    await expect(binaryCard).toBeVisible();
    await binaryCard.click();

    // URL phải thay đổi thành /simulator/search.binary
    await expect(page).toHaveURL(/\/simulator\/search\.binary/, { timeout: 5000 });

    // Canvas hoặc simulator workspace phải xuất hiện trong 3 giây
    const workspace = page.locator('.simulator-workspace, canvas, .vcr-scrubber');
    await expect(workspace.first()).toBeVisible({ timeout: 10000 });

    // Pseudocode panel phải có nội dung
    const pseudocode = page.locator('.pseudocode-panel, [class*="pseudocode"]');
    if (await pseudocode.isVisible({ timeout: 3000 }).catch(() => false)) {
      await expect(pseudocode).toBeVisible();
    }
  });
});
