# Seed — Dữ liệu khởi tạo (SDD §7.5)

Seeder **THẬT** chạy qua `AppDbContext` (EF Core 10) — file `SeedRunner.cs` (idempotent: kiểm tra tồn tại trước khi chèn — SDD §7.5/§10.5).
Dữ liệu khai báo tại `SeedData.cs` (5 Topics, 8 Lessons, 3 Users, 8 Quests, 8 ShopItems, 8 Settings) + `SeedData.Students` (8 student demo `@university.edu.vn`).

SAU `SeedSettingsAsync`, `SeedRunner` gọi **`SeedDemoActivity.SeedAsync`** (file `SeedDemoActivity.cs` skeleton + partial `Students/Progress/Activity/Misc/Class`) — **seed dữ liệu hoạt động người dùng demo (SDD §7.5)**: 8 student `@university.edu.vn`, achievements, progress, submissions, quest/gems/inventory/favorites/feedback, 2 lớp học, code submissions/bug reports/lesson notes (chỉ khi bảng trống). Từ 2026-08-14 có thêm đợt **Seed V2 (PROD-K)** — 6 file partial mới `SeedDemoActivity.V2.*` — xem mục "Seed V2 (PROD-K)" dưới.

## Lệnh chạy seed

> Yêu cầu: SQL Server đang chạy (docker-compose) + DB đã migrate (task 1 — `InitialCreate`).

```powershell
# Từ backend/ — bắt buộc có DSA__Jwt__Secret (Program.cs ném lỗi nếu thiếu) + connection string
$env:DSA__Jwt__Secret = "dev-secret-32-ky-tu-toi-thieu-0123456789abcdef"
$env:ConnectionStrings__Default = "Server=localhost;Database=DsaVisual;User Id=sa;Password=DsaVisual@Dev123;TrustServerCertificate=True"

dotnet run --project src/DsaVisual.Api -- --seed
```

Cơ chế (Program.cs): `--seed` → `Migrate()` → `SeedRunner.SeedAsync(db)` → thoát. **KHÔNG seed khi chạy bình thường.**

Seed idempotent — chạy lại lần 2 an toàn, mọi bước ghi log `Seed: ... thêm/Seed: ... bỏ qua (đã tồn tại)`.

## Mật khẩu seed DEV (⚠️ CHỈ DÙNG LOCAL — không phải production)

| Email | Vai trò | Mật khẩu dev | Ghi chú |
|---|---|---|---|
| `admin@system.local` | ADMIN (IsPrimaryAdmin) | `Admin@123` | Admin chính — quản lý Admin khác (FR-1.9) |
| `teacher@demo.local` | TEACHER | `Teacher@123` | |
| `student@demo.local` | STUDENT | `Student@123` | |

- **8 student demo `*@university.edu.vn`** (SeedData.Students — tạo bởi SeedDemoActivity): mật khẩu dev = **`Student@123`** (⚠️ CHỈ local dev, không dùng production — đổi trước khi deploy, xem dòng dưới).
- Hash: PBKDF2-SHA256 100.000 vòng + salt 16 byte (`DsaVisual.Application/Common/PasswordHasher.cs` — hash thật, verify được).
- **Đổi mật khẩu**: login → `POST /auth/change-password` hoặc Admin reset qua `POST /admin/users/{id}/reset-password` (UsersController). **Bắt buộc đổi trước khi deploy production.**
- Ghi chú: SDD §7.5 có "ép đổi mật khẩu lần đầu", nhưng bảng `Users` hiện **chưa có cột** `MustChangePassword` (entity chỉ có `IsPrimaryAdmin`) → seeder không set cờ này; cần migration bổ sung nếu muốn ép đổi (đề xuất — xem report).

## Phạm vi dữ liệu đã seed

