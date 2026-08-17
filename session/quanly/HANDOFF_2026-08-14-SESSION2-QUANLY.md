# HANDOFF — 14/08/2026 (phiên chiều: dark theme + Ollama vision + gộp khóa Grokking + UI educative/VDSA-main)

> File này bàn giao toàn bộ công việc phiên 14/08 (từ ~12:30 đến ~17:00). Phiên mới đọc file này + `HANDOFF_2026-08-14-QUANLY.md` là nắm hết. KHÔNG cần đọc lại lịch sử cũ.

## 1. TÓM TẮT PHIÊN NÀY (7 việc chính)

| # | Việc | Trạng thái |
|---|---|---|
| 1 | Chạy dự án native (không Docker — máy này không có Docker) | ✅ FE :5173 / BE :5000 / SQL local |
| 2 | Cài Ollama + model vision `qwen2.5vl:7b` (RTX 5050 8GB) + script `tools/vision-read.ps1` | ✅ |
| 3 | Dark theme "đen vừa vừa" toàn app (mặc định + persist + chống flash) | ✅ |
| 4 | Fix đăng nhập (backend chết + rate limit + refresh-token replay loop) | ✅ |
| 5 | Import khóa **Grokking Data Structures** từ VisualizationDSA-main (SQLite) vào backend DsaVisual | ✅ |
| 6 | Bỏ roadmap cũ → flow khóa học kiểu VDSA-main (`/courses` → `/courses/:id` → `/lessons/:id`) | ✅ |
| 7 | Trang chi tiết khóa học: layout y chang VisualizationDSA-main (hero + enroll modal + modules) | ✅ |

## 2. MÔI TRƯỜNG MÁY NÀY (KHÁC HANDOFF CŨ)

- **Repo thật**: `C:\Users\Tam Phuc\Downloads\Visualization-dev\Visualization-dev` (CWD bash tool là `...\Visualization-dev` — repo nằm trong thư mục con trùng tên; `Get-ChildItem` CWD chỉ thấy 1 folder `Visualization-dev`).
- **KHÔNG có Docker** (Testcontainers integration tests sẽ FAIL trên máy này — 77 integration tests cần Docker; unit tests 153 chạy OK).
- **SQL Server native local** (Windows auth `sqlcmd -S localhost -E`), DB `DsaVisual`, login riêng **`dsa_app` / `DsaVisual@Dev123`** (sa bị khóa password).
- Chạy backend cần env: `$env:ConnectionStrings__Default = "Server=localhost;Database=DsaVisual;User Id=dsa_app;Password=DsaVisual@Dev123;TrustServerCertificate=True"`.
- **Ollama 0.32.9** @ localhost:11434, model **`qwen2.5vl:7b`** (đọc ảnh). Model chính (deepseek) KHÔNG đọc ảnh → dùng script `tools/vision-read.ps1 -ImagePath <file>`.
  - ⚠ Ảnh >~90KB → lỗi context 4096 → phải gọi API trực tiếp với `options.num_ctx = 8192` (xem cách gọi trong session).
- Backend :5000 (health `/health`), FE vite dev :5173 (port bị project khác chiếm nhiều lần — xem §7).
- Accounts: `student@demo.local / Student@123` (premium), `admin@system.local / Admin@123`, `teacher@demo.local / Teacher@123`.

## 3. DARK THEME (đã làm — file đổi)

- `frontend/src/stores/ui.ts`: theme mặc định **dark** + persist `localStorage['dsa.theme']` + watch áp class `dark` lên `<html>` (TRƯỚC ĐÂY KHÔNG AI KÍCH HOẠT — store chỉ đổi biến, dark mode chưa bao giờ chạy thật).
- `frontend/index.html`: script chống FOUC (đọc localStorage trước khi mount).
- `frontend/src/styles/tokens.css` + `tailwind.css`: palette dark mới **neutral đen vừa** `#17191D` (card `#1C1F25`, text `#E8EAEE`, border xám) — bỏ teal đậm cũ `#042F2E`, giữ teal `#2DD4BF` accent.
- `frontend/src/styles/vdsa-theme.css` (MỚI): token dark purple của VDSA namespace `vdsa-*` (xem §6).

## 4. IMPORT KHÓA GROKKING (đã merge vào seed — DB thật đã seed)

