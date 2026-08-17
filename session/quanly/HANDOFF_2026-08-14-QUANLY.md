# HANDOFF — 14/08/2026 (quản lý tổng — phiên tiếp theo bắt đầu từ đây)

> File này tóm tắt TOÀN BỘ trạng thái dự án tính đến 02:40 ngày 14/08. Phiên mới đọc file này + `docs/pm-decision-log-viewquality.md` là nắm hết, KHÔNG cần đọc lịch sử cũ.

## 1. DỰ ÁN
- Đồ án tốt nghiệp FPT (GVHD Phạm Ngọc Ái Liên, lớp SD21361, 4 thành viên): **DSA-Visual** — học + trực quan hóa CTDL & giải thuật. Bảo vệ **cuối tháng 8/2026 (~2.5 tuần)**. **CẤM ghi "20 tuần/16 tuần" — chỉ 13 tuần.**
- Nhóm: Mai Tiểu Bảo TD01287 (backend) · Thái Quang Sơn TD01282 (frontend) · Huỳnh Lê Minh Thư TD01131 (engine+test) · Trần Viết Tâm Phúc TD01261 (tài liệu).
- Repo `Tqson3118/Visualization`, nhánh `dev` = tích hợp. **PR base `dev` KHÔNG main**. Commit-as: `.\commit-as.ps1 {son|bao|thu|phuc}` (FE→son, BE→bao, engine/test→thu, docs→phuc).

