# DsaVisual.IntegrationTests

Kiểm thử tích hợp — **WebApplicationFactory + Testcontainers (SQL Server)** (SDD §5.1).

## Trạng thái

Skeleton rỗng — chưa có test. Khi có migration + seed đầu tiên, viết test tại đây:

- `Testcontainers.Mssql` khởi tạo SQL Server container cho từng test class.
- Dùng `WebApplicationFactory<Program>` (cần `Program` public hoặc `[assembly: InternalsVisibleTo]`) để chạy API thật với connection string trỏ container.

## Packages cần thêm khi triển khai

```powershell
dotnet add tests/DsaVisual.IntegrationTests package Testcontainers.Mssql
```
