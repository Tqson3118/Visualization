<!-- Khu vực logo trường: chèn logo Trường Cao Đẳng Thực Hành FPT tại đây -->

TRƯỜNG CAO ĐẲNG THỰC HÀNH FPT

---

**BÁO CÁO ĐỒ ÁN TỐT NGHIỆP**

**HỆ THỐNG HỖ TRỢ HỌC TẬP VÀ TRỰC QUAN HÓA CẤU TRÚC DỮ LIỆU VÀ GIẢI THUẬT (DSA-VISUAL)**

Ngành: Ứng dụng phần mềm

Giảng viên hướng dẫn: Phạm Ngọc Ái Liên

Lớp: SD21361

**Danh sách thành viên thực hiện**

| Mã SV | Họ và tên | Vai trò | Nhiệm vụ chính |
|---|---|---|---|
| TD01287 | Mai Tiểu Bảo | Trưởng nhóm | Backend ASP.NET Core + CSDL (task khó ưu tiên) |
| TD01282 | Thái Quang Sơn | Thành viên | Frontend Vue 3 + giao diện (task khó ưu tiên) |
| TD01131 | Huỳnh Lê Minh Thư | Thành viên | Simulation Engine (code) + Kiểm thử |
| TD01261 | Trần Viết Tâm Phúc | Thành viên | Code hỗ trợ (đơn giản) + Tài liệu + triển khai |

TP.HCM, ngày 12 tháng 8 năm 2026

Ngày bảo vệ: …………………………………………

---

TRƯỜNG CAO ĐẲNG THỰC HÀNH FPT

```{=openxml}
<w:p><w:r><w:br w:type="page"/></w:r></w:p>
```

# LỜI MỞ ĐẦU

Cấu trúc dữ liệu và giải thuật (DSA) là nền tảng của ngành công nghệ thông tin, nhưng cũng là môn học sinh viên gặp nhiều khó khăn nhất trong giai đoạn đầu. Các khái niệm như danh sách liên kết, cây, bảng băm hay các giải thuật sắp xếp, tìm kiếm rất trừu tượng: sinh viên khó hình dung dữ liệu được tổ chức thế nào trong bộ nhớ và giải thuật thao tác ra sao qua từng bước. Sách và giáo trình chỉ cung cấp hình tĩnh và mã nguồn, khiến việc học chủ yếu diễn ra thụ động, thiếu phản hồi trực quan và thiếu luyện tập chủ động.

Từ nhu cầu đó, nhóm gồm 4 sinh viên lớp SD21361 đã xây dựng Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual) — ứng dụng web giúp sinh viên hiểu sâu DSA qua mô phỏng hoạt ảnh từng bước, bài tập tự chấm và theo dõi tiến độ cá nhân. Điểm cốt lõi là cơ chế EDV (Execution-Driven Visualization): mọi giải thuật là mã thật chạy qua StepExecutor, hoạt ảnh là phát lại bản ghi (trace) thật của quá trình thực thi, không hardcode.

Đây là lần thứ hai nhóm viết báo cáo đồ án, và nhóm đã rút ra bài học rõ ràng từ bản báo cáo trước bị hội đồng góp ý: hoạt ảnh hardcode nên không chạy được cho mọi đoạn code; màn hình gộp quá nhiều chức năng khiến người dùng rối; phạm vi dự án trôi dạt sang tính năng ngoài tầm đồ án; báo cáo có số liệu chưa kiểm chứng và danh sách thành viên không chính xác. Báo cáo này khắc phục cả bốn vấn đề: mọi nội dung được truy ngược về tài liệu thiết kế (SRS, SDD, API, kiểm thử), số liệu chưa đo được ghi rõ là chờ hoàn tất kiểm thử, thông tin thành viên xác nhận đúng 4 người thật sự tham gia.

