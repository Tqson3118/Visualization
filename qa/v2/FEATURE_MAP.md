# FEATURE MAP v2 — BẢN ĐỒ TÍNH NĂNG TOÀN DIỆN DSAVISUAL (MCP AUDIT)

> Dự án: **DsaVisual** — Nền tảng học Cấu trúc Dữ liệu & Giải thuật tương tác trực quan.
> Phiên bản tài liệu: **2.0 (MCP Puppeteer QA Audit)**
> Ngày lập: 2026-09-02
> Công cụ thực thi: **MCP Puppeteer Agent**

---

## I. Danh mục màn hình & Ma trận phân quyền (Screen Matrix v2)

| STT | Tên màn hình | Route | Role được phép | Tính năng chính | API endpoint liên quan | Auth Required? | Ghi chú flow / State Machine |
|---|---|---|---|---|---|---|---|
| 01 | **Trang chủ** | `/` | Guest, Student, Teacher, Admin | Hero giới thiệu, CTA bắt đầu học, thống kê nhanh, danh mục tính năng | `GET /api/v1/public/stats` | ❌ Public | Chuyển hướng tới `/path` hoặc `/login` |
| 02 | **Đăng nhập** | `/login` | Guest only | Đăng nhập email & mật khẩu, ghi nhớ đăng nhập, chuyển hướng redirect | `POST /api/v1/auth/login` | ❌ Public | Sau đăng nhập chuyển về URL trước đó hoặc `/path` |
| 03 | **Đăng ký** | `/register` | Guest only | Đăng ký Student hoặc tick "Tôi là giảng viên" (TEACHER_PENDING) | `POST /api/v1/auth/register` | ❌ Public | Student tự login; Teacher chờ Admin duyệt |
| 04 | **Quên mật khẩu** | `/forgot-password` | Guest, All | Nhập email nhận OTP / link reset | `POST /api/v1/auth/forgot-password` | ❌ Public | Token có hiệu lực 30 phút |
| 05 | **Đặt lại mật khẩu** | `/reset-password` | Guest, All | Nhập token và mật khẩu mới | `POST /api/v1/auth/reset-password` | ❌ Public | Thành công chuyển về `/login` |
| 06 | **Chờ duyệt Giảng viên** | `/pending-teacher` | TEACHER_PENDING | Thông báo tài khoản đang chờ Admin phê duyệt, nút làm mới trạng thái | `GET /api/v1/me/profile`, `GET /api/v1/auth/me` | ✅ Role Pending | Chặn khi cố vào `/studio` hoặc `/admin` |
| 07 | **Danh sách Lộ trình** | `/path` (alias: `/courses`) | Public (Guest xem Active) / All | Duyệt lộ trình, lọc danh mục/độ khó, tìm kiếm tiếng Việt có dấu/không dấu | `GET /api/v1/concepts/courses` | ❌ Public / Filter | Guest/SV chỉ thấy `Active & Public` |
| 08 | **Chi tiết Lộ trình** | `/path/:id` | Requires Auth (Student, Teacher, Admin) | Outline cây bài học, tiến độ cá nhân, tác giả, gửi góp ý CourseFeedback | `GET /api/v1/concepts/courses/{id}`, `GET /api/v1/paths/{id}/items`, `POST /api/v1/courses/feedback` | ✅ Auth | SV chỉ xem được Active hoặc ClassOnly của lớp mình |
| 09 | **Học bài giảng** | `/lessons/:id` | Requires Auth | Học lý thuyết Markdown, Sandbox mô phỏng, Quiz, CodeLab, tính tim/XP/Gems | `GET /api/v1/lessons/{id}`, `POST /api/v1/progress/lessons/{id}/complete`, `POST /api/v1/feedback` | ✅ Auth | Khóa tuần tự; trừ tim khi sai quiz; unlock kế |
| 10 | **Kiểm tra cuối khóa** | `/path/:topicId/final-test` | Requires Auth | Bài test tổng hợp cuối lộ trình | `GET /api/v1/topics/{id}/final-test`, `POST /api/v1/exercises/submit` | ✅ Auth | Mở khóa badge hoàn thành |
| 11 | **Khám phá Mô phỏng** | `/simulations` | Public | Danh mục 44 thuật toán/CTDL, lọc danh mục, Benchmark Big-O | `GET /api/v1/simulations` | ❌ Public | Hiển thị tag Big-O và demo badge |
| 12 | **Chi tiết Mô phỏng** | `/simulator/:key` | Demo: Public (3 key) / Còn lại: Auth | Canvas trực quan hóa, Controls Play/Pause/Step/Speed, Input cấu hình biên | `GET /api/v1/simulations/{key}` | ⚡ Mixed | Demo: `sort.bubble`, `search.binary`, `graph.bfs` |
| 13 | **Code Runner** | `/code/:key` | Requires Auth | Viết code thực thi thuật toán, testcases, bắt lỗi biên và syntax | `POST /api/v1/coderuns/run` | ✅ Auth | Hỗ trợ slug mapping chuẩn hóa |
| 14 | **CheatSheet** | `/cheatsheet` | Requires Auth | Tra cứu nhanh Big-O, code template; Xuất PDF (Premium guard) | `GET /api/v1/concepts/cheatsheet` | ✅ Auth | Free user bị chặn mở modal nâng cấp |
| 15 | **Bài tập Đơn lẻ** | `/exercise/:id` | Requires Auth | Làm bài tập thực hành (MCQ, Predict, Simulation Lab), chấm điểm | `GET /api/v1/exercises/{id}`, `POST /api/v1/exercises/{id}/submit` | ✅ Auth | Hết 0 tim chặn submit bài |
| 16 | **Hồ sơ Cá nhân** | `/profile` | Requires Auth | Xem XP, level, avatar, đổi mật khẩu, thống kê học tập | `GET /api/v1/me/profile`, `PUT /api/v1/me/profile`, `POST /api/v1/auth/change-password` | ✅ Auth | Hiển thị huy hiệu và lịch sử |
| 17 | **Bảng xếp hạng** | `/leaderboard` | Requires Auth | BXH tuần/tháng/all-time theo XP | `GET /api/v1/gamification/leaderboard` | ✅ Auth | Tự động highlight thứ hạng bản thân |
| 18 | **Nhiệm vụ Hằng ngày** | `/quests` | Requires Auth | Danh sách Daily Quests, nhận Gems/XP, empty state | `GET /api/v1/gamification/quests`, `POST /api/v1/gamification/quests/{id}/claim` | ✅ Auth | Chống double-claim (idempotent) |
| 19 | **Cửa hàng Gems** | `/shop` | Requires Auth | Mua tim hồi phục, avatar, streak freeze bằng Gems | `GET /api/v1/gamification/shop`, `POST /api/v1/gamification/shop/buy` | ✅ Auth | Kiểm tra số dư gems, trừ gems chính xác |
| 20 | **Nâng cấp Premium** | `/premium` | Requires Auth | Bảng so sánh quyền lợi gói tháng/năm, VietQR Checkout | `POST /api/v1/me/subscription/checkout` | ✅ Auth | Sinh mã QR động kèm thông tin giao dịch |
| 21 | **Quản lý Gói cước** | `/account/subscription` | Requires Auth | Chi tiết subscription, hạn sử dụng, lịch sử thanh toán | `GET /api/v1/me/subscription` | ✅ Auth | Đồng bộ badge Premium |
| 22 | **Danh sách Lớp học** | `/classes` | Requires Auth | SV xem lớp đã vào, join bằng code; GV quản lý lớp, tạo lớp mới | `GET /api/v1/classes`, `POST /api/v1/classes`, `POST /api/v1/classes/join-by-code` | ✅ Auth | Join 2 lần xử lý idempotent |
| 23 | **Chi tiết Lớp học** | `/classes/:id` | Member / Teacher / Admin | Thành viên, lộ trình gán, deadline đếm ngược, nộp bài tập | `GET /api/v1/classes/{id}`, `GET /api/v1/classes/{id}/curriculum`, `PUT /api/v1/classes/{id}/assignments/deadline` | ✅ Auth | Countdown badge cảnh báo hạn chót |
| 24 | **Báo cáo Lớp học** | `/classes/:id/report` | Teacher sở hữu / Admin | Ma trận tiến độ, tỷ lệ hoàn thành, xuất file CSV báo cáo | `GET /api/v1/classes/{id}/report`, `GET /api/v1/classes/{id}/report/export` | ✅ Teacher/Admin | Quyền hạn bảo vệ chặt chẽ |
| 25 | **Studio Giảng viên** | `/studio` | TEACHER, ADMIN | Soạn thảo giáo trình, cây bài học (Theory/Quiz/Lab), gửi duyệt, xem góp ý | `GET/POST/PUT /api/v1/concepts/courses`, `GET/POST/PUT/DELETE /api/v1/paths/{id}/items`, `GET /api/v1/courses/feedback/for-teacher` | ✅ Teacher/Admin | Modal xác nhận khi rời trang dirty |
| 26 | **Quản lý User Admin** | `/admin/users` | ADMIN | Tìm kiếm, lọc role, duyệt TEACHER_PENDING, khóa/mở khóa, reset password | `GET /api/v1/users`, `PUT /api/v1/users/{id}/status`, `POST /api/v1/users/{id}/approve-teacher`, `POST /api/v1/users/{id}/reset-password` | ✅ Admin only | Chặn truy cập từ Student/Teacher |
| 27 | **Thống kê Admin** | `/admin/stats` | ADMIN | Dashboard số liệu tổng thể (Users, Courses, Orders, Submissions) | `GET /api/v1/admin/stats` | ✅ Admin only | Tối ưu truy vấn, retry và cache |
| 28 | **Cài đặt Admin** | `/admin/settings` | ADMIN | Tham số hệ thống, quản trị BugReports | `GET/PUT /api/v1/settings`, `GET/PUT /api/v1/admin/bug-reports` | ✅ Admin only | Lưu và áp dụng tức thì |
| 29 | **Admin Quản lý Lớp** | `/admin/classes` | ADMIN, TEACHER | Xem toàn bộ lớp học trên hệ thống, quản trị tập trung | `GET /api/v1/classes` | ✅ Admin/Teacher | Danh sách quản trị |
| 30 | **Trợ giúp & Báo lỗi** | `/help` | Public (FAQ) / Auth (Submit) | FAQ, gửi form phản ánh lỗi vào bảng BugReports | `POST /api/v1/bug-reports` | ⚡ Mixed | Dữ liệu lưu vào database |
| 31 | **Chính sách riêng tư** | `/privacy` | Public | Điều khoản sử dụng và chính sách bảo mật | Không gọi API | ❌ Public | Trang tĩnh |
| 32 | **Trang 404** | `/:pathMatch(.*)*` | Public | Thông báo trang không tồn tại | Không gọi API | ❌ Public | Điều hướng về Home |

