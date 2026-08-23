# AUDIT — ProfileView.vue (`/profile`) — nhóm C

> Audit theo `standard.md` (10 trục hygiene + trục Đặc trưng tách riêng) + `frontend/DESIGN.md`. Ngày: 13/08/2026. Agent: dev-frontend (nhóm C).

## Câu hỏi bản sắc (BƯỚC D)

**"Xoá chữ/logo đi, nhìn thuần bố cục + màu + animation, có ai đoán được đây là app học cấu trúc dữ liệu không, hay giống demo dashboard bất kỳ?"**

→ **Dashboard bất kỳ.** Hero gradient aurora (teal→violet) + avatar tròn gradient + tên gradient-clip-text + 5 stat card đồng mức `font-weight: 800` + blob blur trang trí = công thức dashboard AI-gen chuẩn. Không một chi tiết nào nói "DSA". Điểm đặc trưng: 2/10.

## Điểm 10 trục (pre-fix)

| # | Trục | Điểm | Bằng chứng |
|---|---|---|---|
| 1 | Spacing/Grid | 7/8 | Gap lẻ: `.profile__identity gap: 4px` (dòng 455 — OK token), `.profile__stat-block gap: 2px` (486 — ngoài scale), `.profile__level-progress gap: 6px` (494), `.profile__topic-lessons gap: 6px` (537), `.profile__overview-progress gap: 4px` (508). Stats row 5 cột `repeat(5,1fr)` (475) không theo grid 12 cột §8. |
| 2 | Breakpoint | 4.5/6 | Stats row mobile → `repeat(2,1fr)` (583) = 5 card lẻ hàng; hero actions margin-left 0. Chưa verify 3 mốc bằng trình duyệt. |
| 3 | Animation | 9/14 | `hover-lift` (transform+shadow-lg 180ms `ease` — global.css 216) trên quick-link/topic/achievement: easing mặc định >150ms (KILL-LIST V2), shadow lên card (cấm §6). `.profile__quick-link transition 180ms ease` (518). Không khoảnh khắc đầu tư; không reduced-motion riêng (dựa global). |
| 4 | Nhất quán thị giác | 6.5/14 | Emoji icon: stat label `🔥 Streak/💎 Gems/❤️ Tim` (229–238), quick-link `🏆🛒👥` (84–87), achievement `🏅🔒` (359) — KILL-LIST icon emoji. Hero gradient + blob `filter: blur(64px)` (419–430) — KILL-LIST hero. Name gradient-clip-text (459–463), avatar gradient (443), streak chip `--gradient-sunset` (469). `font-weight: 800` (449, 492). Có đủ 3 trạng thái list (skeleton/empty) nhưng thiếu error state tab overview (fetchOverview fail → im lặng). |
| 5 | Interactive sizing | 14/16 | Mọi button qua `Button.vue` (grep `<button` = 0 trong view). Nút "Chỉnh sửa/Làm mới/Xuất CSV" sm h-9 36px — target ≥24 OK, là nút phụ. `profile__quick-link` = RouterLink padding 8/16 OK. Nút submit form `size="sm"` (380) — submit chính của form nên md. Nút kề: progress-actions gap 8px OK. |
| 6 | Typography | 7/10 | `font-weight: 800` avatar (449) + stat-value (492); `font-weight: 700` level-progress-label (496). H1 = tên user `text-2xl` (458) + gradient. panel-title `text-md` (18px — ngoài scale §3). Không tracking âm heading. |
| 7 | Depth & Elevation | 4/8 | Hero `box-shadow: var(--shadow-md)` (405) + 5 stat đồng nổi + hover-lift shadow-lg trên card — r2-fixed-12 "mọi thứ nổi bằng nhau". Không card-raised level-2, không hero-stat duy nhất. |
| 8 | A11y | 9/12 | Stat không phải control OK; avatar `aria-hidden` OK; button có text OK. Emoji label đọc máy lặp 🔥💎❤️. passwordError `role="alert"` OK. Contrast: gradient name trên surface (teal-900 on #F0FDFA) OK; stat-value 800 trên surface OK. |
| 9 | Code quality | 5/6 | `cssVar()` trùng LeaderboardView (110–115) — nên composable. `tab` ref + Tabs OK. onMounted `Promise.allSettled` OK. `v-for` key ổn định. |
| 10 | Performance | 6/6 | Route lazy (router 27), VChartLazy lazy-load echarts, không re-render thừa. |

**TỔNG hygiene (pre): 72/100** — dưới ngưỡng 80. Trục dưới sàn: không (sàn thấp nhất 3.6 — depth 4/8 = 4.8 sàn → 4/8 **DƯỚI SÀN 4.8** ✗).

## KILL-LIST vi phạm (phải sửa)
- Hero công thức: gradient aurora + blob blur + shadow → thay surface band level-2 (DESIGN.md §1, §6).
- Icon emoji: 🔥💎❤️🏆🛒👥🏅🔒 → lucide-vue-next (quyết định xuyên-nhóm #2).
- Stat-card công thức: 5 stat đồng nổi 800 → phân cấp 1 hero (XP, block-token) + stat phụ level-1 (quyết định #3, #4).
- Gradient trang trí: hero/avatar/name/streak-chip → bỏ; vùng dữ liệu luôn `canvas-ink` (quyết định #5).
- Easing `ease` 180ms (hover-lift + quick-link) → easing chuẩn enter/exit, micro 100–150ms (quyết định #6).
- `font-weight` 800/700 → 400/500/600 (§3).
- Card shadow → bỏ (chỉ dropdown/modal §6).
