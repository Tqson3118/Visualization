# AUDIT — QuestsView.vue (`/quests`) — nhóm C

> Audit theo `standard.md` + `frontend/DESIGN.md`. Ngày: 13/08/2026. Agent: dev-frontend (nhóm C).

## Câu hỏi bản sắc (BƯỚC D)

**"Xoá chữ/logo đi, nhìn thuần bố cục + màu + animation, có ai đoán được đây là app học cấu trúc dữ liệu không, hay giống demo dashboard bất kỳ?"**

→ **Game daily-quest bất kỳ.** Hero gradient aurora + streak chip gradient sunset + freeze chip + reward amber. Không chi tiết DSA. Điểm đặc trưng: 3/10.

## Điểm 10 trục (pre-fix)

| # | Trục | Điểm | Bằng chứng |
|---|---|---|---|
| 1 | Spacing/Grid | 7/8 | `gap: 6px` streak (227), `gap: 5px` freeze (243), `gap: 6px` body (292), `gap: 4px` reward-value (286 — token OK). `.quests__streak padding: 5px 14px` (233) — 5px lẻ ngoài scale. List 1 cột max 760px OK. |
| 2 | Breakpoint | 5/6 | Mobile ẩn hero-badge margin (305). Chưa verify 3 mốc. |
| 3 | Animation | 11/14 | Card không hover-lift (tốt); không animation thừa. Không khoảnh khắc đầu tư — claim quest xong chỉ toast; **nên thêm confetti khi claim quest cuối (allDone)** (canvas-confetti đã cài, disableForReducedMotion — quyết định #6). |
| 4 | Nhất quán thị giác | 7/14 | Hero gradient + blob + shadow (158–191); title gradient-clip (210–216); streak chip `--gradient-sunset` + shadow-sm (231–235) — KILL-LIST; card--ready `linear-gradient(180deg, success 5% → card)` (277) — gradient trang trí (chỉ border/tint cho phép); reward `text-amber-700 dark:text-amber-400` (113). `font-weight: 800` streak (229) + reward (285); `font-weight: 700` freeze (244) + title-text (294). Toast emoji 💎 trong i18n `claimedToast` (vi.ts 545) — microcopy emoji. |
| 5 | Interactive sizing | 13.5/16 | Nút "Nhận thưởng" `size="sm"` (131) = CTA chính quest → 36px < 40px (§4.1) → md. Không raw `<button`. |
| 6 | Typography | 7/10 | 800/700 vi phạm; H1 `text-2xl` (211) sai hierarchy; reward số không mono (113); xp `+20 XP` không mono (129). |
| 7 | Depth & Elevation | 5/8 | Hero shadow-md; streak chip shadow-sm; bonus/card--ready dùng tint OK; không phân cấp stat (chỉ streak + freeze chip). |
| 8 | A11y | 10/12 | Nút có text OK; `role="status"` bonus (94) OK; freeze `:title` OK. Difficulty Badge có text — OK. |
| 9 | Code quality | 5.5/6 | Logic claim atomic OK; DIFFICULTY map OK; `v-for` key quest.id OK. |
| 10 | Performance | 6/6 | Route lazy. |

**TỔNG hygiene (pre): 77/100** — dưới 80. Trục dưới sàn: depth 5/8 ≥ 4.8 OK (không trục dưới sàn, chỉ thiếu tổng).

## KILL-LIST vi phạm
- Hero gradient + blob + shadow; title gradient; streak chip gradient sunset + shadow.
- Streak = dữ liệu tuần tự → **block-token tối tone resolved** (quyết định #4/#5; resolved = "hoàn thành, streak" DESIGN-IDENTITY §1.2).
- Reward/XP → mono + token (bỏ amber).
- Card--ready gradient → border + tint success (không gradient).
- Nút claim → md. Toast bỏ emoji 💎 (cùng động từ "Nhận thưởng/Đã nhận" §9).
- Confetti khi hoàn thành 5/5 (khoảnh khắc đầu tư duy nhất) — canvas-confetti + disableForReducedMotion.
- `font-weight` 800/700 → 600; H1 48px; padding lẻ 5px → 6px/8px token.
