# PM REPORT — SESSION A (Báo cáo Word) — 12/08/2026

> Trạng thái cuối của SESSION A (task A6, PM_MASTER_PLAN v2). Chạy chế độ --auto.
> Mọi quyết định chi tiết: docs/pm-decision-log-a.md.

## Mục tiêu
Tạo báo cáo đồ án Word DSA-Visual theo docs/BAO_CAO_SPEC.md + docs/DIAGRAM_PROMPTS.md, chuẩn độ sâu S-Clinic (đã đọc tailieu/NET202_Project document_6 (1).pdf, 48 trang), khuôn trường PHẦN 1-7 đầy đủ, mốc 12/05→11/08/2026 (13 tuần), 4 thành viên thật, CẤM "20 tuần".

## Trạng thái từng task

| Task | Nội dung | Trạng thái |
|---|---|---|
| T1 | Đọc chuẩn S-Clinic (PDF) + khuôn mẫu báo cáo cũ + hợp đồng hằng số (parts/00) | DONE (do PM) |
| T2 | 18 ảnh placeholder 1920×1080 có nhãn → tailieu/placeholders/ | DONE |
| T3 | tailieu/diagram-prompts.md — 6 prompt NHÓM B + 8 sơ đồ NHÓM A | DONE |
| T4 | Part 01: Bìa + Lời mở đầu + PHẦN 1 + PHẦN 2 | DONE |
| T5 | Part 02: PHẦN 3 (Analysis) — matrix 75 FR, 3 UC đặc tả 4 mục | DONE |
| T6 | Part 03: PHẦN 4 (Design) — 32 bảng data dictionary, 14 ảnh | DONE |
| T7 | Part 04: PHẦN 5 (Implement) + PHẦN 6 (Testing) | DONE |
| T8 | Part 05: PHẦN 7 + Kết luận + TLTK + Phụ lục A-D | DONE |
| T9 | Ghép BAO_CAO.md + pandoc build docx + verify | DONE |

## File thay đổi / tạo mới

- `tailieu/BAO_CAO.md` — 2.301 dòng, ~149KB (báo cáo nguồn, sẽ ghép ảnh thật sau)
- `tailieu/BaoCaoDoAn.docx` — 305KB, build bằng pandoc `--toc` (không --reference-doc vì template không tồn tại)
- `tailieu/placeholders/` — 18 PNG 1920×1080 (12 màn + 6 sơ đồ NHÓM B), đúng tên file chuẩn
- `tailieu/diagram-prompts.md` — 6 prompt hoàn chỉnh (dán vào ChatGPT), đối chiếu UC 32/32, bảng 32/32 với SRS/SDD
- `tailieu/parts/00..05` — hợp đồng hằng số + 5 part trung gian (giữ lại để review)
- `docs/pm-decision-log-a.md` — 10 quyết định đã ghi

## Kết quả verify (đã chạy lại độc lập)

- Cấu trúc: đủ 14 heading cấp 1 đúng khuôn (LỜI MỞ ĐẦU, PHẦN 1-7, KẾT LUẬN & HƯỚNG PHÁT TRIỂN, TÀI LIỆU THAM KHẢO, PHỤ LỤC A-D) — các dòng `#` trong code fence không lọt mục lục.
- Cụm cấm: "20 tuần" = 0, "16 tuần" = 0, "[KHUNG" = 0 (kiểm tra regex).
- Ảnh: 18/18 ref `placeholders/...` trỏ file tồn tại, đúng 1920×1080.
- Docx: zip hợp lệ, word/document.xml parse OK, 18 media nhúng, pandoc roundtrip docx→plain OK (file mở đọc được).
- Bảng 4 thành viên đúng mã SV TD01287/TD01282/TD01131/TD01261; bảng 10 sprint đúng mốc 12/05→18/05 ... 14/07→20/07; kết thúc phát triển ~11/08 (13 tuần).
- Phần 6 kiểm thử: mọi kết quả chưa chạy đều ghi đúng cụm chuẩn "chờ hoàn tất kiểm thử (tuần 19-20)" — không bịa số.

## Quyết định chính (xem docs/pm-decision-log-a.md)

- Chia báo cáo 5 part file (tiết kiệm context theo PM_MASTER_PLAN), ghép ở task cuối; đánh số Hình/Bảng theo phần.
- Build không --reference-doc (DSA_Visual_Template.docx không tồn tại).
- Nguồn ghi theo vị trí THẬT trong docs (lệch số mục so với spec cũ: SRS §5/§3.1, SDD §8/§11.2...).
- THIRD_PARTY.md chưa tồn tại → Phụ lục C ghi "chưa cập nhật (12/08/2026)" + bảng thư viện trích từ SDD/DEPLOY (không bịa license).

## Việc còn tồn đọng (cần người dùng hoặc session sau)

1. **Ảnh thật**: chụp 12 màn bằng Playwright (app chạy được) + dán 6 prompt NHÓM B vào ChatGPT → ghi đè đúng tên file trong `tailieu/placeholders/` → chạy lại `pandoc tailieu/BAO_CAO.md -o tailieu/BaoCaoDoAn.docx --toc --resource-path=tailieu`.
2. **Sơ đồ NHÓM A thành ảnh** (tùy chọn): mmdc render hoặc giữ code block như hiện tại.
3. **Kết quả kiểm thử thật** (tuần 19-20): thay cụm "chờ hoàn tất kiểm thử (tuần 19-20)" bằng số thật ở Phần 6.
4. **THIRD_PARTY.md**: tạo để bổ sung license Phụ lục C.
5. **Ngày bảo vệ**: điền sau khi có lịch (đang để trống trên bìa).
6. Kiểm tra thủ công 1 lần bằng Word: mục lục, ảnh, định dạng (checklist spec §7).

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu "làm lại <task/mục>" kèm ghi chú, PM chạy lại phần đó.
