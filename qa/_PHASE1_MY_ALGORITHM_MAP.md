# SIMULATION_ALGORITHM_MAP — Bảng tham chiếu 44 mô phỏng

> **Phiên bản**: Phase 1 (đọc source + 1 demo test thật, ngày 2026-09-01)
> **Nguồn**: `frontend/src/engines/catalog.ts`, `frontend/src/engines/generators/`, `frontend/src/engines/renderers/`, `frontend/src/views/SimulatorView.vue`
> **Đối chiếu**: complexity chuẩn theo CLRS / Sedgewick / Goodrich.

---

## 0. Tổng quan

- **Tổng số key**: **44** (đã xác nhận trong `catalog.ts`).
- **Phân nhóm theo `dataStructure`**: Mảng, Ngăn xếp, Hàng đợi, Danh sách liên kết, Cây nhị phân, Cây nhị phân tìm kiếm, Cây AVL, Đống nhị phân, Bảng băm, Đồ thị.
- **Phân nhóm theo `category`**: `structure` (10) + `algorithm` (34).
- **Mức độ `level`**: `basic` + `advanced`.
- **Demo công khai (`demoAllowed: true`)**: **3 key** — `sort.bubble`, `search.binary`, `graph.bfs`. Tất cả 41 key còn lại cần `auth.isAuthenticated === true`.
- **Render pipeline**: PixiJS (chính) + Canvas 2D (fallback). Có 4 painter chuyên dụng: `PixiArrayPainter`, `PixiGraphPainter`, `PixiLinearPainter`, `PixiTreePainter`.
- **Step format (mỗi bước)**: `structure (kind/elements/links) + explanation (1–4 câu tiếng Việt) + pseudocodeLine + highlights + annotations + variables + stats (comparisons/swaps/writes)`.

---

## 1. Bảng chính — 44 mô phỏng

> **Cột đánh dấu**:
> ✅ = Complexity đúng lý thuyết · ⚠️ = Có nghi vấn / sai một phần · ❌ = Sai rõ ràng so với textbook

