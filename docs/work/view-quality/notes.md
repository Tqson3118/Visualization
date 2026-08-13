# NOTES — View Quality Phase 1 · Nhóm D

> Ngày: 14/08/2026 · Agent: dev-frontend (nhóm D — 3 view Classes + 5 view Admin) · Nhánh: feature/view-quality-d

## Ollama 3-gate — KHÔNG CHẠY ĐƯỢC (không báo pass giả)

- Task yêu cầu "Ollama 3 gate riêng ghi ollama-log/<view>.md". Trong repo/worktree KHÔNG có:
  - script/CLI gate Ollama (`docs/work/` chỉ có `bundle-*.txt`, `scorecard.md`, `standard.md`; grep `ollama` toàn repo chỉ ra `docs/work/teacher-register/ollama.md` — tài liệu phiên khác, không phải gate).
  - endpoint/key LLM nào để gọi gate đánh giá 8 view.
- Thay thế đã làm (không thay thế 1-1 cho gate LLM, chỉ là kiểm chứng khách quan):
  - Đo computed style bằng Chrome DevTools (padding button/badge/card shadow/mono/contrast token).
  - Kiểm console error = 0, overflow ngang = 0 ở 3 mốc 1366/768/390, light + dark.
  - Chạy flow thật qua BE :5000 (login teacher/admin, tạo lớp, thêm member, copy mã, tab, xóa lớp, report dữ liệu thật; admin: users/stats/content/ladder/settings + modal + tab + select node).
- Nếu muốn gate Ollama: cần pm/cơ chế bổ sung script + model key — ghi lại để Phase 1 tổng hợp.

## Quan sát khác (ngoài phạm vi sửa — để các nhóm/task sau xử lý)

1. **Dark mode toàn app chưa được wire**: `ui.theme` + `toggleTheme()` tồn tại trong `stores/ui.ts` nhưng KHÔNG có chỗ nào thêm class `dark` vào `<html>` (grep `documentElement` chỉ có canvas chart). 3 view đã test dark bằng cách thêm class thủ công — token OK. Task hạ tầng (App.vue/main.ts) nên wire sớm.
2. **`lucide-vue-next@1.0.0` bị npm deprecate** (khuyến nghị `@lucide/vue`) — quyết định xuyên-nhóm vẫn chốt lucide-vue-next duy nhất; ghi lại khi npm install (báo pm nếu muốn đổi).
3. **`EmptyState` icon prop** nhận tên trong `SVG_PATHS` (`utils/emojiParser.ts`) — `chart`/`bar-chart` không tồn tại → fallback x-circle. View report đã đổi sang `database`; nên thêm `bar-chart` vào SVG_PATHS nếu view khác cần.
4. **`DialogContent` thiếu `aria-describedby`** — cảnh báo reka-ui ở mọi Modal (không chỉ 3 view này): `Modal.vue` (ui chung) render DialogTitle nhưng không có Description → warn console. Đề xuất fix ở task component chung (thêm DialogDescription sr-only).
5. **Gán nội dung (assign) luôn 400**: backend yêu cầu lessonId/exerciseId (ClassService.AddAssignmentAsync), view chỉ gửi null → đã có ghi chú backlog trong i18n (`detailAssignNote`). Không phải lỗi view.
6. **`ClassMemberDto` frontend dùng `id` nhưng backend trả `userId`** (`ClassService.GetByIdAsync` map `UserId = u.Id`) — bảng thành viên hiển thị OK vì chỉ dùng `member.id` cho `:key`/remove; removeMember dùng đúng id=userId. Để ý khi đụng member DTO.
7. **`#d9dde8` exception** (chữ trên panel canvas-ink): khi task token §2.4 thêm `--canvas-text` → thay thế tại `.class-report__lagging-name` + AdminStatsView (chart/donut/hero-value) + strip caption (đã ghi decision log).

## Nhóm D admin — quan sát bổ sung (14/08)

8. **Select/textarea chưa có wrapper shadcn** (AdminContent modal: topic/status/sim select + textarea HTML; AdminLadder: node/exercise select): giữ native `.input` + scoped override token (`--card/--border/--foreground`, text-sm, transition 150ms). Đề xuất task component: thêm `Select.vue`/`Textarea.vue` wrapper shadcn cho Phase 2.
9. **Nội dung rich-text CMS (contentHtml)**: view không render nội dung bài học (chỉ quản lý metadata) → KHÔNG gặp emoji nội dung; nếu Phase 2 render contentHtml, rà emoji icon + microcopy (ghi theo task).
10. **AdminContentView đã bỏ cột "Ngày tạo"** vì `LessonSummary` không có `createdAt` (trước render `formatDate(new Date())` — ngày giả) → thay cột index mono `#01`. Nếu cần ngày thật: backend bổ sung `createdAt` vào DTO lesson rồi thêm lại cột.
11. **Tablet 768**: bảng admin giữ scroll ngang trong container (card-stack chỉ ≤640 — theo precedent nhóm Classes). DESIGN §8 ghi "768 ẩn cột phụ" — cân nhắc nâng lên 768 cho Phase 2 nếu cần (quyết định nhóm, không tự làm).
