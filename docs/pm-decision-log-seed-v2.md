# PM Decision Log — Seed Prod V2 (feature/seed-prod-v2)

## [2026-08-14] Khởi động PROMPT_K_SEED_PROD_V2 (chế độ --auto)
- Quyết định: thực hiện seed V2 theo session/PROMPT_K_SEED_PROD_V2.md, chế độ --auto (prompt đã duyệt sẵn). Worktree riêng `trees/seed-v2` (branch `feature/seed-prod-v2` từ origin/dev). DB SQL Server `DsaVisual` dùng chung — chỉ GHI THÊM, không xóa/ghi đè.
- Ảnh hưởng: toàn bộ task 1-6 + docs; working tree `D:\FPT\neww` không đụng.

## [2026-08-14] Số lượng user V2 + phân bổ persona
- Quyết định: 68 student mới (Hardworking 13 / Average 32 / Slacker 13 / New 10) + 1 showcase = 69 user mới → tổng Users = 95 (26 cũ + 69 mới ≥ ngưỡng 85). Đủ đệm so với ngưỡng, đủ độ lớn cho class members (≥60) và submissions (≥250).
- Ảnh hưởng: SeedDemoActivity.V2.Students.cs; ngưỡng SQL "Users ≥ 85".

## [2026-08-14] Verify DB thật trước khi code (sqlcmd 14/08)
- Quyết định: DB khớp audit: Users=26, Achievements=10, UserAchievements=33, ExerciseSubmissions=53, CodeSubmissions=5, Classes=3, ClassMembers=13, ClassAssignments=7, ContentFeedback=10, BugReports=3, UserNodeProgress=34, UserQuests=178, GemTransactions=141, UserInventory=7, Favorites=27, UserProgress=32, LessonNotes=3, PremiumSubscriptions=1. Danh sách 26 email hiện có đã chụp — V2 KHÔNG trùng (verify bằng SELECT trước khi code).
- Ảnh hưởng: mọi guard Email V2.

## [2026-08-14] Guard mới cho BugReports + CodeSubmissions (thay guard count==0)
- Quyết định: theo bảng delta #5 — BugReports guard (UserId, Description); CodeSubmissions guard (UserId, ExerciseId, SubmittedAt). Giữ idempotent, không đụng 3 bugreport + 5 codesubmission cũ.
- Ảnh hưởng: SeedDemoActivity.V2.Misc.cs.

## [2026-08-14] PlanSeed V2 riêng = 20260814
- Quyết định: V2 dùng hằng số PlanSeed riêng 20260814 (KHÔNG đổi PlanSeed=20260813 của V1) để kế hoạch deterministic độc lập.
- Ảnh hưởn: SeedDemoActivity.V2.Progress.cs.

## [2026-08-14] PlanId "12m" cho premium showcase (lệch pattern "1m")
- Quyết định: PremiumSubscriptions của showcase dùng PlanId "12m" (bảng thật chỉ có row "1m" — nhưng PlanId là string, không có FK cứng; OrderRef theo format hệ thống DSV{userId}T12). Ghi nhận lệch pattern.
- Ảnh hưởng: SeedDemoActivity.V2.Misc.cs + cần verify API premium active sau seed.

## [2026-08-14] Wire V2 vào SeedDemoActivity.cs CHỈ ở Task 6 (theo prompt ≤ +30 dòng)
- Quyết định: các task 1-5 viết file V2 nhưng chưa wire call → verify bằng `dotnet build` + `dotnet test` + chạy seed V1 (xác nhận V1 idempotent không vỡ). Dữ liệu V2 thật trên DB chỉ chạy sau Task 6 khi wire xong → rồi chạy seed 2 lần (lần 2 = 0 thêm) + SQL counts + API smoke. Lý do: tránh wire dần gây diff > 30 dòng và rủi ro seed nửa chừng.
- Ảnh hưởng: thứ tự verify; status.md ghi rõ trạng thái "chưa wire".

## [2026-08-14] Review tổng cuối thay vì review từng task
- Quyết định: dev-test verify build/test/seed từng task (độc lập), dev-review 1 lần duy nhất trên toàn bộ diff trước PR (tiết kiệm chi phí, diff V2 là file mới tách biệt nên rủi ro thấp). Vẫn đạt tiêu chuẩn #8.
- Ảnh hưởng: lịch dispatch.

## [2026-08-14] Achievement V2 SortOrder 11-17 + guard theo Code
- Quyết định: 7 achievement mới (tree-master, graph-expert, code-wizard, speed-demon, lab-master, social-butterfly, quiz-ace), SortOrder 11-17, ConditionJson chỉ metadata. Showcase 17/17.
- Ảnh hưởng: SeedDemoActivity.V2.Students.cs (Task 2).

## [2026-08-14] File data chung SeedDemoActivity.V2.Data.cs
- Quyết định: ngoài 5 file V2.* liệt kê trong prompt, tạo thêm 1 file data dùng chung \SeedDemoActivity.V2.Data.cs\ (danh sách email + persona + index deterministic) để Task 1-6 dùng chung một nguồn — tránh lệch danh sách giữa các file. Vẫn nằm trong phạm vi "file V2 mới".
- Ảnh hưởn: SeedDemoActivity.V2.Data.cs; docs/work/seed-v2/users-v2.md (bảng email/persona).

