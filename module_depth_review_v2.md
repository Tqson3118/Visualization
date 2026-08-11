# 🔬 ĐÁNH GIÁ ĐỘ SÂU v2 — PHÂN TẦNG THẬT, KHÔNG ĐÁNH ĐỒNG

**Bài học từ v1**: Cho mọi module đều "đạt 4 tầng" là đánh giá giả. Thực tế, một số module **đúng là phẳng** — và điều đó có thể chấp nhận được nếu nó là module phụ trợ. Vấn đề chỉ xảy ra khi module **lõi** lại phẳng.

---

## Phân loại trung thực: 3 nhóm

```
┌─────────────────────────────────────────────────────────┐
│  NHÓM 1 — SÂU THẬT (5-6 tầng tương tác)               │
│  Module C (Visualizer), I (Code Runner), J (Gamification)│
│  → Đây là nơi dự án KHÁC BIỆT với các hệ thống khác    │
├─────────────────────────────────────────────────────────┤
│  NHÓM 2 — ĐỦ DÙNG (3-4 tầng, có chiều sâu riêng)      │
│  Module A (Auth), B (Learning Path), D (Practice Ladder) │
│  → Có điểm nhấn riêng nhưng không phải lõi kỹ thuật     │
├─────────────────────────────────────────────────────────┤
│  NHÓM 3 — PHẲNG CÓ CHỦ ĐÍCH (1-2 tầng)                │
│  Module E (Tiến độ), F (Admin), G (Landing), H (Lớp)    │
│  → Phục vụ, không tạo giá trị mới — CẦN THỪA NHẬN      │
└─────────────────────────────────────────────────────────┘
```

---

## NHÓM 1 — SÂU THẬT

### Module C — Visualizer EDV ⭐⭐⭐⭐⭐⭐ (6 tầng)

Đây là module sâu nhất dự án, và **đáng lẽ phải thế** vì nó là lõi:

| Tầng | Nội dung | Chi tiết thực tế |
|------|----------|-----------------|
| 1 | Danh mục | Lọc CTDL/GT, tag, mức độ |
| 2 | Cấu hình đầu vào | 7 loại đầu vào khác nhau cho 7 CTDL (FR-3.4), validation riêng từng loại |
| 3 | Mô phỏng 3 vùng đồng bộ | Canvas + mã giả + giải thích, 7 trạng thái màu, con trỏ có nhãn |
| 4 | Điều khiển nâng cao | Play/pause/step/speed/seek, breakpoint có điều kiện (FR-3.15), phím tắt 7 phím |
| 5 | Chế độ tương tác | Manual Step Practice (FR-3.12) — người học PHẢI chọn thao tác đúng, có feedback sai/đúng |
| 6 | Benchmark Lab | Chạy THẬT 2+ GT, đo ms + so sánh/hoán đổi, overlay lý thuyết (FR-3.20b) |

**Tại sao thật sự sâu**: Không chỉ "hiển thị" mà có **vòng lặp tương tác** — người dùng thay đổi dữ liệu → thấy kết quả khác → so sánh → hiểu sâu hơn. Manual Step Practice biến xem thụ động thành làm chủ động. Benchmark Lab biến lý thuyết Big-O thành con số thật.

### Module I — Code Runner ⭐⭐⭐⭐⭐ (5 tầng)

| Tầng | Nội dung | Chi tiết |
|------|----------|---------|
| 1 | Editor Monaco | Highlight, code mẫu nạp sẵn, khôi phục |
| 2 | Chạy + visual đồng bộ 2 chiều | Click dòng → nhảy bước; chạy → highlight dòng. Đây là tính năng **trả lời trực tiếp** câu hỏi hội đồng |
| 3 | Xử lý lỗi chi tiết | Lỗi cú pháp/timeout/vòng lặp vô hạn → thông báo kèm **dòng lỗi cụ thể**, không treo trình duyệt |
| 4 | Bài tập code + chấm điểm | Test ẩn 10-20 case, chỉ hiện tên không lộ nội dung, pass/fail từng test |
| 5 | Lịch sử nộp + so sánh | Xem lại code cũ, so sánh 2 lần nộp, giảng viên xem bài nộp |

