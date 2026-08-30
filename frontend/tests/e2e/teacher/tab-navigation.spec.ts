import { test, expect } from '../fixtures/auth';

test.describe('Test Suite 1 — Shell & Tab Navigation', () => {
  test('TC-TF-001: Shell render đúng — không còn God Component', async ({ adminPage }) => {
    const startTime = Date.now();
    await adminPage.goto('/admin/content');
    await expect(adminPage.locator('aside, nav[aria-label="Studio navigation"]').first()).toBeVisible({ timeout: 5000 });
    const loadDuration = Date.now() - startTime;
    expect(loadDuration).toBeLessThan(5000);

    // Tab bar hiển thị 4 tabs
    const overviewTab = adminPage.getByTestId('tab-overview');
    const curriculumTab = adminPage.getByTestId('tab-curriculum');
    const exercisesTab = adminPage.getByTestId('tab-exercises');
    const feedbackTab = adminPage.getByTestId('tab-feedback');

    await expect(overviewTab).toBeVisible();
    await expect(curriculumTab).toBeVisible();
    await expect(exercisesTab).toBeVisible();
    await expect(feedbackTab).toBeVisible();

    // Tab "Tổng quan" active mặc định
    await expect(overviewTab).toHaveAttribute('aria-selected', 'true');

    // Không phải God Component: kiểm tra tổng element count
    const elementCount = await adminPage.locator('*').count();
    expect(elementCount).toBeLessThan(1500);
  });

  test('TC-TF-002: Tab switching cập nhật URL query', async ({ adminPage }) => {
    await adminPage.goto('/admin/content?tab=overview');

    // Click tab Giáo trình
    await adminPage.getByTestId('tab-curriculum').click();
    await expect(adminPage).toHaveURL(/tab=curriculum/);
    await expect(adminPage.locator('.curriculum-tree, .chapter-card, [data-testid="teacher-glossary-banner"]').first()).toBeVisible();

    // Click tab Bài tập
    await adminPage.getByTestId('tab-exercises').click();
    await expect(adminPage).toHaveURL(/tab=exercises/);
    await expect(adminPage.getByTestId('studio-exercises-tab')).toBeVisible();
  });

  test('TC-TF-003: Deep link qua URL query', async ({ adminPage }) => {
    // Deep link trực tiếp tới feedback
    await adminPage.goto('/admin/content?tab=feedback');
    await expect(adminPage.getByTestId('tab-feedback')).toHaveAttribute('aria-selected', 'true');
    await expect(adminPage).toHaveURL(/tab=feedback/);
    await expect(adminPage.getByTestId('studio-feedback-tab')).toBeVisible();

    // Deep link với query không hợp lệ -> fallback về overview
    await adminPage.goto('/admin/content?tab=invalid_tab');
    await expect(adminPage.getByTestId('tab-overview')).toHaveAttribute('aria-selected', 'true');
  });

  test('TC-TF-004: Tab state không bị crash khi navigate qua lại', async ({ adminPage }) => {
    await adminPage.goto('/admin/content?tab=curriculum');
    await expect(adminPage.getByTestId('tab-curriculum')).toHaveAttribute('aria-selected', 'true');

    // Navigate sang profile
    await adminPage.goto('/profile');
    await expect(adminPage.locator('.profile, .profile__user, header').first()).toBeVisible();

    // Navigate back to /admin/content
    await adminPage.goto('/admin/content');
    await expect(adminPage.getByTestId('tab-overview')).toBeVisible();
    await expect(adminPage.locator('body')).not.toContainText('Uncaught error');
  });
});
