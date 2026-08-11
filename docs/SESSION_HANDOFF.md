# SESSION HANDOFF — CHUYỂN GIAO TOÀN BỘ (09/08/2026)

> [!CAUTION] **FILE LỊCH SỬ (12/08/2026)** — mục 3/4 (quyết định, việc tiếp theo) đã cũ: Phần 19 ĐÃ được chuyên gia duyệt và vá qua v2.3-v2.5 (xem REVIEW_PRODUCTION_PROMPT.md + changelog Phần 22); SRS/SDD/API/TEST đã sinh mới 12/08. Dùng file này chỉ để tra cứu setup môi trường (mục 1-2).

> File DUY NHẤT cần đọc khi mở session mới tại D:\FPT\neww. Sau đó đọc docs/PRODUCTION_PROMPT.md Phần 19 + 8.0 + 7.0, rồi chạy /pm cho công việc mới.

## 1. OPENCODE SETUP (đã cài xong — KHÔNG cài lại)
- Skills global (~/.agents/skills, 244 skill): obra/superpowers (brainstorming, spec-driven, TDD, planning...), dotnet/skills (Microsoft), managedcode/dotnet-skills, antfu/skills (vue/pinia/vitest...), alexanderop vue-development, addyosmani/agent-skills (code-review), nutlope/hallmark. Skills bổ sung (~/.config/opencode/skills): skill-creator, frontend-design (Anthropic).
- MCP (trong ~/.config/opencode/opencode.jsonc): playwright, github (remote — CẦN setx GITHUB_TOKEN + mở lại terminal), context7, chrome-devtools, memory (lưu ~/.opencode/memory.json).
- Agents project (.opencode/agent/): pm (primary, edit:deny — orchestator có checkpoint), dev, dev-frontend, dev-backend (subagent). Command: /pm (.opencode/command/pm.md).
- Plugin agent-teams: celscin-magentic (opencode.json project-level; tools: celscin_dispatch, celscin_run_tasks, agent_message; đã soi mã, không có mã độc; nguồn Gitee ẩn danh — dùng cẩn thận).
- Công cụ: repomix (npm global).

## 2. DỰ ÁN DOCS (thư mục D:\FPT\neww\docs)
- PRODUCTION_PROMPT.md — 5011 dòng, MASTER sinh toàn bộ tài liệu. Phần quan trọng: 0.5 (bài học bảo vệ), 3 (FR đầy đủ), 6 (UC-01→32), 7.0 (1 màn 1 việc), 8.0 (EDV — code chạy thật qua StepExecutor), 9 (API), 10 (DB 31 bảng), 17 (checklist + ma trận truy vết), 19 (THIẾT KẾ CHỐT — đọc đầu tiên).
- HANDOFF_REBUILD.md — bàn giao tái sinh VisualizationDSA-v2 (phương án B, danh sách file cứu được, việc phải làm).
- SRS.md / SDD.md — bản nháp cũ, SẼ THAY bằng bản sinh lại từ PRODUCTION_PROMPT sau khi chuyên gia duyệt.

## 3. QUYẾT ĐỊNH ĐÃ CHỐT (KHÔNG hỏi lại)
- Phương án B: xây VisualizationDSA-v2 song song; giữ core engine + tài liệu cũ làm nguyên liệu.
- Scope: 10 module A→J (Auth, Học tập+LearningPath+CheatSheet, Visualizer EDV+Benchmark, Practice Ladder+KT cuối lộ trình, Tiến độ, Admin tối giản, Phụ trợ, Lớp học, Code Runner, Gamification&Premium).
- CẮT (12 FR): 1.10, 2.7, 2.8, 2.9, 3.13, 3.17, 3.19, 5.6, 5.7, 6.4, 7.3, 7.5. GIỮ dù đề xuất cắt: 1.11, 2.6, 4.10, 7.4.
- Hệ Tim: 10❤ Free / 30❤ Premium; hồi 30p / 10p (full ≈5h); vào node trừ 1 tim atomic; session 30p resume miễn phí; retry trong session free; xem lại node đã pass free; quest tràn tim → +5 gems.
- Premium = P1 checkout MÔ PHỎNG (không cổng thanh toán thật); hết hạn → downgrade giữ gems/avatar/items.
- Killer features: Practice Ladder, Benchmark Lab (đa n + overlay lý thuyết — FR-3.20b), Two-way sync = DEEP-LINK theo stepIndex (FR-2.11, KHÔNG nhúng canvas — không vi phạm 7.0).
- Seed phải NGHIÊM TÚC (Phần 19.6): 6 chủ đề, 18 bài, mọi code chạy qua StepExecutor + golden data.
- Nguyên tắc độ sâu 19.9: mỗi tính năng ≥ 4 tầng (danh sách → chi tiết → thao tác → vòng lặp), cấm "màn hình đổ 1 đống".

## 4. VIỆC TIẾP THEO (ưu tiên)
1. CHỜ: chuyên gia tư vấn duyệt Phần 19 (đã vá RBAC/UC/API/DB/matrix theo review — 19.8).
2. Sau khi duyệt: sinh SRS.md + SDD.md + API_REFERENCE.md TỪ PRODUCTION_PROMPT.md (dùng skill writing-plans/spec-driven).
3. Khởi tạo VisualizationDSA-v2 theo HANDOFF_REBUILD.md (trích core engine, KHÔNG copy secret/.env/node_modules).
4. Chạy theo quy trình: /pm + agent pm (checkpoint trước mỗi đợt code).

## 5. LƯU Ý
- Các file .md trong docs viết bằng UTF-8; PowerShell đọc console sẽ hiện lỗi font — dùng tool đọc file (Read) thay vì Get-Content.
- Khi sửa PRODUCTION_PROMPT.md: dùng PowerShell thay thế theo anchor (tool edit/write KHÔNG có trong mọi session), mỗi lệnh giữ dưới ~2.5KB kẻo bị cắt.
- Nếu cần review docs tiếp: skill brainstorming (đã cài) + đối chiếu 3 phần 19/8.0/7.0.
- Lịch sử quan trọng: bản cũ VisualizationDSA (175 commits) bị hội đồng chặt vì: hardcode visual (→ EDV), 1 màn 4 chức năng (→ 7.0), scope trôi dạt (→ 2.2 + 19.9), secret lộ git (→ không copy sang v2).