| Bảng | Dữ liệu | Nguồn |
|---|---|---|
| Users | 3 (admin/teacher/student) | `SeedData.cs` |
| Topics | 5: "Sắp xếp & Tìm kiếm" (1), "CTDL tuyến tính" (2), "Cây" (3), "Bảng băm" (4), "Đồ thị" (5) | `SeedData.cs` |
| Lessons | **8 bài Active**: Bubble Sort, Binary Search, Stack, Linked List, BST, AVL, Hash Table, BFS | content-drafts → `content.md` → ContentHtml (sanitize Ganss.Xss) |
| LessonSimulations | mỗi bài ≥ 1 key hợp lệ `shared/simulation-catalog.json` — UNIQUE (LessonId, SimulationKey) | `SeedData.cs` |
| Exercises | 8× MCQ + 8× SIMULATION_LAB + 8× CODE + 5 final-test (lộ trình) = **29** | quiz.json + cấu hình lab/code |
| Questions | MCQ: Bubble 5, Binary 8, Stack 8, Linked List 8, BST 8, AVL 7, Hash 8, BFS 8 (=60) + final test 4/4/4/2/2 (=16) → **76 câu** | `quiz.json` (map: questionText→Content, options→OptionsJson, correctIndex→AnswerJson `[i]`, explanation→Explanation; SINGLE) |
| LearningPaths/Node | **5 path** × (node bài học + "Luyện tập tổng hợp" + "Kiểm tra cuối lộ trình") = **18 node** | SDD §7.3.25 |
| DailyQuests | 8 quest templates (ConditionJson/RewardJson theo SDD §7.3.26) | `SeedData.cs` |
| ShopItems | 8 item (ItemKey UNIQUE) | `SeedData.cs` ← frontend shop_items.json |
| Settings | 8 setting (Key UNIQUE) | `SeedData.cs` |
| ExerciseSubmissions | 9 user activity (8 student `@university.edu.vn` + `student@demo.local`) — bài nộp MCQ/LAB/CODE | `SeedDemoActivity.Progress.cs` |

### SeedDemoActivity — dữ liệu hoạt động người dùng demo (gọi SAU SeedSettingsAsync)

| Bảng | Dữ liệu | Nguồn |
|---|---|---|
| Settings | xóa `allowed.email.domains` nếu còn (fix domain đăng ký) | `SeedDemoActivity.Students.cs` |
| Users | +8 student `*@university.edu.vn` (PasswordHasher PBKDF2 thật, mật khẩu `Student@123`) | `SeedData.Students` |
| Achievements | 10 huy hiệu | `SeedDemoActivity.Students.cs` |
| UserAchievements | trao huy hiệu theo map từng student | `SeedDemoActivity.Students.cs` |
| UserProgress / UserNodeProgress | tiến độ bài học + node lộ trình (CompletedAt/PassedAt khớp SubmittedAt) | `SeedDemoActivity.Progress.cs` |
| ExerciseSubmissions | bài nộp MCQ/LAB/CODE cho 9 user | `SeedDemoActivity.Progress.cs` |
| UserQuests | quest theo ngày (1-13 ngày hoạt động, "app phát hành 1 tháng") | `SeedDemoActivity.Activity.cs` |
| GemTransactions | giao dịch gem (thưởng quest, mua item) | `SeedDemoActivity.Activity.cs` |
| UserInventory | item trong kho | `SeedDemoActivity.Activity.cs` |
| Favorites | bài học yêu thích | `SeedDemoActivity.Activity.cs` |
| ContentFeedback | phản hồi nội dung | `SeedDemoActivity.Activity.cs` |
| CodeSubmissions / BugReports | V1: **CHỈ khi bảng trống** (có runtime → bỏ qua); **V2: guard theo dòng** — xem mục "Seed V2 (PROD-K)" | `SeedDemoActivity.Misc.cs` + `SeedDemoActivity.V2.Misc.cs` |
| LessonNotes | ghi chú bài học | `SeedDemoActivity.Misc.cs` |
| Classes / ClassMembers / ClassAssignments | 2 lớp học demo | `SeedDemoActivity.Class.cs` |

### Bảng KHÔNG seed (dữ liệu runtime — SDD §7.5)

- **NodeSessions, CodeRuns**: tạo runtime khi user vào node / nộp code chạy sandbox (`GamificationService.StartNodeSession`, `CodeRunnerService` — ADR-012) — seed session/benchmark giả không có ý nghĩa.
- **RefreshTokens, OtpCodes, PasswordResetTokens**: auth runtime (`AuthService` tạo khi login/refresh/OTP/reset) — token/hash thật, seed giả vô nghĩa và rủi ro bảo mật.
- **Ngoài ra**: trạng thái per-user phát sinh runtime khác (quest/streak tiếp diễn sau ngày seed, hearts, ...) không seed — SeedDemoActivity chỉ seed "câu chuyện" hoạt động 1 tháng đầu cho 9 user demo.

