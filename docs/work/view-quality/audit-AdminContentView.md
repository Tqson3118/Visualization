# Audit — AdminContentView.vue (`/admin/content`) — Nhóm D (admin)

> Ngày: 14/08/2026 · Agent: dev-frontend (nhóm D — admin) · Nguồn chuẩn: `standard.md` + `frontend/DESIGN.md` + `DESIGN-IDENTITY.md` + 6 quyết định xuyên-nhóm.

## Câu trả lời trục Đặc trưng (đầu file — bắt buộc)

**Không nhận diện được app học CTDL — CMS quản lý bài viết của bất kỳ nền tảng học trực tuyến nào.** Bằng chứng: hero gradient Aurora + blob + gradient-clip (368–427), icon topic gradient mint + `#fff` + shadow-sm (479–490), `hover-lift` topic card (291), th uppercase (440–450), title-text weight 700 (462), **cột "Ngày tạo" hiển thị `formatDate(new Date())` — ngày hôm nay, KHÔNG phải dữ liệu thật (bug)** (255). Không dấu vết Data Bench.

## Điểm trước sửa

| # | Trục | Điểm | Sàn | Bằng chứng vi phạm (dòng/selector) |
|---|---|---|---|---|
| 1 | Spacing/Grid | 7.0 | 4.8 | Token ✓; `gap: 4px` sim-count/date (466, 470) trong scale; topic grid auto-fit ✓ |
| 2 | Breakpoint | 3.5 | 3.6 | Bảng `min-width: 760px` + overflow-x (436–438) — **cấm scroll ngang mobile** (§8 → card-stack); toolbar wrap ✓ |
| 3 | Animation | 5.5 | 8.4 | `hover-lift` topic card (291) 180ms ease + shadow — cơ giới; `tr { transition: background-color 150ms ease }` (454); không khoảnh khắc đầu tư |
| 4 | Nhất quán thị giác | 4.0 | 8.4 | Hero gradient + blob + gradient-clip (368–427) — **KILL-LIST**; topic icon gradient mint (479–490); `.text-muted` legacy (240, 254, 425, 464, 468); th uppercase (443); EmptyState icon="book" ✓ |
| 5 | Interactive sizing | 13.0 | 9.6 | Mọi button qua Button.vue ✓; **`.admin-content__actions` gap `var(--space-xs)` = 4px** (472) — nút Sửa/Xóa cách 4px < 8px; toolbar nút addLesson `sm` (36px) — action chính nên `md` (40px); icon 14px → 16px |
| 6 | Typography | 4.5 | 6.0 | H1 30px (418) ≠ 48px; title-text 700 (462); topic-name `--text-md` 18px (494) → nên text-lg 600 tracking; th uppercase + tracking 0.05em (443–444) |
| 7 | Depth & Elevation | 3.5 | 4.8 | Hero + icon `--shadow-md` (375, 413); topic icon `--shadow-sm` (489); hover-lift shadow; `.card` legacy shadow (223, 434) — KILL-LIST shadow trên card |
| 8 | A11y | 8.5 | 7.2 | th thiếu `scope="col"`; modal select có label ✓; delete dùng `window.confirm` (161) — native confirm OK nhưng trải nghiệm kém (không phải lỗi chặn) |
| 9 | Code quality | 4.5 | 3.6 | **Bug: `formatDate(new Date())` thay vì dữ liệu thật** (255) — LessonSummary không có createdAt → cột hiển thị ngày giả; `topics.value[0]?.id ?? 1` fallback ok; Promise.all + catch riêng topics ✓ |
| 10 | Performance | 5.5 | 3.6 | CATALOG import tĩnh (27) — `@/engines/catalog` vào chunk view (đã lazy route); nhẹ |
| **TỔNG** | | **59.5** | | |

**Đặc-trưng: 2/10** · **Kết luận: KHÔNG ĐẠT** (hygiene <80, đặc-trưng <7, breakpoint dưới sàn).

## Rà TỪNG button (trục 5 — bắt buộc)

| Button | Selector | Qua Button.vue? | Padding | Target | Ghi chú |
|---|---|---|---|---|---|
| Thêm bài học | toolbar (208) | ✅ sm | h-9 px-3 | 36px | action chính tab → md (40px) |
| Soạn Ladder | toolbar (209) | ✅ sm ghost | h-9 px-3 | 36px | ✓ |
| Sửa (row) | (260) | ✅ sm ghost | h-9 px-3 | 36px | liền nút Xóa, gap 4px → 8px |
| Xóa (row) | (263) | ✅ sm danger | h-9 px-3 | 36px | như trên |
| Thêm chủ đề | (278) | ✅ sm | h-9 px-3 | 36px | action chính tab → md |
| Modal: Hủy/Lưu, Hủy/Tạo | (339–340, 351–352) | ✅ | h-10 px-4 | 40px | ✓ |

## KILL-LIST bị vi phạm (phải sửa)

1. Hero gradient + blob + gradient-clip (368–427).
2. Icon tròn gradient mint + shadow (479–490).
3. `hover-lift` 180ms ease (291) + shadow trên card `.card` legacy.
4. th uppercase; weight 700.

## Hướng sửa (đã làm)

- Banner → surface band level-2 + **mono strip block-token** (số bài học + chủ đề — dữ liệu thật từ API, block `data-core` + index mono).
- **Sửa bug ngày giả**: bỏ cột "Ngày tạo" (API không trả createdAt) → thay **cột index mono `#01`** (dữ liệu tuần tự — quyết định #4); i18n `colCreated` → `colIndex`.
- Bảng → chuẩn §4.6: th không uppercase + `scope="col"`, td 12px, hover `muted/50`, **mobile card-stack**; cột sim-count mono.
- Topic card: bỏ gradient icon + hover-lift → icon neutral + hover border strong; name text-lg 600 tracking tight.
- Toolbar action chính `sm` → `md`; actions gap 4px → 8px; icon button 14→16px.
- Thêm error state + nút Thử lại; `.text-muted` → 4 tầng.
- Ghi chú Phase 2: nội dung rich-text (contentHtml) nếu hiển thị emoji → xử lý sau, không sửa nội dung CMS (task ghi rõ).
