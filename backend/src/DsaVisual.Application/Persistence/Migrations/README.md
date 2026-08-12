# Migrations

Thư mục chứa migration EF Core cho **SQL Server**.

**Chưa có migration nào** — schema chưa có CSDL thật. Khi có CSDL (SQL Server local hoặc docker-compose), tạo migration đầu tiên:

```powershell
dotnet ef migrations add InitialCreate --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
```

> Yêu cầu: `dotnet tool install --global dotnet-ef` nếu chưa có.
