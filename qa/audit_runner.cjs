const fs = require('fs');

const BASE_URL = 'http://localhost:5000/api/v1';

async function api(path, options = {}) {
  const url = BASE_URL + path;
  const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
  const res = await fetch(url, { ...options, headers });
  const text = await res.text();
  let json = null;
  try { json = JSON.parse(text); } catch {}
  return { status: res.status, ok: res.ok, data: json, text, headers: res.headers };
}

async function runAudit() {
  console.log('=== STARTING AUTOMATED API & LOGIC AUDIT ===');
  const summary = { passed: 0, failed: 0, findings: [] };

  // 1. Test Seed Logins
  console.log('\n--- 1. Testing Seed Accounts Login ---');
  const accounts = [
    { role: 'ADMIN', email: 'admin@system.local', pass: 'Admin@123' },
    { role: 'TEACHER', email: 'teacher@demo.local', pass: 'Teacher@123' },
    { role: 'STUDENT', email: 'student@demo.local', pass: 'Student@123' },
  ];
  
  const tokens = {};
  for (const acc of accounts) {
    const res = await api('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email: acc.email, password: acc.pass })
    });
    console.log(Login  (): status=, user=, role=);
    if (res.ok) {
      tokens[acc.role] = res.data.accessToken;
      summary.passed++;
    } else {
      summary.failed++;
      summary.findings.push(Cannot login as : );
    }
  }

  // 2. Test Admin Stats Verification (TP-C04)
  console.log('\n--- 2. Testing Admin Stats Data Consistency ---');
  if (tokens.ADMIN) {
    const statsRes = await api('/admin/stats', {
      headers: { Authorization: Bearer  }
    });
    console.log('Admin Stats:', statsRes.status, JSON.stringify(statsRes.data));
    if (statsRes.ok && statsRes.data.totalUsers > 0) {
      summary.passed++;
    } else {
      summary.failed++;
    }
  }

  // 3. Test RBAC: Student accessing Admin/Studio APIs
  console.log('\n--- 3. Testing RBAC Gating on Backend Endpoints ---');
  if (tokens.STUDENT) {
    const authH = { Authorization: Bearer  };
    const checkEndpoints = [
      { name: 'Admin Stats', path: '/admin/stats', method: 'GET' },
      { name: 'Admin Users', path: '/users', method: 'GET' },
      { name: 'Admin BugReports', path: '/admin/bug-reports', method: 'GET' },
      { name: 'Create Topic (Teacher/Admin)', path: '/topics', method: 'POST', body: JSON.stringify({ title: 'Hack' }) }
    ];
    for (const ep of checkEndpoints) {
      const res = await api(ep.path, { method: ep.method, headers: authH, body: ep.body });
      console.log(RBAC Check: Student calling  () -> Status:  (Expected: 401/403));
      if (res.status === 401 || res.status === 403) {
        summary.passed++;
      } else {
        summary.failed++;
        summary.findings.push(RBAC Leak: Student got status  on );
      }
    }
  }

  // 4. Test Course Lifecycle (TP-B01, TP-B04, TP-C01)
  console.log('\n--- 4. Testing Course Lifecycle & State Machine ---');
  if (tokens.TEACHER && tokens.ADMIN) {
    const teacherH = { Authorization: Bearer  };
    const adminH = { Authorization: Bearer  };

    // 4.1 Create Draft Course
    const createCourseRes = await api('/concepts/courses', {
      method: 'POST',
      headers: teacherH,
      body: JSON.stringify({
        title: 'Lộ trình Test QA Audit 2026',
        description: 'Mô tả chi tiết cho lộ trình kiểm thử tự động',
        category: 'Algorithms',
        difficulty: 'Intermediate',
        scope: 'draft'
      })
    });
    console.log('Create Draft Course:', createCourseRes.status, 'ID:', createCourseRes.data?.id, 'Status:', createCourseRes.data?.status);
    const courseId = createCourseRes.data?.id;

    if (courseId) {
      // 4.2 Check if Draft Course appears on Public Courses List (TP-B04: MUST NOT APPEAR)
      const publicCoursesRes = await api('/concepts/courses');
      const isPublicVisible = publicCoursesRes.data?.some(c => c.id == courseId);
      console.log(Draft Course visible on Public /path?  (Expected: false));
      if (!isPublicVisible) summary.passed++;
      else {
        summary.failed++;
        summary.findings.push(Draft Course  is visible to public!);
      }

      // 4.3 Submit for Review (Draft -> PendingReview)
      const submitReviewRes = await api(/concepts/courses//submit-review, {
        method: 'POST',
        headers: teacherH
      });
      console.log('Submit Review (Draft -> PendingReview):', submitReviewRes.status, JSON.stringify(submitReviewRes.data));

      // 4.4 Check Pending Courses in Admin list
      const pendingCoursesRes = await api('/concepts/courses/pending', { headers: adminH });
      const inPending = pendingCoursesRes.data?.some(c => c.id == courseId);
      console.log(Course in Admin Pending List?  (Expected: true));
      if (inPending) summary.passed++;

      // 4.5 Admin Approve Course (PendingReview -> Active)
      const approveRes = await api(/concepts/courses//review, {
        method: 'POST',
        headers: adminH,
        body: JSON.stringify({ approve: true })
      });
      console.log('Admin Approve Course:', approveRes.status, JSON.stringify(approveRes.data));

      // 4.6 Verify Course now appears on Public Courses List (TP-C01)
      const publicAfterApprove = await api('/concepts/courses');
      const isNowPublic = publicAfterApprove.data?.some(c => c.id == courseId);
      console.log(Course visible on Public /path after Approve?  (Expected: true));
      if (isNowPublic) summary.passed++;
      else {
        summary.failed++;
        summary.findings.push(Approved Course  is NOT visible on public /path!);
      }
    }
  }

  // 5. Test Class & Deadline in the past (TP-D01, TP-D02, TP-D03)
  console.log('\n--- 5. Testing Classes, Invite Code & Past Deadline ---');
  if (tokens.TEACHER && tokens.STUDENT) {
    const teacherH = { Authorization: Bearer  };
    const studentH = { Authorization: Bearer  };

    // 5.1 Teacher creates Class
    const createClassRes = await api('/classes', {
      method: 'POST',
      headers: teacherH,
      body: JSON.stringify({
        name: 'Lớp Kiểm thử Tự động QA-01',
        description: 'Lớp demo audit',
        maxStudents: 50
      })
    });
    console.log('Teacher create class:', createClassRes.status, 'ID:', createClassRes.data?.id, 'InviteCode:', createClassRes.data?.inviteCode);
    const classId = createClassRes.data?.id;
    const inviteCode = createClassRes.data?.inviteCode;

    if (classId && inviteCode) {
      // 5.2 Student joins by code
      const joinRes = await api('/classes/join-by-code', {
        method: 'POST',
        headers: studentH,
        body: JSON.stringify({ inviteCode })
      });
      console.log('Student join by code:', joinRes.status, joinRes.data?.name);
      if (joinRes.ok) summary.passed++;

      // 5.3 Student tries to join again (Double-join test)
      const doubleJoinRes = await api('/classes/join-by-code', {
        method: 'POST',
        headers: studentH,
        body: JSON.stringify({ inviteCode })
      });
      console.log('Student double-join by code status:', doubleJoinRes.status, JSON.stringify(doubleJoinRes.data));

      // 5.4 Teacher sets deadline in the past
      const pastDate = new Date(Date.now() - 86400000 * 5).toISOString(); // 5 days ago
      const setDeadlineRes = await api(/classes//assignments/deadline, {
        method: 'PUT',
        headers: teacherH,
        body: JSON.stringify({
          pathItemId: 1,
          dueAt: pastDate,
          allowLateSubmission: true
        })
      });
      console.log('Teacher set deadline in past:', setDeadlineRes.status, JSON.stringify(setDeadlineRes.data));

      // 5.5 Teacher checks Class Report
      const reportRes = await api(/classes//report, { headers: teacherH });
      console.log('Class Report Status:', reportRes.status);
    }
  }

  // 6. Test Gamification: Hearts, Shop, Quests (TP-H01, TP-H02)
  console.log('\n--- 6. Testing Gamification: Hearts, Quests, Shop ---');
  if (tokens.STUDENT) {
    const studentH = { Authorization: Bearer  };

    const profileRes = await api('/auth/me', { headers: studentH });
    console.log('Student Profile:', profileRes.data?.displayName, 'XP:', profileRes.data?.xp);

    const questsRes = await api('/gamification/quests', { headers: studentH });
    console.log('Student Quests:', questsRes.status, Array.isArray(questsRes.data) ? ${questsRes.data.length} quests : JSON.stringify(questsRes.data));

    const shopRes = await api('/gamification/shop', { headers: studentH });
    console.log('Shop Items:', shopRes.status, Array.isArray(shopRes.data) ? ${shopRes.data.length} items : JSON.stringify(shopRes.data));

    const leaderboardRes = await api('/gamification/leaderboard', { headers: studentH });
    console.log('Leaderboard:', leaderboardRes.status, Array.isArray(leaderboardRes.data) ? ${leaderboardRes.data.length} entries : JSON.stringify(leaderboardRes.data));
  }

  // 7. Test Course Feedback & Help BugReport (TP-E01, TP-E02)
  console.log('\n--- 7. Testing Course Feedback & Bug Report ---');
  if (tokens.STUDENT && tokens.TEACHER && tokens.ADMIN) {
    const studentH = { Authorization: Bearer  };
    const teacherH = { Authorization: Bearer  };
    const adminH = { Authorization: Bearer  };

    // 7.1 Student submits course feedback
    const fbSubmitRes = await api('/courses/feedback', {
      method: 'POST',
      headers: studentH,
      body: JSON.stringify({
        courseId: 1,
        type: 'Suggestion',
        content: 'Khóa học rất trực quan nhưng cần bổ sung thêm ví dụ thực tế.'
      })
    });
    console.log('Student submit course feedback:', fbSubmitRes.status, fbSubmitRes.data?.id);
    const fbId = fbSubmitRes.data?.id;

    if (fbId) {
      // 7.2 Teacher views and replies
      const teacherFbRes = await api('/courses/feedback/for-teacher', { headers: teacherH });
      console.log('Teacher view feedbacks:', teacherFbRes.status, 'Count:', teacherFbRes.data?.length);

      const replyRes = await api(/courses/feedback/, {
        method: 'PUT',
        headers: teacherH,
        body: JSON.stringify({
          status: 'Resolved',
          replyText: 'Cảm ơn em, thầy sẽ cập nhật thêm các bài tập ứng dụng trong tuần tới!'
        })
      });
      console.log('Teacher reply feedback:', replyRes.status, 'Status:', replyRes.data?.status);

      // 7.3 Student views reply
      const myFbRes = await api('/courses/feedback/mine?courseId=1', { headers: studentH });
      console.log('Student view mine feedback reply:', myFbRes.data?.[0]?.replyText);
    }

    // 7.4 Help BugReport
    const bugReportRes = await api('/bug-reports', {
      method: 'POST',
      headers: studentH,
      body: JSON.stringify({
        description: 'Lỗi hiển thị trên trang Trợ giúp khi tìm kiếm.',
        context: JSON.stringify({ source: 'help', page: '/help' })
      })
    });
    console.log('Submit BugReport from /help:', bugReportRes.status, 'BugID:', bugReportRes.data?.id);
    const bugId = bugReportRes.data?.id;

    if (bugId) {
      const adminBugRes = await api('/admin/bug-reports', { headers: adminH });
      console.log('Admin list BugReports:', adminBugRes.status, 'Count:', adminBugRes.data?.length);

      const adminUpdateBugRes = await api(/admin/bug-reports/, {
        method: 'PUT',
        headers: adminH,
        body: JSON.stringify({
          status: 'Processing',
          adminNote: 'Đang điều tra nguyên nhân.'
        })
      });
      console.log('Admin update BugReport:', adminUpdateBugRes.status, 'Status:', adminUpdateBugRes.data?.status);
    }
  }

  // 8. Test Teacher Registration & Admin Approval (TP-C02, TP-I01)
  console.log('\n--- 8. Testing Teacher Registration (TEACHER_PENDING) & Admin Approval ---');
  if (tokens.ADMIN) {
    const adminH = { Authorization: Bearer  };
    const rand = Math.floor(Math.random() * 100000);
    const newTeacherEmail = 	eacher_candidate_@test.local;

    const regRes = await api('/auth/register', {
      method: 'POST',
      body: JSON.stringify({
        displayName: ThS. Ứng Viên ,
        email: newTeacherEmail,
        password: 'Password@123',
        isTeacher: true,
        academicDegree: 'Thạc sĩ CNTT',
        teacherBio: 'Ứng viên giảng viên demo test',
        profileLink: 'https://github.com/test'
      })
    });
    console.log('Register Teacher candidate:', regRes.status, 'Role:', regRes.data?.user?.role, 'IsTeacherPending:', regRes.data?.user?.role === 'TEACHER_PENDING');
    const newUserId = regRes.data?.user?.id;

    if (newUserId) {
      // Admin approves teacher
      const approveTeacherRes = await api(/users//approve-teacher, {
        method: 'POST',
        headers: adminH,
        body: JSON.stringify({ isApproved: true })
      });
      console.log('Admin approve teacher:', approveTeacherRes.status);

      // Candidate logins again
      const newLoginRes = await api('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email: newTeacherEmail, password: 'Password@123' })
      });
      console.log('New teacher login after approval: Role =', newLoginRes.data?.user?.role);
    }
  }

  console.log('\n=== SUMMARY ===');
  console.log(Passed: , Failed: );
  if (summary.findings.length > 0) {
    console.log('Findings:');
    summary.findings.forEach(f => console.log(' - ' + f));
  }
}

runAudit().catch(err => console.error('Audit Runner Error:', err));
