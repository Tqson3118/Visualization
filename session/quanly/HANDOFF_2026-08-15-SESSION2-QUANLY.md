# HANDOFF — 15/08/2026 (phiên 2: Kiểm tra lộ trình + chấm code server + khoá node + tân trang UI)

> File bàn giao phiên này. Phiên mới đọc file này + `HANDOFF_2026-08-15-SESSION1-QUANLY.md` là nắm hết.
> KHÔNG cần đọc lịch sử cũ hơn. Backend/FE đang chạy: BE http://localhost:5000 · FE http://localhost:5173.

## 1. TÓM TẮT PHIÊN (10 việc chính — tất cả ✅ + verify)

| # | Việc | Trạng thái |
|---|---|---|
| 1 | Khóa Grokking: nhóm module 5 "Nội dung bài học" → **"Kiểm tra cuối lộ trình"**; node "Luyện tập tổng hợp" → **"Final-Quizz"** | ✅ |
| 2 | **Final-Quizz** = 20 câu MCQ (5 câu/module từ quiz "Tổng Hợp" nguồn, pass ≥ 70%) — exercise 88, node 53 | ✅ |
| 3 | **Kiểm tra cuối lộ trình** = 3 bài CODE phong cách Assignment (Monaco + testcase 3 public + 2 hidden) — exercise 79, node 54 | ✅ |
| 4 | **Khoá node tuần tự**: node mở khi node NGAY TRƯỚC pass (Status=2); node đầu luôn mở; node đã pass luôn mở lại; chặn API 403 (GET lesson, quiz submit, SaveProgress, code-submit) | ✅ |
| 5 | **Chấm code PHÍA MÁY CHỦ (Jint 4.16.0)**: bài ASM/kiểm tra cuối chỉ PASS khi code chạy ĐÚNG trên server — bỏ qua điểm client khai (IsClientDeclared=false); pass đủ TẤT CẢ task con mới pass node | ✅ |
| 6 | **Fix seed ASM 3+4** (nghiêm trọng): sandboxConfig nguồn là object đơn → ConvertTasks rơi fallback rỗng → đề trống; đã bọc thành mảng 1 task + tự chữa ConfigJson (DB cũ 86/87 đã sửa) | ✅ |
| 7 | **Fix lỗ hổng khoá**: SaveProgress trả 403 nếu node bị khoá; cờ `completed` mới để bài lý thuyết pass (trước: bấm "Hoàn thành" không báo backend) | ✅ |
| 8 | **Tân trang trang chủ + header**: bỏ thanh header (chỉ cụm nav nổi GIỮA đỉnh trang, đè lên nền, trong suốt hoàn toàn, lướt xuống trôi đi); chủ đề **vũ trụ toán học tím gradient** (sao/lưới/nebula/glyph O(log n) Σ √n.../quỹ đạo); panel demo terminal chạy engine THẬT; stats/demo/features glass; reveal khi cuộn; hero fade dần khi cuộn | ✅ |
| 9 | **Chòm sao tương tác (canvas global)**: điểm nối nhau, rê chuột → chòm kéo theo chuột; sao băng ~5s 1–2 cơn; số hạt 22–48; tôn trọng prefers-reduced-motion | ✅ |
| 10 | **Footer bê nguyên cấu trúc VisualizationDSA3** (`AppFooter.vue`): brand `~/` + 3 cột link + bottom bar — đổi sang tông tím, link route thật | ✅ |

## 2. BÀI HỌC KỸ THUẬT (lần sau đừng dính lại)

1. **jsdom không có `IntersectionObserver`** → directive v-reveal phải guard `typeof IntersectionObserver === 'undefined'` (nếu không 7 test HomeView fail).
2. **TS không giữ narrowing trong closure** cho tham số nullable → dùng non-null assertion (`canvas.getContext('2d')!`) hoặc guard ở caller.
3. **VLM nhỏ (qwen2.5vl:7b) bịa nội dung** khi xem ảnh toàn trang tối → phải **crop từng vùng + prompt cấm suy đoán** ("chỉ liệt kê những gì chắc chắn thấy, không bịa thêm") mới đọc chuẩn.
4. **Jint sandbox**: `while(true)` → `StatementsCountOverflowException` (KHÔNG phải TimeoutException) — phải catch cả 2 + MemoryLimitExceededException + RecursionDepthOverflowException.
5. **EF exercise `LessonId` NOT NULL** → exercise của node không có lesson phải gắn lesson cuối khóa.
6. Header absolute + app-shell padding-top 53px; trang chủ bù `margin-top: -53px` để hero tràn đỉnh; nền body dark `--color-background` = `#0B0A12` (đồng màu hero, hết dải xám).
7. Playwright verify: dev server đôi khi reload chậm sau khi sửa file (home render null) → **poll chờ element + retry 3 lần** trong script test.