Mô phỏng gắn bài: Bubble→`sort.bubble`; Binary→`search.binary`; Stack→`stack.push/pop/peek`; Linked List→`list.insert/traverse`; BST→`tree.bst-insert/inorder/search`; AVL→`tree.avl-insert`; Hash→`hash.insert/search`; BFS→`graph.bfs`.

Cấu hình exercise:
- **LAB** (`ConfigJson`): `{"simulationKey":"...","maxSteps":n}` — chấm trạng thái cuối, steps ≤ n×1.5 (SDD §4.16, quyết định G-5). MaxScore = 10.
- **CODE** (`ConfigJson`): `{"signature":"...","language":"javascript","testCases":[...]}` — **11 test ẩn/bài** (SORT/SEARCH: `{name,input,expected}`; CTDL: `{name,operations|inserts|searches,expected|inorder|height}` — xem chi tiết trong `SeedRunner.cs`). MaxScore = 100, pass ≥ 70% (ADR-012 — sandbox client chấm).
- MCQ: MaxScore = Σ Points (1 điểm/câu).

## Bảng 40 bài nguồn (content-drafts v2 — đối chiếu title từ content.md)

| # | Bài | # | Bài |
|---|---|---|---|
| lesson-01 | Độ phức tạp thuật toán (Big O) | lesson-21 | Topological Sort (Sắp xếp tô-pô) |
| lesson-02 | Mảng & kỹ thuật cơ bản | lesson-22 | Backtracking (Quay lui) |
| lesson-03 | Chuỗi cơ bản | lesson-23 | Chia để Trị (Divide & Conquer) |
| lesson-04 | Hash Table & Set | lesson-24 | Thuật toán Tham lam (Greedy) |
| lesson-05 | Linked List | lesson-25 | Bài toán Khoảng thời gian (Interval Problems) |
| lesson-06 | Stack | lesson-26 | Ma trận & Các khuôn mẫu xử lý lưới (Matrix / Grid) |
| lesson-07 | Queue & Deque: Hàng đợi và Hàng đợi hai đầu | lesson-27 | Thao tác Bit & Số học (Bit Manipulation) |
| lesson-08 | Đệ quy (Recursion) | lesson-28 | Sắp xếp nâng cao (Merge, Quick, Heap) |
| lesson-09 | Sắp xếp cơ bản (Bubble, Selection, Insertion) | lesson-29 | Quy hoạch động cơ bản (1D & State Machine) |
| lesson-10 | Tìm kiếm: Linear & Binary | lesson-30 | Quy hoạch động nâng cao (2D) |
| lesson-11 | Two Pointers | lesson-31 | Đường đi ngắn nhất (Shortest Path) |
| lesson-12 | Sliding Window | lesson-32 | Cây khung nhỏ nhất (MST) |
| lesson-13 | Binary Search nâng cao | lesson-33 | Union-Find / Disjoint Set Union (DSU) |
| lesson-14 | Prefix Sum & Difference Array | lesson-34 | Trie (Prefix Tree) |
| lesson-15 | Kadane & Maximum Subarray | lesson-35 | Segment Tree (Cây đoạn) |
| lesson-16 | Monotonic Stack & Deque | lesson-36 | Fenwick Tree (Binary Indexed Tree — BIT) |
| lesson-17 | Cây Nhị Phân Tìm Kiếm (BST) | lesson-37 | Thuật toán chuỗi nâng cao (KMP / Rabin-Karp) |
| lesson-18 | Cây & Duyệt cây (DFS / BFS) | lesson-38 | Advanced Data Structures (LFU / Bloom Filter / Skip List) |
| lesson-19 | Heap & Hàng đợi ưu tiên (Priority Queue) | lesson-39 | Tổng ôn & chiến lược phỏng vấn |
| lesson-20 | Đồ thị (Graph): biểu diễn & duyệt BFS/DFS | lesson-40 | DP Patterns (Interval, Bitmask, Tree DP) |

### Đối chiếu 8 bài seed ↔ nguồn

