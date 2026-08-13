# OLLAMA GATE — LadderView `/ladder/:nodeId`

> Ngày: 14/08/2026 · Agent: dev-frontend (Phase 2 bổ sung) · Worktree: D:\FPT\neww-qp2 (feature/view-quality-merge-check)

## KHÔNG chạy được gate 3 câu (Ollama) — ghi thật, không báo pass giả

Agent chạy ở model **deepseek-v4-flash — KHÔNG hỗ trợ đọc ảnh** (lỗi "Cannot read image"). Không thể tự chấm 3 câu hỏi thị giác trên screenshot. Thay vào đó đã verify bằng **DOM assertions + computed style** (chrome-devtools, dev server :5180, auth inject qua Pinia):

| Kiểm tra | Kết quả |
|---|---|
| Banner gradient sunset + blob + shadow | Đã bỏ — chrome = `card-raised #F7FDFD` + `border-subtle` + `shadow: none` + radius 12px (light); dark = `#134E4A` + `rgba(255,255,255,0.06)` |
| Emoji 🪜 | Không còn — icon `lucide-list-ordered` 20px trong ô muted 44px |
| H1 | 48px / 600 / -1.44px (light + dark đúng token) |
| Kicker mono | JetBrains Mono 12px uppercase `PRACTICE LADDER · NODE 01` |
| Strip block-token | nền `#0D1020` (canvas-ink) + index mono `#6B7385` — LUÔN tối cả 2 theme |
| Buttons | 2 nút qua Button.vue: h 40px, px 16px, gap actions 8px; lucide ArrowLeft/ArrowRight 16px |
| Console | 0 error / 0 warn |
| Overflow | 1536/768/390px: không tràn ngang (scrollWidth ≤ innerWidth) |

Screenshot đã lưu để PM/reviewer có mắt xem: `ladder-light.png` (136KB) + `ladder-dark.png` (77KB) trong thư mục này.
