# HANDOFF — Tái sinh dự án VisualizationDSA (Phương án B)

> [!CAUTION] **FILE LỖI THỜI (12/08/2026)** — giữ làm lịch sử tham khảo, KHÔNG dùng làm chuẩn nữa.
> Nội dung ghi **PostgreSQL + Clean Architecture, 5 module, 24 use case, ~73 FR, 24 bảng** — là đề xuất CŨ trước vòng review.
> Chuẩn hiện tại (đã duyệt): **SQL Server + 2 project backend, 10 module A-J, 32 UC, 31 bảng DB** — xem PRODUCTION_PROMPT.md v2.5 + SRS/SDD mới.

> File bàn giao cho session mới. Đọc file này trước khi làm gì.

## Quyết định đã chốt
- **Phương án B**: xây dựng mới song song `D:\FPT\neww\VisualizationDSA-v2` (bản cũ giữ nguyên làm kho nguyên liệu).
- Scope mới: Auth + Lessons + Visualizer (mô phỏng DSA) + Quiz trắc nghiệm + tiến độ học tập.
- CẮT: payment (SePay/VietQR/Gems), gamification (XP/Level/Leaderboard), quiz realtime (SignalR), embed widget, code-IDE Monaco, AI assistant, WebGPU.
- Tech stack giữ nguyên: Vue 3 + TS + Vite + Pinia (FE), C# .NET 8 ASP.NET Core Clean Architecture (BE), PostgreSQL.

## Chẩn đoán bản cũ (VisualizationDSA, 175 commits)
1. Scope bùng nổ: ~30 feature dirs FE, 660 file src FE, 433 file cs BE — chạy feature không chốt scope.
2. Rò rỉ secret trong git: Cloudinary ApiSecret + Supabase DB password (cả production) — phải rotate key, không copy secret sang v2.
3. Test không trung thực: tracking khai 1549 pass, thực tế 611 test/112 fail (vitest sai environment, cần jsdom).
4. UI 4 phong cách xung đột, design token chết (tailwind.config không load — thiếu @config trong style.css).
5. Rác root: patch.py, temp.sql, old_header.vue, ~15 fix-*.cjs/rewrite_*.cjs, todo_opencode_*.md.
6. Docs không khớp code.

## Nguyên liệu cứu được (copy sang v2, KHÔNG tự viết lại từ đầu)
- FE lõi: frontend/src/core/CoreAnimationEngine.ts + features/animation-engine + vcr-player + features/dsa-modules + features/quiz + features/lesson + features/auth.
- BE skeleton: backend/src (Application/Domain/Infrastructure/WebApi) — cấu trúc chuẩn, chọn lọc lại.
- Tài liệu: docs/update85/ (demo-script, diagrams, use-cases 49, api_contract_schema) + tailieu/ (PRO2192_VisualizationDSA_Report.docx, PRO2192_Report_Goc.docx) + plan/ (architecture, api-spec, database).
- Design system: design-system/visualizationdsa/MASTER.md (token màu, chọn lại 1 phong cách).
- Kinh nghiệm: REVIEW_FIX_PLAN.md (danh sách bug đã biết — tránh tái phạm).

## Việc session mới phải làm (theo quy trình PM: /pm)
1. Đọc docs/PRODUCTION_PROMPT.md (khung SRS/SDD đã viết sẵn cho đúng chủ đề).
2. Dựng khung v2: repo mới, AGENTS.md mới, opencode.json, cấu trúc FE/BE.
3. Trích xuất phần lõi từ bản cũ (theo danh sách trên) — kiểm tra build/test trước khi copy.
4. Viết SRS.md + SDD.md khớp 100% code (dùng PRODUCTION_PROMPT làm chuẩn).
5. Cài lại test thật (vitest jsdom), CI cơ bản.
6. KHÔNG copy: .env, appsettings có secret, node_modules, dist, coverage.

## Setup OpenCode đã có (dùng tiếp)
- Agents: pm (primary, edit deny) + dev + dev-frontend + dev-backend (subagent) trong .opencode/.
- Command /pm. Skills: superpowers, dotnet, vue, hallmark... MCP: playwright, github, context7, chrome-devtools, memory.
- Lưu ý: cấu hình MCP/plugin nằm ở opencode.json + ~/.config/opencode/.
## BỔ SUNG SAU BÀN BẠC VỚI USER (QUAN TRỌNG — đọc cùng PRODUCTION_PROMPT.md Phần 0.5, 7.0, 8.0, Module I)

1. KIẾN TRÚC EDV: mọi giải thuật = mã TypeScript thật chạy qua StepExecutor (interpreter có instrument) -> TraceEvent[] -> player vẽ. CẤM hardcode chuỗi bước theo từng GT (lỗi bản cũ bị hội đồng chặt). Chi tiết: PRODUCTION_PROMPT.md Phần 8.0.
2. MÀN HÌNH: 1 màn = 1 việc. Luồng: /learn -> /learn/:lessonId (chỉ thẻ liên kết) -> /simulator/:key | /code/:key | /exercise/:id (3 trang RIÊNG). Cấm gộp học+visual+code+quiz 1 màn. Chi tiết: Phần 7.0.
3. MODULE I (CODE RUNNER): FR-9.1..9.5 — editor Monaco, chạy code người học + visualize, bài tập lập trình chấm tự động (Judge0/WASM), sandbox an toàn, lịch sử nộp.
4. Tổng phạm vi mới: 9 module (A-I), 24 use case, ~73 FR, 24 bảng DB, RBAC 33 hành động.
5. Khi viết SRS/SDD: mở đầu phải có bảng "Bài học từ buổi bảo vệ" (3 dòng: EDV, 1 màn 1 việc, scope) để hội đồng thấy rõ sự thay đổi so với bản cũ.
## CẬP NHẬT MỚI NHẤT: BẢN THIẾT KẾ CHỐT — PHẦN 19
Đọc PRODUCTION_PROMPT.md PHẦN 19 trước tiên: 10 module (A-J), hệ Tim/Gems/Quest/XP/Premium (P1 checkout mô phỏng), killer features (Practice Ladder, Benchmark Lab, Two-way sync), yêu cầu seed nghiêm túc (19.6), danh sách FR cắt (19.7). Phần 19 là nguồn ưu tiên cao nhất cùng Phần 8.0 (EDV) và Phần 7.0 (màn hình). CHỜ: chuyên gia tư vấn duyệt trước khi sinh docs chi tiết.