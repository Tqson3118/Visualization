# FINDINGS — DANH SÁCH LỖI VÀ VẤN ĐỀ PHÁT HIỆN QUA AUDIT DSAVISUAL

> Dự án: **DsaVisual**
> Đơn vị thực hiện: **QA Explorer**
> Ngày audit: 2026-09-02
> Trạng thái: **Hoàn thành toàn diện**

---

### QA-001 — [P1] — Header hiển thị widget và link role khi phiên chưa xác định hoặc hết hạn
- Loại: UI / đồng bộ
- Màn/URL: Mọi màn hình / AppHeader
- Role: Guest / Chưa xác thực
- Bước tái hiện:
  1) Mở trình duyệt khi có cookie phiên cũ dsa.session nhưng accessToken trống hoặc token hết hạn.
  2) Vào trang chủ / hoặc /login.
  3) Quan sát thanh Header phía trên.
- Kỳ vọng vs Thực tế:
  - Kỳ vọng: Chỉ hiển thị nút Đăng nhập / Đăng ký và các link công khai (Lộ trình, Mô phỏng).
  - Thực tế: Header chớp hiển thị Hearts/Gems widget và các nút Studio/Admin trước khi router guard hoàn tất.
- Bằng chứng: qa/evidence/QA-A05_06_guest_path_check.png
- Suy đoán nguyên nhân: frontend/src/components/layout/AppHeader.vue:134 kiểm tra auth.isAuthenticated nhưng trong Pinia khởi tạo chưa đồng bộ trạng thái status === 'authenticated'.
- Gợi ý fix: Chỉ render HeartsGemsWidget và studioTarget khi auth.isAuthenticated && auth.user !== null.

---

### QA-002 — [P1] — AdminStats hiển thị lỗi kết nối khi Backend khởi động lại
- Loại: chức năng / đồng bộ
- Màn/URL: /admin/stats
- Role: ADMIN
- Bước tái hiện:
  1) Đăng nhập tài khoản Admin (admin@system.local).
  2) Vào /admin/stats.
  3) Nếu kết nối mạng hoặc proxy bị trễ lúc tải trang, màn hình chuyển sang trạng thái lỗi.
- Kỳ vọng vs Thực tế:
  - Kỳ vọng: Hiển thị 13 chỉ số thống kê hệ thống (User, Course, Order, Lesson...).
  - Thực tế: Hiển thị dòng chữ 'Không thể tải dữ liệu thống kê từ máy chủ. Thử lại'.
- Bằng chứng: qa/evidence/QA-C03-admin-stats.png
- Suy đoán nguyên nhân: frontend/src/views/AdminStatsView.vue:45 bắt lỗi catch chung mà không có retry policy hoặc fallback cache từ store.
- Gợi ý fix: Thêm cơ chế tự động thử lại (retry 3 lần) và lưu cache thống kê lần tải gần nhất trong Pinia.

---

### QA-003 — [P2] — Code Runner không nhận dạng được slug đường dẫn gạch ngang
- Loại: UX/logic / route
- Màn/URL: /code/:key (VD: /code/bubble-sort)
- Role: ALL
- Bước tái hiện:
  1) Truy cập URL http://localhost:5173/code/bubble-sort.
  2) Quan sát thông báo trên màn hình.
- Kỳ vọng vs Thực tế:
  - Kỳ vọng: Tự động map sang bài tập Bubble Sort (sort.bubble).
  - Thực tế: Hiển thị 'Không tìm thấy bài thử thách — Key bubble-sort chưa có trong danh mục mô phỏng'.
- Bằng chứng: qa/evidence/QA-G01-code-runner.png
- Suy đoán nguyên nhân: frontend/src/views/CodeRunnerView.vue tìm kiếm key chính xác dạng sort.bubble trong CATALOG, không chuẩn hóa slug gạch ngang.
- Gợi ý fix: Thêm hàm chuyển đổi alias slug: bubble-sort -> sort.bubble, binary-search -> search.binary.

---

### QA-004 — [P2] — Sinh viên tham gia lớp 2 lần trả về lỗi 400 thay vì xử lý Idempotent
- Loại: UX/logic / API
- Màn/URL: POST /api/v1/classes/join-by-code
- Role: STUDENT
- Bước tái hiện:
  1) Sinh viên nhập mã mời 8TREHI để tham gia lớp.
  2) Bấm tham gia thành công.
  3) Nhập lại mã mời đó lần thứ 2 và bấm tham gia.
