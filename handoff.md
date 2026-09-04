# HANDOFF — Review Plan "Dọn Rác & Seeder Demo" (phiên trước kết thúc sớm)

> **CẬP NHẬT ROUND 2 (cùng ngày):** User đã sửa plan theo 6 lỗi cũ (Two Pointers→codelab, list.traverse, FK order, 2 pending GV, XP 2500, 4 shop item). Tôi đã review bản mới và **sửa trực tiếp thêm 7 điểm nữa vào implementation_plan.md**: (1) bổ sung 6 bảng chặn FK bị thiếu (UserProgress/ContentFeedback/LessonNotes/CodeRuns/CodeSubmissions/CourseFeedback — toàn bộ NO ACTION, lấy từ dump FK 58 quan hệ hoàn chỉnh, xem dsh-tools/dbfk5.cs); (2) 12→13 lộ trình (11 Active+1 Draft+1 PendingReview); (3) xóa pending cũ Id=6 lethikimngan kẻo Admin thấy 3 đơn; (4) token cleanup (RefreshTokens/Otp/Reset/RegisterOtp); (5) BugReports 18→5 + Favorites treo; (6) viết lại mục 5 thành Bước 0-4: restart backend TRƯỚC khi seed + không restart sau + script idempotent, mọi ngày giờ lấy runtime SYSUTCDATETIME (QuestDate/LastActivityDate/LastHeartAt/hạn nộp), TopicId cho path mới, ClassAssignments trỏ Lesson/Exercise mới; (7) roadmap 3 thêm search.linear. **Plan giờ đạt để thực thi — bước tiếp theo: viết dsh-tools/dbrun_clean_and_seed.cs theo đúng Bước 0-4.**

**Ngày:** 2026-09-03 · **Dự án:** D:\FPT\metqua (DSA Visual — .NET 10 + Vue 3) · **Demo:** sáng mai
**Nhiệm vụ gốc:** review `C:\Users\Administrator\.gemini\antigravity\brain\5297c52d-5269-4ad7-adc3-fb718d0077f3\implementation_plan.md` đã đạt chưa để demo.
**Kết luận ngắn: Plan KHÔNG đạt để chạy thẳng — có 6 lỗi phải sửa trước khi demo. Chi tiết + fix ở dưới.**

## 0. Môi trường thực tế (đã xác minh)

- **DB KHÔNG phải docker** — SQL Server hosted: `Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True;` (lấy từ `backend/src/DsaVisual.Api/appsettings.Development.json`). Docker daemon không chạy. ⇒ mọi ý tưởng "drop DB / reset-db.ps1" trong plan là KHÔNG THỂ — DB này là shared, KHÔNG được drop.
- **Backend đang chạy:** PID 23144, `D:\FPT\metqua\backend\src\DsaVisual.Api\bin\Debug\net10.0\DsaVisual.Api.exe` — port 5000. Health OK: `GET http://localhost:5000/health` → `{"status":"ok"}`.
- **Frontend dev:** Vite port 5173 (PID 2676, node). Có bản deploy: `visualdsa.runasp.net` (backend) + Vercel (frontend) — cùng dùng DB db65198 này.
- **Login đã test OK:** POST `http://localhost:5000/api/v1/auth/login` email/password JSON → 200 + token. Tài khoản demo: `student@demo.local/Student@123`, `teacher@demo.local/Teacher@123`, `admin@system.local/Admin@123` (nguồn: `backend/src/DsaVisual.Application/Persistence/Seed/SeedData.cs:55-57`).
- Tool query DB: các script C# chạy được ở `D:\FPT\metqua\dsh-tools\*.cs` (dùng `dotnet file.cs`, pattern: `#:package Microsoft.Data.SqlClient@6.0.2`). LƯU Ý: chuỗi C# trong run_code phải tránh `\n` literal — dùng `.PadRight` + `.Join("\n")` từ mảng dòng như các file hiện có.

## 1. Đối chiếu Plan vs Thực tế — bảng quyết định

