# PM DECISION LOG — SESSION A2 (6 ảnh NHÓM B thật — SVG tay) — 12/08/2026

> Session con của SESSION A: thay 6 ảnh placeholder NHÓM B bằng ảnh THẬT tự vẽ SVG (không dùng MCP/draw.io/ChatGPT), render PNG 1920x1080, build lại docx, commit + merge.
> Chế độ --auto. Quyết định ghi TRƯỚC khi thực hiện.

## [2026-08-12 03:05] Khởi động SESSION A2
- Quyết định: Chạy theo lệnh user (--auto): feature/diagrams từ dev → dev-docs vẽ SVG → dev-test render PNG → dev-docs build docx → dev-review rà khớp → merge dev; commit qua commit-as.ps1 (phuc: ảnh+tài liệu; thu: script render). Không push (user không yêu cầu — ghi chú trong report).
- Ảnh hưởng: toàn bộ session.

## [2026-08-12 03:05] Xử lý lệch số lượng UC giữa lệnh user và diagram-prompts.md
- Quyết định: Dữ liệu chuẩn = tailieu/diagram-prompts.md (đã đối chiếu SRS 32/32 ở Session A). Cụ thể:
  - 01 tổng quan: 32 UC + 3 tác nhân + ghi chú "khách 4 UC (UC-02,03,14,15)" theo prompt 1.
  - 02 học viên: 24 UC (danh sách prompt 2 — user nhắc "22" nhưng prompt liệt kê 24; theo prompt).
  - 03 giảng viên: 7 UC (UC-09,10,11,20 + UC-04,05,24 theo phân bổ prompt 1 — prompt 3 chỉ liệt kê 4, dùng phân bổ đầy đủ để khớp user "7 giảng viên").
  - 04 admin: 3 UC (UC-12,13,24 theo prompt 1 — prompt 4 chỉ liệt kê 2).
- Ảnh hưởng: 4 file SVG use case.

## [2026-08-12 03:05] ERD 2 ảnh
- Quyết định: 05-erd-tong-quan = 6 cụm (Auth, Học tập, Engine, Lớp học, Gamification, Code Runner) mỗi cụm chứa DANH SÁCH ĐẦY ĐỦ bảng của cụm (gom nhóm 32 bảng theo prompt 5) + đường nối quan hệ giữa cụm có nhãn (1-n, n-n). 06-erd-chi-tiet = 32 bảng dạng khung nhỏ (tiêu đề + PK + FK + 3-6 cột chính) trong lưới; vẽ mũi tên FK chỉ cho ~12 quan hệ chính (Users↔ClassMembers, Classes→ClassMembers, Lessons→Exercises, Exercises→Questions/ExerciseSubmissions/CodeRuns/CodeSubmissions, LearningPaths→LearningPathNodes, LearningPathNodes→NodeSessions/UserNodeProgress, DailyQuests→UserQuests, ShopItems→UserInventory...) — còn lại thể hiện qua dòng FK trong bảng (tránh rối, chữ giữ ≥10pt).
- Ảnh hưởng: 2 file SVG ERD.

## [2026-08-12 03:05] Đường dẫn ảnh trong BAO_CAO.md — cách ít sửa nhất
- Quyết định: BAO_CAO.md đang trỏ 6 ảnh qua `placeholders/0X-*.png` (dòng 334, 398, 453, 482, 916, 945) → đổi ĐÚNG 6 tham chiếu này sang `diagrams/0X-*.png` (PNG thật nằm ở tailieu/diagrams/ theo lệnh user). 12 ảnh màn hình giữ nguyên placeholder. Không copy ảnh chồng lên placeholders (giữ placeholder dir sạch cho ảnh chụp màn sau).
- Ảnh hưởng: tailieu/BAO_CAO.md (6 dòng), tailieu/BaoCaoDoAn.docx (build lại).

## [2026-08-12 03:05] Render SVG→PNG
- Quyết định: Dùng playwright CLI (đã cài chromium 1223/1228/1234 trong %LOCALAPPDATA%\ms-playwright; playwright 1.62.1 resolve qua npx) với script tailieu/render-diagrams.mjs (`npx playwright screenshot` hoặc API node). Script render được commit dưới tên thu. Nếu CLI gặp lỗi môi trường → fallback chrome-devtools MCP.
- Ảnh hưởng: tailieu/render-diagrams.mjs, 6 PNG.

## [2026-08-12 03:05] Rủi ro: file docx có thể đang mở trong Word
- Quyết định: Phát hiện `tailieu/~$oCaoDoAn.docx` (lock file của Word) → file đang được mở. Khi build lại docx nếu pandoc báo lỗi khóa file: ghi vào docs/SETUP_TODO.md + báo user đóng Word rồi chạy lại lệnh pandoc (không tự đổi tên output).
- Ảnh hưởng: tailieu/BaoCaoDoAn.docx, docs/SETUP_TODO.md.
