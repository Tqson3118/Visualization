# PM Decision Log — View Quality (Phase 0/1/2)

## [2026-08-13] Khởi động PROMPT_VIEW_QUALITY_MASTER_V2 (--auto)
- Quyết định: Chạy toàn bộ 3-Phase theo PROMPT_VIEW_QUALITY_MASTER_V2.md ở chế độ --auto. Xác nhận dev @ bf6028c (H/J/K/L đã merge, test xanh 214 BE + 95 FE). Working tree chính D:\FPT\neww có file rác (diagrams sync) — KHÔNG đụng vào, mọi việc trong worktree.
- Ảnh hưởng: toàn bộ frontend view; docs/work/view-quality/*.

## [2026-08-13] Xác nhận phạm vi session Diagrams (mục 0.5.1 bắt buộc)
- Quyết định: Đọc docs/pm-report-diagrams.md + git log feature/diagrams @ 1180128 → session này dựng 6 sơ đồ draw.io TÀI LIỆU (use-case/ERD, tailieu/diagrams/, XML+PNG), KHÔNG làm node-edge/graph rendering trong frontend app. → KHÔNG xung đột phạm vi với Phase 1 Nhóm B (path = @vue-flow/core). Nhóm B ĐƯỢC cài @vue-flow/core khi tới Phase 1, nhưng Phase 0 vẫn đo bundle trước/sau.
- Ảnh hưởng: quyết định cài @vue-flow/core tại Phase 0 BƯỚC E; không chặn Phase 0/1.

## [2026-08-13] Worktree neww-qbase đã tồn tại nhưng trống
- Quyết định: git worktree list cho thấy D:\FPT\neww-qbase đã được tạo sẵn trên nhánh feature/view-quality-base @ bf6028c nhưng chưa có commit mới/file foundation nào → tái sử dụng worktree hiện có, không tạo mới, không reset.
- Ảnh hưởng: Phase 0 làm việc tại D:\FPT\neww-qbase.

## [2026-08-13] P0-1/2/3 DONE — bản sắc + chuẩn audit đã chốt
- Quyết định: DESIGN-IDENTITY.md chốt motif "Data Bench" (2 lớp: sân khấu tối cho dữ liệu / giấy lặng cho UI), signature "Block thở theo bước" (block phản ứng trace thật stepExecutor, mọi block có index mono), 6 màu có nguồn canvasTheme.ts. DESIGN.md 10 § + danh sách token cần thêm (text 4 tầng, border-subtle/strong, card-raised, success/warning/info, canvas-ink/data-core/resolved/conflict/index-muted, radius 4/8/12/16). standard.md 10 trục = 100 + trục Đặc trưng tách riêng + 3 điều kiện ĐẠT; scorecard.md 36 view, 10 view ưu tiên CAO.
- Ảnh hưởng: frontend/DESIGN-IDENTITY.md, frontend/DESIGN.md, docs/work/view-quality/standard.md + scorecard.md (worktree neww-qbase).

## [2026-08-13] Icon library — KHÔNG gỡ dependency ngay
- Quyết định: Chốt lucide-vue-next duy nhất. KHÔNG gỡ @lucide/vue + @phosphor-icons/vue ở Phase 0 (rủi ro vỡ import view đang dùng) — Phase 1 chuyển dần import sang lucide-vue-next, Phase 2 gỡ khi grep sạch. Ghi rõ vào DESIGN.md §4.
- Ảnh hưởng: package.json (chưa đổi); task Phase 1 từng nhóm.

## [2026-08-13] Memory MCP không khởi tạo được store
- Quyết định: Ghi 6 quyết định xuyên-nhóm vào memory MCP thất bại (ENOENT C:\Users\Administrator\.opencode\memory.json). Bỏ qua — DESIGN.md + decision-log là nguồn chuẩn; prompt Phase 1 sẽ nhúng trực tiếp 6 quyết định thay vì bắt session tra memory.
- Ảnh hưởng: prompt dispatch Phase 1 (phải tự chứa 6 quyết định).

## [2026-08-13] Bản sao decision log vào worktree qbase
- Quyết định: File quyết định chính thức đặt tại docs/pm-decision-log-viewquality.md TRONG worktree neww-qbase (để commit theo PR Phase 0). Bản ở D:\FPT\neww (working tree chính) là bản điều hành — gộp vào sau khi merge, tránh ghi 2 nơi.
- Ảnh hưởng: toàn bộ quyết định Phase 0/1/2 sẽ ghi vào qbase.

## [2026-08-13] @vue-flow/core cài tại Phase 0
- Quyết định: CÀI @vue-flow/core@^1.48.2 (npm install @vue-flow/core, thêm 16 packages, 0 vulnerabilities, peer vue ^3.3.0 — khớp vue ^3.5.34 của dự án). Căn cứ: session Diagrams đã xác nhận không xung đột (chỉ làm sơ đồ draw.io tài liệu tailieu/diagrams/, KHÔNG làm node-edge rendering frontend — đã ghi mục [2026-08-13] Xác nhận phạm vi session Diagrams); BƯỚC E Phase 0 khuyến nghị mạnh cài.
- Đo bundle trước/sau (npm run build, so sánh docs/work/view-quality/bundle-before-vueflow.txt vs bundle-after-vueflow.txt): entry chunk `index-DBB1X6Bg.js` = 106.44 kB (gzip 34.36 kB) trước = SAU (cùng hash — không đổi byte nào); vendor = 142.60 kB không đổi; tổng JS 156 chunks = 2,270.21 kB trước = sau (delta 0.00 kB). Lý do: CHƯA có import tĩnh @vue-flow/core nào trong src (grep sạch) → Vite không nhét lib vào bundle entry.
- Ghi chú: Nhóm B Phase 1 BẮT BUỘC import động bằng defineAsyncComponent khi dựng path view (lazy-load), tránh phình entry; nhóm khác không cài trùng. API chính (context7, @vue-flow/core 1.48.x): `import { VueFlow, Handle, Position, useVueFlow } from '@vue-flow/core'` + styles `@vue-flow/core/dist/style.css`; node/edge dạng `ref([{ id, type, label, position }, ...])` + `ref([{ id, source, target, animated }, ...])`, bind qua `v-model:nodes` / `v-model:edges`.
- Ảnh hưởng: package.json (dependency mới), package-lock.json; bundle entry hiện KHÔNG đổi.
