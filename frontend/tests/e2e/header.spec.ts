/**
 * header.spec.ts — E2E Test Suite cho Header (AppHeader + HeartsGemsWidget)
 *
 * Bao gồm các module:
 * - Module M: Header — Hiển thị theo trạng thái Guest (E-HDR-001 ~ E-HDR-010)
 * - Module N: Header — Hiển thị sau đăng nhập (Student) (E-STU-001 ~ E-STU-007)
 * - Module O: Header — Vai trò Teacher/Admin (E-TCH-001 ~ E-TCH-004)
 * - Module P: Dropdown Menu — E2E (E-MNU-001 ~ E-MNU-009)
 * - Module Q: Navigation (Router) — E2E (E-NAV-001 ~ E-NAV-008)
 * - Module R: Mobile Responsive — E2E (E-RES-001 ~ E-RES-010)
 * - Module S: Glass blur & Scroll effect — E2E (E-GLZ-001 ~ E-GLZ-004)
 * - Module T: Security & Edge cases — E2E (E-SEC-001 ~ E-SEC-007)
 * - Module U: Hearts/Gems Widget — E2E (E-HGW-001 ~ E-HGW-007)
 */
import { expect, test } from '@playwright/test';
import { E2E_EMAIL, E2E_PASSWORD, loginViaUi } from './helpers/auth';
import { mockApi } from './helpers/mockApi';

