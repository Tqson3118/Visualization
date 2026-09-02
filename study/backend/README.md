# ⚙️ TÀI LIỆU TOÀN TẬP BACKEND — DSA VISUAL (.NET 10)

Tài liệu này phân tích toàn bộ tầng xử lý phía máy chủ (Backend) của dự án DSA Visual, xây dựng trên nền tảng **ASP.NET Core (.NET 10)** với kiến trúc 2-project tinh gọn.

---

## 🏛️ 1. TỔNG QUAN KIẾN TRÚC BACKEND

```
backend/
├── src/
│   ├── DsaVisual.Api/                  -> Tầng Presentation (Controllers, DTOs, Middlewares)
│   │   ├── Controllers/                -> 25 Controllers tiếp nhận HTTP Request
│   │   ├── Dtos/                       -> Request/Response Models, ErrorResponseDto
│   │   ├── Middlewares/                -> RequestLoggingMiddleware, ErrorHandlingMiddleware
│   │   └── Program.cs                  -> Pipeline khởi chạy, cấu hình DI, CORS, JWT
│   │
│   └── DsaVisual.Application/          -> Tầng Business Logic & Data Access
│       ├── Common/                     -> Result<T>, ErrorCodes, PasswordHasher, RoleNames
│       ├── Persistence/                -> AppDbContext (32 DbSets), Configurations, Migrations
│       ├── Services/                   -> 16 Business Services
│       └── Validators/                 -> FluentValidation Rules
└── tests/                              -> Unit Tests (xUnit) & Integration Tests
```

### 💡 Các triết lý thiết kế quan trọng:
1. **Không dùng Repository Pattern rườm rà**: Các Service truy vấn trực tiếp thông qua `AppDbContext.DbSet`. Sử dụng `.AsNoTracking()` cho tất cả các truy vấn đọc để tối ưu RAM và tốc độ phản hồi.
2. **Result Pattern (`Result<T>`)**: Service không bao giờ ném Exception cho các lỗi nghiệp vụ thông thường (Business Faults). Thay vào đó, trả về `Result<T>.Success(data)` hoặc `Result<T>.Failure(ErrorCode)`.
3. **MapResult Extension**: Controller sử dụng extension method `MapResult(result)` để ánh xạ đồng bộ `Result<T>` sang mã HTTP Status tương ứng (`200 OK`, `400 BadRequest`, `401 Unauthorized`, `404 NotFound`, `409 Conflict`) chuẩn RFC 7807.

---

## 📑 DANH SÁCH CÁC TÀI LIỆU CHI TIẾT TRONG THƯ MỤC `study/backend/`:

| STT | File tài liệu | Nội dung trọng tâm |
|:---:|---|---|
| **1** | [**`01-CONTROLLERS.md`**](file:///d:/FPT/metqua/study/backend/01-CONTROLLERS.md) | Phân tích toàn bộ **25 Controller**, các endpoint RESTful, phân quyền `[Authorize(Roles = ...)]` và định dạng DTO. |
| **2** | [**`02-SERVICES.md`**](file:///d:/FPT/metqua/study/backend/02-SERVICES.md) | Phân tích chi tiết **16 Service nghiệp vụ**: Xử lý Auth, Lộ trình học, Gamification, Chấm bài Codelab, Lớp học, Quản trị. |
| **3** | [**`03-DATABASE-ENTITIES.md`**](file:///d:/FPT/metqua/study/backend/03-DATABASE-ENTITIES.md) | Bản đồ quan hệ CSDL, chi tiết **32 Entity EF Core**, cấu hình Fluent API, Indexing và Khóa ngoại. |
| **4** | [**`04-AUTH-SECURITY.md`**](file:///d:/FPT/metqua/study/backend/04-AUTH-SECURITY.md) | Cơ chế xác thực: JWT Access Token (60p) + Refresh Token Rotation (7 ngày), BCrypt hashing, chính sách mật khẩu, chống Brute-Force. |
