import { expect, type Page } from '@playwright/test';

/** Chờ toast message xuất hiện */
export async function waitForToast(page: Page, text?: string) {
  const toast = page.locator('[role="status"], [role="alert"], .toast, [class*="toast"], [data-sonner-toast]').first();
  await expect(toast).toBeVisible({ timeout: 5000 });
  if (text) {
    await expect(toast).toContainText(text);
  }
  return toast;
}

/** Switch tab và verify URL query */
export async function switchToTab(page: Page, tabName: string | RegExp, tabQuery: string) {
  const tabBtn = page.getByRole('tab', { name: tabName }).first();
  await tabBtn.click();
  await expect(page).toHaveURL(new RegExp(`tab=${tabQuery}`));
}

/** Verify không có window.prompt bị gọi (TC-TF-041 quan trọng) */
export async function assertNoWindowPrompt(page: Page, action: () => Promise<void>) {
  let promptCalled = false;
  const dialogHandler = (dialog: any) => {
    if (dialog.type() === 'prompt') {
      promptCalled = true;
      void dialog.dismiss();
    }
  };
  page.on('dialog', dialogHandler);
  try {
    await action();
  } finally {
    page.off('dialog', dialogHandler);
  }
  expect(promptCalled).toBe(false);
}

/** Drag một item đến vị trí khác trong list */
export async function dragToPosition(page: Page, sourceSelector: string, targetSelector: string) {
  const source = page.locator(sourceSelector).first();
  const target = page.locator(targetSelector).first();
  await source.hover();
  await page.mouse.down();
  await page.waitForTimeout(100);
  const targetBox = await target.boundingBox();
  if (targetBox) {
    await page.mouse.move(targetBox.x + targetBox.width / 2, targetBox.y + targetBox.height / 2, { steps: 5 });
    await page.waitForTimeout(100);
    await page.mouse.up();
  }
}
