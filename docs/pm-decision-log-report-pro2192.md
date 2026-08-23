# PM Decision Log — PROMPT_REPORT_PRO2192 (Fill báo cáo vào template PRO2192)

- Session: `PROMPT_REPORT_PRO2192` — chế độ CHÍNH XÁC TUYỆT ĐỐI (6 GATE đo bằng máy)
- Ngày: 14/08/2026 — Người soạn: dev-docs (commit-as: phuc)
- Nhánh: `docs/report-pro2192` (base: `dev`) — PR base `dev`
- Input: `tailieu/PRO2192_Report (3).docx` (template, 813KB — READ ONLY) + `tailieu/BAO_CAO.md` (151KB, 2301 dòng — READ ONLY)
- Output: `tailieu/BaoCaoDoAn_PRO2192.docx` (file MỚI, không ghi đè)

## 1. QUYẾT ĐỊNH LỚN (ghi TRƯỚC khi code)

### Q1 — Text heading: dùng text MD, style/format từ TEMPLATE
- **Quyết định**: Mọi heading H1/H2/H3 trong output lấy **text từ BAO_CAO.md** (chuẩn nội dung 100%), giữ **style + format + thứ tự** từ heading template tương ứng (clone node).
- **Lý do**: GATE 2 yêu cầu "mọi H1/H2/H3 của BAO_CAO.md hiện diện 100%" (đối chiếu text). Heading template như "3.2.2 Use Cases dành cho khách hàng", "4.4 Sơ đồ lớp DAO", "5.1 Database", "5.2 Layout", "5.5 Servives (Bussiness Logic Layer)" là placeholder chung của FPT, không khớp nội dung md → nếu giữ text template sẽ FAIL GATE 2.
- **Hệ quả**: Số heading trùng lặp trong template (3.2.2 x2, 4.3.1 x2 — lỗi template) tự hết vì text md dùng số chuẩn (3.2.2/3.2.3/3.2.4; 4.3.1/4.3.2). Đây là hệ quả bắt buộc của GATE 2, **không coi là task "sửa lỗi template"** (không đụng styles.xml/format). Backlog ghi ở mục 5.
- Ngoại lệ H1: `KẾT LUẬN` (template) → `KẾT LUẬN & HƯỚNG PHÁT TRIỂN` (md) — giữ style Heading 1, giữ vị trí thứ 9.

### Q2 — Heading md không có trong template → THÊM MỚI (clone style heading cùng cấp)
- Thêm H3: `3.2.3 Use Cases dành cho giảng viên`, `4.4.1 Kiến trúc backend 2 lớp`, `4.4.2 Simulation Engine EDV`, `4.4.3 Máy trạng thái mô phỏng` (style Heading 3 — clone từ H3 mẫu template).
- Thêm H2 (dưới H1 KẾT LUẬN & HƯỚNG PHÁT TRIỂN): `Kết quả đạt được`, `Khó khăn & Bài học kinh nghiệm`, `Hướng phát triển` (style Heading 2).
- Thêm H2 (dưới H1 PHỤ LỤC): `PHỤ LỤC A: Hướng dẫn cài đặt môi trường`, `PHỤ LỤC B: Phím tắt + thuật ngữ`, `PHỤ LỤC C: Thư viện bên thứ ba (license)`, `PHỤ LỤC D: Danh mục mô phỏng (catalog)` (md dùng H1 → hạ xuống Heading 2 để nằm dưới khung H1 `PHỤ LỤC` — theo prompt "dùng Heading 1/2 style phù hợp").

### Q3 — Heading template không có trong md → 5.4.3 Class Diagram
- Template có H3 `5.4.3 Class Diagram`, md không có mục này. **Quyết định**: giữ heading (text md không có nên giữ text template), điền 1 đoạn ngắn trỏ về nội dung thật (class diagram EDV đã trình bày ở 4.4.2) — KHÔNG để "…" (GATE 2 cấm). Backlog: bổ sung class diagram riêng nếu hội đồng yêu cầu.

### Q4 — Heading md cấp 4 (####) — không chèn heading
- Md có `####` (UC-01/UC-25/UC-26 ở 3.3.2; Màn 01-32 ở 4.2.3). GATE 2 chỉ yêu cầu H1/H2/H3 → **H4 chuyển thành đoạn văn in đậm** (style Normal + run bold), không tạo heading mới.

### Q5 — Code fence (```) — xử lý 2 loại
- **`mermaid`**: KHÔNG chèn (docx không render được mermaid; các sơ đồ quan trọng đã có ảnh PNG riêng). Skip toàn bộ block mermaid.
- **Code khác** (typescript/csharp/bash/yaml/nginx/ini/sql/text): chèn thành các đoạn văn style Normal, font giữ nguyên theo node mẫu (Times New Roman 13 VN), mỗi dòng 1 paragraph — không dùng font monospace để không lệch format template (GATE 6).

