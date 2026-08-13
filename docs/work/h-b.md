# H-B — Nâng cấp UI/UX 5 màn ADMIN (users/stats/settings/content/ladder)

- **Branch**: `feature/ux-h-b` (từ `dev`)
- **Ngày**: 2026-08-13
- **Phạm vi**: chỉ UI/UX — KHÔNG đổi contract API, logic store, `src/engines/**`
- **Files sửa**: 5 view admin + `src/i18n/vi.ts` (chỉ thêm section `admin.*`)

## Tóm tắt nâng cấp chung

Mọi view admin giờ theo chuẩn đợt G (Home/Profile/Leaderboard):

1. **Hero gradient Aurora soft** — icon tile gradient + title text-gradient + subtitle + Badge "Quản trị" (copy pattern LeaderboardView: overlay `color-mix(background 58%)` giữ contrast cả light/dark).
2. **AdminNav** giữ nguyên (đã chuẩn) đặt ngay dưới hero.
3. **Tabs shadcn** (wrapper legacy `ui/Tabs.vue` — badge số lượng) cho Users + Content.
4. **Skeleton loading + EmptyState** chuẩn; **hover states** (row hover, hover-lift card), **focus-visible ring** toàn cục.
5. **Responsive**: 0 overflow ngang @1366 + @390 (verify Playwright đo `scrollWidth`), table scroll-x, grid auto-fit.
6. **Dark mode**: toàn bộ dùng token (không hardcode màu) — verify bằng thêm `class="dark"`.
7. **Không đổi selector cũ**: `aria-label` Tìm người dùng/Lọc vai trò/Lọc trạng thái, `id="node-select"`, `id="exercise-select"` đều còn.

## Chi tiết từng view

| View | Nâng cấp chính |
|---|---|
| **AdminUsersView** | Hero Aurora + Tabs (badge chờ duyệt) + filter bar trong card (search icon absolute, 2 select, nút Tìm) + table: avatar vòng gradient (chữ cái đầu), row hover, Badge vai trò/trạng thái, nút Duyệt/Từ chối/Khóa có icon lucide + aria-label nút khóa; modal duyệt có avatar + tên/email rõ ràng. Giữ nguyên `load/switchTab/toggleLock/submitReview`. |
| **AdminStatsView** | Hero Aurora + **5 KPI** Card shadcn + hover-lift (thêm `totalSimulations` — DTO đã có, chưa hiển thị trước đây) với icon tile gradient (aurora/mint/sunset) + **ECharts bar chart 7 ngày** qua `VChartLazy` (màu đọc CSS var → đổi theme không reload) + donut SVG theme-aware (bỏ màu hardcode #5E7A77/#134E4A → var token) + legend chuẩn + note card icon Info. |
| **AdminSettingsView** | Hero Aurora + form chia **3 section** (Chung / Chính sách mật khẩu / Sandbox & Upload) có icon + divider; checkbox custom `accent-color` + label dễ click; error banner role=alert có icon; nút Lưu có icon Save + loading. Giữ nguyên `save()` + payload. |
| **AdminContentView** | Hero Aurora + Tabs badge (Bài học/Chủ đề theo số lượng thật) + toolbar (Thêm bài học + link Ladder ghost) + table: tiêu đề có desc ellipsis, Badge chủ đề/trạng thái, cột mô phỏng icon Network (số 0 xám), ngày có icon CalendarDays, nút Sửa/Xóa icon; topic tab → **grid Card shadcn** hover-lift kèm số bài học mỗi chủ đề (tính từ lessons — presentation only); modal form có id select + textarea mono. Giữ nguyên CRUD. |
| **AdminLadderView** | Hero Aurora + **info banner** (icon Info) thay cho đoạn note trần + node list: số node gradient, stage label, Badge đã gắn/trống có icon Check, selected state ring primary + tint; attach panel giữ `id` select + EmptyState + nút Gắn icon Link2. Giữ nguyên `attach()`. |

## Verify

- `npm run build` → **0 lỗi** (vue-tsc + vite build, 1.86s).
- `npm test` → **11 files / 89 tests PASS** (như baseline, không vỡ gì).
- Dev server + Playwright mock ADMIN (không cần backend): 5 view render, **0 lỗi console**, 0 overflow ngang @1366×768 và @390×844, dark mode OK.
- Ảnh chụp: `docs/work/shots/h-b/*.png` (users-light/dark, stats-light/dark, settings-light, content-light, ladder-light + users/content/ladder-mobile).

## Rủi ro / ghi chú

- `LessonSummary` **không có** `createdAt` → cột "Ngày tạo" giữ hành vi cũ (`new Date()`), chưa fix (ngoài phạm vi — cần backend bổ sung field).
- KPI "Mô phỏng" + biểu đồ 7 ngày/vai trò vẫn dựa dữ liệu minh họa (backend chỉ trả KPI tức thời) — note đã ghi rõ trên màn.
- Ảnh dark của AdminStats: màu chart ECharts lấy theo `ui.theme` (toggle nút theme, không phải class dark tay) — khi bật theme qua UI chart sẽ đổi đúng.
- Chưa làm: breadcrumb (hero title + AdminNav đủ theo chuẩn các màn G), chưa đụng AdminNav component.
