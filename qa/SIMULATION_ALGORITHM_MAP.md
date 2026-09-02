# SIMULATION_ALGORITHM_MAP — Bản đồ thuật toán 44 mô phỏng DsaVisual (v2 — sau khi test)

> Xuất bởi agent QA. v2 cập nhật theo kết quả Giai đoạn 2 (browser + vitest invariant audit).
> Chuẩn mực: lý thuyết thuật toán (CLRS/textbook). Source code chỉ là đối tượng kiểm chứng.
> Phương pháp: (1) đọc source generators/renderers/views; (2) chạy từng sim trên browser (DOM extraction + screenshot);
> (3) vitest audit tự động trên toàn bộ generators (invariants: sorted/multiset/heap/BST+AVL/traversal order/hash bucket/dijkstra reference).

## 0. Kết luận tổng quát

- **Không phát hiện lỗi thuật toán P0** (không sim nào dạy sai bước chạy chuẩn). 44/44 generator sinh bước đúng thứ tự thao tác chuẩn.
- Bằng chứng định lượng (vitest, input mặc định): 6 sorts cho mảng cuối tăng dần + bảo toàn multiset; bubble swap=8 = đúng số inverses của [5,3,8,1,9,2]; heap.insert/extract/heapify giữ max-heap property; 4 traversal BST đúng thứ tự textbook; AVL cuối cùng thỏa BST+|bf|≤1 qua 4 case (LL/RR/RL/no-rot); hash: mỗi key nằm đúng bucket key % m; dijkstra d=[0,9,10,16,25,1,2] khớp tham chiếu tính tay từng số; search.linear/binary đúng vị trí tìm thấy + xử lý miss.
- Các vấn đề thực sự nằm ở **metadata/terminology (P1), demo mặc định thiếu tính đại diện (P2), bảng biến stale (P2), vài chi tiết UI (P3)** — chi tiết trong SIMULATION_FINDINGS.md.

## 1. Kiến trúc (đã xác minh trên runtime)

- catalog.ts (44 key) ⇄ shared/simulation-catalog.json khớp; registry đăng ký factory; buildGenerator lấy metadata theo key.
- Playback: stores/simulation.ts — interval 1200/speed ms, speed 0.25x–4x, breakpoints theo dòng mã giả, jumpTo qua slider.
- Renderers: array (bar-mode + wrap), stackqueue (dọc/ngang), list, tree, hashtable, graph (canvas 2D, màu status nhất quán).
- UI: SimulatorView 3 cột (mã giả | canvas+điều khiển | giải thích); ExplainPanel có Transition out-in 0.2s+0.2s (nguyên nhân text giải thích trễ ~400ms khi chuyển bước nhanh).
- Guest gate: chỉ 3 key demoAllowed (sort.bubble, search.binary, graph.bfs); key khác khi chưa đăng nhập → redirect /login?redirect=/simulator/… (đã quan sát trực tiếp).

## 2. Bảng 44 mô phỏng — trạng thái đã kiểm chứng

Chú thích: ✅ thuật toán đúng + demo mặc định có giá trị học; ✅* thuật toán đúng nhưng demo mặc định kém; ⚠️ có vấn đề cần sửa (chi tiết FINDINGS).

### 2.1 Sorting (6) — tất cả ✅

| # | Key | Generator | Kiểm chứng định lượng (vitest, default input) | Browser |
|---|-----|-----------|------------------------------------------------|---------|
| 1 | sort.bubble | sort/bubble.ts | 68 bước; cmp=15 (n(n-1)/2 — không early-break vì pass cuối mới ổn); swap=8 = đúng số inverses; final [1,2,3,5,8,9] | Bar-mode, con trỏ i/j đúng |
| 2 | sort.selection | sort/selection.ts | 66 bước; cmp=15; swap=4; final sorted | Con trỏ i/minIdx/j |
| 3 | sort.insertion | sort/insertion.ts | 56 bước; cmp=11; writes=13 (=8 dịch + 5 chèn key); final sorted | |
| 4 | sort.merge | sort/merge.ts | 114 bước; cmp=11; writes=32; final sorted | CallStackPanel đệ quy |
| 5 | sort.quick | sort/quick.ts (Lomuto) | 71 bước; cmp=10; swap=4; final sorted | Con trỏ low/high/i/j/pivot |
| 6 | sort.heap | sort/heap.ts | 146 bước; cmp=39; swap=25; final sorted (n=10) | Cây heap + mảng |

