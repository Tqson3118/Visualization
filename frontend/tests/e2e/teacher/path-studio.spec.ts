/**
 * E2E — Teacher Studio (cây lộ trình) + Lớp học (mô hình hợp nhất D2/D7).
 *
 * Phủ happy-path theo plan Phase 2/3 DoD:
 *  1. GV: chọn lộ trình → thêm Folder + Theory + Quiz + Lab → kéo-thả chuyển mục
 *     sang module khác → mở item, sửa tiêu đề, LƯU ngay trong panel phải (không rời màn).
 *  2. GV: đóng panel khi còn thay đổi chưa lưu → hiện xác nhận (dirty guard).
 *  3. Học viên: lớp hiển thị module list từ cây mới + nút "Tiếp tục học" trỏ đúng
 *     mục PAGE chưa hoàn thành đầu tiên (theo thứ tự cây).
 *  4. GV: đặt deadline cho 1 item (PUT /classes/{id}/assignments/deadline) + đổi lộ trình
 *     (PUT /classes/{id}/learning-path).
 *
 * Toàn bộ API được mock ở tầng network (không cần backend) — route đăng ký SAU khi
 * login (fixture) nên ghi đè handler trùng pattern trong helpers/mockApi.ts.
 */
import { expect, type Page } from '@playwright/test';

import { test } from '../fixtures/auth';
import { loginViaUi } from '../helpers/auth';

type Capture = { method: string; url: string; body: Record<string, unknown> };

function captureStore(): { list: Capture[] } {
  return { list: [] };
}

function findCapture(store: { list: Capture[] }, method: string, re: RegExp): Capture | undefined {
  return store.list.filter((c) => c.method === method && re.test(c.url)).pop();
}

/** Cây theo DoD Phase 2: 2 module × (2 theory + 1 quiz + 1 lab). */
function treeFixture(): Array<Record<string, unknown>> {
  return [
    {
      id: 10, pathId: 1, parentId: null, itemType: 'folder', title: 'MODULE 1 — Mảng',
      description: 'Kiến thức nền', sortOrder: 1,
      children: [
        { id: 11, pathId: 1, parentId: 10, itemType: 'theory', title: 'Mảng cơ bản', lessonId: 101, sortOrder: 1, children: [] },
        { id: 12, pathId: 1, parentId: 10, itemType: 'theory', title: 'Hai con trỏ', lessonId: 102, sortOrder: 2, children: [] },
        { id: 13, pathId: 1, parentId: 10, itemType: 'quiz', title: 'Mini-quiz Mảng', finalTestId: 201, sortOrder: 3, children: [] },
      ],
    },
    {
      id: 20, pathId: 1, parentId: null, itemType: 'folder', title: 'MODULE 2 — Xâu',
      description: null, sortOrder: 2,
      children: [
        { id: 21, pathId: 1, parentId: 20, itemType: 'lab', title: 'Lab: Đảo xâu', labExerciseId: 301, sortOrder: 1, children: [] },
      ],
    },
  ];
}

function curriculumFixture(): Record<string, unknown> {
  return {
    classId: 7,
    learningPathId: 1,
    learningPathTitle: 'Lộ trình Cấu trúc dữ liệu',
    title: 'Lộ trình Cấu trúc dữ liệu',
    description: 'Mảng → Xâu → Quiz/Lab',
    published: true,
    progressPct: 25,
    items: [
      {
        pathItemId: 10, itemType: 'folder', title: 'MODULE 1 — Mảng', sortOrder: 1, dueAt: null, status: 'in_progress', bestScore: null,
        children: [
          { pathItemId: 11, lessonId: 101, itemType: 'theory', title: 'Mảng cơ bản', parentId: 10, sortOrder: 1, dueAt: null, status: 'completed', bestScore: null },
          { pathItemId: 12, lessonId: 102, itemType: 'theory', title: 'Hai con trỏ', parentId: 10, sortOrder: 2, dueAt: null, status: 'not_started', bestScore: null },
          { pathItemId: 13, itemType: 'quiz', title: 'Mini-quiz Mảng', parentId: 10, sortOrder: 3, dueAt: null, status: 'not_started', bestScore: null },
        ],
      },
      {
        pathItemId: 20, itemType: 'folder', title: 'MODULE 2 — Xâu', sortOrder: 2, dueAt: null, status: 'not_started', bestScore: null,
        children: [
          { pathItemId: 21, itemType: 'lab', title: 'Lab: Đảo xâu', parentId: 20, sortOrder: 1, dueAt: null, status: 'not_started', bestScore: null },
        ],
      },
    ],
  };
}

