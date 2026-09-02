const { BASE_URL, createBrowserContext, loginAs, logout, takeEvidence } = require('./helpers');
const axios = require('../../frontend/node_modules/axios');

const API_BASE = 'http://localhost:5000/api/v1';

async function runGroupF() {
  console.log('\n======================================================');
  console.log('STARTING TEST GROUP F: RBAC & URL SECURITY AUDIT');
  console.log('======================================================\n');

  const findings = [];
  const { browser, page, consoleLogs, networkErrors } = await createBrowserContext();

  try {
    // ------------------------------------------------------------------------
    // TC-F01: Guest (Unauthenticated) accessing internal URLs
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-F01: Guest URL Access Check ---');
    await logout(page);

    const protectedUrls = ['/studio', '/admin/users', '/admin/settings', '/admin/stats', '/profile', '/classes'];
    for (const url of protectedUrls) {
      await page.goto(`${BASE_URL}${url}`);
      await page.waitForLoadState('networkidle');
      const currentUrl = page.url();
      const isBlocked = currentUrl.includes('/login') || currentUrl.includes('/home') || currentUrl === `${BASE_URL}/`;
      console.log(`[TC-F01 Guest] Access '${url}' -> Current URL: '${currentUrl}' (Blocked: ${isBlocked})`);

      if (!isBlocked) {
        findings.push({
          id: 'QA-F01-ERR',
          severity: 'P0',
          title: `Guest truy cập được màn hình nội bộ ${url}`,
          details: `Khách chưa đăng nhập truy cập URL ${url} không bị chuyển hướng về /login!`
        });
      }
    }
    await takeEvidence(page, 'QA-F01_guest_blocked_to_login');

    // ------------------------------------------------------------------------
    // TC-F02: Student accessing Studio & Admin URLs
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-F02: Student accessing Studio & Admin URLs ---');
    await loginAs(page, 'student@demo.local', 'Student@123');

    const adminUrls = ['/studio', '/teacher', '/admin/users', '/admin/settings', '/admin/stats'];
    for (const url of adminUrls) {
      await page.goto(`${BASE_URL}${url}`);
      await page.waitForLoadState('networkidle');
      const currentUrl = page.url();
      const isBlocked = !currentUrl.includes('/admin/') && !currentUrl.includes('/studio');
      console.log(`[TC-F02 Student] Access '${url}' -> Current URL: '${currentUrl}' (Blocked: ${isBlocked})`);

      if (!isBlocked) {
        findings.push({
          id: 'QA-F02-ERR',
          severity: 'P0',
          title: `Student truy cập được màn hình quản trị ${url}`,
          details: `Học sinh gõ URL ${url} vào trình duyệt nhưng không bị chặn!`
        });
      }
    }
    await takeEvidence(page, 'QA-F02_student_blocked_from_admin');

    // Test direct backend API calls as Student
    const studentToken = await page.evaluate(() => localStorage.getItem('access_token') || sessionStorage.getItem('access_token'));
    if (studentToken) {
      try {
        const res = await axios.get(`${API_BASE}/admin/stats`, {
          headers: { Authorization: `Bearer ${studentToken}` }
        });
        console.log(`[TC-F02 Backend Security] GET /api/v1/admin/stats returned status: ${res.status}`);
        findings.push({
          id: 'QA-F02-API-LEAK',
          severity: 'P0',
          title: 'Backend API /api/v1/admin/stats không kiểm tra Role ADMIN',
          details: 'Student gọi API lấy stats hệ thống nhưng backend trả về HTTP 200!'
        });
      } catch (apiErr) {
        const status = apiErr.response ? apiErr.response.status : 0;
        console.log(`[TC-F02 Backend Security] GET /api/v1/admin/stats as Student correctly rejected with: ${status} ${apiErr.response?.statusText || ''}`);
      }
    }

    // ------------------------------------------------------------------------
    // TC-F03: Teacher accessing Admin-only URLs
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-F03: Teacher accessing Admin Users & Settings ---');
    await loginAs(page, 'teacher@demo.local', 'Teacher@123');

    const adminOnlyUrls = ['/admin/users', '/admin/settings', '/admin/stats'];
    for (const url of adminOnlyUrls) {
      await page.goto(`${BASE_URL}${url}`);
      await page.waitForLoadState('networkidle');
      const currentUrl = page.url();
      const isBlocked = !currentUrl.includes('/admin/users') && !currentUrl.includes('/admin/settings') && !currentUrl.includes('/admin/stats');
      console.log(`[TC-F03 Teacher] Access '${url}' -> Current URL: '${currentUrl}' (Blocked: ${isBlocked})`);

      if (!isBlocked) {
        findings.push({
          id: 'QA-F03-ERR',
          severity: 'P0',
          title: `Teacher truy cập được màn hình Admin ${url}`,
          details: `Giảng viên gõ URL ${url} vào trình duyệt nhưng không bị chặn!`
        });
      }
    }
    await takeEvidence(page, 'QA-F03_teacher_blocked_from_admin');

  } catch (err) {
    console.error('[Error in Group F Execution]:', err);
    findings.push({
      id: 'QA-F-CRASH',
      severity: 'P0',
      title: 'Crash trong Group F',
      details: err.stack || err.message
    });
  } finally {
    await browser.close();
  }

  console.log('\nGroup F Completed with findings:', findings.length);
  return findings;
}

runGroupF();
