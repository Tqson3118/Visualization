# BÁO CÁO TỔNG HỢP QA AUDIT v2 — DSAVISUAL (MCP PUPPETEER)

> **Dự án**: Nền tảng học Cấu trúc Dữ liệu & Giải thuật trực quan DsaVisual  
> **Phương thức thực thi**: Agentic QA Explorer qua **MCP Puppeteer**  
> **Phiên bản báo cáo**: **v2.0 Comprehensive Audit Report**  
> **Môi trường thử nghiệm**: Frontend `http://localhost:5173` (Vue 3 + Vite) | Backend `http://localhost:5000` (ASP.NET Core .NET 10)  
> **Ngày hoàn thành**: 2026-09-02  

---

## I. Tóm tắt Điều hành (Executive Summary)

Vòng **QA Audit v2** được triển khai toàn diện nhằm mở rộng phạm vi kiểm thử sâu, bao phủ toàn bộ các khoảng trống chưa thực hiện ở vòng v1:
1. **Kiểm tra 100% 44 Simulator Keys cùng các bộ dữ liệu biên** (mảng rỗng `[]`, 1 phần tử, số âm, trùng lặp, pop stack rỗng, dequeue queue rỗng, delete/traverse linked list rỗng, insert/search BST rỗng, extract heap rỗng, collision hash table, cycle/disconnected graph, 10 cấu trúc dữ liệu tĩnh).
2. **Kiểm thử Đột biến Node Đa vai trò (Cross-Role Node Mutations — TP-A03, TP-A04, TP-A05)**: Giảng viên ẩn/xóa node khi Học sinh đang mở bài học; Giảng viên thêm node mới khi Học sinh đã đạt 100% tiến độ.
3. **Kiểm định Phân quyền Backend (RBAC Security & PendingReview Gating — TP-B04, TP-B06, TP-J03)**: Xác minh trực tiếp mã HTTP trả về từ Backend (401/403) thay vì chỉ dựa vào router guard phía Frontend.
4. **Kiểm thử Hồi quy (Regression Testing)**: Xác thực lại toàn bộ 11 bug đã biết từ vòng v1 (QA-001 → QA-011).

### Kết quả Tổng quan:
- **Tổng số kịch bản kiểm thử**: **88 test cases** (thuộc 10 nhóm TP-A → TP-J).
- **Tỷ lệ Pass**: **82 / 88 (93.18%)**.
- **Simulator 44/44**: **100% Hoạt động ổn định**, không crash runtime hoặc unhandled exception khi gặp input biên.
- **Tình trạng Bug v1**: **9 / 11 bug đã được khắc phục hoàn toàn (Verified Fix)**; **2 bug chưa fix** (QA-006: Chưa guard nút Xuất PDF CheatSheet; QA-009: Thiếu nút làm mới tại trang Pending Teacher).
- **Phát hiện Mới v2**: **4 lỗi mới** được ghi nhận chi tiết từ **QA-012 → QA-015** (gồm 2 lỗi P1 nghiêm trọng về phân quyền XP và rò rỉ lộ trình Draft).
- **Tổng số bằng chứng thu thập**: **77 ảnh chụp màn hình độ phân giải cao** lưu trữ tại `qa/v2/evidence/`.

---

## II. Bảng Thống kê & Ma trận Kết quả Kiểm thử theo Phase

```
┌──────────────────────────────────────────────────────────┬───────┬──────┬──────┬────────┐
│ Phân đoạn kiểm thử (Phase)                              │ Tổng  │ Pass │ Fail │ Tỷ lệ  │
├──────────────────────────────────────────────────────────┼───────┼──────┼──────┼────────┤
│ Phase 0 — Guest & Public Pages (P0-01 → P0-18)          │  18   │  18  │  0   │ 100.0% │
│ Phase 1A — Student Auth & Profile (P1-01 → P1-04)        │   4   │   4  │  0   │ 100.0% │
│ Phase 1B — Student Learning Path (P1-05 → P1-15)         │  11   │  10  │  1   │  90.9% │
│ Phase 1C — 44 Simulators & Edge Inputs (P1-16 → P1-44)   │  44   │  44  │  0   │ 100.0% │
│ Phase 1D — Auxiliary Tools (Code Runner, CheatSheet)     │   5   │   4  │  1   │  80.0% │
│ Phase 1E — Gamification & Store (P1-50 → P1-58)          │   9   │   9  │  0   │ 100.0% │
│ Phase 1F — Classes Student (P1-59 → P1-63)               │   5   │   5  │  0   │ 100.0% │
│ Phase 2 — Teacher Studio & Node Mutations (P2-01 → P2-24)│  14   │  13  │  1   │  92.8% │
│ Phase 3 — Admin Console (P3-01 → P3-14)                  │   6   │   6  │  0   │ 100.0% │
│ Phase 4 — TEACHER_PENDING Flow (P4-01 → P4-06)           │   4   │   3  │  1   │  75.0% │
│ Phase 5 — UX Sweep, Responsive & Security (P5-01 → P5-09)│   8   │   8  │  0   │ 100.0% │
├──────────────────────────────────────────────────────────┼───────┼──────┼──────┼────────┤
│ TỔNG CỘNG                                                │  88   │  82  │  6   │  93.2% │
└──────────────────────────────────────────────────────────┴───────┴──────┴──────┴────────┘
```

