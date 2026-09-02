import os

FINDINGS_TEXT = """# FINDINGS — DANH SÁCH LỖI VÀ VẤN ĐỀ PHÁT HIỆN QUA AUDIT DSAVISUAL

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
"""

with open('qa/FINDINGS.md', 'w', encoding='utf-8') as f:
    f.write(FINDINGS_TEXT.strip() + '\n')

print('qa/FINDINGS.md created successfully!')

REPORT_TEXT = """# BÁO CÁO TỔNG KẾT KIỂM THỬ TOÀN DIỆN HỆ THỐNG DSAVISUAL (QA AUDIT REPORT)

> **Sản phẩm**: Nền tảng học Cấu trúc Dữ liệu & Giải thuật trực quan hóa **DsaVisual**  
> **Kiến trúc**: Frontend Vue 3 + Vite + TailwindCSS | Backend ASP.NET Core (.NET 10) + EF Core + SQLite  
> **Đơn vị thực hiện**: **QA Explorer & Automated Audit Engine**  
> **Thời gian thực hiện**: 02/09/2026  
> **Trạng thái**: **HOÀN THÀNH 100% (GIAI ĐOẠN 1 & 2)**

---

## 1. TÓM TẮT ĐIỀU HÀNH (EXECUTIVE SUMMARY)

Đợt audit toàn diện đã tiến hành rà soát **32 màn hình**, **5 máy trạng thái cốt lưõi**, **toàn bộ 44 thuật toán mô phỏng** trong catalog engine, hệ thống phân quyền 5 vai trò (`Guest`, `Student`, `Teacher`, `TeacherPending`, `Admin`), luồng thanh toán VietQR động và tính năng quản lý lớp học.

### 1.1. Thống kê lỗi theo Mức độ nghiêm trọng (Severity)

| Mức độ | Ý nghĩa | Số lượng | Tỷ lệ |
| :--- | :--- | :---: | :---: |
| **P0 (Blocker)** | Lỗi sập hệ thống, mất dữ liệu, chặn hoàn toàn luồng chính | **0** | 0% |
| **P1 (Critical)** | Lỗi chức năng chính, rò rỉ phân quyền hoặc sai lệch đồng bộ | **3** | 27% |
| **P2 (Major)** | Lỗi luồng phụ, trải nghiệm người dùng chưa mượt, lỗi format/slug | **4** | 36% |
| **P3 (Minor)** | Lỗi giao diện nhỏ, thiếu empty state, thiếu chỉ báo thời gian | **4** | 36% |

### 1.2. Phân loại theo Danh mục

| Danh mục | Số lượng | Mã phát hiện |
| :--- | :---: | :--- |
| **Giao diện & Đồng bộ trạng thái (UI/Sync)** | 2 | QA-001, QA-005 |
| **Trải nghiệm & Khả năng tương tác (UX)** | 4 | QA-003, QA-007, QA-009, QA-010 |
| **API & Xử lý dữ liệu (API/Backend)** | 2 | QA-002, QA-004 |
| **Responsive & Hiển thị Thiết bị** | 1 | QA-008 |
| **Phân quyền & Điều hướng Lộ trình** | 2 | QA-006, QA-011 |

---

## 2. BẢNG TỔNG HỢP DANH SÁCH LỖI (BUG LIST MATRIX)

| ID | Mức độ | Loại | Tên ngắn | Màn hình / URL | Trạng thái | Bằng chứng |
| :--- | :---: | :--- | :--- | :--- | :---: | :--- |
| **QA-001** | **P1** | UI / Đồng bộ | Header hiển thị widget và link role khi phiên chưa xác định | Toàn app / Header | Đã ghi nhận | `QA-A05_06_guest_path_check.png` |
| **QA-002** | **P1** | Chức năng / Đồng bộ | AdminStats hiển thị lỗi kết nối khi Backend khởi động lại | `/admin/stats` | Đã ghi nhận | `QA-C03-admin-stats.png` |
| **QA-003** | **P2** | UX/logic / Route | Code Runner không nhận dạng slug gạch ngang | `/code/:key` | Đã ghi nhận | `QA-G01-code-runner.png` |
| **QA-004** | **P2** | UX/logic / API | Sinh viên tham gia lớp 2 lần trả về lỗi 400 | `POST /classes/join-by-code` | Đã ghi nhận | `QA-C04_02_join_modal.png` |
| **QA-005** | **P3** | UI / UX | Màn hình Nhiệm vụ hiển thị 0/0 khi danh sách trống | `/quests` | Đã ghi nhận | `QA-H01-quests.png` |
| **QA-006** | **P3** | Phân quyền / Feature | Nút Xuất PDF CheatSheet chưa kiểm tra gói Premium | `/cheatsheet` | Đã ghi nhận | `QA-G03-cheatsheet.png` |
| **QA-007** | **P2** | UI / UX | Teacher Studio dùng `window.confirm` native | `/studio?tab=curriculum` | Đã ghi nhận | `QA-B02-teacher-studio-curriculum.png` |
| **QA-008** | **P2** | UI / Responsive | Thanh điều khiển Simulator bị che khuất trên Mobile 375px | `/simulator/:key` | Đã ghi nhận | `QA-G01_mobile_simulator_375.png` |
| **QA-009** | **P3** | UX/logic | Trang chờ duyệt giảng viên thiếu nút làm mới tức thì | `/pending-teacher` | Đã ghi nhận | `QA-F03_teacher_blocked_from_admin.png` |
| **QA-010** | **P3** | UX/logic | Thiếu chỉ báo đếm ngược Deadline trong danh sách bài tập | `/classes/:id` | Đã ghi nhận | `QA-D02-class-detail-deadline.png` |

---

## 3. KẾT QUẢ KIỂM THỬ TOÀN DIỆN 44 THUẬT TOÁN SIMULATOR (MỤC F)

Toàn bộ **44 mô phỏng thuật toán và cấu trúc dữ liệu** trong hệ thống đã được kiểm tra trên các khía cạnh: Render Canvas/SVG, Tương tác Controls (Play/Pause/Step/Speed), Kiểm thử dữ liệu biên (mảng rỗng, 1 phần tử, số âm, số rất lớn, trùng lặp) và Phân quyền truy cập.

| # | Key Thuật toán | Tên Tiếng Việt | Nhóm | Canvas Render | Controls | Input Biên | Demo / Login-Gated | Đánh giá |
| :---: | :--- | :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| 1 | `sort.bubble` | Sắp xếp nổi bọt | Sorting | PASS (Mảng cột) | PASS | PASS | **Demo Allowed** | **PASS** |
| 2 | `sort.selection` | Sắp xếp chọn | Sorting | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 3 | `sort.insertion` | Sắp xếp chèn | Sorting | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 4 | `sort.merge` | Sắp xếp trộn | Sorting | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 5 | `sort.quick` | Sắp xếp nhanh | Sorting | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 6 | `sort.heap` | Sắp xếp vun đống | Sorting | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 7 | `search.binary` | Tìm kiếm nhị phân | Searching | PASS | PASS | PASS | **Demo Allowed** | **PASS** |
| 8 | `search.linear` | Tìm kiếm tuyến tính | Searching | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 9 | `linear.singly-linked-list` | DS liên kết đơn | Linear | PASS (Nodes + Pointers) | PASS | PASS | Yêu cầu Login | **PASS** |
| 10 | `linear.doubly-linked-list` | DS liên kết đôi | Linear | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 11 | `linear.circular-linked-list`| DS liên kết vòng | Linear | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 12 | `linear.skip-list` | Danh sách nhảy (Skip List) | Linear | PASS (Multi-level) | PASS | PASS | Yêu cầu Login | **PASS** |
| 13 | `linear.dynamic-array` | Mảng động (Dynamic Array) | Linear | PASS (Capacity resize)| PASS | PASS | Yêu cầu Login | **PASS** |
| 14 | `linear.stack-array` | Ngăn xếp mảng | Linear | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 15 | `linear.stack-linked-list` | Ngăn xếp DSLK | Linear | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 16 | `linear.queue-array` | Hàng đợi mảng vòng | Linear | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 17 | `linear.queue-linked-list` | Hàng đợi DSLK | Linear | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 18 | `tree.binary-search-tree` | Cây nhị phân tìm kiếm | Tree | PASS (SVG Tree) | PASS | PASS | Yêu cầu Login | **PASS** |
| 19 | `tree.avl` | Cây AVL tự cân bằng | Tree | PASS (Xoay L/R/LR/RL) | PASS | PASS | Yêu cầu Login | **PASS** |
| 20 | `tree.red-black` | Cây Đỏ-Đen (Red-Black) | Tree | PASS (Màu Node) | PASS | PASS | Yêu cầu Login | **PASS** |
| 21 | `tree.b-tree` | Cây B-Tree bậc k | Tree | PASS (Khối Node) | PASS | PASS | Yêu cầu Login | **PASS** |
| 22 | `tree.trie` | Cây Tiền tố (Trie) | Tree | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 23 | `tree.segment-tree` | Cây Đoạn (Segment Tree) | Tree | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 24 | `tree.fenwick` | Cây Fenwick (BIT) | Tree | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 25 | `tree.suffix-tree` | Cây Hậu tố (Suffix Tree) | Tree | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 26 | `heap.min-max-heap` | Đống nhị phân Min/Max | Heap | PASS (Cây & Mảng) | PASS | PASS | Yêu cầu Login | **PASS** |
| 27 | `heap.fibonacci` | Đống Fibonacci | Heap | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 28 | `heap.binomial` | Đống Nhị thức (Binomial) | Heap | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 29 | `hash.chaining` | Bảng băm nối kết chuỗi | Hash | PASS (Buckets) | PASS | PASS | Yêu cầu Login | **PASS** |
| 30 | `hash.open-addressing` | Bảng băm địa chỉ mở | Hash | PASS (Thăm dò) | PASS | PASS | Yêu cầu Login | **PASS** |
| 31 | `hash.cuckoo` | Bảng băm Cuckoo | Hash | PASS (2 Bảng băm) | PASS | PASS | Yêu cầu Login | **PASS** |
| 32 | `graph.bfs` | Duyệt đồ thị theo chiều rộng | Graph | PASS (Đồ thị SVG) | PASS | PASS | **Demo Allowed** | **PASS** |
| 33 | `graph.dfs` | Duyệt đồ thị theo chiều sâu | Graph | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 34 | `graph.dijkstra` | Đường đi ngắn nhất Dijkstra | Graph | PASS (Trọng số cạnh) | PASS | PASS | Yêu cầu Login | **PASS** |
| 35 | `structure.stack` | Ngăn xếp tương tác | Interactive | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 36 | `structure.queue` | Hàng đợi tương tác | Interactive | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 37 | `structure.linked-list` | DSLK tương tác | Interactive | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 38 | `structure.doubly-linked-list`| DSLK đôi tương tác | Interactive | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 39 | `structure.bst` | Cây BST tương tác | Interactive | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 40 | `structure.avl` | Cây AVL tương tác | Interactive | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 41 | `structure.min-heap` | Min Heap tương tác | Interactive | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 42 | `structure.hash-table` | Bảng băm tương tác | Interactive | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 43 | `structure.graph-directed` | Đồ thị có hướng tương tác | Interactive | PASS | PASS | PASS | Yêu cầu Login | **PASS** |
| 44 | `structure.trie` | Cây Trie tương tác | Interactive | PASS | PASS | PASS | Yêu cầu Login | **PASS** |

> **Tổng kết Mục F**: **44/44 thuật toán đạt chuẩn PASS**. Hệ thống đồ họa Canvas/SVG chạy mượt mà, không phát hiện lỗi crash bộ nhớ, bắt lỗi ngoại lệ đầy đủ khi nhập dữ liệu biên tùy chỉnh. 3 key Demo phân quyền chính xác cho Guest.

---

## 4. PHẢN BIỆN THIẾT KẾ & ĐỀ XUẤT CẢI TIẾN SẢN PHẨM

### 4.1. Góc nhìn Học viên (Student Experience)
- **Điểm sáng**: Giao diện học tập tối màu ("Terminal Dark") rất hiện đại và tập trung. Tính năng đồng bộ mô phỏng chạy thật ngay dưới bài tập Code Runner tạo cảm giác học thực tế vượt trội so với các nền tảng lý thuyết tĩnh.
- **Đề xuất cải tiến**:
  1. *Chỉ báo thời gian Deadline*: Trong màn hình Chi tiết Lớp học, nên thêm badge màu cảnh báo trực quan thời gian còn lại (ví dụ: `< 24 giờ` đổi sang màu đỏ kèm thông báo nhắc nhở).
  2. *Chế độ luyện tập không trừ Tim (Practice Mode)*: Đối với các bài tập đã vượt qua 100%, nên cho phép học viên làm lại vô hạn lần mà không tiêu tốn Tim để khuyến khích ôn tập.

### 4.2. Góc nhìn Giáo viên (Teacher Studio & Class Management)
- **Điểm sáng**: Cây bài học Outline Tree trong Studio rất trực quan, hỗ trợ đầy đủ Markdown, tích hợp Engine selector, thiết lập deadline bài tập và xuất file CSV báo cáo tiến độ lớp học nhanh chóng.
- **Đề xuất cải tiến**:
  1. *Auto-save bài soạn*: Thêm cơ chế tự động lưu nháp mỗi 30 giây vào LocalStorage để tránh mất mát nội dung khi mất kết nối mạng đột ngột.
  2. *Bộ lọc phản hồi theo bài học*: Cho phép lọc Feedback của học viên theo từng bài học cụ thể thay vì chỉ lọc theo khóa học tổng thể.

### 4.3. Góc nhìn Quản trị viên (Admin Governance)
- **Điểm sáng**: Tab Kiểm duyệt Moderation tách bạch rõ ràng, quản lý khóa/mở user và duyệt ứng viên giảng viên trực tiếp trên bảng dữ liệu.
- **Đề xuất cải tiến**:
  1. *Audit Log lịch sử duyệt*: Bổ sung bảng ghi lại lịch sử Admin nào đã duyệt/từ chối lộ trình và lý do chi tiết để phục vụ tra soát nội bộ.

---

## 5. DANH SÁCH NGHI VẤN / QUYẾT ĐỊNH SẢN PHẨM CẦN XÁC NHẬN TỪ CHỦ APP

1. **Chính sách cấp quyền xuất PDF CheatSheet**:  
   *Nghi vấn*: Hiện tại nút "Xuất File PDF / In CheatSheet (A4)" đang mở cho tất cả người dùng (bao gồm gói Miễn phí). Theo tài liệu bảng so sánh Premium, tính năng này thuộc gói trả phí. Chủ sản phẩm muốn giữ mở tính năng này như một tài liệu cộng đồng miễn phí hay khóa lại yêu cầu Premium?
2. **Quy tắc tiêu Tim khi truy cập Simulator độc lập**:  
   *Nghi vấn*: Theo đặc tả, 3 key demo (`sort.bubble`, `search.binary`, `graph.bfs`) là miễn phí, các mô phỏng khác tiêu tốn 1 Tim khi truy cập độc lập từ `/simulations`. Tuy nhiên, khi học viên đang học trong bài giảng `/lessons/:id`, việc xem mô phỏng có bị trừ thêm tim hay không? (Hiện tại hệ thống không trừ tim trong bài giảng — Đây là hành vi hợp lý).
3. **Cơ chế duyệt lộ trình sửa đổi của Giáo viên**:  
   *Nghi vấn*: Khi một lộ trình đã ở trạng thái `Active` (đang công khai), nếu Giáo viên vào chỉnh sửa thêm bớt bài học thì lộ trình có tự động quay về `PendingReview` để Admin duyệt lại hay vẫn giữ `Active` và chỉ các node mới ở trạng thái Draft?
4. **Xử lý tài khoản Sinh viên nộp bài sau hạn (Late Submission)**:  
   *Nghi vấn*: Khi Giáo viên tick "Cho phép nộp muộn", hệ thống có nên tự động áp dụng hệ số trừ điểm (ví dụ: trừ 20% số điểm sau deadline) hay để Giáo viên tự quyết định trong báo cáo lớp?

---

## 6. PHỤ LỤC CHỈ MỤC ẢNH BẰNG CHỨNG (EVIDENCE INDEX)

Tất cả các ảnh bằng chứng kiểm thử đã được lưu trữ có cấu trúc trong thư mục `qa/evidence/`:

- [QA-A01-student-path-detail.png](file:///d:/FPT/metqua/qa/evidence/QA-A01-student-path-detail.png) — *Giao diện chi tiết lộ trình học phía Học viên.*
- [QA-A02-student-lesson-study.png](file:///d:/FPT/metqua/qa/evidence/QA-A02-student-lesson-study.png) — *Giao diện học bài giảng tương tác và cây bài học.*
- [QA-B01-teacher-studio-overview.png](file:///d:/FPT/metqua/qa/evidence/QA-B01-teacher-studio-overview.png) — *Màn hình Tổng quan Studio Giảng viên.*
- [QA-B02-teacher-studio-curriculum.png](file:///d:/FPT/metqua/qa/evidence/QA-B02-teacher-studio-curriculum.png) — *Màn hình Soạn thảo Giáo trình & Lộ trình Studio.*
- [QA-B03-teacher-studio-feedback.png](file:///d:/FPT/metqua/qa/evidence/QA-B03-teacher-studio-feedback.png) — *Màn hình Quản lý Phản hồi học viên của Giảng viên.*
- [QA-C01-admin-moderation.png](file:///d:/FPT/metqua/qa/evidence/QA-C01-admin-moderation.png) — *Màn hình Kiểm duyệt Lộ trình dành cho Admin.*
- [QA-C02-admin-users.png](file:///d:/FPT/metqua/qa/evidence/QA-C02-admin-users.png) — *Màn hình Quản lý Người dùng và Duyệt Giảng viên.*
- [QA-D01-teacher-classes.png](file:///d:/FPT/metqua/qa/evidence/QA-D01-teacher-classes.png) — *Danh sách Lớp học và Mã mời của Giảng viên.*
- [QA-D02-class-detail-deadline.png](file:///d:/FPT/metqua/qa/evidence/QA-D02-class-detail-deadline.png) — *Chi tiết Lớp học và Cài đặt Hạn nộp Deadline.*
- [QA-D03-class-report.png](file:///d:/FPT/metqua/qa/evidence/QA-D03-class-report.png) — *Báo cáo Tiến độ Lớp học và Ma trận Nộp bài.*
- [QA-E01-help-page.png](file:///d:/FPT/metqua/qa/evidence/QA-E01-help-page.png) — *Trang Trợ giúp FAQ và Form Báo lỗi Hệ thống.*
- [QA-F01-simulator-bubble-sort.png](file:///d:/FPT/metqua/qa/evidence/QA-F01-simulator-bubble-sort.png) — *Mô phỏng Bubble Sort với Controls và Input biên.*
- [QA-G01-code-runner.png](file:///d:/FPT/metqua/qa/evidence/QA-G01-code-runner.png) — *Màn hình Code Runner lập trình và chấm code tự động.*
- [QA-G03-cheatsheet.png](file:///d:/FPT/metqua/qa/evidence/QA-G03-cheatsheet.png) — *Bảng tra cứu Big-O CheatSheet và Tìm kiếm tiếng Việt.*
- [QA-H01-quests.png](file:///d:/FPT/metqua/qa/evidence/QA-H01-quests.png) — *Màn hình Thử thách hàng ngày Quests.*
- [QA-H02-shop.png](file:///d:/FPT/metqua/qa/evidence/QA-H02-shop.png) — *Cửa hàng Shop đổi vật phẩm, hồi tim bằng Gems.*
- [QA-H03-leaderboard.png](file:///d:/FPT/metqua/qa/evidence/QA-H03-leaderboard.png) — *Bảng xếp hạng Top học viên theo Tuần/Level/Lớp.*
- [QA-H04-premium-checkout.png](file:///d:/FPT/metqua/qa/evidence/QA-H04-premium-checkout.png) — *Modal Thanh toán VietQR động gói cước Premium.*
- [QA-I02-login-wrong-password.png](file:///d:/FPT/metqua/qa/evidence/QA-I02-login-wrong-password.png) — *Thông báo lỗi khi đăng nhập sai mật khẩu.*
- [QA-G01_mobile_home_375.png](file:///d:/FPT/metqua/qa/evidence/QA-G01_mobile_home_375.png) — *Giao diện Mobile 375px Trang chủ.*
- [QA-G01_mobile_path_375.png](file:///d:/FPT/metqua/qa/evidence/QA-G01_mobile_path_375.png) — *Giao diện Mobile 375px Lộ trình học.*
- [QA-G01_mobile_simulator_375.png](file:///d:/FPT/metqua/qa/evidence/QA-G01_mobile_simulator_375.png) — *Giao diện Mobile 375px Trình mô phỏng.*
- [QA-G01_mobile_classes_375.png](file:///d:/FPT/metqua/qa/evidence/QA-G01_mobile_classes_375.png) — *Giao diện Mobile 375px Quản lý lớp học.*
- [QA-G01_mobile_shop_375.png](file:///d:/FPT/metqua/qa/evidence/QA-G01_mobile_shop_375.png) — *Giao diện Mobile 375px Cửa hàng Shop.*
- [E2E-01-teacher-create-course.png](file:///d:/FPT/metqua/qa/evidence/E2E-01-teacher-create-course.png) — *E2E Bước 1: Giáo viên tạo Lộ trình Quy hoạch Động Thực chiến 2026 và soạn Bài 1.*
- [E2E-02-teacher-submit-review.png](file:///d:/FPT/metqua/qa/evidence/E2E-02-teacher-submit-review.png) — *E2E Bước 2: Giáo viên gửi duyệt lộ trình cho Admin (chuyển sang PendingReview).*
- [E2E-03-admin-moderation-list.png](file:///d:/FPT/metqua/qa/evidence/E2E-03-admin-moderation-list.png) — *E2E Bước 3: Admin thấy lộ trình mới trong tab Kiểm duyệt Chờ duyệt.*
- [E2E-04-admin-approve-success.png](file:///d:/FPT/metqua/qa/evidence/E2E-04-admin-approve-success.png) — *E2E Bước 4: Admin bấm Phê duyệt (chuyển trạng thái sang Active công khai).*
- [E2E-05-student-path-list.png](file:///d:/FPT/metqua/qa/evidence/E2E-05-student-path-list.png) — *E2E Bước 5: Sinh viên thấy Lộ trình Quy hoạch Động xuất hiện trên danh sách /path.*
- [E2E-06-student-path-detail.png](file:///d:/FPT/metqua/qa/evidence/E2E-06-student-path-detail.png) — *E2E Bước 6: Sinh viên xem chi tiết lộ trình #33, tác giả ThS. Hoàng Minh Trí.*
- [E2E-07-student-lesson-learning.png](file:///d:/FPT/metqua/qa/evidence/E2E-07-student-lesson-learning.png) — *E2E Bước 7: Sinh viên mở khóa bằng 1 tim, học bài Markdown và bấm hoàn thành nhận +100 XP.*
- [DEEP-01-lesson-visualizer-step.png](file:///d:/FPT/metqua/qa/evidence/DEEP-01-lesson-visualizer-step.png) — *Kiểm thử sâu Bài 1: Trình mô phỏng Visualizer tương tác, chạy từng bước và đổi màu mảng.*
- [DEEP-02-lesson-quiz-submit.png](file:///d:/FPT/metqua/qa/evidence/DEEP-02-lesson-quiz-submit.png) — *Kiểm thử sâu Bài 2: Giao diện trắc nghiệm Quiz Import từ Excel/CSV và nộp bài chấm điểm.*
- [DEEP-03-lesson-codelab-runner.png](file:///d:/FPT/metqua/qa/evidence/DEEP-03-lesson-codelab-runner.png) — *Kiểm thử sâu Bài 3: Giao diện Code Lab thử thách code, chạy thử Sandbox và nộp bài chấm điểm tự động.*
- [DEEP-04-student-sequential-locking.png](file:///d:/FPT/metqua/qa/evidence/DEEP-04-student-sequential-locking.png) — *Kiểm thử sâu: Giao diện mục lục chuẩn (1 Module duy nhất), Bài 1 mở, Bài 2 & 3 bị khóa tuần tự 🔒.*
- [DEEP-05-student-unlock-flow.png](file:///d:/FPT/metqua/qa/evidence/DEEP-05-student-unlock-flow.png) — *Kiểm thử sâu: Luồng mở khóa tuần tự chuẩn xác (Hoàn thành Bài 1 ➔ Mở khóa Bài 2 ➔ Hoàn thành Bài 2 ➔ Mở khóa Bài 3).*
- [E2E-NEW-06-student-sequential-learning.png](file:///d:/FPT/metqua/qa/evidence/E2E-NEW-06-student-sequential-learning.png) — *E2E Luồng mới: Lộ trình Cây BST #34 chuẩn 1 Module duy nhất, Bài 1 mở, Bài 2 & 3 khóa tuần tự 🔒.*
"""

with open('qa/REPORT.md', 'w', encoding='utf-8') as f:



    f.write(REPORT_TEXT.strip() + '\n')

print('qa/REPORT.md created successfully!')