### ✅ Khớp (không cần sửa)
- **Entity/enum:** UserRole (0 Student, 1 Teacher, 2 TeacherPending, 3 Admin), LearningPathStatus (0 Draft,1 PendingReview,2 Active,3 Rejected,4 ClassOnly), ClassStatus (0 Open, 1 Closed), BugReportStatus (0 New,1 Processing,2 Resolved,3 Closed), LessonStatus (0 Draft,1 PendingReview,2 Active,3 Hidden) — `backend/src/DsaVisual.Application/Persistence/Entities/Enums.cs`.
- **36 bảng** (35 dữ liệu + __EFMigrationsHistory) — plan nói 35, gần đúng.
- **Gamification fields:** Hearts/HeartsMax/Gems/Xp/StreakDays/StreakFreeze/PremiumUntil có đủ trên Users.
- **19/20 visualizer key** trong plan khớp `shared/simulation-catalog.json` = `frontend/src/engines/catalog.ts` (44 generators, CI-enforced khớp nhau). Key chuẩn: `sort.bubble, sort.selection, sort.insertion, sort.quick, sort.merge, search.binary, stack.push, queue.enqueue, list.insert, hash.insert, hash.delete, tree.bst-insert/search/inorder, tree.avl-insert, heap.insert, graph.bfs, graph.dfs, graph.dijkstra`...
- **4 class PRO192/CSD201/ALGO301/DSA202** — mô hình hợp lệ (Name, InviteCode, Semester, Status, OwnerId). Giữ OwnerId=2 (teacher@demo.local có Id=2).
- **Lesson content type:** bài học dùng `sandboxType`: 'dsa' (lý thuyết+visual), 'quiz', 'codelab' — đủ cho cấu trúc "1 LT + 1 Quiz + 1 Codelab" mỗi chương.

### ❌ LỖI 1 — Visualizer KHÔNG TỒN TẠI (làm hỏng demo live)
- Roadmap 3 Chương 2 "Hai con trỏ (Two Pointers)": **KHÔNG có visualizer** — chỉ là codelab task (`frontend/src/features/lesson/utils/codelabTaskRegistry.ts:136`). Frontend sẽ hiện ô lỗi "not found" nếu lesson trỏ simulation key không có trong catalog; **backend còn reject key lạ: SIMULATION_KEY_INVALID**.
- Roadmap 5 Chương 2 "Đảo ngược danh sách liên kết": cũng KHÔNG có visual (catalog chỉ có list.insert/delete/search/traverse).
- **FIX:** dùng key có sẵn thay thế. Ví dụ: Chương 2 Roadmap 3 → gộp vào `search.linear`/nội dung lý thuyết không visual, hoặc bỏ hẳn chương; Roadmap 5 Chương 2 → dùng `list.traverse` + codelab reverse.

### ❌ LỖI 2 — FK chặn script dọn rác (SQL thuần sẽ FAIL)
FK delete rules thực tế (NO ACTION = phải xóa con trước):
- `ExerciseSubmissions → ClassAssignments` (NO ACTION), `ClassAssignments → Classes` (CASCADE)
- `Exercises → Lessons` (NO ACTION), `LearningPathNodes → Lessons` (NO ACTION), `UserNodeProgress → LearningPathNodes` (NO ACTION)
- `LearningPathNodes → LearningPaths` CASCADE, `LessonSimulations → Lessons` CASCADE, `Questions → Exercises` CASCADE.
- **Hiện trạng孤儿:** 46 orphan lessons (plan nói 60), 80 orphan exercises, 322 orphan ExerciseSubmissions (thuộc assignment cũ!), 62/2576 refresh token hết hạn.
- **FIX bắt buộc:** thứ tự xóa: ExerciseSubmissions → ClassAssignments → ClassMembers → Classes; UserNodeProgress → NodeSessions → LearningPathNodes (bỏ node xóa trước) → LessonSimulations/Questions/Exercises → Lessons → LearningPaths. Xóa soft-delete trước, hard-delete sau nếu cần.

### ❌ LỖI 3 — Chỉ có 1 pending teacher (plan cần 2)
- DB hiện có đúng 1: Id=6 `lethikimngan@university.edu.vn` (Lê Thị Kim Ngân, StaffCode=NULL).
- Plan cần 2 đơn: `pending.gv@dsavisual.com` (GV2026-089) + `tranvanhung.pending@university.edu.vn` (GV2026-104) — phải INSERT 2 user Role=2 (TeacherPending) kèm StaffCode/Department (Users có cột StaffCode/Department/AcademicDegree, nullable, StaffCode unique chỉ áp dụng non-null — xem `UserConfiguration.cs:22-31`).

### ❌ LỖI 4 — Gamification mâu thuẫn với Bảng xếp hạng
- Plan: student 1.250 XP = Level 5, "Top 4 BXH". Thực tế Users XP cao nhất: student=8685, Thái Quang Son=2790, teacher=2270, admin=2268, Huỳnh Thúy=2160 → 1250 XP chỉ đứng ~hạng 6+.
- DB hiện tại: student Id=3 XP=8685, Streak=7. Plan muốn streak 5.
- **FIX:** hoặc nâng mục tiêu XP (≥2200 để vào Top 4) hoặc bỏ claim "Top 4". KHÔNG hạ về 1250 nếu muốn giữ hình ảnh BXH đẹp. Hearts 8/10, Gems 450 (mua được item 50-250 gems) — OK.

