# ĐÁP ÁN — Đề 13: Admin Panel

---

## PHẦN I — TRẮC NGHIỆM

| Câu | Đáp án | Giải thích |
|-----|--------|-----------|
| 1 | **B** | AdminUsersView có 2 tab: `all` (tất cả user) và `pending` (TEACHER_PENDING chờ duyệt). Không có tab banned riêng — status filter xử lý qua dropdown. |
| 2 | **C** | `canManageUser(user): return false nếu isSelf(user) \|\| isTargetAdmin(user)`. Bảo vệ tránh Admin tự xóa mình hoặc xóa Admin khác. |
| 3 | **B** | `PUT /admin/users/{id}/role` với body `{ role: 'TEACHER' }`. Handler tại `AdminController` (hoặc `UsersController` tuỳ codebase). |
| 4 | **A** | AdminStatsView hiển thị: 4 KPI card (totalUsers, totalLessons+exercises+simulations, totalRevenue, totalOrders) + Chart 1 (bar chart doanh thu/đơn 7 ngày) + Chart 2 (donut phân bố vai trò). Dùng ECharts qua `VChartLazy`. |
| 5 | **C** | AdminSettingsView có 2 tab: `system` và `reports`. Tab `system` reactive form: `siteName`, `allowedDomains`, `passwordPolicy` (minLength/requireUppercase/requireDigit/requireSpecial), `uploadMaxMb`, `sandboxSeconds`, `sandboxMemoryMb`. |

---

## PHẦN II — TỰ LUẬN

### TL-1: Luồng TEACHER_PENDING → TEACHER *(2.5 điểm)*

**Thang điểm:** 0.5đ mỗi bước.

**Bước 1 — Student đăng ký:**
- ProfileView → Tab **Settings** → Button "Đăng ký làm giáo viên"
- Gọi: `POST /me/request-teacher`
- Body: (không cần body hoặc body rỗng — chỉ cần authenticated)

**Bước 2 — Role thay đổi:**
- Role chuyển thành `TEACHER_PENDING`
- Student **KHÔNG** vào được `/studio` — router guard kiểm tra `role`, chỉ cho phép `['TEACHER', 'ADMIN']`
- TEACHER_PENDING bị redirect về `/` hoặc `/profile`

**Bước 3 — Admin vào duyệt:**
- Admin vào route `/admin/users`
- Chọn tab **`pending`** → thấy danh sách TEACHER_PENDING

**Bước 4 — Admin duyệt:**
- Bấm "Duyệt" trên user cần duyệt
- Gọi: `PUT /admin/users/{id}/role`
- Body: `{ "role": "TEACHER" }`
- Handler: `AdminController` (hoặc `UsersController`)

**Bước 5 — Teacher reload:**
- Teacher cần **F5 (reload page)** hoặc hệ thống tự refresh session
- `main.ts` → `authStore.refresh()` → `GET /auth/refresh` → server trả về user info với `role: 'TEACHER'`
- authStore cập nhật role → router guard pass → `/studio` accessible

```
[Student ProfileView] → POST /me/request-teacher
       ↓ role = TEACHER_PENDING
[/studio] ← blocked by router guard
       ↓
[Admin /admin/users?tab=pending] → PUT /admin/users/{id}/role {role:'TEACHER'}
       ↓
[Teacher F5] → GET /auth/refresh → role='TEACHER'
       ↓
[/studio] ← accessible ✅
```

---

### TL-2: Tìm kiếm & quản lý user *(2.5 điểm)*

**Thang điểm:** 0.5đ mỗi ý.

1. **Cơ chế tìm kiếm:**
   - **Client-side filter** — không call API mỗi lần gõ
   - Dùng `ref`: `search` (text input), `roleFilter` (dropdown role), `statusFilter` (dropdown status)
   - `computed filteredUsers` filter qua danh sách đã load từ server

2. **Các filter có sẵn:**
   - `search` — tìm theo tên/email (text)
   - `roleFilter` — lọc theo role: ALL / STUDENT / TEACHER / TEACHER_PENDING / ADMIN
   - `statusFilter` — lọc theo status: ALL / active / banned

3. **Khóa account (ban):**
   - API: `PUT /admin/users/{id}/status`
   - Body: `{ "isBanned": true }`
   - Unban: `{ "isBanned": false }`

4. **Xóa user vĩnh viễn:**
   - API: `DELETE /admin/users/{id}`
   - Cần `canManageUser(user)` check trước để:
     - Disable nút xóa nếu target là mình hoặc Admin khác
     - Tránh lỗi server-side nếu UI bypass check

5. **Tại sao không tự xóa/ban mình và không xóa Admin khác:**
   ```typescript
   canManageUser(user): boolean {
     if (isSelf(user)) return false;      // không tác động lên chính mình
     if (isTargetAdmin(user)) return false; // không tác động lên Admin khác
     return true;
   }
   ```
   - **isSelf**: `user.id === auth.id` — Admin không thể tự lock out khỏi hệ thống
   - **isTargetAdmin**: `user.role === 'ADMIN'` — ngăn leo thang quyền hạn / xung đột Admin
   - Đây là **defense in depth**: cả UI (disabled button) lẫn backend đều nên kiểm tra
