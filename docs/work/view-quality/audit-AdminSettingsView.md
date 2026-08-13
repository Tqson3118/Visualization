# Audit — AdminSettingsView.vue (`/admin/settings`) — Nhóm D (admin)

> Ngày: 14/08/2026 · Agent: dev-frontend (nhóm D — admin) · Nguồn chuẩn: `standard.md` + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md` + 6 quyết định xuyên-nhóm.

## Câu trả lời trục Đặc trưng (đầu file — bắt buộc)

**Không nhận diện được app học CTDL — form cấu hình admin của bất kỳ hệ thống nào.** Bằng chứng: hero gradient Aurora + blob + title gradient-clip (164–221), section title màu `--color-primary` (234–240 — accent dùng trang trí thụ động), checkbox native 16px, error alert color-mix. Không dấu vết Data Bench.

## Điểm trước sửa

| # | Trục | Điểm | Sàn | Bằng chứng vi phạm (dòng/selector) |
|---|---|---|---|---|
| 1 | Spacing/Grid | 7.5 | 4.8 | Toàn token ✓; `gap: 4px` (211) trong scale; row 2 cột → 1 cột mobile ✓ |
| 2 | Breakpoint | 5.0 | 3.6 | Form max-width 760px + row collapse 640 ✓; không lỗi lớn |
| 3 | Animation | 6.0 | 8.4 | Không animation tùy chỉnh (nút loading spinner shadcn ✓); không khoảnh khắc đầu tư — chấp nhận cho form admin |
| 4 | Nhất quán thị giác | 4.5 | 8.4 | Hero gradient + blob + gradient-clip (164–221) — **KILL-LIST**; section title `color: var(--color-primary)` (239) — **accent trang trí thụ động** (DESIGN §7 Don't 1); error alert `--color-surface` legacy + color-mix (248–249); `.text-muted`? không — dùng `--color-text-muted` (221) |
| 5 | Interactive sizing | 14.5 | 9.6 | Nút Lưu qua Button.vue md ✓; checkbox 16px + label wrap = target đủ ✓; icon Save 15px → 16px chuẩn |
| 6 | Typography | 5.5 | 6.0 | H1 30px (214) ≠ 48px; section-title `--text-md` (18px, 237) — nên text-lg 600 tracking âm; còn lại scale ✓ |
| 7 | Depth & Elevation | 4.5 | 4.8 | Hero `--shadow-md` (172); icon hero shadow (208); form `.card` legacy có `box-shadow: var(--shadow-md)` (global.css 91–93) — **shadow trên card** KILL-LIST |
| 8 | A11y | 9.5 | 7.2 | error role=alert ✓; fieldset+legend visually-hidden ✓; label for qua Input.vue ✓; checkbox label click ✓ |
| 9 | Code quality | 5.5 | 3.6 | Logic gọn; `domainsText` split ✓; **error state load không có nút retry** (mặc định hiển thị form + toast mặc định — hiển thị mặc định gây nhầm "đã lưu cấu hình thật") |
| 10 | Performance | 5.5 | 3.6 | Nhẹ, không ảnh |
| **TỔNG** | | **68.0** | | |

**Đặc-trưng: 2/10** · **Kết luận: KHÔNG ĐẠT** (hygiene <80, đặc-trưng <7).

## Rà TỪNG button (trục 5 — bắt buộc)

| Button | Selector | Qua Button.vue? | Padding | Target | Ghi chú |
|---|---|---|---|---|---|
| Lưu cấu hình | `.admin-settings__save` (146) | ✅ md default | h-10 px-4 | 40px | icon 15px → 16px; justify-end ✓ |

## KILL-LIST bị vi phạm (phải sửa)

1. Hero gradient + blob + gradient-clip (164–221).
2. Section title màu accent (239) — accent chỉ interactive.
3. Card `.card` legacy shadow (global.css) — thay panel token không shadow.
4. Không hex rời — ✓ (dùng color-mix + token), không vi phạm mục này.

## Hướng sửa (đã làm)

- Banner → surface band level-2 (không strip — admin form, giữ tối giản).
- Section title → `text-lg font-semibold tracking-[-0.015em]` + icon neutral (bỏ màu primary).
- Form `.card` legacy shadow → panel token `bg-card border-border rounded-lg` không shadow.
- Error alert → token semantic `destructive` + **nút Thử lại** (load() tách hàm — trước: onMounted inline, lỗi tải hiện form mặc định gây nhầm).
- Icon Save 16px; `--color-text-muted` → `--foreground-secondary`.
