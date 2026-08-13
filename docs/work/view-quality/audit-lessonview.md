# AUDIT — LessonView `/learn/:lessonId`

> Phase 1 audit TRƯỚC khi sửa (13/08/2026 · dev-engine). Nguồn chấm: `standard.md` mục 2 + 3 + 5. Bằng chứng = dòng code `frontend/src/views/LessonView.vue` (gọi tắt LV) + `docs/work/r2-fixed-03-lesson*.png`.

## Câu hỏi đặc trưng — "Đoán được đây là app học CTDL hay dashboard bất kỳ?"

**KHÔNG đoán được** — hero gradient sunset cam→đỏ + text trắng + text-shadow, quiz list card phẳng + icon box primary, không có block-token/index mono/Big-O chip nổi bật (Big-O nằm lẫn trong Card lý thuyết). Che logo → trang khóa học ed-tech bất kỳ (Udemy/Coursera style). Điểm đặc trưng: **3/10**.

## Điểm 10 trục (BEFORE)

| # | Trục | /8 | /6 | /14 | /14 | /16 | /10 | /8 | /12 | /6 | /6 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| | LessonView | 7 | 4.5 | 6 | 6 | 13 | 6.5 | 5 | 9 | 5 | 5 |

**TỔNG hygiene = 67/100** · **Đặc trưng = 3/10** · **KHÔNG ĐẠT** (đặc trưng < 7; animation 6 < sàn 8.4; thị-giác 6 < sàn 8.4).

## Lỗi + bằng chứng

### KILL-LIST (phải sửa)
- **Gradient hero**: `.lesson-view__hero` `background-image: var(--gradient-sunset)` + `color: #fff` + `text-shadow` (LV 197–235) — KILL-LIST mục 5 (gradient cam→đỏ = r2-fixed-03), DESIGN.md §1 banner = surface band level-2. Lớp `.dark` phủ `rgba(4,47,46,0.62)` (LV 221–223) là vá tương phản của gradient — biến mất khi bỏ gradient.
- **Emoji icon chức năng**: 🎯 ở "Sắp xếp & Tìm kiếm" thực tế nằm ở PathView H1 (PV 161 — ghi nhận chéo, sửa ở PathView); LessonView không có emoji riêng nhưng toast `🎉` ở ExerciseView (EV 40).
- **Shadow trên card**: `.lesson-view__theory` `box-shadow: var(--shadow-sm)` (LV 289) — §6 card cấm shadow; hero `box-shadow: var(--shadow-lg)` (LV 205).
- **hover-lift**: quiz card `hover-lift` (LV 154) — global.css lift + shadow-lg → §6 vi phạm.
- **Text shadow**: `.lesson-view__hero-title` `text-shadow: 0 2px 10px rgba(0,0,0,0.16)` (LV 234) — bóng trên text không thuộc token.

### 10 trục
1. **Spacing (7/8)**: dùng token; `gap: 4px` (LV 326) = token OK; theory table `padding: 6px 10px` (LV 298) — 6px/10px không thuộc scale.
2. **Breakpoint (4.5/6)**: hero-actions flex-wrap OK; theory-meta `repeat(auto-fit, minmax(120px,1fr))` OK; không media query — 390px chấp nhận được, không test thực tế.
3. **Animation (6/14)**: không có animation chủ đích nào (0 khoảnh khắc đầu tư); hover-lift 180ms ease trên quiz card (vi phạm easing >150ms); không prefers-reduced-motion cục bộ.
4. **Thị giác (6/14)**: gradient hero + text trắng (KILL-LIST); quiz-icon `color-mix(primary 12%)` tint trang trí (LV 319–320) — accent teal chỉ interactive (§2.3); weight 700 ở theory-meta dd (LV 278), quiz-title (LV 327); Badge primary/success đúng variant.
5. **Interactive (13/16)**: không `<button>` raw; 3 nút Button chuẩn; `← Về lộ trình` dùng ký tự mũi tên text (LV 108) — nên icon lucide; nút "Học tiếp" chỉ hiện toast placeholder (LV 102–107) — microcopy không nói hành động thật (§9); gap 8px OK.
6. **Typography (6.5/10)**: hero-title text-3xl OK (LV 232); 2 chỗ weight 700; `dt` font-weight 600 OK; `dd` mono 700 → nên 600; quiz-title 700 → 600.
7. **Depth (5/8)**: hero shadow-lg + theory shadow-sm (vi phạm); Card shadcn mặc định `shadow-sm` (component dùng chung — ghi nhận, không sửa global).
8. **A11y (9/12)**: breadcrumb nav có aria-label; các nút có text; theory `v-html` không sanitize (rủi ro XSS nội dung CMS — ngoài phạm vi visual nhưng ghi nhận); quiz list không `aria-label` nhóm.
9. **Code (5/6)**: onMounted fetch + catch gán error; không listener cần gỡ; `theoryMeta` computed OK.
10. **Performance (5/6)**: lazy route; LessonDetail tĩnh trong chunk view (30.98 kB) — chấp nhận.

## Kiểm tra button
- `<button` raw: **0**.
- Button.vue: đúng variant/size; "Học tiếp" hành động giả (toast info placeholder) — vi phạm §9 microcopy.

---

## RE-AUDIT (AFTER — 13/08/2026, sau khi sửa)

**Điểm sau**: spacing 7.5 · breakpoint 5 · animation 11.5 · thị-giác 12 · interactive 14 · typography 8.5 · depth 7 · a11y 10 · code 5 · performance 6 → **TỔNG 87.5/100** · **Đặc trưng 8/10** · **ĐẠT**.

Sửa chính: (1) banner surface band level-2 bỏ gradient sunset/text-shadow/lớp phủ dark; (2) "Học tiếp" thành hành động thật (mở mô phỏng đầu bài — tested /learn/1 → /simulator/sort.bubble; không có sim → tab Lý thuyết); (3) "←" → ArrowLeft lucide; (4) 700 → 600/500; quiz-icon accent tint → muted; (5) hover card chỉ border; (6) breadcrumb mono. Ghi nhận: emoji 🎯/👉/📚 còn trong contentHtml bài học (dữ liệu CMS qua v-html, ngoài phạm vi view) — Phase 2.