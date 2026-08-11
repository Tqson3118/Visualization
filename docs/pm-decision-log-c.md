# PM DECISION LOG — SESSION C (Git + Push)

## [2026-08-12 05:45] Session C — git init + commit hiện trạng + push (chế độ --auto)
- Quyết định: nhóm commit theo HIỆN TRẠNG THỰC TẾ (chưa có frontend//backend/ root — Session B chưa sinh): 7 commit theo mapping phân công docs: infra+gitignore→Son (chủ repo Tqson3118, nhóm nhỏ dùng tên phù hợp nhất), docs/tailieu/session/shared→Phuc, source backend→Bao, source frontend UI→Son, engine (src/core + src/features)→Thu, source plan/skills/scripts/.agents→Thu, source document/tailieu/docs/.env.example→Phuc
- Quyết định: .gitignore chặn node_modules/, .env* (trừ *.example), bin/, obj/, dist/, coverage/, .playwright-mcp/, secrets/, *.pem, *.key, *.log, IDE/OS. CHỈ commit .env.example/.env.production.example — KHÔNG commit .env/.env.development (có giá trị thật)
- Quyết định: commit .opencode/ (agent/pm.md, dev.md...) + opencode.json — config opencode cần cho cả team; bỏ node_modules bên trong
- Quyết định: main = toàn bộ commit hiện trạng (tài liệu ổn định); tạo nhánh dev từ main; push cả 2 lên origin https://github.com/Tqson3118/Visualization.git
- Quyết định: push thử với GIT_TERMINAL_PROMPT=0 (không treo terminal); nếu thiếu credential → KHÔNG đổi thiết kế, ghi vào SETUP_TODO cho người dùng đăng nhập GitHub rồi push lại
- Rủi ro: Session A/B chạy song song có thể sinh file mới (tailieu/BAO_CAO.md, frontend//backend/ root) sau commit → file đó nằm untracked, cần commit bổ sung khi A/B hoàn tất (ghi trong report)
- Ảnh hưởng: toàn repo D:\FPT\neww, docs/SETUP_TODO.md, docs/pm-report-c.md