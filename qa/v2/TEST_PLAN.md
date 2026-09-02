# TEST PLAN v2 — MA TRẬN KỊCH BẢN KIỂM THỬ TOÀN DIỆN DSAVISUAL (MCP AUDIT)

> Dự án: **DsaVisual** — Nền tảng học Cấu trúc Dữ liệu & Giải thuật tương tác trực quan.
> Phiên bản tài liệu: **2.0 (MCP Puppeteer QA Audit Plan)**
> Ngày lập: 2026-09-02
> Phạm vi: Toàn diện **Phase 0 → Phase 5** (Bao gồm 44 Simulator Edge Cases & Cross-Role Node Mutations)

---

## I. Tổng quan Chiến lược Kiểm thử v2

| Nhóm kịch bản | Mã | Số lượng kịch bản | Trọng tâm kiểm thử v2 |
|---|---|---|---|
| **A. Lộ trình học sinh** | TP-A | 6 kịch bản | Học bài, khóa tuần tự, trừ tim, **Ẩn/Xóa node khi đang học (TP-A03/A04)**, **Thêm node mới khi học 100% (TP-A05)** |
| **B. Studio Giảng viên** | TP-B | 6 kịch bản | CRUD lộ trình, cây bài học, Dirty modal (**QA-007**), **PendingReview gating (TP-B04)**, ClassOnly, BE 403 Draft |
| **C. Quản trị Admin** | TP-C | 4 kịch bản | Duyệt lộ trình Active ngay lập tức, duyệt Teacher, Lock user, Thống kê hệ thống (**QA-002**) |
| **D. Lớp học & Deadline** | TP-D | 4 kịch bản | Tạo lớp, mời sinh viên, **Chặn deadline quá khứ (TP-D02)**, Đếm ngược hạn nộp (**QA-010**), Báo cáo tiến độ |
| **E. Phản hồi & Đánh giá**| TP-E | 2 kịch bản | Gửi góp ý lộ trình, Giảng viên phản hồi trong Studio Feedback |
| **F. 44 Mô phỏng & Biên** | TP-F | 44 kịch bản | **Kiểm tra toàn bộ 44 simulator keys với input biên, rỗng, âm, trùng, collision, cycle graph** |
| **G. Công cụ bổ trợ** | TP-G | 3 kịch bản | Code Runner slug mapping (**QA-003**), CheatSheet Premium Guard (**QA-006**), Final Test |
| **H. Gamification & Shop** | TP-H | 4 kịch bản | Nhiệm vụ rỗng (**QA-005**), chống claim 2 lần, Shop mua tim & kiểm tra trừ gems, Leaderboard |
| **I. Quản lý Đăng ký GV** | TP-I | 2 kịch bản | Luồng TEACHER_PENDING, Nút làm mới trạng thái (**QA-009**), Admin phê duyệt |
| **J. UX, Responsive, An ninh**| TP-J | 6 kịch bản | Viewport 375px (**QA-008**), 1440px, Backend 401/403 Security, Token hết hạn, XSS escaping |

---

## II. Chi tiết Ma trận Kịch bản Kiểm thử

### TP-A: Lộ trình & Trải nghiệm Học sinh (Student Journey)

| Mã | Tên kịch bản | Mục tiêu & Hành vi kiểm thử | Loại kiểm thử | Kỳ vọng |
|---|---|---|---|---|
| **TP-A01** | Học bài tuần tự & Khóa node | SV hoàn thành bài Theory -> Quiz -> CodeLab. Đáp án sai trừ tim; đúng mở khóa bài tiếp theo | Regression (**QA-011**) | Chỉ bài 1 mở; bài 2, 3 khóa chặt. Hoàn thành bài trước mới mở bài sau |
| **TP-A02** | Refresh trang khi đang học | Đang làm dở quiz / theory, nhấn F5 / Refresh | Mới | Tiến độ không bị mất, trạng thái làm bài bảo lưu |
| **TP-A03** | **Ẩn node khi SV đang học** | GV chuyển trạng thái 1 bài học sang `Hidden` trong khi SV đang mở bài đó | **Mới — Trọng tâm** | SV nhận thông báo bài học đang bảo trì/ẩn; cây outline cập nhật % không tính bài ẩn |
| **TP-A04** | **Xóa node khi SV đang học** | GV xóa bài học khỏi cây giáo trình khi SV đang ở URL bài học đó | **Mới — Trọng tâm** | SV được điều hướng an toàn về outline lộ trình kèm thông báo rõ, không crash 500 |
| **TP-A05** | **Thêm node khi SV đạt 100%** | SV đã hoàn thành 100% lộ trình; GV thêm 1 bài học mới vào lộ trình | **Mới — Trọng tâm** | % tiến độ của SV tự động giảm tương ứng (VD: từ 100% xuống 75%), node mới khóa cho đến khi học |
| **TP-A06** | Thoát giữa chừng khi làm bài | SV nhấn nút Back trình duyệt khi đang trong màn hình Quiz | Mới | Không bị trừ tim oan uổng nếu chưa bấm nộp bài |

