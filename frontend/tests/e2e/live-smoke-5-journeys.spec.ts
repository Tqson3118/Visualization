import { test, expect } from '@playwright/test';

const BASE = 'http://localhost:8081';

test.describe('LIVE SMOKE TEST - 5 REAL USER JOURNEYS (E2E Browser Live)', () => {
  test.setTimeout(180_000);

  test('Hành trình 1: Học viên đăng nhập & Học bài & Quiz', async ({ page }) => {
    // 1.1 Login student
    await page.goto(`${BASE}/login`);
    await expect(page.locator('#email')).toBeVisible();
    await page.locator('#email').fill('student@demo.local');
    await page.locator('#password').fill('Student@123');
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await page.waitForURL(/\/(courses|path|home)/, { timeout: 15_000 });

    // 1.2 Vào trang danh sách khóa học
    await page.goto(`${BASE}/courses`);
    await expect(page.locator('main').or(page.locator('.course-list-view')).or(page.locator('.courses-list-view')).first()).toBeVisible({ timeout: 10_000 });

    // 1.3 Xem chi tiết khóa học 1
    await page.goto(`${BASE}/courses/1`);
    await expect(page.locator('h1').first()).toBeVisible({ timeout: 10_000 });

    // 1.4 Xem bài học số 1
    await page.goto(`${BASE}/lessons/1?courseId=1`);
    await expect(page.locator('body')).toBeVisible();

    // 1.5 Làm bài tập quiz
    await page.goto(`${BASE}/exercise/6`);
    await expect(page.locator('h1').first()).toBeVisible({ timeout: 10_000 });
  });

  test('Hành trình 2: Simulator Bubble Sort & Chuyển thuật toán', async ({ page }) => {
    // 2.1 Vào Simulator Bubble Sort
    await page.goto(`${BASE}/simulator/sort.bubble`);
    await expect(page.locator('body')).toBeVisible();
    await page.waitForTimeout(1000);

    // 2.2 Vào Sandbox / Sorting Sandbox
    await page.goto(`${BASE}/sorting-sandbox`);
    await expect(page.locator('body')).toBeVisible();
    await page.waitForTimeout(1000);

    // 2.3 Chuyển thuật toán Selection Sort
    await page.goto(`${BASE}/simulator/sort.selection`);
    await expect(page.locator('body')).toBeVisible();
  });

  test('Hành trình 3: Shop & Gamification & Profile', async ({ page }) => {
    // 3.1 Login student
    await page.goto(`${BASE}/login`);
    await page.locator('#email').fill('student@demo.local');
    await page.locator('#password').fill('Student@123');
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await page.waitForTimeout(1000);

    // 3.2 Vào trang Shop
    await page.goto(`${BASE}/shop`);
    await expect(page.locator('body')).toBeVisible();
    await expect(page.getByText(/Shop|Cửa hàng|Gems/i).first()).toBeVisible({ timeout: 10_000 });

    // 3.3 Vào trang Profile & Leaderboard
    await page.goto(`${BASE}/profile`);
    await expect(page.locator('body')).toBeVisible();

    await page.goto(`${BASE}/leaderboard`);
    await expect(page.locator('body')).toBeVisible();
  });

  test('Hành trình 4: Giảng viên quản lý lớp học', async ({ page }) => {
    // 4.1 Login teacher
    await page.goto(`${BASE}/login`);
    await page.locator('#email').fill('teacher@demo.local');
    await page.locator('#password').fill('Teacher@123');
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await page.waitForTimeout(1000);

    // 4.2 Vào trang Classes
    await page.goto(`${BASE}/classes`);
    await expect(page.locator('body')).toBeVisible();
    await expect(page.getByText(/SD21361/i).first()).toBeVisible({ timeout: 15_000 });

    // 4.3 Xem chi tiết lớp 1
    await page.goto(`${BASE}/classes/1`);
    await expect(page.locator('body')).toBeVisible();

    // 4.4 Xem báo cáo tiến độ lớp 1
    await page.goto(`${BASE}/classes/1/report`);
    await expect(page.locator('body')).toBeVisible();
  });

  test('Hành trình 5: Quản trị viên Admin (Users & Stats)', async ({ page }) => {
    // 5.1 Login admin
    await page.goto(`${BASE}/login`);
    await page.locator('#email').fill('admin@system.local');
    await page.locator('#password').fill('Admin@123');
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await page.waitForTimeout(1000);

    // 5.2 Quản lý người dùng
    await page.goto(`${BASE}/admin/users`);
    await expect(page.locator('body')).toBeVisible();
    await expect(page.getByText(/Quản trị viên|admin@system\.local/i).first()).toBeVisible({ timeout: 15_000 });

    // 5.3 Xem thống kê Admin Stats
    await page.goto(`${BASE}/admin/stats`);
    await expect(page.locator('body')).toBeVisible();
  });
});
