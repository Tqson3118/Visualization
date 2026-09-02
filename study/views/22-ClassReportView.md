# 📊 VIEW 22: BÁO CÁO TIẾN ĐỘ LỚP HỌC (CLASSREPORTVIEW)

* **Tên file Vue**: [`ClassReportView.vue`](file:///d:/FPT/metqua/frontend/src/views/ClassReportView.vue)
* **Đường dẫn URL**: `/classes/:id/report`
* **Route Name**: `class-report`
* **Quyền truy cập**: Giảng viên sở hữu lớp hoặc Admin (`roles: ['TEACHER', 'ADMIN']`).

---

## 1. CẤU TRÚC GIAO DIỆN & BẢNG SỐ LIỆU ĐIỂM SỐ

```
┌────────────────────────────────────────────────────────────────────────┐
│  📊 BÁO CÁO LỚP: CẤU TRÚC DỮ LIỆU K18      [ 📥 Xuất CSV ] [ 🖨️ In ]   │
├────────────────────────────────────────────────────────────────────────┤
│ THẺ HERO STATS:                                                        │
│ • Sĩ số: 45 sinh viên • Tỷ lệ hoàn thành: 78% • Đúng hạn: 85%          │
├────────────────────────────────────────────────────────────────────────┤
│ 1. BẢNG THỐNG KÊ TỪNG BÀI GIAO (ASSIGNMENTS PROGRESS):                 │
│ Tên bài tập        | Loại        | Đã nộp | Đúng hạn | Trễ hạn | Tỷ lệ │
│ 1. Binary Search   | [Code Lab]  | 42/45  | 40       | 2       | 93%   │
│ 2. Cây AVL Xoay    | [Quiz]      | 35/45  | 35       | 0       | 77%   │
├────────────────────────────────────────────────────────────────────────┤
│ 2. DANH SÁCH HỌC VIÊN CẦN HỖ TRỢ (LAGGING LEARNERS):                   │
│ ⚠️ 3 Học sinh có tiến độ < 40% cần giảng viên nhắc nhở hạn nộp.        │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

1. **Khởi tạo dữ liệu**: Gọi `GET /api/v1/classes/{id}/report` $\rightarrow$ Nhận DTO tổng hợp `ClassReportDto` gồm `totalMembers`, `assignments[]`, `laggingLearners[]`.
2. **Xuất file báo cáo**: Giảng viên bấm nút **"Xuất CSV"** $\rightarrow$ Trình duyệt tự sinh file `.csv` chứa toàn bộ ma trận điểm của lớp để nộp phòng đào tạo.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`ClassReportView.vue`](file:///d:/FPT/metqua/frontend/src/views/ClassReportView.vue)
* **API Client**: `src/api/classes.ts`
* **Backend Controller**: [`ClassesController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/ClassesController.cs)
