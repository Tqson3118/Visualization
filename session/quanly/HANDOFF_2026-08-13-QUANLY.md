# HANDOFF — 13/08/2026 (quản lý tổng — phiên tiếp theo bắt đầu từ đây)

> File này tóm tắt TOÀN BỘ trạng thái dự án + các session đang chạy + prompt sẵn sàng. Phiên mới đọc file này là nắm hết, KHÔNG cần đọc lại lịch sử cũ.

## 1. DỰ ÁN
- Đồ án tốt nghiệp FPT (GVHD Phạm Ngọc Ái Liên, Lớp SD21361, 4 thành viên): **DSA-Visual** — hệ thống hỗ trợ học tập + trực quan hóa CTDL & giải thuật.
- Nhóm: Mai Tiểu Bảo TD01287 (backend) · Thái Quang Sơn TD01282 (frontend) · Huỳnh Lê Minh Thư TD01131 (engine+test) · Trần Viết Tâm Phúc TD01261 (tài liệu).
- Mốc: khởi động 12/05/2026, phát triển hết 11/08/2026, bảo vệ cuối tháng 8. **CẤM ghi "20 tuần/16 tuần" — chỉ 13 tuần.**

## 2. TRẠNG THÁI TỔNG (kiểm chứng 13/08)
- ✅ Docs 12/12 + báo cáo Word + git: `main` (ổn định, chưa merge dev→main — PR #1 mở chờ) + `dev` (tích hợp đầy đủ).
- ✅ Đợt D: backend services/controllers/DTO/migration 32 bảng + seed thật + 44 generator + 33 view. FE 72/72 test, BE 44 unit + 27 integration, e2e 11/11.
- ✅ Đợt E: runMeasure Web Worker + renderer canvas thật + Playwright e2e + integration tests (Testcontainers).
- ✅ Đợt F: THIRD_PARTY thật + checklist §17.9 + TEST_PLAN số liệu.
- ✅ Đợt G: stack mới **tailwindcss 4 + shadcn-vue + motion-v + GSAP + vue-echarts + Lenis + vue-sonner + lucide/phosphor + font Geist/JetBrains Mono** đã merge dev. G-F3D: 12 màn chính 11 PASS/1 lỗi (đã fix G-F3E/G-F3E2). Bug canvas ResizeObserver đã fix.
- ✅ Đợt G-PHU: **9/9 DONE + APPROVE** (feedback endpoint, 2FA MailHog, lesson-sim-keys, breakpoints, **premium QR MB Bank** NGUYEN THI NHU HOA 83863112088386 — nội dung CK tự động DSV<UserId>T<months>, kích hoạt tự động sau 60s, docs đã đồng bộ SRS/SDD/USER_GUIDE/API_REFERENCE/THIRD_PARTY, NFR-5 đo lại 852KB gốc không nới).
- ✅ Git: 8 PR ma base main đã đóng (còn PR #1 dev→main). Quy tắc: PR base `dev`.
- ✅ MCP: exa (key 6e92b042... — đã verify), fetch, context7, playwright, chrome-devtools, memory, github (token fine-grained), duckduckgo. **notebooklm MCP ĐÃ TẮT** (auth fail nhiều lần — profile bị lock bởi node process cũ; đã cleanup_data + kill node/chrome, chưa login thành công; user quyết bỏ dùng).
- ✅ Agent (đội hình 10): pm · dev · dev-backend · dev-frontend · dev-engine · dev-ux · dev-test · dev-e2e (Playwright + Ollama qwen2.5vl:3b — model chính KHÔNG đọc ảnh) · dev-review · dev-docs. Skill `pm-prompt-std` (khuôn prompt 7 phần). MCP github cần GITHUB_TOKEN đã set.
- ✅ Review chéo 2 prompt I/J (user tự viết) đã nhúng khối bài học + BUG-1.

## 3. CÁC SESSION ĐANG CHẠY (song song — KHÔNG đụng nhau)
| Session | Prompt | Nơi chạy | Nhánh | Trạng thái |
|---|---|---|---|---|
| **H — UI review 48 màn** | `PROMPT_H_UI_REVIEW.md` | `D:\FPT\neww` | feature/ux-h-* → merge dev | ĐANG CHẠY (11:20 h-b-e2e mới nhất) |
| **Diagrams — 6 ảnh drawio** | `PROMPT_DIAGRAMS_DRAWIO.md` | **`D:\FPT\neww-diagrams`** (worktree) | feature/diagrams — commit được, KHÔNG merge dev | MỚI TẠO worktree + copy style-guide/notes/samples sẵn |
| **J — Backend audit** | `PROMPT_J_BACKEND_AUDIT.md` | `D:\FPT\neww` | feature/backend-audit hoặc LOCAL | CHƯA CHẠY — user sẽ mở session riêng |

⚠ **Lưu ý H vs Diagrams**: user đã CHỦ ĐỘNG XÓA 6 ảnh SVG cũ tailieu/diagrams/* (lỗi + xấu) — vẽ mới từ đầu. Diagrams chạy ở worktree riêng nên không đụng working tree chính.

## 4. PROMPT SẴN SÀNG (session/ — đã research-verified + nhúng bài học)
- **`PROMPT_H_UI_REVIEW.md`** — đợt H: review + nâng cấp 48 màn (38 route), 5 nhóm, checklist 10 tiêu chí, vòng Ollama 7 tiêu chí (≤3 điểm phải sửa), ảnh FINAL 12 màn cho báo cáo.
- **`PROMPT_DIAGRAMS_DRAWIO.md`** — 6 ảnh (4 use case + 2 ERD) theo mẫu draw.io chính thức. Đã fix 6 lỗi review: repo đúng `jgraph/drawio-diagrams` + `jgraph/drawio-mcp` (5170★ — tài liệu AI chính thức: style-reference, mxfile.xsd, xml-reference); UML 2.5.1 §18.1.4 (include/extend đều nét đứt + mũi tên hở, nhãn «include»/«extend»); hướng include gốc→bị-include; **verify đếm 28 UC hiển thị** (32 − gộp UC-06+07 − loại UC-14/22/26); bỏ 3 extend trỏ UC-03 (không ảnh nào vẽ); 4 actor (Khách/Người học/GV/Admin); palette teal #0D9488 không neon; **0 dây chéo** (script intersect); drawio CLI path check; ollama list check.
- **`PROMPT_I_VIEW_QUALITY.md`** — audit + nâng cấp 36 view (bảng chuẩn 7 trục + thư viện lung linh: canvas-confetti/dotlottie/auto-animate/TresJS cân nhắc — lazy-load bắt buộc; Lighthouse ≥90 a11y/≥80 perf, axe-core 0 critical, 36/36 view có animation, 0 hardcode spacing).
- **`PROMPT_J_BACKEND_AUDIT.md`** — audit 5 trục backend (exception IExceptionHandler .NET 10, business logic RowVersion/idempotency/transaction, EF N+1/NoTracking/keyset, security JWT/RBAC/rate limit/CORS, migration khớp entity). **BUG-1 nhúng sẵn: job downgrade Premium KHÔNG tồn tại** (`GamificationService.cs:840-844` — hết hạn vẫn HeartsMax 30, SRS FR-10.7 yêu cầu clamp 10) — ghi chi tiết `docs/work/audit-notes-backend.md`.

## 5. 📌 BÀI HỌC ĐÃ ĐÚC KẾT (bắt buộc — đã nhúng vào mọi prompt)
1. **LỖI "TASK TRẢ RỖNG" (13/08 — 4/4 lần)**: KHÔNG nhúng file >5KB vào prompt task (style-guide 21KB → subagent vượt context → trả rỗng). Prompt task CHỈ trỏ đường dẫn file. Xử lý: (1) bỏ nhúng → đường dẫn; (2) task test siêu nhỏ; (3) tách nhỏ task (1 ảnh/5-6 view/task); (4) resume session cũ — không tạo task trùng; (5) 2 lần fail → ghi FAIL + lý do.
2. **PR base `dev` KHÔNG main** (đã đóng 8 PR ma). Commit-as: backend→bao, frontend/UX→son, engine/test→thu, docs→phuc.
3. **Ollama review UI bắt buộc 7 tiêu chí** (UI: thẩm mỹ/nhất quán/rõ ràng/phản hồi trực quan · UX: luồng thao tác/tiếp cận/thỏa mãn — 1-5 điểm, ≤3 phải sửa ≤2 vòng).
4. **Task NHỎ + fresh context + explore trước + trạng thái ghi docs/work/** (skill pm-prompt-std).
5. **Không ai tự chấm bài mình**: dev viết → dev-test verify → dev-e2e (UI) → dev-review chốt APPROVE/CHANGES REQUESTED.
6. **--auto: ghi decision log TRƯỚC khi làm; lệch docs chủ ý phải có task dev-docs đồng bộ đi kèm.**

## 6. VIỆC CÒN LẠI (sau khi H/diagrams/J xong)
1. **PROMPT_I** (view quality) — chạy SAU H xong (cùng khu vực views).
2. Merge dev→main (PR #1) sau khi user duyệt bản cuối.
3. 18 ảnh báo cáo: 6 sơ đồ (diagrams session) + 12 màn UI (H session — ảnh FINAL) → build lại docx pandoc (C:\Users\Administrator\AppData\Local\Pandoc\pandoc.exe) theo BAO_CAO.md.
4. Điền ngày bảo vệ/ngành bìa (SETUP_TODO mục 1-2 — cần user).
5. 2FA cần SMTP thật (đang MailHog); premium QR đã xong (G-PHU).
6. Docs đồng bộ rename UC (SRS §5.1 tên cũ vs ảnh dùng tên mới — diagram-notes.md đã ghi).
7. Xóa worktree diagrams khi xong: `git worktree remove D:\FPT\neww-diagrams`.

## 7. MÔI TRƯỜNG
- Backend :5000 (SQL Server Docker, seed student@demo.local/Student@123 — domain đăng ký: `university.edu.vn`), frontend :5174 (5173 bị relay Docker chiếm).
- pandoc: C:\Users\Administrator\AppData\Local\Pandoc\pandoc.exe. Ollama qwen2.5vl:3b @ localhost:11434. draw.io CLI: "C:\Program Files\draw.io\draw.io.exe" (nếu winget cài).
- GITHUB_TOKEN + EXA_API_KEY đã lưu trong ~/.config/opencode/opencode.jsonc (ngoài repo).
