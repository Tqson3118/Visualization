# FEATURE MAP — BẢN ĐỒ TÍNH NĂNG TOÀN DIỆN DSAVISUAL

> Dự án: **DsaVisual** — Nền tảng học Cấu trúc Dữ liệu & Giải thuật tương tác trực quan.
> Phiên bản tài liệu: 1.0 (QA Audit Ready)
> Ngày lập: 2026-09-02

---

## I. Danh mục màn hình & Ma trận phân quyền (Screen Matrix)

| STT | Tên màn hình | Route | Role được phép | Tính năng chính | API liên quan | Ghi chú flow / Trạng thái |
|---|---|---|---|---|---|---|
| 01 | **Trang chủ** | / | Guest, Student, Teacher, Admin | Giới thiệu nền tảng, CTA bắt đầu học, danh sách tính năng nổi bật, thống kê nhanh | GET /api/v1/public/stats | Công khai, có điều hướng tới /path hoặc /login |
| 02 | **Đăng nhập** | /login | Guest only | Đăng nhập tài khoản bằng email & password, remember me, chuyển hướng sau login | POST /api/v1/auth/login | Redirect về trang trước đó nếu có query 
edirect |
| 03 | **Đăng ký** | /register | Guest only | Đăng ký tài khoản Student hoặc tick \'Tôi là giảng viên\' (trở thành TEACHER_PENDING) | POST /api/v1/auth/register | Tự động đăng nhập hoặc chuyển sang chờ duyệt nếu là Teacher |
| 04 | **Quên mật khẩu** | /forgot-password | Guest, All | Nhập email nhận mã OTP / link reset mật khẩu | POST /api/v1/auth/forgot-password | Hiệu lực link 30 phút, dùng 1 lần |
| 05 | **Đặt lại mật khẩu** | /reset-password | Guest, All | Nhập token và mật khẩu mới | POST /api/v1/auth/reset-password | Reset xong chuyển về /login |
| 06 | **Chờ duyệt Giảng viên** | /pending-teacher | TEACHER_PENDING | Màn hình thông báo tài khoản đang chờ Admin phê duyệt, hướng dẫn liên hệ | GET /api/v1/me/profile | Tự động chặn khi TEACHER_PENDING cố vào /studio hoặc /admin |
| 07 | **Danh sách Lộ trình** | /path (alias: /courses, /learn) | Public (Guest xem được danh sách Active) | Duyệt danh sách lộ trình, lọc theo danh mục, độ khó, tìm kiếm tiếng Việt | GET /api/v1/concepts/courses | Guest chỉ thấy lộ trình Active & Public |
| 08 | **Chi tiết Lộ trình** | /path/:id (alias: /courses/:id, /path/:topicId) | Requires Auth (Student, Teacher, Admin) | Xem cây cấu trúc chương/bài (outline tree), tiến độ học, thông tin tác giả, đánh giá, gửi góp ý (CourseFeedback) | GET /api/v1/concepts/courses/{id}, GET /api/v1/paths/{id}/items, POST /api/v1/courses/feedback | SV chỉ xem được lộ trình Active hoặc lộ trình ClassOnly của lớp mình tham gia |
| 09 | **Học bài giảng** | /lessons/:id (alias: /learn/:lessonId) | Requires Auth | Học lý thuyết (Markdown), xem mô phỏng tích hợp (Sandbox), làm Quiz trắc nghiệm, làm CodeLab, gửi feedback bài học | GET /api/v1/lessons/{id}, POST /api/v1/progress/lessons/{id}/complete, POST /api/v1/feedback | Cập nhật tiến độ node, trừ tim nếu sai quiz, cộng XP/Gems |
| 10 | **Kiểm tra cuối khóa** | /path/:topicId/final-test | Requires Auth | Làm bài kiểm tra tổng hợp cuối lộ trình (Legacy compat) | GET /api/v1/topics/{id}/final-test, POST /api/v1/exercises/submit | Mở khóa chứng chỉ / badge khi đạt điểm yêu cầu |
| 11 | **Khám phá Mô phỏng** | /simulations (alias sandboxes) | Public | Danh mục 44 thuật toán và CTDL, lọc theo nhóm (Sort, Search, Tree, Graph, Linear, Structure), Benchmark | GET /api/v1/simulations | Tìm kiếm, xem độ phức tạp Big-O |
| 12 | **Chi tiết Mô phỏng** | /simulator/:key | Public (3 key demo) / Requires Auth (41 key còn lại) | Trực quan hóa thuật toán bằng Canvas tương tác, điều khiển Step/Play/Pause/Speed, nhập dữ liệu tùy biến | GET /api/v1/simulations/{key} | 3 key demo: sort.bubble, search.binary, graph.bfs |
| 13 | **Code Runner** | /code/:key | Requires Auth | Viết mã, chạy và debug thuật toán trên sandbox đa ngôn ngữ (C++, Java, Python, JS) | POST /api/v1/coderuns/run | Giới hạn thời gian execution, bộ nhớ, bắt lỗi syntax/runtime |
| 14 | **CheatSheet** | /cheatsheet | Requires Auth | Bảng tra cứu nhanh độ phức tạp thời gian/không gian, code template; Xuất PDF (Premium) | GET /api/v1/concepts/cheatsheet | Tính năng xuất PDF bị khóa nếu chưa có Premium |
| 15 | **Bài tập Trắc nghiệm** | /exercise/:id | Requires Auth | Làm bài tập đơn lẻ (MCQ, Predict, Simulation Lab), chấm điểm tức thì, trừ tim khi sai | GET /api/v1/exercises/{id}, POST /api/v1/exercises/{id}/submit | Trừ 1 tim nếu sai, cộng XP khi đúng |
| 16 | **Hồ sơ Cá nhân** | /profile (alias: /dashboard) | Requires Auth | Xem thông tin cá nhân, sửa avatar/bio, xem lịch sử học tập, chuỗi streak, thống kê tim/gems | GET /api/v1/me/profile, PUT /api/v1/me/profile | Hiển thị badge, danh sách khóa đang học |
| 17 | **Bảng xếp hạng** | /leaderboard | Requires Auth | Xem xếp hạng học viên theo tuần/tháng/all-time dựa trên XP tích lũy | GET /api/v1/gamification/leaderboard | Tự động highlight vị trí của bản thân |
| 18 | **Nhiệm vụ Hằng ngày** | /quests | Requires Auth | Danh sách Daily Quests (học 1 bài, chạy 1 code runner, giải 1 quiz...), nhận thưởng Gems/XP | GET /api/v1/gamification/quests, POST /api/v1/gamification/quests/{id}/claim | Reset mỗi 00:00 UTC+7; chống claim 2 lần |
| 19 | **Cửa hàng Gems** | /shop | Requires Auth | Mua hồi đầy tim, mua avatar đặc biệt, mua streak freeze bằng Gems | GET /api/v1/gamification/shop, POST /api/v1/gamification/shop/buy | Kiểm tra số dư Gems trước khi trừ |
| 20 | **Nâng cấp Premium** | /premium | Requires Auth | Xem bảng so sánh quyền lợi Free vs Premium, chọn gói tháng/năm, thanh toán qua VietQR | POST /api/v1/me/subscription/checkout | Hiển thị mã QR VietQR động kèm cú pháp chuyển khoản |
| 21 | **Quản lý Gói cước** | /account/subscription | Requires Auth | Xem thông tin gói Premium hiện tại, ngày hết hạn, lịch sử thanh toán, hủy tự động gia hạn | GET /api/v1/me/subscription | Đồng bộ trạng thái với badge Premium trên navbar |
| 22 | **Danh sách Lớp học** | /classes | Requires Auth | Học viên: xem lớp đã tham gia, tham gia bằng mã 6 ký tự. Giáo viên: xem lớp do mình quản lý, tạo lớp mới | GET /api/v1/classes, POST /api/v1/classes, POST /api/v1/classes/join-by-code | GV tạo lớp được cấp mã mời ngẫu nhiên |
| 23 | **Chi tiết Lớp học** | /classes/:id | Requires Auth (Member / Teacher sở hữu / Admin) | Xem thành viên, lộ trình được gán, danh sách bài tập & hạn nộp (deadline), trạng thái trễ hạn | GET /api/v1/classes/{id}, GET /api/v1/classes/{id}/curriculum, PUT /api/v1/classes/{id}/assignments/deadline | GV gán lộ trình, quản lý thành viên, đặt hạn nộp |
| 24 | **Báo cáo Lớp học** | /classes/:id/report | Requires Auth (Teacher sở hữu / Admin) | Xem ma trận tiến độ cả lớp, tỷ lệ hoàn thành từng bài, bài nộp, xuất dữ liệu CSV | GET /api/v1/classes/{id}/report, GET /api/v1/classes/{id}/report/export | Chỉ GV quản lý lớp hoặc Admin mới xem/tải được |
| 25 | **Studio Giảng viên & Admin** | /studio (alias: /admin/content, /teacher) | Requires Auth (TEACHER, ADMIN) | Trung tâm biên soạn: Quản lý lộ trình, cây bài học (Theory/Quiz/Lab), gửi duyệt, xem phản hồi học viên | GET/POST/PUT /api/v1/concepts/courses, GET/POST/PUT/DELETE /api/v1/paths/{id}/items, GET /api/v1/courses/feedback/for-teacher | Bao gồm 4 tab: Overview, Curriculum, Feedback, Moderation (chỉ Admin) |
| 26 | **Quản lý Người dùng Admin** | /admin/users (alias /admin) | Requires Auth (ADMIN) | Danh sách user, tìm kiếm, lọc role/status, đổi role, khóa/mở khóa, reset password, duyệt đơn GV | GET /api/v1/users, PUT /api/v1/users/{id}/status, POST /api/v1/users/{id}/approve-teacher, POST /api/v1/users/{id}/reset-password | Chỉ tài khoản Admin mới truy cập được |
| 27 | **Thống kê Admin** | /admin/stats | Requires Auth (ADMIN) | Dashboard số liệu: tổng user, phân bố role, tổng lộ trình, bài học, bài nộp, active users hôm nay | GET /api/v1/admin/stats | Query gộp 1 lần tối ưu performance |
| 28 | **Cài đặt Hệ thống Admin** | /admin/settings | Requires Auth (ADMIN) | Cấu hình tham số hệ thống (hồi tim, XP, bảo trì), xem và xử lý báo cáo lỗi (BugReports) | GET/PUT /api/v1/settings, GET/PUT /api/v1/admin/bug-reports | Admin cập nhật trạng thái báo cáo lỗi và ghi chú phản hồi |
| 29 | **Quản lý Lớp học Admin** | /admin/classes | Requires Auth (TEACHER, ADMIN) | Xem toàn bộ các lớp trong hệ thống, chuyển quyền hoặc quản trị lớp | GET /api/v1/classes | Danh sách mở rộng cho quản trị |
| 30 | **Trợ giúp & FAQ** | /help | Public (xem FAQ) / Requires Auth (gửi hỗ trợ) | Đọc câu hỏi thường gặp (FAQ), gửi form liên hệ/báo lỗi hệ thống | POST /api/v1/bug-reports | Form gửi dữ liệu thật vào bảng BugReports kèm metadata nguồn |
| 31 | **Chính sách Quyền riêng tư** | /privacy | Public | Điều khoản sử dụng và chính sách bảo mật dữ liệu | Không gọi API | Trang tĩnh |
| 32 | **Trang 404** | /:pathMatch(.*)* | Public | Báo lỗi đường dẫn không tồn tại, nút quay lại trang chủ | Không gọi API | Bắt tất cả route không hợp lệ |

