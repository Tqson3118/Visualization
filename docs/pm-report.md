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
