# 🛒 VIEW 27: QUẢN TRỊ SHOP, GIAO DỊCH & LỚP HỌC TRƯỜNG

* **Tên file Vue**:
  1. [`AdminShopView.vue`](file:///d:/FPT/metqua/frontend/src/views/admin/AdminShopView.vue) (URL: `/admin/shop`)
  2. [`AdminTransactionsView.vue`](file:///d:/FPT/metqua/frontend/src/views/admin/AdminTransactionsView.vue) (URL: `/admin/transactions`)
  3. [`AdminClassesView.vue`](file:///d:/FPT/metqua/frontend/src/views/admin/AdminClassesView.vue) (URL: `/admin/classes`)
* **Quyền truy cập**: Quản trị viên (`roles: ['ADMIN']`).

---

## 1. MÀN HÌNH QUẢN LÝ SHOP (ADMIN SHOP VIEW)

### Mắt thấy gì trên giao diện?
* Bảng danh sách vật phẩm trong cửa hàng (Tên, Mã ItemKey, Loại: Avatar/Khung viền/Vật phẩm tiêu hao, Giá Ngọc).
* Nút **"+ Thêm vật phẩm mới"**: Mở modal nhập tên, giá bán và upload ảnh.
* Nút **Chỉnh sửa / Xóa vật phẩm**.

---

## 2. MÀN HÌNH LỊCH SỬ GIAO DỊCH (ADMIN TRANSACTIONS VIEW)

### Mắt thấy gì trên giao diện?
* Nhật ký biến động số dư Ngọc của toàn bộ người dùng hệ thống:
  * ID giao dịch, Tên người dùng, Loại giao dịch (Thưởng nhiệm vụ / Mua vật phẩm / Nạp tim), Số lượng biến động (`+50 💎` hoặc `-100 💎`), Thời gian giao dịch.

---

## 3. MÀN HÌNH QUẢN LÝ LỚP HỌC TOÀN TRƯỜNG (ADMIN CLASSES VIEW)

### Mắt thấy gì trên giao diện?
* Giám sát toàn bộ các lớp học do các giảng viên tạo ra trên toàn hệ thống.
* Xem sĩ số, số bài tập đã giao và quyền can thiệp/chuyển giao quyền quản lý lớp khi cần thiết.

---

## 4. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend Views**: [`AdminShopView.vue`](file:///d:/FPT/metqua/frontend/src/views/admin/AdminShopView.vue), [`AdminTransactionsView.vue`](file:///d:/FPT/metqua/frontend/src/views/admin/AdminTransactionsView.vue), [`AdminClassesView.vue`](file:///d:/FPT/metqua/frontend/src/views/admin/AdminClassesView.vue)
* **Backend Controller**: [`AdminShopController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/AdminShopController.cs), [`ClassesController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/ClassesController.cs)
