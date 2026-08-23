# PM REPORT — SESSION A2 (6 ảnh NHÓM B thật — SVG tay) — 12/08/2026

> Session con của SESSION A: thay 6 ảnh placeholder NHÓM B bằng ảnh THẬT tự vẽ SVG (không dùng MCP/draw.io/ChatGPT), render PNG 1920×1080, build lại docx, commit + merge. Chế độ --auto. Quyết định: docs/pm-decision-log-a2.md.

## Mục tiêu
Sinh 6 ảnh NHÓM B thật (01-usecase-tong-quan, 02-usecase-hoc-vien, 03-usecase-giang-vien, 04-usecase-admin, 05-erd-tong-quan, 06-erd-chi-tiet) thay thế placeholder → đặt tailieu/diagrams/ → cập nhật BAO_CAO.md → build lại docx → review → commit/merge.

## Trạng thái từng task

| Task | Nội dung | Trạng thái |
|---|---|---|
| T1a | 4 SVG use case (32/24/7/3 UC, khớp SRS §5.1 100%) | DONE |
| T1b | 2 SVG ERD (05: 6 cụm/32 bảng; 06: 32 bảng/PK/FK/13 mũi tên) | DONE (retry — agent dev-docs/dev fail 3 lần, resume thành công) |
| T2 | Script render-diagrams.mjs + render 6 PNG 1920×1080 | DONE (PASS: 60–285KB/file) |
| T3 | Sửa 6 refs BAO_CAO.md (placeholders/ → diagrams/) + build docx | DỞ DANG — build BaoCaoDoAn.docx FAIL vì Word đang mở file (đã ghi SETUP_TODO mục 9) |
| T3b | Build verify BaoCaoDoAn_new.docx | DONE (960KB, media 18, 6 ảnh > 40KB — đã chứng minh pipeline OK) |
| T4 | dev-review 6 ảnh khớp SRS §5.1/SDD | APPROVE (2 Minor đã fix: 06 mũi tên trùng → 13 mũi riêng biệt; 05 thêm nhãn 1-n) |
| T5 | Commit (phuc: ảnh+tài liệu; thu: script) + merge feature/diagrams → dev | DONE (xem bên dưới) |

## File thay đổi / tạo mới
- `tailieu/diagrams/` — 6 SVG nguồn (tự vẽ tay, viewBox 1920×1080, teal #0D9488) + 6 PNG render (1920×1080)
- `tailieu/render-diagrams.mjs` — script render SVG→PNG bằng playwright CLI (bỏ --full-page vì treo trên Windows; SVG viewBox đã đúng kích thước)
- `tailieu/BAO_CAO.md` — 6 dòng refs đổi sang diagrams/ (dòng 334, 398, 453, 482, 916, 945); 12 ảnh màn hình giữ placeholder/
- `tailieu/BaoCaoDoAn_new.docx` — bản verify (960,5KB); BẢN CHÍNH `BaoCaoDoAn.docx` CHƯA build được (Word đang mở)
- `docs/pm-decision-log-a2.md`, `docs/pm-report-a2.md`
- `docs/SETUP_TODO.md` — mục 9: user đóng Word → chạy lại pandoc

## Kết quả verify
- SVG: 6/6 parse OK, viewBox 1920×1080, 0 mojibake, 0 emoji lạ, 0 watermark.
- UC: 01=32/32, 02=24, 03=7, 04=3 — khớp diagram-prompts.md + SRS §5.1 (review 34/34 check PASS).
- ERD: 05=6 cụm + 32 bảng + nhãn quan hệ; 06=32 bảng + PK/FK + 13 mũi tên riêng biệt.
- PNG: 6 file 1920×1080 (PNG header), dung lượng 54–285KB.
- BAO_CAO.md: 18 refs đều trỏ file tồn tại (6 diagrams + 12 placeholders).
- Docx verify (BaoCaoDoAn_new.docx): zip OK, media 18, 6 media > 40KB (285K/205K/115K/84K/60K/55K) — tăng từ 305KB → 960KB so với bản placeholder.

## Quyết định chính (xem docs/pm-decision-log-a2.md)
- UC giảng viên = 7 (09,10,11,20 + 04,05,24), admin = 3 (12,13,24) theo phân bổ prompt 1 (prompt 3/4 chỉ liệt kê 4/2 — dùng đầy đủ).
- 06 ERD chi tiết: chỉ vẽ 13 mũi tên FK chính, FK còn lại hiện qua dòng FK trong khung (tránh rối, chữ ≥ 10pt).
- Đổi refs sang diagrams/ (ít sửa nhất — 6 dòng); giữ placeholder dir sạch cho ảnh màn hình sau.
- Word đang mở BaoCaoDoAn.docx → KHÔNG giết Word, KHÔNG đổi tên output; build tạm _new để verify, user đóng Word rồi chạy lại lệnh pandoc (SETUP_TODO mục 9).

## Việc còn tồn đọng
1. **BẮT BUỘC (1 phút, user)**: đóng Word đang mở `tailieu/BaoCaoDoAn.docx` → chạy:
   `& "C:\Users\Administrator\AppData\Local\Pandoc\pandoc.exe" "D:\FPT\neww\tailieu\BAO_CAO.md" -o "D:\FPT\neww\tailieu\BaoCaoDoAn.docx" --toc --resource-path="D:\FPT\neww\tailieu"` (hoặc xóa BaoCaoDoAn_new.docx sau khi OK). Commit docx cuối nếu muốn.
2. Push feature/diagrams lên GitHub (session này không push theo lệnh user).
3. 12 ảnh màn hình UI vẫn là placeholder (chụp Playwright sau khi app chạy).
4. Tinh chỉnh mỹ thuật tùy chọn: đồng bộ hướng arrowhead trong 06 (gợi ý dev).

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu "làm lại <task/mục>" kèm ghi chú, PM chạy lại phần đó.
