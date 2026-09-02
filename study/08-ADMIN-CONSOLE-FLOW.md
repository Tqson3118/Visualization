# 👑 PHÂN HỆ 8: BẢNG ĐIỀU KHIỂN QUẢN TRỊ ADMIN (ADMIN CONSOLE FLOW)

Phân hệ Quản trị dành riêng cho vai trò **Quản trị viên tối cao (Admin)** để vận hành toàn bộ hệ thống, từ người dùng, tài chính ảo đến các thông số kỹ thuật nền tảng.

---

## 1. MÀN HÌNH 09: QUẢN LÝ NGƯỜI DÙNG (ADMIN USERS VIEW)

* **URL**: `/admin/users`
* **File Vue**: [`AdminUsersView.vue`](file:///d:/FPT/metqua/frontend/src/views/AdminUsersView.vue)
* **Quyền truy cập**: Chỉ Admin (`roles: ['ADMIN']`).

```
┌────────────────────────────────────────────────────────────────────────┐
│ 👥 QUẢN TRỊ NGƯỜI DÙNG                                                │
│ 🔍 [ Tìm theo Tên, Email, Username... (Không phân biệt hoa/thường) ]  │
│ [ Lọc vai trò: Tất cả ▾ | Student | Teacher | Chờ duyệt Giáo viên ]   │
├────────────────────────────────────────────────────────────────────────┤
│ BẢNG DANH SÁCH TÀI KHOẢN:                                              │
│                                                                        │
│ 1. nguyen_van_a | a@edu.vn | Vai trò: [ TEACHER_PENDING (Vàng) ]       │
│    Hành động: [ ✅ Duyệt thành Giáo viên ] [ ❌ Từ chối ]               │
│                                                                        │
│ 2. le_thi_b     | b@gmail  | Vai trò: [ STUDENT (Xanh) ]               │
│    Hành động: [ 🔑 Đổi mật khẩu trực tiếp ] [ 🔒 Khóa tài khoản ]      │
└────────────────────────────────────────────────────────────────────────┘
```

### Các tính năng quản trị trọng tâm:
1. **Phê duyệt Giảng viên**:
   * Khi người dùng đăng ký chọn vai trò Teacher, tài khoản ở trạng thái `TEACHER_PENDING`.
   * Admin bấm **"Duyệt thành Giáo viên"** $\rightarrow$ API `POST /api/v1/admin/users/{id}/approve-teacher` cập nhật Role lên `TEACHER`. Tài khoản lập tức có quyền truy cập vào `/studio` và tạo lớp học.
2. **Đổi mật khẩu trực tiếp (Direct Password Reset)**:
   * Admin có thể bấm nút "Đổi mật khẩu" cho bất kỳ tài khoản nào mà không cần gửi email. Mật khẩu mới được mã hóa BCrypt ngay lập tức.
3. **Tìm kiếm thời gian thực (Search-as-you-type)**:
   * Tìm kiếm gần đúng theo Tên, Email hoặc Tên đăng nhập không phân biệt chữ hoa/thường.

---

## 2. MÀN HÌNH 10: THỐNG KÊ & BÁO CÁO (ADMIN STATS VIEW)

* **URL**: `/admin/stats`
* **File Vue**: [`AdminStatsView.vue`](file:///d:/FPT/metqua/frontend/src/views/AdminStatsView.vue)
* **Quyền truy cập**: Chỉ Admin (`roles: ['ADMIN']`).

### Mắt thấy gì trên giao diện?
* **4 Thẻ số liệu tổng quan**: Tổng người dùng, Số bài học hoàn thành, Số lượt chạy mô phỏng, Tổng doanh thu Ngọc.
* **Biểu đồ tăng trưởng (ECharts)**:
  * Biểu đồ đường (Line chart): Lượng người dùng hoạt động hàng ngày (DAU / MAU).
  * Biểu đồ tròn (Pie chart): Phân bố độ khó bài học được làm nhiều nhất.
  * Biểu đồ cột (Bar chart): Top 5 thuật toán được xem mô phỏng nhiều nhất trên hệ thống.

---

## 3. MÀN HÌNH 11: CÀI ĐẶT NỀN TẢNG (ADMIN SETTINGS VIEW)

* **URL**: `/admin/settings`
* **File Vue**: [`AdminSettingsView.vue`](file:///d:/FPT/metqua/frontend/src/views/AdminSettingsView.vue)
* **Quyền truy cập**: Chỉ Admin (`roles: ['ADMIN']`).

### Cấu hình các tham số hệ thống:
1. **Thiết lập Gamification**:
   * Số tim tối đa (mặc định: `5`).
   * Thời gian hồi 1 tim (mặc định: `30 phút`).
   * Hệ số nhân điểm kinh nghiệm XP bài học và bài tập.
2. **Thiết lập Bảo mật & JWT**:
   * Thời gian sống của Access Token (phút).
   * Thời gian sống của Refresh Token (ngày).
3. **Thiết lập Mail SMTP**:
   * Host, Port, Email gửi thông báo hệ thống.

---

## 4. CÁC MÀN HÌNH QUẢN TRỊ KHÁC

* **Màn hình N-5: Quản lý Shop ([AdminShopView.vue](file:///d:/FPT/metqua/frontend/src/views/admin/AdminShopView.vue))**: Thêm vật phẩm mới, chỉnh sửa giá bán Ngọc, bật/tắt hiển thị trong cửa hàng.
* **Màn hình N-6: Lịch sử Giao dịch ([AdminTransactionsView.vue](file:///d:/FPT/metqua/frontend/src/views/admin/AdminTransactionsView.vue))**: Theo dõi dòng tiền ảo, các lượt nạp tim và mua vật phẩm của sinh viên.
* **Màn hình Quản lý Lớp học ([AdminClassesView.vue](file:///d:/FPT/metqua/frontend/src/views/admin/AdminClassesView.vue))**: Giám sát toàn bộ các lớp học do giảng viên tạo trên toàn trường.
