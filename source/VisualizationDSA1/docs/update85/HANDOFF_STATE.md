# 🔄 HANDOFF STATE — VisualizationDSA (2026-08-06)

> **Mục đích:** ghi chính xác trạng thái hiện tại để agent/dev khác tiếp tục mà không cần đoán lại.
> **Bắt đầu từ:** Docker đã rebuild + verify XONG (CustomRoadmaps=5, Health OK, FE 200). Screenshot đã chụp 88 ảnh. REPORT_G1-G4 đã viết. Việc còn lại: PM quyết các mục treo → commit.

---

## 1. Tổng quan đã làm

| Giai đoạn | Trạng thái code | Build/Test | Ghi chú |
|---|---|---|---|
| G1 (layout + DS + UTF-8 + gating) | ✅ Hoàn thành | FE xanh + 822/822, BE xanh + 50/50 | Screenshot 88 ảnh h-overflow=0 |
| G2 (6 màn chính) | ✅ Hoàn thành code | FE xanh + 822/822 | Screenshot verify từng màn |
| G3 (logic nghiệp vụ) | ✅ Hoàn thành code | BE xanh + 50/50 | Runtime verify: login/enroll/payment probe OK; ⚠️ Enrollment Course vs CustomRoadmap cần PM chốt |
| G4 (seed + test + tài liệu) | ✅ Hoàn thành | FE coverage 71.8% | Docker verify xong; SePay E2E thật chờ tài khoản |

**Build/test local hiện tại (chạy máy thật):**
- `cd frontend && npm run build` → ✅ xanh (vue-tsc -b + vite)
- `cd frontend && npx vitest run` → ✅ 822/822
- `cd backend && dotnet build src/WebApi/WebApi.csproj` → ✅ 0 error
- `cd backend && dotnet test tests/VisualizationDSA.UnitTests` → ✅ 50/50 (thêm 5 test enrollment/reset)

---

## 2. ✅ DOCKER — ĐÃ REBUILD + VERIFY XONG (2026-08-06)

> Lưu ý quan trọng: HANDOFF trước chẩn đoán "seed fail do background job xung đột" là **SAI**. Root cause thật:

### 🔧 Root cause thật (3 lỗi, đã fix)
1. **EF track sai ModuleItem**: `DbSeeder.SeedCoursesAsync` dùng `module.Items.Add(quizItem/codelabItem)` → EF đánh dấu `Modified` → sinh `UPDATE "ModuleItems"` trên row chưa tồn tại → `DbUpdateConcurrencyException: expected 1, affected 0`. Fix: `_context.ModuleItems.AddAsync(...)` (2 chỗ).
2. **Judge0 chiếm app DB**: Judge0 + app cùng dùng `visualization_dsa_dev`. Judge0 tạo bảng trước → `EnsureCreatedAsync` no-op → seed fail `relation "Users" does not exist`. Fix: tách Judge0 sang DB `judge0` (`postgres-init/01-create-judge0-db.sh` + `POSTGRES_DB=judge0`).
3. **Backend race khởi động**: thiếu `depends_on database: service_healthy` → EnsureCreated chạy khi Postgres chưa sẵn. Fix: thêm healthcheck dependency.

### Kết quả verify (đã chạy thật):
```bash
docker compose build backend frontend   # ✅ xanh
docker compose up -d                    # ✅ 6 container up
docker logs vdsa-backend                # [DB SCHEMA PATCH]: OK + [DB SEEDER SUCCESS] (không còn Seed*Error)
```
- `CustomRoadmaps` = **5** (≥3) · `CustomNodes` = 24 · `Users` = 12 · `Quizzes` = 17 · `Courses` = 3 · `Codelabs` = 3
- `GET /health` → `{ status: "Healthy" }` · `GET /` → 200
- API: login 3 vai 200 · `/teacher-studio/roadmaps` (admin) → 5 Published · `POST /enrollments` (CustomRoadmap id) → 201 Active

> ⚠️ Nếu cần reset lại từ đầu (không bắt buộc): `docker compose down && docker volume rm visualizationdsa_pgdata && docker compose up -d`

