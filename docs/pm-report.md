# PM REPORT — ĐỢT SẢN XUẤT TÀI LIỆU (12/08/2026)

**Mục tiêu**: Review PRODUCTION_PROMPT.md bằng 5 skill → kiểm tra docs tuân thủ → sản xuất 12 file bàn giao theo §17.1.

## 1. Kết quả review PRODUCTION_PROMPT.md (v2.5 — 5324 dòng)

| Skill | Kết luận |
|---|---|
| grill-with-docs | Ổn: G-1..G-9 đã vá (seed 8 bài, bỏ Judge0, Lab chấm trạng thái cuối, giải trình KPI, dọn 12 FR cắt, sạch NOTIFICATIONS) |
| domain-modeling | Ổn: glossary §1.8 đủ 8 thuật ngữ miền; thuật ngữ Node/Bậc/Session/Tim nhất quán |
| database-designer | Ổn: 31 bảng đầy đủ cột/index/seed; ERD 2 sơ đồ; CHECK/unique đã bổ sung |
| codebase-onboarding | Thiếu hạ tầng docs — đây là phần đã sản xuất trong đợt này |
| improve-codebase-architecture | Ổn: A-1..A-5 đã vá (2 project, gộp service, bỏ /simulations/run) |

**Không phát hiện mâu thuẫn blocking mới → KHÔNG cần sửa PRODUCTION_PROMPT.md.**

## 2. Trạng thái 12 file bàn giao (trước → sau)

| File | Trước | Sau | Chuẩn (§17.2) |
|---|---|---|---|
| docs/SRS.md | ❌ 247 dòng (bản cũ) | ✔ 1296 dòng v1.0 | ≥900 |
| docs/SDD.md | ❌ 364 dòng (bản cũ) | ✔ 2042 dòng v1.0 | ≥1400 |
| docs/API_REFERENCE.md | ❌ không tồn tại | ✔ 730 dòng v1.0 | ≥700 |
| docs/USER_GUIDE.md | ❌ không tồn tại | ✔ 503 dòng v1.0 | ≥500 |
| docs/TEST_PLAN.md | ❌ không tồn tại | ✔ 759 dòng v1.0 | ≥600 |
| docs/DEPLOY.md | ❌ không tồn tại | ✔ 369 dòng v1.0 | ≥300 |
| docs/GLOSSARY.md | ❌ không tồn tại | ✔ 105 dòng v1.0 | ≥100 |
| docs/README.md | ❌ 21 dòng | ✔ 206 dòng (ma trận 17.8 + FR↔UC↔Module) | mục lục |
| docs/SCREEN_MAP.md | ⚠ 251 dòng | ✔ 306 dòng (thêm mục 10/10A/11 — v2.5) | ≥300 |
| shared/simulation-catalog.json | ❌ không tồn tại | ✔ 46 dòng / 44 entries (34 GT + 10 CTDL) — JSON hợp lệ | ≥40 |
| THIRD_PARTY.md | ❌ không tồn tại | ✔ 72 dòng (NFR-36) | ≥40 |
| README.md (root) | ❌ không tồn tại | ✔ 202 dòng (dev guide + quy tắc nhóm 2.7) | ≥200 |

**Tổng: 8/12 mới hoặc thiếu → đã đạt 12/12 chuẩn.**

## 3. Kết quả checklist §17.9 (rà soát tự động + thủ công)

- [x] Không placeholder `[...]`/`TODO`/`XXX` trong nội dung mô tả (grep).
- [x] ID nhất quán: FR/NFR/UC/TEST grep chéo khớp; "30 bảng" còn sót = 0.
- [x] SRS: 10 module A-J, master matrix, 32 UC (đếm = 32), 36 NFR, AC-1..8.
- [x] SDD: Phần 8 EDV TOÀN BỘ, 15 GT mã giả + bảng trạng thái, 31 bảng (2 ERD), 32 màn.
- [x] API_REFERENCE: mọi endpoint §9.2 (Auth/Public/Topics/Lessons/Simulations/Exercises/Progress/Users/Favorites/Admin/Classes/Notes/CodeRunner/Gamification/Feedback) + error catalog + DTO + RBAC 36.
- [x] DB: 31 bảng đủ cột/khóa/index/seed (gồm NodeSessions §7.3.29).
- [x] RBAC 36 dòng khớp endpoint (mọi endpoint có quyền tối thiểu).
- [x] TEST_PLAN phủ 100% FR mức Cao + ma trận truy vết §11 (17.15).
- [x] Kiểm thử bảo mật 13.3 đầy đủ (TEST-SEC-001..011).
- [x] Trừ tim ≥ 3 case biên: TEST-B-150 (CheatSheet), TEST-B-151 (concurrency thực), TEST-B-152 (session hết hạn) + TEST-B-148..155.
- [x] USER_GUIDE không thuật ngữ kỹ thuật ngoài bảng giải thích.
- [x] Mermaid hợp lệ (classDiagram/erDiagram/sequenceDiagram/graph/stateDiagram — cú pháp v10).
- [x] docs/README.md có ma trận ánh xạ + danh sách 12 file.
- [x] 12 file bàn giao đủ; SCREEN_MAP phủ Màn 01-32 + N-1..N-16.
- [x] Lịch sử thay đổi từng file có bản 1.0.

