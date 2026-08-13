# Audit BỀ MẶT — Migration / Seed / DbContext lifetime

- Ngày: 2026-08-13
- Phạm vi: `backend/src/DsaVisual.Application/Persistence/` (Entities, Configurations, Migrations, Seed) + `Program.cs` + singleton services
- Nhánh làm việc thực tế: `feature/ux-h-c` (đề bài ghi `feature/backend-audit` — xem Ghi chú)
- Phương pháp: chỉ đọc + `git diff --no-index` (không chạy `dotnet ef`, không sửa code)
- Chuỗi migration: `20260812061254_InitialCreate` → `20260812180545_WidenContentFeedbackComment` → `20260812182232_AddOtpCodes` (mới nhất)

## Kết quả chính

- **(a) Migration khớp entity**: 33/33 entity khớp 100% với `AppDbContextModelSnapshot` (so từng property, type, maxLength, default, index, FK, DeleteBehavior). Snapshot == `20260812182232_AddOtpCodes.Designer.cs` (diff chỉ khác boilerplate: `[Migration]`, tên class, `BuildTargetModel`). `Users.HeartsMax default 10` ✓ (entity:16, UserConfiguration.cs:20, InitialCreate, snapshot). `PremiumSubscriptions`/`NodeSessions`/`UserNodeProgress`/`OtpCodes` ✓. Không có `RowVersion`/concurrency token (grep sạch).
- **(b) Seed idempotent**: ĐÚNG cho chạy lại tuần tự — mọi bảng đều check tồn tại (Any/FirstOrDefault) trước Insert, không có try-catch nuốt lỗi (exception lan ra → fail rõ ràng). Có 5 khoá được chốt bằng unique index DB; 5 tập khác chỉ chống trùng ở tầng ứng dụng (xem #2).
- **(c) DbContext lifetime**: `AddDbContext` mặc định Scoped ✓ (Program.cs:96). 4 singleton (TokenService, SettingsCache, LoginAttemptTracker, SubmissionLockRegistry) **không** nhận `AppDbContext` (constructor chỉ nhận IConfiguration/IDateTimeProvider hoặc không có) → không có captive dependency. Không có `IHostedService`/`BackgroundService`/`Timer`/`Task.Run` giữ DbContext. Không có `new AppDbContext` ngoài DI.
- **(d) Migration lịch sử**: tên + ngày hợp lý (3 migration cùng ngày 2026-08-12, thứ tự tăng dần, đúng feature commit GP-T1/GP-T2); git history cho thấy mỗi migration tạo 1 lần, không bị sửa/xoá sau merge (`--diff-filter=M` chỉ là snapshot được cập nhật bởi chính migration mới — hành vi EF bình thường). ProductVersion 10.0.11 đồng nhất snapshot ↔ designer.

## Bảng findings

| # | file:dòng | mức | mô tả | khuyến nghị |
|---|-----------|-----|-------|-------------|
| 1 | `Configurations/LessonConfiguration.cs` (thiếu class cho LessonSimulation) + `Migrations/AppDbContextModelSnapshot.cs:771` + `Entities/LessonSimulation.cs:3` | TRUNG | Entity comment khai "UNIQUE (LessonId, SimulationKey)" (SDD §7.3.8) nhưng DB chỉ có index thường `IX_LessonSimulations_LessonId` — không có unique index, không có configuration class cho bảng này. Lệch doc ↔ schema; đồng thời bỏ mất lớp bảo vệ DB cho seed (xem #2). | Thêm `LessonSimulationConfiguration` + migration bổ sung `HasIndex(LessonId, SimulationKey).IsUnique()` (hoặc sửa comment entity nếu unique cố ý bỏ); cập nhật snapshot. |
| 2 | `Seed/SeedRunner.cs:173,261,292,323,366,396,418,435,452` | TRUNG | Seed idempotent tuần tự 100% (check trước insert ở mọi bảng) nhưng 5 tập khoá KHÔNG có unique index DB chốt: Lessons `(TopicId, Title)` (chỉ có IX_TopicId), Exercises `(LessonId, Title)`, LearningPaths `Title`, LearningPathNodes `(PathId, Title)` (chỉ unique `(PathId, SortOrder)`), LessonSimulations `(LessonId, SimulationKey)`. Chạy `--seed` đồng thời (2 instance / retry sau crash giữa chừng) có thể tạo trùng. Các khoá được chốt DB: Users.Email, Topics.Name (root), DailyQuests.QuestKey, ShopItems.ItemKey, Settings.Key. | Thêm unique index cho 5 tập khoá trên qua migration mới; hoặc khoá seed bằng transaction + `MERGE`/upsert. |
| 3 | `Seed/SeedRunner.cs:104,144,197,237,283,314,345,379,446,491,530,558,585` | THAP | Seed không bọc transaction: `SaveChangesAsync` từng entity riêng lẻ → crash giữa chừng để lại dữ liệu lửng (rerun tự vá nhờ idempotent, không hỏng). Không phải lỗi blocking nhưng thiếu atomicity; cũng không thể lock toàn bộ seed khi chạy song song. | Bọc `db.Database.BeginTransactionAsync()` quanh `SeedAsync` (hoặc ít nhất các nhóm insert liên quan FK vòng FinalTestId). |
| 4 | `Entities/ContentFeedback.cs:10` | THAP | Comment entity cũ: "Comment ... ≤ 200 ký tự" nhưng migration `20260812180545_WidenContentFeedbackComment` đã mở rộng cột lên `nvarchar(1000)` (ProgressConfiguration.cs:86). Chỉ lệch doc comment, schema nhất quán (1000 cả config lẫn snapshot). | Cập nhật comment entity thành "≤ 1000 ký tự". |
| 5 | `Persistence/AppDbContext.cs:7` | THAP | Doc comment khai "32 bảng (24 + 8)" nhưng có 33 DbSet (OtpCodes là bảng 33 — dòng 17 tự ghi chú "bảng 33"). Số liệu doc lệch nhỏ. | Sửa comment thành "33 bảng (25 + 8)". |

## Ghi chú

- Nhánh đang checkout là `feature/ux-h-c`, không phải `feature/backend-audit` như đề bài — toàn bộ kết quả audit theo trạng thái code hiện tại trên nhánh này.
- (d) không phát hiện bất thường nào về tên/ngày/việc sửa migration đã merge.
- Seed chỉ chạy qua cờ `--seed` (Program.cs:172), sau `MigrateAsync()` — không tự chạy khi khởi động thường, đúng thiết kế.

## Tổng kết

- Tổng findings: **5** — CAO: **0** — TRUNG: **2** — THAP: **3**
- Trục bề mặt (migration ↔ entity ↔ snapshot, seed idempotent tuần tự, DbContext Scoped + singleton sạch) hiện TỐT; rủi ro còn lại tập trung ở thiếu unique index DB cho 5 khoá seed + transaction bao seed.
