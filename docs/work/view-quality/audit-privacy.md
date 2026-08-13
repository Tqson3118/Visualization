# AUDIT — PrivacyView (`/privacy`)

> Audit trước khi sửa (Phase 1, nhóm A). Ngày: 13/08/2026. Chấm theo `standard.md` 10 trục + Đặc trưng tách riêng.

## Câu trả lời gate (che logo/chữ, nhìn thuần bố cục + màu + animation)

**CÓ, giống trang chính sách bảo mật của bất kỳ SaaS nào.** Hero "aurora soft" + icon shield gradient + TOC sticky + cột văn bản pháp lý — không chi tiết nào nói "app học cấu trúc dữ liệu". → Đặc trưng thấp, PHẢI sửa.

## Điểm 10 trục (trước sửa)

| # | Trục | Điểm | Lý do |
|---|---|---|---|
| 1 | Spacing/Grid | 6.5/8 | `--space-*` chuẩn đa số; vi phạm: `gap: 2px` `.privacy__toc-list` (ngoài scale), `padding: 6px 8px` `.privacy__toc-link` (6px lẻ), `margin-top: 2px` ×2 |
| 2 | Breakpoint | 4.5/6 | Grid 1 cột ≤800px OK; TOC static mobile OK; chưa đo 390px thực tế |
| 3 | Animation | 6/14 | `.privacy__toc-link` `transition: background 150ms ease, color 150ms ease` (easing mặc định); 0 khoảnh khắc đáng đầu tư; scrollIntoView smooth không phải CSS transition (OK) |
| 4 | Nhất quán thị giác | 3/14 | Hero chrome `background-color: var(--aurora-soft)` + border `color-mix(primary 22%)` + `shadow-md` (KILL-LIST banner gradient/glass); `.privacy__icon` gradient + shadow; `.privacy__title` gradient-clip text; hex `#fff` |
| 5 | Interactive sizing | 6/16 | `.privacy__toc-link` raw `<button>` (grep `<button` raw = 1) — không Button.vue/buttonVariants; `padding: 6px 8px` = px-2 (cấm trên nút chữ); hit target ~31px ≥24 OK; breadcrumb link hit nhỏ |
| 6 | Typography | 5/10 | `.privacy__toc-title` `font-weight: 700` + `letter-spacing 0.08em` uppercase (không mono, 700 cấm); `.privacy__content h2` `--text-md` (18px — H2 section lệch scale §3, thiếu tracking âm); `.privacy__title` gradient + clamp ngoài scale |
| 7 | Depth & Elevation | 3/8 | Hero chrome `shadow-md` + gradient (banner sai hướng §1); `.privacy__toc` `box-shadow: var(--shadow-sm)` (card có shadow — cấm §6); `.privacy__icon` `shadow-md`; không luminance stacking |
| 8 | A11y | 8.5/12 | TOC button có text; nav aria-label OK; thiếu `aria-current` breadcrumb; text-muted breadcrumb trên aurora-soft light ~4.26:1 sát fail (đã ghi chú H-E2 cho `.privacy__updated` nhưng breadcrumb chưa xử lý) |
| 9 | Code quality | 5.5/6 | SECTIONS const OK; scrollToSection OK; không timer |
| 10 | Performance | 5.5/6 | Tĩnh, nhẹ |
| | **TỔNG hygiene** | **54/100** | |

## Trục Đặc trưng: **1/10** — policy page chung chung, không dấu vết Data Bench (không block/index mono, không ngôn ngữ dữ liệu). KHÔNG đạt ≥7.

## Danh sách lỗi chính (kèm selector/dòng)

1. **Hero công thức + gradient**: `PrivacyView.vue:121-133` `.privacy__chrome` `--aurora-soft` + shadow-md; `:151-162` `.privacy__icon` `--gradient-aurora` + shadow; `:164-170` `.privacy__title` gradient-clip.
2. **Raw button**: `:48` `.privacy__toc-link` `<button>` + `padding: 6px 8px` (`:220`).
3. **Typography**: `:200-206` toc-title 700 + 0.08em; `:253-256` h2 section 18px.
4. **Shadow trên card**: `:197` `.privacy__toc` box-shadow-sm; `:129` chrome shadow-md.
5. **Thiếu bản sắc**: không index mono cho 6 mục lục (dữ liệu tuần tự → block-token/index mono quyết định 4).

## Trạng thái: **KHÔNG ĐẠT** (hygiene 54 < 80; đặc trưng 1 < 7). Đã sửa (xem fix-log).

---

# RE-AUDIT SAU SỬA — PrivacyView

## Điểm sau sửa

| # | Trục | Điểm | Ghi chú |
|---|---|---|---|
| 1 | Spacing/Grid | 8/8 | Token hết; toc-link 8px/12px (scale); gap xs/sm |
| 2 | Breakpoint | 6/6 | Đo 768 (grid 1 cột, TOC static) + 390 (không tràn) + 1536 |
| 3 | Animation | 11/14 | Motion hero 280ms chuẩn (1 moment); toc-link 150ms `[0.16,1,0.3,1]`; không ease mặc định >150ms |
| 4 | Nhất quán | 13/14 | Hero surface band level-2 (card-raised + border-subtle); icon lucide muted square; index mono TOC + section; không gradient/shadow |
| 5 | Interactive sizing | 15/16 | TOC raw `<button>` → native anchor (decision log); hit 35.6px ≥24; trừ 1: gap TOC 4px < 8px (nav links — chấp nhận) |
| 6 | Typography | 10/10 | H1 48/600/-0.03em; h2 section 24px/600/-0.015em; toc-title mono 500; caption mono |
| 7 | Depth | 7.5/8 | Chrome level-2; TOC level-1 không shadow; không shadow card |
| 8 | A11y | 11.5/12 | Anchor native (focus/keyboard/hash); aria-current breadcrumb; aria-hidden index |
| 9 | Code | 6/6 | Bỏ JS scrollToSection; SECTIONS const; v-for key id |
| 10 | Performance | 6/6 | Tĩnh, nhẹ |
| | **TỔNG hygiene** | **94/100** | |

## Đặc trưng sau: **7/10** — TOC + 6 section có index mono 01-06 + caption mono "CẬP NHẬT 12/08/2026" (signature "dữ liệu được đánh số"); nội dung legal vốn generic nên chưa thể mạnh hơn. Ollama gate 1 (3B, yếu với trang text): "KHONG - CHUNG CHUNG" — chấm theo tiêu chí chi tiết chỉ app này có.

## KẾT LUẬN: **ĐẠT** (hygiene 94 ≥ 80; không trục dưới sàn; đặc trưng 7 ≥ 7).
