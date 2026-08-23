# AUDIT — PathView `/path/:topicId`

> Phase 1 audit TRƯỚC khi sửa (13/08/2026 · dev-engine). Nguồn chấm: `standard.md` mục 2 (10 trục) + mục 3 (Đặc trưng) + mục 5 (KILL-LIST). Bằng chứng = dòng code `frontend/src/views/PathView.vue` (gọi tắt PV) + `docs/work/r2-fixed-06-path*.png`.

## Câu hỏi đặc trưng — "Đoán được đây là app học CTDL hay dashboard bất kỳ?"

**KHÔNG đoán được** — thuần bố cục + màu + animation: đây là danh sách pill tròn Duolingo-style trên nền gradient aurora + emoji ⭐🔒🏁🎯, không có block-token/index mono nào, không có bất kỳ chi tiết nào nói "cấu trúc dữ liệu". Nếu che logo → dashboard gamified học từ vựng / streak app bất kỳ. Điểm đặc trưng: **2/10** (bằng chứng 4.6 PROMPT: path render card grid phẳng dù nội dung là graph).

## Điểm 10 trục (BEFORE)

| # | Trục | /8 | /6 | /14 | /14 | /16 | /10 | /8 | /12 | /6 | /6 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| | PathView | 6 | 4 | 7 | 5 | 8 | 5 | 5 | 9 | 5 | 5.5 |

**TỔNG hygiene = 59.5/100** · **Đặc trưng = 2/10** · **KHÔNG ĐẠT** (hygiene < 80; đặc trưng < 7; animation 7 < sàn 8.4; thị-giác 5 < sàn 8.4).

## Lỗi + bằng chứng

### KILL-LIST (phải sửa, độc lập điểm)
- **Gradient banner**: `.path-view__hero` `background-color: var(--aurora-soft)` + `::before` `background-image: var(--gradient-aurora)` (PV 296–310) — KILL-LIST mục 5 "Nút/CTA dùng gradient", DESIGN.md §1 "BỎ gradient trang trí ngẫu nhiên".
- **Emoji icon chức năng**: `🎯` trong H1 (PV 161), `🔒/⭐/▶` node icon (PV 206), `🏁` final test (PV 228), `❤` popover cost (PV 247), `starLabel` tạo `'⭐'.repeat()` (PV 152) — KILL-LIST mục 5 "Icon emoji", DESIGN.md §4 "lucide-vue-next duy nhất".
- **Shadow trên card**: hero `box-shadow: var(--shadow-md)` (PV 298), node active `box-shadow: var(--shadow-md)` (PV 377) — DESIGN.md §6 "card cấm shadow".
- **Easing mặc định**: `.pop-fade` `transition: opacity 200ms ease, transform 200ms ease` (PV 415) — KILL-LIST mục 5 bổ sung V2 (ease > 150ms), chuẩn: enter `cubic-bezier(0.16,1,0.3,1)` / exit `cubic-bezier(0.7,0,0.84,0)`.
- **hover-lift nâng khối + shadow**: node dùng `.hover-lift` (PV 199, 223) — global.css 215–227 `translateY(-3px) scale(1.02) + shadow-lg` — vi phạm §6 (hover card chỉ đổi border) + §7 Don't #3.

### 10 trục
1. **Spacing (6/8)**: `gap: 6px` progress (PV 329) không thuộc scale 4/8/12/16/24/32/48/64 (§5). Còn lại dùng `--space-*` đúng.
2. **Breakpoint (4/6)**: không media query nào; node pill không truncate label — nguy cơ tràn chữ 390px; popover `bottom: 10vh` + `min(360px, 100vw-32px)` OK.
3. **Animation (7/14)**: chỉ có popover fade+slide ease 200ms; node không có animation phản ứng trạng thái (signature "block thở theo bước" vắng mặt); hover-lift 180ms ease (vi phạm easing); không prefers-reduced-motion cục bộ.
4. **Thị giác (5/14)**: gradient banner (màu ngoài token); emoji 5 chỗ; font-weight 700 ở hero-kicker (PV 316), progress-label (PV 337), node-label (PV 386), popover-cost (PV 411) — §3 cấm 700 lung tung; `tracking: 0.1em / 0.06em` dương trên label không mono (PV 317, 341); Badge variant lẫn lộn (primary/success/muted — PV 211) đúng variant nhưng nền node pill cùng màu surface → không phân cấp.
5. **Interactive (8/16)**: 2 `<button>` raw (PV 192, 220) — node map (có thể là ngoại lệ canvas nhưng chưa ghi decision log); node pill `padding: var(--space-sm) var(--space-lg)` + font sm → chiều cao ~36px < 40px chuẩn nút chính (§4.1); nút liền kề gap 8px OK; popover dùng Button shadcn OK.
6. **Typography (5/10)**: 4 chỗ weight 700; title `text-2xl` (PV 322) — H1 nhỏ hơn scale §3 (text-4xl); kicker/label không mono nhưng tracking dương.
7. **Depth (5/8)**: hero + node-active shadow (vi phạm); popover shadow-xl (PV 406) — hợp lệ (popover = dropdown exception); không có hero-stat elevation level-2.
8. **A11y (9/12)**: node buttons có `aria-label` + disabled đúng; icon span aria-hidden đúng; popover không quản lý focus (Teleport — focus không kẹt, Esc không đóng); `title` attr trên final-test không đủ cho SR — thiếu aria-label.
9. **Code (5/6)**: watch + immediate đúng; localStorage try/catch đúng; `starLabel` computed trả hàm — OK; không listener rò rỉ.
10. **Performance (5.5/6)**: lazy route; không dep nặng; bundle PathView 7.46 kB.

## Kiểm tra button (theo standard.md trục 5)
- `<button` raw: **2** (PV 192 node, PV 220 final test) — chưa có decision log → vi phạm.
- Button.vue: popover Hủy/Bắt đầu dùng `<Button>` đúng; kích thước sm 36px — chấp nhận (compact) nhưng hit target ≥24×24 OK.
- Chữ chạm viền: node-label `font-weight: 700` + badge trong pill — badge có padding chuẩn, không chạm.

## Ghi chú
- Đã có `@vue-flow/core@1.48.2` (decision log Phase 0, bundle entry không đổi). Node-edge graph là hướng sửa chính (r2-fixed-06).

---

## RE-AUDIT (AFTER — 13/08/2026, sau khi sửa)

**Điểm sau**: spacing 7.5 · breakpoint 5 · animation 11.5 · thị-giác 11.5 · interactive 12 · typography 8.5 · depth 6.5 · a11y 10.5 · code 5 · performance 5.5 → **TỔNG 86/100** · **Đặc trưng 8.5/10** · **ĐẠT** (đủ 3 điều kiện).

Sửa chính: (1) graph node-edge VueFlow lazy (PathGraph + PathFlowNode block-token — signature index mono, chip trạng thái, icon lucide; entry bundle 106.59 kB không đổi, vue-flow trong lazy chunk 154.55 kB); (2) banner surface band level-2 bỏ gradient/shadow; (3) 5 chỗ emoji → lucide; (4) 2 raw button → role="button" trong custom node (canvas exception — decision log) + keyboard Enter/Space; (5) 700 → 600/500; (6) popover easing chuẩn + reduced-motion; (7) bỏ hover-lift. Runtime verify: 0 console error/warn, 0 overflow 390px, click+Enter mở popover, light/dark đúng token.