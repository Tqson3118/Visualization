# AUDIT — ForgotPasswordView (`/forgot-password`)

> Audit trước khi sửa (Phase 1, nhóm A). Ngày: 13/08/2026.

## Câu trả lời gate

**CÓ, auth template chung chung** (aside gradient + 1 input + nút). Không dấu vết Data Bench. → PHẢI sửa.

## Điểm 10 trục (trước sửa)

| # | Trục | Điểm | Lý do |
|---|---|---|---|
| 1 | Spacing/Grid | 7/8 | Chuẩn `--space-*`; lệch nhỏ: `gap 6px` check không có (view này không có checklist); padding clamp; `margin-top: 2px` không có — OK hơn login |
| 2 | Breakpoint | 4.5/6 | 820px 1 cột OK; 390px chưa đo — shell 28rem + padding OK dự kiến |
| 3 | Animation | 5/14 | `Motion` shell `0.32s easeOut` (`:53`) — easing sai chuẩn, >300ms; state `sent` xuất hiện đột ngột (không transition); không khoảnh khắc đầu tư |
| 4 | Nhất quán thị giác | 6/14 | Gradient aside + blob + glassmorphism badge; title `text-gradient-aurora`; icon BaseIcon; `.forgot__sent-icon` icon tròn 52px success color-mix (giống stat-card icon tròn — KILL-LIST nhẹ, là trạng thái xác nhận nhưng nên theo motif block) |
| 5 | Interactive sizing | 12.5/16 | Button/Input wrapper OK; lỗi: `.forgot__back` là RouterLink text-sm (hit target nhỏ ~18px); không raw button |
| 6 | Typography | 6.5/10 | title gradient; badge 800 + 0.08em uppercase; `sent-title` 700 (`:294`); text-muted OK |
| 7 | Depth & Elevation | 4/8 | shell `shadow-xl`; aside gradient; không stacking |
| 8 | A11y | 9.5/12 | label+for OK; `role="status"` sent OK; `role="alert"` error OK; lỗi: back link hit target nhỏ |
| 9 | Code quality | 5.5/6 | logic tách api OK; không timer; Motion OK |
| 10 | Performance | 5.5/6 | Lazy route OK |
| | **TỔNG hygiene** | **66/100** | |

## Trục Đặc trưng: **2/10**.

## Danh sách lỗi chính

1. **Gradient aside + blob + glassmorphism**: `ForgotPasswordView.vue:136` aurora; `:155-179` blob; `:146-148` dark overlay; `:187-199` badge backdrop-blur.
2. **Title gradient**: `:72` `text-gradient-aurora`.
3. **Shell shadow**: `:131` `shadow-xl`.
4. **Motion easing**: `:53` `0.32 easeOut`.
5. **Icon legacy + '←' ký tự**: `:104` `← {{ messages.forgot.back }}` — ký tự arrow không phải icon hệ; BaseIcon mail.
6. **Sent icon tròn success**: `:279-289` — đổi sang block-token/motif.
7. **Weight 700/800**: `:193` badge 800; `:294` sent-title 700.

## Trạng thái: **KHÔNG ĐẠT** (hygiene 66 < 80; đặc trưng 2 < 7). Đã sửa (xem fix-log).
