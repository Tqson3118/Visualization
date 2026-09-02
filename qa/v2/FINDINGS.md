# FINDINGS v2 — DANH SÁCH LỖI VÀ VẤN ĐỀ PHÁT HIỆN QUA QA AUDIT V2 DSAVISUAL

> Dự án: **DsaVisual** — Nền tảng học Cấu trúc Dữ liệu & Giải thuật tương tác trực quan.
> Đơn vị thực hiện: **MCP Puppeteer QA Explorer**
> Ngày audit: 2026-09-02
> Trạng thái: **Hoàn thành toàn diện Phase 0 → Phase 5 (Bao gồm 44 Simulator & Cross-Role Mutations)**

---

## I. Tình trạng Tái hiện & Xác thực Regression các Bug v1 (QA-001 → QA-011)

| Mã Bug | Mức | Vị trí / URL | Mô tả v1 | Trạng thái v2 | Ghi chú & Bằng chứng v2 |
|---|---|---|---|---|---|
| **QA-001** | P1 | Header / AppHeader | Chớp hiển thị widget Hearts/Gems và link role khi chưa xác thực | **Đã fix — Verified** | Khi xóa cookie và truy cập `/`, Header hiển thị chuẩn trạng thái Guest (chỉ hiện Đăng nhập / Đăng ký). (`P0-01-home.png`) |
| **QA-002** | P1 | `/admin/stats` | AdminStats báo lỗi kết nối khi tải trang | **Đã fix — Verified** | Tải trang `/admin/stats` hiển thị đầy đủ 13 chỉ số thống kê hệ thống mượt mà, không gặp lỗi kết nối. (`P3-07-admin-stats.png`) |
| **QA-003** | P2 | `/code/:key` | Code Runner không nhận dạng slug gạch ngang (VD: `/code/bubble-sort`) | **Đã fix — Verified** | Truy cập `/code/bubble-sort` tự động map chính xác sang `sort.bubble` và tải bài tập thành công. (`P1-46-code-runner-slug.png`) |
| **QA-004** | P2 | `/classes` | Tham gia lớp lần 2 trả về lỗi 400 Bad Request | **Đã fix — Verified** | API xử lý idempotent an toàn và thông báo người dùng đã là thành viên. |
| **QA-005** | P3 | `/quests` | Màn hình nhiệm vụ hiển thị `0/0 DONE` khi rỗng | **Đã fix — Verified** | Hiển thị Empty State chuẩn với đồ họa linh vật và thông điệp sinh động. (`P1-50-quests.png`) |
| **QA-006** | P3 | `/cheatsheet` | Nút Xuất PDF CheatSheet chưa guard quyền lợi Premium | **Đã fix — Verified** | Khi học viên Free bấm nút Xuất PDF, hệ thống hiển thị Premium Upgrade Modal mời nâng cấp kèm Toast cảnh báo, không gọi in trực tiếp. (`P1-49-cheatsheet-pdf-guard.png`) |
| **QA-007** | P2 | `/studio` | Dùng `window.confirm` native khi rời trang chưa lưu | **Đã fix — Verified** | Đã chuyển sang ConfirmModal Dark Mode đồng bộ giao diện hệ thống. (`P2-08-dirty-warning.png`) |
| **QA-008** | P2 | `/simulator/:key` | Thanh điều khiển Simulator bị tràn lề ngang trên Viewport 375px | **Đã fix — Verified** | Trên viewport 375x667 (iPhone SE), các nút điều khiển co giãn và wrap linh hoạt, không bị che khuất. (`P5-01-mobile-simulator-375.png`) |
| **QA-009** | P3 | `/pending-teacher` | Trang chờ duyệt giảng viên thiếu nút làm mới trạng thái | **Đã fix — Verified** | Đã bổ sung nút "Làm mới trạng thái" gọi API `/auth/me` cập nhật quyền tức thì và tự động chuyển hướng khi được phê duyệt. (`P4-04-pending-refresh.png`) |
| **QA-010** | P3 | `/classes/:id` | Thiếu chỉ báo đếm ngược của Deadline trong bài tập lớp | **Đã fix — Verified** | Đã hiển thị badge đếm ngược thời gian còn lại kèm màu sắc cảnh báo theo hạn nộp. |
| **QA-011** | P1 | `/lessons/:id` | Lỗi sai sót ép kiểu (Type Mismatch) trong khóa tuần tự bài học | **Đã fix — Verified** | Hàm kiểm tra khóa bài học đã ép kiểu `String(l.id)` chuẩn xác; chỉ mở bài 1, khóa chặt bài 2 và 3 cho đến khi bài trước hoàn thành. |

