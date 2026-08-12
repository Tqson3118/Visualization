# DsaVisual.IntegrationTests

Kiểm thử tích hợp — **WebApplicationFactory + Testcontainers (SQL Server)** (SDD §5.1).

## Kiến trúc

- `MssqlFixture` — MỘT container `mcr.microsoft.com/mssql/server:2022-latest` dùng chung toàn bộ
  collection `"Mssql"` (image phải có sẵn local, test KHÔNG pull image mới).
- `ApiTestFixture` — mỗi test class có MỘT database riêng `DsaVisualTest_<guid>` trên container;
  schema tạo bằng migrations EF (`Database.MigrateAsync`) nên khớp 100% production.
- `ApiFactory` — `WebApplicationFactory<Program>` override connection string + JWT secret test
  (≥ 32 ký tự), Serilog min Warning.
- `IntegrationTestBase` — helper JWT HS256 đúng issuer/audience/secret của app, seed qua DbContext,
  JSON helpers.
- 4 file test: `AuthIntegrationTests` (7), `LessonsIntegrationTests` (10),
  `TopicsIntegrationTests` (7), `PublicApiTests` (2) — tổng 26 test.

## Quy tắc bắt buộc khi viết test

- MỌI seed phải dùng giá trị **unique** (email/tên topic): `CreateUserAsync()` / `CreateTopicAsync()`
  mặc định tự sinh unique, hoặc truyền `UniqueEmail(...)` / tên gắn `Guid.NewGuid()`.
  CẤM email/tên cố định lặp lại giữa các test — database dùng chung trong class và có
  unique index `IX_Users_Email` / `IX_Topics_Name`.
- Các assert đếm số lượng (`X-Total-Count`, `Total`) phải lọc theo topic riêng (`?topicId=`)
  vì lesson/topic của test khác cũng nằm trong cùng database.
- `GET /topics` trả toàn bộ cây (không filter) → tìm node theo tên unique,
  không `Assert.Single` trên toàn cây.
- `[Authorize(Roles)]` bị từ chối → 403 **body rỗng** (ForbidResult), không có envelope `{ error }`.

## Cách chạy

Yêu cầu: Docker đang chạy, image SQL Server 2022 có sẵn local.

```powershell
# Từ backend/
dotnet build DsaVisual.sln
dotnet test DsaVisual.sln --no-build          # UnitTests 44 + IntegrationTests 26
# Hoặc chỉ integration tests:
dotnet test tests/DsaVisual.IntegrationTests/DsaVisual.IntegrationTests.csproj --no-build
```

Container test tự khởi tạo (chờ 2-3 phút lần đầu). KHÔNG cần `docker pull`/`docker stop` thủ công.

## Lưu ý bug app thật (chờ PM quyết định)

JwtBearer mặc định `MapInboundClaims = true` map claim `sub` → `ClaimTypes.NameIdentifier`,
trong khi controllers đọc `User.FindFirst(JwtRegisteredClaimNames.Sub)` → NRE → 500 trên mọi
endpoint authenticated (ảnh hưởng cả production). Test hiện dùng workaround test-side
(`OnTokenValidated` bổ sung claim `sub`) — xem chú thích trong `ApiFactory.cs`.
