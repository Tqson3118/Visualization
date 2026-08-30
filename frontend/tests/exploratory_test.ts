import { chromium, type Page } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';
import { mockApi } from './e2e/helpers/mockApi';
import { loginViaUi } from './e2e/helpers/auth';

interface Issue {
  id: string;
  module: string;
  type: 'BUG' | 'UX_DEFECT' | 'ACCESSIBILITY' | 'EDGE_CASE' | 'LAYOUT';
  severity: 'CRITICAL' | 'MAJOR' | 'MINOR' | 'COSMETIC';
  summary: string;
  stepsToReproduce: string;
  observedBehavior: string;
  expectedBehavior: string;
}

const issues: Issue[] = [];

async function explore() {
  console.log('🔍 Bắt đầu Exploratory & Interactive Testing...');

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    baseURL: 'http://localhost:5175',
    viewport: { width: 1440, height: 900 },
  });

  await context.addCookies([
    { name: 'dsa.session', value: '1', domain: 'localhost', path: '/' },
  ]);

  const page = await context.newPage();

  // ─────────────────────────────────────────────────────────────
  // SCENARIO 1: Teacher Studio - Overview & Quick Actions
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- KỊCH BẢN 1: Studio Overview & Quick Actions ---');
  await loginViaUi(page, 'teacher@test.edu', 'Teacher@123', { role: 'TEACHER' });
  await page.goto('/admin/content?tab=overview');
  await page.waitForTimeout(500);

  // 1.1 Kiểm tra các nút Quick Actions trong Overview
  const openStudioBtn = page.getByRole('button', { name: /Mở Studio biên soạn/i }).first();
  if (await openStudioBtn.isVisible()) {
    await openStudioBtn.click();
    await page.waitForTimeout(300);
    if (!page.url().includes('tab=curriculum')) {
      issues.push({
        id: 'ISSUE-01',
        module: 'Studio Overview',
        type: 'UX_DEFECT',
        severity: 'MINOR',
        summary: 'Nút "Mở Studio biên soạn" không chuyển đúng URL tab=curriculum',
        stepsToReproduce: '1. Vào /admin/content?tab=overview\n2. Bấm "Mở Studio biên soạn"',
        observedBehavior: `URL hiện tại: ${page.url()}`,
        expectedBehavior: 'URL cập nhật thành /admin/content?tab=curriculum',
      });
    }
  }

  // ─────────────────────────────────────────────────────────────
  // SCENARIO 2: Teacher Studio - Curriculum & Chapter/Lesson Actions
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- KỊCH BẢN 2: Studio Curriculum & Chapter/Lesson Actions ---');
  await page.goto('/admin/content?tab=curriculum');
  await page.waitForTimeout(500);

  // 2.1 Kiểm tra TeacherGlossaryBanner trên màn hình nhỏ (Mobile 375px)
  await page.setViewportSize({ width: 375, height: 667 });
  await page.waitForTimeout(300);
  const bannerOnMobile = page.locator('[data-testid="teacher-glossary-banner"]');
  if (await bannerOnMobile.isVisible()) {
    const isBannerOverflown = await page.evaluate(() => {
      const banner = document.querySelector('[data-testid="teacher-glossary-banner"]');
      if (!banner) return false;
      return banner.scrollWidth > document.documentElement.clientWidth;
    });
    if (isBannerOverflown) {
      issues.push({
        id: 'ISSUE-02',
        module: 'Glossary Banner',
        type: 'LAYOUT',
        severity: 'MINOR',
        summary: 'TeacherGlossaryBanner bị tràn ngang trên màn hình mobile 375px',
        stepsToReproduce: '1. Đặt viewport 375x667\n2. Mở tab curriculum',
        observedBehavior: 'Banner có độ rộng scrollWidth lớn hơn clientWidth',
        expectedBehavior: 'Banner co giãn vừa vặn màn hình mobile',
      });
    }
  }
  // Restore desktop viewport
  await page.setViewportSize({ width: 1440, height: 900 });

  // 2.2 Kiểm tra bộ chọn Giáo trình trong Tab Curriculum
  const courseSelector = page.locator('.course-bar select, .course-bar__select').first();
  if (await courseSelector.isVisible()) {
    const courseOptions = await courseSelector.locator('option').count();
    console.log(`Số lượng giáo trình trong selector: ${courseOptions}`);
  }

  // 2.3 Kiểm tra hành vi khi bấm "Tạo bài học mới"
  const createLessonBtn = page.getByRole('button', { name: /Tạo bài học|Tạo bài mới/i }).first();
  if (await createLessonBtn.isVisible()) {
    await createLessonBtn.click();
    await page.waitForTimeout(400);

    const lessonModal = page.locator('[role="dialog"], .modal, .modal-card').first();
    const isModalOpen = await lessonModal.isVisible();
    if (!isModalOpen) {
      issues.push({
        id: 'ISSUE-03',
        module: 'Studio Curriculum',
        type: 'BUG',
        severity: 'MAJOR',
        summary: 'Bấm nút "Tạo bài học" không mở modal tạo bài',
        stepsToReproduce: '1. Vào /admin/content?tab=curriculum\n2. Bấm "Tạo bài học"',
        observedBehavior: 'Không có modal nào xuất hiện',
        expectedBehavior: 'Modal tạo bài học mở ra',
      });
    } else {
      // Đóng modal
      const closeBtn = lessonModal.getByRole('button', { name: /Hủy|Đóng/i }).first();
      if (await closeBtn.isVisible()) await closeBtn.click();
    }
  }

  // ─────────────────────────────────────────────────────────────
  // SCENARIO 3: Review Flow (Admin role) & Reject Modal Edge Cases
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- KỊCH BẢN 3: Admin Review & Reject Modal Edge Cases ---');
  await mockApi(page, { role: 'ADMIN', email: 'admin@test.edu', displayName: 'Quản trị viên' });
  await page.goto('/admin/content?tab=curriculum');
  await page.waitForTimeout(500);

  // 3.1 Reject Course Modal interaction
  const rejectCourseBtn = page.getByRole('button', { name: /Từ chối/i }).first();
  if (await rejectCourseBtn.isVisible()) {
    await rejectCourseBtn.click();
    await page.waitForTimeout(300);

    const rejectModal = page.getByTestId('reject-modal');
    const rejectTextarea = page.getByTestId('reject-reason-input');
    const confirmBtn = page.getByTestId('btn-confirm-reject');

    // Edge case: Nhập khoảng trắng và thử submit
    await rejectTextarea.fill('     \n\t   ');
    const isConfirmDisabled = await confirmBtn.isDisabled();
    if (!isConfirmDisabled) {
      issues.push({
        id: 'ISSUE-04',
        module: 'RejectReasonModal',
        type: 'EDGE_CASE',
        severity: 'MINOR',
        summary: 'RejectReasonModal cho phép xác nhận khi lý do chỉ toàn khoảng trắng hoặc xuống dòng',
        stepsToReproduce: '1. Mở modal từ chối\n2. Nhập dấu cách/tab\n3. Quan sát nút Xác nhận',
        observedBehavior: 'Nút Xác nhận vẫn sáng (enabled)',
        expectedBehavior: 'Nút Xác nhận bị vô hiệu hóa vì .trim() lý do là rỗng',
      });
    }

    // Edge case: Nhấn Enter không có Shift trong ô textarea
    await rejectTextarea.fill('Lý do thử nghiệm');
    await rejectTextarea.press('Enter');
    // Modal không được tự động submit khi chưa bấm nút
    const isModalStillOpen = await rejectModal.isVisible();
    if (!isModalStillOpen) {
      issues.push({
        id: 'ISSUE-05',
        module: 'RejectReasonModal',
        type: 'UX_DEFECT',
        severity: 'MINOR',
        summary: 'Nhấn phím Enter trong Textarea tự động submit form thay vì xuống dòng',
        stepsToReproduce: '1. Nhập lý do\n2. Nhấn Enter',
        observedBehavior: 'Form tự động submit và đóng modal',
        expectedBehavior: 'Textarea phải xuống dòng mới khi nhấn Enter',
      });
    } else {
      // Đóng modal
      await page.getByTestId('btn-cancel-reject').click();
    }
  }

  // ─────────────────────────────────────────────────────────────
  // SCENARIO 4: Studio Exercises Tab - CSV & Search
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- KỊCH BẢN 4: Studio Exercises CSV & Filters ---');
  await page.goto('/admin/content?tab=exercises');
  await page.waitForTimeout(500);

  // 4.1 Filter switching
  const filterMcqBtn = page.locator('button:has-text("Trắc nghiệm MCQ")').first();
  const filterCodeBtn = page.locator('button:has-text("Code Lab")').first();
  const filterAllBtn = page.locator('button:has-text("Tất cả")').first();

  if (await filterMcqBtn.isVisible()) {
    await filterMcqBtn.click();
    await page.waitForTimeout(200);
    await filterCodeBtn.click();
    await page.waitForTimeout(200);
    await filterAllBtn.click();
    await page.waitForTimeout(200);
  }

  // 4.2 CSV sample download
  const downloadCsvBtn = page.locator('button:has-text("File mẫu CSV")').first();
  if (await downloadCsvBtn.isVisible()) {
    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 3000 }).catch(() => [null]),
      downloadCsvBtn.click(),
    ]);
    if (download) {
      console.log(`Đã tải file CSV: ${download.suggestedFilename()}`);
    }
  }

  // ─────────────────────────────────────────────────────────────
  // SCENARIO 5: Studio Feedback Tab
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- KỊCH BẢN 5: Studio Feedback Tab ---');
  await page.goto('/admin/content?tab=feedback');
  await page.waitForTimeout(500);

  const feedbackRows = page.locator('.feedback-item, .card, [data-testid="studio-feedback-tab"] > div');
  const count = await feedbackRows.count();
  console.log(`Số phản hồi hiển thị: ${count}`);

  // ─────────────────────────────────────────────────────────────
  // SCENARIO 6: Class Curriculum Builder & Drag-and-Drop (`/classes/7`)
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- KỊCH BẢN 6: Classroom Curriculum Builder & Drag Reorder ---');
  await mockApi(page, { role: 'TEACHER', email: 'teacher@test.edu', displayName: 'Giảng viên' });
  await page.goto('/classes/7');
  await page.waitForTimeout(500);

  // Switch to curriculum tab
  const classCurriculumTab = page.getByRole('tab', { name: /Lộ trình/i }).or(page.getByText('Lộ trình học')).first();
  if (await classCurriculumTab.isVisible()) {
    await classCurriculumTab.click();
    await page.waitForTimeout(300);

    // 6.1 Test CurriculumBuilderPanel
    const addCurriculumBtn = page.getByRole('button', { name: /Thêm bài vào lộ trình|Thêm từng bài học/i }).first();
    if (await addCurriculumBtn.isVisible()) {
      await addCurriculumBtn.click();
      await page.waitForTimeout(300);

      // Check deadline toggle
      const deadlineToggle = page.getByTestId('toggle-deadline');
      const deadlineInput = page.getByTestId('input-deadline');

      await deadlineToggle.check();
      await page.waitForTimeout(200);

      // Edge case: Đặt deadline về quá khứ
      const isDeadlineInputVisible = await deadlineInput.isVisible();
      if (isDeadlineInputVisible) {
        await deadlineInput.fill('2020-01-01T00:00');
        // Check submit button
        const lesson3Checkbox = page.getByTestId('checkbox-lesson-3').first();
        if (await lesson3Checkbox.isVisible()) {
          await lesson3Checkbox.check();
          const submitBtn = page.getByTestId('btn-add-curriculum');
          const isSubmitEnabled = await submitBtn.isEnabled();
          // Deadline in past is allowed by client picker (backend may validate)
          console.log(`Submit enabled with past deadline: ${isSubmitEnabled}`);
        }
      }

      // Close modal
      const cancelBuilderBtn = page.locator('button:has-text("Hủy")').last();
      if (await cancelBuilderBtn.isVisible()) await cancelBuilderBtn.click();
    }

    // 6.2 Test Move Up / Move Down buttons on Drag List
    const moveUpBtns = page.getByTestId('btn-move-up');
    const moveDownBtns = page.getByTestId('btn-move-down');

    // First item move-up must be disabled
    if (await moveUpBtns.count() > 0) {
      const firstMoveUpDisabled = await moveUpBtns.first().isDisabled();
      if (!firstMoveUpDisabled) {
        issues.push({
          id: 'ISSUE-06',
          module: 'CurriculumDragList',
          type: 'BUG',
          severity: 'MINOR',
          summary: 'Nút Move Up của bài học đầu tiên không bị disabled',
          stepsToReproduce: '1. Vào chi tiết lớp\n2. Mở tab Lộ trình\n3. Quan sát nút mũi tên lên của bài đầu tiên',
          observedBehavior: 'Nút Move Up vẫn bấm được',
          expectedBehavior: 'Nút Move Up của bài đầu tiên phải bị disabled',
        });
      }
    }

    // Last item move-down must be disabled
    if (await moveDownBtns.count() > 0) {
      const lastMoveDownDisabled = await moveDownBtns.last().isDisabled();
      if (!lastMoveDownDisabled) {
        issues.push({
          id: 'ISSUE-07',
          module: 'CurriculumDragList',
          type: 'BUG',
          severity: 'MINOR',
          summary: 'Nút Move Down của bài học cuối cùng không bị disabled',
          stepsToReproduce: '1. Vào chi tiết lớp\n2. Mở tab Lộ trình\n3. Quan sát nút mũi tên xuống của bài cuối cùng',
          observedBehavior: 'Nút Move Down vẫn bấm được',
          expectedBehavior: 'Nút Move Down của bài cuối cùng phải bị disabled',
        });
      }
    }
  }

  // ─────────────────────────────────────────────────────────────
  // SCENARIO 7: Mobile Navigation (375px) on Classroom & Studio
  // ─────────────────────────────────────────────────────────────
  console.log('\n--- KỊCH BẢN 7: Mobile Responsiveness & Navigation ---');
  await page.setViewportSize({ width: 375, height: 812 });
  await page.goto('/admin/content?tab=overview');
  await page.waitForTimeout(400);

  // Check if tabs are accessible on mobile
  const mobileTabOverview = page.getByTestId('tab-overview');
  const mobileTabFeedback = page.getByTestId('tab-feedback');

  const isOverviewVisible = await mobileTabOverview.isVisible();
  const isFeedbackVisible = await mobileTabFeedback.isVisible();

  if (!isOverviewVisible || !isFeedbackVisible) {
    issues.push({
      id: 'ISSUE-08',
      module: 'Studio Shell',
      type: 'LAYOUT',
      severity: 'MINOR',
      summary: 'Các tab Studio bị khuất trên màn hình mobile 375px nếu không có thanh cuộn ngang hỗ trợ',
      stepsToReproduce: '1. Đặt viewport 375px\n2. Vào /admin/content',
      observedBehavior: `Overview: ${isOverviewVisible}, Feedback: ${isFeedbackVisible}`,
      expectedBehavior: 'Tất cả 4 tabs phải hiển thị hoặc cho phép vuốt ngang dễ dàng',
    });
  }

  await browser.close();

  // Save report
  const reportPath = path.join(process.cwd(), 'tests', 'exploratory_findings.json');
  fs.writeFileSync(reportPath, JSON.stringify(issues, null, 2), 'utf-8');
  console.log(`\n======================================================`);
  console.log(`🏁 KẾT QUẢ KHẢO SÁT TRỰC TIẾP TRÊN TRÌNH DUYỆT:`);
  console.log(`Tổng số vấn đề phát hiện: ${issues.length}`);
  console.log(JSON.stringify(issues, null, 2));
  console.log(`======================================================\n`);
}

explore().catch((err) => {
  console.error('Lỗi khi chạy exploratory test:', err);
  process.exit(1);
});
