# PM Decision Log — View Quality (Phase 0/1/2)

## [2026-08-14] Nhóm D (classes) — Fix app-wide: reset `* { padding/margin: 0 }` unlayered trong global.css đang giết utility spacing Tailwind
- Phát hiện (đo computed style bằng DevTools, ngày 14/08): mọi button shadcn (`buttonVariants` h-10 px-4), badge (`px-2.5 py-0.5`), input, tab trong TOÀN APP có `padding = 0px` (VD: EmptyState Button pl=0, Badge "Lớp học" h=17.6px). Nguyên nhân: `global.css` dòng 7–13 reset `*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0 }` KHÔNG nằm trong @layer → theo CSS Cascade 5, author style unlayered thắng MỌI @layer (kể cả `utilities`) bất kể specificity → `px-4/py-2/gap-2/…` chết hàng loạt. Tailwind preflight đã reset padding/margin trong `@layer base` nên việc bỏ 2 dòng này không đổi hành vi với phần tử không dùng utility.
- Quyết định: BỎ `margin: 0; padding: 0` khỏi universal reset global.css (giữ `box-sizing: border-box`). Ảnh hưởng: ≥2 view (toàn app) — NHƯNG đây là điều kiện tiên quyết để trục 5 (interactive sizing: text cách viền ≥8px, badge ≥24px) của MỌI view Phase 1 đạt; không phải quyết định visual cho riêng view nào. Legacy scoped CSS (unlayered, tự đặt padding) không đổi; phần tử không có utility vẫn nhận padding 0 từ preflight base layer.
- Ghi chú kiểm chứng sau sửa: Badge/Button/Input/Tabs phải có padding chuẩn; build + vue-tsc PASS; snapshot DOM lại 3 view classes.
- Ảnh hưởng: `frontend/src/styles/global.css` (1 dòng reset); toàn bộ view dùng shadcn-vue.
- [Cùng vụ] Bỏ `shadow-sm` khỏi base class của `ui/card/Card.vue` (shadcn): DESIGN §6 + KILL-LIST "Card đồng loạt nổi bằng nhau + shadow mềm" — card chỉ nổi bằng luminance (bg-card/bg-card-raised + border-subtle/strong), shadow CHỈ dropdown/modal. Đo computed style 14/08: mọi Card trong app có `0 1px 3px rgba(0,0,0,.1)` từ base này. Thay thế bằng border (đã có `border` trong base) — không đổi màu/radius.
- Ảnh hưởng: `frontend/src/components/ui/card/Card.vue` (bỏ 1 class utility).
- [Cùng vụ] Bổ sung `min-h-6` vào base class của `ui/Badge.vue` (wrapper G-F1b): badge `py-0.5 text-xs` = 21.6px cao, dưới chuẩn 24px DESIGN §4.3 (padding ngang ≥6px + height ≥24px). `min-h-6` chỉ tăng chiều cao tối thiểu, không đổi màu/variant — an toàn cho mọi call site (57+ view dùng Badge).
- [Cùng vụ] Chữ trung tính trên panel `canvas-ink` (LUÔN tối bất kể theme): dùng `#d9dde8` — đúng màu text của ENGINE (`canvasTheme.ts` dòng 19 fallback `--color-text-primary`), cùng nguồn 6 màu palette DESIGN-IDENTITY §1.2. Lý do: không có UI token "chữ sáng trên nền tối" (--foreground đổi theo theme sẽ vỡ light mode; --index-muted 3.8:1 < 4.5:1 cho text nhỏ). Khi task token bổ sung `--canvas-text` → thay thế. Áp dụng: `.class-report__lagging-name` (ClassReportView).
- Ảnh hưởng: ClassReportView (1 thuộc tính CSS); phụ thuộc task token §2.4.