## 4. Quyết định thiết kế đã chọn khi sinh (17.7)

| Vấn đề | Chọn |
|---|---|
| Rich text editor | Quill |
| Chart library | Chart.js |
| Icon | lucide-vue-next |
| Testcontainers | Có (Docker) |
| Frontend ngôn ngữ | TypeScript strict |

## 5. Việc còn lại (đề xuất)

1. Sinh báo cáo Word theo BAO_CAO_SPEC (session A6 — cần pandoc + ảnh placeholder).
2. Bê code tái dùng từ VisualizationDSA theo PM_MASTER_PLAN task B1 (REUSE_REPORT.md).
3. Phê duyệt tài liệu bởi giảng viên (SRS/SDD) trước khi khởi tạo code.

## 6. Xử lý review toàn diện docs/ (12/08/2026 — 15 vấn đề)

| # | Vấn đề | Trạng thái |
|---|---|---|
| 1 | Encoding `simulation-catalog.json` (34 entry vỡ tiếng Việt) | ✅ Đã viết lại UTF-8 đúng — verify `Sắp xếp nổi bọt` tìm thấy chính xác |
| 2 | Docs v2 vs code v1 (PostgreSQL/Clean Architecture) | ✅ Ghi rõ "đặc tả dự kiến — code v2 chưa khởi tạo" ở SRS/SDD/DEPLOY/README root/docs README |
| 3 | SCREEN_MAP 251 < 300 dòng | ✅ Đạt 306 dòng (đã có từ đợt sản xuất); docs/README cập nhật số dòng |
| 4 | Placeholder `[Tên]` khắp nơi | ✅ Điền tên thật 4 thành viên + GVHD Phạm Ngọc Ái Liên vào 12 file (phân công theo BAO_CAO_SPEC §4.1) |
| 5 | demoAllowed chỉ 3/44 | ⚠ GIỮ NGUYÊN — đúng thiết kế FR-7.6 (3 demo công khai); mở rộng demo là quyết định sản phẩm, ghi backlog |
| 6 | HANDOFF_REBUILD + SESSION_HANDOFF lỗi thời | ✅ Thêm banner "FILE LỖI THỜI/LỊCH SỬ" chỉ rõ chuẩn hiện tại (10 module, 32 UC, 31 bảng) |
| 7 | REVIEW_PRODUCTION_PROMPT chưa đóng | ✅ Thêm bảng "TRẠNG THÁI XỬ LÝ" — 22/23 vấn đề đã vá, 1 điểm phụ thuộc quyết định người dùng (giữ Module J) |
| 8 | Mâu thuẫn số bảng DB (24 vs 31 vs ~17) | ✅ Giải quyết qua banner lỗi thời #6 — chuẩn hiện tại 31 bảng nhất quán SRS/SDD/API/TEST |
| 9 | PRODUCTION_PROMPT 405KB quá lớn | ⚠ GIỮ — là nguồn yêu cầu nội bộ (single source of truth), KHÔNG đưa vào bộ bàn giao hội đồng; docs/README ghi rõ vai trò |
| 10 | USER_GUIDE lý tưởng hóa UX chưa có | ✅ Thêm banner "theo đặc tả dự kiến — cập nhật sau khi UI hoàn thiện" |
| 11 | TEST_PLAN là plan không phải report | ✅ Thêm banner nhấn mạnh + cam kết không bịa số liệu |
| 12 | Cross-reference có thể lệch | ⚠ Grep chéo thủ công đạt (checklist 17.9); script tự động ghi vào backlog |
| 13 | pm-report quá ngắn | ✅ Bổ sung mục 6 (bảng xử lý 15 vấn đề) |
| 14 | DEPLOY trộn Linux/Windows path | ✅ Tách rõ: 4.1-4.4 Linux, 4.5 Windows, backup path theo từng OS |
| 15 | `tailieu/NET202_Project document_6 (1).pdf` | ✅ Xác nhận TỒN TẠI (đã kiểm tra) — không cần sửa |