**Tại sao thật sự sâu**: Sandbox 2 tầng (Web Worker + Judge0), giới hạn tài nguyên cụ thể (10s/64MB/200 dòng). Không phải "nhúng 1 editor rồi gọi là xong" — có cả hệ thống chấm, bảo mật, và đồng bộ visual.

### Module J — Gamification & Premium ⭐⭐⭐⭐⭐ (5 tầng)

| Tầng | Nội dung | Chi tiết |
|------|----------|---------|
| 1 | Hệ Tim (hearts) | Atomic server-side, hồi theo thời gian, edge cases (multi-tab, chỉnh đồng hồ, session 30p) |
| 2 | Gems economy | Kiếm (pass node, quest, achievement) → Tiêu (shop 8+ items, giá cụ thể) → giao dịch atomic |
| 3 | Daily Quest + Streak | 5 quest/ngày (2E+2M+1H), streak = hoạt động thực (login không tính), freeze max 2 |
| 4 | XP/Level + Leaderboard | Công thức Level cụ thể, anti-grinding (retry không nhận XP), BXH 3 tab |
| 5 | Premium lifecycle | Checkout mô phỏng → kích hoạt → hết hạn → downgrade job (giữ gems/items, mất quyền lợi) |

**Tại sao thật sự sâu**: Có **kinh tế nội bộ** hoàn chỉnh (kiếm → tích → tiêu → hiệu ứng). Mỗi hệ con (Tim, Gems, Quest, XP) có **số liệu chốt** và **edge cases** cụ thể, không phải placeholder.

---

## NHÓM 2 — ĐỦ DÙNG (có chiều sâu riêng nhưng không phải lõi kỹ thuật)

### Module A — Auth ⭐⭐⭐⭐ (4 tầng, nhưng là "table stakes")

| Tầng | Nội dung |
|------|----------|
| 1 | Đăng ký/đăng nhập (JWT + refresh) |
| 2 | Phân quyền RBAC 3 vai trò (40 dòng ma trận) |
| 3 | Bảo vệ: khóa sau 5 lần sai, rate limit, refresh token HttpOnly |
| 4 | Phê duyệt Teacher, đổi/quên mật khẩu |

**Đánh giá thật**: Module A **được đặc tả tốt** nhưng không tạo giá trị khác biệt — bất kỳ LMS nào cũng có những thứ này. Nó sâu về **bảo mật** (NFR-8→15) nhưng không sâu về **trải nghiệm người dùng**. Hội đồng sẽ không ấn tượng với auth — họ sẽ ấn tượng nếu auth **không có lỗi**.

> [!NOTE]
> **Không cần thêm sâu**. Auth nên ổn định, không cần sáng tạo. Đặc tả hiện tại đủ.

### Module B — Học tập & Learning Path ⭐⭐⭐⭐ (4 tầng, nhờ Learning Path cứu)

| Tầng | Nội dung | Nhận xét thật |
|------|----------|--------------|
| 1 | Cây chủ đề + danh sách bài học | Chuẩn LMS, không khác biệt |
| 2 | Chi tiết bài học (rich text + MathJax) | Chuẩn LMS, không khác biệt |
| 3 | **Learning Path (bản đồ node)** | ĐÂY mới là điểm sâu — node khóa/mở, popover, sao ⭐ |
| 4 | **Two-way sync + CheatSheet** | Deep-link `?step=N` giữa lý thuyết ↔ simulator |

**Đánh giá thật**: Nếu bỏ Learning Path và Two-way sync, Module B chỉ là **CRUD bài học tiêu chuẩn** (2 tầng). Learning Path + Two-way sync **cứu** module này lên 4 tầng. CheatSheet (bảng Big-O tương tác) cũng thêm giá trị — nhưng bản chất vẫn là **bảng có liên kết**, không phải tương tác phức tạp.

