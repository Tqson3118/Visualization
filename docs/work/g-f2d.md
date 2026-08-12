# G-F2D — POLISH LEADERBOARD (MÀN 24) + PROFILE SKILL RADAR (MÀN 32)

- **Task**: Phase 2d — 2 màn chính nhóm 3 (BAO_CAO_SPEC §6.2): `/leaderboard` (Màn 24) + `/profile` (Màn 32, skill radar)
- **Branch**: `feature/ux-polish` (base commit `204fd0b` = G-F2a+F2b+F2c)
- **Ngày**: 12/08/2026 · **Người**: dev-ux (commit-as son)
- **Kết quả**: 2/2 màn DONE — build 0 lỗi · unit 72/72 PASS · e2e 11/11 PASS · screenshot 4/4 PASS

## Màn 24 — Leaderboard `/leaderboard` (DONE)

| Nâng cấp | Chi tiết |
|---|---|
| Hero gradient | Aurora (palette gamification — teal→cyan→violet) + icon trophy (lucide `Trophy`) + badge "Top learners", border glow, blur orb |
| 3 tab | Tabs shadcn (wrapper `Tabs.vue`) Tuần / Level / Lớp — giữ logic `fetchBoard` + phân trang |
| Chart top 10 | Bar chart vue-echarts (lazy) — màu rank vàng/bạc/đồng cho top 3, teal còn lại; tooltip + label vi-VN |
| Reorder hoạt hình | Vue-native `<TransitionGroup>` FLIP (`board-row-move` transform 0.45s cubic-bezier) — tương đương AutoAnimate/motion-v TransitionGroup (motion-v v2 KHÔNG export TransitionGroup); global.css đã cắt transition khi `prefers-reduced-motion` |
| Top 3 highlight | Gradient overlay mờ (Sunset/Aurora/Mint) + border trái màu, medal 🥇🥈🥉 |
| Dòng "Bạn" | Ghim cuối bảng, label "Vị trí của bạn" (aria-label + role=status), badge "Bạn", border primary; row trong bảng có `board-row--me` + badge nếu đúng user |
| Phân trang | `Button` shadcn Trước/Sau + "Trang X / Y" (`board.page`/`board.totalPages` bổ sung vào store — additive, không đổi rows/myRank) |
| Empty/Error | `EmptyState` target (rỗng) + alert-circle (lỗi) |
| Loading | Skeleton lần đầu; đổi tab giữ bảng cũ + mờ nhẹ (`--busy`) để reorder animation thấy được |

## Màn 32 — Profile `/profile` (DONE)

| Nâng cấp | Chi tiết |
|---|---|
| Hero profile card | Gradient Aurora, avatar lớn (initial), tên (giữ `h1` cho selector e2e), email, chips Lv / 🔥 streak / Premium, nút "Chỉnh sửa" (→ tab Cài đặt), stats row (Level/XP/Streak/Gems/Tim), progress bar tiến độ lộ trình (data thật `/progress/me`) |
| Skill radar | vue-echarts `RadarChart` (lazy — VChartLazy) — 5-6 trục = chủ đề `overview.topics` (Sắp xếp & TM / CTDL tuyến tính / Cây / Bảng băm / Đồ thị), value = `progressPct` THẬT — KHÔNG bịa; `topics` rỗng → `EmptyState` + ghi chú |
| 4 tab | Tabs shadcn Tổng quan / Tiến độ / Thành tích / Cài đặt — chức năng giữ nguyên |
| Thành tích | Card hover-lift, unlocked gradient success + Badge "Đã mở/Chưa mở", locked mờ |
| Tiến độ | Card hover-lift, ProgressBar variant theo pct, Badge điểm |
| Cài đặt | Giữ form đổi mật khẩu (không đổi logic) |

## VChartLazy (component mới)

- `src/components/ui/VChartLazy.vue` — lazy `defineAsyncComponent(() => import('vue-echarts'))` → echarts ở chunk riêng (`echarts-*.js` ~323KB gzip 110KB) KHÔNG tăng bundle chính (NFR-5). Đăng ký module `use([CanvasRenderer, BarChart, RadarChart, GridComponent, LegendComponent, TooltipComponent])`. `animation:false` khi `prefers-reduced-motion`. `autoresize`.

## Bug phát hiện & fix (quan trọng)

- vue-echarts `planUpdate` bỏ mọi top-level **array đổi độ dài** vào `replaceMerge` → echarts 6 lỗi `"color" is not valid component main type in "replaceMerge"` → **Unhandled error làm đứng cả component update** (TransitionGroup list bị treo dữ liệu cũ). Xảy ra khi đổi tab/phân trang làm 10→4 bar.
- Fix: **KHÔNG để top-level `color` array** — màu rank chuyển vào từng `data item.itemStyle.color`; radar bỏ `color` top-level (màu đã inline). Verify lại pagination + reorder hoạt động đúng.

## Ảnh verify (docs/work)

| Ảnh | Màn | Theme |
|---|---|---|
| `docs/work/g-f2d-01-leaderboard-light.png` | Leaderboard (chart + bảng + pinned Bạn + pager) | light |
| `docs/work/g-f2d-02-leaderboard-dark.png` | Leaderboard | dark |
| `docs/work/g-f2d-03-profile-light.png` | Profile (hero + skill radar + tabs) | light |
| `docs/work/g-f2d-04-profile-dark.png` | Profile | dark |

## Verify (bắt buộc)

- [x] `npm run build` 0 lỗi (vue-tsc -b + vite build)
- [x] `npm test` 72/72 PASS
- [x] `npx playwright test` 11/11 PASS (bộ chính thức; spec ảnh tạm đã xoá sau khi chụp)
- [x] Dev server + playwright: leaderboard + profile render light+dark, 0 JS error (pageerror), radar canvas render, reorder tab (top 1 đổi Nguyễn Minh Anh → Lê Thị Cẩm Tú), phân trang 1/2→2/2 (4 dòng + pinned), không overflow ngang

## Files đổi

- `frontend/src/views/LeaderboardView.vue` — rewrite UI (hero/tabs/chart/TransitionGroup/pinned/pager)
- `frontend/src/views/ProfileView.vue` — rewrite UI (hero/radar/tabs/achievements)
- `frontend/src/components/ui/VChartLazy.vue` — NEW (lazy echarts wrapper)
- `frontend/src/stores/leaderboard.ts` — + `page`/`totalPages` + tham số `nextPage` (additive)
- `frontend/tests/e2e/helpers/mockApi.ts` — mock `/leaderboard` (PagedResponse, thứ tự theo tab, 14 players) + enrich `/progress/me` topics cho radar

## Ghi chú

- XP/Level header vẫn hiển thị theo store gamification (hiện không có endpoint set level/xp) — KHÔNG bịa data, giữ nguyên như trước.
- Skill radar chỉ render khi `overview.topics.length > 1`; ngược lại EmptyState + ghi chú.
