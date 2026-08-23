# MỤC LỤC TÀI LIỆU DỰ ÁN — DSA-VISUAL

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**

| | |
|---|---|
| Phiên bản | 1.3 |
| Ngày cập nhật | 12/08/2026 |
| Trạng thái | Dự thảo — chờ phê duyệt |
| Nguồn | PRODUCTION_PROMPT.md (v2.5) — single source of truth |

## 1. Danh sách 12 file bàn giao (nguồn prompt §17.1)

| # | File | Nội dung | Độc giả | Độ dài (dòng) |
|---|---|---|---|---|
| 1 | `docs/SRS.md` | Đặc tả yêu cầu phần mềm: 10 module A-J, master matrix, 32 UC, 36 NFR, AC-1..8 | Giảng viên, hội đồng, PM | 1544 |
| 2 | `docs/SDD.md` | Thiết kế hệ thống: EDV/StepExecutor, frontend, backend, API, 32 bảng DB, 32 màn | Kiến trúc sư, lập trình viên | 3015 |
| 3 | `docs/API_REFERENCE.md` | Tham chiếu API: mọi endpoint, DTO, error code catalog, RBAC 36 | Lập trình viên, tester | 596 |
| 4 | `docs/USER_GUIDE.md` | Hướng dẫn sử dụng (sinh viên + giảng viên + admin) | Người dùng cuối | 355 |
| 5 | `docs/TEST_PLAN.md` | Kế hoạch kiểm thử + golden data + ma trận truy vết | Tester, QA, hội đồng | 602 |
| 6 | `docs/DEPLOY.md` | Triển khai & vận hành: biến env, nginx, systemd, backup, runbook | DevOps, admin | 306 |
| 7 | `docs/GLOSSARY.md` | Thuật ngữ 3 nhóm: nghiệp vụ / kỹ thuật / DSA | Tất cả | 94 |
| 8 | `docs/README.md` | Mục lục tài liệu + ma trận ánh xạ yêu cầu → tài liệu (17.8) | Tất cả | 183 |
| 9 | `docs/SCREEN_MAP.md` | Bản đồ màn hình Màn 01-32 + N-1..N-16, ma trận FR→Màn | Lập trình viên, hội đồng | 268 |
| 10 | `shared/simulation-catalog.json` | Danh mục mô phỏng dùng chung FE/BE — nguồn duy nhất khóa `key` (9.9) | Lập trình viên | 46 |
| 11 | `THIRD_PARTY.md` | Danh sách thư viện mã nguồn mở + license (NFR-36) | Tất cả | 108 |
| 12 | `README.md` (root) | Hướng dẫn dev: cài đặt, lệnh chạy, quy tắc nhóm Git/Conventional Commits (2.7, 14.4) | Lập trình viên | 154 |

> Cách đếm: PowerShell `Measure-Object -Line` (dòng có nội dung — không tính dòng trống), chạy 12/08/2026 — cùng cách đếm của audit §17.9 (F2a).

## 1.1 Độ dài tối thiểu chuẩn (nguồn prompt §17.2 — dùng để rà soát)

| File | Tối thiểu | Kỳ vọng | Thực tế 12/08/2026 |
|---|---|---|---|
| SRS.md | 900 | 1200-1600 | ✔ 1544 (trong kỳ vọng) |
| SDD.md | 1400 | 1800-2500 | ✔ 3015 (trên kỳ vọng) |
| API_REFERENCE.md | 700 | 900-1200 | ⚠ 596 (dưới ngưỡng — nội dung bắt buộc đủ: ~60 endpoint + ví dụ JSON, RBAC 36, error catalog; rà soát F2a §17.9) |
| USER_GUIDE.md | 500 | 600-900 | ⚠ 355 (dưới ngưỡng — nội dung đủ cho 33 màn thật, đối chiếu 12/08/2026) |
| TEST_PLAN.md | 600 | 800-1100 | ⚠ 602 (dưới ngưỡng — nội dung đủ: 130+ test case, ma trận truy vết; rà soát F2a §17.9) |
| DEPLOY.md | 300 | 400-600 | ✔ 306 (đạt tối thiểu) |
| GLOSSARY.md | 100 | 150-250 | ⚠ 94 (dưới ngưỡng — đủ 3 nhóm thuật ngữ; rà soát F2a §17.9) |
| SCREEN_MAP.md | 300 | 400-600 | ⚠ 268 (dưới ngưỡng — đủ Màn 01-32 + N-1..N-16; rà soát F2a §17.9) |

