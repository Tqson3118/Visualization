# AUDIT — PremiumView.vue (`/premium`) — nhóm C

> Audit theo `standard.md` + `frontend/DESIGN.md`. Ngày: 14/08/2026. Agent: dev-frontend (nhóm C).

## Câu hỏi bản sắc (BƯỚC D)

**"Xoá chữ/logo đi, nhìn thuần bố cục + màu + animation, có ai đoán được đây là app học cấu trúc dữ liệu không, hay giống demo dashboard bất kỳ?"**

→ **Trang pricing/SaaS bất kỳ.** Hero gradient aurora + blob blur + title gradient-clip, 3 plan card đồng nổi (shadow-md global `.card`), highlight card bằng gradient+shadow-lg+scale, bảng so sánh trắng trơn, icon tròn gradient. Không một chi tiết nào nói "đây là app học CTDL" (không block-token tối, không index mono, không Big-O). Điểm đặc trưng: **2/10**.

## Điểm 10 trục (pre-fix)

| # | Trục | Điểm | Bằng chứng |
|---|---|---|---|
| 1 | Spacing/Grid | 6/8 | `gap: 5px` qr-caption (594); `.premium__hero-title-wrap gap: 4px` (422 — token OK). Grid plans `auto-fit minmax(220px,1fr)` OK. Bảng compare `min-width: 420px` (486) → scroll ngang ở 390px (§8 cấm scroll ngang bảng chính). |
| 2 | Breakpoint | 4/6 | Media duy nhất `max-640px` hero-badge margin (649). Bảng so sánh min-width 420px tràn 390px (scroll ngang) — vi phạm §8 "bảng chính → card-stack mobile". |
| 3 | Animation | 9/14 | Không easing chuẩn: `hover-lift` (180ms ease — global.css 213–220) + `.premium__plan--highlight:hover transform` (459) — ease mặc định >150ms (KILL-LIST easing). Table row `transition background 150ms ease` (502) OK. Không "khoảnh khắc đầu tư" chủ đích (confetti success đã có qua fireConfetti). |
| 4 | Nhất quán thị giác | 5/14 | Hero gradient + blob + shadow (375–405) KILL-LIST; title gradient-clip (424–430); hero icon gradient (409–420); highlight card gradient `linear-gradient(180deg, aurora-soft, card)` + shadow-lg + border primary (451–459) KILL-LIST; `background: #fff` hex rời (578); emoji `✔` (270) + `❤✘✔` trong BENEFITS (47–53) + `🎉` i18n (599, 601); accent primary dùng trang trí (price 468, premium-col 506, countdown strong 638, qr-frame border 579); th uppercase + `letter-spacing: 0.04em` (495–500); icon trộn size 24/12/13/14/15; thiếu error state (fetchPremium lỗi im lặng — plans vẫn render nên chấp nhận, ghi notes). |
| 5 | Interactive sizing | 15/16 | MỌI nút qua Button.vue ✓; plan "Chọn gói" default md 40px ✓; modal ghost/default md ✓; copy sm 36px ≥24 target ✓ (helper, chấp nhận); nút liền kề gap 8px (642–647) ✓; icon+text gap Button base `gap-2` ✓. |
| 6 | Typography | 5/10 | H1 `text-2xl` (425) sai scale (phải 48px); `font-weight: 800` (467, 519, 527) + 700 (506) — cấm; giá không mono; th uppercase + tracking dương (499); success-icon `font-size: 2rem` ngoài scale (527). |
| 7 | Depth & Elevation | 2/8 | Hero shadow-md (383); icon chip shadow-md (419); highlight card shadow-lg + gradient (454–455); 4 card `.card` global shadow-md — mọi thứ nổi bằng nhau (§6 cấm shadow card). Không phân cấp 1 hero-stat. |
| 8 | A11y | 11/12 | Nút có text ✓; QR canvas `role="img"` + aria-label (303–304) ✓; countdown/success `role="status"` ✓; th `scope="col"` ✓; `window.confirm` replace (117) — native, chấp nhận (không đổi logic). |
| 9 | Code quality | 5/6 | Logic QR/countdown/atomic OK; `v-for` key plan.id OK; `setTimeout` redirect 2500ms (190) KHÔNG clear khi unmount — leak timer. |
| 10 | Performance | 6/6 | Route lazy; qrcode import tĩnh hợp lý. |

**TỔNG hygiene (pre): 68/100** — KHÔNG ĐẠT. Trục dưới sàn: thị-giác 5 < 8.4 · depth 2 < 4.8 · typography 5 < 6.0.

## KILL-LIST vi phạm
- Hero công thức: heading + blob gradient (blur 64px) + shadow — banner phải surface band level-2 (DESIGN.md §1/§6).
- Highlight plan: gradient + shadow-lg + scale (451–459) — thay bằng border+tint success (pattern quests__card--ready).
- Card đồng loạt shadow-md (`.card` global) — override `box-shadow: none` (decision log nhóm C #4).
- Emoji `✔❤✘🎉` — thay Check/X lucide (resolved/quaternary) + bỏ emoji i18n.
- Accent primary dùng trang trí (price/cột premium/countdown) — bỏ, chỉ CTA.
- Easing mặc định (hover-lift/scale) — thay transition border-color 150ms `cubic-bezier(0.16,1,0.3,1)`.
- `#fff` hex rời (578) — QR cần nền trắng để scan → decision log mới (functional).
- weight 800/700 → 600; H1 48px; bảng mobile → card-stack (data-label).

## Đặc trưng sau-fix (thiết kế)
- Hero: strip mono dữ liệu `1M · 3M · 12M` (từ PLANS) trên nền canvas-ink — dữ liệu tuần tự có chỉ số (signature "dữ liệu luôn được đánh số").
- Success: BlockToken tone resolved (PREMIUM + tên gói) — "block thở theo bước", khoảnh khắc đầu tư duy nhất + confetti.
- Bảng so sánh: Check/X lucide theo ngữ nghĩa resolved/quaternary (ngôn ngữ trạng thái thuật toán).