### Q6 — Danh sách: đánh số/bullet THỦ CÔNG bằng text
- Template List Paragraph (45) không đảm bảo numPr → **KHÔNG thêm numPr** (đụng numbering.xml sẽ FAIL GATE 1 + rủi ro Word repair).
- Danh sách `-` → paragraph style Normal, prefix `• `; danh sách số `1.` → prefix `1. `, `2. `... (style Normal, font mẫu). Ghi chú: nếu sau này cần list tự đánh số, phải thêm numId vào numbering.xml (backlog).

### Q7 — Bảng: clone node từ bảng mẫu template theo số cột
- Trích 1 bản mỗi loại TRƯỚC khi xóa: 2 cột = T0 (5x2), 3 cột = T6 (3x3), 4 cột = T2 (11x4), 5 cột = T8 (15x5), 7 cột = T1 (32x7, dự phòng).
- Giữ nguyên tblPr/tblGrid (style viền, độ rộng cột); đổ dữ liệu md: dòng 1 = header (giữ format dòng header mẫu), các dòng sau clone row mẫu.
- Số cột md khác mẫu → thêm/bớt cell theo gridCol của bảng mẫu (mẫu 3 cột cho bảng md 3 cột, v.v. — khớp sẵn).
- Tiêu đề bảng dạng `**Bảng X.Y: ...**` → đoạn văn in đậm phía trên bảng.

### Q8 — Ảnh
- 18 ảnh md → chèn bằng `run.add_picture` (python-docx), resolve đường dẫn tương đối theo thư mục `tailieu/`, width ≤ 6 inch (giữ tỷ lệ), căn giữa (clone paragraph mẫu có alignment center).
- Chú thích `*Hình X.Y: ...*` (italic trong md) → paragraph style Normal, run italic (giữ text đầy đủ của md, kể cả "(ảnh placeholder...)" — nội dung md giữ nguyên 100%).
- Media output = 19 (template cũ) + 18 (mới) = **37** (GATE 3).

### Q9 — Guidance template: XÓA toàn bộ
- Với mỗi vùng giữa 2 heading template (cùng cấp hoặc cao hơn), xóa mọi paragraph/table con (guidance + placeholder + bảng mẫu), trừ heading. Đảm bảo GATE 2: guidance sót = 0.
- KHÔNG đụng: phần trước H1 đầu tiên (cover — có thể là textbox/shape), header/footer, "Ngày bảo vệ: ………………" trên cover, "TP.HCM, ngày 12 tháng 8 năm 2026".

### Q10 — Cover / trang đầu md (dòng 1-36 của BAO_CAO.md)
- KHÔNG dùng: template đã có cover riêng (giữ nguyên). Bảng thành viên trên cover md không chèn (bảng 1.2 trong PHẦN 1 đã có đầy đủ).

### Q11 — TOC/field: giữ nguyên, không đụng (prompt: để nguyên field, Word tự cập nhật F9).

## 2. BẢNG ÁNH XẠ SECTION (md → template) — đầy đủ

Ký hiệu: **[THÊM]** = heading mới theo style template; **[ĐỔI TEXT]** = giữ style/vị trí heading template, thay text bằng text md; **backlog** = ghi mục 5.

