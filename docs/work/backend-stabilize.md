# Work — Backend Stabilize

Trạng thái tiến độ ổn định backend (fix flaky test, hardening). Cập nhật sau mỗi task.

## Task 2/5 — Fix flaky perf#9: đồng bộ múi giờ UTC+7 trong PerformanceGuardRegressionTests

### Mục tiêu
Sửa triệt để flaky test `AdminStats_AfterSeeding_ReportsExactDeltas` (perf#9): test FAIL ngẫu nhiên
trong khoảng 17:00–24:00 UTC (00:00–07:00 VN) vì `active.LastActivityDate` được gán theo UTC gốc
trong khi `/admin/stats` tính mốc "hôm nay" theo UTC+7 (`AdminController.GetStats`).

### Thay đổi
- `backend/tests/DsaVisual.IntegrationTests/PerformanceGuardRegressionTests.cs`
  - Thêm biến `nowVn = DateTime.UtcNow.AddHours(7)` (UTC+7, khớp chuẩn mốc `today` của AdminController).
  - `active.LastActivityDate = nowVn;` thay cho `active.LastActivityDate = now;` — đảm bảo
    `LastActivityDate >= today` ở MỌI thời điểm chạy (kể cả 17:00–24:00 UTC).
  - Biến `now` giữ nguyên cho các field không liên quan query ngày (CreatedAt, SubmittedAt, ...).
  - Toàn bộ assert delta (TotalUsers +3, ActiveUsersToday +1, ...) giữ nguyên — không đổi contract.
- `backend/src/DsaVisual.Api/Controllers/AdminController.cs` — rà soát: logic UTC+7 chuẩn, không sửa.

### Kết quả verify
- `dotnet build DsaVisual.sln` → 0 Errors (2 warning NU1903 SSH.NET — có sẵn, không phải mới).
- `dotnet test tests/DsaVisual.UnitTests` → 153/153 PASS.
- `dotnet test tests/DsaVisual.IntegrationTests --filter "FullyQualifiedName~AdminStats_AfterSeeding_ReportsExactDeltas"` → 1/1 PASS (lần chạy 1, không retry).

### Trạng thái
✅ Hoàn thành — commit duy nhất đã tạo. Chờ review / task tiếp theo.

## Task 3/5 — Nâng cấp Testcontainers.MsSql: xử lý NU1903 (SSH.NET) + verify integration ổn định

### Mục tiêu
Hết cảnh báo bảo mật NU1903 (SSH.NET advisory GHSA-q939-rpr3-3284) và integration suite PASS 100%
nhiều lần liên tiếp.

### Audit (14/08)
- `Testcontainers.MsSql 4.13.0` (đang dùng) đã là bản stable mới nhất trên NuGet — KHÔNG tồn tại bản 4.14/4.15+ như giả định ban đầu (verify qua flat-container + search API + GitHub releases).
- Chuỗi dependency: `Testcontainers.MsSql 4.13.0` → `Testcontainers 4.13.0` → `SSH.NET [2025.1.0, )`.
- Advisory GHSA-q939-rpr3-3284 (High): SSH.NET `<= 2025.1.0` vulnerable (ScpClient recursive download — arbitrary file write); bản vá đầu tiên: **SSH.NET 2026.0.0** (ra 09/08/2026, sau Testcontainers 4.13.0 — upstream chưa kịp bump).
- SSH.NET 2026.0.0 nằm trong range `[2025.1.0, )` của Testcontainers → explicit reference hợp lệ, không vỡ constraint.

### Thay đổi
- `backend/tests/DsaVisual.IntegrationTests/DsaVisual.IntegrationTests.csproj` (chỉ 1 file):
  - Thêm `<PackageReference Include="SSH.NET" Version="2026.0.0" />` — remediation chuẩn NU1903 cho transitive package: direct reference đè bản vulnerable 2025.1.0 bằng bản đã vá (khuyến nghị chính thức của NuGet/dotnet audit).
  - `Testcontainers.MsSql` giữ nguyên 4.13.0 (đã là mới nhất, không thể nâng).
  - SSH.NET chỉ được Testcontainers dùng cho Docker host qua SSH — integration tests dùng Docker local (npipe), không chạm code path này nên không ảnh hưởng runtime.

### Kết quả verify
- `dotnet list tests/DsaVisual.IntegrationTests package --vulnerable --include-transitive` → **"has no vulnerable packages"** (hết NU1903).
- `dotnet build DsaVisual.sln` → **0 Warning, 0 Error** (trước đó 2 warning NU1903).
- `dotnet test tests/DsaVisual.IntegrationTests` (full suite, không filter):
  - 3 lần liên tiếp cuối (có trx): **78/78, 78/78, 78/78 — 100% PASS**.
  - Tổng cộng 16 lần chạy với thay đổi: 15 lần 78/78; 1 lần đầu tiên sau build (lạnh) đạt 77/78 với 1 test fail không xác định được tên, KHÔNG tái diễn ở 15 lần sau.
  - Baseline đối chứng (stash bỏ thay đổi, build lại): 3/3 lần 78/78 — flake run-1 không phải do thay đổi package (SSH.NET không được exercise; lỗi thư viện sẽ fail cả suite, không fail 1 test).

### Trạng thái
✅ Hoàn thành — commit duy nhất đã tạo. Ghi nhận: 1 flake lạnh không xác định ở run đầu (chưa tái diễn);
đề xuất task riêng nếu cần truy vết triệt để (chạy có `--logger trx` lặp lại đến khi reproduce, xác định test
cụ thể). Khi Testcontainers ra bản bump SSH.NET floor (4.14+), có thể gỡ explicit reference này.
