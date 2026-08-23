// GP-T9b — chụp lại 12 màn light + dark sau khi sửa (1366x768) → docs/work/r2-fixed-<stt>-<man>[-dark].png
import { createRequire } from 'module';
const require = createRequire('D:/FPT/neww/frontend/package.json');
const { chromium } = require('playwright');

import { mkdirSync, writeFileSync } from 'fs';

const BASE = 'http://localhost:5174';
const OUT = 'D:/FPT/neww/docs/work';
const EMAIL = 'student@demo.local';
const PASS = 'Student@123';
const VIEWPORT = { width: 1366, height: 768 };

const SCREENS = [
  { stt: '01', man: 'home',       path: '/',                       auth: false, waitMs: 3000 },
  { stt: '02', man: 'login',      path: '/login',                  auth: false, waitMs: 1500 },
  { stt: '03', man: 'lesson',     path: '/learn/1',                auth: true,  waitMs: 2500 },
  { stt: '04', man: 'simulator',  path: '/simulator/sort.bubble',  auth: true,  waitMs: 3500 },
  { stt: '05', man: 'exercise',   path: '/exercise/1',             auth: true,  waitMs: 2500 },
  { stt: '06', man: 'path',       path: '/path',                   auth: true,  waitMs: 2500 },
  { stt: '07', man: 'ladder',     path: '/ladder/1',               auth: true,  waitMs: 2500 },
  { stt: '08', man: 'lab',        path: '/ladder/1/lab',           auth: true,  waitMs: 2500 },
  { stt: '09', man: 'code',       path: '/code/sort.bubble',       auth: true,  waitMs: 3000 },
  { stt: '10', man: 'benchmark',  path: '/benchmark/sort.bubble/sort.quick', auth: true, waitMs: 3500 },
  { stt: '11', man: 'leaderboard',path: '/leaderboard',            auth: true,  waitMs: 2500 },
  { stt: '12', man: 'profile',    path: '/profile',                auth: true,  waitMs: 2500 },
];

mkdirSync(OUT, { recursive: true });
const browser = await chromium.launch();
const results = [];

for (const theme of ['light', 'dark']) {
  const context = await browser.newContext({ viewport: VIEWPORT });
  await context.addInitScript((t) => {
    if (t !== 'dark') return;
    const apply = () => document.documentElement.classList.add('dark');
    if (document.documentElement) apply();
    else document.addEventListener('DOMContentLoaded', apply);
  }, theme);

  let loggedIn = false;

  for (const s of SCREENS) {
    const page = await context.newPage();
    const consoleErrors = [];
    page.on('console', (m) => {
      if (m.type() !== 'error') return;
      if (m.text().includes('401 (Unauthorized)')) return;
      consoleErrors.push(m.text().slice(0, 200));
    });
    page.on('pageerror', (e) => consoleErrors.push(`PAGEERROR: ${String(e).slice(0, 200)}`));
    if (s.auth && !loggedIn) {
      await page.goto(`${BASE}/login`, { waitUntil: 'networkidle' });
      await page.fill('#email', EMAIL);
      await page.fill('#password', PASS);
      await Promise.all([
        page.waitForURL((u) => !u.pathname.includes('/login'), { timeout: 20000 }).catch(() => {}),
        page.click('button[type="submit"]'),
      ]);
      await page.waitForTimeout(2000);
      const onLogin = await page.evaluate(() => !location.pathname.includes('/login'));
      if (!onLogin) {
        console.error(`❌ Login thất bại (${theme}) — dừng theme này`);
        break;
      }
      loggedIn = true;
      console.log(`[login ${theme}] OK → ${await page.evaluate(() => location.pathname)}`);
    }

    const file = `${OUT}/r2-fixed-${s.stt}-${s.man}${theme === 'dark' ? '-dark' : ''}.png`;
    await page.goto(`${BASE}${s.path}`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(s.waitMs);

    const overflow = await page.evaluate(() => ({
      h: document.documentElement.scrollWidth - document.documentElement.clientWidth,
      bodyH: document.body.scrollWidth - document.body.clientWidth,
    }));

    await page.screenshot({ path: file });
    const entry = { stt: s.stt, man: s.man, theme, file, overflow, consoleErrors };
    results.push(entry);
    console.log(`${s.stt}-${s.man} | ${theme} | h-overflow=${overflow.h}/${overflow.bodyH}px | consoleErr=${consoleErrors.length} | ${file}`);
    await page.close();
  }
  await context.close();
}

await browser.close();
writeFileSync(`${OUT}/r2-fixed-screenshot-results.json`, JSON.stringify(results, null, 2));
console.log(`\n✅ Xong ${results.length} ảnh → ${OUT}/r2-fixed-*.png`);