| Lesson seed | Topic | Nguồn content.md | Nguồn quiz.json | Ghi chú |
|---|---|---|---|---|
| Bubble Sort | 1 | lesson-09 (mục Bubble) | lesson-09 (chọn 5 câu) | lọc câu đúng chủ đề Bubble/so sánh 3 thuật toán cơ bản |
| Binary Search | 1 | lesson-10 | lesson-10 (8 câu) | |
| Stack | 2 | lesson-06 | lesson-06 (8 câu) | |
| Linked List | 2 | lesson-05 | lesson-05 (8 câu) | |
| BST | 3 | lesson-17 | lesson-17 (8 câu) | |
| AVL | 3 | **tự soạn** (không có bài AVL riêng trong 40 bài; lesson-17 chỉ nhắc AVL) | **tự soạn 7 câu** | content ≥ 3 mục: Ý tưởng / Minh họa (LL-RR-LR-RL) / Độ phức tạp (SDD §4.7) |
| Hash Table | 4 | lesson-04 | lesson-04 (8 câu) | |
| BFS | 5 | lesson-20 (mục BFS) | lesson-20 (8 câu) | |

> `content.md` mã hóa UTF-8 (không BOM) — PowerShell 5.1 `Get-Content` mặc định đọc sai; seeder đọc bằng `File.ReadAllText(path, new UTF8Encoding(false))`.
> Markdown → HTML bằng helper nội bộ `MarkdownToHtml` (h2/h3, strong, em, ul/li, pre/code, p; bảng → đoạn văn) rồi **sanitize Ganss.Xss** (cùng cấu hình DI Program.cs) — không thêm thư viện.

## Quy tắc idempotent (SDD §10.5)

- Users: theo `Email` (UNIQUE, lowercase). Topics: theo `Name` (gốc). Lessons: theo `(TopicId, Title)`.
- LessonSimulations: theo `(LessonId, SimulationKey)`; Exercises: theo `(LessonId, Title)` + `DeletedAt = null`.
- LearningPaths: theo `Title`; Nodes: theo `(PathId, Title)`; DailyQuests: `QuestKey`; ShopItems: `ItemKey`; Settings: `Key`.
- **SeedDemoActivity (V1)**: cùng pattern guard → Add → SaveChanges → log; Users mới theo `Email`; các bảng KHÔNG unique (CodeSubmissions, BugReports, LessonNotes, ...) **chỉ seed khi bảng đang RỖNG** — đã có dữ liệu runtime → log bỏ qua, return.
- **V2 (2026-08-14)**: đổi guard `CodeSubmissions`/`BugReports` từ "bảng rỗng" → **guard theo dòng**: `CodeSubmissions` theo `(UserId, ExerciseId)`, `BugReports` theo `(UserId, Description)` — vẫn idempotent khi DB đã có dữ liệu (chạy lại lần 2 → 0 thêm, không đụng rows cũ).

### Fix domain đăng ký — `allowed.email.domains` (quyết định user 13/08/2026)

- Setting **`allowed.email.domains` KHÔNG còn được seed** (đã xóa khỏi `SeedData.Settings`).
- Nếu DB cũ còn setting này, bước **`SeedCleanupSettingsAsync`** (bước đầu của SeedDemoActivity) **tự xóa** nó khỏi bảng `Settings` — kết quả: **mọi email đều đăng ký được** (bỏ chặn domain).

## Verify sau seed (golden data — SDD §7.5)

