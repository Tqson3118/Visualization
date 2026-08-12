# Persistence — AppDbContext

**DbContext duy nhất** — 32 DbSet theo SDD §7 (lõi học tập 24 + gamification/code 8).

## Quy ước (SDD §5.3.6)

- Cấu hình bằng Fluent API trong `Configurations/` (`IEntityTypeConfiguration`) — **không attribute trên entity**.
- Đặt tên bảng/cột PascalCase (chuẩn EF Core — D-10).
- Xóa mềm: `DeletedAt datetime2 NULL` ở mọi bảng (D-5).
- Đọc: `AsNoTracking()`. Upsert UserProgress trong 1 transaction ngắn.
- Hiện chỉ có `LessonConfiguration` mẫu; các bảng khác dùng convention mặc định — **tạo Configuration khi triển khai service tương ứng**.

## Migration

**Chưa có migration nào** — tạo migration khi có CSDL (SQL Server local hoặc docker-compose):

```powershell
dotnet ef migrations add InitialCreate --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
```

> Yêu cầu: `dotnet tool install --global dotnet-ef`.
