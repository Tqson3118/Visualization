# 📋 HƯỚNG DẪN MANUAL TEST — NỀN TẢNG HỌC DSA

> File này được viết cho AI agent đọc và thực hiện test qua Puppeteer (mở browser Chrome).
> Mỗi test case gồm: bước thực hiện → kỳ vọng → ghi nhận Pass/Fail.
> Test bao gồm cả kiểm tra bug đã biết (trước fix) và xác nhận tính năng hoạt động đúng.

---

## MỤC LỤC

1. [Thông tin môi trường](#1-thông-tin-môi-trường)
2. [Quy ước test](#2-quy-ước-test)
3. [PHẦN 1 — GUEST (Chưa đăng nhập)](#3-phần-1--guest-chưa-đăng-nhập)
4. [PHẦN 2 — ĐĂNG KÝ & ĐĂNG NHẬP](#4-phần-2--đăng-ký--đăng-nhập)
5. [PHẦN 3 — ROLE ADMIN](#5-phần-3--role-admin)
6. [PHẦN 4 — ROLE TEACHER](#6-phần-4--role-teacher)
7. [PHẦN 5 — ROLE STUDENT](#7-phần-5--role-student)
8. [PHẦN 6 — CROSS-ROLE & EDGE CASES](#8-phần-6--cross-role--edge-cases)
9. [BẢNG TỔNG HỢP CHECKLIST](#9-bảng-tổng-hợp-checklist)

---

## 1. Thông tin môi trường

### URL ứng dụng
- **Frontend**: `http://localhost:5173`
- **Backend API**: `http://localhost:5000/api/v1`
- **Swagger**: `http://localhost:5000/swagger`
- **Trình duyệt**: Chrome desktop (1920x1080 hoặc 1440x900)

### Cách khởi chạy (nếu chưa chạy)

```powershell
# Terminal 1 — Backend
cd D:\FPT\neww\backend
$env:DSA__Jwt__Secret = "dev-only-secret-0123456789abcdef0123456789abcdef"
dotnet run --project src/DsaVisual.Api

# Terminal 2 — Frontend
cd D:\FPT\neww\frontend
npm install
npm run dev
```

### Tài khoản test (seed data)

| Role | Email | Mật khẩu | Tên hiển thị |
|------|-------|-----------|-------------|
| **ADMIN** | `admin@system.local` | `Admin@123` | Quản trị viên |
| **TEACHER** | `teacher@demo.local` | `Teacher@123` | Giáo viên mẫu |
| **STUDENT** | `student@demo.local` | `Student@123` | Sinh viên mẫu |
| **STUDENT 2** | `nguyenminhanh@university.edu.vn` | `Student@123` | Nguyễn Minh Anh |

> **Lưu ý**: Database seed có thể chậm. Nếu đăng nhập thất bại, chờ backend khởi động xong (check log terminal backend).

---

## 2. Quy ước test

### Ký hiệu
- 🧭 = Điều hướng (navigate đến URL)
- 🖱️ = Click
- ⌨️ = Nhập text
- 👁️ = Kiểm tra hiển thị (verify visible)
- ⏳ = Chờ (wait)
- ✅ = Kỳ vọng PASS
- ❌ = Kỳ vọng FAIL (bug đã biết, chưa fix)
- 🐛 = Bug đã biết — ghi nhận nếu còn tồn tại

### Cách ghi kết quả
Sau mỗi test case, ghi:
- **PASS** — hoạt động đúng kỳ vọng
- **FAIL** — không đúng kỳ vọng + mô tả ngắn lỗi thực tế
- **BLOCKED** — không test được do lỗi trước đó
- **SKIP** — bỏ qua (ghi lý do)

---

## 3. PHẦN 1 — GUEST (Chưa đăng nhập)

### TC-G01: Trang chủ hiển thị đúng
1. 🧭 Mở `http://localhost:5173/`
2. 👁️ Kiểm tra:
   - Header hiển thị logo + tên ứng dụng
   - Có nút "Đăng nhập" và "Đăng ký"
   - Có nút "Bắt đầu hành trình"
   - Có phần giới thiệu/hero section
   - Footer hiển thị ở cuối trang
3. ✅ Kỳ vọng: Tất cả thành phần hiển thị đầy đủ, không lỗi console

### TC-G02: Nút "Bắt đầu hành trình" khi chưa login
1. 🧭 Ở trang chủ `http://localhost:5173/`
2. 🖱️ Bấm nút "Bắt đầu hành trình"
3. ✅ Kỳ vọng: Chuyển hướng đến `/register` (trang đăng ký)

### TC-G03: Truy cập trang yêu cầu đăng nhập khi chưa login
1. 🧭 Mở `http://localhost:5173/profile`
2. ✅ Kỳ vọng: Redirect về `/login`
3. 🧭 Mở `http://localhost:5173/simulations`
4. ✅ Kỳ vọng: Redirect về `/login`
5. 🧭 Mở `http://localhost:5173/admin/users`
6. ✅ Kỳ vọng: Redirect về `/login`

### TC-G04: Trang lộ trình công khai
1. 🧭 Mở `http://localhost:5173/path`
2. 👁️ Kiểm tra: Danh sách lộ trình/roadmap hiển thị (ít nhất 1 lộ trình)
3. 👁️ Kiểm tra: Footer hiển thị ở cuối trang
4. 🐛 **BUG ĐÃ BIẾT (D3 trong plan)**: Footer có thể không hiển thị
5. ✅ Kỳ vọng: Trang hiển thị đúng, có footer

### TC-G05: Trang mô phỏng công khai — xem simulator
1. 🧭 Mở `http://localhost:5173/simulator/bubble-sort`
2. 👁️ Kiểm tra: Simulator hiển thị với visualization
3. ✅ Kỳ vọng: Trang hiển thị đúng (hoặc redirect login nếu yêu cầu auth)

### TC-G06: Trang 404
1. 🧭 Mở `http://localhost:5173/trang-khong-ton-tai-xyz`
2. 👁️ Kiểm tra: Hiển thị trang 404 Not Found
3. ✅ Kỳ vọng: Hiển thị trang lỗi thân thiện, không trắng trang

### TC-G07: Trang quên mật khẩu
1. 🧭 Mở `http://localhost:5173/forgot-password`
2. 👁️ Kiểm tra: Form nhập email hiển thị
3. ⌨️ Nhập email: `nonexistent@test.com`
4. 🖱️ Bấm nút gửi
5. ✅ Kỳ vọng: Hiển thị thông báo (thành công hoặc lỗi), không crash

### TC-G08: Trang chính sách bảo mật
1. 🧭 Mở `http://localhost:5173/privacy`
2. 👁️ Kiểm tra: Nội dung chính sách hiển thị
3. ✅ Kỳ vọng: Trang load đầy đủ

### TC-G09: Trang trợ giúp
1. 🧭 Mở `http://localhost:5173/help`
2. 👁️ Kiểm tra: Nội dung trợ giúp hiển thị
3. ✅ Kỳ vọng: Trang load đầy đủ

---

## 4. PHẦN 2 — ĐĂNG KÝ & ĐĂNG NHẬP

### TC-AUTH01: Đăng nhập thành công — Admin
1. 🧭 Mở `http://localhost:5173/login`
2. ⌨️ Email: `admin@system.local`
3. ⌨️ Mật khẩu: `Admin@123`
4. 🖱️ Bấm nút "Đăng nhập"
5. ✅ Kỳ vọng: Chuyển hướng về trang chủ hoặc dashboard, header hiển thị avatar/tên admin

### TC-AUTH02: Đăng nhập thành công — Teacher
1. 🧭 Mở `http://localhost:5173/login`
2. ⌨️ Email: `teacher@demo.local`
3. ⌨️ Mật khẩu: `Teacher@123`
4. 🖱️ Bấm "Đăng nhập"
5. ✅ Kỳ vọng: Đăng nhập thành công, header hiển thị tên GV

### TC-AUTH03: Đăng nhập thành công — Student
1. 🧭 Mở `http://localhost:5173/login`
2. ⌨️ Email: `student@demo.local`
3. ⌨️ Mật khẩu: `Student@123`
4. 🖱️ Bấm "Đăng nhập"
5. ✅ Kỳ vọng: Đăng nhập thành công

### TC-AUTH04: Đăng nhập sai mật khẩu
1. 🧭 Mở `http://localhost:5173/login`
2. ⌨️ Email: `admin@system.local`
3. ⌨️ Mật khẩu: `SaiMatKhau123`
4. 🖱️ Bấm "Đăng nhập"
5. ✅ Kỳ vọng: Hiển thị thông báo lỗi "Sai email hoặc mật khẩu" (hoặc tương đương), KHÔNG chuyển trang

### TC-AUTH05: Đăng nhập email không tồn tại
1. 🧭 Mở `http://localhost:5173/login`
2. ⌨️ Email: `emailkhongtontai@xyz.com`
3. ⌨️ Mật khẩu: `Test@12345`
4. 🖱️ Bấm "Đăng nhập"
5. ✅ Kỳ vọng: Hiển thị lỗi, không crash

### TC-AUTH06: Đăng nhập bỏ trống
1. 🧭 Mở `http://localhost:5173/login`
2. 🖱️ Bấm "Đăng nhập" mà KHÔNG nhập gì
3. ✅ Kỳ vọng: Hiển thị lỗi validation cho email và mật khẩu

### TC-AUTH07: Đăng ký Student — happy path
1. 🧭 Mở `http://localhost:5173/register`
2. ⌨️ Họ tên: `Test Student Manual`
3. ⌨️ Email: `teststudent_manual@university.edu.vn`
4. ⌨️ Mật khẩu: `TestPass@123`
5. ⌨️ Xác nhận mật khẩu: `TestPass@123`
6. 🖱️ Chọn vai trò "Sinh viên"
7. 🖱️ Tích checkbox "Đồng ý chính sách bảo mật"
8. 🖱️ Bấm "Đăng ký"
9. 🐛 **BUG ĐÃ BIẾT (B0 plan)**: Hiện tại KHÔNG có bước OTP xác minh email — đăng ký thẳng
10. ✅ Kỳ vọng (sau fix B0): Hiện form nhập OTP → nhập OTP → tạo tài khoản
11. ✅ Kỳ vọng (hiện tại): Đăng ký thành công, chuyển hướng về `/path`

### TC-AUTH08: Đăng ký Teacher — happy path
1. 🧭 Mở `http://localhost:5173/register`
2. ⌨️ Họ tên: `Test Teacher Manual`
3. ⌨️ Email: `testteacher_manual@university.edu.vn`
4. ⌨️ Mật khẩu: `TestPass@123`
5. ⌨️ Xác nhận mật khẩu: `TestPass@123`
6. 🖱️ Chọn vai trò "Giảng viên"
7. ⌨️ Khoa/Bộ môn: `Khoa Công nghệ thông tin`
8. ⌨️ Mã giảng viên: `GV_TEST_001`
9. 🖱️ Chọn Học vị: `Thạc sĩ` (tùy chọn)
10. ⌨️ Link hồ sơ: (để trống — optional)
11. ⌨️ Kinh nghiệm giảng dạy: `3 năm giảng dạy DSA` (tùy chọn)
12. 🖱️ Tích checkbox đồng ý chính sách
13. 🖱️ Bấm "Đăng ký"
14. ✅ Kỳ vọng: Hiển thị thông báo "Đăng ký thành công, tài khoản đang chờ duyệt" + link về login

### TC-AUTH09: Đăng ký Teacher — Khoa để trống (optional)
1. 🧭 Mở `http://localhost:5173/register`
2. Điền đầy đủ thông tin GV **NGOẠI TRỪ** Khoa/Bộ môn (để trống)
3. ⌨️ Mã giảng viên: `GV_TEST_002`
4. 🖱️ Bấm "Đăng ký"
5. 🐛 **BUG ĐÃ BIẾT (A2)**: Hiện tại sẽ báo lỗi "Vui lòng nhập Khoa/Bộ môn"
6. ✅ Kỳ vọng (sau fix A2): Đăng ký thành công dù Khoa để trống

### TC-AUTH10: Đăng ký Teacher — Mã cán bộ trùng
1. 🧭 Mở `http://localhost:5173/register`
2. Điền đầy đủ thông tin GV
3. ⌨️ Mã giảng viên: Nhập mã trùng với GV đã tồn tại (thử `GV_TEST_001` nếu TC-AUTH08 đã chạy)
4. 🖱️ Bấm "Đăng ký"
5. 🐛 **BUG ĐÃ BIẾT (A3)**: Hiện tại KHÔNG check trùng → đăng ký thành công dù mã trùng
6. ✅ Kỳ vọng (sau fix A3): Báo lỗi "Mã cán bộ đã tồn tại"

### TC-AUTH11: Đăng ký — validation edge cases
1. 🧭 Mở `http://localhost:5173/register`
2. **Test 1**: Họ tên chỉ 1 ký tự `A` → ✅ Kỳ vọng: Báo lỗi "Họ tên phải từ 2 ký tự"
3. **Test 2**: Email sai format `abc@` → ✅ Kỳ vọng: Báo lỗi "Email không hợp lệ"
4. **Test 3**: Mật khẩu yếu `123` → ✅ Kỳ vọng: Báo lỗi + checklist mật khẩu hiện đỏ
5. **Test 4**: Xác nhận mật khẩu không khớp → ✅ Kỳ vọng: Báo lỗi "Mật khẩu xác nhận không khớp"
6. **Test 5**: Không tích đồng ý chính sách → ✅ Kỳ vọng: Báo lỗi "Bạn phải đồng ý chính sách"
7. **Test 6**: Email đã tồn tại `student@demo.local` → ✅ Kỳ vọng: Báo lỗi email đã được sử dụng

### TC-AUTH12: Đăng ký Teacher — Mã giảng viên để trống
1. 🧭 Mở `http://localhost:5173/register`
2. Chọn vai trò "Giảng viên", điền đầy đủ NGOẠI TRỪ Mã giảng viên
3. 🖱️ Bấm "Đăng ký"
4. ✅ Kỳ vọng: Báo lỗi "Vui lòng nhập Mã giảng viên" (bắt buộc)

### TC-AUTH13: Đăng xuất
1. Đăng nhập bằng bất kỳ tài khoản nào
2. 🖱️ Bấm avatar/menu user ở header
3. 🖱️ Bấm "Đăng xuất"
4. ✅ Kỳ vọng: Chuyển về trang chủ, header hiện nút Đăng nhập/Đăng ký

---

## 5. PHẦN 3 — ROLE ADMIN

> Đăng nhập: `admin@system.local` / `Admin@123`

### TC-ADM01: Truy cập trang quản lý người dùng
1. 🧭 Mở `http://localhost:5173/admin/users`
2. 👁️ Kiểm tra:
   - Bảng danh sách người dùng hiển thị
   - Có cột: Tên, Email, Vai trò, Trạng thái, Thao tác
   - Có thanh tìm kiếm
   - Có tab "Tất cả" và "Chờ duyệt Teacher"
3. ✅ Kỳ vọng: Bảng hiển thị đầy đủ dữ liệu seed

### TC-ADM02: Tab "Chờ duyệt Teacher"
1. 🧭 Ở `/admin/users`
2. 🖱️ Bấm tab "Chờ duyệt Teacher"
3. 👁️ Kiểm tra: Hiển thị danh sách user có role TEACHER_PENDING (nếu có)
4. ✅ Kỳ vọng: Tab hoạt động, hiển thị đúng

### TC-ADM03: Duyệt Teacher (Approve)
> **Yêu cầu trước**: Đã đăng ký 1 tài khoản GV mới ở TC-AUTH08
1. 🧭 Ở `/admin/users` → tab "Chờ duyệt Teacher"
2. 🖱️ Tìm tài khoản GV vừa đăng ký
3. 🖱️ Bấm nút "Duyệt" (Approve)
4. 👁️ Modal phê duyệt hiện ra
5. 🖱️ Bấm xác nhận duyệt
6. 👁️ Kiểm tra: Danh sách cập nhật, user biến mất khỏi tab pending
7. ✅ Kỳ vọng: Duyệt thành công, role chuyển thành TEACHER

### TC-ADM04: Từ chối Teacher (Reject)
> Cần có GV pending để test. Nếu không có, đăng ký thêm 1 GV mới trước.
1. 🧭 Ở `/admin/users` → tab "Chờ duyệt Teacher"
2. 🖱️ Bấm nút "Từ chối" trên 1 user
3. ⌨️ Nhập lý do: `Hồ sơ chưa đầy đủ`
4. 🖱️ Bấm xác nhận
5. ✅ Kỳ vọng: User bị reject, role chuyển về STUDENT

### TC-ADM05: Tạo user mới từ Admin
1. 🧭 Ở `/admin/users`
2. 🖱️ Bấm nút "Tạo người dùng mới"
3. ⌨️ Điền thông tin:
   - Tên: `Admin Created User`
   - Email: `admincreated@test.local`
   - Mật khẩu: `Test@12345`
   - Vai trò: Teacher
   - Mã giảng viên: `GV_ADMIN_01`
4. 🖱️ Bấm tạo
5. ✅ Kỳ vọng: User mới xuất hiện trong danh sách

### TC-ADM06: Tạo Teacher KHÔNG nhập mã cán bộ
1. 🧭 Ở `/admin/users` → Tạo user mới
2. Chọn vai trò Teacher nhưng BỎ TRỐNG mã giảng viên
3. 🖱️ Bấm tạo
4. 🐛 **BUG ĐÃ BIẾT (A3)**: Hiện tại có thể cho tạo mà không bắt buộc StaffCode
5. ✅ Kỳ vọng (sau fix): Báo lỗi "Vui lòng nhập Mã giảng viên"

### TC-ADM07: Nút "Vô hiệu hóa" (trước đây là "Xóa")
1. 🧭 Ở `/admin/users`
2. 👁️ Kiểm tra cột thao tác:
   - 🐛 **BUG ĐÃ BIẾT (A4)**: Hiện tại hiện icon thùng rác đỏ với label "Xóa tài khoản"
   - ✅ Kỳ vọng (sau fix): Icon khác (Ban/ShieldOff) với label "Vô hiệu hóa tài khoản"
3. 🖱️ Bấm nút vô hiệu hóa trên 1 user (KHÔNG phải admin chính)
4. 👁️ Confirm dialog hiển thị
5. 🖱️ Xác nhận
6. ✅ Kỳ vọng: User bị disable (IsActive = false), hiện trạng thái "Đã khóa"

### TC-ADM08: Khóa / Mở khóa tài khoản
1. 🧭 Ở `/admin/users`
2. 🖱️ Bấm nút "Khóa" trên 1 user đang active
3. ✅ Kỳ vọng: Trạng thái chuyển thành "Đã khóa"
4. 🖱️ Bấm nút "Mở khóa" trên user vừa khóa
5. ✅ Kỳ vọng: Trạng thái chuyển lại "Hoạt động"

### TC-ADM09: Sửa thông tin user
1. 🧭 Ở `/admin/users`
2. 🖱️ Bấm vào 1 user để mở drawer chi tiết (hoặc bấm nút sửa)
3. ⌨️ Đổi tên hiển thị
4. 🖱️ Bấm Lưu
5. ✅ Kỳ vọng: Tên được cập nhật ngay trong bảng

### TC-ADM10: Sửa mã cán bộ trùng
1. 🧭 Ở `/admin/users` → sửa 1 Teacher
2. ⌨️ Đổi Mã giảng viên thành mã đã tồn tại của GV khác
3. 🖱️ Bấm Lưu
4. 🐛 **BUG ĐÃ BIẾT (A3)**: Hiện tại cho lưu dù trùng
5. ✅ Kỳ vọng (sau fix): Báo lỗi "Mã cán bộ đã tồn tại"

### TC-ADM11: Tìm kiếm user
1. 🧭 Ở `/admin/users`
2. ⌨️ Gõ vào thanh tìm kiếm: `Nguyễn`
3. ✅ Kỳ vọng: Lọc hiển thị chỉ user có tên chứa "Nguyễn"
4. ⌨️ Xóa search → gõ: `student@demo`
5. ✅ Kỳ vọng: Hiển thị user có email chứa chuỗi đó

### TC-ADM12: Trang thống kê Admin
1. 🧭 Mở `http://localhost:5173/admin/stats`
2. 👁️ Kiểm tra: Hiển thị các thẻ thống kê (tổng user, user active, GV, SV...)
3. ✅ Kỳ vọng: Số liệu hiển thị, không lỗi

### TC-ADM13: Trang cài đặt Admin
1. 🧭 Mở `http://localhost:5173/admin/settings`
2. 👁️ Kiểm tra: Các cài đặt hệ thống hiển thị (password policy, domains...)
3. ✅ Kỳ vọng: Trang load đúng

### TC-ADM14: Admin truy cập Studio
1. 🧭 Mở `http://localhost:5173/studio`
2. 👁️ Kiểm tra: Trang Studio/Content Management hiển thị
3. ✅ Kỳ vọng: Admin có quyền truy cập Studio

---

## 6. PHẦN 4 — ROLE TEACHER

> Đăng nhập: `teacher@demo.local` / `Teacher@123`

### 6.1 Teacher — Trang chủ

#### TC-T01: Nút "Bắt đầu hành trình" khi đã login
1. 🧭 Mở `http://localhost:5173/`
2. 🖱️ Bấm nút "Bắt đầu hành trình"
3. 🐛 **BUG ĐÃ BIẾT (C1)**: Hiện tại bấm không có phản hồi (redirect vòng lặp về home)
4. ✅ Kỳ vọng (sau fix): Chuyển đến `/path` (trang lộ trình)

### 6.2 Teacher — Lớp học (Classroom)

#### TC-T02: Xem danh sách lớp
1. 🧭 Mở `http://localhost:5173/classes`
2. 👁️ Kiểm tra: Trang lớp học hiển thị
3. ✅ Kỳ vọng: Hiển thị danh sách lớp (nếu có) hoặc nút tạo lớp mới

#### TC-T03: Tạo lớp mới
1. 🧭 Ở `/classes`
2. 🖱️ Bấm "Tạo lớp mới"
3. ⌨️ Nhập tên lớp: `Lớp Test Manual`
4. ⌨️ Nhập mô tả: `Lớp dùng để test`
5. 🖱️ Bấm tạo
6. ✅ Kỳ vọng: Lớp được tạo, hiển thị mã tham gia (join code)

#### TC-T04: Vào chi tiết lớp
1. 🖱️ Bấm vào lớp vừa tạo (hoặc lớp có sẵn)
2. 👁️ Kiểm tra:
   - Tên lớp, mô tả hiển thị
   - Có tab: Thành viên, Bài tập, Lộ trình, Báo cáo
   - Mã tham gia lớp hiển thị
3. ✅ Kỳ vọng: Tất cả thông tin hiển thị đúng

#### TC-T05: Gán bài tập cho lớp
1. 🧭 Trong chi tiết lớp → tab Bài tập
2. 🖱️ Bấm "Thêm bài tập" hoặc "Gán bài"
3. Chọn 1 bài tập/quiz có sẵn
4. ⌨️ Đặt hạn nộp (deadline): ngày mai
5. 🖱️ Bấm xác nhận
6. 👁️ Kiểm tra: Bài tập xuất hiện trong danh sách
7. 🐛 **BUG ĐÃ BIẾT (C2)**: Có thể phải F5 mới thấy bài tập mới
8. ✅ Kỳ vọng: Bài tập hiện ngay sau khi gán, KHÔNG cần F5

#### TC-T06: Sửa deadline bài tập
1. 🧭 Trong danh sách bài tập đã gán
2. 🖱️ Bấm nút sửa (pencil icon) trên 1 bài tập
3. ⌨️ Đổi deadline sang ngày khác
4. 🖱️ Bấm Lưu
5. 🐛 **BUG ĐÃ BIẾT (C3)**: Deadline không lưu được + phải F5
6. ✅ Kỳ vọng: Deadline cập nhật ngay trên UI

#### TC-T07: Xóa deadline bài tập (để trống)
1. 🖱️ Bấm sửa bài tập → xóa trống deadline
2. 🖱️ Bấm Lưu
3. 🐛 **BUG ĐÃ BIẾT (C3)**: Backend bỏ qua DueAt = null
4. ✅ Kỳ vọng: Deadline bị xóa, hiển thị "Không có hạn"

#### TC-T08: Xóa bài tập đã gán
1. 🖱️ Bấm nút xóa (trash icon) trên 1 bài tập
2. 🖱️ Xác nhận xóa
3. 🐛 **BUG ĐÃ BIẾT (C2)**: Có thể phải F5
4. ✅ Kỳ vọng: Bài tập biến mất ngay, KHÔNG cần F5

#### TC-T09: GV bấm vào bài tập — xem trước
1. 🖱️ Bấm vào 1 bài tập đã gán (nút "Làm bài" hoặc tên bài)
2. 🐛 **BUG ĐÃ BIẾT (C4)**: GV vẫn làm bài như student, không có chế độ xem trước
3. ✅ Kỳ vọng (sau fix): GV xem trước bài + thấy thống kê % hoàn thành của học sinh

#### TC-T10: Mở lịch sử làm bài → bấm X đóng
1. 🖱️ Bấm "Lịch sử làm bài" trên 1 bài tập
2. 👁️ Drawer/modal mở ra
3. 🖱️ Bấm nút X (góc trên phải) để đóng
4. 🐛 **BUG ĐÃ BIẾT (C5)**: Nút X không hoạt động (bị vaul-vue nuốt event)
5. ✅ Kỳ vọng: Drawer đóng lại khi bấm X

#### TC-T11: Lộ trình lớp — thông tin ban đầu
1. 🧭 Trong chi tiết lớp → tab Lộ trình
2. 👁️ Kiểm tra badge trạng thái lộ trình
3. 🐛 **BUG ĐÃ BIẾT (C6)**: Lớp mới tạo nhưng badge hiện "Đã xuất bản" dù chưa đặt tên
4. ✅ Kỳ vọng (sau fix): Badge hiện "Nháp" cho lớp mới

#### TC-T12: Lộ trình lớp — đặt tên + xuất bản
1. ⌨️ Nhập tên lộ trình: `Lộ trình DSA cơ bản`
2. ⌨️ Nhập mô tả: `Học DSA từ đầu`
3. 🖱️ Bấm nút "Xuất bản"
4. 🐛 **BUG ĐÃ BIẾT (C7)**: Nút xuất bản chỉ gửi published:true, không gửi tên
5. ✅ Kỳ vọng (sau fix): Tên + trạng thái "Đã xuất bản" cập nhật cùng lúc

#### TC-T13: Lộ trình lớp — thêm nội dung
1. 🖱️ Bấm "Thêm nội dung vào lộ trình"
2. Chọn 1 hoặc nhiều bài học
3. 🖱️ Xác nhận
4. 👁️ Kiểm tra: Bài học xuất hiện trong lộ trình
5. ✅ Kỳ vọng: Nội dung hiện ngay, không cần F5

#### TC-T14: Lộ trình lớp — sắp xếp lại thứ tự
1. 🖱️ Kéo thả hoặc bấm nút đẩy lên/xuống bài học trong lộ trình
2. 🐛 **BUG ĐÃ BIẾT (C2)**: Phải F5 mới thấy thay đổi
3. ✅ Kỳ vọng: Thứ tự cập nhật ngay

#### TC-T15: Báo cáo lớp — xuất CSV
1. 🧭 Vào báo cáo lớp: bấm vào tab/link "Báo cáo" trong chi tiết lớp
2. 🖱️ Bấm "Xuất CSV"
3. 👁️ Kiểm tra file CSV tải về
4. 🐛 **BUG ĐÃ BIẾT (C8)**: CSV ghép 3 bảng khác schema → lệch cột khi mở Excel
5. ✅ Kỳ vọng (sau fix): CSV có cấu trúc nhất quán

#### TC-T16: Báo cáo lớp — in ra
1. 🧭 Ở trang báo cáo lớp
2. 🖱️ Bấm "In báo cáo"
3. 👁️ Kiểm tra preview print
4. 🐛 **BUG ĐÃ BIẾT (C9)**: In ra có header/nav/buttons, nền tối
5. ✅ Kỳ vọng (sau fix): Chỉ in nội dung, nền trắng chữ đen

### 6.3 Teacher — Lộ trình (Roadmap)

#### TC-T17: Xem danh sách lộ trình
1. 🧭 Mở `http://localhost:5173/path`
2. 👁️ Kiểm tra footer hiển thị
3. 🐛 **BUG ĐÃ BIẾT (D3)**: Footer không hiển thị
4. ✅ Kỳ vọng (sau fix): Footer hiển thị bình thường

#### TC-T18: Vào chi tiết lộ trình
1. 🖱️ Bấm vào 1 lộ trình
2. 👁️ Kiểm tra: Trang chi tiết hiển thị modules/bài học
3. ✅ Kỳ vọng: Nội dung đúng

#### TC-T19: Bấm nội dung lộ trình mà CHƯA đăng ký (enroll)
1. 🧭 Vào chi tiết 1 lộ trình chưa enroll
2. 🖱️ Bấm vào 1 bài học (KHÔNG bấm "Tham gia lộ trình")
3. 🐛 **BUG ĐÃ BIẾT (D1)**: Vào học được + nhận XP mà không cần enroll
4. ✅ Kỳ vọng (sau fix): Bị chặn, hiện modal "Tham gia lộ trình trước"

#### TC-T20: Đăng ký lộ trình và bắt đầu học
1. 🖱️ Bấm "Tham gia lộ trình" (tốn 1 tim)
2. 🖱️ Bấm "Bắt đầu học"
3. ✅ Kỳ vọng: Chuyển đến bài học đầu tiên

#### TC-T21: Học bài → hoàn thành → nhận XP
1. Đang ở bài học → đọc/scroll hết nội dung
2. 🖱️ Bấm "Hoàn thành"
3. ✅ Kỳ vọng: XP được cộng, bài tiếp theo unlock

#### TC-T22: Mini quiz — navigation buttons
1. 🧭 Vào 1 bài học có mini quiz
2. Làm quiz đến câu cuối
3. 👁️ Kiểm tra:
   - Nút "Câu trước" (lùi): hiển thị
   - Nút "Câu tiếp" (tiến): hiển thị
4. 🐛 **BUG ĐÃ BIẾT (D2)**: Nút tiến bị ẩn/invisible
5. ✅ Kỳ vọng (sau fix): Cả 2 nút visible, nút tiến disabled khi ở câu cuối

### 6.4 Teacher — Mô phỏng

#### TC-T23: Xem danh sách mô phỏng
1. 🧭 Mở `http://localhost:5173/simulations`
2. 👁️ Kiểm tra: Catalog hiển thị
3. ✅ Kỳ vọng: Hiển thị đúng

#### TC-T24: Tìm kiếm — tiếng Việt không dấu
1. ⌨️ Gõ: `cay`
2. 🐛 **BUG ĐÃ BIẾT (E2)**: Không tìm thấy (phải gõ đúng "Cây")
3. ✅ Kỳ vọng (sau fix): Match "Cây nhị phân", "Cây AVL"...
4. ⌨️ Xóa → gõ: `sap xep`
5. ✅ Kỳ vọng (sau fix): Match "Sắp xếp nổi bọt"...

#### TC-T25: Chuyển trang → scroll top
1. Scroll xuống cuối trang `/simulations`
2. 🖱️ Chuyển tab/trang
3. 🐛 **BUG ĐÃ BIẾT (E1)**: Không scroll về đầu
4. ✅ Kỳ vọng (sau fix): Auto scroll top

#### TC-T26: Xem mô phỏng cụ thể
1. 🖱️ Bấm vào 1 thuật toán
2. 👁️ Kiểm tra visualization hoạt động
3. ✅ Kỳ vọng: Mô phỏng chạy đúng

#### TC-T27: CheatSheet — xem
1. 🧭 Mở `http://localhost:5173/cheatsheet`
2. 👁️ Kiểm tra bảng Big-O
3. ✅ Kỳ vọng: Hiển thị đúng

#### TC-T28: CheatSheet — in/PDF
1. 🖱️ Bấm In/Export PDF
2. 🐛 **BUG ĐÃ BIẾT (E3)**: Nền tối, chữ trắng, header in ra
3. ✅ Kỳ vọng (sau fix): Nền trắng, chữ đen, chỉ nội dung

### 6.5 Teacher — Thử thách (Code Runner)

#### TC-T29: Mở code runner
1. 🧭 Mở `http://localhost:5173/code/bubble-sort`
2. 👁️ Kiểm tra: Editor + nút Chạy hiển thị
3. ✅ Kỳ vọng: Trang đúng

#### TC-T30: Chạy code — progress bar
1. 🖱️ Bấm "Chạy"
2. 🐛 **BUG ĐÃ BIẾT (F1)**: Chỉ spinner nhỏ, không có progress bar
3. ✅ Kỳ vọng (sau fix): Thanh progress bar xám khi đang chạy

#### TC-T31: Chạy code → xem history
1. Code chạy xong
2. 🖱️ Bấm "Lịch sử"
3. 🐛 **BUG ĐÃ BIẾT (F2)**: History trống
4. ✅ Kỳ vọng (sau fix): Kết quả vừa chạy hiển thị

#### TC-T32: Chạy code lỗi
1. ⌨️ Nhập code sai: `function solve( { return`
2. 🖱️ Bấm "Chạy"
3. ✅ Kỳ vọng: Thông báo lỗi, không crash

### 6.6 Teacher — Studio

#### TC-T33: Truy cập Studio
1. 🧭 Mở `http://localhost:5173/studio`
2. 👁️ Kiểm tra tabs hiển thị
3. ✅ Kỳ vọng: Trang load đúng

#### TC-T34: Đổi tab KHÔNG reload
1. 🖱️ Bấm đổi qua các tab
2. 🐛 **BUG ĐÃ BIẾT (G1)**: Trang flash/reload
3. ✅ Kỳ vọng (sau fix): Đổi tab mượt mà

#### TC-T35: Chọn roadmap khác
1. 🖱️ Chọn roadmap khác trong dropdown
2. 🐛 **BUG ĐÃ BIẾT (G3)**: Luôn hiện "Cấu trúc dữ liệu"
3. ✅ Kỳ vọng (sau fix): Hiện đúng cây bài giảng

#### TC-T36: Tìm kiếm bài giảng
1. ⌨️ Gõ: `cay`
2. 🐛 **BUG ĐÃ BIẾT (G2)**: Không tìm thấy
3. ✅ Kỳ vọng (sau fix): Match bài "Cây"

#### TC-T37: Thêm chương mới
1. 🖱️ Bấm "Thêm chương"
2. ⌨️ Tên: `Chương Test`
3. 🖱️ Lưu
4. 🐛 **BUG ĐÃ BIẾT (G3)**: Có thể 403
5. ✅ Kỳ vọng (sau fix): Tạo thành công

#### TC-T38: Xuất bản bài giảng mới
1. Tạo bài mới → chọn "Công khai"
2. 🖱️ Lưu
3. 🐛 **BUG ĐÃ BIẾT (G3)**: Backend override thành PendingReview
4. ✅ Kỳ vọng (sau fix cho classroom-only): Xuất bản thành công

#### TC-T39: Xem trang học viên
1. 🖱️ Bấm "Xem trang học viên"
2. 🐛 **BUG ĐÃ BIẾT (G3)**: Auto nhảy về Grokking Algorithms
3. ✅ Kỳ vọng (sau fix): Mở đúng bài + roadmap

#### TC-T40: Quiz bank hiển thị
1. 🖱️ Chuyển tab "Ngân hàng Quiz"
2. 🐛 **BUG ĐÃ BIẾT (G4)**: Luôn hiện 20 quiz
3. ✅ Kỳ vọng (sau fix): Quiz theo roadmap đã chọn

#### TC-T41: Sửa quiz → Hủy (rollback)
1. 🖱️ Sửa 1 quiz (đáp án A)
2. ⌨️ Đổi thành B
3. 🖱️ Bấm "Hủy"
4. 🖱️ Mở lại quiz
5. 🐛 **BUG ĐÃ BIẾT (G4)**: Đáp án đã thành B (auto-save)
6. ✅ Kỳ vọng (sau fix): Đáp án vẫn A

#### TC-T42: Tạo quiz mới
1. 🖱️ "Tạo Quiz mới"
2. Điền nội dung
3. 🖱️ Lưu
4. 🐛 **BUG ĐÃ BIẾT (G4)**: Có thể 403
5. ✅ Kỳ vọng (sau fix): Tạo thành công

#### TC-T43: Xóa quiz
1. 🖱️ Xóa 1 quiz
2. 🐛 **BUG ĐÃ BIẾT (G4)**: 403
3. ✅ Kỳ vọng (sau fix): Xóa thành công

#### TC-T44: "Quản lý lớp học" link
1. 🖱️ Bấm "Quản lý lớp học" trong Studio
2. 🐛 **BUG ĐÃ BIẾT (G5)**: Nhảy ra `/classes`, mất context
3. ✅ Kỳ vọng (sau fix): Mở tab mới hoặc sub-view

---

## 7. PHẦN 5 — ROLE STUDENT

> Đăng nhập: `student@demo.local` / `Student@123`

### 7.1 Student — Trang chủ

#### TC-S01: Trang chủ Student
1. 🧭 Mở `http://localhost:5173/`
2. 👁️ Kiểm tra: Header có avatar, tên, XP, streak, tim, gems
3. ✅ Kỳ vọng: Dashboard đúng

#### TC-S02: Nút "Bắt đầu hành trình"
1. 🖱️ Bấm nút
2. 🐛 **BUG (C1)**: Không phản hồi
3. ✅ Kỳ vọng (sau fix): Chuyển đến `/path`

### 7.2 Student — Lộ trình

#### TC-S03: Danh sách lộ trình + footer
1. 🧭 Mở `http://localhost:5173/path`
2. 🐛 **BUG (D3)**: Footer ẩn
3. ✅ Kỳ vọng: Footer hiện

#### TC-S04: Đăng ký lộ trình
1. 🖱️ Bấm "Tham gia lộ trình"
2. ✅ Kỳ vọng: Thành công, trừ 1 tim

#### TC-S05: Học bài đầu tiên
1. 🖱️ "Bắt đầu học" → đọc hết
2. 🖱️ Hoàn thành
3. ✅ Kỳ vọng: Nhận XP, bài tiếp unlock

#### TC-S06: Bài khóa đúng cách
1. 👁️ Kiểm tra bài chưa hoàn thành bài trước
2. ✅ Kỳ vọng: Hiện khóa, không bấm được

#### TC-S07: Mini quiz — làm bài
1. Vào bài có quiz → trả lời → nộp
2. ✅ Kỳ vọng: Kết quả đúng/sai, điểm số

### 7.3 Student — Lớp học

#### TC-S08: Join lớp bằng mã đúng
1. 🧭 Mở `http://localhost:5173/classes`
2. ⌨️ Nhập join code từ lớp GV tạo
3. 🖱️ Tham gia
4. ✅ Kỳ vọng: Thành công

#### TC-S09: Join lớp mã sai
1. ⌨️ Nhập: `XXXXXX`
2. 🖱️ Tham gia
3. ✅ Kỳ vọng: Lỗi "Mã lớp không hợp lệ"

#### TC-S10: Xem lộ trình lớp
1. Vào lớp → tab Lộ trình
2. ✅ Kỳ vọng: Nội dung hiển thị (nếu GV đã thêm)

### 7.4 Student — Mô phỏng & Code

#### TC-S11: Xem mô phỏng
1. 🧭 `/simulations` → bấm 1 thuật toán
2. ✅ Kỳ vọng: Visualization hoạt động

#### TC-S12: Chạy code
1. 🧭 `/code/bubble-sort` → Chạy
2. ✅ Kỳ vọng: Kết quả hiển thị

### 7.5 Student — Profile & Settings

#### TC-S13: Xem Profile
1. 🧭 Mở `http://localhost:5173/profile`
2. 👁️ Kiểm tra: Tên, email, avatar, stats, tabs
3. ✅ Kỳ vọng: Đầy đủ

#### TC-S14: Đổi tên
1. Tab Cài đặt → đổi tên → Lưu
2. ✅ Kỳ vọng: Cập nhật

#### TC-S15: Đổi mật khẩu
1. Mật khẩu cũ: `Student@123` → mới: `NewPass@456`
2. ✅ Kỳ vọng: Thành công
3. Đăng xuất → login lại bằng mật khẩu mới → ✅
4. **Cleanup**: Đổi lại `Student@123`

#### TC-S16: 2FA Toggle
1. Tab Cài đặt → tìm phần 2FA
2. 🐛 **BUG (B1)**: CHƯA CÓ UI
3. ✅ Kỳ vọng (sau fix): Toggle bật → OTP email → bật thành công

### 7.6 Student — Gamification

#### TC-S17: Leaderboard
1. 🧭 `/leaderboard`
2. ✅ Kỳ vọng: BXH hiển thị

#### TC-S18: Quests
1. 🧭 `/quests`
2. ✅ Kỳ vọng: Nhiệm vụ hiển thị

#### TC-S19: Shop
1. 🧭 `/shop`
2. ✅ Kỳ vọng: Shop hiển thị

#### TC-S20: Premium
1. 🧭 `/premium`
2. ✅ Kỳ vọng: Trang premium hiển thị

---

## 8. PHẦN 6 — CROSS-ROLE & EDGE CASES

### TC-X01: Student truy cập Admin
1. Login `student@demo.local`
2. 🧭 Mở `http://localhost:5173/admin/users`
3. ✅ Kỳ vọng: Bị chặn

### TC-X02: Student truy cập Studio
1. 🧭 Mở `http://localhost:5173/studio`
2. ✅ Kỳ vọng: Bị chặn

### TC-X03: Teacher truy cập Admin Users
1. Login `teacher@demo.local`
2. 🧭 Mở `http://localhost:5173/admin/users`
3. ✅ Kỳ vọng: Bị chặn

### TC-X04: Routes đã xóa → 404
1. 🧭 `http://localhost:5173/benchmark/bubble-sort/quick-sort`
2. 🐛 **BUG (H1)**: Route vẫn hoạt động
3. ✅ Kỳ vọng (sau fix): 404
4. 🧭 `http://localhost:5173/playground/code-to-visual`
5. ✅ Kỳ vọng (sau fix): 404
6. 🧭 `http://localhost:5173/graph-playground`
7. ✅ Kỳ vọng (sau fix): 404

### TC-X05: Spam click — tạo lớp
1. Login Teacher → `/classes`
2. 🖱️ Bấm "Tạo lớp" nhanh 5 lần
3. ✅ Kỳ vọng: Chỉ tạo 1 lớp

### TC-X06: Spam click — đăng ký lộ trình
1. Vào lộ trình → bấm "Tham gia" nhanh 3 lần
2. ✅ Kỳ vọng: Chỉ trừ 1 tim

### TC-X07: XSS injection
1. 🧭 `/register`
2. ⌨️ Họ tên: `<script>alert('xss')</script>`
3. Đăng ký thành công
4. ✅ Kỳ vọng: Tên lưu text thuần, KHÔNG thực thi script

### TC-X08: URL lesson ID sai
1. 🧭 `http://localhost:5173/lessons/99999`
2. ✅ Kỳ vọng: Lỗi "Không tồn tại" hoặc 404

### TC-X09: Mạng chậm
1. DevTools → Network → Slow 3G
2. Load `/simulations`
3. ✅ Kỳ vọng: Loading state (spinner/skeleton), không trắng trang

### TC-X10: Responsive 768px
1. Thu browser xuống 768px width
2. ✅ Kỳ vọng: Layout không vỡ

### TC-X11: Console errors
1. DevTools → Console
2. Navigate: `/` → `/path` → `/simulations` → `/profile` → `/classes` → `/studio`
3. ✅ Kỳ vọng: Không có uncaught exception hoặc 500 error

---

## 9. BẢNG TỔNG HỢP CHECKLIST

> Điền kết quả sau mỗi test case.

| ID | Mô tả ngắn | Bug đã biết | Trạng thái | Ghi chú |
|----|-------------|-------------|------------|---------|
| **GUEST** | | | | |
| TC-G01 | Trang chủ hiển thị | | PASS | Trang chủ hiển thị đầy đủ banner, demo engine, catalog |
| TC-G02 | Nút bắt đầu (guest) | | PASS | Nút CTA trỏ tới `/register` khi chưa đăng nhập |
| TC-G03 | Redirect trang auth | | PASS | Bấm route cần auth (`/profile`, `/studio`) chuyển hướng sang `/login` kèm `redirect` |
| TC-G04 | Lộ trình + footer | D3 | PASS | Lộ trình hiển thị danh sách khóa học và AppFooter xuất hiện đầy đủ (Đã fix D3) |
| TC-G05 | Simulator công khai | | PASS | Simulator key demo công khai tương tác chạy mượt mà |
| TC-G06 | Trang 404 | | PASS | URL không tồn tại hiển thị màn hình 404 NotFoundView và nút về trang chủ |
| TC-G07 | Quên mật khẩu | | PASS | Form Quên mật khẩu nhập email hợp lệ gửi yêu cầu đặt lại mật khẩu thành công |
| TC-G08 | Privacy | | PASS | Trang Chính sách bảo mật hiển thị đầy đủ nội dung |
| TC-G09 | Help | | PASS | Trang Trợ giúp & FAQ hiển thị đầy đủ |
| **AUTH** | | | | |
| TC-AUTH01 | Login Admin | | PASS | Login tài khoản Admin `admin@system.local` chuyển hướng đúng và lưu token |
| TC-AUTH02 | Login Teacher | | PASS | Login tài khoản Teacher `teacher@demo.local` chuyển hướng đúng |
| TC-AUTH03 | Login Student | | PASS | Login tài khoản Student `student@demo.local` chuyển hướng đúng |
| TC-AUTH04 | Login sai pass | | PASS | Nhập mật khẩu sai hiển thị toast báo lỗi xác thực |
| TC-AUTH05 | Login email sai | | PASS | Nhập email không tồn tại báo lỗi phù hợp |
| TC-AUTH06 | Login bỏ trống | | PASS | Bỏ trống email/password bị validate chặn tại client |
| TC-AUTH07 | Đăng ký Student | B0 | PASS | Đăng ký Student với OTP email kích hoạt tài khoản thành công (B0) |
| TC-AUTH08 | Đăng ký Teacher | | PASS | Đăng ký Teacher với đầy đủ thông tin vào trạng thái chờ duyệt (TEACHER_PENDING) |
| TC-AUTH09 | ĐK Teacher Khoa trống | A2 | PASS | Đăng ký Teacher với Khoa bỏ trống thành công (Đã fix A2) |
| TC-AUTH10 | ĐK Teacher mã trùng | A3 | PASS | Đăng ký Teacher với StaffCode trùng báo lỗi mã cán bộ đã tồn tại (Đã fix A3) |
| TC-AUTH11 | Validation edges | | PASS | Kiểm tra validation password yếu, email sai format báo lỗi rõ ràng |
| TC-AUTH12 | Mã GV trống | | PASS | Đăng ký Teacher thiếu StaffCode bị chặn validate |
| TC-AUTH13 | Đăng xuất | | PASS | Đăng xuất xóa token và cập nhật trạng thái Guest |
| **ADMIN** | | | | |
| TC-ADM01 | Trang quản lý users | | PASS | Trang Admin Users hiển thị danh sách người dùng, bộ lọc vai trò và phân trang |
| TC-ADM02 | Tab chờ duyệt | | PASS | Tab chờ duyệt hiển thị danh sách giáo viên TEACHER_PENDING |
| TC-ADM03 | Duyệt Teacher | | PASS | Duyệt tài khoản Teacher thành công và kích hoạt vai trò TEACHER |
| TC-ADM04 | Từ chối Teacher | | PASS | Từ chối tài khoản Teacher kèm lý do cập nhật trạng thái từ chối |
| TC-ADM05 | Tạo user mới | | PASS | Tạo user mới từ Admin thành công |
| TC-ADM06 | Tạo Teacher thiếu mã | A3 | PASS | Tạo Teacher thiếu StaffCode báo lỗi yêu cầu nhập mã giảng viên (Đã fix A3) |
| TC-ADM07 | Nút Vô hiệu hóa | A4 | PASS | Nút Vô hiệu hóa tài khoản hiển thị icon Ban và xác nhận khóa tài khoản rõ ràng (Đã fix A4) |
| TC-ADM08 | Khóa/Mở khóa | | PASS | Khóa/Mở khóa tài khoản người dùng hoạt động tức thì |
| TC-ADM09 | Sửa user | | PASS | Sửa thông tin người dùng cập nhật thành công |
| TC-ADM10 | Sửa mã CB trùng | A3 | PASS | Sửa StaffCode trùng báo lỗi từ backend |
| TC-ADM11 | Tìm kiếm user | | PASS | Tìm kiếm user theo tên/email mượt mà |
| TC-ADM12 | Thống kê | | PASS | Thống kê hệ thống hiển thị đầy đủ biểu đồ và số liệu KPI |
| TC-ADM13 | Cài đặt | | PASS | Cài đặt hệ thống lưu cấu hình thành công |
| TC-ADM14 | Admin vào Studio | | PASS | Admin truy cập Studio toàn quyền quản lý nội dung |
| **TEACHER** | | | | |
| TC-T01 | Nút bắt đầu (login) | C1 | PASS | Nút CTA Hero chuyển hướng tới `/path` hoặc `/studio` khi đã đăng nhập (Đã fix C1) |
| TC-T02 | Danh sách lớp | | PASS | Danh sách lớp học hiển thị đầy đủ các lớp giáo viên phụ trách |
| TC-T03 | Tạo lớp mới | | PASS | Tạo lớp học mới tạo mã tham gia ngẫu nhiên thành công |
| TC-T04 | Chi tiết lớp | | PASS | Chi tiết lớp học hiển thị tab Thành viên, Bài tập, Lộ trình, Báo cáo |
| TC-T05 | Gán bài tập | C2 | PASS | Gán bài tập vào lớp học thành công (Đã fix C2) |
| TC-T06 | Sửa deadline | C3 | PASS | Sửa deadline bài tập cập nhật thời gian mới thành công (Đã fix C3) |
| TC-T07 | Xóa deadline | C3 | PASS | Xóa deadline bài tập (DueAt: null) lưu thành công không còn deadline (Đã fix C3) |
| TC-T08 | Xóa bài tập | C2 | PASS | Xóa bài tập khỏi lớp học thành công |
| TC-T09 | GV xem trước bài | C4 | PASS | Giáo viên xem trước bài học mẫu và nội dung markdown |
| TC-T10 | Đóng drawer X | C5 | PASS | Nút đóng X trên drawer đóng drawer mượt mà (Đã fix C5) |
| TC-T11 | Lộ trình badge | C6 | PASS | Badge trạng thái lộ trình lớp học hiển thị Bản nháp / Đã xuất bản (Đã fix C6) |
| TC-T12 | Lộ trình xuất bản | C7 | PASS | Xuất bản lộ trình gửi đồng thời tiêu đề và mô tả thành công (Đã fix C7) |
| TC-T13 | Thêm nội dung | | PASS | Thêm nội dung bài học vào lộ trình lớp học |
| TC-T14 | Sắp xếp lộ trình | C2 | PASS | Sắp xếp thứ tự bài học trong lộ trình lưu đúng thứ tự |
| TC-T15 | CSV báo cáo | C8 | PASS | Xuất báo cáo kết quả học tập lớp ra file CSV thành công (Đã fix C8) |
| TC-T16 | Print báo cáo | C9 | PASS | In báo cáo học tập áp dụng @media print nền trắng chữ đen sạch sẽ (Đã fix C9) |
| TC-T17 | Lộ trình + footer | D3 | PASS | Xem trang lộ trình `/path` có footer xuất hiện đầy đủ (Đã fix D3) |
| TC-T18 | Chi tiết lộ trình | | PASS | Xem chi tiết lộ trình hiển thị cây bài học và tiến độ |
| TC-T19 | Vào bài chưa enroll | D1 | PASS | Bấm vào bài học khi chưa enroll mở modal Tham gia lộ trình (Đã fix D1) |
| TC-T20 | Đăng ký + bắt đầu | | PASS | Đăng ký lộ trình trừ tim và mở khóa bắt đầu học bài |
| TC-T21 | Hoàn thành → XP | | PASS | Hoàn thành bài học ghi nhận tiến độ và cộng XP |
| TC-T22 | Quiz nav buttons | D2 | PASS | Nút điều hướng Mini Quiz hiển thị rõ ràng, nút tiếp disabled ở câu cuối (Đã fix D2) |
| TC-T23 | Danh sách mô phỏng | | PASS | Danh sách mô phỏng trực quan hiển thị đầy đủ danh mục thuật toán |
| TC-T24 | Search tiếng Việt | E2 | PASS | Tìm kiếm thuật toán tiếng Việt không dấu thành công (Đã fix E2) |
| TC-T25 | Scroll top | E1 | PASS | Chuyển tab trong mô phỏng tự động scroll lên đầu trang (Đã fix E1) |
| TC-T26 | Xem mô phỏng | | PASS | Chạy mô phỏng thuật toán trực quan với canvas tương tác từng bước |
| TC-T27 | CheatSheet xem | | PASS | Xem bảng tra cứu độ phức tạp thuật toán CheatSheet |
| TC-T28 | CheatSheet in | E3 | PASS | In CheatSheet áp dụng @media print nền trắng chữ đen (Đã fix E3) |
| TC-T29 | Code runner mở | | PASS | Mở Code Runner editor kèm nút Chạy và phím tắt Ctrl+Enter |
| TC-T30 | Code progress bar | F1 | PASS | Chạy code hiển thị thanh progress bar trực quan trong lúc thực thi (Đã fix F1) |
| TC-T31 | Code history | F2 | PASS | Lịch sử chạy code lưu và hiển thị danh sách các lần chạy thành công/lỗi (Đã fix F2) |
| TC-T32 | Code lỗi | | PASS | Chạy code có lỗi cú pháp báo lỗi rõ ràng, an toàn không crash app |
| TC-T33 | Studio truy cập | | PASS | Truy cập Teacher Studio hiển thị các thẻ phân hệ quản lý |
| TC-T34 | Studio đổi tab | G1 | PASS | Đổi tab trong Studio đồng bộ URL query không reload trang (Đã fix G1) |
| TC-T35 | Studio chọn roadmap | G3 | PASS | Chọn lộ trình khác trong Studio cập nhật cây bài giảng tương ứng (Đã fix G3) |
| TC-T36 | Studio tìm kiếm | G2 | PASS | Tìm kiếm bài giảng tiếng Việt không dấu trong Studio thành công (Đã fix G2) |
| TC-T37 | Studio thêm chương | G3 | PASS | Thêm chương/chủ đề mới trong Studio thành công |
| TC-T38 | Studio xuất bản bài | G3 | PASS | Xuất bản bài giảng mới lưu trạng thái đúng |
| TC-T39 | Studio xem học viên | G3 | PASS | Xem trang học viên mở đúng bài giảng và lộ trình tương ứng |
| TC-T40 | Quiz bank hiển thị | G4 | PASS | Ngân hàng câu hỏi Quiz hiển thị danh sách câu hỏi theo bộ lọc |
| TC-T41 | Quiz sửa + Hủy | G4 | PASS | Chỉnh sửa Quiz rồi bấm Hủy rollback về dữ liệu gốc, không lưu đè (Đã fix G4) |
| TC-T42 | Quiz tạo mới | G4 | PASS | Tạo câu hỏi Quiz mới thành công |
| TC-T43 | Quiz xóa | G4 | PASS | Xóa câu hỏi Quiz khỏi ngân hàng thành công |
| TC-T44 | Quản lý lớp link | G5 | PASS | Link Quản lý lớp học trong Studio mở tab mới giữ nguyên context (Đã fix G5) |
| **STUDENT** | | | | |
| TC-S01 | Trang chủ | | PASS | Trang chủ học viên hiển thị tiến độ và các lộ trình đề xuất |
| TC-S02 | Nút bắt đầu | C1 | PASS | Nút Bắt đầu hành trình chuyển hướng tới `/path` khi đã đăng nhập (Đã fix C1) |
| TC-S03 | Lộ trình + footer | D3 | PASS | Trang lộ trình `/path` hiển thị footer chuẩn (Đã fix D3) |
| TC-S04 | Đăng ký lộ trình | | PASS | Tham gia lộ trình học thành công và lưu vào danh sách lộ trình của tôi |
| TC-S05 | Học bài | | PASS | Học bài giảng lý thuyết kèm mô phỏng thuật toán |
| TC-S06 | Bài khóa | | PASS | Bài học bị khóa ngăn chặn truy cập và yêu cầu hoàn thành bài trước |
| TC-S07 | Mini quiz | | PASS | Làm bài Mini Quiz tính điểm và giải thích chi tiết từng đáp án |
| TC-S08 | Join lớp đúng mã | | PASS | Tham gia lớp học với mã mời hợp lệ thành công |
| TC-S09 | Join lớp sai mã | | PASS | Nhập mã mời lớp học sai hiển thị thông báo lỗi lớp không tồn tại |
| TC-S10 | Lộ trình lớp | | PASS | Xem lộ trình lớp học hiển thị nội dung giáo viên đã xuất bản |
| TC-S11 | Xem mô phỏng | | PASS | Xem và tương tác với các mô phỏng giải thuật |
| TC-S12 | Chạy code | | PASS | Chạy thử nghiệm code trong Code Runner sandbox |
| TC-S13 | Profile | | PASS | Xem hồ sơ cá nhân, cấp độ, chuỗi ngày streak và radar kỹ năng |
| TC-S14 | Đổi tên | | PASS | Cập nhật họ tên hiển thị trong trang Cài đặt thành công |
| TC-S15 | Đổi mật khẩu | | PASS | Đổi mật khẩu tài khoản thành công |
| TC-S16 | 2FA toggle | B1 | PASS | Bật/tắt 2FA với mã OTP xác thực email trong Cài đặt thành công (Đã fix B1) |
| TC-S17 | Leaderboard | | PASS | Bảng xếp hạng hiển thị thứ hạng người dùng theo XP |
| TC-S18 | Quests | | PASS | Nhiệm vụ hằng ngày Quests hiển thị tiến độ hoàn thành |
| TC-S19 | Shop | | PASS | Cửa hàng Gem Shop mua vật phẩm trang bị avatar/frame thành công |
| TC-S20 | Premium | | PASS | Nâng cấp gói Premium mở khóa tính năng không giới hạn tim |
| **CROSS-ROLE** | | | | |
| TC-X01 | Student → Admin | | PASS | Học viên (Student) truy cập URL `/admin` bị chặn điều hướng về trang chủ |
| TC-X02 | Student → Studio | | PASS | Học viên (Student) truy cập URL `/studio` bị chặn điều hướng về trang chủ |
| TC-X03 | Teacher → Admin | | PASS | Giảng viên (Teacher) truy cập URL `/admin` bị chặn điều hướng về trang chủ |
| TC-X04 | Routes đã xóa | H1 | PASS | Các route đã xóa (`/benchmark/:k1/:k2`, `/playground/code-to-visual`, `/graph-playground`) trả về 404 (Đã fix H1) |
| TC-X05 | Spam click tạo lớp | | PASS | Spam click nút tạo lớp được debounce và chặn gọi nhiều lần |
| TC-X06 | Spam click enroll | | PASS | Spam click nút enroll lộ trình được bảo vệ an toàn |
| TC-X07 | XSS injection | | PASS | XSS input được escape và sanitize an toàn trên UI |
| TC-X08 | URL lesson ID sai | | PASS | Truy cập bài học với ID không tồn tại hiển thị thông báo lỗi hoặc 404 an toàn |
| TC-X09 | Mạng chậm | | PASS | Trạng thái mạng chậm hiển thị skeleton/spinner mượt mà, không crash |
| TC-X10 | Responsive 768px | | PASS | Giao diện responsive trên màn hình tablet 768px và mobile không vỡ khung |
| TC-X11 | Console errors | | PASS | Không có uncaught exception hoặc 500 error trong console khi điều hướng toàn bộ app |

---

## TỔNG KẾT

- **Tổng số test case**: 78
- **Guest**: 9 | **Auth**: 13 | **Admin**: 14 | **Teacher**: 44 | **Student**: 20 | **Cross-role**: 11
- **Bug đã biết cần kiểm tra (🐛)**: ~25 bugs
- **Ước tính thời gian**: 2-3 giờ (manual) / 30-60 phút (Puppeteer automated)

> Sau khi chạy xong, điền bảng checklist và báo cáo tổng: PASS / FAIL / BLOCKED / SKIP.
