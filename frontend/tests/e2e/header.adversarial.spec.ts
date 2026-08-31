/**
 * header.adversarial.spec.ts — Bộ kiểm thử tình huống khắc nghiệt, biên dị thường & bảo mật cho Header
 */
import { expect, test } from '@playwright/test';
import { E2E_EMAIL, E2E_PASSWORD, loginViaUi } from './helpers/auth';
import { mockApi } from './helpers/mockApi';

test.describe('Header Adversarial & Stress E2E Tests', () => {
  test('ADV-E2E-001: Bấm phím Escape đóng Dropdown Menu', async ({ page }) => {
    await loginViaUi(page);
    await page.locator('.app-header__user').click();
    await expect(page.locator('.app-header__menu')).toBeVisible();

    await page.keyboard.press('Escape');
    await expect(page.locator('.app-header__menu')).toHaveCount(0);
  });

  test('ADV-E2E-002: Bấm phím Escape đóng Mobile Nav Drawer', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await mockApi(page);
    await page.goto('/');
    await page.locator('.app-header__burger').click();
    await expect(page.locator('.app-header__mobile-nav')).toBeVisible();

    await page.keyboard.press('Escape');
    await expect(page.locator('.app-header__mobile-nav')).toHaveCount(0);
  });

  test('ADV-E2E-003: Bấm phím Escape đóng Heart Popover', async ({ page }) => {
    await loginViaUi(page);
    await page.locator('.hearts-gems button.hearts-gems__chip').click();
    await expect(page.locator('.hearts-gems__pop')).toBeVisible();

    await page.keyboard.press('Escape');
    await expect(page.locator('.hearts-gems__pop')).toHaveCount(0);
  });

  test('ADV-E2E-004: Click ra ngoài Heart Popover tự động đóng popover', async ({ page }) => {
    await loginViaUi(page);
    await page.locator('.hearts-gems button.hearts-gems__chip').click();
    await expect(page.locator('.hearts-gems__pop')).toBeVisible();

    await page.locator('body').click({ position: { x: 50, y: 300 } });
    await expect(page.locator('.hearts-gems__pop')).toHaveCount(0);
  });

  test('ADV-E2E-005: Mở Popover Tim rồi chuyển trang sang /simulations → Popover phải tự động đóng', async ({ page }) => {
    await loginViaUi(page);
    await page.locator('.hearts-gems button.hearts-gems__chip').click();
    await expect(page.locator('.hearts-gems__pop')).toBeVisible();

    await page.locator('.app-header__nav').getByRole('link', { name: 'Mô phỏng' }).click();
    await expect(page).toHaveURL(/\/simulations$/);
    await expect(page.locator('.hearts-gems__pop')).toHaveCount(0);
  });

  test('ADV-E2E-006: Server 500 lỗi trên /me/hearts và /me/streak không làm sập Header', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/me/hearts', (route) => route.fulfill({ status: 500, body: 'Server Error' }));
    await page.route('**/api/v1/me/streak', (route) => route.fulfill({ status: 500, body: 'Server Error' }));

    await page.goto('/');
    const header = page.locator('header.app-header');
    await expect(header).toBeVisible();
    await expect(page.locator('.app-header__brand')).toBeVisible();
  });

  test('ADV-E2E-007: Mở mobile menu ở viewport 375px rồi resize lên 1280px không bị vỡ giao diện', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await mockApi(page);
    await page.goto('/');
    await page.locator('.app-header__burger').click();
    await expect(page.locator('.app-header__mobile-nav')).toBeVisible();

    await page.setViewportSize({ width: 1280, height: 800 });
    await expect(page.locator('.app-header__nav')).toBeVisible();
  });

  test('ADV-E2E-008: Spam click 10 lần liên tục vào burger button', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await mockApi(page);
    await page.goto('/');
    const burger = page.locator('.app-header__burger');

    for (let i = 0; i < 10; i++) {
      await burger.click();
    }
    // Sau số lần chẵn (10 lần), menu phải đóng
    await expect(page.locator('.app-header__mobile-nav')).toHaveCount(0);
  });
});
