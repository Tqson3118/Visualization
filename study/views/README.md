# 📖 DANH MỤC CHI TIẾT TỪNG VIEW TRONG HỆ THỐNG (34 VIEWS)

Thư mục `study/views/` chứa tài liệu phân tích **cặn kẽ từng View / Màn hình** trong dự án DSA Visual. Mỗi file đại diện cho 1 hoặc 1 nhóm View liên quan chặt chẽ, mô tả chi tiết từ:
1. **Metadata & Phân quyền truy cập** (Route, Guards, Roles).
2. **Cấu trúc Component & Mắt thấy gì trên giao diện (Visual DOM Wireframe)**.
3. **Chi tiết từng Luồng tương tác (Flows)**: Khởi tạo dữ liệu khi vào trang, Các nút bấm hành động (Click actions), Bắn API, Xử lý Backend & Cập nhật Database SQL Server.
4. **Bản đồ mã nguồn liên quan** (File Vue, Component con, Pinia Store, API Client, Backend Controller, Service, Entity).

---

## 🗂️ DANH SÁCH CÁC FILE HƯỚNG DẪN TỪNG VIEW:

### 1. Phân hệ Khách & Xác thực (Public & Auth)
* [**`01-HomeView.md`**](file:///d:/FPT/metqua/study/views/01-HomeView.md): Màn hình Trang chủ công khai (`/`).
* [**`02-LoginView.md`**](file:///d:/FPT/metqua/study/views/02-LoginView.md): Màn hình Đăng nhập & Điều hướng vai trò (`/login`).
* [**`03-RegisterView.md`**](file:///d:/FPT/metqua/study/views/03-RegisterView.md): Màn hình Đăng ký tài khoản 2 bước qua OTP MailHog (`/register`).
* [**`04-ForgotPasswordView-ResetPasswordView.md`**](file:///d:/FPT/metqua/study/views/04-ForgotPasswordView-ResetPasswordView.md): Màn hình Quên & Đặt lại mật khẩu (`/forgot-password`, `/reset-password`).
* [**`05-PendingTeacherView.md`**](file:///d:/FPT/metqua/study/views/05-PendingTeacherView.md): Màn hình Chờ duyệt Giảng viên (`/pending-teacher`).

### 2. Phân hệ Lộ trình & Bài học (Curriculum & Study)
* [**`06-CoursesListView.md`**](file:///d:/FPT/metqua/study/views/06-CoursesListView.md): Màn hình Danh sách Lộ trình học (`/path`).
* [**`07-CourseDetailView.md`**](file:///d:/FPT/metqua/study/views/07-CourseDetailView.md): Màn hình Chi tiết Lộ trình & Cây bài học (`/path/:id`).
* [**`08-LessonStudyView.md`**](file:///d:/FPT/metqua/study/views/08-LessonStudyView.md): Không gian học tập tích hợp Lý thuyết + Mô phỏng + CodeLab (`/lessons/:id`).
* [**`09-FinalTestView.md`**](file:///d:/FPT/metqua/study/views/09-FinalTestView.md): Màn hình Bài kiểm tra tổng kết cuối lộ trình (`/path/:id/final-test`).

### 3. Phân hệ Phòng thí nghiệm Thuật toán (Algorithm Lab)
* [**`10-SimulationsView.md`**](file:///d:/FPT/metqua/study/views/10-SimulationsView.md): Thư viện 44 mô phỏng & Đo hiệu năng Benchmark (`/simulations`).
* [**`11-SimulatorView.md`**](file:///d:/FPT/metqua/study/views/11-SimulatorView.md): Trình mô phỏng chi tiết 3 vùng VCR Player & AI Explainer (`/simulator/:key`).
* [**`12-CodeRunnerView.md`**](file:///d:/FPT/metqua/study/views/12-CodeRunnerView.md): Trình soạn thảo & Chạy code trực tiếp Monaco (`/code/:key`).
* [**`13-CheatSheetView.md`**](file:///d:/FPT/metqua/study/views/13-CheatSheetView.md): Bảng tra cứu nhanh độ phức tạp Big-O (`/cheatsheet`).

### 4. Phân hệ Luyện tập & Đánh giá (Practice)
* [**`14-ExerciseView.md`**](file:///d:/FPT/metqua/study/views/14-ExerciseView.md): Màn hình Trắc nghiệm & Codelab chấm tự động (`/exercise/:id`).

### 5. Phân hệ Gamification & Kinh tế ảo (Gamification & Shop)
* [**`15-ProfileView.md`**](file:///d:/FPT/metqua/study/views/15-ProfileView.md): Màn hình Hồ sơ cá nhân, Heatmap đóng góp & Thành tích (`/profile`).
* [**`16-LeaderboardView.md`**](file:///d:/FPT/metqua/study/views/16-LeaderboardView.md): Bảng xếp hạng Top 3 Podium & Top 50 (`/leaderboard`).
* [**`17-QuestsView.md`**](file:///d:/FPT/metqua/study/views/17-QuestsView.md): Nhiệm vụ hàng ngày & Nhận thưởng Ngọc (`/quests`).
* [**`18-ShopView.md`**](file:///d:/FPT/metqua/study/views/18-ShopView.md): Cửa hàng mua Tim, Khung viền avatar & Huy hiệu (`/shop`).
* [**`19-PremiumView-SubscriptionView.md`**](file:///d:/FPT/metqua/study/views/19-PremiumView-SubscriptionView.md): Nâng cấp Premium & Quản lý gói thuê bao (`/premium`, `/account/subscription`).

### 6. Phân hệ Lớp học trực tuyến (Classroom)
* [**`20-ClassesView.md`**](file:///d:/FPT/metqua/study/views/20-ClassesView.md): Danh sách lớp học, Tham gia bằng mã & Tạo lớp mới (`/classes`).
* [**`21-ClassDetailView.md`**](file:///d:/FPT/metqua/study/views/21-ClassDetailView.md): Chi tiết lớp học, Quản lý bài tập & Thành viên (`/classes/:id`).
* [**`22-ClassReportView.md`**](file:///d:/FPT/metqua/study/views/22-ClassReportView.md): Báo cáo phân tích điểm số học viên & Xuất Excel (`/classes/:id/report`).

### 7. Phân hệ Studio Soạn bài (Teacher & Curriculum Studio)
* [**`23-AdminContentView-Studio.md`**](file:///d:/FPT/metqua/study/views/23-AdminContentView-Studio.md): Studio thiết kế giáo trình, soạn Tiptap Markdown, nhúng Sandbox (`/studio`).

### 8. Phân hệ Bảng điều khiển Quản trị (Admin Console)
* [**`24-AdminUsersView.md`**](file:///d:/FPT/metqua/study/views/24-AdminUsersView.md): Quản trị người dùng, Đổi mật khẩu trực tiếp, Duyệt Giảng viên (`/admin/users`).
* [**`25-AdminStatsView.md`**](file:///d:/FPT/metqua/study/views/25-AdminStatsView.md): Báo cáo thống kê nền tảng & Biểu đồ ECharts (`/admin/stats`).
* [**`26-AdminSettingsView.md`**](file:///d:/FPT/metqua/study/views/26-AdminSettingsView.md): Cài đặt tham số hệ thống, Gamification, JWT, SMTP (`/admin/settings`).
* [**`27-AdminShopView-AdminTransactionsView-AdminClassesView.md`**](file:///d:/FPT/metqua/study/views/27-AdminShopView-AdminTransactionsView-AdminClassesView.md): Quản trị Shop, Giao dịch & Lớp học trường (`/admin/shop`, `/admin/transactions`, `/admin/classes`).

### 9. Phân hệ Trang tĩnh & Phụ trợ (Static & Utility)
* [**`28-HelpView-PrivacyView-NotFoundView.md`**](file:///d:/FPT/metqua/study/views/28-HelpView-PrivacyView-NotFoundView.md): Trợ giúp FAQ, Chính sách quyền riêng tư & Trang lỗi 404 (`/help`, `/privacy`, `404`).
