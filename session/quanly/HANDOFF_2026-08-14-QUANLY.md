# HANDOFF — 14/08/2026 (quản lý tổng — cập nhật cuối ngày ~13:00)

> File này tóm tắt TOÀN BỘ trạng thái dự án tính đến 13:00 ngày 14/08. Phiên mới đọc file này là nắm hết, KHÔNG cần đọc lịch sử cũ. Trạng thái verify bằng máy (SQL thật / test chạy lại / GitHub API).

## 1. DỰ ÁN
- Đồ án tốt nghiệp FPT (GVHD Phạm Ngọc Ái Liên, lớp SD21361, 4 thành viên): **DSA-Visual**. Bảo vệ cuối tháng 8/2026 (~2 tuần). **CẤM ghi "20 tuần/16 tuần" — chỉ 13 tuần.**
- Nhóm: Mai Tiểu Bảo (backend) · Thái Quang Sơn (frontend) · Huỳnh Lê Minh Thư (engine+test) · Trần Viết Tâm Phúc (tài liệu).
- Repo `Tqson3118/Visualization`, PR base `dev` KHÔNG main. Commit-as: `.\commit-as.ps1 {son|bao|thu|phuc}`.

## 2. TRẠNG THÁI TỔNG (dev @ 7140719 → + #22/#24/#25/#26 — tip mới nhất là merge #26; đã push, local sync)
- ✅ **TẤT CẢ PR đã đóng/merge (trừ #1)**: #20 vis-upgrade · #21 seed-v2 · #23 system-logic-overhaul · #22 urgent-fix (rebase + 12 conflict) · #24 premium-lib · #25 fix 429 message · #26 fix rate-limit refresh. **#19 ui-premium ĐÃ ĐÓNG** (stale — tách 8 file additive → #24).
- ✅ **DB thật**: migration `FullBusinessLogicAndClassOverhaul` (7 cột: AcademicDegree/ProfileLink/IsClassOnly/PublishedAt/RejectionReason/AllowLateSubmission/AdminNote) đã apply. Seed V2: Users 95, showcase@demo.local top 1 (Xp 2790, Level 6, premium 12 tháng).
- ✅ **Deploy Docker 14/08**: daemon tắt → khởi động, rebuild FE+BE bản mới nhất, /health OK, login qua :8081 OK.
- ✅ **Bug login 429 đã fix gốc** (PR #25 + #26): nguyên nhân = token cũ → refresh storm (/auth/refresh 401 × N) đốt hết quota sensitive 60/phút chung với login; refresh/logout giờ thuộc general limit (300/phút); FE redirect /login chỉ 1 lần; LoginView hiện message riêng khi 429.
- ✅ **Test hiện tại**: FE **vitest 174/174** + build PASS · BE **148 unit + 78 integration** (perf#9 AdminStats FLAKY pre-existing — chạy lại lần 2 thường PASS; backlog stabilize).

## 3. QUY TRÌNH ĐÃ CHUẨN HÓA (bài học lớn nhất phiên)
- **Skill `pm-prompt-std` nâng cấp**: BƯỚC 0 — audit code thật + DB thật (~20-30 file, chạy sqlcmd) TRƯỚC khi viết prompt; mục HIỆN TRẠNG (đã audit) + Thế mạnh GIỮ LẠI; task spec có số liệu (formula/ngưỡng/edge case + anti-hardcode test); TIÊU CHUẨN đo bằng máy (build/vitest/SQL/byte-diff); THỨ TỰ dependency + commit list viết sẵn; bảng delta khi nâng cấp prompt cũ. Tham chiếu: `session/PROMPT_VISUALIZE_UPGRADE_V2.md`, `session/PROMPT_UI_PREMIUM_ROUND2.md`, `session/PROMPT_K_SEED_PROD_V2.md` (bản đã nâng cấp — audit phát hiện 8 lỗi bản gốc).
- **Review PR chuẩn**: verify bằng máy (chạy worktree + build + test + SQL + API), không tin claim trong PR body. Phát hiện mẫu: vue-tsc plain pass nhưng `npm run build` fail; 0 test mới cho 5 khối nghiệp vụ; 0 docs sync.
- **Rebase + resolve conflict chuẩn**: worktree riêng → rebase → resolve từng conflict (ưu tiên giữ logic mới nhất + lấy UX cũ nếu tốt) → grep marker còn sót = 0 → build/test → force-push-with-lease → merge.

## 4. PROMPT / SESSION
| Prompt | Trạng thái |
|---|---|
| PROMPT_VIEW_QUALITY_MASTER_V2 | ✅ DONE + MERGED (36/36 view) |
| PROMPT_VISUALIZE_UPGRADE_V2 | ✅ DONE + MERGED (PR #20 — trace playback, wrap, transition) |
| PROMPT_K_SEED_PROD_V2 | ✅ DONE + MERGED (PR #21 — 95 users, showcase) |
| PROMPT_UI_PREMIUM_ROUND2 | 🔒 PR #19 ĐÓNG — đã tách phần giá trị → PR #24 premium-lib MERGED |
| PROMPT_M_FINAL_REVIEW | ⏳ CHƯA CHẠY — kế tiếp |
| PROMPT_N_OPTIMIZATION | ⏳ sau M |

## 5. TỒN ĐỌNG (thứ tự)
1. **PR #1 dev→main** — chờ USER duyệt (không tự merge).
2. **PROMPT_M** (final review 7 trục) → **PROMPT_N** (tối ưu backlog, đo trước/sau).
3. **Backlog**: perf#9 flaky (stabilize — UTC midnight/ActiveUsersToday) · NU1903 SSH.NET (test-only) · UserInventory 66<80 (trần toán học, decision log seed-v2).
4. **18 ảnh báo cáo** → docx pandoc (từ `tailieu/`, lệnh đúng: `& pandoc BAO_CAO.md -o BaoCaoDoAn.docx --toc` — pandoc resolve ảnh theo cwd); điền ngày bảo vệ (SETUP_TODO — cần user).
5. **Dọn worktree đã merge/xong**: `trees/system-logic`, `trees/urgent-fix`, `trees/premium-lib`, `trees/ui-premium` (PR đóng) — `git worktree remove`; giữ `neww-diagrams` (chờ 6 ảnh, KHÔNG merge).
6. 2FA cần SMTP thật (đang MailHog).

## 6. MÔI TRƯỜNG & CÔNG CỤ
- **Docker laptop**: FE `http://localhost:8081` (nginx proxy /api → backend), BE `:5000` (health `/health`), MailHog `:8025` (SMTP 1025), SQL Server 1433 (DsaVisual, sa/DsaVisual@Dev123). Rebuild: `docker compose build && docker compose up -d` từ `D:\FPT\neww`.
- Accounts: student `student@demo.local/Student@123` (premium) · showcase `showcase@demo.local/Student@123` (Level 6, top 1) · teacher `teacher@demo.local/Teacher@123` · admin "Quản trị viên".
- **Rate limit**: sensitive (login/register/2fa/reset/change-password/join) 60/phút/IP; refresh/logout → general 300/phút. 429 hiện message riêng (FE). Bị 429 → chờ 60s.
- **GITHUB_TOKEN** setx user-level; REST API (không gh CLI). **EXA_API_KEY** hardcode opencode.jsonc. Ollama `qwen2.5vl:3b` @ localhost:11434. Word COM 16.0 có (render PDF docx).
- Verify FE: `npm run build` (bắt buộc — vue-tsc plain KHÔNG đủ) + `npx vitest run` (174). Verify BE: `dotnet build` + `dotnet test` (148+78; perf#9 flaky → chạy lại lần 2).
- Migration mới apply bằng: `dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api` (backend/ — đã cài dotnet-ef 10.0.10).

## 7. GIT WORKTREES CÒN TỒN TẠI
`trees/system-logic` · `trees/urgent-fix` · `trees/premium-lib` · `trees/ui-premium` (đều MERGED/ĐÓNG — dọn được) · `neww-diagrams` (feature/diagrams — KHÔNG merge, chờ ảnh) · `neww-seed/teacher/qbase/qa/qb/qc/qd/qp2/qdocs` (cũ, merge xong — dọn cuối đợt).

## 8. BÀI HỌC (bổ sung phiên 14/08 chiều)
1. **Số liệu thật > lời khai báo**: PR body nói "vue-tsc 0 lỗi" nhưng `npm run build` gãy 2 type error; "17/17 acceptance" nhưng 0 test BE mới. Luôn chạy lại build/test trong worktree.
2. **PR cũ (stale)**: đừng merge nguyên khối — đóng + tách phần additive (file MỚI không conflict) thành PR nhỏ (#19 → #24).
3. **Rate-limit sensitive + refresh storm**: refresh/logout không được chung partition với login (self-DoS); FE redirect phải singleton.
4. **Rebase nhiều PR chồng nhau**: merge nhánh có logic nền trước (#23), nhánh sau rebase + resolve (ưu tiên logic mới + lấy UX cũ tốt); grep marker `<<<<<<<` = 0 trước khi build.
5. **pandoc docx**: chạy từ `tailieu/` (resolve ảnh theo cwd) — chạy từ root = ảnh mất.
6. **Docker deploy**: daemon có thể tắt (docker info fail) → khởi động Docker Desktop trước; backend KHÔNG auto-migrate khi start (chỉ khi `--seed`) → phải `dotnet ef database update` riêng.
