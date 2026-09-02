const { BASE_URL, createBrowserContext, loginAs, takeEvidence } = require('./helpers');

async function runGroupE() {
  console.log('\n======================================================');
  console.log('STARTING TEST GROUP E: GAMIFICATION, SHOP, QUESTS & VIETQR');
  console.log('======================================================\n');

  const findings = [];
  const { browser, page, consoleLogs, networkErrors } = await createBrowserContext();

  try {
    await loginAs(page, 'student@demo.local', 'Student@123');

    // ------------------------------------------------------------------------
    // TC-E02: Shop & Hearts Purchase & Negative Gems Prevention
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-E02: Shop & Negative Gems Check ---');
    await page.goto(`${BASE_URL}/shop`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await takeEvidence(page, 'QA-E02_01_shop_view');

    // Get current gems
    const initialGems = await page.evaluate(() => {
      const el = document.querySelector('.hearts-gems__chip--gems, [class*="gems"]');
      return el ? el.innerText : '0';
    });
    console.log(`[TC-E02] Initial Gems displayed: ${initialGems}`);

    // Try to buy an item
    const buyBtn = await page.$('.shop-item button, button:has-text("Mua"), button:has-text("Sở hữu")');
    if (buyBtn) {
      await buyBtn.click();
      await page.waitForTimeout(500);
      await takeEvidence(page, 'QA-E02_02_buy_modal');

      const confirmBuyBtn = await page.$('button:has-text("Xác nhận"), button:has-text("Mua ngay")');
      if (confirmBuyBtn) {
        await confirmBuyBtn.click();
        await page.waitForTimeout(1500);
        await takeEvidence(page, 'QA-E02_03_after_buy_attempt');
      }
    }

    // ------------------------------------------------------------------------
    // TC-E03: Daily Quests & Race Condition (Spam Click)
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-E03: Daily Quests & Race Condition Check ---');
    await page.goto(`${BASE_URL}/quests`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await takeEvidence(page, 'QA-E03_01_quests_view');

    const claimBtn = await page.$('button:has-text("Nhận thưởng"), button:has-text("Claim")');
    if (claimBtn) {
      console.log('[TC-E03] Found claimable quest! Testing rapid multi-click (race condition)...');
      // Rapid spam click 5 times
      const clicks = [
        claimBtn.click().catch(() => {}),
        claimBtn.click().catch(() => {}),
        claimBtn.click().catch(() => {}),
        claimBtn.click().catch(() => {}),
        claimBtn.click().catch(() => {})
      ];
      await Promise.all(clicks);
      await page.waitForTimeout(2000);
      await takeEvidence(page, 'QA-E03_02_after_spam_claim');
    } else {
      console.log('[TC-E03] No quest in claimable state currently.');
    }

    // ------------------------------------------------------------------------
    // TC-E04: Premium Packages & VietQR Modal
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-E04: Premium Packages & VietQR Modal ---');
    await page.goto(`${BASE_URL}/premium`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await takeEvidence(page, 'QA-E04_01_premium_pricing_plans');

    // Click upgrade button
    const upgradeBtn = await page.$('button:has-text("Nâng cấp ngay"), button:has-text("Chọn gói"), button:has-text("Đăng ký Pro")');
    if (upgradeBtn) {
      await upgradeBtn.click();
      await page.waitForTimeout(800);
      await takeEvidence(page, 'QA-E04_02_vietqr_checkout_modal');

      // Check VietQR image, account info and memo
      const qrImg = await page.$('img[alt*="QR" i], [class*="qr"] img, canvas');
      const memoText = await page.innerText('.vietqr-modal, [class*="modal"], [class*="checkout"]').catch(() => '');
      console.log(`[TC-E04] Has QR Code element: ${!!qrImg}`);
      console.log(`[TC-E04] Checkout Memo preview: ${memoText.substring(0, 200).replace(/\n+/g, ' ')}`);

      // Mock Pay confirmation button
      const mockPayBtn = await page.$('button:has-text("Xác nhận đã chuyển"), button:has-text("Thanh toán giả lập"), button:has-text("Đã chuyển khoản")');
      if (mockPayBtn) {
        await mockPayBtn.click();
        await page.waitForTimeout(2000);
        await takeEvidence(page, 'QA-E04_03_after_mock_pay');
        console.log('[TC-E04] Mock Pay submitted.');
      }
    }

  } catch (err) {
    console.error('[Error in Group E Execution]:', err);
    findings.push({
      id: 'QA-E-CRASH',
      severity: 'P0',
      title: 'Crash trong Group E',
      details: err.stack || err.message
    });
  } finally {
    await browser.close();
  }

  console.log('\nGroup E Completed with findings:', findings.length);
  return findings;
}

runGroupE();
