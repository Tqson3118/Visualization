# quest-xp-showcase.md — Bảng tính XP/Gems Showcase (Task 4 + 4b, Seed V2)

> Nguồn: `backend/src/DsaVisual.Application/Persistence/Seed/SeedDemoActivity.V2.Activity.cs`
> (Task 4 — `SeedUserQuestsV2Async`; Task 4b — `V2ItemPlan` lọc ngân sách gems). Mô phỏng lại bằng script độc lập, khớp 100% logic C#.

## 1. Quy tắc deterministic (không random)

- **Rotation quest** (thứ tự `SeedData.Quests`, 8 quest): `learn-1-node, learn-3-node, pass-1-quiz, pass-1-lab, code-run-1, code-run-5, lesson-viewed-2, streak-3` — XP 20/30/20/25/25/50/20/60, gems 3/5/3/4/4/8/3/10.
- **Ngày `d`** (0..29, `d=0` = hôm nay UTC+7) của showcase (Index 68): 5 quest rows bắt đầu `offset = (68 + d) % 8` → `key = Rotation[(offset + j) % 8]`, `j = 0..4`.
- **Claim showcase**:
  1. Claim mọi quest có XP ≤ 25 (quest "thường").
  2. Bù đủ tối thiểu 3 claims — quest XP thấp nhất chưa claim.
  3. Ngày `d % 3 == 0` ("ngày đỉnh cao"): claim thêm quest XP ≥ 50 (ưu tiên cao nhất) nếu có trong window.
  4. Ngày `d % 5 == 0`: bù đủ 4 claims — quest XP thấp nhất chưa claim.
- Mua 2 shop item (Task 4b): `avatar-ai-bot` (50 gems, DaysAgo 1) + `avatar-cyber-hacker` (100 gems, DaysAgo 5). Favorites 5, Feedback 2 (Bubble Sort 5★ / AVL 5★), Streak 30.

## 2. Bảng tính chi tiết 30 ngày

