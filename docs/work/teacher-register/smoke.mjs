// Smoke E2E — Màn Register (Task L) trên backend THẬT local :5001, frontend :5180
// Chạy: cd frontend && node docs/work/teacher-register/smoke.mjs
import { chromium } from 'playwright';
import fs from 'node:fs';
import path from 'node:path';

const BASE = 'http://localhost:5180';
const OUT = path.resolve('docs/work/teacher-register');
const EMAIL = 'gv.smoke.20260813.run5@university.edu.vn';
const PWD = 'Abc@123456';
const ADMIN_EMAIL = 'admin@system.local';
const ADMIN_PWD = 'Admin@123';

fs.mkdirSync(OUT, { recursive: true });
const log = [];
const consoleIssues = [];
let stepCount = 0;

function step(name) {
  stepCount += 1;
  log.push(`\n## Bước ${stepCount}: ${name}`);
}
function ok(msg) {
  log.push(`PASS — ${msg}`);
  console.log(`PASS — ${msg}`);
}
function fail(msg) {
  log.push(`FAIL — ${msg}`);
  console.log(`FAIL — ${msg}`);
  process.exitCode = 1;
}
async function shot(page, name) {
  const p = path.join(OUT, name);
  await page.screenshot({ path: p });
  log.push(`Ảnh: ${name}`);
}
async function overflowCheck(page, label) {
  const r = await page.evaluate(() => ({
    sw: document.body.scrollWidth,
    cw: document.body.clientWidth,
    broken: [...document.images].filter((i) => i.naturalWidth === 0).length,
  }));
  log.push(`[${label}] scrollWidth=${r.sw} clientWidth=${r.cw} → ${r.sw > r.cw ? 'OVERFLOW' : 'no-overflow'}; imgs=${r.broken === 0 ? 'ok' : `broken=${r.broken}`}`);
}
function attachConsole(page, label) {
  page.on('console', (m) => { if (m.type() === 'error') consoleIssues.push(`[${label}] console.error: ${m.text()}`); });
  page.on('pageerror', (e) => consoleIssues.push(`[${label}] pageerror: ${e.message}`));
}

const browser = await chromium.launch();

// ── B1+B2: mở /register, chọn Giảng viên ──
step('Mở /register — chế độ Sinh viên + chuyển Giảng viên');
const ctx1 = await browser.newContext({ viewport: { width: 1280, height: 900 } });
const page = await ctx1.newPage();
attachConsole(page, 'register');
await page.goto(`${BASE}/register`, { waitUntil: 'networkidle' });
await page.waitForSelector('h1:has-text("Đăng ký tài khoản")', { timeout: 15000 });
if ((await page.locator('.register__teacher').count()) === 0) ok('Form gốc hiển thị, form GV ẩn (student mode)');
else fail('Form GV hiển thị ngay ở chế độ mặc định');
await shot(page, 'smoke-01-register-student.png');
await overflowCheck(page, 'register-student');

await page.getByRole('button', { name: 'Giảng viên', exact: true }).click();
await page.waitForSelector('.register__teacher', { timeout: 5000 });
const dep = page.locator('input[placeholder="VD: Khoa Công nghệ thông tin"]');
const code = page.locator('input[placeholder="VD: GV12345"]');
const bio = page.locator('#register-teacher-bio');
if ((await dep.count()) === 1) ok('Field Khoa/Bộ môn hiện (teacher mode)'); else fail('Thiếu field Khoa/Bộ môn');
if ((await code.count()) === 1) ok('Field Mã giảng viên hiện (teacher mode)'); else fail('Thiếu field Mã giảng viên');
if ((await bio.count()) === 1) ok('Field Kinh nghiệm giảng dạy hiện (teacher mode)'); else fail('Thiếu field Kinh nghiệm giảng dạy');
const note = await page.locator('.register__note').innerText();
if (note.includes('Admin xét duyệt')) ok('Ghi chú chờ duyệt hiển thị'); else fail(`Thiếu ghi chú chờ duyệt — "${note}"`);
await shot(page, 'smoke-02-register-teacher.png');
await overflowCheck(page, 'register-teacher');

