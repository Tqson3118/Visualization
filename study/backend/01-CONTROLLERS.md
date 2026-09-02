# 🎮 TÀI LIỆU CONTROLLERS — API PRESENTATION LAYER

Tầng Controllers trong `backend/src/DsaVisual.Api/Controllers/` đóng vai trò là cổng tiếp nhận (Entry Point) của mọi HTTP Request từ Frontend, thực hiện:
1. **Kiểm tra quyền truy cập (Authorization & Roles)**.
2. **Trích xuất thông tin người dùng từ JWT Token (`CurrentUserId`, `CurrentUserRole`)**.
3. **Chuyển giao xử lý cho các Business Service**.
4. **Ánh xạ kết quả `Result<T>` sang phản hồi HTTP chuẩn (`MapResult`)**.

---

## 🏛️ BASE CONTROLLER & CƠ CHẾ MAPRESULT

Tất cả các Controller đều kế thừa từ [`ApiControllerBase.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/ApiControllerBase.cs):

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    // Lấy UserId từ JWT Claim
    protected Guid CurrentUserId => 
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    // Lấy Role từ JWT Claim
    protected string? CurrentUserRole => User.FindFirstValue(ClaimTypes.Role);

    // Ánh xạ Result<T> sang HTTP Result
    protected IActionResult MapResult<T>(Result<T> result) => 
        result.ToActionResult(this);
}
```

### Bảng ánh xạ ErrorCode sang HTTP Status Code ([`MapResultExtensions.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/MapResultExtensions.cs)):

| Error Code Category | HTTP Status | Định dạng phản hồi JSON |
|---|:---:|---|
| `Common.NotFound`, `Lesson.NotFound` | **404 Not Found** | `{ "error": { "code": "Lesson.NotFound", "message": "Bài học không tồn tại" } }` |
| `Auth.InvalidCredentials`, `Auth.Unauthorized` | **401 Unauthorized** | `{ "error": { "code": "Auth.InvalidCredentials", "message": "Mật khẩu không đúng" } }` |
| `Auth.Forbidden`, `Common.Forbidden` | **403 Forbidden** | `{ "error": { "code": "Auth.Forbidden", "message": "Bạn không có quyền truy cập" } }` |
| `Validation.*`, `Gamification.InsufficientGems` | **400 Bad Request** | `{ "error": { "code": "Validation.Failed", "details": [...] } }` |
| `Auth.EmailAlreadyExists`, `Common.Conflict` | **409 Conflict** | `{ "error": { "code": "Auth.EmailAlreadyExists", "message": "Email đã tồn tại" } }` |
| Thành công (`Result.Success(data)`) | **200 OK** | `{ "data": { ... } }` |

---

## 📋 DANH SÁCH 25 CONTROLLERS & ENDPOINTS CHI TIẾT

### 1. [`AuthController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/AuthController.cs) (`/api/v1/auth`)
* `POST /login`: Đăng nhập bằng Email/Username + Mật khẩu $\rightarrow$ Trả về JWT Access Token + Refresh Token (hoặc cờ `requiresTwoFactor`).
* `POST /register-request-otp`: Yêu cầu gửi mã OTP đăng ký qua MailHog.
* `POST /register-verify-otp`: Xác thực mã OTP và khởi tạo tài khoản (`STUDENT` hoặc `TEACHER_PENDING`).
* `POST /refresh-token`: Cấp phát Access Token mới bằng Refresh Token hợp lệ (Rotation).
* `POST /forgot-password` & `POST /reset-password`: Quy trình khôi phục mật khẩu.
* `POST /verify-2fa`: Xác thực mã 2FA 6 số để hoàn tất đăng nhập.
* `POST /logout`: Hủy Refresh Token hiện tại khỏi CSDL.

### 2. [`LessonsController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/LessonsController.cs) (`/api/v1/lessons`)
* `GET /{id}`: Lấy nội dung chi tiết bài học (Lý thuyết Markdown + Cấu hình Sandbox/Simulation).
* `POST /`: `[Authorize(Roles = "TEACHER,ADMIN")]` Tạo bài học mới.
* `PUT /{id}`: `[Authorize(Roles = "TEACHER,ADMIN")]` Cập nhật nội dung bài học, lưu cấu hình mô phỏng.
* `DELETE /{id}`: `[Authorize(Roles = "TEACHER,ADMIN")]` Xóa bài học.
* `POST /{id}/complete`: `[Authorize]` Đánh dấu hoàn thành bài học $\rightarrow$ Mở khóa bài tiếp theo + Thưởng XP.
* `PUT /{id}/notes`: `[Authorize]` Lưu ghi chú cá nhân của học viên vào bài học.

### 3. [`PathItemsController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/PathItemsController.cs) (`/api/v1/path-items`)
* `GET /`: Lấy toàn bộ danh sách lộ trình và cây cấu trúc outline (Chương/Module/Bài học).
* `POST /`: `[Authorize(Roles = "TEACHER,ADMIN")]` Tạo node lộ trình mới.
* `PUT /{id}/move`: `[Authorize(Roles = "TEACHER,ADMIN")]` Kéo thả sắp xếp lại thứ tự bài học trong cây.

### 4. [`ExercisesController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/ExercisesController.cs) (`/api/v1/exercises`)
* `GET /{id}`: Lấy chi tiết đề bài tập trắc nghiệm hoặc bài tập CodeLab.
* `POST /{id}/submit-quiz`: Chấm bài trắc nghiệm $\rightarrow$ Đúng +10 XP, Sai trừ 1 Tim.
* `POST /{id}/submit-code`: Chấm code tự động qua môi trường Sandbox Sandbox $\rightarrow$ Kiểm tra với Test Case ẩn.
* `POST /{id}/submit-final-test`: Chấm bài thi cuối lộ trình ($\ge 70\%$ đạt tiêu chuẩn cấp Chứng nhận).
* `GET /{id}/submissions`: Lấy lịch sử nộp bài của chính học viên.

### 5. [`GamificationController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/GamificationController.cs) (`/api/v1/gamification`)
* `GET /stats`: Lấy thông số Level, XP, số Tim hiện có, số Ngọc, Chuỗi ngày học Streak.
* `GET /quests` & `POST /quests/{id}/claim`: Danh sách nhiệm vụ hàng ngày & Nhận thưởng Ngọc.
* `GET /shop/items` & `POST /shop/buy`: Lấy danh mục vật phẩm & Mua vật phẩm bằng Ngọc.
* `GET /inventory` & `POST /inventory/{id}/equip`: Quản lý túi đồ & Trang bị Khung viền Avatar.
* `GET /leaderboard`: Lấy bảng xếp hạng theo Tuần, Level hoặc theo Lớp học.

### 6. [`ClassesController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/ClassesController.cs) (`/api/v1/classes`)
* `GET /`: Lấy danh sách các lớp học người dùng đang tham gia (học sinh) hoặc quản lý (giáo viên).
* `POST /`: `[Authorize(Roles = "TEACHER,ADMIN")]` Tạo lớp học mới và sinh mã `InviteCode`.
* `POST /join`: Học sinh tham gia lớp học bằng mã mời.
* `GET /{id}/members`: Lấy danh sách học sinh trong lớp.
* `GET /{id}/assignments` & `POST /{id}/assignments`: Quản lý các bài tập được giao cho lớp.
* `GET /{id}/report`: `[Authorize(Roles = "TEACHER,ADMIN")]` Báo cáo phân tích điểm số toàn bộ lớp học.

### 7. [`AdminController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/AdminController.cs) (`/api/v1/admin`)
* `GET /users`: `[Authorize(Roles = "ADMIN")]` Tìm kiếm và lọc danh sách người dùng.
* `POST /users/{id}/approve-teacher`: `[Authorize(Roles = "ADMIN")]` Phê duyệt hồ sơ Giảng viên (`TEACHER_PENDING` $\rightarrow$ `TEACHER`).
* `POST /users/{id}/reset-password`: `[Authorize(Roles = "ADMIN")]` Đổi mật khẩu trực tiếp cho tài khoản mà không cần email.
* `GET /stats`: `[Authorize(Roles = "ADMIN")]` Thống kê tăng trưởng toàn hệ thống.

### 8. Các Controllers Phụ trợ khác:
* [`AdminGamificationController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/AdminGamificationController.cs): Cấu hình số Tim tối đa, thời gian hồi Tim.
* [`AdminShopController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/AdminShopController.cs): Thêm/Sửa/Xóa vật phẩm cửa hàng & Lịch sử giao dịch.
* [`MeController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/MeController.cs): Cập nhật thông tin cá nhân, Đổi mật khẩu, Upload avatar.
* [`AiController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/AiController.cs): Tích hợp AI giải thích bước đi thuật toán (`/explain-step`).
* [`CourseFeedbackController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/CourseFeedbackController.cs): Luồng phản hồi phân tầng (Học sinh gửi $\rightarrow$ Giáo viên trả lời $\rightarrow$ Admin duyệt).
* [`FeedbackController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/FeedbackController.cs): Báo lỗi hệ thống (Bug Reports).
* [`SimulationsController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/SimulationsController.cs) & [`TopicsController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/TopicsController.cs): Đọc danh mục mô phỏng và nhóm chủ đề.
