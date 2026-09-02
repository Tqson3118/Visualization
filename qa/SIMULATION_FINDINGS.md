# SIMULATION_FINDINGS — Danh sách lỗi chi tiết

> **Nguồn**: Báo cáo tổng hợp `qa/SIMULATION_REPORT.md` (2026-09-02)
> **Cập nhật**: 2026-09-01 (bổ sung 3 P0 mới từ re-validate bởi agent M3)

---

## 1. Lỗi P1 (Lỗi lý thuyết / thuật ngữ)

### [SIM-001 / P1] Sai thuật ngữ tiêu đề Bảng băm (`structure.hashtable`)
- **File**: `frontend/src/engines/catalog.ts:99`
- **Vấn đề**: Tiêu đề hiện tại ghi: *"Bảng băm (Hash Table — địa chỉ mở: chuỗi nối kết)"*.
- **Phân tích**: Địa chỉ mở (Open Addressing) và Chuỗi nối kết (Separate Chaining) là 2 phương pháp giải quyết xung đột hoàn toàn độc lập và loại trừ nhau. Cần sửa thành *"Bảng băm (Hash Table — Chuỗi nối kết)"*.
- **Fix**:
```ts
// catalog.ts:99
title: 'Bảng băm (Hash Table — Chuỗi nối kết)'  // bỏ "địa chỉ mở:"
```

### [SIM-002 / P1] Lệch pha giữa Metadata/Mã giả và Thực thi thuật toán (`graph.dijkstra`)
- **File**: `frontend/src/engines/generators/graph/dijkstra.ts`
- **Vấn đề**: Metadata gắn nhãn $O((V+E)\log V)$ và mã giả hiển thị `PQ` (Priority Queue), nhưng code generator thực thi quét tuyến tính $O(V^2)$.
- **Fix đề xuất**: Bổ sung ghi chú giải thích rõ để tránh sinh viên hiểu sai cách tính độ phức tạp, hoặc refactor dùng Min-Heap thật.

---

## 2. Lỗi P2 (Thiết kế sư phạm / Input / Trực quan)

### [SIM-003 / P2] Trùng lặp ID nút tạm thời trong quá trình xóa nút 2 con ở BST (`tree.bst-delete`)
- **File**: `frontend/src/engines/generators/tree/bst.ts` (hàm `deleteWithTrace`)
- **Vấn đề**: Nút gốc và nút successor tạm thời mang cùng một ID `node:<minKey>` trong 1 bước trung gian.
- **Fix**: Đổi tén ID tạm thời thành `node:<minKey>-successor` hoặc dùng metadata để phân biệt.

### [SIM-004 / P2] Thiếu ô nhập `target` độc lập trong Bảng băm (`hash.search`, `hash.delete`)
- **File**: `frontend/src/engines/generators/hash/hashTable.ts`
- **Vấn đề**: Khiến người học không thể mô phỏng trường hợp tìm kiếm hoặc xóa một phần tử không tồn tại trong bảng băm.
- **Fix**: Bổ sung field `target` vào InputSchema của `hash.search` và `hash.delete`.

### [SIM-005 / P2] Trực quan hóa Dequeue bằng thao tác dịch mảng (`queue.dequeue`)
- **File**: `frontend/src/engines/generators/linear/queue.ts`
- **Vấn đề**: Animation dịch toàn bộ mảng sang trái $O(n)$ thay vì cố định các phần tử và tịnh tiến con trỏ $front$ trong mô hình hàng đợi vòng tròn $O(1)$.
- **Fix**: Vẽ mảng tĩnh + 2 con trỏ `Front` / `Rear` xoay vòng.

### [SIM-006 / P2] `structure.heap` không validate tính chất đống với dãy số tự nhập
- **File**: `frontend/src/engines/generators/structure/structures.ts`
- **Vấn đuề**: Khi người học nhập dãy số tùy ý (ví dụ `[3, 9, 5]`), mô phỏng vẫn khẳng định sai sự thật rằng $a[0]=3$ là phần tử lớn nhất.
- **Fix**: Tự động sắp xếp thành max-heap trước khi bắt đầu mô phỏng hoặc cảnh báo.

### [SIM-007 / P2] `structure.queue` mô hình trực quan dịch chuyển mảng
- **File**: `frontend/src/engines/generators/structure/structures.ts`
- **Vấn đề**: 5 bước; mô hình trực quan dùng dịch chuyển mảng giống `queue.dequeue` (cùng vấn đề).