| # | Heading TEMPLATE (giữ style/thứ tự) | Section MD nguồn (BAO_CAO.md) | Loại | Ghi chú |
|---|---|---|---|---|
| 1 | H1 `LỜI MỞ ĐẦU` | `# LỜI MỞ ĐẦU` (dòng 38-48) | map | 4 đoạn + (nguồn…) |
| 2 | H1 `PHẦN 1: GIỚI THIỆU ĐỀ TÀI` | `# PHẦN 1: GIỚI THIỆU ĐỀ TÀI` | map | guidance H1 (P/L) xóa |
| 3 | H2 `1.1 Giới thiệu dự án` | `## 1.1 Giới thiệu dự án` (52-79) | map | đoạn + list số + **Bảng 1.1** (3c) |
| 4 | H2 `1.2 Ban dự án` | `## 1.2 Ban dự án` (81-101) | map | **Bảng 1.2** (4c) + list vai trò |
| 5 | H1 `PHẦN 2: KHẢO SÁT – SURVEY` | `# PHẦN 2` (103) | map | guidance xóa |
| 6 | H2 `2.1 Yêu cầu của khách hàng` | `## 2.1 Yêu cầu của khách hàng` (105-159) | map | **Bảng 2.1** (4c), **2.2** (3c), list NFR |
| 7 | H2 `2.2 Kế hoạch dự án` | `## 2.2 Kế hoạch dự án` (161-200) | map | **Bảng 2.3** (4c), **2.4** (3c) |
| 8 | H1 `PHẦN 3: PHÂN TÍCH - ANALYSIS` | `# PHẦN 3` (203) | map | guidance xóa |
| 9 | H2 `3.1 Mô hình triển khai hệ thống` | `## 3.1 Mô hình triển khai hệ thống` (205-246) | map | **Bảng 3.1** (3c); mermaid skip |
| 10 | H2 `3.2 Sơ đồ Use Cases` | `## 3.2 Sơ đồ Use Cases` (248) | map | |
| 11 | H3 `3.2.1 Tổng quan` | `### 3.2.1 Tổng quan` (250-338) | map | ẢNH 01 (`diagrams/01-usecase-tong-quan.png`) |
| 12 | H3 `3.2.2 Use Cases dành cho khách hàng` → **[ĐỔI TEXT]** `3.2.2 Use Cases dành cho người học` | `### 3.2.2 Use Cases dành cho người học` (340-433) | map | ẢNH 02 + **Bảng 3.2** (3c) |
| 13 | — (không có) **[THÊM H3]** `3.2.3 Use Cases dành cho giảng viên` | `### 3.2.3 Use Cases dành cho giảng viên` (435-466) | thêm | ẢNH 03 + **Bảng 3.3** (3c) |
| 14 | H3 `3.2.2 Use Cases dành cho quản trị` → **[ĐỔI TEXT]** `3.2.4 Use Cases dành cho quản trị viên` | `### 3.2.4 Use Cases dành cho quản trị viên` (468-493) | map | ẢNH 04 + **Bảng 3.4** (3c) |
| 15 | H2 `3.3 Đặc tả yêu cầu hệ thống (SRS)` | `## 3.3` (495) | map | |
| 16 | H3 `3.3.1 Chi tiết use cases dành cho khách hàng` → **[ĐỔI TEXT]** `3.3.1 Ma trận yêu cầu chức năng` | `### 3.3.1 Ma trận yêu cầu chức năng` (497-581) | map | **Bảng 3.5** (5c, 75 FR) |
| 17 | H3 `3.3.2 Chi tiết use cases dành cho quản trị` → **[ĐỔI TEXT]** `3.3.2 Đặc tả use case hạt nhân` | `### 3.3.2 Đặc tả use case hạt nhân` (583-608) | map | 3x H4 (UC-01/25/26) → bold para |
| 18 | H1 `PHẦN 4: THIẾT KẾ - DESIGN` | `# PHẦN 4` (611) | map | guidance xóa |
| 19 | H2 `4.1 Mô hình công nghệ` | `## 4.1 Mô hình công nghệ` (613-654) | map | **Bảng 4.1** (3c); mermaid skip |
| 20 | H2 `4.2 Thiết kế giao diện` | `## 4.2` (656) | map | |
| 21 | H3 `4.2.1 Sitemap` | `### 4.2.1 Sitemap` (658-704) | map | **Bảng 4.2** (3c); mermaid skip |
| 22 | H3 `4.2.2 Layout` | `### 4.2.2 Layout` (706-735) | map | **Bảng 4.3** (2c), **4.4** (2c) |
| 23 | H3 `4.2.3 Giao diện chức năng` | `### 4.2.3 Giao diện chức năng` (737-846) | map | 12x H4 Màn → bold; ẢNH 07-18 (12 ảnh screenshots) |
| 24 | H2 `4.3 Thiết kế dữ liệu` | `## 4.3` (848) | map | |
| 25 | H3 `4.3.1 Sơ đồ quan hệ thực thể (ERD)` | `### 4.3.1 Sơ đồ quan hệ thực thể (ERD)` (850-950) | map | ẢNH 05, 06; mermaid skip |
| 26 | H3 `4.3.1 Chi tiết thực thể` → **[ĐỔI TEXT]** `4.3.2 Chi tiết thực thể (Data Dictionary)` | `### 4.3.2 Chi tiết thực thể (Data Dictionary)` (952-1368) | map | 6 nhóm bold + **Bảng 4.5-4.36** (32 bảng 5c) |
| 27 | H2 `4.4 Sơ đồ lớp DAO` → **[ĐỔI TEXT]** `4.4 Thiết kế phần mềm` | `## 4.4 Thiết kế phần mềm` (1369) | map | |
| 28 | — **[THÊM H3]** `4.4.1 Kiến trúc backend 2 lớp` | `### 4.4.1` (1371-1391) | thêm | mermaid skip |
| 29 | — **[THÊM H3]** `4.4.2 Simulation Engine EDV` | `### 4.4.2` (1393-1477) | thêm | code ts → para; mermaid classDiagram skip |
| 30 | — **[THÊM H3]** `4.4.3 Máy trạng thái mô phỏng` | `### 4.4.3` (1479-1500) | thêm | mermaid skip |
| 31 | H1 `PHẦN 5: THỰC HIỆN – IMPLEMENT` | `# PHẦN 5` (1502) | map | guidance xóa |
| 32 | H2 `5.1 Database` → **[ĐỔI TEXT]** `5.1 Cơ sở dữ liệu` | `## 5.1 Cơ sở dữ liệu` (1504-1536) | map | code csharp → para; list |
| 33 | H2 `5.2 Layout` → **[ĐỔI TEXT]** `5.2 Simulation Engine & Sandbox` | `## 5.2 Simulation Engine & Sandbox` (1538-1581) | map | code text + **Bảng 5.1** (3c) |
| 34 | H2 `5.3 Sơ đồ kiến trúc công nghệ` | `## 5.3` (1583-1628) | map | code text (2 cây thư mục) |
| 35 | H2 `5.4 Các loại sơ đồ` → **[ĐỔI TEXT]** `5.4 Các loại sơ đồ tương tác` | `## 5.4` (1630) | map | |
| 36 | H3 `5.4.1 Sequence Diagram` | `### 5.4.1 Sequence Diagram` (1632-1679) | map | mermaid skip; chú thích italic *Hình 5.1/5.2* |
| 37 | H3 `5.4.2 Activity Diagram` | `### 5.4.2 Activity Diagram` (1681-1719) | map | mermaid skip; *Hình 5.3/5.4* |
| 38 | H3 `5.4.3 Class Diagram` | — (md không có) | giữ+điền ngắn | đoạn ngắn trỏ 4.4.2 (Q3); backlog |
| 39 | H2 `5.5 API` → **[ĐỔI TEXT]** `5.5 API Endpoints` | `## 5.5 API Endpoints` (1721) | map | |
| 40 | H3 `5.5.1 Controllers` | `### 5.5.1 Controllers` (1723-1750) | map | **Bảng 5.2** (4c, 19 endpoint) |
| 41 | H3 `5.5.2 Servives (Bussiness Logic Layer)` → **[ĐỔI TEXT]** `5.5.2 Services (Business Logic)` | `### 5.5.2 Services (Business Logic)` (1752-1789) | map | **Bảng 5.3** (2c) + đoạn quy trình + mermaid skip + *Hình 5.5* |
| 42 | H1 `PHẦN 6: KIỂM THỬ - TESTING` | `# PHẦN 6` (1791) | map | guidance + bảng mẫu T8 xóa |
| 43 | H2 `6.1 Chiến lược kiểm thử` | `## 6.1` (1793-1812) | map | **Bảng 6.1** (3c) |
| 44 | H2 `6.2 Kết quả kiểm thử` | `## 6.2` (1814-1845) | map | **Bảng 6.2** (5c), **6.3** (4c) |
| 45 | H2 `6.3 Hiệu năng + bảo mật + UX` | `## 6.3` (1847-1878) | map | **Bảng 6.4** (4c), **6.5** (4c) + đoạn UX |
| 46 | H1 `PHẦN 7: ĐÓNG GÓI & TRIỂN KHAI` | `# PHẦN 7` (1881) | map | guidance xóa |
| 47 | H2 `7.1 Đóng gói frontend/backend` | `## 7.1` (1883-1927) | map | code bash/yaml → para + **Bảng 7.1** (3c) |
| 48 | H2 `7.2 Triển khai production` | `## 7.2` (1929-2021) | map | code nginx/ini/bash + **Bảng 7.2** (2c); mermaid skip |
| 49 | H2 `7.3 CI/CD + backup` | `## 7.3` (2023-2057) | map | **Bảng 7.3** (2c), **7.4** (2c); code sql |
| 50 | H2 `7.4 Runbook sự cố` | `## 7.4` (2059-2078) | map | **Bảng 7.5** (4c) |
| 51 | H1 `KẾT LUẬN` → **[ĐỔI TEXT]** `KẾT LUẬN & HƯỚNG PHÁT TRIỂN` | `# KẾT LUẬN & HƯỚNG PHÁT TRIỂN` (2080-2130) | map | guidance xóa |
| 52 | — **[THÊM H2]** `Kết quả đạt được` | `## Kết quả đạt được` (2082-2099) | thêm | **Bảng 7.6** (3c) |
| 53 | — **[THÊM H2]** `Khó khăn & Bài học kinh nghiệm` | `## Khó khăn & Bài học kinh nghiệm` (2101-2117) | thêm | bold + list số |
| 54 | — **[THÊM H2]** `Hướng phát triển` | `## Hướng phát triển` (2119-2130) | thêm | list số |
| 55 | H1 `TÀI LIỆU THAM KHẢO` | `# TÀI LIỆU THAM KHẢO` (2132-2144) | map | list 11 mục |
| 56 | H1 `PHỤ LỤC` | — (khung) | map | guidance "…" xóa |
| 57 | — **[THÊM H2]** `PHỤ LỤC A: Hướng dẫn cài đặt môi trường` | `# PHỤ LỤC A...` (2146-2185) | thêm | **Bảng A.1** (3c) + code |
| 58 | — **[THÊM H2]** `PHỤ LỤC B: Phím tắt + thuật ngữ` | `# PHỤ LỤC B...` (2187-2224) | thêm | **Bảng B.1** (2c), **B.2** (2c) |
| 59 | — **[THÊM H2]** `PHỤ LỤC C: Thư viện bên thứ ba (license)` | `# PHỤ LỤC C...` (2226-2246) | thêm | **Bảng C.1** (3c) |
| 60 | — **[THÊM H2]** `PHỤ LỤC D: Danh mục mô phỏng (catalog)` | `# PHỤ LỤC D...` (2248-2301) | thêm | **Bảng D.1** (3c, 44 mô phỏng) |