---

## II. Các Máy Trạng Thái Trọng Yếu (Core State Machines)

### 1. Máy trạng thái Lộ trình (LearningPath)
- **Enum LearningPathStatus**: `Draft (0)` -> `PendingReview (1)` -> `Active (2)` / `Rejected (3)` / `ClassOnly (4)`
- **Enum PathVisibility**: `Private (0)` | `ClassOnly (1)` | `Public (2)`
- **Quy tắc chuyển trạng thái**:
  - `Draft` -> GV gửi duyệt -> `PendingReview` (Chỉ GV tác giả và Admin thấy, ẩn hoàn toàn trên `/path`).
  - `PendingReview` -> Admin bấm Duyệt -> `Active` (Hiển thị ngay lập tức trên `/path` cho tất cả người dùng).
  - `PendingReview` -> Admin từ chối kèm lý do -> `Rejected` (GV nhận thông báo lý do để chỉnh sửa).
  - `Draft` -> GV gán vào lớp học -> `ClassOnly` (Học viên trong lớp thấy trong `/classes/:id`, không cần qua Admin duyệt).

### 2. Máy trạng thái Bài học (Lesson & Node Mutation)
- **Enum LessonStatus**: `Draft (0)` | `PendingReview (1)` | `Active (2)` | `Hidden (3)`
- **Enum PathItemType**: `Folder (0)` | `Theory (1)` | `Quiz (2)` | `Lab (3)`
- **Quy tắc khóa tuần tự (Sequential Locking)**:
  - Bài đầu tiên trong module luôn mở khóa.
  - Bài thứ `N` chỉ mở khóa khi bài thứ `N-1` đã hoàn thành (`status === 'completed'`).
