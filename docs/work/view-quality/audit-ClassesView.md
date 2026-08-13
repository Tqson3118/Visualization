# Audit — ClassesView.vue (`/classes`) — Nhóm D

> Ngày: 14/08/2026 · Agent: dev-frontend (nhóm D — classes) · Nguồn chuẩn: `standard.md` + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md` (6 quyết định xuyên-nhóm).

## Câu trả lời trục Đặc trưng (đầu file — bắt buộc)

**Nhìn thuần bố cục + màu + animation: đây là trang "class management" của MỌI LMS/dashboard SaaS — KHÔNG đoán được là app học CTDL.** Bằng chứng: hero gradient cam→hồng (`--gradient-sunset`, dòng 217) + icon tròn gradient (dòng 282) + card trắng `hover-lift` bóng đổ (dòng 127 + global.css 215–227) + chip mã mời dùng `color-mix` primary. Duy nhất 1 dấu vết đặc trưng: chip mã mời `font-mono` (dòng 333) — quá nhỏ để nhận diện.

## Điểm trước sửa

| # | Trục | Điểm | Sàn | Bằng chứng vi phạm (dòng/selector) |
|---|---|---|---|---|
| 1 | Spacing/Grid | 5.0 | 4.8 | `gap: 2px` (291), `gap: 6px` (322, 334), `padding: 3px 10px` (337) — ngoài scale 4/8/12/16… |
| 2 | Breakpoint | 4.5 | 3.6 | Grid `minmax(280px,1fr)` ổn; hero giảm padding ≤640 (356) ✓; chưa test 390 — modal/actions wrap OK |
| 3 | Animation | 6.0 | 8.4 | Card `hover-lift` = translate + **shadow** 180ms **ease** (127 + global.css 216) — vừa cấm shadow trên card (DESIGN §6) vừa easing mặc định; không có "khoảnh khắc đầu tư" |
| 4 | Nhất quán thị giác | 5.0 | 8.4 | **KILL-LIST hero công thức**: gradient + `::after` overlay (222–232) + `text-shadow` (239) + `#fff` hex (218); icon tròn gradient (282–288) + `--shadow-sm`; `--gradient-sunset` trùng banner nhiều view; `--radius-xl` cho banner (216) thay vì surface band level-2 |
| 5 | Interactive sizing | 13.0 | 9.6 | 0 `<button` raw ✓ (toàn bộ qua `Button.vue`/buttonVariants). Lỗi nhỏ: card `role="button"` (128–133) thiếu phím **Space**; CTA hero `size="md"` (40px) — mobile cần 44px (`lg`) |
| 6 | Typography | 5.5 | 6.0 | H1 `var(--text-3xl)` 36px (237) ≠ 48px/-0.03em (DESIGN §3); card name là `h2` (137) nhưng 18px — lệch cấp heading (nên h3 + `text-lg font-semibold tracking-tight`); chip `font-weight: 700` (336) — cấm; `letter-spacing: 0.06em` dương trên heading chip — chỉ chấp nhận mono label ngắn |
| 7 | Depth & Elevation | 3.0 | 4.8 | Hero `box-shadow: var(--shadow-lg)` (219) — card level-2 CẤM shadow; card hover `--shadow-lg` (hover-lift); icon `--shadow-sm` (288); mọi card nổi bằng nhau — không phân cấp |
| 8 | A11y | 8.5 | 7.2 | Card `role="button"` + Enter (132) nhưng thiếu Space; `aria-label` card ✓; contrast chữ trắng trên gradient light ~4.5:1 (GP-T9b) — chấp nhận được nhưng thiết kế sai |
| 9 | Code quality | 5.0 | 3.6 | `v-for` key `cls.id` ổn định ✓; không timer; logic store gọn. Trừ nhẹ: `join()`/`createClass()` duplicate pattern (chấp nhận — 2 modal khác nhau) |
| 10 | Performance | 5.5 | 3.6 | Không ảnh/lazy cần thiết; import nhẹ; route lazy chung ✓ |
| **TỔNG** | | **61.0** | | |

**Đặc-trưng: 2/10** · **Kết luận: KHÔNG ĐẠT** (hygiene <80, đặc-trưng <7).

## Rà TỪNG button (trục 5 — bắt buộc)

| Button | Selector | Qua Button.vue? | Padding | Target | Ghi chú |
|---|---|---|---|---|---|
| Tạo lớp mới / Nhập mã lớp | `.classes__hero-actions` (99–104) | ✅ `size="md"` | h-10 px-4 (chuẩn) | 40px | Nên `lg` mobile (44px) |
| Hủy (modal join) | `.classes__modal-actions` (170) | ✅ ghost | h-10 px-4 | 40px | gap 8px ✓ |
| Tham gia (submit) | (171) | ✅ default | h-10 px-4 | 40px | disabled logic ✓ |
| Hủy / Tạo lớp (modal create) | (191–192) | ✅ | h-10 px-4 | 40px | gap 8px ✓ |
| Card lớp (clickable) | `.classes__card` (127–133) | ⚠️ role=button tự chế | p-6 | full card | thiếu Space handler |

## KILL-LIST bị vi phạm (phải sửa)

1. Hero công thức: heading + desc + CTA + gradient blob phía sau (dòng 92–106, 207–232).
2. Card đồng loạt nổi bằng nhau + shadow hover (hover-lift).
3. Icon stock gradient lung tung (classes__card-icon).
4. Easing mặc định `ease` >150ms? (180ms ease hover-lift — vi phạm chuẩn BƯỚC E).
5. `#fff` hex rời (218) + text-shadow.

## Hướng sửa (đã làm)

- Banner → surface band level-2 `bg-card-raised` + `border-b border-border-subtle`, KHÔNG gradient/shadow/radius; phải: mono strip block-token dữ liệu THẬT (số lớp + tổng thành viên, block + index mono) — quyết định xuyên-nhóm #1/#4/#5.
- Card lớp → level-1 (`bg-card` + `border-border` + `rounded-lg`), hover chỉ đổi border → `border-strong`, bỏ hover-lift/shadow; icon neutral `bg-muted text-foreground-secondary`; chip mã mời → block-token tối (`bg-canvas-ink` + mono + `text-resolved`).
- CTA hero `size="lg"`; card thêm phím Space; H1 48px/-0.03em; card name h3 `text-lg font-semibold tracking-tight`; bỏ 700.
- Animation: 1 khoảnh khắc duy nhất — strip blocks stagger-enter (transform+opacity, `cubic-bezier(0.16,1,0.3,1)` 200–300ms, delay 40ms/block); reduced-motion do global.css root đã lo.
