# THUẬT NGỮ (GLOSSARY)

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**

| | |
|---|---|
| Loại tài liệu | Bảng thuật ngữ |
| Phiên bản | 1.0 |
| Ngày cập nhật | 12/08/2026 |
| Trạng thái | Dự thảo |
| Người soạn | Trần Viết Tâm Phúc |
| Người duyệt | Phạm Ngọc Ái Liên |
| Nguồn yêu cầu | PRODUCTION_PROMPT.md §1.8 (thuật ngữ chuẩn), §17.3.7-17.3.8 (khuôn) |

## Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 12/08/2026 | Trần Viết Tâm Phúc | Sinh mới từ PRODUCTION_PROMPT.md v2.5 |

---

# 1. NHÓM NGHIỆP VỤ

| Thuật ngữ | Định nghĩa |
|---|---|
| Mô phỏng (Simulation) | Quá trình trình diễn từng bước thực thi của một giải thuật trên một cấu trúc dữ liệu với dữ liệu đầu vào cụ thể |
| Bước (Step) | Một trạng thái tĩnh của toàn bộ vùng trực quan cùng giải thích và dòng mã giả tương ứng |
| CTDL / GT | Cấu trúc dữ liệu / Giải thuật (thuật toán) |
| Generator | Hàm thuần túy sinh ra chuỗi `Step[]` từ dữ liệu đầu vào |
| Renderer | Mô-đun vẽ một `Structure` lên Canvas/DOM |
| Bài tập dự đoán bước | Dạng bài tập yêu cầu người học dự đoán trạng thái sau một số bước nhất định |
| Node (nút lộ trình) | Đơn vị học tập trong Learning Path (một bài học + Ladder 3 bậc); KHÔNG nhầm với "nút" của danh sách liên kết hay cây |
| Bậc (Stage) | Một trong 3 bước của Practice Ladder: Bậc 1 Quiz → Bậc 2 Interactive Lab → Bậc 3 Code Challenge |
| Practice Ladder | Chuỗi luyện tập tuần tự 3 bậc của một node; pass bậc trước mới mở bậc sau |
| Session học 30 phút | Khoảng thời gian 30 phút kể từ lượt "vào node" đầu tiên có trừ tim; trong session, vào lại cùng node / retry bậc miễn phí |
| Tim (Hearts) | Quỹ năng lượng giới hạn (Free 10 / Premium 30), trừ 1 tim mỗi lượt "vào node", hồi theo thời gian (FR-10.1) |
| Pass node | Hoàn thành tuần tự cả 3 bậc Ladder theo ngưỡng: Quiz ≥ 60%, Lab đạt, Code ≥ 70% test |
| Final test | Bài kiểm tra tổng hợp cuối Learning Path, trộn quiz + dự đoán bước từ các node của path |
| NodeSession | Bản ghi phiên học 30 phút của 1 người dùng tại 1 node (điểm dừng, bước đang dở) |
| Learning Path | Lộ trình học tập gồm chuỗi node, mở khóa tuần tự |
| CheatSheet | Bảng tóm tắt độ phức tạp (Big-O) của các GT/CTDL, tương tác được, mở thẳng mô phỏng |
| Gems | Điểm thưởng kiếm được qua hoạt động học tập, dùng mua vật phẩm trong Shop |
| Streak | Số ngày liên tục người học có hoạt động học tập thực tế |
| Daily Quest | Nhiệm vụ hằng ngày (5 quest: 2 dễ + 2 trung bình + 1 khó), reset 00:00 UTC+7 |
| Premium | Gói nâng cấp (1/3/12 tháng) với quyền lợi mở rộng; checkout MÔ PHỎNG (không thanh toán thật) |
| Benchmark Lab | Công cụ chạy thật 2+ giải thuật ở nhiều kích thước dữ liệu, so sánh số liệu đo với đường lý thuyết |
| Tiến độ | Tổng hợp trạng thái đã xem bài, đã làm bài tập và điểm của một người học |
| Người học / Người dạy | Sinh viên (vai trò Student) / Giảng viên (vai trò Teacher) |
| Interactive Lab | Bậc 2 của Ladder: người học thao tác kéo-thả trên canvas, server chấm trạng thái cuối + giới hạn số bước |

# 2. NHÓM KỸ THUẬT