### Thống kê ánh xạ
- Section map (template có sẵn): 45 dòng có sẵn → 41 map trực tiếp + 4 map đổi text H2/H3 (Q1).
- **[THÊM]**: 11 heading mới (3 H3 nhóm use case/4.4; 3 H2 kết luận; 4 H2 phụ lục).
- **[ĐỔI TEXT]**: H2/H3 8 dòng (3.2.2, 3.2.2-qtrị, 3.3.1, 3.3.2, 4.3.1-cttt, 4.4, 5.1, 5.2, 5.4, 5.5, 5.5.2, KẾT LUẬN) — style giữ nguyên.
- **Backlog**: 5.4.3 Class Diagram (giữ heading, nội dung ngắn), H4 không phải heading, cover md không dùng, "Ngày bảo vệ: ....." chưa điền, bảng C.1 license chưa đầy đủ (nội dung md tự ghi), list không tự đánh số (numPr), TOC field cần F9.

## 3. KỸ THUẬT FILL (theo prompt Bước 3)
- CLONE NODE XML: mọi paragraph/table mới = `copy.deepcopy` từ node mẫu template (Normal/List Paragraph/Heading 1/2/3 + bảng mẫu 2c/3c/4c/5c), clear run cũ, đổ text/run mới — KHÔNG dùng `add_paragraph`/`add_table` thuần.
- Xóa: với mỗi vùng section (giữa 2 heading), xóa mọi `w:p`/`w:tbl` con (trừ heading chính).
- Ảnh: `run.add_picture` width ≤ 6 inch, paragraph căn giữa (clone mẫu centered nếu có, không thì set alignment CENTER + keep font).
- Lưu output `tailieu/BaoCaoDoAn_PRO2192.docx`.

## 4. VERIFY (GATE) — script `verify_report.py` in PASS/FAIL từng gate
- GATE 1: zipfile so byte-to-byte 7 part (styles.xml, numbering.xml, settings.xml, theme/*, fontTable.xml, header*.xml, footer*.xml, webSettings.xml nếu có) — phải IDENTICAL.
- GATE 2: (a) mọi H1/H2/H3 md hiện diện trong text output (normalize whitespace, match chuỗi); (b) guidance template sót = 0 (paragraph output không khớp bất kỳ paragraph guidance template — trích từ dump, normalize) + grep "…" trong body paragraphs = 0.
- GATE 3: word/media = 37; 18 ảnh md map đủ.
- GATE 4: Word COM mở readonly (throw = corrupt) + export PDF 3 trang → `docs/work/report-pro2192/preview/`.
- GATE 5: 3 đoạn sample (LỜI MỞ ĐẦU, PHẦN 1, PHẦN 5) == BAO_CAO.md (so chuỗi, bỏ whitespace).
- GATE 6: 3 section mẫu — style paragraph đúng (Normal/Heading), font run = mẫu (Times New Roman 13 VN), không run Calibri/null.
- Kích thước output 1.5-5MB; 11 H1 đúng thứ tự.
