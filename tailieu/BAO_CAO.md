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

# PHẦN 1: GIỚI THIỆU ĐỀ TÀI

## 1.1 Giới thiệu dự án

Sinh viên ngành công nghệ thông tin khi học môn Cấu trúc dữ liệu và giải thuật gặp 3 khó khăn điển hình:

1. **Trừu tượng**: khó hình dung cách dữ liệu được tổ chức trong bộ nhớ (liên kết, con trỏ, chỉ số) và cách giải thuật thao tác trên dữ liệu.
2. **Thiếu phản hồi trực quan**: sách và giáo trình chỉ có hình tĩnh và mã; sinh viên không thấy chuyển động từng bước, không thấy lý do vì sao giải thuật hoạt động như vậy.
3. **Thiếu luyện tập chủ động**: sinh viên không được thực hành dự đoán kết quả từng bước — kỹ năng quan trọng nhất để hiểu sâu giải thuật.

Hệ thống DSA-Visual được xây dựng nhằm giải quyết các khó khăn trên thông qua 4 giải pháp cốt lõi:
- **Mô phỏng từng bước**: Trực quan hóa mọi thao tác trên cấu trúc dữ liệu và giải thuật bằng hình ảnh động.
- **Đồng bộ ba vùng hiển thị**: Kết nối trực quan giữa mã giả, cấu trúc dữ liệu và lời giải thích chi tiết trong cùng một màn hình.
- **Luyện tập chủ động ba bậc**: Cung cấp hệ thống bài tập tự chấm gồm trắc nghiệm, thực hành tương tác và thử thách lập trình.
- **Theo dõi tiến độ**: Hỗ trợ người học kiểm soát lộ trình học tập cá nhân và cung cấp báo cáo học tập cho giảng viên.

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
| Mã nguồn mở | Một phần | Có | Có | Nội bộ |

Kết luận khảo sát: Các hệ thống hiện có đều sở hữu thế mạnh về mô phỏng từng bước nhưng chưa đáp ứng trọn vẹn nhu cầu giảng dạy và học tập trong nhà trường. Trên cơ sở đó, nhóm phát triển xác định các mục tiêu cốt lõi cho DSA-Visual gồm: giao diện tiếng Việt thân thiện, đồng bộ trực quan giữa mã giả và đồ họa, hệ thống bài tập thực hành tự chấm theo lộ trình ba bậc, hỗ trợ giảng viên biên soạn nội dung và theo dõi tiến độ sinh viên.

Yêu cầu chức năng của hệ thống được phân bổ qua 10 nhóm chức năng chính (Bảng 2.2), tập trung vào các nghiệp vụ học tập, mô phỏng và quản lý đào tạo cốt lõi.

**Bảng 2.2: Tóm tắt yêu cầu chức năng theo nhóm**

| Nhóm chức năng | Chức năng chính | FR tiêu biểu |
|---|---|---|
| Tài khoản | Đăng ký, đăng nhập, đổi/khôi phục mật khẩu, phê duyệt giảng viên | FR-1.1 đến FR-1.9 |
| Học tập | Quản lý chủ đề, bài học, Learning Path, ghi chú cá nhân | FR-2.1 đến FR-2.11 |
| Mô phỏng | Danh mục mô phỏng, hiển thị 3 vùng đồng bộ, điều khiển giải thuật | FR-3.1 đến FR-3.19 |
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
| S9 | 07/07→13/07 | Premium, Lớp học phần | Gems Shop, Premium mô phỏng, lớp học phần |
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


# PHẦN 3: PHÂN TÍCH - ANALYSIS

## 3.1 Mô hình triển khai hệ thống

Hệ thống DSA-Visual được triển khai theo mô hình 3 tầng: trình duyệt (Vue 3 SPA) gọi REST API (ASP.NET Core) và API đọc/ghi dữ liệu trên SQL Server. Toàn bộ Simulation Engine (EDV) chạy phía trình duyệt nên việc sinh bước mô phỏng không tốn tài nguyên máy chủ; máy chủ chỉ nhận các thao tác cần lưu trữ (phiên học, tiến độ, chấm điểm).

```mermaid
graph TB
    subgraph Client["Trình duyệt — SPA (Vue 3 + Vite + TS)"]
        UI[Giao diện SPA]
        STORE[Pinia Stores<br/>auth/lesson/simulation/progress/gamification...]
        ENGINES[Simulation Engine EDV<br/>StepExecutor + Generator + Renderer + Registry]
        CODE[Code Runner Sandbox<br/>Web Worker + Monaco]
        UI --> STORE
        STORE --> ENGINES
        STORE --> CODE
    end
    subgraph Server["Backend (ASP.NET Core — 2 project)"]
        API[Controllers /api/v1/*]
        SVC[Services<br/>Auth/Lesson/Exercise/Progress/Gamification/CodeRunner...]
        DB[AppDbContext + EF Core]
        API --> SVC
        SVC --> DB
    end
    subgraph Data["Dữ liệu"]
        SQL[(SQL Server 2019+)]
    end
    STORE -->|REST + JWT| API
    CODE -->|trace chấm điểm client-side| ENGINES
    DB --> SQL
    AUTH[JWT Bearer + Refresh cookie] -.-> API
```

Sơ đồ trên mô tả luồng chính: giao diện SPA gọi API qua REST kèm JWT, API xử lý qua lớp Service rồi lưu vào SQL Server bằng EF Core; riêng mô phỏng và chấm code chạy ngay trong trình duyệt (Web Worker), máy chủ chỉ lưu lịch sử. Các thành phần chính được tóm tắt ở Bảng 3.1.

**Bảng 3.1: Các thành phần chính của hệ thống**

| Thành phần | Vai trò | Công nghệ |
|---|---|---|
| Client | Hiển thị giao diện, chạy mô phỏng EDV, chạy và chấm code trong sandbox | Vue 3 + Vite + TypeScript, Pinia, Web Worker, Monaco |
| API Server | Xác thực, cung cấp dữ liệu bài học/mô phỏng, chấm điểm bài tập, quản lý tiến độ và gamification | ASP.NET Core (DsaVisual.Api + DsaVisual.Application), EF Core |
| Database | Lưu toàn bộ dữ liệu: người dùng, bài học, bài tập, tiến độ, phiên học, code | SQL Server 2019+ |

## 3.2 Sơ đồ Use Cases

### 3.2.1 Tổng quan

Hệ thống phục vụ **3 tác nhân chính**:

- **Người học (Student)** — xem bài học, chạy mô phỏng, làm bài tập, luyện tập theo lộ trình, quản lý hồ sơ và tham gia lớp học phần;
- **Giảng viên (Teacher)** — biên soạn bài học/bài tập, xem báo cáo giảng dạy và quản lý lớp học phần;
- **Quản trị viên (Admin)** — quản lý người dùng và cấu hình hệ thống.

Bên cạnh đó còn tác nhân **Khách (chưa đăng nhập)** với các chức năng tạo tài khoản, đăng nhập, xem demo công khai và khôi phục mật khẩu. Sơ đồ use case tổng thể gồm đủ 32 use case (UC-01 → UC-32):

```mermaid
graph TD
    subgraph "Hệ thống DSA-Visual"
        A[UC-01 Chạy mô phỏng giải thuật]
        B[UC-02 Tạo tài khoản]
        C[UC-03 Đăng nhập và duy trì phiên]
        D[UC-04 Xem bài học]
        E[UC-05 Tìm kiếm bài học]
        F[UC-06 Làm bài tập trắc nghiệm]
        G[UC-07 Làm bài tập dự đoán bước<br/>(Bậc 2 Lab)]
        H[UC-08 Xem tiến độ cá nhân]
        I[UC-09 Biên soạn bài học]
        J[UC-10 Biên soạn bài tập]
        K[UC-11 Xem báo cáo giảng dạy]
        L[UC-12 Quản lý người dùng]
        M[UC-13 Quản trị cấu hình]
        N[UC-14 Xem demo công khai]
        O[UC-15 Khôi phục mật khẩu]
        P[UC-16 Xem chi tiết bài học và mở module riêng]
        Q[UC-17 Viết và chạy code trong sandbox]
        R[UC-18 Nộp bài tập lập trình]
        S[UC-19 Xem lịch sử nộp bài code]
        T[UC-20 Quản lý lớp học phần]
        U[UC-21 Tham gia lớp bằng mã mời]
        V[UC-22 Ghi chú cá nhân]
        W[UC-23 Xem thành tích và huy hiệu]
        X[UC-24 Gửi phản hồi và báo lỗi]
        Y[UC-25 Học theo Learning Path]
        Z[UC-26 Làm Practice Ladder]
        AA[UC-27 Làm bài kiểm tra cuối lộ trình]
        AC[UC-29 Làm Daily Quest và giữ Streak]
        AD[UC-30 Mua vật phẩm Gems Shop]
        AE[UC-31 Xem Leaderboard]
        AF[UC-32 Nâng cấp Premium]
    end
    Khach["Khách (chưa đăng nhập)"] --> B
    Khach --> C
    Khach --> N
    Khach --> O
    NguoiHoc["Người học (Student)"] --> A
    NguoiHoc --> D
    NguoiHoc --> E
    NguoiHoc --> F
    NguoiHoc --> G
    NguoiHoc --> H
    NguoiHoc --> P
    NguoiHoc --> Q
    NguoiHoc --> R
    NguoiHoc --> S
    NguoiHoc --> U
    NguoiHoc --> V
    NguoiHoc --> W
    NguoiHoc --> X
    NguoiHoc --> Y
    NguoiHoc --> Z
    NguoiHoc --> AA
    NguoiHoc --> AB
    NguoiHoc --> AC
    NguoiHoc --> AD
    NguoiHoc --> AE
    NguoiHoc --> AF
    NguoiDay["Giảng viên (Teacher)"] --> D
    NguoiDay --> E
    NguoiDay --> I
    NguoiDay --> J
    NguoiDay --> K
    NguoiDay --> T
    NguoiDay --> X
    Admin["Quản trị viên (Admin)"] --> L
    Admin --> M
    Admin --> X
```

![Hình 3.1 - Sơ đồ use case tổng thể](diagrams/01-usecase-tong-quan.png)

