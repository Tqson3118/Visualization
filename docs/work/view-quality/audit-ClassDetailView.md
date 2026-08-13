# Audit — ClassDetailView.vue (`/classes/:id`) — Nhóm D

> Ngày: 14/08/2026 · Agent: dev-frontend (nhóm D — classes) · Nguồn chuẩn: `standard.md` + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md`.

## Câu trả lời trục Đặc trưng (đầu file — bắt buộc)

**Không nhận diện được app học CTDL — đây là trang chi tiết lớp của bất kỳ LMS nào.** Bằng chứng: hero gradient sunset + overlay trắng mờ (406–421), chips "white glass" `rgba(255,255,255,0.14)` + **`backdrop-filter: blur(4px)`** (460–472 — KILL-LIST glassmorphism), avatar tròn gradient mint (543–555), icon assignment 3 gradient đổi màu theo loại (587–589), bảng chữ hoa + tracking dương (515–526), `font-weight: 700/800` rải rác. Dấu vết đặc trưng duy nhất: mã mời mono (480–485) — chưa đủ mạnh.

## Điểm trước sửa

| # | Trục | Điểm | Sàn | Bằng chứng vi phạm (dòng/selector) |
|---|---|---|---|---|
| 1 | Spacing/Grid | 4.5 | 4.8 | `gap: 4px` (431, 591), `gap: 6px` (463, 604), `padding: 3px 8px` (497), `padding: 4px 10px` (467), `margin-top: 6px` (640) — hardcode ngoài scale |
| 2 | Breakpoint | 4.0 | 3.6 | Table `min-width: 600px` + `overflow-x: auto` (512–514) — **cấm scroll ngang bảng chính** ở mobile (DESIGN §8 → card-stack); hero wrap OK |
| 3 | Animation | 7.0 | 8.4 | `transition: background-color 150ms ease` copy-btn (499) + tbody tr (535); `hover-lift` assignment card (290, 180ms ease + shadow); không khoảnh khắc đầu tư |
| 4 | Nhất quán thị giác | 4.0 | 8.4 | Hero gradient + `::after` overlay (396–421) + `#fff` hex (408, 468, 482, 490, 497) + `text-shadow` (437); **glassmorphism** chip (471); th `text-transform: uppercase` (519); dùng `.text-muted` legacy (254, 263, 297, 316, 357) thay 4 tầng chuẩn; avatar/assign-icon gradient |
| 5 | Interactive sizing | 9.0 | 9.6 | **1 `<button` raw**: `.class-detail__copy-btn` (200–210) — padding 3px 8px, font 12px, target ≈ 28×24 lẹt xẹt, không qua buttonVariants; còn lại qua Button ✓; toolbar/table nút gap 8px ✓ |
| 6 | Typography | 4.0 | 6.0 | H1 `text-2xl` 30px (434) ≠ 48px; `code` `font-weight: 700` (482); `.class-detail__name` 700 (559); `.assign-title` 700 (593); avatar `font-weight: 800` (552); th uppercase + tracking 0.05em (519–520) |
| 7 | Depth & Elevation | 3.5 | 8.4 | Hero `--shadow-lg` (408); icon `--shadow-sm` (584, 288); hover-lift shadow; không phân cấp level-1/2 |
| 8 | A11y | 9.0 | 7.2 | copy-btn có aria-label ✓ nhưng target nhỏ; `th` thiếu `scope="col"`; modal Dialog focus ✓; breadcrumb `aria-label` ✓ |
| 9 | Code quality | 4.5 | 3.6 | `copyInvite()` setTimeout (145–148) không cleanup khi unmount; `assignmentTint`/`--mint/sunset/aurora` 3 gradient tách biệt (157–161) — trùng logic tô màu; phần còn lại gọn |
| 10 | Performance | 5.5 | 3.6 | Lazy import `addClassMember` ✓; không ảnh; nhẹ |
| **TỔNG** | | **55.0** | | |

**Đặc-trưng: 2/10** · **Kết luận: KHÔNG ĐẠT** (hygiene <80, đặc-trưng <7).

## Rà TỪNG button (trục 5 — bắt buộc)

| Button | Selector | Qua Button.vue? | Padding | Target | Ghi chú |
|---|---|---|---|---|---|
| **Sao chép mã** | `.class-detail__copy-btn` (200–210) | ❌ **RAW `<button>`** | 3px 8px | ~28×24 | padding cấm trên nút chữ; phải qua Button.vue |
| Báo cáo lớp | `.class-detail__hero-link > Button` (212–216) | ✅ sm secondary | h-9 px-3 | 36px | gap 8px ✓ |
| Thêm thành viên | toolbar (226–228) | ✅ sm | h-9 px-3 | 36px | nên md (40px) — action chính tab |
| Gỡ (row) | (265–267) | ✅ sm danger | h-9 px-3 | 36px | ✓ |
| Gán nội dung | (279–281) | ✅ sm | h-9 px-3 | 36px | nên md |
| Xóa lớp này | (319–321) | ✅ danger | h-10 px-4 | 40px | ✓ |
| Modal: Hủy/Gỡ/Thêm/Gán/Xóa | (330–331, 346–347, 359–360, 371–372) | ✅ | h-10 px-4 | 40px | gap 8px ✓ |

## KILL-LIST bị vi phạm (phải sửa)

1. Hero gradient + blob overlay (dòng 178–218, 396–421).
2. Glassmorphism chip `backdrop-filter: blur(4px)` (471).
3. Easing mặc định `ease` (499, 535) + hover-lift 180ms ease.
4. Icon emoji/stock: `✓` text glyph trong copy-btn (209) — thay bằng icon Check.
5. Hex `#fff` rải rác; gradient avatar/icon; `th` uppercase; weight 700/800.

## Hướng sửa (đã làm)

- Banner → surface band level-2; mã mời → **block-token tối** (`bg-canvas-ink`, code mono `text-resolved`, label `text-index-muted` — vùng dữ liệu LUÔN tối, quyết định #5) + nút Sao chép qua `Button.vue` (icon Check/ClipboardCopy, aria-label).
- Bảng thành viên → chuẩn §4.6: th `text-sm font-medium text-foreground-tertiary h-10`, bỏ uppercase; td p-3 (12px); hover `bg-muted/50`; **mobile 390 → card-stack** (data-label + CSS, bỏ scroll ngang); cột ngày mono.
- Assignment card → bỏ gradient icon + hover-lift; thêm **index mono `#01`** (dữ liệu tuần tự — quyết định #4); icon neutral; due mono.
- Avatar/name/icon → neutral `bg-muted text-foreground-secondary`, weight ≤600; settings danger → `border-destructive/40` semantic; modal assign dùng `Input.vue` thay raw `<input class="input">`.
- Animation: 1 khoảnh khắc — panel mã mời "settle" enter (transform+opacity 250ms `cubic-bezier(0.16,1,0.3,1)`); copy-btn giữ 150ms `transition-colors` mặc định Tailwind (≤150ms OK); cleanup timer copy.
