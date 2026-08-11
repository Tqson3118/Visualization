# PM DECISION LOG — SESSION C (Git + Push)

## [2026-08-12 05:45] Session C — git init + commit hiện trạng + push (chế độ --auto)
- Quyết định: nhóm commit theo HIỆN TRẠNG THỰC TẾ (chưa có frontend//backend/ root — Session B chưa sinh): 7 commit theo mapping phân công docs: infra+gitignore→Son (chủ repo Tqson3118, nhóm nhỏ dùng tên phù hợp nhất), docs/tailieu/session/shared→Phuc, source backend→Bao, source frontend UI→Son, engine (src/core + src/features)→Thu, source plan/skills/scripts/.agents→Thu, source document/tailieu/docs/.env.example→Phuc
- Quyết định: .gitignore chặn node_modules/, .env* (trừ *.example), bin/, obj/, dist/, coverage/, .playwright-mcp/, secrets/, *.pem, *.key, *.log, IDE/OS. CHỈ commit .env.example/.env.production.example — KHÔNG commit .env/.env.development (có giá trị thật)
- Quyết định: commit .opencode/ (agent/pm.md, dev.md...) + opencode.json — config opencode cần cho cả team; bỏ node_modules bên trong
- Quyết định: main = toàn bộ commit hiện trạng (tài liệu ổn định); tạo nhánh dev từ main; push cả 2 lên origin https://github.com/Tqson3118/Visualization.git
- Quyết định: push thử với GIT_TERMINAL_PROMPT=0 (không treo terminal); nếu thiếu credential → KHÔNG đổi thiết kế, ghi vào SETUP_TODO cho người dùng đăng nhập GitHub rồi push lại
- Rủi ro: Session A/B chạy song song có thể sinh file mới (tailieu/BAO_CAO.md, frontend//backend/ root) sau commit → file đó nằm untracked, cần commit bổ sung khi A/B hoàn tất (ghi trong report)
- Ảnh hưởng: toàn repo D:\FPT\neww, docs/SETUP_TODO.md, docs/pm-report-c.md
## [2026-08-12 05:55] Session C — BLOCKER: 3 git repo lồng nhau trong source/
- Phát hiện: source/VisualizationDSA, VisualizationDSA1, VisualizationDSA3 mỗi cái có .git riêng (branch master/g0-security/main) → git add source/* chỉ tạo gitlink, KHÔNG commit được nội dung (C3 block, dev đã dừng đúng quy trình)
- Quyết định: PHƯƠNG ÁN A — backup 3 .git sang C:\Users\ADMINI~1\AppData\Local\Temp\opencode\git-backup-20260812\ (giữ nguyên lịch sử gốc), xóa .git lồng nhau, track source/ như file thường trong mono-repo. LÝ DO: master plan xem source/ là vật liệu đầu vào (Session B "bê code từ 3 source"), không phải submodule cần remote riêng; dùng submodule sẽ phá ý nghĩa "commit hiện trạng" + không có remote công khai cho 3 repo con
- Quyết định: file mới tailieu/parts/* (Session A đang chạy song song) KHÔNG thuộc session C — để untracked, ghi chú trong report; chỉ commit file PM của session C (report + SETUP_TODO) ở commit cuối
- Ảnh hưởng: source/ trở thành file thường trong repo gốc; lịch sử 3 repo con chỉ còn trong backup local