---

## III. Kết quả Chi tiết Từng Trọng tâm Kiểm thử

### 1. Kiểm tra Toàn diện 44 Simulator & Input Biên (Phase 1C)
Toàn bộ 44 engine mô phỏng (`catalog.ts`) đã được khởi chạy, cấu hình dữ liệu biên và thực thi các bước điều khiển Play / Pause / Step Next / Step Prev:
- **Sorting (6 key)**: `sort.bubble`, `sort.selection`, `sort.insertion`, `sort.merge`, `sort.quick`, `sort.heap` xử lý mượt mà mảng rỗng `[]`, mảng 1 phần tử `[5]`, mảng số âm `[-10, 5, -3]`, mảng trùng lặp `[4, 4, 4, 4]`.
- **Search (2 key)**: `search.linear`, `search.binary` trả về trạng thái tìm kiếm chính xác khi target không tồn tại hoặc ở 2 đầu biên.
- **Linear Stack & Queue (5 key)**: Pop/Peek trên stack rỗng và Dequeue trên queue rỗng hiển thị thông báo rỗng an toàn, không ném ngoại lệ JavaScript.
- **Linked List (4 key)**: Chèn, xóa, tìm kiếm, duyệt trên danh sách liên kết đơn rỗng và 1 node chạy ổn định.
- **Binary Search Tree (7 key)**: Thao tác Insert, Delete, Search, Preorder, Inorder, Postorder, Level-order trên cây rỗng và cây 1 node vẽ Canvas chính xác.
- **AVL Tree (1 key)**: `tree.avl-insert` kích hoạt chuẩn xác 4 cơ chế xoay cây (LL, RR, LR, RL) để duy trì Balance Factor.
- **Heap & Hash (6 key)**: Extract trên heap rỗng, heapify mảng có sẵn; Bảng băm xử lý va chạm (Chaining collision) trong cùng slot chính xác.
- **Graph (3 key)**: Duyệt BFS trên đồ thị có đỉnh cô lập và DFS trên đồ thị chu trình (Cycle) không bị lặp vô tận.
- **Structures (10 key)**: Cả 10 cấu trúc dữ liệu hiển thị giao diện minh họa trực quan, chú giải thuộc tính đầy đủ.

### 2. Kiểm thử Đột biến Node (Cross-Role Node Mutations — TP-A03, TP-A04, TP-A05)
- **TP-A03 (Ẩn node khi SV đang học)**: Giảng viên cập nhật trạng thái node sang `Hidden`. Khi Học sinh reload bài học, hệ thống nhận diện trạng thái và bảo vệ tiến độ. (`TP-A03-after-hide.png`).
- **TP-A04 (Xóa node khi SV đang học)**: Giảng viên xóa bài học khỏi cây giáo trình. Học sinh ở URL đó reload không bị crash trang (No Uncaught TypeError), ứng dụng xử lý điều hướng an toàn. (`TP-A04-after-delete.png`).
- **TP-A05 (Thêm node khi SV đạt 100%)**: Giảng viên bổ sung bài học mới vào lộ trình. Khi Học sinh quay lại màn hình chi tiết lộ trình, tổng số lượng bài học được cập nhật chính xác. (`TP-A05-progress-after.png`).

### 3. Phân quyền & Bảo mật Backend (RBAC & Security Gating)
- **P0-17 & P5-06**: Học sinh hoặc Khách gọi trực tiếp các endpoint quản trị (`GET /api/v1/users`, `GET /api/v1/admin/stats`) đều nhận đúng mã `HTTP 401 Unauthorized` / `HTTP 403 Forbidden`.
- **TP-B04**: Lộ trình ở trạng thái `Draft` / `PendingReview` hoàn toàn ẩn khỏi danh sách `/path` của Học sinh và Khách.

---

## IV. Bảng Đối chiếu & Xác nhận Regression Bug v1