## 7. Xử lý review navigation & luồng di chuyển (12/08/2026)

> Nguồn: review "TÍNH NĂNG & LUỒNG DI CHUYỂN" (7 vấn đề + 3 đề xuất tính năng). Nguyên tắc: sửa PRODUCTION_PROMPT (v2.6) trước, rồi đồng bộ SDD/USER_GUIDE/SCREEN_MAP.

| # | Vấn đề | Quyết định | File đã sửa |
|---|---|---|---|
| 1 | "Học tập" quá chung; không có đường vào xem GT tự do | ✅ Đổi **"Lộ trình"** `/path` + thêm **"Khám phá"** `/simulations` lên sidebar | PROMPT §20.5.2, SDD §8.7/§3.3/§8.4 (Màn 33), USER_GUIDE, SCREEN_MAP |
| 2 | Quest/Leaderboard chìm trong "⋯ Thêm" | ✅ Đưa **"Thử thách"** `/quests` lên sidebar chính | như trên |
| 3 | Benchmark không có đường vào | ✅ Tab "So sánh" bên trong **Khám phá** | như trên |
| 4 | CheatSheet bị ẩn | ✅ Tab "CheatSheet" bên trong **Khám phá** (vẫn giữ `/cheatsheet` route riêng) | như trên |
| 5 | Teacher "Soạn bài" nhập nhằng | ✅ Đổi **"Quản lý nội dung"** `/admin/*` | như trên |
| 6 | Admin thiếu lối vào nội dung | ✅ Thêm **"Nội dung"** `/admin/lessons` | như trên |
| 7 | Xem thuật toán ở 3 nơi | ✅ Phân biệt rõ: Lộ trình (học theo trình tự, trừ tim, ghi điểm) vs Khám phá (tự do, trừ tim trừ 3 demo) — ghi chú cả 2 nơi | SDD §8.7, USER_GUIDE |
| 5.1 | Teacher Hub `/teacher/dashboard` | ⏸ BACKLOG (16.2) — tránh scope trôi dạt | — |
| 5.2 | Playground không trừ tim | ⏸ **KHÔNG áp dụng** — mâu thuẫn trực tiếp quyết định 20.4 đã chốt (mọi lượt xem mô phỏng đều trừ tim; nội dung là giá trị lõi); ghi backlog nếu đổi chính sách | — |
| 5.3 | Widget "Hôm nay" trên Home sau login | ⏸ BACKLOG (tùy chọn GĐ3, chi phí thấp) | — |

> Mọi thay đổi CHỈ ảnh hưởng sidebar + 1 route (`/simulations` trở thành Màn 33 chính thức) — KHÔNG đụng FR/kiến trúc/business logic. Changelog v2.6 đã ghi vào PRODUCTION_PROMPT Phần 22.
---

## 8. Kết luận bàn giao — Batch 2FA Gmail SMTP + final verification (17/08/2026)

> Nội dung này kết thúc batch đang mở: sửa lỗi JSON appsettings.Development.json (smtp.gmail.com) + re-verify toàn bộ + đóng trạng thái bàn giao. Quy ước đóng trạng thái: **PARTIAL** (automated verification PASS, Task 4/6 BLOCKED) hoặc **DONE** (chỉ khi PM xác nhận/duyệt rõ 2 blocker deferred).

### 8.1 Kết quả automated verification (lệnh thật, chạy lại trong batch này)

| Hạng mục | Lệnh | Kết quả |
|---|---|---|
| JSON hợp lệ | ConvertFrom-Json + python json.load | **PASS** — keys: DSA, ConnectionStrings, Serilog |
| Secret scan diff | git diff + pattern scan (AKIA/ghp_/BEGIN/sk-/smtp user-pass/app password) | **SẠCH** — diff chỉ thêm 2 dấu phẩy (Email.SmtpHost, Email.SmtpPort); match duy nhất là hostname smtp.gmail.com |
| dotnet build | dotnet build DsaVisual.sln -c Debug | **PASS** — Build succeeded, 0 Warning(s), 0 Error(s), exit 0 |
| dotnet test | dotnet test DsaVisual.sln -c Debug --no-build | **PASS** — Unit 159/159 + Integration 78/78 = **237/237**, exit 0 (DsaVisual.Api.Tests: scaffold rỗng — 0 test source, 'No test is available') |
| npm run build | npm run build (vue-tsc + vite) | **PASS** — vite built in 2.06s, exit 0 |
| npx vitest run | npx vitest run | **PASS** — **23 files / 207 tests** passed (207), exit 0 (stderr 'canvas getContext not implemented' = jsdom noise, không ảnh hưởng) |
| Docker thực tế | docker ps + docker compose ps -a | Xem 8.2 |
| Git sau push | git status --porcelain -b + git rev-parse HEAD/origin/dev | **CLEAN** — HEAD = origin/dev = d05f36f → sau report commit: xem 8.6 |

