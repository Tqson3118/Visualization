# Audit — ClassReportView.vue (`/classes/:id/report`) — Nhóm D

> Ngày: 14/08/2026 · Agent: dev-frontend (nhóm D — classes) · Nguồn chuẩn: `standard.md` + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md`.

## Câu trả lời trục Đặc trưng (đầu file — bắt buộc)

**Không nhận diện được app học CTDL — đây là "report dashboard" của mọi SaaS giáo dục.** Bằng chứng: hero gradient compact + icon tròn trắng mờ (230–251, 263–274), 4 KPI card với icon tròn **3 gradient khác nhau** + shadow (306–320), `hover-lift` + shadow trên mọi card (147), bảng chữ hoa (346–356), avatar tròn gradient (373–385), EmptyState `icon="chart"` không tồn tại trong `SVG_PATHS` → fallback `x-circle` (136). Số "đo được" (sims/exercises/best) không mono. Dấu vết đặc trưng: 0.

## Điểm trước sửa

| # | Trục | Điểm | Sàn | Bằng chứng vi phạm (dòng/selector) |
|---|---|---|---|---|
| 1 | Spacing/Grid | 6.0 | 4.8 | `gap: 2px` (276); còn lại theo token ✓ |
| 2 | Breakpoint | 4.5 | 3.6 | Table `min-width: 640px` + scroll ngang (344) — cấm ở mobile; KPI `auto-fit minmax(170px)` OK; chưa test 390 |
| 3 | Animation | 7.0 | 8.4 | `hover-lift` 180ms ease + shadow trên 4 KPI (147); `tbody tr transition 150ms ease` (365); không khoảnh khắc đầu tư; không enter animation |
| 4 | Nhất quán thị giác | 4.5 | 8.4 | Hero gradient + overlay (230–251) + `#fff` (236, 273, 282, 292); KPI icon gradient 3 màu (318–320) — **công thức stat-card KILL-LIST**; th uppercase (352); `.text-muted` legacy (162, 191, 197); EmptyState icon `chart` → fallback sai; `✓` text glyph (195) |
| 5 | Interactive sizing | 14.0 | 9.6 | 0 `<button` raw ✓; Export/Print `sm` (36px) — nên md; gap 8px ✓; `:loading` export ✓ |
| 6 | Typography | 4.0 | 6.0 | H1 `text-2xl` (278) ≠ 48px/-0.03em; KPI value `font-weight: 800` (328); avatar 800 (381); name 700 (389); th uppercase + tracking 0.05em (350–351); cột số không mono (196–198) |
| 7 | Depth & Elevation | 3.5 | 8.4 | Hero `--shadow-lg` (237); icon `--shadow-sm` (315); hover-lift shadow; **4 KPI nổi bằng nhau — không có 1 hero-stat/màn** (quyết định #3) |
| 8 | A11y | 9.5 | 7.2 | th thiếu `scope="col"`; nút có text ✓; `aria-busy` loading ✓; breadcrumb aria-label ✓ |
| 9 | Code quality | 5.0 | 3.6 | `KPIS` array + tint string (78–83, 149) — tách được khỏi view; `exportCsv` link click không append DOM (57–62) — chấp nhận (hoạt động); phần còn lại gọn |
| 10 | Performance | 5.5 | 3.6 | Không ảnh; import nhẹ; lazy route ✓ |
| **TỔNG** | | **63.5** | | |

**Đặc-trưng: 2/10** · **Kết luận: KHÔNG ĐẠT** (hygiene <80, đặc-trưng <7).

## Rà TỪNG button (trục 5 — bắt buộc)

| Button | Selector | Qua Button.vue? | Padding | Target | Ghi chú |
|---|---|---|---|---|---|
| Xuất CSV | `.class-report__actions` (115–117) | ✅ sm default | h-9 px-3 | 36px | nên md — action chính |
| In | (118–120) | ✅ sm secondary | h-9 px-3 | 36px | gap 8px ✓ |
| Về chi tiết lớp (EmptyState) | (140–141) | ✅ (trong EmptyState chung) | h-10 px-4 | 40px | ✓ |

## KILL-LIST bị vi phạm (phải sửa)

1. Hero gradient + overlay (102–123, 229–251).
2. **Công thức stat-card**: icon tròn + số to + gradient 3 màu (78–83, 306–320) — 4 card nổi bằng nhau.
3. Easing mặc định `ease` (365) + hover-lift 180ms ease.
4. `✓` text glyph thay icon; EmptyState icon sai; hex `#fff`; weight 800/700; th uppercase.

## Hướng sửa (đã làm)

- Banner → surface band level-2 compact, title 48px, sub **mono** `Tên lớp · ID 07` (dữ liệu — mono).
- KPI → **1 hero-stat duy nhất (Thành viên)**: card level-2 + dark panel `bg-canvas-ink` + value `text-data-core` 30px + index mono `text-index-muted` (quyết định #3/#4/#5 — vùng dữ liệu tối); 3 stat còn lại level-1, label `text-foreground-tertiary text-xs`, value Geist 600 2xl `tracking-[-0.015em]`, KHÔNG icon tròn, KHÔNG shadow/hover-lift.
- Bảng sinh viên → chuẩn §4.6: bỏ uppercase, td 12px, hover muted, **cột số mono** (sims/exercises/best/ID), `Check`/`Minus` lucide cho cột Đã xem, mobile card-stack (data-label) thay scroll ngang.
- Avatar neutral; weight ≤600; EmptyState `icon="database"`; toast/nút cùng động từ ✓ (giữ).
- Animation: 1 khoảnh khắc — hero-stat dark panel enter (transform+opacity 250ms `cubic-bezier(0.16,1,0.3,1)`); bỏ mọi transition ease >150ms.