### ⚠️ LỖI 5 — Auto-seed khi backend khởi động sẽ ĐÈ dữ liệu sau khi dọn
`backend/src/DsaVisual.Api/Program.cs:396-438` — MỖI LẦN backend start (không chỉ --seed) đều chạy:
`SeedRunner.FixMismatchedQuestionsAsync`, `AutoRepairOrphanTheoryNodesAsync`, **`SeedTeacherCoursesData.SeedAsync`** (ép teacher Gems≥2500, xóa DeletedAt...), `CleanModulePrefixesAsync`, `ReconcileSequentialNodeProgressAsync`.
⇒ Nếu seeder demo không idempotent với các routine này, dữ liệu "chuẩn" sẽ bị sửa lại mỗi lần restart backend trước demo. Cần đọc kỹ `SeedTeacherCoursesData.cs` (36KB) xem nó có đè XP/path/class nào không; safest: chạy dọn+seed lần cuối rồi KHÔNG restart backend nữa.

### ⚠️ LỖI 6 — Shop rác: plan đếm 3 item nhưng thực tế 4+
Items rác: Id 13 "avt" (100), Id 15 "11a" SIÊU SIÊU SIẾU (5), Id 16 "1" cà bí bà rá (20), Id 11 avatar-dragon-1788291511887 (350, key có timestamp = tạo từ UI demo). Id 16 đang nằm trong inventory của student (đã mua) — phải xóa UserInventory của các item này TRƯỚC khi xóa ShopItems (FK CASCADE UserInventory→ShopItems nên cascade tự lo, nhưng kiểm tra lại).

### ℹ️ Ghi chú thêm
- Classes hiện có 15 (rác: "lớp du bây bê", "lớp xe", "lớp diu", "test"...) — plan "xóa 15 lớp cũ" OK nhưng nhớ xóa ExerciseSubmissions của các assignment đó trước (322 rows NO ACTION).
- LearningPaths hiện 25 (Status: 17 Active, 3 Draft+..., 1 PendingReview, vài ClassOnly). Plan "xóa 13 lộ trình rác" — cần chọn theo Id cụ thể, tránh xóa path 1-8 chuẩn (Status=2, CreatedBy=1) mà frontend đang trỏ. Đặc biệt path có node cấu trúc module bài bản (Path 1: Bubble Sort node có Lesson 1/82/84...).
- DailyQuests có 8 quest; UserQuests 3204 rows — plan dọn ẩn quest cũ nhưng cẩn thận FK CASCADE UserQuests→DailyQuests.
- Subagent backend-entities (id 5cd213e7-3303-4b2a-81dd-ae05c49fec6c) CHƯA kịp trả kết quả khi hết token — kết quả phân tích entity phía trên là tự điều tra trực tiếp bằng script DB + grep, đã đủ dùng.

## 2. Việc CẦN LÀM ở phiên sau (theo thứ tự)

1. **Sửa plan:** fix LỖI 1 (bỏ/thay 2 visual thiếu), LỖI 4 (XP Top-4), bổ sung thứ tự xóa FK vào "Bước 1", thêm 2 pending teacher mới (Role=2), cập nhật số rác thực tế (46 lessons / 80 exercises / 322 submissions / 62 tokens / 4 shop items / 2576 refresh tokens — plan nói 2512).
2. **Quyết định strategy seed:** viết `SeedDemoData.cs` mới (partial, theo pattern `SeedDemoActivity.V2.*.cs`) thay vì SQL script thuần, vì: a) DB shared không drop được, b) phải harmonize với auto-seed LỖI 5, c) codebase đã có sẵn infra (`SeedRunner.cs` 91KB + README.md 19KB docs đầy đủ shapes quiz/codelab/LAB/CODE exercise).
3. **Chạy seeder** rồi **integrity check** bằng script `dsh-tools` (đếm orphan = 0, FK ok).
4. **E2E smoke:** login 3 tài khoản qua API; mở FE 5173; verify: 12 lộ trình hiển thị, 4 lớp mới, student có hearts 8/10 + gems 450 + quest 1 claimed, admin thấy 2 pending GV + 1 path PendingReview + 5 bug (2 Resolved/1 Processing/2 New — hiện DB có 18 bug reports lẫn rác QA, phải dọn).
5. **Không restart backend sau khi seed lần cuối** (hoặc verify SeedTeacherCoursesData không phá dữ liệu mới).

## 4. Đã xác nhận bằng nghiên cứu độc lập (subagent frontend, 2026-09-03)