---

## 3. Test account

| Vai | Email | Mật khẩu |
|---|---|---|
| Admin | `admin@visualizationdsa.dev` | `Admin@2024` |
| Teacher | `demo@visualizationdsa.dev` | `Demo@2024` |
| Student | `nguyenvana@visualizationdsa.dev` | `User@2024` |

---

## 4. Những gì đã code (tóm tắt theo file)

### G1 — Cứu layout + Design System
- `views/lesson/LessonStudyView.vue`, `MyClassrooms`, `StudentClassroom`, `TeacherStudioRoadmapEditor`, `TeacherClassroomAnalytics`, `PremiumCheckout`: `min-h-screen/h-screen` → `h-full`.
- `DocsView`, `CourseDetailView`, `GraphView`: gỡ `overflow-hidden` view root.
- `GraphView`, `DocsTableOfContents`: `top-[72px]/[88px]` → `top-[var(--header-height)]`.
- Xóa `views/ClassroomDetailView.vue` + `views/ClassroomDashboard.vue` (dead).
- `styles/theme.css`: dọn token trùng, **merge design-tokens.css**, xóa cyberpunk (D13), đổi `terminal-dark`→`dark` (D14), audit light block.
- `useThemeStore.ts`: type `dark|light` + localStorage fallback `terminal-dark`.
- UTF-8: sửa mojibake `TeacherStudioRoadmapEditor.vue`, `LessonDiscussionPanel.vue`.
- Gating demo: `GamificationWorkspace` (+50XP → DEV), `SortingView` (VISUALGO→DEV MODE), `dsaApi` (demo badge), xóa leaderboard giả, bỏ theme item "mua chết".
- alert()/confirm() → ToastContainer + ConfirmDialogStore (`features/ui/store/useConfirmDialogStore.ts` + `ConfirmDialogHost`).
- Tài liệu: `docs/update85/design-system.md`, `docs/screenshots/README.md` + `scripts/screenshots/screenshot.mjs`.

### G2 — Thiết kế màn chính
- Wireframe: `docs/update85/wireframes/W1-W6.txt`.
- Landing: hero CTA, grid thuật toán thật (ALGORITHM_CATALOG), Freemium section, bỏ GitHub/dead links/stats trùng.
- **Màn mới**: `views/courses/AlgorithmLibraryView.vue` (route `/algorithms`) — grid + sidebar lọc + search + badge Premium.
- Lesson Study: rewrite 1 trang cuộn (stepper trạng thái, loading/error, nút hoàn thành, resume scroll + POST progress).
- Profile: "Hồ sơ cá nhân", bỏ About/Preferences, thêm tab Nộp đơn Teacher, avatar ring token.
- Dashboard: ẩn SkillRadar, "Tiếp tục bài đang học", streak dữ liệu thật.
- Visualizer: panel graph `min(360px,85vw)` + collapse mobile, CodeIDE 1 cột mobile, Playground share URL hash→query, tour guard.
- Teacher: route `/teacher-studio/:id`, RoadmapEditor responsive dọc, quiz dropdown tự load, Admin bỏ "Super Admin".

### G3 — Logic nghiệp vụ
- Step lock server-side (`LessonController.CompleteLesson` — 403 nếu quiz/codelab chưa pass).
- Resume: LessonStudy đọc/ghi progress.
- Ẩn `correctIndex` khỏi payload quiz học viên; ngưỡng pass 70% FE/BE.
- **Enrollment (mới)**: `RoadmapEnrollment` entity + migration + `EnrollmentController` (POST/DELETE/GET my, max 3 active) + nút Đăng ký CourseDetail + badge list.
- Hearts: trừ 1 tim khi quiz fail (StatelessQuizController inject IHeartService).
- Judge0 thật: `Judge0Service` POST `/submissions/batch` + poll (loại bỏ mock).
- PracticeLadder UI: fix auth token (useAuthStore).
- Teacher: category enum case-insensitive, QuizBuilder → `/concepts/quiz/*` thật, Theory DELETE + contentMd.
- Admin: mount tab Teacher Approvals (API thật) + Roadmap Approvals, broadcast notification endpoint.
- Payment: `GET /payments/orders/my`.
- Auth: forgot-password + reset-password (token 15p, dev trả token), route `/reset-password` + `ResetPasswordView.vue`.
- AI: render markdown + sanitize (marked + dompurify), nhãn quota đúng.