| d | Quest rows (5) | Claimed | XP/ngày | Gems/ngày |
|---|---|---|---|---|
| 0 | code-run-1, code-run-5, lesson-viewed-2, streak-3, learn-1-node | code-run-1, lesson-viewed-2, streak-3, learn-1-node | 125 | 20 |
| 1 | code-run-5, lesson-viewed-2, streak-3, learn-1-node, learn-3-node | lesson-viewed-2, learn-1-node, learn-3-node | 70 | 11 |
| 2 | lesson-viewed-2, streak-3, learn-1-node, learn-3-node, pass-1-quiz | lesson-viewed-2, learn-1-node, pass-1-quiz | 60 | 9 |
| 3 | streak-3, learn-1-node, learn-3-node, pass-1-quiz, pass-1-lab | streak-3, learn-1-node, pass-1-quiz, pass-1-lab | 125 | 20 |
| 4 | learn-1-node, learn-3-node, pass-1-quiz, pass-1-lab, code-run-1 | learn-1-node, pass-1-quiz, pass-1-lab, code-run-1 | 90 | 14 |
| 5 | learn-3-node, pass-1-quiz, pass-1-lab, code-run-1, code-run-5 | learn-3-node, pass-1-quiz, pass-1-lab, code-run-1 | 100 | 16 |
| 6 | pass-1-quiz, pass-1-lab, code-run-1, code-run-5, lesson-viewed-2 | pass-1-quiz, pass-1-lab, code-run-1, code-run-5, lesson-viewed-2 | 140 | 22 |
| 7 | pass-1-lab, code-run-1, code-run-5, lesson-viewed-2, streak-3 | pass-1-lab, code-run-1, lesson-viewed-2 | 70 | 11 |
| 8 | code-run-1, code-run-5, lesson-viewed-2, streak-3, learn-1-node | code-run-1, lesson-viewed-2, learn-1-node | 65 | 10 |
| 9 | code-run-5, lesson-viewed-2, streak-3, learn-1-node, learn-3-node | lesson-viewed-2, streak-3, learn-1-node, learn-3-node | 130 | 21 |
| 10 | lesson-viewed-2, streak-3, learn-1-node, learn-3-node, pass-1-quiz | lesson-viewed-2, learn-1-node, learn-3-node, pass-1-quiz | 90 | 14 |
| 11 | streak-3, learn-1-node, learn-3-node, pass-1-quiz, pass-1-lab | learn-1-node, pass-1-quiz, pass-1-lab | 65 | 10 |
| 12 | learn-1-node, learn-3-node, pass-1-quiz, pass-1-lab, code-run-1 | learn-1-node, pass-1-quiz, pass-1-lab, code-run-1 | 90 | 14 |
| 13 | learn-3-node, pass-1-quiz, pass-1-lab, code-run-1, code-run-5 | pass-1-quiz, pass-1-lab, code-run-1 | 70 | 11 |
| 14 | pass-1-quiz, pass-1-lab, code-run-1, code-run-5, lesson-viewed-2 | pass-1-quiz, pass-1-lab, code-run-1, lesson-viewed-2 | 90 | 14 |
| 15 | pass-1-lab, code-run-1, code-run-5, lesson-viewed-2, streak-3 | pass-1-lab, code-run-1, lesson-viewed-2, streak-3 | 130 | 21 |
| 16 | code-run-1, code-run-5, lesson-viewed-2, streak-3, learn-1-node | code-run-1, lesson-viewed-2, learn-1-node | 65 | 10 |
| 17 | code-run-5, lesson-viewed-2, streak-3, learn-1-node, learn-3-node | lesson-viewed-2, learn-1-node, learn-3-node | 70 | 11 |
| 18 | lesson-viewed-2, streak-3, learn-1-node, learn-3-node, pass-1-quiz | lesson-viewed-2, streak-3, learn-1-node, pass-1-quiz | 120 | 19 |
| 19 | streak-3, learn-1-node, learn-3-node, pass-1-quiz, pass-1-lab | learn-1-node, pass-1-quiz, pass-1-lab | 65 | 10 |
| 20 | learn-1-node, learn-3-node, pass-1-quiz, pass-1-lab, code-run-1 | learn-1-node, pass-1-quiz, pass-1-lab, code-run-1 | 90 | 14 |
| 21 | learn-3-node, pass-1-quiz, pass-1-lab, code-run-1, code-run-5 | pass-1-quiz, pass-1-lab, code-run-1, code-run-5 | 120 | 19 |
| 22 | pass-1-quiz, pass-1-lab, code-run-1, code-run-5, lesson-viewed-2 | pass-1-quiz, pass-1-lab, code-run-1, lesson-viewed-2 | 90 | 14 |
| 23 | pass-1-lab, code-run-1, code-run-5, lesson-viewed-2, streak-3 | pass-1-lab, code-run-1, lesson-viewed-2 | 70 | 11 |
| 24 | code-run-1, code-run-5, lesson-viewed-2, streak-3, learn-1-node | code-run-1, lesson-viewed-2, streak-3, learn-1-node | 125 | 20 |
| 25 | code-run-5, lesson-viewed-2, streak-3, learn-1-node, learn-3-node | code-run-5, lesson-viewed-2, learn-1-node, learn-3-node | 120 | 19 |
| 26 | lesson-viewed-2, streak-3, learn-1-node, learn-3-node, pass-1-quiz | lesson-viewed-2, learn-1-node, pass-1-quiz | 60 | 9 |
| 27 | streak-3, learn-1-node, learn-3-node, pass-1-quiz, pass-1-lab | streak-3, learn-1-node, pass-1-quiz, pass-1-lab | 125 | 20 |
| 28 | learn-1-node, learn-3-node, pass-1-quiz, pass-1-lab, code-run-1 | learn-1-node, pass-1-quiz, pass-1-lab, code-run-1 | 90 | 14 |
| 29 | learn-3-node, pass-1-quiz, pass-1-lab, code-run-1, code-run-5 | pass-1-quiz, pass-1-lab, code-run-1 | 70 | 11 |

**Tổng showcase:**
- UserQuests rows = 30 × 5 = **150**
- Claims = **109** (18 ngày × 4 + 1 ngày × 5 + 11 ngày × 3) — trong 104-116 ✓
- Σ XP = **2790** — trong 2600-2900 ✓ (Level = 1 + floor(sqrt(27.9)) = 6)
- Σ Gems earn = **439**; spend 2 item (`avatar-ai-bot` 50 + `avatar-cyber-hacker` 100) = **150** → **Gems = 289** (Task 4b: bù 1 item cho showcase khi 16 user Average bị cắt item thứ 2 — xem §4.1)
- StreakDays = 30, LastActivityDate = hôm nay UTC+7, Favorites 5, Feedback 2.

## 3. Tổng dự kiến 5 entity theo persona (Task 4 + Task 4b)

