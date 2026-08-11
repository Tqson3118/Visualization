# SESSION CONTINUATION PROMPT — DSA-Visual (nối tiếp 12/08/2026)

> **File DUY NHẤT cần đọc khi mở session mới tại `D:\FPT\neww`.** Sau đó đọc `docs/PRODUCTION_PROMPT.md` (Phần 19 + 8.0 + 7.0 + 20/21) rồi mới bắt đầu công việc.

---

## 1. NGỮ CẢNH DỰ ÁN

- **Dự án**: Hệ thống hỗ trợ học tập và trực quan hóa CTDL & Giải thuật (DSA-Visual) — đồ án FPT, 4 thành viên (SD21361), GVHD Phạm Ngọc Ái Liên.
- **Đề tài mới = bản v2** thay thế VisualizationDSA (bản cũ 175 commits bị hội đồng chặt vì: hardcode visual → EDV, 1 màn 4 chức năng → "1 màn 1 việc", scope trôi dạt → loại trừ rõ).
- **Kiến trúc cốt lõi**: EDV (Execution-Driven Visualization) — mọi giải thuật là mã TypeScript THẬT chạy qua `StepExecutor` → TraceEvent[] → hoạt ảnh phát lại trace. CẤM hardcode chuỗi bước.
- **Công nghệ**: Vue 3 + Pinia + Vite + TS · ASP.NET Core 8 (2 project: `DsaVisual.Api` + `DsaVisual.Application`, KHÔNG Repository) · SQL Server + EF Core 8 · JWT (refresh rotate-invalidate).
- **Scope**: 10 module A-J, 75 FR giữ, 32 UC, 36 NFR, 31 bảng DB, 33 màn (32 + Khám phá `/simulations`), 20 tuần / 10 sprint.

## 2. TRẠNG THÁI HIỆN TẠI (12/08/2026 — cuối phiên trước)

### 2.1 Bộ tài liệu ĐÃ HOÀN THÀNH (12/12 file bàn giao, v1.0)

| File | Dòng | Ghi chú |
|---|---|---|
| docs/SRS.md | 1296 | Master matrix 75 FR, 32 UC, 36 NFR |
| docs/SDD.md | 2042 | EDV toàn bộ, 31 bảng (2 ERD), 33 màn |
| docs/API_REFERENCE.md | 730 | Mọi endpoint + error catalog + RBAC 36 |
| docs/TEST_PLAN.md | 759 | Golden data + TEST-B-148..155 trừ tim |
| docs/USER_GUIDE.md | 503 | 3 vai trò |
| docs/DEPLOY.md | 369 | Runbook 10 sự cố |
| docs/GLOSSARY.md | 105 | 3 nhóm thuật ngữ |
| docs/README.md | 206 | Ma trận 17.8 + FR↔UC↔Module |
| docs/SCREEN_MAP.md | 306 | Màn 01-33 + N-1..N-16 |
| shared/simulation-catalog.json | 44 entries | 34 GT + 10 CTDL, UTF-8 đúng |
| THIRD_PARTY.md | 72 | NFR-36 |
| README.md (root) | 202 | Dev guide + quy tắc nhóm |

Báo cáo tiến độ: `docs/pm-report.md` (§6: 15 vấn đề review đã xử lý; §7: review navigation đã áp dụng).

### 2.2 Các review ĐÃ XỬ LÝ (đừng làm lại)

1. **Review PRODUCTION_PROMPT (G-1..G-9, D-1..D-11, A-1..A-5)**: đã vá vào v2.3-v2.5 — bảng trạng thái 22/23 đã vá tại `docs/REVIEW_PRODUCTION_PROMPT.md`.
2. **Review toàn diện docs/ (15 vấn đề)**: catalog fix encoding, điền tên thật 4 thành viên + GVHD, banner "đặc tả dự kiến — code v2 chưa khởi tạo" ở SRS/SDD/DEPLOY/README, đánh dấu HANDOFF_REBUILD/SESSION_HANDOFF = FILE LỖI THỜI, DEPLOY tách Linux/Windows, TEST_PLAN banner PLAN≠REPORT...
3. **Review navigation (7 vấn đề)**: áp dụng v2.6 — sidebar mới (Student: Lộ trình/Khám phá/Hồ sơ/Thử thách/Lớp học; Teacher: +Quản lý nội dung/Báo cáo; Admin: +Nội dung), `/simulations` = Màn 33 "Khám phá" trên sidebar (tab Benchmark + CheatSheet), phân biệt "học theo lộ trình" vs "xem mô phỏng tự do".

### 2.3 Quyết định ĐÃ CHỐT (KHÔNG hỏi lại — trừ khi user chủ động đổi)

- **Tim gating (20.4)**: mọi lượt "vào node"/mở mô phỏng trừ 1 tim atomic (trừ: session 30p, node đã pass, Benchmark, Bậc 2/3 cùng node); mở từ CheatSheet VẪN trừ; cơ chế chống double-spend v2.5 (UPDATE điều kiện `ExpiresAt < @now` + @@ROWCOUNT + UNIQUE NodeSessions). **Playground không trừ tim ĐÃ BỊ TỪ CHỐI** (mâu thuẫn 20.4) — ghi backlog.
- **Premium = checkout MÔ PHỎNG** (không cổng thanh toán thật); downgrade giữ gems/items, clamp Hearts 10.
- **Chấm Code Bậc 3**: sandbox Web Worker phía client, KHÔNG Judge0; test ẩn đóng gói bundle — mức cam kết "chống lười làm", KHÔNG cam kết chống trích xuất/giả mạo.
- **Lab Bậc 2**: chấm TRẠNG THÁI CUỐI + số bước ≤ chuẩn × 1.5 (không chấm trace từng bước).
- **Seed**: 8 bài chất lượng cao + ~90 test ẩn; 10 bài còn lại → backlog GĐ2.
- **Backend**: 2 project, không Repository, Result<T> + FluentValidation + ErrorCodes; GamificationService 1 seam (ADR-011); chấm code client (ADR-012).
- **12 FR đã cắt** (1.10, 2.7, 2.8, 2.9, 3.13, 3.17, 3.19, 5.6, 5.7, 6.4, 7.3, 7.5) — KHÔNG sinh đặc tả/không code.
- **Navigation v2.6**: sidebar theo vai trò như mục 2.2.3; redirect `/learn`→`/path`, `/dashboard`→`/profile`.