## [2026-08-14] Nhóm D (admin) — tái dùng màu text canvas trên panel tối + palette chart đọc engine vars
- Mở rộng quyết định 14/08 (entry #4): text trung tính trên panel `bg-canvas-ink` dùng `#d9dde8` (màu `--color-text-primary` engine `canvasTheme.ts`) — áp dụng thêm cho AdminStatsView (chart ECharts + donut + caption strip), hero-strip caption, node-id block AdminLadderView. Không có UI token "chữ sáng trên nền tối" (khi task token bổ sung `--canvas-text` → thay thế).
- Biểu đồ ECharts (AdminStatsView): palette đọc CSS var `--canvas-ink`/`--data-core`/`--index-muted` (fallback cùng hex) + text `#d9dde8` — vùng dữ liệu LUÔN tối bất kể theme (quyết định #5); axis/splitLine dùng `color-mix(data-core 25%, transparent)` — không hex rời ngoài 6 màu palette + #d9dde8.
- Donut phân bố vai trò dùng `data-core` (Student) / `resolved` (Teacher) / `index-muted` (Admin — neutral, 10% nhỏ) — 3 màu ngôn ngữ dữ liệu, không bịa màu mới, không dùng accent teal (chỉ interactive).
- Ảnh hưởng: AdminStatsView, AdminUsersView, AdminContentView (strip block-token), AdminLadderView (node-id block).

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

<!-- MERGED-BLOCK: nhom khac -->
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

<!-- MERGED-BLOCK: nhom khac -->
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



## [2026-08-13] Nhom B (dev-engine) - 4 view hoc tap: PathView + LessonView + SimulatorView + ExerciseView

- Quyet dinh 1 (PathView -> node-edge graph): Dung @vue-flow/core@1.48.2 da cai (Phase 0) cho PathView - node/edge THAT thay cho pill Duolingo (bang chung r2-fixed-06: path dang render card grid phang du noi dung la graph). Import DONG bang defineAsyncComponent + CSS '@vue-flow/core/dist/style.css' import rieng trong component lazy PathGraph - entry bundle KHONG tang (do lai bundle sau build, ghi so vao day). Node custom theo block-token (nen canvas-ink, block data-core/resolved/conflict, index mono) - signature "Block tho theo buoc".

- Quyet dinh 2 (raw <button> trong vue-flow node): Node custom cua VueFlow la vung canvas dac biet (standard.md truc 5 - "tru canvas/editor/table-cell dac biet -> ghi decision log") - node dung <button> that de co focus/keyboard dung (a11y), KHONG qua Button.vue (node khong phai button UI chuan).

- Quyet dinh 3 (Button.vue them size icon): Mo rong wrapper Button.vue them size 'icon' | 'icon-sm' | 'icon-lg' (map buttonVariants san co) - thay the 4 raw <button> cua SimulatorView (favorite/share/call-stack/legend). Backward compatible (size cu giu nguyen).

- Quyet dinh 4 (SimulatorView chrome): Bo gradient-mint + blob blur + text-gradient (KILL-LIST). Chrome = surface band level-2 (bg-card-raised + border-border-subtle); khung canvas-wrap chuyen nen canvas-ink (motif toi lan toa ra khung ngoai - quyet dinh xuyen-nhom #5); chip dem buoc (ControlBar/StatsBar) bo gradient -> primary solid + mono.

- Quyet dinh 5 (Banner 3 view): PathView/LessonView/SimulatorView banner = surface band level-2, KHONG gradient, KHONG shadow, H1 text-3xl (trong scale §3, nhat quan voi SimulationsView da dung clamp 2xl-3xl).

- Quyet dinh 6 (emoji -> lucide): Bo toan bo emoji icon chuc nang trong 4 view (🎯🔒⭐▶🏁❤🎉) -> icon lucide-vue-next (Route/Milestone, Lock, Play, CheckCircle2, Flag, Heart, PartyPopper-khong dung emoji trong toast).

- Ghi chu: Khong xoa @lucide/vue + @phosphor-icons/vue (decision log Phase 0 - Phase 2 moi go). Cac component simulator khac (PseudocodePanel/LegendPanel/ExplainPanel/ManualPracticePanel/DemoBanner) con gradient/hex/700 - ngoai pham vi task nay, ghi notes Phase 2.- KET QUA BUNDLE (sau khi cai PathGraph vue-flow): entry `index-*.js` = 106.59 kB (gzip 34.42) vs 106.44 kB (gzip 34.36) truoc - delta +0.15 kB (khong doi y nghia, do hash/order). @vue-flow/core nam TRON trong lazy chunk `PathGraph-*.js` = 154.54 kB (gzip 50.31) - chi tai khi mo /path/:topicId. PathView chunk 7.42 kB (truoc 7.46). SimulatorView 28.29 kB (truoc 27.94, +icon lucide). Build PASS (vue-tsc + vite).

## [2026-08-13] Nhom B (dev-engine) - Task P1-B2: LabView + CodeRunnerView (2 view)

- Quyet dinh 1 (LabStage.vue - component con cua LabView): LabView render truc tiep LabStage voi props (title/initialArray/standardSteps). De LabView dat chuan (vung du lieu LUON toi - quyet dinh xuyen-nhom #5, canvas block-token, bo emoji/gradient), bat buoc sua toi thieu LabStage.vue (cung dung trong LadderShell/LadderView - cai thien ca 2, khong doi API/logic). Tien le: ControlBar/StatsBar da sua toi thieu cho SimulatorView.

- Quyet dinh 2 (raw button trong canvas LabStage): Cell o hoan doi la vung canvas du lieu (standard.md truc 5 - "tru canvas/editor/table-cell dac biet -> ghi decision log") - giu <button> that de co focus/keyboard, NHUNG them aria-pressed khi selected + aria-label, bo weight 800, bo shadow hover (chi doi border-color).

- Quyet dinh 3 (vung du lieu toi): LabStage canvas (day o + index) + CodeRunner editor/gutter/output box = vung du lieu -> bg-canvas-ink LUON toi bat ke theme (quyet dinh #5). Text tren nen toi dung token index-muted + color-mix(in srgb, var(--color-index-muted) 40%, white) cho text chinh (khong hex roi; --color-index-muted = #6B7385 la token da co).

- Quyet dinh 4 (ghi chu dev Monaco): Bo hẳn dong "* Monaco editor se duoc bat khi cai goi monaco-editor..." (r2-fixed-09) - thay bang caption phim tat mono huu ich (Ctrl+Enter chay) theo Content Voice §9; KHONG can giu thong bao ky thuat cho nguoi hoc.

- Quyet dinh 5 (empty state LabStage): LabStage chua co empty state - bo sung EmptyState component chung (motif [ ] da redesign san) khi khong co du lieu, copy theo §9; giu API cu (khong pha call site).

- Ghi chu: icon "🪜 vỡ glyph" + empty state puzzle (r2-fixed-07) nam o LadderView.vue:85 + QuizStage.vue:157 - NGOAI pham vi task 2-view nay (LadderView la view khac), ghi notes Phase 2 cho task LadderView. BenchmarkPanel raw <button> chip thuoc BenchmarkView - task sau.



## [2026-08-13] Nhom B (dev-engine) - Task P1-B3 (CUOI): BenchmarkView + PathRedirectView (2 view)

- Quyet dinh 1 (pham vi BenchmarkPanel.vue): BenchmarkPanel la NOI DUNG duy nhat cua BenchmarkView (route /benchmark/:k1/:k2) - empty state "scissors", label "Miễn phí tim (20.4)", chip raw <button>, bang ket qua deu nam trong component nay. De BenchmarkView dat chuan bat buoc sua toi thieu BenchmarkPanel.vue (khong doi API/logic/worker/echarts tree-shake). Day la task cuoi cua nhom B - notes.md da ghi san "BenchmarkPanel raw button chip thuoc task BenchmarkView".

- Quyet dinh 2 (copy label "Miễn phí tim (20.4)"): CHUOI NAY KHONG nam trong i18n (grep vi.ts khong co) - hardcode tai BenchmarkPanel.vue:282 + BenchmarkView.vue:50; "(20.4)" la so tham chieu SDD lo ra UI (KILL-LIST microcopy). Y nghia that: benchmark KHONG tru tim nguoi hoc. Sua copy ro nghia thanh "Không tốn tim" (ca 2 cho, bo so muc 20.4). KHONG dung VD "Tìm kiếm tuyến tính — 20.4ms" cua prompt vi sai y nghia (do la nhan xet cua tac gia prompt, khong phai noi dung that cua man hinh).

- Quyet dinh 3 (benchmark chips -> Button.vue): 5-17 chip chon thuật toán dang raw <button> (benchmark__chip, padding 4px 12px + gradient khi on) -> Button.vue variant primary/secondary size sm + aria-pressed (toggle segment chuan buttonVariants, khong tao variant moi). Bo gradient chip + shadow-sm + translateY hover -> hover border-color theo variant co san.

- Quyet dinh 4 (bang + chart + ket luan = vung du lieu LUON toi): Quyet dinh xuyen-nhom #5 - bang ket qua benchmark (so lieu do duoc) + bieu do + panel ket luan chuyen nen canvas-ink, chu mono index-muted / block data-core (block-token cho duration, n = index mono). chartOption ECharts doc mau tu CSS var qua cssVar() san co (pattern CodeRunnerView) - series palette doc --color-data-core/resolved/conflict/warning/info, KHONG hex roi (fallback hex chi khi var khong doc duoc). Khi chart luon toi thi khong con phu thuoc ui.theme (gio nguyen logic recompute, khong xoa).

- Quyet dinh 5 (bang benchmark mobile -> card-stack): standard §8 cam scroll ngang bang chinh o mobile. Tai <=640px: thead an, tr -> card (grid 2 cot, td co ::before data-label mono) - CSS-only, khong doi DOM JS.

- Quyet dinh 6 (PathRedirectView card -> RouterLink): card topic dang <article role="button" tabindex="0" @keydown.enter> (thieu Space + focus semantics khong chuan) -> RouterLink den {name:'path-topic'} - link native (Enter/Space/focus-visible co san, global :focus-visible da co). Bo openTopic() (logic giu nguyen qua router). Bo class .card/.card--interactive (shadow-md + hover shadow-lg/translateY/scale - vi pham §6) -> card level-1 + hover border-color + focus ring.

- Quyet dinh 7 (PathRedirectView header -> surface band level-2): giong PathView/CodeRunnerView - bg-card-raised + border-subtle + kicker mono "LEARNING PATH · TOPIC 01/05" + H1 text-3xl/600/-0.02em + Route icon lucide (bo 🎯). Card index vong tron primary + weight 800 -> kicker mono index-muted (bo accent trang tri + weight 800).

- Quyet dinh 8 (EmptyState icons): PathRedirectView icon="map" KHONG ton tai trong SVG_PATHS (fallback x-circle - bug am tham) -> "book"; BenchmarkPanel icon="scissors" (cay keo khong lien quan benchmark) -> "hourglass" (do thoi gian thuc te, co trong SVG_PATHS). Empty state = EmptyState component chung (da redesign).

- Quyet dinh 9 (animation 1-2 khoanh khac/view): BenchmarkView - chi results region (bang+chart) enter 250ms cubic-bezier(0.16,1,0.3,1) khi co du lieu; PathRedirectView - card grid stagger enter (max 8 * 40ms delay, 240ms, easing chuan). Ca 2 kem prefers-reduced-motion. Bo --transition-fast (ease 150ms) tren chip/control (KILL-LIST V2).



<!-- MERGED-BLOCK: nhom khac -->

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







## [2026-08-13] Nhóm A (dev-frontend) — 5 view landing/auth (feature/view-quality-a)



- Quyết định 1 — HomeView header lặp: App.vue đã render AppHeader toàn cục (brand + Đăng nhập/Đăng ký cho khách). HomeView tự render header thứ 2 (`home__nav`) → 2 dòng "DSA Visual" + nav chồng nhau. SỬA: bỏ header nội bộ trong HomeView, giữ AppHeader toàn cục (minimal, không đụng component dùng chung).



- Quyết định 2 — Hero Home: bỏ `bg-aurora-gradient` + blob ::before/::after + text-shadow + shadow-lg (KILL-LIST hero công thức + gradient). Thay = surface band level-2 (`bg-card-raised border-b border-border-subtle`) + panel demo tối `bg-canvas-ink` chạy mô phỏng THẬT: `getSimulation()` từ engines + `generate()` ra Step[], autoplay nhẹ, render bằng DOM block (block-token + index mono + status data-core/resolved/conflict) — KHÔNG dùng canvas ArrayRenderer trong task này (cần harness đầy đủ) → ghi notes Phase 2. 3 demo: sort.bubble / search.binary / graph.bfs (đúng FR-7.6, catalog.ts demoAllowed).



- Quyết định 3 — Auth views (login/register/forgot/reset): brand aside gradient aurora + blob + glassmorphism badge → thay bằng panel tối `bg-canvas-ink` motif Data Bench (badge mono, tagline, điểm bán bằng block-token mini + index mono). Trên panel luôn tối dùng opacity utilities `white/90, white/60` + `text-index-muted` cho viền/text (không phải hex rời; border-subtle theo theme sai màu trên nền tối — tương đương engine canvasTheme fallback #d9dde8/#6b7385).



- Quyết định 4 — Icon: 5 view chuyển hết BaseIcon → lucide-vue-next. `ui/Input.vue` (dùng chung 10 view) sửa ADDITIVE: prop `icon` nhận thêm Component lucide (string legacy vẫn qua BaseIcon) → view khác không đổi hành vi.



- Quyết định 5 — global.css page transition: `220ms ease` (easing mặc định, >150ms — KILL-LIST V2) → enter `cubic-bezier(0.16,1,0.3,1)`, exit `cubic-bezier(0.7,0,0.84,0)`. Ảnh hưởng mọi route (file dùng chung) — sửa tối thiểu, giữ 220ms + prefers-reduced-motion có sẵn.



- Quyết định 6 — Register segmented vai trò: 2 `<button>` raw `.register__role-option` → Button (ghost, size default) + `aria-pressed`, active = `bg-card border border-border` (bỏ shadow-sm — §6 shadow chỉ dropdown/modal/focus).



- Quyết định 7 — Mini-sim + prefers-reduced-motion: khi reduce → không autoplay, hiện frame đầu tĩnh. Swap pop 240ms `cubic-bezier(0.16,1,0.3,1)` (transform/opacity); đổi status màu = trạng thái (không transition >150ms).



- Ghi chú shared: `AppHeader.vue`, `Button.vue`, `ui/card/*`, `ui/input/*` KHÔNG sửa trong task này (ngoài 5 view); chỉ Input.vue (Q4) + global.css (Q5) additive/minimal.







## [2026-08-13] Nhóm A — HOTFIX: reset `* { padding: 0 }` unlayered đè mọi padding utility Tailwind



- Bằng chứng đo thực tế (chrome-devtools, /login, guest context): input shadcn có class `px-3 pl-9` nhưng `getComputedStyle().paddingLeft = 0px`; Button submit `h-44px` nhưng `paddingLeft = 0px`; CTA hero `px-8` → 0px. Nguyên nhân: `global.css` dòng 7-13 khai báo `* { box-sizing; margin: 0; padding: 0 }` UNLAYERED — theo CSS cascade, style unlayered thắng mọi `@layer` (Tailwind v4 utilities nằm trong `@layer utilities`) → toàn bộ padding utility của app chết (không chỉ 5 view của nhóm A; shadcn button/input khắp app mất padding ngang → chữ chạm viền, vi phạm trục 5 interactive sizing).



- SỬA (tối thiểu, 1 chỗ): chuyển reset đó vào `@layer base` (cùng pattern đã có trong file: `button,input,select,textarea { font: inherit; color: inherit }` đã nằm trong @layer base từ H-E1). Tailwind preflight vốn đã reset margin/box-sizing trong base layer → UI legacy (.card/.btn/.input có padding tường minh, unlayered) KHÔNG đổi; shadcn controls khắp app được khôi phục padding thiết kế.



- Rủi ro: thay đổi thị giác toàn app theo đúng thiết kế (từ "0 padding sai" → "padding chuẩn") — không phải regression, nhưng ảnh hưởng view khác theo chiều sửa lỗi; ghi rõ để pm biết.







## [2026-08-13] Nhóm A (dev-frontend) — 4 view landing còn lại (NotFound/Privacy/Help/Simulations)



- Quyết định 1 — TOC PrivacyView: raw `<button>` `.privacy__toc-link` → native anchor `<a href="#sec-N">` (điều hướng mục lục, giữ aria-label breadcrumb nav). Lý do: TOC là navigation, anchor native cho focus/keyboard/URL-hash miễn phí + Lenis anchors:true xử lý smooth; bỏ hàm JS scrollToSection (trục 5a `<button` raw = 0 đạt qua bỏ button, không phải wrap button).



- Quyết định 2 — FAQ trigger HelpView: raw `<button>` → `buttonVariants({variant:'ghost'})` + `cn` override layout `w-full h-auto justify-between gap-3 rounded-md px-3 py-2` (accordion row control — cần h-auto vì câu hỏi wrap 2 dòng; padding ≥ px-3/py-2 đạt chuẩn tối thiểu, focus-visible ring từ buttonVariants). Làm đúng trục 5a "qua buttonVariants()".



- Quyết định 3 — Badge.vue (shared, ~57 call site): thêm `min-h-6` vào base class — badge height ≥ 24px (trục 5f). Additive, không đổi màu/size chữ; ảnh hưởng toàn app theo hướng sửa lỗi chuẩn.



- Quyết định 4 — SimulationsView: `BenchmarkPanel` + `CheatSheetTable` (chunk nặng) → `defineAsyncComponent` lazy theo tab (trục 10 route-level splitting; không sửa file 2 component này).



- Quyết định 5 — NotFound 404: motif "mảng mất index" — panel tối canvas-ink, block `4·0·4` (bg-data-core) + block 03 dashed conflict "out of bounds" + index mono 00–03 → kể câu chuyện 404 bằng ngôn ngữ dữ liệu của app; CTA qua buttonVariants; bỏ gradient ring/Compass/shadow/800.



- Quyết định 6 — Banner 3 view (Privacy/Help/Simulations): bỏ aurora-soft/gradient-mint + shadow + clamp title → surface band level-2 (`bg-card-raised` + `border-border-subtle`, rounded-lg, không shadow) + H1 48px/600/-0.03em + icon lucide muted square (pattern HomeView/LoginView đã chốt); Simulations thêm strip block-token + index mono trong banner (dữ liệu tuần tự → quyết định 4 xuyên-nhóm).



- Quyết định 7 — Stat Simulations: 3 stat đồng hạng → bỏ gradient/800 → value Geist 600 text-2xl tracking -0.015em + label tertiary text-xs; strip block-token là "hero motif" duy nhất (tối đa 1/màn); không tạo thêm hero-stat.







## [2026-08-13] Nhóm A — HOTFIX 2: global.css `a { color }` unlayered đè text-primary-foreground trên RouterLink-as-button



- Bằng chứng đo thực tế (chrome-devtools): CTA "Khám phá mô phỏng" HomeView + CTA 404 (NotFoundView) có `bg-primary` (oklch(0.52 0.12 185)) nhưng `color: rgb(0,126,114)` = --color-primary → chữ teal trên nền teal, contrast ~1.2:1 (fail WCAG AA). Nguyên nhân: `global.css` dòng `a { color: var(--color-primary) }` UNLAYERED — thắng mọi @layer kể cả utilities → `text-primary-foreground` (utility) chết trên mọi anchor dùng buttonVariants (404, HomeView hero CTA — view task A1 cũng dính nhưng re-audit bỏ sót).



- SỬA (minimal, 1 chỗ): đưa `a { color; text-decoration }` + `a:hover { underline }` vào `@layer base` (utility thắng lại — đúng ý định thiết kế); thêm `a.inline-flex:hover { text-decoration: none }` để anchor-button (buttonVariants đều có inline-flex, legacy .btn cũng vậy) không bị gạch chân khi hover. Text link thường không đổi (vẫn primary từ base). Ảnh hưởng toàn app theo hướng SỬA lỗi contrast (cùng pattern hotfix padding 2026-08-13).







## [2026-08-14] Nhóm A (dev-frontend) — CheatSheetView `/cheatsheet` (view cuối nhóm A)



- Quyết định 1 — Chrome header CheatSheet: bỏ `--gradient-mint` ×3 (chrome bg, icon square, title gradient-clip) + `shadow-md` + `::after` overlay 68% → surface band level-2 (`--color-card-raised` + `--color-border-subtle`, rounded-lg, KHÔNG shadow) + H1 48px/600/-0.03em + icon lucide `Table2` 20px muted square (pattern SimulationsView/HomeView đã chốt).



- Quyết định 2 — Strip mono trong banner: thêm strip Big-O block-token `BIG-O 00–04 · CHEATSHEET` (5 chip tối `--color-canvas-ink`, value mono text-xs `white/90` + index mono `--color-index-muted` 00–04) — quyết định xuyên-nhóm "banner = surface band + strip mono dữ liệu" + "cheat-sheet đầy chip Big-O dùng mono chuẩn".



- Quyết định 3 — Chip Big-O trong bảng: giá trị complexity (best/average/worst/space) từ text mono trần → **block-token chip tối** `bg-canvas-ink` + `font-mono text-sm` + text `rgba(255,255,255,0.92)` (tiền lệ đã chốt: panel LUÔN tối không theo theme → white/* không phải hex, tương đương engine canvasTheme fallback — notes.md mục 3); min-h-6 + px ≥ 8px (trục 5f).



- Quyết định 4 — Raw `<button>` ×2 (chip lọc + "▶ Xem mô phỏng") → `Button` shadcn (`ui/button`): chip = `variant="outline" size="sm"` + `aria-pressed` + active `bg-primary text-primary-foreground border-primary` (tailwind-merge qua cn — RegisterView precedent); nút mở sim = `variant="outline" size="sm"` + icon lucide `Play` 16px, bỏ glyph `▶`; giữ aria-label `Mở mô phỏng {title}`.



- Quyết định 5 — Bảng mobile: `min-width: 640px` + `overflow-x: auto` (scroll ngang bảng chính — cấm §8) → CSS-only card-stack ≤640px: thead ẩn, mỗi `<tr>` = 1 card (`bg-card border rounded-lg p-4`), td flex `justify-between` + `::before content: attr(data-label)` (thêm `:data-label` i18n cho 5 cột số liệu/hành động; cột tên full-width). DOM table giữ nguyên cho screen-reader — không render 2 template.



- Quyết định 6 — i18n: hardcode chuỗi tiếng Việt trong CheatSheetTable → thêm key `messages.cheatsheet.*` (all/searchPlaceholder/searchAria/col*/simulate/openSimulation/source…); sửa chuỗi `sub` bỏ glyph `▶`; badge count `primary` → `muted` (accent CHỈ interactive — count là meta, không phải CTA).



- Ghi chú: `global.css` `.input { transition: border-color 200ms ease }` (easing mặc định >150ms) còn dùng trong search của view — file shared ngoài phạm vi task (đã ghi notes, đề xuất task chung); EmptyState giữ `icon="search"` (BaseIcon legacy — shared component giữ API cũ).

## [2026-08-13] Resolve conflict global.css (nhánh C vs dev/A)
- Quyết định: conflict reset padding — giữ bản dev (A): đưa * { margin/padding/box-sizing } vào @layer base thay vì xóa hẳn (bản C). Lý do: explicit reset an toàn hơn phụ thuộc preflight, bản A đã merge + verified (95/95 test, visual).
- Ảnh hưởng: frontend/src/styles/global.css (giữ @layer base).

## [2026-08-13] Resolve conflict nhánh D vs dev
- Quyết định: Badge.vue — giữ ours (cả 2 đã có min-h-6, conflict chỉ comment). global.css — giữ theirs/dev (@layer base), nhất quán với resolve nhánh C.
- Ảnh hưởng: Badge.vue, global.css (dev đã merge).

## [2026-08-14 02:35] KẾT THÚC PHIÊN --auto PROMPT_VIEW_QUALITY_MASTER_V2
- Quyết định: Phase 0 + Phase 1 (4 nhóm) + Phase 2 đã xong, tất cả merge dev (PR #12, #13, #14, #15, #16, #17, #18). 36/36 view ĐẠT (scorecard cuối đã commit). KHÔNG merge PR #1 (dev→main) — chờ vòng QA cuối (tồn đọng #1).
- Ảnh hưởng: dev @ 528eeef (sau #18); worktree: neww-qbase (q0), neww-qa/b/c/d (Phase 1), neww-qp2 (P2), neww-qdocs (docs) — giữ nguyên cho phiên sau.
- Tồn đọng chính: Ollama QA nhóm D + 3 view P2; Lighthouse/axe-core toàn 36 view; gỡ @lucide/vue + @phosphor-icons khỏi package.json; emoji trong contentHtml CMS; theme toggle wire <html>.