| Thuật ngữ | Định nghĩa |
|---|---|
| EDV (Execution-Driven Visualization) | Kiến trúc trực quan hóa: mọi giải thuật là mã thật chạy qua StepExecutor, hoạt ảnh = phát lại trace thực thi |
| StepExecutor | Bộ thực thi có gắn thiết bị đo, chạy code mẫu và ghi TraceEvent[] |
| TraceEvent | Bản ghi một câu lệnh quan trọng khi thực thi: dòng code, snapshot biến, phần tử highlight, giải thích |
| SPA | Single Page Application — ứng dụng một trang web tải một lần, thay đổi nội dung không cần tải lại |
| REST API | Giao thức giao tiếp giữa frontend và backend theo chuẩn HTTP |
| JWT | JSON Web Token — chuỗi mã hóa dùng để xác thực người dùng giữa frontend và backend |
| Access token | Token ngắn hạn (60 phút) gửi kèm mỗi request, lưu trong bộ nhớ trình duyệt |
| Refresh token | Token dài hạn (7 ngày) lưu trong cookie an toàn, dùng để xin lại access token khi hết hạn; có cơ chế rotate-invalidate |
| Rotate-invalidate | Cơ chế thu hồi token cũ ngay khi cấp token mới; phát hiện replay → thu hồi cả chuỗi phiên |
| EF Core | Thư viện C# để truy vấn và lưu dữ liệu vào cơ sở dữ liệu (ORM) |
| DbContext / DbSet | Điểm vào truy vấn của EF Core; Service truy cập trực tiếp qua DbSet (không Repository pattern) |
| Migration | Cơ chế của EF Core để thay đổi cấu trúc bảng theo phiên bản |
| Canvas | Vùng vẽ đồ họa của trình duyệt, dùng để vẽ các mô phỏng |
| Web Worker | Luồng chạy nền trong trình duyệt, dùng làm sandbox chạy code người học (không có Judge0/container server) |
| Sandbox | Môi trường chạy code cách ly, giới hạn 10 giây / 64MB / 200 dòng |
| Registry | Cơ chế đăng ký mô phỏng: thêm CTDL/GT mới không cần sửa mã lõi |
| Sanitize | Làm sạch nội dung HTML để loại bỏ mã độc trước khi lưu/hiển thị |
| XSS / CSRF / SQLi / IDOR | Các kiểu tấn công bảo mật: chèn mã độc, giả mạo yêu cầu, chèn SQL, truy cập tài nguyên người khác |
| Rate limit | Giới hạn số lượng yêu cầu trong một khoảng thời gian |
| Serilog | Thư viện ghi nhật ký cấu trúc phía máy chủ |
| Result\<T\> | Mẫu trả về của Service: Success/Fail + ErrorCode + message tiếng Việt |
| Golden data | Bộ dữ liệu chuẩn có kết quả mong đợi tính trước, dùng để kiểm tra thuật toán |
| Testcontainers | Thư viện chạy container (VD: SQL Server) trong kiểm thử tích hợp |
| TestSeed | Bộ dữ liệu khởi tạo riêng cho môi trường kiểm thử |
| CI/CD | Continuous Integration / Continuous Deployment — tự động hóa build, kiểm thử, triển khai |

# 3. NHÓM THUẬT NGỮ DSA

| Thuật ngữ | Định nghĩa |
|---|---|
| Pivot | Phần tử chốt được chọn trong Quick Sort để chia mảng thành hai phần |
| Heapify | Thao tác sắp xếp lại cây nhị phân để thỏa mãn tính chất heap |
| Balance factor | Độ chênh lệch chiều cao giữa cây con trái và phải của một nút trong cây AVL |
| Bubble up / Sift down | Di chuyển phần tử lên/xuống trong heap để giữ tính chất heap |
| BFS / DFS | Hai cách duyệt đồ thị: theo chiều rộng (dùng hàng đợi) và theo chiều sâu (dùng ngăn xếp) |
| Relax (cạnh) | Cập nhật khoảng cách ngắn hơn khi duyệt cạnh trong Dijkstra |
| Bucket | Vị trí trong bảng băm; các phần tử có cùng giá trị băm xếp trong cùng bucket |
| Hàm băm | Hàm ánh xạ khóa sang vị trí trong bảng băm (VD: `h(k) = k mod m`) |
| Inorder / Preorder / Postorder / Level-order | Bốn cách duyệt cây: trái-giữa-phải / gốc-trái-phải / trái-phải-gốc / theo tầng |
| Call stack | Ngăn xếp lời gọi hàm đệ quy đang thực thi |
| Breakpoint | Điểm dừng có điều kiện trong mô phỏng (theo dòng mã giả hoặc điều kiện trạng thái) |
| Big-O | Ký hiệu mô tả độ phức tạp thời gian/không gian của giải thuật |
| Worst case / Best case / Average case | Các trường hợp dữ liệu khiến giải thuật chậm nhất / nhanh nhất / trung bình |
| LIFO / FIFO | Vào sau ra trước (stack) / Vào trước ra trước (queue) |
| LinkedList | Danh sách liên kết: các nút nối nhau bằng con trỏ |
| Hash collision | Xung đột băm: nhiều khóa có cùng giá trị băm |
| Adjacency list | Danh sách kề: cách biểu diễn đồ thị bằng danh sách các đỉnh kề |
| d[] / parent[] | Mảng khoảng cách / mảng cha trong Dijkstra |
| Trace | Nhật ký thực thi (TraceEvent[]) ghi trong lúc code chạy, phát lại thành hoạt ảnh |
| Test ẩn (hidden tests) | Bộ test không hiển thị cho sinh viên, dùng để chấm bài tập lập trình (đóng gói trong bundle client — mức cam kết "chống lười làm", KHÔNG cam kết chống trích xuất) |