## 3. VIỆC TIẾP THEO (ưu tiên giảm dần)

1. **Sinh báo cáo Word** theo `docs/BAO_CAO_SPEC.md` (session A6): `tailieu/BAO_CAO.md` → `tailieu/BaoCaoDoAn.docx` bằng pandoc; ẢNH PLACEHOLDER 1920×1080 sinh bằng PowerShell System.Drawing vào `tailieu/placeholders/`; 6 prompt NHÓM B → `tailieu/diagram-prompts.md` (đã có DIAGRAM_PROMPTS.md làm khung).
2. **Bê code tái dùng** từ `VisualizationDSA/` theo `docs/PM_MASTER_PLAN.md` task B1: ghi `docs/REUSE_REPORT.md` (bảng file cũ → bê/không/lý do/nơi đích), copy vào `frontend/` + `backend/` skeleton (theo SDD §3.1/§5.1), ưu tiên: design-system, Canvas renderers (chỉnh theo EDV), generator logic, seeder pattern, docker-compose/nginx. CẤM copy: PostgreSQL-specific, Repository pattern, feature đã cắt, secret.
3. **Khởi tạo repo v2** + cập nhật tài liệu khớp code thật (SRS/SDD hiện là "đặc tả dự kiến" — ghi rõ).
4. **Chờ phê duyệt SRS/SDD bởi giảng viên** trước khi code (điểm cổng).

## 4. QUY TẮC LÀM VIỆC (bắt buộc)

1. **Thứ tự ưu tiên nguồn khi mâu thuẫn** (prompt §21): Phần 20/21 > Phần 8 (EDV) = Phần 7 (1 màn 1 việc) > Phần 19 > Phần 0-17. Changelog v2.6 = bản mới nhất.
2. **Sửa nguồn trước, mapping sau**: thay đổi gì → sửa `docs/PRODUCTION_PROMPT.md` (kèm dòng changelog v2.x) → mới đồng bộ SRS/SDD/API/TEST/USER_GUIDE/DEPLOY/SCREEN_MAP/docs README.
3. **ID bất biến**: FR/NFR/UC/API/TB/TEST lấy nguyên văn từ PRODUCTION_PROMPT; cấm phát minh ID mới.
4. **Tên thật**: Người soạn = thành viên phụ trách (Mai Tiểu Bảo / Thái Quang Sơn / Huỳnh Lê Minh Thư / Trần Viết Tâm Phúc), Người duyệt = Phạm Ngọc Ái Liên — KHÔNG để `[Tên]`.
5. **Encoding**: dùng Read/Write/Edit tool hoặc .NET `[System.IO.File]::ReadAllText(path, UTF8)` + `WriteAllText(path, text, UTF8NoBOM)` — CẤM `Set-Content`/`Get-Content` thuần (đã từng gây double-encode catalog).
6. **Kiểm tra cuối**: checklist §17.9 + grep chéo ID + đếm dòng theo bảng §17.2 (độ dài chuẩn tại docs/README §1.1).
7. **Không bịa số liệu** (bài học báo cáo cũ): chưa chạy test → ghi "chờ tuần 19-20".
8. **CẤM commit** secret/.env/appsettings chứa mật khẩu/node_modules/bin/obj/dist (bài học bản cũ: secret lộ git).

## 5. FILE CẦN ĐỌC TRƯỚC KHI LÀM (tùy task)

| Task | Đọc |
|---|---|
| Báo cáo Word | BAO_CAO_SPEC.md + DIAGRAM_PROMPTS.md + 12 file bàn giao |
| Bê code | PM_MASTER_PLAN.md (task B1) + SDD §3/§5 + AGENTS.md cũ |
| Khởi tạo repo | SDD §3.1/§5.1 + README.md root + HANDOFF_REBUILD (chỉ để tham khảo lịch sử) |
| Sửa 1 tính năng | PRODUCTION_PROMPT phần tương ứng + SRS §3 + SDD phần tương ứng + SCREEN_MAP |
| Viết test | TEST_PLAN + SDD §4.8 (golden data) + SDD §4.9A/4.9B (trace chuẩn) |

## 6. LƯU Ý KỸ THUẬT MÔI TRƯỜNG

- Console PowerShell hiển thị lỗi font tiếng Việt (`S?p x?p`) — ĐÓ LÀ HIỂN THỊ, file vẫn đúng UTF-8; verify bằng `Contains()`/IndexOf, không tin console.
- `shared/simulation-catalog.json` là nguồn duy nhất khóa `key` FE/BE — CI so sánh với `engines/catalog.ts`, khác → fail build.
- MCP/agents đã cài: playwright, github, context7, chrome-devtools, memory; agent pm + dev/dev-frontend/dev-backend.