## 3. THAY ĐỔI BACKEND

- **`CodelabJudgeService.cs`** (MỚI): chấm code bằng Jint — TimeoutInterval 1.5s / MaxStatements 200k / LimitMemory 32MB / StackOverflowGuard; `TryParseTasks` chỉ parse ConfigJson dạng ARRAY; so sánh output sau bỏ khoảng trắng (khớp FE normalizeOutput).
- **`ExerciseService.SubmitCodeAsync`**: khi ConfigJson array + có `TaskId` → server chấm lại (bỏ qua Score/Passed/Total client); node pass = TẤT CẢ task con có bài nộp full-pass (đếm qua ResultJson wrapper `{judged, entryFunction, ...}` + PassedTests==TotalTests); quest pass_node/code_run giữ nguyên.
- **`CodeSubmitRequest`**: +`TaskId` (fallback match theo entryFunction). **`CodeSubmitResultDto`/`CodeTestCaseResultDto`**: +`Error`.
- **`ConceptsController`**:
  - `IsNodeLockedAsync` (node trước theo SortOrder pass?) — dùng ở GET lessons/{id} (403), quiz/submit (403), SaveProgress (403).
  - `SaveProgress`: +`Completed` flag trong payload → node pass chỉ khi node KHÔNG có Code exercise (bài ASM bắt buộc qua code-submit chấm server); `LessonProgressPayload` +`Completed`.
  - `LessonDetailResponse` +`ExerciseId` (code exercise id để FE nộp server); `ConceptsLessonDto` +`Locked`.
  - `BuildLessonsAsync`: `Locked = !prevPassed && !passed` (duyệt theo SortOrder).
- **Seed** (`SeedGrokkingData` + `.Helpers.cs`):
  - Node practice đổi tên idempotent "Luyện tập tổng hợp"→"Final-Quizz"; `SeedFinalQuizzAsync` (20 câu); `SeedFinalCodingTestAsync` (3 task code, chuyển MCQ cũ → CODE giữ Id, tự sửa ConfigJson khi lệch); `BuildAssignmentConfig` xử lý config object đơn → mảng 1 task; assignment exercises tự chữa ConfigJson.
- **NuGet**: Jint 4.16.0 (BSD-2-Clause) — đã ghi THIRD_PARTY.md (phiên bản 1.4).

## 4. THAY ĐỔI FRONTEND

- **Header (`AppHeader.vue`)**: KHÔNG còn thanh header — cụm brand + nav + action nổi GIỮA đỉnh trang (absolute, transparent, không blur/border), padding-top 30px, chữ nav **mono + letter-spacing 0.06em + gap 2rem**; mục hiện cho CẢ KHÁCH; hamburger mobile < 900px (menu glass); user dropdown glass tím.
- **Trang chủ (`HomeView.vue`)**: vũ trụ tím gradient — sao tĩnh 2 lớp + lưới tọa độ + nebula (24s) + glyph tĩnh + quỹ đạo (60s) + panel demo terminal (3 chấm mac, `simulator://bubble-sort`, LIVE, engine thật) + stats dải glass (4 stat) + 3 demo card + 3 feature card + CTA band + **v-reveal** (fade+slide theo scroll, stagger) + **hero fade dần khi cuộn** (opacity 0.92 over 420px, copy translateY parallax).
- **`useCosmicField.ts`** (MỚI, global trong App.vue): chòm sao nối điểm + theo chuột + sao băng 4–6.5s 1–2 cơn; 22–48 hạt; reduced-motion → 1 khung tĩnh.
- **`AppFooter.vue`** (MỚI, bê VDSA3): thay footer cũ; ẩn trên lesson-study/courses.
- **`lessonApi.ts`**: +`exerciseId` trong LessonDetailResponse; `submitCodelab(exerciseId, code, taskId)` → POST /exercises/{id}/code-submit (clientRequestId randomUUID); message lỗi 403 từ backend.
- **`useLessonStore.ts`**: +`lessonFinished` (set khi markLessonCompleted, gửi `completed` trong payload); lessonMeta +`exerciseId`; loadLesson bắt message lỗi.
- **`LessonStepCodeLab.vue`**: submit → chạy thử client → **gửi server chấm** (exerciseId + taskId); server từ chối → báo lỗi, không pass; bỏ syncXP(50)/task + xoá useUserProgressStore (dead code).
- **`LessonStudyView.vue`**: sidebar khoá (lock icon, disabled), finishLesson → syncToServer(force) gửi completed; truyền exerciseId.
- **`CourseDetailView.vue`**: lesson locked → icon lock + "Bị khóa" + disabled.
- **`styles/tokens.css`**: dark `--color-background` #17191D → **#0B0A12**.
- **Tests**: HomeView.spec cập nhật (stats 3 card + mono 100%, hero 2 CTA + band 2 CTA, demo-run button); auth.spec mock +xp/level.

