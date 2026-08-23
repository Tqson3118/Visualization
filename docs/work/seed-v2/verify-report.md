# Verify Report — Seed V2 trên DB thật (SQL Server docker `DsaVisual`)

- Ngày chạy: 2026-08-14 04:40-04:55 UTC+7 (seed run 1: 04:40:30-04:40:43; run 2: 04:41:52-04:41:58; API smoke: 04:45-04:55)
- Môi trường: worktree `trees/seed-v2`, backend `dotnet run --project src/DsaVisual.Api -- --seed`, DB `Server=localhost;Database=DsaVisual` (container `neww-sqlserver-1`)
- Người verify: tester độc lập (không sửa code)
- Evidence: `seed-run1.log`, `seed-run2.log`, `sql-counts.txt`, `consistency.txt`, `consistency-nodes.txt`, `consistency-sample.txt`, `api-smoke.txt`, `api-run.log`, `api-run.err.log` (cùng thư mục)
- Ghi chú encoding: log run 1 viết qua console CP437 (từ `[Console]` mặc định) → ký tự tiếng Việt bị mã hóa lại (ê→├¬); đếm dùng pattern chịu mã hóa, kết quả xác nhận khớp nội dung đọc thủ công. Log run 2 chạy với `[Console]::OutputEncoding=UTF8` (vẫn Tee-Object UTF-16).

## Bảng kết quả

| Mục | Kết quả thật | PASS/FAIL | Ghi chú |
|---|---|---|---|
| Seed lần 1 | V2 thêm: Students 69, Achievements 7, UserAchievements 337, UserProgress 377, UserNodeProgress 602, ExerciseSubmissions 254, UserQuests 2856, GemTransactions 1360, UserInventory 60, Favorites 245, ContentFeedback 25, Classes 1, ClassMembers 53, ClassAssignments 13, BugReports 7, CodeSubmissions 7, Premium 1. Tổng cuối: Users=95, Achievements=17, UA=370, UP=409, UNP=636, ExSub=371, UQ=3136, GemTx=1501, Inv=67, Fav=272, FB=35, CodeSub=12, Bug=10, Notes=3, Classes=4 | PASS | Chạy 04:40:30-04:40:43 (~13s), thoát sạch "Seed hoàn tất". Khớp 100% số liệu docs/status.md |
| Seed lần 2 (idempotent) | `Select-String 'Seed: V2.*thêm [1-9]'` = **1 match**: `Seed: V2 CodeSubmissions thêm 7 / bỏ qua 0` | **FAIL** | Mọi phần V2 khác đều 0 thêm (Students 0/69, UA 0/337, UQ 0/2856, BugReports 0/7, Premium bỏ qua...). CodeSubmissions: 12 → 19 (mỗi run +7) |
| SQL: Users ≥ 85 | 95 | PASS | 26 V1 + 69 V2 |
| SQL: Achievements = 17 | 17 | PASS | |
| SQL: UserAchievements ≥ 300 | 370 | PASS | |
| SQL: ExerciseSubmissions ≥ 250 | 371 | PASS | |
| SQL: CodeSubmissions ≥ 10 | 19 | PASS (có bug nền) | ≥10 đạt nhưng 19 = 12 gốc + 7 bản sao do lỗi idempotency mục trên |
| SQL: Classes = 4 | 4 | PASS | DSA213(1), ADVNCE(2), 2TH0YJ(3 runtime test), GRPH21(4) |
| SQL: ClassMembers ≥ 60 | 66 | PASS | |
| SQL: ClassAssignments ≥ 20 | 20 | PASS | |
| SQL: ContentFeedback ≥ 20 | 35 | PASS | |
| SQL: BugReports = 10 | 10 | PASS | Status 0-3: 5/2/2/1 |
| SQL: UserNodeProgress ≥ 600 | 636 | PASS | |
| SQL: UserQuests ≥ 3000 | 3136 | PASS | |
| SQL: GemTransactions ≥ 1500 | 1501 | PASS | |
| SQL: UserInventory ≥ 80 | 67 | **FAIL (lệch đã biết)** | Trần toán học 61 do (gems≥0 + UNIQUE(UserId,ItemId) + XP persona); dự kiến ~67 theo quest-xp-showcase.md §4.1 — đúng dự kiến, dưới ngưỡng 80 |
| SQL: Favorites ≥ 250 | 272 | PASS | |
| SQL: UserProgress ≥ 400 | 409 | PASS | |
| SQL: LessonNotes = 3 | 3 | PASS | |
| Showcase: Xp ∈ 2600-2900 | 2790 | PASS | |
| Showcase: Gems ≥ 300 | 289 | **FAIL (lệch đã biết)** | quest-xp-showcase.md: earn 439 − spend 150 (2 item) = 289 — thiết kế chọn bù item cho showcase; dưới ngưỡng 300 |
| Showcase: StreakDays = 30 | 30 | PASS | |
| Showcase: LastActivityDate hôm nay | 2026-08-14 00:00:00 | PASS | = hôm nay UTC+7 (00:00 đầu ngày, seed chạy 04:40) |
| Showcase: PremiumUntil > now | 2027-07-15 04:40 | PASS | active |
| Showcase: HeartsMax = 30 | 30 | PASS | |
| Showcase: PlanId "12m" / Status 0 / OrderRef DSV{id}T12 | 12m / 0 / DSV2092T12 | PASS | Id thật 2092 → DSV2092T12 |
| BadNodes (Status=2 thiếu submission MaxScore) | Query gốc: 5; query điều chỉnh theo schema: **0** | PASS (có ghi chú) | Query gốc join sai: `e.Id=COALESCE(FinalTestId,LessonId)` gắn nhầm exercise có Id trùng LessonId (vd node 2 → "Lab: Bubble Sort" thay vì "Quiz: Binary Search"). Đúng model: `e2.NodeId=n.Id OR e2.Id=n.FinalTestId` → 13/13 node Status=2 có submission điểm tối đa, BadNodes=0 |
| Consistency Xp 5 user | HW 1120=1120 · AVG 600=600 · SLK 70=70 · NEW 0=0 · Showcase 2790=2790 | PASS (5/5) | Xp = Σ reward xp quest Claimed=1 |
| Consistency Gems 5 user | HW 29=179−150 · AVG 45=95−50 · SLK 11=11−0 · NEW 0=0−0 · Showcase 289=439−150 | PASS (5/5) | Spend lưu âm trong GemTransactions; khớp InventoryCost × PriceGems |
| API smoke: login + auth/me | POST /api/v1/auth/login OK; GET /api/v1/auth/me → id=2092, email=showcase@demo.local | PASS | Route thật không có /api/v1/me (đã thích nghi: /api/v1/auth/me) |
| API smoke: streak + premium + inventory | /api/v1/me/streak → streakDays=30; /api/v1/premium/status → 12m, active, expires 2027-07-15; /api/v1/me/inventory → 2 items | PASS | Route thật thuộc GamificationController [Route("api/v1")] |
| API smoke: leaderboard ×2 | tab=level top1 userId=2092 xp=2790; tab=week top1 userId=2092 xp=2790 | PASS | Cả 2 tab top 1 = showcase |
| API smoke: class report ×3 | Lớp 1 DSA213: 8 asg, OnTime 29/Late 6/NS 149/Lagging 10; Lớp 2 ADVNCE: 6 asg, 22/4/82/10; Lớp 4 GRPH21: 6 asg, 24/7/113/10 | PASS | Student bị 403 (đúng quyền); login teacher@demo.local/Teacher@123 để lấy report |
| API smoke: admin bug reports | Login admin@system.local/Admin@123 OK; GET /api/v1/admin/bug-reports → 10 báo cáo: New 5/Processing 2/Resolved 2/Closed 1 (đủ 4 trạng thái) | PASS | Khớp SQL Status 0-3 |

