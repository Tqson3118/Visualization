# 06. Authentication, authorization, Teacher Pending

## Phân biệt
- **Authentication**: xác định bạn là ai; login, access token, refresh token.
- **Authorization**: xác định bạn được làm gì; role, policy, ownership.

## Flow
Login → backend kiểm tra password → phát access/refresh token → frontend gửi access token → backend đọc userId/role → controller/service kiểm tra quyền.

Teacher đăng ký → role `TEACHER_PENDING` → bị giới hạn quyền → Admin duyệt → role thành `TEACHER` → được vào Studio.

## Code cần tra
- `frontend/src/router/index.ts`
- `backend/src/DsaVisual.Api/Controllers/AuthController.cs`
- `backend/src/DsaVisual.Application/Services/AuthService.cs`
- `backend/src/DsaVisual.Api/Controllers/AdminController.cs`
- `backend/src/DsaVisual.Application/Persistence/Entities/Enums.cs`
- `backend/src/DsaVisual.Api/Program.cs`

## Câu hỏi bảo vệ
Router guard không đủ bảo mật vì user vẫn gọi API trực tiếp. Backend phải kiểm tra role và ownership ở service/controller.

## Checklist phải học thuộc
401 thường là chưa xác thực/Token sai; 403 là đã xác thực nhưng không đủ quyền. Role check và ownership check là hai lớp khác nhau.

## Cách tra code
Trace login response → token storage → interceptor/header → middleware → CurrentUserId/role → controller attribute → service permission.

## Câu hỏi khó
Teacher Pending có thể tự đổi role bằng request không? Không; role do backend/admin thay đổi. Router guard chỉ UX, không phải security boundary.

## 7. Flow đăng nhập và quyền đến UI

1. Bắt đầu tại `frontend/src/router/index.ts`: tìm route meta `requiresAuth`, `roles`, và navigation guard. Ghi route nào chuyển teacher pending sang trang chờ.
2. Login UI gọi API auth; tra LoginView và auth store bằng Ctrl+F `login`, `accessToken`, `refreshToken`.
3. API client/interceptor gắn Authorization header vào request.
4. Backend middleware cấu hình ở `backend/src/DsaVisual.Api/Program.cs`; controller đọc CurrentUserId/role.
5. Với duyệt teacher, UI bắt đầu ở `frontend/src/views/AdminUsersView.vue:401-420`; dòng 416 gọi `adminApi.approveTeacher`. API nằm tại `frontend/src/api/admin.ts:156-157`.
6. Backend tra AdminController endpoint approve teacher → service cập nhật role/status → response quay về API → AdminUsersView:420 hiển thị toast và reload danh sách.

**Kết thúc UI:** route guard re-evaluate user state; user pending thấy màn hình chờ, teacher đã duyệt vào được Studio. 401/403 phải được map thành thông báo/redirect phù hợp.

## Flow diễn giải bằng lời
User nhập login ở UI; frontend gọi Auth API, backend xác minh rồi trả token. API client gắn access token vào request. Router ở `frontend/src/router/index.ts` chỉ bảo vệ điều hướng; backend `Program.cs` middleware đọc token, controller lấy current user và service kiểm tra role/ownership. Sai token trả 401, đủ login nhưng sai quyền trả 403. Admin duyệt teacher bắt đầu ở `frontend/src/views/AdminUsersView.vue:401-420`, gọi `frontend/src/api/admin.ts:156-157`; backend cập nhật pending thành teacher, response về UI để reload danh sách/toast và lần sau cho vào Studio.
