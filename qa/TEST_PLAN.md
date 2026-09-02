# TEST PLAN — MA TRẬN KỊCH BẢN KIỂM THỬ TOÀN DIỆN DSAVISUAL

> Dự án: **DsaVisual** — Nền tảng học Cấu trúc Dữ liệu & Giải thuật tương tác trực quan.
> Thực hiện bởi: **QA Explorer** (Playwright MCP Engine).
> Phiên bản: 1.0 (Audit Baseline).
> Ngày lập: 2026-09-02.

---

## I. Mục tiêu & Nguyên tắc kiểm thử

1. **Khó tính & Phản biện**: Đánh giá không chỉ về chức năng PASS/FAIL mà còn về trải nghiệm người dùng (UX), tính nhất quán dữ liệu (Data Integrity), đồng bộ trạng thái (State Sync) và phân quyền (RBAC Security).
2. **Kiểm tra chéo (Cross-role Verification)**: Mọi thao tác CRUD (tạo ở vai trò A -> kiểm tra hiển thị và quyền ở vai trò B) phải được đối chiếu ở cả Frontend UI, Console log và Network responses.
3. **Thử thách dữ liệu biên**: Kiểm tra các trường hợp input rỗng, whitespace, chuỗi siêu dài (>500 ký tự), emoji, ký tự đặc biệt, mã HTML/XSS script, số âm, ngày trong quá khứ, double-submit, refresh/back giữa chừng form.
4. **Phạm vi Simulator**: Kiểm thử toàn bộ 44 thuật toán và cấu trúc dữ liệu trong danh mục engines/catalog.ts.

---

## II. Ma trận Kịch bản Chi tiết (Test Scenarios Matrix)

### A. Lộ trình học phía Học viên (Trọng tâm trải nghiệm)

| Mã test | Kịch bản | Role | Màn / URL | Các bước thực hiện | Kết quả kỳ vọng |
|---|---|---|---|---|---|
| **TP-A01** | Học liên tục nhiều node & unlock tự động | Student | /path/:id, /lessons/:id | 1) Vào lộ trình Active. 2) Học bài Theory, làm Quiz, làm CodeLab. 3) Hoàn thành bài học. | Node tiếp theo được mở khóa tự động, % tiến độ tăng chính xác, nút Học tiếp trên lộ trình trỏ đúng bài kế tiếp. |
| **TP-A02** | Lưu trạng thái tiến độ qua refresh/logout | Student | /path/:id, /lessons/:id | 1) Học dở ở node 2. 2) Refresh trình duyệt. 3) Logout rồi Login lại tài khoản. | Tiến độ % giữ nguyên, nút Học tiếp đưa học viên vào đúng vị trí đang học dở. |
| **TP-A03** | GV ẨN (Hidden) node khi SV đang học | Teacher + Student | /studio, /lessons/:id | 1) SV mở bài học node X. 2) GV chuyển trạng thái node X thành Hidden trong Studio. 3) SV refresh bài học. | SV nhận thông báo bài học đang tạm ẩn để cập nhật (không crash), trang lộ trình cập nhật lại tổng số node và % tiến độ. |
| **TP-A04** | GV XÓA node khi SV đang học | Teacher + Student | /studio, /path/:id | 1) GV xóa node X khỏi lộ trình. 2) SV mở lại trang chi tiết lộ trình /path/:id. | Hệ thống không bị crash, node kế tiếp mở hợp lệ nếu thỏa điều kiện, % tiến độ không bị nhảy lỗi. |
| **TP-A05** | GV THÊM node mới khi SV đã hoàn thành 100% | Teacher + Student | /studio, /path/:id | 1) SV đã học xong 100% lộ trình. 2) GV thêm node mới vào lộ trình. 3) SV vào lại trang lộ trình. | % hoàn thành tụt xuống tương ứng, có thông báo rõ ràng, node mới ở trạng thái mở để học tiếp. |
| **TP-A06** | Bỏ dở giữa bài (Back / Refresh) | Student | /lessons/:id | 1) Làm dở Quiz/Code chưa submit. 2) Bấm nút Back hoặc reload trang. 3) Vào lại bài. | Không bị mất tim vô lý, trạng thái bản nháp được xử lý an toàn, cho phép làm lại từ đầu. |

