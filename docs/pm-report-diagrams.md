# PM REPORT — Diagrams draw.io (Phiên --auto 13/08/2026 — LẦN 2)

## 1. MỤC TIÊU
Dựng 6 sơ đồ draw.io theo session/PROMPT_DIAGRAMS_DRAWIO.md + điều kiện user chốt 13/08: 28 UC hiển thị (gộp UC-06+07 → "Làm bài tập", loại UC-14/22/26, rename 7 UC), include/extend đều nét đứt + mũi tên hở (UML 2.5.1 §18.1.4), bố cục 0 chéo, palette teal #0D9488; ERD 32 bảng theo SDD §7.1/§7.2. Làm việc trên worktree feature/diagrams (commit được), KHÔNG merge dev. BƯỚC 1-2 đã xong phiên trước (style-guide/notes/samples) — không research lại.

## 2. TRẠNG THÁI TỪNG TASK
| Task | Nội dung | Agent | Kết quả |
|---|---|---|---|
| T0 | Test hạ tầng (PONG + đọc file) | dev-docs | DONE (PONG-OK — hạ tầng hoạt động) |
| T1 | Style-guide cập nhật v0.2 (include/extend nét đứt, ellipse #F0FDFA, boundary #CBD5E1/#F8FAFC, STYLE-UC-GROUP) + task-spec mới + decision log | pm | DONE |
| T2 | 01-usecase-tong-quan.drawio | dev-docs ×3 + dev ×1 | **FAIL** (4 lần trả kết quả rỗng, không tạo file — đã làm đủ 5 bước: bỏ nhúng ✓, test nhỏ ✓, tách 1 ảnh/task ✓, resume ✓; task 02-06 cùng cấu trúc THÀNH CÔNG → lỗi cục bộ task 01, không phải hạ tầng) |
| T3 | 02-usecase-hoc-vien.drawio (19 UC, 6 include + 2 extend) | dev-docs | DONE cơ bản — **THIẾU 19 edge actor→UC** (task sửa bổ sung fail 2 lần rỗng → ghi tồn đọng) |
| T4 | 03-usecase-giang-vien.drawio (7 UC, 1 extend) | dev-docs | DONE (6/6 verify PASS, 0 chéo) |
| T5 | 04-usecase-admin.drawio (3 UC, 0 quan hệ) | dev-docs | DONE (5/5 verify PASS) |
| T6 | 05-erd-tong-quan.drawio (32 bảng) | dev-docs (gen-erd.mjs) | DONE (32 bảng đúng SDD, 10 edge, XML parse OK) |
| T7 | 06-erd-chi-tiet.drawio (9 bảng) | dev-docs (gen-erd.mjs) | DONE (9 bảng đúng, 11 edge, XML parse OK) |
| T8 | Export PNG (draw.io CLI 31.1.8) | pm | DONE 5/5 (02: 248KB, 03: 124KB, 04: 72KB, 05: 204KB, 06: 191KB) — 01 không có .drawio nên KHÔNG export |
| T9 | Verify máy (đếm UC/bảng) | pm | PASS (chi tiết §4) |
| T10 | Verify mắt (Ollama qwen2.5vl:3b) | pm | PASS 5/5 (chi tiết §5) |
| T11 | Commit feature/diagrams | pm | DONE (c949ce9) |

## 3. FILE THAY ĐỔI
- **tailieu/diagrams/** (worktree feature/diagrams + đã sync sang working tree chính): 5 .drawio mới (02,03,04,05,06) + 5 PNG mới; XÓA 6 SVG cũ + 01 PNG/SVG cũ (user xóa chủ ý — vẽ mới từ đầu, không phục hồi bản cũ). **01-usecase-tong-quan chưa có (FAIL).**
- **docs/work/**: diagram-style-guide.md (v0.2 — 23KB), diagram-task-spec.md (mới — dữ liệu dựng 6 ảnh), mxfile.xsd (tải từ drawio-mcp), gen-erd.mjs, vision-diagram-*.txt (5 file), small-05/06.png.
- **docs/**: pm-decision-log-diagrams.md (append 7 quyết định), pm-report-diagrams.md (file này).

## 4. VERIFY MÁY (nguồn sự thật — grep/parse XML)
- 02 = 19 UC đúng danh sách (01,04,05,06,08,16,17,18,19,21,23,24,25,27,28,29,30,31,32) — ✅
- 03 = 7 UC (04,05,09,10,11,20,24) — ✅; 04 = 3 UC (12,13,24) — ✅
- Union 02+03+04 = **25 UC hiển thị**; thiếu UC-02/03/15 (nằm ở ảnh 01 chưa tạo) → tổng đúng 28 khi có ảnh 01. Cấm mã UC-07/14/22/26: KHÔNG xuất hiện (✅).
- ERD: 05 = **32 bảng** (24 lõi + 8 gamification/code, đúng tên SDD), 06 = **9 bảng** chi tiết — ✅; XML parse 5/5 file OK.
- 0 dây chéo: 03 (0), 04 (n/a — 3 edge đơn), 02 (8 edge quan hệ — 0 chéo theo báo cáo task); ERD script self-check 21/21 edge hợp lệ.
- mxfile.xsd: đã tải (29KB) — dùng để validate chuẩn; các file đã parse OK bằng [xml].

## 5. VERIFY MẮT (Ollama qwen2.5vl:3b — đã pull sẵn)
- 02: đọc được 19 UC + mô tả đúng hệ thống — ✅
- 03: đọc được 7 UC + extend UC-11→20 — ✅
- 04: đọc được 3 UC + actor Admin — ✅
- 05: nhận diện đúng 2 khối (24 + 8 bảng) — ✅ (retry với ảnh resize 1400px — ảnh gốc 1624px bị model từ chối)
- 06: phân tích đúng quan hệ Users↔PremiumSubscriptions, Lessons↔NodeSessions... — ✅
- Không phát hiện chữ cắt/lỗi font/rối trong 5 ảnh.

## 6. QUYẾT ĐỊNH ĐÃ GHI (docs/pm-decision-log-diagrams.md)
1. Không research lại; cập nhật style-guide v0.2 theo quyết định 14:10 (include lẫn extend nét đứt, ellipse nền #F0FDFA, boundary #CBD5E1/#F8FAFC, thêm STYLE-UC-GROUP).
2. Task prompt chỉ trỏ đường dẫn file (bài học phiên trước — không nhúng 37KB).
3. Làm việc tại worktree D:\FPT\neww-diagrams (feature/diagrams — commit được); không merge dev.
4. Task 01 FAIL 4 lần → chuyển agent dev retry cuối → vẫn rỗng → FAIL chính thức (giới hạn --auto: quá 2 lần = FAIL, không tự làm thay, không đổi thiết kế).
5. Task sửa 02 (thêm 19 edge actor→UC) FAIL 2 lần → ghi tồn đọng.
6. Công cụ export: draw.io CLI AppData\Local\Programs\draw.io\draw.io.exe (31.1.8) — verify tồn tại; Ollama qwen2.5vl:3b sẵn có.

## 7. VIỆC CÒN TỒN ĐỌNG (chờ user quyết)
1. **Ảnh 01-usecase-tong-quan CHƯA TẠO** (FAIL 4 lần) — cần "làm lại 01" (thử lại lần sau, có thể thử cách khác: prompt rút gọn hơn/script generator như ERD).
2. **Ảnh 02 thiếu 19 edge actor→UC** (association) — file dùng được (19 UC + 8 quan hệ đúng) nhưng chưa chuẩn UML; cần task sửa.
3. Sửa refs BAO_CAO.md nếu trỏ ảnh cũ (chưa đụng — G_Phu đang xử lý docs).
4. Build docx verify (pandoc) — chờ khi đủ 6 ảnh.

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu 'làm lại <task/mục>' kèm ghi chú, PM chạy lại phần đó.
