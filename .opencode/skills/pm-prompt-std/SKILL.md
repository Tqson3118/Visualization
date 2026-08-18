---
description: Chuẩn hóa cấu trúc prompt dispatch mà PM viết cho subagent — ép khuôn: mục tiêu, file đọc, tiêu chuẩn hoàn thành, lệnh verify, commit-as, log. Dùng khi PM viết bất kỳ prompt task nào (--auto hoặc có duyệt) để mọi session nhất quán, giảm lỗi hiểu sai, chống context phình.
---

# PM Prompt Standard — khuôn prompt dispatch cho subagent

Dùng để viết MỌI prompt task giao cho dev/ dev-backend/ dev-frontend/ dev-engine/ dev-ux/ dev-test/ dev-e2e/ dev-review/ dev-docs. Mục đích: task nhỏ, tự chứa đủ ngữ cảnh, verify có lệnh thật, kết quả đo đếm được. (Tham khảo prompt chuẩn mực: `session/PROMPT_VISUALIZE_UPGRADE_V2.md` + `session/PROMPT_UI_PREMIUM_ROUND2.md` — viết từ audit code thật.)

## BƯỚC 0 — Audit code thật TRƯỚC khi viết prompt (bắt buộc)

Prompt viết từ trí nhớ/giả định = subagent sửa nhầm. Model mạnh đều đọc ~20-30 file thật rồi mới viết:
1. Đọc file chính + lân cận task: engine/store/view/renderer/composable + test tương ứng. KHÔNG chỉ đọc tên file — đọc hàm, hằng số, ngưỡng.
2. Trích vào prompt (mục HIỆN TRẠNG): kích thước file (dòng/KB), tên hàm, hằng số, ngưỡng (threshold/limit), hành vi hiện tại, **thứ ĐANG THIẾU** (VD: painter thiếu arcGlow/dashedRoundRect; store không giữ trace → "đã verify").
3. Ghi rõ "HIỆN TRẠNG (đã audit từ code thật <ngày>)" → subagent khỏi re-explore, prompt không lệch reality.
4. Kiểm xung đột session: liệt kê worktree đang tồn tại (`git worktree list` + `trees/`), file nào session khác có thể đụng → ghi cảnh báo trong prompt (VD: "CodeRunnerView có thể bị MASTER đụng → kiểm worktree trước").
5. Claim chưa verify → để "chưa verify" hoặc bỏ — KHÔNG bịa số liệu. Audit thấy vấn đề mới ngoài scope → nêu rõ, không chữa trong prompt này.

## Khuôn 7 phần (bắt buộc, đúng thứ tự)

