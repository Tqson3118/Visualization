# OLLAMA GATE — NodeHubView `/path/:topicId/node/:nodeId`

> Ngày: 14/08/2026 · Agent: dev-frontend (Phase 2 bổ sung) · Worktree: D:\FPT\neww-qp2 (feature/view-quality-merge-check)

## KHÔNG chạy được gate 3 câu (Ollama) — ghi thật, không báo pass giả

Agent chạy ở model **deepseek-v4-flash — KHÔNG hỗ trợ đọc ảnh**. Không thể tự chấm 3 câu hỏi thị giác trên screenshot. Thay vào đó đã verify bằng **DOM assertions + computed style** (chrome-devtools, dev server :5180):

| Kiểm tra | Kết quả |
|---|---|
| Banner gradient sunset + blob + overlay hack | Đã bỏ — chrome = `card-raised #F7FDFD` + `border-subtle` + `shadow: none` (light); dark = `#134E4A` + `rgba(255,255,255,0.06)` |
| Kicker mono dữ liệu thật | `NODE 03 · SORT.INSERTION` (node 3) / `NODE 01 · SORT.BUBBLE` (node 1) — mono uppercase |
| H1 | 48px / 600 / -1.44px, màu token (không còn #fff + text-shadow) |
| Icon | `lucide-graduation-cap` 20px, ô muted 44px, màu tertiary (không gradient/shadow) |
| CTA "Mở mô phỏng" | h 40px (md), icon Play 16px; nút back h 40px + `lucide-arrow-left` |
| Badge | muted (không primary trang trí) |
| Tabs | 3 tab shadcn, chiều cao 37.6px (≥24 target); chuyển tab practice → LadderShell render OK |
| Panel transition | easing chuẩn enter `cubic-bezier(0.16,1,0.3,1)` 200ms / exit `cubic-bezier(0.7,0,0.84,0)` 150ms |
| Console | 0 error / 0 warn |
| Overflow | 1440 + 390px: không tràn ngang |

Ghi chú ngoài phạm vi: tab Lý thuyết render LessonDetail chứa emoji nội dung (`📝 Ghi chú`, `🎯 Sắp xếp cơ bản`, `👉 Bấm`, `📚 Tham khảo`) — component chung ngoài scope 3 view, đã ghi notes.md.
