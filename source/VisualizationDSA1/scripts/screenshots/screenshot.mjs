import { chromium } from 'playwright';
import { mkdirSync, existsSync } from 'fs';

// ============================================================
// UI Screenshot Tool — VisualizationDSA (dùng cho G1/G2)
// Chụp mỗi màn ở 2 theme (dark/light) x 4 breakpoint (375/768/1024/1440)
// Kết quả: docs/screenshots/<BATCH>/<màn>_<theme>_<breakpoint>.png
// ============================================================

// ── CẤU HÌNH ────────────────────────────────────────────────
const BASE = process.env.VDS_BASE ?? 'http://localhost:5173';   // frontend (Docker/nginx — CORS an toàn, có data)
const BATCH = process.env.VDS_BATCH ?? 'G1.1';                  // thư mục con trong docs/screenshots/
const OUT = process.env.VDS_OUT ?? `D:/FPT/og/VisualizationDSA/docs/screenshots/${BATCH}`;
const TOUR_KEY = 'guided_tour_seen';
const THEME_KEY = 'app-theme';                                  // useThemeStore.ts STORAGE_KEY

// Tài khoản demo (DEV_SETUP mục 3)
const DEMO_EMAIL = process.env.VDS_EMAIL ?? 'demo@visualizationdsa.dev';
const DEMO_PASS = process.env.VDS_PASS ?? 'Demo@2024';

// Định nghĩa các màn cần chụp. `auth: true` → login trước khi chụp.
// `waitFor` → selector chờ (optional). `cookies` → inject access token vào localStorage.
const VIEWPORTS = [
  { w: 375, h: 812, label: '375' },
  { w: 768, h: 1024, label: '768' },
  { w: 1024, h: 768, label: '1024' },
  { w: 1440, h: 900, label: '1440' },
];

// ── BƯỚC CHẠY (ghi vào docs/screenshots/README.md) ──────────
// 1. cd VisualizationDSA/frontend && npm run dev -- --port 5174
// 2. cd scripts/screenshots && npm i playwright && npx playwright install chromium
// 3. node screenshot.mjs
//    - Tùy biến: VDS_BATCH=G2 node screenshot.mjs ; VDS_BASE=...
// ============================================================

const browser = await chromium.launch();

function assertCfg(list) {
  if (!Array.isArray(list) || list.length === 0) {
    console.error('❌ SCREENS trống — định nghĩa screens trong script (mảng {name, path, auth, waitFor}).');
    process.exit(1);
  }
}

async function login(page) {
  await page.goto(`${BASE}/login`, { waitUntil: 'networkidle' });
  await page.fill('#auth-email', DEMO_EMAIL);
  await page.fill('#auth-password', DEMO_PASS);
  await Promise.all([
    page.waitForURL((u) => !u.pathname.includes('/login'), { timeout: 20000 }).catch(() => {}),
    page.click('button[type="submit"]'),
  ]);
  await page.waitForTimeout(1500);
  const onLogin = await page.evaluate(() => !location.pathname.includes('/login'));
  if (!onLogin) {
    console.error(`❌ Login thất bại với ${DEMO_EMAIL}`);
    process.exit(1);
  }
}

async function makeContext(theme) {
  const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
  await context.addInitScript((t) => {
    // Tắt guided tour (App.vue tự động chạy nếu chưa từng thấy)
    localStorage.setItem('guided_tour_seen', 'true');
    // Đổi theme: dark (mặc định) hoặc light (key chuẩn useThemeStore)
    if (t === 'light') localStorage.setItem('app-theme', 'light');
    else localStorage.removeItem('app-theme');
  }, theme);
  return context;
}

export async function captureScreens(screens, { only = [] } = {}) {
  assertCfg(screens);
  mkdirSync(OUT, { recursive: true });
  const results = [];

  for (const theme of ['dark', 'light']) {
    for (const s of screens) {
      if (only.length > 0 && !only.includes(s.name)) continue;

      const context = await makeContext(theme);
      const page = await context.newPage();
      page.on('console', (m) => { if (m.type() === 'error') console.log(`[console.error][${s.name}]`, m.text().slice(0, 160)); });

      if (s.auth) await login(page);

      for (const vp of VIEWPORTS) {
        await page.setViewportSize({ width: vp.w, height: vp.h });
        await page.goto(`${BASE}${s.path}`, { waitUntil: 'networkidle' });
        await page.waitForTimeout(s.waitMs ?? 1200);
        if (s.waitFor) {
          await page.waitForSelector(s.waitFor, { timeout: 15000 }).catch(() => {
            console.log(`⚠️  [${s.name}] không thấy selector "${s.waitFor}" (có thể trang lỗi/empty)`);
          });
        }

        // Kiểm tra tràn ngang (layout vỡ): scrollWidth > clientWidth
        const overflow = await page.evaluate(() => {
          const de = document.documentElement;
          return { hOverflow: de.scrollWidth - de.clientWidth, scrollWidth: de.scrollWidth, clientWidth: de.clientWidth };
        });

        const file = `${OUT}/${s.name}_${theme}_${vp.label}.png`;
        await page.screenshot({ path: file, fullPage: true });
        results.push({ screen: s.name, theme, vp: vp.label, overflow: overflow.hOverflow, file });
        console.log(`${s.name} | ${theme} | ${vp.label} | h-overflow=${overflow.hOverflow}px`);
      }

      await page.close();
      await context.close();
    }
  }

  await browser.close();
  return results;
}

// ── SCREENS MẶC ĐỊNH (G1.1 — các view đã sửa layout) ───────
// Để chụp bộ màn khác: `import { captureScreens } from './screenshot.mjs'` rồi gọi với mảng riêng.
// IDs lấy từ seed G4.1 thật (docker đang chạy) — verify trước khi chụp.
const G1_1_SCREENS = [
  { name: 'landing', path: '/', auth: false, waitMs: 2000 },
  { name: 'algorithm-library', path: '/algorithms', auth: false, waitMs: 1500 },
  { name: 'courses', path: '/courses', auth: false, waitMs: 1500 },
  { name: 'course-detail', path: '/courses/135e3de0-d250-491a-a998-cb25696ce04e', auth: true, waitMs: 1500 },
  { name: 'lesson-study', path: '/lessons/158e9002-1a43-40ab-aadb-c26ce7c81d8a', auth: true, waitMs: 2000 },
  { name: 'graph', path: '/graph', auth: false, waitMs: 1500 },
  { name: 'dashboard', path: '/dashboard', auth: true, waitMs: 1500 },
  { name: 'profile', path: '/profile', auth: true, waitMs: 1500 },
  { name: 'docs', path: '/docs/intro', auth: false, waitMs: 1500 },
  { name: 'premium-checkout', path: '/checkout', auth: false, waitMs: 1500 },
  { name: 'my-classrooms', path: '/classrooms', auth: true, waitMs: 1500 },
];

if (import.meta.url === `file:///${process.argv[1]?.replace(/\\/g, '/')}`) {
  const r = await captureScreens(G1_1_SCREENS);
  console.log(`\n✅ Xong ${r.length} ảnh → ${OUT}`);
}
