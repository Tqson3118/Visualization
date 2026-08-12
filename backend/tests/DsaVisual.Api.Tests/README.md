# DsaVisual.Api.Tests

Kiểm thử Controller / DTO — SDD §5.1 ("kiểm thử controller/DTO").

## Trạng thái

Skeleton rỗng — chưa có test. Khi có controller mới, viết test tại đây:

- Unit test controller: mock `ILessonService` (Moq/NSubstitute) → kiểm tra status code, header `X-Total-Count`, envelope lỗi chuẩn qua `MapResult`.
- Test DTO: JSON serialization camelCase, `JsonIgnore` khi `ContentHtml` null.

## Packages cần thêm khi triển khai

```powershell
dotnet add tests/DsaVisual.Api.Tests package Moq
```