// ── B3: điền đủ + submit ──
step('Điền form GV đầy đủ + submit');
await page.fill('input[placeholder="Nguyễn Văn A"]', 'GV Smoke Test');
await page.fill('input[placeholder="ban@truong.edu.vn"]', EMAIL);
const pwdInputs = page.locator('input[type="password"]');
await pwdInputs.nth(0).fill(PWD);
await pwdInputs.nth(1).fill(PWD);
await dep.fill('Khoa CNTT');
await code.fill('GV12345');
await bio.fill('5 nam giang day CTDL & GT');
await page.locator('input[type="checkbox"]').check();
await page.getByRole('button', { name: 'Đăng ký', exact: true }).click();
try {
  await page.waitForSelector('.register__pending', { timeout: 15000 });
  ok('Thông báo chờ duyệt hiện');
} catch {
  fail(`Không thấy thông báo chờ duyệt — URL=${page.url()}`);
  const body = await page.locator('body').innerText();
  log.push(body.slice(0, 600));
}
if (page.url().includes('/register')) ok('Không chuyển trang login (vẫn ở /register)');
else fail(`URL sai: ${page.url()}`);
await shot(page, 'smoke-03-register-pending.png');
await overflowCheck(page, 'register-pending');
await ctx1.close();

// ── B4: case thiếu (context MỚI — không có session register) ──
step('Case thiếu: bỏ trống Khoa/Bộ môn + Mã GV');
const ctx2 = await browser.newContext({ viewport: { width: 1280, height: 900 } });
const page2 = await ctx2.newPage();
attachConsole(page2, 'register-missing');
await page2.goto(`${BASE}/register`, { waitUntil: 'networkidle' });
await page2.waitForSelector('h1:has-text("Đăng ký tài khoản")', { timeout: 15000 });
await page2.getByRole('button', { name: 'Giảng viên', exact: true }).click();
await page2.fill('input[placeholder="Nguyễn Văn A"]', 'GV Thieu');
await page2.fill('input[placeholder="ban@truong.edu.vn"]', EMAIL.replace('gv.smoke', 'gv.thieu'));
const p2 = page2.locator('input[type="password"]');
await p2.nth(0).fill(PWD);
await p2.nth(1).fill(PWD);
await page2.fill('#register-teacher-bio', 'bio');
await page2.locator('input[type="checkbox"]').check();
// 1) Submit KHÔNG blur trước → quan sát phản hồi (fieldErrors chỉ hiện khi touched)
await page2.getByRole('button', { name: 'Đăng ký', exact: true }).click();
await page2.waitForTimeout(600);
const errBeforeBlur = await page2.locator('[role="alert"]').allTextContents();
if (errBeforeBlur.length === 0) {
  log.push('GAP-UX: submit với field GV trống (chưa blur) → KHÔNG hiện lỗi inline (validation chỉ chặn API)');
} else {
  log.push(`INFO: submit không blur vẫn hiện lỗi: ${errBeforeBlur.join(' | ')}`);
}
if (page2.url().includes('/register')) ok('Không submit khi thiếu field (vẫn ở /register)');
else fail(`Bị submit dù thiếu — URL=${page2.url()}`);
// 2) Blur 2 field trống (focus trước — Playwright blur() no-op nếu chưa focus) → touched=true → lỗi inline hiện
const depIn = page2.locator('input[placeholder="VD: Khoa Công nghệ thông tin"]');
const codeIn = page2.locator('input[placeholder="VD: GV12345"]');
await depIn.focus(); await depIn.blur();
await codeIn.focus(); await codeIn.blur();
await page2.waitForTimeout(300);
const errText = await page2.locator('.register__teacher [role="alert"]').allTextContents();
if (errText.some((t) => t.includes('Vui lòng nhập Khoa/Bộ môn'))) ok('Lỗi "Vui lòng nhập Khoa/Bộ môn" hiện ở field');
else fail(`Không thấy lỗi Khoa/Bộ môn — errors: ${errText.join(' | ')}`);
if (errText.some((t) => t.includes('Vui lòng nhập Mã giảng viên'))) ok('Lỗi "Vui lòng nhập Mã giảng viên" hiện');
else fail(`Không thấy lỗi Mã GV — errors: ${errText.join(' | ')}`);
if (page2.url().includes('/register')) ok('Không submit khi thiếu field (vẫn ở /register)');
else fail(`Bị submit dù thiếu — URL=${page2.url()}`);
await shot(page2, 'smoke-04-register-missing-department.png');
await ctx2.close();

