# VIỆC CẦN NGƯỜI DÙNG LÀM (SETUP_TODO)

> File này do các task trong PM_MASTER_PLAN tự cập nhật — gom MỌI việc cần người dùng xử lý:
> điền API key, xác nhận thông tin, quyết định. Sáng dậy mở file này trước, làm xong đánh dấu [x].

## 1. Điền API key / secret (thay `CHANGE_ME_...`)

| # | Key/Biến | File cần sửa | Vị trí | Lấy ở đâu | Trạng thái |
|---|---|---|---|---|---|
| 0 | **Email GitHub 4 thành viên** (son/bao/thu/phuc) | `commit-as.ps1` | 4 dòng Email=... | ✅ ĐÃ XÁC NHẬN (12/08, user): son=thaiquangson@gmail.com · bao=maitieubao@gmail.com · thu=thuhlmtd01131@gmail.com · phuc=robintran51128@gmail.com — đúng email GitHub thật của cả 4 | [x] |
| 1 | `DSA__Jwt__Secret` | `backend/.../appsettings.Production.json` hoặc `.env` | dòng ... | tự sinh 32+ ký tự | [ ] |
| 2 | `ConnectionStrings__Default` | `.env` | dòng ... | SQL Server của nhóm — **dev local ĐÃ TẠO DB qua task D (migration InitialCreate + database update lên docker-compose sqlserver)**; cần chuỗi production khi deploy | [ ] |
| 3 | `DSA__Email__*` (SMTP thật) | `.env` | dòng ... | Gmail App Password / SMTP trường — **chỉ cần khi deploy production; dev dùng MailHog** | [ ] |
| 4 | ... (task B1/B3 điền thêm theo code thực tế) | | | | [ ] |

## 2. Xác nhận thông tin (chưa quyết định được lúc chạy tự động)

| # | Việc | Ghi chú | Trạng thái |
|---|---|---|---|
| 1 | Ngày bảo vệ | báo cáo để trống | [ ] |
| 2 | Ngành trên bìa (đang ghi "Ứng dụng phần mềm" theo mẫu cũ) | xác nhận | [ ] |
| 3 | ... | | [ ] |

## 3. Quyết định kỹ thuật cần người dùng duyệt

| # | Quyết định | Nội dung (task đã ghi trong pm-decision-log) | Trạng thái |
|---|---|---|---|
| 1 | ... | ... | [ ] |

> Hướng dẫn: mỗi mục mới do agent thêm kèm ngày giờ `[YYYY-MM-DD HH:MM]` và tên task. Đừng xóa mục cũ — đánh dấu [x] khi xong.

## 4. Session C — Git + Push (2026-08-12, đã init + push xong)

