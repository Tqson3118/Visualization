# SCREEN MAP — ÁNH XẠ FR → MÀN HÌNH & ĐÁNH GIÁ ĐỘ SÂU (cập nhật 12/08/2026 — bản gốc 09/08/2026)

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**

| | |
|---|---|
| Loại tài liệu | Bản đồ màn hình (Screen Map) — nguồn bắt buộc khi sinh SDD mục Thiết kế UI/UX (PRODUCTION_PROMPT §21 mục 7) |
| Phiên bản | 1.0 |
| Ngày cập nhật | 12/08/2026 |
| Trạng thái | Dự thảo |
| Người soạn | Trần Viết Tâm Phúc |
| Người duyệt | Phạm Ngọc Ái Liên |
| Tài liệu liên quan | SDD.md §8 (Thiết kế giao diện), USER_GUIDE.md, PRODUCTION_PROMPT.md §7/§20.2/§20.5 |
| Nguồn yêu cầu | PRODUCTION_PROMPT.md Phần 3 (FR), §7 (Màn 01-12), §20.2 (Màn 13-30), §20.5 (Màn 31-32), §17.13/17.14 (khuôn chuẩn) |
| Giả định chính | 1) Mọi FR phải có màn/route tương ứng trước khi bàn giao dev. 2) Màn ⚠/✗ trong ma trận là việc phải vá trước khi dev. 3) Route cũ `/learn`, `/dashboard` chuyển hướng sang `/path`, `/profile` (§20.5.6). |

## Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 12/08/2026 | Trần Viết Tâm Phúc | Bổ sung front matter theo khuôn §17.11 + Lịch sử thay đổi (vá review) — nội dung ma trận giữ nguyên từ bản cập nhật 12/08/2026 |

> Nguồn kiểm tra: `PRODUCTION_PROMPT.md` Phần 3 (FR), 6.16 (UC-16→32), 7.0-7.9 (Màn 01-12 + wireframe), 17.13/17.14 (khuôn chuẩn), 19.1-19.9 (thiết kế chốt), 20.0-20.4 (ghi đè).
> Mục đích: cầu nối FR → Màn cho dev. Mức độ: **✓** = đã có màn + đặc tả dùng được; **⚠** = có màn nhưng thiếu chi tiết/mâu thuẫn; **✗** = KHÔNG có màn/UI.
> Khi sinh SDD §8: mọi màn ghi ⚠/✗ phải được bổ sung đặc tả theo khuôn 17.14 trước khi bàn giao cho dev.
> **CẬP NHẬT 12/08/2026**: SDD §8.4 đã đặc tả đủ 32 màn (bảng tổng hợp mục 10A); SCREEN_MAP này là chi tiết theo từng FR.

## 1. Đánh giá tổng quan

| # | Nhóm lỗ hổng | Mức độ |
|---|---|---|
| 1 | 10 FR mới (2.10, 3.20/3.20b, 4.11, 4.12, 7.6, 9.6, 10.1→10.7) CHƯA có đặc tả chi tiết theo khuôn 17.13 (7 thuộc tính) — chỉ có 1 dòng ma trận + UC ngắn + số liệu rải 19.2-19.4 | **NGHIÊM TRỌNG** |
| 2 | 14 FR không có màn/UI tương ứng (xem Mục 4) | **NGHIÊM TRỌNG** |
| 3 | 6 mâu thuẫn nội tại chưa chốt (xem Mục 5) — trong đó Benchmark Lab là xung đột thiết kế thật sự | **CAO** |
| 4 | Màn 01-30 chưa đạt khuôn 17.14 (thiếu Mục đích/Bảng thành phần/Tương tác/Trạng thái/Phím tắt/Responsive/Guard/Lỗi); FR gốc chưa đủ 7 thuộc tính 17.13; 19.9 (4 tầng) chưa áp dụng đủ cho mọi FR | TRUNG BÌNH |

## 2. Các FR ĐÃ CẮT (không triển khai — tránh nhầm lẫn)

| FR | Tên | FR | Tên |
|---|---|---|---|
| FR-1.10 | Quản lý phiên đăng nhập | FR-5.6 | Mục tiêu cá nhân và nhắc nhở |
| FR-2.7 | Lịch sử bài học gần đây | FR-5.7 | Xuất báo cáo tiến độ cá nhân |
| FR-2.8 | Mục tiêu học tập của bài học | FR-6.4 | Gửi thông báo hệ thống (Broadcast) |
| FR-2.9 | Xuất bài học PDF (chỉ Premium CheatSheet PDF giữ — Màn 18) | FR-7.3 | Thông báo trong hệ thống |
| FR-3.13 | So sánh hai GT song song | FR-7.5 | Báo cáo lỗi từ người dùng |
| FR-3.17 | Bookmark bước | FR-3.19 | Lịch sử mô phỏng và tiếp tục |

## 3. Ma trận FR → Màn hình

### Module A — Auth & tài khoản (10 FR giữ)

