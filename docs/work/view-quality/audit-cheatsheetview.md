# AUDIT — CheatSheetView (`/cheatsheet` — CheatSheet Premium: bảng Big-O + thuật toán)

> Audit trước khi sửa (Phase 1, nhóm A). Ngày: 14/08/2026. Chấm theo `standard.md` 10 trục + Đặc trưng tách riêng. Phạm vi: `CheatSheetView.vue` + `CheatSheetTable.vue` (component con của view — dữ liệu chỉ số/độ phức tạp) + key i18n `messages.cheatsheet`.

## Câu trả lời gate (che logo/chữ, nhìn thuần bố cục + màu + animation)

**CÓ, giống bảng tra cứu của bất kỳ SaaS/dashboard nào** — chrome gradient "Cyber Mint" + icon hộp gradient + title gradient-clip + shadow đổ là hero công thức chung chung; bảng trắng 6 cột với badge màu là bảng dữ liệu SaaS bất kỳ. Dấu vết app này chỉ còn: giá trị Big-O mono (`font-family: var(--font-mono)` ở `.cheatsheet__complexity`) + key mono trong EmptyState — chưa đủ mạnh, bị gradient át. → Đặc trưng thấp, PHẢI nâng bằng ngôn ngữ Data Bench (chip Big-O block-token tối + strip mono trong banner).

## Điểm 10 trục (trước sửa)

| # | Trục | Điểm | Lý do |
|---|---|---|---|
| 1 | Spacing/Grid | 7/8 | Đa số `--space-*` chuẩn; vi phạm: `margin-top: 2px` (CheatSheetView.vue:120 — ngoài scale 4/8/12/16...), `gap: 6px` + `margin-top: 4px` (CheatSheetTable.vue:174 — 6px lẻ), `padding: 4px 10px` sim-btn (:183 — 10px lẻ + padding dọc 4px trên nút chữ < 8px) |
| 2 | Breakpoint | 4.5/6 | Table `min-width: 640px` + wrapper `overflow-x: auto` (CheatSheetTable.vue:147,156) → **scroll ngang bảng chính ở 390px** (vi phạm §8: bảng phải chuyển card-stack); chưa đo 3 mốc thực tế |
| 3 | Animation | 5/14 | 0 khoảnh khắc đáng đầu tư (chrome tĩnh); `.input` global `transition: border-color 200ms ease` (global.css:169) — easing mặc định >150ms (KILL-LIST V2); không có gì khác animate |
| 4 | Nhất quán thị giác | 3.5/14 | Gradient mint ×3: `.cheatsheet-view__chrome` `--gradient-mint` (L61), `__icon` (L98), `__title` gradient-clip (L108-113) — KILL-LIST gradient banner/hero; `::after` overlay 68% che gradient (patch contrast, không xử lý gốc — L69-76); chrome + icon `box-shadow: var(--shadow-md)` (L63,104); Badge count `variant="primary"` — accent dùng trang trí (L36); th `text-transform: uppercase` + `letter-spacing: 0.5px` (L161-163 — cấm tracking dương heading); EmptyState chung có (OK), skeleton/error N/A vì dữ liệu tĩnh CATALOG |
| 5 | Interactive sizing | 7/16 | **2 raw `<button>`**: chip lọc `.cheatsheet__chip` (L47) — height ~26px < 40px, không qua buttonVariants, không `aria-pressed`; `.cheatsheet__sim-btn` (L105) — `padding: 4px 10px` (< 8px), font 700; badge đã có min-h-6 (shared, đo 25.6px) |
| 6 | Typography | 5/10 | H1 `clamp(text-2xl → text-3xl)` 30–36px — lệch scale §3 (H1 = 48px) + gradient-clip (L108); `.cheatsheet__name` `font-weight: 700` (L172) + sim-btn 700 (L185) — cấm 700; th uppercase + tracking 0.5px dương (L161-163); complexity mono `text-xs` — chuẩn §3 mono data = 14px (`text-sm`) |
| 7 | Depth & Elevation | 3/8 | Chrome gradient + `shadow-md` (cấm shadow card — shadow chỉ dropdown/modal §6); icon shadow; 0 hero-stat/block-token — bảng dữ liệu chỉ số mà không có 1 khối "sân khấu tối" nào |
| 8 | A11y | 8.5/12 | Breadcrumb nav aria-label OK; sim-btn aria-label OK; th action `aria-label` OK nhưng thiếu `scope="col"`; chips không `aria-pressed`; search input aria-label OK; breadcrumb hiện tại không `aria-current` |
| 9 | Code quality | 5.5/6 | computed lọc OK; `v-for` key `item.key` ổn định OK; không timer; logic lọc trùng SimulationsView (component riêng — chấp nhận, đề xuất composable nếu lặp lần 3) |
| 10 | Performance | 6/6 | Route lazy `() => import(...)` (router:25); không import tĩnh nặng; CheatSheetTable đã lazy theo tab ở SimulationsView |
| | **TỔNG hygiene** | **55/100** | |

