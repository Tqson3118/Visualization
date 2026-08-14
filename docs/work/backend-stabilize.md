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
