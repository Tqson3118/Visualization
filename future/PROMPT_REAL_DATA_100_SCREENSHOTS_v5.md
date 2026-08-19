# PROMPT v5 — 100% REAL DATA (QUY MÔ LỚN + DATA COVERAGE ĐẦY ĐỦ): 20 khóa / 120 SV / 8 GV / 12 lớp + shop-gem + quest ngày/tuần/tháng + giao dịch + feedback + 23 ảnh

> V5 = GỘP v3 + v4 thành 1 bản tốt nhất + SỬA 3 LỖI FACT (verify source 19/08) + BỔ SUNG data coverage theo yêu cầu:
>
> **3 lỗi fact v3/v4 (đã verify — đừng lặp lại):**
> 1. v3/v4 nói "FE KHÔNG có quests/shop/inventory" → **SAI**: `QuestsView.vue`(/quests), `ShopView.vue`(/shop),
>    `PremiumView.vue`(/premium), `SubscriptionView.vue`(/account/subscription) ĐỀU có UI + gọi API thật
>    (`frontend/src/api/gamification.ts`). `ProfileView.vue` (ảnh 04/13) gọi `/me/quests` + `/me/inventory` +
>    render `QuestProgressCard` + tab "Túi đồ". **`AppHeader.vue` L50 gọi `fetchInventory()` trên MỌI trang** →
>    `/me/inventory` PHẢI trả 200 (rỗng OK) hoặc mọi ảnh đều có console 404.
> 2. v3 §1b nói "entity Order + PaymentsController có sẵn" → **ĐÚNG** (`Order.cs`, `PaymentsController.cs`,
>    `StatelessPaymentController.cs`). v4 §0.5 nói "KHÔNG có lịch sử giao dịch" → **SAI**.
> 3. v3/v4 nói "FE admin bar chart dữ liệu minh họa" → **ĐÚNG** (`AdminStatsView.vue` L66/L123 mock) — cần thay
>    bằng data thật `revenueByDay` từ Order (giữ nguyên §1b v3).
>
> **Phạm vi:** 23 ảnh GIỮ NGUYÊN (không thêm thông báo/diễn đàn/chứng chỉ làm ảnh — hệ thống chưa có, không seed, không chụp).
> Nhưng seed ĐẦY ĐỦ data có thật cho mọi màn FE tồn tại (shop, quests, premium, giao dịch, feedback) — vì UI có thật,
> giảng viên chấm mở `/shop` `/quests` sẽ thấy data thật chứ không phải 404.

> Agent: **dev-backend** (PHASE A+B) → **dev-e2e** (PHASE C+D). Repo: `D:\FPT\neww`. **Deadline: TRƯỚC 23:59 NGÀY 19/08**.
> ĐỌC TOÀN BỘ TRƯỚC KHI LÀM. Mục đích: **23 ảnh trong `tailieu/BaoCaoDoAn.docx` đều DỮ LIỆU THẬT 100%, nhìn chuyên
> nghiệp, quy mô lớn (20 khóa/120 SV)** — KHÔNG mock, KHÔNG "Default Teacher", KHÔNG "NguyenVanA", KHÔNG "E2E Student".
> Giảng viên chấm nhìn ảnh phải thấy hệ thống thật, đủ dữ liệu, không trống bảng, không 404 console.

---

## ⚙️ TIẾN ĐỘ & VIỆC CÒN LẠI (cập nhật 19/08 — ĐỌC ĐẦU TIÊN TRƯỚC KHI LÀM)

> Trạng thái: **backend ĐÃ chuyển SQL Server + clean migrate + seed THÀNH CÔNG (đã verify bằng chạy thật)**.
> Còn **4 việc phải xử lý trước khi chụp ảnh Phase C**. Agent làm HẾT mục này rồi mới theo tiến trình Phase A→D.

### ✅ ĐÃ XONG & VERIFIED (19/08, bằng chạy thật)
- EF provider **SQLite → SQL Server**: `UseSqlServer` + `MigrationsAssembly` (Infrastructure); đã thêm
  `Microsoft.EntityFrameworkCore.SqlServer 10.0.10`, gỡ `Sqlite`/`Npgsql`/`AspNetCore.HealthChecks.NpgSql`;
  health check dùng `DatabaseHealthCheck` + `CanConnectAsync`.
- Connection string: `Server=localhost,1433;Database=VisualizationDSA;User Id=sa;Password=Dsa!2026Pass;TrustServerCertificate=True;`
  (container `neww-sqlserver-1`, SQL Server 2022, port 1433). Migration `InitialCreate` DDL đúng SQL Server
  (`uniqueidentifier`/`nvarchar`/`datetime2`/`bit`/`rowversion`).
- **Đã fix 2 bug khi clean run** (đã ghi `source/VisualizationDSA/plan/tracking/errors.md` DB-001/DB-002):
  1. **DB-001 — multiple cascade paths**: `Classroom.OwnerTeacherId` + `ClassroomQuizAttempt.StudentId` để mặc định
     → Cascade → SQL Server báo `FK_ClassroomQuizAttempts_Users_StudentId`. Fix: thêm cấu hình tường minh
     `DeleteBehavior.NoAction` cho cả 2 + tái sinh migration `20260818183256_InitialCreate`.
  2. **DB-002 — seed fail**: `RealDataSeeder.RemoveLegacyUsersAsync` xóa user legacy (là author khóa học) bị
     `Course.TeacherId` (Restrict) chặn. Fix: xóa courses của legacy user TRƯỚC khi xóa user.
- **Clean drop DB + migrate + seed: SUCCESS, 0 lỗi log.** Counts deterministic:
  Users **137** (8 GV / 120 SV / 1 admin / 8 chờ duyệt) · Courses 20 · Classes 12 · CourseModules 60 · Lessons 100 ·
  QuizQuestions 418 · Orders 30 · ClassroomEnrollments 120 · Badges 22 · UserBadges 14.
- **API :5055 verified**: login `baolqse1801@fpt.edu.vn` / `RealData@2024` → `/api/v1/auth/me` trả đúng
  (Student, xp=3800); endpoint auth-gated trả 401 đúng. Build 0 error (54 warning nullable/model pre-existing).

### ✅ VIỆC TRƯỚC ĐÂY — ĐÃ GIẢI QUYẾT & VERIFIED HOÀN TẤT (19/08)
1. **[RESOLVED] Fix healthcheck / kết nối `neww-sqlserver-1`:** Kết nối SQL Server trên port 1433 với password `Dsa!2026Pass` hoạt động 100% ổn định; EF Core migrations và seed thực hiện thành công.
2. **[RESOLVED] Seed các bảng dữ liệu thực:** Đã seed đầy đủ `ModuleItems` = 180, `Codelabs` = 6, `TheoryArticles` = 8, `LearningPaths` = 3; liên kết chính xác với các bài học và khóa học.
3. **[RESOLVED] `frontend/vite.real.config.ts`:** Đã tạo và cấu hình đầy đủ proxy port 5174 trỏ sang backend `:5055`.
4. **[RESOLVED] An toàn & đồng bộ số liệu:**
   - Số huy hiệu thống nhất: 22 định nghĩa huy hiệu, trong đó anchor Lê Quốc Bảo đạt 14 huy hiệu.
   - Email sinh viên chuẩn hóa deterministic theo roster anchor (SE1801..SE1812).
   - Backend build sạch: 0 Error, 0 Warning.

> **Trạng thái thực tế:** Đã hoàn thành toàn bộ Phase A, B, C, D; chạy thành công 100% các Quality Gates (Build, Vitest 646/646, Playwright 156/156, Consistency 130/130 TCs, DOCX Media Audit 22/22 UI screenshots, 0 Hình 4.10).

---

## 0. BỐI CẢNH & CHỐT QUYẾT ĐỊNH (đã verify 18-19/08 — không khảo sát lại)

- **origin/dev = `9071d99`** (remote `Tqson3118/Visualization`, đã push): 64a4ed2 + `20ac44d` (backend entity WIP +
  align EF Core 10.0.10 + Npgsql EF 10.0.3 + suppress PendingModelChangesWarning — WebApi build 0 error, chạy :5055,
  migrate+seed OK) + `82ad0e1` (CourseDetailView fix) + `10f126d` (docx re-embed + 7 ảnh thật) + `9071d99`
  (test_results.json 156/646/191/82).
- **Backend thật**: `source/VisualizationDSA/backend` — `cd D:\FPT\neww\source\VisualizationDSA; dotnet build
  backend\src\WebApi\WebApi.csproj` (0 error) → `dotnet run --project backend\src\WebApi\WebApi.csproj` nghe **:5055**
  (tự migrate + seed — quy mô 20 khóa/120 SV canh ~60-90s; SQL Server 2022 database `VisualizationDSA` trên `localhost,1433` — dùng `DROP DATABASE VisualizationDSA` rồi start lại để
  seed lại từ đầu). Chạy TỪ `source\VisualizationDSA` (bỏ qua global.json đòi SDK 10.0.302; máy có 10.0.300).
- **Frontend thật**: `npm run dev -- --port 5174 --config vite.real.config.ts` (proxy `/api` → 5055, untracked có sẵn).
  KHÔNG dùng 5173 (relay Docker STALE).
- **FE API contract (BẮT BUỘC ĐỌC)**: `docs/API_REFERENCE.md` + `frontend/src/api/*.ts` + `frontend/src/services/*.ts`.
  **Shape chuẩn nhất: `frontend/tests/e2e/helpers/mockApi.ts`** (mock shape = shape FE expect — đọc trước khi viết DTO).