> Cách đếm: PowerShell `Measure-Object -Line` (dòng có nội dung — không tính dòng trống), chạy 12/08/2026 — cùng cách đếm của audit §17.9 (F2a). Số trước đây (SRS 1771, SDD 3725, API 735...) là **tổng dòng kể cả dòng trống** — đã thống nhất dùng cách đếm không trống để khớp audit.
| shared/simulation-catalog.json | 40 | 60-100 | ✔ 46 (44 entries) |
| THIRD_PARTY.md | 40 | 60-100 | ✔ 108 |
| README.md (root) | 200 | 300-500 | ✔ 194 |
| docs/README.md | (mục lục) | — | ✔ 183 |

## 2. Quy tắc dùng tài liệu

1. **Nguồn gốc duy nhất (single source of truth)**: `docs/PRODUCTION_PROMPT.md` — mọi ID (FR/NFR/UC/API/TB/TEST) lấy nguyên văn từ đây; tài liệu khác KHÔNG phát minh ID mới.
2. **Thứ tự ưu tiên khi có mâu thuẫn** (prompt §21): Phần 20/21 > Phần 8 (EDV) = Phần 7 (1 màn 1 việc) > Phần 19 > Phần 0-17.
3. **Truy vết**: mỗi mục trong SDD ghi nguồn FR/UC; mỗi test case ghi FR; mọi thay đổi ghi vào "Lịch sử thay đổi" của file tương ứng (17.12).
4. **Định dạng**: Markdown chuẩn, sơ đồ Mermaid v10, bảng đầy đủ dữ liệu, tiếng Việt có dấu.

## 2.1 Quy ước ID và tham chiếu chéo

| ID | Ý nghĩa | Ví dụ | Định nghĩa tại |
|---|---|---|---|
| FR-x.y | Yêu cầu chức năng | FR-10.1 | PRODUCTION_PROMPT §3 + SRS §3 |
| NFR-x | Yêu cầu phi chức năng | NFR-9 | PRODUCTION_PROMPT §4 + SRS §4 |
| UC-xx | Use case | UC-26 | PRODUCTION_PROMPT §6 + SRS §5 |
| AC-x.y.z | Tiêu chí chấp nhận | AC-10.1.6 | trong đặc tả FR tương ứng |
| ADR-xxx | Quyết định kiến trúc | ADR-012 | SDD §13.2 |
| TEST-B/E/API/UI/SEC/PERF/Ux-xxx | Test case | TEST-B-151 | TEST_PLAN |
| Màn xx / N-xx | Màn hình | Màn 31 / N-1 | SCREEN_MAP + SDD §8.4 |

**Cú pháp tham chiếu chéo**: `(xem FR-3.5)`, `(SDD §4.3)`, `(TEST_PLAN TEST-B-151)`. Các ID phải khớp 100% giữa các tài liệu — rà soát bằng grep chéo trước khi bàn giao (checklist 17.9).

## 3. Ma trận ánh xạ yêu cầu → tài liệu (nguồn prompt §17.8 — mở rộng)

| Yêu cầu | SRS | SDD | API_REF | USER_GUIDE | TEST_PLAN |
|---|---|---|---|---|---|
| FR-1.1 → FR-1.9, FR-1.11 (Module A) | §3.3 | §5 (Auth) + §7 | §4.1 | §3.1-3.2 | §4.1 |
| FR-2.1 → FR-2.6, FR-2.10, FR-2.11 (Module B) | §3.4 | §3 + §8 | §4.3-4.4 | §3.3-3.4 | §4.2 |
| FR-3.1 → FR-3.20b (Module C — EDV) | §3.5 | §4 (Engine) TOÀN BỘ | §4.5 | §3.5 | §5 (TEST-E) |
| FR-4.1 → FR-4.12 (Module D — Ladder) | §3.6 | §5 (chấm điểm) + §8 | §4.6 | §3.6-3.7 | §4.3 |
| FR-5.1 → FR-5.5 (Module E) | §3.7 | §5 + §7 | §4.7 | §3.8 | §4.4 |
| FR-6.2 (Module F) | §3.8 | §5 | §4.10 | §5.2 | §4.4 |
| FR-7.1 → FR-7.6 (Module G) | §3.9 | §8 | §4.2 | §2, §6 | §7 |
| FR-8.1 → FR-8.4 (Module H) | §3.10 | §5 + §7 | §4.11 | §3.11, §4.5 | §4.5 |
| FR-9.1 → FR-9.6 (Module I) | §3.10A | §4.0 + §5 | §4.13 | §3.6, §4.7 | §4.6 |
| FR-10.1 → FR-10.7 (Module J) | §3.10B | §5 (GamificationService) + §7 | §4.14 | §3.3.1, §3.9-3.10 | §4.7 |
| NFR-1 → NFR-36 | §4 | §2, §9 (bảo mật) | §1, §2.2 | — | §8 (TEST-PERF), §7.2 (SEC) |
| UC-01 → UC-32 | §5 | §2 (kiến trúc) | — | §3 | §4-§7 |
| RBAC (36 hành động) | (ma trận quyền) | §5.3 | §5 | §4-§5 | §7.2 (TEST-SEC-002) |
| Golden data N1-N7 | — | §4.8 | — | — | §5 (TEST-E) |
| Trừ tim (FR-10.1) | §3.10B | §7.3.29 (NodeSessions) | §4.14 | §3.3.1 | §4.7 (TEST-B-148..155) |