function classDetailFixture(role: 'TEACHER' | 'STUDENT'): Record<string, unknown> {
  return {
    id: 7,
    name: 'Lớp DSA 01',
    description: 'Nhóm 2 — CTDL & Giải thuật',
    inviteCode: 'ABC123',
    ownerId: 1,
    memberCount: 3,
    createdAt: '2026-08-01T00:00:00.000Z',
    role,
    learningPathId: 1,
    learningPathTitle: 'Lộ trình Cấu trúc dữ liệu',
    curriculumPublished: true,
  };
}

let nextItemId = 100;

async function installStudioApi(page: Page, store: { list: Capture[] }): Promise<void> {
  // Cây mutable — PUT đổi title để GET sau save trả dữ liệu mới (giống backend thật)
  const mockTree: Array<Record<string, unknown>> = treeFixture();
  const setTitle = (id: number, title: string): void => {
    const walk = (list: Array<Record<string, unknown>>): void => {
      for (const it of list) {
        if (it.id === id) it.title = title;
        if (Array.isArray(it.children)) walk(it.children as Array<Record<string, unknown>>);
      }
    };
    walk(mockTree);
  };
  // Thứ tự đăng ký: route đăng ký SAU được ưu tiên. Route đặc thù (/move) đăng ký sau route tổng.
  await page.route('**/api/v1/items/*', async (route) => {
    const method = route.request().method();
    const url = route.request().url();
    const body = (route.request().postDataJSON() as Record<string, unknown>) ?? {};
    if (method === 'PUT') {
      store.list.push({ method, url, body });
      if (typeof body.title === 'string') setTitle(11, body.title);
      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ id: 11, pathId: 1, itemType: 'theory', title: body.title ?? 'Mảng cơ bản', sortOrder: 1, children: [] }) });
      return;
    }
    if (method === 'DELETE') {
      store.list.push({ method, url, body });
      await route.fulfill({ status: 200, contentType: 'application/json', body: '{}' });
      return;
    }
    await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ id: 11, pathId: 1, itemType: 'theory', title: 'Mảng cơ bản', lessonId: 101, sortOrder: 1, children: [] }) });
  });
  await page.route('**/api/v1/items/*/move', async (route) => {
    const method = route.request().method();
    const url = route.request().url();
    const body = (route.request().postDataJSON() as Record<string, unknown>) ?? {};
    store.list.push({ method, url, body });
    await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ id: 11, pathId: 1, parentId: body.parentId ?? null, itemType: 'theory', title: 'Mảng cơ bản', sortOrder: body.sortOrder ?? 1, children: [] }) });
  });
  await page.route('**/api/v1/paths/1/items', async (route) => {
    const method = route.request().method();
    const url = route.request().url();
    if (method === 'GET') {
      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(mockTree) });
      return;
    }
    if (method === 'POST') {
      const body = (route.request().postDataJSON() as Record<string, unknown>) ?? {};
      store.list.push({ method, url, body });
      const id = nextItemId++;
      const type = String(body.itemType ?? 'folder');
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id, pathId: 1, parentId: body.parentId ?? null, itemType: type, title: body.title ?? 'Mục mới',
          sortOrder: 99, children: [],
        }),
      });
      return;
    }
    await route.fulfill({ status: 405, contentType: 'application/json', body: '{}' });
  });
}

async function installClassApi(page: Page, role: 'TEACHER' | 'STUDENT', store: { list: Capture[] }): Promise<void> {
  await page.route('**/api/v1/classes/7', async (route) => {
    if (route.request().method() !== 'GET') {
      await route.continue();
      return;
    }
    await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(classDetailFixture(role)) });
  });
  await page.route('**/api/v1/classes/7/curriculum', async (route) => {
    await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(curriculumFixture()) });
  });
  await page.route('**/api/v1/classes/7/assignments/deadline', async (route) => {
    const method = route.request().method();
    const url = route.request().url();
    const body = (route.request().postDataJSON() as Record<string, unknown>) ?? {};
    if (method === 'PUT') {
      store.list.push({ method, url, body });
      await route.fulfill({ status: 200, contentType: 'application/json', body: '{}' });
      return;
    }
    await route.continue();
  });
  await page.route('**/api/v1/classes/7/learning-path', async (route) => {
    const method = route.request().method();
    const url = route.request().url();
    const body = (route.request().postDataJSON() as Record<string, unknown>) ?? {};
    if (method === 'PUT') {
      store.list.push({ method, url, body });
      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(classDetailFixture('TEACHER')) });
      return;
    }
    await route.continue();
  });
}