---

### B. Studio Giáo viên (Biên soạn & Quản lý trạng thái)

| Mã test | Kịch bản | Role | Màn / URL | Các bước thực hiện | Kết quả kỳ vọng |
|---|---|---|---|---|---|
| **TP-B01** | Tạo mới lộ trình từ đầu | Teacher | /studio | 1) Tạo lộ trình mới. 2) Thêm Folder, Theory, Quiz, CodeLab. 3) Soạn nội dung Markdown/Tiptap, gắn mô phỏng. | Cây giáo trình hiển thị cấu trúc phân cấp chuẩn xác, lưu trữ đầy đủ các thuộc tính của từng loại node. |
| **TP-B02** | Đối chiếu tính năng Studio với dữ liệu Seed | Teacher | /studio | 1) So sánh các trường của SeedTeacherCoursesData (Title, Desc, Objectives, Highlights, Testimonials, Difficulty, Category). | UI Teacher Studio hỗ trợ nhập và sửa đầy đủ tất cả các trường dữ liệu như dữ liệu Seed. |
| **TP-B03** | Trạng thái lưu & Cảnh báo Unsaved Changes | Teacher | /studio | 1) Chỉnh sửa bài giảng chưa bấm Lưu. 2) Chuyển sang tab khác hoặc bấm rời trang. | Hiển thị modal cảnh báo thay đổi chưa lưu, tránh mất dữ liệu hoặc sai lệch trạng thái. |
| **TP-B04** | Gửi duyệt lộ trình (PendingReview Gating) | Teacher + Guest/Student | /studio, /path | 1) GV bấm gửi công khai lộ trình. 2) Kiểm tra trạng thái thành PendingReview. 3) Đăng nhập SV/Guest vào /path. | Lộ trình KHÔNG xuất hiện trên /path của SV và Khách cho tới khi được Admin phê duyệt. |
| **TP-B05** | Lộ trình ClassOnly & Lộ trình Rejected | Teacher | /studio | 1) GV chọn chế độ ClassOnly và gán cho lớp. 2) Admin từ chối lộ trình kèm lý do. | ClassOnly dùng được ngay cho lớp không cần admin duyệt; Rejected hiển thị lý do từ chối và cho phép GV sửa gửi lại. |
| **TP-B06** | Draft Gating khi gõ URL trực tiếp | Student, Guest | /path/:id | 1) Guest/Student gõ URL trực tiếp tới lộ trình Draft của GV. | Bị chặn ở FE và BE trả mã lỗi 403 Forbidden / 404 Not Found, không lộ nội dung nháp. |

---

### C. Admin Console (Phê duyệt & Quản trị Hệ thống)

| Mã test | Kịch bản | Role | Màn / URL | Các bước thực hiện | Kết quả kỳ vọng |
|---|---|---|---|---|---|
| **TP-C01** | Duyệt lộ trình chờ duyệt (PendingReview) | Admin + Student | /studio?tab=moderation, /path | 1) Admin vào tab Duyệt lộ trình. 2) Xem trước cây bài học và bấm Duyệt. 3) Vào /path phía SV. | Lộ trình chuyển sang Active, xuất hiện NGAY LẬP TỨC trên danh sách /path cho toàn bộ học viên và khách. |
| **TP-C02** | Duyệt tài khoản Giảng viên chờ duyệt | Admin + TeacherPending | /admin/users, /studio | 1) Admin duyệt yêu cầu của TEACHER_PENDING. 2) Tài khoản đó đăng nhập lại. 3) Truy cập /studio. | Role chuyển thành TEACHER, tài khoản truy cập được vào /studio và bắt đầu soạn bài giảng bình thường. |
| **TP-C03** | Quản lý User: Khóa/Mở khóa & Reset mật khẩu | Admin | /admin/users | 1) Khóa 1 tài khoản user (IsActive = false). 2) Thử đăng nhập bằng tài khoản bị khóa. 3) Reset password. | Tài khoản bị khóa không thể đăng nhập; reset password cập nhật mật khẩu mới thành công. |
| **TP-C04** | Kiểm tra Admin Stats & Settings | Admin | /admin/stats, /admin/settings | 1) Đối chiếu thủ công các chỉ số (Tổng Users, Students, Teachers, Courses, Classes, Active Today). 2) Đổi tham số cài đặt. | Số liệu thống kê khớp 100% với dữ liệu thực trong DB, thay đổi cài đặt hệ thống có hiệu lực tức thì. |