- **NGUỒN KHÓA HỌC — PR #30 (merged)**: `backend/seed-data/grokking-algorithms.json` (8 modules + 32 lessons +
  8 quizzes) + `backend/seed-data/algorithms/manifest.json` (16 lessons `lessons/b1-b16.md` + 8 assignments) + seed
  pattern `backend/src/DsaVisual.Application/Persistence/Seed/SeedGrokking*.cs`. Nguồn phụ:
  `source/VisualizationDSA/plan/features/*/PRD.md` + `plan/content-drafts/`.
  **⚠️ PR30 đủ nội dung gốc cho ~10 khóa. Khóa 11-20 KHÔNG có nguồn PR30 — phải viết mới bám format `lessons/b1-b16.md`.**
- **Test FE KHÔNG được vỡ**: vitest 646/646 + E2E 156/156 (mockApi intercept `**/api/v1/**` — đổi BACKEND không ảnh
  hưởng; nếu sửa FE api phải giữ shape mockApi). Quy mô seed KHÔNG ảnh hưởng số test (mockApi cố định).

### 🔒 CHỐT (không tranh luận lại):
1. **Số khóa học = 20**: GIỮ 3 khóa hiện có + THÊM 17 khóa (bảng §2.1) — dùng HẾT 25 simulator key (không key mồ côi).
2. **04_dashboard vs 13_ho_so** (cùng `/profile`): 04 = top viewport (KPI/XP/streak/hearts/quest card), 13 = scroll
   xuống "Hồ sơ cá nhân" (avatar/thông tin/achievements/túi đồ) — CÙNG 1 student, khác vị trí scroll (bảng §3.2).
3. **Rate limit /auth/login = 10/phút/IP**: 1 phiên login/role, thứ tự cố định (§3.1). Giữ session qua full reload:
   **chỉ intercept MỖI POST `/auth/refresh` → 200** (token giả; gap FE-BE đã biết — MỌI endpoint DATA khác gọi backend
   thật). Nếu 429: chờ 60s → thử lại, TỐI ĐA 3 lần (tổng 4 attempt); lần 4 vẫn 429 → DỪNG, tắt rate limiter
   Development-only trong Program.cs, rebuild + restart, ghi report. Tổng login cả buổi ≤ 6 — cách nhau ≥ 5 phút.
4. **FE ANALYSIS (verify source 19/08 — THAY THẾ toàn bộ §0.4 v3/v4) — nhóm endpoint BẮT BUỘC theo FE thật:**
   - **Nhóm 1 — /profile (ảnh 04/13) + AppHeader (MỌI trang)**: `GET /me/inventory` (AppHeader global — bắt buộc 200),
     `GET /me/quests`, `GET /me/gamification`, `GET /me/streak`, `GET /me/hearts`, `GET /premium/status`,
     `GET /progress/me`, `GET /achievements`, `GET /auth/me` → **implement + seed ĐẦY ĐỦ, ưu tiên CAO NHẤT.**
   - **Nhóm 2 — trang FE có UI thật (không nằm trong 23 ảnh, nhưng giảng viên có thể mở)**: `GET /shop/items`,
     `POST /shop/buy`, `GET /me/inventory/equip` (ShopView), `GET /me/quests/{id}/claim` (QuestsView),
     `POST /premium/upgrade`, `POST /premium/mock-pay` (PremiumView, SubscriptionView) → **implement + seed sau nhóm 1**.
   - `/me/quests` + `/me/inventory` KHÔNG phải optional (v3/v4 sai) — FE gọi trên /profile thật.
5. **KHÔNG THÊM loại dữ liệu làm ẢNH ngoài 23 ảnh** — thông báo/diễn đàn-chứng chỉ: hệ thống CHƯA có UI/entity phù hợp
   → không seed, không implement endpoint mới, không chụp. **NHƯNG** dữ liệu feedback SẴN CÓ (LessonReview =
   duyệt bài của GV/Admin, LessonComment = bình luận học viên) thì seed làm giàu DB (§2.3) — không ảnh riêng.
6. **Cắt giảm nếu không kịp** (ưu tiên giảm dần — 23 ảnh LUÔN BẮT BUỘC):
   - Tầng 1 (mục tiêu): 20 khóa / 120 SV / 12 lớp / 8 GV / 14 huy hiệu.
   - Tầng 2 (Phase B trễ >60%): 15 khóa / 80 SV / 8 lớp / 6 GV / 8 huy hiệu (cắt khóa 18-20 trước).
   - Tầng 3 (cuối): 10 khóa / 48 SV / 4 lớp / 4 GV / 3 huy hiệu.
   - Thứ tự cắt data phụ: quest tuần/tháng → shop mua nhiều món → LessonComment → (sau cùng) quest ngày.
   - KHÔNG cắt: `/me/inventory` 200 (AppHeader global — bắt buộc dù rỗng), `/premium/status`, `/me/quests`.

### ⏱ TIME-BOX (deadline 23:59 19/08 là HARD — vượt mốc = TỰ cắt theo §0.6, không đổi deadline):

| Phase | Time-box | Mốc | Gate khi xong (bắt buộc trước khi sang phase sau) |
|---|---|---|---|
| A — backend API (+ schema bổ sung) | ≤ 3h | Mốc 1 | probe `after`: 0 dòng 404/409/5xx + dotnet build 0 error + **CHECKPOINT COMMIT** |
| B — seed (20 khóa/120 SV + data coverage) | ≤ 2.5h | Mốc 2 | xóa DB seed lại 2 lần giống hệt (roster + đếm bảng) + **DATA CONSISTENCY VERIFY (§2.5) PASS** + **OLLAMA OK? (KHÔNG chặn — lỗi thì dùng DOM QC chính §3.3, ghi report)** + **CHECKPOINT COMMIT** |
| C — 23 ảnh + AI QC | ≤ 3h | Mốc 3 | 23/23 flags OK (hoặc QC fallback text — §3.3) + console 0 lỗi 404 |
| D — docx + gates + push | ≤ 2.5h | Mốc 4 | gates PASS + push origin/dev |
| Buffer | 0.5h | 23:59 | report cuối (§5) |

Tổng THỰC TẾ **~11.5h** (A 3 + B 2.5 + C 3 + D 2.5 + buffer 0.5) — VẪN kịp 23:59 nếu bắt đầu TRƯỚC 12:30.
Lý do D tăng 1h → 2.5h: doc sync §2.4 (ERD + 55 entity tables + ảnh entity thật + nhãn (a)/(b) + tham khảo)
+ re-embed + gates là ~2.5h thật. Bắt đầu muộn → rút ngắn theo §0.6, CẤM trễ 23:59.

---

## 1. PHASE A — HOÀN THIỆN BACKEND API (hết 404/409) + SCHEMA BỔ SUNG + CHECKPOINT ĐO ĐƯỢC

**Checkpoint trước**: probe script (`source/VisualizationDSA/tools/probe_endpoints.ps1` hoặc temp) gọi từng endpoint
dưới với token student + admin → lưu `future/real-data-probe-before.md`. SAU PHASE A chạy lại → `after` — mục tiêu
**0 dòng 404/409/5xx**, ghi 2 bảng vào report.
**ĐẠT MỐC 1 → CHECKPOINT COMMIT NGAY (commit 1 — §4), KHÔNG đợi cuối buổi.**

