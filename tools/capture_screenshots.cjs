const { chromium } = require('@playwright/test');
const path = require('path');
const fs = require('fs');

const BASE_URL = 'http://localhost:5174';
const OUT_DIR = path.resolve(__dirname, '../tailieu/screenshots');

if (!fs.existsSync(OUT_DIR)) {
  fs.mkdirSync(OUT_DIR, { recursive: true });
}

const SAMPLE_IDS = {
  courseId: '054aea3b-0971-419e-846b-7fdbe5e55766',
  classId: '2052c098-bd7f-40e0-a769-b6cc3d2eeec8',
  lessonId: 'c6e0dd79-9521-4fdc-ac8b-fc0d93d68918',
  quizId: '705a9360-7559-4d78-a0b6-ac1f97b292d3'
};

async function run() {
  console.log('=== STARTING 23 REAL DATA SCREENSHOT CAPTURE ===');
  const browser = await chromium.launch({
    headless: true,
    args: ['--disable-dev-shm-usage', '--no-sandbox']
  });

  const results = [];

  async function createPage(role = 'guest') {
    const context = await browser.newContext({
      viewport: { width: 1440, height: 900 },
      colorScheme: 'dark',
      deviceScaleFactor: 1
    });

    const page = context.newPage();
    const p = await page;

    // Set dark theme in localStorage
    await p.addInitScript(() => {
      try {
        localStorage.setItem('dsa.theme', 'dark');
        document.documentElement.classList.add('dark');
      } catch (e) {}
    });

    let currentToken = '';

    // Route intercept ONLY for POST /auth/refresh as permitted in prompt §3.1
    await p.route('**/api/v1/auth/refresh', async (route) => {
      if (currentToken) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            accessToken: currentToken,
            expiresIn: 3600
          })
        });
      } else {
        await route.continue();
      }
    });

    // Track console 404/500 errors
    p.on('response', (res) => {
      const status = res.status();
      const url = res.url();
      if ((status === 404 || status >= 500) && url.includes('/api/')) {
        console.warn(`[API ERROR ${status}] ${url}`);
      }
    });

    if (role === 'student') {
      console.log('Logging in as Student (Lê Quốc Bảo)...');
      await p.goto(`${BASE_URL}/login`);
      await p.fill('input[type="email"], input[name="email"]', 'baolqse1801@fpt.edu.vn');
      await p.fill('input[type="password"], input[name="password"]', 'RealData@2024');
      await p.click('button[type="submit"]');
      await p.waitForTimeout(2000);
      currentToken = await p.evaluate(() => {
        try {
          const pinia = window.__pinia || window.pinia;
          const auth = pinia ? pinia.state.value.auth : null;
          return auth ? auth.accessToken : '';
        } catch (e) {
          return '';
        }
      });
    } else if (role === 'admin') {
      console.log('Logging in as Admin (Nguyễn Văn Hùng)...');
      await p.goto(`${BASE_URL}/login`);
      await p.fill('input[type="email"], input[name="email"]', 'hungnv@fpt.edu.vn');
      await p.fill('input[type="password"], input[name="password"]', 'RealData@2024');
      await p.click('button[type="submit"]');
      await p.waitForTimeout(2000);
      currentToken = await p.evaluate(() => {
        try {
          const pinia = window.__pinia || window.pinia;
          const auth = pinia ? pinia.state.value.auth : null;
          return auth ? auth.accessToken : '';
        } catch (e) {
          return '';
        }
      });
    }

    return { page: p, context };
  }

  async function snap(page, urlPath, filename, options = {}) {
    const fullUrl = `${BASE_URL}${urlPath}`;
    console.log(`\n[Capturing] ${filename} -> ${fullUrl}`);
    await page.goto(fullUrl, { waitUntil: 'networkidle', timeout: 30000 });
    await page.waitForTimeout(options.waitMs || 2500);

    if (options.actions) {
      await options.actions(page);
      await page.waitForTimeout(1500);
    }

    if (options.scroll) {
      await page.evaluate((selector) => {
        if (selector) {
          const el = document.querySelector(selector);
          if (el) el.scrollIntoView({ behavior: 'instant', block: 'start' });
        } else {
          window.scrollTo(0, document.body.scrollHeight / 2);
        }
      }, options.scrollSelector);
      await page.waitForTimeout(1500);
    }

    const filePath = path.join(OUT_DIR, filename);
    await page.screenshot({ path: filePath, fullPage: false });

    const stats = fs.statSync(filePath);
    const sizeKb = Math.round(stats.size / 1024);
    const pass = sizeKb >= 20;
    console.log(`[Saved] ${filename} (${sizeKb} KB) - ${pass ? 'PASS' : 'WARN: SMALL'}`);
    results.push({ filename, urlPath, sizeKb, pass });
  }

  try {
    // ── PUBLIC SESSION ──
    console.log('\n--- 1. Public Session ---');
    const pub = await createPage('guest');
    await snap(pub.page, '/', '01_landing.png');
    await snap(pub.page, '/login', '02_login.png');
    await snap(pub.page, '/register', '03_register.png');
    await pub.context.close();

    // ── STUDENT SESSION (Lê Quốc Bảo) ──
    console.log('\n--- 2. Student Session ---');
    const stu = await createPage('student');
    await snap(stu.page, '/path', '05_lo_trinh.png');
    await snap(stu.page, `/path/${SAMPLE_IDS.courseId}`, '06_lo_trinh_detail.png');
    await snap(stu.page, `/path/${SAMPLE_IDS.courseId}`, '07_node_hub.png', {
      scroll: true,
      scrollSelector: '.course-detail__modules, .course-detail__curriculum, [class*="module"], [class*="lesson"]'
    });
    await snap(stu.page, `/lessons/${SAMPLE_IDS.lessonId}`, '14_lesson_detail.png');
    await snap(stu.page, '/simulations', '08_mo_phong.png');
    await snap(stu.page, '/simulator/sort.bubble', '09_mo_phong_detail.png', {
      actions: async (p) => {
        try {
          const stepBtn = p.locator('button:has-text("Bước tới"), button[aria-label*="next"], button:has-text("Next")').first();
          if (await stepBtn.isVisible()) {
            await stepBtn.click();
            await p.waitForTimeout(600);
            await stepBtn.click();
          }
        } catch (e) {}
      }
    });
    await snap(stu.page, '/profile', '04_dashboard.png');
    await snap(stu.page, '/profile', '13_ho_so.png', {
      scroll: true,
      scrollSelector: '.profile-view__section, [class*="achieve"], [class*="inventory"], [class*="badge"]'
    });
    await snap(stu.page, '/classes', '10_lop_hoc.png');
    await snap(stu.page, `/classes/${SAMPLE_IDS.classId}`, '11_lop_hoc_detail.png');
    await snap(stu.page, '/leaderboard', '12_bang_xep_hang.png');
    await snap(stu.page, `/exercise/${SAMPLE_IDS.quizId}`, '15_exercise.png');
    await snap(stu.page, `/ladder/${SAMPLE_IDS.lessonId}`, '16_ladder.png');
    await snap(stu.page, `/ladder/${SAMPLE_IDS.lessonId}/lab`, '17_lab.png');
    await snap(stu.page, '/code/sort.bubble', '18_code_runner.png');
    await snap(stu.page, '/benchmark/sort.bubble/sort.quick', '19_benchmark.png');
    await stu.context.close();

    // ── ADMIN SESSION (Nguyễn Văn Hùng) ──
    console.log('\n--- 3. Admin Session ---');
    const adm = await createPage('admin');
    await snap(adm.page, '/admin/stats', '20_admin_dashboard.png');
    await snap(adm.page, '/admin/users', '21_admin_users.png');
    await snap(adm.page, '/admin/content', '22_admin_content.png');
    await snap(adm.page, '/admin/settings', '23_admin_settings.png');
    await adm.context.close();

  } catch (err) {
    console.error('ERROR during capture:', err);
  } finally {
    await browser.close();
  }

  console.log('\n=== CAPTURE SUMMARY ===');
  console.table(results);
  const allPass = results.length === 23 && results.every(r => r.pass);
  console.log(allPass ? '>>> ALL 23 SCREENSHOTS CAPTURED SUCCESSFULLY (PASS) <<<' : '>>> SOME SCREENSHOTS FAILED <<<');
}

run();
