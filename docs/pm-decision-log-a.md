# PM DECISION LOG — SESSION A (Báo cáo Word)

> File ghi mọi quyết định ảnh hưởng kết quả của SESSION A (task A6 trong PM_MASTER_PLAN v2).
> Chạy ở chế độ --auto ngày 12/08/2026. Các quyết định được ghi TRƯỚC khi thực hiện.

## [2026-08-12 01:40] Khởi động SESSION A (chế độ --auto)
- Quyết định: Chạy toàn bộ SESSION A không chờ duyệt (cờ --auto trong lệnh người dùng). Kế hoạch coi như đã được duyệt trước; người dùng kiểm tra qua docs/pm-report-a.md.
- Ảnh hưởng: toàn bộ các task của session (viết BAO_CAO.md, placeholders, diagram-prompts, build docx).

## [2026-08-12 01:40] Cấu trúc viết báo cáo — tách part file (tiết kiệm context)
- Quyết định: Theo chỉ đạo của PM_MASTER_PLAN ("TIẾT KIỆM CONTEXT: đọc file docs THEO TỪNG PHẦN"), BAO_CAO.md (~50-70 trang) được viết thành 5 part file trong tailieu/parts/ (mỗi agent con chỉ đọc nguồn của chương mình), sau đó ghép thành tailieu/BAO_CAO.md ở task cuối. Đánh số Hình/Bảng theo phần (Hình 3.1, Bảng 2.1...) để các part độc lập không cần phối hợp.
- Ảnh hưởng: tailieu/parts/00-bai-hoc-va-hang-so.md (hợp đồng chung), 01..05 (nội dung), task assemble cuối.

## [2026-08-12 01:40] Template tham chiếu docx
- Quyết định: tailieu/DSA_Visual_Template.docx KHÔNG tồn tại → lệnh build KHÔNG dùng --reference-doc (theo BAO_CAO_SPEC §2: "không có thì bỏ --reference-doc").
- Ảnh hưởng: lệnh build = pandoc tailieu/BAO_CAO.md -o tailieu/BaoCaoDoAn.docx --toc.

## [2026-08-12 01:40] Nguồn thiếu: docs/THIRD_PARTY.md và tailieu/screenshots/
- Quyết định: (1) THIRD_PARTY.md chưa tồn tại → Phụ lục C ghi "chưa cập nhật (12/08/2026)" + bảng thư viện trích từ SDD/DEPLOY (tên lib + nguồn dùng), KHÔNG bịa license. (2) screenshots chưa có → dùng 18 ảnh placeholder PNG 1920×1080 có nhãn theo §6.1; ảnh thật sau này ghi đè đúng tên file trong tailieu/placeholders/ rồi chạy lại pandoc.
- Ảnh hưởng: tailieu/placeholders/ (18 file), Phụ lục C trong part 05.

## [2026-08-12 01:40] Cụm từ "tuần 19-20" vs CẤM "20 tuần"
- Quyết định: CẤM "20 tuần/16 tuần" ở mọi nơi (dự án = 13 tuần). Riêng cụm đánh dấu kết quả chưa kiểm thử được phép dùng ĐÚNG cụm chuẩn của spec: "chờ hoàn tất kiểm thử (tuần 19-20)" (BAO_CAO_SPEC §5.0A.6 và quy tắc 4 — cụm này nằm trong spec, không phải tuyên bố độ dài dự án).
- Ảnh hưởng: toàn bộ nội dung báo cáo, đặc biệt Phần 6 (kiểm thử).

## [2026-08-12 01:40] Ngày trên bìa
- Quyết định: Bìa ghi "TP.HCM, ngày 12 tháng 8 năm 2026" (ngày tạo báo cáo, hôm nay). Ngày bảo vệ để trống (chưa có lịch — người dùng điền sau theo BAO_CAO_SPEC §4.1).
- Ảnh hưởng: part 01 (bìa).

## [2026-08-12 01:40] Sơ đồ Mermaid nhóm A
- Quyết định: Giữ Mermaid dạng code block cho sơ đồ NHÓM A quan trọng (spec quy tắc 3) kèm 1-2 câu giải thích; ảnh NHÓM B (use case/ERD) trỏ placeholder trong tailieu/placeholders/. Không chặn tiến độ nếu mmdc lỗi (DIAGRAM_PROMPTS).
- Ảnh hưởng: Phần 3 (use case), Phần 4 (ERD), tailieu/diagram-prompts.md.

## [2026-08-12 01:40] Phát hiện kỹ thuật: simulation-catalog.json có UTF-8 BOM
- Quyết định: Đọc catalog bằng encoding utf-8-sig khi trích Phụ lục D (44 mô phỏng).
- Ảnh hưởng: part 05 (Phụ lục D).

## [2026-08-12 02:10] Lệch số mục nguồn thực tế (agent phát hiện khi đọc)
- Quyết định: Các agent con ghi nguồn theo VỊ TRÍ THẬT trong file thay vì số mục trong BAO_CAO_SPEC (chỉ là bản đồ tham chiếu cũ): UC ở SRS §5 (không phải §6), master matrix FR ở SRS §3.1, design system ở SDD §8, backlog hướng phát triển ở SDD §11.2 (không có §16), sequence UC-25 không tồn tại trong SRS → dùng sequence UC-06 có sẵn + mô tả trừ tim atomic bằng lời, DEPLOY có 10 sự cố → tóm tắt 8 sự cố theo yêu cầu spec.
- Ảnh hưởng: ghi chú nguồn `(nguồn: ...)` trong toàn bộ BAO_CAO.md.

## [2026-08-12 02:10] Làm sạch wireframe ASCII Màn 05
- Quyết định: Wireframe Màn 05 (simulator) có emoji (⏮▶★) trong SDD gốc — loại bỏ, thay bằng chữ thường (Phát, Dừng, Bước...) để tuân thủ quy tắc cấm ký tự lạ.
- Ảnh hưởng: mục 4.2.3 của BAO_CAO.md.

## [2026-08-12 02:10] Độ dài BAO_CAO.md
- Quyết định: BAO_CAO.md 2.301 dòng (~149KB) — hơi vượt ước lượng 2.200 dòng nhưng spec giới hạn theo TRANG khi render (mỗi mục 1-4 trang, tổng 50-70 trang), không giới hạn dòng → giữ nguyên, ghi nhận.
- Ảnh hưởng: tailieu/BAO_CAO.md, tailieu/BaoCaoDoAn.docx.

## [2026-08-12 02:10] Verify cuối độc lập (PM review)
- Quyết định: Chạy lại kiểm tra độc lập: 14 heading H1 đúng cấu trúc (LỜI MỞ ĐẦU, PHẦN 1-7, KẾT LUẬN, TLTK, PHỤ LỤC A-D), 0 lần "20 tuần"/"16 tuần"/"[KHUNG", 18/18 ảnh placeholder tồn tại, docx zip OK + 18 media + roundtrip OK → coi SESSION A hoàn tất.
- Ảnh hưởng: docs/pm-report-a.md.