| Persona | SL | UserQuests | Claims (quest-earn) | Gems earn | Shop spend (items) | Favorites | Feedback |
|---|---|---|---|---|---|---|---|
| Showcase | 1 | 150 | 109 | 439 | 150 (2) | 5 | 2 |
| Hardworking | 13 | 1360 | 521 | 2489 | 1950 (26) | 116 | 13 |
| Average | 32 | 1296 | 624 | 2768 | 1600 (32) | 112 | 10 |
| Slacker | 13 | 50 | 46 | 263 | 0 (0) | 12 | 0 |
| New | 10 | 0 | 0 | 0 | 0 (0) | 0 | 0 |
| **Tổng** | 69 | **2856** | **1300** | **5959** | **3700 (60)** | **245** | **25** |

- GemTransactions = quest-earn 1300 + shop-spend 60 = **1360** (mục tiêu 1360-1600 ✓ — Hardworking Index 0-5 claim thêm quest j2 ngày 0 để bù 6 earn-tx, XP sau 1095-1405 ≤ 1500 ✓)
- UserQuests **2856** (2820-3100 ✓) · UserInventory **60** (mục tiêu 73-110 — KHÔNG đạt, xem §4.2) · Favorites **245** (223-300 ✓) · ContentFeedback **25** (10-30 ✓)

## 4. Quyết định / ghi chú

### 4.1. Task 4b — gems ≥ 0 cho mọi user V2 (thay quyết định cũ "chấp nhận gems âm")
- **Lỗi cũ (Task 4)**: 16 user Average (Index%8 ≥ 4) mua 2 item (150 gems) trong khi earn chỉ 93-111 → Gems = earn − spend = **-57..-39** (báo cáo ước -88..-11, thực tế mô phỏng chính xác -57..-39). Nguyên nhân: tỷ lệ reward gems/XP cao nhất của 8 quest là 10 gems / 60 XP (≈ 1/6) → với XP ≤ 750 (khung Average) không thể kiếm đủ 150 gems.
- **Sửa (Task 4b)**: `V2ItemPlan` lọc theo ngân sách — chỉ giữ item khi Σ gems earn (từ quest claim plan, `V2GemsEarned`) ≥ tổng chi đến item đó; item rẻ đứng trước. Kết quả: 16 user Average còn **1 item** (`avatar-ai-bot` 50) → Gems **43-61** ✓; toàn bộ user V2 có Gems ≥ 0 (min = 0 ở nhóm New). Quest claims giữ nguyên cho 16 user này (XP không đổi).
- **Bù inventory + GemTransactions**: showcase mua thêm `avatar-cyber-hacker` (2 item, Gems 389 → 289) + 6 user Hardworking (Index 0-5) claim thêm quest j2 ngày 0 (XP 1095-1405, vẫn ≤ 1500) → GemTransactions = 1360.

### 4.2. UserInventory 60 < mục tiêu 73-110 — trần khả thi (bất khả thi toán học)
Giữ đồng thời 3 ràng buộc: (a) Gems ≥ 0 (Task 4b), (b) UNIQUE (UserId, ItemId) — mỗi item chỉ mua 1 lần/user, (c) XP persona (AVG ≤ 750, HW ≤ 1500, showcase ≤ 2900) → trần item khả mua:
- **Average**: earn tối đa = 750 × 10/60 = **125 gems** < 150 (item thứ 2 rẻ nhất) → **≤ 1 item/user** (32).
- **Hardworking**: earn tối đa = 1500 × 10/60 = **250 gems** < 300 (item thứ 3 rẻ nhất) → **≤ 2 item/user** (26).
- **Showcase**: earn 439 → **≤ 3 item** (50+100+150); chọn 2 item để giữ Gems 289.
- Slacker/New: earn ≤ 30 < 50 → 0 item.
- **Trần tổng = 32 + 26 + 3 = 61** (đạt 60 với lựa chọn showcase 2 item). Mục tiêu 73-110 của PROMPT_K không khả thi với rule gems ≥ 0 — **đã báo cáo lệch task**; nếu muốn đạt 73 cần 1 trong: item rẻ hơn (thêm shop item ~30 gems), nới XP persona, hoặc bỏ unique (UserId, ItemId) — đều ngoài phạm vi Task 4b.

### 4.3. Các quyết định giữ nguyên từ Task 4
1. **Slacker dùng quest cố định** `[code-run-5, lesson-viewed-2]` mỗi ngày (thay vì rotation) — XP/user 70-190 luôn trong khung 40-200 dù 1-3 ngày, không phụ thuộc offset.
2. **Recompute Xp/Gems/StreakDays** chỉ khi `addedClaimed > 0` (pattern V1 Activity.cs:279-297), flush `SaveChanges` trước khi tính tổng từ DB — chạy lại lần 2 → 0 dòng thêm, không đụng giá trị đã seed.
