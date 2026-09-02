# 📊 VIEW 13: BẢNG TRA CỨU ĐỘ PHỨC TẠP BIG-O (CHEATSHEETVIEW)

* **Tên file Vue**: [`CheatSheetView.vue`](file:///d:/FPT/metqua/frontend/src/views/CheatSheetView.vue)
* **Đường dẫn URL**: `/cheatsheet`
* **Route Name**: `cheatsheet`
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

---

## 1. CẤU TRÚC GIAO DIỆN & BẢNG SO SÁNH

Màn hình hiển thị bảng tra cứu toàn diện về Độ phức tạp Thời gian (Time Complexity) và Không gian (Space Complexity) cho các cấu trúc dữ liệu và giải thuật trong chương trình:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ 📊 BẢNG TRA CỨU NHANH ĐỘ PHỨC TẠP (BIG-O CHEATSHEET)                                   │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 1. CẤU TRÚC DỮ LIỆU (DATA STRUCTURES):                                                 │
│ Cấu trúc           | Truy cập (Access) | Tìm kiếm (Search) | Chèn (Insert) | Xóa (Delete) │
│ Mảng (Array)       | O(1) [Xanh]       | O(n) [Vàng]       | O(n) [Vàng]   | O(n) [Vàng]  │
│ DSLK (Linked List) | O(n) [Vàng]       | O(n) [Vàng]       | O(1) [Xanh]   | O(1) [Xanh]  │
│ Cây BST            | O(log n) [Xanh]   | O(log n) [Xanh]   | O(log n)      | O(log n)     │
│ Cây AVL            | O(log n) [Xanh]   | O(log n) [Xanh]   | O(log n)      | O(log n)     │
│ Bảng băm (Hash)    | N/A               | O(1) [Xanh]       | O(1) [Xanh]   | O(1) [Xanh]  │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 2. THUẬT TOÁN SẮP XẾP (SORTING ALGORITHMS):                                            │
│ Thuật toán         | Tốt nhất (Best)   | Trung bình (Avg)  | Xấu nhất      | Bộ nhớ       │
│ Bubble Sort        | O(n)              | O(n²)             | O(n²)         | O(1)         │
│ Merge Sort         | O(n log n)        | O(n log n)        | O(n log n)    | O(n)         │
│ Quick Sort         | O(n log n)        | O(n log n)        | O(n²)         | O(log n)     │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

1. **Tra cứu & So sánh**: Người học xem nhanh đặc tính kỹ thuật của từng thuật toán để áp dụng vào bài tập phỏng vấn hoặc tối ưu code.
2. **Nhảy nhanh sang Simulator**: Mỗi dòng trong bảng đều có nút bấm *"Xem trực quan"* $\rightarrow$ Nhấp vào sẽ điều hướng thẳng sang `/simulator/{key}` để quan sát thuật toán đó vận hành.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`CheatSheetView.vue`](file:///d:/FPT/metqua/frontend/src/views/CheatSheetView.vue)
* **Frontend Component**: `src/components/lesson/CheatSheetTable.vue`
* **Catalog Data**: [`frontend/src/engines/catalog.ts`](file:///d:/FPT/metqua/frontend/src/engines/catalog.ts)