Báo cáo gồm 7 phần. Phần 1 giới thiệu đề tài và ban dự án. Phần 2 trình bày khảo sát các hệ thống tương tự và kế hoạch dự án. Phần 3 mô tả phân tích yêu cầu hệ thống (mô hình triển khai, sơ đồ use case, đặc tả yêu cầu). Phần 4 trình bày thiết kế công nghệ, giao diện, dữ liệu và phần mềm. Phần 5 mô tả quá trình thực hiện. Phần 6 báo cáo kết quả kiểm thử. Phần 7 mô tả đóng gói và triển khai. Cuối báo cáo là Kết luận và hướng phát triển, Tài liệu tham khảo và 4 phụ lục hỗ trợ (hướng dẫn cài đặt môi trường, phím tắt và thuật ngữ, thư viện bên thứ ba, danh mục mô phỏng).

(nguồn: PRODUCTION_PROMPT §0.5; SRS §1.2)

# PHẦN 1: GIỚI THIỆU ĐỀ TÀI

## 1.1 Giới thiệu dự án

Sinh viên ngành công nghệ thông tin khi học môn Cấu trúc dữ liệu và giải thuật gặp 3 khó khăn điển hình:

1. **Trừu tượng**: khó hình dung cách dữ liệu được tổ chức trong bộ nhớ (liên kết, con trỏ, chỉ số) và cách giải thuật thao tác trên dữ liệu.
2. **Thiếu phản hồi trực quan**: sách và giáo trình chỉ có hình tĩnh và mã; sinh viên không thấy chuyển động từng bước, không thấy lý do vì sao giải thuật hoạt động như vậy.
3. **Thiếu luyện tập chủ động**: sinh viên không được thực hành dự đoán kết quả từng bước — kỹ năng quan trọng nhất để hiểu sâu giải thuật.

DSA-Visual giải quyết bằng: (a) mô phỏng từng bước mọi thao tác trên CTDL/GT theo cơ chế EDV (mã thật chạy, trace phát lại); (b) đồng bộ trực quan – mã giả – giải thích trong cùng một màn hình; (c) Practice Ladder 3 bậc với bài tập dự đoán chấm tự động; (d) theo dõi tiến độ cá nhân và báo cáo cho giảng viên.

Mục tiêu dự án được đo lường bằng 8 KPI G1-G8 (Bảng 1.1).

**Bảng 1.1: Mục tiêu dự án (KPI G1-G8)**

| Mã | Mục tiêu | Giá trị mục tiêu |
|---|---|---|
| G1 | Phủ nội dung học tập | ≥ 10 cấu trúc dữ liệu có mô phỏng |
| G2 | Phủ giải thuật | ≥ 14 giải thuật có mô phỏng (thiết kế: 15) |
| G3 | Mức độ sử dụng | ≥ 80% sinh viên trong lớp đăng ký và truy cập ≥ 1 lần/tuần |
| G4 | Hiệu quả học tập | Điểm trung bình kiểm tra chương của lớp ≥ 7.0/10 |
| G5 | Sự hài lòng | Điểm khảo sát UX (thang 5) ≥ 4.0/5 |
| G6 | Độ ổn định | Uptime giai đoạn thí điểm 4 tuần ≥ 99.5% |
| G7 | Hiệu năng | Thời gian phản hồi API p95 ≤ 800ms |
| G8 | Độ mượt mô phỏng | FPS khi mô phỏng ≥ 55 fps |

Ghi chú: G3 và G5 đo trên người dùng có đủ Tim/Premium theo thiết kế; G4 đo ngoài hệ thống (điểm kiểm tra chương do giảng viên chấm).

(nguồn: SRS §2.1, §2.2)

## 1.2 Ban dự án

Dự án do 4 thành viên lớp SD21361 thực hiện (Bảng 1.2). Đây là danh sách đã được xác nhận, không có thành viên nào ngoài danh sách này.

**Bảng 1.2: Danh sách thành viên và phân công**

| Mã SV | Họ và tên | Vai trò | Nhiệm vụ chính |
|---|---|---|---|
| TD01287 | Mai Tiểu Bảo | Trưởng nhóm | Backend ASP.NET Core + CSDL (task khó ưu tiên) |
| TD01282 | Thái Quang Sơn | Thành viên | Frontend Vue 3 + giao diện (task khó ưu tiên) |
| TD01131 | Huỳnh Lê Minh Thư | Thành viên | Simulation Engine (code) + Kiểm thử |
| TD01261 | Trần Viết Tâm Phúc | Thành viên | Code hỗ trợ (đơn giản) + Tài liệu + triển khai |