// ── B5: admin duyệt ──
step('Admin login + duyệt teacher trong modal');
const ctx3 = await browser.newContext({ viewport: { width: 1280, height: 900 } });
const page3 = await ctx3.newPage();
attachConsole(page3, 'admin');
await page3.goto(`${BASE}/login`, { waitUntil: 'networkidle' });
await page3.fill('#email', ADMIN_EMAIL);
await page3.fill('#password', ADMIN_PWD);
await page3.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
await page3.waitForURL('**/path', { timeout: 15000 });
ok('Admin login thành công (redirect /path)');
await page3.goto(`${BASE}/admin/users`, { waitUntil: 'networkidle' });
await page3.getByRole('tab', { name: /Chờ duyệt Teacher/ }).click();
await page3.fill('input[placeholder="Tìm theo tên/email..."]', EMAIL);
await page3.getByRole('button', { name: 'Tìm', exact: true }).click();
await page3.waitForTimeout(1500);
const row = page3.locator('tr', { hasText: EMAIL }).first();
if ((await row.count()) > 0) ok('User smoke xuất hiện trong tab Chờ duyệt');
else fail('Không tìm thấy user smoke trong tab Chờ duyệt');
await row.getByRole('button', { name: 'Duyệt' }).click();
await page3.waitForSelector('text=Thông tin giảng viên', { timeout: 5000 });
const modalText = await page3.locator('body').innerText();
for (const need of ['Khoa/Bộ môn', 'Khoa CNTT', 'Mã giảng viên', 'GV12345', 'Kinh nghiệm giảng dạy', '5 nam giang day CTDL & GT']) {
  if (modalText.includes(need)) ok(`Modal hiển thị: "${need}"`);
  else fail(`Modal THIẾU: "${need}"`);
}
await shot(page3, 'smoke-05-admin-review-modal.png');
await page3.getByRole('button', { name: 'Xác nhận duyệt' }).click();
await page3.waitForTimeout(1500);
if ((await page3.locator('body').innerText()).includes('Đã duyệt giảng viên!')) ok('Toast "Đã duyệt giảng viên!"');
else fail('Không thấy toast duyệt thành công');
await page3.waitForTimeout(800);

// ── B6: logout admin → login GV ──
step('Logout admin → login GV vừa duyệt');
let mePayload = null;
let loginPayload = null;
const httpIssues = [];
page3.on('response', async (resp) => {
  const url = resp.url();
  if (url.includes('/api/v1/auth/me') && resp.status() === 200) {
    mePayload = await resp.json().catch(() => null);
  }
  if (url.includes('/api/v1/auth/login') && resp.status() === 200) {
    loginPayload = await resp.json().catch(() => null);
  }
  if (resp.status() >= 400 && !url.includes('/api/v1/auth/refresh')) {
    httpIssues.push(`[${resp.status()}] ${url}`);
  }
});
await page3.locator('.app-header__user').click();
await page3.getByRole('button', { name: 'Đăng xuất' }).click();
await page3.waitForURL('**/login', { timeout: 10000 });
ok('Logout admin về /login');
await page3.fill('#email', EMAIL);
await page3.fill('#password', PWD);
await page3.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
try {
  await page3.waitForURL('**/path', { timeout: 15000 });
  ok('GV login thành công (redirect /path)');
} catch {
  fail(`GV login thất bại — URL=${page3.url()}`);
  const body = await page3.locator('body').innerText();
  log.push(body.slice(0, 500));
}
await page3.waitForTimeout(1500);
log.push(`/auth/me (app tự gọi) → ${JSON.stringify(mePayload)}`);
const loginUserRole = loginPayload?.user?.role ?? null;
log.push(`/auth/login response user → ${JSON.stringify(loginPayload?.user ?? null)}`);
if (loginUserRole === 'TEACHER') ok('Role = TEACHER (response /auth/login)');
else if (mePayload?.role === 'TEACHER') ok('Role = TEACHER (app /auth/me)');
else fail(`Role không xác định: login.user.role=${loginUserRole} me=${mePayload?.role}`);
log.push(`HTTP >=400 (không tính refresh): ${httpIssues.length === 0 ? 'không có' : httpIssues.join(' ; ')}`);
const adminLink = await page3.locator('.app-header__nav a', { hasText: 'Quản trị' }).count();
if (adminLink === 1) ok('Header hiện link "Quản trị" (đúng quyền TEACHER/ADMIN)');
else fail(`Header link Quản trị: ${adminLink} (mong đợi 1)`);
const headerLabel = await page3.locator('.app-header__user').getAttribute('aria-label');
if (headerLabel === 'GV Smoke Test') ok('Header hiển thị tên GV Smoke Test');
else fail(`Header label: ${headerLabel}`);
await shot(page3, 'smoke-06-teacher-logged-in.png');
await overflowCheck(page3, 'teacher-logged-in');
await ctx3.close();

await browser.close();

log.push('\n## Console issues');
if (consoleIssues.length === 0) log.push('Không có console.error / pageerror.');
else consoleIssues.forEach((i) => log.push(`- ${i}`));
fs.writeFileSync(path.join(OUT, 'smoke-raw.log'), log.join('\n'), 'utf8');
console.log('\n=== KẾT THÚC SMOKE ===');
console.log(log.join('\n'));