### G4 — Seed + Test + Tài liệu
- `DbSeeder.SeedDSARoadmapsAsync`: 3 roadmap thật (Sorting 6 node, Graph 6 node, OOP 7 node) gắn quizId.
- Test: `RoadmapEnrollmentTests.cs` (5 test) → BE 50/50. FE coverage 71.8%.
- Dead code: xóa `features/lesson/components/*` (VisualizerPanel, CodeLabPanel, QuizPanel, TheoryPanel, LessonTabs), `LessonResumeToast.vue`.
- Tài liệu: `docs/update85/diagrams.md` (use-case/sequence/architecture Mermaid), `demo-script.md` (kịch bản 10 phút + fallback), cập nhật `README.md`.

---

## 5. ✅ PM ĐÃ QUYẾT (2026-08-06) + VIỆC ĐÃ LÀM THÊM / CÒN LẠI

### Quyết định PM (đã chốt)
1. **Enrollment (mâu thuẫn #3)**: Library (`/courses`) hiển thị **CustomRoadmaps** — KHÔNG dùng Courses. Backend ĐÃ XONG, FE đang dở (xem dưới).
2. **Commit**: **commit 1 lần trên branch hiện tại** (`g0-security`) — toàn bộ working tree.
3. **PracticeLadder**: **Giữ + làm thật** (D10).
4. **Google OAuth (G3.8.3)**: **Wire thật** — key placeholder trong `.env`, PM sửa sau.
5. **Hạ tầng thật**: key placeholder trong `.env` → code wire đủ, PM chỉ việc điền giá trị.

### ✅ ĐÃ LÀM THÊM (session này, build FE+BE đều xanh)

**a) Hoàn thiện 2 task còn thiếu tick trong plan:**
- **G2.4.5 Resume progress**: đã có code từ trước (LessonStudyView đọc `lastScrollPercent` + `POST /concepts/lessons/{id}/progress` debounce) → **tick ✅** trong `implementation_plan_detail.md:153`.
- **G2.7.6 Classroom**: sửa `ClassroomItemPlayer.vue` (`hasNext` hardcoded `false` → prop `hasNext?`), `StudentClassroomView.vue` thêm `:has-next="hasNextItem"` + `@back="navigateBack"` + `@next="navigateToNext"` + `hasNextItem` computed → **tick ✅** plan:179.

**b) Hạ tầng thật — wire đủ, key placeholder trong `.env`:**
- **SMTP (G3.8.1)**: tạo `IEmailService` + `SmtpEmailService` (skip khi placeholder, log thay vì fail). Wire vào `StatelessAuthController.ForgotPassword` (gửi email nếu `IsConfigured`, vẫn trả token dev khi chưa có key). Đăng ký DI trong `Program.cs`.
- **Cloudinary (G3.5.5)**: `CloudinaryUploadService` đọc `Cloudinary__ApiSecret` từ env; không còn throw "config missing" — khi thiếu key trả null (FE giữ base64 fallback).
- **Google OAuth (G3.8.3) BACKEND XONG**: `IAuthService.GoogleLoginAsync` + `AuthService` implement (find-or-create user theo email, issue token); endpoint `POST /api/v1/concepts/auth/google-login` (validate idToken qua Google tokeninfo khi có client id thật; placeholder → bỏ validate, dùng email/name truyền lên); DTO `StatelessGoogleLoginRequest`.
- **`.env`**: thêm placeholder `GOOGLE__CLIENTID/CLIENTSECRET`, `CLOUDINARY__APISECRET`, `SMTP__HOST/PORT/USER/PASSWORD/FROMEMAIL`.
- **`docker-compose.yml`**: backend nhận thêm các env trên.

