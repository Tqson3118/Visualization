# OLLAMA GATE — FinalTestView `/path/:topicId/final-test`

> Ngày: 14/08/2026 · Agent: dev-frontend (Phase 2 bổ sung) · Worktree: D:\FPT\neww-qp2 (feature/view-quality-merge-check)

## KHÔNG chạy được gate 3 câu (Ollama) — ghi thật, không báo pass giả

Agent chạy ở model **deepseek-v4-flash — KHÔNG hỗ trợ đọc ảnh**. Không thể tự chấm 3 câu hỏi thị giác trên screenshot. Thay vào đó đã verify bằng **DOM assertions + computed style** (chrome-devtools, dev server :5180):

| Kiểm tra | Kết quả |
|---|---|
| Banner gradient aurora + blob + overlay hack | Đã bỏ — chrome = `card-raised #F7FDFD` + `border-subtle` + `shadow: none` (light); dark = `#134E4A` + `rgba(255,255,255,0.06)` |
| Kicker mono dữ liệu thật | `FINAL TEST · PASS ≥ 70%` (threshold thật 70) — mono uppercase |
| H1 | 48px / 600 / -1.44px, màu token |
| Rules strip | 3 card level-1 `#FFFFFF` + `border`, **shadow: none**, hover chỉ đổi border (transition border-color 150ms); giá trị số mono (`≥ 70%`, `20%`), weight 600 (bỏ 700) |
| Icon rules | lucide Gauge/Trophy/Repeat 16px, ô muted 32px, màu tertiary (không gradient) |
| Badge | muted |
| Nút back | h 40px + `lucide-arrow-left`; i18n bỏ `←` + bỏ 🏅 toast |
| Console | 0 error / 0 warn |
| Overflow | 1440 + 390px: không tràn ngang; rules 390px xếp 1 cột (358px) |
| Dark | chrome #134E4A, rule #0F3D3A, H1 #CCFBF1 — đủ token |
