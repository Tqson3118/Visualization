# 🏫 PHÂN HỆ 6: LỚP HỌC TRỰC TUYẾN (CLASSROOM FLOW)

Phân hệ Lớp học kết nối Giảng viên và Sinh viên, cho phép tổ chức lớp, giao bài tập theo lộ trình và theo dõi báo cáo tiến độ chi tiết của từng sinh viên.

---

## 1. MÀN HÌNH 19: DANH SÁCH LỚP HỌC (CLASSES VIEW)

* **URL**: `/classes`
* **File Vue**: [`ClassesView.vue`](file:///d:/FPT/metqua/frontend/src/views/ClassesView.vue)
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

```
┌────────────────────────────────────────────────────────────────────────┐
│ 🏫 QUẢN LÝ LỚP HỌC                                                     │
│ [ + Tạo lớp học mới (Giảng viên) ]   [ + Tham gia bằng mã Code (Sinh viên) ]│
├────────────────────────────────────────────────────────────────────────┤
│ ┌──────────────────────┐  ┌──────────────────────┐                     │
│ │ 🎓 DSA_K18_FA26      │  │ 🎓 THUẬT TOÁN NÂNG CAO│                     │
│ │ GV: ThS. Nguyễn Văn A│  │ GV: TS. Trần Thị B   │                     │
│ │ Mã lớp: DSA-9821     │  │ Mã lớp: ADV-4412     │                     │
│ │ Sĩ số: 45 sinh viên  │  │ Sĩ số: 38 sinh viên  │                     │
│ │ Bài tập đang mở: 3   │  │ Bài tập đang mở: 1   │                     │
│ │ [ Vào lớp học → ]    │  │ [ Vào lớp học → ]    │                     │
│ └──────────────────────┘  └──────────────────────┘                     │
└────────────────────────────────────────────────────────────────────────┘
```

### 2 Luồng tương tác chính:
1. **Dành cho Sinh viên (Tham gia lớp)**:
   * Bấm nút *"Tham gia bằng mã Code"* $\rightarrow$ Nhập mã 6-8 ký tự (ví dụ: `DSA-9821`).
   * Frontend gửi `POST /api/v1/classes/join { code: "DSA-9821" }`.
   * Backend kiểm tra mã hợp lệ $\rightarrow$ Thêm sinh viên vào bảng `ClassMembers` với trạng thái `Enrolled`.
2. **Dành cho Giảng viên (Tạo lớp mới)**:
   * Bấm nút *"Tạo lớp học mới"* $\rightarrow$ Nhập Tên lớp, Mô tả, Niên khóa.
   * Backend tự động sinh `InviteCode` duy nhất và gán `OwnerId = currentUserId`.

---

## 2. MÀN HÌNH 20: CHI TIẾT LỚP HỌC (CLASS DETAIL VIEW)

* **URL**: `/classes/:id`
* **File Vue**: [`ClassDetailView.vue`](file:///d:/FPT/metqua/frontend/src/views/ClassDetailView.vue)
* **Quyền truy cập**: Thành viên của lớp hoặc Giảng viên sở hữu.

### Mắt thấy gì trên giao diện?
1. **Tab 1: Bài tập (Assignments)**:
   * Danh sách bài tập do giảng viên giao kèm hạn nộp (Deadline).
   * Sinh viên bấm *"Làm bài"* $\rightarrow$ Điều hướng tới bài tập tương ứng.
   * Giảng viên có nút *"Giao bài tập mới"* (chọn bài từ ngân hàng đề hoặc lộ trình).
2. **Tab 2: Danh sách sinh viên (Members)**:
   * Hiển thị bảng danh sách thành viên trong lớp, ngày tham gia, tiến độ hoàn thành bài tập.
3. **Tab 3: Thông báo lớp (Announcements)**:
   * Kênh trao đổi, nhắc nhở hạn nộp bài.

---

## 3. MÀN HÌNH 21: BÁO CÁO TIẾN ĐỘ LỚP HỌC (CLASS REPORT VIEW)

* **URL**: `/classes/:id/report`
* **File Vue**: [`ClassReportView.vue`](file:///d:/FPT/metqua/frontend/src/views/ClassReportView.vue)
* **Quyền truy cập**: Giảng viên phụ trách lớp (`roles: ['TEACHER', 'ADMIN']`).

### Mắt thấy gì trên giao diện?
* **Biểu đồ phân phối điểm số (Score Distribution Chart)**: Tỷ lệ sinh viên đạt Giỏi, Khá, Trung bình, Yếu.
* **Tỷ lệ nộp bài đúng hạn (On-time Submission Rate)**.
* **Bảng ma trận điểm chi tiết (Gradebook Matrix)**:
  * Từng hàng là 1 Sinh viên (MSSV, Họ tên).
  * Từng cột là 1 Bài tập được giao (Điểm số, Thời gian nộp, Trạng thái Đã nộp/Chưa nộp/Trễ hạn).
* **Nút xuất báo cáo (Export Excel / CSV)**: Xuất toàn bộ bảng điểm để giảng viên tổng kết học phần.