> [!IMPORTANT]
> **Rủi ro**: Nếu seed data kém (ít bài, lý thuyết sơ sài, không có mô phỏng đính kèm) → Module B sẽ bị lộ ngay là "vỏ bọc rỗng". Phần 19.6 yêu cầu 18 bài phủ 10 CTDL×15 GT — đây là nỗ lực **nội dung**, không phải kỹ thuật.

### Module D — Practice Ladder ⭐⭐⭐⭐ (4 tầng, nhưng 2/3 bậc mượn từ module khác)

| Tầng | Nội dung | Tự xây hay mượn? |
|------|----------|-------------------|
| 1 | Stepper 3 bậc (UI Màn 14) | Tự xây — UI mới |
| 2 | Bậc 1: Quiz (≥60%) | **MƯỢN** FR-4.2 engine quiz có sẵn |
| 3 | Bậc 2: Interactive Lab (kéo thả) | **TỰ XÂY** — component mới, canvas editable |
| 4 | Bậc 3: Code Challenge (≥70%) | **MƯỢN** Module I engine code |

**Đánh giá thật**: Practice Ladder là **killer feature về mặt sư phạm** (chứng minh hiểu từng cấp), nhưng **về mặt kỹ thuật**, chỉ Bậc 2 (Interactive Lab) là thực sự mới. Bậc 1 và 3 tái sử dụng engine đã có.

Bậc 2 (Lab) hiện **mô tả còn mỏng nhất** trong prompt:
- "Cho phép kéo-thả phần tử/thao tác" — kéo thả CÁI GÌ cụ thể? Hoán đổi 2 ô? Chọn pivot? Chèn node vào cây?
- "Server chấm bằng so khớp trace" — trace cuối hay trace từng bước?
- Màn 15 chỉ nói "chế độ editable" — nhưng editable như thế nào cho từng CTDL?

> [!WARNING]
> **Đây là điểm yếu sâu nhất của prompt**: Bậc 2 (Lab) là tính năng **chưa từng tồn tại** trong bản cũ, là cái mới nhất, nhưng lại được mô tả **ít chi tiết nhất**. Cần bổ sung đặc tả cho ít nhất 3-4 kịch bản Lab cụ thể (VD: Lab sắp xếp, Lab chèn BST, Lab duyệt đồ thị).

---

## NHÓM 3 — PHẲNG CÓ CHỦ ĐÍCH (thừa nhận thẳng thắn)

### Module E — Tiến độ & Báo cáo ⭐⭐⭐ (3 tầng — chủ yếu ĐỌC, ít TƯƠNG TÁC)

| Tầng | Nội dung | Vấn đề |
|------|----------|--------|
| 1 | Dashboard KPI (thẻ tổng quan) | Chỉ **hiển thị số**, không tương tác |
| 2 | Tiến độ theo topic (progress bar) | Chỉ **hiển thị thanh**, bấm vào → về bài học (không phải tương tác mới) |
| 3 | Báo cáo giảng viên + xuất CSV | **Đọc bảng + bấm xuất** — tầng tương tác thấp nhất |

**Đánh giá thật**: Module E là module **chỉ đọc (read-only)**. Người dùng không "làm" gì mới ở đây — chỉ xem dữ liệu mà các module khác (C, D, I) đã tạo ra. Biểu đồ điểm theo thời gian (FR-5.2) thêm 1 chút giá trị trực quan, nhưng bản chất vẫn là hiển thị.

**Huy hiệu (FR-5.5)** từng có thể tạo chiều sâu nhưng nó **không có vòng lặp hành vi** rõ ràng — nhận huy hiệu → xong, không ảnh hưởng gameplay (không như XP/Level ở Module J ảnh hưởng Leaderboard).

