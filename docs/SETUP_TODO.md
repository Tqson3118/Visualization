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