---

### TP-B: Studio Giảng viên (Teacher Curriculum & Studio)

| Mã | Tên kịch bản | Mục tiêu & Hành vi kiểm thử | Loại kiểm thử | Kỳ vọng |
|---|---|---|---|---|
| **TP-B01** | Tạo lộ trình mới & Cây bài | GV tạo Course -> Thêm Module -> Thêm Theory, Quiz, Lab | Kế thừa & Mở rộng | Cây bài học hiển thị đúng cấp bậc Folder/Node |
| **TP-B02** | Soạn thảo Theory & Quiz | Soạn Markdown có preview, thêm câu hỏi quiz nhiều lựa chọn | Kế thừa | Preview hiển thị chuẩn xác, lưu dữ liệu thành công |
| **TP-B03** | Cảnh báo Dirty khi chuyển tab | GV sửa nội dung chưa bấm Lưu, click chuyển sang tab khác hoặc thoát | Regression (**QA-007**) | Hiển thị Confirm Modal đồng bộ giao diện Dark Mode, không dùng window.confirm native |
| **TP-B04** | **Gửi duyệt & Pending Gating** | GV gửi duyệt lộ trình -> Chuyển sang `PendingReview`. Kiểm tra phía SV và Guest | **Mới — Trọng tâm** | Lộ trình PendingReview KHÔNG xuất hiện trên `/path` của SV/Guest. Backend trả về 403 nếu SV gõ URL trực tiếp |
| **TP-B05** | Lộ trình ClassOnly | GV tạo lộ trình chỉ dành riêng cho lớp học | Mới | Chỉ sinh viên trong lớp được gán mới thấy và học được |
| **TP-B06** | Bảo vệ URL Draft | SV cố tình truy cập vào URL bài học thuộc lộ trình Draft của GV khác | **Mới — Security** | Backend trả về 403 Forbidden / 404 Not Found, không lộ nội dung giáo trình |

---

### TP-C: Bảng điều khiển Quản trị (Admin Console)

| Mã | Tên kịch bản | Mục tiêu & Hành vi kiểm thử | Loại kiểm thử | Kỳ vọng |
|---|---|---|---|---|
| **TP-C01** | Phê duyệt lộ trình Pending | Admin vào Moderation tab duyệt lộ trình -> Chuyển sang `Active` | Mới — Cross-role | Lộ trình xuất hiện NGAY LẬP TỨC trên danh sách `/path` của tất cả người dùng |
| **TP-C02** | Duyệt tài khoản Giảng viên | Admin vào `/admin/users` duyệt ứng viên `TEACHER_PENDING` | Kế thừa — Cross-role | User được nâng quyền thành `TEACHER`, đăng nhập vào được `/studio` |
| **TP-C03** | Khóa & Mở khóa tài khoản | Admin khóa tài khoản user vi phạm | Mới — Security | User bị khóa không thể đăng nhập hoặc bị revoke token lập tức |
| **TP-C04** | Thống kê hệ thống | Tải dữ liệu trang `/admin/stats` | Regression (**QA-002**) | Dashboard tải mượt mà, đầy đủ 13 chỉ số, có cơ chế retry và cache không báo lỗi |

---

### TP-D: Quản lý Lớp học & Hạn nộp (Classes & Deadlines)

