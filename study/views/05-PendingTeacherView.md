# ⏳ VIEW 05: CHỜ PHÊ DUYỆT GIẢNG VIÊN (PENDINGTEACHERVIEW)

* **Tên file Vue**: [`PendingTeacherView.vue`](file:///d:/FPT/metqua/frontend/src/views/PendingTeacherView.vue)
* **Đường dẫn URL**: `/pending-teacher`
* **Route Name**: `pending-teacher`
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`), đặc biệt dành cho tài khoản có Role = `TEACHER_PENDING`.

---

## 1. CẤU TRÚC GIAO DIỆN & VAI TRÒ

Màn hình này xuất hiện khi một người dùng đăng ký tài khoản với tư cách Giảng viên. Hồ sơ của họ cần được Quản trị viên (Admin) xét duyệt trước khi được cấp quyền truy cập vào Studio biên soạn giáo án (`/studio`) hoặc tạo lớp học (`/classes`).

```
┌────────────────────────────────────────────────────────┐
│             🎓 ĐANG CHỜ PHÊ DUYỆT GIẢNG VIÊN            │
│                                                        │
│   [ ⏳ Icon đồng hồ vàng nhấp nháy ]                   │
│                                                        │
│   Hồ sơ của bạn đang được xét duyệt!                   │
│   Xin chào ThS. Nguyễn Văn A, tài khoản Giảng viên của │
│   bạn đã được ghi nhận và đang chờ Admin phê duyệt.   │
│                                                        │
│   [ 🔄 Kiểm tra trạng thái duyệt ]                      │
│   [ 🏠 Về trang chủ học tập ]    [ 🚪 Đăng xuất ]       │
└────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Kiểm tra lại trạng thái duyệt (Check Approval Status)
1. Người dùng bấm nút **"Kiểm tra trạng thái duyệt"** (`handleRefreshStatus`).
2. Frontend gọi `auth.fetchMe()` $\rightarrow$ Gửi request `GET /api/v1/me`.
3. Backend trả về thông tin User hiện tại từ Database.
4. **Phân nhánh kết quả**:
   * Nếu Admin **đã duyệt** (`user.role === 'TEACHER'`):
     * Giao diện hiện thông báo chúc mừng thành công.
     * Tự động chuyển hướng ngay sang `/studio`.
   * Nếu Admin **chưa duyệt** (`user.role === 'TEACHER_PENDING'`):
     * Giao diện hiện thông báo thông tin: *"Hồ sơ vẫn đang trong hàng đợi xét duyệt"*.

### 🔹 Flow 2: Cơ chế Router Guard chặn truy cập trái phép
Trong file [`frontend/src/router/index.ts`](file:///d:/FPT/metqua/frontend/src/router/index.ts#L380-L386):
* Bất cứ khi nào tài khoản `TEACHER_PENDING` cố tình gõ URL `/studio`, `/teacher`, `/admin/*` $\rightarrow$ Router Guard tự động bắt lại và chuyển hướng về `/pending-teacher`.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`PendingTeacherView.vue`](file:///d:/FPT/metqua/frontend/src/views/PendingTeacherView.vue)
* **Frontend Router**: [`router/index.ts`](file:///d:/FPT/metqua/frontend/src/router/index.ts)
* **API Endpoint**: `GET /api/v1/me`
* **Admin Approval API**: `POST /api/v1/admin/users/{id}/approve-teacher` (được kích hoạt từ [`AdminUsersView.vue`](file:///d:/FPT/metqua/frontend/src/views/AdminUsersView.vue))
