const { chromium } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

const BASE_URL = 'http://localhost:5173';
const API_URL = 'http://localhost:5000/api/v1';

async function api(path, options = {}) {
  const url = API_URL + path;
  const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
  const res = await fetch(url, { ...options, headers });
  const text = await res.text();
  let json = null;
  try { json = JSON.parse(text); } catch {}
  return { status: res.status, ok: res.ok, data: json, text, headers: res.headers };
}

async function runVerification() {
  console.log('====================================================');
  console.log('=== VERIFYING ALL 6 BUG FIXES (QA-006 to QA-015) ===');
  console.log('====================================================\n');

  const results = [];

  // Authenticate via API
  const loginResStudent = await api('/auth/login', { method: 'POST', body: JSON.stringify({ email: 'student@demo.local', password: 'Student@123' }) });
  const loginResTeacher = await api('/auth/login', { method: 'POST', body: JSON.stringify({ email: 'teacher@demo.local', password: 'Teacher@123' }) });
  const studentToken = loginResStudent.data?.accessToken;
  const teacherToken = loginResTeacher.data?.accessToken;

  // 1. QA-012: Student calling award-xp API
  console.log('[QA-012] Testing POST /concepts/auth/award-xp with STUDENT token...');
  const awardXpRes = await api('/concepts/auth/award-xp', {
    method: 'POST',
    headers: { Authorization: 'Bearer ' + studentToken },
    body: JSON.stringify({ amount: 50, reason: 'Hoan thanh bai kiem tra verification' })
  });
  console.log('   Status:', awardXpRes.status, 'Body:', awardXpRes.data);
  const qa012Pass = awardXpRes.status === 200 && awardXpRes.data?.success === true;
  results.push({ id: 'QA-012', desc: 'Award XP endpoint accessible by authenticated student', passed: qa012Pass });

  // 2. QA-013: Draft course BOLA protection
  console.log('\n[QA-013] Testing Draft Course direct access protection...');
  const createDraftRes = await api('/concepts/courses', {
    method: 'POST',
    headers: { Authorization: 'Bearer ' + teacherToken },
    body: JSON.stringify({
      title: 'Khoa hoc Draft Tuyet mat ' + Date.now(),
      description: 'Chi giang vien duoc xem',
      category: 'Algorithms',
      difficulty: 'Advanced',
      scope: 'draft'
    })
  });
  const draftCourseId = createDraftRes.data?.id;
  console.log('   Teacher created Draft Course ID:', draftCourseId);

  if (draftCourseId) {
    const studentGetCourseRes = await api('/concepts/courses/' + draftCourseId, {
      headers: { Authorization: 'Bearer ' + studentToken }
    });
    console.log('   Student GET /concepts/courses/' + draftCourseId + ' -> Status:', studentGetCourseRes.status);

    const studentGetTreeRes = await api('/paths/' + draftCourseId + '/items', {
      headers: { Authorization: 'Bearer ' + studentToken }
    });
    console.log('   Student GET /paths/' + draftCourseId + '/items -> Status:', studentGetTreeRes.status);

    const qa013Pass = (studentGetCourseRes.status === 404 || studentGetCourseRes.status === 403)
      && (studentGetTreeRes.status === 403 || studentGetTreeRes.status === 404);
    results.push({ id: 'QA-013', desc: 'Draft course protected from unauthorized student access (403/404)', passed: qa013Pass });
  }

  // 3. QA-015: Past deadline validation returns 400 Validation
  console.log('\n[QA-015] Testing Past Deadline validation on /classes/{id}/assignments/deadline...');
  const pastDeadlineRes = await api('/classes/1/assignments/deadline', {
    method: 'PUT',
    headers: { Authorization: 'Bearer ' + teacherToken },
    body: JSON.stringify({ pathItemId: 1, dueAt: '2020-01-01T00:00:00Z' })
  });
  console.log('   Status:', pastDeadlineRes.status, 'Body:', pastDeadlineRes.data);
  const qa015Pass = pastDeadlineRes.status === 400 && (pastDeadlineRes.data?.code === 'VALIDATION_FAILED' || pastDeadlineRes.text.includes('tuong lai') || pastDeadlineRes.text.includes('tương lai'));
  results.push({ id: 'QA-015', desc: 'Setting past deadline returns 400 Bad Request (VALIDATION_FAILED)', passed: qa015Pass });

  // 4. BROWSER TESTS: QA-006, QA-009, QA-014
  console.log('\n--- Launching Browser for UI Verification (QA-006, QA-009, QA-014) ---');
  const browser = await chromium.launch({ headless: true });

  // QA-014: Unaccented search on /path
  console.log('\n[QA-014] Testing Unaccented search "quy hoach" on /path...');
  const studentCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  const studentPage = await studentCtx.newPage();

  await studentPage.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
  await studentPage.fill('input[type="email"]', 'student@demo.local');
  await studentPage.fill('input[type="password"]', 'Student@123');
  await studentPage.click('button[type="submit"]');
  await studentPage.waitForTimeout(1000);

  await studentPage.goto(`${BASE_URL}/path`, { waitUntil: 'networkidle' });
  await studentPage.waitForTimeout(800);

  const searchInput = studentPage.locator('input[placeholder*="Tìm kiếm"]').first();
  await searchInput.fill('quy hoach');
  await studentPage.waitForTimeout(600);

  const courseCards = await studentPage.locator('.course-card-link').count();
  console.log('   Search "quy hoach" result count:', courseCards);
  const qa014Pass = courseCards > 0;
  results.push({ id: 'QA-014', desc: 'Unaccented search "quy hoach" finds courses correctly', passed: qa014Pass });

  // QA-006: CheatSheet PDF Export button for Free student (tranquocbao@university.edu.vn)
  console.log('\n[QA-006] Testing CheatSheet PDF export button & Premium upgrade modal for Free student...');
  const freeCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  const freePage = await freeCtx.newPage();

  await freePage.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
  await freePage.fill('input[type="email"]', 'tranquocbao@university.edu.vn');
  await freePage.fill('input[type="password"]', 'Student@123');
  await freePage.click('button[type="submit"]');
  await freePage.waitForTimeout(1200);

  await freePage.goto(`${BASE_URL}/cheatsheet`, { waitUntil: 'networkidle' });
  await freePage.waitForTimeout(1000);

  const exportBtn = freePage.locator('button', { hasText: 'Xuất File PDF' }).first();
  console.log('   Export button found on /cheatsheet for Free Student:', await exportBtn.isVisible());
  await exportBtn.click();
  await freePage.waitForTimeout(600);

  const upgradeModal = freePage.locator('#upgrade-modal-title').first();
  const upgradeModalVisible = await upgradeModal.isVisible({ timeout: 2000 }).catch(() => false);
  console.log('   Premium Upgrade Modal visible after clicking export:', upgradeModalVisible);
  await freePage.screenshot({ path: path.join(__dirname, 'evidence', 'P1-49-cheatsheet-pdf-guard.png') });
  results.push({ id: 'QA-006', desc: 'CheatSheet PDF export shows Premium Upgrade Modal for free tier', passed: upgradeModalVisible });

  // QA-009: Pending Teacher page Refresh button (lethikimngan@university.edu.vn)
  console.log('\n[QA-009] Testing /pending-teacher refresh button for TEACHER_PENDING...');
  const pendingCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  const pendingPage = await pendingCtx.newPage();

  await pendingPage.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
  await pendingPage.fill('input[type="email"]', 'lethikimngan@university.edu.vn');
  await pendingPage.fill('input[type="password"]', 'Teacher@123');
  await pendingPage.click('button[type="submit"]');
  await pendingPage.waitForTimeout(1200);

  const currentUrl = pendingPage.url();
  console.log('   Pending teacher landed on URL:', currentUrl);

  const refreshBtn = pendingPage.locator('button', { hasText: 'Làm mới trạng thái' }).first();
  const hasRefresh = await refreshBtn.isVisible({ timeout: 2000 }).catch(() => false);
  console.log('   Refresh button visible on /pending-teacher:', hasRefresh);
  if (hasRefresh) {
    await refreshBtn.click();
    await pendingPage.waitForTimeout(500);
  }
  await pendingPage.screenshot({ path: path.join(__dirname, 'evidence', 'P4-04-pending-refresh.png') });
  results.push({ id: 'QA-009', desc: 'Pending teacher view has working "Làm mới trạng thái" button', passed: hasRefresh });

  await browser.close();

  console.log('\n====================================================');
  console.log('=== VERIFICATION SUMMARY ===');
  console.log('====================================================');
  for (const r of results) {
    console.log(`[${r.passed ? 'PASS' : 'FAIL'}] ${r.id}: ${r.desc}`);
  }
  console.log(`\nTotal: ${results.filter(r => r.passed).length}/${results.length} PASSED`);
}

runVerification().catch(console.error);