---

### D. Lớp học & Hạn nộp (Class & Assignments)

| Mã test | Kịch bản | Role | Màn / URL | Các bước thực hiện | Kết quả kỳ vọng |
|---|---|---|---|---|---|
| **TP-D01** | GV tạo lớp, lấy mã mời & gán lộ trình | Teacher | /classes, /classes/:id | 1) Tạo lớp học mới, nhận Invite Code 6 ký tự. 2) Gán lộ trình học vào lớp. | Lớp học được khởi tạo thành công, lộ trình và danh sách bài học hiển thị chính xác trong lớp. |
| **TP-D02** | Cấu hình Deadline trong quá khứ & Nộp muộn | Teacher + Student | /classes/:id | 1) GV đặt hạn nộp DueAt trong quá khứ. 2) SV mở chi tiết lớp học. 3) SV nộp bài sau hạn. | SV thấy badge Quá hạn màu đỏ rõ ràng; bài nộp được đánh dấu trạng thái Nộp muộn trong ClassReport. |
| **TP-D03** | SV tham gia lớp bằng mã mời | Student | /classes | 1) Thử nhập sai mã mời. 2) Nhập mã lớp khác. 3) Nhập đúng mã và thử tham gia 2 lần. | Báo lỗi rõ ràng khi mã sai, tham gia đúng lớp khi mã đúng, chặn tham gia trùng lặp. |
| **TP-D04** | Báo cáo Lớp học (ClassReport) & Xuất CSV | Teacher | /classes/:id/report | 1) Xem ma trận tiến độ của từng học viên. 2) Bấm Xuất báo cáo CSV. | Số liệu tiến độ và bài nộp khớp 100% với hoạt động thật, tải về file CSV định dạng chuẩn. |
| **TP-D05** | Xóa SV khỏi lớp / Gỡ gán lộ trình | Teacher + Student | /classes/:id | 1) GV xóa 1 SV khỏi lớp. 2) SV kiểm tra danh sách lớp và tiến độ cá nhân. | SV không còn trong lớp nhưng không bị mất tiến độ học cá nhân trên lộ trình. |

---

### E. Feedback & Trợ giúp (CourseFeedback & /help)

| Mã test | Kịch bản | Role | Màn / URL | Các bước thực hiện | Kết quả kỳ vọng |
|---|---|---|---|---|---|
| **TP-E01** | Học viên gửi góp ý lộ trình -> GV phản hồi | Student + Teacher | /path/:id, /studio?tab=feedback | 1) SV gửi feedback về lộ trình. 2) GV mở tab Feedback trong Studio và trả lời. 3) SV xem lại phản hồi. | GV thấy góp ý mới, gửi trả lời thành công; SV xem được câu trả lời trong mục Ý kiến của tôi. |
| **TP-E02** | Form Trợ giúp /help gửi dữ liệu thật | All, Admin | /help, /admin/settings | 1) Vào /help gửi form liên hệ/báo lỗi. 2) Admin vào Cài đặt -> Báo cáo lỗi & Ý kiến. | Dữ liệu được ghi nhận vào bảng BugReports với nguồn help, Admin xem và cập nhật trạng thái phản hồi. |

---

### F. Simulator — Kiểm thử Toàn bộ 44 Thuật toán & CTDL

