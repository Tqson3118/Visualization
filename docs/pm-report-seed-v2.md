# PM Report — Seed Prod V2 (PROMPT_K_SEED_PROD_V2)

**Trạng thái: HOÀN THÀNH (10/10 task DONE)** · Branch `feature/seed-prod-v2` · PR: https://github.com/Tqson3118/Visualization/pull/21 (base `dev`)

## File tạo/sửa (12 file, +2658/-7 so origin/dev)
6 file V2 mới (Data/Students/Progress/Activity/Class/Misc) + SeedDemoActivity.cs (+21 dòng wire 15 bước) + SeedDemoActivityTests.cs (đã duyệt) + README.md + docs/work/seed-v2/* (evidence) + docs/pm-decision-log-seed-v2.md. V1 diff = **0**.

## Kết quả verify (đo bằng máy — DB thật SQL Server docker 14/08)
- Build: **0 error** · Test: **Unit 137/137 PASS**, Integration 76/77 (1 fail pre-existing `AdminStats_AfterSeeding` — fail cả trên dev sạch, không do V2)
- Seed chạy 4 lần: lần 2+ = **0 thêm** (idempotent; fix 6c guard CodeSubmissions mốc cố định)
- SQL counts: 16/17 đạt ngưỡng — Users **95** · Achievements **17** · UserAchievements **370** · ExerciseSubmissions **371** · CodeSubmissions **12** · Classes **4** · ClassMembers **66** · ClassAssignments **20** · ContentFeedback **35** · BugReports **10** · UserNodeProgress **636** · UserQuests **3136** · GemTransactions **1500** · UserInventory **66 ⚠** · Favorites **272** · UserProgress **409** · LessonNotes **3**
- Consistency: showcase Xp **2790** (Level 6, top 1 leaderboard) · Gems **389** · Streak 30 · LastActivity 2026-08-14 · Premium active (12m, DSV2092T12, HeartsMax 30) · BadNodes **0** (13/13 node Status=2 có full-score) · Xp/Gems 5 user mẫu khớp Σ quest claim (5/5)
- API smoke **9/9**: login, /me, leaderboard level+week, class report 3 lớp (3 nhóm + Lagging), admin bug-reports 4 trạng thái

## Lệch docs (đều ghi decision log)
1. Showcase pass 13/13 node CÓ exercise (5 node "Luyện tập tổng hợp" không có exercise — DB thật) thay vì 18
2. UserInventory 66 < 80 — trần toán học (Gems ≥ 0 + UNIQUE + XP persona + giá item ≥ 50)
3. Guard CodeSubmissions (UserId, ExerciseId) mốc cố định (idempotent thật) — lệch nhỏ so với prompt
4. Test cập nhật 17/3 (behavior mới) + sửa bug AsNoTracking (Xp/Gems persist)

## Decision log
`docs/pm-decision-log-seed-v2.md` — **18 mục** (số lượng user 13/32/13/10, guard mới, PlanSeedV2, PlanId 12m, lệch 18→13 node, gems âm→fix, AsNoTracking fix, CodeSubmissions idempotent, UserInventory, dọn 7 rows rác...)

## Đề xuất bước sau (KHÔNG làm)
- Fix pre-existing test `AdminStats_AfterSeeding_ReportsExactDeltas` (time-dependent UTC vs UTC+7 — đề xuất dùng `_clock.UtcNow.AddHours(7)`)
- Nếu muốn UserInventory ≥ 80: thêm shop item giá ≤ 30 gems (cần sửa SeedData — ngoài phạm vi)
- Gộp PR sau khi merge dev; chạy lại seed khi chuyển môi trường mới

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu 'làm lại <task/mục>' kèm ghi chú, PM chạy lại phần đó.