## [2026-08-14 03:15] Flake integration test AdminStats_AfterSeeding_ReportsExactDeltas (pre-existing)
- Quyết định: test fail ổn định (3 lần: worktree + dev sạch) NHƯNG fail trên cả branch dev sạch 715dc33 (không có code V2) → pre-existing, không do V2. Ngoài phạm vi (CẤM sửa file không liên quan). Chạy tiếp, ghi chú vào report cuối + đề xuất sửa sau.
- Ảnh hưởng: không chặn task; kết quả test cuối = 137/137 unit + 76/77 integration (1 pre-existing fail).

## [2026-08-14 03:4x] Lệch Task 3: showcase pass 13/18 node (không phải 18)
- Quyết định: verify SQL LearningPathNodes: 18 node tổng = 13 node có exercise (LessonId hoặc FinalTestId) + 5 node \"Luyện tập tổng hợp\" (Id 3,7,11,14,17) LessonId=NULL AND FinalTestId=NULL → KHÔNG thể có ExerciseSubmission full-score → không thể Status=2 theo rule hệ thống (ExerciseService.cs:745 / Progress.cs:501-511). Showcase pass 100% 13 node có exercise (18 UserNodeProgress: 13 pass + 5 active). Số "18 node Status=2" trong PROMPT_K không khả thi với DB thật — chấp nhận 13 pass.
- Ảnh hưởng: SeedDemoActivity.V2.Progress.cs (commit 811828e); tiêu chuẩn #4 vẫn đạt (mọi Status=2 có full-score); số node pass không ảnh hưởng XP (XP từ quest).

## [2026-08-14 03:4x] Lệch Task 3: số node/submission persona thấp hơn gợi ý
- Quyết định: chấp nhận plan agent (HW pass 6-8 subs 7-10; AVG pass 2-4 subs 2-5; UserProgress max 8 lesson/user vì DB chỉ có 8 lesson active) — vì tổng 3 ngưỡng (602/377/254) vẫn đạt, tỷ trọng persona giữ.
- Ảnh hưởn: chỉ số kế hoạch; không ảnh hưởng ngưỡng máy đo.

## [2026-08-14 04:0x] Task 4: gems âm → sửa (Task 4b, commit 8cf98c9) + UserInventory lệch ngưỡng
- Quyết định 1: gems âm KHÔNG chấp nhận (UI xấu) → yêu cầu sửa: mua item chỉ khi earn ≥ giá. Kết quả: 0 user gems âm (min 0, showcase 289).
- Quyết định 2: UserInventory V2 = 60 (tổng DB 67 < ngưỡng 80). Agent chứng minh trần khả thi = 61 với (Gems ≥ 0 + UNIQUE (UserId,ItemId) + XP persona + giá item thấp nhất 50): AVG earn max 125 < 150 → ≤ 1 item; HW ≤ 2; showcase ≤ 3. **CHẤP NHẬN LỆCH** — ngưỡng 80 không khả thi toán học với rule hệ thống; ghi rõ trong report + đề xuất bước sau (thêm shop item giá ≤ 30 nếu muốn đạt 80 — ngoài phạm vi CẤM sửa SeedData).
- Ảnh hưởng: tiêu chuẩn #3 (UserInventory) lệch 67/80; các ngưỡng khác vẫn đạt.

## [2026-08-14 04:4x] Task 6 phát hiện + sửa bug AsNoTracking (commit 78a0ef1 → cbc623a)
- Quyết định: V2.Activity.cs load users AsNoTracking → Xp/Gems/Streak không persist (bug thật, verify code). Sửa: bỏ AsNoTracking (user tracked như V1). Test: GIỮ đếm 17 achievements/3 classes (behavior mới hợp lệ), KHÔNG né bug bằng scope test — khôi phục 2 test balance/XP kiểm toàn bộ user V2 (đây là verify chống số ảo). Xóa comment tự thú lỗi.
- Ảnh hưởng: SeedDemoActivity.V2.Activity.cs + SeedDemoActivityTests.cs; đảm bảo consistency SQL + API smoke.

## [2026-08-14 05:0x] Lỗi idempotency CodeSubmissions (verify phát hiện) + fix
- Quyết định: V2.Misc.cs guard (UserId, ExerciseId, SubmittedAt) với SubmittedAt = now-based → mỗi lần chạy timestamp khác → +7 rows/lần. Fix: (1) SubmittedAt deterministic từ MỐC CỐ ĐỊNH theo index (không phụ thuộc now); (2) guard đổi (UserId, ExerciseId) — plan V2 mỗi user nộp 1 bài/exercise nên guard này đủ, đảm bảo idempotent kể cả với rows cũ; (3) XÓA 7 rows CodeSubmissions trùng (UserId, ExerciseId) do 2 lần chạy verify — dữ liệu rác do chính session tạo (không phải dữ liệu có sẵn), giữ bản SubmittedAt sớm nhất.
- Ảnh hưởng: SeedDemoActivity.V2.Misc.cs; DB CodeSubmissions 19 → 12.

## [2026-08-14 05:0x] Showcase Gems 289 < 300 (verify) → sửa mua 1 item
- Quyết định: showcase mua 2 item (spend 150) → Gems 289 ngoài khoảng 300-420. Sửa: mua 1 item (spend 50) → Gems 389 ∈ 300-420 ✓. Inventory tổng 67 → 66 (vẫn lệch ngưỡng 80 đã chấp nhận).
- Ảnh hưởng: V2.Activity.cs (V2ItemPlan showcase); consistency Gems.