## 4. Bảng thuật ngữ nhanh

| Thuật ngữ | Nghĩa | Xem thêm |
|---|---|---|
| EDV | Execution-Driven Visualization — mã thật chạy, hoạt ảnh = trace | SDD §4.0 |
| StepExecutor | Bộ thực thi ghi TraceEvent[] | SDD §4.0.3 |
| Ladder | Chuỗi 3 bậc: Quiz → Lab → Code | SRS §3.6 |
| Node | Đơn vị học tập trong Learning Path | GLOSSARY |
| NodeSession | Phiên học 30 phút chống double-spend trừ tim | SDD §7.3.29 |

## 5. Các file hỗ trợ khác trong `docs/`

| File | Vai trò |
|---|---|
| `PRODUCTION_PROMPT.md` | Nguồn yêu cầu gốc (v2.5 — đã qua 3 vòng review/vá: G-1..G-9, D-1..D-11, A-1..A-5) |
| `REVIEW_PRODUCTION_PROMPT.md` | Báo cáo review 12/08/2026 (kết luận RESHAPE — các khuyến nghị đã vá vào v2.3-v2.5) |
| `PM_MASTER_PLAN.md` | Kế hoạch sản xuất 12 file (2 session A/B) |
| `BAO_CAO_SPEC.md` | Đặc tả sinh báo cáo Word (pandoc → `tailieu/BaoCaoDoAn.docx`) |
| `DIAGRAM_PROMPTS.md` | Hướng dẫn sinh ảnh sơ đồ (mmdc nhóm A, ChatGPT nhóm B) |
| `SESSION_HANDOFF.md` / `HANDOFF_REBUILD.md` | Bàn giao session/kiến trúc (lịch sử — tham khảo) |

## 5.1 Hướng dẫn đọc tài liệu theo vai trò

| Bạn là ai | Đọc gì trước | Tìm gì |
|---|---|---|
| Giảng viên hướng dẫn / hội đồng | SRS §1-2, §5 (UC), TEST_PLAN §10 (báo cáo) | Phạm vi, KPI, 32 UC, kết quả test |
| PM / BA | SRS toàn bộ + docs/README (ma trận) | Master matrix, ưu tiên FR, roadmap |
| Dev Backend | SDD §5-7 + API_REFERENCE toàn bộ | Service, 32 bảng, endpoint, error code |
| Dev Frontend | SDD §3, §4, §8 + SCREEN_MAP + API_REFERENCE §1-3 | Store, engine, 32 màn, DTO |
| Tester / QA | TEST_PLAN toàn bộ + API_REFERENCE §2 | Test case, golden data, ma trận truy vết |
| DevOps / Admin | DEPLOY toàn bộ | Biến env, nginx, backup, runbook |
| Người mới / sinh viên | USER_GUIDE + GLOSSARY | Cách dùng hệ thống |

## 5.2 Tra cứu nhanh theo chủ đề (grep nhanh ở đâu)