test.describe('Header E2E — Modules M ~ U', () => {
  // ── Module M: Header — Hiển thị theo trạng thái Guest ──
  test.describe('Module M: Trạng thái Guest (E-HDR-001 ~ E-HDR-010)', () => {
    test('E-HDR-001: Guest: Logo DSA Visual hiển thị với alt text', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      const logo = page.locator('.app-header__brand-img');
      await expect(logo).toBeVisible();
      await expect(logo).toHaveAttribute('alt', 'DSA Visual');
    });

    test('E-HDR-002: Guest: click logo chuyển về trang chủ', async ({ page }) => {
      await mockApi(page);
      await page.goto('/simulations');
      await page.locator('.app-header__brand').click();
      await expect(page).toHaveURL(/\/$/);
    });

    test('E-HDR-003: Guest: hiện đủ 4 link nav chính', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      const nav = page.locator('.app-header__nav');
      for (const name of ['Lộ trình', 'Mô phỏng', 'Thử thách', 'Cửa hàng']) {
        await expect(nav.getByRole('link', { name, exact: true })).toBeVisible();
      }
    });

    test('E-HDR-004: Guest: KHÔNG hiện link Studio', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      const studioLink = page.locator('.app-header__nav').getByRole('link', { name: 'Studio', exact: true });
      await expect(studioLink).toHaveCount(0);
    });

    test('E-HDR-005: Guest: hiện nút Đăng nhập', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      const loginLink = page.locator('.app-header__login');
      await expect(loginLink).toBeVisible();
      await expect(loginLink).toHaveText('Đăng nhập');
    });

    test('E-HDR-006: Guest: hiện nút Đăng ký', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      const regLink = page.locator('.app-header__register');
      await expect(regLink).toBeVisible();
      await expect(regLink).toHaveText('Đăng ký');
    });

    test('E-HDR-007: Guest: KHÔNG hiện HeartsGemsWidget', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await expect(page.locator('.hearts-gems')).toHaveCount(0);
    });

    test('E-HDR-008: Guest: KHÔNG hiện avatar button', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await expect(page.locator('.app-header__user')).toHaveCount(0);
    });

    test('E-HDR-009: Guest: click "Đăng nhập" chuyển đến /login', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await page.locator('.app-header__login').click();
      await expect(page).toHaveURL(/\/login$/);
    });

    test('E-HDR-010: Guest: click "Đăng ký" chuyển đến /register', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await page.locator('.app-header__register').click();
      await expect(page).toHaveURL(/\/register$/);
    });
  });

  // ── Module N: Header — Hiển thị sau đăng nhập (Student) ──
  test.describe('Module N: Trạng thái Student (E-STU-001 ~ E-STU-007)', () => {
    test('E-STU-001: Student: hiện HeartsGemsWidget', async ({ page }) => {
      await loginViaUi(page);
      await expect(page.locator('.hearts-gems')).toBeVisible();
    });

    test('E-STU-002: Student: hiện hearts 10/10', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { initialHearts: 10, heartsMax: 10 });
      await expect(page.locator('.hearts-gems button.hearts-gems__chip')).toContainText('10/10');
    });

    test('E-STU-003: Student: hiện gems', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { initialGems: 120 });
      const gemsChip = page.locator('.hearts-gems__chip[aria-label="Đá quý"]');
      await expect(gemsChip).toBeVisible();
      await expect(gemsChip).toContainText('120');
    });

    test('E-STU-004: Student: hiện avatar button', async ({ page }) => {
      await loginViaUi(page);
      await expect(page.locator('.app-header__user')).toBeVisible();
    });

    test('E-STU-005: Student: KHÔNG hiện Đăng nhập / Đăng ký', async ({ page }) => {
      await loginViaUi(page);
      await expect(page.locator('.app-header__login')).toHaveCount(0);
      await expect(page.locator('.app-header__register')).toHaveCount(0);
    });

    test('E-STU-006: Student: KHÔNG hiện link Studio', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { role: 'STUDENT' });
      await expect(page.locator('.app-header__nav').getByRole('link', { name: 'Studio' })).toHaveCount(0);
    });

    test('E-STU-007: Student: vẫn hiện 4 nav links chính', async ({ page }) => {
      await loginViaUi(page);
      const nav = page.locator('.app-header__nav');
      for (const name of ['Lộ trình', 'Mô phỏng', 'Thử thách', 'Cửa hàng']) {
        await expect(nav.getByRole('link', { name, exact: true })).toBeVisible();
      }
    });
  });

  // ── Module O: Header — Vai trò Teacher/Admin ──
  test.describe('Module O: Vai trò Teacher/Admin (E-TCH-001 ~ E-TCH-004)', () => {
    test('E-TCH-001: Teacher: hiện link Studio trong nav', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { role: 'TEACHER' });
      const studioLink = page.locator('.app-header__nav').getByRole('link', { name: 'Studio' });
      await expect(studioLink).toBeVisible();
    });

    test('E-TCH-002: Teacher: click Studio chuyển đến /studio', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { role: 'TEACHER' });
      await page.locator('.app-header__nav').getByRole('link', { name: 'Studio' }).click();
      await expect(page).toHaveURL(/\/studio/);
    });

    test('E-TCH-003: Admin: hiện link Studio trong nav', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { role: 'ADMIN' });
      const studioLink = page.locator('.app-header__nav').getByRole('link', { name: 'Studio' });
      await expect(studioLink).toBeVisible();
    });

    test('E-TCH-004: Admin: click Studio chuyển đến /studio', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { role: 'ADMIN' });
      await page.locator('.app-header__nav').getByRole('link', { name: 'Studio' }).click();
      await expect(page).toHaveURL(/\/studio/);
    });
  });

  // ── Module P: Dropdown Menu — E2E ──
  test.describe('Module P: Dropdown Menu (E-MNU-001 ~ E-MNU-009)', () => {
    test('E-MNU-001: Click avatar button mở dropdown menu', async ({ page }) => {
      await loginViaUi(page);
      await expect(page.locator('.app-header__menu')).toHaveCount(0);
      await page.locator('.app-header__user').click();
      await expect(page.locator('.app-header__menu')).toBeVisible();
    });

    test('E-MNU-002: Menu có đủ 5 items', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__user').click();
      const menu = page.locator('.app-header__menu');
      await expect(menu.getByRole('link', { name: 'Hồ sơ' })).toBeVisible();
      await expect(menu.getByRole('link', { name: 'Bảng xếp hạng' })).toBeVisible();
      await expect(menu.getByRole('link', { name: 'Premium' })).toBeVisible();
      await expect(menu.getByRole('link', { name: 'Trợ giúp' })).toBeVisible();
      await expect(menu.getByRole('button', { name: 'Đăng xuất' })).toBeVisible();
    });

    test('E-MNU-003: Click "Hồ sơ" chuyển đến /profile và đóng menu', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__user').click();
      await page.locator('.app-header__menu').getByRole('link', { name: 'Hồ sơ' }).click();
      await expect(page).toHaveURL(/\/profile$/);
      await expect(page.locator('.app-header__menu')).toHaveCount(0);
    });

    test('E-MNU-004: Click "Bảng xếp hạng" chuyển đến /leaderboard', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__user').click();
      await page.locator('.app-header__menu').getByRole('link', { name: 'Bảng xếp hạng' }).click();
      await expect(page).toHaveURL(/\/leaderboard$/);
    });

    test('E-MNU-005: Click "Premium" chuyển đến /premium', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__user').click();
      await page.locator('.app-header__menu').getByRole('link', { name: 'Premium' }).click();
      await expect(page).toHaveURL(/\/premium$/);
    });

    test('E-MNU-006: Click "Trợ giúp" chuyển đến /help', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__user').click();
      await page.locator('.app-header__menu').getByRole('link', { name: 'Trợ giúp' }).click();
      await expect(page).toHaveURL(/\/help$/);
    });

    test('E-MNU-007: Click "Đăng xuất" logout và chuyển về /login', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__user').click();
      await page.locator('.app-header__menu').getByRole('button', { name: 'Đăng xuất' }).click();
      await expect(page).toHaveURL(/\/login$/);
      await expect(page.locator('.app-header__user')).toHaveCount(0);
      await expect(page.locator('.app-header__login')).toBeVisible();
    });

    test('E-MNU-008: Click ra ngoài menu đóng dropdown', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__user').click();
      await expect(page.locator('.app-header__menu')).toBeVisible();

      // Click outside
      await page.locator('body').click({ position: { x: 50, y: 300 } });
      await expect(page.locator('.app-header__menu')).toHaveCount(0);
    });

    test('E-MNU-009: Menu đóng khi navigate qua nav link', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__user').click();
      await expect(page.locator('.app-header__menu')).toBeVisible();

      await page.locator('.app-header__nav').getByRole('link', { name: 'Mô phỏng' }).click();
      await expect(page.locator('.app-header__menu')).toHaveCount(0);
    });
  });

  // ── Module Q: Navigation (Router) — E2E ──
  test.describe('Module Q: Navigation (Router) (E-NAV-001 ~ E-NAV-008)', () => {
    test('E-NAV-001: Click "Lộ trình" chuyển đến /path', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__nav').getByRole('link', { name: 'Lộ trình' }).click();
      await expect(page).toHaveURL(/\/path$/);
    });

    test('E-NAV-002: Click "Mô phỏng" chuyển đến /simulations', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__nav').getByRole('link', { name: 'Mô phỏng' }).click();
      await expect(page).toHaveURL(/\/simulations$/);
    });

    test('E-NAV-003: Click "Thử thách" chuyển đến /quests', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__nav').getByRole('link', { name: 'Thử thách' }).click();
      await expect(page).toHaveURL(/\/quests$/);
    });

    test('E-NAV-004: Click "Cửa hàng" chuyển đến /shop', async ({ page }) => {
      await loginViaUi(page);
      await page.locator('.app-header__nav').getByRole('link', { name: 'Cửa hàng' }).click();
      await expect(page).toHaveURL(/\/shop$/);
    });

    test('E-NAV-005: Guest click "Thử thách" bị redirect về /login có query redirect', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await page.locator('.app-header__nav').getByRole('link', { name: 'Thử thách' }).click();
      await expect(page).toHaveURL(/\/login\?redirect=(%2F|\/)quests/);
    });

    test('E-NAV-006: Guest click "Cửa hàng" bị redirect về /login có query redirect', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await page.locator('.app-header__nav').getByRole('link', { name: 'Cửa hàng' }).click();
      await expect(page).toHaveURL(/\/login\?redirect=(%2F|\/)shop/);
    });

    test('E-NAV-007: Active link có class router-link-exact-active', async ({ page }) => {
      await mockApi(page);
      await page.goto('/simulations');
      const simLink = page.locator('.app-header__nav').getByRole('link', { name: 'Mô phỏng' });
      await expect(simLink).toHaveClass(/router-link-exact-active/);
    });

    test('E-NAV-008: Header có position: fixed và luôn hiển thị khi scroll', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      const header = page.locator('header.app-header');
      await expect(header).toBeVisible();

      await page.evaluate(() => window.scrollTo(0, 500));
      await expect(header).toBeVisible();
    });
  });

  // ── Module R: Mobile Responsive — E2E ──
  test.describe('Module R: Mobile Responsive (E-RES-001 ~ E-RES-010)', () => {
    test.beforeEach(async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 812 });
    });

    test('E-RES-001: Mobile 375px: ẩn nav desktop, hiện burger button', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await expect(page.locator('.app-header__burger')).toBeVisible();
      await expect(page.locator('.app-header__nav')).toBeHidden();
    });

    test('E-RES-002: Mobile: click burger mở mobile nav', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await expect(page.locator('.app-header__mobile-nav')).toHaveCount(0);
      await page.locator('.app-header__burger').click();
      await expect(page.locator('.app-header__mobile-nav')).toBeVisible();
    });

    test('E-RES-003: Mobile nav Guest có đủ 4 link + Đăng nhập + Đăng ký', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await page.locator('.app-header__burger').click();
      const mobileNav = page.locator('.app-header__mobile-nav');

      await expect(mobileNav.getByRole('link', { name: 'Lộ trình' })).toBeVisible();
      await expect(mobileNav.getByRole('link', { name: 'Mô phỏng' })).toBeVisible();
      await expect(mobileNav.getByRole('link', { name: 'Thử thách' })).toBeVisible();
      await expect(mobileNav.getByRole('link', { name: 'Cửa hàng' })).toBeVisible();
      await expect(mobileNav.getByRole('link', { name: 'Đăng nhập' })).toBeVisible();
      await expect(mobileNav.getByRole('link', { name: 'Đăng ký' })).toBeVisible();
    });

    test('E-RES-004: Mobile nav authed có 4 nav + link tài khoản', async ({ page }) => {
      await mockApi(page, { authenticated: true });
      await page.goto('/path');
      await page.locator('.app-header__burger').click();
      const mobileNav = page.locator('.app-header__mobile-nav');

      await expect(mobileNav.getByRole('link', { name: 'Hồ sơ' })).toBeVisible();
      await expect(mobileNav.getByRole('link', { name: 'Bảng xếp hạng' })).toBeVisible();
      await expect(mobileNav.getByRole('link', { name: 'Premium' })).toBeVisible();
      await expect(mobileNav.getByRole('link', { name: 'Trợ giúp' })).toBeVisible();
      await expect(mobileNav.getByRole('button', { name: 'Đăng xuất' })).toBeVisible();
    });

    test('E-RES-005: Mobile: click link đóng burger menu', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await page.locator('.app-header__burger').click();
      await expect(page.locator('.app-header__mobile-nav')).toBeVisible();

      await page.locator('.app-header__mobile-nav').getByRole('link', { name: 'Mô phỏng' }).click();
      await expect(page.locator('.app-header__mobile-nav')).toHaveCount(0);
    });

    test('E-RES-006: Mobile: teacher thấy Studio trong mobile nav', async ({ page }) => {
      await mockApi(page, { authenticated: true, role: 'TEACHER' });
      await page.goto('/path');
      await page.locator('.app-header__burger').click();
      const studioLink = page.locator('.app-header__mobile-nav').getByRole('link', { name: 'Studio' });
      await expect(studioLink).toBeVisible();
    });

    test('E-RES-007: Mobile: Đăng nhập/Đăng ký trên header bị ẩn CSS (display: none)', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await expect(page.locator('.app-header__login')).toBeHidden();
      await expect(page.locator('.app-header__register')).toBeHidden();
    });

    test('E-RES-008: Mobile 390px: không có thanh cuộn ngang (overflow)', async ({ page }) => {
      await page.setViewportSize({ width: 390, height: 844 });
      await mockApi(page);
      await page.goto('/');
      const overflow = await page.evaluate(() => {
        return document.documentElement.scrollWidth > document.documentElement.clientWidth;
      });
      expect(overflow).toBeFalsy();
    });

    test('E-RES-009: Mobile: Đăng xuất từ mobile nav', async ({ page }) => {
      await mockApi(page, { authenticated: true });
      await page.goto('/path');
      await page.locator('.app-header__burger').click();
      await page.locator('.app-header__mobile-nav').getByRole('button', { name: 'Đăng xuất' }).click();
      await expect(page).toHaveURL(/\/login$/);
    });

    test('E-RES-010: Tablet 768px+ nav desktop hiện lại, burger ẩn', async ({ page }) => {
      await page.setViewportSize({ width: 1024, height: 768 });
      await mockApi(page);
      await page.goto('/');
      await expect(page.locator('.app-header__nav')).toBeVisible();
      await expect(page.locator('.app-header__burger')).toBeHidden();
    });
  });

  // ── Module S: Glass blur & Scroll effect — E2E ──
  test.describe('Module S: Glass blur & Scroll effect (E-GLZ-001 ~ E-GLZ-004)', () => {
    test('E-GLZ-001: Chưa scroll: không có class app-header--scrolled', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await expect(page.locator('header.app-header')).not.toHaveClass(/app-header--scrolled/);
    });

    test('E-GLZ-002: Scroll > 50px: có class app-header--scrolled', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await page.evaluate(() => window.scrollTo(0, 100));
      await expect(page.locator('header.app-header')).toHaveClass(/app-header--scrolled/);
    });

    test('E-GLZ-003: Scroll lên lại <= 50px: mất class app-header--scrolled', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      await page.evaluate(() => window.scrollTo(0, 100));
      await expect(page.locator('header.app-header')).toHaveClass(/app-header--scrolled/);

      await page.evaluate(() => window.scrollTo(0, 0));
      await expect(page.locator('header.app-header')).not.toHaveClass(/app-header--scrolled/);
    });

    test('E-GLZ-004: Logo shrink khi scroll', async ({ page }) => {
      await mockApi(page);
      await page.goto('/');
      const logo = page.locator('.app-header__brand-img');
      const initialHeight = (await logo.boundingBox())?.height ?? 44;

      await page.evaluate(() => window.scrollTo(0, 100));
      await page.waitForTimeout(350);

      const scrolledHeight = (await logo.boundingBox())?.height ?? 38;
      expect(scrolledHeight).toBeLessThanOrEqual(initialHeight);
    });
  });

  // ── Module T: Security & Edge cases — E2E ──
  test.describe('Module T: Security & Edge cases (E-SEC-001 ~ E-SEC-007)', () => {
    test('E-SEC-001: Student cố truy cập /studio bị redirect về /profile', async ({ page }) => {
      await mockApi(page, { authenticated: true, role: 'STUDENT' });
      await page.goto('/studio');
      await expect(page).toHaveURL(/\/profile$/);
    });

    test('E-SEC-002: Guest cố truy cập /profile bị redirect về /login', async ({ page }) => {
      await mockApi(page);
      await page.goto('/profile');
      await expect(page).toHaveURL(/\/login\?redirect=(%2F|\/)profile/);
    });

    test('E-SEC-003: Guest cố truy cập /admin bị redirect về /login', async ({ page }) => {
      await mockApi(page);
      await page.goto('/admin');
      await expect(page).toHaveURL(/\/login/);
    });

    test('E-SEC-004: TEACHER_PENDING cố vào /studio bị chuyển hướng về /pending-teacher', async ({ page }) => {
      await mockApi(page, { authenticated: true, role: 'TEACHER_PENDING' });
      await page.goto('/studio');
      await expect(page).toHaveURL(/\/pending-teacher$/);
    });

    test('E-SEC-005: Đã đăng nhập truy cập /login bị chuyển hướng về trang chủ', async ({ page }) => {
      await mockApi(page, { authenticated: true });
      await page.goto('/login');
      await expect(page).toHaveURL(/\/$/);
    });

    test('E-SEC-006: Header không có console error bất thường khi tải trang', async ({ page }) => {
      const errors: string[] = [];
      page.on('console', (msg) => {
        if (msg.type() === 'error') errors.push(msg.text());
      });
      page.on('pageerror', (err) => errors.push(err.message));

      await mockApi(page);
      await page.goto('/');
      await page.waitForTimeout(500);

      const unexpected = errors.filter((e) => !e.includes('favicon') && !/401 \(Unauthorized\)/.test(e));
      expect(unexpected).toHaveLength(0);
    });

    test('E-SEC-007: Logout reset trạng thái gamification và avatar', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { initialHearts: 8, initialGems: 300 });
      await expect(page.locator('.hearts-gems')).toBeVisible();

      await page.locator('.app-header__user').click();
      await page.locator('.app-header__menu').getByRole('button', { name: 'Đăng xuất' }).click();
      await expect(page).toHaveURL(/\/login$/);

      await expect(page.locator('.hearts-gems')).toHaveCount(0);
      await expect(page.locator('.app-header__user')).toHaveCount(0);
    });
  });

  // ── Module U: Hearts/Gems Widget — E2E ──
  test.describe('Module U: Hearts/Gems Widget (E-HGW-001 ~ E-HGW-007)', () => {
    test('E-HGW-001: Hearts hiện 10/10', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { initialHearts: 10, heartsMax: 10 });
      await expect(page.locator('.hearts-gems button.hearts-gems__chip')).toContainText('10/10');
    });

    test('E-HGW-002: Hearts hiện 3/10', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { initialHearts: 3, heartsMax: 10 });
      await expect(page.locator('.hearts-gems button.hearts-gems__chip')).toContainText('3/10');
    });

    test('E-HGW-003: Hearts = 0 có class cảnh báo --empty', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { initialHearts: 0, heartsMax: 10 });
      const heartsBtn = page.locator('.hearts-gems button.hearts-gems__chip');
      await expect(heartsBtn).toContainText('0/10');
      await expect(heartsBtn).toHaveClass(/hearts-gems__chip--empty/);
    });

    test('E-HGW-004: Click hearts chip hiện popover "Tim của bạn"', async ({ page }) => {
      await loginViaUi(page);
      await expect(page.locator('.hearts-gems__pop')).toHaveCount(0);
      await page.locator('.hearts-gems button.hearts-gems__chip').click();
      const popover = page.locator('.hearts-gems__pop');
      await expect(popover).toBeVisible();
      await expect(popover.locator('.hearts-gems__pop-title')).toHaveText('Tim của bạn');
    });

    test('E-HGW-005: Gems hiện đúng số 500', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { initialGems: 500 });
      const gemsChip = page.locator('.hearts-gems__chip[aria-label="Đá quý"]');
      await expect(gemsChip).toContainText('500');
    });

    test('E-HGW-006: Streak hiện khi streakDays > 0', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { streakDays: 7 });
      const streakChip = page.locator('.hearts-gems__chip[aria-label="Chuỗi ngày"]');
      await expect(streakChip).toBeVisible();
      await expect(streakChip).toContainText('🔥 7');
    });

    test('E-HGW-007: Streak ẩn khi streakDays = 0', async ({ page }) => {
      await loginViaUi(page, E2E_EMAIL, E2E_PASSWORD, { streakDays: 0 });
      const streakChip = page.locator('.hearts-gems__chip[aria-label="Chuỗi ngày"]');
      await expect(streakChip).toHaveCount(0);
    });
  });
});