| FR | Tên | Màn/Route | Độ | Lỗ hổng cần xử lý |
|---|---|---|---|---|
| FR-1.1 | Đăng ký | Màn 02 `/register` | ✓ | — |
| FR-1.2 | Đăng nhập | Màn 02 `/login` | ✓ | — |
| FR-1.3 | Gia hạn phiên | Hạ tầng (axios interceptor) | ✓ | Không cần màn |
| FR-1.4 | Đăng xuất | AppShell (menu người dùng) | ✓ | Không cần màn riêng |
| FR-1.5 | Đổi mật khẩu | **KHÔNG CÓ** | ✗ | Cần màn `Tài khoản/Bảo mật` (xem Mục 4, Màn N-1) |
| FR-1.6 | Khôi phục mật khẩu | **KHÔNG CÓ** | ✗ | Cần `/forgot-password` + `/reset-password` (Màn N-2) |
| FR-1.7 | Cập nhật hồ sơ | **KHÔNG CÓ** | ✗ | Cần `/account/profile` (Màn N-1) |
| FR-1.8 | Phê duyệt Teacher | Màn 29 (tab trong `/admin/users`) | ✓ | — |
| FR-1.9 | Quản lý người dùng | Màn 10 `/admin/users` | ✓ | — |
| FR-1.11 | 2FA qua email | **KHÔNG CÓ** | ✗ | Màn nhập mã OTP + cài đặt bảo mật (GĐ3, gộp Màn N-1) |

### Module B — Học tập: Learning Path + CheatSheet (8 FR giữ)

| FR | Tên | Màn/Route | Độ | Lỗ hổng cần xử lý |
|---|---|---|---|---|
| FR-2.1 | Quản lý topic | Màn 09 `/admin/topics` | ✓ | — |
| FR-2.2 | Quản lý bài học | Màn 09 `/admin/lessons` | ✓ | — |
| FR-2.3 | Danh sách bài học | Màn 03 `/learn` | ✓ | — |
| FR-2.4 | Chi tiết bài học | Màn 04 `/learn/{lessonId}` | ⚠ | Màn 04 thiếu: nút Ghi chú (FR-2.6), nút Đánh giá (FR-7.4), nút "▶ Xem bước này" (FR-2.11) |
| FR-2.5 | Tìm kiếm bài học | Search box AppShell | ⚠ | Không có trang kết quả tìm kiếm — cần làm rõ: dropdown gợi ý hay trang `/search` |
| FR-2.6 | Ghi chú cá nhân | **KHÔNG CÓ UI** | ✗ | Nút ghi chú + drawer soạn thảo trong Màn 04 (autosave 1s, dấu chấm ở danh sách) |
| FR-2.10 | Learning Path | Màn 13 `/path/{topicId}` | ✓ | Đặc tả tốt (Duolingo-style) |
| FR-2.11 | Two-way sync deep-link | Màn 04 + Màn 05 | ⚠ | Màn 05 (7.4) CHƯA có nút "Xem lý thuyết liên quan" — phải bổ sung vào header Màn 05 |

### Module C — Visualizer EDV + Benchmark (18 FR giữ)

| FR | Tên | Màn/Route | Độ | Lỗ hổng cần xử lý |
|---|---|---|---|---|
| FR-3.1 | Danh mục mô phỏng | **KHÔNG CÓ màn riêng** | ✗ | Cần trang `/simulations` (lọc theo CTDL/tag/mức độ) hoặc làm rõ là tab trong Màn 03 |
| FR-3.2 | Khởi tạo mô phỏng | Màn 05 | ✓ | Nhớ gắn quy tắc trừ tim (20.4) + tham chiếu FR-10.1 |
| FR-3.3 | Hiển thị 3 vùng | Màn 05 | ✓ | — |
| FR-3.4 | Cấu hình đầu vào | Màn 05 (modal) | ✓ | — |
| FR-3.5 | Điều khiển | Màn 05 | ✓ | Đầy đủ + phím tắt |
| FR-3.6 | Trạng thái phần tử | Màn 05 + 7.6 | ✓ | — |
| FR-3.7 | Bảng mã giả | Màn 05 | ✓ | — |
| FR-3.8 | Tùy chọn hiển thị | Màn 05 | ⚠ | Không đặc tả UI nút (nằm đâu, dạng gì — dropdown/cog menu) |
| FR-3.9 | Bộ đếm thống kê | Màn 05 | ✓ | — |
| FR-3.10 | Yêu thích mô phỏng | Nút ★ Màn 05 | ✗ | "Danh sách yêu thích trong hồ sơ cá nhân" (FR-3.10) — KHÔNG có màn hồ sơ/yêu thích |
| FR-3.11 | Chia sẻ link | Màn 05 | ✓ | — |
| FR-3.12 | Manual Step Practice | Chế độ trong Màn 05 | ⚠ | Nút "Tự thực hành" + hộp chọn thao tác chưa được đặc tả UI trong Màn 05 |
| FR-3.14 | Call Stack | Panel Màn 05 (tùy chọn) | ⚠ | Màn 05 không nhắc panel call stack — cần ghi rõ vị trí (7.6 mới nhắc "tùy chọn") |
| FR-3.15 | Breakpoint | Màn 05 | ✗ | Không đặc tả UI (bấm lề mã giả + cài điều kiện) |
| FR-3.16 | Mini Quiz sau mô phỏng | **KHÔNG CÓ** | ✗ | Banner cuối Màn 05 + panel trả lời — chưa có đặc tả nào |
| FR-3.18 | Dark Mode | **KHÔNG CÓ** | ✗ | Cần toggle trong cài đặt/profile (gộp Màn N-1); palette tối chưa có trong 7.2 |
| FR-3.20 | Benchmark Lab (1 n) | Màn 17 `/benchmark/{k1}/{k2}` | ⚠ | **MÂU THUẪN thiết kế** — xem Mục 5, điểm 3 |
| FR-3.20b | Benchmark multi-n + overlay | Màn 17 | ⚠ | **MÂU THUẪN thiết kế** — xem Mục 5, điểm 3 |

