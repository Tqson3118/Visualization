# AUDIT — SubscriptionView.vue (`/account/subscription`) — nhóm C

> Audit theo `standard.md` + `frontend/DESIGN.md`. Ngày: 14/08/2026. Agent: dev-frontend (nhóm C).

## Câu hỏi bản sắc (BƯỚC D)

**"Xoá chữ/logo đi, nhìn thuần bố cục + màu + animation, có ai đoán được đây là app học cấu trúc dữ liệu không, hay giống demo dashboard bất kỳ?"**

→ **Trang quản lý subscription SaaS bất kỳ.** Hero gradient aurora + blob + title gradient, 2 card trắng đồng nổi, chip days-left primary tint. Không chi tiết DSA (không block-token, không index mono). Điểm đặc trưng: **2/10**.

## Điểm 10 trục (pre-fix)

| # | Trục | Điểm | Bằng chứng |
|---|---|---|---|
| 1 | Spacing/Grid | 6/8 | `gap: 6px` status-label (263), confirm-loss (308) — lẻ ngoài scale; days-left `padding: 4px 14px` (280) — 14px lẻ (12/16); title-wrap gap 4px OK (228). |
| 2 | Breakpoint | 5/6 | Media duy nhất `max-640px` hero-badge (316); benefits grid minmax(220px) → 1 cột OK; actions wrap OK. |
| 3 | Animation | 10/14 | Không animation tuỳ biến, không khoảnh khắc đầu tư; không vi phạm easing (không hover-lift). Thiếu 1 khoảnh khắc chủ đích. |
| 4 | Nhất quán thị giác | 5/14 | Hero gradient + blob + shadow (181–211); title gradient-clip (230–236); icon chip gradient (215–226); chip days-left `bg primary 10%` + border primary (277–279) — accent dùng trang trí (§2.1 cấm); emoji `❤` trong BENEFITS (28–32) + LOSES (54–59); icon 14/15 trộn; **thiếu error state**: store `fetchPremium` nuốt lỗi (gamification.ts 89–95) → lỗi API hiện empty state "Bạn chưa có gói Premium" (nói dối người dùng). |
| 5 | Interactive sizing | 15.5/16 | MỌI nút qua Button.vue ✓; renew/cancel default md 40px ✓; modal footer keep/confirm md ✓ gap DialogFooter space-x-2 = 8px ✓. |
| 6 | Typography | 5.5/10 | H1 `text-2xl` (231) sai scale (48px); `font-weight: 700` status-value (268) + 800 days-left (275); days-left không mono (dữ liệu "còn X ngày"). |
| 7 | Depth & Elevation | 3/8 | Hero shadow-md (189); status + benefits card `.card` shadow-md — không phân cấp; không hero-stat (chỉ chip phụ). |
| 8 | A11y | 11/12 | Nút có text ✓; loading `aria-busy` (87) ✓; days-left `role="status"` (126) ✓; EmptyState icon/action ✓. |
| 9 | Code quality | 5/6 | Logic cancel đơn giản OK; `v-for` key ổn định; `daysLeft` computed OK; nhưng phụ thuộc store nuốt lỗi → không phân biệt được error/empty (trục 4). |
| 10 | Performance | 6/6 | Route lazy. |

**TỔNG hygiene (pre): 72/100** — KHÔNG ĐẠT. Trục dưới sàn: thị-giác 5 < 8.4 · depth 3 < 4.8.

## KILL-LIST vi phạm
- Hero công thức gradient + blob + shadow → surface band level-2 (DESIGN.md §1/§6).
- Accent primary làm nền chip trang trí (days-left) → bỏ; days-left = dữ liệu có chỉ số → **BlockToken resolved** (hero-stat 1/màn, quyết định #5).
- Emoji `❤` (BENEFITS/LOSES) → bỏ, đưa chuỗi vào i18n không emoji.
- Card đồng loạt shadow-md (`.card` global) → override `box-shadow: none`.
- weight 700/800 → 600; H1 48px; ngày hết hạn → mono.
- Error state: gọi API trực tiếp (pattern ShopView) + EmptyState retry.

## Đặc trưng sau-fix (thiết kế)
- Hero-stat BlockToken resolved "CÒN LẠI {n} NGÀY" (vùng dữ liệu luôn tối, index mono) — 1 hero-stat/màn + khoảnh khắc đầu tư (enter 300ms easing chuẩn).
- Giá trị hết hạn mono tabular; trạng thái gia hạn = Badge success.