- **Hành vi Đột biến Node (Node Mutation)**:
  - **Ẩn node (Hidden)**: Học sinh đang mở bài bị chặn nội dung, thông báo bài học đang bảo trì/ẩn bởi giảng viên. Tiến độ tổng của lộ trình tự động loại trừ node ẩn.
  - **Xóa node (Delete)**: Node bị xóa khỏi cây, học sinh đang ở URL đó được điều hướng an toàn về outline lộ trình, không crash ứng dụng.
  - **Thêm node mới**: Khi lộ trình có node mới được thêm vào, % tiến độ của học sinh đã đạt 100% được tính toán lại chính xác.

### 3. Máy trạng thái Lớp học & Hạn nộp (Class & Deadline)
- **Enum ClassStatus**: `Open (0)` | `Closed (1)`
- **Deadline**: GV cấu hình hạn nộp (chặn đặt ngày trong quá khứ); Badge đếm ngược màu vàng (<3 ngày), đỏ (<24h); Nộp sau hạn đánh dấu "Trễ hạn".

### 4. Máy trạng thái Gamification (Hearts / Gems / XP / Quests)
- **Tim (Hearts)**: Tối đa 5 tim (Free) / 30 tim (Premium). Trả lời sai quiz trừ 1 tim. 0 tim chặn làm bài mới, gợi ý nạp/mua tại Shop.
- **Gems & Quests**: Hoàn thành nhiệm vụ/bài học nhận Gems; Claim nhiệm vụ là idempotent; Mua tim tại Shop kiểm tra số dư chặt chẽ.

