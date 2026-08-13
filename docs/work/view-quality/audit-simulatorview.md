# AUDIT — SimulatorView `/simulator/:key`

> Phase 1 audit TRƯỚC khi sửa (13/08/2026 · dev-engine). Nguồn chấm: `standard.md` mục 2 + 3 + 5. Bằng chứng = dòng code `frontend/src/views/SimulatorView.vue` (gọi tắt SV) + `frontend/src/components/simulator/ControlBar.vue` (CB) + `StatsBar.vue` (SB) + `docs/work/r2-fixed-04-simulator*.png`.

## Câu hỏi đặc trưng — "Đoán được đây là app học CTDL hay dashboard bất kỳ?"

**ĐOÁN ĐƯỢC (một phần)** — vùng canvas tối với block màu data-core/resolved/conflict + index mono là dấu vân tay "Data Bench" đúng bản sắc (DESIGN-IDENTITY §1.1). Nhưng khung ngoài (chrome gradient mint + đốm sáng blur + text-gradient title + pill "So sánh/Hoán đổi" gradient) kéo ngược về "SaaS colorful dashboard". Nếu che canvas → phần còn lại chung chung. Điểm đặc trưng: **6/10** (canvas cứu điểm, chrome hại điểm).

## Điểm 10 trục (BEFORE)

| # | Trục | /8 | /6 | /14 | /14 | /16 | /10 | /8 | /12 | /6 | /6 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| | SimulatorView | 5.5 | 4.5 | 9 | 4 | 8 | 6 | 5 | 9.5 | 5 | 5 |

**TỔNG hygiene = 61.5/100** · **Đặc trưng = 6/10** · **KHÔNG ĐẠT** (hygiene < 80; thị-giác 4 < sàn 8.4; interactive 8 < sàn 9.6).

## Lỗi + bằng chứng

### KILL-LIST (phải sửa)
- **Gradient chrome**: `.simulator__chrome` `background-image: var(--gradient-mint)` + `::before` blob `filter: blur(56px)` (SV 365–397) — KILL-LIST mục 5 (gradient + blob), DESIGN.md §1 banner = surface band level-2.
- **Text gradient**: `.simulator__title` `background-image: var(--gradient-mint)` + `background-clip: text` (SV 417–423) — KILL-LIST (màu trang trí vô nghĩa), chữ phải `text-foreground`.
- **Gradient chip dữ liệu**: ControlBar `.control-bar__indicator` `background-image: var(--gradient-mint)` (CB 149–158); StatsBar `.stats__item--step` `background-image: var(--gradient-mint)` (SB 79–83) — gradient trên chip "đo được" (bộ đếm bước) — phải mono + primary solid.
- **Shadow trên card**: chrome `box-shadow: var(--shadow-md)` (SV 373); canvas-wrap `box-shadow: var(--shadow-sm)` (SV 495); ControlBar `box-shadow: var(--shadow-sm)` (CB 128) + `.control-bar__play` (CB 146); StatsBar không shadow nhưng chip step nền gradient.
- **Easing/hover**: `.simulator__icon-btn` `transition: var(--transition-fast)` = `all 150ms ease` (SV 445) — 150ms ease ở biên giới, chấp nhận nhưng hover `translateY(-1px)` (SV 451) không cần thiết; `.simulator__toggle` không có transition khai báo.

### 10 trục
1. **Spacing (5.5/8)**: `.simulator__canvas-meta` `padding: 6px 10px 2px` (SV 502) — 6px/10px/2px không thuộc scale §5; `gap: 6px` (SV 504, CB 163); breadcrumb `margin-bottom: 4px` (SV 412) = token OK; còn lại token đúng.
2. **Breakpoint (4.5/6)**: grid 3/6/3 → 1fr @1024 (SV 551–554) OK; 768/390 chưa test thực; toolbar icon-btn 36px < 40px chuẩn mobile.
3. **Animation (9/14)**: canvas thật có animation engine (ngoài scope); UI ngoài canvas không có khoảnh khắc đầu tư; icon-btn hover translateY; không reduced-motion cục bộ (engine canvas có cơ chế riêng).
4. **Thị giác (4/14)**: gradient chrome + text-gradient + 2 gradient chip + 4 nút raw (SV 194, 203, 322, 325); `#fff` hex rời (SV 484); `color-mix(... black 8%)` không token; font-weight 700 ở bp-badge (SV 472), toggle (SV 527); subtitle `text-muted` (SV 428) nên tier-2.
5. **Interactive (8/16)**: 4 `<button>` raw — 2 icon-btn 36×36 (SV 194–205, thiếu 40px chuẩn icon h-10 w-10) + 2 toggle text `padding: 4px 8px` = px-2 (SV 530 — cấm px-0/1/2 trên nút chữ, §4.1); nút liền kề gap 8px OK.
6. **Typography (6/10)**: title text-2xl (SV 418) — H1 nên text-3xl; 2 chỗ weight 700 (SV 472, 527); breadcrumb không mono (nên mono theo §5 "breadcrumb mono trong luồng học"); footer không mono (phím tắt = thứ "đo được").
7. **Depth (5/8)**: chrome shadow-md + canvas-wrap shadow-sm (vi phạm §6); bp-badge shadow-sm (badge trạng thái — nhẹ, vẫn vi phạm "shadow chỉ dropdown/modal"); popover InputModal là modal — shadow hợp lệ.
8. **A11y (9.5/12)**: icon-btn có aria-label tốt; breakpoint badge role="status"; phím tắt đầy đủ + không nuốt input; toggles có text; select tốc độ có aria-label.
9. **Code (5/6)**: onKeydown gắn onMounted + gỡ onBeforeUnmount đúng; watch key reset; không trùng logic.
10. **Performance (5/6)**: lazy route; engine chunk 477.94 kB chia sẻ chung (không phải lỗi view); SimulatorView chunk 27.94 kB OK.

## Kiểm tra button
- `<button` raw: **4** (SV 194 favorite, SV 203 share, SV 322 call stack, SV 325 legend) — chưa ghi decision log → vi phạm.
- Icon-btn 36×36: target ≥24×24 OK nhưng < 40px chuẩn icon button (§4.1) và `transition` ease.
- Toggle "Call stack/Legend": text button `padding 4px 8px` (px-2) — cấm trên nút chữ.

---

## RE-AUDIT (AFTER — 13/08/2026, sau khi sửa)

**Điểm sau**: spacing 7 · breakpoint 4.5 · animation 11.5 · thị-giác 11.5 · interactive 12.5 · typography 8.5 · depth 6.5 · a11y 10.5 · code 5.5 · performance 6 → **TỔNG 84.5/100** · **Đặc trưng 8.5/10** · **ĐẠT**.

Sửa chính: (1) chrome bỏ gradient-mint + blob + text-gradient → surface band level-2; (2) 4 raw button → Button.vue (icon 40x40 + aria-pressed; ghost sm + Chevron); (3) khung canvas = bg-canvas-ink + meta mono (motif tối lan tỏa); (4) ControlBar/StatsBar bỏ gradient chip → primary solid + mono; bỏ shadow-sm; (5) states bỏ .card shadow → simulator__panel; (6) footer mono. Giữ NGUYÊN engine/CanvasArea. Ghi nhận Phase 2: PseudocodePanel/LegendPanel/ExplainPanel/ManualPracticePanel/DemoBanner còn gradient/hex/700.