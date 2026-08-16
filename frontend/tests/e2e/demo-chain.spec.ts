/**
 * demo-chain.spec.ts — DEMO_01_FULL_PRESENTATION_FLOW (Trục 14, ROUND 2)
 *
 * Chuỗi liên tục mô phỏng thuyết trình với BACKEND THẬT (:8081/:5000 — KHÔNG mock):
 *   showcase công khai → login student → path → lesson (NodeHub) → simulator →
 *   exercise (nộp bài) → premium → profile/leaderboard → logout (guard) →
 *   teacher (classes + report) → admin (users/settings).
 *
 * Bắt: state leak (logout → guard /login?redirect), stale state (role switch),
 * route transition (URL sau mỗi bước), modal đóng, console error, pageerror,
 * duplicate API request, response 4xx/5xx.
 *
 * Chạy: npx playwright test --config=playwright.demo.config.ts
 * Yêu cầu: BE :5000 + FE :8081 đang chạy + DB đã seed (reset-db.ps1).
 */
import { expect, test, type Page } from '@playwright/test';

const BASE = 'http://localhost:8081';
const ACC = {
  student: { email: 'student@demo.local', password: 'Student@123' },
  teacher: { email: 'teacher@demo.local', password: 'Teacher@123' },
  admin: { email: 'admin@system.local', password: 'Admin@123' },
};

/** Collector console error + pageerror + API request log cho 1 page. */
function watch(page: Page): { errors: string[]; apiCalls: string[]; httpErrors: string[] } {
  const errors: string[] = [];
  const apiCalls: string[] = [];
  const httpErrors: string[] = [];
  page.on('console', (msg) => {
    if (msg.type() === 'error') errors.push(msg.text());
  });
  page.on('pageerror', (err) => errors.push('pageerror: ' + err.message));
  page.on('request', (req) => {
    const u = req.url();
    if (u.includes('/api/v1')) apiCalls.push(u);
  });
  page.on('response', (res) => {
    const u = res.url();
    if (u.includes('/api/v1') && res.status() >= 400) {
      httpErrors.push(res.status() + ' ' + u + ' @page=' + page.url());
    }
  });
  return { errors, apiCalls, httpErrors };
}

async function login(page: Page, email: string, password: string): Promise<void> {
  await page.goto('/login');
  await expect(page.getByRole('heading', { name: 'Đăng nhập' })).toBeVisible();
  await page.locator('#email').fill(email);
  await page.locator('#password').fill(password);
  await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
  await expect(page).toHaveURL(/\/path$/, { timeout: 15_000 });
}

async function logout(page: Page, displayName: string): Promise<void> {
  // Avatar header mở menu → nút "Đăng xuất" (AppHeader aria-label = displayName)
  await page.locator(`button[aria-label="${displayName}"]`).click();
  await page.getByRole('button', { name: 'Đăng xuất', exact: true }).click();
  await expect(page).toHaveURL(/\/login$/);
}

