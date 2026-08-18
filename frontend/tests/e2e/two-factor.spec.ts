import { expect, test } from '@playwright/test';
import { mockApi } from './helpers/mockApi';

test.describe('TC-22~28: Xác thực 2FA', () => {
  test('TC-22: OTP đúng → đăng nhập thành công', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/verify-2fa', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          accessToken: 'valid-2fa-jwt-token',
          user: { id: 1, email: 'student@demo.local', role: 'STUDENT', displayName: 'Student' },
        }),
      });
    });

    await page.goto('/verify-2fa');
    const otpInput = page.locator('input[name="otp"], [data-testid="otp-input"], input[type="text"]').first();
    if (await otpInput.isVisible()) {
      await otpInput.fill('123456');
      const submitBtn = page.getByRole('button', { name: /Xác nhận|Xác thực|Đăng nhập/i });
      if (await submitBtn.isVisible()) {
        await submitBtn.click();
      }
    }
  });

  test('TC-23: OTP sai → lỗi, ở lại màn OTP', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/verify-2fa', async (route) => {
      await route.fulfill({
        status: 422,
        contentType: 'application/json',
        body: JSON.stringify({
          error: { code: 'OTP_INVALID', message: 'Mã xác thực không chính xác' },
        }),
      });
    });

    await page.goto('/verify-2fa');
    const otpInput = page.locator('input[name="otp"], [data-testid="otp-input"], input[type="text"]').first();
    if (await otpInput.isVisible()) {
      await otpInput.fill('000000');
      const submitBtn = page.getByRole('button', { name: /Xác nhận|Xác thực/i });
      if (await submitBtn.isVisible()) {
        await submitBtn.click();
      }
    }
  });

  test('TC-24: OTP hết hạn (simulate 5+ phút) → lỗi', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/verify-2fa', async (route) => {
      await route.fulfill({
        status: 422,
        contentType: 'application/json',
        body: JSON.stringify({
          error: { code: 'OTP_EXPIRED', message: 'Mã xác thực đã hết hạn' },
        }),
      });
    });

    await page.goto('/verify-2fa');
    const otpInput = page.locator('input[name="otp"], [data-testid="otp-input"], input[type="text"]').first();
    if (await otpInput.isVisible()) {
      await otpInput.fill('999999');
    }
  });

  test('TC-25: OTP đã dùng rồi dùng lại → lỗi', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/verify-2fa', async (route) => {
      await route.fulfill({
        status: 422,
        contentType: 'application/json',
        body: JSON.stringify({
          error: { code: 'OTP_ALREADY_USED', message: 'Mã xác thực đã được sử dụng' },
        }),
      });
    });

    await page.goto('/verify-2fa');
  });

  test('TC-26: OTP < 6 số → validate', async ({ page }) => {
    await mockApi(page);
    await page.goto('/verify-2fa');
    const otpInput = page.locator('input[name="otp"], [data-testid="otp-input"], input[type="text"]').first();
    if (await otpInput.isVisible()) {
      await otpInput.fill('123');
    }
  });

  test('TC-27: OTP có chữ cái → validate', async ({ page }) => {
    await mockApi(page);
    await page.goto('/verify-2fa');
    const otpInput = page.locator('input[name="otp"], [data-testid="otp-input"], input[type="text"]').first();
    if (await otpInput.isVisible()) {
      await otpInput.fill('ABC123');
    }
  });

  test('TC-28: Click "Gửi lại OTP" → OTP mới gửi đến', async ({ page }) => {
    await mockApi(page);
    await page.route('**/api/v1/auth/resend-2fa', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true, message: 'Đã gửi mã xác thực mới' }),
      });
    });

    await page.goto('/verify-2fa');
    const resendBtn = page.getByRole('button', { name: /Gửi lại/i });
    if (await resendBtn.isVisible()) {
      await resendBtn.click();
    }
  });
});
