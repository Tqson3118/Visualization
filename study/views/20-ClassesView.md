# 🏫 VIEW 20: DANH SÁCH LỚP HỌC (CLASSESVIEW)

* **Tên file Vue**: [`ClassesView.vue`](file:///d:/FPT/metqua/frontend/src/views/ClassesView.vue)
* **Đường dẫn URL**: `/classes`
* **Route Name**: `classes`
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

---

## 1. CẤU TRÚC GIAO DIỆN

```
┌────────────────────────────────────────────────────────────────────────┐
│  🏫 QUẢN LÝ LỚP HỌC TRỰC TUYẾN                                         │
│  [ + Tham gia lớp bằng Mã Mời ]        [ + Tạo lớp học mới (GV/Admin) ]│
├────────────────────────────────────────────────────────────────────────┤
│ DANH SÁCH CÁC LỚP HỌC:                                                 │
│                                                                        │
│ ┌──────────────────────────────┐  ┌──────────────────────────────────┐ │
│ │ 🎓 CẤU TRÚC DỮ LIỆU K18      │  │ 🎓 THUẬT TOÁN NÂNG CAO - NHÓM 2  │ │
│ │ Giảng viên: ThS. Nguyễn A    │  │ Giảng viên: TS. Trần B           │ │
│ │ Mã mời: [ DSA-9921 ]         │  │ Mã mời: [ ADV-4412 ]             │ │
│ │ Sĩ số: 45 học viên           │  │ Sĩ số: 38 học viên               │ │
│ │ [ Vào lớp học → ]            │  │ [ Vào lớp học → ]                │ │
│ └──────────────────────────────┘  └──────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Sinh viên tham gia lớp bằng Mã mời (Join Class by Code)
1. Sinh viên bấm nút **"Tham gia lớp bằng Mã Mời"** $\rightarrow$ Modal mở ra.
2. Nhập mã mời 6-8 ký tự (ví dụ: `DSA-9921`).
3. Gửi request `POST /api/v1/classes/join { code }`.
4. Backend [`ClassService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/ClassService.cs) kiểm tra mã mời hợp lệ $\rightarrow$ Thêm bản ghi vào bảng `ClassMembers`.
5. Trả về thông tin lớp học $\rightarrow$ Thêm lớp mới vào danh sách trên UI.

### 🔹 Flow 2: Giảng viên tạo lớp học mới (Create Class Flow)
1. Giảng viên bấm nút **"Tạo lớp học mới"**.
2. Nhập Tên lớp học, Mô tả, Khóa học.
3. Gửi `POST /api/v1/classes { name, description }`.
4. Backend tự động sinh mã `InviteCode` duy nhất và gán `OwnerId = currentUserId`.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`ClassesView.vue`](file:///d:/FPT/metqua/frontend/src/views/ClassesView.vue)
* **Frontend Store**: [`classStore.ts`](file:///d:/FPT/metqua/frontend/src/stores/classStore.ts)
* **Backend Controller**: [`ClassesController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/ClassesController.cs)
* **Backend Service**: [`ClassService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/ClassService.cs)
* **Database Entities**: [`Class.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Persistence/Entities/Class.cs), [`ClassMember.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Persistence/Entities/ClassMember.cs)
