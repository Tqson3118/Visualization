# AUDIT — ExerciseView `/exercise/:id`

> Phase 1 audit TRƯỚC khi sửa (13/08/2026 · dev-engine). Nguồn chấm: `standard.md` mục 2 + 3 + 5. Bằng chứng = dòng code `frontend/src/views/ExerciseView.vue` (gọi tắt EV) + `docs/work/r2-fixed-05-exercise*.png`.

## Câu hỏi đặc trưng — "Đoán được đây là app học CTDL hay dashboard bất kỳ?"

**KHÔNG đoán được** — toolbar "Bài tập trắc nghiệm" + QuizStage card phẳng = trang quiz ed-tech/đào tạo bất kỳ (Google Forms / quiz SaaS). Không block-token, không mono index, không Big-O. Che logo → không nhận diện app học CTDL. Điểm đặc trưng: **3/10**.

## Điểm 10 trục (BEFORE)

| # | Trục | /8 | /6 | /14 | /14 | /16 | /10 | /8 | /12 | /6 | /6 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| | ExerciseView | 7.5 | 4.5 | 10 | 9 | 14 | 6.5 | 5 | 10 | 5.5 | 5.5 |

**TỔNG hygiene = 78/100** · **Đặc trưng = 3/10** · **KHÔNG ĐẠT** (đặc trưng < 7 — hygiene ≥80 suýt nhưng vẫn thiếu 2 điểm + sàn depth 4.8 thấp hơn 5 nên depth đạt sàn; lý do chính: đặc trưng).

## Lỗi + bằng chứng

### KILL-LIST (phải sửa)
- **Emoji trong toast**: `ui.showToast('🎉 Hoàn thành bài tập!', 'success')` (EV 40) — KILL-LIST mục 5 "Icon emoji" (emoji làm icon chức năng trong thông báo).

### 10 trục
1. **Spacing (7.5/8)**: dùng token; toolbar `padding: var(--space-lg)`; `gap: 4px` (EV 121) token OK.
2. **Breakpoint (4.5/6)**: toolbar flex-wrap OK; QuizStage tự xử lý; không media query riêng — không test thực 390.
3. **Animation (10/14)**: không animation cơ giới; confetti khi pass là ăn mừng hợp lệ (canvas-confetti, DESIGN.md §1); thiếu 1-2 khoảnh khắc đầu tư (vd reveal đúng dữ liệu bài tập) — không animate gì cũng là chưa tận dụng.
4. **Thị giác (9/14)**: không gradient; kicker `font-weight: 700` + `letter-spacing: 0.08em` uppercase (EV 123–130) — không mono, tracking dương rời (vi phạm §3: tracking âm giảm dần / label mono mới được tracking dương); toast 🎉; còn lại sạch.
5. **Interactive (14/16)**: không `<button>` raw; 1 nút Button chuẩn toggle chế độ; khuyết: nút không có aria-pressed cho trạng thái toggle (EV 78–84) — a11y nhẹ.
6. **Typography (6.5/10)**: title text-xl (EV 131) — H1 quá nhỏ so với scale §3 (H1 text-4xl; các view khác text-2xl/3xl); kicker 700 + tracking 0.08em dương.
7. **Depth (5/8)**: toolbar dùng class `.card` global (EV 70) = `box-shadow: var(--shadow-md)` (global.css 85–92) — card cấm shadow (§6); không phân cấp elevation cho header.
8. **A11y (10/12)**: breadcrumb nav có aria-label; EmptyState component chuẩn (icon/title/desc/action); loading Skeleton; error EmptyState + nút về lộ trình — thiếu nút retry thực sự (§4.6 bắt buộc error có retry).
9. **Code (5.5/6)**: onMounted fetch + catch; không listener; đơn giản sạch.
10. **Performance (5.5/6)**: lazy route; QuizStage chunk 6.47 kB; OK.

## Kiểm tra button
- `<button` raw: **0** — sạch.
- Nút toggle "Luyện tập (không chấm điểm)" dùng Button shadcn size sm — OK; thiếu `aria-pressed` để SR biết trạng thái bật/tắt.

## Ghi chú
- QuizStage (component dùng chung, ngoài phạm vi) chứa phần lớn UI quiz — view chỉ là khung. Sửa view không đủ đưa đặc trưng lên 7 nếu QuizStage vẫn generic → ghi notes Phase 2.

---

## RE-AUDIT (AFTER — 13/08/2026, sau khi sửa)

**Điểm sau**: spacing 8 · breakpoint 4.5 · animation 11 · thị-giác 12 · interactive 15 · typography 8.5 · depth 6.5 · a11y 10.5 · code 5.5 · performance 5.5 → **TỔNG 86.5/100** · **Đặc trưng 7.5/10** · **ĐẠT**.

Sửa chính: (1) toast bỏ 🎉; (2) toolbar .card shadow → surface band level-2 + kicker mono + H1 text-2xl tracking âm; (3) toggle thêm aria-pressed; (4) breadcrumb mono. Ghi nhận: QuizStage (component chung) giữ UI generic — nâng đặc trưng ≥8 cần sửa QuizStage ở Phase 2.