Vai trò chuyên môn của từng thành viên:

- Mai Tiểu Bảo (Trưởng nhóm): chịu trách nhiệm kiến trúc backend ASP.NET Core và cơ sở dữ liệu, nhận các task khó ưu tiên, điều phối tiến độ chung của nhóm.
- Thái Quang Sơn: phụ trách frontend Vue 3 và toàn bộ giao diện người dùng, xây dựng các màn hình học tập, mô phỏng, luyện tập theo thiết kế, nhận các task khó ưu tiên.
- Huỳnh Lê Minh Thư: phụ trách Simulation Engine (viết code generator, renderer cho cơ chế EDV) và kiểm thử hệ thống.
- Trần Viết Tâm Phúc: hỗ trợ code các phần đơn giản, phụ trách viết tài liệu và triển khai hệ thống.

(nguồn: SRS §2.4; SDD §1.5)

# PHẦN 2: KHẢO SÁT – SURVEY

## 2.1 Yêu cầu của khách hàng

Trước khi đặc tả yêu cầu, nhóm khảo sát 3 hệ thống trực quan hóa DSA phổ biến hiện nay:

- **VisuAlgo**: bộ minh họa giải thuật của Đại học Quốc gia Singapore (NUS), mô phỏng nhiều CTDL/GT, có bài tập quiz.
- **USFCA DS Visualizations**: bộ mô phỏng của Đại học San Francisco (Mỹ), mã nguồn mở, giao diện đơn giản.
- **Algorithm-Visualizer**: nền tảng cộng đồng, cho phép xem trực quan hóa từ mã nguồn do người dùng đóng góp.

Kết quả so sánh theo 9 tiêu chí được trình bày ở Bảng 2.1.

**Bảng 2.1: So sánh hệ thống tương tự**

| Tiêu chí | VisuAlgo | USFCA | Algorithm-Visualizer | DSA-Visual (đề xuất) |
|---|---|---|---|---|
| Ngôn ngữ giao diện | Tiếng Anh | Tiếng Anh | Tiếng Anh | Tiếng Việt |
| Mô phỏng từng bước | Có | Có | Có | Có |
| Giải thích bằng lời | Có (tùy chọn) | Hạn chế | Hạn chế | Bắt buộc mỗi bước |
| Mã giả đồng bộ | Có | Không | Có | Có (mã thật EDV) |
| Bài tập dự đoán bước | Có (quiz) | Không | Không | Có (Interactive Lab) |
| Theo dõi tiến độ cá nhân | Không | Không | Không | Có |
| Giảng viên biên soạn nội dung | Không | Không | Không | Có |
| So sánh đo thật với lý thuyết | Không | Không | Không | Có (Benchmark Lab) |
| Mã nguồn mở | Một phần | Có | Có | Nội bộ |

Kết luận khảo sát: 3 hệ thống trên đều mạnh về mô phỏng từng bước nhưng chưa đáp ứng đủ nhu cầu học tập tại trường. Từ đó, nhóm xác định 6 yêu cầu cốt lõi cho DSA-Visual: (1) đồng bộ 3 vùng trực quan – mã giả – giải thích từ trace thật (EDV); (2) hệ thống bài tập tự chấm theo Practice Ladder 3 bậc; (3) theo dõi tiến độ cá nhân và báo cáo cho giảng viên; (4) giảng viên tự biên soạn nội dung; (5) Benchmark so sánh số liệu đo thật với lý thuyết; (6) giao diện tiếng Việt.

Yêu cầu chức năng (FR) của hệ thống gồm 10 nhóm chức năng (Bảng 2.2). Trong quá trình duyệt, 12 FR đã được cắt để giữ phạm vi đúng tầm đồ án (không cổng thanh toán thật, không realtime, không online judge tự do).

**Bảng 2.2: Tóm tắt yêu cầu chức năng theo nhóm**

