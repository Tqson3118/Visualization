# Migrations

Thư mục chứa migration EF Core cho **SQL Server**.

## Trạng thái — [2026-08-12] Task D (backend-services)

- ✅ `20260812061254_InitialCreate` — tạo đủ **32 bảng** theo SDD §7 (lõi học tập 24 + gamification/code 8 + `__EFMigrationsHistory`).
- ✅ `dotnet ef database update` **đã chạy thành công** lên SQL Server docker-compose (`neww-sqlserver-1`, SA dev password) — database `DsaVisual` đã tồn tại với đầy đủ bảng.
- ⚠️ Schema `nvarchar(max)` dùng mapping mặc định của EF Core (string không `HasMaxLength`); config chứa `HasMaxLength` + unique index + FK `Restrict` (tránh multiple cascade path SQL Server).

## Lệnh tạo/lưu migration

```powershell
dotnet ef migrations add <TênMôTả> --project src/DsaVisual.Application --startup-project src/DsaVisual.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
```

> Lưu ý: bắt buộc `--output-dir Persistence/Migrations` (thư mục migration chuẩn của repo).
> Yêu cầu: `dotnet tool install --global dotnet-ef` nếu chưa có.
