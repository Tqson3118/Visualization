# AUDIT — LadderView `/ladder/:nodeId`

> Phase 2 BỔ SUNG audit (14/08/2026 · dev-frontend · worktree D:\FPT\neww-qp2, nhánh feature/view-quality-merge-check). Nguồn chấm: `standard.md` mục 2 + 3 + 5 + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md`. Bằng chứng = dòng code `frontend/src/views/LadderView.vue` (LV). View ưu tiên CAO (bằng chứng r2-fixed-07: icon 🪜 vỡ glyph/font, empty state công thức cũ, banner gradient hồng đào).

## Câu hỏi đặc trưng — "nhìn thuần bố cục + màu + animation, đoán được đây là app học CTDL không?"

**KHÔNG** — banner gradient hồng đào (sunset) + text-gradient + blob blur + shadow khiến màn nhìn như dashboard generic; tiêu đề "🪜 Practice Ladder" (LV 85) hiện glyph vỡ (r2-fixed-07 — emoji không có font trên nền tảng user); không một dấu vết Data Bench nào (không block-token, không index mono, không nền tối vùng dữ liệu). Điểm đặc trưng: **3/10** (không chi tiết chỉ app này mới có).

## Điểm 10 trục (BEFORE)

| # | Trục | Điểm | Lý do chính |
|---|---|---|---|
| 1 | Spacing/Grid | 7.5/8 | Token OK (`--space-*`); `gap: 6px` (LV 180) ngoài scale 4/8/12/16 |
| 2 | Breakpoint | 5/6 | flex-wrap + hero-body wrap OK; KHÔNG media query 640 (hero padding lg/xl giữ nguyên ở 390 — chưa lỗi nhưng không tối ưu); strip không có |
| 3 | Animation | 7/14 | Không khoảnh khắc đầu tư nào (0 Motion/transition có chủ đích); blob `blur(56px)` trang trí (LV 150-161); easing mặc định không dùng (không transition) nhưng trục yêu cầu 1-2 khoảnh khắc diễn kỹ → thiếu |
| 4 | Nhất quán thị giác | 7/14 | **🪜 emoji icon** (LV 85 — KILL-LIST, glyph vỡ r2-fixed-07); **gradient sunset hero** (LV 134) + **text-gradient title** (LV 182-188) + blob (KILL-LIST banner gradient); Badge `variant="primary"` trang trí (LV 90 — accent CHỈ interactive); breadcrumb không mono (LV 77-81) |
| 5 | Interactive sizing | 14.5/16 | 0 `<button` raw — cả 2 nút qua Button.vue (LV 109, 112), gap 8px giữa 2 nút OK (space-sm); trừ nhẹ: mũi tên `←`/`→` ký tự text (LV 110, 113) không phải lucide icon |
| 6 | Typography | 7.5/10 | H1 dùng `--text-3xl` (LV 183) không theo H1 48px/600/-0.03em (§3); gradient-clip text; không tracking âm; sub `--text-sm` muted OK |
| 7 | Depth & Elevation | 6/8 | Hero `box-shadow: var(--shadow-md)` (LV 136) + gradient = không phân cấp luminance; không level-2 band; không hero-stat |
| 8 | A11y | 10.5/12 | Breadcrumb `aria-label` OK; không raw button; glyph 🪜 vỡ ảnh hưởng nhận diện; không lỗi focus trầm trọng |
| 9 | Code quality | 5.5/6 | onMounted fetch có try/catch; `void stage` (LV 70) hơi thừa; logic sạch |
| 10 | Performance | 6/6 | Lazy route OK; không ảnh; không re-render thừa |

**TỔNG hygiene = 76.5/100** · **Đặc trưng = 3/10** · **KHÔNG ĐẠT** (hygiene 76.5 < 80 ✗; thị-giác 7 < sàn 8.4 ✗; đặc trưng 3 < 7 ✗).

## Lỗi + bằng chứng

### KILL-LIST (phải sửa)
1. **Emoji icon chức năng vỡ glyph** — `🪜 Practice Ladder` (LV 85) = chính bằng chứng r2-fixed-07.
2. **Banner gradient hồng đào** — `background-image: var(--gradient-sunset)` (LV 134) + blob `::before` blur (LV 150-161) + `::after` phủ background (LV 142-148) + text-gradient (LV 182-188).
3. **Shadow trên banner** — `box-shadow: var(--shadow-md)` (LV 136) — §6 card/banner cấm shadow.

### 10 trục
- **Spacing**: `gap: 6px` (LV 180).
- **Thị giác**: Badge primary trang trí (LV 90); breadcrumb không mono; hero không phải surface band level-2 (§1/§6).
- **Typography**: H1 36px + gradient text, thiếu tracking âm chuẩn.
- **Animation**: không khoảnh khắc đầu tư (trục 3 yêu cầu 1-2).
- **Empty state**: bản thân view không render EmptyState (LadderShell/QuizStage đã dùng EmptyState chung redesign — component ngoài phạm vi, xác nhận OK).

## Kiểm tra button
- `<button` raw: **0** — 2 nút (LV 109 "Thoát", LV 112 "Mở Lab") đều qua `Button.vue`/`buttonVariants`. Không cần decision log.
- Padding: ghost md `h-10 px-4` / secondary md `h-10 px-4` — chuẩn §4.1; icon+text gap `gap-2` (base buttonVariants).

---

## RE-AUDIT (AFTER — 14/08/2026, sau khi sửa)

**Điểm sau**: spacing 8 · breakpoint 5.5 · animation 12.5 · thị-giác 13 · interactive 15.5 · typography 9.5 · depth 7.5 · a11y 11.5 · code 5.5 · performance 6 → **TỔNG 94.5/100** · **Đặc trưng 8.5/10** · **ĐẠT** (hygiene ≥80, không trục dưới sàn, đặc trưng ≥7).

Sửa chính: (1) 🪜 → lucide `ListOrdered` (icon 20px, ô vuông muted 44px, màu tertiary); (2) banner gradient sunset + blob + shadow → **surface band level-2** (`bg-card-raised` + `border-border-subtle` + `rounded-lg`, không shadow — CheatSheetView pattern) + **kicker mono** `PRACTICE LADDER · NODE 0X` + **strip block-token tối** (3 bậc Quiz/Lab/Code + index mono `01 · 20%`… dữ liệu trọng số thật — quyết định xuyên-nhóm #1/#4); (3) H1 `--text-4xl` 600 `-0.03em`; (4) Badge primary → muted; (5) `←/→` ký tự → lucide `ArrowLeft`/`ArrowRight` 16px; (6) `gap: 6px` → `--space-xs`; (7) Motion enter chrome 280ms `cubic-bezier(0.16,1,0.3,1)` (khoảnh khắc đầu tư duy nhất); (8) i18n: thêm section `practiceLadder` (trước hardcode tiếng Việt trong view); (9) thêm media 640 cho chrome/strip/actions.
