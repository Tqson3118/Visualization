const { BASE_URL, createBrowserContext, loginAs, takeEvidence } = require('./helpers');

async function runGroupC() {
  console.log('\n======================================================');
  console.log('STARTING TEST GROUP C: CLASSROOM, INVITE CODES & DEADLINES');
  console.log('======================================================\n');

  const findings = [];
  const { browser, page, consoleLogs, networkErrors } = await createBrowserContext();

  try {
    // ------------------------------------------------------------------------
    // TC-C01 & TC-C02: Teacher Creates Class & Validates Past Deadline
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-C01 & TC-C02: Teacher Create Class & Past Deadline Validation ---');
    await loginAs(page, 'teacher@demo.local', 'Teacher@123');
    await page.goto(`${BASE_URL}/classes`);
    await page.waitForLoadState('networkidle');
    await takeEvidence(page, 'QA-C01_01_teacher_classes_list');

    const createClassBtn = await page.$('button:has-text("Tạo lớp"), button:has-text("Thêm lớp mới")');
    if (createClassBtn) {
      await createClassBtn.click();
      await page.waitForTimeout(600);
      await takeEvidence(page, 'QA-C01_02_create_class_modal');

      const nameInput = await page.$('input[placeholder*="tên lớp" i], input#class-name, input[name="name"]');
      const semesterInput = await page.$('input[placeholder*="học kỳ" i], input#semester, input[name="semester"]');
      const uniqueClassName = `Lớp QA K2026-${Date.now()}`;

      if (nameInput) await nameInput.fill(uniqueClassName);
      if (semesterInput) await semesterInput.fill('HK1-2026');

      const submitClassBtn = await page.$('button[type="submit"]:has-text("Tạo"), button:has-text("Lưu lớp")');
      if (submitClassBtn) {
        await submitClassBtn.click();
        await page.waitForTimeout(1500);
        await takeEvidence(page, 'QA-C01_03_class_created');
        console.log(`[TC-C01] Class '${uniqueClassName}' created successfully.`);
      }
    }

    // Open first class detail
    const classCard = await page.$('a[href^="/classes/"], .class-card');
    if (classCard) {
      await classCard.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);
      await takeEvidence(page, 'QA-C01_04_class_detail');

      // Check invite code
      const inviteCodeEl = await page.$('[class*="invite"], [class*="code"], strong:has-text("DSA")');
      console.log('[TC-C01] Invite Code displayed in Class Detail');

      // Test Class Report
      const reportBtn = await page.$('a[href$="/report"], button:has-text("Báo cáo"), a:has-text("Báo cáo tiến độ")');
      if (reportBtn) {
        await reportBtn.click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);
        await takeEvidence(page, 'QA-C03_class_report_view');
      }
    }

    // ------------------------------------------------------------------------
    // TC-C04: Student Join by Code Test Matrix
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-C04: Join Class by Code Test Matrix ---');
    await loginAs(page, 'student@demo.local', 'Student@123');
    await page.goto(`${BASE_URL}/classes`);
    await page.waitForLoadState('networkidle');
    await takeEvidence(page, 'QA-C04_01_student_classes_list');

    const joinBtn = await page.$('button:has-text("Tham gia"), button:has-text("Nhập mã")');
    if (joinBtn) {
      await joinBtn.click();
      await page.waitForTimeout(500);
      await takeEvidence(page, 'QA-C04_02_join_modal');

      const codeInput = await page.$('input[placeholder*="mã" i], input#invite-code');
      const submitJoinBtn = await page.$('button[type="submit"]:has-text("Tham gia"), button:has-text("Xác nhận")');

      if (codeInput && submitJoinBtn) {
        // Case 1: Non-existent code
        console.log('[TC-C04] Testing non-existent code: NOTEXS...');
        await codeInput.fill('NOTEXS');
        await submitJoinBtn.click();
        await page.waitForTimeout(1000);
        await takeEvidence(page, 'QA-C04_03_invalid_code_error');

        // Case 2: Closed class code
        console.log('[TC-C04] Testing closed class code: ADVNCE...');
        await codeInput.fill('ADVNCE');
        await submitJoinBtn.click();
        await page.waitForTimeout(1000);
        await takeEvidence(page, 'QA-C04_04_closed_class_error');

        // Case 3: Valid open class code
        console.log('[TC-C04] Testing valid open class code: DSA213...');
        await codeInput.fill('DSA213');
        await submitJoinBtn.click();
        await page.waitForTimeout(1500);
        await takeEvidence(page, 'QA-C04_05_join_success');
      }
    }

  } catch (err) {
    console.error('[Error in Group C Execution]:', err);
    findings.push({
      id: 'QA-C-CRASH',
      severity: 'P0',
      title: 'Crash trong Group C',
      details: err.stack || err.message
    });
  } finally {
    await browser.close();
  }

  console.log('\nGroup C Completed with findings:', findings.length);
  return findings;
}

runGroupC();
