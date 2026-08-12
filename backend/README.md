# Backend — DsaVisual (ASP.NET Core / .NET 10)

Skeleton v2 theo **SDD §5.1** — các Service còn **TODO** (chỉ `LessonService` là mẫu hoàn chỉnh).

## Kiến trúc (2 project — quyết định A-1)

```
backend/
├── src/
│   ├── DsaVisual.Api/           # Web API (presentation layer)
│   │   ├── Controllers/         # LessonsController (mẫu) + MapResult
│   │   ├── Dtos/                # ErrorResponseDto / ErrorDto / ErrorDetailDto
│   │   ├── Middlewares/         # RequestLogging → ErrorHandling
│   │   └── Program.cs           # pipeline theo SDD §5.8
│   └── DsaVisual.Application/   # Business logic + data access (DbContext trực tiếp)
│       ├── Services/            # 12 service (LessonService mẫu + 11 TODO)
│       ├── Persistence/         # AppDbContext (32 DbSet), Configurations, Migrations
│       ├── Validators/          # FluentValidation (LessonValidator mẫu)
│       └── Common/              # Result<T>, ErrorCodes, Pagination, DateTimeProvider
└── tests/
    ├── DsaVisual.UnitTests/     # xUnit — LessonValidatorTests
    ├── DsaVisual.IntegrationTests/  # rỗng — WebApplicationFactory + Testcontainers
    └── DsaVisual.Api.Tests/     # rỗng — controller/DTO
```

- **KHÔNG** có Domain/Infrastructure tách riêng, **KHÔNG** Repository pattern — Service truy vấn DbContext qua DbSet trực tiếp (`AsNoTracking` cho đọc).
- Service trả `Result<T>` + ErrorCode theo catalog; Controller map qua `MapResult`.
- Lỗi chuẩn: `{ "error": { "code", "message", "field", "details" } }` (API_REFERENCE §2.1).

## Chạy dev

```powershell
# 1. Cấu hình biến môi trường (secret JWT ≥ 32 ký tự — xem .env.example, DEPLOY §2)
$env:DSA__Jwt__Secret = "<chuỗi ngẫu nhiên ≥ 32 ký tự>"

# 2. Chạy API (Swagger: http://localhost:5000/swagger — Development/Staging)
dotnet run --project src/DsaVisual.Api
```

Dev dùng SQL Server local hoặc `docker compose up -d sqlserver mailhog` (DEPLOY §3.3).

## Migration (khi có CSDL)

```powershell
dotnet ef migrations add InitialCreate --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
```

Yêu cầu: `dotnet tool install --global dotnet-ef`. Chi tiết: `src/DsaVisual.Application/Persistence/README.md`.

## Verify

```powershell
dotnet build DsaVisual.sln
dotnet test tests/DsaVisual.UnitTests
```
