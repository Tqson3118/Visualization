# AUDIT — LabView `/ladder/:nodeId/lab`

> Phase 1 audit TRƯỚC khi sửa (13/08/2026 · dev-engine). Nguồn chấm: `standard.md` mục 2 + 3 + 5 + `frontend/DESIGN.md`. Bằng chứng = dòng code `frontend/src/views/LabView.vue` (LV) + `frontend/src/components/ladder/LabStage.vue` (LS — component con render trực tiếp bởi LabView) + bằng chứng screenshot `docs/work/r2-fixed-07-ladder.png` / `r2-fixed-08-lab.png`.

## Câu hỏi đặc trưng — "nhìn thuần bố cục + màu + animation, đoán được đây là app học CTDL không?"

**KHÔNG chắc** — có dấu vết (breadcrumb "Lab", 3 thẻ Mô tả/Mục tiêu/Hướng dẫn, canvas ô số có index) nhưng bị che lấp bởi: 3 icon tile gradient-mint giống dashboard generic, card hover shadow + lift đồng loạt, canvas nền light-theme (không phải sân khấu tối), emoji 🎉 trong toast/win. Vùng dữ liệu (dãy ô số + index) là đúng signature Data Bench nhưng CHƯA đủ mạnh vì nền không tối. Điểm đặc trưng: **5/10** (có dấu vết nhưng chưa rõ, cần đẩy canvas tối + bỏ gradient/emoji).

## Điểm 10 trục (BEFORE)

| # | Trục | Điểm | Lý do chính |
|---|---|---|---|
| 1 | Spacing/Grid | 7/8 | Token OK (`--space-*`), nhưng `gap: 2px` (LS 219) + `font-size: 10px` (LS 245) ngoài scale; info-grid minmax 220px lệch grid 12 cột chuẩn |
| 2 | Breakpoint | 4.5/6 | flex-wrap OK; không media query riêng; canvas ô 64px × 6 + wrap OK 390px (chưa test runtime thật) |
| 3 | Animation | 9/14 | `transition: box-shadow 180ms ease, transform 180ms ease` (LV 113) = hover shadow+lift (KILL-LIST); `--transition-fast` = `all 150ms ease` (LS 220) ease mặc định; không easing chuẩn enter/exit; không prefers-reduced-motion cục bộ |
| 4 | Nhất quán thị giác | 7/14 | gradient-mint icon tile (LV 131) + gradient title (LS 179) + cell--done gradient (LS 238) — KILL-LIST gradient; 🎉 emoji toast (LS 103) + win (LS 158); breadcrumb không mono (LV 98-103); icon tile `color: var(--color-on-primary)` trên gradient |
| 5 | Interactive sizing | 10/16 | `<button>` raw trong canvas (LS 127 — canvas exception hợp lệ, ghi decision log); cell hover = shadow-md + translateY(-2px) (LS 224-228) — vi phạm hover chuẩn; cell `font-weight: 800` (LS 212) cấm 700+; nút controls qua Button.vue OK |
| 6 | Typography | 6.5/10 | info-title `text-md` (18px — LV 140) không theo scale H4 (text-lg/xl); win `font-weight: 700` (LS 260); cell weight 800; idx 10px < caption 12px (LS 245) |
| 7 | Depth & Elevation | 5/8 | hover card shadow-md (LV 116) — card cấm shadow; cell shadow-sm + hover shadow-md; không phân cấp elevation (không surface band, không hero) |
| 8 | A11y | 8.5/12 | cell button có type/disabled/aria? thiếu aria-pressed khi selected (LS 127-138); win/fail role="status" OK; nút canvas hover không focus ring chuẩn (outline global OK) |
| 9 | Code quality | 5/6 | onMounted không có (không fetch); simKey computed fallback OK; không listener/timer; logic sạch |
| 10 | Performance | 5/6 | lazy route OK; không ảnh; không re-render thừa (state local LabStage) |