```sql
SELECT 'Users' t, COUNT(*) n FROM Users UNION ALL SELECT 'Topics', COUNT(*) FROM Topics
UNION ALL SELECT 'Lessons', COUNT(*) FROM Lessons UNION ALL SELECT 'LessonSimulations', COUNT(*) FROM LessonSimulations
UNION ALL SELECT 'Exercises', COUNT(*) FROM Exercises UNION ALL SELECT 'Questions', COUNT(*) FROM Questions
UNION ALL SELECT 'LearningPaths', COUNT(*) FROM LearningPaths UNION ALL SELECT 'LearningPathNodes', COUNT(*) FROM LearningPathNodes
UNION ALL SELECT 'DailyQuests', COUNT(*) FROM DailyQuests UNION ALL SELECT 'ShopItems', COUNT(*) FROM ShopItems
UNION ALL SELECT 'Settings', COUNT(*) FROM Settings UNION ALL SELECT 'Achievements', COUNT(*) FROM Achievements
UNION ALL SELECT 'UserAchievements', COUNT(*) FROM UserAchievements UNION ALL SELECT 'UserProgress', COUNT(*) FROM UserProgress
UNION ALL SELECT 'UserNodeProgress', COUNT(*) FROM UserNodeProgress UNION ALL SELECT 'ExerciseSubmissions', COUNT(*) FROM ExerciseSubmissions
UNION ALL SELECT 'UserQuests', COUNT(*) FROM UserQuests UNION ALL SELECT 'GemTransactions', COUNT(*) FROM GemTransactions
UNION ALL SELECT 'UserInventory', COUNT(*) FROM UserInventory UNION ALL SELECT 'Favorites', COUNT(*) FROM Favorites
UNION ALL SELECT 'ContentFeedback', COUNT(*) FROM ContentFeedback UNION ALL SELECT 'CodeSubmissions', COUNT(*) FROM CodeSubmissions
UNION ALL SELECT 'BugReports', COUNT(*) FROM BugReports UNION ALL SELECT 'LessonNotes', COUNT(*) FROM LessonNotes
UNION ALL SELECT 'Classes', COUNT(*) FROM Classes UNION ALL SELECT 'ClassMembers', COUNT(*) FROM ClassMembers
UNION ALL SELECT 'ClassAssignments', COUNT(*) FROM ClassAssignments;
```

Ngưỡng (bản chạy thật 2026-08-12): Users=3, Topics=5, Lessons=8, LessonSimulations=14, Exercises=29, Questions=76, LearningPaths=5, LearningPathNodes=18, DailyQuests=8, ShopItems=8, Settings=9.

### Ngưỡng sau khi seed hoạt động demo (SeedDemoActivity — 2026-08-13)

| Bảng | Ngưỡng | Bảng | Ngưỡng |
|---|---|---|---|
| Users | ≥ 9 | UserQuests | ≥ 20 |
| Achievements | = 10 | GemTransactions | ≥ 30 |
| UserAchievements | ≥ 10 | UserInventory | ≥ 7 |
| UserProgress | ≥ 15 | Favorites | ≥ 10 |
| UserNodeProgress | ≥ 20 | ContentFeedback | ≥ 5 |
| ExerciseSubmissions | ≥ 30 | Classes | = 2 |
| CodeSubmissions | ≥ 3 | ClassMembers | ≥ 10 |
| BugReports | ≥ 2 | ClassAssignments | ≥ 6 |
| LessonNotes | ≥ 2 | | |

### Ngưỡng sau khi seed hoạt động demo — Seed V2 (PROD-K, 2026-08-14)

Bản chạy thật trên SQL Server docker 14/08/2026 (nguồn: `docs/work/seed-v2/sql-counts-final.txt` + `verify-final.md` — chạy seed 2 lần, lần 2 = 0 thêm).

| Bảng | Ngưỡng | Thực tế | Bảng | Ngưỡng | Thực tế |
|---|---|---|---|---|---|
| Users | ≥ 85 | 95 | UserQuests | ≥ 3000 | 3136 |
| Achievements | = 17 | 17 | GemTransactions | ≥ 1500 | 1500 |
| UserAchievements | ≥ 300 | 370 | UserInventory | ≥ 80 | **66 ⚠️** |
| UserProgress | ≥ 400 | 409 | Favorites | ≥ 250 | 272 |
| UserNodeProgress | ≥ 600 | 636 | ContentFeedback | ≥ 20 | 35 |
| ExerciseSubmissions | ≥ 250 | 371 | Classes | = 4 | 4 |
| CodeSubmissions | ≥ 10 | 12 | ClassMembers | ≥ 60 | 66 |
| BugReports | = 10 | 10 | ClassAssignments | ≥ 20 | 20 |
| LessonNotes | = 3 | 3 | | | |

⚠️ **UserInventory 66 < 80 — lệch ĐÃ DUYỆT, không phải bug seed**: trần toán học do rule hệ thống `Gems ≥ 0` + UNIQUE `(UserId, ItemId)` + XP persona (giá item thấp nhất 50): AVG earn max 125 < 150 → ≤ 1 item; HW ≤ 2; showcase ≤ 3 (kỳ vọng thiết kế ~66-67 theo `docs/work/seed-v2/quest-xp-showcase.md` §4.1). Chi tiết: `docs/pm-decision-log-seed-v2.md` [2026-08-14 04:0x] — muốn đạt 80 cần thêm shop item giá ≤ 30 (ngoài phạm vi, CẤM sửa SeedData).