| # | Key | Tên (tiếng Việt) | CTDL | Category | Level | Complexity (best/avg/worst/space) | Demo? | Render type | File generator (path) | Pseudocode (số dòng) | Complexity audit |
|---|-----|--------------------|------|----------|-------|-------------------------------------|-------|-------------|------------------------|----------------------|------------------|
| 1 | `sort.bubble` | Sắp xếp nổi bọt (Bubble Sort) | Mảng | algorithm | basic | O(n) / O(n²) / O(n²) / O(1) | ✅ | array bar | `generators/sort/bubble.ts` | 10 | ✅ Đúng (early termination cho best) |
| 2 | `sort.selection` | Sắp xếp chọn (Selection Sort) | Mảng | algorithm | basic | O(n²) / O(n²) / O(n²) / O(1) | ❌ | array bar | `generators/sort/selection.ts` | (chưa đếm) | ✅ Đúng (không có early termination) |
| 3 | `sort.insertion` | Sắp xếp chèn (Insertion Sort) | Mảng | algorithm | basic | O(n) / O(n²) / O(n²) / O(1) | ❌ | array bar | `generators/sort/insertion.ts` | (chưa đếm) | ✅ Đúng |
| 4 | `sort.merge` | Sắp xếp trộn (Merge Sort) | Mảng | algorithm | advanced | O(n log n) / O(n log n) / O(n log n) / O(n) | ❌ | array bar | `generators/sort/merge.ts` | (chưa đếm) | ✅ Đúng (mảng phụ O(n)) |
| 5 | `sort.quick` | Sắp xếp nhanh (Quick Sort — Lomuto) | Mảng | algorithm | advanced | O(n log n) / O(n log n) / O(n²) / O(log n) | ❌ | array bar | `generators/sort/quick.ts` | (chưa đếm) | ✅ Đúng (Lomuto pivot) |
| 6 | `sort.heap` | Sắp xếp vun đống (Heap Sort) | Mảng | algorithm | advanced | O(n log n) / O(n log n) / O(n log n) / O(1) | ❌ | array+tree | `generators/sort/heap.ts` | 12 (last line "end procedures" — lỗi chính tả nhỏ) | ✅ Đúng (in-place) |
| 7 | `search.linear` | Tìm kiếm tuyến tính (Linear Search) | Mảng | algorithm | basic | O(1) / O(n) / O(n) / O(1) | ❌ | array bar | `generators/search/linear.ts` | (chưa đếm) | ✅ Đúng |
| 8 | `search.binary` | Tìm kiếm nhị phân (Binary Search) | Mảng | algorithm | basic | O(1) / O(log n) / O(log n) / O(1) | ✅ | array bar | `generators/search/binary.ts` | 9 | ✅ Đúng |
| 9 | `stack.push` | Ngăn xếp — Đẩy (Stack Push) | Ngăn xếp | algorithm | basic | O(1) / O(1) / O(1) / O(n) | ❌ | linear | `generators/linear/stack.ts` | (chưa đếm) | ✅ Đúng |
| 10 | `stack.pop` | Ngăn xếp — Lấy ra (Stack Pop) | Ngăn xếp | algorithm | basic | O(1) / O(1) / O(1) / O(n) | ❌ | linear | `generators/linear/stack.ts` | (chưa đếm) | ✅ Đúng |
| 11 | `stack.peek` | Ngăn xếp — Xem đỉnh (Stack Peek) | Ngăn xếp | algorithm | basic | O(1) / O(1) / O(1) / O(n) | ❌ | linear | `generators/linear/stack.ts` | (chưa đếm) | ✅ Đúng |
| 12 | `queue.enqueue` | Hàng đợi — Thêm (Enqueue) | Hàng đợi | algorithm | basic | O(1) / O(1) / O(1) / O(n) | ❌ | linear | `generators/linear/queue.ts` | (chưa đếm) | ✅ Đúng |
| 13 | `queue.dequeue` | Hàng đợi — Lấy ra (Dequeue) | Hàng đợi | algorithm | basic | O(1) / O(1) / O(1) / O(n) | ❌ | linear | `generators/linear/queue.ts` | (chưa đếm) | ✅ Đúng |
| 14 | `list.insert` | Danh sách liên kết — Chèn | DSLK đơn | algorithm | basic | O(1) / O(1) / O(1) / O(n) | ❌ | linear | `generators/linear/linkedList.ts` | (chưa đếm) | ✅ Đúng (head) |
| 15 | `list.delete` | Danh sách liên kết — Xóa | DSLK đơn | algorithm | basic | O(1) / O(n) / O(n) / O(n) | ❌ | linear | `generators/linear/linkedList.ts` | (chưa đếm) | ✅ Đúng (search O(n)) |
| 16 | `list.search` | Danh sách liên kết — Tìm | DSLK đơn | algorithm | basic | O(1) / O(n) / O(n) / O(n) | ❌ | linear | `generators/linear/linkedList.ts` | (chưa đếm) | ✅ Đúng |
| 17 | `list.traverse` | Danh sách liên kết — Duyệt | DSLK đơn | algorithm | basic | O(n) / O(n) / O(n) / O(1) | ❌ | linear | `generators/linear/linkedList.ts` | (chưa đếm) | ✅ Đúng |
| 18 | `tree.bst-insert` | BST — Chèn | Cây BST | algorithm | basic | O(log n) / O(log n) / O(n) / O(n) | ❌ | tree | `generators/tree/bst.ts` | 9 | ✅ Đúng |
| 19 | `tree.bst-delete` | BST — Xóa | Cây BST | algorithm | basic | O(log n) / O(log n) / O(n) / O(n) | ❌ | tree | `generators/tree/bst.ts` | 12 (3 trường hợp + transplant) | ✅ Đúng (in-order successor) |
| 20 | `tree.bst-search` | BST — Tìm | Cây BST | algorithm | basic | O(log n) / O(log n) / O(n) / O(n) | ❌ | tree | `generators/tree/bst.ts` | 7 | ✅ Đúng |
| 21 | `tree.bst-preorder` | BST — Duyệt trước (Pre-order) | Cây BST | algorithm | basic | O(n) / O(n) / O(n) / O(n) | ❌ | tree | `generators/tree/bst.ts` | 5 (dùng chung traversal PSEUDO) | ✅ Đúng |
| 22 | `tree.bst-inorder` | BST — Duyệt giữa (In-order) | Cây BST | algorithm | basic | O(n) / O(n) / O(n) / O(n) | ❌ | tree | `generators/tree/bst.ts` | 5 | ✅ Đúng |
| 23 | `tree.bst-postorder` | BST — Duyệt sau (Post-order) | Cây BST | algorithm | basic | O(n) / O(n) / O(n) / O(n) | ❌ | tree | `generators/tree/bst.ts` | 5 | ✅ Đúng |
| 24 | `tree.bst-levelorder` | BST — Duyệt theo mức (BFS) | Cây BST | algorithm | basic | O(n) / O(n) / O(n) / O(n) | ❌ | tree | `generators/tree/bst.ts` | 7 | ✅ Đúng |
| 25 | `tree.avl-insert` | Cây AVL — Chèn (cân bằng) | Cây AVL | algorithm | advanced | O(log n) / O(log n) / O(log n) / O(n) | ❌ | tree | `generators/tree/avl.ts` | (chưa đếm) | ✅ Đúng |
| 26 | `heap.insert` | Đống — Chèn (Heap Insert) | Đống nhị phân | algorithm | basic | O(1) / O(log n) / O(log n) / O(n) | ❌ | heap | `generators/heap/heapOps.ts` | (chưa đếm) | ✅ Đúng |
| 27 | `heap.extract` | Đống — Lấy max (Extract-Max) | Đống nhị phân | algorithm | basic | O(log n) / O(log n) / O(log n) / O(n) | ❌ | heap | `generators/heap/heapOps.ts` | (chưa đếm) | ✅ Đúng |
| 28 | `heap.heapify` | Đống — Vun đống (Heapify) | Đống nhị phân | algorithm | advanced | O(n) / O(n) / O(n) / O(1) | ❌ | heap | `generators/heap/heapOps.ts` | (chưa đếm) | ✅ Đúng (bottom-up O(n)) |
| 29 | `hash.insert` | Bảng băm — Chèn | Bảng băm | algorithm | basic | O(1) / O(1) / O(n) / O(n) | ❌ | hashtable | `generators/hash/hashTable.ts` | (chưa đếm) | ✅ Đúng (separate chaining) |
| 30 | `hash.search` | Bảng băm — Tìm | Bảng băm | algorithm | basic | O(1) / O(1) / O(n) / O(n) | ❌ | hashtable | `generators/hash/hashTable.ts` | (chưa đếm) | ✅ Đúng |
| 31 | `hash.delete` | Bảng băm — Xóa | Bảng băm | algorithm | basic | O(1) / O(1) / O(n) / O(n) | ❌ | hashtable | `generators/hash/hashTable.ts` | (chưa đếm) | ✅ Đúng |
| 32 | `graph.bfs` | Đồ thị — BFS | Đồ thị | algorithm | basic | O(V+E) / O(V+E) / O(V+E) / O(V) | ✅ | graph | `generators/graph/bfs.ts` | (chưa đếm) | ✅ Đúng |
| 33 | `graph.dfs` | Đồ thị — DFS | Đồ thị | algorithm | basic | O(V+E) / O(V+E) / O(V+E) / O(V) | ❌ | graph | `generators/graph/dfs.ts` | (chưa đếm) | ✅ Đúng |
| 34 | `graph.dijkstra` | Đồ thị — Dijkstra (đường đi ngắn nhất) | Đồ thị | algorithm | advanced | O((V+E) log V) / O((V+E) log V) / O((V+E) log V) / O(V) | ❌ | graph | `generators/graph/dijkstra.ts` | (chưa đếm) | ✅ Đúng (binary heap PQ) |
| 35 | `structure.array` | Mảng (Array) | Mảng | structure | basic | O(1) / O(1) / O(1) / O(n) | ❌ | array bar | `generators/structure/structures.ts` | (intro ≥5 step) | ✅ Đúng (read/write O(1)) |
| 36 | `structure.linkedlist` | Danh sách liên kết đơn | DSLK đơn | structure | basic | O(1) / O(n) / O(n) / O(n) | ❌ | linear | `generators/structure/structures.ts` | (intro) | ✅ Đúng |
| 37 | `structure.stack` | Ngăn xếp (Stack) | Ngăn xếp | structure | basic | O(1) / O(1) / O(1) / O(n) | ❌ | linear | `generators/structure/structures.ts` | (intro) | ✅ Đúng |
| 38 | `structure.queue` | Hàng đợi (Queue) | Hàng đợi | structure | basic | O(1) / O(1) / O(1) / O(n) | ❌ | linear | `generators/structure/structures.ts` | (intro) | ✅ Đúng |
| 39 | `structure.binarytree` | Cây nhị phân (Binary Tree) | Cây nhị phân | structure | basic | **O(log n)** / O(n) / O(n) / O(n) | ❌ | tree | `generators/structure/structures.ts` | (intro) | ⚠️ **`best: O(log n)` KHÔNG chính xác cho Binary Tree tổng quát** — chỉ đúng khi cây cân bằng. Cần đổi về O(n) hoặc đổi tên thành "Balanced Binary Tree". |
| 40 | `structure.bst` | Cây nhị phân tìm kiếm (BST) | Cây BST | structure | basic | O(log n) / O(log n) / O(n) / O(n) | ❌ | tree | `generators/structure/structures.ts` | (intro) | ✅ Đúng |
| 41 | `structure.avl` | Cây AVL (cân bằng) | Cây AVL | structure | advanced | O(log n) / O(log n) / O(log n) / O(n) | ❌ | tree | `generators/structure/structures.ts` | (intro — **chỉ 4 step, không minh họa 4 rotation**) | ⚠️ **Quality P0**: Animation intro AVL **không dạy được 4 rotation** (LL/RR/LR/RL). Sinh viên chỉ thấy text "thực hiện xoay" mà không thấy step-by-step. |
| 42 | `structure.heap` | Đống nhị phân (Binary Heap — max-heap) | Đống nhị phân | structure | advanced | O(1) / O(log n) / O(log n) / O(n) | ❌ | heap | `generators/structure/structures.ts` | (intro) | ✅ Đúng |
| 43 | `structure.hashtable` | Bảng băm (Hash Table — địa chỉ mở: chuỗi nối kết) | Bảng băm | structure | basic | O(1) / O(1) / O(n) / O(n) | ❌ | hashtable | `generators/structure/structures.ts` | (intro) | ✅ Đúng |
| 44 | `structure.graph` | Đồ thị (Graph — có hướng/vô hướng, trọng số) | Đồ thị | structure | advanced | O(V+E) / O(V+E) / O(V+E) / O(V+E) | ❌ | graph | `generators/structure/structures.ts` | (intro) | ✅ Đúng |