- Kỳ vọng vs Thực tế:
  - Kỳ vọng: Trả về trạng thái thành công kèm thông báo 'Bạn đã là thành viên của lớp này' và điều hướng vào lớp.
  - Thực tế: Backend trả về HTTP 400 Bad Request gây hiện tượng thông báo lỗi đỏ.
- Bằng chứng: qa/evidence/QA-C04_02_join_modal.png
- Suy đoán nguyên nhân: backend/src/DsaVisual.Api/Controllers/ClassesController.cs kiểm tra _db.ClassMembers.AnyAsync() và ném BadRequest khi đã tồn tại.
- Gợi ý fix: Trả về 200 OK kèm payload lớp học hiện tại nếu sinh viên đã ở trong lớp (Idempotent API).

---

### QA-005 — [P3] — Màn hình Nhiệm vụ hàng ngày hiển thị tiến độ 0/0 khi danh sách trống
- Loại: UI / UX
- Màn/URL: /quests
- Role: STUDENT
- Bước tái hiện:
  1) Đăng nhập Student và vào /quests.
  2) Khi chưa có nhiệm vụ nào được cấu hình cho ngày hôm đó.
- Kỳ vọng vs Thực tế:
  - Kỳ vọng: Hiển thị Empty state sinh động với minh họa linh vật và thông điệp 'Chưa có nhiệm vụ mới hôm nay'.
  - Thực tế: Hiển thị 'Đã hoàn thành 0/0 · 0/0 DONE · Streak 0'.
- Bằng chứng: qa/evidence/QA-H01-quests.png
- Suy đoán nguyên nhân: frontend/src/views/QuestsView.vue thiếu điều kiện Empty State khi mảng rỗng.
- Gợi ý fix: Thêm component EmptyState khi quests.length === 0.

---

### QA-006 — [P3] — Nút Xuất PDF CheatSheet chưa kiểm tra quyền lợi Premium
- Loại: phân quyền / tính năng
- Màn/URL: /cheatsheet
- Role: STUDENT (Gói Free)
- Bước tái hiện:
  1) Học viên gói Miễn phí (Free) vào /cheatsheet.
  2) Bấm nút 'Xuất File PDF / In CheatSheet (A4)'.
- Kỳ vọng vs Thực tế:
  - Kỳ vọng: Theo bảng so sánh tại /premium, tính năng Xuất PDF thuộc gói Premium -> Cần mở modal mời nâng cấp.
  - Thực tế: Trình duyệt mở thẳng hộp thoại in window.print() cho mọi người dùng.
- Bằng chứng: qa/evidence/QA-G03-cheatsheet.png
- Suy đoán nguyên nhân: frontend/src/views/CheatSheetView.vue gắn trực tiếp click gọi window.print() mà không kiểm tra auth.isPremium.
- Gợi ý fix: Thêm guard kiểm tra if (!auth.isPremium) { openPremiumModal(); return; }.

---

### QA-007 — [P2] — Teacher Studio dùng window.confirm native khi rời trang chưa lưu
- Loại: UI / UX
- Màn/URL: /studio?tab=curriculum
- Role: TEACHER / ADMIN
- Bước tái hiện:
  1) Giảng viên sửa bài giảng trong Studio Curriculum (tạo trạng thái Dirty).
  2) Click chuyển sang tab Feedback hoặc Overview.
- Kỳ vọng vs Thực tế:
  - Kỳ vọng: Hiển thị modal xác nhận giao diện Dark Mode (ConfirmModal.vue) đồng bộ phong cách DSA Visual.
  - Thực tế: Bật hộp thoại popup alert native của trình duyệt (window.confirm).
- Bằng chứng: qa/evidence/QA-B02-teacher-studio-curriculum.png
- Suy đoán nguyên nhân: frontend/src/views/AdminContentView.vue:44 và :60 gọi window.confirm(...).
- Gợi ý fix: Tích hợp uiStore.showConfirmModal(...) để hiển thị modal đồng bộ.

---

### QA-008 — [P2] — Thanh điều khiển Simulator bị che khuất trên màn hình Mobile 375px
- Loại: UI / Responsive
- Màn/URL: /simulator/:key (Viewport 375px)
- Role: ALL
- Bước tái hiện:
  1) Mở trình duyệt ở kích thước 375x667 (iPhone SE).
  2) Vào bất kỳ mô phỏng nào (VD: /simulator/sort.bubble).
- Kỳ vọng vs Thực tế:
  - Kỳ vọng: Các nút điều khiển (Play/Pause/Step/Speed) co giãn linh hoạt và không bị tràn thanh cuộn ngang.
  - Thực tế: Thanh footer controls bị tràn lề ngang, che khuất một phần nút chỉnh tốc độ và nút âm thanh.
