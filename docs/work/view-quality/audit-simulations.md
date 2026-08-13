# AUDIT — SimulationsView (`/simulations` — Khám phá)

> Audit trước khi sửa (Phase 1, nhóm A). Ngày: 13/08/2026. Chấm theo `standard.md` 10 trục + Đặc trưng tách riêng.

## Câu trả lời gate (che logo/chữ, nhìn thuần bố cục + màu + animation)

**CÓ, giống trang catalog của bất kỳ SaaS nào** — nhưng có dấu vết yếu của app này: key mô phỏng mono (`item.key`), chip độ phức tạp `Big-O` mono, EmptyState motif khung mảng. Dấu vết chưa đủ mạnh (hero gradient "Cyber Mint" + stat gradient át hết). → Đặc trưng thấp-trung bình, PHẢI nâng.

## Điểm 10 trục (trước sửa)

| # | Trục | Điểm | Lý do |
|---|---|---|---|
| 1 | Spacing/Grid | 6.5/8 | `--space-*` chuẩn đa số; vi phạm: `gap: 6px` `.simulations__stat` + `.simulations__card-badges` (lẻ), `gap: 2px` `.simulations__complexity` |
| 2 | Breakpoint | 4.5/6 | Grid `auto-fill minmax(260px,1fr)` OK; media 640 OK; select min-width 160px; chưa đo 390px thực tế |
| 3 | Animation | 5/14 | `.simulations__card` class `hover-lift` (global `180ms ease` + `shadow-lg` hover — easing mặc định + shadow card cấm); 0 khoảnh khắc đáng đầu tư |
| 4 | Nhất quán thị giác | 3/14 | Gradient mint ×4 (`.simulations__chrome`, `__icon`, `__title`, `__stat dd`) — KILL-LIST; `::after` overlay 68% che gradient (patch contrast, không xử lý gốc); stat dd `font-weight: 800`; accent primary dùng trang trí `.simulations__card-icon` |
| 5 | Interactive sizing | 7/16 | Không raw `<button>` (pagination/clearFilters qua Button.vue OK); card `role="button"` tabindex=0 + Enter OK nhưng **thiếu Space**; `hover-lift` trên card (shadow + ease mặc định); badge `py-0.5 + text-xs` → height ~20px < 24px (trục 5f) |
| 6 | Typography | 5/10 | `.simulations__stat dd` 800 (cấm >600); `.simulations__open` 700; `.simulations__complexity dt` 700; `.simulations__title` clamp 30–36px (H1 lệch scale §3 = 48px) + gradient |
| 7 | Depth & Elevation | 3/8 | Chrome gradient + `shadow-md`; stat dd gradient text; 3 stat đồng hạng — 0 hero-stat (thiếu phân cấp §6); card hover `shadow-lg` (cấm) |
| 8 | A11y | 8.5/12 | Card role=button Enter OK, thiếu Space; aria-label card/select OK; pagination nav aria-label OK; EmptyState OK |
| 9 | Code quality | 5.5/6 | computed lọc OK; v-for key `item.key` OK; không timer |
| 10 | Performance | 4.5/6 | **`BenchmarkPanel` + `CheatSheetTable` import TĨNH** — cả 2 chunk nặng vào bundle SimulationsView dù chỉ render theo tab (nên `defineAsyncComponent` — trục 10 route-level splitting) |
| | **TỔNG hygiene** | **53/100** | |

## Trục Đặc trưng: **3/10** — có dấu vết (key mono, Big-O chip, EmptyState block motif) nhưng hero gradient + stat chung chung át hết; chưa đủ mạnh. KHÔNG đạt ≥7.

## Danh sách lỗi chính (kèm selector/dòng)

1. **Hero công thức + gradient**: `SimulationsView.vue:316-337` `.simulations__chrome` `--gradient-mint` + shadow-md + `::after` overlay; `:355-366` `__icon` gradient; `:368-374` `__title` gradient-clip; `:406-414` `__stat dd` gradient-clip + 800.
2. **hover-lift trên card**: `:232` `class="hover-lift simulations__card"` → shadow-lg + `180ms ease`.
3. **Badge < 24px**: `:245,254` Badge height ~20px (py-0.5 + text-xs).
4. **Typography 700/800**: `:408` dd 800; `:490` dt 700; `:498` open 700.
5. **Performance**: `:25-26` import tĩnh CheatSheetTable + BenchmarkPanel.
6. **Card keyboard**: `:237` chỉ `@keydown.enter`, thiếu Space.
7. **Thiếu block-token**: stats 3 số không có 1 hero/block motif; chưa có strip block + index mono trong banner.

## Trạng thái: **KHÔNG ĐẠT** (hygiene 53 < 80; đặc trưng 3 < 7). Đã sửa (xem fix-log).

---

# RE-AUDIT SAU SỬA — SimulationsView

## Điểm sau sửa

| # | Trục | Điểm | Ghi chú |
|---|---|---|---|
| 1 | Spacing/Grid | 8/8 | Token hết (đã bỏ gap 6px/2px) |
| 2 | Breakpoint | 6/6 | Đo 768 (grid 2 cột 352px) + 390 (1 cột 343px, bench 196px wrap OK) + 1536 |
| 3 | Animation | 11.5/14 | Bỏ hover-lift (shadow-lg + 180ms ease) → hover chỉ đổi border 150ms chuẩn; Motion hero 280ms |
| 4 | Nhất quán | 13.5/14 | Chrome band level-2; strip block-token + index mono; stat Geist 600; badge min-h-6 (đo 25.6px) |
| 5 | Interactive sizing | 15.5/16 | Không raw button; card role=button + Enter + **Space** mới; pagination Button |
| 6 | Typography | 9.5/10 | H1 48/600/-0.03em; stat 30/600/-0.015em; open 500; bỏ 700/800 |
| 7 | Depth | 7.5/8 | Chrome level-2; bench = hero motif duy nhất; stats level-1; card shadow-none (Card.vue base shadow-sm bị override — ghi chú shared) |
| 8 | A11y | 11/12 | Space key mới; aria-label giữ nguyên |
| 9 | Code | 5.5/6 | computed OK; lazy async tab |
| 10 | Performance | 6/6 | BenchmarkPanel + CheatSheetTable → defineAsyncComponent (đo build: BenchmarkPanel chunk 35.5kB tách riêng, load theo tab) |
| | **TỔNG hygiene** | **94/100** | |

## Đặc trưng sau: **9/10** — strip block-token "INDEX 00–04 · CATALOG 44" (block data-core + 1 resolved + index mono) trong banner + key mono + Big-O chip; Ollama gate 1: "RO RANG APP HOC CTDL".

## KẾT LUẬN: **ĐẠT** (hygiene 94 ≥ 80; không trục dưới sàn; đặc trưng 9 ≥ 7).
