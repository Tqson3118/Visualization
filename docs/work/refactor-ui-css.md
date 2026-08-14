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
