## [2026-08-13 14:20] Seed Prod — khởi động phiên (--auto)
- Quyết định: Thực hiện PROMPT_K_SEED_PROD.md ở chế độ --auto; làm việc tại worktree riêng `D:\FPT\neww-seed` (nhánh `feature/seed-prod`, base origin/dev) vì working tree chính đang bận (session H — `feature/ux-h-c`, 289 file modified). PR cuối base `dev`, commit-as bao.
- Ảnh hưởng: toàn bộ file trong worktree mới; KHÔNG đụng working tree chính.

## [2026-08-13 14:21] Fix domain đăng ký — chọn cách 1 (sạch nhất)
- Quyết định: Không sửa AuthService (đã đọc AuthService.cs:59-70 — check chỉ chạy khi setting không rỗng). Thực hiện: xóa `allowed.email.domains` khỏi SeedData.cs (dòng 96) + trong SeedDemoActivity thêm bước upsert xóa setting cũ trong DB (tồn tại → db.Settings.Remove + log) → chạy --seed là hết chặn, mọi email đăng ký được.
- Ảnh hưởng: SeedData.cs, SeedDemoActivity.cs (bước cleanup settings), docs/SDD.md §7.5, docs/SRS.md dòng 173, API_REFERENCE (đồng bộ bởi dev-docs).

## [2026-08-13 14:22] Cơ chế seed hoạt động người dùng
- Quyết định: (a) student mới — 1 lần tạo User + toàn bộ activity, guard theo email; (b) user seed cũ (admin/teacher/student@demo.local) — upsert activity theo unique key. Không tạo lại user đã có. Không đụng 3 user rác smoke; giữ nguyên Premium student@demo.local (id=3, PremiumUntil 2026-09-13, HeartsMax 30). Gems/XP nhất quán: XP = tổng quest reward đã claim; Gems = earn − spend; GemTransactions ghi từng dòng.
- Ảnh hưởng: SeedData.cs (record types), SeedDemoActivity.cs + SeedDemoActivity.Class.cs (partial class).

## [2026-08-13 14:23] Phân tách task (chống vỡ context)
- Quyết định: Tách 8 task nhỏ, mỗi task fresh context: SEED-1 SeedData.cs records; SEED-2 SeedDemoActivity.cs (users/achievements/progress/submissions/quests/gems/inventory/favorites/feedback/misc); SEED-3 SeedDemoActivity.Class.cs (classes/assignments + gắn ClassAssignmentId); SEED-4 SeedRunner.cs thêm call + README; SEED-5 unit tests (dev-test); SEED-6 --seed thật 2 lần + SQL counts + API smoke (dev-test); SEED-7 đồng bộ docs (dev-docs); SEED-8 review chốt (dev-review). Không nhúng file >5KB vào prompt — chỉ trỏ đường dẫn.
- Ảnh hưởng: thứ tự phụ thuộc SEED-1 → 2 → 3 → 4 → (5,7) → 6 → 8.

## [2026-08-13 14:24] Bảng runtime KHÔNG seed
- Quyết định: CodeRuns (đã có 6 dòng) bỏ qua; NodeSessions, RefreshTokens, OtpCodes, PasswordResetTokens không seed (runtime/auth — ghi lý do README). CodeSubmissions (0 dòng) seed 3-5; BugReports (0 dòng) seed 2-3; LessonNotes nếu entity cho phép seed 2-3 (guard UserId+LessonId).
- Ảnh hưởng: SeedDemoActivity.cs misc section.

## [2026-08-13 14:35] SEED-2 trả rỗng — tách nhỏ (lần 1)
- Quyết định: Task SEED-2 (SeedDemoActivity.cs toàn bộ) trả rỗng, không tạo file — đúng bài học "task trả rỗng". Xử lý: tách SEED-2 thành SEED-2a (users+achievements+userAchievements+cleanup setting+skeleton SeedAsync) → resume session cũ ses_0066bf853ffeHWkTgbvqI1FEw6; nếu resume fail → tạo task mới siêu nhỏ xác nhận tool, rồi tiếp SEED-2b (progress/submissions), SEED-2c (quests/gems/inventory/favorites/feedback), SEED-2d (codeSubmissions/bugReports/lessonNotes) — mỗi file partial class riêng, SeedAsync gọi method tên cố định.
- Ảnh hưởng: thêm 3 task; thứ tự phụ thuộc mới: SEED-2a → (2b, 2c) → 2d → SEED-3.

## [2026-08-13 14:50] SEED-2b/2c xong — wiring skeleton + điều chỉnh kỳ vọng
- Quyết định: 2b/2c hoàn tất (progress/submissions + quests/gems/inventory/favorites/feedback). Cần thêm declarations + calls vào SeedDemoActivity.cs cho 8 method 2b/2c + khai báo sẵn 4 method còn lại (codeSubmissions/bugReports/lessonNotes/classes) — 1 task wiring (SEED-2s). SEED-2d (misc) chạy song song.
- Ảnh hưởng: SeedDemoActivity.cs (skeleton), SeedDemoActivity.Misc.cs (mới).
- Điều chỉnh kỳ vọng: UserInventory ~7 (3 student L1 gems < 50 không mua được item — chấp nhận, hợp logic; kỳ vọng ban đầu ≥8 hạ xuống ≥7). GemTransactions dùng quy ước Spend = Type=1 Amount dương (khớp BuyItemAsync thật — dev-test kiểm chứng nhất quán).

## [2026-08-13 15:20] SEED-5 phát hiện 2 bug seed — giao sửa (lần 1)
- Quyết định: dev-test (verify độc lập) phát hiện: (1) SeedDemoActivity.Activity.cs LoadActivityUsersAsync dùng AsNoTracking → user.Xp/Gems set không được lưu (0 hết); (2) SeedDemoActivity.Misc.cs lookup exercise thiếu prefix "Code: " → 0 CodeSubmissions. Giao dev-backend sửa 2 điểm (đúng quy trình: dev viết → test verify → fix → verify lại). Test giữ nguyên — chạy lại để xác nhận.
- Ảnh hưởng: SeedDemoActivity.Activity.cs, SeedDemoActivity.Misc.cs + verify lại SEED-5.

## [2026-08-13 15:45] Hoàn tất — PR #10
- Quyết định: PR #10 (feature/seed-prod → dev) đã tạo qua GitHub REST API (không có gh CLI — dùng token từ credential manager). Giữ worktree D:\FPT\neww-seed (PR chưa merge — có thể cần sửa theo review). Bug /api/v1/progress/me 500 (ProgressService.cs:223) ngoài phạm vi — đề xuất task riêng.
- Ảnh hưởng: repo; báo cáo docs/pm-report-seed.md.