**TỔNG hygiene = 67/100** · **Đặc trưng = 5/10** · **KHÔNG ĐẠT** (hygiene < 80, animation 9 < sàn 8.4? — 9 ≥ 8.4 đạt sàn; thị-giác 7 < sàn 8.4 ✗, depth 5 ≥ 4.8 đạt, typography 6.5 ≥ 6 đạt → trục dưới sàn: thị-giác 7 < 8.4; tổng 67 < 80).

## Lỗi + bằng chứng

### KILL-LIST (phải sửa)
1. **Gradient trang trí** — icon tile `background-image: var(--gradient-mint)` (LV 131), `lab-stage__title` text-gradient (LS 177-183), `lab-stage__cell--done` gradient-mint (LS 236-241).
2. **Emoji icon chức năng** — `ui.showToast('🎉 Chúc mừng qua Bậc 2!')` (LS 103), win message `🎉 Đã hoàn thành...` (LS 158).
3. **Card hover shadow + lift** — `.lab-view__info-card:hover { box-shadow: var(--shadow-md); transform: translateY(-2px) }` (LV 115-118); `.lab-stage__cell:hover` shadow-md + translateY (LS 224-228).
4. **Easing mặc định** — `--transition-fast: all 150ms ease` dùng ở cell (LS 220).

### 10 trục
- **Spacing**: `gap: 2px` (LS 219 — không token), `font-size: 10px` (LS 245 — caption min 12px).
- **Breakpoint**: chưa test 390 runtime.
- **Thị giác**: breadcrumb không mono (LV 98-103), font-size text-sm.
- **Interactive**: cell weight 800 (LS 212); không aria-pressed cho ô selected (LS 127).
- **Typography**: info-title text-md ngoài scale H4 (LV 140); win weight 700 (LS 260).
- **Depth**: không surface band; mọi card nổi bằng shadow.

## Kiểm tra button
- `<button` raw: **1** — LabStage cell (LS 127): canvas/editor exception — đúng quy tắc trục 5 "trừ canvas/editor/table-cell đặc biệt → ghi decision log". Cần decision log + bổ sung aria-pressed.
- Các nút khác (Hoàn tác/Làm lại/Xem lý thuyết/Nộp bài + Về Ladder) qua Button.vue OK.

## Ghi chú phạm vi
- "icon bậc thang vỡ glyph" (r2-fixed-07) = `🪜 Practice Ladder` ở **LadderView.vue:85** (view KHÁC, ngoài phạm vi task 2-view này) — ghi notes Phase 2 cho task LadderView. "icon mảnh ghép + vòng tròn xám" = EmptyState cũ dùng icon puzzle trong QuizStage.vue:157 (Bậc 1, ngoài phạm vi LabView) — EmptyState.vue đã redesign sẵn; LabStage hiện CHƯA có empty state → bổ sung dùng component chung.
- LabStage.vue là component con của LabView (render trực tiếp với props) — sửa tối thiểu theo chuẩn, ghi decision log (tiền lệ ControlBar/StatsBar task trước).

---

## RE-AUDIT (AFTER — 13/08/2026, sau khi sửa)

**Điểm sau**: spacing 7.5 · breakpoint 4.5 · animation 11.5 · thị-giác 12.5 · interactive 13.5 · typography 8.5 · depth 7 · a11y 10.5 · code 5 · performance 5.5 → **TỔNG 86/100** · **Đặc trưng 8/10** · **ĐẠT** (hygiene ≥80, không trục dưới sàn: thị-giác 12.5 ≥ 8.4, interactive 13.5 ≥ 9.6, tất cả ≥ sàn; đặc trưng 8 ≥ 7).

Sửa chính: (1) info cards bỏ gradient tile + shadow hover → bg-muted + border-color hover; (2) canvas LabStage → nền tối `canvas-ink` (vùng dữ liệu LUÔN tối) + cell block data-core + index mono 12px; (3) bỏ 🎉 toast/win; (4) weight 800/700 → 600; (5) "← Về Ladder" → lucide ArrowLeft; (6) breadcrumb mono; (7) easing chuẩn + aria-pressed cell; (8) EmptyState chung cho LabStage khi không có dữ liệu (motif [ ] + copy §9). Ghi nhận: 🪜 LadderView.vue:85 ngoài phạm vi — task LadderView.
