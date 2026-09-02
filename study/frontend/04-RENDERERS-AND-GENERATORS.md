# 🎨 TÀI LIỆU RENDERERS & 44 GENERATORS MÔ PHỎNG

Tài liệu này phân tích chi tiết các bộ vẽ đồ họa Canvas/Pixi và hệ thống 44 Generator thuật toán trong `frontend/src/engines/`.

---

## 🖌️ 1. HỆ THỐNG CÁC BỘ RENDER ĐỒ HỌA (RENDERERS)

### 1. [`arrayRenderer.ts`](file:///d:/FPT/metqua/frontend/src/engines/renderers/arrayRenderer.ts) — Mảng & Thuật toán Sắp xếp / Tìm kiếm
* **Chế độ vẽ**: Bar Mode V3 (vẽ các cột số phát sáng có chiều cao tỷ lệ thuận với giá trị) hoặc Box Mode (các ô vuông số).
* **Hiệu ứng Swap**: Tính toán nội suy tọa độ $X(t) = X_0 + (X_1 - X_0) \cdot \text{ease}(t)$ trong khoảng 150–250ms.
* **Con trỏ biến**: Vẽ mũi tên chỉ thị con trỏ $i, j, \text{left}, \text{right}, \text{mid}$ bên dưới từng cột.

### 2. [`treeRenderer.ts`](file:///d:/FPT/metqua/frontend/src/engines/renderers/treeRenderer.ts) — Cây BST & Cây AVL
* **Bố cục cây**: Tự động tính toán tọa độ $(X, Y)$ của từng Node theo phân cấp tầng (Level-order Tree Layout).
* **Xoay cây AVL (Tree Rotations)**: Trực quan hóa 4 trường hợp xoay:
  * Xoay Trái-Trái (LL) & Xoay Phải-Phải (RR).
  * Xoay kép Trái-Phải (LR) & Xoay kép Phải-Trái (RL).
  * Tọa độ các Node dịch chuyển mượt mà về vị trí cân bằng mới.

### 3. [`graphRenderer.ts`](file:///d:/FPT/metqua/frontend/src/engines/renderers/graphRenderer.ts) — Đồ thị BFS / DFS / Dijkstra
* **Vẽ nút & Cạnh**: Render các đỉnh (Vertices), các cung có hướng/vô hướng và trọng số cạnh.
* **Trạng thái duyệt**:
  * Chưa thăm: Viền xám mờ.
  * Đang duyệt trong Queue/Stack: Viền vàng nhấp nháy.
  * Đã thăm (Visited): Tô màu xanh `resolved`.
* **Dijkstra**: Hiển thị bảng cập nhật khoảng cách ngắn nhất $d[v]$ theo từng bước quét.

### 4. [`stackQueueRenderer.ts`](file:///d:/FPT/metqua/frontend/src/engines/renderers/stackQueueRenderer.ts) & [`hashTableRenderer.ts`](file:///d:/FPT/metqua/frontend/src/engines/renderers/hashTableRenderer.ts)
* **Stack**: Trực quan hóa đáy (Bottom) và đỉnh (Top), hiệu ứng phần tử rơi vào (Push) và bay ra (Pop).
* **Queue**: Trực quan hóa đầu (Front) và đuôi (Rear), hiệu ứng xếp hàng (FIFO).
* **Hash Table**: Bảng băm với các Bucket từ $0 \dots M-1$, giải quyết đụng độ bằng danh sách liên kết chuỗi (Separate Chaining).

---

## 📚 2. DANH MỤC 44 GENERATORS THUẬT TOÁN ([`catalog.ts`](file:///d:/FPT/metqua/frontend/src/engines/catalog.ts))

Mọi Generator đều có Factory đăng ký tập trung tại `catalog.ts`:

| Nhóm | Số lượng | Danh sách thuật toán tiêu biểu |
|---|:---:|---|
| **Sắp xếp (Sort)** | 6 | Bubble Sort, Selection Sort, Insertion Sort, Merge Sort, Quick Sort (Lomuto), Heap Sort. |
| **Tìm kiếm (Search)** | 2 | Linear Search (Tuyến tính), Binary Search (Nhị phân chia đôi). |
| **Ngăn xếp & Hàng đợi** | 5 | Stack Push, Stack Pop, Stack Peek, Queue Enqueue, Queue Dequeue. |
| **Danh sách liên kết** | 4 | List Insert, List Delete, List Search, List Traverse. |
| **Cây & Cân bằng** | 8 | BST Insert, BST Delete, BST Search, Preorder, Inorder, Postorder, Level-order, AVL Tree Insert (LL/RR/LR/RL). |
| **Đống nhị phân (Heap)**| 3 | Heap Insert (Bubble up), Heap Extract-Max (Sift down), Heapify $O(n)$. |
| **Bảng băm (Hash)** | 3 | Hash Insert, Hash Search, Hash Delete (Separate Chaining). |
| **Đồ thị (Graph)** | 3 | Graph BFS (Hàng đợi), Graph DFS (Ngăn xếp), Dijkstra Shortest Path. |
| **Cấu trúc dữ liệu** | 10 | Trực quan hóa tổng quan 10 cấu trúc dữ liệu nền tảng. |