## Kết luận

- **PASS phần lớn**: seed lần 1 đúng số liệu thiết kế; SQL counts 16/17 bảng đạt ngưỡng (1 lệch đã biết: UserInventory); showcase consistency 8/9 field (1 lệch đã biết: Gems 289); BadNodes=0 (sau khi điều chỉnh join theo schema thật); Xp/Gems 5/5 user khớp; API smoke toàn bộ 9/9 endpoint hoạt động đúng.
- **FAIL thật sự — 1 lỗi**: **Idempotency CodeSubmissions** (`SeedDemoActivity.V2.Misc.cs:127-134`): guard tồn tại dùng `SubmittedAt` tính từ `now` (`now.AddDays(-DaysAgo)...`) nên mỗi lần chạy timestamp khác → 7 dòng thêm lại mỗi run (CodeSubmissions 12→19; run thứ 3 sẽ 26). Gợi ý fix: guard theo `(UserId, ExerciseId)` không kèm SubmittedAt, hoặc làm deterministic theo ngày cố định (không phụ thuộc now), không phải thêm UNIQUE constraint.
- **2 FAIL có tài liệu** (không phải bug): UserInventory 67 < 80 và showcase Gems 289 < 300 — cả hai đều khớp đúng con số thiết kế trong `quest-xp-showcase.md` (trần toán học gems≥0 + unique item; showcase mua 2 item).
- **Khuyến nghị**: Sẵn sàng dev-review cho toàn bộ nội dung seed ngoại trừ mục idempotency CodeSubmissions. Nên fix guard CodeSubmissions (1 thay đổi nhỏ) trước khi mở PR, hoặc pm quyết định chấp nhận (chỉ ảnh hưởng dữ liệu demo, không phá ngưỡng nào khác).