> **Có chấp nhận được không?** CÓ — vì Module E là module **hỗ trợ**. Tiến độ không cần sâu, nó cần **chính xác**. Nhưng khi viết SRS/SDD, **đừng cố** làm cho E có vẻ phức tạp hơn bản chất — trình bày thẳng: "module hiển thị tổng hợp dữ liệu từ các module lõi".

### Module F — Admin ⭐⭐ (2 tầng — cố ý tối giản)

| Tầng | Nội dung |
|------|----------|
| 1 | Bảng quản lý users (lọc/phân trang) |
| 2 | Chi tiết + thao tác (khóa/duyệt/reset) |

**Đánh giá thật**: 2 FR (FR-6.2 cấu hình, FR-1.9 quản lý users — thuộc F theo RBAC). Phần 3.6 tuyên bố rõ "Module F tinh gọn theo chủ đề" và loại trừ audit UI, health check. Đây **đúng là phẳng** và **đúng là có chủ đích**.

> **Không cần sửa**. Admin tối giản cho đồ án 4 người, 20 tuần là hợp lý.

### Module G — Trang phụ trợ ⭐⭐ (2 tầng — landing + FAQ)

| Tầng | Nội dung | Nhận xét |
|------|----------|----------|
| 1 | Landing page + 3 demo | Demo là embedding Module C — G không tự có engine |
| 2 | FAQ | Trang tĩnh |

**Đánh giá thật**: Module G **mượn** giá trị từ Module C (3 demo visualizer). Bản thân G chỉ là **shell tĩnh** bọc ngoài. Không phải vấn đề — landing page không cần sâu. Nhưng đừng đếm "3 demo" là chiều sâu **của G** — đó là chiều sâu của C.

### Module H — Lớp học phần ⭐⭐⭐ (3 tầng — CRUD + Join + Report mượn từ E)

| Tầng | Nội dung | Tự xây hay mượn? |
|------|----------|-------------------|
| 1 | CRUD lớp + mã mời 6 ký tự | Tự xây — nhưng là CRUD chuẩn |
| 2 | Gán lộ trình + hạn nộp | Tự xây — tương tác thật (gán + deadline) |
| 3 | Báo cáo lớp + BXH lớp | **MƯỢN** từ E (tiến độ) + J (leaderboard) |

**Đánh giá thật**: Module H là **"keo dán"** giữa E và J — nó tổ chức dữ liệu theo lớp thay vì theo cá nhân. Giá trị của H nằm ở **ngữ cảnh lớp học** (hạn nộp, đúng hạn/trễ/chưa nộp), không phải ở kỹ thuật phức tạp.

> **Có chấp nhận được không?** CÓ — nhưng cần thừa nhận H là module **tổ chức dữ liệu**, không phải module tạo trải nghiệm mới.

---

## 📊 Bảng tổng hợp trung thực

| Module | Tầng sâu thật | Tự xây / mượn | Giá trị khác biệt | Rủi ro "phẳng" |
|--------|---------------|---------------|-------------------|----------------|
| **C — Visualizer** | **6** | 100% tự xây | ⭐⭐⭐⭐⭐ Lõi kỹ thuật | ✅ Không |
| **I — Code Runner** | **5** | 90% tự xây | ⭐⭐⭐⭐⭐ Trả lời hội đồng | ✅ Không |
| **J — Gamification** | **5** | 100% tự xây | ⭐⭐⭐⭐ Động lực học | ✅ Không |
| **D — Ladder** | **4** | 40% tự, 60% mượn C+I | ⭐⭐⭐⭐ Killer feature sư phạm | ⚠️ Bậc 2 mỏng |
| **B — Learning** | **4** | 60% chuẩn LMS | ⭐⭐⭐ Nhờ Learning Path | ⚠️ Phụ thuộc seed |
| **A — Auth** | **4** | 100% chuẩn ngành | ⭐⭐ Table stakes | ✅ Không (không cần sâu) |
| **H — Lớp** | **3** | CRUD + keo dán E/J | ⭐⭐ Ngữ cảnh lớp học | ⚠️ Bỏ E/J thì rỗng |
| **E — Tiến độ** | **3** | Chỉ đọc dữ liệu | ⭐⭐ Hỗ trợ | ⚠️ Read-only |
| **G — Landing** | **2** | Shell tĩnh + mượn C | ⭐ Marketing | ✅ Không (không cần sâu) |
| **F — Admin** | **2** | CRUD chuẩn | ⭐ Vận hành | ✅ Không (cố ý tối giản) |