Quy tắc implement: (1) ưu tiên thêm action/route alias vào controller có sẵn trả ĐÚNG shape FE (xem src/api/* +
mockApi.ts); (2) endpoint chưa có logic → implement theo mô hình service+controller có sẵn; (3) schema: CHỈ thêm
theo §1b, KHÔNG tự ý; mỗi thay đổi = `dotnet ef migrations add` (snapshot cũ hơn model nhưng đã suppress, DB xóa tạo
lại ở PHASE B nên an toàn); (4) `dotnet build backend\src\WebApi\WebApi.csproj` = **0 error** sau mỗi thay đổi;
(5) KHÔNG sửa FE trừ bắt buộc (2 ngoại lệ duy nhất: AdminStatsView bỏ mock chart §1c + thêm field AdminStatsDto —
cả 2 đã được phép). **Phạm vi endpoint KHÔNG đổi so với v3/v4 + bổ sung nhóm 2 (§0.4).**
**ROLLBACK migration (BẮT BUỘC):** nếu `dotnet ef migrations add` gây build error/model conflict → `dotnet ef
migrations remove` NGAY → sửa entity §1b → thêm lại. KHÔNG để migration rác trong commit 1 (check: `dotnet ef
migrations list` chỉ còn các migration mới sạch).

| # | Trang ảnh | Route FE | Endpoint FE gọi (contract thật) | Backend hiện tại | Ưu tiên | Hành động |
|---|---|---|---|---|---|---|
| 1 | 01_landing | `/` | — | OK | — | — |
| 2 | 02_login | `/login` | POST `/auth/login` | OK | — | — |
| 3 | 03_register | `/register` | POST `/auth/register` | OK | — | — |
| 4 | 04_dashboard, 13_ho_so | `/profile` | GET `/progress/me`, `/me/gamification`, `/me/hearts`, `/auth/me`, `/me/streak`, `/me/quests`, `/me/inventory`, `/achievements`, `/premium/status` | 404 (trừ `/auth/me`) | **CAO — A1** | **Implement TẤT CẢ (Nhóm 1 §0.4 — ProfileView + AppHeader gọi thật, bắt buộc)** + seed §2.3 |
| 5 | 05_lo_trinh | `/path` | GET `/concepts/courses` | OK | — | — |
| 6 | 06_lo_trinh_detail, 07_node_hub | `/path/{id}` | GET `/concepts/courses/{id}` | OK (thiếu rating/highlights/testimonials/objectives/outcomes/xpReward → §1b + PHASE B) | TRUNG — A2 | §1b cột metadata Course + PHASE B seed |
| 7 | 08_mo_phong | `/simulations` | GET `/simulations`, `/simulations/{key}`, `/simulations/{key}/schema`, `/simulations/{key}/demo` | 404 (chỉ `/algorithms` list) | **CAO — A1** | **Implement** catalog + detail + schema + demo (hết 25 key §2.2) |
| 8 | 09_mo_phong_detail | `/simulator/sort.bubble` | engine local | OK | — | — |
| 9 | 10_lop_hoc, 11_lop_hoc_detail | `/classes`, `/classes/{id}` | GET/POST `/classes`, `/classes/{id}`, `/classes/{id}/members`, `/assignments`, `/report`, `/curriculum`, `/classes/join-by-code` | 404 (backend `/classrooms`) | **CAO — A1** | **Implement** alias `/classes` → logic Classroom; verify với **12 lớp** |
| 10 | 12_bang_xep_hang | `/leaderboard` | GET `/leaderboard` (week/level/class) | 404 (có `/leaderboard/top`, `/me/rank`) | **CAO — A1** | **Implement** GET `/leaderboard` shape FE (gamification.ts); verify pool **120 SV** |
| 11 | 14_lesson_detail | `/lessons/{id}` | GET `/concepts/lessons/{id}`, POST `/complete`, `/progress` | OK | — | — |
| 12 | 15_exercise | `/exercise/{id}` | GET `/exercises`, `/exercises/{id}`, POST `/submit`, `/practice` | 404 (có `/quizzes`) | **CAO — A1** | **Implement** exercises wrapper Quiz → ExerciseDto |
| 13 | 16_ladder, 17_lab | `/ladder/{lessonId}`, `/ladder/{id}/lab` | GET `/ladder/{lessonId}`, GET `/topics`, GET `/exercises?nodeId&stage`, GET `/me/hearts`, POST `/ladder/{lessonId}/stage/{stage}/pass` | Ladder GET/POST có; topics/exercises?stage/hearts 404 | **CAO — A1** | **Implement** `/topics`, `/exercises?nodeId&stage`, `/me/hearts` |
| 14 | 18_code_runner | `/code/{key}` | POST/GET `/code-runs`, `/code-runs/{id}/trace`, `/exercises/{id}/code-submissions/me` | 404 | **CAO — A1** | **Implement** code-run (PistonApiUrl có sẵn) + my submissions |
| 15 | 19_benchmark | `/benchmark/{k1}/{k2}` | POST `/benchmarks/run` | 404 | THẤP — A2 | **Implement** benchmark run (BenchmarkRunDto); verify **6 cặp** §2.2; chụp KHÔNG embed (Hình 4.10 không tồn tại — §3.2b) |
| 16 | 20_admin_dashboard | `/admin/stats` | GET `/admin/stats` | 404 | **CAO — A1** | **Implement** stats + **thống kê giao dịch §1c** (revenue/orders thật) |
| 17 | 21_admin_users | `/admin/users` | GET `/users?page&pageSize` (PagedResponse), PUT role/status | 404 | **CAO — A1** | **Implement** GET paged + update role/active — verify phân trang **128 user** |
| 18 | 22_admin_content | `/admin/content` | GET `/topics`, `/lessons`, POST/PUT lessons/topics | 404 | **CAO — A1** | **Implement** topics + lessons CRUD shape FE; verify ≥ 20 khóa |
| 19 | 23_admin_settings | `/admin/settings` | GET/PUT `/admin/settings` | **409** | TRUNG — A2 | **Fix** versioning ([ApiVersion] đúng/route rõ) |
| 20 | — | `/favorites` | GET `/favorites` | 409 | THẤP — A2 | **Fix 409** |
| 21 | (không ảnh) — Nhóm 2 §0.4 | `/shop`, `/quests`, `/premium` | GET `/shop/items`, POST `/shop/buy`, PUT `/me/inventory/equip`, POST `/me/quests/{id}/claim`, POST `/premium/upgrade`, POST `/premium/mock-pay` | 404 | TRUNG — A2 | **Implement sau Nhóm 1** (UI FE có thật — ShopView/QuestsView/PremiumView) + seed §2.3 |

**Phân đợt PHASE A (time-box 3h):**
- **A1 (≤2h)** = toàn bộ hàng **CAO** (4, 7, 9, 10, 12, 13, 14, 16, 17, 18) + schema §1b + migration + build 0 error → **checkpoint: probe `after` 0 dòng 404/409/5xx trên nhóm 1**.
- **A2 (≤1h)** = hàng TRUNG/THẤP (6, 15, 19, 20, 21). **Probe Nhóm 2 (hàng 21) chạy RIÊNG sau khi Nhóm 1 pass gate** — nếu A2 chưa xong, hàng 15/20 được phép cắt (không ảnh riêng), hàng 21 seed tối thiểu để ProfileView không rỗng.
- Trễ nữa → áp §0.6 (cắt tầng).

### 1b — SCHEMA BỔ SUNG (CHỈ thêm các mục này — FE contract đòi mà entity hiện thiếu; verify 19/08)

FE gọi `/me/quests`, `/shop/items`, `/shop/buy`, `/me/inventory`, `/me/inventory/equip` nhưng backend KHÔNG có entity
quest/shop/inventory; `Course` KHÔNG có field rating/testimonials. Thêm (mỗi mục = 1 migration, DB xóa tạo lại an toàn):

| Entity / Field mới | Field bắt buộc | Ghi chú |
|---|---|---|
| `Quest` | Id, **QuestKey** (unique), Title, Description, **Period (Daily/Weekly/Monthly)**, ConditionJson, RewardJson (gems/xp), SortOrder, IsActive | Period enum cho quest ngày/tuần/tháng (§2.3); khớp DBML bảng `quests` |
| `UserQuest` | Id, QuestId, UserId, Progress, Status (NotStarted/InProgress/Completed/Claimed), ClaimedAt | FE shape QuestDto: questId/title/description/progress/targetCount/reward{gems,xp}/period |
| `ShopItem` | Id, Name, Description, Slot (avatar/frame/theme/misc), PriceGems, IconUrl, SortOrder | FE ShopItemDto: id/name/description/slot/priceGems/iconUrl |
| `UserInventory` | Id, UserId, ItemId, Equipped (bool), AcquiredAt | FE InventoryItemDto: id/itemId/name/slot/iconUrl/equipped |
| `GemTransaction` | Id, UserId, Amount, Type (Earn/Spend), ReferenceId, CreatedAt | Nguồn gem: quest/shop mua; nếu quá tốn thời gian → ít nhất ghi spend khi mua |
| `CourseReview` (testimonial) | Id, CourseId, UserId, Rating (1-5), Comment, CreatedAt | Lời "đã học xong và đánh giá" (§2.3) — FE CourseDetailView đọc testimonials/rating |
| `Course` thêm cột | Rating (decimal), RatingCount, LearningObjectives (json/text), KeyOutcomes, Highlights, XpReward | `rating`/`ratingCount` tính từ CourseReview hoặc seed trực tiếp; text fields seed sẵn |

Ràng buộc: giữ convention đặt tên có sẵn (Guid Id, private setters, navigation virtual), khóa ngoại chuẩn, không phá
entity đang dùng bởi stream khác. Nếu `dotnet ef migrations` báo model pending → BÌNH THƯỜNG (đã suppress).

### 1c — THỐNG KÊ GIAO DỊCH (admin xem khi mua) — BẮT BUỘC (giữ nguyên v3 §1b — đã verify Order có sẵn)

**Hiện trạng:** `AdminStatsView.vue` có 5 KPI + bar chart 7 ngày DỮ LIỆU MINH HỌA (L66) + donut vai trò minh họa
(L123). Backend `PaymentsController` có POST `/payments/order`, GET `/payments/orders/{orderId}/status`,
POST `/payments/sepay-webhook` — KHÔNG có thống kê. Entity `Order` ĐÃ CÓ: `UserId, PaymentCode, TransactionReference,
Amount (decimal), Status (Pending/Completed/Cancel), CreatedAt, CompletedAt` (+ migration AddOrderExpiresAt) — đủ.

1. **Backend** — mở rộng GET `/admin/stats` (KHÔNG tạo endpoint mới) trả thêm: `totalOrders`, `totalRevenue` (CHỈ
   Completed), `pendingOrders`, `completedOrders`, `cancelledOrders`, `revenueByDay` `[{ date, revenue, orders }]`
   **7 ngày gần nhất** (ngày 0 đơn trả revenue=0/orders=0), `recentOrders` 8-10 đơn `[{ id, userDisplayName, email,
   amount, status, paymentCode, createdAt, completedAt }]`.
2. **FE — sửa tối thiểu (được phép, ghi lý do commit)**: `AdminStatsView.vue` thay bar chart mock bằng `revenueByDay`
   THẬT + thêm 1-2 KPI "Giao dịch"/"Doanh thu" (totalOrders/totalRevenue) + (nếu kịp) danh sách recentOrders;
   `frontend/src/api/admin.ts` thêm field vào `AdminStatsDto` (giữ field cũ). KHÔNG đổi 5 KPI cũ.
3. **Seed (§2.3)** — đơn hàng THẬT.

---

## 2. PHASE B — SEED CHUYÊN NGHIỆP QUY MÔ LỚN + DATA COVERAGE ĐẦY ĐỦ

**CÁCH SEED (SQL Server 2022):** seed CHẠY KHI backend start (Program.cs tự `Migrate()` + seed DbSeeder).
KHÔNG chạy seed script riêng lúc backend đang serve. Muốn seed lại: **stop backend → xóa
`DROP DATABASE VisualizationDSA` trên SQL Server → start lại** (backend tự migrate + seed sạch). Kiểm tra log startup
có dòng seed OK trước khi chụp. Nếu buộc seed script riêng: chỉ chạy khi backend ĐÃ STOP.

### 2.1 — KHÓA HỌC: 3 cũ + 17 mới = 20 (giữ nguyên v4 — dùng hết 25 simulator key)

**Khóa 1-10 — GIỮ (nguồn PR30):** như v4: (1) Cơ bản, (2) Trung cấp, (3) Nâng cao (GIỮ seed cũ), (4) Tìm kiếm &
Độ phức tạp `search.linear/binary`, (5) Sắp xếp cơ bản `sort.bubble/selection/insertion`, (6) Đệ quy & Chia để trị
`sort.merge/quick` + `sorting/recursion.md`, (7) Heap `tree.heap-max/min`, (8) Đồ thị BFS/DFS/Dijkstra
`graph.bfs/dfs/dijkstra`, (9) Tham lam & QHĐ (benchmark) + `sorting/greedy.md` + `sorting/dynamic-programming.md`,
(10) Cây nhị phân & Stack/Queue `tree.bst`, `stack-queue`.

**Khóa 11-20 — MỚI (viết theo style `lessons/b1-b16.md`):** (11) Bảng băm `hash.chaining`, (12) Tìm kiếm nâng cao
`search.jump/interpolation`, (13) AVL `tree.avl`, (14) Sắp xếp nâng cao `sort.heap/radix`, (15) MST `graph.kruskal/prim`,
(16) Bellman-Ford + Topological `graph.bellman-ford/topological`, (17) QHĐ nâng cao `dp.knapsack`,
(18) Two Pointers & Sliding Window, (19) Trie & Union-Find, (20) Ôn tổng hợp & Luyện phỏng vấn (tổng hợp quiz/exercise,
ít viết mới). Khóa 18-20 ưu tiên trung bình/thấp — cắt trước nếu trễ.

Mỗi khóa: 3-6 lessons (contentMd THẬT), **2-3 quizzes/khóa, 15-25 câu MCQ/quiz**, 1 assignment, `sandboxType`/`sandboxConfig`
khớp sandbox FE, đủ field: `rating` (3.8-4.9), `ratingCount` (50-300), `learningObjectives` (3-5), `keyOutcomes` (3-5),
`highlights` (3-4), `testimonials` **4-5 lời** (từ CourseReview §2.3 — SV đã học xong), `author` (GV roster),
`xpReward`, `progressPercent` (theo từng user). **Ladder ≥ 5 stage/lesson.**

### 2.2 — ROSTER (NHÚNG ĐẦY ĐỦ — deterministic 100%, không cần đọc v4)

**Giảng viên (8)** — gán làm `author` khóa học + chủ nhiệm lớp (20 khóa / 8 GV, 12 lớp / 8 GV):

| Tên | Chức danh | Email | Chủ khóa (author) | Chủ nhiệm lớp |
|---|---|---|---|---|
| Nguyễn Minh Trí | TS. — Khoa CNTT, ĐH FPT | trinm@fpt.edu.vn | 1, 4, 11 | SE1801, SE1809 |
| Lê Hoàng Nam | ThS. — Khoa CNTT, ĐH FPT | namlh@fpt.edu.vn | 2, 5, 12 | SE1802, SE1810 |
| Phạm Thu Hà | TS. — Khoa CNTT, ĐH FPT | hapt@fpt.edu.vn | 3, 6, 13 | SE1803, SE1811 |
| Trần Văn Khánh | ThS. — Khoa CNTT, ĐH FPT | khanhtv@fpt.edu.vn | 7, 14, 20 | SE1804, SE1812 |
| Đinh Quang Huy | TS. — Khoa CNTT, ĐH FPT | huydq@fpt.edu.vn | 8, 15 | SE1805 |
| Vũ Ngọc Diệp | ThS. — Khoa CNTT, ĐH FPT | diepvn@fpt.edu.vn | 9, 16 | SE1806 |
| Bùi Xuân Trường | ThS. — Khoa CNTT, ĐH FPT | truongbx@fpt.edu.vn | 10, 17 | SE1807 |
| Ngô Bích Ngân | TS. — Khoa CNTT, ĐH FPT | ngannb@fpt.edu.vn | 18, 19 | SE1808 |

**Sinh viên (a): 24 anchor** (2 SV/lớp × 12 lớp, XP giảm dần toàn cục — leaderboard top, testimonials, ảnh 04/13).

| Rank | Tên | Email | Lớp | XP | Cấp | Streak | Ghi chú |
|---|---|---|---|---|---|---|---|
| 1 | Lê Quốc Bảo | baolqse1801@fpt.edu.vn | SE1801 | 3800 | 10 | 21 | `isPremium: true` — chụp ảnh 04/13 (badge PRO) |
| 2 | Trần Thị Hồng Nhung | nhungtthse1802@fpt.edu.vn | SE1802 | 3550 | 9 | 28 | `isPremium: true` |
| 3 | Phạm Minh Đức | ducpmse1803@fpt.edu.vn | SE1803 | 3200 | 9 | 14 | |
| 4 | Nguyễn Hoàng Anh | anhnhse1804@fpt.edu.vn | SE1804 | 2900 | 8 | 19 | |
| 5 | Vũ Thị Mai Linh | linhvtmse1805@fpt.edu.vn | SE1805 | 2650 | 8 | 15 | `isPremium: true` |
| 6 | Đặng Tuấn Kiệt | kietdtse1806@fpt.edu.vn | SE1806 | 2400 | 7 | 8 | |
| 7 | Bùi Ngọc Ánh | anhbngse1807@fpt.edu.vn | SE1807 | 2150 | 7 | 11 | |
| 8 | Hoàng Văn Sơn | sonhvse1808@fpt.edu.vn | SE1808 | 1950 | 6 | 5 | |
| 9 | Đỗ Thùy Trang | trangdtse1809@fpt.edu.vn | SE1809 | 1750 | 6 | 17 | |
| 10 | Ngô Đức Huy | huyndse1810@fpt.edu.vn | SE1810 | 1550 | 5 | 3 | |
| 11 | Lý Hải Long | longlhse1811@fpt.edu.vn | SE1811 | 1380 | 5 | 9 | |
| 12 | Trịnh Thanh Tâm | tamttse1812@fpt.edu.vn | SE1812 | 1220 | 4 | 2 | |
| 13 | Nguyễn Thị Thu | thunttse1801@fpt.edu.vn | SE1801 | 1080 | 4 | 6 | |
| 14 | Phạm Quốc Toàn | toanpqse1802@fpt.edu.vn | SE1802 | 950 | 4 | 4 | |
| 15 | Trần Ngọc Linh | linhtnse1803@fpt.edu.vn | SE1803 | 840 | 3 | 10 | |
| 16 | Vũ Minh Khoa | khoavmse1804@fpt.edu.vn | SE1804 | 740 | 3 | 7 | |
| 17 | Đặng Thị Lan | landtse1805@fpt.edu.vn | SE1805 | 650 | 3 | 3 | |
| 18 | Hoàng Anh Tuấn | tuanhase1806@fpt.edu.vn | SE1806 | 580 | 2 | 12 | |
| 19 | Bùi Văn Hùng | hungbvse1807@fpt.edu.vn | SE1807 | 510 | 2 | 5 | |
| 20 | Lý Thị Ngọc | ngocltse1808@fpt.edu.vn | SE1808 | 450 | 2 | 8 | |
| 21 | Đỗ Minh Châu | chaudmse1809@fpt.edu.vn | SE1809 | 390 | 2 | 3 | |
| 22 | Ngô Thị Hoa | hoantse1810@fpt.edu.vn | SE1810 | 330 | 1 | 6 | |
| 23 | Trịnh Văn Dũng | dungtvse1811@fpt.edu.vn | SE1811 | 280 | 1 | 2 | |
| 24 | Lê Thị Kim Anh | anhltkse1812@fpt.edu.vn | SE1812 | 230 | 1 | 4 | |

**Sinh viên (b): 96 generated** (rank 25-120, 8 SV/lớp còn lại × 12 lớp) — công thức deterministic, `for i in 25..120`:

```
POOLS (cố định, KHÔNG đổi thứ tự — đảm bảo determinism):
  SURNAMES   = [Nguyễn, Trần, Lê, Phạm, Hoàng, Huỳnh, Phan, Vũ, Võ, Đặng, Bùi, Đỗ, Hồ, Ngô, Dương, Lý, Đinh, Trịnh]  (18)
  MID_MALE   = [Văn, Minh, Hữu, Đức, Quang, Xuân, Công, Thành]                                                        (8)
  MID_FEMALE = [Thị, Ngọc, Thu, Thanh, Kim, Hồng, Diệu, Bích]                                                          (8)
  GIVEN_MALE   = [Nam, Sơn, Hùng, Dũng, Phong, Khôi, Bảo, Vinh, Đạt, Kiên, Hiếu, Long, Việt, Tùng, Quân, Thắng, Duy, Hải, Phúc, An]   (20)
  GIVEN_FEMALE = [Hoa, Lan, Mai, Ngọc, Trang, Huyền, Thảo, Vy, Nhi, Hạnh, Chi, My, Anh, Yến, Quỳnh, Thư, Linh, Hà, Nga, Xuân]         (20)
  CLASSES    = [SE1801..SE1812]  (12, theo thứ tự)

for i = 25..120 (tổng 96 SV):
  gender      = (i % 2 == 0) ? "nam" : "nữ"
  surname     = SURNAMES[i % 18]
  middle      = gender=="nam" ? MID_MALE[i % 8] : MID_FEMALE[i % 8]
  given       = gender=="nam" ? GIVEN_MALE[i % 20] : GIVEN_FEMALE[i % 20]
  fullName    = surname + " " + middle + " " + given
  class       = CLASSES[(i - 1) % 12]
  xp          = max(1, round(220 * exp(-(i - 24) / 35)))       // dốc mượt từ ~210 xuống 1
  level       = tier(xp)   // dùng lại ngưỡng level đã seed cho khóa 1-3
  streak      = (i * 7) % 22                                    // 0-21, xác định
  email       = transliterate(given) + initials(surname, middle) + class.toLowerCase() + "@fpt.edu.vn"
              // vd i=30, nam, surname=Đặng, middle=Xuân, given=Kiên → "kiendxse18xx@fpt.edu.vn"
  isPremium   = false
```

Kết quả: 2 anchor + 8 generated = **10 SV/lớp × 12 lớp = 120 SV**, roster 100% deterministic.
QC sau seed: `COUNT(*) Users Role='Student'` = 120; trùng email = 0.

**`transliterate()` — QUY TẮC CỤ THỂ (BẮT BUỘC — agent KHÔNG tự chế):**
```
transliterate(s) = normalize(s, NFD) → bỏ mọi combining mark (unicode diacritics) → thay 'đ'/'Đ'→'d'/'D'
                   → viết thường → chỉ giữ a-z (bỏ ký tự còn lại).
Ví dụ: Đức→duc · Nguyễn→nguyen · Thị→thi · Xuân→xuan · Đặng→dang · Trịnh→trinh · Quốc→quoc
```
`initials(surname, middle)` = chữ cái đầu mỗi từ, đã transliterate (Đặng Xuân → dx).

**Admin (1)**: Nguyễn Văn Hùng — Quản trị hệ thống — hungnv@fpt.edu.vn.

**Teacher-approval queue (8)** — để ảnh 21_admin_users có trạng thái "chờ phê duyệt" thật:

| Tên | Email | Chức danh xin đăng ký | Trạng thái |
|---|---|---|---|
| Phan Thị Ngọc | ngocpttse@fpt.edu.vn | ThS. Giảng viên CNTT | Chờ duyệt |
| Đào Minh Quân | quandmse@fpt.edu.vn | ThS. Giảng viên CNTT | Chờ duyệt |
| Nguyễn Thị Lan | lannttse@fpt.edu.vn | TS. Giảng viên CNTT | Chờ duyệt |
| Hoàng Văn Bình | binhhvse@fpt.edu.vn | ThS. Giảng viên CNTT | Chờ duyệt |
| Lê Thị Phúc | phucltse@fpt.edu.vn | ThS. Giảng viên CNTT | Chờ duyệt |
| Trương Đình Vũ | vutdse@fpt.edu.vn | TS. Giảng viên CNTT | Chờ duyệt |
| Mai Thị Yến | yenmtse@fpt.edu.vn | ThS. Giảng viên CNTT | Chờ duyệt |
| Đặng Công Sơn | sondcse@fpt.edu.vn | ThS. Giảng viên CNTT | Chờ duyệt |

**Mật khẩu**: ghi `future/real-data-accounts.md` (KHÔNG commit — thêm `.gitignore` ngay), report chỉ ghi đường dẫn.

### 2.3 — DATA COVERAGE (MỞ RỘNG so với v3/v4 — theo yêu cầu: shop-gem, quest ngày/tuần/tháng, người mua, feedback, người học xong đánh giá)

**Checklist seed (mục nào có ✅ = bắt buộc, ◐ = làm nếu kịp):**
**Tầng hóa:** mọi mục ✅ là Tầng 1 (đủ giờ thì làm hết). Khi cắt theo §0.6: Tầng 2 bỏ bớt khóa 18-20 + quiz khóa 11-20
chỉ 10 câu; Tầng 3 giữ TỐI THIỂU — quest daily 3 + weekly 1, shop-gem cho Bảo 3 món, orders ≥ 15, codelab ≥ 3,
CourseReview khóa 1-10. KHÔNG bao giờ cắt: `/me/inventory` 200, `/premium/status`, `/me/quests`, 14 huy hiệu cho Bảo.

- ✅ **Shop-gem (10-12 items)**: avatar frame / avatar / theme / màu tên — giá 10.000-99.000 gems (xem `frontend/src/data/shop_items.json` làm gợi ý shape + i18n `shop.*`). **UserInventory**: Bảo (user chụp ảnh) có 2-3 món, **equip 1 frame + 1 avatar** để AppHeader hiển thị + tab "Túi đồ" trên 13_ho_so không rỗng; vài SV khác 1 món. **GemTransaction**: ghi Earn khi nhận quest/thưởng, Spend khi mua (ít nhất cho Bảo).
- ✅ **Quest DAILY (3-5) + WEEKLY (2-3) + MONTHLY (1-2)** (entity `Quest.Period` §1b): mỗi quest có TargetCount + RewardGems/Xp. Seed `UserQuest` cho Bảo: 2 daily hoàn thành + 1 daily in-progress + 1 weekly in-progress (để `QuestProgressCard` trên ảnh 04 có trạng thái đa dạng thật); vài SV khác 1-2 quest. Claim được (POST claim trả gems/xp, đúng shape mockApi).
- ✅ **Người mua / giao dịch (Order) — 25-40 đơn, rải 14 ngày**: Amount hợp lý (premium 199.000/299.000/499.000đ + đơn gems 10.000-99.000đ); **`PaymentCode` = regex `^FPT2608\d{8}$`** (VD `FPT260890000001`), `TransactionReference` `SEPAY-...` cho Completed; status ~70% Completed (CompletedAt = CreatedAt + 2-10 phút), ~15% Cancel, ~15% Pending (**1-2 Pending HÔM NAY**); user = roster (Bảo mua premium 2 lần — khớp `isPremium:true`); **7 ngày gần nhất phải NHÌN THẬT trên chart**: ngày 2-5 đơn, có ngày 0-1, tổng revenue tăng dần.
- ✅ **Người đã học xong & đánh giá (CourseReview/testimonial)**: mỗi khóa 4-5 review từ SV **đã hoàn thành khóa đó** (khớp progress seed: SV rank cao đã học xong khóa 1-3, thêm vài khóa 4-10). Rating 4-5 sao, comment kiểu SV tốt nghiệp thật (KHÔNG "E2E Student"). `rating`/`ratingCount` của Course = tính từ reviews (hoặc seed khớp).
- ✅ **Feedback (LessonComment)**: 3-6 comment/bài học nổi bật (khóa 1-3) từ SV roster, có 1-2 reply (ParentId) — chỉ seed entity, KHÔNG cần endpoint/ảnh (LessonComment đã có entity).
- ✅ **LessonReview (QA duyệt bài)**: 2-3 bài chờ duyệt + 3-4 đã duyệt (ReviewerAdmin = Hùng/GV) — dữ liệu quản trị thật, không ảnh riêng.
- ✅ **14 huy hiệu (Badge)** — gán tự động theo điều kiện từ XP/streak/progress đã seed (KHÔNG liệt kê tay từng SV):

  | # | Tên huy hiệu | Điều kiện | Độ hiếm |
  |---|---|---|---|
  | 1 | Người mới bắt đầu | Hoàn thành bài học đầu tiên | Phổ biến |
  | 2 | Chuỗi 3 ngày | Streak ≥ 3 ngày liên tiếp | Phổ biến |
  | 3 | Chuỗi 7 ngày | Streak ≥ 7 ngày liên tiếp | Không phổ biến |
  | 4 | Chuỗi 14 ngày | Streak ≥ 14 ngày liên tiếp | Hiếm |
  | 5 | Chuỗi 30 ngày | Streak ≥ 30 ngày liên tiếp | Cực hiếm |
  | 6 | Hoàn thành khóa đầu tiên | Hoàn thành 1 khóa học | Phổ biến |
  | 7 | Học viên chăm chỉ | Hoàn thành 5 khóa học | Không phổ biến |
  | 8 | Học viên xuất sắc | Hoàn thành 10 khóa học | Hiếm |
  | 9 | Bậc thầy thuật toán | Hoàn thành cả 20 khóa học | Huyền thoại |
  | 10 | Điểm tuyệt đối | Đạt 100% điểm 1 quiz | Không phổ biến |
  | 11 | Vượt bậc thang | Hoàn thành 5/5 stage ladder của 1 lesson | Không phổ biến |
  | 12 | Lập trình viên tích cực | Nộp ≥ 10 code submission | Hiếm |
  | 13 | Top 3 bảng xếp hạng | Nằm trong top 3 leaderboard tuần | Hiếm |
  | 14 | Top 10 bảng xếp hạng | Nằm trong top 10 leaderboard tuần | Không phổ biến |
- ✅ **12 lớp SE1801-1812**: 10 SV/lớp (2 anchor + 8 generated), chủ nhiệm theo bảng GV, mã mời cố định, assignment gán, tiến độ + điểm hợp lý, 3-4 enrollment Left/Kicked rải vài lớp.
- ✅ **25 simulator key** (bảng v4: sort.*, search.*, graph.*, tree.*, stack-queue, hash.chaining, dp.knapsack) — mỗi key ≥ 1 khóa tham chiếu.
- ✅ **Benchmark 6 cặp**: (bubble vs quick), (linear vs binary), (bfs vs dfs), (dijkstra vs bellman-ford), (kruskal vs prim), (merge vs heap).
- ✅ **Settings đầy đủ** (maxHearts, streakFreezeDuration, xpMultiplier, maintenanceMode, domain email...).
- ✅ **Codelab 6-8 + TestCase + Hint + Template** (entity `Codelab`/`CodelabTestCase`/`CodelabHint`/`CodelabTemplate` CÓ SẴN nhưng **CHƯA được seed** — verify 19/08, đây là lỗ hổng lab/testcase): mỗi codelab: Title, Description, **InitialCode (code chạy được)**, Constraints, Examples, Tags, MaxRuntimeMs, MaxMemoryBytes, AllowedLanguages + **3-5 TestCase** (Input/ExpectedOutput/IsHidden/ScoreWeight, ≥1 hidden) + **1-2 Hint** + Template đủ ngôn ngữ (csharp/python/java/js). Gán vào lesson/ladder code stage. QC: mỗi codelab chạy thử 1 lần qua Piston API → pass (nếu Piston offline, verify qua submit shape, ghi report). **GIỚI HẠN QC codelab ≤ 20 phút**: Piston chậm → chỉ chạy thử **2 codelab đại diện** + verify phần còn lại bằng shape (đừng để codelab nuốt time-box Phase B).
   **Piston OFFLINE hoàn toàn → "shape-only QC" PHẢI thỏa CẢ 3**: (1) `InitialCode` chạy được khi syntax-check local
   (dotnet/python/node khởi chạy 0 lỗi), (2) TestCase `Input`/`ExpectedOutput` khớp logic bài (đọc code, đối chiếu),
   (3) Template đủ 4 ngôn ngữ csharp/python/java/javascript. Ghi report `Piston offline — shape-only QC`.
- ✅ **Quiz đủ cho 20 khóa**: giữ 13 quiz có sẵn + 39 lesson quiz (SeedQuizzesAsync/SeedCoursesAsync) + **thêm quiz cho khóa 11-20** (2-3/khóa; **khóa 1-10 giữ 15-25 câu, khóa 11-20 tối thiểu 10-15 câu/quiz** — giảm tải Phase B, vẫn đủ shape ExerciseDto FE; **khóa 20 REUSE quiz khóa 1-10** + ít câu mới). Đảm bảo màn 15_exercise có bài tập đầy đủ ở mọi khóa.
- ✅ **Learning path Grokking + node đủ** (GapF9LearningPathHearts migration có sẵn — seed LearningPath/Node/UserNodeProgress/NodeSession cho /path/{id} + ladder đẹp, cả 20 khóa). Hearts **10/10** user mới.
- ◐ **Favorite** (sửa 409 ở dòng 20 PHASE A) — seed 2-4 favorite của Bảo (khóa/lesson đã xem) cho tab yêu thích không rỗng. ◐ **LessonNote** — 1-2 ghi chú của Bảo (entity có sẵn).
- ✅ **Số liệu admin phản ánh quy mô thật**: 128 user (120 SV + 8 GV), 20 khóa, quiz/submissions/orders — stats KHÔNG số giả.

### 2.5 — DATA CONSISTENCY VERIFY (BẮT BUỘC — chạy sau seed, trước CHECKPOINT COMMIT 2)

Script/query sau PHẢI PASS hết (ghi kết quả vào report; fail → sửa seed rồi seed lại — KHÔNG commit 2 khi chưa PASS):

```sql
-- (1) Leaderboard khớp roster: top 24 theo TotalXp giảm dần = đúng 24 anchor (§2.2)
SELECT TOP 24 Username, TotalXp FROM Users WHERE Role='student' ORDER BY TotalXp DESC;
-- (2) CourseReview hợp lệ: mọi review của user đã hoàn thành khóa đó (progress 100%)
SELECT COUNT(*) FROM CourseReviews r
LEFT JOIN (SELECT UserId, CourseId FROM UserProgress WHERE PercentComplete = 100) p
  ON r.UserId = p.UserId AND r.CourseId = p.CourseId
WHERE p.UserId IS NULL;                       -- mong đợi = 0
-- (3) Ví gem cân bằng: mọi user có SUM(GemTransaction) >= 0 và khớp Users.Gems
SELECT UserId, SUM(CASE WHEN Type='earn' THEN Amount ELSE -Amount END) AS balance
FROM GemTransactions GROUP BY UserId HAVING SUM(CASE WHEN Type='earn' THEN Amount ELSE -Amount END) < 0;
                                                                          -- mong đợi = 0 dòng
-- (4) Đơn mua Premium: Order Completed kiểu premium → user phải isPremium = true
SELECT o.Id FROM Orders o JOIN Users u ON u.Id = o.UserId
WHERE o.Status = 'completed' AND o.Amount >= 199000 AND u.IsPremium = false;   -- mong đợi = 0 dòng
-- (5) Trùng email / thiếu vai trò
SELECT Email, COUNT(*) FROM Users GROUP BY Email HAVING COUNT(*) > 1;           -- mong đợi = 0 dòng
SELECT COUNT(*) FROM Users WHERE Role NOT IN ('student','teacher','pending_teacher','admin'); -- = 0
```

Ngoài ra: `COUNT(*) Users` = 128 (120 SV + 8 GV + 0 chờ duyệt nếu chờ duyệt vẫn là user) — ghi đúng con số thực tế.

### 2.4 — DOC & DB DIAGRAM SYNC (BẮT BUỘC — mọi entity/cột mới §1b PHẢI khớp docs + sơ đồ)

Schema thật trong DB phải khớp 3 nơi: docs + ERD (dbdiagram) + docx entity tables. Nếu lệch = lỗ hổng khi chấm.
**Gắn phase:** các mục đánh dấu **(A)** làm ngay trong PHASE A (≤1h), **(B)** làm trong PHASE B, **(D)** làm trong PHASE D.
Không có nhãn = chỉ rà soát, không sửa.

1. **(A · ~10 phút)** **DBML (nguồn dbdiagram.io — BẢN CHUẨN): `tailieu/diagrams/dsa-visual-schema.dbml`** — ĐÃ viết mới theo ĐÚNG
   schema backend đang chạy (**55 bảng / 9 nhóm** — đã verify: mọi ref hợp lệ): `users`(+gems, metadata §1b), `course_modules`, `module_items`,
   `lessons`, `quizzes`+`quiz_questions`+`quiz_attempts`, `codelabs`+`codelab_test_cases/hints/templates/submissions`,
   `classrooms`(+enrollments/lessons/modules/items/overrides/quizzes/attempts/announcements),
   `learning_paths`(+nodes/sessions/user_node_progress/stage_progress/user_lesson_progress), `badges`+`user_badges`,
   `quests`(period Daily/Weekly/Monthly)+`user_quests`, `shop_items`+`user_inventory`+`gem_transactions`, `orders`,
   `notifications`, `course_reviews`, `lesson_comments`+`lesson_reviews`, `theory_articles`, `semantic_concept_nodes`.
   KHÔNG còn bảng schema cũ (classes/achievements/code_runs/exercises/content_feedback/bug_reports...).
   Agent PHẢI đối chiếu entity code với DBML này — tên cột/kiểu lệch thì sửa DBML (không sửa ngược entity code).
2. **(D · ~15 phút)** **ERD trong docx (Hình 4.25)**: import `dsa-visual-schema.dbml` vào dbdiagram.io → export PNG (cùng khung hình cũ) →
   thay ảnh Hình 4.25 trong Phase D (ghi rõ trong §4). **Vì schema đổi toàn bộ, ảnh ERD cũ + Bảng 4.x entity tables
   cũ (32 bảng) PHẢI thay bằng bộ mới (55 bảng).**
3. **(B · ~20 phút)** **`source/VisualizationDSA/plan/architecture/BUSINESS-DATABASE-GUIDE.md`**: cập nhật "40 Table" → **55 Table** +
   thay/sửa mô tả các nhóm bảng theo schema mới (Class→Classroom, Achievement→Badge, CodeRun→Codelab,
   Exercise→Quiz, thêm Quest/Shop/Order/CourseReview...).
4. **(D · ~1.5h)** **Docx Bảng 4.x entity tables + Hình 4.3.2.x greybox**: **THAY bộ cũ bằng 55 bảng entity mới** (format có sẵn).
   **CHỈ chạy phần chèn riêng (pattern P1 trong `word_format_run.py`) — KHÔNG chạy `--steps=P1` trần (sẽ đụng toàn bộ docx).**
5. **(A · ~15 phút)** **`docs/API_REFERENCE.md`**: thêm endpoint mới (quests/shop/inventory/premium + admin/stats mở rộng) — contract khớp code.
6. **KHÔNG sửa số caption ảnh 4.1-4.24** (bất biến).
7. **(D · ~20 phút)** **Nhãn ảnh con (a)/(b) cho phần 4.2.3.x**: các mục 4.2.3.1 "Giao diện Trang chủ" (và mọi mục 4.2.3.x khác
   có NHIỀU ảnh trong 1 mục con — 4.2.3.2 Đăng nhập/Đăng ký, 4.2.3.14 Dashboard, v.v.) hiện chỉ có 1 caption
   **KHÔNG có nhãn (a)/(b)**. Rà soát toàn docx: mục nào chèn ≥2 ảnh thì đánh dấu **Hình 4.X: ... (a)**, **(b)**...
   và caption mục con thêm dòng chú thích tương ứng. Giữ nguyên số caption gốc.
8. **(D · ~40 phút)** **Hình 4.3.2.x "Ảnh chi tiết thực thể" = ẢNH BẢNG THẬT, KHÔNG phải hộp xám**: hiện tại các Hình 4.3.2.x
   (vd 4.3.2.1 Ảnh chi tiết thực thể User) là **hộp xám placeholder**. Phải GEN LẠI ẢNH cho ĐỦ 55 bảng:
   mỗi entity một hình bảng chi tiết thực thể (render dạng bảng thuộc tính: cột, kiểu, PK/FK, ghi chú — khớp
   `dsa-visual-schema.dbml`), KHÔNG trùng với Bảng 4.x entity tables (bảng đó giữ dạng bảng chữ, còn hình này
   là ảnh bảng/mini-ERD 1 bảng). Gộp nhiều entity cùng nhóm nếu docx gộp sẵn — theo đúng cấu trúc chương 3.
9. **(B · ~15 phút)** **API key / tích hợp ngoài PHẢI xuất hiện trong docx**: app có 4 tích hợp dùng key/URL ngoài mà tài liệu
   CHƯA nhắc: (1) **Piston API** `https://emkc.org/api/v2/piston` (chấm bài code — PistonCodeJudgeService),
   (2) **SePay webhook** `ApiKey` (xác thực webhook thanh toán), (3) **AI API** `OpenRouter`/`AimlApi`
   (config `SET_VIA_ENV_*`, model `openai/gpt-5-5` — hỗ trợ AI/gợi ý), (4) **JWT secret** (security).
   Bổ sung vào: mục kiến trúc/thiết kế hệ thống + security + phần tham chiếu.
10. **(D · ~20 phút)** **TÀI LIỆU THAM KHẢO (đầu trang 465 docx — 18 mục)**: rà soát đúng/sai từng mục (kiểm tra năm, nhà xuất
    bản, URL còn hoạt động). Bổ sung mục còn thiếu nhưng ĐƯỢC dùng thực tế trong đề tài: **Piston API docs**,
    **SePay docs** (sepay.vn), **OpenRouter docs**, **dbdiagram.io / DBML syntax**, **EF Core Migrations
    (Microsoft Learn)**, **Ollama + Qwen2.5-VL** (nếu dùng trong đánh giá UI), **VisuAlgo** (đã có)...
    Mỗi mục bổ sung phải được trỏ tới chỗ dùng trong chương 2/3 (không thêm mục "trang trí").

**Tổng doc sync §2.4 ≈ 3.5-4h** (A: 25' + B: 35' + D: ~2h35 — vừa khớp time-box mới D=2.5h). Nếu trễ, cắt theo thứ tự: 10 → 7 → 9, sau đó mới 8/4.

---

## 3. PHASE C — CHỤP 23 ẢNH (100% REAL, PHẠM VI KHÔNG ĐỔI — CẤM mock data, chỉ mock `/auth/refresh`)

### 3.1 — Phiên chụp (giữ v4 — 1 login/role, thứ tự cố định)

Setup: backend :5055 + FE :5174. Playwright headless chromium 1440×900, theme dark. Kill port 5174 cũ; RAM thấp →
1 worker, không song song.

**Ollama — OPTIONAL enhancement, KHÔNG phải blocker**: `ollama list` (qwen2.5vl:3b cài sẵn — verify 18/08, 3.2GB).
CLI treo >60s → `ollama serve` nền rồi thử lại. Model thiếu → `ollama pull qwen2.5vl:3b` NỀN trong PHASE A.
**QC CHÍNH = DOM assertions §3.2** (verify DOM có data, không error/skeleton/empty + console 0 lỗi 404) — đáng tin
hơn ảnh AI. Ollama chỉ DÙNG THÊM để mô tả ảnh (flag §3.3); nếu lỗi giữa chừng/không chạy → fallback QC text + ghi
`QC fallback: text` — KHÔNG chặn, KHÔNG quay lại sửa Ollama.

```
PHIÊN STUDENT (1 lần login Lê Quốc Bảo, điều hướng SPA bằng click):
  05 /path → 06 /path/{id1} → 07 scroll lessons → 14 /lessons/{lessonId} → 08 /simulations → 09 /simulator/sort.bubble →
  04 /profile (top) → 13 /profile scroll hồ sơ → 10 /classes → 11 /classes/{id} → 12 /leaderboard → 15 /exercise/{id} →
  16 /ladder/{id} → 17 /ladder/{id}/lab → 18 /code/{key} → 19 /benchmark/{k1}/{k2}
PHIÊN ADMIN (1 lần login Nguyễn Văn Hùng):
  20_admin_dashboard → 21_admin_users → 22_admin_content → 23_admin_settings
PHIÊN PUBLIC: 01 / → 02 /login → 03 /register
```
- Giữ session qua full reload: `page.route('**/api/v1/auth/refresh', ...)` → 200 `{ accessToken: '<jwt giả>',
  expiresIn: 3600 }` (chỉ endpoint này — mọi DATA khác gọi backend thật). Bị đá /login → báo report.
- 429: chờ 60s → thử lại, tối đa 3 lần; lần 4 vẫn 429 → tắt rate limiter Dev-only, rebuild, restart, ghi report (§0.3).
- Simulator (09): bấm "Bước tới" 2 lần. Mỗi ảnh: wait networkidle + 1.5-2.5s; verify DOM (có data, không
  error/skeleton/empty) + **console KHÔNG có 404** (đặc biệt `/me/inventory`, `/me/quests`, `/premium/status` — nếu
  còn 404 → QUAY LẠI PHASE A, không chấp nhận).

### 3.2 — Bảng acceptance (số liệu theo quy mô v5)

| File | Route | Login | Acceptance (DOM phải có) |
|---|---|---|---|
| 01_landing.png | `/` | — | Hero + 3 mô phỏng + nav đủ (Sandbox) |
| 02_login.png | `/login` | — | Form login |
| 03_register.png | `/register` | — | Form đăng ký + chọn vai trò |
| 04_dashboard.png | `/profile` (top) | Bảo | KPI thật: XP/level/streak/hearts + badge PRO + **QuestProgressCard có quest thật** |
| 05_lo_trinh.png | `/path` | Bảo | **20 card** lộ trình — KHÔNG Offline Mode |
| 06_lo_trinh_detail.png | `/path/{id1}` | Bảo | Title + lessons + rating/4-5 testimonials/objectives/author |
| 07_node_hub.png | `/path/{id1}` scroll | Bảo | Module/lesson list thật |
| 08_mo_phong.png | `/simulations` | Bảo | Catalog **25 item** + category — nút mở chạy sandbox |
| 09_mo_phong_detail.png | `/simulator/sort.bubble` | Bảo | Simulator + mảng + animation |
| 10_lop_hoc.png | `/classes` | Bảo | List **12 lớp** (tên lớp, GV, mã mời) |
| 11_lop_hoc_detail.png | `/classes/{id}` | Bảo | Members (10 SV/lớp) + assignments + tiến độ |
| 12_bang_xep_hang.png | `/leaderboard` | Bảo | Rank top thật (XP khớp roster), pool **120 SV** |
| 13_ho_so.png | `/profile` (scroll hồ sơ) | Bảo | Avatar + thông tin + achievements (14 loại) + **tab Túi đồ có frame/avatar equipped** |
| 14_lesson_detail.png | `/lessons/{id}` | Bảo | Nội dung lesson markdown thật |
| 15_exercise.png | `/exercise/{id}` | Bảo | Bài tập thật (câu hỏi + options, 15-25 câu) |
| 16_ladder.png | `/ladder/{id}` | Bảo | ≥5 bậc Quiz/Lab/Code + trạng thái + hearts |
| 17_lab.png | `/ladder/{id}/lab` | Bảo | Interactive lab (input + swaps + kết quả) |
| 18_code_runner.png | `/code/{key}` | Bảo | Editor + console + nút submit (chạy thử 1 lần) |
| 19_benchmark.png | `/benchmark/{k1}/{k2}` | Bảo | Bảng so sánh thật (1 trong 6 cặp) |
| 20_admin_dashboard.png | `/admin/stats` | Hùng | Thống kê thật (128 user/20 khóa) **+ KPI Giao dịch/Doanh thu + bar chart 7 ngày data THẬT từ revenueByDay (KHÔNG minh họa)** |
| 21_admin_users.png | `/admin/users` | Hùng | Bảng phân trang **128 user** + 8 chờ duyệt, control phân trang rõ |
| 22_admin_content.png | `/admin/content` | Hùng | Topics/lessons thật (**20 khóa**) |
| 23_admin_settings.png | `/admin/settings` | Hùng | Settings form giá trị thật |

### 3.2b — ÁNH XẠ FILE → HÌNH 4.X (KHÔNG ĐỔI so với v3/v4 — 23 ảnh, cùng caption)

Giữ NGUYÊN bảng v4 (§3.2b): 01_landing→4.1, 02_login→4.2, 14_lesson_detail→4.3, 09_mo_phong_detail→4.4,
15_exercise→4.5, 05_lo_trinh→4.6, 16_ladder→4.7, 17_lab→4.8, 18_code_runner→4.9, **19_benchmark→4.10 (⚠️ caption
KHÔNG tồn tại trong docx — verify 18/08 — chụp nhưng KHÔNG embed, đừng tự thêm)**, 12_bang_xep_hang→4.11,
13_ho_so→4.12, 03_register→4.14, 04_dashboard→4.15, 06_lo_trinh_detail→4.16, 07_node_hub→4.17, 08_mo_phong→4.18,
10_lop_hoc→4.19, 11_lop_hoc_detail→4.20, **20_admin_dashboard→4.21, 21_admin_users→4.22, 22_admin_content→4.23,
23_admin_settings→4.24** (tên admin đã đổi 20-23, đồng bộ `word_format_run.py`). 4.13 sitemap, 4.25 ERD — không phải
screenshot. Số CAPTION bất biến; số TÊN FILE 20-23 đổi cho unique.
**VALIDATION bắt buộc trong Phase D**: chạy lệnh bên dưới PHẢI in `PASS` — nếu fail, agent đã embed nhầm 4.10 hoặc
chèn thừa ảnh thì sửa NGAY trước khi commit 3:
```powershell
python -c "from docx import Document; d=Document(r'tailieu\BaoCaoDoAn.docx'); caps=[p.text.strip() for p in d.paragraphs if 'Hình 4.1' in p.text or p.text.startswith('Hình 4.')]; bad=[c for c in caps if 'Hình 4.10' in c]; imgs=[r for r in d.part.rels.values() if 'image' in r.reltype]; print('4.10 in captions:', bad or 'NONE'); print('embedded images:', len(imgs)); print('PASS' if not bad else 'FAIL')"
```

### 3.3 — Rà ảnh bằng AI (giữ v4) — qwen2.5vl:3b mô tả TỪNG ảnh + flag
`[OK]/[PLACEHOLDER]/[MOCK]/[EMPTY]/[ERROR]`. Flag ≠ OK → chụp lại/fix data. (Vòng lặp: trang lỗi → quay PHASE A/B —
KHÔNG chấp nhận ảnh lỗi, KHÔNG mock.) Ollama lỗi giữa chừng → fallback QC snapshot text + ghi "QC fallback: text" —
KHÔNG chặn.

---

## 4. PHASE D — RE-EMBED DOCX + VERIFY + COMMIT

```powershell
cd D:\FPT\neww\tailieu
Copy-Item BaoCaoDoAn.docx BaoCaoDoAn.backup-<giờ>.docx
python word_format_run.py --steps=M                              # ⚠️ CHỈ --steps=M — CẤM chạy trần (P3/P1/UC/IMG sẽ đổi lại docx)
                                                                 # M = mode "embed screenshots only": chèn 23 ảnh .png đúng tên §3.2b
                                                                 #     vào đúng caption Hình 4.1-4.24 (bỏ qua 4.10) — KHÔNG đụng nội dung khác
