const { BASE_URL, createBrowserContext, loginAs, logout, takeEvidence } = require('./helpers');

async function runGroupA() {
  console.log('\n======================================================');
  console.log('STARTING TEST GROUP A: LEARNING PATH & STATE MUTATIONS');
  console.log('======================================================\n');

  const findings = [];
  const { browser, page, consoleLogs, networkErrors } = await createBrowserContext();

  try {
    // ------------------------------------------------------------------------
    // TC-A01: Tiến độ học tập & Mở khóa Node tiếp theo
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-A01: Tiến độ học tập & Mở khóa Node ---');
    await loginAs(page, 'student@demo.local', 'Student@123');
    await page.goto(`${BASE_URL}/path`);
    await page.waitForLoadState('networkidle');
    await takeEvidence(page, 'QA-A01_01_path_list');

    // Click on course card
    const courseCard = await page.$('a[href^="/path/"]');
    if (courseCard) {
      await courseCard.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);
      await takeEvidence(page, 'QA-A01_02_course_detail');

      // Check if enroll button is present ("Mở khóa lộ trình với 1 Tim" or "Bắt đầu học")
      const enrollBtn = await page.$('button:has-text("Mở khóa lộ trình"), button:has-text("Bắt đầu học với 1")');
      if (enrollBtn) {
        console.log('[TC-A01] Enrolling in course...');
        await enrollBtn.click();
        await page.waitForTimeout(500);
        await takeEvidence(page, 'QA-A01_03_enroll_modal');

        const confirmBtn = await page.$('button:has-text("Xác nhận mở khóa"), button:has-text("Xác nhận"), button:has-text("Đồng ý")');
        if (confirmBtn) {
          await confirmBtn.click();
          await page.waitForTimeout(1000);
          await takeEvidence(page, 'QA-A01_04_enrolled');
        }
      }

      // Check "Học tiếp" button or first lesson link
      const startStudyBtn = await page.$('button:has-text("Học tiếp"), button:has-text("Bắt đầu"), a:has-text("Học tiếp"), a:has-text("Bắt đầu")');
      if (startStudyBtn) {
        console.log('[TC-A01] Clicking Start/Continue Study button...');
        await startStudyBtn.click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1500);
        await takeEvidence(page, 'QA-A01_05_lesson_study_view');

        // Look for mark as completed button
        const markCompleteBtn = await page.$('button:has-text("Đánh dấu đã học"), button:has-text("Hoàn thành bài học"), button:has-text("Tiếp tục")');
        if (markCompleteBtn) {
          console.log('[TC-A01] Marking lesson as completed...');
          await markCompleteBtn.click();
          await page.waitForTimeout(1500);
          await takeEvidence(page, 'QA-A01_06_lesson_completed_modal');

          // Close modal or proceed
          const modalCloseBtn = await page.$('button:has-text("Đóng"), button:has-text("Bài tiếp theo"), button:has-text("Tiếp tục")');
          if (modalCloseBtn) {
            await modalCloseBtn.click();
            await page.waitForTimeout(1000);
          }
        }
      }
    }

    // ------------------------------------------------------------------------
    // TC-A05: Quy trình Duyệt Lộ trình (Gating & Isolation)
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-A05: Gating & Isolation (Draft -> PendingReview -> Active) ---');
    // Step 1: Teacher creates a course
    await loginAs(page, 'teacher@demo.local', 'Teacher@123');
    await page.goto(`${BASE_URL}/studio?tab=overview`);
    await page.waitForLoadState('networkidle');
    await takeEvidence(page, 'QA-A05_01_teacher_studio_overview');

    const newCourseBtn = await page.$('button:has-text("Tạo khóa học mới"), button:has-text("Tạo lộ trình mới"), button:has-text("Thêm lộ trình")');
    let testCourseTitle = `Lộ trình Test Gating QA-${Date.now()}`;
    if (newCourseBtn) {
      await newCourseBtn.click();
      await page.waitForTimeout(600);
      await takeEvidence(page, 'QA-A05_02_new_course_modal');

      const titleInput = await page.$('input#course-title, input[placeholder*="tên" i], input[placeholder*="tiêu đề" i], input[type="text"]');
      if (titleInput) {
        await titleInput.fill(testCourseTitle);
        const submitBtn = await page.$('button[type="submit"]:has-text("Tạo"), button:has-text("Tạo khóa học"), button:has-text("Lưu")');
        if (submitBtn) {
          await submitBtn.click();
          await page.waitForTimeout(1500);
          await takeEvidence(page, 'QA-A05_03_course_created_in_studio');
          console.log(`[TC-A05] Created draft course: ${testCourseTitle}`);
        }
      }
    }

    // Step 2: Submit for review if button exists
    const submitReviewBtn = await page.$(`button:has-text("Gửi duyệt"), button:has-text("Xuất bản")`);
    if (submitReviewBtn) {
      await submitReviewBtn.click();
      await page.waitForTimeout(1000);
      await takeEvidence(page, 'QA-A05_04_submitted_for_review');
    }

    // Step 3: Check Student cannot see Pending course on /path
    await loginAs(page, 'student@demo.local', 'Student@123');
    await page.goto(`${BASE_URL}/path`);
    await page.waitForLoadState('networkidle');
    const studentPathText = await page.innerText('body');
    const isVisibleToStudent = studentPathText.includes(testCourseTitle);
    console.log(`[TC-A05 Verification] Course visible to Student while PendingReview: ${isVisibleToStudent}`);
    await takeEvidence(page, 'QA-A05_05_student_path_check');

    // Step 4: Check Guest cannot see Pending course on /path
    await logout(page);
    await page.goto(`${BASE_URL}/path`);
    await page.waitForLoadState('networkidle');
    const guestPathText = await page.innerText('body');
    const isVisibleToGuest = guestPathText.includes(testCourseTitle);
    console.log(`[TC-A05 Verification] Course visible to Guest while PendingReview: ${isVisibleToGuest}`);
    await takeEvidence(page, 'QA-A05_06_guest_path_check');

    if (isVisibleToStudent || isVisibleToGuest) {
      findings.push({
        id: 'QA-A05-ERR',
        severity: 'P0',
        title: 'Khóa học PendingReview/Draft bị lộ ra ngoài trang công khai',
        role: 'Student / Guest',
        screen: '/path',
        details: `Khóa học chưa được duyệt '${testCourseTitle}' vẫn hiển thị cho Student/Guest trên /path.`
      });
    }

    // Step 5: Admin Moderation Tab approves
    await loginAs(page, 'admin@system.local', 'Admin@123');
    await page.goto(`${BASE_URL}/studio?tab=moderation`);
    await page.waitForLoadState('networkidle');
    await takeEvidence(page, 'QA-A05_07_admin_moderation_tab');

    // ------------------------------------------------------------------------
    // TC-A07: So sánh Studio UI với Dữ liệu Seed
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-A07: Studio UI & Outline Tree Inspection ---');
    await loginAs(page, 'teacher@demo.local', 'Teacher@123');
    await page.goto(`${BASE_URL}/studio?tab=curriculum`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1500);
    await takeEvidence(page, 'QA-A07_01_studio_curriculum');

    const treeNodes = await page.$$('.tree-node, [class*="node"], [class*="outline"]');
    console.log(`[TC-A07] Found ${treeNodes.length} outline tree elements in Studio Curriculum`);

  } catch (err) {
    console.error('[Error in Group A Execution]:', err);
    findings.push({
      id: 'QA-A-CRASH',
      severity: 'P0',
      title: 'Lỗi trong kịch bản kiểm thử Group A',
      details: err.stack || err.message
    });
  } finally {
    await browser.close();
  }

  console.log('\nGroup A Completed with findings:', findings.length);
  return findings;
}

runGroupA();
