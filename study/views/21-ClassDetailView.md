# 📖 VIEW 21: CHI TIẾT LỚP HỌC (CLASSDETAILVIEW)

* **Tên file Vue**: [`ClassDetailView.vue`](file:///d:/FPT/metqua/frontend/src/views/ClassDetailView.vue)
* **Đường dẫn URL**: `/classes/:id`
* **Route Name**: `class-detail`
* **Quyền truy cập**: Thành viên của lớp hoặc Giảng viên sở hữu (`requiresAuth: true`).

---

## 1. CẤU TRÚC GIAO DIỆN & 3 TABS QUẢN TRỊ

```
┌────────────────────────────────────────────────────────────────────────┐
│  🎓 LỚP: CẤU TRÚC DỮ LIỆU K18       [ Mã mời: DSA-9921 📋 ] [ Sĩ số: 45]│
│  [ 📊 Xem Báo cáo điểm số (Dành cho GV) ]                              │
├────────────────────────────────────────────────────────────────────────┤
│ [ Tab 1: Bài tập được giao ]  [ Tab 2: Thành viên ]  [ Tab 3: Cài đặt ]│
├────────────────────────────────────────────────────────────────────────┤
│ TAB 1: DANH SÁCH BÀI TẬP (ASSIGNMENTS):                                │
│                                                                        │
│ 1. 📝 Bài tập 1: Cài đặt Binary Search                                 │
│    Hạn nộp: 23:59 15/09/2026 • Đã nộp: 42/45                          │
│    Hành động sinh viên: [ Làm bài ngay → ]                             │
│    Hành động giáo viên: [ ✎ Chỉnh sửa ] [ 📊 Xem bài nộp ]             │
│                                                                        │
│ 2. 🌲 Bài tập 2: Duyệt cây BST                                         │
│    Hạn nộp: 23:59 22/09/2026 • Đã nộp: 10/45                          │
│    [ + Giao bài tập mới (Giảng viên) ]                                 │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Giảng viên giao bài tập mới (Assign Exercise Flow)
1. Giảng viên bấm nút **"+ Giao bài tập mới"**.
2. Modal mở ra cho phép:
   * Chọn bài tập từ Ngân hàng đề có sẵn hoặc tạo bài tập mới.
   * Đặt Hạn nộp (Deadline) và Điểm số tối đa.
3. Gửi `POST /api/v1/classes/{id}/assignments { exerciseId, deadline }`.
4. Backend lưu vào bảng `ClassAssignments` $\rightarrow$ Tự động gửi thông báo đến toàn bộ sinh viên trong lớp.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`ClassDetailView.vue`](file:///d:/FPT/metqua/frontend/src/views/ClassDetailView.vue)
* **Backend Controller**: [`ClassesController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/ClassesController.cs)
* **Database Entity**: [`ClassAssignment.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Persistence/Entities/ClassAssignment.cs)