---

## 2. Ghi chú quan trọng về từng thuật toán (Phase 1)

### 2.1. Sort (6 key)
- **Bubble Sort**: Pseudocode 10 dòng, **có early termination** (swapped=false → return) → best = O(n). Animation push step "mảng đã sắp xếp, kết thúc sớm" → đúng. **Cảnh báo nhỏ**: có dòng `line: 10` "end procedure" ở step cuối, nhưng PSEUDOCODE chỉ 9 dòng kết thúc bằng "end procedure" — line 10 là step bonus. Cần verify PseudocodePanel không highlight dòng trống.
- **Selection Sort**: chưa đọc chi tiết.
- **Insertion Sort**: chưa đọc chi tiết.
- **Merge Sort**: chưa đọc chi tiết.
- **Quick Sort**: chưa đọc chi tiết.
- **Heap Sort**: Pseudocode 12 dòng, **lỗi chính tả nhỏ** ở dòng cuối `"end procedures"` (có chữ "s"). Không ảnh hưởng thuật toán, nhưng **gây ấn tượng xấu** trong mã giả.

### 2.2. Search (2 key)
- **Linear Search**: chưa đọc chi tiết.
- **Binary Search**: Pseudocode 9 dòng — chuẩn CLRS.

### 2.3. Linear (7 key: stack, queue, list)
- Tất cả ở file `generators/linear/{stack,queue,linkedList}.ts` (3 file, 7 generator).
- Complexity từng operation đều đúng lý thuyết. Chưa verify logic animation.

