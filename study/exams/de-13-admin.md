# Đề 13 — Admin Panel (Users / Stats / Settings)

**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm  
**Bao phủ:** AdminUsersView, AdminStatsView, AdminSettingsView, luồng TEACHER_PENDING → TEACHER

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1.** AdminUsersView.vue (route `/admin/users`) có bao nhiêu tab và chức năng của tab "pending" là gì?

- A. 1 tab (all) — không có tab pending
- B. 2 tab: `all` / `pending` — tab pending hiển thị danh sách user cần xét duyệt vai trò TEACHER
- C. 3 tab: `all` / `pending` / `banned`
- D. 2 tab: `active` / `pending`

---

**Câu 2.** Hàm `canManageUser(user)` trong AdminUsersView trả về `false` trong trường hợp nào?

- A. Khi user đang bị banned
- B. Khi target user là TEACHER
- C. Khi target là chính mình (isSelf) HOẶC target là Admin khác (isTargetAdmin)
- D. Khi target user chưa xác thực email

---

**Câu 3.** Để duyệt một TEACHER_PENDING lên TEACHER, Admin gọi API nào?

- A. POST /admin/users/{id}/approve
- B. PUT /admin/users/{id}/role với body `{ role: 'TEACHER' }` → AdminController hoặc UsersController
- C. PATCH /admin/approve-teacher/{id}
- D. PUT /me/role với body `{ role: 'TEACHER' }`

---

**Câu 4.** AdminStatsView hiển thị những loại dữ liệu chính nào? (Chọn đáp án đầy đủ nhất)

- A. Tổng user, tổng bài học, tổng doanh thu, tổng giao dịch + 2 biểu đồ ECharts
- B. Tổng user, tổng doanh thu, số lớp học, bảng xếp hạng
- C. Số user online, tổng bài nộp, active sessions, server uptime
- D. Tổng user, doanh thu, số lớp, số bài học + biểu đồ Line

---

**Câu 5.** AdminSettingsView có mấy tab và tab "system" cho phép cấu hình những gì?

- A. 1 tab — chỉ cấu hình site name
- B. 3 tab: system / reports / logs
- C. 2 tab: `system` / `reports` — tab system cấu hình: siteName, allowedDomains, passwordPolicy, uploadMaxMb, sandboxSeconds, sandboxMemoryMb
- D. 2 tab: `general` / `security`

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

### Câu TL-1 (2.5 điểm)

**Đề bài:** Trace **toàn bộ luồng TEACHER_PENDING → TEACHER** từ khi Student đăng ký làm giáo viên đến khi họ có thể truy cập `/studio`:

1. Student thực hiện hành động gì ở ProfileView? API nào được gọi?
2. Sau đó, role của Student thay đổi thành gì? Họ có vào được `/studio` không? Tại sao?
3. Admin vào màn hình nào để duyệt? Tab nào?
4. Admin thực hiện thao tác gì? API nào được gọi?
5. Sau khi Admin duyệt, Teacher cần làm gì để role có hiệu lực? (Gợi ý: liên quan đến authStore)

*(Yêu cầu: nêu đủ 5 bước, có tên API endpoint cụ thể)*

---

### Câu TL-2 (2.5 điểm)

**Đề bài:** Giải thích **cơ chế tìm kiếm và quản lý user** trong AdminUsersView:

1. Tìm kiếm user hoạt động theo cơ chế nào? (client-side hay server-side? dùng ref nào?)
2. Những filter nào có sẵn ngoài ô tìm kiếm text?
3. Khóa account (ban) thực hiện qua API nào? Body request là gì?
4. Xóa user vĩnh viễn gọi API nào? Tại sao cần `canManageUser()` check trước?
5. Giải thích tại sao Admin không thể tự xóa hoặc ban chính mình, và không thể xóa Admin khác.

*(Gợi ý: `canManageUser`, `isSelf`, `isTargetAdmin`)*