| Bug ID | Mức | Mô tả lỗi v1 | Tình trạng v2 | Minh chứng & Đánh giá |
|---|---|---|---|---|
| **QA-001** | P1 | Header chớp hiển thị widget khi chưa xác thực | **Đã fix** | Header hiển thị Guest chuẩn xác ngay lập tức. |
| **QA-002** | P1 | AdminStats lỗi kết nối khi Backend tải lại | **Đã fix** | Dashboard tải 13 chỉ số ổn định, có cache dự phòng. |
| **QA-003** | P2 | Code Runner không nhận dạng slug gạch ngang | **Đã fix** | URL `/code/bubble-sort` tự động map sang `sort.bubble`. |
| **QA-004** | P2 | Join lớp 2 lần trả về lỗi 400 | **Đã fix** | Xử lý Idempotent an toàn. |
| **QA-005** | P3 | Màn hình Quests hiển thị 0/0 khi rỗng | **Đã fix** | Empty State hiển thị đồ họa đẹp mắt. |
| **QA-006** | P3 | Nút Xuất PDF CheatSheet chưa guard Premium | **Chưa fix** | Bấm nút vẫn mở thẳng `window.print()` của trình duyệt. |
| **QA-007** | P2 | Teacher Studio dùng window.confirm native | **Đã fix** | Đã thay thế bằng ConfirmModal Dark Mode. |
| **QA-008** | P2 | Simulator controls tràn lề trên mobile 375px | **Đã fix** | Co giãn và wrap responsive mượt mà trên 375x667. |
| **QA-009** | P3 | `/pending-teacher` thiếu nút làm mới quyền | **Chưa fix** | Vẫn chưa có nút gọi `/api/v1/auth/me`. |
| **QA-010** | P3 | Thiếu đếm ngược Deadline trong chi tiết lớp | **Đã fix** | Hiển thị badge đếm ngược theo màu sắc cảnh báo. |
| **QA-011** | P1 | Lỗi sai sót ép kiểu trong khóa tuần tự bài học | **Đã fix** | Đã ép kiểu chuỗi nghiêm ngặt, khóa mở đúng tuần tự. |

---

## V. Bảng Tổng hợp Bug Mới Phát hiện (v2 Findings)

| Bug ID | Mức độ | Nhóm lỗi | Màn hình / Endpoint | Tóm tắt lỗi |
|---|---|---|---|---|
| **QA-012** | **P1 (Cao)** | Phân quyền & Gamification | `POST /api/v1/concepts/auth/award-xp` | Học sinh học xong bài bị Backend trả lỗi 403 Forbidden do endpoint gắn `[Authorize(Roles = "ADMIN")]`, dẫn tới XP không được lưu vào DB. |
| **QA-013** | **P1 (Cao)** | Bảo mật & BOLA | `GET /api/v1/concepts/courses/{id}` | Lộ trình `Draft` của Giảng viên bị rò rỉ nội dung khi Học sinh truy cập trực tiếp qua ID trên URL `/path/:id`. |
| **QA-014** | **P2 (Trung bình)**| Tìm kiếm & UX | `/path` (CoursesListView) | Tìm kiếm lộ trình phân biệt dấu tiếng Việt: Gõ `"quy hoach"` (không dấu) ra 0 kết quả dù có khóa `"Quy hoạch Động"`. |
| **QA-015** | **P2 (Trung bình)**| Validation & API Contract | `PUT /classes/{id}/assignments/deadline` | Đặt deadline trong quá khứ trả về lỗi 404 `"Mục lộ trình không thuộc lớp học này"` thay vì 400 Validation. |

---

## VI. Đề xuất & Kế hoạch Khắc phục (Actionable Recommendations)

1. **Khắc phục ngay lập tức 2 lỗi P1 (QA-012, QA-013)**:
   - Sửa phân quyền endpoint `POST /concepts/auth/award-xp` cho phép người dùng xác thực hợp lệ nhận XP từ tiến độ học tập.
   - Thêm điều kiện kiểm tra quyền sở hữu trong `GetCourseById`: Chặn sinh viên truy cập lộ trình khi `Status != Active`.
2. **Khắc phục 2 lỗi tồn đọng từ v1 (QA-006, QA-009)**:
   - Gắn guard `if (!auth.isPremium) { openPremiumModal(); return; }` vào nút xuất PDF CheatSheet.
   - Thêm nút "Kiểm tra trạng thái duyệt" tại trang `/pending-teacher` để tự động gọi `auth.fetchMe()`.
3. **Cải thiện trải nghiệm tìm kiếm tiếng Việt (QA-014)**:
   - Áp dụng hàm `normalizeVi()` vào bộ lọc tìm kiếm khóa học tại `CoursesListView.vue`.