python tailieu/docx_verify_after.py                              # hoặc docx_final_verify.py
python tailieu/verify_consistency.py                             # nếu tồn tại
# Mong đợi: Bảng 6.2 (Engine 646/646, E2E 156/156, UX 5/5/0) + Bảng 6.6 (NV1..NV5) giữ nguyên;
# ảnh = 23 màn + captions Hình 4.1–4.24 (KHÔNG có caption Hình 4.10 — trạng thái cũ, đừng tự thêm).
```

**GATES (bắt buộc, chạy cuối, lưu output):**
```
cd D:\FPT\neww\frontend && npm run build                                            # PASS
cd D:\FPT\neww\frontend && npx vitest run                                           # 646/646
cd D:\FPT\neww\frontend && npx playwright test                                       # 156/156 (mockApi — không vỡ)
cd D:\FPT\neww\source\VisualizationDSA && dotnet build backend\src\WebApi\WebApi.csproj    # 0 error
# Verify ảnh — ĐÚNG 23 file theo tên §3.2b (bỏ qua ~12 ảnh cũ tên khác trong dir):
python -c "import os; names=['01_landing','02_login','03_register','04_dashboard','05_lo_trinh','06_lo_trinh_detail','07_node_hub','08_mo_phong','09_mo_phong_detail','10_lop_hoc','11_lop_hoc_detail','12_bang_xep_hang','13_ho_so','14_lesson_detail','15_exercise','16_ladder','17_lab','18_code_runner','19_benchmark','20_admin_dashboard','21_admin_users','22_admin_content','23_admin_settings']; miss=[n for n in names if not os.path.exists(rf'tailieu/screenshots/{n}.png')]; small=[n for n in names if os.path.exists(rf'tailieu/screenshots/{n}.png') and os.path.getsize(rf'tailieu/screenshots/{n}.png')<20000]; print('MISSING:',miss); print('SMALL(<20KB):',small); print('PASS' if not miss and not small else 'FAIL')"
git diff --check
```

**Branch & commit**: tạo **`feature/real-data-100`** từ origin/dev (`git checkout -b feature/real-data-100 origin/dev`),
làm hết trên đó, cuối cùng merge vào dev + `git push origin HEAD:dev`. CHỈ commit paths của mình — KHÔNG commit hộ,
KHÔNG `git add -A`.

**COMMIT THEO CHECKPOINT:**
- commit 1 — SAU PHASE A đạt Mốc 1: `source/VisualizationDSA/backend/**` (API endpoints + schema §1b)
- commit 2 — SAU PHASE B đạt Mốc 2: `source/VisualizationDSA/backend/**` (seed 20 khóa/120 SV + data coverage §2.3) + `tailieu/diagrams/dsa-visual-schema.dbml` + `BUSINESS-DATABASE-GUIDE.md` (sync §2.4)
- commit 3 — SAU PHASE D: `tailieu/screenshots/*.png` + `tailieu/BaoCaoDoAn.docx` (+ `tailieu/test_results.json` nếu số đổi)
- commit 4: FE nếu có sửa (AdminStatsView §1c hoặc ngoại lệ khác — kèm lý do trong message)

---

## 5. BÁO CÁO CUỐI

```markdown
STATUS: READY / PARTIAL / BLOCKED
BRANCH / HEAD: (sha sau push, phải là origin/dev)
TIMING: PHASE A started→ended HH:MM | PHASE B HH:MM | PHASE C HH:MM | PHASE D HH:MM (mỗi phase 1 dòng, để audit)
MILESTONES: Mốc 1-4 đúng time-box? (A ≤3h / B ≤2.5h / C ≤3h / D ≤2.5h) — trễ bao nhiêu, cắt gì (§0.6 tầng nào)
CHECKPOINT A: 404/409/5xx trước = X → sau = Y (dòng nào còn lỗi) | console 404 còn: [...]
PHASE B: 20 khóa? (Y/N) | 120 SV? (Y/N) | 12 lớp? (Y/N) | 14 huy hiệu? (Y/N) | 25 sim key? (Y/N)
  DATA COVERAGE: shop-gem? (Y/N) | quest D/W/M? (Y/N) | Order/buyer? (Y/N) | CourseReview/testimonial? (Y/N) |
  LessonComment? (Y/N) | LessonReview? (Y/N) | Codelab+TestCase? (Y/N) | accounts: future/real-data-accounts.md
DOC SYNC (§2.4): DBML khớp entity code? (Y/N) | ERD Hình 4.25 đã thay? (Y/N) | BUSINESS-DATABASE-GUIDE cập nhật? (Y/N) |
  docx Bảng 4.x entity mới? (Y/N) | API_REFERENCE cập nhật? (Y/N)
PHASE C: 23/23 REAL? | qwen2.5vl flags OK=? | console 0 lỗi? (Y/N) | ảnh chụp lại: [...]
PHASE D: docx re-embed OK (ảnh count, Bảng 6.2/6.6) | gates: build/vitest/E2E/dotnet/PNG-verify
OPTIONAL còn lại: [...]
KNOWN LIMITATIONS / PM DECISION NEEDED:
```

---

## 6. QUY TẮC / CẢNH BÁO

- KHÔNG đụng file staged/untracked của stream khác (`git status --short` đầu buổi rất bẩn — chỉ đụng paths §4).
  KHÔNG `git add -A`.
- KHÔNG sửa FE trừ 2 ngoại lệ được phép (AdminStatsView §1c + field AdminStatsDto); mọi sửa FE giữ mockApi shape
  (646/156 không vỡ).
- Backend test project (xUnit) 211 lỗi compile từ WIP stream khác — KHÔNG thuộc phạm vi, không cố fix hết.
- Schema: CHỈ thêm theo §1b — KHÔNG tự ý thêm entity/field khác (đã verify FE contract cần gì).
- `dotnet ef migrations` báo pending changes là BÌNH THƯỜNG (đã suppress); DB xóa tạo lại PHASE B nên migration mới áp sạch.
- KHÔNG đổi Bảng 6.6 (số giây UX NV1-5 — đo trước, không liên quan task này).
- Ảnh 01/02/03/05/06/07/09 đã là bản thật — VẪN chụp lại (data mới khác hẳn, ảnh cũ không dùng lại).
- Tên file ảnh admin ĐÃ đổi 14-17_admin_* → 20-23_admin_* (đồng bộ `word_format_run.py` + §3.2b) — ĐỪNG quay lại tên
  cũ; ảnh cũ tên khác trong `tailieu/screenshots/` là rác lịch sử — bỏ qua, KHÔNG xóa.
- **KHÔNG seed notifications/forum/certificates làm ẢNH** — hệ thống chưa có UI phù hợp (đã chốt §0.5). NHƯNG dữ liệu
  feedback SẴN CÓ (LessonComment/LessonReview) thì seed — không cần endpoint/ảnh mới.
- **3 lỗi fact v3/v4 KHÔNG được lặp lại** (§0 đầu file): FE CÓ quests/shop/inventory/premium thật; Order CÓ sẵn;
  AdminStatsView mock chart PHẢI thay.