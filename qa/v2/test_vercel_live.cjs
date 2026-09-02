const { chromium } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

const TARGET_URL = 'https://frontend-eta-ashen-89.vercel.app';

async function testVercelFrontend() {
  console.log('====================================================');
  console.log(`=== TESTING LIVE VERCEL FRONTEND: ${TARGET_URL} ===`);
  console.log('====================================================\n');

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1280, height: 800 },
    userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36'
  });

  const page = await context.newPage();
  const consoleErrors = [];

  page.on('console', msg => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  });

  const results = [];

  // 1. Home Page (/)
  console.log('1. Testing Home Page (/) ...');
  const homeRes = await page.goto(`${TARGET_URL}/`, { waitUntil: 'networkidle', timeout: 30000 });
  const homeStatus = homeRes ? homeRes.status() : 0;
  const homeTitle = await page.title();
  const headerVisible = await page.locator('header.app-header').isVisible().catch(() => false);
  console.log(`   Status: ${homeStatus} | Title: "${homeTitle}" | Header: ${headerVisible ? 'OK' : 'MISSING'}`);
  await page.screenshot({ path: path.join(__dirname, 'evidence', 'vercel-live-home.png') });
  results.push({ page: 'Home (/)', status: homeStatus, ok: homeStatus === 200 && headerVisible });

  // 2. Path List Page (/path)
  console.log('\n2. Testing Courses List (/path) ...');
  const pathRes = await page.goto(`${TARGET_URL}/path`, { waitUntil: 'networkidle', timeout: 30000 });
  const pathStatus = pathRes ? pathRes.status() : 0;
  await page.waitForTimeout(1000);
  const courseCardsCount = await page.locator('.course-card-link').count();
  console.log(`   Status: ${pathStatus} | Course Cards found: ${courseCardsCount}`);
  await page.screenshot({ path: path.join(__dirname, 'evidence', 'vercel-live-path.png') });
  results.push({ page: 'Courses (/path)', status: pathStatus, ok: pathStatus === 200 && courseCardsCount > 0 });

  // 3. Simulations Catalog (/simulations)
  console.log('\n3. Testing Simulations Catalog (/simulations) ...');
  const simRes = await page.goto(`${TARGET_URL}/simulations`, { waitUntil: 'networkidle', timeout: 30000 });
  const simStatus = simRes ? simRes.status() : 0;
  await page.waitForTimeout(1000);
  const simCardsCount = await page.locator('.simulations__card').count();
  console.log(`   Status: ${simStatus} | Simulation Cards on page: ${simCardsCount}`);
  await page.screenshot({ path: path.join(__dirname, 'evidence', 'vercel-live-simulations.png') });
  results.push({ page: 'Simulations (/simulations)', status: simStatus, ok: simStatus === 200 && simCardsCount > 0 });

  // 4. Single Simulator Engine (/simulator/sort.bubble)
  console.log('\n4. Testing Simulator Engine (/simulator/sort.bubble) ...');
  const singleSimRes = await page.goto(`${TARGET_URL}/simulator/sort.bubble`, { waitUntil: 'networkidle', timeout: 30000 });
  const singleSimStatus = singleSimRes ? singleSimRes.status() : 0;
  await page.waitForTimeout(1500);
  const canvasOrStage = await page.locator('canvas, svg.simulation-canvas, .simulator-stage, [data-testid="canvas"]').count();
  console.log(`   Status: ${singleSimStatus} | Canvas/Stage elements: ${canvasOrStage}`);
  await page.screenshot({ path: path.join(__dirname, 'evidence', 'vercel-live-simulator-bubble.png') });
  results.push({ page: 'Simulator Engine (/simulator/sort.bubble)', status: singleSimStatus, ok: singleSimStatus === 200 && canvasOrStage > 0 });

  // 5. Login Page (/login)
  console.log('\n5. Testing Login Page (/login) ...');
  const loginRes = await page.goto(`${TARGET_URL}/login`, { waitUntil: 'networkidle', timeout: 30000 });
  const loginStatus = loginRes ? loginRes.status() : 0;
  const loginFormVisible = await page.locator('input[type="email"]').isVisible().catch(() => false);
  console.log(`   Status: ${loginStatus} | Email Input: ${loginFormVisible ? 'OK' : 'MISSING'}`);
  await page.screenshot({ path: path.join(__dirname, 'evidence', 'vercel-live-login.png') });
  results.push({ page: 'Login (/login)', status: loginStatus, ok: loginStatus === 200 && loginFormVisible });

  // 6. Register Page (/register)
  console.log('\n6. Testing Register Page (/register) ...');
  const registerRes = await page.goto(`${TARGET_URL}/register`, { waitUntil: 'networkidle', timeout: 30000 });
  const registerStatus = registerRes ? registerRes.status() : 0;
  const registerFormVisible = await page.locator('input[type="email"], input[name="email"]').isVisible().catch(() => false);
  console.log(`   Status: ${registerStatus} | Register Form: ${registerFormVisible ? 'OK' : 'MISSING'}`);
  await page.screenshot({ path: path.join(__dirname, 'evidence', 'vercel-live-register.png') });
  results.push({ page: 'Register (/register)', status: registerStatus, ok: registerStatus === 200 && registerFormVisible });

  console.log('\n====================================================');
  console.log('=== VERCEL LIVE TEST SUMMARY ===');
  console.log('====================================================');
  for (const r of results) {
    console.log(`[${r.ok ? 'PASS' : 'FAIL'}] ${r.page} -> HTTP ${r.status}`);
  }
  console.log(`\nTotal: ${results.filter(r => r.ok).length}/${results.length} PASSED`);

  if (consoleErrors.length > 0) {
    console.log('\n--- Console Warnings / Errors ---');
    consoleErrors.slice(0, 5).forEach(e => console.log('   !', e));
  } else {
    console.log('\n✓ Zero console errors detected on live deployment.');
  }

  await browser.close();
}

testVercelFrontend().catch(console.error);
