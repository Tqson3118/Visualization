/**
 * capture-r2-screenshots.mjs — Trục 16 ROUND 2: screenshot visual regression 1366/390/768
 * cho 13 màn demo/report → docs/work/r2-<w>-<NN>-<name>.png (chạy với backend THẬT :8081).
 * Chạy: node frontend/scripts/capture-r2-screenshots.mjs (từ frontend/ — cần playwright lib)
 */
import { chromium } from 'playwright';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const OUT = path.resolve(__dirname, '../../docs/work');
const BASE = 'http://localhost:8081';
const VIEWPORTS = [
  { w: 1366, h: 768 },
  { w: 390, h: 844 },
  { w: 768, h: 1024 },
];

const SCREENS = [
  { n: '01-home', url: '/', role: null },
  { n: '02-login', url: '/login', role: null },
  { n: '03-path', url: '/path', role: 'student' },
  { n: '04-lesson', url: '/path/2/node/5', role: 'student' },
  { n: '05-simulator', url: '/simulator/stack.push', role: 'student' },
  { n: '06-exercise', url: '/exercise/12', role: 'student' },
  { n: '07-premium', url: '/premium', role: 'student' },
  { n: '08-profile', url: '/profile', role: 'student' },
  { n: '09-leaderboard', url: '/leaderboard', role: 'student' },
  { n: '10-classes', url: '/classes', role: 'teacher' },
  { n: '13-class-report', url: '/classes/1/report', role: 'teacher' },
  { n: '11-admin', url: '/admin/users', role: 'admin' },
  { n: '12-showcase', url: '/simulator/sort.bubble', role: null },
];

const ACC = {
  student: { email: 'student@demo.local', password: 'Student@123', name: 'Sinh viên mẫu' },
  teacher: { email: 'teacher@demo.local', password: 'Teacher@123', name: 'Giáo viên mẫu' },
  admin: { email: 'admin@system.local', password: 'Admin@123', name: 'Quản trị viên' },
};

async function login(page, role) {
  await page.goto(BASE + '/login');
  await page.locator('#email').fill(ACC[role].email);
  await page.locator('#password').fill(ACC[role].password);
  await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
  await page.waitForURL(/\/path$/, { timeout: 15000 });
}

async function logout(page, role) {
  await page.locator(`button[aria-label="${ACC[role].name}"]`).click();
  await page.getByRole('button', { name: 'Đăng xuất', exact: true }).click();
  await page.waitForURL(/\/login$/, { timeout: 15000 });
}

const browser = await chromium.launch();
const context = await browser.newContext({ viewport: { width: 1366, height: 768 } });
const page = await context.newPage();

let currentRole = null;
for (const s of SCREENS) {
  if (s.role !== currentRole) {
    if (currentRole) await logout(page, currentRole);
    if (s.role) await login(page, s.role);
    currentRole = s.role;
  }
  for (const v of VIEWPORTS) {
    await page.setViewportSize({ width: v.w, height: v.h });
    await page.goto(BASE + s.url, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(2600);
    const out = path.join(OUT, `r2-${v.w}-${s.n}.png`);
    await page.screenshot({ path: out });
    console.log('saved', out);
  }
}
await browser.close();
console.log('ALL DONE');
