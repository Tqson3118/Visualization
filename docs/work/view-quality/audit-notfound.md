# AUDIT — NotFoundView (`/:pathMatch(.*)*` — 404)

> Audit trước khi sửa (Phase 1, nhóm A). Ngày: 13/08/2026. Chấm theo `standard.md` 10 trục + Đặc trưng tách riêng.

## Câu trả lời gate (che logo/chữ, nhìn thuần bố cục + màu + animation)

**CÓ, giống trang 404 của bất kỳ SaaS nào.** Vòng tròn gradient aurora (tím→xanh) phát sáng với số "404" + icon la bàn + heading gradient text + nút CTA tròn bo — không một chi tiết nào nói "app học cấu trúc dữ liệu". → Đặc trưng thấp, PHẢI sửa.

## Điểm 10 trục (trước sửa)

| # | Trục | Điểm | Lý do |
|---|---|---|---|
| 1 | Spacing/Grid | 7.5/8 | `--space-*` chuẩn, max-width 480px OK; vi phạm nhỏ: `inset: 10px` trên ring trang trí (lẻ, ngoài scale) |
| 2 | Breakpoint | 5/6 | Bố cục trung tâm tự co, clamp số 404 OK; chưa đo thực tế 390px |
| 3 | Animation | 5/14 | Motion `duration 0.4 ease:'easeOut'` (easing mặc định >150ms — KILL-LIST V2); CTA `hover-glow` `180ms ease` (easing mặc định + transform); 0 khoảnh khắc "diễn" có chủ đích |
| 4 | Nhất quán thị giác | 4/14 | Gradient aurora ×2 (ring + heading text) — KILL-LIST; `box-shadow: var(--shadow-xl)` trên vòng tròn; `color: #fff` hex rời; icon Compass 26px lệch quy ước 16–20px; `letter-spacing: 0.04em` dương trên số 404 |
| 5 | Interactive sizing | 6/16 | CTA RouterLink qua class `.btn btn-primary hover-glow` (legacy padding 12/24px, không buttonVariants — trục 5a); `hover-glow` easing mặc định + shadow |
| 6 | Typography | 5/10 | `font-weight: 800` số 404 (cấm >600); `font-size: clamp(3.5rem,10vw,5rem)` + `letter-spacing 0.04em` ngoài scale §3; H1 dùng `--text-2xl` (30px — lệch H1 48px); không tracking âm |
| 7 | Depth & Elevation | 3/8 | Vòng tròn gradient + `shadow-xl` (shadow dày ngoài dropdown/modal — cấm §6); không luminance stacking; không block-token |
| 8 | A11y | 9/12 | aria-hidden decorative OK; H1 duy nhất OK; focus-visible global OK; rủi ro contrast: số 404 trắng trên gradient light KHÔNG có scrim (`.dark` mới có scrim) |
| 9 | Code quality | 5.5/6 | Không ref/timer; comment rõ; không trùng logic |
| 10 | Performance | 5.5/6 | Route eager? nhỏ, không ảnh; Motion-v nhẹ |
| | **TỔNG hygiene** | **55.5/100** | |

## Trục Đặc trưng: **1/10** — vòng tròn gradient + compass + CTA chung chung, hoàn toàn không nhận diện app học CTDL. KHÔNG đạt ≥7.

## Danh sách lỗi chính (kèm selector/dòng)

1. **KILL-LIST hero công thức + gradient**: `NotFoundView.vue:54-65` `.not-found__ring` `background-image: var(--gradient-aurora)` + `box-shadow: var(--shadow-xl)`; `:102-108` `.not-found__heading` gradient-clip text.
2. **Easing mặc định**: `:17` Motion `ease: 'easeOut'` 0.4s; `:27` CTA class `hover-glow` (global.css `180ms ease` + shadow-lg).
3. **Typography**: `:83-84` `font-weight: 800` + `letter-spacing: 0.04em`; `:103` heading `--text-2xl` (30px).
4. **CTA không qua buttonVariants**: `:28` `.btn.btn-primary` legacy.
5. **Hex rời**: `:84` `color: #fff`; `:76` `rgba(255,255,255,0.5)` (dashed ring — bỏ luôn).
6. **Không bản sắc**: không block-token, không index mono, không ngôn ngữ dữ liệu.

## Trạng thái: **KHÔNG ĐẠT** (hygiene 55.5 < 80; đặc trưng 1 < 7). Đã sửa (xem fix-log).

---

# RE-AUDIT SAU SỬA — NotFoundView

## Điểm sau sửa

| # | Trục | Điểm | Ghi chú |
|---|---|---|---|
| 1 | Spacing/Grid | 8/8 | Mọi spacing token; block 48px + gap 8px |
| 2 | Breakpoint | 6/6 | Đo 1536/768/390: không tràn (390: panel 327px, 4 block 48px khít) |
| 3 | Animation | 12/14 | 1 khoảnh khắc: Motion 280ms `[0.16,1,0.3,1]`; bỏ hover-glow ease mặc định; trừ JS không check reduce (global CSS đã cắt CSS anim) |
| 4 | Nhất quán | 13.5/14 | Panel tối canvas-ink + block data-core/resolved/conflict + index mono (motif §1); không gradient/hex rời (chỉ white opacity — pattern đã chốt) |
| 5 | Interactive sizing | 16/16 | CTA qua buttonVariants lg (44px, px-8); `<button` raw = 0; contrast đo 21:1 |
| 6 | Typography | 10/10 | H1 48/600/-0.03em; label mono uppercase hợp lệ; weight ≤600 |
| 7 | Depth | 7.5/8 | Panel canvas-ink elevation duy nhất, không shadow; trừ 0.5 (border white/12 chủ ý trên nền tối) |
| 8 | A11y | 11.5/12 | CTA contrast 21:1 (đo); panel aria-hidden; H1 duy nhất |
| 9 | Code | 6/6 | const BENCH_BLOCKS; v-for key ổn định |
| 10 | Performance | 6/6 | Nhẹ |
| | **TỔNG hygiene** | **96.5/100** | |

## Đặc trưng sau: **9/10** — mảng 4·0·4 + block 03 dashed conflict "out of bounds" + index mono 00-03 kể đúng câu chuyện 404 bằng ngôn ngữ dữ liệu; Ollama gate 1: "RO RANG APP HOC CTDL".

## KẾT LUẬN: **ĐẠT** (hygiene 96.5 ≥ 80; không trục dưới sàn; đặc trưng 9 ≥ 7).
