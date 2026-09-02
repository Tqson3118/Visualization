const { BASE_URL, createBrowserContext, loginAs, logout, takeEvidence } = require('./helpers');

async function runGroupD() {
  console.log('\n======================================================');
  console.log('STARTING TEST GROUP D: 2-WAY FEEDBACK & DEAD-END UX');
  console.log('======================================================\n');

  const findings = [];
  const { browser, page, consoleLogs, networkErrors } = await createBrowserContext();

  try {
    const feedbackMsg = `Ý kiến test QA-${Date.now()}: Bài học rất trực quan, mong bổ sung thêm bài tập code.`;

    // ------------------------------------------------------------------------
    // Step 1: Student submits feedback on course detail
    // ------------------------------------------------------------------------
    console.log('\n--- Step 1: Student submits course feedback ---');
    await loginAs(page, 'student@demo.local', 'Student@123');
    await page.goto(`${BASE_URL}/path`);
    await page.waitForLoadState('networkidle');

    const courseLink = await page.$('a[href^="/path/"]');
    if (courseLink) {
      await courseLink.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Scroll to feedback section
      await page.evaluate(() => {
        const el = document.getElementById('course-feedback');
        if (el) el.scrollIntoView();
      });
      await page.waitForTimeout(500);
      await takeEvidence(page, 'QA-D01_01_student_feedback_form');

      const textarea = await page.$('#course-feedback textarea, textarea[placeholder*="ý kiến" i]');
      if (textarea) {
        await textarea.fill(feedbackMsg);
        const submitBtn = await page.$('#course-feedback button[type="submit"], button:has-text("Gửi ý kiến")');
        if (submitBtn) {
          await submitBtn.click();
          await page.waitForTimeout(1500);
          await takeEvidence(page, 'QA-D01_02_feedback_submitted');
          console.log('[TC-D01] Feedback submitted by Student.');
        }
      }
    }

    // ------------------------------------------------------------------------
    // Step 2: Teacher opens Studio Feedback tab and replies
    // ------------------------------------------------------------------------
    console.log('\n--- Step 2: Teacher views feedback in Studio and replies ---');
    await loginAs(page, 'teacher@demo.local', 'Teacher@123');
    await page.goto(`${BASE_URL}/studio?tab=feedback`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1500);
    await takeEvidence(page, 'QA-D01_03_teacher_studio_feedback');

    // Find the feedback item and reply
    const replyBtn = await page.$('button:has-text("Trả lời"), button:has-text("Phản hồi")');
    if (replyBtn) {
      await replyBtn.click();
      await page.waitForTimeout(500);
      await takeEvidence(page, 'QA-D01_04_teacher_reply_modal');

      const replyInput = await page.$('textarea[placeholder*="nội dung" i], textarea[placeholder*="phản hồi" i], textarea');
      if (replyInput) {
        const replyText = `Cảm ơn bạn! Giảng viên đã ghi nhận và đang bổ sung thêm bài tập mới. (QA-${Date.now()})`;
        await replyInput.fill(replyText);
        const sendReplyBtn = await page.$('button[type="submit"]:has-text("Gửi"), button:has-text("Lưu phản hồi"), button:has-text("Gửi phản hồi")');
        if (sendReplyBtn) {
          await sendReplyBtn.click();
          await page.waitForTimeout(1500);
          await takeEvidence(page, 'QA-D01_05_teacher_reply_sent');
          console.log('[TC-D01] Teacher replied successfully.');
        }
      }
    }

    // ------------------------------------------------------------------------
    // Step 3: Dead-end Check - Student verifies where the reply appears
    // ------------------------------------------------------------------------
    console.log('\n--- Step 3: Dead-end UX Verification for Student ---');
    await loginAs(page, 'student@demo.local', 'Student@123');
    
    // Check 1: In Course Detail feedback section
    await page.goto(`${BASE_URL}/path`);
    await page.waitForLoadState('networkidle');
    const firstCourse = await page.$('a[href^="/path/"]');
    if (firstCourse) {
      await firstCourse.click();
      await page.waitForLoadState('networkidle');
      await page.evaluate(() => {
        const el = document.getElementById('course-feedback');
        if (el) el.scrollIntoView();
      });
      await page.waitForTimeout(1000);
      await takeEvidence(page, 'QA-D02_01_student_feedback_reply_view');
      const pageText = await page.innerText('body');
      console.log(`[Dead-end Check] Student sees teacher reply in CourseDetail: ${pageText.includes('đã phản hồi') || pageText.includes('Cảm ơn bạn!')}`);
    }

    // Check 2: In Profile View (/profile)
    await page.goto(`${BASE_URL}/profile`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await takeEvidence(page, 'QA-D02_02_student_profile_view');

    // ------------------------------------------------------------------------
    // Step 4: Help page Contact Form & Admin Bug Reports
    // ------------------------------------------------------------------------
    console.log('\n--- Step 4: Help Page Form & Admin Bug Reports ---');
    await page.goto(`${BASE_URL}/help`);
    await page.waitForLoadState('networkidle');
    await takeEvidence(page, 'QA-D03_01_help_page');

    const helpName = await page.$('input[placeholder*="tên" i], input[autocomplete="name"]');
    const helpEmail = await page.$('input[placeholder*="email" i], input[autocomplete="email"]');
    const helpMsg = await page.$('textarea#help-message, textarea[placeholder*="nội dung" i]');

    if (helpName) await helpName.fill('Nguyễn QA Student');
    if (helpEmail) await helpEmail.fill('student@demo.local');
    if (helpMsg) await helpMsg.fill('Cần hỗ trợ hướng dẫn sử dụng tính năng mô phỏng cây AVL (Test QA).');

    const sendHelpBtn = await page.$('button[type="submit"]:has-text("Gửi"), button:has-text("Gửi yêu cầu")');
    if (sendHelpBtn) {
      await sendHelpBtn.click();
      await page.waitForTimeout(1500);
      await takeEvidence(page, 'QA-D03_02_help_submitted');
      console.log('[TC-D03] Help contact submitted.');
    }

    // Check Admin View for Bug Reports / Help requests
    await loginAs(page, 'admin@system.local', 'Admin@123');
    await page.goto(`${BASE_URL}/admin/settings`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await takeEvidence(page, 'QA-D03_03_admin_settings_bug_reports');

  } catch (err) {
    console.error('[Error in Group D Execution]:', err);
    findings.push({
      id: 'QA-D-CRASH',
      severity: 'P0',
      title: 'Crash trong Group D',
      details: err.stack || err.message
    });
  } finally {
    await browser.close();
  }

  console.log('\nGroup D Completed with findings:', findings.length);
  return findings;
}

runGroupD();
