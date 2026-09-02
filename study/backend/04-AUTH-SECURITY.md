# 🛡️ TÀI LIỆU BẢO MẬT & XÁC THỰC (AUTH & SECURITY)

Tài liệu này phân tích chi tiết cơ chế bảo vệ danh tính người dùng, bảo mật API và phân quyền trong hệ thống DSA Visual.

---

## 🔑 1. CƠ CHẾ JWT ACCESS TOKEN & REFRESH TOKEN ROTATION

Hệ thống sử dụng mô hình xác thực kép:
1. **Access Token (JWT)**:
   * Thời hạn sống ngắn: **60 phút** (`DSA__Jwt__AccessTokenLifetimeMinutes`).
   * Chứa các Claims: `nameid` (UserId), `role` (`STUDENT` / `TEACHER` / `TEACHER_PENDING` / `ADMIN`), `email`, `jti` (Mã định danh token).
   * Ký số bằng thuật toán `HMAC-SHA256` với khóa đối xứng bí mật (`DSA__Jwt__Secret` $\ge 32$ ký tự).
2. **Refresh Token**:
   * Thời hạn sống: **7 ngày** (`DSA__Jwt__RefreshTokenLifetimeDays`).
   * Lưu trong bảng `RefreshTokens` của SQL Server.
   * **Cơ chế Token Rotation**: Mỗi khi Frontend gọi `POST /api/v1/auth/refresh-token`:
     * Token cũ bị đánh dấu `IsUsed = true`.
     * Backend phát hành một Refresh Token hoàn toàn mới kèm Access Token mới.
     * Nếu phát hiện token đã từng dùng bị gửi lại (nguy cơ bị đánh cắp) $\rightarrow$ Backend lập tức thu hồi toàn bộ token của User đó (`IsRevoked = true`) để ngăn chặn kẻ tấn công.

---

## 🔒 2. MÃ HÓA MẬT KHẨU (PASSWORD HASHING)

* Sử dụng thuật toán **BCrypt** mạnh mẽ trong [`PasswordHasher.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Common/PasswordHasher.cs).
* Mật khẩu thô không bao giờ được lưu trong Database.
* Tự động sinh Salt ngẫu nhiên cho từng tài khoản, chống lại tấn công Rainbow Table.

---

## 🚫 3. CHỐNG TẤN CÔNG DÒ MẬT KHẨU (BRUTE-FORCE PROTECTION)

* [`LoginAttemptTracker.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/LoginAttemptTracker.cs) theo dõi số lần đăng nhập sai theo IP và Email:
  * Sau **5 lần** nhập sai liên tiếp $\rightarrow$ Tài khoản bị khóa tạm thời trong 15 phút.
  * Khi nhập sai OTP đăng ký/quên mật khẩu quá 5 lần $\rightarrow$ Hủy mã OTP lập tức.

---

## 👥 4. PHÂN QUYỀN TRUY CẬP THEO VAI TRÒ (RBAC)

Được định nghĩa trong [`RoleNames.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Common/RoleNames.cs):

| Vai trò | Quyền hạn trong hệ thống |
|---|---|
| **`STUDENT`** | Học bài, mở khóa lộ trình, chạy mô phỏng, làm bài tập, tham gia lớp học bằng mã mời, mua vật phẩm shop. |
| **`TEACHER_PENDING`** | Tài khoản giáo viên mới đăng ký đang chờ Admin duyệt. Chỉ được học thử như sinh viên, bị chặn vào Studio. |
| **`TEACHER`** | Có toàn bộ quyền của Student + Quyền tạo lớp học, giao bài tập, vào Curriculum Studio biên soạn giáo án và chấm bài. |
| **`ADMIN`** | Quản trị viên tối cao: Phê duyệt Giảng viên, đổi mật khẩu trực tiếp, xem thống kê toàn trường, quản lý Shop và cấu hình hệ thống. |
