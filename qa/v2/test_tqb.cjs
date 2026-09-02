const { chromium } = require('@playwright/test');

async function testTQB() {
  const browser = await chromium.launch({ headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  const page = await ctx.newPage();

  await page.goto('http://localhost:5173/login', { waitUntil: 'networkidle' });
  await page.fill('input[type="email"]', 'tranquocbao@university.edu.vn');
  await page.fill('input[type="password"]', 'Student@123');
  await page.click('button[type="submit"]');
  await page.waitForTimeout(1200);

  await page.goto('http://localhost:5173/cheatsheet', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);

  const authState = await page.evaluate(() => {
    return {
      auth: JSON.parse(localStorage.getItem('auth') || '{}'),
      isProBadge: document.querySelector('.app-header')?.innerText
    };
  });
  console.log('Auth state in browser:', authState);

  const printBtn = page.locator('button', { hasText: 'Xuất File PDF' }).first();
  console.log('Print button text:', await printBtn.innerText().catch(e => e.message));
  await printBtn.click();
  await page.waitForTimeout(600);

  const modal = page.locator('#upgrade-modal-title, :has-text("Mở khóa Xuất PDF CheatSheet")').first();
  console.log('Modal visible:', await modal.isVisible().catch(e => e.message));

  await browser.close();
}

testTQB().catch(console.error);