### 8.2 Môi trường Docker thực tế (đúng trạng thái hiện hữu, không lý tưởng hóa)

- **Backend** neww-backend-1 (compose project 'neww'): **Up (healthy)**, 0.0.0.0:5000->8080/tcp, health HTTP 200. Env dùng DSA__Email__SmtpHost=mailhog / SmtpPort=1025 (env override file) — container không bị ảnh hưởng bởi appsettings.
- **Database**: compose neww-sqlserver-1 **Exited (255)** — KHÔNG chạy; SQL Server đang hoạt động trong batch này là **testcontainer mssql 2022** (Testcontainers spawn khi chạy dotnet test, port host random ->1433, có Ryuk reaper). Ngoài ra legacy vdsa-database postgres:15 (5433) của stack V1 đang Up (leftover, KHÔNG thuộc compose hiện tại).
- **Frontend**: compose neww-frontend-1 **Exited (255)** 21h trước; không có FE dev server trên 5173/8081/5001 (probe connect fail) — FE chạy qua local build/CI, không qua Docker lúc này.
- **MailHog**: compose neww-mailhog-1 **Exited (255)**; port 8025/1025 không listen — MailHog hiện KHÔNG chạy.
- **Redis**: vdsa-redis (6379) đang Up nhưng thuộc legacy stack V1 — **compose hiện tại KHÔNG có Redis** (SDD §5.6: 'KHÔNG Postgres/Redis/Judge0/Cloudinary'). Báo cáo đúng theo môi trường thực tế.

### 8.3 Task 4 — [BLOCKED] Xác thực OTP Gmail THẬT (2FA)

Chưa thể kết luận DONE: cần **PM xác nhận trên môi trường thật** 3 điều:
1. Tài khoản Gmail thật **nhận được** email chứa mã OTP gửi qua SMTP smtp.gmail.com:587.
2. Nhập mã vào POST /auth/2fa/verify → **verify thành công** (đúng 1 lần).
3. **Dùng lại mã cũ** → bị từ chối (OTP_USED, mã dùng 1 lần — AC-1.11.2).

Lưu ý kỹ thuật (không phải blocker mới): code SMTP (AuthService.cs / UserService.cs) chỉ đọc DSA:Email:SmtpHost/Port/From — **chưa** có SmtpUsername/SmtpPassword trong code lẫn config; Gmail yêu cầu AUTH bằng **App Password** — phải cấp qua env var DSA__Email__* ở môi trường chạy (SETUP_TODO §10.1), **KHÔNG** nhập vào appsettings để commit. KHÔNG yêu cầu PM dán app password vào chat.

### 8.4 Task 6 — [BLOCKED] Ngày bảo vệ + ngành học (bìa BAO_CAO)

Chưa thể kết luận DONE: cần **PM cung cấp** (1) ngày bảo vệ chính xác và (2) ngành học chính xác để điền vào bìa BAO_CAO — mục 'Ngày bảo vệ: .....' hiện vẫn để trống (đúng quy định PROMPT_REPORT_PRO2192: chỉ điền khi user cho ngày).

### 8.5 Tuân thủ ràng buộc batch

- **KHÔNG** reset database; **KHÔNG** tắt 2FA hiện tại; **KHÔNG** yêu cầu app password trong chat — tất cả giữ nguyên.

### 8.6 KẾT LUẬN BÀN GIAO: **PARTIAL**

> Automated verification **PASS toàn bộ** (JSON, secret scan, dotnet build/test 237/237, FE build + vitest 23 files/207 tests, git clean + in sync). **Task 4 và Task 6 BLOCKED** chờ PM. Theo quy ước, trạng thái bàn giao = **PARTIAL** — chuyển sang **DONE** chỉ khi PM xác nhận/duyệt rõ 2 blocker deferred (OTP Gmail thật verified + cung cấp ngày bảo vệ/ngành học).
### 8.7 Cập nhật batch 2 (17/08/2026) — XỬ LÝ BLOCKER SMTP KỸ THUẬT (commit 30e4316, pushed origin/dev)