## Trục Đặc trưng: **3/10** — giá trị Big-O mono + key mono là dấu vết duy nhất; chrome gradient mint + bảng trắng 6 cột + badge màu đổi lung tung = bảng tra cứu SaaS bất kỳ. KHÔNG đạt ≥7.

## Danh sách lỗi chính (kèm selector/dòng)

1. **Hero công thức + gradient**: `CheatSheetView.vue:55-67` `.cheatsheet-view__chrome` `background-image: var(--gradient-mint)` + `box-shadow: var(--shadow-md)`; `:69-76` `::after` overlay 68% (patch contrast); `:94-105` `__icon` gradient + shadow; `:107-113` `__title` gradient-clip.
2. **Raw `<button>` ×2**: `CheatSheetTable.vue:47-55` `.cheatsheet__chip` (không aria-pressed, cao ~26px); `:105-112` `.cheatsheet__sim-btn` (`padding: 4px 10px`, glyph `▶`, font 700).
3. **Bảng scroll ngang mobile**: `CheatSheetTable.vue:147` `.cheatsheet__table-wrap { overflow-x: auto }` + `:156` `min-width: 640px` — vi phạm §8 (card-stack bắt buộc, cấm scroll ngang bảng chính).
4. **Typography**: `:172` name 700; `:161-163` th uppercase + `letter-spacing: 0.5px`; `CheatSheetView.vue:108` H1 clamp 30–36px + gradient.
5. **Thiếu block-token/hero motif**: dữ liệu Big-O (chỉ số) đang là text mono trần trên bảng trắng — chưa có chip block-token tối `canvas-ink`, chưa có strip mono trong banner (quyết định 4 xuyên-nhóm: nơi có chỉ số → block-token + index mono).
6. **Easing mặc định**: `.input` 200ms `ease` (global.css:169) dùng trong table search.

## Trạng thái: **KHÔNG ĐẠT** (hygiene 55 < 80; đặc trưng 3 < 7). Đã sửa (xem re-audit dưới + fix-log).

---

# RE-AUDIT SAU SỬA — CheatSheetView

## Điểm sau sửa

| # | Trục | Điểm | Ghi chú |
|---|---|---|---|
| 1 | Spacing/Grid | 8/8 | Bỏ 2px/6px/10px lẻ — toàn token scale 4/8/12/16/24/32; internal (chip 8px, td 12/16px) < external (section 24px) |
| 2 | Breakpoint | 6/6 | Đo 3 mốc: 1366 (bảng full 12 cột grid), 768 (bảng khớp, không tràn), 390 (card-stack — thead ẩn, mỗi tr = 1 card, td flex + `data-label`; không scroll ngang) |
| 3 | Animation | 12/14 | Motion chrome 280ms `[0.16,1,0.3,1]` (khoảnh khắc duy nhất — enter); hover row/chip 150ms chuẩn; không easing mặc định >150ms (`.input` global 200ms ease còn trong file shared — ghi notes, ngoài phạm vi view) |
| 4 | Nhất quán | 13.5/14 | Chrome = surface band level-2 `card-raised + border-subtle` (bỏ gradient/shadow/overlay); strip Big-O block-token + index mono trong banner (BIG-O 00–04); chip Big-O tối `canvas-ink` trong bảng; icon lucide Table2 20px muted square; badge count → `muted` (accent không trang trí); th hết uppercase/tracking; EmptyState chung giữ |
| 5 | Interactive sizing | 15.5/16 | 0 raw `<button>` — chips + sim-btn qua Button.vue (outline sm h-9 px-3, gap icon-text 8px, aria-pressed); badge min-h-6; chip Big-O min-h-6 (24px) |
| 6 | Typography | 9.5/10 | H1 48px/600/-0.03em; th `text-sm font-medium text-tertiary h-10` (§4.6); bỏ 700 (name 600); Big-O `font-mono text-sm` (§3 mono data 14px) |
| 7 | Depth | 7.5/8 | Banner level-2; strip Big-O = hero motif duy nhất/màn; bảng level-1 `bg-card border-border` không shadow; chip tối = block-token (vùng dữ liệu luôn tối) |
| 8 | A11y | 11.5/12 | aria-pressed chips; `scope="col"`; `aria-current="page"` breadcrumb; data-label cho td khi card-stack; search aria-label; contrast chip `white/92` trên canvas-ink ≥ 7:1 |
| 9 | Code | 6/6 | computed giữ; key `item.key`; không timer; i18n thay hardcode; không trùng class CSS |
| 10 | Performance | 6/6 | Route lazy sẵn; không import mới |
| | **TỔNG hygiene** | **94/100** | |

## Đặc trưng sau: **9/10** — strip banner `BIG-O 00–04` (block tối data-core + index mono) + bảng đầy chip Big-O block-token `canvas-ink` mono — màn hình chỉ app dạy CTDL này mới có; bỏ hết gradient công thức.

## KẾT LUẬN: **ĐẠT** (hygiene 94 ≥ 80; không trục dưới sàn; đặc trưng 9 ≥ 7).