| STT | Nhóm | Key mô phỏng | Tên hiển thị | Input tùy biến biên (Mảng rỗng, 1 phần tử, số âm, số lớn, trùng) | Chế độ Demo (Guest) | Controls (Play/Pause/Step/Speed) |
|---|---|---|---|---|---|---|
| 01 | Sort | sort.bubble | Sắp xếp nổi bọt (Bubble Sort) | Test input âm, trùng, rỗng, 1 phần tử | Cho phép (Demo) | Next, Prev, Play, Pause, Speed slider |
| 02 | Sort | sort.selection | Sắp xếp chọn (Selection Sort) | Test input âm, trùng, rỗng, 1 phần tử | Yêu cầu đăng nhập | Next, Prev, Play, Pause, Speed slider |
| 03 | Sort | sort.insertion | Sắp xếp chèn (Insertion Sort) | Test input âm, trùng, rỗng, 1 phần tử | Yêu cầu đăng nhập | Next, Prev, Play, Pause, Speed slider |
| 04 | Sort | sort.merge | Sắp xếp trộn (Merge Sort) | Test mảng lẻ, mảng chẵn, số lớn | Yêu cầu đăng nhập | Next, Prev, Play, Pause, Speed slider |
| 05 | Sort | sort.quick | Sắp xếp nhanh (Quick Sort - Lomuto) | Test mảng đã sắp xếp, mảng nghịch đảo | Yêu cầu đăng nhập | Next, Prev, Play, Pause, Speed slider |
| 06 | Sort | sort.heap | Sắp xếp vun đống (Heap Sort) | Test mảng trùng lặp, mảng 1 phần tử | Yêu cầu đăng nhập | Next, Prev, Play, Pause, Speed slider |
| 07 | Search | search.linear | Tìm kiếm tuyến tính | Test tìm thấy, không tìm thấy, mảng rỗng | Yêu cầu đăng nhập | Step, Play, Pause, tìm target |
| 08 | Search | search.binary | Tìm kiếm nhị phân | Test target ở biên trái, phải, giữa, không có | Cho phép (Demo) | Step, Play, Pause, chia đôi khoảng |
| 09 | Linear | stack.push | Ngăn xếp — Push | Test push chuỗi, số lớn, đầy stack | Yêu cầu đăng nhập | Animation push vào đỉnh stack |
| 10 | Linear | stack.pop | Ngăn xếp — Pop | Test pop khi stack rỗng | Yêu cầu đăng nhập | Animation lấy phần tử đỉnh |
| 11 | Linear | stack.peek | Ngăn xếp — Peek | Test peek khi stack rỗng / có phần tử | Yêu cầu đăng nhập | Highlight phần tử đỉnh |
| 12 | Linear | queue.enqueue | Hàng đợi — Enqueue | Test enqueue nhiều phần tử | Yêu cầu đăng nhập | Animation thêm vào cuối hàng đợi |
| 13 | Linear | queue.dequeue | Hàng đợi — Dequeue | Test dequeue khi queue rỗng | Yêu cầu đăng nhập | Animation lấy phần tử đầu hàng |
| 14 | Linear | list.insert | Danh sách liên kết — Chèn | Test chèn đầu, chèn cuối, chèn giữa | Yêu cầu đăng nhập | Vẽ con trỏ next, dịch chuyển node |
| 15 | Linear | list.delete | Danh sách liên kết — Xóa | Test xóa node đầu, cuối, không tồn tại | Yêu cầu đăng nhập | Cập nhật con trỏ, giải phóng node |
| 16 | Linear | list.search | Danh sách liên kết — Tìm kiếm | Test tìm thấy / không tìm thấy | Yêu cầu đăng nhập | Duyệt tuần tự qua các con trỏ |
| 17 | Linear | list.traverse | Danh sách liên kết — Duyệt | Test danh sách rỗng, 1 node, nhiều node | Yêu cầu đăng nhập | Duyệt từ head đến tail |
| 18 | Tree | 	ree.bst-insert | BST — Chèn node | Test chèn số trùng, cây lệch trái/phải | Yêu cầu đăng nhập | Duyệt so sánh lớn/bé và tạo node |
| 19 | Tree | 	ree.bst-delete | BST — Xóa node | Test xóa node lá, node 1 con, node 2 con | Yêu cầu đăng nhập | Tìm node thế mạng và nối lại nhánh |
| 20 | Tree | 	ree.bst-search | BST — Tìm kiếm | Test tìm giá trị có / không trong cây | Yêu cầu đăng nhập | Duyệt theo nhánh cây nhị phân |
| 21 | Tree | 	ree.bst-preorder | BST — Duyệt Preorder (N-L-R) | Test thứ tự duyệt | Yêu cầu đăng nhập | Highlight từng node theo thứ tự |
| 22 | Tree | 	ree.bst-inorder | BST — Duyệt Inorder (L-N-R) | Test thứ tự duyệt tăng dần | Yêu cầu đăng nhập | Highlight từng node theo thứ tự |
| 23 | Tree | 	ree.bst-postorder | BST — Duyệt Postorder (L-R-N) | Test thứ tự duyệt | Yêu cầu đăng nhập | Highlight từng node theo thứ tự |
| 24 | Tree | 	ree.bst-levelorder | BST — Duyệt Level-order (BFS) | Test duyệt theo từng tầng | Yêu cầu đăng nhập | Hàng đợi phụ trợ hiển thị rõ |
| 25 | Tree | 	ree.avl-insert | Cây AVL — Chèn kèm xoay | Test 4 trường hợp xoay: LL, RR, LR, RL | Yêu cầu đăng nhập | Tự động cân bằng và animation xoay |
| 26 | Heap | heap.insert | Đống nhị phân — Chèn (Bubble up) | Test chèn giá trị lớn nhất / nhỏ nhất | Yêu cầu đăng nhập | Đổi chỗ với node cha lên đỉnh |
| 27 | Heap | heap.extract | Đống nhị phân — Trích xuất Max | Test extract khi heap rỗng / 1 node | Yêu cầu đăng nhập | Đưa node cuối lên gốc và Sift down |
| 28 | Heap | heap.heapify | Đống nhị phân — Heapify mảng | Test mảng lộn xộn | Yêu cầu đăng nhập | Xây dựng Max-Heap từ mảng ban đầu |
| 29 | Hash | hash.insert | Bảng băm — Chèn (Chaining) | Test đụng độ băm (hash collision) | Yêu cầu đăng nhập | Nối danh sách liên kết tại bucket |
| 30 | Hash | hash.search | Bảng băm — Tìm kiếm | Test tìm key có đụng độ / không có | Yêu cầu đăng nhập | Tính hash index và duyệt bucket |
| 31 | Hash | hash.delete | Bảng băm — Xóa | Test xóa key ở đầu/giữa/cuối bucket | Yêu cầu đăng nhập | Cắt tỉa node trong bucket |
| 32 | Graph | graph.bfs | Đồ thị — Duyệt BFS | Test đồ thị có chu trình, đồ thị rời rạc | Cho phép (Demo) | Hàng đợi, mảng visited, highlight đỉnh |
| 33 | Graph | graph.dfs | Đồ thị — Duyệt DFS | Test đồ thị có nhánh sâu, chu trình | Yêu cầu đăng nhập | Ngăn xếp đệ quy, backtrack |
| 34 | Graph | graph.dijkstra | Đồ thị — Tìm đường đi ngắn nhất | Test trọng số dương, đỉnh không tới được | Yêu cầu đăng nhập | Cập nhật bảng khoảng cách và trace |
| 35 | Structure | structure.array | Cấu trúc dữ liệu Mảng | Test truy xuất O(1), thêm/xóa O(n) | Yêu cầu đăng nhập | Hiển thị ô nhớ liên tục và chỉ số index |
| 36 | Structure | structure.linkedlist | Cấu trúc Danh sách liên kết | Test các node rời rạc liên kết bằng con trỏ | Yêu cầu đăng nhập | Node chứa data và pointer next |
| 37 | Structure | structure.stack | Cấu trúc Ngăn xếp (LIFO) | Test trực quan cơ chế vào sau ra trước | Yêu cầu đăng nhập | Đỉnh top, chiều sâu stack |
| 38 | Structure | structure.queue | Cấu trúc Hàng đợi (FIFO) | Test trực quan cơ chế vào trước ra trước | Yêu cầu đăng nhập | Con trỏ Front và Rear |
| 39 | Structure | structure.binarytree | Cấu trúc Cây nhị phân | Test cấu trúc phân nhánh trái/phải | Yêu cầu đăng nhập | Node gốc, con trái, con phải |
| 40 | Structure | structure.bst | Cấu trúc BST | Test tính chất cây tìm kiếm nhị phân | Yêu cầu đăng nhập | Cây thỏa mãn Left < Root < Right |
| 41 | Structure | structure.avl | Cấu trúc Cây AVL | Test hệ số cân bằng Height Balance Factor | Yêu cầu đăng nhập | Hiển thị chỉ số cân bằng từng node |
| 42 | Structure | structure.heap | Cấu trúc Binary Heap | Test cấu trúc mảng biểu diễn cây nhị phân | Yêu cầu đăng nhập | Cây nhị phân gần hoàn chỉnh |
| 43 | Structure | structure.hashtable | Cấu trúc Bảng băm | Test bảng bucket và hàm băm | Yêu cầu đăng nhập | Danh sách bucket và chuỗi liên kết |
| 44 | Structure | structure.graph | Cấu trúc Đồ thị | Test ma trận kề / danh sách kề | Yêu cầu đăng nhập | Đỉnh, cạnh, có hướng và vô hướng |

