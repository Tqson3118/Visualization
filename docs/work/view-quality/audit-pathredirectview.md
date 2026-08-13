# AUDIT — PathRedirectView `/path` (redirect từ `/learn`)

> Phase 1 audit TRƯỚC khi sửa (13/08/2026 · dev-engine · task P1-B3). Nguồn chấm: `standard.md` mục 2 + 3 + 5 + `frontend/DESIGN.md`. Bằng chứng = dòng code `frontend/src/views/PathRedirectView.vue` (PR). Ghi chú: view này là **topic selector** (5 chủ đề → `/path/:topicId`), không phải redirect thuần — giữ nguyên chức năng.

## Câu hỏi đặc trưng — "nhìn thuần bố cục + màu + animation, đoán được đây là app học CTDL không?"

**KHÔNG** — header phẳng + 🎯 emoji + grid card trắng với vòng tròn số primary + ProgressBar = công thức app học bất kỳ (Duolingo-like), không có dấu vết Data Bench: không kicker mono, không index mono, không block-token, không surface band. Điểm đặc trưng: **4/10**.

## Điểm 10 trục (BEFORE)

| # | Trục | Điểm | Lý do chính |
|---|---|---|---|
| 1 | Spacing/Grid | 8/8 | Toàn token `--space-*`; margin-top 4px = `--space-xs` hợp lệ; grid `minmax(300px,1fr)` + gap md OK; internal (card gap sm) < external (view gap lg) OK |
| 2 | Breakpoint | 4.5/6 | Grid auto-fill 300px: lý thuyết không tràn 390 (container 358px → 1 cột 300px+) — chưa test runtime; skeleton 5×88px OK |
| 3 | Animation | 9/14 | Không animation cơ giới, nhưng **không khoảnh khắc đầu tư nào** (view load tĩnh); skeleton pulse từ component OK; không cần reduced-motion (không có chuyển động riêng) |
| 4 | Nhất quán thị giác | 6.5/14 | **🎯 emoji** (PR 66) — KILL-LIST emoji; **vòng tròn index `background: var(--color-primary)`** (PR 137-149) — accent dùng trang trí thụ động (accent chỉ interactive); **header phẳng không phải surface band** (PR 65-70) — lệch quyết định banner xuyên-nhóm; không kicker mono; card dùng `.card`/`.card--interactive` (global.css 85-101: **shadow-md + hover shadow-lg/translateY/scale** — vi phạm §6); EmptyState icon="map" **không tồn tại trong SVG_PATHS** → fallback x-circle (bug âm thầm, emojiParser.ts:38 default) |
| 5 | Interactive sizing | 11.5/16 | `<button` raw = 0 OK; nhưng card dùng `<article role="button">` tự chế — keyboard chỉ Enter, **thiếu Space** (PR 91); không focus-visible riêng (nhờ global :focus-visible); clickable qua RouterLink chuẩn hơn |
| 6 | Typography | 7/10 | **weight 800 trên index** (PR 146) — CẤM 700+; H1 text-2xl (PR 119) — nhỏ hơn H3 chuẩn của nhóm (text-3xl/600/-0.02em, decision log nhóm B mục 5) + thiếu tracking âm; card title text-md (PR 151) — card title chuẩn text-lg/600 |
| 7 | Depth & Elevation | 6.5/8 | Card `.card` có shadow-md (global.css 90) + hover shadow-lg — card cấm shadow (§6); vòng tròn primary = "pop" không đúng phân cấp; không hero-stat (view nhỏ, chấp nhận) |
| 8 | A11y | 9/12 | role="button" article: Enter OK, **Space thiếu** (WCAG 2.1.1), không aria-label (tên từ text con — OK); card title/desc đọc được; ProgressBar show-label OK; empty state component OK |
| 9 | Code quality | 5.5/6 | onMounted fetch + fallback LOCAL_TOPICS rõ; `displayTopics` computed OK; v-for key = topic.id ổn định; không listener cần gỡ |
| 10 | Performance | 5/6 | Lazy route (router index 12); view nhỏ; 5 skeleton khi load OK |

**TỔNG hygiene = 72.5/100** · **Đặc trưng = 4/10** · **KHÔNG ĐẠT** (hygiene 72.5 < 80; thị-giác 6.5 < sàn 8.4; đặc trưng 4 < 7).

## Lỗi + bằng chứng

### KILL-LIST (phải sửa)
1. **Emoji 🎯** (PR 66) → lucide `Route` (nhất quán PathView).
2. **Card shadow + hover lift** qua `.card--interactive` (global.css 94-101) — "card nổi bằng nhau" + hover shadow.
3. **Accent trang trí**: vòng tròn index bg primary (PR 141).
4. **Microcopy**: title/sub OK nhưng không có kicker/nhịp Data Bench; note fallback "* Hiển thị dữ liệu mẫu..." (PR 103-105) — giữ nhưng chuẩn hoá (thông tin hữu ích).

### 10 trục
- Typography: weight 800 (PR 146); H1 text-2xl → text-3xl + tracking âm; card title → text-lg/600.
- A11y: thiếu Space trên card keyboard; chuyển RouterLink (decision log mục 6) — Enter/Space/focus native.
- Depth: bỏ `.card` shadow → card level-1 (bg-card + border-border + rounded-lg).
- Thị-giác: header → surface band level-2 + kicker mono (decision log mục 7); index → mono index-muted "TOPIC 01/05" (bỏ vòng tròn primary); EmptyState icon map (không tồn tại) → book (decision log mục 8).

## Kiểm tra button
- `<button` raw: **0** — sạch.

## Ghi chú phạm vi
- Giữ NGUYÊN: logic fetch topics + fallback + progressStore.fetchOverview + route/name. Bỏ openTopic() (RouterLink thay thế — decision log mục 6). Skeleton khớp kích thước card thật (trục 4d). Thêm stagger enter cho card grid (decision log mục 9).

---

## RE-AUDIT (AFTER — 13/08/2026, sau khi sửa)

**Điểm sau**: spacing 8 · breakpoint 5 · animation 11 · thị-giác 12 · interactive 13.5 · typography 8.5 · depth 7 · a11y 10.5 · code 5.5 · performance 5 → **TỔNG 86/100** · **Đặc trưng 8/10** · **ĐẠT** (hygiene ≥80, không trục dưới sàn, đặc trưng ≥7).

Sửa chính: (1) header → surface band level-2 + kicker mono `LEARNING PATH · TOPIC 01/05` + H1 text-3xl/600/-0.02em + Route icon (bỏ 🎯); (2) card `<article role="button">` → RouterLink (native Enter/Space/focus); (3) bỏ `.card/.card--interactive` → card level-1 + hover border-color + focus-visible ring (global có sẵn); (4) vòng tròn primary/800 → kicker mono index-muted `TOPIC 01/05` trong card; (5) EmptyState icon "map" (không tồn tại → x-circle) → "book"; (6) skeleton 88px → 150px khớp card thật; (7) card grid stagger enter 240ms easing chuẩn + prefers-reduced-motion; (8) card title text-lg/600 tracking âm; (9) note fallback chuẩn hoá text-tertiary xs.
