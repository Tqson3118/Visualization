# AUDIT — RegisterView (`/register`)

> Audit trước khi sửa (Phase 1, nhóm A). Ngày: 13/08/2026.

## Câu trả lời gate

**CÓ, giống auth đăng ký template bất kỳ** (aside gradient + form + segmented role + checklist). Không chi tiết nào của "phòng thí nghiệm dữ liệu". → PHẢI sửa.

## Điểm 10 trục (trước sửa)

| # | Trục | Điểm | Lý do |
|---|---|---|---|
| 1 | Spacing/Grid | 6.5/8 | Đa số chuẩn `--space-*`; lệch: `gap: 4px 12px` checklist (4px không trong scale? 4px = --space-xs OK; 12px = p-3 OK nhưng là mix gap lẻ), `gap: 6px` check, `margin-bottom: 2px` `:485`, `padding: 4px` role-group |
| 2 | Breakpoint | 4/6 | 820px → 1 cột OK; 390px CHƯA đo: form dài (8 field + teacher) + checklist 2 cột `white-space: nowrap` có thể tràn ngang; shell max 28rem OK |
| 3 | Animation | 5/14 | `Motion` shell: `duration 0.32, ease 'easeOut'` (`:156`) — easing không chuẩn (easeOut ≠ cubic-bezier chuẩn), 320ms > 300ms; không khoảnh khắc đầu tư; role switch không transition chủ đích |
| 4 | Nhất quán thị giác | 5.5/14 | Gradient aurora aside + title + blob — KILL-LIST; badge glassmorphism; `register__teacher` box `color-mix(primary 4%)` nền tô màu trang trí (accent teal dùng không interactive); icon BaseIcon; `<button>` raw role-option |
| 5 | Interactive sizing | 10/16 | 2 `<button class="register__role-option">` raw (`:252-262`) padding 0.5rem — không qua Button.vue (vi phạm grep `<button` raw = 0); checkbox 16px target nhỏ (24px chuẩn WCAG 2.5.8 — native checkbox hit ~16px, chấp nhận native nhưng nên padding); Button submit OK; textarea `.input` legacy |
| 6 | Typography | 6/10 | title gradient; badge 800 + 0.08em; `register__check--ok` 600; `register__pending-title` 700 (`:653`); label legacy `.label` 600 OK; textarea dùng `.input` (text-base) |
| 7 | Depth & Elevation | 4/8 | shell `shadow-xl`; role-option active `shadow-sm` (nút/card cấm shadow); aside gradient; không luminance stacking |
| 8 | A11y | 9/12 | label+for OK; `aria-pressed` role-option OK; error `role="alert"` OK; lỗi: `register__check-mark` dùng ký tự '✓'/'○' (emoji-like glyph, không phải icon hệ) — chấp nhận được cho state nhưng trục icon cấm emoji làm icon chức năng → đổi; checkbox không focus-visible custom (native OK) |
| 9 | Code quality | 5.5/6 | validate() gộp fieldErrors OK; `onBlur` OK; không timer; Motion dùng đúng chỗ; `TEACHER_BIO_MAX` const OK |
| 10 | Performance | 5.5/6 | Lazy route OK; view vừa; không ảnh |
| | **TỔNG hygiene** | **61/100** | |

## Trục Đặc trưng: **2/10** — đăng ký template chung chung.

## Danh sách lỗi chính

1. **Gradient aside + title + blob + glassmorphism**: `RegisterView.vue:368` aside aurora; `:387-411` blob; `:175` title text-gradient; `:419-431` badge backdrop-blur; `:378-380` dark overlay.
2. **Raw button segmented**: `:252-262` `.register__role-option` `<button>` (2 cái) — phải qua Button.vue; active `shadow-sm` `:557`.
3. **Shell shadow**: `:362` `box-shadow: var(--shadow-xl)`.
4. **Motion easing sai**: `:156` `{ duration: 0.32, ease: 'easeOut' }` → `cubic-bezier(0.16,1,0.3,1)` + ≤300ms.
5. **Icon legacy**: BaseIcon (`:163-166` points, `:179` shield) + Input `icon="..."` string.
6. **✓/○ glyph**: `:232` `{{ rule.ok ? '✓' : '○' }}` — thay bằng lucide (Check/Circle).
7. **Teacher box tô màu primary 4%**: `:568` `color-mix(primary 4%)` — accent trang trí thụ động.
8. **text-gradient + weight 700**: `:653` pending-title 700.
9. **Checkbox target 16px**: `:599-605` — thêm padding hit area.

## Trạng thái: **KHÔNG ĐẠT** (hygiene 61 < 80; đặc trưng 2 < 7). Đã sửa (xem fix-log).