### 2.2 Searching (2) — ✅ (binary có auto-sort minh bạch)

| # | Key | Kiểm chứng | Ghi chú |
|---|-----|-----------|---------|
| 7 | search.linear | target 8 trong [5,3,8,1,9,2] → tìm thấy tại 2, cmp=3 | ✅ |
| 8 | search.binary | **input chưa sort → bước 2 thông báo "tự sắp xếp thành [1,3,7,9]" + canvas chuyển mảng sorted** (minh bạch); tìm thấy đúng vị trí; miss case xử lý đúng | ✅ design note: cân nhắc cảnh báo mạnh hơn |

### 2.3 Stack/Queue/List (9)

| # | Key | Kết quả |
|---|-----|---------|
| 9–11 | stack.push / stack.pop / stack.peek | ⚠️ Thuật toán LIFO đúng, NHƯNG 3 trang chạy CHUNG demo mặc định ['Push 5','Push 3','Pop'] — tham số _preferred là dead code (linear/stack.ts:108) → stack.peek mặc định không hề demo peek |
| 12–13 | queue.enqueue / queue.dequeue | ✅ Model mảng con trỏ (front++/rear++, cell thành muted) KHỚP pseudocode; dequeue demo mặc định trộn Enqueue/Dequeue; giới hạn "mảng tuyến tính cứng" được giải thích rõ trong step |
| 14 | list.insert | ✅* Thuật toán đúng (insertHead); demo mặc định chèn vào danh sách RỖNG (initialValues=[]) — 5 bước; bảng biến stale (n=0 suốt demo dù canvas có nút) |
| 15 | list.delete | ✅* Fallback [10,20,30,40] xóa đầu — 3 bước, đúng; bảng biến stale (n=4 sau khi đã xóa) |
| 16 | list.search | ✅ cmp=4 trên [10,20,30,40]; bảng biến stale (current không chạy theo) |
| 17 | list.traverse | ✅ 7 bước |

### 2.4 Tree (8)

| # | Key | Kết quả |
|---|-----|---------|
| 18 | tree.bst-insert | ✅ chèn 25 vào [50,30,70,20,40,60,80], cmp=3 đúng path 50→30→20 |
| 19 | tree.bst-delete | ✅* thuật toán 3-case đúng (đã test xóa nút 2 con: copy successor 60 → xóa đệ quy — mid-state 2 nút "60" là ĐÚNG textbook, id phân biệt node:50/node:60); ⚠️ demo mặc định xóa 25 KHÔNG tồn tại → no-op; ⚠️ vars stale (x=50/current=50 suốt demo, trong khi explanation nói x=60 ở pha đệ quy) |
| 20 | tree.bst-search | ✅* đúng, nhưng default target 25 → chỉ demo miss |
| 21–24 | bst-preorder/inorder/postorder/levelorder | ✅ Thứ tự đúng 100% textbook: [50,30,20,40,70,60,80] / [20,30,40,50,60,70,80] / [20,40,30,60,80,70,50] / [50,30,70,20,40,60,80]; ⚠️ 3 traversal đầu dùng pseudocodeLine=7 ở bước cuối nhưng mã giả chỉ có 6 dòng → bước cuối không highlight dòng nào (vitest A1 bắt được; browser al=0 khớp) |
| 25 | tree.avl-insert | ✅ Xoay 4 case đúng (LL/RR/RL/no-rot qua vitest); xoay hiển thị rõ: "|balance|=|2| > 1 → cần xoay / Trường hợp LL: xoay phải quanh nút 50 / Sau xoay LL nút 30 cân bằng: h=3, bf=0"; ⚠️ demo mặc định (chèn 25) không kích hoạt xoay; ⚠️ chip balance=2 kẹt ở bước cuối |

### 2.5 Heap (3) — tất cả ✅

| # | Key | Kiểm chứng |
|---|-----|-----------|
| 26 | heap.insert | chèn 15 vào [10,7,9,4,6,8] → [15,7,10,4,6,8,9] max-heap OK, cmp=2 swap=2 |
| 27 | heap.extract | max=10; [9,7,8,4,6] max-heap OK |
| 28 | heap.heapify | giữ nguyên [10,7,9,4,6,8] (đã là max-heap) — ✅* demo mặc định "không có gì thay đổi" (input mặc định đã là heap) |