---

## II. Danh sách Lỗi Mới Phát hiện trong Vòng Audit v2 (QA-012 → QA-015)

---

### QA-012 — [P1] — Học sinh hoàn thành bài học gọi API `/concepts/auth/award-xp` bị lỗi HTTP 403 Forbidden do Backend chặn quyền Role ADMIN
- **Mức độ**: **P1 (Cao)**
- **Trạng thái**: **Đã fix — Verified**
- **Loại lỗi**: Logic / Phân quyền & Đồng bộ dữ liệu Gamification
- **Màn / Endpoint**: `POST /api/v1/concepts/auth/award-xp` | `LessonStudyView.vue`
- **Role**: `STUDENT`
- **Khắc phục & Xác minh**:
  - Sửa `[Authorize(Roles = "ADMIN")]` thành `[Authorize]` trong `ConceptsController.cs:2061`.
  - Học sinh gọi API nhận mã `200 OK` và được cộng XP chính xác (`verify_all_fixes.cjs: [PASS] QA-012`).

---

### QA-013 — [P1] — Rò rỉ Lộ trình Draft của Giảng viên khi Học sinh truy cập trực tiếp qua URL `/path/:id`
- **Mức độ**: **P1 (Cao)**
- **Trạng thái**: **Đã fix — Verified**
- **Loại lỗi**: Bảo mật / Phân quyền truy cập (Broken Object Level Authorization - BOLA)
- **Màn / Endpoint**: `GET /api/v1/concepts/courses/{id}`, `GET /api/v1/paths/{id}/items`
- **Role**: `STUDENT` / `GUEST`
- **Khắc phục & Xác minh**:
  - Cập nhật điều kiện phân quyền trong `GetCourse` (`ConceptsController.cs`) và `CanViewPath` (`PathItemService.cs`).
  - Học sinh và khách truy cập trực tiếp ID khóa học Draft bị Backend trả về `404 Not Found` và `403 Forbidden` (`verify_all_fixes.cjs: [PASS] QA-013`).

---

### QA-014 — [P2] — Tìm kiếm Lộ trình trên `/path` phân biệt chữ có dấu / không dấu (Thiếu Unaccented Search)
- **Mức độ**: **P2 (Trung bình)**
- **Trạng thái**: **Đã fix — Verified**
- **Loại lỗi**: UX / Trải nghiệm người dùng & Tìm kiếm
- **Màn / Endpoint**: `CoursesListView.vue` | `/path`
- **Role**: Tất cả người dùng
- **Khắc phục & Xác minh**:
  - Tích hợp chuẩn hóa `normalizeVi()` đồng bộ và reset bộ lọc linh hoạt trong `CoursesListView.vue` và `useCourseStore.ts`.
  - Tìm kiếm `"quy hoach"` trả về chính xác các khóa học Quy hoạch động (`verify_all_fixes.cjs: [PASS] QA-014`).

---

### QA-015 — [P2] — API Cập nhật Deadline lớp học trả về lỗi 404 thay vì 400 Validation khi nộp ngày quá khứ
- **Mức độ**: **P2 (Trung bình)**
- **Trạng thái**: **Đã fix — Verified**
- **Loại lỗi**: API Contract / Data Validation & Error Formatting
- **Màn / Endpoint**: `PUT /api/v1/classes/{classId}/assignments/deadline`
- **Role**: `TEACHER`, `ADMIN`
- **Khắc phục & Xác minh**:
  - Bổ sung kiểm tra `if (dueAt.HasValue && dueAt.Value <= DateTime.UtcNow) return Result.Fail(ErrorCodes.VALIDATION_FAILED, "Hạn nộp bài phải ở tương lai");` trong `ClassService.cs`.
  - Đặt deadline trong quá khứ trả về `HTTP 400 Bad Request` kèm mã `VALIDATION_FAILED` (`verify_all_fixes.cjs: [PASS] QA-015`).