test.describe('Teacher Studio — cây lộ trình (mô hình hợp nhất)', () => {
  test('GV: thêm folder + 3 loại item → kéo-thả sang module khác → save trong panel phải', async ({ teacherPage: page }) => {
    const store = captureStore();
    await installStudioApi(page, store);

    await page.goto('/studio?tab=curriculum');
    await expect(page.getByTestId('path-select')).toBeVisible();
    await page.getByTestId('path-select').selectOption('1');
    await expect(page.getByTestId('outline-tree')).toBeVisible();
    await expect(page.getByTestId('outline-node-10')).toBeVisible();

    // 1) Thêm Folder vào gốc
    await page.getByTestId('add-item-trigger').click();
    const postFolder = page.waitForResponse((r) => r.request().method() === 'POST' && /\/api\/v1\/paths\/1\/items$/.test(r.url()));
    await page.getByTestId('add-item-folder').click();
    const folderBody = (await (await postFolder).request().postDataJSON()) as Record<string, unknown>;
    expect(folderBody.itemType).toBe('folder');

    // 2) Thêm Theory
    await expect(page.getByTestId('outline-node-10')).toBeVisible();
    await page.getByTestId('add-item-trigger').click();
    const postTheory = page.waitForResponse((r) => r.request().method() === 'POST' && /\/api\/v1\/paths\/1\/items$/.test(r.url()));
    await page.getByTestId('add-item-theory').click();
    const theoryBody = (await (await postTheory).request().postDataJSON()) as Record<string, unknown>;
    expect(theoryBody.itemType).toBe('theory');

    // 3) Thêm Quiz
    await page.getByTestId('add-item-trigger').click();
    const postQuiz = page.waitForResponse((r) => r.request().method() === 'POST' && /\/api\/v1\/paths\/1\/items$/.test(r.url()));
    await page.getByTestId('add-item-quiz').click();
    const quizBody = (await (await postQuiz).request().postDataJSON()) as Record<string, unknown>;
    expect(quizBody.itemType).toBe('quiz');

    // 4) Thêm Lab
    await page.getByTestId('add-item-trigger').click();
    const postLab = page.waitForResponse((r) => r.request().method() === 'POST' && /\/api\/v1\/paths\/1\/items$/.test(r.url()));
    await page.getByTestId('add-item-lab').click();
    const labBody = (await (await postLab).request().postDataJSON()) as Record<string, unknown>;
    expect(labBody.itemType).toBe('lab');

    // 6) Mở item → sửa tiêu đề → Lưu ngay trong panel (không rời màn hình)
    await page.getByTestId('outline-node-11').click();
    await expect(page.getByTestId('item-editor-slideover')).toBeVisible();
    const urlBeforeSave = page.url();
    await page.locator('[data-testid="item-editor-slideover"] input[type="text"]').first().fill('Mảng cơ bản (đã sửa)');
    const putItem = page.waitForResponse((r) => r.request().method() === 'PUT' && /\/api\/v1\/items\/\d+$/.test(r.url()));
    await page.getByTestId('item-editor-save').click();
    const putBody = (await (await putItem).request().postDataJSON()) as Record<string, unknown>;
    expect(putBody.title).toBe('Mảng cơ bản (đã sửa)');
    await expect(page.getByTestId('item-editor-slideover')).toBeVisible();
    expect(page.url()).toBe(urlBeforeSave);

    // Đóng panel trước khi kéo — cây ở chế độ readonly khi panel mở
    await page.getByTestId('item-editor-close').click();
    const dirtyAccept = page.getByTestId('dirty-confirm-accept');
    if (await dirtyAccept.isVisible().catch(() => false)) {
      await dirtyAccept.click();
    }
    await expect(page.getByTestId('item-editor-slideover')).toBeHidden();

    // 7) Kéo-thả: chuyển 11 sang MODULE 2 (20)
    const urlOfMove = page.waitForResponse((r) => r.request().method() === 'POST' && /\/api\/v1\/items\/11\/move$/.test(r.url()));
    const targetRow = page.getByTestId('outline-node-20');
    const box = await targetRow.boundingBox();
    await page.getByTestId('drag-handle-11').dispatchEvent('dragstart');
    await targetRow.dispatchEvent('dragover', { clientY: box ? box.y + box.height / 2 : 0 });
    await targetRow.dispatchEvent('drop');
    await page.getByTestId('drag-handle-11').dispatchEvent('dragend');
    const moveBody = (await (await urlOfMove).request().postDataJSON()) as Record<string, unknown>;
    expect(moveBody.parentId).toBe(20);
  });

  test('GV: đóng panel khi còn thay đổi chưa lưu → hiện xác nhận dirty', async ({ teacherPage: page }) => {
    await installStudioApi(page, captureStore());
    await page.goto('/studio?tab=curriculum');
    await page.getByTestId('path-select').selectOption('1');
    await expect(page.getByTestId('outline-node-11')).toBeVisible();

    await page.getByTestId('outline-node-11').click();
    await expect(page.getByTestId('item-editor-slideover')).toBeVisible();
    await page.locator('[data-testid="item-editor-slideover"] input[type="text"]').first().fill('Thay đổi chưa lưu');

    await page.getByTestId('item-editor-close').click();
    await expect(page.getByTestId('dirty-confirm-accept')).toBeVisible();
    await page.getByTestId('dirty-confirm-accept').click();
    await expect(page.getByTestId('item-editor-slideover')).toBeHidden();
  });

  test('Học viên: module list theo cây + "Tiếp tục học" trỏ đúng mục chưa hoàn thành đầu tiên', async ({ page, context }) => {
    const store = captureStore();
    await context.addCookies([{ name: 'dsa.session', value: '1', domain: 'localhost', path: '/' }]);
    await loginViaUi(page, 'student@test.edu', 'Student@123', { role: 'STUDENT' });
    // Đăng ký route SAU mockApi (login) để ghi đè handler trùng pattern
    await installClassApi(page, 'STUDENT', store);

    await page.goto('/classes/7?tab=curriculum');
    // Fallback: nếu query tab chưa mở tab Lộ trình thì click tab
    const continueBtn = page.getByTestId('continue-learning');
    if (!(await continueBtn.isVisible().catch(() => false))) {
      await page.getByRole('tab', { name: /Lộ trình/i }).first().click().catch(() => undefined);
    }
    await expect(page.getByText('MODULE 1 — Mảng')).toBeVisible();
    await expect(page.getByText('MODULE 2 — Xâu')).toBeVisible();
    // nextUp = theory 12 (11 đã completed) → lessonId 102 → /lessons/102
    await expect(continueBtn).toBeVisible();
    await continueBtn.click();
    await page.waitForURL(/\/lessons\/102/, { timeout: 10_000 });
  });

  test('GV: đặt deadline cho item + đổi lộ trình cho lớp', async ({ teacherPage: page }) => {
    const store = captureStore();
    await installClassApi(page, 'TEACHER', store);

    await page.goto('/classes/7?tab=curriculum');
    const continueBtn = page.getByTestId('continue-learning');
    if (!(await continueBtn.isVisible().catch(() => false))) {
      await page.getByRole('tab', { name: /Lộ trình/i }).first().click().catch(() => undefined);
    }

    // Deadline cho item 11
    const putDeadline = page.waitForResponse((r) => r.request().method() === 'PUT' && /\/assignments\/deadline$/.test(r.url()));
    await page.getByTestId('deadline-edit-11').click();
    await page.locator('input[type="datetime-local"]').first().fill('2026-09-15T23:59');
    await page.locator('#allow-late-toggle').check();
    await page.getByTestId('deadline-save').click();
    const deadlineBody = (await (await putDeadline).request().postDataJSON()) as Record<string, unknown>;
    expect(deadlineBody.pathItemId).toBe(11);
    expect(deadlineBody.allowLateSubmission).toBe(true);

    // Đổi lộ trình lớp → mở modal chọn lộ trình trước, rồi PUT /classes/7/learning-path
    await page.getByRole('button', { name: /Đổi Lộ trình khác|Chọn Lộ trình giảng dạy/ }).click();
    await expect(page.getByTestId('learning-path-select')).toBeVisible();
    // Options là radio input (value = path id), không phải <select>
    await page.locator('[data-testid="learning-path-select"] input[type="radio"][value="2"]').check();
    const putPath = page.waitForResponse((r) => r.request().method() === 'PUT' && /\/learning-path$/.test(r.url()));
    await page.getByTestId('learning-path-confirm').click();
    const pathBody = (await (await putPath).request().postDataJSON()) as Record<string, unknown>;
    expect(pathBody.learningPathId).toBe(2);
    expect(findCapture(store, 'PUT', /\/learning-path$/)).toBeTruthy();
  });
});