---

## 3. Lỗi P3 (Cosmetic / metadata nhỏ)

### [SIM-008 / P3] `structure.binarytree` complexity `best: O(log n)` không chính xác
- **File**: `frontend/src/engines/catalog.ts:89`
- **Vấn đề**: Binary Tree tổng quát (không cân bằng) có thể skewed → search $O(n)$. `best: O(log n)` chỉ đúng với Balanced Binary Tree.
- **Fix**: Đổi về O(n) hoặc tách "Balanced Binary Tree" thành key riêng.

---

## 4. P0 nghi vấn từ agent M3 (2026-09-01) — ĐÃ VERIFY: 3/3 LÀ FALSE POSITIVE

> **Phương pháp verify**: Source review (SimulatorView.vue, useSimulation.ts, simulation.ts, useKeyboardShortcuts.ts, useCodeTracePlayback.ts) + Playwright browser test với cookie cleared.
> **Kết luận**: Không tìm thấy bug thật. Cả 3 P0 đều do suy diễn sai từ quan sát trực tiếp.

### [P0-NEW-001] **BÁC BỎ** — Auth guard KHÔNG block (suy diễn sai)

- **Quan sát ban đầu**: Navigate `/simulator/sort.selection` (key không-demo) khi header hiển thị user → page render OK.
- **Suy diễn sai**: App không cần auth để xem simulator.
- **Bằng chứng thật** (Playwright test ngày 2026-09-01):
  1. Clear cookie `dsa.session` qua DevTools
  2. Navigate `/simulator/sort.selection` → **URL tự động redirect** thành `/login?redirect=/simulator/sort.selection`
  3. Trang login render đầy đủ form đăng nhập
- **Kết luận**: Auth guard **HOẠT ĐỘNG ĐÚNG**. Lúc trước mình thấy page render vì cookie `dsa.session=1` vẫn tồn tại (HttpOnly session cookie thật), nhưng mình không verify cookie trước khi conclude.

### [P0-NEW-002] **BÁC BỎ** — Mỗi navigation trừ 1 tim (suy diễn sai)

- **Quan sát ban đầu**: Tim giảm từ 13/30 → 10/10 sau 3 navigation.
- **Suy diễn sai**: Mỗi navigation sang simulator trừ 1 tim.
- **Bằng chứng thật** (source review ngày 2026-09-01):
  - `SimulatorView.vue` **KHÔNG** import hoặc gọi `useHeartSystem`, `spendHeart`, `enterNode`
  - `grep "spendHeart|enterNode|useHeartSystem|hearts" SimulatorView.vue` → **0 match**
  - Logic trừ tim chỉ có trong `useHeartSystem.ts` → gọi từ **lesson visualizer** (`LessonVisualizer`, lesson study), **KHÔNG** từ simulator
- **Kết luận**: Simulator KHÔNG trừ tim. Lần trước mình ở trang lesson, click "Bước tới" trong LessonVisualizer mới trừ tim.

### [P0-NEW-003] **BÁC BỎ** — Button "Bước tới" navigate sang simulator khác (suy diễn sai)

- **Quan sát ban đầu**: Click "Bước tới" trên sort.selection → URL đổi sang search.linear.
- **Suy diễn sai**: Button "Bước tới" có behavior sai (navigate thay vì step).
- **Bằng chứng thật** (source review ngày 2026-09-01):
  - `SimulatorView.vue:196-199`: `ArrowRight` → `stepForward()` (tăng currentIndex, không navigate)
  - `simulation.ts`: `stepForward()` chỉ `currentIndex.value += 1`, **không** gọi `router.push`
  - `useCodeTracePlayback.ts`: `stepForward()` chỉ thay đổi frame, không có router
  - `useSimulation.ts`: ủy quyền cho store, không có router logic
  - **Không có** phím tắt nào navigate simulator (Space, ArrowRight, ArrowLeft, Home, End, [, ], \\ chỉ điều khiển playback)
- **Kết luận**: Button "Bước tới" hoạt động đúng. Lần trước URL đổi có thể do click nhầm breadcrumb/header link.

### Bài học rút ra

- **Không bao giờ** kết luận bug từ observation đơn lẻ mà không verify bằng code hoặc reproduction có kiểm soát.
- **Luôn check state** (cookie, localStorage, Pinia) trước khi test UI behavior.
- **Suy diễn từ URL change** là rất nguy hiểm — phải tìm code path gây ra change.