| Nhóm chức năng | Chức năng chính | FR tiêu biểu |
|---|---|---|
| Tài khoản | Đăng ký, đăng nhập, đổi/khôi phục mật khẩu, phê duyệt giảng viên | FR-1.1 đến FR-1.9 |
| Học tập | Quản lý chủ đề, bài học, Learning Path, ghi chú cá nhân | FR-2.1 đến FR-2.11 |
| Mô phỏng | Danh mục mô phỏng, hiển thị 3 vùng đồng bộ, điều khiển, Benchmark Lab | FR-3.1 đến FR-3.20b |
| Luyện tập | Practice Ladder 3 bậc, kiểm tra cuối lộ trình | FR-4.1 đến FR-4.12 |
| Tiến độ và báo cáo | Ghi nhận tiến độ, dashboard cá nhân, báo cáo giảng viên | FR-5.1 đến FR-5.5 |
| Quản trị | Quản lý người dùng, thống kê, cấu hình hệ thống | FR-5.4, FR-6.2 |
| Trang phụ trợ | Trang chủ, 3 demo công khai, FAQ, đánh giá nội dung | FR-7.1 đến FR-7.6 |
| Lớp học phần | Tạo lớp, quản lý sinh viên, gán nội dung và hạn nộp, báo cáo theo lớp | FR-8.1 đến FR-8.4 |
| Code Runner | Soạn mã nhúng, chạy mã kèm trực quan hóa, sandbox an toàn | FR-9.1 đến FR-9.6 |
| Gamification và Premium | Tim, Gems Shop, Daily Quest, Streak, XP/Level, Leaderboard, Premium | FR-10.1 đến FR-10.7 |

Yêu cầu phi chức năng (NFR) tóm tắt theo 8 nhóm:

- **Hiệu năng (NFR-1 đến NFR-7)**: API phản hồi p95 ≤ 800ms; sinh chuỗi bước mô phỏng mảng 100 phần tử ≤ 500ms; ≥ 55 FPS khi điều hướng bước; tải trang đầu ≤ 3s; ≥ 200 người dùng đồng thời.
- **Bảo mật (NFR-8 đến NFR-15)**: mật khẩu băm mạnh (bcrypt cost 12); JWT access 60 phút + refresh token xoay vòng; phân quyền kiểm tra server-side; rate limit; HTTPS bắt buộc; nhật ký bảo mật.
- **Khả năng mở rộng (NFR-16 đến NFR-19)**: thêm mô phỏng mới chỉ thêm generator và renderer; backend 2 lớp Controller → Service (KHÔNG dùng Repository theo NFR-17); API versioning; cấu hình hóa hằng số nghiệp vụ.
- **Khả năng sử dụng (NFR-20 đến NFR-24)**: chạy mô phỏng đầu tiên ≤ 2 phút không cần hướng dẫn; giao diện tiếng Việt; thông báo lỗi rõ ràng; trợ năng WCAG 2.1 AA; phím tắt cho mô phỏng.
- **Tương thích (NFR-25 đến NFR-27)**: Chrome, Edge, Firefox 2 phiên bản mới nhất; độ phân giải ≥ 1024×768 không vỡ layout; desktop/laptop là thiết bị chính.
- **Độ tin cậy (NFR-28 đến NFR-30)**: uptime ≥ 99.5% giai đoạn thí điểm; backup hàng ngày giữ 14 bản; mọi exception đều có log đầy đủ.
- **Bảo trì (NFR-31 đến NFR-34)**: tuân thủ chuẩn code; hàm ≤ 40 dòng, class ≤ 400 dòng; public API có tài liệu; kiểm thử tự động backend ≥ 60%, generator ≥ 90%.
- **Tuân thủ (NFR-35, NFR-36)**: dữ liệu cá nhân theo Nghị định 13/2023/NĐ-CP; chỉ dùng thư viện mã nguồn mở có liệt kê license.

(nguồn: SRS §2.6 — khảo sát; SRS §3.1 — FR; SRS §4.1-§4.8 — NFR; PRODUCTION_PROMPT §1.9)

## 2.2 Kế hoạch dự án

