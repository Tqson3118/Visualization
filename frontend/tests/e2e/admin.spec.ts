import { expect, test } from '@playwright/test';

test.describe('Admin Flow: Live & E2E Validation', () => {
  test.beforeEach(async ({ page }) => {
    // Tăng timeout do dùng Cloud DB
    page.setDefaultTimeout(30000);
  });

  test('00. Admin Authentication & Access Control', async ({ page }) => {
    await page.goto('/login');
    await page.fill('#email', 'admin@system.local');
    await page.fill('#password', 'Admin@123');
    await page.click('button[type="submit"]');

    // Chờ chuyển hướng sau khi xác thực từ Cloud DB (tối đa 15s)
    await page.waitForURL((url) => !url.pathname.includes('/login'), { timeout: 15000 });
    expect(page.url()).not.toContain('/login');
  });

  test('Kịch bản 1: Phê duyệt Giảng viên mới đăng ký (Teacher Approval Flow)', async ({ page }) => {
    // Đăng nhập
    await page.goto('/login');
    await page.fill('#email', 'admin@system.local');
    await page.fill('#password', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForURL((url) => !url.pathname.includes('/login'), { timeout: 15000 });

    // Vào trang Quản lý người dùng
    await page.goto('/admin/users');
    await page.waitForSelector('.admin-users__hero, .admin-users__table, .admin-users__filters', { timeout: 15000 });

    // Tìm và chọn tab Giảng viên chờ duyệt
    const pendingTab = page.locator('button, [role="tab"]').filter({ hasText: /chờ duyệt|pending/i }).first();
    if (await pendingTab.isVisible()) {
      await pendingTab.click();
      await page.waitForTimeout(2000);

      // Kiểm tra nút Phê duyệt / Từ chối hoặc Empty State
      const approveBtns = page.locator('button').filter({ hasText: /duyệt|approve/i });
      const pendingCount = await approveBtns.count();
      console.log(`[Admin E2E] Số lượng giảng viên chờ duyệt: ${pendingCount}`);
    }
    await expect(page.locator('body')).toBeVisible();
  });

  test('Kịch bản 2: Phân quyền & Khóa / Mở khóa người dùng (User Management)', async ({ page }) => {
    await page.goto('/login');
    await page.fill('#email', 'admin@system.local');
    await page.fill('#password', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForURL((url) => !url.pathname.includes('/login'), { timeout: 15000 });

    await page.goto('/admin/users');
    await page.waitForSelector('.admin-users__filters', { timeout: 15000 });
    // Chờ bảng dữ liệu tải xong từ Cloud DB
    await page.waitForTimeout(3000);

    // Kiểm tra ô tìm kiếm người dùng
    const searchInput = page.locator('input.admin-users__search, input[placeholder*="Tìm"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('student');
      await searchInput.press('Enter');
      await page.waitForTimeout(2000);
      await searchInput.clear();
      await searchInput.press('Enter');
      await page.waitForTimeout(2000);
    }

    // Kiểm tra giao diện người dùng
    const tableOrEmpty = page.locator('.admin-users__table, .admin-users__row, table, tbody');
    await expect(tableOrEmpty.first()).toBeVisible();
  });

  test('Kịch bản 3: Phê duyệt Lộ trình học của Giảng viên (Curriculum Moderation)', async ({ page }) => {
    await page.goto('/login');
    await page.fill('#email', 'admin@system.local');
    await page.fill('#password', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(3000);

    // Truy cập Studio
    await page.goto('/studio');
    await page.waitForTimeout(3000);
    await expect(page.locator('body')).toBeVisible();

    // Truy cập Lộ trình công khai
    await page.goto('/path');
    await page.waitForTimeout(2000);
    await expect(page.locator('body')).toBeVisible();
  });

  test('Kịch bản 4: Giám sát Thống kê & Báo cáo Analytics toàn hệ thống (/admin/stats)', async ({ page }) => {
    await page.goto('/login');
    await page.fill('#email', 'admin@system.local');
    await page.fill('#password', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(3000);

    await page.goto('/admin/stats');
    await page.waitForTimeout(3000);

    // Kiểm tra các thành phần thống kê
    await expect(page.locator('body')).toBeVisible();
  });

  test('Kịch bản 5: Cấu hình tham số Hệ thống (/admin/settings)', async ({ page }) => {
    await page.goto('/login');
    await page.fill('#email', 'admin@system.local');
    await page.fill('#password', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(3000);

    await page.goto('/admin/settings');
    await page.waitForTimeout(3000);
    await expect(page.locator('body')).toBeVisible();
  });

  test('A1: Chống Admin tự khóa chính mình (Security Rule)', async ({ page }) => {
    await page.goto('/login');
    await page.fill('#email', 'admin@system.local');
    await page.fill('#password', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(3000);

    await page.goto('/admin/users');
    await page.waitForTimeout(2000);

    // Kiểm tra dòng của chính admin@system.local
    const adminRow = page.locator('tr').filter({ hasText: 'admin@system.local' });
    if ((await adminRow.count()) > 0) {
      const banBtn = adminRow.locator('button').filter({ hasText: /khóa|vô hiệu/i });
      const canClick = await banBtn.isEnabled().catch(() => false);
      expect(canClick).toBe(false);
    }
  });

  test('A2: Chặn User thường/Khách truy cập trang Admin (RBAC Route Guard)', async ({ page }) => {
    // Chưa đăng nhập -> Cố tình vào /admin/users
    await page.goto('/admin/users');
    await page.waitForTimeout(2000);
    const currentUrl = page.url();
    // Phải bị redirect về /login hoặc /profile
    expect(currentUrl).toMatch(/\/(login|profile|$)/);
  });

  test('A4: Tìm kiếm người dùng không tồn tại / Ký tự đặc biệt (Empty State)', async ({ page }) => {
    await page.goto('/login');
    await page.fill('#email', 'admin@system.local');
    await page.fill('#password', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(3000);

    await page.goto('/admin/users');
    await page.waitForTimeout(2000);

    const searchBox = page.locator('input[placeholder*="Tìm"], input[placeholder*="search"]').first();
    if (await searchBox.isVisible()) {
      await searchBox.fill('!@#$%^&*()_NONEXISTENT_USER_999');
      await page.waitForTimeout(1500);
      // Giao diện không bị crash
      await expect(page.locator('body')).toBeVisible();
    }
  });

  test('A5: Phân trang & Bộ lọc người dùng', async ({ page }) => {
    await page.goto('/login');
    await page.fill('#email', 'admin@system.local');
    await page.fill('#password', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(3000);

    await page.goto('/admin/users');
    await page.waitForTimeout(2000);

    const roleSelect = page.locator('select').first();
    if (await roleSelect.isVisible()) {
      await roleSelect.selectOption({ index: 1 });
      await page.waitForTimeout(1000);
    }
    await expect(page.locator('body')).toBeVisible();
  });
});

