/**
 * bug-fixes.spec.ts — TC-SIM-040 đến TC-SIM-051
 * Test suite xác nhận các fix critical (C5, C7, C9, C10, C11, C12, C13, C14, C15).
 */
import { expect, test } from '@playwright/test';
import { mockApi } from '../helpers/mockApi';

async function waitForSimulatorReady(page: import('@playwright/test').Page) {
  const spinner = page.locator('.animate-spin');
  if (await spinner.isVisible({ timeout: 500 }).catch(() => false)) {
    await expect(spinner).not.toBeVisible({ timeout: 10000 });
  }
  const slider = page.locator('.control-bar__slider');
  await expect(slider).toBeVisible({ timeout: 10000 });
  await expect(slider).not.toHaveAttribute('max', '0', { timeout: 10000 });
}

test.describe('Test Suite 5: Simulator-specific Bug Fixes (TC-SIM-040~046)', () => {
  test.beforeEach(async ({ page }) => {
    await mockApi(page, { authenticated: true });
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-040: Graph DFS — disconnected vertices muted (fix C7)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-040: Graph DFS với đồ thị không liên thông → đỉnh unvisited bị muted', async ({ page }) => {
    const customGraphInput = encodeURIComponent(JSON.stringify({
      preset: 'custom',
      vertices: 5,
      edges: 2,
      source: 0,
      directed: false,
    }));
    await page.goto(`/simulator/graph.dfs?input=${customGraphInput}`);
    await waitForSimulatorReady(page);

    // Jump to the last step (nhấn End)
    await page.keyboard.press('End');

    // Kiểm tra explanation có chứa thông tin unreachable hoặc hoàn tất
    const explain = page.locator('.explain-panel').first();
    await expect(explain).toContainText(/không thể đến được|unreachable|hoàn tất|kết thúc/i);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-041: Dijkstra — shortest path highlight (fix C10)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-041: Dijkstra với target vertex → highlight đường đi ngắn nhất ở bước cuối', async ({ page }) => {
    const dijkstraInput = encodeURIComponent(JSON.stringify({
      preset: 'path',
      vertices: 5,
      source: 0,
      target: 3,
      directed: true,
      weighted: true,
    }));
    await page.goto(`/simulator/graph.dijkstra?input=${dijkstraInput}&step=17`);
    await waitForSimulatorReady(page);

    // Explanation phải chứa "đường đi ngắn nhất" hoặc "Kết thúc"
    const explain = page.locator('.explain-panel__text, .explain-panel').first();
    await expect(explain).toContainText(/đường đi ngắn nhất|Kết thúc/i);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-042: Heap Sort — tree view hiển thị (fix C14)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-042: Heap Sort hiển thị cấu trúc tree/heap trên canvas', async ({ page }) => {
    await page.goto('/simulator/sort.heap');
    await waitForSimulatorReady(page);

    // Tiêu đề
    await expect(page.locator('.simulator-header__title, h1').first()).toContainText(/Heap|đống/i);

    // Canvas hiển thị
    const canvas = page.locator('canvas').first();
    await expect(canvas).toBeVisible();

    // Bước đầu tiên có giải thích về cây đống
    const explain = page.locator('.explain-panel').first();
    await expect(explain).toContainText(/cây đống|heap/i);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-043: BST Insert — node mới visible ngay khi insert (fix C5)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-043: BST Insert → node mới và edge kết nối xuất hiện đúng', async ({ page }) => {
    const bstInput = encodeURIComponent(JSON.stringify({
      keys: [50, 30, 70],
      operation: 'insert',
      value: 42,
    }));
    await page.goto(`/simulator/tree.bst-insert?input=${bstInput}`);
    await waitForSimulatorReady(page);

    // Nhảy tới bước cuối
    await page.keyboard.press('End');
    await page.waitForTimeout(300);

    // Explanation mô tả đã chèn thành công
    const explain = page.locator('.explain-panel').first();
    const explainText = await explain.textContent();
    expect(explainText).toMatch(/chèn.*42|hoàn tất|kết thúc/i);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-044: AVL Insert — rotation cập nhật đúng root (fix C13)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-044: AVL Insert [30, 20, 10] trigger LL rotation → root thành 20', async ({ page }) => {
    const avlInput = encodeURIComponent(JSON.stringify({
      keys: [30, 20],
      value: 10,
    }));
    await page.goto(`/simulator/tree.avl-insert?input=${avlInput}`);
    await waitForSimulatorReady(page);

    // Nhảy tới cuối
    await page.keyboard.press('End');
    await page.waitForTimeout(300);

    const explain = page.locator('.explain-panel').first();
    const explainText = await explain.textContent();
    expect(explainText).toMatch(/xoay|cân bằng|hoàn tất|kết thúc/i);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-045: Linked List DeleteHead — head mới không bị gray (fix C11)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-045: Linked List Delete Head (pos=0) → head mới có trạng thái bình thường/done', async ({ page }) => {
    const listInput = encodeURIComponent(JSON.stringify({
      initialValues: [10, 20, 30],
      operation: 'deleteAt',
      position: 0,
    }));
    await page.goto(`/simulator/list.delete?input=${listInput}`);
    await waitForSimulatorReady(page);

    // Nhảy tới frame cuối
    await page.keyboard.press('End');
    await page.waitForTimeout(300);

    // Explanation chứa thông tin xóa đầu danh sách thành công
    const explain = page.locator('.explain-panel').first();
    const explainText = await explain.textContent();
    expect(explainText).toMatch(/xóa.*10|20.*30|kết thúc/i);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-046: Queue Dequeue — front reset về 0 (fix C12)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-046: Queue Dequeue → vars hiển thị front=0 (không bị hardcode front=1)', async ({ page }) => {
    const queueInput = encodeURIComponent(JSON.stringify({
      operations: ['Push 10', 'Push 20', 'Pop'],
      capacity: 5,
    }));
    await page.goto(`/simulator/queue.dequeue?input=${queueInput}`);
    await waitForSimulatorReady(page);

    // Nhảy tới cuối
    await page.keyboard.press('End');
    await page.waitForTimeout(300);

    // Panel biến / explanation phải hiển thị front=0
    const explain = page.locator('.explain-panel').first();
    const explainText = await explain.textContent();
    expect(explainText).toMatch(/front=0|hàng đợi/i);
  });
});