### Module D — Practice Ladder (12 FR giữ)

| FR | Tên | Màn/Route | Độ | Lỗ hổng cần xử lý |
|---|---|---|---|---|
| FR-4.1 | Quản lý bài tập (CRUD) | Màn 09 `/admin/exercises` (cũ) | ⚠ | Theo 20.3: exercise giờ thuộc **node của Learning Path** — Màn 09 chưa có UI soạn Quiz/Lab/Code gắn node; cần tab mới (Màn N-6) |
| FR-4.2 | Làm bài trắc nghiệm | Bậc 1 (Màn 14 QuizStage, layout Màn 06) | ✓ | — |
| FR-4.3 | Dự đoán bước | Bậc 2 (Màn 15 Lab) theo 20.3 | ✓ | Đã vá đặc tả Lab 15.1-15.3 (3 kịch bản + chấm trace từng bước) — xem Mục 9 |
| FR-4.4 | Đánh giá + lịch sử bài làm | SUBMISSIONS chung | ✗ | KHÔNG có màn xem lịch sử nộp quiz/lab cho người học lẫn giảng viên |
| FR-4.5 | Ngân hàng câu hỏi | **KHÔNG CÓ** | ✗ | Cần tab Ngân hàng câu hỏi (Màn N-6): tìm/lọc/tag, chọn vào bài tập |
| FR-4.6 | Practice Mode (luyện tập) | **KHÔNG CÓ UI** | ✗ | Nút "Luyện tập" song song "Làm bài" trong Bậc 1 — chưa đặc tả |
| FR-4.7 | Hints | **KHÔNG CÓ UI** | ✗ | Nút "Gợi ý" + mức gợi ý + tiêu thụ Hint token (19.3) — chưa đặc tả |
| FR-4.8 | Xáo trộn câu/đáp án | Hạ tầng | ✓ | Không cần màn |
| FR-4.9 | Giải thích phương án sai | Màn kết quả (7.9.2) | ✓ | — |
| FR-4.10 | Nhập CSV | **KHÔNG CÓ** | ✗ | Tab Import CSV trong Màn N-6 (tải mẫu 10 cột + báo lỗi theo dòng) |
| FR-4.11 | Practice Ladder | Màn 14 `/ladder/{nodeId}` | ✓ | Đặc tả tốt (stepper 3 bậc, tách component) |
| FR-4.12 | Kiểm tra cuối lộ trình | Màn 30 `/path/{topicId}/final-test` | ✓ | — |

### Module E — Tiến độ & báo cáo (5 FR giữ)

| FR | Tên | Màn/Route | Độ | Lỗ hổng cần xử lý |
|---|---|---|---|---|
| FR-5.1 | Ghi nhận tiến độ | Hạ tầng (upsert) | ✓ | Không cần màn |
| FR-5.2 | Dashboard cá nhân | Màn 08 `/dashboard` | ✓ | — |
| FR-5.3 | Báo cáo giảng viên | 7.9.3 `/report` (cũ) + Màn 21 | ⚠ | Tham chiếu "Màn 03 (7.9.3)" trong 20.2.2 là SAI số hiệu (7.9.3 là wireframe `/report`, không phải Màn 03); báo cáo theo bài học (ngoài lớp) vẫn thiếu route rõ |
| FR-5.4 | Thống kê hệ thống | Màn 11 `/admin/stats` | ✓ | — |
| FR-5.5 | Huy hiệu thành tích | **KHÔNG CÓ** | ✗ | UC-23 nhắc "trang Thành tích" nhưng không tồn tại — cần `/account/achievements` (Màn N-4) |

### Module F — Admin tối giản (3 FR giữ: 1.9, 5.4, 6.2)

| FR | Tên | Màn/Route | Độ | Lỗ hổng cần xử lý |
|---|---|---|---|---|
| FR-1.9 | Quản lý người dùng | Màn 10 | ✓ | — |
| FR-5.4 | Thống kê hệ thống | Màn 11 | ✓ | — |
| FR-6.2 | Cấu hình hệ thống | **KHÔNG CÓ** | ✗ | Cần `/admin/settings` (domain email, tên hệ thống, chính sách MK, giới hạn upload) (Màn N-5) |

### Module G — Trang phụ trợ (4 FR giữ)

| FR | Tên | Màn/Route | Độ | Lỗ hổng cần xử lý |
|---|---|---|---|---|
| FR-7.1 | Trang chủ công khai | Màn 01 `/` | ✓ | — |
| FR-7.2 | FAQ | Màn 12 | ✓ | — |
| FR-7.4 | Đánh giá nội dung | **KHÔNG CÓ UI** | ✗ | Nút sao 1-5 + bình luận trong Màn 04 (ẩn danh, 1 lần/người); chỉ cho đánh giá khi ĐÃ "Đánh dấu đã học" bài đó (v2.9 — 403 nếu chưa) |
| FR-7.6 | Demo công khai 3 visualizer | Màn 01 | ⚠ | Màn 01 chỉ đặc tả 1 demo bubble sort — **mâu thuẫn** FR-7.6 (3 demo) — xem Mục 5, điểm 4 |

### Module H — Lớp học phần (4 FR giữ)