---

## 🎯 Vấn đề thật sự cần lo (không phải "nhiều mà không sâu")

Prompt **không** bị lỗi "nhiều mà không sâu" — nó bị lỗi **lệch sâu**:

### 1. Top 3 module (C, I, J) chiếm ~70% độ phức tạp kỹ thuật
Đây là **đúng** — hệ thống nên tập trung vào lõi. Nhưng rủi ro: nếu C hoặc I delay, toàn bộ dự án bị ảnh hưởng (D mượn từ cả C lẫn I).

### 2. Module D (Practice Ladder) — Bậc 2 (Lab) là "hố đen" đặc tả

> [!CAUTION]
> **Đây là điểm yếu nghiêm trọng nhất cần bổ sung ngay.**

Hiện tại Bậc 2 (Interactive Lab) chỉ có:
- "Tái sử dụng VisualizationCanvas ở chế độ editable" (Màn 15)
- "Kéo-thả phần tử/thao tác"
- "Server chấm bằng so khớp trace"

**Cần bổ sung**:
- **Lab Sắp xếp**: Người học kéo thả 2 ô để hoán đổi? Hay chọn 2 phần tử rồi bấm "Swap"? Cho phép bao nhiêu thao tác tối đa?
- **Lab BST**: Người học chọn vị trí chèn trên cây? Hay chọn nút con trái/phải?
- **Lab Đồ thị**: Người học chọn đỉnh kế tiếp để duyệt (BFS/DFS)? Hay chọn cạnh relax (Dijkstra)?
- **Cơ chế chấm**: So khớp trace CUỐI (kết quả) hay từng bước (quá trình)?

### 3. Module E, H — đủ dùng nhưng đừng cố "nâng cấp giả"

Khi viết SRS/SDD, **thừa nhận thẳng** rằng E và H là module hỗ trợ. Đừng viết 10 trang đặc tả cho dashboard khi bản chất nó chỉ là "query + hiển thị". Viết ngắn gọn, chính xác, dành thời gian cho C/D/I/J.

### 4. Chất lượng Seed Data > Số lượng FR

Module B có 11 FR nhưng giá trị thật nằm ở **nội dung** 18 bài học, 15 mã GT chạy được, golden data test. Nếu seed data kém → cả B, C, D đều "vỏ rỗng" dù FR đầy đủ.

---

## ✅ Khuyến nghị cuối cùng

| # | Hành động | Vì sao |
|---|-----------|--------|
| 1 | **Bổ sung đặc tả Bậc 2 (Lab)** — ít nhất 3 kịch bản cụ thể | Lỗ hổng đặc tả lớn nhất |
| 2 | **Phân bổ thời gian SRS/SDD theo tỷ lệ sâu thật** — C/I/J chiếm 60%, A/B/D 30%, E/F/G/H 10% | Tránh viết dài đều cho module phẳng |
| 3 | **Seed data song song** (19.6) — mỗi sprint phải có ≥2 bài hoàn chỉnh | Module B/D rỗng nếu không có nội dung |
| 4 | **Thừa nhận module phẳng** khi trình bày — hội đồng sẽ tôn trọng sự trung thực hơn cố "phình" | Tránh bị hỏi "phần này có gì đặc biệt?" mà không trả lời được |