### 2.4. Tree (8 key: 7 BST + 1 AVL)
- **BST (7 key)**: File `bst.ts` rất chất lượng, dùng **in-order successor** cho delete (biến thể hợp lệ của CLRS §12.3). Mỗi operation có pseudocode riêng. **Cảnh báo logic nhỏ** trong `deleteWithTrace` (xem findings §2).
- **AVL (1 key)**: File `avl.ts` chưa đọc chi tiết.

### 2.5. Heap (3 key)
- File `heapOps.ts` gom 3 operation (insert, extract, heapify). Chưa đọc chi tiết.

### 2.6. Hash (3 key)
- File `hashTable.ts` gom 3 operation. Chưa đọc chi tiết.

### 2.7. Graph (3 key: BFS, DFS, Dijkstra)
- `dijkstra.ts` pseudocode bắt đầu bằng `procedure Dijkstra(G, s)` — đúng. Input có 7 fields (preset, directed, weighted, vertices, edges, source, target) → cho phép nhiều scenario test.

### 2.8. Structure (10 key)
- Tất cả ở file `structures.ts` (727 dòng).
- Mỗi structure intro có **≥ 5 step giới thiệu** (khởi tạo, access, insert, delete, search, tổng kết complexity) → đúng SDD §4.2.
- **Vấn đề quality**:
  - `structure.avl` chỉ 4 step intro, **không minh họa được 4 rotation** — sinh viên không thể hiểu rotation.
  - `structure.bst` (intro) animation xóa nút 2 con **chỉ highlight, KHÔNG thực sự xóa** — gây hiểu nhầm "nút 50 bị thay bằng 60" nhưng cây vẫn đủ 7 nút.

