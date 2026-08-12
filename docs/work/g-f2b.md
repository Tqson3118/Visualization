# G-F2b — POLISH 5 MÀN CHÍNH (nhóm 1)

> Ngày: 12/08/2026 · Nhánh: `feature/ux-polish` (base 53d1905 G-F2a)
> Phạm vi: Home `/`, Login `/login`, Lesson detail `/learn/:id`, Exercise `/exercise/:id`, Learning Path `/path/:topicId` (BAO_CAO_SPEC §6.2 — 5/12 ảnh).
> Nguồn đặc tả: SDD Màn 01/02/04/06/13 (§8.4A) + PRODUCTION_PROMPT §20.2.2.
> KHÔNG đụng `engines/*` + `components/simulator/*` + canvas.

## Tóm tắt kết quả

| # | Màn | Route | Trạng thái | Điểm nâng cấp chính |
|---|-----|-------|-----------|---------------------|
| 1 | 01 — Trang chủ | `/` | ✅ DONE | Hero gradient Aurora (bg-aurora-gradient) + heading trắng + CTA 2 nút (light/outline-light + hover-glow/lift); stats strip (44 mô phỏng / 5 nhóm / 3 cấp độ từ CATALOG); **section 3 demo công khai** (FR-7.6: sort.bubble / search.binary / graph.bfs) dùng Card shadcn + hover-lift + icon lucide + độ phức tạp + nút "Chạy thử" mở `/simulator/{key}`; feature highlight 3 card (Card shadcn + hover-lift + icon). Dark mode đẹp (gradient dark riêng trong palettes.css). |
| 2 | 02 — Đăng nhập | `/login` | ✅ DONE | Split layout: trái = brand panel gradient Aurora (badge + tagline + 3 feature point có icon BaseIcon), phải = form. Input shadcn wrapper (label/error/icon — icon mail/lock), Button loading state + block, link "Quên mật khẩu?" + "Đăng ký ngay". **GIỮ nguyên** `#email`/`#password` + heading/button "Đăng nhập" (auth e2e phụ thuộc). Logic login/redirect không đổi. |
| 3 | 04 — Chi tiết bài học | `/learn/:id` | ✅ DONE | Hero gradient Sunset mềm (bg-sunset-gradient + overlay trắng) + breadcrumb + badge "Bài học"/"Đã học" + nút "Đánh dấu đã học" (toast success/error) + "Học tiếp"/"Về lộ trình". **Tabs shadcn**: Nội dung (LessonDetail hide-header — giữ notes/rating/sims/exercises) / Lý thuyết (card độ phức tạp từ CATALOG + article rich-text) / Quiz (thẻ bài tập Card + hover + nút "Làm bài", EmptyState khi trống). Thẻ mô phỏng/bài tập trong LessonDetail nâng cấp Card shadcn + hover-lift + icon. |
| 4 | 06 — Bài tập trắc nghiệm | `/exercise/:id` | ✅ DONE | Header toolbar card (kicker + title + description + toggle luyện tập). **QuizStage**: ProgressBar tiến độ câu hỏi (show-label, success khi 100%), option dạng **button selectable** (hover nâng nhẹ + active = viền primary + ring + check), toast kết quả khi nộp (≥60% success / ngược lại warning), **confetti 'success'** khi pass (watch result+passed), nút "Làm lại" giữ sẵn. Logic submit giữ nguyên (G-BF2 pre-check đủ câu). |
| 5 | 13 — Learning Path | `/path/:topicId` | ✅ DONE | Hero gradient nhẹ (bg aurora-soft + vạch gradient + card tiến độ riêng chứa ProgressBar + đếm node đã qua); node map dùng **Badge status** (Đã qua/Đang học/Khóa) + **hover-lift** (node không khóa); EmptyState khi không có node (giữ sẵn). **GIỮ nguyên** aria-label node (e2e FR-10.1 phụ thuộc) + popover "❤ 1 — trừ 1 tim khi vào node" + nút "Bắt đầu". |

## Fix kèm (cần thiết để verify 11/11 E2E)

