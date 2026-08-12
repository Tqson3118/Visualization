# PROMPT BỔ SUNG — Use Case + ERD theo MẪU draw.io (research → phân tích XML → apply dữ liệu dự án)

Dán vào `/pm "..." --auto` (⚠ **CHẾ ĐỘ LOCAL-ONLY**: G_Phu đang chạy trên dev — KHÔNG tạo nhánh mới, KHÔNG merge, KHÔNG commit vào dev; chỉ làm việc ở working tree + commit vào nhánh `feature/diagrams` hiện có nếu cần, hoặc để file local và ghi log):

```
Đọc session/HANDOFF_2026-08-13-UI.md + tailieu/diagram-prompts.md (dữ liệu actor/UC) + docs/SRS.md §5.1 (UC-01..32 + phân bổ actor) + docs/SDD.md §7.1 (ERD 24 bảng mermaid) + §7.2 (ERD Gamification 8 bảng mermaid — ĐÂY LÀ NGUỒN DỮ LIỆU ERD CHUẨN 32 BẢNG, không tự bịa quan hệ).

⚠ RÀNG BUỘC CHẠY (do G_Phu đang chạy song song):
- Làm việc LOCAL: toàn bộ đầu ra (mẫu .drawio tải về, style-guide, 6 file .drawio, 6 PNG) đặt trong docs/work/diagrams-*/ + tailieu/diagrams/ — KHÔNG đụng file code, KHÔNG sửa BAO_CAO.md lúc này (G_Phu task 8 đang sửa docs — tránh conflict); ghi refs cần sửa vào docs/work/diagram-notes.md để đợt sau apply.
- KHÔNG tạo nhánh mới, KHÔNG merge dev. Nếu commit thì vào nhánh feature/diagrams (đã tồn tại, có commit cũ) — hoặc để local, ghi log là được.
- Khi G_Phu merge xong → đợt sau rebase/apply refs BAO_CAO.md.

QUY TRÌNH 3 BƯỚC (bắt buộc — KHÔNG vẽ từ số 0):

## BƯỚC 1 — RESEARCH MẪU (dev-docs + webfetch)
1. Lấy mẫu .drawio CHÍNH THỐNG: github.com/jgraph/drawio-templates → thư mục `uml/` (use case) + `er/` hoặc `database/` (ERD) → tải 2-3 file mẫu về `docs/work/diagram-samples/` (dùng raw.githubusercontent.com). Nếu không tìm thấy mẫu use case trong repo template → tìm trong drawio `resources/templates` (UML class/use case có sẵn) hoặc ghi rõ lý do + tự dựng theo chuẩn UML.
2. Research chuẩn UML use case (1 trang ghi chú docs/work/diagram-notes.md): quy tắc «include» (nét liền, tam giác rỗng, hướng UC gọi → UC bị include) + «extend» (nét đứt, hướng UC mở rộng → UC gốc, kèm điều kiện); actor bên ngoài khung, UC trong khung hệ thống.
3. Research ERD notation: crow's foot / chen (nếu mẫu dùng loại nào thì theo loại đó; mermaid SDD đang dùng crow's foot `||--o{`).

## BƯỚC 2 — PHÂN TÍCH XML MẪU (dev-docs, ghi vào docs/work/diagram-xml-notes.md)
1. Mở file .drawio mẫu (định dạng XML mxGraphModel): liệt kê cấu trúc — phần tử <mxCell> cho shape (vertex, style="...;shape=actor/ellipse/table...") và edge (style="...;edgeStyle=...;dashed=1"), phần <mxGeometry> (x/y/width/height), <mxPoint> (anchor), style strings cụ thể cho: actor hình người que, use case elip, khung hệ thống, bảng ERD (3 dòng tên/PK/cột), mũi tên include/extend.
2. Chốt STYLE CHUẨN cho dự án (copy nguyên style string từ mẫu, đổi màu sang teal #0D9488 + mã màu chuẩn): viết vào docs/work/diagram-style-guide.md — file này là chuẩn duy nhất để dựng 6 ảnh.

## BƯỚC 3 — APPLY DỮ LIỆU DỰ ÁN → 6 file .drawio XML → export PNG (dev-docs)
1. USE CASE 4 ảnh (dữ liệu từ docs/SRS.md §5.1 — mã UC GIỮ NGUYÊN, **tên hiển thị theo BẢNG RENAME user chốt 13/08** — mọi thay đổi tên ghi decision log):
   - **RENAME + GỘP/LOẠI (user chốt 13/08 — mã giữ nguyên; mọi thay đổi ghi decision log)**:
     - GỘP: **UC-06 + UC-07 → "Làm bài tập"** (trắc nghiệm + dự đoán bước chung 1 UC; ghi chú deprecated UC-07 trong diagram-notes.md).
     - LOẠI khỏi mọi ảnh (ghi chú lý do vào diagram-notes.md): **UC-14** (demo — không cần thiết) · **UC-22** (ghi chú cá nhân) · **UC-26** (Ladder — các bậc đã có UC riêng: trắc nghiệm = UC-06, code = UC-18).
     - RENAME tên hiển thị: UC-17→"Chạy code trong sandbox" · UC-18→"Nộp code" · UC-19→"Xem lịch sử bài làm" · UC-24→"Gửi phản hồi và báo lỗi" · UC-25→"Học theo lộ trình" · UC-29→"Nhiệm vụ hằng ngày" · UC-31→"Xem bảng xếp hạng". Các UC còn lại giữ tên gốc.
   - **CẤU TRÚC PHÂN TẦNG (bắt buộc)**:
     - **Ảnh 01-tổng quan**: CHỈ UC chính đại diện nhóm module (gọi thẳng từ actor) — **TUYỆT ĐỐI KHÔNG include/extend ở ảnh này**. 3 actor + UC chính mỗi nhóm, chỉ nét liền actor→UC.
     - **Ảnh 02 (Người học)**: KHÔNG có UC-02/UC-03 (thuộc Khách — precondition đã đăng nhập). **CÓ UC-01 "Chạy mô phỏng"** — đích của include (UC-06→UC-01, UC-29→UC-01); nó là "xem thuật toán" — UC trung tâm của hệ thống, không trùng vai trò ảnh tổng quan (ảnh chi tiết = UC + quan hệ, ảnh tổng quan = nhóm).
     - **Ảnh 03 (Giảng viên)**: UC-04,05,09,10,11,20,24 + UC-11→UC-20.
     - **Ảnh 04 (Admin)**: UC-12,13,24.
     - QUAN HỆ «include» (chỉ trong ảnh actor): UC-03←UC-02 · UC-01←UC-04 · UC-06←UC-01 (UC-06 gồm dự đoán bước — cần xem mô phỏng) · UC-27←UC-06+UC-18 · UC-18←UC-17 · UC-29←UC-01.
     - QUAN HỆ «extend» (nét đứt, kèm điều kiện): UC-15→UC-03 · UC-30→UC-03 (chỉ vẽ nếu UC-03 có mặt trong ảnh — nếu không, bỏ + ghi note) · UC-32→UC-03 · UC-24→UC-16 · UC-19→UC-18 · UC-11→UC-20.
     - KHÔNG CHẮC → không vẽ mũi tên + ghi docs/work/diagram-notes.md (tuyệt đối không tự bịa).
2. **MÀU SẮC (user chốt — KHÔNG sáng/basic)**: dùng palette trung tính + teal chủ đạo #0D9488 (đậm vừa, không neon): nền khung hệ thống trắng/`#F8FAFC` (light) · khung viền `#CBD5E1` · actor `#334155` (xám đậm, không đen) · UC elip viền `#0D9488` + chữ `#0F172A` + nền `#F0FDFA` (teal 50) · nhóm module nền `#F1F5F9` viền đứt `#94A3B8` · mũi tên include `#0D9488` nét liền / extend `#94A3B8` nét đứt. CẤM màu neon/đỏ tươi/nhiều màu lòe loẹt.3. **BỐ CỤC CHỐNG CHÉO DÂY (user chốt — bắt buộc, thay kiểu "4 UC 1 cột")**:
   - Actor trái, UC chia thành **2-3 CỘT DỌC** bên phải (KHÔNG 1 cột dài), mỗi cột 1 nhóm module; trong cột xếp **theo thứ tự luồng thao tác** (UC chính trước, UC include ngay DƯỚI UC gọi nó — dây include ngắn thẳng đứng).
   - Quy tắc chống chéo: (a) UC có quan hệ (include/extend) đặt GẦN nhau cùng cột — KHÔNG tách 2 cột xa; (b) dây chỉ nối theo chiều ngang trong cột hoặc thẳng đứng liền kề; (c) extend vẽ đi vòng ngoài (bên phải cột) nếu chéo; (d) sau khi dựng: đếm số cặp dây chéo trong XML (edge intersect) — nếu > 0 cặp chéo trong cùng 1 nhóm → ĐIỀU CHỈNH vị trí UC (đổi thứ tự cột) tới khi 0 chéo; (e) nhóm quá chật (nhiều UC + quan hệ) → tách 2 ảnh cùng style.
   - Mục tiêu đo đếm được: **0 dây chéo** trong mỗi ảnh (kiểm bằng script intersect) + mọi dây dài ≤ 2 hàng.
2. ERD 2 ảnh (dữ liệu từ SDD §7.1 + §7.2 — 32 bảng, PK/FK/cardinality giữ NGUYÊN): 05-er-tổng-quan (32 bảng gom 2 khối: lõi học tập 24 + gamification 8, quan hệ chính giữa khối) + 06-er-chi-tiết (bảng chi tiết: Users, Lessons, Exercises, LearningPathNodes, NodeSessions, UserProgress, ClassMembers, CodeRuns, PremiumSubscriptions — bảng 3 dòng: tên/PK/cột theo SDD).
3. Tạo 6 file .drawio XML theo style-guide (thư mục tailieu/diagrams/).
4. EXPORT PNG 1920×1080: cài draw.io nếu chưa có (`winget install JGraph.Draw`) → `drawio -x -f png -o <out> <file>.drawio` (hoặc npm `drawio-headless` nếu winget fail — ghi rõ tool dùng). Xác nhận 6 PNG tồn tại, kích thước đúng.
5. Build lại docx: pandoc tailieu/BAO_CAO.md -o tailieu/BaoCaoDoAn.docx --toc (C:\Users\Administrator\AppData\Local\Pandoc\pandoc.exe) — kiểm tra BAO_CAO.md trỏ ảnh qua đường dẫn nào (placeholders/ hay diagrams/) để thay đúng.

## VERIFY (dev-test + dev-review)
- 6 PNG render được, không lỗi font/tràn chữ (mở bằng Playwright xác nhận file PNG non-trivial).
- **REVIEW CHÉO 2 TẦNG (bắt buộc — không chỉ nhìn ảnh)**:
  a) Máy (nguồn sự thật — grep): đếm UC-01..UC-32 trong file .drawio XML ↔ SRS §5.1 (đủ 32, đúng tên, đúng actor); đếm 32 bảng trong ERD ↔ SDD §7.1/§7.2 (đủ PK/FK/cardinality); include/extend khớp bảng quyết định — viết kết quả đếm vào docs/work/diagram-notes.md.
  b) Mắt (Ollama vision): gửi 6 PNG cho qwen2.5vl:3b (base64 → http://localhost:11434/api/generate) — prompt: "Mô tả sơ đồ này: nhãn đọc được không, mũi tên nào rõ/rối, bố cục cân không, chữ bị cắt không" → lưu docs/work/vision-diagram-*.txt; điểm bất thường → sửa lại SVG/XML rồi export lại.
- Mở ảnh bằng Playwright (file://) chụp lại để xác nhận render chuẩn (không cần MCP mới).
- dev-review: tổng hợp 2 tầng trên + tiếng Việt không sai.
- Lưu bảng: ảnh | nguồn mẫu | file .drawio | file PNG | ghi chú.

Quy trình: CHẾ ĐỘ LOCAL — không merge, không tạo branch; commit (nếu có) vào feature/diagrams. Commit: drawio/docs → phuc, script export → thu. Ghi log: docs/pm-report-diagrams.md + docs/pm-decision-log-diagrams.md. Việc còn chờ (sửa BAO_CAO.md refs, merge) ghi vào docs/work/diagram-notes.md. --auto
```