### 2.6 Hash (3) — tất cả ✅

| # | Key | Kiểm chứng |
|---|-----|-----------|
| 29 | hash.insert | bucket = key % 11 đúng (vitest); chèn vào HEAD chuỗi |
| 30 | hash.search | ✅ CÓ field riêng inp-targetValue (mặc định 41): tìm 1 khóa, idx=41%11=8 đúng; nhánh miss tồn tại khi target không có trong keys |
| 31 | hash.delete | ✅ xóa 41, nối chuỗi đúng |

### 2.7 Graph (3) — tất cả ✅

| # | Key | Kiểm chứng |
|---|-----|-----------|
| 32 | graph.bfs | thứ tự [0,1,5,2,6,3,4] trên đồ thị 7 đỉnh 9 cạnh, parent tree nhất quán; xử lý disconnected (audit C7 có sẵn) |
| 33 | graph.dfs | [0..6], stack-based tương đương đệ quy tăng dần |
| 34 | graph.dijkstra | d=[0,9,10,16,25,1,2] KHỚP tham chiếu tính tay trên đúng edge list có trọng số (10 cạnh, có hướng); metadata O(V²) THÀNH THẬT khớp extract-min quét tuyến tính |

### 2.8 Structure (10)

| # | Key | Kết quả |
|---|-----|---------|
| 35 | structure.array | ✅* cốt lõi đúng; explanation "xóa đầu dịch n-1 phần tử" đếm lệch 1 (5 phần tử dịch, ghi 4) |
| 36 | structure.linkedlist | ✅ |
| 37 | structure.stack | ✅ |
| 38 | structure.queue | ✅ pointer model (cell muted, không dịch chuyển) khớp "front++ → O(1)" |
| 39 | structure.binarytree | ✅ (stats comparisons=7 mang tính minh họa) |
| 40 | structure.bst | ✅ |
| 41 | structure.avl | ✅ (audit C2: [1,2,3] → root 2, height/bf đúng) |
| 42 | structure.heap | ✅ input tùy chỉnh KHÔNG phải heap → "đã tự động heapify" minh bạch (sai dự đoán ban đầu của map v1) |
| 43 | structure.hashtable | ⚠️ TITLE sai thuật ngữ: "địa chỉ mở: chuỗi nối kết" — open addressing ≠ separate chaining; implement là chaining |
| 44 | structure.graph | ✅* C1 audit: cycle 4 đỉnh → 4 cạnh đúng; explanation cứng "cạnh n-1" chỉ đúng cho preset path; text kết nói "Dijkstra O((V+E)logV)" trong khi sim dijkstra ghi O(V²) — không nhất quán |

## 3. Complexity metadata audit — đã đối chiếu chip trên UI (title đủ best/avg/worst/space)

- 40/44 keys: khớp lý thuyết chuẩn (chi tiết bảng trong REPORT 5.4).
- Ngoại lệ ghi nhận: heap.insert best=O(log n) (chặt hơn là O(1) — conservative, không dạy sai); structure.binarytree best=O(log n) (best của search trên cây thường là O(1)); graph.dijkstra O(V²) khớp implementation (honest) — nhưng thiếu chữ "phiên bản PQ là O((V+E)logV)".
- structure.hashtable: sai TERMINOLOGY trong title (không phải complexity).

## 4. Các nghi vấn đã ĐÓNG so với map v1

1. bst-delete duplicate id → KHÔNG phải bug (mid-state 2 nút "60" chủ ý, id distinct) — vitest A9.
2. hash.search "không có target" → SAI: có inp-targetValue riêng.
3. dijkstra "metadata khai PQ mà code quét tuyến tính" → SAI: metadata đã ghi O(V²) thành thật.
4. queue.dequeue "shift toàn mảng" → SAI: pointer model đúng ở cả linear.queue và structure.queue.
5. structure.heap "không validate, nói sai a[0] max" → SAI: auto-heapify + minh bạch trong explanation.
6. binary search "auto-sort im lặng" → SAI: có bước 2 thông báo + canvas chuyển mảng sorted.
