# SEED-PROD — Đợt seed hoạt động người dùng demo + bỏ chặn domain đăng ký

**Ngày:** 13/08/2026 · **Nhánh:** `feature/seed-prod` (base `origin/dev`) · **Trạng thái:** DONE — đã chạy thật lên SQL Server docker (`neww-sqlserver-1`, DB `DsaVisual`)

## Mục tiêu

1. **Seed dữ liệu hoạt động người dùng demo** (`SeedDemoActivity` — chạy SAU `SeedSettingsAsync`, idempotent): 8 student `@university.edu.vn` + toàn bộ "câu chuyện" hoạt động 1 tháng — 10 achievements + UserAchievements, UserProgress/UserNodeProgress, ExerciseSubmissions, UserQuests/GemTransactions/UserInventory/Favorites/ContentFeedback, 2 lớp học + ClassMembers/ClassAssignments, misc (CodeSubmissions/BugReports/LessonNotes — chỉ khi bảng trống).
2. **Bỏ chặn domain đăng ký** (quyết định user 13/08/2026): setting `allowed.email.domains` KHÔNG còn được seed (xóa khỏi `SeedData.Settings`); bước `SeedCleanupSettingsAsync` (bước đầu của SeedDemoActivity) tự xóa setting cũ còn sót trong DB → **mọi email hợp lệ (kể cả `@gmail.com`) đăng ký được** — mã check AuthService.cs:59-70 chỉ chạy khi setting không rỗng.

## Cách chạy

```powershell
# Từ backend/ — bắt buộc có DSA__Jwt__Secret + connection string (xem backend/src/DsaVisual.Application/Persistence/Seed/README.md)
$env:DSA__Jwt__Secret = "dev-secret-32-ky-tu-toi-thieu-0123456789abcdef"
$env:ConnectionStrings__Default = "Server=localhost;Database=DsaVisual;User Id=sa;Password=DsaVisual@Dev123;TrustServerCertificate=True"
dotnet run --project src/DsaVisual.Api -- --seed
```

- `--seed` → `Migrate()` → `SeedRunner.SeedAsync(db)` → thoát (KHÔNG seed khi chạy bình thường).
- Chạy **2 lần**: lần 2 xác nhận idempotent — **0 bản ghi mới** (log `Seed: ... bỏ qua (đã tồn tại)`); số dòng cuối cùng khớp bảng bên dưới (không trùng lặp user/lớp/activity).

## Bảng đếm thật (DB docker — verify 13/08/2026)

Nguồn: `docker exec neww-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "***" -C -d DsaVisual -Q "SELECT COUNT(*) ..."` (SQL đầy đủ trong Seed README §Verify sau seed).

| Bảng | Số dòng | Bảng | Số dòng |
|---|---|---|---|
| Users | **17** | UserQuests | 178 |
| Achievements | 10 | GemTransactions | 141 |
| UserAchievements | 33 | UserInventory | 7 |
| UserProgress | 31 | Favorites | 27 |
| UserNodeProgress | 33 | ContentFeedback | 10 |
| ExerciseSubmissions | 53 | Classes | 2 |
| CodeSubmissions | 5 | ClassMembers | 12 |
| BugReports | 3 | ClassAssignments | 7 |
| LessonNotes | 3 | Settings `allowed.email.domains` | **0** |

> **Users = 17, không phải 14** (con số 14 trong task SEED-6 là lúc đếm TRƯỚC API smoke): 14 = 3 user seed gốc (admin/teacher/student) + 3 user rác smoke giữ nguyên + 8 student demo `@university.edu.vn` (id 2004-2011); **+3 user do chính API smoke tạo ra**: `test-gmail-433791@gmail.com` (id2012 — register @gmail.com) + `gv.smoke.20260813@university.edu.vn` (id2013) + `gv.smoke.20260813.run2@university.edu.vn` (id2014) — 2 tài khoản TeacherPending từ smoke đăng ký giảng viên. Các bảng còn lại khớp 100% số liệu task.

## An toàn (không đụng dữ liệu có sẵn)

- **3 user rác smoke giữ nguyên** (còn đủ, IsActive=1): `qa.debug3@university.edu.vn` (id1002), `univ123@university.edu.vn` (id1003), `smoke2fa1786559250@university.edu.vn` (id1004).
- **student@demo.local premium giữ nguyên**: subscription `OrderRef DSV3T1` (PlanId `1m`), `PremiumUntil 2026-09-13 03:09:35`, `HeartsMax 30`, Hearts 30.
- **1 user test register @gmail.com** tạo trong API smoke: `test-gmail-433791@gmail.com` (id2012) — không premium, Gems/XP 0.
- Settings hiện tại = 8 key (đã bớt `allowed.email.domains`): `site.name`, `password.policy.minLength`, `upload.maxSizeMb`, `simulation.maxArraySize`, `simulation.maxGraphVertices`, `auth.maxLoginAttempts`, `auth.lockoutMinutes`, `simulation.defaultSpeed`.