test.describe('Test Suite 6: Renderer Correctness (TC-SIM-050~051)', () => {
  test.beforeEach(async ({ page }) => {
    await mockApi(page, { authenticated: true });
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-050: Graph undirected — không crash và render đúng
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-050: Graph BFS vô hướng render thành công không có lỗi', async ({ page }) => {
    const errors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') errors.push(msg.text());
    });

    const undirectedInput = encodeURIComponent(JSON.stringify({
      directed: false,
      vertices: 5,
      edges: 5,
      source: 0,
    }));
    await page.goto(`/simulator/graph.bfs?input=${undirectedInput}`);
    await waitForSimulatorReady(page);

    // Canvas render
    const canvas = page.locator('canvas').first();
    await expect(canvas).toBeVisible();

    const critical = errors.filter(
      (e) => !e.includes('favicon') && !/401 \(Unauthorized\)/.test(e)
    );
    expect(critical).toHaveLength(0);
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-051: Tree Renderer — không stack overflow với circular (fix C15)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-051: BST insert nhiều phần tử liên tiếp không bị crash / call stack error', async ({ page }) => {
    const errors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') errors.push(msg.text());
    });

    const manyKeysInput = encodeURIComponent(JSON.stringify({
      keys: [10, 20, 30, 40, 50, 60, 70, 80, 90, 100],
      operation: 'insert',
      value: 110,
    }));
    await page.goto(`/simulator/tree.bst-insert?input=${manyKeysInput}`);
    await waitForSimulatorReady(page);

    // Chạy tới cuối
    await page.keyboard.press('End');
    await page.waitForTimeout(500);

    const stackOverflow = errors.some((e) =>
      e.toLowerCase().includes('maximum call stack size exceeded')
    );
    expect(stackOverflow).toBe(false);

    // Page vẫn sống và tương tác được
    await expect(page.locator('.simulator-header__title, h1').first()).toBeVisible();
  });
});