| Mã | Tên kịch bản | Mục tiêu & Hành vi kiểm thử | Loại kiểm thử | Kỳ vọng |
|---|---|---|---|---|
| **TP-D01** | Tạo lớp & Sinh viên tham gia | GV tạo lớp, lấy mã mời 6 ký tự; SV nhập mã tham gia lớp | Kế thừa & Regression (**QA-004**) | Tham gia thành công. Nhập lại mã lần 2 xử lý Idempotent thông báo đã là thành viên |
| **TP-D02** | **Chặn đặt Deadline quá khứ** | GV cố tình nhập hạn nộp bài tập là một ngày/giờ trong quá khứ | **Mới — Validation** | Form hiển thị thông báo lỗi và chặn không cho submit deadline quá khứ |
| **TP-D03** | Chỉ báo Deadline đếm ngược | Xem chi tiết bài tập có hạn nộp | Regression (**QA-010**) | Hiển thị badge đếm ngược (VD: 'Còn 2 ngày', 'Hết hạn sau 5 giờ') kèm màu cảnh báo |
| **TP-D04** | Báo cáo & Xuất dữ liệu lớp | GV xem ma trận tiến độ học tập của các thành viên trong lớp | Kế thừa | Thống kê tỷ lệ hoàn thành chính xác, xuất file CSV đầy đủ |

---

### TP-E: Phản hồi & Góp ý (Feedback & Moderation)

| Mã | Tên kịch bản | Mục tiêu & Hành vi kiểm thử | Loại kiểm thử | Kỳ vọng |
|---|---|---|---|---|
| **TP-E01** | Gửi phản hồi bài học / lộ trình | SV gửi góp ý nội dung bài học; GV vào `/studio?tab=feedback` xem và trả lời | Kế thừa & Mở rộng | GV nhận được feedback, gửi phản hồi thành công |
| **TP-E02** | Báo lỗi hệ thống (/help) | Người dùng gửi form báo lỗi tại trang `/help` | Kế thừa | Gửi thành công vào cơ sở dữ liệu `BugReports` |

---

### TP-F: 44 Mô Phỏng & Kiểm thử Biên (Simulations & Edge Cases)

| Nhóm | Số lượng | Kịch bản kiểm thử biên chi tiết |
|---|---|---|
| **1. Sorting (6 key)** | 6 | `sort.bubble`, `sort.selection`, `sort.insertion`, `sort.merge`, `sort.quick`, `sort.heap`: Kiểm tra mảng rỗng `[]`, mảng 1 phần tử `[5]`, mảng số âm `[-10, 5, -3]`, mảng trùng lặp `[4, 4, 4, 4]`, mảng số lớn `[99999]` |
| **2. Searching (2 key)** | 2 | `search.linear`, `search.binary`: Mảng rỗng, tìm kiếm phần tử không tồn tại trong mảng, tìm kiếm phần tử ở vị trí biên (đầu/cuối) |
| **3. Stack (3 key)** | 3 | `stack.push`, `stack.pop`, `stack.peek`: **Pop trên stack rỗng**, **Peek trên stack rỗng**, Push đầy ngăn xếp |
| **4. Queue (2 key)** | 2 | `queue.enqueue`, `queue.dequeue`: **Dequeue trên queue rỗng**, Enqueue liên tục |
| **5. Linked List (4 key)** | 4 | `list.insert`, `list.delete`, `list.search`, `list.traverse`: **Delete trên list rỗng**, **Traverse trên list rỗng**, Chèn vị trí âm |
| **6. BST (7 key)** | 7 | `tree.bst-insert`, `tree.bst-delete`, `tree.bst-search`, `tree.bst-preorder`, `tree.bst-inorder`, `tree.bst-postorder`, `tree.bst-levelorder`: **Thao tác trên cây rỗng**, cây 1 node, xóa node không tồn tại |
| **7. AVL Tree (1 key)** | 1 | `tree.avl-insert`: Kiểm tra kích hoạt 4 trường hợp mất cân bằng và xoay cây (LL, RR, LR, RL) |
| **8. Binary Heap (3 key)** | 3 | `heap.insert`, `heap.extract`, `heap.heapify`: **Extract trên heap rỗng**, heapify mảng đã sắp xếp sẵn |
| **9. Hash Table (3 key)** | 3 | `hash.insert`, `hash.search`, `hash.delete`: Va chạm nhiều key vào cùng slot (Chaining collision), tìm/xóa key không tồn tại |
| **10. Graph (3 key)** | 3 | `graph.bfs`, `graph.dfs`, `graph.dijkstra`: Đồ thị có đỉnh cô lập không liên thông, đồ thị có chu trình khép kín (Cycle), tìm đường không tồn tại |
| **11. Structures (10 key)** | 10 | `structure.array`, `structure.linkedlist`, `structure.stack`, `structure.queue`, `structure.binarytree`, `structure.bst`, `structure.avl`, `structure.heap`, `structure.hashtable`, `structure.graph`: Render trực quan, cấu trúc dữ liệu tĩnh |

---

### TP-G: Công cụ Bổ trợ (Auxiliary Tools)

