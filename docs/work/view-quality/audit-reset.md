# AUDIT — ResetPasswordView (`/reset-password`)

> Audit trước khi sửa (Phase 1, nhóm A). Ngày: 13/08/2026.

## Câu trả lời gate

**CÓ, auth template chung chung** (aside gradient + form + checklist). Không dấu vết Data Bench. → PHẢI sửa.

## Điểm 10 trục (trước sửa)

| # | Trục | Điểm | Lý do |
|---|---|---|---|
| 1 | Spacing/Grid | 6.5/8 | Chuẩn chủ yếu; lệch: `gap: 4px 12px` checklist (`:301`), `gap 6px` check, `margin-bottom: var(--space-xs)` OK |
| 2 | Breakpoint | 4/6 | 820px 1 cột OK; 390px: checklist 2 cột `white-space: nowrap` nguy cơ tràn — chưa đo |
| 3 | Animation | 5/14 | Motion `0.32 easeOut` (`:74`); success state đột ngột; không khoảnh khắc đầu tư; `setTimeout` redirect 2000ms không liên quan visual |
| 4 | Nhất quán thị giác | 6/14 | Gradient aside + blob + glassmorphism badge; title gradient; BaseIcon; success icon tròn 52px (KILL-LIST icon tròn); ✓/○ glyph checklist |
| 5 | Interactive sizing | 12.5/16 | Button/Input wrapper OK; back link hit target nhỏ; không raw button |
| 6 | Typography | 6/10 | title gradient; badge 800 + 0.08em; `success-title` 700; checklist OK 400 |
| 7 | Depth & Elevation | 4/8 | shell `shadow-xl`; aside gradient; không stacking |
| 8 | A11y | 9.5/12 | label+for OK; `role="status"` OK; `role="alert"` OK; lỗi: back link hit target; token missing error OK |
| 9 | Code quality | 5/6 | `v-for` key `idx` trong checklist (`:117`) — list tĩnh không sắp xếp lại → chấp nhận được nhưng nên key ổn định; setTimeout redirect không cleanup (nếu unmount trước 2s → redirect vẫn chạy — timer không gỡ) |
| 10 | Performance | 5.5/6 | Lazy route OK |
| | **TỔNG hygiene** | **64/100** | |

## Trục Đặc trưng: **2/10**.

## Danh sách lỗi chính

1. **Gradient aside + blob + glassmorphism**: `ResetPasswordView.vue:173` aurora; `:192-216` blob; `:183-185` dark overlay; `:224-236` badge backdrop-blur.
2. **Title gradient**: `:93` text-gradient-aurora.
3. **Shell shadow**: `:168` shadow-xl.
4. **Motion easing**: `:74` `0.32 easeOut`.
5. **Icon legacy + '←' + ✓/○**: `:122` glyph; `:140` '←'; BaseIcon key/lock/check-circle.
6. **Success icon tròn**: `:354-364`.
7. **Timer không cleanup**: `:53` `setTimeout(...)` redirect 2000ms không clear khi unmount (vi phạm trục 9 — listener/timer gắn phải gỡ).
8. **Weight 700/800**: badge 800; success-title 700.

## Trạng thái: **KHÔNG ĐẠT** (hygiene 64 < 80; đặc trưng 2 < 7). Đã sửa (xem fix-log).
