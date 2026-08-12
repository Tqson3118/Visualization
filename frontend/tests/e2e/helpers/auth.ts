/**
 * Helper đăng nhập qua UI cho E2E.
 *
 * Lý do KHÔNG seed token trực tiếp: token auth chỉ nằm trong memory Pinia (ADR-004)
 * — mỗi page.goto() tạo phiên mới, store rỗng. Đăng nhập qua UI trong CÙNG phiên là
 * cách duy nhất giữ trạng thái auth xuyên suốt test (không reload trang sau login).
 */
import { expect, type Page } from '@playwright/test';

import { mockApi } from './mockApi';

export const E2E_EMAIL = 'e2e@test.edu';
export const E2E_PASSWORD = 'E2e@12345';

/**
 * Mock API + mở /login + điền form + submit.
 * Mặc định chờ kết thúc ở /path (LoginView redirect mặc định — SDD §3.4).
 */
export async function loginViaUi(page: Page, email = E2E_EMAIL, password = E2E_PASSWORD): Promise<void> {
  await mockApi(page);
  await page.goto('/login');
  await page.locator('#email').fill(email);
  await page.locator('#password').fill(password);
  await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
  await expect(page).toHaveURL(/\/path$/);
}
