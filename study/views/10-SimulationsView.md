# 🔭 VIEW 10: THƯ VIỆN THUẬT TOÁN (SIMULATIONSVIEW)

* **Tên file Vue**: [`SimulationsView.vue`](file:///d:/FPT/metqua/frontend/src/views/SimulationsView.vue)
* **Đường dẫn URL**: `/simulations`
* **Route Name**: `simulations`
* **Quyền truy cập**: Công khai (`meta: { public: true }`).

---

## 1. CẤU TRÚC GIAO DIỆN

```
┌────────────────────────────────────────────────────────────────────────┐
│  🔭 THƯ VIỆN 44 THUẬT TOÁN & CẤU TRÚC DỮ LIỆU                           │
│  [ Tab 1: Danh mục mô phỏng (44) ]       [ Tab 2: CheatSheet Big-O ]   │
├────────────────────────────────────────────────────────────────────────┤
│ BỘ LỌC THEO NHÓM THUẬT TOÁN:                                           │
│ [ Tất cả ] [ 🔀 Sắp xếp (6) ] [ 🔍 Tìm kiếm (2) ] [ 🌲 Cây (8) ]       │
│ [ 🕸️ Đồ thị (3) ] [ 🧱 Cấu trúc Tuyến tính (6) ] [ 🗝️ Bảng băm (3) ]    │
├────────────────────────────────────────────────────────────────────────┤
│ LƯỚI CARD THUẬT TOÁN (Gom nhóm theo Prefix Key):                       │
│                                                                        │
│ ── NHÓM SẮP XẾP (SORTING) ──────────────────────────────────────────   │
│ ┌──────────────────────┐  ┌──────────────────────┐                     │
│ │ 🟢 Bubble Sort       │  │ 🟢 Quick Sort (Lomuto│                     │
│ │ Cấu trúc: Mảng       │  │ Cấu trúc: Mảng       │                     │
│ │ Big-O: O(n²) [Vàng]  │  │ Big-O: O(n log n)[Xk]│                     │
│ │ [ Chạy mô phỏng → ]  │  │ [ Chạy mô phỏng → ]  │                     │
│ └──────────────────────┘  └──────────────────────┘                     │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Nạp danh mục từ Single Source of Truth
1. View nạp hằng số `CATALOG` từ [`src/engines/catalog.ts`](file:///d:/FPT/metqua/frontend/src/engines/catalog.ts).
2. Tự động nhóm các thuật toán theo tiền tố (`sort.*`, `search.*`, `tree.*`, `heap.*`, `hash.*`, `graph.*`, `structure.*`).
3. Chip Big-O tự động đổi màu theo độ phức tạp thời gian:
   * $O(1), O(\log n), O(n), O(n \log n)$: Badge Xanh lá (Success).
   * $O(n^2)$: Badge Vàng cam (Warning).
   * $O(2^n), O(n!)$: Badge Đỏ (Danger).

### 🔹 Flow 2: Chuyển tab sang Bảng tra cứu CheatSheet
* Khi bấm sang Tab 2, component [`CheatSheetTable.vue`](file:///d:/FPT/metqua/frontend/src/components/lesson/CheatSheetTable.vue) được lazy-load động (`defineAsyncComponent`) để tiết kiệm dung lượng ban đầu.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`SimulationsView.vue`](file:///d:/FPT/metqua/frontend/src/views/SimulationsView.vue)
* **Frontend Component**: `src/components/lesson/CheatSheetTable.vue`
* **Catalog Data**: [`frontend/src/engines/catalog.ts`](file:///d:/FPT/metqua/frontend/src/engines/catalog.ts)