- **Nguồn**: `C:\Users\Tam Phuc\Downloads\VisualizationDSA-main\VisualizationDSA-main\backend\src\WebApi\visualization_dsa.db` (SQLite) — course "Grokking Data Structures" (id `71692158-...`), 4 modules, 16 lessons.
- **Extract**: `backend/seed-data/grokking-course.json` (436KB — đã copy vào repo; sqlite-tool đã xóa khỏi trees/).
- **Backend mới**:
  - `backend/src/DsaVisual.Application/Persistence/Seed/SeedGrokkingData.cs` + `SeedGrokkingData.Helpers.cs` — import: 4 topics mới (Module 1-4), 16 lessons (8 theory tiếng Việt + 4 mini-quizz + 4 assignment), 1 path "Grokking Data Structures" (id=7, 18 nodes), exercises (12 quiz MCQ + 8 lab + 4 code assignment + final test), **ẩn 5 path cũ** (`IsActive=false`).
  - `backend/src/DsaVisual.Api/Controllers/ConceptsController.cs` — **adapter API VDSA** trả đúng format VisualizationDSA-main: `GET /concepts/courses`, `GET /concepts/courses/{id}`, `GET /concepts/lessons/{id}`, `GET/POST /concepts/quiz/{id|submit}`, `GET/POST /concepts/auth/progress/{id}`, `POST /concepts/auth/award-xp`. Map: LearningPath→Course, Node→Lesson (sandboxType theo exercise: theory content dài→'dsa', mini-quizz→'quiz', assignment→'codelab'), Quiz→Exercise MCQ, codelab→ConfigJson tasks (entryFunction/testCases/hints giữ nguyên).
  - `SeedDemoActivity.Progress.cs` ParseTestCount: sửa hỗ trợ ConfigJson dạng array tasks (crash khi seed).
  - Fix lỗi: JsonArray cần using System.Text.Json.Nodes; UTF8Encoding cần using System.Text; ContentMd lấy từ JSON lessons phẳng (có moduleId).
- **Verify DB thật**: Topics=9, Lessons=24, Exercises=54, Questions=221, LearningPaths=6 (5 cũ IsActive=0 + Grokking active), node 1 "Học: Bài 1: Array" active, quiz 10 câu chấm đúng, codelab 3 tasks.
- ⚠ **Re-seed khi cần**: xóa 4 exercises assignment cũ (FK ExerciseSubmissions/CodeSubmissions trước) rồi chạy `dotnet run --project src/DsaVisual.Api -- --seed` (idempotent, ~3 phút).

## 5. FLOW KHÓA HỌC MỚI (bỏ roadmap)

- **Router** (`frontend/src/router/index.ts`): 
  - MỚI: `/courses` (name `courses` → CoursesListView), `/courses/:id` (name `course-detail` → CourseDetailView), `/lessons/:id` (name `lesson-study` → LessonStudyView).
  - CŨ redirect: `/path` → `/courses`; `/path/:topicId` → `/courses/:topicId`; `/path/:topicId/node/:nodeId` → `/courses/:topicId`; `/learn` → `/courses`.
  - `AppHeader.vue`: nav "Lộ trình" → `{ name: 'courses' }`. `LoginView`/`RegisterView`: redirect mặc định `/courses` (sửa cả RegisterView.spec.ts expectation `{name:'path'}` → `{name:'courses'}`).
- **File FE copy từ VisualizationDSA-main** (đã sed namespace class → `vdsa-*`):
  - `views/courses/CoursesListView.vue` (danh sách khóa, filter tabs, card + progress)
  - `views/courses/CourseDetailView.vue` + `.css` (hero banner + enroll modal + objectives + outcomes + modules accordion)
  - `views/lesson/LessonStudyView.vue` + `LessonCompletionModal.vue` + `components/LessonStep{Theory,Quiz,CodeLab}.vue` (bỏ LessonStepViz — flow Theory→Quiz→CodeLab)
  - `shared/components/BaseIcon.vue` (copy nguyên)
  - `features/lesson/types/lesson.types.ts`, `utils/codelabExecutor.ts` + `codelab.worker.ts` + `codelabTaskRegistry.ts` + `sandboxConfig.ts`
  - `features/lesson/services/lessonApi.ts` (viết lại — dùng `/api/v1/concepts/*`), `store/useLessonStore.ts` (viết lại — bỏ LESSONS local, lấy từ backend)
  - `features/quiz-system/service/statelessQuizApi.ts` (viết lại base URL `/api/v1/concepts/quiz/*`)
  - `features/courses/store/useCourseStore.ts` (MỚI — enrollment localStorage `dsa-course-enrolled`)
  - `services/courseApi.ts` (viết lại — dùng `/api/v1/concepts/courses/*`)
  - `features/user-progress/store/useUserProgressStore.ts` (MỚI tối giản — syncXP local)
  - **Đã cài**: `monaco-editor@0.55.1` (CodeLab editor).
- **Flow**: `/courses` (danh sách) → bấm khóa → `/courses/7` (chi tiết) → "Đăng ký khóa học" (modal) → "Start Learning" → `/lessons/:id` (sidebar modules + Theory/Quiz/CodeLab).

## 6. LƯU Ý KỸ THUẬT (bài học phiên này)

