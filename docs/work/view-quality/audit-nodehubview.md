# AUDIT — NodeHubView `/path/:topicId/node/:nodeId`

> Phase 2 BỔ SUNG audit (14/08/2026 · dev-frontend · worktree D:\FPT\neww-qp2, nhánh feature/view-quality-merge-check). Nguồn chấm: `standard.md` mục 2 + 3 + 5 + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md`. Bằng chứng = dòng code `frontend/src/views/NodeHubView.vue` (NH).

## Câu hỏi đặc trưng — "nhìn thuần bố cục + màu + animation, đoán được đây là app học CTDL không?"

**KHÔNG** — banner gradient sunset + blob + dark-overlay hack + icon gradient + shadow-lg khiến màn giống dashboard bất kỳ; 3 tab Lý thuyết/Luyện tập/Cheatsheet chung chung; không kicker mono, không index, không vùng dữ liệu tối. Điểm đặc trưng: **4/10** (có dấu vết breadcrumb "Lộ trình" + tên node nhưng chưa đủ mạnh).

## Điểm 10 trục (BEFORE)

| # | Trục | Điểm | Lý do chính |
|---|---|---|---|
| 1 | Spacing/Grid | 7.5/8 | Token OK; `gap: 2px` (NH 278) ngoài scale |
| 2 | Breakpoint | 5/6 | Media 640 có (NH 370); flex-wrap OK; title `clamp()` 4vw tự do (NH 284) |
| 3 | Animation | 9/14 | Motion hero `ease: 'easeOut'` (NH 127) — easing mặc định không chuẩn (KILL-LIST V2); panel transition `180ms ease` (NH 313-315) — ease mặc định >150ms bị cấm; không reduced-motion cục bộ (global có) |
| 4 | Nhất quán thị giác | 8.5/14 | **Gradient sunset hero** (NH 223) + **icon gradient** (NH 266) + blob (NH 232-243) + **dark-overlay hack** `.dark::after` rgba(4,47,46,0.72) (NH 247-253 — vá contrast thay vì sửa bề mặt); Badge `primary` trang trí (NH 137); title `color: #fff` + `text-shadow` (NH 285-287) |
| 5 | Interactive sizing | 13/16 | 0 `<button` raw; nhưng CTA chính "Mở mô phỏng" `size="sm"` (NH 143, 168) = 36px < 40px chuẩn; icon Play `:size="14"` (NH 144, 169) < 16px quy ước; `←` ký tự text (NH 193) |
| 6 | Typography | 7.5/10 | H1 `clamp(--text-2xl, 4vw, --text-3xl)` + #fff + text-shadow (NH 284-288) — không scale §3, không tracking âm; fallback-title `--text-md` (18px) ngoài scale H4 (NH 354) |
| 7 | Depth & Elevation | 5.5/8 | Hero `shadow-lg` (NH 225) + icon `shadow-md` (NH 272) — banner/icon cấm shadow (§6); không phân cấp luminance |
| 8 | A11y | 10.5/12 | Breadcrumb aria-label OK; không raw button; không lỗi focus trầm trọng; overlay hack làm chữ trắng nhưng phụ thuộc màu cứng |
| 9 | Code quality | 5.5/6 | Logic sạch (computed/onMounted try-catch); map TOPIC_NODE_LESSONS ổn định |
| 10 | Performance | 6/6 | Lazy route OK; Tabs shadcn; không ảnh |

**TỔNG hygiene = 78/100** · **Đặc trưng = 4/10** · **KHÔNG ĐẠT** (hygiene 78 < 80 ✗; đặc trưng 4 < 7 ✗). Không trục dưới sàn (thị-giác 8.5 ≥ 8.4; depth 5.5 ≥ 4.8) — chỉ fail tổng + đặc trưng.

## Lỗi + bằng chứng

### KILL-LIST (phải sửa)
1. **Banner gradient** — `background-image: var(--gradient-sunset)` (NH 223) + blob (NH 232) + `::after` overlay (NH 247-253).
2. **Easing mặc định** — `ease: 'easeOut'` (NH 127), panel `180ms ease` (NH 313-315).
3. **Shadow trên banner/icon** — `shadow-lg` (NH 225), `shadow-md` icon (NH 272).

### 10 trục
- **Spacing**: `gap: 2px` (NH 278).
- **Thị giác**: badge primary trang trí; icon gradient; title #fff + text-shadow; breadcrumb không mono.
- **Interactive**: CTA sm < 40px; icon 14px.
- **Typography**: clamp title ngoài scale; fallback-title text-md.
- **Depth**: không surface band level-2.

## Kiểm tra button
- `<button` raw: **0** — CTA + fallback CTA + back đều qua `Button.vue`. Tabs shadcn (TabsTrigger) OK.
- Lỗi sizing: CTA chính `size="sm"` (NH 143, 168) → phải `default` (md, h-10 = 40px).

---

## RE-AUDIT (AFTER — 14/08/2026, sau khi sửa)

**Điểm sau**: spacing 8 · breakpoint 5.5 · animation 12.5 · thị-giác 13 · interactive 15 · typography 9.5 · depth 7.5 · a11y 11.5 · code 5.5 · performance 6 → **TỔNG 94/100** · **Đặc trưng 8/10** · **ĐẠT** (hygiene ≥80, không trục dưới sàn, đặc trưng ≥7).

Sửa chính: (1) gradient sunset + blob + overlay hack → **surface band level-2** + **kicker mono** `NODE 04 · SORT.BUBBLE` (dữ liệu thật từ route/simKey — quyết định #1); (2) Motion `easeOut` → `[0.16, 1, 0.3, 1]` 280ms; panel transition → 200ms `cubic-bezier(0.16,1,0.3,1)` enter / `cubic-bezier(0.7,0,0.84,0)` exit; (3) icon gradient + shadow → ô vuông muted 44px + icon lucide 20px màu tertiary; (4) H1 `--text-4xl` 600 `-0.03em`; badge primary → muted; (5) CTA `size="sm"` → `default` + Play 16px; `←` → lucide ArrowLeft; (6) fallback-title `--text-xl` 600; (7) `gap: 2px` → `--space-xs`; (8) i18n: bỏ 📖 khỏi `fallbackTitle`, bỏ ▶ khỏi `fallbackCta`.
