# AUDIT — HomeView (`/`)

> Audit trước khi sửa (Phase 1, nhóm A). Ngày: 13/08/2026. Chấm theo `standard.md` 10 trục + Đặc trưng tách riêng.

## Câu trả lời gate (che logo/chữ, nhìn thuần bố cục + màu + animation)

**CÓ, giống demo dashboard bất kỳ.** Gradient aurora (tím→xanh) trên hero + logo + stat value + icon squares, blob mờ phía sau, stat-card 3 số to giữa, card 3 cột hover-lift — không một chi tiết nào nói "app học cấu trúc dữ liệu". Chỉ còn dấu vết: chữ mono trong `home__demo-meta` (rất nhỏ). → Đặc trưng thấp, PHẢI sửa.

## Điểm 10 trục (trước sửa)

| # | Trục | Điểm | Lý do |
|---|---|---|---|
| 1 | Spacing/Grid | 7.5/8 | `--space-*` chuẩn; vi phạm nhỏ: gap 6px/4px/2px trong badge/stat (lẻ, ngoài scale 4/8/12…); clamp tùy biến padding hero (nằm trong scale-ish nhưng không phải token thuần) |
| 2 | Breakpoint | 4.5/6 | `minmax(260px,1fr)` grid tự co OK; hero clamp co chữ tới 32px OK; KHÔNG test 390px: nav brand + 2 link có nguy cơ tràn (chưa đo); stats 1 cột dưới 640 OK |
| 3 | Animation | 6/14 | `.home__demo-btn` transition `180ms ease` (easing mặc định >150ms — KILL-LIST V2); hover-lift/hover-glow `180ms ease`; không easing chuẩn; 0 khoảnh khắc đáng đầu tư (mọi thứ fade/hover như nhau); không rõ reduced-motion trong view (global có) |
| 4 | Nhất quán thị giác | 6/14 | Gradient aurora ×5 chỗ (brand, hero, stat value, demo-icon, section-title) — KILL-LIST; accent teal dùng trang trí (kicker, gradient) không interactive; icon emoji? không, nhưng icon squares gradient + 2 bộ gradient khác nhau; `<button>` raw demo-btn không qua Button.vue; badge glassmorphism (backdrop-blur) |
| 5 | Interactive sizing | 10/16 | `home__demo-btn` raw `<button>` padding 0.6rem/1rem (lệch buttonVariants), font 700; 2 CTA hero là RouterLink custom `.btn--light/.btn--outline-light` (không buttonVariants, border 2px); `.btn--sm` padding tự đè 0.5rem; link nav hit target nhỏ (<40px) |
| 6 | Typography | 6.5/10 | `font-weight: 800` brand/stat/heading (cấm >600); `font-weight: 700` kicker/badge/dt (cấm); `letter-spacing: 0.08em/0.1em` dương uppercase (chỉ cho phép label mono ngắn); title dùng clamp ngoài scale §3; không tracking âm cho heading |
| 7 | Depth & Elevation | 4.5/8 | Hero `shadow-lg` + blob; stats card `shadow-md`; demo-icon `shadow-md` — card có shadow (cấm §6); 3 stat đồng hạng, 0 hero-stat; không luminance stacking |
| 8 | A11y | 9/12 | `aria-label` demo-btn OK; focus-visible toàn cục OK; demo-btn keyboard OK; lỗi: icon-only? không; link "Đăng ký" target nhỏ; heading H1 duy nhất OK |
| 9 | Code quality | 5/6 | computed demos/stats OK; `openDemo` OK; không timer; CSS specificity: `.btn--sm` override padding theo class phụ (đè 2 class .btn cùng lúc — dễ triệt tiêu); trùng header nav với AppHeader (logic trùng) |
| 10 | Performance | 5.5/6 | Route eager (HomeView import trực tiếp — trang chủ nên eager, OK); CATALOG tĩnh nhỏ; không ảnh/lazy issue |
| | **TỔNG hygiene** | **64.5/100** | |

## Trục Đặc trưng: **2/10** — nhìn chung chung như mọi landing SaaS (gradient hero + stats + 3 card). Không đạt ≥7.

## Danh sách lỗi chính (kèm selector/dòng)

1. **Header trùng 2 lần (bug thật)**: `App.vue:37` `<AppHeader/>` (brand + Đăng nhập/Đăng ký) + `HomeView.vue:56-64` `.home__nav` (cùng brand + nav) → 2 dòng "DSA Visual" + nav chồng nhau.
2. **Hero công thức + gradient**: `HomeView.vue:68` `bg-aurora-gradient`, `:220-244` blob `::before/::after` blur 48px, `:214` `box-shadow: var(--shadow-lg)`, `:264-266` overlay dark, `:277` `text-shadow`, `:294-312` `.btn--light/.btn--outline-light` custom.
3. **Gradient text**: `:186` `.home__brand` background-clip text; `:337` `.home__stat-value`; `:104` `text-gradient-aurora` section title.
4. **Stat-card formula**: `:315-324` `.home__stats` `box-shadow-md` + `border-radius-xl`; `:326-341` value 800 + gradient.
5. **Icon squares gradient**: `:405-420` `.home__demo-icon` aurora / sunset gradient + shadow.
6. **Raw button**: `:128-137` `.home__demo-btn` `<button>` (không Button.vue), `transition 180ms ease`, hover translateY + shadow.
7. **Font weight/letter-spacing vi phạm §3**: `:184` 800; `:246-260` badge 700 + 0.08em; `:371-377` kicker 700 + 0.1em; `:450` dt 700.
8. **Glassmorphism**: `:259` badge `backdrop-filter: blur(4px)`.
9. **hover-lift/hover-glow**: `:60,76,112,153` dùng class global `180ms ease` + shadow-lg hover (card cấm shadow; easing mặc định).
10. **Thiếu bản sắc**: không block-token, không index mono, không Big-O chip trong hero; chỉ còn mono meta nhỏ trong card demo.

## Trạng thái: **KHÔNG ĐẠT** (hygiene 64.5 < 80; đặc trưng 2 < 7). Ưu tiên CAO — đã sửa (xem fix-log).