---

## II. Các Máy Trạng Thái Trọng Yếu (Core State Machines)

### 1. Máy trạng thái Lộ trình học (LearningPathStatus & PathVisibility)
- **Enum LearningPathStatus**: Draft (0) | PendingReview (1) | Active (2) | Rejected (3) | ClassOnly (4).
- **Enum PathVisibility**: Private (0) | ClassOnly (1) | Public (2).
- **Luồng chuyển đổi**:
  - Draft -> GV bấm gửi duyệt công khai -> PendingReview.
  - Draft -> GV gán trực tiếp vào lớp -> ClassOnly (không cần duyệt admin).
  - PendingReview -> Admin duyệt -> Active (hiển thị trên /path).
  - PendingReview -> Admin từ chối -> Rejected (GV sửa và gửi lại).
- **Quy tắc hiển thị**:
  - Draft, PendingReview: Chỉ Tác giả và Admin thấy trong Studio.
  - Active & Public: Toàn bộ người dùng và khách thấy trên /path.
  - ClassOnly: Chỉ SV trong lớp được gán mới thấy trong /classes/:id.

### 2. Máy trạng thái Bài học (LessonStatus & PathItemType)
- **Enum LessonStatus**: Draft (0) | PendingReview (1) | Active (2) | Hidden (3).
- **Enum PathItemType**: Folder (0) | Theory (1) | Quiz (2) | Lab (3).
- **Hành vi khi Ẩn/Xóa**: SV đang mở bài bị chặn nội dung và có thông báo rõ; % tiến độ tính lại chính xác.

### 3. Máy trạng thái Lớp học & Hạn nộp (Class & Deadline)
- **Enum ClassStatus**: Open (0) | Closed (1).
- **Deadline**: GV set hạn nộp; quá hạn hiển thị badge đỏ; nộp sau hạn ghi nhận trễ hạn trong ClassReport.

### 4. Máy trạng thái Gamification
- **Tim (Hearts)**: Tối đa 5 tim (Free) / 30 tim (Premium). Sai trừ 1 tim. Hết tim chặn làm bài.
- **Gems & Quests**: Nhận qua hoàn thành bài và Daily Quests; chống double-claim; dùng mua tim tại /shop.

### 5. Máy trạng thái Premium & VietQR
- **VietQR Checkout**: Sinh mã QR động -> Xác nhận thanh toán -> Cấp badge Premium, mở khóa tải PDF CheatSheet và tăng giới hạn tim.
