# Seed — Dữ liệu khởi tạo (SDD §7.5)

Kế hoạch seed dữ liệu khởi tạo cho SQL Server qua `AppDbContext` (EF Core 10).
Mọi seed **idempotent**: kiểm tra tồn tại trước khi chèn (SDD §7.5 — seeder chạy qua StepExecutor + golden data).

## Phạm vi dữ liệu (SDD §7.5)

| Bảng | Dữ liệu seed | Nguồn |
|---|---|---|
| Users | 1 Admin (`admin@system.local` — ép đổi mật khẩu lần đầu), 1 Teacher mẫu, 1 Student mẫu | `SeedData.cs` |
| Topics | 5 chủ đề: "Sắp xếp & Tìm kiếm", "CTDL tuyến tính", "Cây", "Bảng băm", "Đồ thị" | `SeedData.cs` |
| Lessons | **8 bài mẫu**: Bubble Sort, Binary Search, Stack, Linked List, BST, AVL, Hash Table, BFS — rich theory + 1 mô phỏng EDV + 5-10 quiz (giải thích tiếng Việt) + 1 lab + 1 code challenge (test ẩn ~11/bài) | `frontend/src/data/lessons.ts` (stub + quick-sort mẫu) + `source/VisualizationDSA3/plan/content-drafts/v2/lesson-XX/` |
| LessonSimulations | gắn mô phỏng theo key — VD bài Bubble Sort → `sort.bubble` | `frontend/src/data/lessons.ts` → `simulations[]` |
| Exercises/Questions | 8 bài × (quiz 5-10 câu + lab + code challenge) theo 19.6A/19.6B | `content-drafts/*/quiz.json` (mẫu lesson-01) |
| LearningPaths/Node | 5 path × (node bài học + node luyện tập tổng hợp + final test) | TODO GĐ2 |
| DailyQuests | 8 quest templates (19.3A) | TODO GĐ2 |
| ShopItems | 8 item (19.3) | `frontend/src/data/shop_items.json` (10 item sẵn — chọn 8, map `price`→`PriceGems`, `type`→`Type`) |
| Settings | `site.name`, `allowed.email.domains`, `password.policy.minLength=8`, `upload.maxSizeMb=5`, `simulation.maxArraySize=100`, `simulation.maxGraphVertices=50`, `auth.maxLoginAttempts=5`, `auth.lockoutMinutes=15`, `simulation.defaultSpeed=1` | `SeedData.cs` |

> 10 bài còn lại + test ẩn → backlog GĐ2 (SDD §7.5). Nguồn content thật có sẵn **40 bài**:
> `source/VisualizationDSA3/plan/content-drafts/v2/lesson-01..lesson-40/` — mỗi bài gồm `content.md` (lý thuyết tiếng Việt) + `quiz.json` (5-10 câu, `{questionText, options, correctIndex, explanation}`).
> `content.md` là Markdown → cần chuyển HTML + **sanitize bằng Ganss.Xss** (ràng buộc `Lesson.ContentHtml` — SDD §7.3.2).

## Nguồn dữ liệu frontend (đường dẫn từ repo root)

- `frontend/src/data/courses.ts` — `SEED_COURSES`: 4 course DSA, `topicId` map Topics SDD §7.5 (1=Sắp xếp & Tìm kiếm, 2=CTDL tuyến tính, 3=Cây, 4=Bảng băm, 5=Đồ thị).
- `frontend/src/data/lessons.ts` — `LESSONS`: 13 bài (quick-sort đầy đủ theory/quiz/codelab; 12 bài stub có `simulations[]` key hợp lệ trong `shared/simulation-catalog.json`).
- `frontend/src/data/shop_items.json` — 10 item cửa hàng gems (V1, đã verify id unique).

Mapping lesson → content-drafts (đã scan, chưa đối chiếu hết từng bài):
`lesson-01` = Big O (không có trong 8 bài SDD — dùng làm bài bổ trợ/quiz riêng); các lesson-XX còn lại cần đối chiếu title trước khi seed chi tiết (TODO).

## Lệnh seed (qua DbContext — TODO task sau)

```powershell
# Tạo migration + CSDL trước (Persistence/README.md)
dotnet ef migrations add InitialCreate --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
```

Seeder triển khai dạng `DbInitializer` (hoặc `IHostedService` dev-only) gọi từ `Program.cs`:

```
if (args.Contains("--seed")) → chạy SeedRunner.SeedAsync(db) sau Migrate()
```

Quy tắc idempotent (SDD 10.5):
- Users: kiểm tra theo `Email` (UNIQUE).
- Topics: kiểm tra theo `Name`.
- Lessons: kiểm tra theo (`TopicId`, `Title`).
- LessonSimulations: `UNIQUE (LessonId, SimulationKey)` — đã có trong entity.
- ShopItems: `ItemKey` UNIQUE.
- Settings: `Key` UNIQUE.
- Golden data: sau seed, chạy bộ kiểm tra đếm dòng (không ít hơn ngưỡng SDD §7.5) — chưa triển khai, TODO.

## ⚠️ KHÔNG dùng `source/VisualizationDSA1/backend/seed-demo-course.sql`

File SQL cũ của V1 **KHÔNG được bê/dùng**. Lý do:
1. Schema khác hẳn v2: `AspNetUsers` (Id GUID, Identity ASP.NET) vs `Users` (Id int — SDD §7.3.1); các bảng `Courses`, `CourseModules`, `CourseModuleItems`, `Codelabs`, `Classrooms` **không tồn tại** trong SDD v2 (thay bằng Topics/Lessons/Exercises/Questions/LessonSimulations).
2. PasswordHash là chuỗi giả (không verify được); v2 cần hash thật qua `PasswordHasher` + `IsPrimaryAdmin` (ép đổi mật khẩu lần đầu).
3. Cần viết lại hoàn toàn cho schema v2 (TODO — `SeedData.cs` hiện chỉ mô tả cấu trúc, chưa chèn dữ liệu thật).

## Trạng thái hiện tại

- `SeedData.cs`: mô tả cấu trúc + dữ liệu TỐI THIỂU (3 topic, 3 lesson, 3 user, 8 shop item, 9 setting) — **chưa chạy qua DbContext**.
- TODO: seed thật (Topics 5 + Lessons 8 + Exercises/Questions từ content-drafts + LearningPaths/DailyQuests GĐ2) ở task sau.