- `tests/e2e/helpers/mockApi.ts` — `POST /auth/refresh` trước đây **luôn trả 200** → app boot (main.ts gọi refresh theo ADR-004) nghĩ "đã đăng nhập" → guard guestOnly đá `/login` về `/home` → **8/11 E2E FAIL pre-existing** (đã ghi ở docs/work/g-f2a.md). Fix: mock refresh trả **401 khi chưa login/register trong phiên** (mô phỏng cookie HttpOnly), trả 200 sau khi login — giống hành vi production. KHÔNG đổi production code. Sau fix: **11/11 PASS**.
- `tests/e2e/helpers/mockApi.ts` — bổ sung `MOCK_LESSON.simulations/exercises` + route `GET /exercises/{id}` (dữ liệu demo thuần — để ảnh Màn 04/06 render đủ thẻ quiz/mô phỏng). Không đổi kỳ vọng test hiện có.

## File tạo / sửa

**Sửa (5 view + component dùng chung + i18n):**
- `frontend/src/views/HomeView.vue` — hero Aurora + stats + 3 demo công khai + feature (Card shadcn + lucide + hover-lift).
- `frontend/src/views/LoginView.vue` — split layout + Input/Button shadcn + forgot-password; giữ selector e2e.
- `frontend/src/views/LessonView.vue` — hero Sunset + Tabs Nội dung/Lý thuyết/Quiz + mark-viewed + toast.
- `frontend/src/views/ExerciseView.vue` — toolbar card + confetti 'success' khi pass.
- `frontend/src/views/PathView.vue` — hero gradient nhẹ + Badge status + hover-lift + ProgressBar hero.
- `frontend/src/components/lesson/LessonDetail.vue` — prop `hideHeader` (tránh trùng header với hero) + thẻ sim/exercise dùng Card shadcn + hover-lift + icon.
- `frontend/src/components/ladder/QuizStage.vue` — ProgressBar + option selectable + toast kết quả + confetti khi pass.
- `frontend/src/i18n/vi.ts` — thêm chuỗi home.* (demo/stats) + auth.* (loginSubtitle/forgotPassword/brandPoint*).
- `frontend/tests/e2e/helpers/mockApi.ts` — fix refresh 401 (mở khoá 11/11) + mock lesson/exercise demo.

**Tạo mới (ảnh verify §6.2 — docs/work/):**
- `g-f2b-01-home.png`, `g-f2b-02-login.png`, `g-f2b-04-lesson-detail.png`, `g-f2b-06-exercise.png`, `g-f2b-13-learning-path.png` (+ bản `-dark.png` cho mỗi màn).

## Verify

- ✅ `npm run build` — 0 lỗi (vue-tsc + vite).
- ✅ `npm test` — **72/72 PASS** (8 files).
- ✅ `npx playwright test` — **11/11 PASS** (auth 3, code-runner 3, ladder 2, simulator 3) — mock refresh fix mở khoá 8 test trước đây FAIL.
- ✅ Verify browser bằng Playwright (spec tạm đã xoá sau khi chạy): 5 màn render light + dark, **0 console error / 0 pageerror**, **không overflow ngang**, đủ selector: 3 demo card (`.home__demo`), `.login__aside`, `.lesson-view__hero-title` + tabs, `.quiz-stage__question` + 4 options, `.path-view__node` (3) + badge.

## Ghi chú / rủi ro

- **Ngưỡng pass quiz**: task ghi "confetti khi pass (≥70%)" nhưng QuizStage giữ ngưỡng **≥60%** (FR-4.2/Bậc 1) — **KHÔNG đổi logic**; confetti bắn theo sự kiện pass thật của component (score ≥60). Final test ≥70% là quy định FR-4.12 khác màn. Ghi rõ để master nắm.
- Confetti ở QuizStage áp dụng chung cho mọi nơi dùng QuizStage (Exercise/Ladder/NodeHub) — nhất quán trải nghiệm, không phá test.
- Không chạm `engines/*`, `components/simulator/*`, canvas. Các file `.opencode/`, `docs/pm-decision-log-g.md`, ảnh e2e/f3 cũ là thay đổi của agent khác — KHÔNG nằm trong commit này.