- **Sed namespace class**: class Tailwind của VDSA (`bg-bg-surface`, `text-accent`, `border-border-subtle`...) XUNG ĐỘT shadcn (`bg-accent` = teal của app) → mọi file copy phải đổi sang `vdsa-*` (map trong `trees/sed-namespace*.ps1` — đã xóa, giữ trong git history nếu cần). Theme tokens nằm `frontend/src/styles/vdsa-theme.css` (import trong main.ts SAU tailwind.css).
- **Port 5173 bị chiếm nhiều lần** bởi vite của `VisualizationDSA-main` (user mở project kia): symptom = trang hiển thị Landing của project khác. Fix: `Stop-Process` node PID chiếm 5173 → restart `npm run dev -- --port 5173 --strictPort` từ `frontend/`.
- **Rate limit 429**: sensitive 60 req/min/IP (`DSA:RateLimit:Sensitive`) — login + refresh liên tục qua Playwright đốt nhanh → chờ 60-75s hoặc restart backend (in-memory). Vòng lặp 401→refresh→redirect: bug có sẵn `client.ts:110-112` (khi ở /login chưa có cookie, request 401 → redirect loop) — KHÔNG phải do phiên này, chưa fix.
- **Refresh token replay**: cookie refresh qua vite proxy không cập nhật sau rotate → dùng API login riêng (host 5000) lấy cookie set thủ công vào Playwright context để verify (xem `trees/*-check*.mjs` pattern — đã xóa, pattern: `apiLogin()` node http → `context.addCookies`).
- **Ollama đọc ảnh >90KB**: gọi `/api/generate` với `options.num_ctx=8192`.
- **Test**: FE `npx vitest run` = 164 PASS; BE `dotnet test tests/DsaVisual.UnitTests` = 153 PASS; integration tests (77) FAIL trên máy này (cần Docker — chạy trên máy kia). `npm run build` PASS (chunk monaco 3.6MB — lazy qua route).

## 7. TỒN ĐỌNG / VIỆC TIẾP THEO

1. **PROMPT_VISUALIZE_UPGRADE** (engine visualize) — worktree `D:\FPT\neww-engine` CHƯA tạo trên máy này (HANDOFF cũ nói máy kia); trên máy này nếu cần thì tạo mới.
2. **Docs đồng bộ**: SRS/SDD/API_REFERENCE/SCREEN_MAP chưa cập nhật cho: dark theme mặc định, flow `/courses`, ConceptsController (API mới), khóa Grokking (topics/lessons mới), endpoint `GET /learning-paths`. Cần task dev-docs.
3. **THIRD_PARTY.md**: cần thêm `monaco-editor` + `@monaco-editor/loader` (đã có trong package.json + THIRD_PARTY chưa cập nhật).
4. **`frontend/src/data/lessons.ts`** (FE seed cũ) + `PathView/PathRedirectView/NodeHubView` vẫn còn trong repo (không dùng — route đã redirect; có thể xóa sau khi docs đồng bộ).
5. **Integration tests** — chạy trên máy có Docker để xác nhận (214 tests).
6. **PR #1 dev→main** — vẫn chờ USER duyệt (không tự merge).
7. **2FA SMTP thật** — đang MailHog (chưa có trên máy này — chưa cài MailHog native; nếu test 2FA cần chạy MailHog hoặc SMTP khác).
8. 18 ảnh báo cáo + docx pandoc — chưa làm phiên này.
9. Xóa worktree cũ nếu dọn: `git worktree remove D:\FPT\neww-teacher` (đã merge) — máy này không có worktree D:\FPT (khác máy).

## 8. GIT (phiên này CHƯA commit — cần kiểm tra)

- Repo `Tqson3118/Visualization` — nhánh hiện tại đang dùng là gì phải kiểm tra (máy này có thể đang ở dev hoặc nhánh khác — `git status` lúc phiên không chạy được do path lồng). **Chưa commit/push gì từ phiên này.**
- Commit-as: `.\commit-as.ps1 {son|bao|thu|phuc}` (FE→son, BE→bao, engine/test→thu, docs→phuc). PR base `dev`.

## 9. CÁC FILE/SCRIPT TẠM (trees/ — đã dọn phần lớn, giữ lại gì)

- `trees/`: `grokking-course.json` (bản gốc extract — bản chính thức đã ở `backend/seed-data/`), log backend (`backend-run.log`, `backend-seed*.log`), ảnh verify (`dark-*.png`, `vdsa-*.png`, `final-*.png`, `enroll-page.png`, `educative-*.png`, `home-shot.png`, `ocr-test.png`, `login-debug.png` — có thể xóa khi không cần), `vision-read` script ở `tools/vision-read.ps1` (giữ).
- Không còn file temp rải ngoài repo.

## 10. MẸO NHANH (verify nhanh phiên sau)

```powershell
# Backend (máy này)
$env:ConnectionStrings__Default = "Server=localhost;Database=DsaVisual;User Id=dsa_app;Password=DsaVisual@Dev123;TrustServerCertificate=True"
Start-Process dotnet -ArgumentList "run --project src/DsaVisual.Api --launch-profile http" -WorkingDirectory "...\frontend\..\backend"

# FE
npm run dev -- --port 5173 --strictPort   # từ frontend/

# API test nhanh
$login = Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/v1/auth/login -ContentType "application/json" -Body '{"Email":"student@demo.local","Password":"Student@123"}'
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/concepts/courses/7" -Headers @{Authorization="Bearer $($login.accessToken)"}

# Đọc ảnh (Ollama)
.\tools\vision-read.ps1 -ImagePath "path.png"
```