*Hình 3.1: Sơ đồ use case tổng thể — 3 tác nhân và các chức năng chính của hệ thống. (ảnh placeholder — sinh ảnh thật bằng prompt NHÓM B #1)*

### 3.2.2 Use Cases dành cho người học

Nhóm người học gồm 24 use case: xem bài học và mô phỏng giải thuật, làm bài tập (trắc nghiệm, dự đoán bước, lập trình), học theo lộ trình, tham gia lớp học phần và các chức năng gamification. Sơ đồ nhóm như sau:

```mermaid
graph TD
    subgraph "Hệ thống DSA-Visual — nhóm Người học"
        A[UC-01 Chạy mô phỏng giải thuật]
        B[UC-02 Tạo tài khoản]
        C[UC-03 Đăng nhập và duy trì phiên]
        D[UC-04 Xem bài học]
        E[UC-05 Tìm kiếm bài học]
        F[UC-06 Làm bài tập trắc nghiệm]
        G[UC-07 Làm bài tập dự đoán bước<br/>(Bậc 2 Lab)]
        H[UC-08 Xem tiến độ cá nhân]
        N[UC-14 Xem demo công khai]
        Q[UC-17 Viết và chạy code trong sandbox]
        R[UC-18 Nộp bài tập lập trình]
        S[UC-19 Xem lịch sử nộp bài code]
        U[UC-21 Tham gia lớp bằng mã mời]
        V[UC-22 Ghi chú cá nhân]
        W[UC-23 Xem thành tích và huy hiệu]
        X[UC-24 Gửi phản hồi và báo lỗi]
        Y[UC-25 Học theo Learning Path]
        Z[UC-26 Làm Practice Ladder]
        AA[UC-27 Làm bài kiểm tra cuối lộ trình]
        AC[UC-29 Làm Daily Quest và giữ Streak]
        AD[UC-30 Mua vật phẩm Gems Shop]
        AE[UC-31 Xem Leaderboard]
        AF[UC-32 Nâng cấp Premium]
    end
    NguoiHoc["Người học (Student)"] --> A
    NguoiHoc --> B
    NguoiHoc --> C
    NguoiHoc --> D
    NguoiHoc --> E
    NguoiHoc --> F
    NguoiHoc --> G
    NguoiHoc --> H
    NguoiHoc --> N
    NguoiHoc --> Q
    NguoiHoc --> R
    NguoiHoc --> S
    NguoiHoc --> U
    NguoiHoc --> V
    NguoiHoc --> W
    NguoiHoc --> X
    NguoiHoc --> Y
    NguoiHoc --> Z
    NguoiHoc --> AA
    NguoiHoc --> AB
    NguoiHoc --> AC
    NguoiHoc --> AD
    NguoiHoc --> AE
    NguoiHoc --> AF
```

![Hình 3.2 - Sơ đồ use case nhóm người học](diagrams/02-usecase-hoc-vien.png)

*Hình 3.2: Sơ đồ use case dành cho người học — 24 chức năng học tập và luyện tập chính. (ảnh placeholder — sinh ảnh thật bằng prompt NHÓM B #1)*

Các use case chính được liệt kê ở Bảng 3.2:

**Bảng 3.2: Danh sách use case dành cho người học**

| Mã UC | Tên | Mô tả |
|---|---|---|
| UC-01 | Chạy mô phỏng giải thuật | Mở mô phỏng, cấu hình dữ liệu đầu vào, điều khiển từng bước với 3 vùng đồng bộ (trực quan – mã giả – giải thích) |
| UC-02 | Tạo tài khoản | Đăng ký bằng email + mật khẩu, vai trò mặc định Student; chọn "Tôi là giảng viên" thì chờ Admin duyệt |
| UC-03 | Đăng nhập và duy trì phiên | Đăng nhập nhận JWT, tự động gia hạn phiên bằng refresh token, đăng xuất thu hồi phiên |
| UC-04 | Xem bài học | Duyệt cây chủ đề, đọc lý thuyết, đánh dấu đã học và mở module riêng (mô phỏng/bài tập/code) |
| UC-05 | Tìm kiếm bài học | Gõ từ khóa, hệ thống gợi ý bài học sau 300ms, chọn kết quả để mở bài học |
| UC-06 | Làm bài tập trắc nghiệm | Trả lời câu hỏi, nộp bài, hệ thống chấm điểm tự động và hiển thị giải thích |
| UC-07 | Làm bài tập dự đoán bước | Thao tác trên canvas editable, nộp, hệ thống chấm trạng thái cuối + giới hạn số bước |
| UC-08 | Xem tiến độ cá nhân | Xem thẻ KPI tổng quan, tiến độ theo từng chủ đề và nhảy tới bài học chưa học |
| UC-14 | Xem demo công khai | Khách chạy thử 3 demo (Bubble Sort, Binary Search, BFS) không cần tài khoản |
| UC-17 | Viết và chạy code trong sandbox | Soạn/hiệu chỉnh code trong Monaco, chạy an toàn trong Web Worker, xem trace đồng bộ editor – visual |
| UC-18 | Nộp bài tập lập trình | Nộp code, hệ thống chấm bằng test ẩn (tĩnh + ngẫu nhiên) và trả kết quả từng test |
| UC-19 | Xem lịch sử nộp bài code | Xem danh sách lần nộp, mở lại code cũ kèm kết quả và so sánh 2 lần nộp |
| UC-21 | Tham gia lớp bằng mã mời | Nhập mã mời 6 ký tự để vào lớp học phần đang mở |
| UC-22 | Ghi chú cá nhân | Soạn ghi chú gắn với bài học, tự lưu sau 1 giây, xem lại và xóa |
| UC-23 | Xem thành tích và huy hiệu | Xem huy hiệu đã mở và huy hiệu ẩn, nhận toast khi đạt huy hiệu mới |
| UC-24 | Gửi phản hồi và báo lỗi | Đánh giá sao + nhận xét bài học, gửi báo lỗi kèm ngữ cảnh tự động (URL, bước mô phỏng) |
| UC-25 | Học theo Learning Path | Chọn lộ trình, xem bản đồ node, vào node đang mở (trừ tim), pass node để mở khóa node kế |
| UC-26 | Làm Practice Ladder | Chuỗi 3 bậc Quiz → Lab → Code; pass bậc trước mới mở bậc sau, retry trong phiên miễn phí |
| UC-27 | Làm bài kiểm tra cuối lộ trình | Làm final test sau khi pass toàn bộ node, ngưỡng pass ≥ 70% |
| UC-29 | Làm Daily Quest và giữ Streak | Nhận thử thách hằng ngày, hoàn thành để giữ chuỗi ngày học liên tục |
| UC-30 | Mua vật phẩm Gems Shop | Dùng gems đổi vật phẩm (tim, streak freeze...) trong cửa hàng |
| UC-31 | Xem Leaderboard | Xem bảng xếp hạng theo XP |
| UC-32 | Nâng cấp Premium | Checkout mô phỏng gói Premium, quản lý hết hạn |

### 3.2.3 Use Cases dành cho giảng viên

Nhóm giảng viên gồm 4 use case: biên soạn nội dung, xem báo cáo và quản lý lớp học phần:

```mermaid
graph TD
    subgraph "Hệ thống DSA-Visual — nhóm Giảng viên"
        I[UC-09 Biên soạn bài học]
        J[UC-10 Biên soạn bài tập]
        K[UC-11 Xem báo cáo giảng dạy]
        T[UC-20 Quản lý lớp học phần]
    end
    NguoiDay["Giảng viên (Teacher)"] --> I
    NguoiDay --> J
    NguoiDay --> K
    NguoiDay --> T
```

![Hình 3.3 - Sơ đồ use case nhóm giảng viên](diagrams/03-usecase-giang-vien.png)

*Hình 3.3: Sơ đồ use case dành cho giảng viên — 4 chức năng biên soạn, báo cáo và quản lý lớp. (ảnh placeholder — sinh ảnh thật bằng prompt NHÓM B #1)*

**Bảng 3.3: Danh sách use case dành cho giảng viên**

| Mã UC | Tên | Mô tả |
|---|---|---|
| UC-09 | Biên soạn bài học | Tạo/sửa chủ đề và bài học rich-text, gắn mô phỏng có sẵn kèm cấu hình mặc định, gắn bài tập, lưu nháp và kích hoạt |
| UC-10 | Biên soạn bài tập | Tạo bài tập theo loại câu hỏi (chọn 1/nhiều/đúng-sai), soạn đáp án và giải thích, xem trước và kích hoạt |
| UC-11 | Xem báo cáo giảng dạy | Xem thống kê bài học (người xem, % hoàn thành, điểm trung bình) và xuất CSV |
| UC-20 | Quản lý lớp học phần | Tạo lớp + mã mời 6 ký tự, thêm/xóa sinh viên, gán nội dung kèm hạn nộp, xem báo cáo lớp và xuất CSV |

### 3.2.4 Use Cases dành cho quản trị viên

Nhóm quản trị viên gồm 2 use case:

```mermaid
graph TD
    subgraph "Hệ thống DSA-Visual — nhóm Quản trị viên"
        L[UC-12 Quản lý người dùng]
        M[UC-13 Quản trị cấu hình]
    end
    Admin["Quản trị viên (Admin)"] --> L
    Admin --> M
```

![Hình 3.4 - Sơ đồ use case nhóm quản trị viên](diagrams/04-usecase-admin.png)

*Hình 3.4: Sơ đồ use case dành cho quản trị viên — quản lý người dùng và cấu hình hệ thống. (ảnh placeholder — sinh ảnh thật bằng prompt NHÓM B #1)*

**Bảng 3.4: Danh sách use case dành cho quản trị viên**

| Mã UC | Tên | Mô tả |
|---|---|---|
| UC-12 | Quản lý người dùng | Xem danh sách người dùng, khóa/mở khóa, phê duyệt tài khoản giảng viên, đặt lại mật khẩu; mọi thao tác đều ghi log máy chủ |
| UC-13 | Quản trị cấu hình hệ thống | Chỉnh cấu hình hệ thống (domain email, chính sách mật khẩu, giới hạn upload), lưu và áp dụng ngay không cần khởi động lại |

## 3.3 Đặc tả yêu cầu hệ thống (SRS)

### 3.3.1 Ma trận yêu cầu chức năng

Bảng 3.5 tổng hợp đầy đủ **75 yêu cầu chức năng (FR)** của hệ thống theo 10 module (A — Xác thực, B — Bài học, C — Mô phỏng, D — Practice Ladder, E — Tiến độ, F — Quản trị, G — Trang phụ trợ, H — Lớp học phần, I — Code Runner, J — Gamification & Premium). 12 FR đã được duyệt cắt (FR-1.10, FR-2.7, FR-2.8, FR-2.9, FR-3.13, FR-3.17, FR-3.19, FR-5.6, FR-5.7, FR-6.4, FR-7.3, FR-7.5) không nằm trong bảng.

**Bảng 3.5: Ma trận yêu cầu chức năng (master matrix FR)**

| Mã FR | Tên | Mô tả ngắn | UC liên quan | Ưu tiên |
|---|---|---|---|---|
| FR-1.1 | Đăng ký tài khoản | Khách tạo tài khoản bằng email + mật khẩu, vai trò mặc định Student | UC-02 | Cao |
| FR-1.2 | Đăng nhập | Xác thực email + mật khẩu, cấp access token và refresh cookie | UC-03 | Cao |
| FR-1.3 | Gia hạn phiên | Tự động cấp access token mới khi hết hạn bằng refresh token | UC-03 | Cao |
| FR-1.4 | Đăng xuất | Thu hồi refresh token và xóa cookie phiên | UC-03 | Cao |
| FR-1.5 | Đổi mật khẩu | Người dùng đổi mật khẩu của mình sau khi xác thực | UC-03 | TB |
| FR-1.6 | Khôi phục mật khẩu | Gửi link đặt lại mật khẩu qua email, hiệu lực 30 phút, dùng 1 lần | UC-15 | TB |
| FR-1.7 | Cập nhật thông tin cá nhân | Sửa họ tên, avatar trong hồ sơ cá nhân | UC-03 | TB |
| FR-1.8 | Phê duyệt tài khoản giảng viên | Admin duyệt hoặc từ chối tài khoản TeacherPending | UC-12 | TB |
| FR-1.9 | Quản lý người dùng | Admin khóa/mở khóa, đặt lại mật khẩu, ghi log mọi thao tác | UC-12 | TB |
| FR-1.11 | Xác thực hai lớp | Yêu cầu mã 6 số gửi email khi đăng nhập (nếu bật) | UC-03 | Thấp |
| FR-2.1 | Quản lý chủ đề | CRUD chủ đề (topic) cho giảng viên | UC-09 | Cao |
| FR-2.2 | Quản lý bài học | CRUD bài học rich-text, gắn mô phỏng và bài tập | UC-09 | Cao |
| FR-2.3 | Xem danh sách bài học | Duyệt cây chủ đề kèm trạng thái tiến độ từng bài | UC-04 | Cao |
| FR-2.4 | Xem chi tiết bài học | Đọc nội dung lý thuyết, đánh dấu đã học, mở module riêng | UC-04 | Cao |
| FR-2.5 | Tìm kiếm bài học | Gợi ý kết quả theo từ khóa sau 300ms | UC-05 | TB |
| FR-2.6 | Ghi chú cá nhân trên bài học | Ghi chú riêng gắn bài học, tự lưu sau 1 giây | UC-04 | TB |
| FR-2.10 | Learning Path | Lộ trình node mở khóa tuần tự theo kết quả học | UC-25 | Cao |
| FR-2.11 | Two-way sync bằng deep-link | Mở thẳng mô phỏng tại bước N qua link `?step=N` và ngược lại | UC-01 | Cao |
| FR-3.1 | Danh mục mô phỏng | Xem danh sách mô phỏng theo loại giải thuật | UC-01 | Cao |
| FR-3.2 | Khởi tạo mô phỏng | Kiểm tra tim, nạp cấu hình mặc định và sinh chuỗi bước khi mở | UC-01 | Cao |
| FR-3.3 | Hiển thị đồng bộ 3 vùng | Canvas trực quan, mã giả và giải thích cập nhật theo từng bước | UC-01 | Cao |
| FR-3.4 | Cấu hình dữ liệu đầu vào | Nhập/đổi dữ liệu mẫu, sinh lại chuỗi bước về bước 0 | UC-01 | Cao |
| FR-3.5 | Điều khiển mô phỏng | Phát/dừng/bước tiếp/bước lùi/về đầu/về cuối, đổi tốc độ | UC-01 | Cao |
| FR-3.6 | Trạng thái trực quan của phần tử | Tô màu trạng thái phần tử theo từng bước thực thi | UC-01 | Cao |
| FR-3.7 | Bảng mã giả đồng bộ | Highlight dòng mã giả tương ứng bước đang chạy | UC-01 | Cao |
| FR-3.8 | Tùy chọn hiển thị | Ẩn/hiện vùng, đổi tốc độ và màu sắc hiển thị | UC-01 | TB |
| FR-3.9 | Bộ đếm thống kê | Đếm số bước, số lần chạy và thống kê so sánh | UC-01 | TB |
| FR-3.10 | Lưu mô phỏng yêu thích | Đánh dấu mô phỏng yêu thích để mở nhanh | UC-01 | Thấp |
| FR-3.11 | Chia sẻ liên kết mô phỏng | Sao chép link chia sẻ mô phỏng hiện tại | UC-01 | Thấp |
| FR-3.12 | Thực hành bước thủ công | Người học tự thao tác, hệ thống kiểm tra kết quả | UC-01 | Cao |
| FR-3.14 | Hiển thị ngăn xếp đệ quy | Call Stack hiển thị trong mô phỏng đệ quy | UC-01 | TB |
| FR-3.15 | Điểm dừng có điều kiện | Dừng mô phỏng khi gặp điều kiện do người học chỉ định | UC-01 | TB |
| FR-3.16 | Kiểm tra nhanh sau mô phỏng | Mini quiz ngắn sau khi xem xong mô phỏng | UC-01 | TB |
| FR-3.18 | Chế độ tối | Giao diện tối cho toàn hệ thống | — | TB |
| FR-4.1 | Quản lý bài tập | CRUD bài tập cho giảng viên (loại, câu hỏi, đáp án, bậc Ladder) | UC-10 | Cao |
| FR-4.2 | Làm bài tập trắc nghiệm | Bậc 1 Quiz: trả lời, nộp, chấm điểm tự động, hiển thị giải thích | UC-06 | Cao |
| FR-4.3 | Bài tập dự đoán bước | Bậc 2 Lab: chấm trạng thái cuối + giới hạn số bước | UC-07 | TB |
| FR-4.4 | Đánh giá và lịch sử bài làm | Lưu điểm, giữ điểm cao nhất, xem lại bài làm | UC-06 | TB |
| FR-4.5 | Ngân hàng câu hỏi dùng lại | Tái sử dụng câu hỏi theo chủ đề/tag | UC-10 | Thấp |
| FR-4.6 | Chế độ luyện tập | Làm lại bài tập không tính vào điểm chính thức | UC-06 | TB |
| FR-4.7 | Gợi ý trả lời | Hints trừ 20%/gợi ý, tối thiểu giữ 40% điểm câu | UC-06 | TB |
| FR-4.8 | Xáo trộn câu hỏi và phương án | Trộn câu hỏi + phương án theo seed ổn định | UC-06 | TB |
| FR-4.9 | Giải thích theo phương án sai | Hiển thị giải thích riêng cho từng phương án sai | UC-06 | TB |
| FR-4.10 | Nhập câu hỏi từ CSV | Nhập hàng loạt câu hỏi từ file CSV | UC-10 | Thấp |
| FR-4.11 | Practice Ladder tuần tự | Chuỗi 3 bậc Quiz → Lab → Code theo từng node | UC-26 | Cao |
| FR-4.12 | Kiểm tra cuối lộ trình | Final test sau khi pass toàn bộ node, ngưỡng pass ≥ 70% | UC-27 | Cao |
| FR-5.1 | Ghi nhận tiến độ | Cập nhật tiến độ người học sau mỗi hành động học | UC-08 | Cao |
| FR-5.2 | Dashboard tiến độ cá nhân | Thẻ KPI + thanh tiến độ theo chủ đề | UC-08 | Cao |
| FR-5.3 | Báo cáo giảng viên | Thống kê bài học, xuất CSV | UC-11 | TB |
| FR-5.4 | Thống kê hệ thống | Báo cáo tổng hợp cho Admin | — | TB |
| FR-5.5 | Huy hiệu thành tích | Trao huy hiệu đúng 1 lần theo sự kiện học tập | UC-08 | TB |
| FR-6.2 | Cấu hình hệ thống | Chỉnh cấu hình, áp dụng ngay không cần khởi động lại | UC-13 | TB |
| FR-7.1 | Trang chủ công khai | Trang chủ với demo công khai | UC-14 | TB |
| FR-7.2 | Trang trợ giúp | Trang FAQ và trợ giúp | — | TB |
| FR-7.4 | Đánh giá nội dung | Sao + nhận xét ≤ 200 ký tự cho bài học | — | Thấp |
| FR-7.6 | Demo công khai 3 visualizer | Chạy thử Bubble Sort, Binary Search, BFS không cần tài khoản | UC-14 | TB |
| FR-8.1 | Tạo và quản lý lớp học phần | Tạo lớp + mã mời duy nhất 6 ký tự | — | TB |
| FR-8.2 | Quản lý sinh viên trong lớp | Thêm/xóa sinh viên, tham gia bằng mã mời | — | TB |
| FR-8.3 | Gán nội dung và hạn nộp | Gán nội dung bắt buộc kèm hạn nộp theo lớp | — | TB |
| FR-8.4 | Báo cáo theo lớp | Báo cáo % hoàn thành, điểm TB, danh sách chậm trễ, xuất CSV | — | TB |
| FR-9.1 | Trình soạn mã nhúng | Editor Monaco nạp sẵn code mẫu, highlight cú pháp | UC-17 | Cao |
| FR-9.2 | Chạy mã và trực quan hóa | Chạy code an toàn, phát trace đồng bộ editor – visual | UC-17 | Cao |
| FR-9.3 | Bài tập lập trình + chấm tự động | Chấm bằng test ẩn (tĩnh + ngẫu nhiên), trả kết quả từng test | UC-18 | TB |
| FR-9.4 | Sandbox an toàn | Chạy trong Web Worker, chặn vòng lặp vô hạn, không treo trình duyệt | UC-17 | Cao |
| FR-9.5 | Lịch sử nộp bài code | Xem lại các lần nộp và so sánh kết quả | UC-19 | TB |
| FR-9.6 | Sandbox giới hạn chi tiết | Giới hạn 10 giây / 64MB / 200 dòng, cấm import và I/O ngoài | UC-17 | Cao |
| FR-10.1 | Tim, hồi tim và session | Trừ 1 tim atomic khi vào node, session 30 phút resume miễn phí | UC-25 | Cao |
| FR-10.2 | Gems + Gems Shop | Kiếm gems và đổi vật phẩm trong cửa hàng | UC-30 | TB |
| FR-10.3 | Daily Quest | Thử thách hằng ngày, hoàn thành nhận thưởng | UC-29 | TB |
| FR-10.4 | Streak + Streak Freeze | Giữ chuỗi ngày học liên tục, dùng vật phẩm giữ chuỗi | UC-29 | TB |
| FR-10.5 | XP & Level | Tích lũy XP, tăng cấp, trao XP 1 lần khi pass đầu | UC-25 | TB |
| FR-10.6 | Leaderboard | Bảng xếp hạng theo XP | UC-31 | TB |
| FR-10.7 | Premium và hết hạn | Gói Premium (P1), checkout mô phỏng, quản lý hết hạn | UC-32 | TB |

### 3.3.2 Đặc tả các use case chính

Phần này đặc tả 3 use case tiêu biểu của hệ thống theo 4 nội dung: mô tả chức năng, dữ liệu liên quan, đối tượng sử dụng và yêu cầu bảo mật.

#### UC-01 — Chạy mô phỏng giải thuật

- **Mô tả chức năng:** Người học mở mô phỏng từ Node Hub hoặc từ bài học. Hệ thống kiểm tra tim, nạp cấu hình mặc định và sinh chuỗi bước, sau đó hiển thị bước 0. Người học điều khiển phát/dừng/bước tiếp/bước lùi, kéo thanh tiến trình hoặc đổi tốc độ; cả 3 vùng (trực quan, mã giả, giải thích) cập nhật đồng bộ theo từng bước. Khi hoàn tất, hệ thống hiển thị tóm tắt thống kê và lưu trạng thái resume nếu đang trong phiên học.
- **Dữ liệu liên quan:** `Lessons`, `LessonSimulations`, `NodeSessions`, `UserProgress` (ghi nhận sự kiện chạy mô phỏng ≥ 5 bước).
- **Đối tượng sử dụng:** Người học (đã đăng nhập); khách chỉ dùng được 3 demo công khai không lưu dữ liệu.
- **Yêu cầu bảo mật:** Xác thực JWT cho người đăng nhập; trừ tim atomic phía server (mã lỗi 403 `HEARTS_EMPTY` khi hết tim); demo công khai bị chặn truy cập các API cần phiên.

#### UC-25 — Học theo Learning Path và mở khóa node

- **Mô tả chức năng:** Người học chọn lộ trình, xem bản đồ node (khóa/đang học/hoàn thành 1-3 sao) và bấm vào node đang mở. Hệ thống kiểm tra và trừ 1 tim, tạo mới hoặc gia hạn phiên học rồi đưa người học vào Node Hub. Khi pass node, hệ thống cập nhật tiến độ, mở khóa node kế và trao XP 1 lần cho lần pass đầu; hết lộ trình thì mở bài kiểm tra cuối.
- **Dữ liệu liên quan:** `LearningPaths`, `LearningPathNodes`, `NodeSessions`, `UserNodeProgress`, `Users` (tim, XP).
- **Đối tượng sử dụng:** Người học (đã đăng nhập).
- **Yêu cầu bảo mật:** Xác thực JWT; trừ tim atomic server-side chống double-spend khi mở nhiều tab cùng lúc; dùng server timestamp để chống chỉnh đồng hồ thiết bị gian lận hồi tim.

#### UC-26 — Làm Practice Ladder (Quiz → Lab → Code)

- **Mô tả chức năng:** Với mỗi node, người học trải qua 3 bậc: pass Quiz ≥ 60% mới mở Bậc 2 Lab; pass Lab (chấm trạng thái cuối khớp kết quả chuẩn và số bước không vượt giới hạn) mới mở Bậc 3 Code; pass Code ≥ 70% test ẩn thì pass node. Retry bậc trong phiên 30 phút không trừ thêm tim; thoát giữa chừng thì resume đúng bậc đang dở.
- **Dữ liệu liên quan:** `Exercises`, `ExerciseSubmissions`, `CodeRuns`, `CodeSubmissions`, `NodeSessions`, `UserNodeProgress`.
- **Đối tượng sử dụng:** Người học (đã đăng nhập và vào node).
- **Yêu cầu bảo mật:** Xác thực JWT; server guard chặn vào bậc sau khi chưa pass bậc trước; chấm điểm phía server, nộp bài idempotent (không tính 2 lần khi mất mạng).


# PHẦN 4: THIẾT KẾ - DESIGN

## 4.1 Mô hình công nghệ

Hệ thống gồm 3 lớp công nghệ rõ ràng: frontend là SPA Vue 3 chạy trong trình duyệt và chứa toàn bộ Simulation Engine EDV; backend là API ASP.NET Core gồm 2 project; dữ liệu lưu trong SQL Server. Frontend gọi backend qua REST có xác thực JWT. Sơ đồ kiến trúc tổng thể như sau:

```mermaid
graph TB
    subgraph Client["Trình duyệt — SPA (Vue 3 + Vite + TS)"]
        UI[Giao diện SPA]
        STORE[Pinia Stores<br/>auth/lesson/simulation/progress/gamification...]
        ENGINES[Simulation Engine EDV<br/>StepExecutor + Generator + Renderer + Registry]
        CODE[Code Runner Sandbox<br/>Web Worker + Monaco]
        UI --> STORE
        STORE --> ENGINES
        STORE --> CODE
    end
    subgraph Server["Backend (ASP.NET Core — 2 project)"]
        API[Controllers /api/v1/*]
        SVC[Services<br/>Auth/Lesson/Exercise/Progress/Gamification/CodeRunner...]
        DB[AppDbContext + EF Core]
        API --> SVC
        SVC --> DB
    end
    subgraph Data["Dữ liệu"]
        SQL[(SQL Server 2019+)]
    end
    STORE -->|REST + JWT| API
    CODE -->|trace chấm điểm client-side| ENGINES
    DB --> SQL
    AUTH[JWT Bearer + Refresh cookie] -.-> API
```

**Bảng 4.1: Tổng hợp công nghệ theo lớp**

| Lớp | Công nghệ | Vai trò |
|---|---|---|
| Frontend | Vue 3 + Pinia + Vite + TypeScript | Giao diện SPA, quản lý trạng thái, chạy Simulation Engine EDV và Code Runner ngay trong trình duyệt |
| Backend | ASP.NET Core + EF Core | API REST `/api/v1`, xử lý nghiệp vụ trong Service, truy cập dữ liệu qua DbContext |
| CSDL | SQL Server 2019+ | Lưu trữ 32 bảng dữ liệu, lưu lịch sử chấm điểm |

Điểm công nghệ đáng chú ý: Simulation Engine EDV chạy phía client nên bước lùi miễn phí và sinh bước nhanh (NFR-2); Code Runner chấm code trong sandbox Web Worker, backend chỉ lưu lịch sử; phiên đăng nhập dùng JWT access token trong bộ nhớ kèm refresh cookie an toàn (ADR-004).

## 4.2 Thiết kế giao diện

### 4.2.1 Sitemap

Hệ thống có khoảng 32 màn chính (SDD §8.4 đặc tả đủ 33 route, gồm Màn 33 Khám phá). Sơ đồ luồng màn hình chính như sau:

```mermaid
graph LR
    LANDING[Trang chủ] --> AUTH[Đăng nhập/Đăng ký]
    LANDING --> PATH[/path/:topicId - Learning Path/]
    PATH --> NODE[/path/:topicId/node/:nodeId - Node Hub/]
    NODE --> SIM[/simulator/:key/]
    NODE --> LADDER[/ladder/:nodeId/]
    LADDER --> LAB[/ladder/:nodeId/lab/]
    LADDER --> CODE[/code/:key/]
    PATH --> FINAL[/path/:topicId/final-test/]
    PATH --> CHEAT[/cheatsheet/]
    CHEAT --> SIM
    NODE --> CHEAT
    SIM -->|"Xem lý thuyết liên quan (FR-2.11)"| NODE
    LANDING --> PROFILE[/profile/]
    PROFILE --> QUESTS[/quests/]
    PROFILE --> LEADER[/leaderboard/]
    PROFILE --> SHOP[/shop/]
    LANDING --> CLASSES[/classes/]
    CLASSES --> CLASSDETAIL[/classes/:id/]
    CLASSDETAIL --> CLASSREPORT[/classes/:id/report/]
    LANDING --> PREMIUM[/premium/]
    PREMIUM --> SUB[/account/subscription/]
    LANDING --> ADMIN[/admin/*/]
```

Mũi tên trong sơ đồ thể hiện đường đi chính của người dùng: khách ghé trang chủ rồi đăng ký, người học đi từ bản đồ lộ trình vào Node Hub rồi rẽ sang mô phỏng/luyện tập, giảng viên và quản trị viên đi vào nhóm màn riêng của vai trò. Các màn được nhóm theo chức năng như bảng dưới:

**Bảng 4.2: Nhóm màn theo chức năng**

| Nhóm | Màn | Số màn |
|---|---|---|
| Công khai | 01 Trang chủ, 02 Đăng nhập/Đăng ký, 12 Trợ giúp | 3 |
| Học tập | 03 Danh sách bài học (redirect), 04 Chi tiết bài học, 13 Learning Path, 18 CheatSheet, 30 Final Test, 31 Node Hub | 6 |
| Mô phỏng | 05 Simulator, 33 Khám phá | 2 |
| Luyện tập | 06 Bài tập trắc nghiệm, 07 Dự đoán bước (sáp nhập Bậc 2), 14 Practice Ladder, 15 Interactive Lab, 16 Code Runner | 5 |
| Gamification | 22 Shop, 23 Daily Quest, 24 Leaderboard, 25 Premium, 26 Checkout (modal), 27 Quản lý gói, 28 Modal Hết tim | 7 |
| Lớp học | 19 Danh sách lớp, 20 Chi tiết lớp, 21 Báo cáo lớp | 3 |
| Quản trị | 08 Dashboard (redirect), 09 Quản trị nội dung, 10 Quản lý người dùng, 11 Thống kê, 29 Chờ duyệt Teacher | 5 |
| Hồ sơ | 32 Hồ sơ cá nhân | 1 |

### 4.2.2 Layout

Giao diện dùng một hệ thống thiết kế thống nhất cho mọi màn: màu, font, cỡ chữ và bộ component dùng chung được định nghĩa một chỗ, không viết CSS rải rác.

**Bảng 4.3: Hệ thống thiết kế (design tokens)**

| Mục | Đặc tả |
|---|---|
| Ngôn ngữ | Tiếng Việt có dấu, mọi chuỗi nằm trong file i18n |
| Font | Inter/Roboto (fallback Segoe UI, Arial); mã giả dùng JetBrains Mono/Consolas |
| Cỡ chữ | Nội dung 14px, form 16px, tiêu đề 20/24/32px |
| Màu chủ đạo | Primary #2563EB, Secondary #0F172A, Success #16A34A, Warning #D97706, Danger #DC2626, Background #F8FAFC, Surface #FFFFFF |
| Màu trạng thái mô phỏng | default #CBD5E1, active #FACC15, highlight #FB923C, swap #EF4444, done #22C55E, error #B91C1C, muted #E2E8F0 |
| Bo góc/đổ bóng | Thẻ 8px, nút 6px; shadow nhẹ, modal 0 10px 25px |
| Component tự xây | Button, Input, Select, Modal, Toast, Table, Card, Tabs, Tooltip, Skeleton, EmptyState, Badge, ProgressBar, Drawer |
| Thư viện hỗ trợ | Icon lucide-vue-next, soạn thảo rich-text Quill, biểu đồ Chart.js |

Toàn bộ màn học được bọc trong App shell gồm header chung và sidebar trái theo vai trò. Header luôn hiển thị widget tim/gems/streak của người dùng. Menu khác nhau cho 3 vai trò:

**Bảng 4.4: Sidebar theo vai trò**

| Vai trò | Menu chính |
|---|---|
| Sinh viên | Lộ trình (/path), Khám phá (/simulations), Hồ sơ (/profile), Thử thách (/quests), Lớp học (/classes), Cửa hàng (/shop), Premium, Trợ giúp |
| Giảng viên | Lộ trình, Khám phá, Quản lý nội dung (/admin/*), Lớp học (/classes), Báo cáo (/reports), còn lại như Sinh viên |
| Quản trị viên | Người dùng (/admin/users), Nội dung (/admin/lessons), Cấu hình (/admin/settings), Thống kê (/admin/stats), còn lại như Giảng viên |

Hai route cũ `/learn` và `/dashboard` tự chuyển hướng sang `/path` và `/profile` để giữ một lối vào duy nhất.

### 4.2.3 Giao diện chức năng

Dưới đây là các giao diện chức năng chính của hệ thống, được chụp trực tiếp từ ứng dụng thực tế.

#### 4.2.3.1 Giao diện Trang chủ

![Hình 4.1 - Trang chủ](screenshots/01-home.png)
*Hình 4.1: Giao diện Trang chủ hệ thống*

Trang chủ giới thiệu tổng quan về nền tảng, cung cấp các tính năng nổi bật và hỗ trợ người dùng trải nghiệm nhanh các thuật toán mô phỏng mẫu.

#### 4.2.3.2 Giao diện Đăng nhập và Đăng ký

![Hình 4.2 - Đăng nhập/Đăng ký](screenshots/02-login.png)
*Hình 4.2: Giao diện Đăng nhập và Đăng ký*

Hệ thống xác thực tài khoản an toàn cho sinh viên và giảng viên, tích hợp kiểm tra dữ liệu đầu vào và hỗ trợ khôi phục mật khẩu.

#### 4.2.3.3 Giao diện Chi tiết bài học

![Hình 4.3 - Chi tiết bài học](screenshots/04-lesson-detail.png)
*Hình 4.3: Giao diện Chi tiết bài học*

Trình bày nội dung lý thuyết cấu trúc dữ liệu, tích hợp ghi chú học tập cá nhân và liên kết trực tiếp tới các bài mô phỏng tương ứng.

#### 4.2.3.4 Giao diện Trình mô phỏng thuật toán

![Hình 4.4 - Màn hình mô phỏng](screenshots/05-simulator.png)
*Hình 4.4: Giao diện Trình mô phỏng thuật toán trực quan*

Không gian học tập trực quan với bố cục ba vùng đồng bộ: mã giả thuật toán, đồ họa trực quan và bảng giải thích từng bước thực thi. Người học có thể điều chỉnh tốc độ, nhảy bước, đặt điểm dừng và tùy biến dữ liệu đầu vào.

#### 4.2.3.5 Giao diện Làm bài tập trắc nghiệm

![Hình 4.5 - Bài tập trắc nghiệm](screenshots/06-exercise.png)
*Hình 4.5: Giao diện Làm bài tập trắc nghiệm*

Cung cấp bài tập trắc nghiệm củng cố kiến thức lý thuyết, tự động chấm điểm và hiển thị giải thích chi tiết cho từng câu hỏi.

#### 4.2.3.6 Giao diện Bản đồ lộ trình học tập

![Hình 4.6 - Learning Path](screenshots/13-learning-path.png)
*Hình 4.6: Giao diện Bản đồ lộ trình học tập (Learning Path)*

Hiển thị trực quan tiến trình học tập theo các chặng kiến thức và trạng thái mở khóa bài học từ cơ bản đến nâng cao.

#### 4.2.3.7 Giao diện Khung luyện tập ba bậc

![Hình 4.7 - Practice Ladder](screenshots/14-ladder.png)
*Hình 4.7: Giao diện Khung luyện tập ba bậc (Practice Ladder)*

Khung rèn luyện kiến thức toàn diện qua ba cấp độ thử thách liên tiếp: Trắc nghiệm (Quiz), Thực hành tương tác (Lab) và Thử thách lập trình (Code).

#### 4.2.3.8 Giao diện Phòng thực hành tương tác

![Hình 4.8 - Interactive Lab](screenshots/15-lab.png)
*Hình 4.8: Giao diện Phòng thực hành tương tác (Interactive Lab)*

Môi trường cho phép người học trực tiếp thao tác trên cấu trúc dữ liệu để dự đoán và giải quyết bài toán theo kịch bản yêu cầu.

#### 4.2.3.9 Giao diện Trình soạn thảo và thực thi mã nguồn

![Hình 4.9 - Code Runner](screenshots/16-code-runner.png)
*Hình 4.9: Giao diện Trình soạn thảo và thực thi mã nguồn (Code Runner)*

Môi trường lập trình trực tuyến hỗ trợ viết mã và quan sát thuật toán thực thi theo thời gian thực trên canvas trực quan.

#### 4.2.3.10 Giao diện Đo điểm chuẩn hiệu năng

Công cụ so sánh hiệu năng thực tế giữa các thuật toán trên nhiều kích thước dữ liệu đầu vào khác nhau.

#### 4.2.3.11 Giao diện Bảng xếp hạng thành tích

![Hình 4.11 - Bảng xếp hạng](screenshots/24-leaderboard.png)
*Hình 4.11: Giao diện Bảng xếp hạng thành tích*

Vinh danh thành tích học tập của sinh viên theo tuần, cấp độ và lớp học phần nhằm thúc đẩy động lực rèn luyện.

#### 4.2.3.12 Giao diện Hồ sơ cá nhân người dùng

![Hình 4.12 - Hồ sơ cá nhân](screenshots/32-profile.png)
*Hình 4.12: Giao diện Hồ sơ cá nhân người dùng*

Theo dõi tiến độ học tập tổng quan, danh hiệu đạt được và quản lý thông tin tài khoản cá nhân.

## 4.3 Thiết kế dữ liệu

### 4.3.1 Sơ đồ quan hệ thực thể (ERD)

Cơ sở dữ liệu gồm 32 bảng chia 2 nhóm: lõi học tập 24 bảng (tài khoản, nội dung bài học, bài tập, tiến độ, lớp học, lộ trình) và gamification/code 8 bảng (nhiệm vụ, shop, đá quý, Premium, code runner). Users xuất hiện ở cả 2 sơ đồ để vẽ quan hệ, không đếm thêm.

(a) ERD lõi học tập (24 bảng):

```mermaid
erDiagram
    Users ||--o{ RefreshTokens : has
    Users ||--o{ PasswordResetTokens : has
    Users ||--o{ UserProgress : has
    Users ||--o{ Favorites : has
    Users ||--o{ ExerciseSubmissions : submits
    Users ||--o{ LessonNotes : "owns"
    Users ||--o{ UserAchievements : earns
    Users ||--o{ ContentFeedback : gives
    Users ||--o{ BugReports : reports
    Users }o--o{ Classes : "manages (OwnerId)"
    Topics ||--o{ Topics : "parent"
    Topics ||--o{ Lessons : contains
    Lessons ||--o{ LessonSimulations : has
    Lessons ||--o{ Exercises : has
    Lessons ||--o{ UserProgress : tracked
    Lessons ||--o{ LessonNotes : "noted"
    Lessons ||--o{ ContentFeedback : receives
    Exercises ||--o{ Questions : has
    Exercises ||--o{ ExerciseSubmissions : receives
    Classes ||--o{ ClassMembers : has
    Classes ||--o{ ClassAssignments : assigns
    ClassMembers ||--o{ Users : includes
    Achievements ||--o{ UserAchievements : unlocked_by
    LearningPaths }o--o{ Topics : "thuộc (tùy chọn)"
    LearningPaths ||--o{ LearningPathNodes : has
    LearningPathNodes ||--o{ Lessons : "node bài học (tùy chọn)"
    LearningPathNodes ||--o{ Exercises : "stages (FinalTestId/NodeId)"
    Users ||--o{ NodeSessions : "mở phiên (vào node)"
    LearningPathNodes ||--o{ NodeSessions : "theo dõi"
    Users ||--o{ UserNodeProgress : "tiến độ node"
    LearningPathNodes ||--o{ UserNodeProgress : "chấm điểm"

    Users { int Id PK; string Email UK; string PasswordHash; string DisplayName; int Role; bool IsActive; bool IsPrimaryAdmin; bool TwoFactorEnabled; string? AvatarUrl; date? StreakLastProcessed; datetime CreatedAt; datetime? UpdatedAt; datetime? DeletedAt }
    RefreshTokens { int Id PK; int UserId FK; string TokenHash UK; datetime ExpiresAt; datetime? RevokedAt; string? CreatedByIp; datetime CreatedAt }
    PasswordResetTokens { int Id PK; int UserId FK; string TokenHash UK; datetime ExpiresAt; bool Used; datetime CreatedAt }
    Topics { int Id PK; int? ParentId FK; string Name; string Description; int SortOrder; int CreatedBy FK; datetime CreatedAt; datetime? UpdatedAt; datetime? DeletedAt }
    Lessons { int Id PK; int TopicId FK; string Title; string Description; string ContentHtml; int SortOrder; int Status; int CreatedBy FK; int? UpdatedBy; datetime CreatedAt; datetime? UpdatedAt; datetime? DeletedAt }
    LessonSimulations { int Id PK; int LessonId FK; string SimulationKey; string Title; string? DefaultInputJson; int SortOrder }
    LessonNotes { int Id PK; int UserId FK; int LessonId FK; string ContentHtml; datetime UpdatedAt }
    Exercises { int Id PK; int LessonId FK; int? NodeId FK; int? Stage; string? ConfigJson; string Title; string Description; int Type; int? DurationMinutes; int MaxScore; int Status; int CreatedBy FK; datetime CreatedAt; datetime? UpdatedAt; datetime? DeletedAt }
    Questions { int Id PK; int ExerciseId FK; string Content; string OptionsJson; string AnswerJson; string? Explanation; string? Hint1; string? Hint2; string? Hint3; string? WrongExplanationsJson; bool KeepOrder; int Points; int SortOrder }
    ExerciseSubmissions { int Id PK; int UserId FK; int ExerciseId FK; int? ClassAssignmentId FK NULL; int Score; string AnswersJson; string ResultJson; datetime SubmittedAt; int? DurationSeconds }
    UserProgress { int Id PK; int UserId FK; int LessonId FK; bool Viewed; int SimulationCount; int? BestScore; datetime? CompletedAt; datetime UpdatedAt }
    Favorites { int Id PK; int UserId FK; string SimulationKey; string? InputJson; datetime CreatedAt }
    Settings { int Id PK; string Key UK; string Value; string Description; datetime UpdatedAt; int UpdatedBy }
    Classes { int Id PK; string Name; string InviteCode UK; string? Semester; string? Description; int OwnerId FK; int Status; datetime CreatedAt; datetime? DeletedAt }
    ClassMembers { int Id PK; int ClassId FK; int UserId FK; datetime JoinedAt }
    ClassAssignments { int Id PK; int ClassId FK; int? LessonId FK; int? ExerciseId FK; datetime? DueAt; datetime CreatedAt }
    Achievements { int Id PK; string Code UK; string Name; string Description; string? IconUrl; string ConditionJson; int SortOrder }
    UserAchievements { int Id PK; int UserId FK; int AchievementId FK; datetime EarnedAt }
    ContentFeedback { int Id PK; int UserId FK; int LessonId FK; int Rating; string? Comment; datetime CreatedAt; datetime? UpdatedAt }
    BugReports { int Id PK; int? UserId FK; string Description; string? ContextJson; int Status; int? AssigneeId FK; datetime CreatedAt; datetime? ResolvedAt }
    LearningPaths { int Id PK; string Title; string? Description; int? TopicId FK; int SortOrder; bool IsActive; int CreatedBy FK }
    LearningPathNodes { int Id PK; int PathId FK; string Title; int? LessonId FK; int SortOrder; int? FinalTestId FK }
    NodeSessions { int Id PK; int UserId FK; int NodeId FK; datetime StartedAt; datetime ExpiresAt; int? Stage; int? StepIndex }
    UserNodeProgress { int Id PK; int UserId FK; int NodeId FK; int Status; int Stars; int NodeScore; datetime? UnlockedAt; datetime? PassedAt; datetime UpdatedAt }
```

![Hình 4.13 - ERD tổng quan](diagrams/05-erd-tong-quan.png)
*Hình 4.13: ERD tổng quan lõi học tập 24 bảng. (ảnh placeholder — chụp thật thay sau)*

(b) ERD gamification/code (8 bảng + Users tham chiếu):

```mermaid
erDiagram
    Users ||--o{ UserQuests : completes
    Users ||--o{ UserInventory : owns
    Users ||--o{ GemTransactions : transacts
    Users ||--o{ PremiumSubscriptions : subscribes
    Users ||--o{ CodeRuns : runs
    Users ||--o{ CodeSubmissions : submits
    DailyQuests ||--o{ UserQuests : has
    ShopItems ||--o{ UserInventory : purchased
    Exercises ||--o{ CodeRuns : "chạy thử (tùy chọn)"
    Exercises ||--o{ CodeSubmissions : "chấm điểm"

    Users { int Id PK; string Email UK; string PasswordHash; string DisplayName; int Role; bool IsActive; bool IsPrimaryAdmin; bool TwoFactorEnabled; string? AvatarUrl; int Hearts; int HeartsMax; datetime LastHeartAt; int Gems; int Xp; int StreakDays; int StreakFreeze; date? StreakLastProcessed; datetime? PremiumUntil; date? LastActivityDate; datetime CreatedAt; datetime? UpdatedAt; datetime? DeletedAt }
    DailyQuests { int Id PK; string QuestKey UK; string Title; int Type; string ConditionJson; string RewardJson; bool PoolEnabled }
    UserQuests { int Id PK; int UserId FK; int QuestId FK; date QuestDate; int Progress; bool Claimed }
    ShopItems { int Id PK; string ItemKey UK; string Name; int PriceGems; int MaxStack; int Type; int? DurationHours }
    UserInventory { int Id PK; int UserId FK; int ItemId FK; int Quantity; datetime PurchasedAt; datetime? ExpiresAt }
    GemTransactions { int Id PK; int UserId FK; int Type; int Amount; string? RefType; string? RefId; datetime CreatedAt }
    PremiumSubscriptions { int Id PK; int UserId FK; string? PlanId; datetime StartedAt; datetime? ExpiresAt; int Status; string? OrderRef; datetime CreatedAt }
    CodeRuns { int Id PK; int UserId FK; int? ExerciseId FK; string Code; string InputJson; int Status; string? OutputJson; string? ErrorJson; string? TraceJson; int DurationMs; datetime CreatedAt }
    CodeSubmissions { int Id PK; int UserId FK; int ExerciseId FK; string Code; int Score; int PassedTests; int TotalTests; string ResultJson; datetime SubmittedAt }
```

![Hình 4.14 - ERD chi tiết](diagrams/06-erd-chi-tiet.png)
*Hình 4.14: ERD chi tiết nhóm gamification và code runner 8 bảng. (ảnh placeholder — chụp thật thay sau)*

Hai nhóm bảng được tách để dễ đọc: nhóm lõi phục vụ nội dung học và tiến độ, nhóm gamification phục vụ động lực học (tim, đá quý, nhiệm vụ, bảng xếp hạng) và lịch sử chấm code. Bảng giao dịch như GemTransactions và CodeRuns chỉ ghi thêm, không sửa xóa, phục vụ đối soát sau này.

### 4.3.2 Chi tiết thực thể (Data Dictionary)

Quy ước chung: mọi bảng có cột `Id int` làm khóa chính tự tăng; kiểu ngày giờ dùng datetime2; các bảng nội dung có CreatedAt; xóa dùng xóa mềm qua cột DeletedAt. Phần này liệt kê đủ 32 bảng, chia 6 nhóm, chỉ mô tả các cột quan trọng nhất.

**Bảng 4.5: Bảng Users**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh tài khoản, tự tăng. |
| Email | nvarchar(256) | UK | Có | Email đăng nhập, chuẩn hóa viết thường. |
| PasswordHash | nvarchar(256) | — | Có | Mật khẩu đã băm, không lưu bản rõ. |
| DisplayName | nvarchar(100) | — | Có | Tên hiển thị trên màn hình. |
| Role | int | — | Có | Vai trò: 0 sinh viên, 1 giảng viên, 2 chờ duyệt, 3 quản trị. |
| IsActive | bit | — | Có | Tài khoản đang hoạt động hay bị khóa. |
| IsPrimaryAdmin | bit | — | Có | Đánh dấu quản trị chính duy nhất của hệ thống. |
| TwoFactorEnabled | bit | — | Có | Bật xác thực 2 lớp qua email hay không. |
| AvatarUrl | nvarchar(500) | — | Không | Đường dẫn ảnh đại diện sau khi tải lên. |
| Hearts | int | — | Có | Số tim còn lại để vào node luyện tập. |
| HeartsMax | int | — | Có | Trần tim: bản thường 10, Premium 30. |
| LastHeartAt | datetime2 | — | Có | Thời điểm tim bắt đầu hồi, tính lại khi đọc. |
| Gems | int | — | Có | Số đá quý dùng mua vật phẩm trong shop. |
| Xp | int | — | Có | Tổng kinh nghiệm tích lũy, quy ra cấp độ. |
| StreakDays | int | — | Có | Số ngày học liên tục liền mạch. |
| StreakFreeze | int | — | Có | Số ngày đóng băng chuỗi còn dùng, tối đa 2. |
| PremiumUntil | datetime2 | — | Không | Hạn cuối gói Premium, hết hạn tự hạ cấp. |
| CreatedAt/UpdatedAt/DeletedAt | datetime2 | — | Có/Không | Thời gian tạo, cập nhật và đánh dấu xóa mềm. |

**Bảng 4.6: Bảng RefreshTokens**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh phiên làm mới. |
| UserId | int | FK | Có | Tài khoản sở hữu phiên. |
| TokenHash | nvarchar(64) | UK | Có | Mã băm của token thô, không lưu token gốc. |
| PreviousTokenHash | nvarchar(64) | — | Không | Token bị thay bởi token này khi xoay vòng. |
| ExpiresAt | datetime2 | — | Có | Hạn dùng 7 ngày của phiên. |
| RevokedAt | datetime2 | — | Không | Thời điểm thu hồi khi đăng xuất hoặc đổi mật khẩu. |
| CreatedAt | datetime2 | — | Có | Thời điểm tạo phiên. |

**Bảng 4.7: Bảng PasswordResetTokens**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh mã đặt lại. |
| UserId | int | FK | Có | Tài khoản yêu cầu đặt lại mật khẩu. |
| TokenHash | nvarchar(64) | UK | Có | Mã băm của token khôi phục. |
| ExpiresAt | datetime2 | — | Có | Hạn dùng 30 phút của mã. |
| Used | bit | — | Có | Đã dùng hay chưa, mỗi mã dùng một lần. |
| CreatedAt | datetime2 | — | Có | Thời điểm tạo mã. |

**Bảng 4.8: Bảng Settings**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh cấu hình. |
| Key | nvarchar(100) | UK | Có | Tên cấu hình, ví dụ site.name, auth.maxLoginAttempts. |
| Value | nvarchar(500) | — | Có | Giá trị của cấu hình. |
| Description | nvarchar(500) | — | Không | Ghi chú cấu hình này dùng để làm gì. |
| UpdatedAt | datetime2 | — | Có | Thời điểm sửa gần nhất. |
| UpdatedBy | int | FK | Có | Người sửa cấu hình. |

**Bảng 4.9: Bảng Topics**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh chủ đề. |
| ParentId | int | FK | Không | Chủ đề cha, rỗng với chủ đề cấp 1. |
| Name | nvarchar(100) | — | Có | Tên chủ đề, duy nhất trong cùng cha. |
| Description | nvarchar(500) | — | Không | Mô tả ngắn nội dung chủ đề. |
| SortOrder | int | — | Có | Thứ tự hiển thị trong danh sách. |
| CreatedBy | int | FK | Có | Tài khoản tạo chủ đề. |
| CreatedAt/UpdatedAt/DeletedAt | datetime2 | — | Có/Không | Thời gian tạo, sửa, xóa mềm. |

**Bảng 4.10: Bảng Lessons**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh bài học. |
| TopicId | int | FK | Có | Chủ đề chứa bài học. |
| Title | nvarchar(200) | — | Có | Tiêu đề bài học. |
| Description | nvarchar(500) | — | Không | Mô tả ngắn bài học. |
| ContentHtml | nvarchar(max) | — | Có | Nội dung lý thuyết đã làm sạch mã HTML. |
| SortOrder | int | — | Có | Thứ tự bài trong chủ đề. |
| Status | int | — | Có | Trạng thái: 0 nháp, 1 công khai, 2 ẩn. |
| CreatedBy | int | FK | Có | Giảng viên tạo bài, giữ quyền sở hữu. |
| UpdatedBy | int | FK | Không | Người sửa bài gần nhất. |
| CreatedAt/UpdatedAt/DeletedAt | datetime2 | — | Có/Không | Thời gian tạo, sửa, xóa mềm. |

**Bảng 4.11: Bảng LessonSimulations**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh liên kết. |
| LessonId | int | FK | Có | Bài học chứa mô phỏng. |
| SimulationKey | nvarchar(100) | — | Có | Mã mô phỏng như sort.bubble, duy nhất trong bài. |
| Title | nvarchar(200) | — | Có | Tên hiển thị của mô phỏng trong bài. |
| DefaultInputJson | nvarchar(max) | — | Không | Bộ dữ liệu mẫu mặc định khi mở mô phỏng. |
| SortOrder | int | — | Có | Thứ tự thẻ mô phỏng trong bài. |

**Bảng 4.12: Bảng LessonNotes**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh ghi chú. |
| UserId | int | FK | Có | Người học viết ghi chú. |
| LessonId | int | FK | Có | Bài học được ghi chú, mỗi người một ghi chú mỗi bài. |
| ContentHtml | nvarchar(max) | — | Có | Nội dung ghi chú đã làm sạch. |
| UpdatedAt | datetime2 | — | Có | Thời điểm tự lưu gần nhất. |

**Bảng 4.13: Bảng Exercises**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh bài tập. |
| LessonId | int | FK | Có | Bài học chứa bài tập. |
| NodeId | int | FK | Không | Node luyện tập sở hữu bài tập (bậc 1/2/3). |
| Stage | int | — | Không | Bậc trong Ladder: 1 quiz, 2 lab, 3 code. |
| ConfigJson | nvarchar(max) | — | Không | Cấu hình lab/code: chữ ký hàm, test ẩn. |
| Title | nvarchar(200) | — | Có | Tiêu đề bài tập. |
| Description | nvarchar(500) | — | Không | Hướng dẫn làm bài. |
| Type | int | — | Có | Loại: 0 trắc nghiệm, 1 dự đoán bước, 2 lab, 3 code. |
| DurationMinutes | int | — | Không | Giới hạn thời gian, rỗng là không giới hạn. |
| MaxScore | int | — | Có | Tổng điểm tối đa, tính lại khi lưu. |
| Status | int | — | Có | Trạng thái: 0 nháp, 1 công khai. |
| CreatedBy | int | FK | Có | Người soạn bài tập. |
| DeletedAt | datetime2 | — | Không | Đánh dấu xóa mềm. |

**Bảng 4.14: Bảng Questions**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh câu hỏi. |
| ExerciseId | int | FK | Có | Bài tập chứa câu hỏi. |
| Content | nvarchar(max) | — | Có | Nội dung câu hỏi dạng Markdown. |
| OptionsJson | nvarchar(max) | — | Có | Danh sách phương án A, B, C, D. |
| AnswerJson | nvarchar(max) | — | Có | Đáp án đúng theo loại câu hỏi. |
| Explanation | nvarchar(max) | — | Không | Giải thích hiển thị sau khi nộp bài. |
| Hint1..Hint3 | nvarchar(500) | — | Không | Ba mức gợi ý, tốn token khi xem. |
| KeepOrder | bit | — | Có | Giữ nguyên thứ tự phương án, không xáo trộn. |
| Points | int | — | Có | Điểm của câu, từ 1 đến 10. |
| SortOrder | int | — | Có | Thứ tự câu trong bài. |

**Bảng 4.15: Bảng Favorites**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh mục yêu thích. |
| UserId | int | FK | Có | Người dùng lưu mô phỏng. |
| SimulationKey | nvarchar(100) | — | Có | Mã mô phỏng được lưu, mỗi người một lần. |
| InputJson | nvarchar(max) | — | Không | Bộ dữ liệu đã cấu hình lúc lưu. |
| CreatedAt | datetime2 | — | Có | Thời điểm thêm yêu thích. |
**Bảng 4.16: Bảng UserProgress**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh dòng tiến độ. |
| UserId | int | FK | Có | Người học. |
| LessonId | int | FK | Có | Bài học, mỗi người một dòng mỗi bài. |
| Viewed | bit | — | Có | Đã mở xem bài học hay chưa. |
| SimulationCount | int | — | Có | Số lần chạy mô phỏng của bài. |
| BestScore | int | — | Không | Điểm cao nhất đạt được. |
| CompletedAt | datetime2 | — | Không | Thời điểm xem xong và có điểm. |
| UpdatedAt | datetime2 | — | Có | Thời điểm cập nhật gần nhất. |

**Bảng 4.17: Bảng UserNodeProgress**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh dòng tiến độ node. |
| UserId | int | FK | Có | Người học. |
| NodeId | int | FK | Có | Node trên lộ trình, mỗi người một dòng mỗi node. |
| Status | int | — | Có | Trạng thái: 0 khóa, 1 mở, 2 đã qua. |
| Stars | int | — | Có | Số sao đạt được từ 1 đến 3. |
| NodeScore | int | — | Có | Điểm node, giữ giá trị cao nhất. |
| UnlockedAt | datetime2 | — | Không | Thời điểm node được mở khóa. |
| PassedAt | datetime2 | — | Không | Thời điểm qua cả 3 bậc. |
| UpdatedAt | datetime2 | — | Có | Thời điểm cập nhật gần nhất. |

**Bảng 4.18: Bảng NodeSessions**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh phiên. |
| UserId | int | FK | Có | Người học, mỗi người một phiên mỗi node. |
| NodeId | int | FK | Có | Node đang học. |
| StartedAt | datetime2 | — | Có | Thời điểm vào node, dùng giờ máy chủ. |
| ExpiresAt | datetime2 | — | Có | Hạn phiên 30 phút, hết hạn vào lại phải trừ tim. |
| Stage | int | — | Không | Bậc đang dở: 1 quiz, 2 lab, 3 code. |
| StepIndex | int | — | Không | Bước mô phỏng đang dở để học tiếp. |

**Bảng 4.19: Bảng ExerciseSubmissions**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh bài nộp. |
| UserId | int | FK | Có | Người làm bài. |
| ExerciseId | int | FK | Có | Bài tập được làm. |
| ClassAssignmentId | int | FK | Không | Bài giao qua lớp nếu nộp theo lớp. |
| Score | int | — | Có | Điểm đạt được. |
| AnswersJson | nvarchar(max) | — | Có | Câu trả lời đã chọn. |
| ResultJson | nvarchar(max) | — | Có | Kết quả chi tiết để tái hiện màn kết quả. |
| DurationSeconds | int | — | Không | Thời gian làm bài. |
| SubmittedAt | datetime2 | — | Có | Thời điểm nộp bài. |

**Bảng 4.20: Bảng LearningPaths**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh lộ trình. |
| Title | nvarchar(200) | — | Có | Tên lộ trình. |
| Description | nvarchar(500) | — | Không | Mô tả lộ trình. |
| TopicId | int | FK | Không | Chủ đề gắn với lộ trình, tùy chọn. |
| SortOrder | int | — | Có | Thứ tự lộ trình, mở khóa tuần tự. |
| IsActive | bit | — | Có | Lộ trình còn hiển thị hay không. |
| CreatedBy | int | FK | Có | Người tạo lộ trình. |

**Bảng 4.21: Bảng LearningPathNodes**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh node. |
| PathId | int | FK | Có | Lộ trình chứa node. |
| Title | nvarchar(200) | — | Có | Tên node hiển thị trên bản đồ. |
| LessonId | int | FK | Không | Bài học của node, rỗng với node luyện tập tổng hợp. |
| SortOrder | int | — | Có | Thứ tự node, duy nhất trong lộ trình. |
| FinalTestId | int | FK | Không | Bài kiểm tra cuối nếu node là final test. |

**Bảng 4.22: Bảng ContentFeedback**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh đánh giá. |
| UserId | int | FK | Có | Người đánh giá. |
| LessonId | int | FK | Có | Bài được đánh giá, mỗi người một lần mỗi bài. |
| Rating | int | — | Có | Số sao từ 1 đến 5. |
| Comment | nvarchar(200) | — | Không | Nhận xét ngắn, tối đa 200 ký tự. |
| CreatedAt/UpdatedAt | datetime2 | — | Có | Thời điểm gửi và sửa đánh giá. |

**Bảng 4.23: Bảng BugReports**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh báo lỗi. |
| UserId | int | FK | Không | Người báo lỗi, rỗng với khách. |
| Description | nvarchar(2000) | — | Có | Mô tả sự cố. |
| ContextJson | nvarchar(max) | — | Không | Bối cảnh: đường dẫn, trình duyệt, bước mô phỏng. |
| Status | int | — | Có | Trạng thái: 0 mới, 1 đang xử lý, 2 đã xử lý, 3 đóng. |
| AssigneeId | int | FK | Không | Người phụ trách xử lý. |
| CreatedAt/ResolvedAt | datetime2 | — | Có/Không | Thời điểm tạo và giải quyết. |

**Bảng 4.24: Bảng Classes**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh lớp. |
| Name | nvarchar(200) | — | Có | Tên lớp. |
| InviteCode | nvarchar(6) | UK | Có | Mã mời 6 ký tự chữ hoa và số. |
| Semester | nvarchar(50) | — | Không | Học kỳ của lớp. |
| Description | nvarchar(500) | — | Không | Mô tả lớp. |
| OwnerId | int | FK | Có | Giảng viên sở hữu lớp. |
| Status | int | — | Có | Trạng thái: 0 mở, 1 đóng. |
| CreatedAt/DeletedAt | datetime2 | — | Có/Không | Thời gian tạo và xóa mềm. |

**Bảng 4.25: Bảng ClassMembers**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh thành viên. |
| ClassId | int | FK | Có | Lớp tham gia. |
| UserId | int | FK | Có | Sinh viên, mỗi người một dòng mỗi lớp. |
| JoinedAt | datetime2 | — | Có | Thời điểm vào lớp. |

**Bảng 4.26: Bảng ClassAssignments**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh bài giao. |
| ClassId | int | FK | Có | Lớp được giao. |
| LessonId | int | FK | Không | Bài học được giao, bắt buộc có bài học hoặc bài tập. |
| ExerciseId | int | FK | Không | Bài tập được giao. |
| DueAt | datetime2 | — | Không | Hạn nộp, quá hạn không nộp được. |
| CreatedAt | datetime2 | — | Có | Thời điểm giao bài. |
**Bảng 4.27: Bảng DailyQuests**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh mẫu nhiệm vụ. |
| QuestKey | nvarchar(100) | UK | Có | Mã nhiệm vụ như learn-1-node. |
| Title | nvarchar(200) | — | Có | Tên nhiệm vụ hiển thị. |
| Type | int | — | Có | Mức khó: 0 dễ, 1 trung bình, 2 khó. |
| ConditionJson | nvarchar(max) | — | Có | Điều kiện hoàn thành, ví dụ qua 1 node. |
| RewardJson | nvarchar(max) | — | Có | Phần thưởng khi hoàn thành. |
| PoolEnabled | bit | — | Có | Mẫu còn nằm trong danh sách chọn hay không. |

**Bảng 4.28: Bảng UserQuests**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh nhiệm vụ cá nhân. |
| UserId | int | FK | Có | Người nhận nhiệm vụ. |
| QuestId | int | FK | Có | Mẫu nhiệm vụ được giao. |
| QuestDate | date | — | Có | Ngày của nhiệm vụ, reset lúc 00:00. |
| Progress | int | — | Có | Số tiến độ đã đạt. |
| Claimed | bit | — | Có | Đã nhận thưởng hay chưa. |

**Bảng 4.29: Bảng ShopItems**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh vật phẩm. |
| ItemKey | nvarchar(100) | UK | Có | Mã vật phẩm như hint-token, streak-freeze. |
| Name | nvarchar(200) | — | Có | Tên vật phẩm. |
| PriceGems | int | — | Có | Giá bằng đá quý. |
| MaxStack | int | — | Có | Số lượng tối đa sở hữu. |
| Type | int | — | Có | Loại: 0 dùng một lần, 1 vĩnh viễn, 2 có hạn. |
| DurationHours | int | — | Không | Số giờ hiệu lực với vật phẩm có hạn. |

**Bảng 4.30: Bảng UserInventory**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh dòng kho. |
| UserId | int | FK | Có | Người sở hữu. |
| ItemId | int | FK | Có | Vật phẩm sở hữu, mỗi loại một dòng. |
| Quantity | int | — | Có | Số lượng đang có. |
| IsEquipped | bit | — | Có | Đang trang bị hay không, cùng loại chỉ một cái. |
| PurchasedAt | datetime2 | — | Có | Thời điểm mua. |
| ExpiresAt | datetime2 | — | Không | Hạn dùng với vật phẩm có hạn. |

**Bảng 4.31: Bảng GemTransactions**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh giao dịch. |
| UserId | int | FK | Có | Người thực hiện giao dịch. |
| Type | int | — | Có | Loại: 0 nhận, 1 chi tiêu. |
| Amount | int | — | Có | Số lượng, luôn dương. |
| RefType | nvarchar(50) | — | Không | Lý do: qua node, mua shop, nhiệm vụ. |
| RefId | int | — | Không | Đối tượng liên quan tới giao dịch. |
| CreatedAt | datetime2 | — | Có | Thời điểm giao dịch, chỉ ghi thêm không sửa. |

**Bảng 4.32: Bảng PremiumSubscriptions**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh gói. |
| UserId | int | FK | Có | Người đăng ký. |
| PlanId | nvarchar(50) | — | Có | Gói 1, 3 hay 12 tháng. |
| StartedAt | datetime2 | — | Có | Thời điểm bắt đầu hiệu lực. |
| ExpiresAt | datetime2 | — | Không | Thời điểm hết hạn, hết hạn tự hạ cấp. |
| Status | int | — | Có | Trạng thái: 0 hoạt động, 1 hết hạn, 2 thanh toán thử. |
| OrderRef | nvarchar(100) | — | Không | Mã tham chiếu đơn hàng. |
| CreatedAt | datetime2 | — | Có | Thời điểm tạo gói. |

**Bảng 4.33: Bảng Achievements**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh huy hiệu. |
| Code | nvarchar(100) | UK | Có | Mã huy hiệu như streak-7, sort-master. |
| Name | nvarchar(200) | — | Có | Tên huy hiệu. |
| Description | nvarchar(500) | — | Có | Mô tả điều kiện để người học hiểu. |
| ConditionJson | nvarchar(max) | — | Có | Điều kiện trao: đếm số lần, chuỗi ngày, điểm số. |
| SortOrder | int | — | Có | Thứ tự hiển thị. |

**Bảng 4.34: Bảng UserAchievements**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh huy hiệu đã trao. |
| UserId | int | FK | Có | Người nhận huy hiệu. |
| AchievementId | int | FK | Có | Huy hiệu được trao, mỗi người một lần mỗi loại. |
| EarnedAt | datetime2 | — | Có | Thời điểm nhận huy hiệu. |

**Bảng 4.35: Bảng CodeRuns**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh lần chạy. |
| UserId | int | FK | Có | Người chạy code. |
| ExerciseId | int | FK | Không | Bài tập code liên quan nếu có. |
| Code | nvarchar(max) | — | Có | Mã nguồn đã chạy. |
| InputJson | nvarchar(max) | — | Có | Dữ liệu đầu vào của lần chạy. |
| Status | int | — | Có | Trạng thái: 0 chờ, 1 đang chạy, 2 thành công, 3 lỗi, 4 quá giờ. |
| OutputJson | nvarchar(max) | — | Không | Kết quả đầu ra. |
| ErrorJson | nvarchar(max) | — | Không | Thông báo lỗi nếu có. |
| TraceJson | nvarchar(max) | — | Không | Trace nén để tái hiện màn trực quan. |
| DurationMs | int | — | Có | Thời gian chạy tính bằng mili giây. |
| CreatedAt | datetime2 | — | Có | Thời điểm chạy, dọn sau 30 ngày. |

**Bảng 4.36: Bảng CodeSubmissions**

| Tên cột | Kiểu dữ liệu | Khóa | Bắt buộc | Mô tả chi tiết |
|---|---|---|---|---|
| Id | int | PK | Có | Số định danh bài nộp. |
| UserId | int | FK | Có | Người nộp bài. |
| ExerciseId | int | FK | Có | Bài tập code được nộp. |
| Code | nvarchar(max) | — | Có | Mã nguồn nộp. |
| Score | int | — | Có | Điểm đạt được. |
| PassedTests | int | — | Có | Số test ẩn vượt qua. |
| TotalTests | int | — | Có | Tổng số test ẩn. |
| ResultJson | nvarchar(max) | — | Có | Chi tiết kết quả từng test. |
| SubmittedAt | datetime2 | — | Có | Thời điểm nộp bài. |
## 4.4 Thiết kế phần mềm

### 4.4.1 Kiến trúc backend 2 lớp

Hệ thống backend được thiết kế theo mô hình kiến trúc hai tầng tinh gọn: tầng API tiếp nhận và điều phối yêu cầu, tầng Dịch vụ (Service) đảm nhiệm toàn bộ quy trình nghiệp vụ và truy vấn cơ sở dữ liệu. Thiết kế này giúp tối ưu hóa hiệu năng, giảm thiểu độ phức tạp trung gian và tạo thuận lợi cho công tác bảo trì, kiểm thử.

```mermaid
sequenceDiagram
    participant C as Controller
    participant V as Validator
    participant S as Service
    participant DB as DbContext
    C->>V: Kiểm tra dữ liệu đầu vào
    V-->>C: Phản hồi nếu không hợp lệ
    C->>S: Yêu cầu xử lý nghiệp vụ
    S->>DB: Truy vấn dữ liệu
    DB-->>S: Trả về kết quả
    S-->>C: Phản hồi dữ liệu chuẩn hóa
```

### 4.4.2 Simulation Engine EDV

Bộ máy mô phỏng (Simulation Engine) là thành phần cốt lõi của hệ thống, thực hiện tính toán từng bước chuyển động của giải thuật dựa trên mã nguồn thực tế. Mỗi bước mô phỏng lưu trữ trạng thái cấu trúc dữ liệu, các phần tử cần làm nổi bật và lời giải thích tương ứng bằng tiếng Việt. Quá trình tính toán diễn ra trực tiếp phía trình duyệt người dùng, đảm bảo hình ảnh hiển thị mượt mà và hỗ trợ tính năng xem lại các bước trước đó mà không tạo tải lên máy chủ.

```mermaid
classDiagram
    class Simulation {
        +string key
        +string title
        +InputConfig input
        +Step[] steps
        +string[] pseudocode
        +Statistics stats
        +generate() Step[]
    }
    class Step {
        +int index
        +Structure structure
        +string explanation
        +int pseudocodeLine
        +string[] highlights
        +Variables variables
        +Statistics stats
    }
    class Structure {
        +string kind
        +Element[] elements
        +Link[] links
    }
    class SimulationGenerator {
        <<interface>>
        +string key
        +string title
        +Step[] generate(InputConfig input)
    }
    Simulation "1" *-- "*" Step
    Step "1" *-- "1" Structure
    SimulationGenerator ..> Simulation : khởi tạo
```

Mỗi bước mô phỏng chứa trạng thái cấu trúc dữ liệu bất biến, danh sách phần tử tô màu và bộ đếm thống kê so sánh/hoán đổi, giúp việc bổ sung thuật toán mới diễn ra thuận tiện mà không cần thay đổi cấu trúc lõi.

### 4.4.3 Máy trạng thái mô phỏng

Trình điều khiển mô phỏng hoạt động theo một máy trạng thái tập trung nhằm đảm bảo sự đồng bộ giữa các nút bấm và phím tắt điều khiển:

```mermaid
stateDiagram-v2
    [*] --> idle: Tải thuật toán
    idle --> running: Bắt đầu phát
    idle --> finished: Nhảy đến bước cuối
    running --> paused: Tạm dừng
    running --> finished: Đạt bước cuối
    paused --> running: Tiếp tục phát
    paused --> idle: Đặt lại về đầu
    finished --> idle: Đặt lại về đầu
    finished --> running: Chạy lại từ đầu
```

Trình điều khiển gồm 4 trạng thái: Khởi tạo (Idle) khi mới nạp dữ liệu; Đang chạy (Running) chuyển bước tự động theo tốc độ; Tạm dừng (Paused) giữ nguyên bước hiện tại; Hoàn tất (Finished) khi kết thúc thuật toán. Người học có thể tùy ý điều khiển tiến trình hoặc đặt lại trạng thái ban đầu bất cứ lúc nào.

# PHẦN 5: THỰC HIỆN – IMPLEMENT

## 5.1 Cơ sở dữ liệu

Hệ thống dùng phương pháp **Code-First**: mọi bảng được khai báo bằng entity C# trong `DsaVisual.Application/Persistence`, cấu hình quan hệ bằng Fluent API trong thư mục `Configurations/` (không dùng attribute trên entity), thay đổi cấu trúc thực hiện qua EF Core Migrations — đây là cách duy nhất đổi schema, không sửa DB trực tiếp. Entity sau được trích từ bảng `NodeSessions` (phiên học 30 phút — cốt lõi của cơ chế trừ tim):

```csharp
// DsaVisual.Application/Persistence/Entities/NodeSession.cs (trích)
public class NodeSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int NodeId { get; set; }
    public DateTime StartedAt { get; set; }   // server clock — chống chỉnh đồng hồ
    public DateTime ExpiresAt { get; set; }   // StartedAt + 30 phút
    public int? Stage { get; set; }           // bậc đang dở: 1=Quiz, 2=Lab, 3=Code
    public int? StepIndex { get; set; }       // bước mô phỏng đang dở (resume)
}
```

Các điểm cấu hình Fluent API quan trọng:

- Unique index `(UserId, NodeId)` trên `NodeSessions` — tuần tự hóa 2 request vào node song song, chỉ 1 lần trừ tim (chống double-spend, FR-10.1).
- Unique index `Email` trên `Users` — ràng buộc đăng nhập; unique `(UserId, LessonId)` trên `UserProgress` để upsert tiến độ không nhân đôi bản ghi.
- Khóa ngoại cascade cho bảng con theo cha (`Questions.ExerciseId`, `RefreshTokens.UserId`) — xóa cha sẽ gom con.
- Xóa mềm bằng cột `DeletedAt datetime2 NULL` trên các bảng nội dung; toàn bộ tên bảng/cột viết PascalCase (quy ước chung 32 bảng).

Các migration/seed quan trọng:

- Migration `AddNodeSessions` — thêm bảng `NodeSessions` + unique index `(UserId, NodeId)` cho FR-10.1.
- Migration `AddUserNodeProgress` — thêm bảng `UserNodeProgress` chuẩn hóa tiến độ node, thay cho tính runtime từ bài nộp.
- Seeder idempotent — 1 Admin (`admin@system.local`, ép đổi mật khẩu lần đầu) + 5 chủ đề gốc + 8 bài học mẫu (mỗi bài gắn 1 mô phỏng EDV + 5-10 câu quiz + 1 lab + 1 code challenge) + 5 Learning Path; mọi code seed phải chạy khớp golden data.
- Script `sync-catalog` — đồng bộ danh mục mô phỏng từ `shared/simulation-catalog.json` sang bảng seed backend (CI so sánh 2 danh sách key, khác là fail build).

## 5.2 Simulation Engine & Sandbox

Bộ máy mô phỏng thực thi thuật toán theo từng bước lệnh dựa trên mã giả chuẩn. Ví dụ với thuật toán Sắp xếp nổi bọt (Bubble Sort):

```text
1.  procedure bubbleSort(a[0..n-1])
2.    for i ← 0 to n-2 do
3.      swapped ← false
4.      for j ← 0 to n-2-i do
5.        if a[j] > a[j+1] then
6.          swap a[j], a[j+1]
7.          swapped ← true
8.      if swapped = false then
9.        return
10.   end procedure
```

Mỗi thao tác so sánh, hoán đổi đều tự động sinh ra một khung hình trực quan tương ứng. Để đảm bảo độ tin cậy, mỗi giải thuật đều được xây dựng kèm bộ dữ liệu kiểm thử chuẩn (Bảng 5.1):

**Bảng 5.1: Bộ dữ liệu kiểm thử chuẩn (Golden Test Data)**

| Nhóm | Đặc điểm | Ví dụ kiểm thử |
|---|---|---|
| N1 | Mảng rỗng hoặc 1 phần tử | `[]`, `[5]` |
| N2 | Mảng đã sắp xếp tăng dần | `[1, 2, 3, 4, 5]` |
| N3 | Sắp xếp giảm dần (Trường hợp xấu nhất) | `[5, 4, 3, 2, 1]` |
| N4 | Dữ liệu có phần tử trùng lặp | `[4, 2, 4, 1, 4]` |
| N5 | Dữ liệu chứa số âm | `[-3, 7, -1, 0, 2]` |
| N6 | Kích thước dữ liệu lớn | Mảng ngẫu nhiên 100 phần tử |
| N7 | Đặc thù giải thuật | Tìm kiếm có/không thấy khóa, đồ thị có chu trình |

Phần thực thi mã nguồn (Code Runner) được xử lý an toàn trong môi trường hộp cát (Sandbox) trên trình duyệt, tự động đối chiếu kết quả đầu ra với các bộ dữ liệu kiểm thử để đánh giá bài nộp.

## 5.3 Sơ đồ kiến trúc công nghệ

Hệ thống được tổ chức phân tách rõ ràng thành hai khối chính:

- **Phía máy khách (Frontend - Vue 3 SPA)**: Chịu trách nhiệm toàn bộ về giao diện người dùng, trình điều khiển mô phỏng trực quan, các bộ diễn họa Canvas và môi trường thực hành tương tác.
- **Phía máy chủ (Backend - ASP.NET Core API)**: Đảm nhiệm xác thực bảo mật, quản lý nội dung học tập, lưu trữ tiến độ cá nhân, chấm điểm tự động và các dịch vụ đào tạo.

Hệ thống kiểm thử tự động được xây dựng đa tầng gồm kiểm thử đơn vị (Unit Tests), kiểm thử tích hợp (Integration Tests) và kiểm thử giao diện đầu cuối (E2E Tests) nhằm đảm bảo hệ thống vận hành ổn định và tin cậy.

## 5.4 Các loại sơ đồ tương tác

### 5.4.1 Sequence Diagram

**(a) UC-01 — Chạy mô phỏng giải thuật** (giữ nguyên từ SRS):

```mermaid
sequenceDiagram
    participant H as Người học
    participant SPA as Frontend (Vue 3)
    participant API as Backend API
    participant DB as Database

    H->>SPA: bấm mở mô phỏng (từ node/bài học)
    SPA->>API: POST /learning-path/nodes/{nodeId}/enter (nếu từ path)
    API->>DB: transaction trừ tim + tạo/resume NodeSession (FR-10.1)
    API-->>SPA: { session, heartsLeft } hoặc 403 HEARTS_EMPTY
    SPA->>SPA: loadSim(key, input) → StepExecutor chạy code thật → TraceEvent[]
    SPA->>SPA: render bước 0 (Canvas + mã giả + giải thích)
    Loop chuyển bước theo tốc độ
        SPA->>SPA: play() → stepForward() theo interval (1200/speed ms)
        SPA->>SPA: cập nhật đồng bộ 3 vùng + bộ đếm
    End
    H->>SPA: nhấn nút điều khiển (pause/step/seek/speed)
    SPA->>SPA: state machine chuyển trạng thái (idle/running/paused/finished)
```

*Hình 5.1: Sequence UC-01 — vào node trừ tim trước, sau đó toàn bộ mô phỏng sinh và phát lại ở phía frontend.*

**(b) UC-25 — Học theo Learning Path và mở khóa node.** SRS chỉ đặc tả sequence diagram cho UC-01, UC-03, UC-04, UC-06, UC-09 — UC-25 không có sơ đồ riêng, nên thay bằng sequence **UC-06 (Nộp bài tập trắc nghiệm)** — quy trình chấm điểm liên quan trực tiếp đến Practice Ladder; cơ chế trừ tim atomic của UC-25 được mô tả bằng lời ở mục 5.5.2:

```mermaid
sequenceDiagram
    participant H as Người học
    participant SPA as Frontend
    participant API as Backend
    participant DB as Database
    H->>SPA: trả lời câu hỏi (chọn đáp án)
    SPA->>SPA: lưu trạng thái cục bộ (có thể sửa)
    H->>SPA: bấm "Nộp bài"
    SPA->>API: POST /exercises/{id}/submit {answers}
    API->>API: validate đáp án + khóa chống nộp trùng (422)
    API->>DB: đọc AnswerJson từng câu
    API->>API: chấm điểm theo loại (SINGLE/MULTI/BOOLEAN)
    API->>DB: lưu ExerciseSubmissions + upsert UserProgress (BestScore)
    API-->>SPA: { score, results[], explanation }
    SPA-->>H: màn kết quả + giải thích
```

*Hình 5.2: Sequence UC-06 — chấm điểm server-side, lưu bài nộp và upsert điểm cao nhất trong một quy trình.*

### 5.4.2 Activity Diagram

**(a) State machine mô phỏng** (giữ nguyên từ SDD §3.5):

```mermaid
stateDiagram-v2
    [*] --> idle: loadSim()
    idle --> running: play()
    idle --> finished: jumpTo(cuối)
    running --> paused: pause()
    running --> finished: đạt bước cuối
    running --> running: stepForward() (tự động)
    paused --> running: play()
    paused --> idle: reset()
    paused --> finished: stepForward() ở bước cuối
    finished --> idle: reset()
    finished --> running: play() (chạy lại từ đầu)
```

*Hình 5.3: State machine của player mô phỏng — mọi chuyển trạng thái phát qua store `simulation` để nút điều khiển và phím tắt phản ứng thống nhất.*

**(b) Luồng Practice Ladder** (dựng theo đặc tả FR-4.11, mỗi node gồm 3 bậc tuần tự):

```mermaid
flowchart TD
    A[Bấm node đang mở trên Learning Path] --> B[Trừ 1 tim atomic + tạo session 30 phút]
    B --> C[Bậc 1 Quiz]
    C -->|điểm >= 60%| D[Bậc 2 Interactive Lab]
    C -->|rớt| C
    D -->|trạng thái cuối khớp chuẩn StepExecutor,<br/>số bước <= chuẩn x 1.5| E[Bậc 3 Code Challenge]
    D -->|rớt| D
    E -->|pass >= 70% test ẩn| F[Pass node — UserNodeProgress cập nhật]
    E -->|rớt| E
    F --> G[Mở khóa node kế / mở final test cuối lộ trình]
```

*Hình 5.4: Luồng Practice Ladder 3 bậc — server guard chặn vào bậc sau khi chưa pass bậc trước; retry trong session 30 phút không trừ tim; điểm node = Quiz 20% + Lab 30% + Code 50% (giữ MAX).*

## 5.5 API Endpoints

### 5.5.1 Controllers

Danh sách endpoint chính theo nhóm (trích từ API_REFERENCE — toàn bộ endpoint nằm dưới gốc `/api/v1`):

**Bảng 5.2: Endpoint chính theo nhóm chức năng**

| Nhóm | Method | Endpoint | Chức năng |
|---|---|---|---|
| Auth | POST | `/auth/login` | Đăng nhập, trả JWT access token + cookie refresh |
| Auth | POST | `/auth/refresh` | Làm mới token (rotate-invalidate) |
| Public | GET | `/public/simulations/{key}/run` | Chạy demo công khai (3 key) |
| Topics | GET / POST | `/topics` | Xem cây chủ đề / tạo chủ đề |
| Lessons | GET | `/lessons` | Danh sách bài học (lọc, phân trang) |
| Lessons | POST | `/lessons/{id}/mark-viewed` | Đánh dấu đã học (upsert UserProgress) |
| Simulations | GET | `/simulations` | Danh mục mô phỏng kèm schema, độ phức tạp |
| Exercises | GET | `/exercises/{id}` | Chi tiết bài tập (KHÔNG trả đáp án) |
| Exercises | POST | `/exercises/{id}/submit` | Nộp bài → điểm + đáp án + giải thích |
| Exercises | POST | `/exercises/{id}/code-submit` | Nộp bài code (chấm client sandbox) |
| Progress | GET | `/progress/me` | Tiến độ tổng hợp cá nhân |
| Progress | GET | `/progress/report` | Báo cáo giảng viên (kèm xuất CSV) |
| Learning Path | POST | `/learning-path/{id}/nodes/{nodeId}/enter` | Trừ 1 tim atomic + tạo/resume session |
| Learning Path | GET | `/learning-path/{id}` | Bản đồ node của lộ trình |
| Classes | POST | `/classes/{id}/join` | Tham gia lớp bằng mã mời 6 ký tự |
| Classes | GET | `/classes/{id}/report` | Báo cáo lớp học phần |
| Code Runner | POST | `/code-runs` | Lưu lần chạy code (trace do client sinh) |
| Gamification | POST | `/me/quests/{id}/claim` | Nhận thưởng quest (atomic) |
| Shop | POST | `/shop/buy` | Mua vật phẩm (trừ gems atomic) |
| Admin | GET | `/admin/stats` | Thống kê hệ thống |

### 5.5.2 Services (Business Logic)

**Bảng 5.3: 12 service và trách nhiệm chính**

| Service | Trách nhiệm chính |
|---|---|
| AuthService | đăng ký, đăng nhập, refresh (rotate-invalidate), logout, khôi phục mật khẩu, khóa tạm |
| UserService | CRUD người dùng, khóa/mở, đổi vai trò, phê duyệt Teacher, ẩn danh hóa |
| TopicService | cây chủ đề, CRUD, reorder, chặn xóa khi có con |
| LessonService | CRUD bài học, sanitize HTML, gắn mô phỏng, đánh dấu đã học, quyền sở hữu |
| SimulationCatalogService | danh mục mô phỏng + schema (đồng bộ key với frontend) |
| ExerciseService | CRUD bài tập/câu hỏi, chấm điểm (SINGLE/MULTI/BOOLEAN/Lab), chống nộp trùng, import CSV |
| ProgressService | upsert tiến độ, dashboard, báo cáo giảng viên + CSV, báo cáo lớp |
| FavoriteService | CRUD yêu thích |
| SettingService | cấu hình hệ thống + cache |
| ClassService | CRUD lớp, mã mời 6 ký tự, thêm/xóa sinh viên, gán nội dung + hạn nộp, báo cáo lớp |
| CodeRunnerService | lưu CodeRuns, lịch sử nộp + so sánh (chấm chạy client sandbox) |
| GamificationService | một điểm vào duy nhất Module J: hearts/session (trừ tim atomic), quest/streak, shop/gems, premium, achievement |

**Quy trình trừ lượt học (Tim):** Mỗi lượt mở bài học hoặc làm bài tập mới sẽ tiêu tốn 1 Tim. Hệ thống quản lý phiên học 30 phút nhằm đảm bảo trải nghiệm học tập liền mạch:
- **Bài học đã hoàn thành:** Người học có thể mở lại để ôn tập kiến thức bất kỳ lúc nào mà không bị trừ Tim.
- **Phiên học đang còn hiệu lực:** Trong khoảng thời gian 30 phút kể từ khi mở bài học, người học có thể tải lại trang hoặc tiếp tục làm bài mà không bị trừ thêm Tim.
- **Bắt đầu phiên học mới:** Hệ thống trừ 1 Tim và khởi tạo phiên học có thời hạn 30 phút.
- **Trường hợp hết Tim:** Hệ thống thông báo và hướng dẫn người học đổi Tim bằng Đá quý hoặc chờ thời gian hồi phục tự nhiên.10.1).** Mọi lượt "vào node" (mở mô phỏng hoặc vào Ladder, trừ node đã pass) trừ đúng 1 tim. Toàn bộ thao tác chạy trong 1 transaction ngắn theo thứ tự bắt buộc: (1) kiểm tra node đã pass → miễn phí, không trừ; (2) thử `UPDATE NodeSessions` gia hạn session hết hạn với điều kiện `ExpiresAt < @now`, kiểm tra `@@ROWCOUNT` — nếu gia hạn được thì sang bước trừ tim; (3) nếu không có dòng nào được gia hạn thì `INSERT` session mới — unique `(UserId, NodeId)` tuần tự hóa, INSERT trùng (session còn hiệu lực, kể cả do request song song tạo) thì resume không trừ; (4) `UPDATE Users SET Hearts = Hearts - 1 WHERE Id = @id AND Hearts > 0` — không có dòng nào bị cập nhật (hết tim) thì rollback toàn bộ và trả 403 `HEARTS_EMPTY`. Nhờ vậy 2 request song song chỉ trừ 1 lần tim. Mọi quy trình nghiệp vụ khác chạy theo luồng xử lý chuẩn sau:

```mermaid
sequenceDiagram
    participant C as Controller
    participant V as Validator
    participant S as Service
    participant DB as DbContext
    C->>V: validate(request)
    V-->>C: 400 nếu lỗi
    C->>S: xử lý nghiệp vụ
    S->>DB: EF Core (DbSet trực tiếp, AsNoTracking cho đọc)
    DB-->>S: kết quả
    S-->>C: Result<T> / DTO
```

*Hình 5.5: Luồng xử lý chuẩn backend — Controller chỉ nhận DTO và gọi Service; Service trả `Result<T>` với mã lỗi tiếng Việt; Service truy vấn DbContext trực tiếp qua DbSet.*

# PHẦN 6: KIỂM THỬ - TESTING

## 6.1 Chiến lược kiểm thử

Kiểm thử theo mô hình kim tự tháp: nền móng là unit test cho engine và service, giữa là integration test cho API, trên cùng là E2E cho luồng người dùng, kèm các tầng hiệu năng, bảo mật và UX:

**Bảng 6.1: Phân loại chiến lược kiểm thử**

| Cấp độ | Công cụ | Đối tượng / mục tiêu độ bao phủ |
|---|---|---|
| Unit — Generator | Vitest | ≥ 90% dòng `engines/` (golden data N1-N7 cho 15 giải thuật) |
| Unit — Store/Composable | Vitest + Vue Test Utils | ≥ 70% |
| Unit — Backend Service | xUnit | ≥ 60% (ưu tiên Auth, Exercise, Progress, Gamification) |
| Integration — API | xUnit + WebApplicationFactory + Testcontainers (SQL Server) | 100% endpoint chính, mọi nhánh HTTP status |
| E2E — luồng người dùng | Playwright | 12 luồng chính (học tập, Ladder 3 bậc, code runner...) |
| Hiệu năng | k6 + Lighthouse | theo NFR-1..NFR-7 |
| Bảo mật | checklist 13.3 + OWASP ZAP (cơ bản) | toàn bộ checklist 13.3 |
| UX | 5 người dùng (3 chưa dùng hệ thống tương tự) + SUS | SUS ≥ 70/100 |

Dữ liệu kiểm thử dùng TestSeed riêng (20 user 3 vai trò, 5 topic, 12 bài học, 8 bài tập, 200 bản ghi tiến độ — không dùng seed production).

## 6.2 Kết quả kiểm thử

Tại thời điểm viết báo cáo (12/08/2026), TEST_PLAN là kế hoạch đã đặc tả đầy đủ test case nhưng **chưa chạy** — bảng PASS/FAIL được điền sau khi thực thi ở giai đoạn hoàn thiện, kết quả thật sẽ được cập nhật vào báo cáo sau:

**Bảng 6.2: Báo cáo tổng hợp theo nhóm test (TEST_PLAN §10 — chưa thực thi)**

| Nhóm test | Tổng số | PASS | FAIL | Ghi chú |
|---|---|---|---|---|
| Backend (TEST-B) | chưa chạy | chưa chạy | chưa chạy | chờ hoàn tất kiểm thử (tuần 19-20) |
| Engine (TEST-E) | chưa chạy | chưa chạy | chưa chạy | chờ hoàn tất kiểm thử (tuần 19-20) |
| API (TEST-API) | chưa chạy | chưa chạy | chưa chạy | chờ hoàn tất kiểm thử (tuần 19-20) |
| E2E (TEST-UI) | chưa chạy | chưa chạy | chưa chạy | chờ hoàn tất kiểm thử (tuần 19-20) |
| Bảo mật (TEST-SEC) | chưa chạy | chưa chạy | chưa chạy | chờ hoàn tất kiểm thử (tuần 19-20) |
| Hiệu năng (TEST-PERF) | chưa chạy | chưa chạy | chưa chạy | chờ hoàn tất kiểm thử (tuần 19-20) |
| UX (TEST-UX) | chưa chạy | chưa chạy | chưa chạy | chờ hoàn tất kiểm thử (tuần 19-20) |

**Bảng 6.3: Kịch bản tiêu biểu đã thiết kế (kết quả điền sau khi chạy)**

| Mã test case | Mô tả | Kỳ vọng | Kết quả |
|---|---|---|---|
| TEST-B-001 | Đăng ký tài khoản thành công | 201, email chuẩn hóa lowercase, đăng nhập lại được | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-B-045 | Nộp bài SINGLE đúng | Điểm đúng theo đáp án | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-B-137..141 | Practice Ladder tuần tự | Chưa pass Quiz → `LADDER_LOCKED`; pass Code ≥ 70% → pass node | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-B-148 | Vào node mới trừ đúng 1 tim | 200 + `heartsLeft:9` + 1 bản ghi NodeSessions | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-B-151 | 2 request song song cùng enter | Chỉ 1 lần trừ tim (concurrency thực) | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-E-003 | Bubble sort trace chuẩn `[3,1,2]` (20 bước) | So khớp 100% bảng trace mốc vàng | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-E-035 | Hiệu năng sinh bước mảng 100 | Trung bình ≤ 500ms, không lần nào > 800ms | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-UI-001 | Luồng học tập hoàn chỉnh (E2E) | Toàn bộ luồng không lỗi, tiến độ đúng | chờ hoàn tất kiểm thử (tuần 19-20) |

Ngưỡng chất lượng trước khi bàn giao (Definition of Done): 100% test case nhóm B/E/API của FR mức Cao PASS; FAIL mở tối đa 3 lỗi trung bình có kế hoạch; coverage generator ≥ 90%; 8 kịch bản hiệu năng đạt ngưỡng; kiểm thử bảo mật 13.3 toàn bộ PASS. Mọi FAIL khi chạy phải kèm nguyên nhân, người sửa và ngày pass lại — không bịa số liệu.

## 6.3 Hiệu năng + bảo mật + UX

**Bảng 6.4: Kịch bản hiệu năng (TEST-PERF-001..008)**

| Mã | Kịch bản | Ngưỡng | Kết quả |
|---|---|---|---|
| TEST-PERF-001 | Sinh bước mảng 100 (5 GT sắp xếp, 50 lần chạy) | ≤ 500ms trung bình, 100% < 800ms | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-PERF-002 | Sinh bước đồ thị 50 đỉnh (20 lần chạy) | ≤ 1s trung bình, 100% < 1.5s | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-PERF-003 | Điều hướng 1000 bước liên tục (Chrome) | ≥ 55 FPS trung bình | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-PERF-004 | GET /lessons (1000 bài, 50 VU × 5 phút) | p95 ≤ 800ms, 0 lỗi 5xx | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-PERF-005 | POST submit bài 10 câu (20 VU song song) | p95 ≤ 1.5s, chấm đúng 100% | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-PERF-006 | Login đồng thời (50 VU × 30s) | p95 ≤ 1s, 0 lỗi | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-PERF-007 | Tải SPA lần đầu cold cache (Lighthouse) | FCP ≤ 1.5s, bundle ≤ 500KB | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-PERF-008 | Đồng thời tổng hợp 70% đọc / 30% ghi (200 VU × 15 phút) | p95 ≤ 1.2s, 0 lỗi 5xx | chờ hoàn tất kiểm thử (tuần 19-20) |

**Bảng 6.5: Kiểm thử bảo mật (TEST-SEC — tóm tắt checklist 13.3)**

| Mã | Nội dung | Kỳ vọng | Kết quả |
|---|---|---|---|
| TEST-SEC-001 | Token giả/sai chữ ký | 401 | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-SEC-002 | Student gọi endpoint Teacher/Admin | 403 | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-SEC-003 | Truy cập UserProgress người khác (đổi id) | 404, không lộ tồn tại | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-SEC-004 | Nộp `<script>` trong contentHtml | Sanitize, không thực thi | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-SEC-005 | SQL injection `' OR 1=1 --` | Không lỗi SQL, trả an toàn | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-SEC-006 | 6 lần đăng nhập sai liên tiếp | Khóa tạm (429) + log | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-SEC-007 | Upload `.exe` giả `.png` | Từ chối (magic bytes) | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-SEC-008 | Xóa refresh token khi đổi mật khẩu | Phiên cũ vô hiệu | chờ hoàn tất kiểm thử (tuần 19-20) |
| TEST-SEC-009..011 | Sandbox: vòng lặp vô hạn / đệ quy sâu / truy cập file-network | Chặn sạch, không treo trình duyệt | chờ hoàn tất kiểm thử (tuần 19-20) |

**UX (SUS).** Kế hoạch: 5 người dùng ngoài nhóm (3 người chưa dùng hệ thống tương tự) thực hiện 5 nhiệm vụ (tạo tài khoản, chạy mô phỏng bubble sort với dữ liệu tự nhập, làm bài tập trắc nghiệm, tìm bài học, xem báo cáo tiến độ) và chấm SUS. Chỉ tiêu: tỷ lệ hoàn thành 100% cả 5 nhiệm vụ, SUS ≥ 70/100. Số liệu đo thật chưa có nên ghi nhận: **chờ hoàn tất kiểm thử (tuần 19-20)**; kết quả sẽ kèm bảng thời gian thực hiện và danh sách vấn đề UX theo mức ưu tiên.


# PHẦN 7: ĐÓNG GÓI & TRIỂN KHAI

## 7.1 Đóng gói frontend/backend

Hệ thống được đóng gói thành hai phần độc lập: frontend Vue 3 build ra thư mục tĩnh `dist/`, backend ASP.NET Core publish ra bộ file chạy được. Lệnh build frontend:

```bash
cd frontend
npm ci
npm run build          # output dist/
```

`npm ci` cài đúng phiên bản dependency theo lockfile (dùng cho môi trường tự động), `npm run build` gọi Vite biên dịch ra thư mục `dist/` phục vụ tĩnh được qua nginx. Lệnh publish backend:

```bash
cd backend
dotnet publish src/DsaVisual.Api -c Release -o /opt/dsavisual/api
```

`dotnet publish` biên dịch project API theo cấu hình Release và gom toàn bộ file cần thiết vào thư mục đích (kèm runtime, không cần cài .NET SDK trên máy chạy).

Đối với môi trường phát triển chuẩn hóa, nhóm cung cấp `docker-compose.dev.yml` khởi động SQL Server và MailHog (bắt chước SMTP):

```yaml
# docker-compose.dev.yml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "DsaVisual@Dev123"
    ports: ["1433:1433"]
  mailhog:
    image: mailhog/mailhog
    ports: ["1025:1025", "8025:8025"]   # SMTP 1025, UI 8025
```

**Bảng 7.1: Các service trong docker-compose (môi trường dev)**

| Service | Hình ảnh | Công dụng |
|---|---|---|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | Cơ sở dữ liệu SQL Server, mở cổng 1433 |
| `mailhog` | `mailhog/mailhog` | Máy chủ SMTP giả để xem email gửi ra tại `http://localhost:8025` |

Nếu chưa cấu hình SMTP thật, hệ thống ghi log và hiển thị link/mã trong log dev — không chặn đăng ký/đăng nhập.

## 7.2 Triển khai production

Kiến trúc triển khai production được mô tả bằng sơ đồ dưới: nginx làm điểm vào duy nhất, vừa phục vụ file tĩnh frontend vừa chuyển tiếp request API sang Kestrel; API kết nối SQL Server và SMTP (tùy chọn):

```mermaid
graph LR
    User((Người dùng)) --> LB[Nginx/Reverse Proxy<br/>443 TLS + static files]
    LB --> FE[Frontend static<br/>dist/]
    LB --> API[ASP.NET Core API<br/>Kestrel :5000]
    API --> DB[(SQL Server)]
    API --> SMTP[SMTP server (tùy chọn)]
```

Nginx đảm nhận 2 vai trò: phục vụ file tĩnh frontend (`dist/`) và reverse proxy sang Kestrel :5000. Mọi request vào production đều qua TLS 1.2+ (HSTS). Cấu hình nginx:

```nginx
# /etc/nginx/sites-available/dsa-visual
server {
    listen 80;
    server_name dsa-visual.example.edu.vn;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;
    server_name dsa-visual.example.edu.vn;

    ssl_certificate     /etc/ssl/certs/dsa-visual.crt;
    ssl_certificate_key /etc/ssl/private/dsa-visual.key;
    add_header Strict-Transport-Security "max-age=31536000" always;

    root /var/www/dsavisual;
    index index.html;

    location / { try_files $uri $uri/ /index.html; }        # SPA fallback

    location /api/ {
        proxy_pass http://127.0.0.1:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    client_max_body_size 10m;                                 # upload ảnh ≤ 5MB + lề
}
```

API chạy như service hệ thống bằng systemd (Linux), tự khởi động lại khi lỗi:

```ini
# /etc/systemd/system/dsavisual-api.service
[Unit]
Description=DSA-Visual API
After=network.target

[Service]
WorkingDirectory=/opt/dsavisual/api
ExecStart=/usr/bin/dotnet DsaVisual.Api.dll
Restart=always
EnvironmentFile=/etc/dsavisual/env      # chứa các biến DSA__*
User=www-data

[Install]
WantedBy=multi-user.target
```

```bash
systemctl daemon-reload
systemctl enable --now dsavisual-api
systemctl status dsavisual-api
```

Nếu triển khai trên Windows, nhóm dùng IIS với ASP.NET Core Hosting Bundle (site trỏ `DsaVisual.Api.exe`, in-process) hoặc chạy Kestrel đơn giản phía sau proxy. Các biến môi trường quan trọng (giá trị cụ thể không ghi trong báo cáo):

**Bảng 7.2: Biến môi trường hệ thống**

| Biến | Mô tả |
|---|---|
| `DSA__Jwt__Secret` | Khóa ký token JWT — chuỗi ngẫu nhiên từ 32 ký tự trở lên, tuyệt đối không commit |
| `ConnectionStrings__Default` | Chuỗi kết nối SQL Server |
| `DSA__Jwt__AccessTokenMinutes` | Thời hạn access token (mặc định 60 phút) |
| `DSA__Jwt__RefreshTokenDays` | Thời hạn refresh token (mặc định 7 ngày) |
| `DSA__Cors__AllowedOrigins` | Danh sách origin được phép gọi API |
| `DSA__Email__SmtpHost` / `SmtpPort` / `From` | Cấu hình SMTP; để trống = chế độ log/MailHog |
| `DSA__Storage__Path` | Thư mục lưu file upload |
| `DSA__Simulation__MaxArraySize` | Giới hạn mảng mô phỏng (100 phần tử) |
| `DSA__Simulation__MaxGraphVertices` | Giới hạn số đỉnh đồ thị (50) |
| `DSA__Auth__MaxLoginAttempts` / `LockoutMinutes` | Khóa đăng nhập sau 5 lần sai 15 phút |
| `VITE_API_BASE_URL` | Base URL API cho frontend (khai báo trong `.env.production`) |

Quy ước chung: biến backend dùng tiền tố `DSA__` (ánh xạ `appsettings.json` theo chuẩn ASP.NET Core); secret không bao giờ nằm trong `appsettings.Production.json` đã commit; frontend không chứa secret nào.

## 7.3 CI/CD + backup

Pipeline CI/CD dùng GitHub Actions với 3 job chạy song song mỗi lần push hoặc tạo pull request:

**Bảng 7.3: Các job chính của GitHub Actions (ci.yml)**

| Job | Nội dung chính |
|---|---|
| `frontend` | Cài Node 20 → `npm ci` → lint (ESLint) → typecheck (`vue-tsc --noEmit`) → unit test (Vitest) → build → quét bảo mật dependency (`npm audit`) |
| `backend` | Cài .NET 8 → build solution → chạy test với SQL Server chạy trong service container → quét lỗ hổng package (`dotnet list package --vulnerable`) |
| `catalog-sync` | Chạy `scripts/check-catalog-sync.js` so sánh `shared/simulation-catalog.json` với danh sách key phía frontend — khác là fail build |

Sau khi CI pass, staging được deploy tự động qua script `scripts/deploy-staging.sh`. Deploy production làm thủ công qua tag `release/*` (workflow `deploy-prod.yml`, SSH + script). Trước khi deploy: backup DB trước, chạy migration DB trước khi deploy code.

Chính sách backup cơ sở dữ liệu:

```sql
-- Backup full hàng ngày 02:00 (giữ 14 bản — script job lịch)
BACKUP DATABASE DsaVisual TO DISK = 'D:\backups\DsaVisual_20260812.bak' WITH COMPRESSION;

-- Restore (test restore 1 lần/tháng, ghi biên bản)
RESTORE DATABASE DsaVisual FROM DISK = 'D:\backups\DsaVisual_20260812.bak'
  WITH REPLACE, RECOVERY;
```

**Bảng 7.4: Chính sách backup**

| Mục | Chính sách |
|---|---|
| Backup full | Hàng ngày lúc 02:00, giữ 14 bản |
| Backup log | Mỗi 4 giờ |
| Test restore | 1 lần/tháng, ghi biên bản |
| Lưu trữ | Ổ khác máy chủ (network share / object storage) |

## 7.4 Runbook sự cố

Bảng dưới tóm tắt 8 sự cố thường gặp nhất khi vận hành (rút gọn từ danh sách 10 sự cố của DEPLOY):

**Bảng 7.5: Runbook sự cố thường gặp**

| Sự cố | Nguyên nhân | Xử lý | Thời gian mục tiêu |
|---|---|---|---|
| API trả 503 liên tục | SQL Server ngừng | Kiểm tra service SQL, khởi động lại, xem log lỗi SQL, restore backup nếu lỗi dữ liệu | 30 phút |
| 500 liên tục ở API | Lỗi code / config sai | Xem log file gần nhất, kiểm tra biến môi trường, rollback phiên bản trước | 1 giờ |
| Đăng nhập chậm | Quá nhiều request hash mật khẩu | Kiểm tra rate limit hoạt động, tăng tài nguyên, kiểm tra lockout DB | 1 giờ |
| Upload ảnh lỗi | Hết dung lượng ổ | Kiểm tra `df -h`, dọn upload tạm (job đêm), mở rộng ổ | 30 phút |
| Mô phỏng chậm phía client | Dữ liệu quá giới hạn / máy yếu | Xác nhận giới hạn hệ thống, gợi ý giảm kích thước dữ liệu, kiểm tra phiên bản trình duyệt | 2 giờ |
| Token lỗi hàng loạt | Secret JWT bị thay đổi | Kiểm tra `DSA__Jwt__Secret`, khôi phục giá trị cũ, người dùng đăng nhập lại | 30 phút |
| Email không gửi | SMTP lỗi | Kiểm tra queue email trong DB, kiểm tra kết nối SMTP, bật lại service | 1 giờ |
| Backup thất bại | Hết dung lượng / quyền | Xem log job backup, giải phóng dung lượng, chạy lại thủ công | 1 giờ |

Kèm theo đó, nhóm duy trì kế hoạch rollback: giữ 2 bản deploy gần nhất (rollback code trong 15 phút); rollback DB chỉ khi migration gây lỗi, restore backup trước mốc migration; mọi rollback phải ghi nhật ký (ai, khi nào, lý do, kết quả).

# KẾT LUẬN & HƯỚNG PHÁT TRIỂN

## Kết quả đạt được

Qua quá trình nghiên cứu và phát triển, đề tài DSA-Visual đã đạt được các kết quả quan trọng sau:

- **Về mặt chức năng**: Hệ thống hoàn thiện đầy đủ các luồng nghiệp vụ cốt lõi gồm học tập theo 5 lộ trình kiến thức, mô phỏng từng bước thuật toán, khung luyện tập 3 bậc (Trắc nghiệm, Thực hành tương tác, Lập trình trực tiếp), hệ thống trò chơi hóa học tập (Gamification), quản lý lớp học phần dành cho giảng viên và bảng điều khiển quản trị hệ thống.
- **Về mặt kỹ thuật**: Nền tảng xây dựng thành công bộ máy mô phỏng EDV thực thi mã nguồn thật để phát lại chuỗi hành động trực quan chính xác; tích hợp môi trường hộp cát Web Worker bảo mật phía trình duyệt và triển khai cơ chế xác thực JWT an toàn kết hợp xoay vòng Refresh Token.
- **Về mặt giao diện**: Giao diện học tập được thiết kế đồng bộ ba vùng hiển thị (mã giả, đồ họa tương tác, giải thích chi tiết), hỗ trợ chế độ ban đêm (Dark Mode) và tương thích tốt trên các thiết bị máy tính.

## Khó khăn & Bài học kinh nghiệm

- **Khối lượng nội dung lớn**: Việc chuẩn hóa dữ liệu mô phỏng và xây dựng bộ dữ liệu kiểm thử chuẩn cho nhiều thuật toán phức tạp đòi hỏi thời gian đối soát lớn.
- **Môi trường chạy mã nguồn**: Việc cách ly môi trường thực thi trên trình duyệt cần cân đối giữa giới hạn an toàn bảo mật và khả năng hỗ trợ các cấu trúc dữ liệu nâng cao.

## Hướng phát triển

Dựa trên kết quả đạt được, nhóm phát triển định hướng mở rộng hệ thống trong các giai đoạn tiếp theo:

- Mở rộng hệ thống chấm mã nguồn tự do cho phép người học nộp giải pháp thuật toán hoàn chỉnh không giới hạn theo khuôn mẫu cố định.
- Bổ sung các thuật toán và cấu trúc dữ liệu nâng cao: Cây đỏ-đen, Cây B-Tree, Cây Trie, Thuật toán Prim, Kruskal, Floyd-Warshall và Thuật toán tìm kiếm chuỗi KMP.
- Tích hợp trợ lý trí tuệ nhân tạo (AI Assistant) hỗ trợ phân tích bước chạy thuật toán, giải thích nguyên lý và đưa ra gợi ý sửa lỗi mã nguồn theo ngữ cảnh học tập.
- Tối ưu hóa giao diện đa nền tảng cho thiết bị di động và bổ sung hỗ trợ đa ngôn ngữ (Tiếng Anh và Tiếng Việt).
- Tích hợp cổng thanh toán trực tuyến chính thức phục vụ đăng ký các gói học tập nâng cao.

# TÀI LIỆU THAM KHẢO

1. Thomas H. Cormen, Charles E. Leiserson, Ronald L. Rivest, Clifford Stein. *Introduction to Algorithms (4th Edition)*, MIT Press, 2022.
2. Robert Sedgewick, Kevin Wayne. *Algorithms (4th Edition)*, Addison-Wesley Professional, 2011.
3. Mark Allen Weiss. *Data Structures and Algorithm Analysis in C++ (4th Edition)*, Pearson, 2014.
4. Donald E. Knuth. *The Art of Computer Programming, Volume 1: Fundamental Algorithms (3rd Edition)*, Addison-Wesley, 1997.
5. Robert C. Martin. *Clean Architecture: A Craftsman's Guide to Software Structure and Design*, Prentice Hall, 2017.
6. Martin Fowler. *Patterns of Enterprise Application Architecture*, Addison-Wesley Professional, 2002.
7. Steven Halim, Felix Halim. *VisuAlgo — Visualising Data Structures and Algorithms through Animation*, National University of Singapore (NUS), 2015. https://visualgo.net
8. Clifford A. Shaffer, Matthew L. Cooper, et al. *Algorithm Visualization: The State of the Field*, ACM Transactions on Computing Education (TOCE), Vol. 10, No. 3, 2010.
9. Saraiya, P., Shaffer, C. A., McCrickard, D. S., & North, C. *Effective Features of Algorithm Visualizations*, ACM SIGCSE Bulletin, 36(1), 381–385, 2004.
10. David Galles. *Data Structure Visualizations*, University of San Francisco, 2020. https://www.cs.usfca.edu/~galles/visualization
11. Microsoft Corporation. *.NET and ASP.NET Core Documentation*, Microsoft Learn, 2024. https://learn.microsoft.com/aspnet/core
12. Microsoft Corporation. *Entity Framework Core Documentation*, Microsoft Learn, 2024. https://learn.microsoft.com/ef/core
13. Evan You. *Vue.js 3 — The Progressive JavaScript Framework Documentation*, 2024. https://vuejs.org
14. World Wide Web Consortium (W3C). *Web Workers Specification*, W3C Recommendation, 2023. https://www.w3.org/TR/workers/
15. Open Web Application Security Project (OWASP). *OWASP Top 10 Web Application Security Risks*, 2021. https://owasp.org/Top10/
16. Internet Engineering Task Force (IETF). *RFC 7519: JSON Web Token (JWT)*, May 2015. https://datatracker.ietf.org/doc/html/rfc7519
17. Chính phủ Việt Nam. *Nghị định số 13/2023/NĐ-CP về Bảo vệ dữ liệu cá nhân*, ban hành ngày 17/04/2023.

# PHỤ LỤC A: Hướng dẫn cài đặt môi trường

**Yêu cầu phần mềm tối thiểu:**

**Bảng A.1: Yêu cầu phần mềm**

| Thành phần | Phiên bản | Mục đích |
|---|---|---|
| .NET SDK | 8.0+ | Build backend |
| ASP.NET Core Runtime | 8.0+ | Chạy API |
| Node.js | 20+ | Build frontend |
| SQL Server | 2019+ | Production; dev có thể dùng SQLite/LocalDB |
| Nginx | 1.24+ | Reverse proxy + static files (Linux) |
| Docker | 24+ | Tùy chọn — dev chuẩn hóa (SQL Server + MailHog) |

**Bước 1 — Cài frontend:** chạy `npm install` để cài dependency, sau đó `npm run dev` khởi động Vite dev server tại cổng 5173 (proxy `/api` sang localhost:5000). Xem kết quả tại `http://localhost:5173`.

```bash
cd frontend
npm install
npm run dev        # Vite dev server :5173, proxy /api → localhost:5000
```

**Bước 2 — Cài backend:** khôi phục và build solution bằng `dotnet restore` + `dotnet build`, chạy migration tạo CSDL, rồi khởi động API kèm seed dữ liệu mẫu. Kiểm tra tại Swagger `http://localhost:5000/swagger`.

```powershell
cd backend
dotnet restore
dotnet build

# Migration + seed
dotnet ef database update --project src/DsaVisual.Application --startup-project src/DsaVisual.Api
dotnet run --project src/DsaVisual.Api --seed    # seed idempotent (chạy lại không nhân đôi)
```

**Bước 3 — Cấu hình cơ sở dữ liệu:** dev dùng SQL Server local hoặc SQLite qua biến `ConnectionStrings__Default=Data Source=dsavisual-dev.db` (đặt trong `appsettings.Development.json` hoặc biến môi trường). Muốn môi trường chuẩn, khởi động SQL Server + MailHog bằng `docker compose -f docker-compose.dev.yml up -d`.

**Bước 4 — Chạy production (Linux):** build frontend bằng `npm ci` + `npm run build`, publish backend bằng `dotnet publish src/DsaVisual.Api -c Release -o /opt/dsavisual/api`, cấu hình nginx và systemd theo PHẦN 7.2.

# PHỤ LỤC B: Phím tắt + thuật ngữ

**Bảng B.1: Phím tắt trang mô phỏng**

| Phím | Chức năng |
|---|---|
| `Space` | Phát / Tạm dừng mô phỏng |
| `→` / `←` | Bước tiếp theo / bước trước đó |
| `Home` / `End` | Về bước đầu tiên / nhảy tới bước cuối |
| `[` / `]` | Giảm / tăng tốc độ chạy |
| `C` | Mở hộp thoại cấu hình dữ liệu |
| `F` | Lưu vào mục yêu thích |

**Bảng B.2: Thuật ngữ viết tắt**

| Thuật ngữ | Giải thích |
|---|---|
| DSA | Cấu trúc dữ liệu và giải thuật — lĩnh vực của hệ thống |
| CTDL / GT | Cấu trúc dữ liệu / Giải thuật (thuật toán) |
| EDV (Execution-Driven Visualization) | Kiến trúc lõi: mã thật chạy qua StepExecutor, hoạt ảnh = phát lại trace thực thi |
| StepExecutor | Bộ thực thi gắn thiết bị đo, chạy code mẫu và ghi TraceEvent[] |
| TraceEvent | Bản ghi một câu lệnh quan trọng khi thực thi: dòng code, snapshot biến, phần tử highlight, giải thích |
| SPA | Single Page Application — ứng dụng web tải một lần, chuyển nội dung không tải lại trang |
| JWT | JSON Web Token — chuỗi mã hóa xác thực; access token 60 phút, refresh token 7 ngày trong cookie an toàn |
| EF Core | Thư viện C# (ORM) truy vấn và lưu dữ liệu vào cơ sở dữ liệu |
| Migration | Cơ chế EF Core thay đổi cấu trúc bảng theo phiên bản |
| Sandbox | Môi trường chạy code cách ly phía client (Web Worker), giới hạn 10 giây / 64MB / 200 dòng |
| Big-O | Ký hiệu mô tả độ phức tạp thời gian/không gian của giải thuật |
| BFS / DFS | Duyệt đồ thị theo chiều rộng (hàng đợi) / theo chiều sâu (ngăn xếp) |
| Practice Ladder | Chuỗi luyện tập 3 bậc: Quiz → Interactive Lab → Code Challenge |
| NodeSession | Bản ghi phiên học 30 phút của một người học tại một node |
| KPI | Chỉ số đo lường mục tiêu dự án (G1-G8) |
| UC / FR / NFR | Use case / Yêu cầu chức năng / Yêu cầu phi chức năng |
| SUS | Bảng khảo sát đánh giá mức độ dùng được của giao diện |

# PHỤ LỤC C: Thư viện bên thứ ba (license)

Chưa cập nhật đầy đủ (12/08/2026) — bảng dưới liệt kê thư viện chính theo nguồn SDD/DEPLOY, giấy phép sẽ bổ sung khi có THIRD_PARTY.md.

**Bảng C.1: Thư viện bên thứ ba chính**

| Thư viện | Công dụng | Ghi chú |
|---|---|---|
| Vue 3 | Framework frontend | Dùng toàn bộ giao diện (SDD §3) |
| Pinia | Quản lý trạng thái frontend | Store auth/lesson/simulation/progress/gamification (SDD §3.2) |
| Vite | Build tool frontend | Dev server + build production (SDD §3.9) |
| Axios | HTTP client | Gọi API, interceptor token (SDD §3.4) |
| Monaco Editor | Trình soạn mã | Code Runner Màn 16 (SDD §8) |
| Mermaid | Vẽ sơ đồ trong tài liệu | Sơ đồ kiến trúc, state machine (DEPLOY §1.3) |
| ASP.NET Core | Backend framework | API 2 lớp Controller → Service → DbContext (SDD §5) |
| Entity Framework Core | ORM | Code-First + Migrations (SDD §7) |
| Serilog | Ghi log có cấu trúc | Rolling file 90 ngày (DEPLOY §7.1) |
| xUnit | Unit test backend | DsaVisual.UnitTests (SDD §5) |
| Vitest | Unit test frontend | Test store và component (SDD §3.7) |
| Testcontainers | Chạy container trong test | Integration test trên SQL Server thật (DEPLOY §5.4) |
| k6 | Load test | Kịch bản login (DEPLOY §9) |

# PHỤ LỤC D: Danh mục mô phỏng (catalog)

Danh mục mô phỏng được đồng bộ từ file `shared/simulation-catalog.json` — 44 mô phỏng chia 2 nhóm: 34 thao tác giải thuật (algorithm) và 10 cấu trúc dữ liệu (structure). Trong đó 3 mô phỏng được mở xem công khai tại trang chủ không cần đăng nhập: Bubble Sort, Binary Search, BFS.

**Bảng D.1: Danh mục 44 mô phỏng**

| Nhóm | Tên mô phỏng | Mô tả |
|---|---|---|
| Sắp xếp | Sắp xếp nổi bọt (Bubble Sort) | So sánh và hoán đổi cặp phần tử liền kề, đưa phần tử lớn về cuối |
| Sắp xếp | Sắp xếp chọn (Selection Sort) | Chọn phần tử nhỏ nhất còn lại đưa về đầu mảng |
| Sắp xếp | Sắp xếp chèn (Insertion Sort) | Chèn từng phần tử vào đúng vị trí trong đoạn đã sắp xếp |
| Sắp xếp | Sắp xếp trộn (Merge Sort) | Chia mảng làm đôi rồi trộn các nửa đã sắp xếp (chia để trị) |
| Sắp xếp | Sắp xếp nhanh (Quick Sort — Lomuto) | Chọn chốt (pivot) và phân chia mảng quanh chốt |
| Sắp xếp | Sắp xếp vun đống (Heap Sort) | Vun mảng thành đống rồi trích phần tử lớn nhất về cuối |
| Tìm kiếm | Tìm kiếm tuyến tính (Linear Search) | Duyệt từng phần tử cho tới khi gặp giá trị cần tìm |
| Tìm kiếm | Tìm kiếm nhị phân (Binary Search) | Chia đôi đoạn tìm kiếm trên mảng đã sắp xếp |
| Ngăn xếp | Ngăn xếp — Push | Đẩy phần tử vào đỉnh ngăn xếp (LIFO) |
| Ngăn xếp | Ngăn xếp — Pop | Lấy phần tử trên đỉnh ngăn xếp ra |
| Ngăn xếp | Ngăn xếp — Peek | Xem phần tử trên đỉnh ngăn xếp mà không lấy ra |
| Hàng đợi | Hàng đợi — Enqueue | Thêm phần tử vào cuối hàng đợi (FIFO) |
| Hàng đợi | Hàng đợi — Dequeue | Lấy phần tử đầu hàng đợi ra |
| Danh sách liên kết | Danh sách liên kết — Chèn | Chèn nút vào đầu, cuối hoặc vị trí k |
| Danh sách liên kết | Danh sách liên kết — Xóa | Xóa nút theo vị trí hoặc giá trị |
| Danh sách liên kết | Danh sách liên kết — Tìm kiếm | Duyệt tìm nút chứa giá trị cần tìm |
| Danh sách liên kết | Danh sách liên kết — Duyệt | Đi qua toàn bộ nút theo thứ tự liên kết |
| Cây | BST — Chèn | Chèn khóa vào đúng vị trí trên cây nhị phân tìm kiếm |
| Cây | BST — Xóa | Xóa khóa và nối lại cây cho đúng tính chất BST |
| Cây | BST — Tìm kiếm | Dò tìm khóa theo quan hệ lớn hơn/nhỏ hơn của BST |
| Cây | BST — Duyệt Preorder | Duyệt theo thứ tự gốc – trái – phải |
| Cây | BST — Duyệt Inorder | Duyệt theo thứ tự trái – gốc – phải |
| Cây | BST — Duyệt Postorder | Duyệt theo thứ tự trái – phải – gốc |
| Cây | BST — Duyệt Level-order | Duyệt theo từng tầng từ trên xuống (BFS) |
| Cây | Cây AVL — Chèn kèm xoay (LL/RR/LR/RL) | Chèn và tự xoay để cây luôn cân bằng |
| Đống | Đống nhị phân — Chèn (bubble up) | Thêm phần tử và đẩy lên đúng vị trí |
| Đống | Đống nhị phân — Trích xuất max (sift down) | Lấy phần tử lớn nhất và sắp lại cho đúng tính chất đống |
| Đống | Đống nhị phân — Heapify | Vun mảng thành đống nhị phân |
| Bảng băm | Bảng băm — Chèn (chuỗi nối kết) | Tính hàm băm rồi chèn vào bucket tương ứng |
| Bảng băm | Bảng băm — Tìm kiếm | Tính hàm băm rồi tìm trong bucket tương ứng |
| Bảng băm | Bảng băm — Xóa | Xóa khóa khỏi bucket tương ứng |
| Đồ thị | Đồ thị — Duyệt BFS | Duyệt theo chiều rộng dùng hàng đợi |
| Đồ thị | Đồ thị — Duyệt DFS | Duyệt theo chiều sâu dùng ngăn xếp |
| Đồ thị | Đồ thị — Dijkstra (đường đi ngắn nhất) | Tìm đường đi ngắn nhất từ đỉnh nguồn |
| CTDL | Mảng (Array) | Cấu trúc lưu trữ tuần tự, truy cập theo chỉ số |
| CTDL | Danh sách liên kết đơn (Singly Linked List) | Các nút nối nhau bằng con trỏ |
| CTDL | Ngăn xếp (Stack — LIFO) | Cấu trúc vào sau ra trước |
| CTDL | Hàng đợi (Queue — FIFO) | Cấu trúc vào trước ra trước |
| CTDL | Cây nhị phân (Binary Tree) | Cây mỗi nút tối đa hai con |
| CTDL | Cây nhị phân tìm kiếm (BST) | Cây thỏa quan hệ khóa trái/phải |
| CTDL | Cây AVL (cân bằng) | Cây tự cân bằng theo độ chênh lệch chiều cao |
| CTDL | Đống nhị phân (Binary Heap — max-heap) | Cây đầy đủ thỏa tính chất đống |
| CTDL | Bảng băm (Hash Table — chuỗi nối kết) | Bảng ánh xạ khóa qua hàm băm |
| CTDL | Đồ thị (Graph — có hướng/vô hướng, trọng số) | Cấu trúc đỉnh và cạnh, có thể có trọng số |
