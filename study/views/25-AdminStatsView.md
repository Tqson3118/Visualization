# 📈 VIEW 25: BÁO CÁO THỐNG KÊ TOÀN DIỆN (ADMINSTATSVIEW)

* **Tên file Vue**: [`AdminStatsView.vue`](file:///d:/FPT/metqua/frontend/src/views/AdminStatsView.vue)
* **Đường dẫn URL**: `/admin/stats`
* **Route Name**: `admin-stats`
* **Quyền truy cập**: Chỉ Quản trị viên (`roles: ['ADMIN']`).

---

## 1. CẤU TRÚC GIAO DIỆN & BIỂU ĐỒ ECHARTS

```
┌────────────────────────────────────────────────────────────────────────┐
│  📈 BÁO CÁO THỐNG KÊ HỆ THỐNG                                          │
├────────────────────────────────────────────────────────────────────────┤
│ 4 THẺ CHỈ SỐ KPI CHÍNH:                                                │
│ ┌──────────────────────┐  ┌──────────────────────┐                     │
│ │ 👥 TỔNG NGƯỜI DÙNG   │  │ 📖 BÀI HỌC HOÀN THÀNH│                     │
│ │ 1,248 (+12% tuần)    │  │ 8,450 lượt           │                     │
│ └──────────────────────┘  └──────────────────────┘                     │
│ ┌──────────────────────┐  ┌──────────────────────┐                     │
│ │ 🔬 LƯỢT CHẠY MÔ PHỎNG│  │ 💎 DOANH THU NGỌC    │                     │
│ │ 24,190 lần           │  │ 154,000 Gems         │                     │
│ └──────────────────────┘  └──────────────────────┘                     │
├────────────────────────────────────────────────────────────────────────┤
│ KHU VỰC BIỂU ĐỒ TRỰC QUAN HÓA (ECharts Component):                     │
│ • Biểu đồ đường: Lượng người học hoạt động hàng ngày (DAU).            │
│ • Biểu đồ tròn: Phân bố độ khó bài học được làm nhiều nhất.            │
│ • Biểu đồ cột: Top 5 thuật toán được xem mô phỏng nhiều nhất.          │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

1. Khi Component mounted, gọi `GET /api/v1/admin/stats`.
2. Backend truy vấn tổng hợp dữ liệu từ các bảng `Users`, `UserNodeProgress`, `CodeRuns`, `GemTransactions`.
3. Dữ liệu được nạp vào component biểu đồ [`VChartLazy.vue`](file:///d:/FPT/metqua/frontend/src/components/ui/VChartLazy.vue) để vẽ biểu đồ mượt mà.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`AdminStatsView.vue`](file:///d:/FPT/metqua/frontend/src/views/AdminStatsView.vue)
* **Backend Controller**: [`AdminController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/AdminController.cs)
