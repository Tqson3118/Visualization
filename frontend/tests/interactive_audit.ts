import { chromium, type Page } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';
import { mockApi } from './e2e/helpers/mockApi';
import { loginViaUi } from './e2e/helpers/auth';

interface Finding {
  category: 'BUG' | 'UX_ISSUE' | 'ACCESSIBILITY' | 'RESPONSIVE_OVERFLOW' | 'CONSOLE_WARNING';
  severity: 'HIGH' | 'MEDIUM' | 'LOW';
  title: string;
  description: string;
  route: string;
  selector?: string;
  details?: any;
}

const findings: Finding[] = [];
const consoleErrors: string[] = [];
const consoleWarns: string[] = [];

async function runInteractiveAudit() {
  console.log('🚀 Bắt đầu Interactive Browser Audit trên http://localhost:5175...');

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    baseURL: 'http://localhost:5175',
    viewport: { width: 1440, height: 900 },
  });

  await context.addCookies([
    {
      name: 'dsa.session',
      value: '1',
      domain: 'localhost',
      path: '/',
    },
  ]);

  const page = await context.newPage();

  // Listen to console
  page.on('console', (msg) => {
    if (msg.type() === 'error') {
      consoleErrors.push(`[Console Error] ${msg.text()} (${msg.location().url}:${msg.location().lineNumber})`);
    } else if (msg.type() === 'warning') {
      consoleWarns.push(`[Console Warn] ${msg.text()}`);
    }
  });

  page.on('pageerror', (err) => {
    consoleErrors.push(`[Unhandled Exception] ${err.message}\n${err.stack}`);
  });

  // ─────────────────────────────────────────────────────────────
  // 1. Audit Auth & Login Flow
  // ─────────────────────────────────────────────────────────────
  console.log('📋 Kiểm tra 1: Auth & Login Flow...');
  const { loginViaUi } = await import('./e2e/helpers/auth');
  await loginViaUi(page, 'teacher@test.edu', 'Teacher@123', { role: 'TEACHER' });
  console.log(`Current URL sau login: ${page.url()}`);

  // ─────────────────────────────────────────────────────────────
  // 2. Audit Studio Shell & Tab Navigation (`/studio` & `/admin/content`)
  // ─────────────────────────────────────────────────────────────
  console.log('📋 Kiểm tra 2: Studio Shell & Tab Navigation...');
  await page.goto('http://localhost:5175/admin/content');
  await page.waitForLoadState('networkidle');

  // Check element count & render time
  const studioBody = await page.evaluate(() => document.body.innerHTML);
  if (!studioBody || studioBody.length < 100) {
    findings.push({
      category: 'BUG',
      severity: 'HIGH',
      title: 'Studio Shell không render nội dung khi truy cập /admin/content',
      description: 'Trang /admin/content bị rỗng hoặc không tải được StudioShell.',
      route: '/admin/content',
    });
  }

  // Check Tab Bar Accessibility
  const tabs = page.locator('[role="tab"], [data-testid^="tab-"]');
  const tabCount = await tabs.count();
  if (tabCount < 4) {
    findings.push({
      category: 'UX_ISSUE',
      severity: 'MEDIUM',
      title: 'Thiếu tabs trên Studio navigation',
      description: `Chỉ tìm thấy ${tabCount} tabs (kỳ vọng 4 tabs: overview, curriculum, exercises, feedback).`,
      route: '/admin/content',
    });
  }

  // Test Tab Switching and URL sync
  console.log('Testing Tab switching & URL sync...');
  await page.click('[data-testid="tab-curriculum"]');
  await page.waitForTimeout(300);
  const urlAfterCurriculum = page.url();
  const isCurriculumSelected = await page.getAttribute('[data-testid="tab-curriculum"]', 'aria-selected');
  if (!urlAfterCurriculum.includes('tab=curriculum') || isCurriculumSelected !== 'true') {
    findings.push({
      category: 'BUG',
      severity: 'HIGH',
      title: 'Studio Navigation không chọn tab Curriculum đúng',
      description: `URL: ${urlAfterCurriculum}, aria-selected: ${isCurriculumSelected}`,
      route: '/admin/content',
    });
  }

  await page.click('[data-testid="tab-exercises"]');
  await page.waitForTimeout(300);
  const urlAfterExercises = page.url();
  const isExercisesSelected = await page.getAttribute('[data-testid="tab-exercises"]', 'aria-selected');
  if (!urlAfterExercises.includes('tab=exercises') || isExercisesSelected !== 'true') {
    findings.push({
      category: 'BUG',
      severity: 'HIGH',
      title: 'Studio Navigation không chọn tab Exercises đúng',
      description: `URL: ${urlAfterExercises}, aria-selected: ${isExercisesSelected}`,
      route: '/admin/content',
    });
  }

  // ─────────────────────────────────────────────────────────────
  // 3. Audit TeacherGlossaryBanner & localStorage
  // ─────────────────────────────────────────────────────────────
  console.log('📋 Kiểm tra 3: TeacherGlossaryBanner...');
  await page.goto('http://localhost:5175/admin/content?tab=curriculum');
  await page.waitForTimeout(300);

  const banner = page.locator('[data-testid="teacher-glossary-banner"]');
  const isBannerVisible = await banner.isVisible();

  if (isBannerVisible) {
    // Click dismiss button
    const dismissBtn = page.locator('[data-testid="btn-dismiss-banner"]');
    await dismissBtn.click();
    await page.waitForTimeout(200);

    const bannerAfterDismiss = await banner.isVisible();
    const storedVal = await page.evaluate(() => localStorage.getItem('teacher_glossary_banner_dismissed_v1'));

    if (bannerAfterDismiss) {
      findings.push({
        category: 'BUG',
        severity: 'MEDIUM',
        title: 'Nút Dismiss banner không ẩn banner ngay lập tức',
        description: 'Sau khi click nút đóng, banner vẫn còn trên DOM.',
        route: '/admin/content?tab=curriculum',
      });
    }
    if (storedVal !== 'true') {
      findings.push({
        category: 'BUG',
        severity: 'MEDIUM',
        title: 'Trạng thái Dismiss banner không được lưu vào localStorage',
        description: `Giá trị trong localStorage: ${storedVal}`,
        route: '/admin/content?tab=curriculum',
      });
    }
  }

  // ─────────────────────────────────────────────────────────────
  // 4. Audit RejectReasonModal (Input validation, length, keyboard)
  // ─────────────────────────────────────────────────────────────
  console.log('📋 Kiểm tra 4: RejectReasonModal Validation & Keyboard Accessibility...');
  // Switch to Admin role to see Pending Courses
  await mockApi(page, { role: 'ADMIN', email: 'admin@test.edu', displayName: 'Quản trị viên' });
  await page.goto('http://localhost:5175/admin/content?tab=curriculum');
  await page.waitForTimeout(500);

  const rejectBtn = page.locator('button:has-text("Từ chối")').first();
  if (await rejectBtn.isVisible()) {
    await rejectBtn.click();
    await page.waitForTimeout(300);

    const modal = page.locator('[data-testid="reject-modal"]');
    const textarea = page.locator('[data-testid="reject-reason-input"]');
    const confirmBtn = page.locator('[data-testid="btn-confirm-reject"]');
    const cancelBtn = page.locator('[data-testid="btn-cancel-reject"]');

    // Test 4.1: Empty reason disables submit
    const isSubmitDisabledWhenEmpty = await confirmBtn.isDisabled();
    if (!isSubmitDisabledWhenEmpty) {
      findings.push({
        category: 'BUG',
        severity: 'MEDIUM',
        title: 'RejectReasonModal cho phép submit khi ô lý do trống',
        description: 'Nút Xác nhận từ chối không bị disabled khi chưa nhập lý do.',
        route: '/admin/content?tab=curriculum',
      });
    }

    // Test 4.2: Whitespace-only reason validation
    await textarea.fill('    ');
    const isSubmitDisabledWhitespace = await confirmBtn.isDisabled();
    if (!isSubmitDisabledWhitespace) {
      findings.push({
        category: 'UX_ISSUE',
        severity: 'LOW',
        title: 'RejectReasonModal cho phép submit toàn khoảng trắng',
        description: 'Lý do chỉ chứa dấu cách vẫn kích hoạt nút xác nhận.',
        route: '/admin/content?tab=curriculum',
      });
    }

    // Test 4.3: Overflow test with 1000 characters
    const longText = 'A'.repeat(1200);
    await textarea.fill(longText);
    const hasModalOverflowed = await page.evaluate(() => {
      const modalEl = document.querySelector('[data-testid="reject-modal"]');
      if (!modalEl) return false;
      return modalEl.scrollHeight > window.innerHeight;
    });
    if (hasModalOverflowed) {
      findings.push({
        category: 'UX_ISSUE',
        severity: 'LOW',
        title: 'RejectReasonModal bị tràn màn hình khi nhập văn bản rất dài',
        description: 'Modal không giới hạn chiều cao tối đa (max-h) và thanh cuộn khi văn bản dài.',
        route: '/admin/content?tab=curriculum',
      });
    }

    // Test 4.4: Close modal
    await cancelBtn.click();
    await page.waitForTimeout(200);
  }

  // ─────────────────────────────────────────────────────────────
  // 5. Audit Studio Exercises Search & Filter
  // ─────────────────────────────────────────────────────────────
  console.log('📋 Kiểm tra 5: Studio Exercises Search & Special Characters...');
  await page.goto('http://localhost:5175/admin/content?tab=exercises');
  await page.waitForTimeout(400);

  const searchInput = page.locator('input[placeholder*="Tìm bài tập"]').first();
  if (await searchInput.isVisible()) {
    // Test special regex chars: [, *, +, (, \, $, ^
    try {
      await searchInput.fill('[abc\\*+');
      await page.waitForTimeout(300);
      const isExercisesTabAlive = await page.locator('[data-testid="studio-exercises-tab"]').isVisible();
      if (!isExercisesTabAlive) {
        findings.push({
          category: 'BUG',
          severity: 'HIGH',
          title: 'Tìm kiếm bài tập bị crash khi nhập ký tự đặc biệt Regex',
          description: 'Nhập các ký tự [ * + \\ gây lỗi RegExp không hợp lệ.',
          route: '/admin/content?tab=exercises',
        });
      }
    } catch (err) {
      findings.push({
        category: 'BUG',
        severity: 'HIGH',
        title: 'Tìm kiếm bài tập phát sinh ngoại lệ',
        description: String(err),
        route: '/admin/content?tab=exercises',
      });
    }
  }

  // ─────────────────────────────────────────────────────────────
  // 6. Audit Classroom & Curriculum Builder (`/classes/7`)
  // ─────────────────────────────────────────────────────────────
  console.log('📋 Kiểm tra 6: Classroom & Curriculum Builder...');
  await mockApi(page, { role: 'TEACHER', email: 'teacher@test.edu', displayName: 'Giảng viên' });
  await page.goto('http://localhost:5175/classes/7');
  await page.waitForTimeout(500);

  const curriculumTab = page.locator('[role="tab"]:has-text("Lộ trình"), button:has-text("Lộ trình học")').first();
  if (await curriculumTab.isVisible()) {
    await curriculumTab.click();
    await page.waitForTimeout(300);

    // Open Builder Panel
    const addContentBtn = page.locator('button:has-text("Thêm bài vào lộ trình"), button:has-text("Thêm từng bài học")').first();
    if (await addContentBtn.isVisible()) {
      await addContentBtn.click();
      await page.waitForTimeout(300);

      // Check modal layout
      const builderModal = page.locator('.curriculum-builder-panel, [role="dialog"]').first();
      const isBuilderModalVisible = await builderModal.isVisible();

      if (!isBuilderModalVisible) {
        findings.push({
          category: 'BUG',
          severity: 'HIGH',
          title: 'CurriculumBuilderPanel không mở khi click nút Thêm bài vào lộ trình',
          description: 'Modal builder không xuất hiện.',
          route: '/classes/7',
        });
      } else {
        // Test Select All Topic
        const selectAllTopicBtn = page.locator('[data-testid="btn-select-all-topic"]').first();
        if (await selectAllTopicBtn.isVisible()) {
          await selectAllTopicBtn.click();
          await page.waitForTimeout(200);

          // Check if disabled items were accidentally selected
          const disabledCheckedCount = await page.evaluate(() => {
            const disabledCheckboxes = Array.from(document.querySelectorAll('input[type="checkbox"]:disabled'));
            return disabledCheckboxes.filter((cb: any) => cb.checked).length;
          });

          if (disabledCheckedCount > 0) {
            findings.push({
              category: 'BUG',
              severity: 'MEDIUM',
              title: 'Nút "Chọn tất cả chương" chọn nhầm các bài đã có trong lớp (disabled)',
              description: `Đã có ${disabledCheckedCount} bài bị disabled nhưng vẫn bị đánh dấu checked.`,
              route: '/classes/7',
            });
          }
        }

        // Close builder panel
        const closeBtn = page.locator('button:has-text("Hủy"), [aria-label="Close"]').first();
        if (await closeBtn.isVisible()) {
          await closeBtn.click();
          await page.waitForTimeout(200);
        }
      }
    }
  }

  // ─────────────────────────────────────────────────────────────
  // 7. Audit Responsive Overflow across Mobile (390px), Tablet (768px), Desktop (1440px)
  // ─────────────────────────────────────────────────────────────
  console.log('📋 Kiểm tra 7: Responsive Layout & Horizontal Overflow...');
  const viewports = [
    { name: 'Mobile (390x844)', width: 390, height: 844 },
    { name: 'Tablet (768x1024)', width: 768, height: 1024 },
    { name: 'Desktop (1440x900)', width: 1440, height: 900 },
  ];

  const routesToTest = [
    '/admin/content?tab=overview',
    '/admin/content?tab=curriculum',
    '/admin/content?tab=exercises',
    '/admin/content?tab=feedback',
    '/classes/7',
  ];

  for (const vp of viewports) {
    await page.setViewportSize({ width: vp.width, height: vp.height });
    for (const r of routesToTest) {
      await page.goto(`http://localhost:5175${r}`);
      await page.waitForTimeout(400);

      // Check horizontal overflow
      const overflow = await page.evaluate(() => {
        const docWidth = document.documentElement.scrollWidth;
        const clientWidth = document.documentElement.clientWidth;
        return {
          hasOverflow: docWidth > clientWidth + 1,
          scrollWidth: docWidth,
          clientWidth: clientWidth,
        };
      });

      if (overflow.hasOverflow) {
        findings.push({
          category: 'RESPONSIVE_OVERFLOW',
          severity: 'HIGH',
          title: `Tràn ngang màn hình (${overflow.scrollWidth}px > ${overflow.clientWidth}px) trên ${vp.name}`,
          description: `Giao diện bị thanh cuộn ngang gây vỡ bố cục trên thiết bị ${vp.name}.`,
          route: r,
          details: overflow,
        });
      }
    }
  }

  // ─────────────────────────────────────────────────────────────
  // 8. Audit Console Errors
  // ─────────────────────────────────────────────────────────────
  console.log('📋 Kiểm tra 8: Console Errors & Reactivity Warnings...');
  for (const err of consoleErrors) {
    findings.push({
      category: 'BUG',
      severity: 'HIGH',
      title: 'Console Error phát hiện trong quá trình tương tác',
      description: err,
      route: 'Console',
    });
  }

  for (const warn of consoleWarns) {
    if (warn.includes('[Vue warn]') || warn.includes('Unhandled') || warn.includes('Failed to load resource')) {
      findings.push({
        category: 'CONSOLE_WARNING',
        severity: 'LOW',
        title: 'Vue Warning / Cảnh báo runtime',
        description: warn,
        route: 'Console',
      });
    }
  }

  await browser.close();

  // Save findings report
  const reportPath = path.join(process.cwd(), 'tests', 'interactive_audit_report.json');
  fs.writeFileSync(reportPath, JSON.stringify(findings, null, 2), 'utf-8');
  console.log(`\n✅ Hoàn thành audit! Tìm thấy ${findings.length} vấn đề. Báo cáo lưu tại ${reportPath}`);
  console.log(JSON.stringify(findings, null, 2));
}

runInteractiveAudit().catch((err) => {
  console.error('Lỗi khi chạy audit script:', err);
  process.exit(1);
});