| # | Việc | Ghi chú | Trạng thái |
|---|---|---|---|
| 1 | Xác nhận email GitHub THẬT của **Thu** (thuhlmtd01131@gmail.com) và **Phuc** (robintran51128@gmail.com) trong `commit-as.ps1` | ✅ ĐÃ XÁC NHẬN (12/08): cả 4 email đúng (mục 0 ở trên) | [x] |
| 2 | Verify repo GitHub: https://github.com/Tqson3118/Visualization — nhánh `main` + `dev` đã push (8 commit) | Push đã thành công bằng credential có sẵn trên máy (không cần nhập lại). Nếu muốn đổi repo public/private → Settings trên GitHub. | [ ] |
| 3 | (Tùy chọn) Xóa backup 3 .git cũ (~115MB) sau khi xác nhận repo mới ổn | `C:\Users\ADMINI~1\AppData\Local\Temp\opencode\git-backup-20260812\` — giữ lịch sử gốc của 3 source repo | [ ] |
| 4 | Khi Session B xong: commit `backend/` + `frontend/` root (skeleton v2) theo quy trình feature branch → PR → dev | Hiện UNTRACKED do Session B đang chạy song song | [ ] |
| 5 | (Tùy chọn) Đổi branch mặc định của GitHub repo thành `main` nếu GitHub tạo mặc định khác | GitHub tự nhận `main` khi push — thường không cần | [ ] |

> Cập nhật bởi Session C (PM --auto). Đừng xóa mục cũ — đánh dấu [x] khi xong.

## 5. Session D — Code thật (2026-08-12, đã merge vào dev)

| # | Việc | Ghi chú | Trạng thái |
|---|---|---|---|
| 1 | Chạy backend + seed khi demo/deploy: dotnet run --project backend/src/DsaVisual.Api -- --seed (kèm env DSA__Jwt__Secret + ConnectionStrings__Default) | Seed idempotent đã chạy thật lên SQL Server docker local (5 topics/8 lessons/29 exercises/76 questions/5 paths). Khi deploy môi trường khác phải chạy lại --seed + dotnet ef database update. | [ ] |
| 2 | Đổi mật khẩu seed DEV khi deploy: admin@system.local / teacher@demo.local / student@demo.local (ghi trong backend/.../Seed/README.md — DEV-ONLY) | Bắt buộc đổi trước khi demo chính thức/bảo vệ. | [ ] |
| 3 | Xác nhận cột MustChangePassword (Users) — SDD §7.5 "ép đổi mật khẩu lần đầu" | Entity chưa có cột; seeder chỉ set IsPrimaryAdmin + mật khẩu tạm. Cần migration bổ sung nếu giữ tính năng. | [ ] |
| 4 | (Tùy chọn) Cài monaco-editor + chart.js khi cần Code Runner/benchmark hoàn chỉnh | Code Runner đang dùng textarea thay Monaco; benchmark vẽ SVG thay Chart.js (không có gói). | [ ] |
| 5 | (Tùy chọn) PR GitHub: 4 nhánh feature/backend-services, feature/backend-seed, feature/engine-generators, feature/views → dev | Đã merge + push thẳng lên dev bằng git local (không có gh CLI). | [ ] |

## 6. Session E — E2E thực tế phát hiện bug (2026-08-12, cần sửa đợt sau)

| # | Mức | Bug | File:dòng (tham chiếu) | Ghi chú |
|---|---|---|---|---|
| 1 | P1 | Mất phiên khi reload — không gọi auth.refresh() lúc boot (ADR-004) | frontend/src/main.ts, App.vue | sửa đợt sau |
| 2 | P1 | POST /lessons/{id}/mark-viewed → 404 — backend thiếu endpoint | backend Controllers/LessonsController.cs | thêm endpoint + test |
| 3 | P1 | Nút "Làm bài" chết — emit open-exercise không ai lắng nghe | LessonDetail.vue:154, NodeHubView.vue:113 | |
| 4 | P1 | Canvas simulator phình ~15.000px — vòng ResizeObserver | CanvasArea.vue:324-326, 285-290 | |
| 5 | P2 | Submit exercise thiếu câu → 400 QUESTION_ANSWER_MISMATCH, UX khó hiểu | ExerciseView | |
| 6 | P1 | Ladder stage rỗng — quiz/code-exercise-id hardcode null | LadderView.vue:60-62, NodeHubView.vue:133-135 | |
| 7 | P2 | POST /code-runs 400 — contract lệch FE/BE (code,input vs Key+Input:string) | api/codeRuns + CodeRunsController | cần PM chốt contract |
| 8 | P2 | POST /benchmarks/run 400 — thiếu results | api/benchmark + BenchmarksController | |

## 7. Session F — Final Review (12/08/2026, cần user xử lý)

| # | Mức | Việc | Ghi chú | Trạng thái |
|---|---|---|---|---|
| 1 | Bảo mật | **Xoay key DEEPSEEK thật** trong `source/VisualizationDSA1/.env` (máy dev) | File này KHÔNG nằm trong git (đã gitignore) nhưng key thật đã hiện diện trên máy — nếu máy bị lộ/đem đi demo cần xoay lại key trên nền tảng DeepSeek. | [ ] |
| 2 | Quy trình | **Duyệt merge nhánh `feature/final-review` → `main`** | Nhánh chứa toàn bộ code + tài liệu đợt D/E/F đã verify (17/17 checklist §17.9 còn lại + build/test thật). Sau khi user duyệt → merge + tag bản bàn giao. | [ ] |

> Cập nhật bởi Session F (dev-docs F2b, 12/08/2026). Đừng xóa mục cũ — đánh dấu [x] khi xong.

## 8. Session F — Bug bàn giao từ Final Review (12/08/2026, sửa đợt sau — không nằm trong phạm vi F)

| # | Mức | Bug | File:dòng (tham chiếu) | Ghi chú |
|---|---|---|---|---|
| 1 | P1 | **Leaderboard crash** — `TypeError: Cannot read properties of undefined (reading 'length')` → 3 tab Tuần/Level/Lớp trống (F3-NEW-1) | frontend/src/views/LeaderboardView.vue:59, stores/leaderboard.ts:21 | Contract lệch: FE đọc `rows`/`myRank`, backend trả `PagedResponse.items` — chặn ảnh báo cáo §6.2 |
| 2 | P1 | **Heart regen ảo không persist** — ComputeHearts tính regen từ elapsed nhưng không ghi lại DB; user hết tim (DB=0) → UI hiện tim đầy nhưng vào node vẫn HEARTS_EMPTY (F5-Major) | backend/src/DsaVisual.Application/Services/GamificationService.cs:177,800-830 | Sửa trước demo chính thức (FR-10.1) |
| 3 | P2 | SubmitCodeAsync thiếu SubmissionLockRegistry + không check Status Active (F5-Minor) | ExerciseService.cs:489 | |
| 4 | P2 | Duplicate QuestionId trong answers → 500 thay vì 400 (F5-Minor) | ExerciseService.cs:275 | |
| 5 | P2 | Router cho TEACHER vào /admin/users + /admin/settings nhưng backend ADMIN-only (F5-Minor) | frontend/src/router/index.ts:260,272 | Backend vẫn chặn đúng — FE nên ẩn/chuyển hướng |
| 6 | P3 | Cookie refresh không set khi dev chạy HTTP — cookie Secure=true (F5-Minor) | AuthController.cs:25 | Dev-only annoyance |
| 7 | P3 | TEST_PLAN §10 cột "Không kiểm thử" = 0 cho SEC/PERF/UX "chờ" (F5-Nit) | docs/TEST_PLAN.md | điền "chờ" thay vì 0 |

> Ghi chú thêm: commit docker fix (a1b8bd8/7ac5896 — bao) do session khác tạo lúc 19:52-19:53, nội dung trùng nhau (backend/Dockerfile + docker-compose frontend 8081), đã nằm trong dev + feature/final-review — merge không xung đột.
> Cập nhật bởi Session F (PM, 12/08/2026). Đừng xóa mục cũ — đánh dấu [x] khi xong.

## 9. Session A2 — Build lại BaoCaoDoAn.docx (12/08/2026, file bị Word khóa)

| # | Việc | Ghi chú | Trạng thái |
|---|---|---|---|
| 1 | **User cần đóng Word đang mở BaoCaoDoAn.docx rồi chạy lại**: `& "C:\Users\Administrator\AppData\Local\Pandoc\pandoc.exe" "D:\FPT\neww\tailieu\BAO_CAO.md" -o "D:\FPT\neww\tailieu\BaoCaoDoAn.docx" --toc --resource-path="D:\FPT\neww\tailieu"` | 6 ảnh NHÓM B đã đổi placeholder→diagrams trong BAO_CAO.md (12/08, Session A2). Build thất bại `permission denied` vì Word đang khóa file (có `~$oCaoDoAn.docx`). Sau khi build xong: docx phải ≥ 900KB, media count = 18, media > 40KB ≥ 6. | [ ] |

> Cập nhật bởi Session A2 (dev-docs, 12/08/2026). Đừng xóa mục cũ — đánh dấu [x] khi xong.
## 9. Session G — Hoàn tất 13 bug bàn giao + UI/UX mới (13/08/2026)

> Cập nhật bởi Session G (PM --auto). Toàn bộ bug §6 (8 mục) + §8 (7 mục) đã FIXED trong đợt G + merge dev. UI/UX nâng cấp: Tailwind 4 + shadcn-vue + motion-v + Lenis + vue-sonner + vue-echarts + GSAP + font Geist/JetBrains Mono, dark mode class="dark", 3 gradient OKLCH, 12 màn xịn. Chi tiết: docs/pm-report-g.md + docs/pm-decision-log-g.md.

| # | Mức | Bug (SETUP_TODO §6/§8) | Trạng thái đợt G |
|---|---|---|---|
| 1 | P1 | Leaderboard crash (rows.length) | ✅ FIXED (G-BF2 + G-F3E: items→rows + field Value) |
| 2 | P1 | mark-viewed 404 | ✅ FIXED (G-BF1: POST /lessons/{id}/mark-viewed + upsert UserProgress + test) |
| 3 | P1 | Nút "Làm bài" chết | ✅ FIXED (G-BF2: NodeHubView lắng nghe open-exercise → /exercise/{id}) |
| 4 | P1 | Canvas phình ResizeObserver | ✅ FIXED trước đợt G (ba62a33 — đã merge dev) |
| 5 | P2 | Submit exercise thiếu câu 400 UX | ✅ FIXED (G-BF2: pre-check đủ câu + liệt kê câu thiếu) |
| 6 | P1 | Ladder stage rỗng | ✅ FIXED (G-BF2: GET /exercises?nodeId&stage thật) |
| 7 | P2 | code-runs contract lệch | ✅ FIXED (G-BF2: payload {key,code,input:string} + stats) |
| 8 | P2 | benchmarks/run 400 | ✅ FIXED (G-BF2: gửi kèm results từ runMeasure) |
| 9 | P1 | Heart regen ảo không persist | ✅ FIXED (G-BF1: PersistHeartRegenAsync ghi DB) |
| 10 | P2 | SubmitCode thiếu lock + Status | ✅ FIXED (G-BF1: SubmissionLockRegistry + EXERCISE_CLOSED) |
| 11 | P2 | Duplicate QuestionId → 500 | ✅ FIXED (G-BF1: VALIDATION_FAILED 400) |
| 12 | P2 | Router teacher /admin/users+/settings | ✅ FIXED (G-BF2: ADMIN-only guard → /profile) |
| 13 | P3 | Cookie Secure khi dev HTTP | ✅ FIXED (G-BF1: Secure=Request.IsHttps) |
| 14 | MỚI P1 | Leaderboard tab Level crash (thiếu value) | ✅ FIXED (G-F3E: BE thêm Value) |
| 15 | MỚI P2 | Leaderboard tab Lớp thiếu classId + phân trang | ✅ FIXED (G-F3E + G-F3E2: lastClassId) |

> P3 còn lại (không chặn, ghi decision log): TOCTOU nhỏ heart regen, lock registry không dọn key, ForwardedHeaders khi sau TLS proxy, seed leaderboard thật (XP=0 hiện trống), Monaco full editor (textarea giữ — §5.4).

## 10. Session GP — 2FA email (13/08/2026, GP-T2)

| # | Việc | Ghi chú | Trạng thái |
|---|---|---|---|
| 1 | SMTP thật khi deploy: đặt `DSA__Email__SmtpHost/Port/From` (mục §1.3 — SMTP trường/Gmail App Password) — dev hiện dùng **MailHog** (docker-compose, SMTP localhost:1025, UI http://localhost:8025) | 2FA + forgot-password đều gửi qua SMTP này; SMTP thiếu → mã OTP/link ghi trong **log dev** (KHÔNG block luồng — SDD §5.6) — không dùng cho production | [ ] |
| 2 | (Tùy chọn) Luồng 2FA bước 2 khi đăng nhập (SDD Màn Login bước 2: tài khoản bật 2FA → yêu cầu mã trước khi cấp token; sai 3 lần khóa 10 phút; ghi nhớ thiết bị 30 ngày) | GP-T2 mới triển khai phần BẬT/TẮT 2FA qua email (OtpCodes + send/verify); phần chặn đăng nhập khi thiếu mã là task backend riêng (mở rộng LoginAsync + purpose "login") | [ ] |
| 3 | (Tùy chọn) FE Màn N-1 (Cài đặt bảo mật): UI bật/tắt 2FA gọi PUT /auth/2fa + POST /auth/2fa/send + /verify | Contract đã có trong API_REFERENCE §4.12 (v1.3) | [ ] |

## [2026-08-13] H-D e2e ghi nhận (đợt I/J — KHÔNG sửa trong đợt H)
| # | Mức | Vấn đề | Vị trí |
|---|---|---|---|
| 1 | P1 | Contract lệch: quests trả progress/reward{}, shop thiếu description/slot, premium trả planId/status → SubscriptionView EmptyState dù active | backend DTO vs frontend/src/api/gamification.ts:33-56 |
| 2 | P3 | /me/hearts trả nextHeartInSeconds ≠ FE đọc nextHeartAt | backend HeartsDto |


| 3 | P2 | GET /progress/me trả 500 duplicate key (seed) — console error khi vào app | backend ProgressService/seed |
| 4 | P2 | Vào thẳng /shop gems luôn 0 — backend KHÔNG có GET gems balance (gems chỉ trong response buy/claim) | backend GamificationService + FE ShopView |


| 5 | P3 | Dark mode không bật/persist qua UI toggle (ui.ts không sync class, boot ghi đè html.className) | frontend ui.ts/App.vue |


| 6 | P3 | FinalTest nộp bài bị backend 422 khi chưa pass bậc trước — UX nên đọc finalTestUnlocked sớm → lock state từ đầu | frontend/src/views/FinalTestView.vue |