**Không tồn tại shape `content[].type==='visual'`** — bài học = node với `sandboxType` + `sandboxConfig` (JSON string); visual trỏ bằng **catalog key** qua 3 kênh:
1. `sandboxConfig`: `{"simulationKey":"sort.bubble"}` (1 key) hoặc `{"simulationKeys":["a","b"]}` (nhiều).
2. Bảng `LessonSimulations` (LessonId, SimulationKey, Title, SortOrder) → API detail trả `simulations` + `simulationKeys`.
3. Marker inline trong markdown lý thuyết: `[Mô phỏng: sort.bubble]` (regex ở `LessonStepTheory.vue`).

**`sandboxType` hợp lệ (switch ở LessonStudyView.vue:188-215):** `'dsa'` (lý thuyết), `'quiz'`, `'codelab'`, `'folder'`. KHÔNG có 'visual'/'code'. Lý thuyết = 1 chuỗi markdown `contentMd` duy nhất.

**Full 44 catalog keys (nguồn chân lý shared/simulation-catalog.json = engines/catalog.ts):**
sort.bubble, sort.selection, sort.insertion, sort.merge, sort.quick, sort.heap, search.linear, search.binary, stack.push, stack.pop, stack.peek, queue.enqueue, queue.dequeue, list.insert, list.delete, list.search, list.traverse, tree.bst-insert, tree.bst-delete, tree.bst-search, tree.bst-preorder, tree.bst-inorder, tree.bst-postorder, tree.bst-levelorder, tree.avl-insert, heap.insert, heap.extract, heap.heapify, hash.insert, hash.search, hash.delete, graph.bfs, graph.dfs, graph.dijkstra, structure.array, structure.linkedlist, structure.stack, structure.queue, structure.binarytree, structure.bst, structure.avl, structure.heap, structure.hashtable, structure.graph.

**Shape quiz cho seeder:** node `sandboxType="quiz"`, `sandboxConfig {"quizId":"<exerciseId>"}`. Câu hỏi: `{id, text, type:'SINGLE'|'MULTIPLE', options:[], correctIndex?|correctIndices?, explanation?}`; DB lưu `OptionsJson` + `AnswerJson` (mảng index đúng). Chấm server-side qua POST /concepts/quiz/submit (GET giấu đáp án). Ví dụ: `backend/seed-data/algorithms/quizzes/*.json`.

**Shape codelab cho seeder:** node `sandboxType="codelab"`, `sandboxConfig` = JSON (chấp nhận: mảng task / `{tasks:[...]}` / 1 task). CodeLabTask: `{id?, title?, description, initialCode, solution, entryFunction?, testCases:[{input, expectedOutput, isHidden?}], hints?}` — `input` là JSON-encoded MẢNG tham số, `expectedOutput` = JSON.stringify kết quả. Ví dụ thật: `backend/seed-data/grokking-course.json` + `algorithms/assignments/a3-binary-search.json`.

**Giải pháp chính cho Two Pointers (LỖI 1):** KHÔNG seed simulation key — seed dạng lesson codelab với **title chứa "Two Pointers"/"Hai con trỏ"** để auto-match task registry (`codelabTaskRegistry.ts:136`), hoặc để chương đó chỉ có lý thuyết+codelab, không visual.

**Lưu ý validation bất đối xứng:** API `LessonUpsertRequest.SimulationKeys` bị chặn key lạ (`SIMULATION_KEY_INVALID`, LessonService.cs:613), nhưng `SeedRunner.SeedLessonSimulationsAsync` chèn thẳng KHÔNG validate — key sai chỉ lộ thành ô lỗi "Chưa tìm thấy bộ mô phỏng" trong player lúc demo (không crash nhưng xấu).

## 5. File quan trọng cần đọc ở phiên sau
- `backend/src/DsaVisual.Application/Persistence/Seed/README.md` — 19KB, mô tả toàn bộ seed + shapes ConfigJson exercise (LAB maxSteps, CODE testCases, MCQ points).
- `backend/src/DsaVisual.Application/Persistence/Seed/SeedRunner.cs` — 91KB, entry `SeedAsync`; flag CLI: `dotnet run --project src/DsaVisual.Api -- --seed` (Migrate + seed rồi thoát).
- `shared/simulation-catalog.json` — 44 keys, nguồn chân lý visualizer. **`demoAllowed:true` chỉ sort.bubble + 1-2 key khác** — flag này KHÔNG ràng buộc render, chỉ metadata.
- `frontend/src/engines/catalog.ts` — frontend registry khớp catalog.json.
- `frontend/scripts/reset-db.ps1` — vô dụng ở DB này (trỏ docker + `D:\FPT\neww`).
- Dùng `dsh-tools/dbtables.cs, dborphan.cs, dbfk4.cs, dbdetail.cs, dbschema.cs` để re-verify sau seed.