---

## 4. P0 nghi vấn từ agent M3 (2026-09-01) — ĐÃ VERIFY: 3/3 LÀ FALSE POSITIVE

> **Phương pháp verify**: Source review (SimulatorView.vue, useSimulation.ts, simulation.ts, useKeyboardShortcuts.ts, useCodeTracePlayback.ts) + Playwright browser test với cookie cleared.
> **Kết luận**: Không tìm thấy bug thật. Cả 3 P0 đều do suy diễn sai từ quan sát trực tiếp.

### [P0-NEW-001] **BÁC BỎ** — Auth guard KHÔNG block (suy diễn sai)

- **Quan sát ban đầu**: Navigate `/simulator/sort.selection` (key không-demo) khi header hiển thị user → page render OK.
- **Suy diễn sai**: App không cần auth để xem simulator.
- **Bằng chứng thật** (Playwright test ngày 2026-09-01):
  1. Clear cookie `dsa.session` qua DevTools
  2. Navigate `/simulator/sort.selection` → **URL tự động redirect** thành `/login?redirect=/simulator/sort.selection`
  3. Trang login render đầy đủ form đăng nhập
- **Kết luận**: Auth guard **HOẠT ĐỘNG ĐÚNG**. Lúc trước mình thấy page render vì cookie `dsa.session=1` vẫn tồn tại (HttpOnly session cookie thật), nhưng mình không verify cookie trước khi conclude.

### [P0-NEW-002] **BÁC BỎ** — Mỗi navigation trừ 1 tim (suy diễn sai)

- **Quan sát ban đầu**: Tim giảm từ 13/30 → 10/10 sau 3 navigation.
- **Suy diễn sai**: Mỗi navigation sang simulator trừ 1 tim.
- **Bằng chứng thật** (source review ngày 2026-09-01):
  - `SimulatorView.vue` **KHÔNG** import hoặc gọi `useHeartSystem`, `spendHeart`, `enterNode`
  - `grep "spendHeart|enterNode|useHeartSystem|hearts" SimulatorView.vue` → **0 match**
  - Logic trừ tim chỉ có trong `useHeartSystem.ts` → gọi từ **lesson visualizer** (`LessonVisualizer`, lesson study), **KHÔNG** từ simulator
- **Kết luận**: Simulator KHÔNG trừ tim. Lần trước mình ở trang lesson, click "Bước tới" trong LessonVisualizer mới trừ tim.

### [P0-NEW-003] **BÁC BỎ** — Button "Bước tới" navigate sang simulator khác (suy diễn sai)

- **Quan sát ban đầu**: Click "Bước tới" trên sort.selection → URL đổi sang search.linear.
- **Suy diễn sai**: Button "Bước tới" có behavior sai (navigate thay vì step).
- **Bằng chứng thật** (source review ngày 2026-09-01):
  - `SimulatorView.vue:196-199`: `ArrowRight` → `stepForward()` (tăng currentIndex, không navigate)
  - `simulation.ts`: `stepForward()` chỉ `currentIndex.value += 1`, **không** gọi `router.push`
  - `useCodeTracePlayback.ts`: `stepForward()` chỉ thay đổi frame, không có router
  - `useSimulation.ts`: ủy quyền cho store, không có router logic
  - **Không có** phím tắt nào navigate simulator (Space, ArrowRight, ArrowLeft, Home, End, [, ], \\ chỉ điều khiển playback)
- **Kết luận**: Button "Bước tới" hoạt động đúng. Lần trước URL đổi có thể do click nhầm breadcrumb/header link.

### Bài học rút ra

- **Không bao giờ** kết luận bug từ observation đơn lẻ mà không verify bằng code hoặc reproduction có kiểm soát.
- **Luôn check state** (cookie, localStorage, Pinia) trước khi test UI behavior.
- **Suy diễn từ URL change** là rất nguy hiểm — phải tìm code path gây ra change.

---


## 5. Tổng kết

| Phân loại | Số lượng |
|-------------|------------|
| Lỗi P1 (lý thuyết / thuật ngữ) | 2 (cũ) |
| Lỗi P2 (UX / Input) | 5 (cũ) |
| Lỗi P3 (cosmetic) | 1 (cũ) |
| **P0 nghi vấn (M3)** | **3 (đã verify: 3/3 false positive)** |
| **Tổng cộng** | **8 (cũ) + 3 P0 (false positive) = 11 findings, 0 bug mới** |