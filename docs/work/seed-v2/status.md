# status.md — Seed Prod V2

## Task 1 — Users V2 (DONE 14/08 03:10, commit 222fefb)
- 69 user (13 HW / 32 AVG / 13 SLK / 10 NEW + showcase), guard Email, CreatedAt rải 30 ngày. Build 0 err · Unit 137/137 · Int 76/77 (1 pre-existing). Chưa wire.

## Task 2 — Achievements V2 (DONE 14/08 03:2x, commit 1ac367c)
- +7 achievement (tree-master..quiz-ace, SortOrder 11-17) + plan UserAchievements theo persona = 337 dự kiến (showcase 17/17, HW 127, AVG 176, SLK 12, NEW 5). Build 0 err · Unit 137/137. Chưa wire.

## Task 4b — Fix gems âm cho 16 user Average (DONE 14/08, commit task-4b)
- Vấn đề: 16 user Average (Index%8 ≥ 4) mua 2 item (150 gems) nhưng earn chỉ 93-111 → Gems -57..-39. Sửa: `V2ItemPlan` lọc theo ngân sách Σ gems earn (item rẻ trước, user không đủ → không mua) → 16 user còn 1 item (ai-bot 50), Gems 43-61 ✓; showcase mua thêm 1 item (2 item, Gems 389 → 289); +6 claim Hardworking (Index 0-5, ngày 0) giữ GemTransactions = 1360.
- Số cuối 5 entity: UserQuests 2856 · GemTransactions 1360 · UserInventory 60 · Favorites 245 · ContentFeedback 25. Min gems V2 = 0 (nhóm New) — KHÔNG còn user gems âm.
- LỆCH task: UserInventory 60 < mục tiêu 73-110 — trần khả thi với (Gems ≥ 0 + unique (UserId, ItemId) + XP persona) = 61 (AVG ≤ 1 item vì earn max 125 < 150, HW ≤ 2 vì 250 < 300, showcase ≤ 3); chi tiết docs/work/seed-v2/quest-xp-showcase.md §4.2.
- Build 0 err · Unit 137/137. Chưa wire (Task 6).