Dự án chạy theo quy trình phát triển linh hoạt với 10 sprint, mỗi sprint khoảng 1 tuần, tổng thời gian 13 tuần (khởi động 12/05/2026, kết thúc phát triển khoảng 11/08/2026). Quy trình trải qua 4 giai đoạn nối tiếp:

1. **Khảo sát và phân tích yêu cầu**: khảo sát hệ thống tương tự, đặc tả FR/NFR, lập kế hoạch sprint.
2. **Thiết kế hệ thống**: thiết kế kiến trúc backend, cơ sở dữ liệu, giao diện và Simulation Engine.
3. **Phát triển**: lập trình theo từng sprint, ưu tiên các chức năng mức Cao trước.
4. **Kiểm thử và triển khai**: kiểm thử toàn diện, hoàn tất tài liệu, triển khai môi trường staging.

Nội dung 10 sprint được trình bày ở Bảng 2.3.

**Bảng 2.3: Kế hoạch 10 sprint**

| Số sprint | Thời gian | Nội dung chính | Kết quả chính |
|---|---|---|---|
| S1 | 12/05→18/05 | Khởi động dự án | Repo, ứng dụng hello-world, SRS phiên bản đầu |
| S2 | 19/05→25/05 | Lõi backend | API xác thực, quản lý chủ đề/bài học, phân quyền |
| S3 | 26/05→01/06 | Engine EDV cơ bản | StepExecutor, registry, 3 giải thuật đầu, renderer mảng |
| S4 | 02/06→08/06 | Giao diện học tập cốt lõi | Màn học tập, chi tiết bài học, mô phỏng, Learning Path |
| S5 | 09/06→15/06 | Mở rộng engine | Đủ 15 giải thuật, renderer cây/đồ thị/heap/bảng băm |
| S6 | 16/06→22/06 | Practice Ladder và Code Runner phần 1 | Bậc 1 Quiz, Bậc 2 Interactive Lab, Monaco và sandbox Web Worker |
| S7 | 23/06→29/06 | Code Runner phần 2 và chấm điểm | Bậc 3 Code Challenge với test ẩn, tổng hợp tiến độ |
| S8 | 30/06→06/07 | Gamification lõi | Tim, Streak, XP/Level, Daily Quest, Leaderboard |
| S9 | 07/07→13/07 | Premium, Lớp học phần, Benchmark | Gems Shop, Premium mô phỏng, lớp học phần, Benchmark Lab |
| S10 | 14/07→20/07 | Hoàn thiện | Kiểm thử toàn diện, tối ưu hiệu năng, bảo mật, tài liệu, deploy staging |

Sau 10 sprint, giai đoạn hoàn thiện từ 21/07 đến 11/08/2026 gồm: kiểm thử toàn diện (chức năng, hiệu năng, bảo mật), hoàn tất 12 file tài liệu bàn giao và triển khai môi trường staging. Giai đoạn báo cáo từ 12/08 đến 31/08/2026: viết báo cáo Word, làm slide và chuẩn bị bảo vệ; ngày bảo vệ cụ thể do trường sắp xếp (để trống trên bìa).

Phân công theo giai đoạn được tóm tắt ở Bảng 2.4.

**Bảng 2.4: Phân công theo giai đoạn**

| Giai đoạn | Nội dung chính | Thành viên chính |
|---|---|---|
| Khảo sát và phân tích yêu cầu | Khảo sát hệ thống tương tự, đặc tả FR/NFR, lập kế hoạch | Cả nhóm (Bảo điều phối) |
| Thiết kế hệ thống | Kiến trúc backend, cơ sở dữ liệu, giao diện, Simulation Engine | Bảo, Sơn, Thư |
| Phát triển | Lập trình theo 10 sprint, ưu tiên chức năng mức Cao | Bảo, Sơn, Thư, Phúc |
| Kiểm thử và triển khai | Kiểm thử toàn diện, tài liệu hóa, deploy staging | Thư, Phúc |

(nguồn: PRODUCTION_PROMPT §20.1 — nội dung sprint; mốc thời gian: BAO_CAO_SPEC §4)
