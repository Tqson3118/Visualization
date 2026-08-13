# Audit — AdminStatsView.vue (`/admin/stats`) — Nhóm D (admin)

> Ngày: 14/08/2026 · Agent: dev-frontend (nhóm D — admin) · Nguồn chuẩn: `standard.md` + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md` + 6 quyết định xuyên-nhóm.

## Câu trả lời trục Đặc trưng (đầu file — bắt buộc)

**Không nhận diện được app học CTDL — trang KPI + 2 biểu đồ của bất kỳ admin dashboard nào.** Bằng chứng: hero gradient Aurora + blob + title gradient-clip (245–304), 5 stat-card công thức KILL-LIST (icon tròn 3 gradient aurora/mint/sunset + `#fff` + shadow-sm + hover-lift + số weight 800, dòng 315–341), donut SVG font-weight 800, legend weight 700, th/tag uppercase + tracking. Dấu vết duy nhất: chart đọc CSS var (theme-aware) — chưa đủ mạnh.

## Điểm trước sửa

| # | Trục | Điểm | Sàn | Bằng chứng vi phạm (dòng/selector) |
|---|---|---|---|---|
| 1 | Spacing/Grid | 7.0 | 4.8 | Grid token ✓; `gap: 4px` (292) — trong scale; `margin-top: 2px` note-icon (379) — 2px ngoài scale; `grid 7fr/5fr` + auto-fit ✓ |
| 2 | Breakpoint | 5.0 | 3.6 | KPI grid auto-fit (309, 387) ✓; charts stack ≤900 ✓; hero wrap ✓ — không lỗi lớn |
| 3 | Animation | 5.5 | 8.4 | `hover-lift` (180ms ease + translate + shadow, 179) trên MỌI KPI card — "animation cơ giới" KILL-LIST; không khoảnh khắc đầu tư; ease mặc định |
| 4 | Nhất quán thị giác | 3.0 | 8.4 | Hero gradient + blob + gradient-clip (245–304); **stat-card công thức KILL-LIST** ×5 (315–341: icon tròn gradient + shadow + #fff); donut `font-weight="800"` (207); legend 700 (370); `.text-muted` legacy (197, 228, 302, 368); tag uppercase (352–358); **5 KPI cùng level — vi phạm §6 "tối đa 1 hero-stat"** |
| 5 | Interactive sizing | 15.0 | 9.6 | Không button nào trong view (chỉ skeleton/badge) — không vi phạm; badge hero ✓ |
| 6 | Typography | 4.5 | 6.0 | H1 30px (295) ≠ 48px; KPI value weight 800 (337); legend 700 (370); tag uppercase + tracking (355–356); donut svg text 10px < 12px caption (208) |
| 7 | Depth & Elevation | 3.0 | 4.8 | Hero + icon `--shadow-md` (253, 289); KPI icon `--shadow-sm` (324); hover-lift shadow; không phân cấp level-1/2; **0 hero-stat đúng nghĩa** |
| 8 | A11y | 9.0 | 7.2 | Donut role=img + aria-label ✓; skeleton aria-busy ✓; text 10px donut nhỏ; contrast legend-dot OK |
| 9 | Code quality | 5.0 | 3.6 | `cssVar()` helper đọc theme ✓; `DONUT_COLORS` dùng `--color-*` legacy thay 4 tầng/canvas; `weekOption` phụ thuộc theme → recompute ✓; ROLES/WEEK mock ghi rõ ✓ |
| 10 | Performance | 5.5 | 3.6 | `VChartLazy` lazy echarts ✓ (chunk echarts riêng 323 kB); nhẹ |
| **TỔNG** | | **62.5** | | |

**Đặc-trưng: 2/10** · **Kết luận: KHÔNG ĐẠT** (hygiene <80, đặc-trưng <7).

## Rà TỪNG button (trục 5 — bắt buộc)

Không có button nào trong view (chỉ Badge + Skeleton) — trục 5 không vi phạm trực tiếp.

## KILL-LIST bị vi phạm (phải sửa)

1. Hero gradient + blob + gradient-clip (245–304).
2. **Stat-card công thức: icon tròn + số to + hover-lift, dùng 5 cái cùng lúc** (315–341).
3. Easing `ease` hover-lift 180ms (217–229 global).
4. `#fff` (323) + weight 800/700.

## Hướng sửa (đã làm)

- Banner → surface band level-2 + **mono strip block-token** (5 chỉ số, block `data-core` + index mono, stagger-enter — khoảnh khắc đầu tư).
- **1 hero-stat** (Tổng người dùng): card level-2 + **block-token tối** `bg-canvas-ink` + block `data-core` + index mono (quyết định #3/#4/#5); 4 KPI còn lại level-1 không icon/shadow/hover-lift, số Geist 600.
- **2 vùng biểu đồ → LUÔN tối `bg-canvas-ink`** (quyết định #5): bar ECharts palette đọc CSS var `--data-core`/`--index-muted`/`--canvas-ink` + text `#d9dde8` (màu text engine — decision log 14/08); donut màu `data-core/resolved/index-muted` (3 màu ngôn ngữ dữ liệu, không bịa), legend mono, bỏ hex rời.
- Tag/chart-title → mono caption; weight 800/700 → 600/500; donut text ≥12px.
- Thêm error state + nút Thử lại; note card dùng 4 tầng token.