## Kết quả API smoke (tóm tắt)

- `GET /api/v1/leaderboard`, `GET /api/v1/classes`, report giảng viên, `GET /api/v1/achievements` → **200** (dữ liệu 8 student demo + 2 lớp hiển thị đúng).
- `POST /api/v1/auth/register` với email `@gmail.com` → **201** — **KHÔNG `DOMAIN_NOT_ALLOWED`** (xác nhận đã bỏ chặn domain).

## Vấn đề ghi chú (NGOÀI PHẠM VI seed)

- `GET /api/v1/progress/me` trả **500** — `ArgumentException` (duplicate key) ở tầng mapping/truy vấn progress; dữ liệu seed hợp lệ (counts chuẩn, không trùng khóa UNIQUE). **Đề xuất task riêng** (bug backend, không thuộc đợt seed) — không chặn merge đợt này.

---

## REVIEW ĐỘC LẬP (13/08 ~13:10) — VERDICT: ✅ APPROVE (có điều kiện — 1 lỗi Cao phải xử lý trước merge)

### Verify lại thật (chạy độc lập)
- Backend build: 0 lỗi. Test: 97/97 unit (gồm 9 test SeedDemoActivityTests mới) + 31/31 integration PASS.
- Test seed 9 cái phủ đúng trọng tâm: counts, idempotent lần 2 = 0 thay đổi, Gems = earn − spend, XP/level theo quest claim, passed node ↔ submission full-score, bounds, xóa setting domain (cả trường hợp pre-existing), không duplicate unique key. ✅
- Seed thật lên DB docker 2 lần (idempotent), counts khớp, API smoke 200, register @gmail.com → 201 không DOMAIN_NOT_ALLOWED. ✅
- Code: partial class tách theo nhóm, guard theo key unique, deterministic (dùng chung kế hoạch), log "thêm/bỏ qua" — đúng pattern SeedRunner. PR #10 đã tạo base dev. ✅

### 🔴 LỖI CAO (phải xử lý — không được coi "ngoài phạm vi")
**GET /api/v1/progress/me trả 500** — root cause đã xác định: ProgressService.cs:218-223 LoadCountsAsync query UserProgress **KHÔNG lọc p.UserId == userId** → ToDictionary(p => p.LessonId) ném ArgumentException khi ≥2 user cùng học 1 lesson. Trước K: DB chỉ 1 dòng → không crash; sau K: 31 dòng → **mọi student gọi màn "Tiến độ của tôi" đều 500**. Seed K chính là cái làm bug bung ra → không thể đẩy sang M. 
→ Xử lý: (a) fix 1 dòng trong ProgressService (thêm filter userId — thuộc đợt J backend audit đang chạy, cùng vùng service) hoặc (b) session K tự fix trước khi merge PR #10. Kèm test tái hiện: 2 user cùng lesson → /progress/me 200.

### Ghi chú phụ (không chặn merge)
- 3 user smoke mới do API smoke tạo (test-gmail-433791@gmail.com id2012 + 2 gv.smoke TeacherPending id2013/2014) — theo quyết định user "giữ nguyên user rác", OK; lưu ý 2 TeacherPending sẽ hiện trong tab chờ duyệt khi demo.
- NU1903 SSH.NET (pre-existing) → backlog review M.
- Sau merge K: data leaderboard/classes/achievements đầy đủ → H (UI review) có dữ liệu thật để chụp ảnh — tiện cho cả đợt M.

### KẾT QUẢ XỬ LÝ (session K, 16:20) — ĐÃ FIX + VERIFY ✅
- Fix: ProgressService.cs — LoadCountsAsync(userId, ct) + filter p.UserId == userId + call site GetMyOverviewAsync truyền userId (commit dd63d87).
- Test tái hiện: ProgressServiceTests.cs (3 test — 2 user cùng lesson → Success; user không progress → Success; không inflate progress user khác).
- Verify: full suite 100/100 unit + 31/31 integration PASS; API thật :5001 (code fix) /progress/me → 200 cho huynhthuy + nguyentrang (dữ liệu riêng biệt). Backend docker :5000 bản cũ vẫn 500 → sau merge PR #10 phải rebuild/restart backend.
- PR #10 (feature/seed-prod → dev) giờ sẵn sàng merge sau khi pass CI review.