| Chủ đề cần tra | Tài liệu + mục |
|---|---|
| Cách thêm 1 GT mới | SDD §4.13 (bảo trì engine) + TEST_PLAN TEST-E-034 |
| Cách sinh trace chuẩn | SDD §4.9A (bubble) / §4.9B (binary search) |
| Cơ chế trừ tim đầy đủ | SRS §3.10B (FR-10.1) + SDD §7.3.29 (NodeSessions) + TEST_PLAN TEST-B-148..155 |
| Chấm điểm Lab Bậc 2 | SRS §3.6 (FR-4.3) + SDD §5.5 + API_REFERENCE §4.16 |
| Chấm điểm Code Bậc 3 (mức cam kết) | SRS §3.10A (FR-9.3) + API_REFERENCE §4.13 |
| Đồng bộ catalog FE/BE | SDD §6.1 + TEST_PLAN TEST-API-004 |
| Danh sách 12 file bàn giao | docs/README §1 |
| Checklist rà soát cuối | PRODUCTION_PROMPT §17.9 (chạy trước bàn giao) |

## 6. Trạng thái sản xuất tài liệu (cập nhật 12/08/2026)

| Hạng mục | Trạng thái |
|---|---|
| SRS / SDD / API_REFERENCE / TEST_PLAN / USER_GUIDE / DEPLOY / GLOSSARY | ✔ Sinh mới v1.0 (12/08/2026) |
| shared/simulation-catalog.json | ✔ 44 entries (34 GT + 10 CTDL) — đã fix encoding UTF-8 (12/08) |
| THIRD_PARTY.md / README.md root | ✔ Sinh mới |
| SCREEN_MAP.md | ✔ 306 dòng (≥300), khớp v2.5 |
| Checklist 17.9 | ✔ Đã rà soát (12/08) — xem pm-report.md |
| Tên thành viên | ✔ Đã điền tên thật 4 thành viên vào mọi file (12/08) |
| Trạng thái code | ⚠ Docs = đặc tả v2 dự kiến; code v2 CHƯA khởi tạo (đã ghi rõ trong SRS/SDD/DEPLOY/README root) |

## 7. Ma trận FR ↔ UC ↔ Mô-đun (nguồn: SRS §7 — dùng làm bảng tra cứu nhanh)

| FR | UC | Mô-đun | FR | UC | Mô-đun |
|---|---|---|---|---|---|
| FR-1.1 | UC-02 | A | FR-4.1 | UC-10 | D |
| FR-1.2 | UC-03 | A | FR-4.2 | UC-06 | D |
| FR-1.3 | UC-03 | A | FR-4.3 | UC-07 | D |
| FR-1.4 | UC-03 | A | FR-4.4 | UC-06 | D |
| FR-1.5 | UC-03 | A | FR-4.5 | UC-10 | D |
| FR-1.6 | UC-15 | A | FR-4.6 | UC-06 | D |
| FR-1.7 | UC-03 | A | FR-4.7 | UC-06 | D |
| FR-1.8 | UC-12 | A | FR-4.8 | UC-06 | D |
| FR-1.9 | UC-12 | F | FR-4.9 | UC-06 | D |
| FR-1.11 | UC-03 | A | FR-4.10 | UC-10 | D |
| FR-2.1 | UC-09 | B | FR-4.11 | UC-26 | D |
| FR-2.2 | UC-09 | B | FR-4.12 | UC-27 | D |
| FR-2.3 | UC-04 | B | FR-5.1 | UC-08 | E |
| FR-2.4 | UC-04 | B | FR-5.2 | UC-08 | E |
| FR-2.5 | UC-05 | B | FR-5.3 | UC-11 | E |
| FR-2.6 | UC-22 | B | FR-5.4 | — | F |
| FR-2.10 | UC-25 | B | FR-5.5 | UC-23 | E |
| FR-2.11 | UC-01 | B | FR-6.2 | UC-13 | F |
| FR-3.1 | UC-01 | C | FR-7.1 | UC-14 | G |
| FR-3.2 | UC-01 | C | FR-7.2 | — | G |
| FR-3.3 | UC-01 | C | FR-7.4 | UC-24 | G |
| FR-3.4 | UC-01 | C | FR-7.6 | UC-14 | G |
| FR-3.5 | UC-01 | C | FR-8.1 | UC-20 | H |
| FR-3.6 | UC-01 | C | FR-8.2 | UC-20/21 | H |
| FR-3.7 | UC-01 | C | FR-8.3 | UC-20 | H |
| FR-3.8 | UC-01 | C | FR-8.4 | UC-20 | H |
| FR-3.9 | UC-01 | C | FR-9.1 | UC-17 | I |
| FR-3.10 | UC-01 | C | FR-9.2 | UC-17 | I |
| FR-3.11 | UC-01 | C | FR-9.3 | UC-18 | I |
| FR-3.12 | UC-01 | C | FR-9.4 | UC-17 | I |
| FR-3.14 | UC-01 | C | FR-9.5 | UC-19 | I |
| FR-3.15 | UC-01 | C | FR-9.6 | UC-17 | I |
| FR-3.16 | UC-01 | C | FR-10.1 | UC-25 | J |
| FR-3.18 | — | C | FR-10.2 | UC-30 | J |
| FR-3.20 | UC-28 | C | FR-10.3 | UC-29 | J |
| FR-3.20b | UC-28 | C | FR-10.4 | UC-29 | J |
| — | — | — | FR-10.5 | UC-25/26 | J |
| — | — | — | FR-10.6 | UC-31 | J |
| — | — | — | FR-10.7 | UC-32 | J |

