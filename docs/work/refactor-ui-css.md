# WORK LOG — Refactor UI CSS (PROMPT_REFACTOR_VIEWS_CSS)

> Session: trích xuất khối layout/CSS lặp trong 9 views (Admin ×5, Class ×3, Home) thành shared components.
> Nguồn chuẩn: `frontend/DESIGN.md` (§2/§3/§6) + `frontend/src/styles/tokens.css` + `tailwind.css`.

## Task A — components (14/08/2026 · dev-frontend · branch feature/refactor-ui-css)

Tạo mới 4 shared components (KHÔNG sửa view nào — 9 views sẽ refactor ở các task sau):

| File | Dòng | Nội dung |
|---|---|---|
| `frontend/src/components/ui/PageHero.vue` | ~150 | Banner surface band level-2: props title/description/badge/padding('xl'\|'lg')/border('bottom'\|'full'); slots badges/title/description/actions/side/bottom; media 640px |
| `frontend/src/components/admin/AdminHeroStrip.vue` | ~150 | Mono strip block-token canvas-ink (5 block + index mono): props count/label/activeIndices; @keyframes strip-enter 280ms + prefers-reduced-motion; caption text-right → left ở mobile |
| `frontend/src/components/ui/StatCard.vue` | ~190 | KPI 2 cấp: hero = Card level-2 + panel tối canvas-ink (slot #panel + value data-core + label mono index-muted, enter 280ms) / default = Card level-1 (icon + label tertiary + value tabular-nums + diff mono); loading → Skeleton 108px |
| `frontend/src/components/ui/DetailSection.vue` | ~55 | Section drawer: title uppercase xs 600 tracking 0.06em + icon lucide 14 + content flex column |

### Verify thật (chạy tại `D:\FPT\neww\trees\refactor-ui-css\frontend`)

- `npm run build` → vue-tsc 0 lỗi, vite **"✓ built in 1.46s"**.
- `npx vitest run` → **174 passed (174)**, 19 test files (component mới chưa có test — không vỡ test cũ).
- Không class trùng tên view cũ (grep `page-hero|admin-strip|stat-card|detail-section` trong `frontend/src` chỉ khớp keyframes scoped view cũ); CSS scoped của component tự hash → không xung đột.
- Không hex rời (chỉ dùng token `--card-raised/--border-subtle/--canvas-ink/--data-core/--index-muted/--foreground[-secondary/-tertiary]/--muted/--border` + `rgba(66,85,255,0.25)` đã có trong audit strip/panel).

### Ghi chú quyết định

- StatCard hero dùng `color: var(--data-core)` cho value (theo ClassReportView 338-369 như task spec); AdminStatsView đang dùng `#d9dde8` — view đó khi refactor có thể chèn content qua slot `#panel`/`#default` hoặc đề xuất token `--canvas-text` (đã có note #7 ở docs/work/view-quality/notes.md).
- StatCard hero không đặt `aria-hidden` lên panel (value là nội dung có nghĩa); `#panel` slot để view chèn block row (VD AdminStatsView 3 blocks trước value).

## Task B — admin views (14/08/2026 · dev-frontend · branch feature/refactor-ui-css)

Refactor 5 màn admin dùng 4 shared components (Task A): PageHero + AdminHeroStrip thay banner/strip; StatCard thay hero-KPI + 4 KPI; DetailSection thay drawer section. KHÔNG đổi logic/i18n/event; xóa toàn bộ CSS scoped trùng.

| File | CSS scoped trước → sau |
|---|---|
| `frontend/src/views/AdminStatsView.vue` | 304 → 126 |
| `frontend/src/views/AdminUsersView.vue` | 499 → 365 |
| `frontend/src/views/AdminContentView.vue` | 455 → 335 |
| `frontend/src/views/AdminSettingsView.vue` | 186 → 145 |
| `frontend/src/views/AdminLadderView.vue` | 175 → 131 |
| **TOTAL** | **1619 → 1102** (−32%) |

Sửa thêm (BẮT BUỘC đi kèm Task B):
- `frontend/src/components/ui/StatCard.vue`: + prop `index?: string`; level hero hiển thị head (icon + label) CHỈ khi có `icon`, panel hiển thị `.stat-card__index` (= index || label); `v-bind="$attrs"` để view truyền class grid (VD `admin-stats__kpi-hero`); CSS + `.stat-card--hero .stat-card__head` + `.stat-card__index`; icon rule mở rộng `.stat-card--default .stat-card__icon` → `.stat-card__icon` (head hero giống head default).
- `frontend/src/components/admin/AdminHeroStrip.vue`: FIX BUG reactivity — `size` tính 1 lần ở setup nên `count` động (pendingCount/lessons.length) đóng băng ở giá trị khởi tạo (1 block). Sửa `size` thành computed; + test `AdminHeroStrip.spec.ts` (4 cases: count render, count=0 → 1 block empty, reactive update, activeIndices thắng count).

### Verify thật (chạy tại `D:\FPT\neww\trees\refactor-ui-css\frontend`)

- `npm run build` → vue-tsc 0 lỗi, vite **"✓ built in 1.45s"**.
- `npx vitest run` → **178 passed (178)** — 174 cũ + 4 test mới AdminHeroStrip (20 test files).
- Browser check (Chrome DevTools, login seed admin@system.local/Admin@123, dev server 5173): cả 5 màn render đúng — PageHero badge/title/sub + strip động đúng số block (Users 02 chờ duyệt → 2 blocks; Content 8 bài/5 chủ đề → 5 blocks) + StatCard hero (head Users+label, panel 3 blocks + value + index USERS·01, grid-column span 2 desktop / span 1 mobile) + 4 KPI default + 4 DetailSection drawer (icon GraduationCap chỉ section 2) + container max-width giữ (settings 760px, ladder 1000px). Console error = 0 (2 warning shadcn Drawer có sẵn — không phải từ refactor), horizontal overflow = 0 (desktop + 640px).
- Không thêm `<button` raw; không hex rời mới (StatCard hero value dùng `var(--data-core)` theo Task A — khác `#d9dde8` cũ, đã ghi note Task A); scoped component tự hash → không xung đột class.

### Ghi chú quyết định Task B

- StatCard hero panel `flex: 1; justify-content: center` (thiết kế Task A — cho ClassReport full-width) làm hero card cao hơn bản cũ ~20px khi nằm trong grid 2 cột — chấp nhận theo thiết kế shared component, không override.
- Label hero head dùng `.stat-card__label` base (mono index-muted) thay vì CardDescription muted-foreground cũ — theo đúng CSS spec Task B (chỉ thêm 2 rule).
- Không chạm `PageHero`/`DetailSection` (không lỗi); strip caption mobile `text-align: left` do AdminHeroStrip tự lo (media 640px).

### Commit

- `13d4ba7` refactor(admin): adopt shared PageHero and strip components across all admin views (son)