---

### G. Học liệu khác (Code Runner, Exercise, FinalTest, CheatSheet)

| Mã test | Kịch bản | Role | Màn / URL | Các bước thực hiện | Kết quả kỳ vọng |
|---|---|---|---|---|---|
| **TP-G01** | Code Runner đa ngôn ngữ | Student | /code/:key | 1) Chạy mã đúng (C++, Java, Python, JS). 2) Thử cú pháp lỗi. 3) Thử vòng lặp vô hạn. | In kết quả chính xác, bắt lỗi syntax chi tiết, dừng an toàn khi gặp timeout/vượt sandbox limit. |
| **TP-G02** | Bài tập Trắc nghiệm (Bậc 1) | Student | /exercise/:id | 1) Trả lời đúng -> nhận XP. 2) Trả lời sai -> trừ 1 tim. 3) Xem giải thích. | Cộng XP chính xác, trừ đúng 1 tim khi sai, hiển thị lời giải thích rõ ràng. |
| **TP-G03** | Final Test (Kiểm tra cuối khóa) | Student | /path/:topicId/final-test | 1) Truy cập route kiểm tra cuối lộ trình. 2) Làm toàn bộ câu hỏi và nộp bài. | Tính điểm tổng kết, cấp chứng chỉ/badge khi vượt qua điểm sàn. |
| **TP-G04** | CheatSheet & Tìm kiếm tiếng Việt | Student | /cheatsheet, /path | 1) Tìm kiếm không dấu (quy hoach) và có dấu (quy hoạch). 2) Xuất PDF CheatSheet (Free vs Premium). | Tìm kiếm hỗ trợ normalize tiếng Việt chuẩn; tính năng xuất PDF yêu cầu gói Premium. |

