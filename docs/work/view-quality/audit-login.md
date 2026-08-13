# AUDIT — LoginView (`/login`)

> Audit trước khi sửa (Phase 1, nhóm A). Ngày: 13/08/2026. Chấm theo `standard.md`.

## Câu trả lời gate (che logo/chữ, nhìn thuần bố cục + màu + animation)

**CÓ, giống split-auth của bất kỳ SaaS nào.** Nửa trái gradient aurora + 3 bullet icon, nửa phải form trắng — thấy là auth page chung chung, không nhận diện được "học thuật toán trực quan". → Đặc trưng thấp, PHẢI sửa.

## Điểm 10 trục (trước sửa)

| # | Trục | Điểm | Lý do |
|---|---|---|---|
| 1 | Spacing/Grid | 7/8 | `--space-*` đa số chuẩn; lệch: gap 6px icon-text badge, `margin-top: 2px` (`:269`), clamp tùy biến; grid 1.05fr/1fr OK |
| 2 | Breakpoint | 4.5/6 | 820px → 1 cột OK; chưa đo 390px: aside gradient + form padding `clamp(1.5rem,4vw,2.5rem)` co OK; nút submit block ≥ 40px OK |
| 3 | Animation | 5/14 | Không có khoảnh khắc đáng đầu tư (0 animation chủ đích); spinner button default; page transition global `ease` (KILL-LIST V2); transition trong `.card` legacy `--transition-smooth 250ms ease` |
| 4 | Nhất quán thị giác | 6/14 | Gradient aurora ×2 (aside `:143`, title `:73` text-gradient) + blob `::before/::after` blur 52px — KILL-LIST; badge glassmorphism `backdrop-blur` `:205`; icon BaseIcon (bộ icon legacy, không lucide); shell `box-shadow-xl` trên card (cấm) |
| 5 | Interactive sizing | 12/16 | Button wrapper `ui/Button.vue` OK (h-10); Input wrapper OK (h-10, pl-9 icon); lỗi: link "Quên mật khẩu?" text-xs (hit target ~14px < 24px); link switch text-sm OK; `.login__forgot` font 600 text-xs |
| 6 | Typography | 6.5/10 | `.login__title` text-3xl (36px OK) nhưng gradient text; aside badge 800 + 0.08em; point 600; `text-muted` subtitle OK; không tracking âm heading; weight 800 badge |
| 7 | Depth & Elevation | 4/8 | `login__shell` `shadow-xl` (card cấm shadow); aside gradient = "nổi" bằng màu trang trí; không luminance stacking; brand points không phân cấp |
| 8 | A11y | 9.5/12 | label + for OK (Input wrapper); `role="alert"` error OK; aria-label aside OK; lỗi: forgot link target nhỏ; `novalidate` + JS validate OK; icon trong input `pointer-events-none` OK |
| 9 | Code quality | 5.5/6 | logic tách store OK; BRAND_POINTS const OK; không timer; không trùng logic; CSS specificity OK (class rõ) |
| 10 | Performance | 5.5/6 | Lazy route OK (router), view nhỏ, không ảnh |
| | **TỔNG hygiene** | **65.5/100** | |

## Trục Đặc trưng: **2/10** — split auth gradient chuẩn template, không chi tiết nào của Data Bench.

## Danh sách lỗi chính

1. **Aside gradient + blob + glassmorphism** (KILL-LIST): `LoginView.vue:143` `background-image: var(--gradient-aurora)`; `:162-186` ::before/::after blur 52px; `:153-154` dark overlay; `:205` badge `backdrop-filter`.
2. **Card container shadow**: `:136` `box-shadow: var(--shadow-xl)` trên `.login__shell` (cấm §6).
3. **Gradient text title**: `:73` `.login__title.text-gradient-aurora`.
4. **Icon legacy BaseIcon**: `:12` import BaseIcon; dùng ở `:63` point icon + truyền `icon="mail"/"lock"` vào Input (`:80,:89`) — không phải lucide-vue-next (quyết định xuyên-nhóm 2).
5. **Hit target nhỏ**: `:99-102` `.login__forgot` text-xs → ~14px chiều cao chạm.
6. **Weight 800/600 + letter-spacing dương**: `:199-206` badge 800 + 0.08em uppercase; `:227-231` point 600.
7. **Thiếu bản sắc**: không block-token/index mono/Big-O; 3 bullet icon chung chung (sparkles/target/check-circle).

## Trạng thái: **KHÔNG ĐẠT** (hygiene 65.5 < 80; đặc trưng 2 < 7). Đã sửa (xem fix-log).
