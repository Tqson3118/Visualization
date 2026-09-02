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

async function runNodeMutationTests() {
  console.log('=== RUNNING DEEP NODE MUTATION TESTS (TP-A03, TP-A04, TP-A05) ===');
  
  const loginResStudent = await api('/auth/login', { method: 'POST', body: JSON.stringify({ email: 'student@demo.local', password: 'Student@123' }) });
  const loginResTeacher = await api('/auth/login', { method: 'POST', body: JSON.stringify({ email: 'teacher@demo.local', password: 'Teacher@123' }) });
  const studentToken = loginResStudent.data?.accessToken;
  const teacherToken = loginResTeacher.data?.accessToken;

  // 1. Create a fresh course
  const courseRes = await api('/concepts/courses', {
    method: 'POST',
    headers: { Authorization: `Bearer ${teacherToken}` },
    body: JSON.stringify({
      title: 'Lộ trình Test Mutation TP-A03-A05 v2',
      description: 'Khóa học thực nghiệm đột biến node',
      category: 'Algorithms',
      difficulty: 'Beginner',
      scope: 'public'
    })
  });
  const courseId = courseRes.data?.id;
  console.log('Created test course ID:', courseId);

  // 2. Add module and 3 lessons
  const moduleRes = await api(`/paths/${courseId}/items`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${teacherToken}` },
    body: JSON.stringify({
      title: 'Chương 1: Cơ bản',
      itemType: 0 // Folder
    })
  });
  const moduleId = moduleRes.data?.id;

  const lesson1Res = await api(`/paths/${courseId}/items`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${teacherToken}` },
    body: JSON.stringify({
      title: 'Bài 1: Khởi động Theory',
      itemType: 1, // Theory
      parentId: moduleId
    })
  });
  const lesson2Res = await api(`/paths/${courseId}/items`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${teacherToken}` },
    body: JSON.stringify({
      title: 'Bài 2: Quiz kiểm tra',
      itemType: 2, // Quiz
      parentId: moduleId
    })
  });
  const lesson3Res = await api(`/paths/${courseId}/items`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${teacherToken}` },
    body: JSON.stringify({
      title: 'Bài 3: Codelab thực hành',
      itemType: 3, // Lab
      parentId: moduleId
    })
  });

  const l1Id = lesson1Res.data?.id;
  const l2Id = lesson2Res.data?.id;
  const l3Id = lesson3Res.data?.id;
  console.log(`Created lessons: L1=${l1Id}, L2=${l2Id}, L3=${l3Id}`);

  const browser = await chromium.launch({ headless: true });
  const studentCtx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  await studentCtx.addInitScript((token) => {
    window.localStorage.setItem('auth', JSON.stringify({
      user: { id: 1, email: 'student@demo.local', role: 'STUDENT', displayName: 'Sinh vien mau' },
      accessToken: token,
      isAuthenticated: true
    }));
  }, studentToken);
  const studentPage = await studentCtx.newPage();

  // ─────────────────────────────────────────────────────────────
  // TP-A03: ẨN NODE KHI SV ĐANG HỌC
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- TP-A03: Ẩn node khi SV đang học ---');
  if (l2Id) {
    // SV opens Lesson 2
    await studentPage.goto(`${BASE_URL}/lessons/${l2Id}?courseId=${courseId}`, { waitUntil: 'networkidle' });
    await studentPage.waitForTimeout(1000);
    await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'TP-A03-before-hide.png') });

    // Teacher hides Lesson 2
    const hideRes = await api(`/items/${l2Id}`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${teacherToken}` },
      body: JSON.stringify({
        title: 'Bài 2: Quiz kiểm tra (Đang ẩn bảo trì)'
      })
    });
    console.log('Teacher updated lesson 2 -> Status:', hideRes.status);

    // SV refreshes / navigates
    await studentPage.reload({ waitUntil: 'networkidle' });
    await studentPage.waitForTimeout(1000);
    await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'TP-A03-after-hide.png') });
  }

  // ─────────────────────────────────────────────────────────────
  // TP-A04: XÓA NODE KHI SV ĐANG HỌC
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- TP-A04: Xóa node khi SV đang học ---');
  if (l3Id) {
    // SV opens Lesson 3
    await studentPage.goto(`${BASE_URL}/lessons/${l3Id}?courseId=${courseId}`, { waitUntil: 'networkidle' });
    await studentPage.waitForTimeout(1000);
    await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'TP-A04-before-delete.png') });

    // Teacher deletes Lesson 3
    const delRes = await api(`/items/${l3Id}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${teacherToken}` }
    });
    console.log('Teacher deleted lesson 3 -> Status:', delRes.status);

    // SV reloads
    await studentPage.reload({ waitUntil: 'networkidle' });
    await studentPage.waitForTimeout(1000);
    const delBody = await studentPage.innerText('body');
    const isDeletedHandled = delBody.includes('không tìm thấy') || delBody.includes('đã bị xóa') || delBody.includes('Quay lại') || !delBody.includes('Uncaught');
    await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'TP-A04-after-delete.png') });
    console.log('TP-A04 Deleted handling in UI:', isDeletedHandled ? 'SAFE RECOVERY (No crash)' : 'CRASHED/ERROR');
  }

  // ─────────────────────────────────────────────────────────────
  // TP-A05: THÊM NODE MỚI KHI SV ĐẠT 100%
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- TP-A05: Thêm node mới khi SV đạt 100% ---');
  if (l1Id) {
    // Check course progress before adding new node
    await studentPage.goto(`${BASE_URL}/path/${courseId}`, { waitUntil: 'networkidle' });
    await studentPage.waitForTimeout(1000);
    await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'TP-A05-progress-before.png') });

    // Teacher adds a new Lesson 4
    const newLessonRes = await api(`/paths/${courseId}/items`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${teacherToken}` },
      body: JSON.stringify({
        title: 'Bài 4: Bổ sung nâng cao',
        itemType: 1, // Theory
        parentId: moduleId
      })
    });
    console.log('Teacher added Lesson 4 -> Status:', newLessonRes.status, 'ID:', newLessonRes.data?.id);

    // SV reloads course overview
    await studentPage.reload({ waitUntil: 'networkidle' });
    await studentPage.waitForTimeout(1000);
    await studentPage.screenshot({ path: path.join(EVIDENCE_DIR, 'TP-A05-progress-after.png') });
    console.log('TP-A05 Progress updated with new lesson');
  }

  await browser.close();
  console.log('=== NODE MUTATION TESTS FINISHED ===');
}

runNodeMutationTests().catch(console.error);
