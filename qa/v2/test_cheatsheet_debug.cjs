const { chromium } = require('@playwright/test');

async function testCheatSheet() {
  const browser = await chromium.launch({ headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  await ctx.addInitScript(() => {
    window.localStorage.setItem('auth', JSON.stringify({
      user: { id: 1, email: 'student@demo.local', role: 'STUDENT', isPremium: false },
      accessToken: 'dummy-token',
      isAuthenticated: true
    }));
  });
  const page = await ctx.newPage();
  await page.goto('http://localhost:5173/cheatsheet', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  
  const buttons = await page.locator('button').allInnerTexts();
  console.log('Buttons found:', buttons);
  
  const printBtn = page.locator('button', { hasText: 'Xuất File PDF' }).first();
  console.log('Print button visible:', await printBtn.isVisible());
  await printBtn.click();
  await page.waitForTimeout(600);
  
  const modalText = await page.locator('#upgrade-modal-title').innerText().catch(err => 'Err: ' + err.message);
  console.log('Modal title:', modalText);
  
  await browser.close();
}

testCheatSheet().catch(console.error);
