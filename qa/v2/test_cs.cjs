const { chromium } = require('@playwright/test');

async function testCheatSheetModal() {
  const browser = await chromium.launch({ headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  const page = await ctx.newPage();

  await page.goto('http://localhost:5173/login', { waitUntil: 'networkidle' });
  await page.fill('input[type="email"]', 'student@demo.local');
  await page.fill('input[type="password"]', 'Student@123');
  await page.click('button[type="submit"]');
  await page.waitForTimeout(1000);

  await page.goto('http://localhost:5173/cheatsheet', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);

  const buttonTexts = await page.locator('button').allInnerTexts();
  console.log('All buttons:', buttonTexts);

  // Expose mock window.print
  await page.evaluate(() => {
    window.printCalled = false;
    window.print = () => { window.printCalled = true; };
  });

  const btn = page.locator('button', { hasText: 'Xuất File PDF' }).first();
  await btn.click();
  await page.waitForTimeout(600);

  const printCalled = await page.evaluate(() => window.printCalled);
  console.log('Was window.print called?', printCalled);

  const bodyHtml = await page.innerHTML('body');
  const hasModal = bodyHtml.includes('Mở khóa Xuất PDF CheatSheet') || bodyHtml.includes('upgrade-modal-title');
  console.log('Has Upgrade Modal in DOM?', hasModal);

  await browser.close();
}

testCheatSheetModal().catch(console.error);