| FR | Tên | Màn/Route | Độ | Lỗ hổng cần xử lý |
|---|---|---|---|---|
| FR-8.1 | Tạo/quản lý lớp | Màn 19 `/classes` | ✓ | **Mâu thuẫn mã mời 6 vs 8 ký tự** — xem Mục 5, điểm 1 |
| FR-8.2 | Quản lý sinh viên | Màn 20 (tab Thành viên) | ✓ | — |
| FR-8.3 | Gán nội dung + hạn nộp | Màn 20 (tab Lộ trình đã gán) | ✓ | — |
| FR-8.4 | Báo cáo lớp | Màn 21 `/classes/{id}/report` | ✓ | Sửa tham chiếu sai ở 20.2.2 (xem FR-5.3) |

### Module I — Code Runner (6 FR giữ)

| FR | Tên | Màn/Route | Độ | Lỗ hổng cần xử lý |
|---|---|---|---|---|
| FR-9.1 | Trình soạn mã | Màn 16 `/code/{key}` | ✓ | — |
| FR-9.2 | Code → Visual 2 chiều | Màn 16 | ✓ | Custom Testcase (nhập input tùy ý chạy thử) + nút "So sánh code chuẩn" (Trace Diff 2 canvas, chỉ khi code trace-được) — v2.9 |
| FR-9.3 | Bài tập code + chấm | Bậc 3 (Màn 16) | ⚠ | Phía **Teacher soạn bài code** (signature + test ẩn 10-20) KHÔNG có màn — cần tab trong Màn N-6 |
| FR-9.4 | Sandbox an toàn | Hạ tầng | ⚠ | **Mâu thuẫn giới hạn** 5s/128MB vs 10s/64MB/200 dòng — xem Mục 5, điểm 2 |
| FR-9.5 | Lịch sử nộp + so sánh | **KHÔNG CÓ** | ✗ | Cần `/code/{key}/history` (danh sách nộp, xem code cũ, diff 2 lần nộp) (Màn N-7) |
| FR-9.6 | Sandbox giới hạn chi tiết | Hạ tầng | ⚠ | Gộp với FR-9.4 sau khi chốt con số |

### Module J — Gamification & Premium (7 FR giữ)

| FR | Tên | Màn/Route | Độ | Lỗ hổng cần xử lý |
|---|---|---|---|---|
| FR-10.1 | Tim & hồi & session | Màn 28 (modal Hết tim) + HeartsGemsWidget | ✓ | Đặc tả tốt (19.2 + 20.4) |
| FR-10.2 | Gems + Shop | Màn 22 `/shop` | ⚠ | THIẾU **màn Kho đồ + Equip** (19.9 nhắc "kho → equip" nhưng không màn) — Màn N-8 |
| FR-10.3 | Daily Quest | Màn 23 `/quests` | ✓ | — |
| FR-10.4 | Streak + freeze | **KHÔNG CÓ màn hiển thị** | ✗ | Streak/đếm ngày hiện ở đâu (profile/quest/widget)? Chưa đặc tả — gộp Màn N-1/N-4 |
| FR-10.5 | XP & Level | **KHÔNG CÓ màn hiển thị** | ✗ | Hiển thị level/XP ở profile + widget — gộp Màn N-1 |
| FR-10.6 | Leaderboard | Màn 24 `/leaderboard` | ⚠ | Theo 19.9 tầng 2: "bấm user → hồ sơ học tập" — Màn 24 không đặc tả hành động trên dòng |
| FR-10.7 | Premium + hết hạn | Màn 25/26/27 | ✓ | Màn 26 gộp vào 25 (modal 2 bước) — đã ghi rõ |

## 4. Màn hình cần BỔ SUNG đặc tả (16 màn mới)

| Mã | Route đề xuất | FR phục vụ | Ghi chú |
|---|---|---|---|
| N-1 | `/account/profile` + `/account/security` | 1.5, 1.7, 1.11, 3.10, 3.18, 10.4, 10.5 | Hồ sơ + bảo mật: đổi MK, 2FA, dark mode, danh sách yêu thích, level/streak |
| N-2 | `/forgot-password` + `/reset-password` | 1.6 | 2 route, 1 thiết kế; token 1 lần 30 phút |
| N-3 | `/simulations` (danh mục mô phỏng) | 3.1 | Lọc CTDL/GT/tag/mức độ; phân trang. **CẬP NHẬT v2.6 (12/08/2026)**: trở thành **Màn 33 "Khám phá"** trên sidebar chính — kèm tab "So sánh" (Benchmark Lab) + tab "CheatSheet"; mở mô phỏng cụ thể từ đây VẪN trừ tim theo 20.4 (trừ 3 demo công khai) |
| N-4 | `/account/achievements` | 5.5 | Trang Thành tích (đã mở/ẩn) |
| N-5 | `/admin/settings` | 6.2 | Cấu hình hệ thống (domain, policy MK, upload limit) |
| N-6 | `/admin/ladder` (hoặc tab trong 09) | 4.1, 4.5, 4.10, 9.3 | Soạn node: gắn Quiz (từ ngân hàng câu hỏi + import CSV), Lab, Code (signature + test ẩn) |
| N-7 | `/code/{key}/history` | 9.5 | Lịch sử nộp + so sánh 2 lần nộp |
| N-8 | `/shop/inventory` (tab) | 10.2 | Kho đồ + equip khung/avatar/theme, bán/xóa item |
| N-9 | Trang kết quả tìm kiếm | 2.5 | Hoặc dropdown gợi ý — cần chốt |
| N-10 | UI trong Màn 04 | 2.6, 7.4, 2.11 | Drawer ghi chú + đánh giá sao + nút "▶ Xem bước này" |
| N-11 | UI trong Màn 05 | 3.8, 3.12, 3.14, 3.15, 3.16, 2.11 | Menu tùy chọn hiển thị, nút Tự thực hành, panel call stack, breakpoint, banner mini quiz, nút "Xem lý thuyết liên quan" |
| N-12 | UI trong Màn 14 | 4.6, 4.7 | Nút "Luyện tập" + nút "Gợi ý" (Hint token) |
| N-13 | UI lịch sử nộp quiz/lab | 4.4 | Hoặc tab trong Màn 14 — cần chốt vị trí |
| N-14 | Tab "Chờ duyệt Teacher" | 1.8 | Đã có (Màn 29) — KHÔNG cần làm mới, chỉ xác nhận |
| N-15 | `/path/{topicId}/node/{nodeId}` — Node Hub | 2.4, 2.10, 2.11, 4.11 | Màn 31 (20.5.5): 3 tab Lý thuyết/Luyện tập/Cheatsheet, MỖI TAB 1 component tách (bài học 7.0) — điểm vào duy nhất cho luồng học B+C+D+I+J |
| N-16 | `/profile` — Hồ sơ | 5.2, 5.5, 1.5, 1.7, 3.18, 10.4, 10.5 | Màn 32 (20.5.5): 4 tab Tổng quan/Tiến độ (Màn 08)/Thành tích/Cài đặt; quest + BXH + shop vẫn route riêng |

