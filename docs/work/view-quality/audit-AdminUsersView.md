# Audit — AdminUsersView.vue (`/admin/users`) — Nhóm D (admin)

> Ngày: 14/08/2026 · Agent: dev-frontend (nhóm D — admin) · Nguồn chuẩn: `standard.md` + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md` + 6 quyết định xuyên-nhóm.

## Câu trả lời trục Đặc trưng (đầu file — bắt buộc)

**Không nhận diện được app học CTDL — dashboard quản trị người dùng của bất kỳ SaaS/LMS nào.** Bằng chứng: hero gradient Aurora + blob blur + title gradient-clip (dòng 291–348), icon tròn gradient (325–336), avatar gradient + weight 800 (407–419), th uppercase + tracking dương (380–390), tên người dùng weight 700 (423), modal duyệt block uppercase + 700 (460–466), bảng scroll ngang mobile (min-width 720px + overflow-x, dòng 376–378). Không có dấu vết Data Bench nào: 0 block-token, 0 index mono, 0 vùng tối canvas-ink.

## Điểm trước sửa

| # | Trục | Điểm | Sàn | Bằng chứng vi phạm (dòng/selector) |
|---|---|---|---|---|
| 1 | Spacing/Grid | 6.5 | 4.8 | Phần lớn theo token ✓; `gap: 4px` hero-title-wrap (338) — 4px có trong scale, chấp nhận; `.admin-users__search` `padding-left: 34px` (366) — 34 không thuộc scale 4/8/12/16/24/32/48/64 |
| 2 | Breakpoint | 3.5 | 3.6 | Bảng `min-width: 720px` + `.table-scroll overflow-x: auto` (376–378) — **cấm scroll ngang bảng chính ở mobile** (DESIGN §8 → card-stack); hero wrap OK |
| 3 | Animation | 6.0 | 8.4 | `transition: background-color 150ms ease` (399) — ease mặc định đúng ≤150ms nhưng không chủ đích; không khoảnh khắc đầu tư; hero không animation |
| 4 | Nhất quán thị giác | 3.5 | 8.4 | Hero gradient Aurora + blob `::before` blur(64px) + `::after` overlay + `box-shadow: var(--shadow-md)` (291–336) — **KILL-LIST hero công thức**; title gradient-clip (340–346); icon/avatar gradient (325–336, 407–419); `.text-muted` legacy (195, 207, 244, 362, 425...) thay 4 tầng chuẩn; th `text-transform: uppercase` (383); `color-mix(primary 5%)` hover bảng (401) |
| 5 | Interactive sizing | 11.0 | 9.6 | Mọi button qua `Button.vue` ✓; **`.admin-users__actions` gap `var(--space-xs)` = 4px** (428) — nút liền kề (Duyệt/Từ chối, Khóa/Mở) cách 4px < 8px chuẩn; nút sm 36px ≥24 target ✓; search Button sm secondary + icon 14px (161–163) — icon button nên 16px |
| 6 | Typography | 4.0 | 6.0 | H1 `--text-2xl` 30px (341) ≠ 48px H1; `font-weight: 800` avatar (416); `font-weight: 700` name (423) + review-name (446) + review-info-title (462); th uppercase + tracking 0.05em (383–384) |
| 7 | Depth & Elevation | 3.0 | 4.8 | Hero `--shadow-md` (299); icon hero `--shadow-md` (335); avatar/icon gradient "nổi" đồng loạt; không phân cấp level-1/level-2 |
| 8 | A11y | 9.5 | 7.2 | Input/select có aria-label ✓; lock button aria-label động ✓; **th thiếu `scope="col"`**; EmptyState role=status ✓; modal Dialog focus ✓ |
| 9 | Code quality | 5.5 | 3.6 | Logic gọn, không timer; toast/error xử lý đầy đủ; `initial()` helper ok; không trùng logic |
| 10 | Performance | 5.5 | 3.6 | Không ảnh, không lazy cần thiết; nhẹ; `v-for` key id ✓ |
| **TỔNG** | | **58.0** | | |

**Đặc-trưng: 2/10** · **Kết luận: KHÔNG ĐẠT** (hygiene <80, đặc-trưng <7, trục breakpoint dưới sàn).

## Rà TỪNG button (trục 5 — bắt buộc)

| Button | Selector | Qua Button.vue? | Padding | Target | Ghi chú |
|---|---|---|---|---|---|
| Tìm | `.admin-users__search-btn` (161) | ✅ sm secondary | h-9 px-3 | 36px | gap 8px ✓; icon 14px → 16px |
| Duyệt (row) | (211) | ✅ sm secondary | h-9 px-3 | 36px | liền nút Từ chối, gap hiện 4px → 8px |
| Từ chối (row) | (214) | ✅ sm danger | h-9 px-3 | 36px | như trên |
| Khóa/Mở (row) | (219) | ✅ sm ghost | h-9 px-3 | 36px | ✓ |
| Modal: Hủy / Xác nhận | (269–276) | ✅ | h-10 px-4 | 40px | gap 8px ✓ |

## KILL-LIST bị vi phạm (phải sửa)

1. Hero gradient + blob + gradient-clip title (291–348) — **bỏ, thay surface band level-2** + mono strip block-token (quyết định #1/#4).
2. Avatar/icon tròn gradient + `#fff`-ish (`--color-on-primary`) — KILL-LIST "stat-card/icon tròn" kiểu.
3. Easing mặc định `ease` 150ms (399) — giữ 150ms (≤150ms OK) nhưng chỉ animate transform/opacity cho khoảnh khắc đầu tư.
4. th uppercase + tracking dương; weight 700/800.

## Hướng sửa (đã làm)

- Banner → surface band `bg-card-raised` + border-subtle + **mono strip block-token dữ liệu thật** (số chờ duyệt, block `data-core` + index mono, stagger-enter 280ms `cubic-bezier(0.16,1,0.3,1)` — khoảnh khắc đầu tư, reduced-motion đúng).
- Bảng → chuẩn §4.6: th `text-sm font-medium text-foreground-tertiary h-10`, bỏ uppercase; td 12px; hover `color-mix(muted 50%)`; `scope="col"`; **mobile card-stack** thay scroll ngang; cột ngày mono.
- Avatar/icon → neutral `bg-muted text-foreground-secondary`, weight ≤600; `.text-muted` → 4 tầng token.
- Actions gap 4px → 8px; icon button 14→16px.
- Thêm **error state + nút Thử lại** (trước: load fail → toast + EmptyState "Không có người dùng" gây hiểu nhầm).
- Empty state thêm description lời mời hành động (§9).