## 5. VERIFY ĐÃ CHẠY (phiên này)

- BE: `dotnet test` = **166 PASS** (153 cũ + 13 mới CodelabJudgeTests: judge đúng/sai/compile/timeout/stack-guard/parse; SubmitCode server-judge: đúng→pass node, sai→không pass, khai 100/100→bị bỏ qua, thiếu/sai TaskId→400).
- FE: vue-tsc sạch · vitest **164 PASS** · `npm run build` PASS.
- E2E (node http + Playwright, dữ liệu THẬT, user tạm đăng ký):
  - Suite kiểm tra cuối 29/29 (A: Final-Quizz 10 · B: 3 bài code 10 · C: tích hợp 4 · D hồi quy) — báo cáo: `session/quanly/TESTCASES_FINAL_TEST_2026-08-15.md`
  - Khoá node 16/16 (L1–L15 + L14a)
  - ASM server-judge 14/14 (A1–A14)
  - Nghiệp vụ 8/8 (P1–P8)
  - Full-walk 4 Assignment 24/24 (ASM1 3 task, ASM2 2 task, ASM3 1 task getUserInfo, ASM4 1 task inorderTraversal)
  - Vision-review (Ollama qwen2.5vl:7b): nav/title/bench/footer đọc chuẩn.

## 6. TÀI KHOẢN

- `student@demo.local / Student@123` · `teacher@demo.local / Teacher@123` · `admin@system.local / Admin@123`.

## 7. TỒN ĐỌNG / VIỆC TIẾP THEO

1. **CHƯA COMMIT GÌ** (cả phiên 1 lẫn phiên 2) — commit theo `.\commit-as.ps1 {son|bao|thu|phuc}` (FE→son, BE→bao, engine/test→thu, docs→phuc), PR base `dev`.
2. **Dọn user test rác trong DB demo** (15 user: locktest_* 7, asmtest_* 3, fullwalk_* 5 — Id 81–95, kèm progress/submissions) — user đã chốt: **xóa MỘT LẦN khi mọi việc xong**; xoá theo FK (UserNodeProgress, ExerciseSubmissions, CodeSubmissions, UserProgress, ...).
3. **Docs đồng bộ** (task dev-docs): SRS/SDD/API_REFERENCE/SCREEN_MAP — khoá node, chấm server (Jint), Final-Quizz/Kiểm tra cuối, `/courses` UI mới, dark theme #0B0A12.
4. **THIRD_PARTY.md** đã thêm Jint 4.16.0 (v1.4) — nhớ đồng bộ `dotnet list package` nếu thêm gói.
5. **Vision-review pipeline đã dùng được**: `.\tools\vision-read.ps1 -ImagePath <png> [-Prompt "..."]` (Ollama qwen2.5vl:7b local) — dùng CROP vùng + prompt cấm suy đoán; console UTF8: `[Console]::OutputEncoding = [Text.Encoding]::UTF8`.
6. Tồn đọng cũ (chưa đụng): refresh-loop 401 (client.ts:110-112), PROMPT_VISUALIZE_UPGRADE bước Viz, xoá `frontend/src/data/lessons.ts` + PathView/PathRedirectView/NodeHubView (route đã redirect).

## 8. MẸO NHANH (verify phiên sau)

```powershell
# Backend (dừng dotnet cũ trước để tránh file-lock: Get-Process dotnet | Stop-Process -Force)
$env:ConnectionStrings__Default = "Server=localhost;Database=DsaVisual;User Id=dsa_app;Password=DsaVisual@Dev123;TrustServerCertificate=True"
$env:ASPNETCORE_ENVIRONMENT = "Development"
Start-Process dotnet -ArgumentList "run --project src/DsaVisual.Api --launch-profile http" -WorkingDirectory "...\backend" -WindowStyle Hidden

# FE (từ frontend/)
cmd /c "npm run dev -- --port 5173 --strictPort"

# Seed lại (idempotent — tự chữa ConfigJson ASM nếu lệch)
dotnet run --project src/DsaVisual.Api -- --seed

# Vision-read ảnh (crop vùng trước bằng Playwright, prompt cấm suy đoán)
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
.\tools\vision-read.ps1 -ImagePath "anh.png" -Prompt "Chỉ liệt kê những gì bạn CHẮC CHẮN thấy, không bịa thêm: chữ, màu, bố cục."
```
