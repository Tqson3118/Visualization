# AUDIT — CodeRunnerView `/code/:key`

> Phase 1 audit TRƯỚC khi sửa (13/08/2026 · dev-engine). Nguồn chấm: `standard.md` mục 2 + 3 + 5 + `frontend/DESIGN.md`. Bằng chứng = dòng code `frontend/src/views/CodeRunnerView.vue` (CR) + screenshot `docs/work/r2-fixed-09-code.png`.

## Câu hỏi đặc trưng — "nhìn thuần bố cục + màu + animation, đoán được đây là app học CTDL không?"

**ĐOÁN ĐƯỢC một phần** — layout editor (trái) + output/canvas (phải) + CanvasArea động = rất đặc trưng (app học thuật toán). Nhưng bị hạ: chrome header gradient-mint + border tint + shadow (dashboard generic), title text-gradient + 💻 emoji, ghi chú dev lộ UI "* Monaco editor sẽ được bật...", editor nền theo theme light (vi phạm quyết định #5 vùng dữ liệu LUÔN tối), panel shadow-sm, nút ◀▶ ký tự thay icon. Điểm đặc trưng: **6/10** (có canvas + mono nhưng chrome + editor chưa đúng sân khấu tối).

## Điểm 10 trục (BEFORE)

| # | Trục | Điểm | Lý do chính |
|---|---|---|---|
| 1 | Spacing/Grid | 7/8 | Token `--space-*` OK; `gap: 6px` (CR 330, 402 — không token), `margin-bottom: 4px` (CR 263, 284 — không token), gutter `padding: 12px 8px 12px 0` (CR 352 — 12px không trong scale 4/8/12? — 12px có trong scale §5 (4/8/12/16...) OK), textarea `padding: 12px 14px` (CR 377 — 14px KHÔNG trong scale) |
| 2 | Breakpoint | 4.5/6 | grid 1fr 1fr → 1fr <1000px OK (CR 471-473); chưa test 390 runtime (editor 2 cột chồng, gutter + textarea vẫn ổn về lý thuyết) |
| 3 | Animation | 10/14 | Không animation cơ giới; `--transition-fast` ease mặc định 150ms (gutter/status); không khoảnh khắc đầu tư; button loading spinner OK |
| 4 | Nhất quán thị giác | 6.5/14 | **Chrome gradient-mint + border tint primary + shadow-md** (CR 239-248) — KILL-LIST; **title text-gradient + 💻 emoji** (CR 91, 276-282) — emoji icon + gradient; ghi chú dev Monaco lộ UI (CR 142-144); panel-title svg `color: var(--color-primary)` trang trí (CR 335); editor light-theme (vi phạm quyết định #5); breadcrumb không mono (CR 258-266) |
| 5 | Interactive sizing | 11/16 | Nút qua Button.vue OK; ◀▶ ký tự text thay icon (CR 203-204) target nhỏ? — size sm h-9 OK nhưng dùng ký tự ◀▶ không phải lucide; "▶ Chạy" ký tự (CR 146); icon History size 15 (CR 97 — lẻ 15, chuẩn 16); panel-title svg size 15 (CR 121, 155) |
| 6 | Typography | 6.5/10 | `font-weight: 700` panel-title (CR 332), empty-title (CR 297); title text-2xl (CR 277) + gradient; sub text-xs (CR 284 — body sm 14px chuẩn, xs chỉ caption); textarea font-size 13px (CR 372 — ngoài scale §3, mono data 14px) |
| 7 | Depth & Elevation | 5/8 | Panel `box-shadow: var(--shadow-sm)` (CR 315) — card cấm shadow; chrome shadow-md; editor-wrap nền theo theme (không canvas-ink); không phân cấp surface band |
| 8 | A11y | 9.5/12 | textarea có aria-label (CR 137 — giữ theo e2e); gutter aria-hidden OK; nút ◀▶ không aria-label (chữ ◀ hiển thị = text, tạm OK nhưng sẽ thành icon → cần aria-label); `--ok/--error/--idle` role status/alert OK; empty-state thiếu retry-action chuẩn EmptyState component (dùng div.card tự chế CR 107-113) |
| 9 | Code quality | 5.5/6 | onMounted load template + sim; onBeforeUnmount stopPlayback OK; gutterLines computed OK; không trùng logic |
| 10 | Performance | 5.5/6 | lazy route; textarea + gutter đơn giản; CanvasArea reuse; OK |

**TỔNG hygiene = 71/100** · **Đặc trưng = 6/10** · **KHÔNG ĐẠT** (hygiene 71 < 80; thị-giác 6.5 < sàn 8.4; depth 5 ≥ 4.8 đạt; animation 10 ≥ 8.4 đạt).

## Lỗi + bằng chứng

### KILL-LIST (phải sửa)
1. **Gradient chrome header**: `background-image: var(--gradient-mint)` + `border: 1px solid color-mix(primary 28%, border)` + `box-shadow: var(--shadow-md)` + `::after` overlay (CR 239-256).
2. **Emoji + text-gradient title**: `💻 {{ meta?.title }} — Code Challenge` (CR 91) + `background-clip: text; color: transparent` (CR 276-282).
3. **Ghi chú dev lộ UI** (r2-fixed-09): "* Monaco editor sẽ được bật khi cài gói monaco-editor (SDD Màn 16 — @monaco-editor/loader đã có)." (CR 142-144) — PHẢI bỏ, thay nội dung hữu ích (phím tắt) theo §9.
4. **Easing mặc định**: `--transition-fast` ease 150ms (CR panel-title).

### 10 trục
- **Spacing**: `gap: 6px` (CR 330, 402), `margin-bottom: 4px` (CR 263, 284), textarea `padding: 12px 14px` (CR 377).
- **Typography**: weight 700 (CR 297, 332); textarea 13px; sub text-xs thay vì sm.
- **Depth**: panel shadow-sm (CR 315); editor light-theme — phải LUÔN tối `canvas-ink` (quyết định #5).
- **A11y**: nút ◀▶ khi đổi sang icon cần aria-label; empty state dùng div tự chế → EmptyState component chung + action.

## Kiểm tra button
- `<button` raw: **0** — sạch (tất cả qua Button.vue).
- Nút "◀"/"▶" bước mô phỏng (CR 203-204): size sm ghost, ký tự ◀▶ → chuyển lucide StepBack/StepForward + aria-label.

## Ghi chú phạm vi
- Giữ nguyên logic/route + textarea aria-label + text "Thành công · Xms" (e2e — comment CR 6).
- BenchmarkPanel vẫn có `<button>` raw chips (benchmark__chip) — thuộc BenchmarkView, ngoài phạm vi task 2-view này (task sau).

---

## RE-AUDIT (AFTER — 13/08/2026, sau khi sửa)

**Điểm sau**: spacing 8 · breakpoint 4.5 · animation 11 · thị-giác 12.5 · interactive 14.5 · typography 8.5 · depth 7 · a11y 11 · code 5.5 · performance 5.5 → **TỔNG 88.5/100** · **Đặc trưng 8.5/10** · **ĐẠT** (hygiene ≥80, không trục dưới sàn, đặc trưng ≥7).

Sửa chính: (1) chrome header gradient+shadow+::after → surface band level-2 (`bg-card-raised` + `border-b border-border-subtle`, không shadow, không gradient); (2) bỏ 💻 + text-gradient → H1 text-3xl text-foreground + kicker mono; (3) **bỏ ghi chú dev Monaco** → thay caption phím tắt mono hữu ích (Ctrl+Enter); (4) editor LUÔN tối `bg-canvas-ink` (gutter + textarea + output box) + text mono sáng qua token/color-mix (không hex rời); (5) panel bỏ shadow-sm → border chuẩn; (6) ◀▶ → lucide StepBack/StepForward + aria-label; (7) "▶ Chạy" → lucide Play; (8) gap 6px→8px (gap-2), margin 4px→space token, padding 14px→16px, weight 700→600, icon size 15→16; (9) empty state → EmptyState component chung (icon database + action); (10) breadcrumb mono.
