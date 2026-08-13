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
