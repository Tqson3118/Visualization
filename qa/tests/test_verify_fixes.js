const { BASE_URL, createBrowserContext, loginAs, takeEvidence } = require('./helpers');

async function runVerifyFixes() {
  console.log('\n======================================================');
  console.log('STARTING PLAYWRIGHT VERIFICATION OF ALL NEW FIXES');
  console.log('======================================================\n');

  const { browser, page, consoleLogs, networkErrors } = await createBrowserContext();
  const testResults = {};

  try {
    // ------------------------------------------------------------------------
    // TEST 1: Shop Avatar/Frame Purchase, Unique Ownership & Quick Equip
    // ------------------------------------------------------------------------
    console.log('\n--- Test 1: Shop Avatar/Frame Purchase & Quick Equip ---');
    await loginAs(page, 'teacher@demo.local', 'Teacher@123');
    await page.goto(`${BASE_URL}/shop`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await takeEvidence(page, 'QA-FIX-01_01_teacher_shop_view');

    // Find first cosmetic item that has "Mua" button
    const buyButton = await page.$('.shop__card:not(.shop__card--owned) button:has-text("Mua")');
    if (buyButton) {
      const itemName = await page.evaluate(el => {
        const card = el.closest('.shop__card');
        return card ? card.querySelector('.shop__name')?.textContent?.trim() : 'Item';
      }, buyButton);
      console.log(`[Test 1] Purchasing item: "${itemName}"...`);
      await buyButton.click();
      await page.waitForTimeout(2000);
      await takeEvidence(page, 'QA-FIX-01_02_after_purchase');

      const equipButton = await page.$('button:has-text("Trang bị ngay"), button:has-text("Gỡ trang bị")');
      const isOwnedBadge = await page.$('.shop__card--owned, span:has-text("Đã sở hữu"), span:has-text("Đang dùng")');
      console.log(`[Test 1] Has Equip Button: ${!!equipButton}, Has Owned Badge: ${!!isOwnedBadge}`);
      testResults.cosmeticOwnership = !!equipButton && !!isOwnedBadge;

      if (equipButton) {
        console.log('[Test 1] Clicking quick equip button from shop...');
        await equipButton.click();
        await page.waitForTimeout(1500);
        await takeEvidence(page, 'QA-FIX-01_03_after_quick_equip');
      }
    } else {
      console.log('[Test 1] Checking existing owned cosmetic items...');
      const equipButton = await page.$('button:has-text("Trang bị ngay"), button:has-text("Gỡ trang bị")');
      testResults.cosmeticOwnership = !!equipButton;
      await takeEvidence(page, 'QA-FIX-01_02_already_owned_items');
    }

    // ------------------------------------------------------------------------
    // TEST 2: Shop Missing Gems Hint & 1-Click Quest Navigation
    // ------------------------------------------------------------------------
    console.log('\n--- Test 2: Shop Missing Gems Hint & Quest Navigation ---');
    await loginAs(page, 'student@demo.local', 'Student@123');
    await page.goto(`${BASE_URL}/shop`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const missingGemsLink = await page.$('a[href="/quests"]:has-text("Thiếu"), a:has-text("Kiếm tại Thử thách")');
    console.log(`[Test 2] Missing Gems hint link found: ${!!missingGemsLink}`);
    await takeEvidence(page, 'QA-FIX-02_01_student_shop_missing_gems_hint');

    if (missingGemsLink) {
      await missingGemsLink.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);
      const url = page.url();
      console.log(`[Test 2] Navigated to Quests page: ${url.includes('/quests')}`);
      testResults.missingGemsLink = url.includes('/quests');
      await takeEvidence(page, 'QA-FIX-02_02_navigated_to_quests');
    }

    // ------------------------------------------------------------------------
    // TEST 3: Feedback Tab on Student Profile
    // ------------------------------------------------------------------------
    console.log('\n--- Test 3: Feedback Tab on Student Profile ---');
    await page.goto(`${BASE_URL}/profile?tab=feedback`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1500);

    const feedbackPanel = await page.$('.profile__feedback-panel, .profile__feedback-title');
    const feedbackCards = await page.$$('.profile__feedback-card');
    const teacherReplies = await page.$$('.profile__teacher-reply');
    console.log(`[Test 3] Feedback Panel visible: ${!!feedbackPanel}`);
    console.log(`[Test 3] Total Feedback Cards rendered: ${feedbackCards.length}`);
    console.log(`[Test 3] Teacher Replies rendered: ${teacherReplies.length}`);
    testResults.profileFeedbackTab = !!feedbackPanel;
    await takeEvidence(page, 'QA-FIX-03_01_student_profile_feedback_tab');

    // ------------------------------------------------------------------------
    // TEST 4: Modal Data Loss Protection (:is-dirty) on ESC
    // ------------------------------------------------------------------------
    console.log('\n--- Test 4: Modal Data Loss Protection on ESC ---');
    await loginAs(page, 'teacher@demo.local', 'Teacher@123');
    await page.goto(`${BASE_URL}/classes`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const createClassBtn = await page.$('.classes__hero-actions button, button:has-text("Tạo lớp")');
    if (createClassBtn) {
      await createClassBtn.click();
      await page.waitForTimeout(600);

      const nameInput = await page.$('input[placeholder*="tên lớp" i], input#class-name, input');
      if (nameInput) {
        await nameInput.fill('Lớp thử nghiệm bảo vệ dữ liệu form khi bấm ESC');
        await page.waitForTimeout(300);

        let dialogTriggered = false;
        let dialogMessage = '';

        // Handle native browser confirm dialog: dismiss first
        page.once('dialog', async dialog => {
          dialogTriggered = true;
          dialogMessage = dialog.message();
          console.log(`[Test 4] Native confirm dialog popped up: "${dialogMessage}". Dismissing (Cancel)...`);
          await dialog.dismiss();
        });

        // Press Escape
        console.log('[Test 4] Pressing Escape key on dirty modal...');
        await page.keyboard.press('Escape');
        await page.waitForTimeout(800);

        const isModalStillOpen = await page.isVisible('input[placeholder*="tên lớp" i], input#class-name, input');
        const retainedValue = await nameInput.inputValue();
        console.log(`[Test 4] Modal still open after ESC dismiss: ${isModalStillOpen}`);
        console.log(`[Test 4] Input value retained: "${retainedValue}"`);
        await takeEvidence(page, 'QA-FIX-04_01_modal_data_retained_after_esc');
        testResults.modalProtection = dialogTriggered && isModalStillOpen && retainedValue.length > 0;
      }
    }

    // ------------------------------------------------------------------------
    // TEST 5: Avatar 403 Error Sweep
    // ------------------------------------------------------------------------
    console.log('\n--- Test 5: Avatar 403 Error Sweep ---');
    const avatar403s = networkErrors.filter(e => e.includes('403') || e.includes('pngtree'));
    console.log(`[Test 5] Total 403 / PNGTree errors during entire session: ${avatar403s.length}`);
    testResults.noAvatar403s = avatar403s.length === 0;

  } catch (err) {
    console.error('[Error during Playwright verification]:', err);
  } finally {
    await browser.close();
  }

  console.log('\n======================================================');
  console.log('VERIFICATION SUMMARY RESULTS:', JSON.stringify(testResults, null, 2));
  console.log('======================================================\n');
  return testResults;
}

runVerifyFixes();