| Hạng mục | Kết quả |
|---|---|
| EmailOptions | MỚI `backend/src/DsaVisual.Application/Options/EmailOptions.cs` — map section `DSA:Email` (appsettings / env `DSA__Email__*`): SmtpHost, SmtpPort, SmtpUsername, SmtpPassword, From, UseMailHog (mặc định true = MailHog dev) |
| SmtpClientFactory | MỚI `Services/SmtpClientFactory.cs` — tập trung AUTH/TLS: `UseMailHog=false` → `EnableSsl=true` (TLS/STARTTLS, Gmail 587) + `Credentials=NetworkCredential(username,password)` khi có đủ; `UseMailHog=true` → không AUTH/TLS (MailHog); Timeout 10s (GP-T2) |
| AuthService / UserService | 3 điểm gửi email (reset password, mã 2FA, duyệt/từ chối GV) chuyển sang `SmtpClientFactory.Create(EmailOptions.FromConfiguration(config))` — KHÔNG còn `new SmtpClient(host,port)` raw, không credential ẩn |
| Config mapping | appsettings.json (base): thêm SmtpUsername/SmtpPassword/UseMailHog=true (host rỗng = email tắt) · appsettings.Development.json: **MailHog** localhost:1025 UseMailHog=true (phục hồi email dev; bỏ hardcode gmail không AUTH) · appsettings.Production.json: **Gmail smtp.gmail.com:587 UseMailHog=false** (creds rỗng — qua env) · docker-compose: `DSA__Email__UseMailHog`=env default true + passthrough `DSA__Email__SmtpUsername/SmtpPassword` · backend/.env.example: tài liệu hoá; SETUP_TODO §10.1 cập nhật |
| Secret | **SẠCH** — `SmtpPassword` chỉ là chuỗi rỗng `""`/`=` trong file cấu hình + text placeholder '<Gmail App Password>' trong docs; KHÔNG có giá trị mật thật trong diff/commit/log/report |
| dotnet build | `dotnet build DsaVisual.sln` → **Build succeeded, 0 Warning(s), 0 Error(s), exit 0** |
| dotnet test | **Unit 159/159 + Integration 78/78 = 237/237 PASS, exit 0** (chạy trên binary MỚI sau khi build PASS — vòng 1 build fail do thiếu using đã sửa) |
| FE build + vitest | `npm run build` PASS (2.48s) + `npx vitest run` **23 files / 207 tests PASS, exit 0** |
| Docker (4 nhóm) | (1) **compose Up**: neww-backend-1 :5000 healthy — (2) **Testcontainers**: mssql 2022 + ryuk spawn trong lúc `dotnet test` rồi tự dọn — (3) **legacy**: vdsa-database postgres:5433 + vdsa-redis:6379 Up (stack V1) — (4) **Exited**: neww-sqlserver-1, neww-mailhog-1, neww-frontend-1 (compose), vdsa-frontend/backend/judge0 (V1); ui-premium-* trạng thái Created (worktree khác) |
| Git | **CLEAN — HEAD = origin/dev = 30e4316** |

### 8.8 RÀNG BUỘC (giữ nguyên)

- **KHÔNG** reset database; **KHÔNG** tắt 2FA; **KHÔNG** yêu cầu app password trong chat.
- Task 4 OTP Gmail THẬT: runtime nay **SẴN SÀNG** (Production mặc định Gmail AUTH/TLS qua env `DSA__Email__SmtpUsername/SmtpPassword`) → chờ **PM xác nhận thủ công 3 điều**: (1) OTP đến Gmail thật, (2) verify lần đầu thành công, (3) dùng lại OTP bị từ chối.
- Task 6 ngày bảo vệ + ngành học: **tiếp tục BLOCKED** cho tới khi PM cung cấp số liệu chính xác.

### 8.9 KẾT LUẬN BÀN GIAO: **PARTIAL (giữ nguyên)**

> Toàn bộ automated verification **PASS** trên code MỚI (build 0W/0E, 237/237 BE tests, FE build + 207 tests, secret scan sạch, git clean + HEAD=origin/dev). **DONE chỉ khi**: (a) Gmail SMTP AUTH chạy thực tế và PM xác nhận OTP flow 3/3, (b) Task 6 đã điền hoặc PM xác nhận chính thức deferred, (c) git clean + HEAD=origin/dev, (d) toàn bộ automated verification vẫn PASS.