## 5. Mâu thuẫn cần PM/chuyên gia CHỐT (6 điểm)

| # | Vấn đề | Phương án A | Phương án B | Nguồn xung đột |
|---|---|---|---|---|
| 1 | Độ dài mã mời lớp | **6 ký tự** | 8 ký tự | 19.1 + Màn 19 (6) vs FR-8.1 + UC-21 (8) |
| 2 | Giới hạn sandbox | **5s / 128MB** | 10s / 64MB / 200 dòng | FR-9.4 vs FR-9.6 + 19.1 |
| 3 | Thiết kế Benchmark Lab | **Multi-n (10/50/100/500/1000) + biểu đồ overlay lý thuyết tự fit** (UC-28, FR-3.20b, 19.5) | 2 canvas chạy song song cùng 1 dataset + biểu đồ cột (Màn 17 — giống FR-3.13 ĐÃ CẮT) | Màn 17 (20.2.2) vs UC-28/19.5 |
| 4 | Demo công khai | **3 demo** (FR-7.6) | 1 demo bubble sort (Màn 01) | FR-7.6 vs Màn 01 |
| 5 | Màn 07 (7.4, dự đoán bước độc lập) | **Xóa/ghi chú "đã sáp nhập vào Lab Màn 15"** (20.3) | Giữ màn độc lập | 7.4 vs 20.3 |
| 6 | Tham chiếu "Màn 03 (7.9.3)" trong 20.2.2 (Màn 21) | **Sửa thành "wireframe 7.9.3"** — tham chiếu đúng số hiệu | Giữ nguyên | 20.2.2 vs 7.9 |

> Khuyến nghị mặc định (nếu PM không có ý kiến): 1 → 6 ký tự; 2 → 10s/64MB/200 dòng; 3 → Multi-n + overlay (đúng tinh thần FR-3.20b "killer feature"); 4 → 3 demo; 5 → ghi chú sáp nhập; 6 → sửa số hiệu.

> ✅ **ĐÃ CHỐT & VÁ 09/08/2026** (chọn toàn bộ khuyến nghị mặc định): 1 → mã mời **6 ký tự** (đã sửa FR-8.1, UC-21, DB `InviteCode nvarchar(6)`); 2 → sandbox **10s/64MB/200 dòng** (đã sửa FR-9.4); 3 → Benchmark **multi-n + overlay lý thuyết tự fit** (đã viết lại Màn 17); 4 → **3 demo** (đã vá Màn 01); 5 → Màn 07 **ghi chú sáp nhập** vào Bậc 2 Lab; 6 → **sửa số hiệu** (wireframe 7.9.3).

## 6. Lỗ hổng khuôn đặc tả (phải vá khi sinh SRS/SDD)

1. **10 FR mới** (2.10, 3.20, 3.20b, 4.11, 4.12, 7.6, 9.6, 10.1→10.7): viết lại đặc tả đủ 7 thuộc tính theo 17.13 (Mô tả / Luồng / Ngoại lệ / AC mã hóa / Ràng buộc / Nguồn / Ghi chú). Dữ liệu nguồn: 19.2-19.4, 20.4, UC-25→32, 17.15.
2. **Màn 01-30 + 16 màn mới (Mục 4)**: đặc tả theo khuôn 17.14 — 10 mục/màn, trong đó bắt buộc: bảng Thành phần/Hành vi, bảng Tương tác/trạng thái nút, Trạng thái (loading/empty/error), Điều kiện truy cập (guard + trừ tim), Lỗi có thể gặp.
3. **19.9 (4 tầng)** chưa được chứng minh cho mọi FR — cần bổ sung: Leaderboard tầng 2 (bấm user → hồ sơ), Shop tầng 4 (kho → equip), Quest tầng 4 (streak), Benchmark tầng 4 (lưu kết quả), CheatSheet tầng 3-4, Code Runner tầng 4 (so sánh lần nộp).
4. **FR gốc thiếu AC mã hóa**: FR-1.1→9.5 mới có 4/7 thuộc tính (thiếu AC dạng AC-x.y.z, Ràng buộc, Nguồn, Ghi chú) — SRS phải hoàn thiện.

