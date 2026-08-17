# PM DECISION LOG — USE CASE UPGRADE (include/extend + gom nhóm) — 12/08/2026

> Session theo session/PROMPT_USECASE_UPGRADE.md. Chạy trên nhánh feature/diagrams (theo file — không đụng code app). User lệnh: "đổi qua feature/ux-foundation lại... làm xong khoan đổi nhánh và push".

## [2026-08-12 03:50] Diễn giải lệnh user + chọn nhánh
- Quyết định: (1) Đổi về feature/ux-foundation trước (đã làm — khôi phục trạng thái nhánh cho session G song song). (2) Task trong PROMPT_USECASE_UPGRADE.md chạy trên feature/diagrams (file quy định rõ) — đã switch. (3) Sau khi xong: KHÔNG merge vào dev, KHÔNG push — dừng tại feature/diagrams, báo cáo để user quyết.
- Ảnh hưởng: toàn bộ session; không có commit merge/push.

## [2026-08-12 03:50] Áp dụng bảng quan hệ include/extend của PROMPT
- Quyết định: Dùng NGUYÊN VĂN 8 «include» (A) + 8 «extend» (B) trong file: include — UC-03←02, UC-01←04, UC-07←01, UC-26←07+18, UC-27←06+18, UC-18←17, UC-29←01, UC-23←31 (UC-23←31: file nói "chưa chắc chắn → ghi chú tham khảo và vẽ nét đứt dashed" → vẽ dashed + ghi notes). extend — UC-15→03 (đk quên mật khẩu), UC-14→01 (đk chưa đăng nhập), UC-30→03 (đk hết gems — giữ extend, ghi rõ), UC-22→04 (đk bật ghi chú), UC-32→03 (đk chọn nâng cấp), UC-24→16 (tùy chọn từ màn bài học), UC-19→18 (sau khi nộp), UC-11→20 (chỉ GV có lớp).
- Ảnh hưởng: 4 SVG use case.

## [2026-08-12 03:50] Gom nhóm module + 2 UC chưa được gom
- Quyết định: Gom theo 7 nhóm của file: Xác thực & tài khoản (02,03,15,12,13) · Học tập (04,05,25,16,22) · Mô phỏng (01,14,28,07) · Bài tập & Code (06,17,18,19,10) · Lớp học (20,21,11) · Gamification (23,26,27,29,31,30,32) · Phản hồi (24). UC-08 (Xem tiến độ cá nhân) và UC-09 (Biên soạn bài học) KHÔNG nằm trong nhóm nào của file → theo quy tắc C: vẽ như UC độc lập (không gom, không tự bịa nhóm) + ghi vào docs/work/diagram-notes.md.
- Ảnh hưởng: 01-tổng quan (32 UC: 30 gom + 2 độc lập); các ảnh theo actor dùng cùng nhóm nhưng chỉ chứa UC của actor đó.

## [2026-08-12 03:50] Kỹ thuật vẽ — tránh lỗi task lớn đã gặp
- Quyết định: Viết SVG bằng SCRIPT GENERATOR python (cách đã thành công ở Session A2) thay vì viết trực tiếp file SVG cỡ lớn (đã fail 3 lần trước). Script sinh 4 SVG, tự verify. Nếu agent trả kết quả rỗng → resume session (cách đã hoạt động).
- Ảnh hưởng: 4 file SVG 01-04.

## [2026-08-12 03:50] Word vẫn đang mở BaoCaoDoAn.docx
- Quyết định: Build docx verify bằng tên BaoCaoDoAn_new.docx (không ghi đè file đang khóa, không đổi tên output chính, không giết Word). SETUP_TODO mục 9 đã ghi từ Session A2.
- Ảnh hưởng: tailieu/BaoCaoDoAn_new.docx (verify).

## [2026-08-12 03:50] Commit cuối session
- Quyết định: Commit trên feature/diagrams theo phân công: SVG/docs/log → phuc (commit-as.ps1), script render (nếu đổi) → thu. KHÔNG merge, KHÔNG push (lệnh user "khoan").
- Ảnh hưởng: git history local feature/diagrams.

## [2026-08-13 HH:MM] Chạy --auto theo session/PROMPT_DIAGRAMS_DRAWIO.md
- Quyết định: Chạy chế độ --auto, LOCAL-ONLY (G_Phu đang chạy song song trên dev): KHÔNG tạo nhánh mới, KHÔNG merge, KHÔNG commit vào dev; hiện đang ở nhánh feature/ux-review2 → KHÔNG đổi nhánh, làm việc ở working tree, để local + ghi log (đúng phương án prompt cho phép).
- Ảnh hưởng: toàn bộ đầu ra (mẫu, style-guide, 6 .drawio, 6 PNG, log) nằm local; không có commit/push.

