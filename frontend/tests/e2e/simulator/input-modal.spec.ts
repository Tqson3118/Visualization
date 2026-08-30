/**
 * input-modal.spec.ts — TC-SIM-030 đến TC-SIM-032
 * Test suite cho Input Modal và cấu hình tham số đầu vào.
 */
import { expect, test } from '@playwright/test';
import { mockApi } from '../helpers/mockApi';

async function waitForSimulatorReady(page: import('@playwright/test').Page) {
  const spinner = page.locator('.animate-spin');
  if (await spinner.isVisible({ timeout: 500 }).catch(() => false)) {
    await expect(spinner).not.toBeVisible({ timeout: 10000 });
  }
  const ready = page.locator('.control-bar__slider, .vcr-scrubber, .simulator-workspace, canvas').first();
  await expect(ready).toBeVisible({ timeout: 10000 });
}

test.describe('TC-SIM-030~032: Input Modal & Custom Parameters', () => {
  test.beforeEach(async ({ page }) => {
    await mockApi(page, { authenticated: true });
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-030: Mở và submit Input Modal
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-030: Mở modal, nhập mảng mới và submit → re-render simulator', async ({ page }) => {
    await page.goto('/simulator/sort.bubble');
    await waitForSimulatorReady(page);

    // Click nút "Cấu hình đầu vào"
    const configBtn = page.getByRole('button', {
      name: /cấu hình|tùy chỉnh|dữ liệu|input/i,
    });
    await expect(configBtn).toBeVisible({ timeout: 5000 });
    await configBtn.click();

    // Modal phải mở
    const modal = page.locator('.input-modal, [class*="modal"]');
    await expect(modal.first()).toBeVisible({ timeout: 3000 });

    // Tìm input nhập mảng
    const arrayInput = modal.locator('input#inp-values, input[placeholder*="phân cách"], input[id*="inp-"]').first();
    await expect(arrayInput).toBeVisible();

    // Nhập array "5, 3, 8, 1, 9, 2"
    await arrayInput.fill('5, 3, 8, 1, 9, 2');

    // Submit form (nút "Chạy" hoặc "Áp dụng")
    const submitBtn = modal.locator('button[type="submit"]');
    await expect(submitBtn).toBeVisible();
    await submitBtn.click();

    // Modal phải đóng
    await expect(page.locator('.input-modal')).not.toBeVisible({ timeout: 3000 });

    // Scrubber phải reset về 0 (bắt đầu lại từ đầu)
    const scrubber = page.locator('.control-bar__slider, .vcr-scrubber, input[type="range"]').first();
    await expect(scrubber).toHaveValue('0');
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-031: Input validation trong modal
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-031: Nhập giá trị không hợp lệ → hiển thị lỗi validation', async ({ page }) => {
    await page.goto('/simulator/graph.bfs');
    await waitForSimulatorReady(page);

    // Mở Input Modal
    const configBtn = page.getByRole('button', {
      name: /cấu hình|tùy chỉnh|dữ liệu|input/i,
    });
    await expect(configBtn).toBeVisible({ timeout: 5000 });
    await configBtn.click();

    const modal = page.locator('.input-modal');
    await expect(modal).toBeVisible();

    // Tìm input số (ví dụ: source vertex hoặc vertices)
    const numInput = modal.locator('input[type="number"], input#inp-vertices, input#inp-source').first();
    if (await numInput.isVisible({ timeout: 2000 }).catch(() => false)) {
      // Nhập giá trị vượt max hoặc âm
      await numInput.fill('-10');

      const submitBtn = modal.locator('button[type="submit"]');
      await submitBtn.click();

      // Phải có message lỗi
      const errorMsg = modal.locator('.input-modal__error, .input-modal__form-error, [role="alert"]');
      await expect(errorMsg.first()).toBeVisible({ timeout: 3000 });
    }
  });

  // ────────────────────────────────────────────────────────────
  // TC-SIM-032: Checkbox binding trong modal (fix m15)
  // ────────────────────────────────────────────────────────────
  test('TC-SIM-032: Checkbox directed/undirected trong modal toggle đúng state', async ({ page }) => {
    await page.goto('/simulator/graph.dfs');
    await waitForSimulatorReady(page);

    // Mở Input Modal
    const configBtn = page.getByRole('button', {
      name: /cấu hình|tùy chỉnh|dữ liệu|input/i,
    });
    await expect(configBtn).toBeVisible({ timeout: 5000 });
    await configBtn.click();

    const modal = page.locator('.input-modal');
    await expect(modal).toBeVisible();

    // Checkbox directed bên trong modal
    const checkbox = modal.locator('input#inp-directed, input[type="checkbox"]').first();
    await expect(checkbox).toBeVisible();

    // Lấy state ban đầu
    const initialChecked = await checkbox.isChecked();

    // Click toggle checkbox
    await checkbox.click();
    expect(await checkbox.isChecked()).toBe(!initialChecked);

    // Click lại lần nữa
    await checkbox.click();
    expect(await checkbox.isChecked()).toBe(initialChecked);

    // Toggle và submit
    await checkbox.click();
    const finalChecked = await checkbox.isChecked();

    const submitBtn = modal.locator('button[type="submit"]');
    await submitBtn.click();

    // Modal đóng thành công
    await expect(page.locator('.input-modal')).not.toBeVisible({ timeout: 3000 });

    // Mở lại modal để kiểm tra state được lưu
    await configBtn.click();
    const reopenedModal = page.locator('.input-modal');
    await expect(reopenedModal).toBeVisible();
    const reopenedCheckbox = reopenedModal.locator('input#inp-directed, input[type="checkbox"]').first();
    await expect(reopenedCheckbox).toBeVisible();
    expect(await reopenedCheckbox.isChecked()).toBe(finalChecked);
  });
});