## 7. Khuyến nghị tích hợp AI (trả lời câu hỏi PM — ĐÃ CHỐT 09/08/2026)

### 7.1 Nguyên tắc chung
- KHÔNG thêm AI vào scope chính (20 tuần, 75 FR đã kín — tránh trôi dạt như bản cũ).
- AI chỉ ở mức **PoC GĐ3 / backlog**; mọi tính năng AI phải có **fallback offline** (không có mạng → vẫn chạy bằng template sẵn có) để demo phòng bảo vệ an toàn.
- AI **không chấm điểm, không sinh nội dung chính thức** — chỉ "nói/gợi ý"; điểm, thưởng do server tính (tránh câu hỏi "AI chấm sai thì sao").
- Tiêu thụ **Hint token / Gems** (hệ thống 19.3 ĐÃ CÓ) → chống spam, gắn kinh tế sản phẩm, không cần hạ tầng mới.

### 7.2 Bảng đánh giá vị trí AI tiềm năng

| Vị trí AI | Giá trị | Rủi ro | Dễ demo | Kết luận |
|---|---|---|---|---|
| A. Giải thích bước thông minh (Visualizer) | Cao — đúng lõi | Thấp (dữ liệu có từ StepExecutor) | Rất dễ | Hợp lý nhất — template + dữ liệu trước, LLM là nâng cấp |
| B. AI Tutor chat (hỏi lý thuyết, RAG trên bài học + trace) | Rất cao — "wow" | TB (API, latency, prompt injection) | Dễ nếu có mạng | Hợp lý nếu có fallback offline |
| C. AI giải thích lỗi code người học (Module I) | Cao — hợp chỗ chấm tự động | TB-Cao (gửi code ra ngoài) | Khá dễ | Hợp lý hạng 2 — tận dụng sandbox + test ẩn |
| D. AI sinh câu hỏi quiz cho Teacher | TB | Thấp nhất (Teacher duyệt lại) | Rất dễ | An toàn nhất, ít "ăn điểm" bảo vệ |
| E. AI viết kết luận Benchmark (từ số liệu đo thật) | TB | Rất thấp | Rất dễ | Nhỏ gọn — phụ kiện cho killer feature |
| F. AI đề xuất lộ trình cá nhân | TB | Thấp | Dễ | **KHÔNG gọi là AI** (chỉ heuristics — hỏi sâu là lộ) |

### 7.3 Chốt đề xuất: AI Assistant — 1 endpoint `/ai/ask`, 3 chế độ

Vị trí: **Màn 05 (mô phỏng) + Màn 16 (Code Runner)** — chạm đúng 2 killer feature (EDV, Code Runner):

1. **Giải thích bước** — mở rộng câu giải thích hiện tại của StepExecutor (prompt đóng, dữ liệu bước có sẵn).
2. **Giải thích lỗi code** — context = code người học + kết quả test ẩn (đã có trong sandbox) → AI nói lỗi ở đâu + gợi ý.
3. **Hỏi lý thuyết liên quan** — RAG mini trên chính bài học đang học (nội dung đã có trong DB, không cần hệ thống tìm kiếm mới).

Điều kiện bắt buộc: (1) tốn Hint token/Gems; (2) fallback = template hiện tại khi offline; (3) không chấm điểm, không sinh nội dung chính thức.
> Ghi vào backlog 16.2 (PRODUCTION_PROMPT.md) mục 9 — PoC GĐ3.

## 8. Thứ tự xử lý đề xuất

1. PM chốt 6 mâu thuẫn (Mục 5) — 30 phút.
2. Vá PRODUCTION_PROMPT.md (thêm Phần 22 hoặc sửa inline): đặc tả 10 FR mới theo 17.13 + 14 màn mới theo 17.14 + sửa 6 mâu thuẫn.
3. Sinh SRS/SDD/API_REFERENCE từ bản đã vá (theo thứ tự ưu tiên Phần 20/21 > 8 = 7 > 19 > 0-17).
4. Kiểm tra lần cuối bằng checklist 19.9: mỗi FR trả lời được "người dùng làm gì tiếp theo sau màn này?"

## 10. Cập nhật theo PRODUCTION_PROMPT v2.3-v2.5 (12/08/2026 — ảnh hưởng màn hình)

