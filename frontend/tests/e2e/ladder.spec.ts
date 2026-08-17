/**
 * ladder.spec.ts — TEST-UI-005 (guard /ladder/*) + TEST-UI-007 (một phần: stepper 3 bậc)
 * + FR-10.1 (trừ tim khi vào node mới — mock POST .../enter trả heartsLeft giảm).
 *
 * Luồng mock:
 *   - LadderView onMounted → lessonStore.fetchTopics() → GET /topics.
 *   - FR-10.1: đi qua luồng THẬT trên UI — PathView.startNode() gọi
 *     POST /learning-path/{id}/nodes/{nodeId}/enter → mock trả heartsLeft = hearts - 1
 *     (stateful trong helpers/mockApi.ts) → gamificationStore.hearts cập nhật →
 *     widget tim header (HeartsGemsWidget, aria-label "Tim: X/Y") hiển thị giảm.
 *
 * ⚠ TEST-UI-007 đầy đủ (Quiz ≥60% → Lab → Code ≥70%): LadderView hiện truyền
 *   quiz-exercise=null / code-exercise-id=null → KHÔNG có dữ liệu quiz/code để pass qua UI.
 *   Chờ task gắn dữ liệu vào LadderView.
 */
import { expect, test } from '@playwright/test';

import { E2E_EMAIL, E2E_PASSWORD } from './helpers/auth';
import { mockApi } from './helpers/mockApi';

test.describe('Ladder — Màn 14', () => {
  test('guard chưa đăng nhập → /login?redirect=... → sau login hiển thị stepper 3 bậc (TEST-UI-005/007)', async ({ page }) => {
    await mockApi(page);
    await page.goto('/ladder/1');

    // Guard requiresAuth → login kèm redirect
    await expect(page).toHaveURL(/\/login\?redirect=/);

    // Login ngay → quay lại /ladder/1
    await page.locator('#email').fill(E2E_EMAIL);
    await page.locator('#password').fill(E2E_PASSWORD);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    await expect(page).toHaveURL(/\/ladder\/1$/);

    await expect(page.getByRole('heading', { name: /Practice Ladder/ })).toBeVisible();

    // Stepper 3 bậc: Quiz (active) → Lab/Code (locked — chưa pass Quiz)
    const stepper = page.getByRole('list', { name: 'Các bậc luyện tập' });
    await expect(stepper).toBeVisible();
    await expect(stepper.getByText('Quiz')).toBeVisible();
    await expect(stepper.getByText('Lab')).toBeVisible();
    await expect(stepper.getByText('Code')).toBeVisible();

    await expect(page.getByRole('button', { name: /Quiz/ })).toBeEnabled();
    // Scope theo stepper — ngoài stepper còn nút "Mở Lab trực tiếp →" khớp /Lab/
    await expect(stepper.getByRole('button', { name: /Lab/ })).toBeDisabled();
    await expect(stepper.getByRole('button', { name: /Code/ })).toBeDisabled();
  });

  test('FR-10.1: hearts widget 10/10 + routing contract /path → /courses (PR30)', async ({ page }) => {
    await mockApi(page);
    await page.goto('/login');
    await page.locator('#email').fill(E2E_EMAIL);
    await page.locator('#password').fill(E2E_PASSWORD);
    await page.getByRole('button', { name: 'Đăng nhập', exact: true }).click();
    // PR30: /path redirect → /courses (roadmap cũ thay bằng Grokking courses)
    await expect(page).toHaveURL(/\/courses$/);

    // Header: HeartsGemsWidget fetch GET /me/hearts (mock) → 10/10
    await expect(page.getByLabel('Tim: 10/10')).toBeVisible();

    // PR30: route cũ /path/{id} (bản đồ node + enter trừ tim qua UI PathView) redirect sang
    // course-detail — luồng UI cũ đã bị thay bằng courses; contract API
    // POST /learning-path/{id}/nodes/{nodeId}/enter vẫn giữ (xem gamification.pr30.spec.ts).
    await page.goto('/path/1');
    await expect(page).toHaveURL(/\/courses\/1$/);
  });
});
