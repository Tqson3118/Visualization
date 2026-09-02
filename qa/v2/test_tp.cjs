const { chromium } = require('@playwright/test');

async function testTeacherPending() {
  const browser = await chromium.launch({ headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  const page = await ctx.newPage();

  await page.goto('http://localhost:5173/login', { waitUntil: 'networkidle' });
  await page.fill('input[type="email"]', 'lethikimngan@university.edu.vn');
  await page.fill('input[type="password"]', 'Teacher@123');
  await page.click('button[type="submit"]');
  await page.waitForTimeout(1200);

  console.log('Current URL after login:', page.url());

  const refreshBtn = page.locator('button', { hasText: 'Làm mới trạng thái' }).first();
  console.log('Refresh button visible:', await refreshBtn.isVisible());
  if (await refreshBtn.isVisible()) {
    await refreshBtn.click();
    await page.waitForTimeout(600);
    console.log('Clicked refresh button successfully');
  }

  await browser.close();
}

testTeacherPending().catch(console.error);
