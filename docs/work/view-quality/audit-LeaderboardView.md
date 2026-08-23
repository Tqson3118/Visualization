# AUDIT — LeaderboardView.vue (`/leaderboard`) — nhóm C

> Audit theo `standard.md` + `frontend/DESIGN.md`. Ngày: 13/08/2026. Agent: dev-frontend (nhóm C).

## Câu hỏi bản sắc (BƯỚC D)

**"Xoá chữ/logo đi, nhìn thuần bố cục + màu + animation, có ai đoán được đây là app học cấu trúc dữ liệu không, hay giống demo dashboard bất kỳ?"**

→ **Dashboard gamification bất kỳ.** Hero gradient aurora + trophy + badge emoji 🏆 + huy chương emoji 🥇🥈🥉 + top-3 row phủ gradient. Chỉ có yếu tố "leaderboard" chung chung. Điểm đặc trưng: 3/10.

## Điểm 10 trục (pre-fix)

| # | Trục | Điểm | Bằng chứng |
|---|---|---|---|
| 1 | Spacing/Grid | 7/8 | Gap lẻ: `.board-row__rank gap: 4px` (463 — token OK), `.board-row__streak gap: 2px` (497 — ngoài scale), `.leaderboard__hero-title-wrap gap: 4px` OK. `.board-row padding: 0.75rem var(--space-md)` (432 — 12px dọc = p-3 token OK). Grid list 1 cột + pager OK. |
| 2 | Breakpoint | 5/6 | Mobile ẩn chart + value-label + pinned-label (536–541) — hợp lý. Chưa verify 3 mốc thực tế. |
| 3 | Animation | 9/14 | `.board-row-move transition: transform 0.45s cubic-bezier(0.22,1,0.36,1)` (421) — 450ms + easing ngoài chuẩn (quyết định #6: 200–300ms, enter `0.16,1,0.3,1`). `.board-row-enter-active transition: opacity 0.3s ease` (422) — `ease` mặc định (KILL-LIST V2). `.board-row transition: background-color 180ms ease` (436). FLIP reorder = khoảnh khắc đầu tư hợp lý nhưng tham số sai chuẩn. |
| 4 | Nhất quán thị giác | 6/14 | Emoji medal 🥇🥈🥉 (74, 230) + hero badge `🏆 Top learners` (175) — KILL-LIST. Hero gradient + blob (330–354) + title gradient-clip (379–384). Top-3 row `::before` gradient-sunset/aurora/mint opacity 0.1–0.18 (440–457) — gradient trang trí. me-badge gradient bg (489–493). Pinned row `box-shadow: var(--shadow-sm)` (508). Empty error state thiếu nút retry (201–206). Chart màu rank hex `#f59e0b/#94a3b8/#d97706` (114). |
| 5 | Interactive sizing | 14.5/16 | Mọi button qua Button.vue (grep `<button` = 0). Pager sm 36px phụ OK, gap 16px OK. Row leaderboard không clickable — OK. |
| 6 | Typography | 7/10 | `font-weight: 800` rank-num (465), avatar-fallback (478), me-badge (489), pinned-label (513, + `letter-spacing: 0.02em` dương 517); `font-weight: 700` value (499), chart-title (406). H1 `text-2xl` (379) — sai hierarchy (H1 = text-4xl §3). Value số không mono (499) — dữ liệu phải mono §3. |
| 7 | Depth & Elevation | 4.5/8 | Hero shadow-md + blob; pinned shadow-sm; top-3 phủ gradient "nổi" bằng gradient thay vì luminance. Rank không phân cấp bằng block-token. |
| 8 | A11y | 9.5/12 | TransitionGroup `tag="ol"` + `role="status"` pinned (262) OK. `aria-label` pager OK. Emoji medal đọc máy 🥇 (không aria-hidden — 230 có aria-hidden ✓). Error state không retry được = không đủ "thông báo + nút retry" (trục 4d). |
| 9 | Code quality | 5/6 | cssVar trùng ProfileView; logic fetchBoard ở store OK; `v-for` key userId ổn định ✓. |
| 10 | Performance | 6/6 | Route lazy (26), VChartLazy, TransitionGroup chỉ 1 list. |

**TỔNG hygiene (pre): 73.5/100** — dưới 80. Trục dưới sàn: depth 4.5/8 < 4.8 ✗.

## KILL-LIST vi phạm
- Hero công thức + gradient + blob; badge emoji 🏆; medal emoji 🥇🥈🥉.
- Gradient overlay top-3 rows; me-badge gradient; title gradient text.
- Rank → **block-token + index mono** (quyết định #4: leaderboard rank bắt buộc); chart top-10 → vùng dữ liệu **luôn tối** (quyết định #5).
- Easing `ease`/450ms → chuẩn #6; EmptyState error thêm retry (trục 4d); copy §9 cho noClass (thêm CTA "Đi tới Lớp học").
- `font-weight` 800/700 → 600; H1 48px; value mono; tracking dương bỏ.
