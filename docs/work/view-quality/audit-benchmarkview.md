# AUDIT — BenchmarkView `/benchmark/:k1/:k2`

> Phase 1 audit TRƯỚC khi sửa (13/08/2026 · dev-engine · task P1-B3). Nguồn chấm: `standard.md` mục 2 + 3 + 5 + `frontend/DESIGN.md`. Bằng chứng = dòng code `frontend/src/views/BenchmarkView.vue` (BV) + `frontend/src/components/benchmark/BenchmarkPanel.vue` (BP — nội dung duy nhất của view, decision log mục 1).

## Câu hỏi đặc trưng — "nhìn thuần bố cục + màu + animation, đoán được đây là app học CTDL không?"

**KHÔNG rõ** — chrome header gradient-mint + shadow + title text-gradient + icon tile gradient (BV 78-131) là công thức dashboard generic; bảng số liệu n/size × ms/comparisons + chart ECharts có chất "đo thực tế" nhưng nền sáng + font không mono → chưa thành ngôn ngữ Data Bench. Emoji ⚖ (BP 281), ▶ (BP 312), empty state icon cây kéo ✂ (BP 322 — không liên quan benchmark). Điểm đặc trưng: **5/10** (ý tưởng đo thật độc đáo nhưng vẻ ngoài chưa tách khỏi dashboard SaaS).

## Điểm 10 trục (BEFORE)