**c) Enrollment backend (Library → CustomRoadmaps):**
- `ITeacherStudioService.GetPublishedRoadmapsAsync()` + implement (Status=Published && Visibility=Public) + endpoint `GET /api/v1/teacher-studio/roadmaps/published` `[AllowAnonymous]`.

### 🔧 CÒN LẠI (agent kế tiếp làm — thứ tự ưu tiên)

**1. Frontend Google OAuth (nối nốt backend đã xong):**
- [ ] `AuthView.vue:handleGoogleLogin()` hiện toast "Đang tích hợp" (`AuthView.vue:214`) → gọi `statelessAuthApi.googleLogin(...)` (đã thêm function ở `statelessAuthApi.ts`). Dùng Google Identity Services (GIS) lấy idToken nếu có `VITE_GOOGLE_CLIENT_ID`, nếu chưa có key → fallback nhập email/name (dev) gọi endpoint.
- [ ] Xử lý response giống `logIn` trong `useAuthStore.ts` (xem `setSession` pattern dòng 86) — có thể thêm `googleLogin()` vào store hoặc gọi trực tiếp + `setSession`.

**2. Frontend Enrollment — Library hiển thị CustomRoadmaps:**
- [ ] `CoursesListView.vue`: `loadCourses()` từ `GET /api/v1/concepts/courses` → `GET /api/v1/teacher-studio/roadmaps/published`; map `CustomRoadmapDto` (name→title, description, nodes.length→totalLessons, tags→category); bỏ/sửa filter category/difficulty theo Course enum.
- [ ] `CourseDetailView.vue`: `loadCourseDetail()` từ `/concepts/courses/{id}` → `/teacher-studio/roadmaps/{id}`; render node list (PracticeLadder dùng `components/practice/PracticeLadder.vue` — cần nodeId + sessionId từ `POST /api/v1/session/{nodeId}/enter`); enroll đã đúng (`roadmapId`).
- [ ] `router/routes.ts`: giữ `/courses` + `/courses/:id`; thêm route node study nếu cần.
- [ ] Screenshot lại `/courses` + `/courses/:id`; verify enroll E2E (student → mở roadmap → Đăng ký → 201, không còn 500).

**3. PracticeLadder (giữ + làm thật):**
- [ ] Node seed hiện chỉ có `quizId`, chưa có lab/leetcode → seed thêm hoặc chấp nhận lock giữa. Verify flow Quiz→Lab→LeetCode node-based.

**4. Commit 1 lần** (PM quyết định #2): `git add -A && git commit` trên `g0-security`.

**5. Việc phụ nhỏ (đã ghi trong REPORT):**
- [ ] G2.15: PasswordStrengthMeter (AU1) chưa tồn tại, route `/s/` (ES2) chưa có.
- [ ] AdminUsersController (R6): Teacher vẫn đọc user list — cần chốt endpoint classroom-scoped nếu cấm Teacher.
- [ ] SePay E2E thật (G4.2) khi PM có tài khoản.

---

## 6. Lưu ý kỹ thuật quan trọng

- **KHÔNG chuyển `EnsureCreated` → `MigrateAsync`** — migration order repo bị sai (Sprint4 < Initial), sẽ fail. Dùng EnsureCreated + schema patch raw SQL.
- **Màu qua token** (design-system.md). **Cấm alert()/confirm()** — dùng toast + ConfirmDialogHost.
- **Backend startup seed ~30-40s** sau `docker compose up` (có delay background job 30s — giữ nhưng không phải fix cho lỗi seed; fix thật nằm ở DbSeeder ModuleItem AddAsync + judge0 DB tách riêng + depends_on healthcheck).
- `ApiVersion` của vài controller dùng `api/v{version}` → URL thật là `/api/v1/...` (versioning resolves).
- CORS: chỉ cho origin `5173/3000` — dev server port khác bị chặn. Screenshot script dùng BASE 5173 (Docker frontend) để có dữ liệu.
- **Screenshot**: `cd scripts/screenshots && node screenshot.mjs` → 88 ảnh `docs/screenshots/G1.1/`. Script đã sửa theme key `app-theme` + ID seed thật.
