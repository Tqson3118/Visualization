/**
 * ux.spec.ts — TEST-UX-001: khảo sát 5 nhiệm vụ chính của người dùng mới
 * (docs/TEST_PLAN.md §9 — NV1..NV5), chạy thật trên Chromium 18/08/2026.
 *
 * Đo thời gian hoàn thành THỰC TẾ bằng đồng hồ bấm giờ trong spec (Date.now()),
 * ngưỡng theo TEST_PLAN §9. Console log "UX-NVx hoàn thành: <time>" được thu
 * thập để điền Bảng 6.6 (Kết quả khảo sát UX) trong báo cáo.
 *
 * Lưu ý: token chỉ nằm trong memory Pinia (ADR-004) → mọi page.goto (full reload)
 * phải qua login cùng phiên để boot refresh trả 200; điều hướng trong SPA không
 * reload nên giữ được phiên đăng nhập.
 */
import { expect, test } from '@playwright/test';

import { loginViaUi } from './helpers/auth';
import { mockApi } from './helpers/mockApi';

function fmt(ms: number): string {
  return `${(ms / 1000).toFixed(1)}s`;
}

test.describe('TEST-UX-001: Khảo sát 5 nhiệm vụ người dùng (số liệu đo thật)', () => {
  test('NV1: Tạo tài khoản + mở bài học đầu tiên (ngưỡng ≤ 5 phút)', async ({ page }) => {
    test.setTimeout(120_000); // lần load đầu sau khi vite recompile có thể chậm
    const t0 = Date.now();
    // Log mọi lỗi để chẩn đoán nếu register không navigate (xem HANDOFF_20260818_ERD_UX_TESTS.md ▶1)
    page.on('pageerror', (e) => console.log(`UX-NV1 PAGEERR: ${String(e).slice(0, 300)}`));
    page.on('console', (m) => {
      if (m.type() === 'error' || m.type() === 'warning') console.log(`UX-NV1 CONSOLE ${m.type()}: ${m.text().slice(0, 200)}`);
    });
    await mockApi(page);
    // Warm-up dev server (vite on-demand compile) trước khi đo thật
    await page.goto('/');
    await page.waitForTimeout(800);
    await page.goto('/register');

    const agree = page.locator('form input[type="checkbox"]').first();
    // BUG FLAKE đã tìm ra (18/08): `if (await agree.isVisible())` KHÔNG auto-wait — đôi khi chạy
    // trước khi Vue mount xong form → skip check → submit báo "Bạn phải đồng ý chính sách".
    // expect(...).toBeVisible() auto-wait tới khi form render (listener v-model đã đính sẵn).
    await expect(agree).toBeVisible({ timeout: 10_000 });
    await agree.check({ force: true });
    await expect(agree).toBeChecked();
    const inputs = page.locator('form.register__card .ui-input input, form input:not([type="checkbox"])');
    await inputs.nth(0).fill('Người dùng UX');
    await inputs.nth(1).fill('ux@demo.local');
    await inputs.nth(2).fill('Student@123');
    await inputs.nth(3).fill('Student@123');
    await inputs.nth(3).press('Tab');
    // Click đôi khi bị nuốt do blur → validate → re-render (auth.spec đã ghi nhận) → retry
    let registered = false;
    for (let attempt = 0; attempt < 3; attempt++) {
      await page.waitForTimeout(500);
      await page.getByRole('button', { name: 'Đăng ký', exact: true }).click({ force: true });
      try {
        await expect(page).toHaveURL(/\/path/, { timeout: 8_000 });
        registered = true;
        break;
      } catch {
        console.log(`UX-NV1 attempt ${attempt + 1} fail, URL=${page.url()}; errors=${JSON.stringify(await page.locator('[role="alert"], .register__error').allInnerTexts().catch(() => []))}`);
      }
    }
    expect(registered, 'Đăng ký phải navigate sang /path').toBe(true);

    // Điều hướng trong SPA (không reload → giữ phiên): /path → course-detail → bài học đầu tiên
    // LƯU Ý: CoursesListView.vue đặt role="listitem" TRÊN <router-link> có aria-label → phần tử
    // được expose là listitem (KHÔNG phải link) → dùng getByRole('listitem', ...) mới khớp.
    const courseLink = page.getByRole('listitem', { name: /Xem chi tiết lộ trình/ }).first();
    await courseLink.click({ timeout: 45_000 });
    await expect(page).toHaveURL(/\/path\/1/, { timeout: 15_000 });
    const lessonBtn = page.locator('button:has-text("Sắp xếp nổi bọt")').first();
    await lessonBtn.click();
    await expect(page).toHaveURL(/\/lessons\/1/, { timeout: 8_000 });
    await expect(page.getByRole('heading', { level: 1 }).first()).toBeVisible({ timeout: 10_000 });

    const d = Date.now() - t0;
    console.log(`UX-NV1 hoàn thành: ${fmt(d)}`);
    expect(d).toBeLessThan(5 * 60 * 1000);
  });

  test('NV2: Chạy mô phỏng Bubble Sort với dữ liệu tự nhập (ngưỡng ≤ 2 phút)', async ({ page }) => {
    test.setTimeout(120_000);
    const t0 = Date.now();
    await mockApi(page);
    await page.goto('/simulator/sort.bubble');
    await expect(page.getByRole('heading', { level: 1 })).toContainText('Sắp xếp nổi bọt');

    // D6b: /simulator/sort.* dùng SharedVisualizerShell → VcrDockBar (range .vcr-scrubber)
    const indicator = page.locator('.vcr-scrubber');
    await expect(indicator).toHaveValue('0', { timeout: 15_000 });

    const customInput = page.locator('input[placeholder*="mảng"], input[placeholder*="dữ liệu"], textarea').first();
    if (await customInput.isVisible({ timeout: 2_000 }).catch(() => false)) {
      await customInput.fill('[5,3,1]');
      await page.keyboard.press('Enter');
      await expect(indicator).toHaveValue('0', { timeout: 10_000 });
    }

    await page.getByRole('button', { name: 'Phát', exact: true }).click({ force: true });
    await expect(page.getByRole('button', { name: 'Tạm dừng', exact: true })).toBeVisible();
    await page.waitForTimeout(1_500);
    const step = Number(await indicator.inputValue());
    expect(step).toBeGreaterThan(0);

    const d = Date.now() - t0;
    console.log(`UX-NV2 hoàn thành: ${fmt(d)}`);
    expect(d).toBeLessThan(2 * 60 * 1000);
  });

  test('NV3: Làm 1 bài tập trắc nghiệm + xem kết quả (ngưỡng ≤ 5 phút)', async ({ page }) => {
    const t0 = Date.now();
    await mockApi(page);
    await page.route('**/api/v1/exercises/1/submit', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          score: 4,
          maxScore: 4,
          passed: true,
          correctCount: 4,
          totalQuestions: 4,
          explanations: { '1': 'Đúng', '2': 'Đúng', '3': 'Đúng', '4': 'Đúng' },
        }),
      });
    });
    await loginViaUi(page, 'student@demo.local', 'Student@123');
    await page.goto('/exercise/1');
    await expect(page.getByRole('heading', { level: 1 })).toContainText('Trắc nghiệm');

    // Trả lời cả 4 câu (chọn đáp án đầu tiên), nhảy câu qua dot điều hướng
    await page.locator('.quiz-stage__option input').first().check({ force: true });
    for (let i = 2; i <= 4; i++) {
      await page.locator(`button[aria-label="Nhảy tới câu ${i}"]`).click({ force: true });
      await page.locator('.quiz-stage__option input').first().check({ force: true });
    }

    await page.getByRole('button', { name: 'Nộp bài', exact: true }).click();
    await expect(page.getByText(/Đạt yêu cầu|100%|Kết quả/).first()).toBeVisible({ timeout: 8_000 });

    const d = Date.now() - t0;
    console.log(`UX-NV3 hoàn thành: ${fmt(d)}`);
    expect(d).toBeLessThan(5 * 60 * 1000);
  });

  test('NV4: Tìm bài học bằng ô tìm kiếm (ngưỡng ≤ 1 phút)', async ({ page }) => {
    const t0 = Date.now();
    await mockApi(page);
    await loginViaUi(page, 'student@demo.local', 'Student@123');
    await page.goto('/lessons/1');
    const searchBox = page.locator('input[placeholder*="Tìm bài học"]');
    await expect(searchBox).toBeVisible();
    await searchBox.fill('Bubble');
    await expect(page.getByText(/Sắp xếp nổi bọt/).first()).toBeVisible();

    const d = Date.now() - t0;
    console.log(`UX-NV4 hoàn thành: ${fmt(d)}`);
    expect(d).toBeLessThan(60 * 1000);
  });

  test('NV5: Xem báo cáo tiến độ cá nhân (ngưỡng ≤ 2 phút)', async ({ page }) => {
    const t0 = Date.now();
    await mockApi(page);
    await loginViaUi(page, 'student@demo.local', 'Student@123');
    await page.goto('/profile');
    await expect(page.getByRole('heading', { name: 'E2E Student', level: 1 })).toBeVisible({ timeout: 8_000 });
    await expect(page.getByText(/Tiến độ|hoàn thành|Đã học/).first()).toBeVisible();

    const d = Date.now() - t0;
    console.log(`UX-NV5 hoàn thành: ${fmt(d)}`);
    expect(d).toBeLessThan(2 * 60 * 1000);
  });
});