test.setTimeout(240_000);
test.describe('DEMO_01_FULL_PRESENTATION_FLOW — backend thật', () => {
  test('chuỗi liên tục student → teacher → admin + showcase', async ({ page }) => {
    const { errors, apiCalls, httpErrors } = watch(page);

    // ── 1. SHOWCASE công khai (chưa login — route simulator không cần auth) ──
    await page.goto('/'); // warmup (vite cold compile sau restart)
    await page.waitForLoadState('domcontentloaded');
    await page.goto('/simulator/sort.bubble');
    await expect(page).toHaveURL(/\/simulator\/sort\.bubble$/);
    await expect(page.locator('main').last()).toBeVisible({ timeout: 45_000 });
    // Không bị redirect về /login (guard không áp dụng route công khai)
    expect(page.url()).not.toContain('/login');

    // ── 2. LOGIN student → /path ──
    await login(page, ACC.student.email, ACC.student.password);
    await expect(page.getByRole('link', { name: /Sắp xếp & Tìm kiếm/ })).toBeVisible();

    // ── 3. LEARNING PATH: topic 2 → node "Học: Stack" ──
    await page.goto('/path/2');
    await expect(page.getByRole('button', { name: /Học: Stack/ })).toBeVisible({ timeout: 15_000 });
    await page.getByRole('button', { name: /Học: Stack/ }).click();
    // Modal xác nhận mở
    await expect(page.getByRole('button', { name: 'Bắt đầu', exact: true })).toBeVisible();
    await page.getByRole('button', { name: 'Bắt đầu', exact: true }).click();
    await expect(page).toHaveURL(/\/path\/2\/node\/5$/);

    // ── 4. LESSON — NodeHub: title + kicker + lý thuyết đúng node ──
    await expect(page.getByRole('heading', { name: 'Học: Stack', level: 1 })).toBeVisible();
    await expect(page.locator('.node-hub__kicker')).toContainText('STACK.PUSH');
    await expect(page.getByRole('heading', { name: 'Stack', level: 2 }).first()).toBeVisible({ timeout: 15_000 });
    // Modal đã đóng (không còn nút Bắt đầu)
    await expect(page.getByRole('button', { name: 'Bắt đầu', exact: true })).toHaveCount(0);

    // ── 5. SIMULATOR từ NodeHub ──
    await page.getByRole('button', { name: 'Mở mô phỏng', exact: true }).first().click();
    await expect(page).toHaveURL(/\/simulator\/stack\.push$/);
    await expect(page.locator('main').last()).toBeVisible();
    // Về lại NodeHub (route transition ngược)
    await page.goto('/path/2/node/5');
    await expect(page.getByRole('heading', { name: 'Học: Stack', level: 1 })).toBeVisible();

    // ── 6. EXERCISE: Quiz: Stack — chọn đáp án + nộp bài ──
    // Lưu ý: id exercise phụ thuộc seed hiện tại (sau reset: 1-5 final test, 6-29 quiz/lab/code;
    // Quiz: Stack = 12 — deterministic với seed V2 hiện tại; nếu seed đổi, assertion H1 sẽ fail rõ ràng).
    await page.goto('/exercise/12');
    await expect(page.getByRole('heading', { name: 'Quiz: Stack', level: 1 })).toBeVisible({ timeout: 15_000 });
    // QuizStage pre-check (G-BF2): phải trả lời ĐỦ câu mới cho nộp → trả lời cả 8 câu
    const option = page.locator('label.quiz-stage__option').first();
    for (let i = 0; i < 8; i += 1) {
      await option.click();
      if (i < 7) await page.getByRole('button', { name: 'Câu tiếp', exact: true }).click();
    }
    await page.getByRole('button', { name: 'Nộp bài', exact: true }).click();
    // Kết quả: danh sách giải thích từng câu xuất hiện
    await expect(page.locator('.quiz-stage__explain-item').first()).toBeVisible({ timeout: 20_000 });

    // ── 7. PREMIUM ──
    await page.goto('/premium');
    await expect(page.locator('main').last()).toBeVisible();
    await expect(page.getByText(/Premium|Gói/).first()).toBeVisible();

    // ── 8. PROFILE + LEADERBOARD ──
    await page.goto('/profile');
    await expect(page.getByRole('heading', { name: 'Sinh viên mẫu', level: 1 }).or(page.getByRole('heading', { name: 'Sinh viên mẫu' }))).toBeVisible({ timeout: 15_000 });
    await page.goto('/leaderboard');
    await expect(page.locator('main').last()).toBeVisible();

    // ── 9. LOGOUT → guard: /profile chưa đăng nhập → /login?redirect= ──
    await logout(page, 'Sinh viên mẫu');
    await page.goto('/profile');
    await expect(page).toHaveURL(/\/login\?redirect=/);

    // ── 10. TEACHER: classes + class report ──
    await login(page, ACC.teacher.email, ACC.teacher.password);
    await page.goto('/classes');
    await expect(page.getByRole('button', { name: /SD21361 — Cấu trúc dữ liệu/ }).first()).toBeVisible({ timeout: 15_000 });
    await page.goto('/classes/1');
    await expect(page.locator('main').last()).toBeVisible();
    await page.goto('/classes/1/report');
    await expect(page.locator('main').last()).toBeVisible({ timeout: 20_000 });

    // ── 11. ADMIN: users + settings ──
    await logout(page, 'Giáo viên mẫu');
    await login(page, ACC.admin.email, ACC.admin.password);
    await page.goto('/admin/users');
    await expect(page.locator('main').last()).toBeVisible({ timeout: 20_000 });
    await page.goto('/admin/settings');
    await expect(page.locator('main').last()).toBeVisible({ timeout: 15_000 });

    // ── 12. ASSERT cuối: không console error / pageerror / API 4xx-5xx ──
    const normalized = new Map<string, number>();
    for (const u of apiCalls) {
      const key = u.replace(/\/\d+(\?|$)/g, '/:id$1').split('?')[0];
      normalized.set(key, (normalized.get(key) ?? 0) + 1);
    }
    // eslint-disable-next-line no-console
    console.log('API calls per endpoint:', JSON.stringify([...normalized.entries()].sort((a, b) => b[1] - a[1]).slice(0, 15)));
    console.log('HTTP errors:', JSON.stringify(httpErrors));
    console.log('Console errors:', JSON.stringify(errors));
    expect(httpErrors, 'API 4xx/5xx trong chuỗi demo').toEqual([]);
    // Loại trừ session-restore hợp lệ (/auth/refresh, /auth/me, /me/hearts, /me/inventory — header fetch mỗi trang)
    const dupes = [...normalized.entries()]
      .filter(([u]) => !/\/(auth|me)\//.test(u))
      .filter(([, n]) => n > 5); // >5 mới coi là bất thường (lesson/exercise fetch hợp lệ ở nhiều view)
    expect(dupes, 'Duplicate API request (cùng endpoint > 5 lần): ' + JSON.stringify(dupes)).toEqual([]);
    expect(errors, 'Console error / pageerror trong chuỗi demo').toEqual([]);
  });
});
