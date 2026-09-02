# ℹ️ PHÂN HỆ 9: CÁC TRANG PHỤ TRỢ & ĐIỀU HƯỚNG TĨNH (STATIC & UTILITY VIEWS)

Phân hệ này bao gồm các trang thông tin hỗ trợ người dùng, điều khoản dịch vụ và xử lý các lỗi điều hướng (404 Not Found).

---

## 1. MÀN HÌNH 12: TRUNG TÂM TRỢ GIÚP & FAQ (HELP VIEW)

* **URL**: `/help`
* **File Vue**: [`HelpView.vue`](file:///d:/FPT/metqua/frontend/src/views/HelpView.vue)
* **Quyền truy cập**: Công khai (Public).

### Mắt thấy gì trên giao diện?
* **Thanh tìm kiếm câu hỏi trợ giúp**: Tìm nhanh hướng dẫn sử dụng.
* **Các chủ đề FAQ thường gặp**:
  * "Làm sao để phục hồi Tim khi hết lượt học?"
  * "Làm thế nào để đăng ký làm Giảng viên và mở lớp?"
  * "Cách sử dụng phím tắt trên Trình mô phỏng thuật toán (Phím Space để Play/Pause, phím mũi tên để Step)."
* **Form gửi phản hồi trực tiếp cho Đội ngũ Hỗ trợ**.

---

## 2. MÀN HÌNH 13: CHÍNH SÁCH BẢO MẬT & QUYỀN RIÊNG TƯ (PRIVACY VIEW)

* **URL**: `/privacy`
* **File Vue**: [`PrivacyView.vue`](file:///d:/FPT/metqua/frontend/src/views/PrivacyView.vue)
* **Quyền truy cập**: Công khai (Public).

### Mắt thấy gì trên giao diện?
* Quy định bảo mật thông tin tài khoản, cam kết không chia sẻ dữ liệu sinh viên cho bên thứ ba.
* Quy chuẩn lưu trữ mã hóa mật khẩu (BCrypt) và quản lý phiên đăng nhập an toàn (JWT Refresh Token Rotation).

---

## 3. MÀN HÌNH 404: KHÔNG TÌM THẤY TRANG (NOT FOUND VIEW)

* **URL**: `/:pathMatch(.*)*` (Bất kỳ URL nào không khớp với định tuyến hệ thống)
* **File Vue**: [`NotFoundView.vue`](file:///d:/FPT/metqua/frontend/src/views/NotFoundView.vue)
* **Quyền truy cập**: Mọi người.

### Mắt thấy gì trên giao diện?
* Hình ảnh minh họa trực quan sinh động 404 (Algorithm Node Missing).
* Thông báo: "Đường dẫn bạn truy cập không tồn tại hoặc đã được thay đổi".
* Nút CTA **"Quay về Trang chủ"** hoặc **"Xem Danh sách Lộ trình"**.
