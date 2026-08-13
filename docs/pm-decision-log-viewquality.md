# PM Decision Log — View Quality (Phase 0/1/2)

## [2026-08-14] Nhóm C — Premium/Subscription (2 view còn lại): triển khai 6 quyết định xuyên-nhóm + 1 ngoại lệ QR
- Quyết định — QR frame nền TRẮNG (ngoại lệ duy nhất "0 hex rời"): thư viện `qrcode` vẽ module đen trên nền trong suốt; chuẩn QR (ISO/IEC 18004) yêu cầu vùng yên tĩnh nền sáng để máy quét nhận diện — dark-mode canvas-ink sẽ hỏng scan. Dùng utility `bg-white` (Tailwind, KHÔNG phải hex literal trong `<style>`), frame `border-border-subtle`, bỏ ring/shadow primary trang trí cũ. Không token nào thay thế được — ghi nhận ngoại lệ chức năng.
- Quyết định — Khoảnh khắc đầu tư: PremiumView = success BlockToken tone resolved (resolved = "đã ổn định/đúng đáp án" — nâng cấp thành công) enter 300ms easing chuẩn + confetti (đã có qua fireConfetti); SubscriptionView = hero-stat BlockToken "CÒN LẠI n ngày" enter 300ms (pattern ProfileView §5).
- Quyết định — SubscriptionView error state: store `fetchPremium` nuốt lỗi (gamification.ts 89–95, ngoài phạm vi) → view gọi `gamificationApi.fetchPremiumStatus()` TRỰC TIẾP (pattern ShopView) để phân biệt error/empty — không sửa store (ảnh hưởng 36 view).
- Quyết định — Bảng so sánh Free vs Premium trên mobile → card-stack (thead ẩn, `td::before` data-label từ i18n) — §8 cấm scroll ngang bảng chính; desktop giữ table chuẩn §4.6.
- Ảnh hưởng: PremiumView.vue, SubscriptionView.vue, src/i18n/vi.ts (bỏ emoji ❤🎉, thêm premium.compareRows + subscription.benefits/loses/errorTitle/errorDesc/retry/activeBadge, bỏ premium.badge không còn dùng), docs/work/view-quality/audit-*.md + fix-log.md + scorecard.md.

## [2026-08-13] Nhóm C — 4 view gamification (Profile/Shop/Quests/Leaderboard): triển khai 6 quyết định xuyên-nhóm
- Quyết định 1 — Component chung mới `frontend/src/components/ui/BlockToken.vue` (không nằm trong 4 view): quyết định xuyên-nhóm #4 yêu cầu "block-token 1 class chung + index mono" tái dùng XP/streak/gems/rank. Tạo 1 component nhỏ (props label/value/index/tone) để 4 view + nhóm khác dùng chung, tránh trùng logic (trục code 9). Dùng token `--canvas-ink/--data-core/--resolved/--warning/--index-muted` (§2.1) — 0 hex rời. Không đổi token CSS.
- Quyết định 2 — Banner/hero 4 view = surface band level-2: `bg-card-raised border border-border-subtle rounded-lg`, bỏ gradient/blob/shadow/radius-xl (DESIGN.md §1 + §6 level-2). Title H1 = `text-4xl font-semibold tracking-[-0.03em]` (giữ nguyên ở mobile — §8 cấm "mobile font riêng"). Icon chip hero = `bg-muted text-foreground-secondary rounded-lg size-12`.
- Quyết định 3 — Chữ trên nền `canvas-ink` (value hero, rank chip) dùng token `--index-muted` (#6B7385): contrast ≈ 4.26:1 với mono 14px (sát dưới 4.5:1) nhưng ≥ 3:1 với text lớn (value 24px semibold = large text) — đây là token hệ thống đã chốt §2.1 dành riêng cho "text thứ cấp trên nền tối", EmptyState đã dùng. Chấp nhận theo token, KHÔNG tự chế màu mới; note trong audit a11y.
- Quyết định 4 — Card dùng class global `.card` (global.css 85–92 có `box-shadow: var(--shadow-md)`): trong 4 view override `box-shadow: none` (scoped) vì §6 cấm shadow card — không sửa global.css (ảnh hưởng 36 view, ngoài phạm vi nhóm C).
- Quyết định 5 — Khoảnh khắc đầu tư: QuestsView confetti khi claim quest cuối (hoàn thành 5/5) — `canvas-confetti` đã cài + `disableForReducedMotion`; ProfileView = reveal nhẹ hero-stat (1 phần tử duy nhất). Không fade+slide tràn lan.
- Ảnh hưởng: ProfileView.vue, ShopView.vue, QuestsView.vue, LeaderboardView.vue, BlockToken.vue (mới), src/i18n/vi.ts (bỏ emoji toast + key mới profile/leaderboard), docs/work/view-quality/audit-*.md + fix-log.md + scorecard.md + notes.md.

## [2026-08-13] Nhóm C — Fix app-wide: reset `* { padding: 0 }` (global.css unlayered) đè mọi padding utility Tailwind
- Bằng chứng: DevTools computed — nút `Button.vue` sm "Chỉnh sửa" (ProfileView) có `padding: 0px` dù buttonVariants khai báo `px-3` (12px). Root cause: `src/styles/global.css` dòng 7–13 reset `* { margin: 0; padding: 0 }` nằm NGOÀI @layer → unlayered thắng `@layer utilities` của Tailwind v4 → mọi `px-*/py-*/p-*` trên component shadcn bị vô hiệu (trục interactive-sizing 5 — user phàn nàn rõ nhất; DESIGN.md §4.1 "text cách viền ≥ 8px").
- Quyết định: XÓA block reset `* { margin: 0; padding: 0 }` khỏi global.css (không bọc vào @layer base — lightningcss minify fail `Unexpected token Delim('*')` với universal selector trong @layer). Tailwind preflight (đã có sẵn trong tailwind.css, nằm @layer base) reset margin/padding/box-sizing tương đương → utilities thắng đúng chuẩn cascade, hành vi phần tử không dùng utility không đổi. Chỉ sửa global.css, không đụng các view khác. H-E1 trước đây chỉ bọc `button/input/select/textarea { font/color }` — thiếu `*` reset.
- Ảnh hưởng: toàn bộ component dùng padding utility (shadcn button/input/badge/card...) — ĐÃ verify sau fix bằng DevTools (padding button = 12px). Ghi nhận để các nhóm khác không sửa trùng.

## [2026-08-13] Nhóm C — Tabs.vue: tab hit target 22px < 24px (WCAG 2.5.8) — min-h-9
- Bằng chứng: DevTools `getBoundingClientRect` TabsTrigger (ProfileView/LeaderboardView) = 22px cao. Tabs.vue dùng chung 36 view.
- Quyết định: thêm `min-h-9` (36px) vào class TabsTrigger trong `components/ui/Tabs.vue` (giữ py-2, không đổi chiều cao thị giác nhiều) — đạt ≥ 24×24 target chuẩn trục 5. Ảnh hưởng app-wide nhưng tăng nhẹ padding an toàn.

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
