# ℹ️ VIEW 28: TRANG PHỤ TRỢ (HELPVIEW, PRIVACYVIEW & NOTFOUNDVIEW)

* **Tên file Vue**:
  1. [`HelpView.vue`](file:///d:/FPT/metqua/frontend/src/views/HelpView.vue) (URL: `/help`)
  2. [`PrivacyView.vue`](file:///d:/FPT/metqua/frontend/src/views/PrivacyView.vue) (URL: `/privacy`)
  3. [`NotFoundView.vue`](file:///d:/FPT/metqua/frontend/src/views/NotFoundView.vue) (URL: 404 Catch-all)
* **Quyền truy cập**: Công khai (`Public`).

---

## 1. MÀN HÌNH TRỢ GIÚP & FAQ (HELP VIEW)

### Mắt thấy gì trên giao diện?
1. **Accordion FAQ**: 6 câu hỏi thường gặp về Hồi Tim, Đăng ký Giảng viên, Giới hạn Sandbox và Nâng cấp Premium.
2. **Form Báo lỗi & Đóng góp ý kiến (Bug Report / Contact Form)**:
   * Nhập Họ tên, Email, Nội dung báo lỗi $\ge 10$ ký tự.
   * Gửi `POST /api/v1/feedback/bug-reports` $\rightarrow$ Lưu vào bảng `BugReports`.
   * Phản hồi được chuyển thẳng đến **Admin Console > Cài đặt nền tảng > Báo cáo & Ý kiến** để Ban Quản Trị xử lý.

---

## 2. MÀN HÌNH CHÍNH SÁCH QUYỀN RIÊNG TƯ (PRIVACY VIEW)

### Mắt thấy gì trên giao diện?
* Văn bản quy chuẩn về bảo mật dữ liệu sinh viên, bảo vệ mật khẩu bằng thuật toán mã hóa một chiều BCrypt và quản lý phiên an toàn qua JWT Refresh Token Rotation.

---

## 3. MÀN HÌNH 404: KHÔNG TÌM THẤY TRANG (NOT FOUND VIEW)

### Mắt thấy gì trên giao diện?
* Đồ họa trực quan minh họa nút mạng thuật toán bị ngắt kết nối.
* Nút CTA: *"Quay về Trang chủ"* hoặc *"Khám phá Thư viện Thuật toán"*.

---

## 4. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend Views**: [`HelpView.vue`](file:///d:/FPT/metqua/frontend/src/views/HelpView.vue), [`PrivacyView.vue`](file:///d:/FPT/metqua/frontend/src/views/PrivacyView.vue), [`NotFoundView.vue`](file:///d:/FPT/metqua/frontend/src/views/NotFoundView.vue)
* **Backend Controller**: [`FeedbackController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/FeedbackController.cs)
* **Database Entity**: [`BugReport.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Persistence/Entities/BugReport.cs)