1. **BỐI CẢNH (2-3 dòng)**: session nào, đọc file nào trước (handoff/report/decision log có liên quan), nguồn chuẩn (PRODUCTION_PROMPT → SDD/API_REFERENCE/SCREEN_MAP). KHÔNG bắt agent đọc lại cả repo.
2. **MỤC TIÊU (1 câu)**: 1 task = 1 mục tiêu duy nhất. Nếu có 2 mục tiêu → tách 2 task.
3. **HIỆN TRẠNG (đã audit) + Thế mạnh GIỮ LẠI**: kết quả BƯỚC 0 — vấn đề đã verify kèm đường dẫn + số liệu thật, liệt kê từng vấn đề + thứ ĐANG THIẾU. Kèm danh sách **GIỮ LẠI** (cấm vỡ: palette/font/token/test/hành vi cũ/pattern đang hoạt động) — chống regression.
4. **PHẠM VI (danh sách file/dir chính xác)**: ghi rõ file nào được sửa, file nào CẤM đụng (vd engines/** là của dev-engine, docs/ là của dev-docs). Ghi cả ràng buộc đặc thù repo (CẤM Repository/MediatR/PostgreSQL/secret thật; catalog khớp shared/simulation-catalog.json).
5. **TIÊU CHUẨN HOÀN THÀNH (đo đếm được)**: "xong khi X PASS + Y không lỗi + Z file" — không viết "làm cho tốt". **Đo bằng máy, không nhìn**: pixel-verify (script đọc canvas/2 snapshot khác nhau), Lighthouse, grep đếm = 0, bundle delta có ngưỡng gzip (VD: tăng < 15KB), axe 0 critical.
6. **LỆNH VERIFY THẬT (bắt buộc)**: liệt kê lệnh cụ thể + kết quả kỳ vọng. Với task UI: bắt buộc vòng Ollama theo **KHUNG 7 TIÊU CHÍ** (UI: thẩm mỹ, nhất quán, rõ ràng, phản hồi trực quan · UX: luồng thao tác, khả năng tiếp cận, độ thỏa mãn) — mỗi tiêu chí chấm 1-5 + nhận xét cụ thể; tiêu chí ≤ 3 điểm PHẢI sửa (≤ 2 vòng) hoặc từ chối có lý do. Repo không có lệnh → ghi "repo không có lệnh X" thay vì đoán.
7. **GIT (bắt buộc)**: nhánh feature từ `dev` (KHÔNG main); PR base `dev` (đã đóng 8 PR ma vì lỗi này); commit qua `.\commit-as.ps1 <tên> "<message Conventional Commits>"` — phân vai: backend→bao, frontend/UX→son, engine/test→thu, docs→phuc. Trạng thái ghi `docs/work/<task>.md`. **Viết sẵn danh sách commit theo từng task** (1 commit/task, message cụ thể).

## Task spec bên trong prompt (mỗi task)

- **Vấn đề (đã verify)** → **Thiết kế có số liệu cụ thể**: công thức/ngưỡng/sampling thật (VD: `step = ceil(len/3000)`, `maxPerRow = max(10, floor((w-2*margin)/44))`, `slotW = (w-2*margin)/n`) — không "làm cho mượt".
- **Edge cases bắt buộc nêu**: reduced-motion, resize/zoom, empty/error/timeout → **fallback giữ hành vi cũ** (VD: trace rỗng → generator preview như cũ).
- **Test bắt buộc kèm con số**: test case cụ thể (n=60 → ≥2 hàng; 5.000 event → ~3.000 frame). Task data-driven phải có **anti-hardcode test**: "sửa input/code → output/trace đổi" (chứng minh không fake).
- **Giới hạn phạm vi sửa**: file view chỉ cho sửa tối thiểu, logic tách vào composable/util.

## Quy tắc chống lỗi đã đúc kết (nhớ áp dụng)

- **Task NHỎ**: 1 module/nhóm file ≤ ~5-10 file. "12 service + 14 controller" = phải tách thành nhiều task. KHÔNG resume task dài — mỗi lần gọi task là fresh context.
- **Chống context phình**: trước khi giao task phức tạp, dùng `explore` khảo sát vùng code, trích ngữ cảnh ngắn vào prompt — agent không phải đọc lại toàn bộ.
- **Không ai tự chấm bài mình**: dev viết → dev-test verify độc lập → dev-e2e (nếu UI) → dev-review chốt APPROVE/CHANGES REQUESTED.
- **--auto**: ghi quyết định vào `docs/pm-decision-log-<tên>.md` TRƯỚC khi thực hiện; mọi lệch docs chủ ý đều phải có mục đồng bộ docs (dev-docs) đi kèm.
- **Số liệu thật**: THIRD_PARTY/checklist §17.9 phải dùng lệnh thật (npm ls / dotnet list package / build đo bundle), không "chờ tuần X" nếu đã có số.
- **THỨ TỰ THỰC HIỆN theo dependency**: trong prompt ghi rõ thứ tự task + 1 dòng lý do (nền tảng trước: painter primitives → renderers → composable → view cuối cùng vì phức tạp nhất). Task xong → commit + full test trước khi làm task sau; KHÔNG làm task sau khi test task trước fail.
- **Khi nâng cấp prompt cũ**: kèm bảng so sánh cũ/mới (thêm task gì + lý do) — người đọc thấy ngay delta và lý do từng thay đổi.

## Ví dụ tối thiểu (task đúng chuẩn)

```
[dev-frontend, feature/ux-x] Mục tiêu: <1 câu>.
Đọc: <file liên quan + 1-2 nguồn chuẩn>.
Hiện trạng (audit 14/08): <vấn đề đã verify kèm path + số liệu>; GIỮ LẠI: <palette/font/test/hành vi cấm vỡ>.
Phạm vi: <file được sửa> — CẤM: engines/**, docs/, đổi token màu.
Xong khi: build PASS + test không vỡ + smoke 2 màn + <đo bằng máy: pixel-verify/Lighthouse/grep>.
Verify: npm run build; npm test; playwright mở /x console 0 lỗi; chụp → Ollama nhận xét → sửa ≤ 2 vòng.
Git: feature/ux-x từ dev; commit-as son "feat: ..."; trạng thái docs/work/ux-x.md.
Báo cáo: file, verify (kết quả thật), vấn đề, PR link, đề xuất.
```
