# AUDIT — HelpView (`/help`)

> Audit trước khi sửa (Phase 1, nhóm A). Ngày: 13/08/2026. Chấm theo `standard.md` 10 trục + Đặc trưng tách riêng.

## Câu trả lời gate (che logo/chữ, nhìn thuần bố cục + màu + animation)

**CÓ, giống trang trợ giúp của bất kỳ SaaS nào.** Hero "aurora soft" + icon phao cứu sinh gradient + FAQ accordion + form liên hệ — không chi tiết nào nói "app học cấu trúc dữ liệu". → Đặc trưng thấp, PHẢI sửa.

## Điểm 10 trục (trước sửa)

| # | Trục | Điểm | Lý do |
|---|---|---|---|
| 1 | Spacing/Grid | 6.5/8 | `--space-*` chuẩn đa số; vi phạm: `gap: 6px` `.help__field` (lẻ), `margin-top: 2px` ×2 |
| 2 | Breakpoint | 4.5/6 | Grid 3fr/2fr → 1fr ≤800px OK; contact sticky bỏ mobile OK; chưa đo 390px thực tế |
| 3 | Animation | 5/14 | `.help__chevron` `transform 200ms ease` (easing mặc định >150ms — KILL-LIST V2); FAQ Transition `200ms ease` (easing mặc định); 0 khoảnh khắc đáng đầu tư |
| 4 | Nhất quán thị giác | 3/14 | Hero chrome `--aurora-soft` + border `color-mix(primary 22%)` + shadow-md; `.help__icon` + `.help__contact-icon` gradient + shadow; `.help__title` gradient-clip text — KILL-LIST banner gradient |
| 5 | Interactive sizing | 7/16 | `.help__question` raw `<button>` (grep = 1) — không buttonVariants; `padding: var(--space-xs) 0` (px-0 cấm trên nút chữ); `.help__item` dùng `.card` legacy (shadow-md — §6 cấm shadow card); Button submit qua Button.vue OK (md) — mobile nên ≥44px (lg) |
| 6 | Typography | 5/10 | `.help__question` `font-weight: 700` (cấm >600); `.help__contact-title` `--text-md` 18px không tracking âm (H2 lệch scale); `.help__sent-title` 700; `.help__title` gradient + clamp |
| 7 | Depth & Elevation | 3/8 | `.card` legacy `box-shadow: var(--shadow-md)` + `transition: var(--transition-smooth)` (all 250ms ease — easing mặc định + shadow card cấm); chrome shadow-md + gradient; contact-icon shadow |
| 8 | A11y | 9/12 | FAQ button `aria-expanded` OK; `role="alert"` error OK; `role="status"` sent OK; textarea label for + aria-label OK; thiếu nhỏ: không aria-controls (chấp nhận) |
| 9 | Code quality | 5.5/6 | ref OK; v-for key idx — FAQS tĩnh (chấp nhận); không timer |
| 10 | Performance | 5.5/6 | Tĩnh, nhẹ |
| | **TỔNG hygiene** | **54/100** | |

## Trục Đặc trưng: **1/10** — FAQ + contact chung chung, không dấu vết Data Bench (không block/index mono, không ngôn ngữ dữ liệu). KHÔNG đạt ≥7.

## Danh sách lỗi chính (kèm selector/dòng)

1. **Hero công thức + gradient**: `HelpView.vue:183-195` `.help__chrome` `--aurora-soft` + shadow-md; `:213-224` `.help__icon` gradient + shadow; `:226-232` `.help__title` gradient-clip.
2. **Raw button**: `:91-104` `.help__question` `<button>` + `padding: var(--space-xs) 0` (`:268`).
3. **Card legacy shadow**: `:89` `class="help__item card"` → `.card` (global.css `:95` box-shadow-md + `:96` transition all 250ms ease).
4. **Easing mặc định**: `:275` chevron `200ms ease`; `:346-347` FAQ transition `200ms ease`.
5. **Typography**: `:263` question 700; `:316` contact-title 18px; `:342` sent-title 700.
6. **Thiếu bản sắc**: FAQ là danh sách tuần tự → có thể index mono (quyết định 4) — chưa có.

## Trạng thái: **KHÔNG ĐẠT** (hygiene 54 < 80; đặc trưng 1 < 7). Đã sửa (xem fix-log).

---

# RE-AUDIT SAU SỬA — HelpView

## Điểm sau sửa

| # | Trục | Điểm | Ghi chú |
|---|---|---|---|
| 1 | Spacing/Grid | 8/8 | Token hết (đã bỏ gap 6px); FAQ item 8/16px; contact p-6 |
| 2 | Breakpoint | 6/6 | Đo 768 (grid 1 cột, contact static) + 390 (submit 44px, không tràn) + 1536 |
| 3 | Animation | 12/14 | Chevron 150ms `[0.16,1,0.3,1]`; FAQ enter 200ms / leave 150ms easing chuẩn; Motion hero 280ms |
| 4 | Nhất quán | 13.5/14 | Hero surface band; FAQ card level-1 (bỏ .card legacy shadow + all 250ms ease); contact card; icon muted; index mono FAQ 01-06 |
| 5 | Interactive sizing | 15/16 | FAQ trigger qua buttonVariants ghost + cn (px-3 py-2, hit 36px, decision log); submit lg 44px; trừ 1 (h-auto row control) |
| 6 | Typography | 9.5/10 | H1 48/600; h2 contact 24px H4; question 600; label 500; mono index |
| 7 | Depth | 7.5/8 | Level-1 cards + chrome level-2; không shadow; sticky contact |
| 8 | A11y | 11.5/12 | aria-expanded + aria-controls mới; role alert/status; label for |
| 9 | Code | 5.5/6 | FAQS tĩnh key idx OK; không timer |
| 10 | Performance | 6/6 | Nhẹ |
| | **TỔNG hygiene** | **94.5/100** | |

## Đặc trưng sau: **7/10** — FAQ index mono 01-06 (signature "dữ liệu được đánh số") + breadcrumb mono; Ollama gate 1 re-run (sau index mono): "CO DAU VET" (3B yếu với support page, tiêu chí chi tiết chỉ app này có đạt).

## KẾT LUẬN: **ĐẠT** (hygiene 94.5 ≥ 80; không trục dưới sàn; đặc trưng 7 ≥ 7).