| # | Thay đổi | Ảnh hưởng màn hình |
|---|---|---|
| 1 | Seed giảm 18 → **8 bài** (19.6A) | Màn 03/13: danh sách 8 bài seed (Bubble, Binary Search, Stack, Linked List, BST, AVL, Hash, BFS); 10 bài còn lại backlog GĐ2 |
| 2 | Chấm Lab Bậc 2 = **trạng thái cuối + giới hạn bước ×1.5** (G-5) | Màn 15: bỏ "chấm trace từng bước"; hiển thị "Đã dùng x/Y"; hoàn tác không tính bộ đếm |
| 3 | Chấm code = **sandbox Web Worker client**, không Judge0 (G-6, ADR-012) | Màn 16: chạy/nộp trong trình duyệt; test ẩn không hiển thị qua API/UI |
| 4 | Benchmark dùng **`runMeasure` KHÔNG trace** (v2.5) | Màn 17: O(n²) tối đa n=500, O(n log n) tối đa n=1000; timeout 5s → "N/A" |
| 5 | Trừ tim **v2.5**: UPDATE điều kiện `ExpiresAt < @now` + @@ROWCOUNT chống double-spend cho cả session hết hạn | Màn 05/13/31: gọi `POST enter`; Màn 28 đếm ngược; mở từ CheatSheet vẫn trừ |
| 6 | **31 bảng** (thêm NodeSessions — v2.4) | Không đổi màn; ảnh hưởng backend + TEST_B-148..155 |
| 7 | Redirect `/learn` → `/path`, `/dashboard` → `/profile` (20.5.6) | Màn 03/08 thành redirect; Màn 13/32 là điểm đến |
| 8 | Sandbox giới hạn **10s/64MB/200 dòng** (FR-9.4/9.6) | Màn 16: thông báo giới hạn khi vượt |
| 9 | Mã mời lớp **6 ký tự** (FR-8.1) | Màn 19: ô nhập mã 6 ký tự |
| 10 | **3 demo công khai** (FR-7.6) | Màn 01: 3 thẻ demo Bubble/Binary/BFS |
| 11 | Màn 07 ghi chú sáp nhập Bậc 2 Lab (20.3) | Màn 07 không còn độc lập — tham chiếu engine |
| 12 | Nâng sao KHÔNG cấp XP (v2.5) | Màn 13: tooltip sao chỉ +gems |
| 13 | Premium downgrade clamp Hearts 10 (v2.4) | Màn 27: thông báo hết hạn + quyền lợi còn lại |

## 10A. Bảng tổng hợp 32 màn → route (nguồn: SDD §8.4 — 12/08/2026)

| Màn | Route | Nguồn FR | Màn | Route | Nguồn FR |
|---|---|---|---|---|---|
| 01 | `/` | FR-7.1, 7.6 | 17 | `/benchmark/{k1}/{k2}` | FR-3.20, 3.20b |
| 02 | `/login`, `/register` | FR-1.1, 1.2 | 18 | `/cheatsheet` | FR-2.10, 10.7 |
| 03 | `/learn` (redirect `/path`) | FR-2.3 | 19 | `/classes` | FR-8.1 |
| 04 | `/learn/{lessonId}` | FR-2.4, 2.6, 7.4, 2.11 | 20 | `/classes/{id}` | FR-8.2, 8.3 |
| 05 | `/simulator/{key}` | FR-3.2→3.16, 2.11 | 21 | `/classes/{id}/report` | FR-8.4, 5.3 |
| 06 | `/exercise/{id}` | FR-4.2, 4.6-4.9 | 22 | `/shop` | FR-10.2 |
| 07 | (sáp nhập Bậc 2) | FR-4.3 | 23 | `/quests` | FR-10.3 |
| 08 | `/dashboard` (redirect `/profile`) | FR-5.2 | 24 | `/leaderboard` | FR-10.6 |
| 09 | `/admin/lessons`, `/admin/topics` | FR-2.1, 2.2 | 25 | `/premium` | FR-10.7 |
| 10 | `/admin/users` | FR-1.9, 1.8 | 26 | (gộp 25 — checkout modal) | FR-10.7 |
| 11 | `/admin/stats` | FR-5.4 | 27 | `/account/subscription` | FR-10.7 |
| 12 | `/help`, `/privacy`, 404, 500 | FR-7.2 | 28 | (overlay — Hết tim) | FR-10.1 |
| 13 | `/path/{topicId}` | FR-2.10 | 29 | (tab 10 — Chờ duyệt Teacher) | FR-1.8 |
| 14 | `/ladder/{nodeId}` | FR-4.11 | 30 | `/path/{topicId}/final-test` | FR-4.12 |
| 15 | `/ladder/{nodeId}/lab` | FR-4.3 | 31 | `/path/{topicId}/node/{nodeId}` | FR-2.10, 2.11, 4.11 |
| 16 | `/code/{key}` | FR-9.1→9.3 | 32 | `/profile` | FR-5.2, 5.5, 1.5, 1.7, 3.18, 10.4, 10.5 |

> Cộng 16 màn bổ sung N-1..N-16 (Mục 4) đã được gộp: N-1/N-2/N-3/N-4/N-5/N-6/N-7/N-8/N-9/N-10/N-11/N-12/N-13/N-14/N-15/N-16 → chi tiết đặc tả tại SDD §8.4 (bảng 32 màn) — các tab con (N-10/N-11/N-12/N-13) nằm trong đặc tả Màn 04/05/14.

## 9. Cập nhật theo module_depth_review_v2.md (ĐÃ ÁP DỤNG)

> Tài liệu tham chiếu: `module_depth_review_v2.md` (gốc đặt tại root thư mục dự án — nên chuyển vào `docs/`). Phân loại độ sâu thật: C=6, I=5, J=5, D=4 (Bậc 2 mỏng → **đã vá**), B=4, A=4, H=3, E=3, G=2, F=2.