- Bằng chứng: qa/evidence/QA-G01_mobile_simulator_375.png
- Suy đoán nguyên nhân: frontend/src/components/simulator/SimulatorApp.vue dùng fixed width hoặc flex không wrap trên viewport dưới 400px.
- Gợi ý fix: Áp dụng CSS media query @media (max-width: 480px) thu nhỏ padding và icon button.

---

### QA-009 — [P3] — Trang chờ duyệt giảng viên thiếu nút làm mới trạng thái tức thì
- Loại: UX/logic
- Màn/URL: /pending-teacher
- Role: TEACHER_PENDING
- Bước tái hiện:
  1) Ứng viên đăng ký tài khoản giảng viên mới và được chuyển tới /pending-teacher.
  2) Admin duyệt tài khoản trên /admin/users.
  3) Ứng viên ở trang /pending-teacher không có nút bấm để kiểm tra lại quyền.
- Kỳ vọng vs Thực tế:
  - Kỳ vọng: Có nút 'Kiểm tra trạng thái duyệt' để tự động gọi /api/v1/auth/me và chuyển hướng vào /studio khi đã duyệt.
  - Thực tế: Người dùng buộc phải đăng xuất rồi đăng nhập lại thủ công.
- Bằng chứng: qa/evidence/QA-F03_teacher_blocked_from_admin.png
- Suy đoán nguyên nhân: frontend/src/views/PendingTeacherView.vue là trang tĩnh, chỉ có nút Quay về trang chủ và Đăng xuất.
- Gợi ý fix: Thêm nút 'Làm mới trạng thái' gọi await auth.fetchMe().

---

### QA-010 — [P3] — Thiếu chỉ báo thời gian còn lại của Deadline trong danh sách bài tập lớp
- Loại: UX/logic
- Màn/URL: /classes/:id
- Role: STUDENT
- Bước tái hiện:
  1) Học viên vào chi tiết lớp học có bài tập sắp đến hạn.
- Kỳ vọng vs Thực tế:
  - Kỳ vọng: Hiển thị badge đếm ngược (VD: 'Còn 2 ngày', 'Hết hạn sau 5 giờ').
  - Thực tế: Chỉ hiển thị ngày giờ cố định định dạng ISO hoặc DD/MM/YYYY.
- Bằng chứng: qa/evidence/QA-D02-class-detail-deadline.png
- Suy đoán nguyên nhân: frontend/src/views/ClassDetailView.vue chỉ format date bằng formatDate() mà chưa tính toán timeRemaining().
- Gợi ý fix: Bổ sung helper formatTimeRemaining(dueAt) với màu sắc cảnh báo (Đỏ khi < 24h, Vàng khi < 3 ngày).

---

### [QA-011] P1 — Lỗi hiển thị module thừa và sai sót ép kiểu (Type Mismatch) trong logic khóa tuần tự bài học phía Học sinh
- Mức độ: P1 (Cao)
- Loại lỗi: logic / phân quyền & điều hướng lộ trình
- URL / Màn hình: /lessons/:id?courseId=:id (LessonStudyView.vue)
- Các bước tái hiện:
  1. Giảng viên tạo lộ trình mới có module, sau đó thêm các bài học mới. Các node mồ côi cũ chưa dọn dẹp tạo ra nhiều module rác ('Module mới', 'Nội dung bài học').
  2. Học sinh mở bài học 1: Do hàm isLessonLocked so sánh strict l.id === lesson.id (number vs string), findIndex trả về -1, kích hoạt if (idx <= 0) return false.
  3. Hậu quả: Toàn bộ các bài học sau (Bài 3) bị mở khóa sai trái dù Bài 1 và Bài 2 chưa hoàn thành.
- Kết quả mong muốn: Chỉ hiển thị module cha chuẩn; Bài 1 mở, Bài 2 & 3 phải khóa chặt 🔒 cho đến khi bài trước hoàn thành.
- Kết quả thực tế: Hiển thị 3 module rải rác và Bài 3 bị mở khóa sai tuần tự.
- Suy đoán nguyên nhân: So sánh type number vs string trong findIndex và thứ tự ưu tiên kiểm tra locked trước khi check completed.
- Gợi ý fix: Đã dọn dẹp cây giáo trình chuẩn 1 module 3 bài; sửa isLessonLocked ép kiểu String(l.id) === String(lesson.id) và ưu tiên mở khóa tuần tự khi bài trước hoàn thành.