## 8. Ma trận truy vết FR → API → DB → TEST (tóm tắt — đầy đủ tại TEST_PLAN §11)

| Nhóm FR | Endpoint chính | Bảng DB chính | Nhóm test |
|---|---|---|---|
| FR-1.x (Auth) | /auth/* | Users, RefreshTokens, PasswordResetTokens | TEST-B-001..022, 094..096 |
| FR-2.x (Học tập) | /topics, /lessons, /learning-path/* | Topics, Lessons, UserProgress, LearningPaths, NodeSessions | TEST-B-023..035, 133..136 |
| FR-3.x (Engine) | /simulations | (catalog seed) | TEST-E-001..035 |
| FR-4.x (Ladder) | /exercises/* | Exercises, Questions, ExerciseSubmissions | TEST-B-039..061, 105..115, 137..144 |
| FR-5.x (Tiến độ) | /progress/*, /achievements | UserProgress, Achievements | TEST-B-062..069, 116..119 |
| FR-7.x (Phụ trợ) | /public/*, /feedback | ContentFeedback | TEST-UI-004, TEST-B-076..077 |
| FR-8.x (Lớp) | /classes/* | Classes, ClassMembers, ClassAssignments | TEST-B-079..090 |
| FR-9.x (Code Runner) | /code-runs, /code-submit | CodeRuns, CodeSubmissions | TEST-B-124..132, 145..147, TEST-SEC-009..011 |
| FR-10.x (Gamification) | /me/hearts, /learning-path/*/enter, /me/quests, /shop/*, /premium/* | Users, NodeSessions, DailyQuests, UserQuests, ShopItems, UserInventory, GemTransactions, PremiumSubscriptions | TEST-B-148..183 |

## 9. Lịch sử thay đổi tài liệu

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 12/08/2026 | Mai Tiểu Bảo | Tạo mới: mục lục 12 file + ma trận ánh xạ 17.8 + ma trận FR↔UC↔Module + trạng thái sản xuất |
| 1.1 | 12/08/2026 | Mai Tiểu Bảo | Vá review: cập nhật bảng độ dài tài liệu theo số dòng thực tế sau khi vá lỗi review (SRS 1302, SDD 2079, API 735, TEST 780, DEPLOY 404, SCREEN_MAP 326, THIRD_PARTY 83, README root 194) |
| 1.2 | 12/08/2026 | Trần Viết Tâm Phúc | F2b: thống nhất cách đếm `Measure-Object -Line` (dòng không trống — khớp audit §17.9 F2a) và cập nhật số dòng thật toàn bộ bảng §1 + §1.1 (SRS 1543, SDD 2992, API 596, USER_GUIDE 355, TEST_PLAN 599, DEPLOY 306, GLOSSARY 94, SCREEN_MAP 268, README root 154, THIRD_PARTY 88); ghi trạng thái ⚠ cho file dưới ngưỡng dòng kèm lý do nội dung đủ |
| 1.3 | 12/08/2026 | Trần Viết Tâm Phúc | Đợt G (ux-finalize): cập nhật version + số dòng thật của các docs đã sửa — SRS 1.3 (1544), SDD 1.4 (3015), TEST_PLAN 1.3 (602), THIRD_PARTY 1.2 (108); ghi chú stack UI/UX mới (Tailwind 4 + shadcn-vue + vue-echarts + font Geist) — chi tiết tại SDD §3.8/§3.9, THIRD_PARTY §1, REUSE_REPORT §6 |

