# Seed — Dữ liệu khởi tạo (SDD §7.5)

Seeder **THẬT** chạy qua `AppDbContext` (EF Core 10) — file `SeedRunner.cs` (idempotent: kiểm tra tồn tại trước khi chèn — SDD §7.5/§10.5).
Dữ liệu khai báo tại `SeedData.cs` (5 Topics, 8 Lessons, 3 Users, 8 Quests, 8 ShopItems, 9 Settings).

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
| Settings | 9 setting (Key UNIQUE) | `SeedData.cs` |
| ExerciseSubmissions | **KHÔNG seed** (dữ liệu người dùng) | — |

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

## Verify sau seed (golden data — SDD §7.5)

```sql
SELECT 'Users' t, COUNT(*) n FROM Users UNION ALL SELECT 'Topics', COUNT(*) FROM Topics
UNION ALL SELECT 'Lessons', COUNT(*) FROM Lessons UNION ALL SELECT 'LessonSimulations', COUNT(*) FROM LessonSimulations
UNION ALL SELECT 'Exercises', COUNT(*) FROM Exercises UNION ALL SELECT 'Questions', COUNT(*) FROM Questions
UNION ALL SELECT 'LearningPaths', COUNT(*) FROM LearningPaths UNION ALL SELECT 'LearningPathNodes', COUNT(*) FROM LearningPathNodes
UNION ALL SELECT 'DailyQuests', COUNT(*) FROM DailyQuests UNION ALL SELECT 'ShopItems', COUNT(*) FROM ShopItems
UNION ALL SELECT 'Settings', COUNT(*) FROM Settings;
```

Ngưỡng (bản chạy thật 2026-08-12): Users=3, Topics=5, Lessons=8, LessonSimulations=14, Exercises=29, Questions=76, LearningPaths=5, LearningPathNodes=18, DailyQuests=8, ShopItems=8, Settings=9.

## ⚠️ KHÔNG dùng `source/VisualizationDSA1/backend/seed-demo-course.sql`

File SQL cũ của V1 **KHÔNG được bê/dùng**: schema khác hẳn v2 (`AspNetUsers`/`Courses`/`CourseModules`/`Codelabs` không tồn tại trong SDD v2), PasswordHash chuỗi giả — v2 dùng `PasswordHasher` PBKDF2 thật.

## Trạng thái

- `SeedData.cs` + `SeedRunner.cs`: **seed thật chạy được qua AppDbContext sau Migrate** (`--seed`), idempotent — đã chạy thật lên SQL Server docker local (xem ngưỡng ở trên).
- 32 bài nguồn còn lại (ngoài 8 bài seed) → backlog GĐ2 (SDD §7.5): thêm Lessons/Exercises/Questions tương tự (SeedData + QuizSelection + CodeTestCases).