| Mã | Tên kịch bản | Mục tiêu & Hành vi kiểm thử | Loại kiểm thử | Kỳ vọng |
|---|---|---|---|---|
| **TP-G01** | Slug mapping Code Runner | Truy cập `/code/bubble-sort` (slug gạch ngang) | Regression (**QA-003**) | Tự động chuyển đổi hoặc nhận dạng map sang `sort.bubble`, không báo lỗi không tìm thấy |
| **TP-G02** | CheatSheet Premium Guard | User Free bấm nút xuất PDF CheatSheet | Regression (**QA-006**) | Mở modal mời nâng cấp Premium, không mở thẳng hộp thoại in native |
| **TP-G03** | Tra cứu CheatSheet & Lọc | Lọc theo độ phức tạp thời gian/không gian và tìm kiếm | Kế thừa | Hiển thị bảng tra cứu chính xác |

---

### TP-H: Gamification, Nhiệm vụ & Cửa hàng (Gamification & Store)

| Mã | Tên kịch bản | Mục tiêu & Hành vi kiểm thử | Loại kiểm thử | Kỳ vọng |
|---|---|---|---|---|
| **TP-H01** | Empty State Nhiệm vụ | Vào `/quests` khi chưa có nhiệm vụ | Regression (**QA-005**) | Hiển thị Empty State đẹp mắt, không hiển thị 0/0 Done |
| **TP-H02** | Nhận thưởng Quests Idempotent | Bấm nhận thưởng 1 quest nhiều lần liên tiếp | Mới | Nhận thưởng 1 lần duy nhất, không bị duplicate gems/XP |
| **TP-H03** | Mua tim tại Shop | Mua tim khi đủ gems và khi thiếu gems | Mới | Đủ gems: trừ gems và hồi tim. Thiếu gems: thông báo lỗi rõ ràng |
| **TP-H04** | Bảng xếp hạng XP | Xem bảng xếp hạng tuần và tháng | Kế thừa | Highlight đúng vị trí người dùng |

---

### TP-I: Quy trình Đăng ký Giảng viên (Teacher Registration Flow)

| Mã | Tên kịch bản | Mục tiêu & Hành vi kiểm thử | Loại kiểm thử | Kỳ vọng |
|---|---|---|---|---|
| **TP-I01** | Nút làm mới trạng thái duyệt | Đăng nhập tài khoản `TEACHER_PENDING` tại `/pending-teacher` | Regression (**QA-009**) | Có nút "Làm mới trạng thái" gọi API cập nhật quyền mà không cần logout |
| **TP-I02** | Chặn vào Studio khi chưa duyệt | `TEACHER_PENDING` gõ URL `/studio` | Mới — Security | Bị điều hướng chặn về lại `/pending-teacher` |

---

### TP-J: Trải nghiệm Giao diện, Responsive & Bảo mật (UX, Responsive & Security)

| Mã | Tên kịch bản | Mục tiêu & Hành vi kiểm thử | Loại kiểm thử | Kỳ vọng |
|---|---|---|---|---|
| **TP-J01** | Responsive Mobile 375px | Xem Simulator controls trên Viewport 375px (iPhone SE) | Regression (**QA-008**) | Thanh footer controls co giãn chuẩn, không bị tràn cuộn ngang |
| **TP-J02** | Responsive Desktop 1440px | Toàn bộ các trang trên màn hình lớn 1440px | Kế thừa | Bố cục cân đối, không vỡ layout |
| **TP-J03** | Chặn truy cập trái phép phía Backend | Gọi trực tiếp API `/api/v1/users` hoặc `/api/v1/admin/stats` không token hoặc bằng role Student | **Mới — Security BE** | Backend trả về đúng HTTP 401 Unauthorized / HTTP 403 Forbidden |
| **TP-J04** | XSS Protection form Input | Nhập mã độc `<script>alert(1)</script>` tại form đăng nhập/đăng ký/search | Mới — Security | Dữ liệu được escape an toàn, không thực thi script |
| **TP-J05** | Form Double Submit | Click đúp chuột liên tục vào nút Submit đăng ký / mua shop | Mới — Concurrency | Vô hiệu hóa nút trong lúc gửi (debouncing/loading), không tạo duplicate |
| **TP-J06** | Header flicker khi token hết hạn | Mở trang khi phiên đăng nhập hết hạn | Regression (**QA-001**) | Header hiển thị trạng thái Guest ngay lập tức, không bị chớp widget |