## [2026-08-13 HH:MM] Nguồn mẫu chính thống
- Quyết định: repo github.com/jgraph/drawio-templates KHÔNG tồn tại (404) → dùng repo chính jgraph/drawio tại src/main/webapp/templates/: uml/uml_1.xml + uml/uml_2.xml (use case/class), software/entity_relationship.xml + software/database_1.xml (ERD/bảng 3 dòng). Đây là templates chính thức của draw.io.
- Ảnh hưởn: docs/work/diagram-samples/, style-guide Bước 2.

## [2026-08-13 HH:MM] Kế thừa dữ liệu đợt A2
- Quyết định: KHÔNG tự bịa quan hệ — kế thừa NGUYÊN VĂN 8 «include» + 8 «extend» + 7 nhóm gom + UC-08/09 độc lập đã chốt trong docs/pm-decision-log-diagrams.md (đợt A2). Dữ liệu UC/bảng lấy từ SRS §5.1 + SDD §7.1/§7.2 + tailieu/diagram-prompts.md.
- Ảnh hưởng: 4 file .drawio use case (include nét liền, extend nét đứt kèm điều kiện).

## [2026-08-13 HH:MM] Đầu ra và refs BAO_CAO.md
- Quyết định: 6 .drawio + 6 PNG mới GHI ĐÈ lên 6 PNG/SVG cũ trong tailieu/diagrams/ (bản cũ còn trong git history feature/diagrams). KHÔNG sửa BAO_CAO.md lúc này (G_Phu task 8 đang sửa) — refs cần sửa ghi vào docs/work/diagram-notes.md. Build docx verify bằng tên BaoCaoDoAn_new.docx (không ghi đè file đang mở).
- Ảnh hưởng: tailieu/diagrams/*.drawio + *.png, docs/work/diagram-notes.md, tailieu/BaoCaoDoAn_new.docx.

## [2026-08-13 HH:MM] Công cụ export
- Quyết định: Cài draw.io CLI qua winget (JGraph.Draw) để export PNG 1920x1080; nếu winget fail → fallback npm drawio-headless. Ghi rõ tool dùng trong báo cáo.
- Ảnh hưởng: Bước 4 prompt, docs/pm-report-diagrams.md.

## [2026-08-13 HH:MM] Task 4 (ERD) fail 2 lần kết quả trống → đổi phương pháp
- Quyết định: dev-docs trả về rỗng 2 lần cho task dựng 2 file ERD .drawio (05, 06) → áp dụng bài học A2 (đã ghi trong decision log cũ): KHÔNG viết XML tay cỡ lớn, dùng SCRIPT GENERATOR (node) sinh 2 file XML, chạy + verify; tách thành 1 task duy nhất với prompt ngắn gọn hơn, yêu cầu báo cáo bắt buộc. Nếu vẫn fail → task FAIL, ghi log, chuyển sang làm thủ công bằng dev generic.
- Ảnh hưởng: tailieu/diagrams/05-erd-tong-quan.drawio + 06-erd-chi-tiet.drawio, docs/work/.

## [2026-08-13 HH:MM] Export PNG: tool + kích thước
- Quyết định: winget (lỗi mạng) + drawio-headless (socket hang up) đều fail → tải MSI JGraph.Draw 31.1.8 qua mirror ghfast.top, cài silent msiexec /qn, export bằng draw.io CLI `-x -f png --scale 1 --width 1920 --height 1080` (chiều cao giữ 1080, chiều rộng theo content thực vì bố cục cell tràn nhẹ: 01=2034, 02=2023, 03=1848, 04=2366, 05=2403x1339, 06=2527). Không ép méo. Ghi rõ trong report.
- Ảnh hưởng: 6 PNG trong tailieu/diagrams/, docs/pm-report-diagrams.md.

## [2026-08-13 14:10] Phiên diagrams --auto (PROMPT_DIAGRAMS_DRAWIO.md) — dựng lại 6 ảnh theo quyết định mới
- Quyết định: User chốt chạy --auto và "bỏ qua mấy cái cũ bị lỗi" → BỎ 6 file .drawio + PNG/SVG cũ (bản 32 UC cũ, include nét liền, tên cũ) — DỰNG LẠI TOÀN BỘ 6 file theo quyết định chốt 13/08 (28 UC hiển thị). Không sửa lại bản cũ.
- Ảnh hưởng: tailieu/diagrams/*.drawio + *.png (6/6 thay mới); docs/work/diagram-style-guide.md (cập nhật mục UC).

## [2026-08-13 14:10] Chốt 28 UC hiển thị + rename (nguồn: PROMPT + SRS §5.1)
- Quyết định: Gộp UC-06+UC-07 → "UC-06 Làm bài tập" (UC-07 deprecated, ghi note). Loại khỏi mọi ảnh: UC-14 (demo), UC-22 (ghi chú cá nhân), UC-26 (Ladder). Rename hiển thị: UC-17→"Chạy code trong sandbox", UC-18→"Nộp code", UC-19→"Xem lịch sử bài làm", UC-25→"Học theo lộ trình", UC-29→"Nhiệm vụ hằng ngày", UC-31→"Xem bảng xếp hạng"; UC-24 giữ "Gửi phản hồi và báo lỗi"; còn lại giữ tên SRS.
- Ảnh hưởng: 4 file use case .drawio; đếm chuẩn = 28 UC (32 − 1 gộp − 3 loại).

## [2026-08-13 14:10] Chốt cấu trúc 4 ảnh use case
- Quyết định: Ảnh 01-tổng quan: 4 actor (Khách/Người học/Giảng viên/Admin) mỗi người 1 cột, CHỈ UC đại diện nhóm, TUYỆT ĐỐI không include/extend. Ảnh 02-Người học: 19 UC (01,04,05,06,08,16,17,18,19,21,23,24,25,27,28,29,30,31,32) — KHÔNG UC-02/03/15 (thuộc Khách), CÓ UC-01. Ảnh 03-GV: UC-04,05,09,10,11,20,24 (7 UC). Ảnh 04-Admin: UC-12,13,24 (3 UC).
- Include (hướng UC gốc→UC bị include, chỉ vẽ trong ảnh actor): ảnh 02: UC-01→UC-04, UC-06→UC-01, UC-27→UC-06, UC-27→UC-18, UC-18→UC-17, UC-29→UC-01. Extend: ảnh 02: UC-24→UC-16, UC-19→UC-18; ảnh 03: UC-11→UC-20.
- BỎ + ghi note: UC-03→UC-02 (không ảnh nào có cả 02+03); UC-15→UC-03, UC-30→UC-03, UC-32→UC-03 (ảnh 01 cấm extend, ảnh 02 không có UC-03).
- Ảnh hưởng: 4 file .drawio use case; docs/work/diagram-notes.md (ghi các quan hệ bỏ).

## [2026-08-13 14:10] Chốt UC đại diện Ảnh 01 (11 UC) — không tự bịa nhóm, kế thừa 7 nhóm đợt A2 đã lọc
- Quyết định: Khách: UC-03 (đại diện Xác thực & tài khoản). Người học: UC-01 (Mô phỏng), UC-06 (Bài tập & Code), UC-25 (Học tập), UC-29 (Gamification), UC-21 (Lớp học). Giảng viên: UC-09 (Học tập), UC-10 (Bài tập & Code), UC-20 (Lớp học). Admin: UC-12, UC-13 (Quản trị & tài khoản). UC-24 (Phản hồi) vẽ 1 elip chung nối thẳng 3 actor Người học+GV+Admin. KHÔNG vẽ: UC-02/15, UC-08, UC-04/05/16/17/18/19/23/27/28/30/31/32 ở ảnh 01 (có đủ ở ảnh chi tiết) — ghi note lý do.
- Ảnh hưởn: 01-usecase-tong-quan.drawio; diagram-notes.md.

## [2026-08-13 14:10] Style-guide cập nhật theo quyết định mới (chỉ mục use case; ERD giữ nguyên)
- Quyết định: include LẪN extend đều NÉT ĐỨT + mũi tên HỞ open (UML 2.5.1 §18.1.4 đã research): include stroke #0D9488, extend stroke #94A3B8; nhãn «include»/«extend» font 9-10pt màu #64748B, đặt giữa dây không đè đường. UC ellipse nền #F0FDFA viền #0D9488 chữ #0F172A (bỏ nền trắng cũ). Khung hệ thống viền #CBD5E1 nền trắng/#F8FAFC. Nhóm module nền #F1F5F9 viền đứt #94A3B8. Actor #334155. CẤM neon/đỏ tươi.
- Ảnh hưởng: docs/work/diagram-style-guide.md (mục d/e + bảng màu), 4 file use case.

## [2026-08-13 14:10] Công cụ + chế độ chạy
- Quyết định: draw.io CLI = C:\Users\Administrator\AppData\Local\Programs\draw.io\draw.io.exe (31.1.8, winget) — export `-x -f png -o <out> <in>`. LOCAL-ONLY: KHÔNG commit/merge vào dev (đang đứng dev); để local + ghi log (đúng prompt); commit (nếu có) vào feature/diagrams. Kế thừa bài học A2: dựng .drawio bằng SCRIPT GENERATOR (node), không viết XML tay.
- Ảnh hưởn: toàn bộ phiên; docs/pm-report-diagrams.md + docs/pm-decision-log-diagrams.md.

## [2026-08-13 14:30] Task T2 (use case) + T3 (ERD) fail 2 lần kết quả rỗng → chuyển agent
- Quyết định: dev-docs trả về RỖNG 2 lần (dispatch + resume) cho cả T2 (4 use case) và T3 (2 ERD), không tạo file nào. Áp dụng bài học + quyết định đã duyệt phiên trước (decision log cũ): chuyển sang làm thủ công bằng agent dev (generic) với script generator node/PowerShell. Đây là lần thử CUỐI cho mỗi task — nếu vẫn rỗng/fail → task FAIL + DỪNG toàn bộ theo giới hạn an toàn --auto.
- Ảnh hưởng: toàn bộ 6 file .drawio; tailieu/diagrams/, docs/work/.

## [2026-08-13 14:50] DỪNG TOÀN BỘ --auto (2 task liên tiếp FAIL — lỗi hạ tầng subagent)
- Quyết định: T2 (4 use case) + T3 (2 ERD) thử 4 lần (dev-docs x2, dev x1, general x1) — MỌI task thật trả kết quả RỖNG, không tạo file nào; riêng task test siêu nhỏ (PONG-OK, không gọi tool) chạy được → kết luận lỗi hạ tầng subagent khi thực thi (không phải lỗi nội dung prompt). Theo giới hạn an toàn --auto: 2 task liên tiếp FAIL → DỪNG toàn bộ, KHÔNG tự làm thay, KHÔNG tự đổi thiết kế.
- Ảnh hưởng: 6 file .drawio + 6 PNG CHƯA tạo; tailieu/diagrams/ hiện RỖNG (bản cũ lỗi đã bị xóa trước đó — bản cũ còn trong git history feature/diagrams nếu cần khôi phục). Toàn bộ phiên dừng; ghi docs/pm-report-diagrams.md.

## [2026-08-13 15:00] Phiên diagrams --auto LẦN 2 (PROMPT_DIAGRAMS_DRAWIO.md — user chốt điều kiện 13/08)
- Quyết định: (1) KHÔNG research lại BƯỚC 1-2 — dùng thẳng docs/work/diagram-style-guide.md (chuẩn duy nhất) + diagram-notes.md + diagram-samples/ đã có. (2) Cập nhật style-guide lên v0.2 theo quyết định chốt 14:10 (phiên trước chưa kịp áp): include LẪN extend đều NÉT ĐỨT + mũi tên hở (include #0D9488, extend #94A3B8, nhãn #64748B); ellipse nền #F0FDFA; boundary viền #CBD5E1 nền #F8FAFC; thêm STYLE-UC-GROUP (nền #F1F5F9 viền đứt #94A3B8). (3) Tạo docs/work/diagram-task-spec.md (dữ liệu dựng 6 ảnh) — prompt task CHỈ trỏ đường dẫn, KHÔNG nhúng 37KB style-guide/notes (bài học phiên trước). (4) Làm việc tại worktree D:\FPT\neww-diagrams (branch feature/diagrams — commit được); KHÔNG merge dev (session khác đang chạy). (5) draw.io CLI = C:\Users\Administrator\AppData\Local\Programs\draw.io\draw.io.exe (31.1.8, đã verify tồn tại); Ollama qwen2.5vl:3b đã có (ollama list OK).
- Ảnh hưởng: docs/work/diagram-style-guide.md (v0.2), docs/work/diagram-task-spec.md (mới), 6 file .drawio + 6 PNG tailieu/diagrams/ (bản cũ đã bị user xóa — vẽ mới từ đầu).

## [2026-08-13 15:00] Xử lý mâu thuẫn đếm ảnh 01 (11 UC đại diện vs 28 UC hiển thị)
- Quyết định: Quyết định 14:10 chốt ảnh 01 vẽ 11 UC đại diện nhưng danh sách thiếu UC-02/UC-15 (Khách) → tổng UC duy nhất = 26 < 28 (verify yêu cầu 28). SỬA: ảnh 01 vẽ ĐỦ cột Khách (UC-02, UC-03, UC-15) → 14 elip UC ảnh 01; tổng 4 ảnh = 28 UC duy nhất. Không include/extend ở ảnh 01 (cấm theo PROMPT). Ghi chú UC-02/03/15 thuộc Khách đã có trong task-spec.
- Ảnh hưởng: 01-usecase-tong-quan.drawio, docs/work/diagram-task-spec.md (mục B/E).

## [2026-08-13 15:15] Task 01 (01-usecase-tong-quan) trả rỗng 2 lần (dispatch + resume) — retry cuối session mới
- Quyết định: Task 01 trả kết quả RỖNG + không tạo file ở cả lần dispatch đầu và resume (đúng quy trình 5 bước: bỏ nhúng ✓ → test nhỏ PONG-OK ✓ → tách 1 ảnh/task ✓ → resume vẫn rỗng). Task 02 (cùng dev-docs, cùng cấu trúc prompt, cùng lúc) THÀNH CÔNG → loại trừ lỗi hạ tầng toàn cục; nghi session 01 bị lỗi context. Theo giới hạn --auto: 1 task tối đa 2 lần sửa → thử LẦN CUỐI bằng dispatch session MỚI (fresh, không resume); nếu vẫn rỗng → task 01 = FAIL, ghi báo cáo, KHÔNG tự làm thay (luật --auto).
- Ảnh hưởng: 01-usecase-tong-quan.drawio; docs/pm-report-diagrams.md.

## [2026-08-13 15:20] Task 01 fail 3 lần (dev-docs dispatch + resume + dispatch mới) — chuyển agent dev retry cuối
- Quyết định: Task 01 (01-usecase-tong-quan) trả RỖNG + không tạo file ở 3 lần (2 session dev-docs khác nhau + 1 resume). Task 02 (cùng dev-docs, cùng lúc) THÀNH CÔNG → hạ tầng OK, lỗi cục bộ task/session 01. Ghi task 01 = FAIL theo quy trình 5 bước (đã: bỏ nhúng ✓, test nhỏ PONG-OK ✓, tách 1 ảnh/task ✓, resume ✓). Retry LẦN CUỐI: chuyển agent `dev` (generic) với prompt rút gọn — đây là quyền điều phối PM (không đổi thiết kế); phương án đã chuẩn bị trong decision log phiên trước. Nếu dev vẫn rỗng → task 01 = FAIL chính thức, ghi báo cáo, KHÔNG tự vẽ tay, KHÔNG đổi thiết kế.
- Ảnh hưởng: 01-usecase-tong-quan.drawio; docs/pm-report-diagrams.md.

## [2026-08-13 15:30] Task 01 = FAIL chính thức (4 lần rỗng — vượt giới hạn --auto); task sửa 02 thử lại lần 2
- Quyết định: Task 01 (01-usecase-tong-quan) trả RỖNG 4 lần: dev-docs (dispatch) → dev-docs (resume) → dev-docs (dispatch mới) → dev (generic). Đã thực hiện đủ quy trình 5 bước user quy định (bỏ nhúng ✓, task test nhỏ PONG-OK ✓ — hạ tầng hoạt động, tách 1 ảnh/task ✓ — 02/03/04/05/06 cùng cấu trúc THÀNH CÔNG, resume ✓, ghi FAIL). Theo giới hạn an toàn --auto (quá 2 lần = FAIL, không tự làm thay, không tự đổi thiết kế): task 01 = FAIL, chờ user quyết "làm lại 01" cuối phiên. KHÔNG 2 task liên tiếp FAIL (02/03/04/05/06 OK) → không dừng toàn bộ.
- Task sửa 02 (thêm 19 edge actor→UC): lần 1 rỗng → thử lần 2 (resume). Nếu rỗng → ghi tồn đọng vào báo cáo (file 02 vẫn dùng được, thiếu association).
- Ảnh hưởng: 01-usecase-tong-quan.drawio (chưa tồn tại), 02-usecase-hoc-vien.drawio (thiếu 19 edge actor→UC), docs/pm-report-diagrams.md.

## [2026-08-13 15:40] Task sửa 02 (thêm 19 edge actor→UC) = FAIL (2 lần rỗng) — ghi tồn đọng
- Quyết định: Task sửa 02 trả RỖNG 2 lần (dispatch + resume), file không đổi (8 edge, thiếu 19 association actor→UC). Theo giới hạn --auto (2 lần = FAIL): ghi task = FAIL, KHÔNG tự sửa thay. Ảnh 02 vẫn dùng được (6 include + 2 extend đúng, 19 UC đúng) — chỉ thiếu association actor→UC so với chuẩn UML. Ghi tồn đọng vào báo cáo, chờ user quyết.
- Ảnh hưởng: 02-usecase-hoc-vien.drawio (thiếu 19 edge), docs/pm-report-diagrams.md.