| # | Trục | Điểm | Lý do chính |
|---|---|---|---|
| 1 | Spacing/Grid | 7/8 | Token `--space-*` OK; `gap: 6px` (BP 422 — ngoài scale 4/8/12), select padding `4px 8px` (BP 423 — đúng scale nhưng < 40px hit target, xem trục 5) |
| 2 | Breakpoint | 4.5/6 | Table `min-width: 520px` + `overflow-x: auto` (BP 430-432) — **scroll ngang bảng chính, vi phạm §8 mobile** (390px chưa test runtime); chart 320px height OK; chips wrap OK |
| 3 | Animation | 9.5/14 | Chip `transition: var(--transition-fast)` = `all 150ms ease` + `transform: translateY(-1px)` hover (BP 408-411) — easing mặc định (KILL-LIST V2) + `all` animate không cần thiết; ECharts animation 400ms (BP 226 — chấp nhận, có `prefersReducedMotion` check BP 194-200); không khoảnh khắc đầu tư nào được "diễn" |
| 4 | Nhất quán thị giác | 5/14 | **Chrome gradient-mint + border tint primary + shadow-md + ::after overlay** (BV 74-94) — KILL-LIST; **icon tile gradient-mint + shadow** (BV 112-123); **title text-gradient** (BV 125-131); badge copy "Miễn phí tim" + "(20.4)" số SDD lộ UI (BV 50, BP 282 — không có trong i18n, hardcode); **emoji ⚖** (BP 281) + **▶** (BP 312) — KILL-LIST emoji/char; **empty state icon "scissors" ✂** (BP 322 — cây kéo không liên quan benchmark); raw `<button>` chip + raw `<select>` (BP 289-305 — không qua component chuẩn); breadcrumb không mono (BV 35-39); bảng/chart nền sáng = vùng dữ liệu không tối (quyết định #5); LINE_PALETTE hex rời (BP 185) |
| 5 | Interactive sizing | 9.5/16 | **Raw `<button>` chips** (BP 289-299) — vi phạm 5a (grep `<button` raw ≠ 0); chip height ~26px (padding 4px 12px + text-xs) dưới 40px nhưng ≥ 24px WCAG; **raw `<select>`** (BP 305, padding 4px 8px → ~30px < 40px min-height §4.4); ▶ là ký tự text trong Button (BP 312); export ghost OK; gap chips 8px OK |
| 6 | Typography | 6/10 | **`font-weight: 800`** trên `.benchmark__n` (BP 446) — CẤM 700+ (DESIGN §3); panel title "⚖ Benchmark Lab" text-2xl + text-gradient (BP 385-391) — trùng H1 view + gradient; chip font-xs + weight 600 (BP 404-405 — nút chuẩn text-sm 500); select font mặc định trình duyệt ~13px ngoài scale |
| 7 | Depth & Elevation | 4.5/8 | **Chrome shadow-md** (BV 82) + **icon tile shadow-md** (BV 122) — card cấm shadow (§6); **chip--on shadow-sm** (BP 417) + gradient nổi lung tung; không phân cấp level-1/level-2; title gradient = mọi thứ đều "nổi" |
| 8 | A11y | 8.5/12 | Chip toggle **không aria-pressed** (BP 289-299 — trạng thái chọn không lộ cho SR); select có implicit label bao quanh OK (BP 303); progress role="status" OK (BP 316); empty state không CTA (chấp nhận — controls phía trên); chart aria-label OK (BP 363-364) |
| 9 | Code quality | 5/6 | ref/computed đúng chỗ; `run()` dùng `await new Promise(setTimeout 0)` nhường UI OK; v-for key ổn định (key/size); không listener cần gỡ; `buildResults` đúng contract BE |
| 10 | Performance | 5.5/6 | Lazy route (router index 24); ECharts tree-shake (BP 28-29); chart recompute theo theme hợp lý; worker đo (ADR-012) không chặn UI |

**TỔNG hygiene = 65/100** · **Đặc trưng = 5/10** · **KHÔNG ĐẠT** (hygiene 65 < 80; thị-giác 5 < sàn 8.4; depth 4.5 < sàn 4.8; interactive 9.5 ≥ 9.6? — 9.5 < 9.6 → dưới sàn, thêm nữa).

## Lỗi + bằng chứng

### KILL-LIST (phải sửa)
1. **Gradient chrome**: `background-image: var(--gradient-mint)` + border tint + `box-shadow: var(--shadow-md)` + `::after` overlay (BV 74-94).
2. **Text-gradient title** (BV 125-131) + icon tile gradient (BV 112-123).
3. **Emoji/char icon**: ⚖ (BP 281), ▶ (BP 312) — thay lucide `Scale`/`Play`.
4. **Easing mặc định**: `--transition-fast` (ease 150ms) trên chip hover translateY (BP 408-411).
5. **Microcopy lộ số tài liệu**: "Miễn phí tim (20.4)" (BP 282) — "(20.4)" là số mục SDD, người học không hiểu; chuỗi không nằm trong i18n (`src/i18n/vi.ts` grep "Miễn phí tim" = 0). → "Không tốn tim" (decision log mục 2).
6. **Empty state icon "scissors"** (BP 322) — cây kéo vô nghĩa với benchmark → "hourglass" + copy §9.

### 10 trục
- Spacing: `gap: 6px` (BP 422). Interactive: raw button chips + raw select < 40px. Typography: weight 800 (BP 446). Depth: shadow trên chrome/icon/chip. A11y: chip thiếu aria-pressed. Thị-giác: breadcrumb không mono; bảng/chart nền sáng (vùng dữ liệu phải tối); hex LINE_PALETTE (BP 185).

## Kiểm tra button
- `<button` raw: **2 chỗ** — chip chọn thuật toán (BP 289-299) + select là `<select>` raw (BP 305). Cả 2 thuộc task này (notes.md mục 2 đã ghi trước).

## Ghi chú phạm vi
- Giữ NGUYÊN: logic run/measure (worker + ECharts), buildResults contract BE, exportCsv, prop API BenchmarkPanel (`defaultKeys`), route. Sửa tối thiểu BenchmarkPanel (decision log mục 1). Chips → Button.vue (decision log mục 3). Bảng+chart+conclusion → vùng tối (decision log mục 4). Mobile card-stack (decision log mục 5).

---

## RE-AUDIT (AFTER — 13/08/2026, sau khi sửa)

**Điểm sau**: spacing 7.5 · breakpoint 5 · animation 11 · thị-giác 12.5 · interactive 14.5 · typography 8.5 · depth 6.5 · a11y 10.5 · code 5.5 · performance 5.5 → **TỔNG 87.5/100** · **Đặc trưng 8/10** · **ĐẠT** (hygiene ≥80, không trục dưới sàn, đặc trưng ≥7).

Sửa chính: (1) chrome → surface band level-2 (bỏ gradient/shadow/::after) + kicker mono + H1 text-3xl; (2) badge "Miễn phí tim (20.4)" → "Không tốn tim" (2 chỗ, bỏ số mục SDD); (3) chips raw → Button.vue sm + aria-pressed (bỏ gradient/shadow/translateY hover); (4) select chuẩn hóa h-40px token; (5) bảng + chart + conclusion → vùng dữ liệu LUÔN tối canvas-ink: n = index mono, duration = block-token data-core, so sánh = mono index-muted; LINE_PALETTE hex → đọc CSS var canvas palette; (6) empty state ✂ → hourglass + copy §9; (7) ⚖▶ → lucide Scale/Play; (8) mobile ≤640px bảng → card-stack (cấm scroll ngang); (9) weight 800 → 600/500; (10) results region enter 250ms easing chuẩn + prefers-reduced-motion; (11) breadcrumb mono.
