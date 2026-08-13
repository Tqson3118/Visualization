---
description: Chuẩn hóa cấu trúc prompt dispatch mà PM viết cho subagent — ép khuôn: mục tiêu, file đọc, tiêu chuẩn hoàn thành, lệnh verify, commit-as, log. Dùng khi PM viết bất kỳ prompt task nào (--auto hoặc có duyệt) để mọi session nhất quán, giảm lỗi hiểu sai, chống context phình.
---

# PM Prompt Standard — khuôn prompt dispatch cho subagent

Dùng để viết MỌI prompt task giao cho dev/ dev-backend/ dev-frontend/ dev-engine/ dev-ux/ dev-test/ dev-e2e/ dev-review/ dev-docs. Mục đích: task nhỏ, tự chứa đủ ngữ cảnh, verify có lệnh thật, kết quả đo đếm được.

## Khuôn 7 phần (bắt buộc, đúng thứ tự)

1. **BỐI CẢNH (2-3 dòng)**: session nào, đọc file nào trước (handoff/report/decision log có liên quan), nguồn chuẩn (PRODUCTION_PROMPT → SDD/API_REFERENCE/SCREEN_MAP). KHÔNG bắt agent đọc lại cả repo.
2. **MỤC TIÊU (1 câu)**: 1 task = 1 mục tiêu duy nhất. Nếu có 2 mục tiêu → tách 2 task.
3. **PHẠM VI (danh sách file/dir chính xác)**: ghi rõ file nào được sửa, file nào CẤM đụng (vd engines/** là của dev-engine, docs/ là của dev-docs). Ghi cả ràng buộc đặc thù repo (CẤM Repository/MediatR/PostgreSQL/secret thật; catalog khớp shared/simulation-catalog.json).
4. **TIÊU CHUẨN HOÀN THÀNH (đo đếm được)**: "xong khi X PASS + Y không lỗi + Z file" — không viết "làm cho tốt".
5. **LỆNH VERIFY THẬT (bắt buộc)**: liệt kê lệnh cụ thể + kết quả kỳ vọng. Với task UI: bắt buộc vòng Ollama theo **KHUNG 7 TIÊU CHÍ** (UI: thẩm mỹ, nhất quán, rõ ràng, phản hồi trực quan · UX: luồng thao tác, khả năng tiếp cận, độ thỏa mãn) — mỗi tiêu chí chấm 1-5 + nhận xét cụ thể; tiêu chí ≤ 3 điểm PHẢI sửa (≤ 2 vòng) hoặc từ chối có lý do. Repo không có lệnh → ghi "repo không có lệnh X" thay vì đoán.
6. **GIT (bắt buộc)**: nhánh feature từ `dev` (KHÔNG main); PR base `dev` (đã đóng 8 PR ma vì lỗi này); commit qua `.\commit-as.ps1 <tên> "<message Conventional Commits>"` — phân vai: backend→bao, frontend/UX→son, engine/test→thu, docs→phuc. Trạng thái ghi `docs/work/<task>.md`.
7. **BÁO CÁO (≤ 10 dòng)**: file đã sửa, lệnh verify + kết quả, vấn đề/điểm lệch docs (phải có lý do trong decision log), đề xuất bước sau (không làm).

## Quy tắc chống lỗi đã đúc kết (nhớ áp dụng)

- **Task NHỎ**: 1 module/nhóm file ≤ ~5-10 file. "12 service + 14 controller" = phải tách thành nhiều task. KHÔNG resume task dài — mỗi lần gọi task là fresh context.
- **Chống context phình**: trước khi giao task phức tạp, dùng `explore` khảo sát vùng code, trích ngữ cảnh ngắn vào prompt — agent không phải đọc lại toàn bộ.
- **Không ai tự chấm bài mình**: dev viết → dev-test verify độc lập → dev-e2e (nếu UI) → dev-review chốt APPROVE/CHANGES REQUESTED.
- **--auto**: ghi quyết định vào `docs/pm-decision-log-<tên>.md` TRƯỚC khi thực hiện; mọi lệch docs chủ ý đều phải có mục đồng bộ docs (dev-docs) đi kèm.
- **Số liệu thật**: THIRD_PARTY/checklist §17.9 phải dùng lệnh thật (npm ls / dotnet list package / build đo bundle), không "chờ tuần X" nếu đã có số.

## Ví dụ tối thiểu (task đúng chuẩn)

```
[dev-frontend, feature/ux-x] Mục tiêu: <1 câu>.
Đọc: <file liên quan + 1-2 nguồn chuẩn>.
Phạm vi: <file được sửa> — CẤM: engines/**, docs/, đổi token màu.
Xong khi: build PASS + test không vỡ + smoke 2 màn.
Verify: npm run build; npm test; playwright mở /x console 0 lỗi; chụp → Ollama nhận xét → sửa ≤ 2 vòng.
Git: feature/ux-x từ dev; commit-as son "feat: ..."; trạng thái docs/work/ux-x.md.
Báo cáo: file, verify, vấn đề, đề xuất.
```