---

## 3. Phụ lục A — Cấu trúc 1 step

```typescript
interface Step {
  index: number;                                   // thứ tự step (0-based)
  structure: Structure;                            // trạng thái CTDL tại step này
  explanation: string;                             // 1–4 câu tiếng Việt
  pseudocodeLine: number;                          // dòng mã giả highlight (1-based)
  highlights: string[];                            // id element được highlight
  annotations: string[];                           // text thêm (vd: 'i=2, j=3', 'a[2]=7 > a[3]=4 → hoán đổi')
  variables: Record<string, string | number | boolean | null>;
  stats: { comparisons: number; swaps: number; writes: number };
  version: 1;
}
```

## 4. Phụ lục B — Thống kê chất lượng code (Phase 1)

| Metric | Giá trị |
|--------|---------|
| Tổng generator files | 19 (gom 44 generator factory) |
| Tổng pseudocode lines (sample) | ~150+ dòng |
| File lớn nhất | `structures.ts` (727 dòng, 10 factory) |
| File thứ 2 | `bst.ts` (641 dòng, 7 factory) |
| Có lỗi chính tả pseudocode | 1 (`sort/heap.ts` "end procedures") |
| Generator nào KHÔNG tìm thấy PSEUDOCODE qua regex | 2 (`tree/bst.ts`, `structure/structures.ts` — vì chứa nhiều PSEUDOCODE_* riêng) |
| Console errors khi load `/simulations` | 0 |
| Console errors khi load `/simulator/sort.bubble` | 0 |
| Console warnings khi test | 1 (chưa rõ nguồn, cần verify) |