| # | Hành động | Trạng thái |
|---|---|---|
| 1 | Bổ sung đặc tả Bậc 2 Interactive Lab — Màn 15.1-15.3: 3 kịch bản bắt buộc (Sắp xếp/BST/Đồ thị) + **chấm trace TỪNG BƯỚC** (≥80% + trace cuối đúng) + giới hạn thao tác ×1.5 + Hoàn tác + gợi ý tốn Hint token | **ĐÃ VÁ** vào PRODUCTION_PROMPT.md §20.2.2 (Màn 15) + ghi chú FR-4.3 |
| 2 | Phân bổ nỗ lực khi sinh SRS/SDD: **C/I/J ~60%, A/B/D ~30%, E/F/G/H ~10%** — viết ngắn, chính xác cho module phẳng, đừng "phình" | Áp dụng khi sinh tài liệu |
| 3 | Seed data song song với code (19.6): ≥2 bài hoàn chỉnh/sprint; 18 bài phủ 10 CTDL×15 GT + golden data | Áp dụng trong roadmap |
| 4 | Trình bày thẳng khi bảo vệ: E/F/G/H là module hỗ trợ; G (demo) và H (báo cáo/BXH) MƯỢN giá trị từ C/I/J/E — thừa nhận, không cố "phình" | Ghi vào tài liệu trình bày |
| 5 | Module B phụ thuộc chất lượng seed — nếu seed kém, B/D/C đều "vỏ rỗng" dù FR đầy đủ | Giám sát ở sprint S3-S5 |
| 6 | Module D rủi ro dây chuyền (mượn C + I): nếu C/I delay → D chết theo — buffer trong kế hoạch | Ghi vào SDD mục Rủi ro |
| 7 | **Navigation theo vai trò** (phân tích "10 module kỹ thuật ≠ cách người dùng nhìn thấy"): GIỮ module A-J, thêm lớp ánh xạ module→menu — 4 vùng (Học & Luyện / Hồ sơ / Lớp / Khác), sidebar Student/Teacher/Admin, bảng Menu→Route→Module, sơ đồ lồng theo vùng, Màn 31 (Node Hub) + Màn 32 (Hồ sơ), redirect /learn→/path, /dashboard→/profile | **ĐÃ VÁ** vào PRODUCTION_PROMPT.md §20.5 + Phần 21 mục 6 — đưa đầy đủ vào SDD |
| 8 | Chốt 6 mâu thuẫn (Mục 5) — theo khuyến nghị mặc định: mã mời 6 ký tự, sandbox 10s/64MB/200 dòng, Benchmark multi-n + overlay, 3 demo, Màn 07 sáp nhập, tham chiếu wireframe 7.9.3 | **ĐÃ VÁ** vào PRODUCTION_PROMPT.md (FR-8.1, UC-21, DB InviteCode, FR-9.4, Màn 17, Màn 07, Màn 21, Màn 01, sơ đồ 7.0, Màn 13→Node Hub, Màn 05 nút lý thuyết, 20.2.5) |
| 9 | Audit lần 2 (rà lại phần chưa đọc): bổ sung số liệu chốt §19.10 (điểm node/sao⭐/final test ≥70%/điểm lộ trình 80-20), §19.6A (bảng 18 bài seed + 6 path × 5 node), §19.3A (8 quest templates), 11.4 (7 service mới H/I/J), 11.6 (MailHog dev), 12.2 (4 store mới), 12.4 (router guard đầy đủ), 10.2.25 (EXERCISES.NodeId+Stage), UC-07/UC-14, 14.2 (TEST-B-133..183), Phần 22 changelog, 20.3 FR-4.6 (luyện tập trong session) | **ĐÃ VÁ** — PRODUCTION_PROMPT.md nay đạt v2.2, sẵn sàng sinh SRS/SDD/API_REFERENCE |
| 10 | Chốt logic chấm Code Bậc 3 + testcase: chấm theo OUTPUT (không so implementation); code không trace-được vẫn chạy/nộp, chỉ bỏ visual bước; §19.6B | **ĐÃ VÁ** — seeder + TEST_PLAN lấy nguồn từ §19.6B |

## 11. Checklist rà soát màn hình trước khi bàn giao (12/08/2026)

- [ ] Mọi FR trong master matrix (SRS §3.1) có màn/route tương ứng (đối chiếu bảng 10A + Mục 3).
- [ ] Mọi màn đạt khuôn 17.14: Mục đích / Nguồn FR / Bố cục / Thành phần / Tương tác / Trạng thái / Phím tắt / Responsive / Điều kiện truy cập / Lỗi có thể gặp.
- [ ] Màn 05 (simulator) có wireframe ASCII (SDD §8.5) + giải thích 4 vùng bắt buộc.
- [ ] Màn 14 (Ladder) wireframe (SDD §8.6) — stepper 3 bậc tách component.
- [ ] Node Hub (Màn 31) + Hồ sơ (Màn 32): mỗi tab là 1 component TÁCH (A-5) — không vi phạm "1 màn 1 việc".
- [ ] Quy tắc trừ tim 20.4 hiện đúng ở: Màn 05 (vào simulator), Màn 31 (vào node), Màn 18 (CheatSheet → mở mô phỏng vẫn trừ), Màn 17 (Benchmark miễn phí), Màn 28 (Hết tim).
- [ ] Redirect `/learn` → `/path`, `/dashboard` → `/profile` (20.5.6).
- [ ] Sidebar theo vai trò (SDD §8.7) + HeartsGemsWidget ở header mọi trang.
- [ ] Độ sâu 19.9: mỗi tính năng trả lời được "người dùng làm gì tiếp theo?" (VD: Leaderboard bấm user → hồ sơ; Shop → kho → equip).
- [ ] Đối chiếu route thực tế `router/index.ts` vs bảng 10A — không route thừa/thiếu.