---

### H. Gamification & Thanh toán Premium

| Mã test | Kịch bản | Role | Màn / URL | Các bước thực hiện | Kết quả kỳ vọng |
|---|---|---|---|---|---|
| **TP-H01** | Trừ tim & Mua hồi tim tại Shop | Student | /exercise/:id, /shop | 1) Trả lời sai để tim về 0. 2) Thử làm bài tiếp. 3) Vào /shop mua hồi tim bằng Gems. | Bị chặn làm bài khi 0 tim (hiện popup), mua hồi tim thành công trừ đúng Gems và tim hồi đầy ngay. |
| **TP-H02** | Daily Quests & Chống double-claim | Student | /quests | 1) Hoàn thành nhiệm vụ ngày. 2) Bấm Nhận thưởng. 3) Bấm liên tục 2 lần nút nhận. | Nhận đúng XP/Gems, nút chuyển trạng thái Claimed, chặn hoàn toàn double-claim. |
| **TP-H03** | Bảng xếp hạng & Đồng bộ XP | Student | /leaderboard | 1) Hoàn thành bài học nhận XP. 2) Mở bảng xếp hạng. | Điểm số và thứ hạng của học viên được cập nhật chính xác trên bảng xếp hạng. |
| **TP-H04** | Nâng cấp Premium qua VietQR | Student | /premium, /account/subscription | 1) Chọn gói tháng/năm. 2) Kiểm tra mã VietQR động. 3) Xác nhận thanh toán giả lập. | Hiển thị VietQR kèm cú pháp chuyển khoản, tài khoản nâng cấp Premium, badge Premium sáng trên Header. |

