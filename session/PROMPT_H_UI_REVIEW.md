# PROMPT ĐỢT H — REVIEW + NÂNG CẤP UX/UI TOÀN BỘ (48 màn)

Dán vào `/pm "..." --auto`:

```
Đọc session/HANDOFF_2026-08-13-UI.md + docs/pm-report-gp.md (đợt G phụ — phải merge xong vào dev trước khi bắt đầu) + docs/work/g-f3d.md + g-f2*.md (đã polish màn nào rồi — không làm lại) + docs/SCREEN_MAP.md (Màn 01-32 + N-1..N-16) + docs/BAO_CAO_SPEC.md §6.2 (12 màn bắt buộc đẹp nhất).
Nguồn chuẩn: docs/PRODUCTION_PROMPT.md → SDD (tokens §8.1, stack §3.1) → SCREEN_MAP. Phân vai theo .opencode/agent/: dev-ux sửa, dev-e2e chụp+Ollama, dev-test verify, dev-review chốt, dev-docs đồng bộ. Task NHỎ theo TỪNG MÀN/nhóm màn, trạng thái docs/work/<task>.md.

Đợt H — REVIEW + NÂNG CẤP UX/UI TOÀN BỘ. Quy tắc:
- Đi qua TẤT CẢ 38 route thực tế (36 view) theo nhóm, mỗi nhóm 1 task nhánh riêng:
  A. 12 màn báo cáo (ƯU TIÊN CAO NHẤT — BAO_CAO_SPEC §6.2): /, /login, /learn/:id, /simulator/:key (chỉ chrome UI — canvas giữ nguyên), /exercise/:id, /path, /ladder/:id, /ladder/:id/lab, /code/:key, /benchmark/:k1/:k2, /leaderboard, /profile.
  B. Admin (5): /admin, /admin/users, /admin/stats, /admin/settings, /admin/content, /admin/ladder.
  C. Lớp học (3): /classes, /classes/:id, /classes/:id/report.
  D. Gamification & tài khoản: /shop, /quests, /premium (+ QR mới), /account/subscription, /profile tabs, /leaderboard.
  E. Phụ: /register, /forgot-password, /reset-password, /simulations, /cheatsheet, /help, /privacy, /404, /path/:topicId, /path/:topicId/node/:nodeId, /path/:topicId/final-test.
- MỖI màn kiểm tra theo checklist: (1) bố cục cân đối (grid, spacing nhất quán token), (2) contrast ≥ 4.5:1 text, (3) empty state + loading skeleton đẹp, (4) hover/focus/active state rõ, (5) dark mode + light mode, (6) responsive 1366×768 + mobile 390px không overflow, (7) shadcn component + motion-v micro-interaction (page transition, button hover), (8) gradient OKLCH theo nhóm (Aurora gamification, Sunset streak/badge, Cyber Mint canvas), (9) không chữ cắt/tràn, (10) console 0 lỗi.
- VÒNG LẶP OLLAMA BẮT BUỘC (mỗi nhóm màn): dev-ux sửa xong → dev-e2e chụp ảnh light+dark → gửi Ollama qwen2.5vl:3b prompt "liệt kê điểm XẤU cụ thể (nền chói, chữ khó đọc, lệch spacing, chồng lấn, thiếu nhất quán) + gợi ý sửa" → dev-ux SỬA HẾT nhận xét khả thi (≤ 2 vòng) → bảng nhận xét→trạng thái (ĐÃ SỬA file:dòng / TỪ CHỐI lý do) lưu docs/work/.
- KHÔNG đổi: token màu teal #0D9488, engines/** + CanvasArea (đã fix ResizeObserver), contract API, logic store.
- Sau mỗi nhóm: dev-test verify (build + test + smoke 2-3 màn nhóm đó), dev-review verdict.
- Cuối đợt: dev-e2e chạy LẠI 12 màn chính + chụp ảnh FINAL → lưu docs/work/final-<màn>.png (đây là nguồn 12 ảnh UI cho báo cáo Word — thay placeholder 12 màn) + bảng tổng hợp so sánh đợt F → G → H.

Quy trình: feature/ux-h-<nhóm> từ dev → verify → review → merge dev. Commit: frontend/UX → son, engine/test → thu, docs → phuc. Ghi log: docs/pm-report-h.md + docs/pm-decision-log-h.md. Việc cần user → docs/SETUP_TODO.md. --auto
```