## 2. TRẠNG THÁI TỔNG (verify 14/08 02:30 — dev @ d0895c6, đã push origin)
- ✅ **H/J/K/L + canvas + MASTER view-quality ĐÃ MERGE XONG**: PR #1-18 (trừ #1 dev→main chờ user duyệt; #9-8-7... ma đã đóng).
- ✅ **MASTER (PROMPT_VIEW_QUALITY_MASTER_V2)** — 36/36 view ĐẠT (hygiene ≥80 + không trục dưới sàn + đặc trưng ≥7; trước: ~60/100 hygiene / 2-3/10 đặc trưng). Phase 0 (PR #12: DESIGN-IDENTITY motif "Data Bench" + DESIGN.md 10 § + standard.md 10 trục + scorecard + EmptyState `[ ]` + @vue-flow/core lazy 154KB chunk, entry 108KB) → Phase 1 4 nhóm (PR #13-16: A 94-96.5 hero engine thật, B 84.5-88.5 path vue-flow, C 91-93.5 BlockToken + 1 hero-stat, D 92-93.5) → Phase 2 (PR #17-18: bổ sung Ladder/NodeHub/FinalTest + HelpView raw button + grep hội tụ emoji/gradient/spacing/icon = 0).
- ✅ Test sau merge: vue-tsc sạch, vitest **95/95**, BE **137 unit + 77 integration = 214**; `npm run build` PASS.
- ✅ **Docker LAPTOP đã deploy bản mới** (14/08 02:30): rebuild neww-frontend + neww-backend, 4/4 healthy. Verify thật trên browser: Home hero mới chạy engine thật (bubble bước 1/99), /path kicker mono mới, **/path/1 vue-flow 5 nodes/4 edges**, console 0 lỗi.
- ✅ Quy tắc mới (13/08 tối): **folder `D:\FPT\neww\trees\` = nơi để file tạm/ghi chú phụ (pre-approve quyền)** — mọi session phải để file phụ TRONG trees/, evidence chính thức docs/work/<session>/, dọn temp trước khi báo xong (HANDOFF mục 5.0).

## 3. PROMPT SẴN SÀNG (session/quanly/)
- **`PROMPT_VIEW_QUALITY_MASTER_V2.md`** — ĐÃ CHẠY XONG (không dùng lại). File tham khảo cấu trúc prompt chuẩn.
- **`PROMPT_VISUALIZE_UPGRADE.md`** (MỚI 14/08, CHƯA CHẠY) — nâng cấp engine visualize, session riêng, worktree `D:\FPT\neww-engine`, nhánh `feature/vis-upgrade` → PR base dev. 4 task: (1) **trace-driven playback code user** (CodeRunner — "viết code tới đâu visual tới đó"; composable `useCodeTracePlayback`, sample 50k→3k frame, anti-hardcode test "sửa code → trace đổi"; sửa DUY NHẤT view CodeRunnerView), (2) **wrap layout mảng dài** (ArrayRenderer, n>36 nhiều hàng, index toàn cục), (3) **stack/queue "thở"** (useStructureTransition 200ms ease-out + prefers-reduced-motion), (4) graph meta.x/y tùy chọn. CẤM thêm thư viện. Lưu ý: chạy SAU Phase 1A/B merge (đã merge) — nhưng CodeRunnerView có thể bị MASTER session khác đụng → kiểm worktree trước.
- **`PROMPT_M_FINAL_REVIEW.md`** — final review 7 trục trước bảo vệ (chạy sau khi visualize + tồn đọng dọn xong).
- **`PROMPT_N_OPTIMIZATION.md`** — tối ưu theo backlog (sau M), đo trước/sau bắt buộc.
- `PROMPT_I/II/FOUNDATION` — đã bị MASTER_V2 thay thế (đã chạy xong), KHÔNG chạy lại.

## 4. TỒN ĐỌNG (thứ tự đề xuất)
1. **QA Ollama cuối cho nhóm D + 3 view P2** (Ladder/NodeHub/FinalTest) — MASTER chưa kịp: chụp light+dark, 3 gate (7 tiêu chí + spacing + bản sắc) qua `qwen2.5vl:3b` (model chính KHÔNG đọc ảnh — phải qua Ollama hoặc pixel-data).
2. **Lighthouse + axe-core** toàn 36 view (A11y ≥90 / Perf ≥80, axe 0 critical) — cần trước bảo vệ.
3. **Gỡ 2 icon lib cũ**: `@lucide/vue` + `@phosphor-icons/vue` vẫn trong package.json (views chỉ dùng `lucide-vue-next` 32×, phosphor 0) — gỡ để sạch bundle + THIRD_PARTY.
4. **Emoji 🪜 còn ở `LadderView.vue:85`** ("Practice Ladder" title) — thay icon lucide.
5. **PR #1 dev→main** — chờ USER duyệt (không tự merge).
6. **Chạy PROMPT_VISUALIZE_UPGRADE** (session riêng — có thể song song với 1-3 vì file khác: engines/ + CodeRunnerView; chú ý xung đột CodeRunnerView).
7. **PROMPT_M** → **PROMPT_N** (backlog `docs/work/final-review-2/backlog.md` — nếu có).
8. **18 ảnh báo cáo** (6 sơ đồ diagrams session + 12 màn UI) → docx pandoc (C:\Users\Administrator\AppData\Local\Pandoc\pandoc.exe) theo BAO_CAO.md; điền ngày bảo vệ (SETUP_TODO — cần user).
9. **Diagrams session** (worktree `D:\FPT\neww-diagrams`, feature/diagrams — KHÔNG merge dev) — file tailieu/diagrams trong working tree chính vẫn là rác chưa commit, đừng đụng.
10. 2FA cần SMTP thật (đang MailHog).

## 5. MÔI TRƯỜNG & CÔNG CỤ (14/08)
- **Docker laptop**: FE `http://localhost:8081` (nginx proxy /api → backend), BE `http://localhost:5000` (health `/health`), MailHog UI `:8025` (SMTP 1025), SQL Server 1433 (DsaVisual, sa/DsaVisual@Dev123, volume giữ data seed K). Rebuild: `docker compose build && docker compose up -d` (từ D:\FPT\neww).
- Accounts: student `student@demo.local / Student@123` (premium, seed K: 8 student + 2 lớp); admin "Quản trị viên".
- FE dev server riêng: `:5174` (5173 bị relay chiếm).
- **GITHUB_TOKEN**: setx user-level — session mới lấy `$tok=[Environment]::GetEnvironmentVariable('GITHUB_TOKEN','User')`; dùng REST API (không có gh CLI): `Invoke-RestMethod api.github.com/repos/Tqson3118/Visualization/pulls -Headers @{Authorization="Bearer $tok";"User-Agent"="opencode"}`.
- **EXA_API_KEY** hardcode `~/.config/opencode/opencode.jsonc` (6e92b042-...). Ollama `qwen2.5vl:3b` @ localhost:11434 (POST /api/generate, JSON {model, prompt, images:[base64], stream:false} — gọi RIÊNG từng gate, không gộp 3 câu).
- MCP: github, exa, context7, playwright, chrome-devtools, memory, duckduckgo, fetch. **Memory MCP có thể fail store trong 1 số session (lỗi env)** — DESIGN.md/decision-log là nguồn chuẩn, không phụ thuộc memory.
- Verify FE: `npx vue-tsc --noEmit` + `npx vitest run` (95) + `npm run build`. Verify BE: `dotnet build DsaVisual.sln` + `dotnet test` (214).

## 6. BÀI HỌC (đã nhúng mọi prompt)
1. KHÔNG nhúng file >5KB vào prompt task — chỉ trỏ đường dẫn (lỗi "task trả rỗng" 4/4 lần).
2. PR base dev; commit-as đúng người; worktree riêng mỗi session (neww-q*, neww-engine...).
3. Ollama 3 gate độc lập (7 tiêu chí / spacing / bản sắc) — không bù trừ; ≤2-5 vòng lặp fix, vòng không cải thiện → dừng + ghi lý do.
4. Không tự chấm bài mình: dev → test → review độc lập.
5. Decision log GHI TRƯỚC khi làm; lệch docs → task đồng bộ đi kèm.
6. **Vệ sinh workspace**: file tạm → `D:\FPT\neww\trees\`; evidence → `docs/work/<session>/`; dọn temp trước báo xong; không rải file vào repo root.
7. Quyết định xuyên-nhóm phải chốt trước khi chạy song song (bài học banner-ngẫu-nhiên).
8. Verify phải ĐO ĐƯỢC (Lighthouse/axe/grep/computed-style/pixel-data) — không "nhìn ổn là ổn".

## 7. GIT WORKTREES ĐANG TỒN TẠI
`neww-diagrams` (feature/diagrams — KHÔNG merge) · `neww-seed` · `neww-teacher` (đã merge xong — có thể xóa) · `neww-qbase` (đã merge) · `neww-qa/qb/qc/qd/qp2` (MASTER — đã merge) · `neww-engine` (CHƯA tạo — cho visualize). Dọn worktree đã merge xong nếu cần: `git worktree remove`.
