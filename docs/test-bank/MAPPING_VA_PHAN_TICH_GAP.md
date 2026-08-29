# 📊 BẢNG MAPPING ĐỀ THI ↔ MÃ NGUỒN THỰC TẾ & PHÂN TÍCH COVERAGE

> **Cập nhật:** 2026-08-27 (thẩm định + vá errata) · thống kê & drift tự sinh 2026-08-28 bởi `tools/generate-test-bank-mapping.mjs`.
> **Source of truth:** File `.md` (không phải `.docx`) — chỉnh sửa trên `.md` trước, export `.docx` sau.
> **Quy chuẩn:** 19 Bộ đề chuyên sâu (Đề 01–13 theo màn hình nghiệp vụ, Đề 14–17 các view còn lại, Đề 18–19 lớp Engine/API) + 01 Bộ đề Tổng hợp Final Test.

---

## 1. THỐNG KÊ TỔNG QUAN (đếm lại từ codebase)

| Loại | Số lượng thực tế | Số được phủ trong đề | Tỷ lệ |
|------|------------------|----------------------|-------|
| Views (.vue) | 51 | ~49 | ~96% |
| Stores (.ts) | 12 (9 core + 3 spec) | 9 + spec | ~100% |
| Controllers (.cs) | 20 | ~18 | ~90% |
| Services (.cs) | 31 (gồm interface + impl) | ~13 impl | ~100% impl trọng tâm |
| Composables (.ts) | 16 | 14 (qua Đề 17) | ~88% |

> Lưu ý: 51 view kể cả các view con trong `views/sorting`, `views/graph`, `views/lesson`...; một số chỉ là thành phần con được dùng qua view cha.

<!-- AUTO:DRIFT:START -->
## 1.5 🤖 BÁO CÁO DRIFT (tự sinh — đừng chỉnh tay)

> Sinh ngày 2026-08-28 bởi `tools/generate-test-bank-mapping.mjs`. Chạy lại script này sau khi codebase đổi để phát hiện tham chiếu hỏng.

**Thống kê quét:** 51 views · 12 stores · 20 controllers · 31 services · 16 composables · tổng 755 file nguồn được đánh chỉ mục.

**Kết quả:** ✅ Không phát hiện drift — mọi file được tham chiếu trong bảng mục 4 và trong các file đề thi đều tồn tại trong codebase.
<!-- AUTO:DRIFT:END -->

---

## 2. ✅ DANH SÁCH VIEW TỪNG "CHƯA PHỦ" — GIỜ ĐÃ PHỦ (qua Đề 14–17)

| # | File | Đề phủ | Trạng thái |
|---|------|--------|-----------|
| 1 | `HelpView.vue` | Đề 14 | ✅ |
| 2 | `PrivacyView.vue` | Đề 14 | ✅ |
| 3 | `NodeHubView.vue` | Đề 15 | ✅ |
| 4 | `PathRedirectView.vue` | Đề 16 | ✅ |
| 5 | `FinalTestView.vue` | Đề 16 | ✅ |
| 6 | `TeacherStudioView.vue` | Đề 15 | ✅ |
| 7 | `AdminFeedbackView.vue` | Đề 16 | ✅ |
| 8 | `AdminLadderView.vue` | Đề 15 (nhắc) | ✅ |
| 9 | `AdminContentView.vue` | Đề 15 (qua /studio) | ✅ |

---

## 3. ⚠️ CÒN LẠI — GAPS CHƯA CÓ ĐỀ RIÊNG (chấp nhận được: cross-cutting/infra)

| Loại | Files | Lý do chưa có đề riêng |
|------|-------|------------------------|
| i18n | `i18n/vi.ts` (~1200 message keys — không có bản en.ts) | Cross-cutting; được dùng gián tiếp qua mọi đề |
| Features con | `features/vcr-player`, `features/guided-tour`, `features/algorithm-sandbox`, `features/courses` | Được dùng qua view cha (SortingView, SimulatorView, CoursesListView) |
| Backend infra | `Program.cs` pipeline, middleware `ErrorHandlingMiddleware`, EF migrations, seed data | Không có UI riêng để "trace luồng"; đủ phủ qua các câu TL |
| DB schema | EF entities (Users, Lessons, Classes, UserGamification...) | Đề cập gián tiếp qua Service/Controller |

---

## 4. BẢNG MAPPING ĐỀ ↔ FILES (19 đề + Final)

