const { BASE_URL, createBrowserContext, loginAs, takeEvidence } = require('./helpers');

async function runGroupG() {
  console.log('\n======================================================');
  console.log('STARTING TEST GROUP G: UX SWEEP, MODAL & RESPONSIVE');
  console.log('======================================================\n');

  const findings = [];

  // --------------------------------------------------------------------------
  // TC-G01: Mobile 375x667 Responsive Sweep
  // --------------------------------------------------------------------------
  console.log('\n--- Running TC-G01: Mobile 375x667 Responsive Sweep ---');
  const mobileCtx = await createBrowserContext({ viewport: { width: 375, height: 667 }, isMobile: true, hasTouch: true });
  const mobilePage = mobileCtx.page;

  try {
    await loginAs(mobilePage, 'student@demo.local', 'Student@123');

    const screens = [
      { name: 'home', url: '/' },
      { name: 'path', url: '/path' },
      { name: 'simulator', url: '/simulator/sort.bubble' },
      { name: 'classes', url: '/classes' },
      { name: 'shop', url: '/shop' },
      { name: 'quests', url: '/quests' },
      { name: 'profile', url: '/profile' },
      { name: 'help', url: '/help' },
      { name: 'privacy', url: '/privacy' }
    ];

    for (const scr of screens) {
      await mobilePage.goto(`${BASE_URL}${scr.url}`);
      await mobilePage.waitForLoadState('networkidle');
      await mobilePage.waitForTimeout(500);

      // Check horizontal overflow
      const overflow = await mobilePage.evaluate(() => {
        const bodyWidth = document.body.scrollWidth;
        const htmlWidth = document.documentElement.scrollWidth;
        const winWidth = window.innerWidth;
        return {
          hasOverflow: htmlWidth > winWidth + 2 || bodyWidth > winWidth + 2,
          scrollWidth: Math.max(bodyWidth, htmlWidth),
          innerWidth: winWidth
        };
      });

      console.log(`[Mobile 375px] ${scr.name} (${scr.url}) -> Overflow: ${overflow.hasOverflow} (${overflow.scrollWidth}px / ${overflow.innerWidth}px)`);
      await takeEvidence(mobilePage, `QA-G01_mobile_${scr.name}_375`);

      if (overflow.hasOverflow) {
        findings.push({
          id: `QA-G01-OVERFLOW-${scr.name.toUpperCase()}`,
          severity: 'P2',
          title: `Vỡ layout: Tràn thanh cuộn ngang trên Mobile 375px tại trang ${scr.name}`,
          screen: scr.url,
          details: `Chiều rộng thực tế ${overflow.scrollWidth}px vượt quá viewport 375px, gây thanh cuộn ngang bất thường.`
        });
      }
    }
  } finally {
    await mobileCtx.browser.close();
  }

  // --------------------------------------------------------------------------
  // TC-G03: Modal Data Loss on ESC / Backdrop Click
  // --------------------------------------------------------------------------
  console.log('\n--- Running TC-G03: Modal Data Loss on ESC / Backdrop Click ---');
  const desktopCtx = await createBrowserContext({ viewport: { width: 1440, height: 900 } });
  const page = desktopCtx.page;

  try {
    // Test 1: Class Create Modal
    await loginAs(page, 'teacher@demo.local', 'Teacher@123');
    await page.goto(`${BASE_URL}/classes`);
    await page.waitForLoadState('networkidle');

    const addClassBtn = await page.$('button:has-text("Tạo lớp"), button:has-text("Thêm lớp mới")');
    if (addClassBtn) {
      await addClassBtn.click();
      await page.waitForTimeout(500);

      const nameInput = await page.$('input[placeholder*="tên lớp" i], input#class-name, input[name="name"]');
      if (nameInput) {
        await nameInput.fill('Dữ liệu đang nhập dở chưa lưu...');
        await page.waitForTimeout(200);

        // Press ESC key
        console.log('[TC-G03] Pressing ESC key on active Modal with unsaved input...');
        await page.keyboard.press('Escape');
        await page.waitForTimeout(500);

        // Check if modal closed immediately without confirm
        const isModalVisible = await page.isVisible('[class*="modal"], [role="dialog"]');
        console.log(`[TC-G03] Modal still visible after ESC: ${isModalVisible}`);
        await takeEvidence(page, 'QA-G03_modal_esc_test');

        if (!isModalVisible) {
          findings.push({
            id: 'QA-G03-MODAL-DATALOSS',
            severity: 'P2',
            title: 'Mất dữ liệu form khi đóng Modal bằng phím ESC không có cảnh báo xác nhận',
            screen: '/classes (Create Class Modal)',
            details: 'Người dùng nhập dở nội dung vào modal, nhấn phím ESC thì modal tự đóng ngay lập tức và xóa sạch dữ liệu vừa nhập mà không hỏi xác nhận.'
          });
        }
      }
    }

  } finally {
    await desktopCtx.browser.close();
  }

  console.log('\nGroup G Completed with findings:', findings.length);
  return findings;
}

runGroupG();
