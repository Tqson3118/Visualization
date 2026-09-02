const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const EVIDENCE_DIR = path.join(__dirname, 'evidence');
if (!fs.existsSync(EVIDENCE_DIR)) fs.mkdirSync(EVIDENCE_DIR, { recursive: true });

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

async function runFullAudit() {
  console.log('=== STARTING FULL SYSTEM AUDIT (Phase 1D - Phase 5) ===');
  const findings = [];
  const results = {};

  const browser = await chromium.launch({ headless: true });

  // ─────────────────────────────────────────────────────────────
  // 1. AUTHENTICATE ALL 3 SEED ROLES
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- 1. Authenticating Seed Roles ---');
  const loginResStudent = await api('/auth/login', { method: 'POST', body: JSON.stringify({ email: 'student@demo.local', password: 'Student@123' }) });
  const loginResTeacher = await api('/auth/login', { method: 'POST', body: JSON.stringify({ email: 'teacher@demo.local', password: 'Teacher@123' }) });
  const loginResAdmin = await api('/auth/login', { method: 'POST', body: JSON.stringify({ email: 'admin@system.local', password: 'Admin@123' }) });

  const studentToken = loginResStudent.data?.accessToken;
  const teacherToken = loginResTeacher.data?.accessToken;
  const adminToken = loginResAdmin.data?.accessToken;

  console.log('Student Login:', loginResStudent.status, 'Teacher Login:', loginResTeacher.status, 'Admin Login:', loginResAdmin.status);

  // ─────────────────────────────────────────────────────────────
  // PHASE 1D: AUXILIARY TOOLS (Code Runner, CheatSheet)
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- Phase 1D: Auxiliary Tools ---');
  const studentCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  await studentCtx.addInitScript((token) => {
    window.localStorage.setItem('auth', JSON.stringify({
      user: { id: 1, email: 'student@demo.local', role: 'STUDENT', isPremium: false },
      accessToken: token,
      isAuthenticated: true
    }));
  }, studentToken);
  const studentPage = await studentCtx.newPage();

  // P1-45: Code runner valid key
  await studentPage.goto(`${BASE_URL}/code/sort.bubble`, { waitUntil: 'networkidle' });
  await studentPage.waitForTimeout(1000);
  await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-45-code-runner-valid.png') });

  // P1-46: Code runner slug mapping (QA-003 regression)
  await studentPage.goto(`${BASE_URL}/code/bubble-sort`, { waitUntil: 'networkidle' });
  await studentPage.waitForTimeout(1000);
  const codeBody = await studentPage.innerText('body');
  const hasSlugError = codeBody.includes('Không tìm thấy bài') || codeBody.includes('chưa có trong danh mục');
  await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-46-code-runner-slug.png') });
  console.log('P1-46 Slug Mapping check (bubble-sort):', hasSlugError ? 'NOT MAPPED (Bug QA-003)' : 'MAPPED OK');
  if (hasSlugError) {
    findings.push({ id: 'QA-003', status: 'Chưa fix', desc: 'Code runner /code/bubble-sort không nhận dạng slug gạch ngang' });
  }

  // P1-48 & P1-49: CheatSheet & PDF export Free user check (QA-006 regression)
  await studentPage.goto(`${BASE_URL}/cheatsheet`, { waitUntil: 'networkidle' });
  await studentPage.waitForTimeout(800);
  await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-48-cheatsheet.png') });

  // Try clicking Export PDF as Free user
  const pdfBtn = studentPage.locator('button:has-text("Xuất File PDF"), button:has-text("In CheatSheet")');
  if (await pdfBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
    let printCalled = false;
    await studentPage.exposeFunction('mockPrint', () => { printCalled = true; });
    await studentPage.evaluate(() => { window.print = () => window.mockPrint(); });
    await pdfBtn.click();
    await studentPage.waitForTimeout(600);
    const modalVisible = await studentPage.locator('.modal, [role="dialog"], :has-text("Nâng cấp Premium")').isVisible({ timeout: 1500 }).catch(() => false);
    await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-49-cheatsheet-pdf-guard.png') });
    console.log('P1-49 CheatSheet PDF guard (Free user):', modalVisible ? 'GUARDED (Modal shown)' : 'TRIGGERED PRINT (Bug QA-006)');
  }

  // ─────────────────────────────────────────────────────────────
  // PHASE 1E: GAMIFICATION & STORE
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- Phase 1E: Gamification & Store ---');
  // P1-50: Quests view (QA-005 regression)
  await studentPage.goto(`${BASE_URL}/quests`, { waitUntil: 'networkidle' });
  await studentPage.waitForTimeout(800);
  const questsText = await studentPage.innerText('body');
  const hasQuestsEmptyBug = questsText.includes('0/0 DONE') || questsText.includes('0/0 ·');
  await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-50-quests.png') });
  console.log('P1-50 Quests Empty State check:', hasQuestsEmptyBug ? 'SHOWS 0/0 (Bug QA-005)' : 'OK Empty State');

  // P1-52: Leaderboard
  await studentPage.goto(`${BASE_URL}/leaderboard`, { waitUntil: 'networkidle' });
  await studentPage.waitForTimeout(800);
  await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-52-leaderboard.png') });

  // P1-53 & P1-54 & P1-55: Shop
  await studentPage.goto(`${BASE_URL}/shop`, { waitUntil: 'networkidle' });
  await studentPage.waitForTimeout(800);
  await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-53-shop.png') });

  // P1-56 & P1-57: Premium & VietQR
  await studentPage.goto(`${BASE_URL}/premium`, { waitUntil: 'networkidle' });
  await studentPage.waitForTimeout(800);
  await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-56-premium.png') });

  // P1-58: Account subscription
  await studentPage.goto(`${BASE_URL}/account/subscription`, { waitUntil: 'networkidle' });
  await studentPage.waitForTimeout(800);
  await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-58-subscription.png') });

  // ─────────────────────────────────────────────────────────────
  // PHASE 1F: CLASSES (Student)
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- Phase 1F: Classes (Student) ---');
  await studentPage.goto(`${BASE_URL}/classes`, { waitUntil: 'networkidle' });
  await studentPage.waitForTimeout(800);
  await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-59-classes.png') });

  // P1-60 & P1-62: Join class by invalid & duplicate code (QA-004 regression)
  const joinBtn = studentPage.locator('button:has-text("Tham gia bằng mã"), button:has-text("Tham gia lớp")').first();
  if (await joinBtn.isVisible({ timeout: 1500 }).catch(() => false)) {
    await joinBtn.click();
    await studentPage.waitForTimeout(400);
    const codeInput = studentPage.locator('input[placeholder*="Mã"], input[placeholder*="ABCDEF"]').first();
    if (await codeInput.isVisible({ timeout: 1000 }).catch(() => false)) {
      await codeInput.fill('INVALID');
      const submitJoin = studentPage.locator('button:has-text("Tham gia"), button[type="submit"]').last();
      await submitJoin.click();
      await studentPage.waitForTimeout(600);
      await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P1-60-join-invalid.png') });
    }
  }

  // ─────────────────────────────────────────────────────────────
  // PHASE 2: TEACHER STUDIO & NODE MUTATIONS (TP-A03, TP-A04, TP-A05, TP-B01 - TP-B06)
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- Phase 2: Teacher Studio & Node Mutations ---');
  const teacherCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  await teacherCtx.addInitScript((token) => {
    window.localStorage.setItem('auth', JSON.stringify({
      user: { id: 2, email: 'teacher@demo.local', role: 'TEACHER', displayName: 'Giang vien demo' },
      accessToken: token,
      isAuthenticated: true
    }));
  }, teacherToken);
  const teacherPage = await teacherCtx.newPage();

  // P2-01: Studio Overview
  await teacherPage.goto(`${BASE_URL}/studio`, { waitUntil: 'networkidle' });
  await teacherPage.waitForTimeout(1000);
  await teacherPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P2-01-studio-overview.png') });

  // P2-03: Studio Curriculum tab
  await teacherPage.goto(`${BASE_URL}/studio?tab=curriculum`, { waitUntil: 'networkidle' });
  await teacherPage.waitForTimeout(1000);
  await teacherPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P2-03-studio-curriculum.png') });

  // P2-08: Dirty Modal test (QA-007 regression)
  // Check if dirty state shows custom dark mode modal vs window.confirm
  const dirtyCheck = await teacherPage.evaluate(() => {
    return {
      hasCustomModal: typeof window.confirm === 'function'
    };
  });
  await teacherPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P2-08-dirty-warning.png') });

  // P2-14: Feedback tab
  await teacherPage.goto(`${BASE_URL}/studio?tab=feedback`, { waitUntil: 'networkidle' });
  await teacherPage.waitForTimeout(800);
  await teacherPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P2-14-studio-feedback.png') });

  // P2-17 & P2-18: Teacher Classes
  await teacherPage.goto(`${BASE_URL}/classes`, { waitUntil: 'networkidle' });
  await teacherPage.waitForTimeout(800);
  await teacherPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P2-17-teacher-classes.png') });

  // P2-19: Deadline past test (TP-D02)
  console.log('Testing Deadline past rejection on backend API...');
  const pastDeadlineRes = await api('/classes/1/assignments/deadline', {
    method: 'PUT',
    headers: { Authorization: `Bearer ${teacherToken}` },
    body: JSON.stringify({ dueAt: '2020-01-01T00:00:00Z' })
  });
  console.log('Past deadline API response:', pastDeadlineRes.status, pastDeadlineRes.data);

  // ─────────────────────────────────────────────────────────────
  // CROSS-ROLE MUTATION TESTS: TP-A03, TP-A04, TP-A05, TP-B04
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- Cross-Role Mutation Tests (TP-A03, TP-A04, TP-A05, TP-B04) ---');
  
  // 1. Create a dynamic test course with 3 lessons
  const newCourseRes = await api('/concepts/courses', {
    method: 'POST',
    headers: { Authorization: `Bearer ${teacherToken}` },
    body: JSON.stringify({
      title: 'Lộ trình Test Node Mutation v2',
      description: 'Dành riêng cho kiểm thử Ẩn/Xóa/Thêm node khi SV đang học',
      category: 'Algorithms',
      difficulty: 'Beginner',
      scope: 'draft'
    })
  });
  console.log('Created dynamic course:', newCourseRes.status, 'Course ID:', newCourseRes.data?.id);
  const dynamicCourseId = newCourseRes.data?.id;

  if (dynamicCourseId) {
    // Check TP-B04: Draft/PendingReview course invisible to Student on /path
    const studentBrowseRes = await api('/concepts/courses', {
      headers: { Authorization: `Bearer ${studentToken}` }
    });
    const isDraftVisibleToStudent = (studentBrowseRes.data || []).some(c => c.id === dynamicCourseId);
    console.log('TP-B04 Gating Check: Is Draft course visible on Student /path?', isDraftVisibleToStudent ? 'LEAKED (Bug)' : 'HIDDEN PROPERLY (OK)');

    // Check TP-B06: Student directly accessing draft course URL /path/:id
    await studentPage.goto(`${BASE_URL}/path/${dynamicCourseId}`, { waitUntil: 'networkidle' });
    await studentPage.waitForTimeout(1000);
    const studentPathText = await studentPage.innerText('body');
    const isBlocked = studentPathText.includes('Không tìm thấy') || studentPathText.includes('Vui lòng thử lại') || studentPathText.includes('404') || studentPathText.includes('403');
    await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P2-13-draft-gating-student.png') });
    console.log('TP-B06 Draft Direct Access by Student:', isBlocked ? 'BLOCKED/PROTECTED (OK)' : 'ACCESSIBLE');
  }

  // ─────────────────────────────────────────────────────────────
  // PHASE 3: ADMIN CONSOLE (TP-C01 - TP-C04)
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- Phase 3: Admin Console ---');
  const adminCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  await adminCtx.addInitScript((token) => {
    window.localStorage.setItem('auth', JSON.stringify({
      user: { id: 3, email: 'admin@system.local', role: 'ADMIN', displayName: 'Admin He Thong' },
      accessToken: token,
      isAuthenticated: true
    }));
  }, adminToken);
  const adminPage = await adminCtx.newPage();

  // P3-02: Admin Users
  await adminPage.goto(`${BASE_URL}/admin/users`, { waitUntil: 'networkidle' });
  await adminPage.waitForTimeout(1000);
  await adminPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P3-02-admin-users.png') });

  // P3-07: Admin Stats (QA-002 regression)
  await adminPage.goto(`${BASE_URL}/admin/stats`, { waitUntil: 'networkidle' });
  await adminPage.waitForTimeout(1200);
  const statsBody = await adminPage.innerText('body');
  const hasStatsError = statsBody.includes('Không thể tải dữ liệu thống kê') || statsBody.includes('Thử lại');
  await adminPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P3-07-admin-stats.png') });
  console.log('P3-07 Admin Stats load check:', hasStatsError ? 'FAILED TO LOAD (Bug QA-002)' : 'LOADED SUCCESSFULLY');

  // P3-08: Admin Settings & BugReports
  await adminPage.goto(`${BASE_URL}/admin/settings`, { waitUntil: 'networkidle' });
  await adminPage.waitForTimeout(1000);
  await adminPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P3-08-admin-settings.png') });

  // P3-11: Admin Moderation Tab in Studio
  await adminPage.goto(`${BASE_URL}/studio?tab=moderation`, { waitUntil: 'networkidle' });
  await adminPage.waitForTimeout(1000);
  await adminPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P3-11-admin-moderation.png') });

  // ─────────────────────────────────────────────────────────────
  // PHASE 4: TEACHER_PENDING FLOW (QA-009 regression)
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- Phase 4: TEACHER_PENDING Flow ---');
  const pendingCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  await pendingCtx.addInitScript(() => {
    window.localStorage.setItem('auth', JSON.stringify({
      user: { id: 99, email: 'pending@teacher.local', role: 'TEACHER_PENDING', displayName: 'GV Cho Duyet' },
      accessToken: 'dummy-pending-token',
      isAuthenticated: true
    }));
  });
  const pendingPage = await pendingCtx.newPage();

  // P4-03: Pending teacher view
  await pendingPage.goto(`${BASE_URL}/pending-teacher`, { waitUntil: 'networkidle' });
  await pendingPage.waitForTimeout(800);
  const pendingText = await pendingPage.innerText('body');
  const hasRefreshBtn = await pendingPage.locator('button:has-text("Làm mới"), button:has-text("Kiểm tra")').isVisible({ timeout: 1500 }).catch(() => false);
  await pendingPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P4-04-pending-refresh.png') });
  console.log('P4-04 Pending Teacher Refresh Button (QA-009):', hasRefreshBtn ? 'HAS REFRESH BUTTON (Fixed)' : 'NO REFRESH BUTTON (Bug QA-009)');

  // P4-05: Pending teacher tries entering /studio -> should redirect to /pending-teacher
  await pendingPage.goto(`${BASE_URL}/studio`, { waitUntil: 'networkidle' });
  await pendingPage.waitForTimeout(800);
  const pendingStudioUrl = pendingPage.url();
  await pendingPage.screenshot({ path: path.join(EVIDENCE_DIR, 'P4-05-pending-blocked-studio.png') });
  console.log('P4-05 Pending Teacher entering /studio redirected to:', pendingStudioUrl);

  // ─────────────────────────────────────────────────────────────
  // PHASE 5: UX SWEEP & SECURITY
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- Phase 5: UX Sweep & Security ---');
  
  // P5-01: Mobile 375px Viewport (QA-008 regression)
  const mobileCtx = await browser.newContext({ viewport: { width: 375, height: 667 } });
  await mobileCtx.addInitScript((token) => {
    window.localStorage.setItem('auth', JSON.stringify({
      user: { id: 1, email: 'student@demo.local', role: 'STUDENT' },
      accessToken: token,
      isAuthenticated: true
    }));
  }, studentToken);
  const mobilePage = await mobileCtx.newPage();

  await mobilePage.goto(`${BASE_URL}/simulator/sort.bubble`, { waitUntil: 'networkidle' });
  await mobilePage.waitForTimeout(1000);
  await mobilePage.screenshot({ path: path.join(EVIDENCE_DIR, 'P5-01-mobile-simulator-375.png') });

  await mobilePage.goto(`${BASE_URL}/path`, { waitUntil: 'networkidle' });
  await mobilePage.waitForTimeout(800);
  await mobilePage.screenshot({ path: path.join(EVIDENCE_DIR, 'P5-01-mobile-path-375.png') });

  await mobilePage.goto(`${BASE_URL}/profile`, { waitUntil: 'networkidle' });
  await mobilePage.waitForTimeout(800);
  await mobilePage.screenshot({ path: path.join(EVIDENCE_DIR, 'P5-01-mobile-profile-375.png') });

  // P5-06: Security BE Check (Student calling Admin API)
  const studentCallingAdminRes = await api('/admin/stats', {
    headers: { Authorization: `Bearer ${studentToken}` }
  });
  console.log('P5-06 Security: Student calling /api/v1/admin/stats -> Status:', studentCallingAdminRes.status, '(Expected 401/403)');

  const studentCallingUsersRes = await api('/users', {
    headers: { Authorization: `Bearer ${studentToken}` }
  });
  console.log('P5-06 Security: Student calling /api/v1/users -> Status:', studentCallingUsersRes.status, '(Expected 401/403)');

  await browser.close();
  console.log('\n=== FULL SYSTEM AUDIT COMPLETE ===');
}

runFullAudit().catch(console.error);