| Đề | Module | Views | Stores (FE) | Controllers (BE) | Services (BE) | Composables |
|----|--------|-------|-------------|-------------------|----------------|-------------|
| 01 | Auth & Session | LoginView, RegisterView, ForgotPasswordView, ResetPasswordView | auth.ts | AuthController | AuthService | — |
| 02 | Home & Router | HomeView, SimulatorView, CoursesListView | ui.ts (router) | — | — | useScrollReveal |
| 03 | Simulator Engine | SimulatorView | simulation.ts | FavoritesController | SimulationCatalogService | useSimulation |
| 04 | Explore/CheatSheet/Benchmark | SimulationsView, CheatSheetView, BenchmarkView | — | — | — | — |
| 05 | Sandbox 4 Tab | SortingView (incl. Searching/Graph/StackQueue tabs) | — | — | — | — |
| 06 | Learning Path | CoursesListView, CourseDetailView, LessonStudyView, LessonView | lesson.ts, progress.ts, useCourseStore | LessonsController, ConceptsController | LessonService, TopicService | — |
| 07 | Practice Ladder | LadderView, LabView, ExerciseView | — | ExercisesController | ExerciseService | — |
| 08 | Code Runner | CodeRunnerView | codeRunner.ts | CodeRunsController | CodeRunnerService, CodelabJudgeService | useCodeTracePlayback |
| 09 | Gamification | QuestsView, LeaderboardView | gamification.ts, leaderboard.ts | GamificationController | GamificationService | — |
| 10 | Shop & Premium | ShopView, PremiumView, SubscriptionView | gamification.ts | GamificationController | GamificationService | useCountdown |
| 11 | Profile | ProfileView | gamification.ts | MeController, AuthController | UserService, AuthService | — |
| 12 | Classes & Studio | ClassesView, ClassDetailView, ClassReportView, AdminLessonEditorView | classStore.ts | ClassesController | ClassService | — |
| 13 | Admin Panel | AdminUsersView, AdminStatsView, AdminSettingsView | ui.ts | AdminController, UsersController, SettingsController | UserService, SettingService | usePagination |
| 14 | Help/Privacy/Feedback | HelpView, PrivacyView | — | FeedbackController, CourseFeedbackController, PublicController | — (FeedbackController truy vấn AppDbContext trực tiếp, không qua Service) | useLenis |
| 15 | Teacher Studio | TeacherStudioView, NodeHubView, AdminLessonEditorView | ui.ts | ConceptsController, LessonsController | LessonService, TopicService | — |
| 16 | FinalTest/Redirect | FinalTestView, PathRedirectView, AdminFeedbackView | — | ExercisesController | ExerciseService | — |
| 17 | Composables & Effects | (cross-cutting) | simulation.ts | — | — | useSimulation, useCodeTracePlayback, useStructureTransition, useSoundEffects, useConfetti, useLenis, useScrollReveal, useKeyboardShortcuts, useDebounce, usePagination, useCountdown, useConfirm, usePixiStage, useCosmicField |
| 18 | Engine Core | (no view) | simulation.ts | — | — | (engines/registry.ts, catalog.ts, generators/*, core/stepExecutor.ts, renderers/*) |
| 19 | API Layer & Router | (no view) | — | — | — | (api/client.ts, api/*.ts, router/index.ts) |
| Final | E2E Integration | (tổng hợp) | (tổng hợp) | (tổng hợp) | (tổng hợp) | (tổng hợp) |

---

## 5. KHUYẾN NGHỊ BẢO TRÌ

1. **Đồng bộ .docx**: xuất lại 2 file `.docx` từ `.md` mỗi khi cập nhật (hiện đang cập nhật trong đợt này).
2. **Xáo trộn đáp án A/B/C/D**: từng bị lệch (~89% đáp án B + đáp án đúng thường dài nhất → dễ đoán). Đã chạy script xáo trộn cân bằng trong đợt này; khi thêm câu mới phải giữ cân bằng.
3. **File .md là source of truth** — không bao giờ sửa trực tiếp trên `.docx`.
4. **Tái tạo file mapping này** mỗi khi thêm/bớt View/Controller/Composable. Khuyến nghị: viết script quét codebase + sinh bảng này tự động (CI) thay vì giữ thủ công.

---

*File mapping này phải được cập nhật mỗi khi thêm View/Controller/Composable mới vào codebase.*