### 5. Máy trạng thái Premium Subscription
- **Free** -> Checkout VietQR -> Thanh toán -> **Premium Active** (Badge Pro/Premium trên header, mở khóa PDF CheatSheet, mở rộng giới hạn tim).

---

## III. Danh mục 44 Mô Phỏng (Simulator Catalog v2)

| # | Key | Tên thuật toán / CTDL | Nhóm | Level | Demo Public | Input biên cần kiểm tra |
|---|---|---|---|---|---|---|
| 1 | `sort.bubble` | Sắp xếp nổi bọt (Bubble Sort) | Algorithm / Sort | Basic | ✅ Có | `[]`, `[5]`, `[-3,1,-7]`, `[3,3,3]`, `[99999]` |
| 2 | `sort.selection` | Sắp xếp chọn (Selection Sort) | Algorithm / Sort | Basic | ❌ Không | `[]`, `[1]`, `[9,8,7,6,5]`, `[5,5,5]` |
| 3 | `sort.insertion` | Sắp xếp chèn (Insertion Sort) | Algorithm / Sort | Basic | ❌ Không | `[]`, `[1]`, mảng đã sorted, mảng đảo ngược |
| 4 | `sort.merge` | Sắp xếp trộn (Merge Sort) | Algorithm / Sort | Advanced | ❌ Không | `[]`, `[1]`, mảng lẻ phần tử, mảng trùng |
| 5 | `sort.quick` | Sắp xếp nhanh (Quick Sort) | Algorithm / Sort | Advanced | ❌ Không | `[]`, `[1]`, pivot xấu nhất, mảng trùng |
| 6 | `sort.heap` | Sắp xếp vun đống (Heap Sort) | Algorithm / Sort | Advanced | ❌ Không | `[]`, `[1]`, mảng có số âm |
| 7 | `search.linear` | Tìm kiếm tuyến tính (Linear Search) | Algorithm / Search | Basic | ❌ Không | Target ở đầu, cuối, không tồn tại, mảng rỗng |
| 8 | `search.binary` | Tìm kiếm nhị phân (Binary Search) | Algorithm / Search | Basic | ✅ Có | Mảng rỗng, target không có, target ở biên |
| 9 | `stack.push` | Ngăn xếp — Push | Algorithm / Stack | Basic | ❌ Không | Push liên tục đến max capacity |
| 10 | `stack.pop` | Ngăn xếp — Pop | Algorithm / Stack | Basic | ❌ Không | **Pop trên stack rỗng** |
| 11 | `stack.peek` | Ngăn xếp — Peek | Algorithm / Stack | Basic | ❌ Không | **Peek trên stack rỗng** |
| 12 | `queue.enqueue` | Hàng đợi — Enqueue | Algorithm / Queue | Basic | ❌ Không | Enqueue liên tục |
| 13 | `queue.dequeue` | Hàng đợi — Dequeue | Algorithm / Queue | Basic | ❌ Không | **Dequeue trên queue rỗng** |
| 14 | `list.insert` | DSLK Đơn — Chèn | Algorithm / List | Basic | ❌ Không | Chèn đầu, chèn cuối, vị trí âm, vị trí vượt size |
| 15 | `list.delete` | DSLK Đơn — Xóa | Algorithm / List | Basic | ❌ Không | **Xóa trên list rỗng**, xóa phần tử không có |
| 16 | `list.search` | DSLK Đơn — Tìm kiếm | Algorithm / List | Basic | ❌ Không | Tìm node không tồn tại, list rỗng |
| 17 | `list.traverse` | DSLK Đơn — Duyệt | Algorithm / List | Basic | ❌ Không | **Duyệt trên list rỗng**, list 1 node |
| 18 | `tree.bst-insert` | BST — Chèn | Algorithm / Tree | Basic | ❌ Không | **Chèn vào cây rỗng**, chèn giá trị trùng |
| 19 | `tree.bst-delete` | BST — Xóa | Algorithm / Tree | Advanced | ❌ Không | Xóa node lá, node 1 con, node 2 con, không có |
| 20 | `tree.bst-search` | BST — Tìm kiếm | Algorithm / Tree | Basic | ❌ Không | Tìm node gốc, node lá, không tồn tại |
| 21 | `tree.bst-preorder` | BST — Duyệt Preorder | Algorithm / Tree | Basic | ❌ Không | Cây rỗng, cây 1 node, cây lệch trái/phải |
| 22 | `tree.bst-inorder` | BST — Duyệt Inorder | Algorithm / Tree | Basic | ❌ Không | Cây rỗng, cây 1 node (kết quả tăng dần) |
| 23 | `tree.bst-postorder` | BST — Duyệt Postorder | Algorithm / Tree | Basic | ❌ Không | Cây rỗng, cây 1 node |
| 24 | `tree.bst-levelorder` | BST — Duyệt Level-order | Algorithm / Tree | Basic | ❌ Không | Cây rỗng, cây đầy đủ, cây lệch |
| 25 | `tree.avl-insert` | Cây AVL — Chèn & Xoay | Algorithm / AVL | Advanced | ❌ Không | Kích hoạt xoay LL, RR, LR, RL |
| 26 | `heap.insert` | Đống nhị phân — Chèn | Algorithm / Heap | Advanced | ❌ Không | Chèn phần tử lớn nhất / nhỏ nhất |
| 27 | `heap.extract` | Đống nhị phân — Trích xuất Max | Algorithm / Heap | Advanced | ❌ Không | **Trích xuất từ heap rỗng**, heap 1 phần tử |
| 28 | `heap.heapify` | Đống nhị phân — Heapify | Algorithm / Heap | Advanced | ❌ Không | Mảng đã sorted sẵn, mảng đảo ngược |
| 29 | `hash.insert` | Bảng băm — Chèn | Algorithm / Hash | Basic | ❌ Không | Chèn trùng key, collision cùng slot |
| 30 | `hash.search` | Bảng băm — Tìm kiếm | Algorithm / Hash | Basic | ❌ Không | Key không tồn tại, sau khi có collision |
| 31 | `hash.delete` | Bảng băm — Xóa | Algorithm / Hash | Basic | ❌ Không | Xóa key không tồn tại, xóa trong chuỗi nối kết |
| 32 | `graph.bfs` | Đồ thị — Duyệt BFS | Algorithm / Graph | Basic | ✅ Có | Đồ thị không liên thông, đỉnh cô lập |
| 33 | `graph.dfs` | Đồ thị — Duyệt DFS | Algorithm / Graph | Basic | ❌ Không | Đồ thị có chu trình (cycle), đồ thị cây |
| 34 | `graph.dijkstra` | Đồ thị — Dijkstra | Algorithm / Graph | Advanced | ❌ Không | Không có đường đi đến đích, đồ thị rỗng |
| 35 | `structure.array` | Cấu trúc Mảng | Structure | Basic | ❌ Không | Render trực quan, tra cứu chỉ số |
| 36 | `structure.linkedlist` | Cấu trúc DSLK Đơn | Structure | Basic | ❌ Không | Render con trỏ next, head, tail |
| 37 | `structure.stack` | Cấu trúc Ngăn xếp | Structure | Basic | ❌ Không | Render đỉnh top, cơ chế LIFO |
| 38 | `structure.queue` | Cấu trúc Hàng đợi | Structure | Basic | ❌ Không | Render đầu front, đuôi rear, FIFO |
| 39 | `structure.binarytree` | Cấu trúc Cây nhị phân | Structure | Basic | ❌ Không | Render nút gốc, con trái/phải |
| 40 | `structure.bst` | Cấu trúc Cây BST | Structure | Basic | ❌ Không | Render tính chất BST (trái < gốc < phải) |
| 41 | `structure.avl` | Cấu trúc Cây AVL | Structure | Advanced | ❌ Không | Render hệ số cân bằng Balance Factor |
| 42 | `structure.heap` | Cấu trúc Đống nhị phân | Structure | Advanced | ❌ Không | Render biểu diễn mảng vs cây nhị phân |
| 43 | `structure.hashtable` | Cấu trúc Bảng băm | Structure | Basic | ❌ Không | Render mảng bucket và chuỗi liên kết |
| 44 | `structure.graph` | Cấu trúc Đồ thị | Structure | Advanced | ❌ Không | Render ma trận kề / danh sách kề |