---

### I. Auth, Phân quyền & An ninh URL

| Mã test | Kịch bản | Role | Màn / URL | Các bước thực hiện | Kết quả kỳ vọng |
|---|---|---|---|---|---|
| **TP-I01** | Đăng ký tài khoản & Quên mật khẩu | Guest | /register, /forgot-password, /reset-password | 1) Đăng ký SV mới. 2) Gửi yêu cầu quên mật khẩu. 3) Đặt mật khẩu mới qua token. | Luồng đăng ký và khôi phục mật khẩu hoạt động trơn tru, đăng nhập bằng mật khẩu mới thành công. |
| **TP-I02** | Đăng nhập sai & Giới hạn bảo vệ | Guest | /login | 1) Nhập sai mật khẩu liên tiếp nhiều lần. | Báo lỗi đăng nhập rõ ràng, hệ thống ổn định, không bị crash form. |
| **TP-I03** | Truy cập trực tiếp URL phân quyền (FE + BE) | Guest, Student | /studio, /admin/*, /classes/:id (lớp khác) | 1) Gõ tay URL các trang quản trị hoặc lớp học không thuộc về mình. | FE tự động redirect về /profile hoặc /login; Backend trả về mã lỗi 401 Unauthorized / 403 Forbidden. |
| **TP-I04** | Logout và bấm Back trên trình duyệt | Student, Teacher | /profile, /studio | 1) Đăng xuất tài khoản. 2) Bấm nút Back trên trình duyệt. | Không quay lại trang nội bộ mang dữ liệu cũ, session được dọn dẹp sạch sẽ. |

---

### J. Kiểm tra UX & Responsive Sweep

| Mã test | Kịch bản | Role | Màn / URL | Các bước thực hiện | Kết quả kỳ vọng |
|---|---|---|---|---|---|
| **TP-J01** | Responsive 375px (Mobile) vs 1440px (Desktop) | All | Toàn bộ 32 màn hình | 1) Resize viewport về 375px. 2) Resize viewport về 1440px. 3) Kiểm tra vỡ giao diện, tràn chữ, cuộn ngang. | Giao diện hiển thị mượt mà, không bị vỡ bố cục, không tràn lề ngang ngoài ý muốn, text đọc rõ ràng. |
| **TP-J02** | Modal & Drawer thoát an toàn | All | Các màn có Modal / Drawer | 1) Nhập form dở trong Modal/Drawer. 2) Bấm ESC hoặc click ra backdrop. | Có hộp thoại xác nhận hủy nếu đang sửa dở, không làm mất dữ liệu ngoài ý muốn. |
| **TP-J03** | Rà soát lỗi chữ, chính tả & nút trùng | All | Toàn bộ 32 màn hình | 1) Quét toàn bộ nhãn, nút bấm, thông báo, empty state, loading skeleton. | Không có 2 nút làm 1 việc, không sai chính tả, không lẫn lộn tiếng Anh - tiếng Việt khó hiểu. |