## Seed V2 (PROD-K — 2026-08-14)

Đợt seed mở rộng cho demo prod (prompt `PROMPT_K_SEED_PROD_V2`, branch `feature/seed-prod-v2`), chạy **trên DB thật đã có dữ liệu V1 + runtime** — chỉ GHI THÊM, không sửa/xóa dữ liệu cũ (xem `docs/pm-decision-log-seed-v2.md`).

- **6 file partial mới** (nối vào `SeedDemoActivity.cs`, gọi SAU V1): `SeedDemoActivity.V2.Data.cs` (data dùng chung: email/persona/index deterministic) + `V2.Students.cs` (69 user + 7 achievement + UserAchievements), `V2.Progress.cs` (submissions/progress), `V2.Activity.cs` (quests/gems/inventory/favorites/feedback), `V2.Class.cs` (lớp mới), `V2.Misc.cs` (code submissions/bug reports/premium showcase).
- **`PlanSeedV2 = 20260814`** — hằng số riêng, KHÔNG đổi `PlanSeed=20260813` của V1 → kế hoạch deterministic độc lập (`V2.Progress.cs`).
- **69 user mới theo 5 persona**: Hardworking 13 / Average 32 / Slacker 13 / New 10 + **showcase** → tổng Users = 95 (26 cũ + 69 mới).
- **Showcase `showcase@demo.local`** (id 2092): Xp **2790** (Level 6), Gems 389, Streak 30, Premium **12m** `DSV2092T12` (ExpiresAt 2027-07-15), HeartsMax 30 — rank 1 leaderboard level/week (verify API smoke).
- **7 achievement mới** (SortOrder 11-17, guard Code): tree-master, graph-expert, code-wizard, speed-demon, lab-master, social-butterfly, quiz-ace → Achievements = 17 (10 V1 + 7 V2); showcase đạt 17/17.
- **Lớp mới "AI1702 — Thuật toán Đồ thị"** (InviteCode `GRPH21`, đóng, 24 member, 6 assignment) + bổ sung member/assignment cho lớp cũ → Classes = 4 (3 seed + 1 rác QA).
- Verify: 16/17 ngưỡng PASS; lệch duy nhất UserInventory (66 < 80 — xem ghi chú trên); chạy seed 2 lần → lần 2 = **0 thêm** (idempotent, kể cả CodeSubmissions với guard `(UserId, ExerciseId)` mới).

## ⚠️ KHÔNG dùng `source/VisualizationDSA1/backend/seed-demo-course.sql`

File SQL cũ của V1 **KHÔNG được bê/dùng**: schema khác hẳn v2 (`AspNetUsers`/`Courses`/`CourseModules`/`Codelabs` không tồn tại trong SDD v2), PasswordHash chuỗi giả — v2 dùng `PasswordHasher` PBKDF2 thật.

## Trạng thái

- `SeedData.cs` + `SeedRunner.cs`: **seed thật chạy được qua AppDbContext sau Migrate** (`--seed`), idempotent — đã chạy thật lên SQL Server docker local (xem ngưỡng ở trên).
- `SeedDemoActivity` (skeleton + partial Students/Progress/Activity/Misc/Class): **đã nối vào SeedRunner** — gọi SAU `SeedSettingsAsync` (SEED-4), idempotent, chỉ seed hoạt động khi bảng trống (CodeSubmissions/BugReports) và tự xóa `allowed.email.domains` ở DB cũ.
- **Seed V2 (PROD-K)**: 6 file `SeedDemoActivity.V2.*` đã nối vào `SeedDemoActivity.cs` — **đã chạy thật 2 lần idempotent (lần 2 = 0 thêm) trên SQL Server docker 14/08/2026**; 16/17 ngưỡng PASS (xem bảng ngưỡng V2 + `docs/work/seed-v2/verify-final.md`).
- 32 bài nguồn còn lại (ngoài 8 bài seed) → backlog GĐ2 (SDD §7.5): thêm Lessons/Exercises/Questions tương tự (SeedData + QuizSelection + CodeTestCases).
