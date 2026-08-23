# AUDIT — FinalTestView `/path/:topicId/final-test`

> Phase 2 BỔ SUNG audit (14/08/2026 · dev-frontend · worktree D:\FPT\neww-qp2, nhánh feature/view-quality-merge-check). Nguồn chấm: `standard.md` mục 2 + 3 + 5 + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md`. Bằng chứng = dòng code `frontend/src/views/FinalTestView.vue` (FT) + `frontend/src/i18n/vi.ts` (i18n).

## Câu hỏi đặc trưng — "nhìn thuần bố cục + màu + animation, đoán được đây là app học CTDL không?"

**KHÔNG** — banner gradient aurora (teal→cyan→violet) + blob + dark-overlay + icon gradient giống dashboard generic; rules strip 3 card hover-lift + shadow đồng loạt (KILL-LIST "mọi thứ nổi bằng nhau"); không kicker mono, không index, không vùng dữ liệu tối. Điểm đặc trưng: **4/10**.

## Điểm 10 trục (BEFORE)

| # | Trục | Điểm | Lý do chính |
|---|---|---|---|
| 1 | Spacing/Grid | 7.5/8 | Token OK; `gap: 2px` (FT 243) ngoài scale |
| 2 | Breakpoint | 5/6 | Media 640 có (FT 331); rules `auto-fit minmax(180px,1fr)` wrap OK 390 |
| 3 | Animation | 9/14 | Motion hero `ease: 'easeOut'` (FT 103) — easing mặc định; rule hover `transform 180ms ease, box-shadow 180ms ease` (FT 282) — ease mặc định + hover-lift |
| 4 | Nhất quán thị giác | 8.5/14 | **Gradient aurora hero** (FT 190) + blob (FT 199-210) + **dark-overlay hack** (FT 214-220) + **icon gradient** (FT 233) + badge primary trang trí (FT 113); title #fff + text-shadow (FT 251-254) |
| 5 | Interactive sizing | 14/16 | 0 `<button` raw — back qua Button.vue; trừ nhẹ: `←` ký tự text trong i18n `backToMap` (i18n 794) |
| 6 | Typography | 7/10 | H1 `clamp()` + #fff + text-shadow (FT 251-254); **`font-weight: 700`** (FT 313 — cấm 700); rule-label `font-weight: 600` text-xs (FT 308 — label 400/500); rule-value không mono (dữ liệu số ≥70%, 20% lẽ ra mono) |
| 7 | Depth & Elevation | 5.5/8 | Hero `shadow-lg` (FT 191) + icon `shadow-md` (FT 239); **rule card `shadow-sm` + hover `shadow-md` + translateY(-2px)`** (FT 282-289) — KILL-LIST card nổi đồng loạt + hover-lift |
| 8 | A11y | 10.5/12 | Breadcrumb aria-label OK; rules `aria-label` OK; không raw button |
| 9 | Code quality | 5.5/6 | Logic fetch/build/submit sạch; `void pathId` (FT 50) hơi thừa |
| 10 | Performance | 6/6 | Lazy route OK |

**TỔNG hygiene = 78.5/100** · **Đặc trưng = 4/10** · **KHÔNG ĐẠT** (hygiene 78.5 < 80 ✗; đặc trưng 4 < 7 ✗). Không trục dưới sàn (thị-giác 8.5 ≥ 8.4; depth 5.5 ≥ 4.8) — chỉ fail tổng + đặc trưng.

## Lỗi + bằng chứng

### KILL-LIST (phải sửa)
1. **Banner gradient aurora** — `background-image: var(--gradient-aurora)` (FT 190) + blob (FT 199) + overlay (FT 214-220).
2. **Easing mặc định** — `ease: 'easeOut'` (FT 103); rule hover `180ms ease` (FT 282).
3. **Card nổi đồng loạt** — 3 rule card `shadow-sm` + hover `shadow-md` + lift (FT 281-289).
4. **Weight 700** (FT 313).

### 10 trục
- **Spacing**: `gap: 2px` (FT 243).
- **Thị giác**: badge primary; icon gradient; title #fff + text-shadow; emoji 🏅 trong i18n `toastPassed` (i18n 795).
- **Typography**: rule-value không mono (số liệu nên mono §3).
- **Depth**: không surface band level-2.

## Kiểm tra button
- `<button` raw: **0** — chỉ 1 nút back qua Button.vue (FT 159). QuizStage con có button riêng (component ngoài phạm vi).
- Padding/height: ghost md `h-10 px-4` chuẩn.

---

## RE-AUDIT (AFTER — 14/08/2026, sau khi sửa)

**Điểm sau**: spacing 8 · breakpoint 5.5 · animation 12.5 · thị-giác 13 · interactive 15 · typography 9.5 · depth 7.5 · a11y 11.5 · code 5.5 · performance 6 → **TỔNG 94/100** · **Đặc trưng 8/10** · **ĐẠT** (hygiene ≥80, không trục dưới sàn, đặc trưng ≥7).

Sửa chính: (1) gradient aurora + blob + overlay → **surface band level-2** + **kicker mono** `FINAL TEST · PASS ≥ 70%` (dữ liệu thật threshold — quyết định #1); (2) Motion `easeOut` → `[0.16, 1, 0.3, 1]` 280ms; (3) **rules strip bỏ shadow + hover-lift** → level-1 (surface + border), hover chỉ đổi border → border-strong, transition border-color 150ms chuẩn; giá trị số mono (`≥ 70%`, `20%`) + weight 600 (bỏ 700); label 400 tertiary; (4) icon gradient + shadow → ô muted 44px + lucide 20px tertiary; (5) H1 `--text-4xl` 600 `-0.03em`; badge primary → muted; (6) `←`/🏅 trong i18n → lucide ArrowLeft + bỏ emoji; (7) `gap: 2px` → `--space-xs`; (8) media 640 cho rules/actions.
