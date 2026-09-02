const { chromium } = require('../../frontend/node_modules/playwright');
const path = require('path');
const fs = require('fs');

const BASE_URL = 'http://localhost:5173';
const EVIDENCE_DIR = path.resolve(__dirname, '../evidence');

if (!fs.existsSync(EVIDENCE_DIR)) {
  fs.mkdirSync(EVIDENCE_DIR, { recursive: true });
}

async function createBrowserContext(options = {}) {
  const browser = await chromium.launch({
    headless: true,
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });

  const context = await browser.newContext({
    viewport: options.viewport || { width: 1440, height: 900 },
    ...options
  });

  const page = await context.newPage();
  const consoleLogs = [];
  const networkErrors = [];

  page.on('console', (msg) => {
    const text = msg.text();
    const type = msg.type();
    consoleLogs.push({ type, text });
    if (type === 'error') {
      console.log(`[Browser Console Error] ${text}`);
    }
  });

  page.on('response', (res) => {
    const status = res.status();
    if (status >= 400) {
      networkErrors.push({
        url: res.url(),
        status,
        statusText: res.statusText()
      });
    }
  });

  page.on('pageerror', (err) => {
    consoleLogs.push({ type: 'pageerror', text: err.message });
    console.log(`[Browser Uncaught Exception] ${err.message}`);
  });

  return { browser, context, page, consoleLogs, networkErrors };
}

async function loginAs(page, email, password) {
  // First clear cookies and storage
  await page.context().clearCookies();
  await page.goto(`${BASE_URL}/`);
  await page.evaluate(() => {
    try {
      localStorage.clear();
      sessionStorage.clear();
    } catch (e) {}
  });

  await page.goto(`${BASE_URL}/login`);
  await page.waitForLoadState('networkidle');

  // Fill credentials
  const emailSelector = 'input[type="email"], input[autocomplete="email"], input[placeholder*="email" i]';
  const passSelector = 'input[type="password"], input[autocomplete="current-password"]';

  await page.waitForSelector(emailSelector, { timeout: 10000 });
  await page.fill(emailSelector, email);
  await page.fill(passSelector, password);
  
  await Promise.all([
    page.waitForNavigation({ timeout: 10000 }).catch(() => {}),
    page.click('button[type="submit"]')
  ]);

  await page.waitForTimeout(1000);
}

async function logout(page) {
  await page.context().clearCookies();
  await page.goto(`${BASE_URL}/`);
  await page.evaluate(() => {
    try {
      localStorage.clear();
      sessionStorage.clear();
    } catch (e) {}
  });
  await page.reload();
  await page.waitForLoadState('networkidle');
}

async function takeEvidence(page, name) {
  const filePath = path.join(EVIDENCE_DIR, `${name}.png`);
  await page.screenshot({ path: filePath, fullPage: false });
  console.log(`[Evidence Saved] ${filePath}`);
  return filePath;
}

module.exports = {
  BASE_URL,
  EVIDENCE_DIR,
  createBrowserContext,
  loginAs,
  logout,
  takeEvidence
};
