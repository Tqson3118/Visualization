const { BASE_URL, createBrowserContext, loginAs, takeEvidence } = require('./helpers');

async function runAdminExtensionTests() {
  console.log('\n======================================================');
  console.log('STARTING PLAYWRIGHT TESTING OF ADMIN EXTENSIONS');
  console.log('======================================================\n');

  const { browser, page } = await createBrowserContext();
  const results = {};

  try {
    // ------------------------------------------------------------------------
    // Step 1: Admin logs in and opens /admin/shop
    // ------------------------------------------------------------------------
    console.log('\n--- Step 1: Admin Shop & Gamification Management ---');
    await loginAs(page, 'admin@system.local', 'Admin@123');
    await page.waitForTimeout(1000);

    await page.goto(`${BASE_URL}/admin/shop`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1500);
    await takeEvidence(page, 'QA-ADMIN-01_shop_management');

    const shopTitle = await page.isVisible('h1:has-text("Quản lý Cửa hàng & Gamification")');
    console.log(`[Admin Shop] Console visible: ${shopTitle}`);

    // Click "Thêm vật phẩm"
    const addItemBtn = await page.$('button:has-text("Thêm vật phẩm")');
    if (addItemBtn) {
      await addItemBtn.click();
      await page.waitForTimeout(600);

      const uniqueKey = `avatar-dragon-${Date.now()}`;
      await page.fill('input[placeholder*="avatar-dragon" i]', uniqueKey);
      await page.fill('input[placeholder*="Rồng Thần" i]', 'Avatar Rồng Huyền Thoại');
      await page.fill('input[type="number"][min="0"]', '350');

      await page.click('button[type="submit"]:has-text("Lưu vật phẩm")');
      await page.waitForTimeout(2000);
      await takeEvidence(page, 'QA-ADMIN-02_item_created');

      const itemRow = await page.isVisible(`td:has-text("${uniqueKey}")`);
      console.log(`[Admin Shop] Created item row found: ${itemRow}`);
      results.shopItemCreated = itemRow;
    }

    // Check Gem transactions tab
    const txTabBtn = await page.$('button:has-text("Nhật ký Dòng tiền Gems")');
    if (txTabBtn) {
      await txTabBtn.click();
      await page.waitForTimeout(1000);
      await takeEvidence(page, 'QA-ADMIN-03_gem_transactions_tab');
      console.log('[Admin Shop] Gem transactions tab loaded.');
    }

    // ------------------------------------------------------------------------
    // Step 2: Admin opens /admin/transactions and grants Pro
    // ------------------------------------------------------------------------
    console.log('\n--- Step 2: Admin Transactions & Manual Pro Grant ---');
    await page.goto(`${BASE_URL}/admin/transactions`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1500);
    await takeEvidence(page, 'QA-ADMIN-04_transactions_list');

    const txTitle = await page.isVisible('h1:has-text("Giao dịch & Quản lý Gói Pro")');
    console.log(`[Admin Transactions] Console visible: ${txTitle}`);

    // Click "Kích hoạt Pro thủ công"
    const grantProBtn = await page.$('button:has-text("Kích hoạt Pro thủ công")');
    if (grantProBtn) {
      await grantProBtn.click();
      await page.waitForTimeout(600);

      await page.fill('input[type="email"]', 'student@demo.local');
      await page.click('button[type="submit"]:has-text("Xác nhận Kích hoạt Pro")');
      await page.waitForTimeout(2500);
      await takeEvidence(page, 'QA-ADMIN-05_grant_pro_success');

      const studentSubRow = await page.isVisible('p:has-text("student@demo.local")');
      console.log(`[Admin Transactions] Student Pro row visible: ${studentSubRow}`);
      results.proGranted = studentSubRow;
    }

    // ------------------------------------------------------------------------
    // Step 3: Student logs in and verifies Pro status
    // ------------------------------------------------------------------------
    console.log('\n--- Step 3: Student verifies active Pro status ---');
    await loginAs(page, 'student@demo.local', 'Student@123');
    await page.waitForTimeout(1000);

    await page.goto(`${BASE_URL}/profile`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    await takeEvidence(page, 'QA-ADMIN-06_student_pro_active');

    const proBadge = await page.isVisible('span:has-text("Premium")');
    console.log(`[Student Profile] Has Premium badge: ${proBadge}`);
    results.studentHasPremiumBadge = proBadge;

  } catch (err) {
    console.error('[Error in Admin Extension Tests]:', err);
  } finally {
    await browser.close();
  }

  console.log('\n======================================================');
  console.log('ADMIN EXTENSION TEST RESULTS:', JSON.stringify(results, null, 2));
  console.log('======================================================\n');
  return results;
}

runAdminExtensionTests();
