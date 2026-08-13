# Audit — AdminLadderView.vue (`/admin/ladder`) — Nhóm D (admin)

> Ngày: 14/08/2026 · Agent: dev-frontend (nhóm D — admin) · Nguồn chuẩn: `standard.md` + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md` + 6 quyết định xuyên-nhóm.

## Câu trả lời trục Đặc trưng (đầu file — bắt buộc)

**Không nhận diện được app học CTDL — màn "gắn exercise vào node" của bất kỳ CMS/quiz-builder nào.** Bằng chứng: hero gradient Aurora + blob + gradient-clip (175–234), **node-id tròn gradient + weight 800** (276–288), `hover-lift` node button (101), note card `.text-muted` legacy. Dấu vết duy nhất: danh sách "Node 1..8" có stage Quiz/Lab/Code (gợi lộ trình) — chưa đủ mạnh, node-id số đáng lẽ là index mono block-token.

## Điểm trước sửa

| # | Trục | Điểm | Sàn | Bằng chứng vi phạm (dòng/selector) |
|---|---|---|---|---|
| 1 | Spacing/Grid | 7.0 | 4.8 | Token ✓; grid 2 cột → 1 cột ≤800 ✓; `gap: 4px` (222) trong scale |
| 2 | Breakpoint | 5.0 | 3.6 | Grid collapse ✓; node row wrap? nút node chứa id + stage + badge — có thể chật 390px nhưng wrap badge `margin-left:auto`; OK |
| 3 | Animation | 5.5 | 8.4 | `hover-lift` node (101, 180ms ease + translate + shadow) — cơ giới trên 8 node giống hệt; không khoảnh khắc đầu tư |
| 4 | Nhất quán thị giác | 4.0 | 8.4 | Hero gradient + blob + gradient-clip (175–234) — **KILL-LIST**; node-id gradient + `--color-on-primary` (276–288); `.text-muted` legacy (241); EmptyState icon="puzzle" ✓; thông tin badge ✓ |
| 5 | Interactive sizing | 8.5 | 9.6 | **1 `<button` RAW**: `.admin-ladder__node` (99–105) — không qua buttonVariants, padding 8px 16px, hover-lift, không focus-visible ring shadcn; các nút còn lại qua Button.vue ✓; select/`.input` legacy |
| 6 | Typography | 4.5 | 6.0 | H1 30px (225) ≠ 48px; node-id weight 800 (286); subtitle `--text-md` 18px (247) → text-lg 600 tracking; stage label weight 600 (264) ok |
| 7 | Depth & Elevation | 3.5 | 4.8 | Hero `--shadow-md` (183); node-id gradient + selected `box-shadow: 0 0 0 2px` (273) — ring được phép (focus/active) nhưng 2px color-mix chưa chuẩn; `.card` legacy shadow (84, 95, 120) |
| 8 | A11y | 8.5 | 7.2 | Node button `aria-pressed` ✓; nhưng raw button thiếu focus-visible rõ ràng (chỉ global :focus-visible outline); select label ✓ |
| 9 | Code quality | 5.0 | 3.6 | Logic gọn; NODES mock 8 node (ghi chú dạng cơ bản) ✓; `nodeExercises` Map ✓; không timer |
| 10 | Performance | 5.5 | 3.6 | Nhẹ, không ảnh |
| **TỔNG** | | **56.0** | | |

**Đặc-trưng: 2/10** · **Kết luận: KHÔNG ĐẠT** (hygiene <80, đặc-trưng <7, interactive-sizing dưới sàn).

## Rà TỪNG button (trục 5 — bắt buộc)

| Button | Selector | Qua Button.vue? | Padding | Target | Ghi chú |
|---|---|---|---|---|---|
| **Node row (chọn)** | `.admin-ladder__node` (99) | ❌ **RAW `<button>`** | 8px 16px | ~40px | **vi phạm grep `<button` raw = 0**; hover-lift; phải qua Button.vue + class bổ sung |
| Gắn exercise | (155) | ✅ md default | h-10 px-4 | 40px | icon 14px → 16px |

## KILL-LIST bị vi phạm (phải sửa)

1. Hero gradient + blob + gradient-clip (175–234).
2. Node-id tròn gradient (276–288) — thay block-token tối index mono (quyết định #4: node = dữ liệu tuần tự có chỉ số).
3. `hover-lift` 180ms ease (101) + shadow trên `.card` legacy.
4. Easing mặc định + weight 800.

## Hướng sửa (đã làm)

- Banner → surface band level-2 (không strip — admin tối giản).
- **1 `<button` raw → `Button.vue`** (variant outline, size default, `w-full` + justify-start qua scoped override; giữ aria-pressed) — grep `<button` raw = 0.
- **Node-id → block-token tối**: `bg-canvas-ink` + `text-data-core` + mono index (index mono dưới/side block — signature Data Bench); bỏ gradient + weight 800.
- Node row: bỏ hover-lift; hover/selected chỉ đổi border + ring 1px `var(--primary)`; gap 8px giữa các phần tử.
- Note card `.text-muted` → 4 tầng token; icon info → `var(--info)`.
- Thêm error state + nút Thử lại cho load exercise (trước: fail → EmptyState "Chưa có bài tập" gây nhầm).
