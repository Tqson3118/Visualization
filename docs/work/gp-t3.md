# GP-T3 — Rà simulation key: lessons.ts ↔ shared/simulation-catalog.json

- **Nhánh:** `feature/lesson-sim-keys` (tạo từ `dev`)
- **Ngày:** 2026-08-13
- **Phạm vi:** `frontend/src/data/lessons.ts` (13 bài) vs `shared/simulation-catalog.json` (44 key cố định — KHÔNG thêm key mới)

## 1. Catalog 44 key (danh sách đầy đủ)

`sort.bubble, sort.selection, sort.insertion, sort.merge, sort.quick, sort.heap` ·
`search.linear, search.binary` · `stack.push, stack.pop, stack.peek` ·
`queue.enqueue, queue.dequeue` · `list.insert, list.delete, list.search, list.traverse` ·
`tree.bst-insert, tree.bst-delete, tree.bst-search, tree.bst-preorder, tree.bst-inorder, tree.bst-postorder, tree.bst-levelorder, tree.avl-insert` ·
`heap.insert, heap.extract, heap.heapify` · `hash.insert, hash.search, hash.delete` ·
`graph.bfs, graph.dfs, graph.dijkstra` ·
`structure.array, structure.linkedlist, structure.stack, structure.queue, structure.binarytree, structure.bst, structure.avl, structure.heap, structure.hashtable, structure.graph`

## 2. Bảng rà từng bài (13/13)

| Bài (id) | Key trong lessons.ts | Có trong catalog 44 key? | Xử lý |
|---|---|---|---|
| bubble-sort | `sort.bubble` | ✅ | giữ nguyên |
| selection-sort | `sort.selection` | ✅ | giữ nguyên |
| insertion-sort | `sort.insertion` | ✅ | giữ nguyên |
| quick-sort | `sort.quick` | ✅ | giữ nguyên |
| merge-sort | `sort.merge` | ✅ | giữ nguyên |
| heap-sort | `sort.heap`, `heap.heapify`, `heap.extract` | ✅ | giữ nguyên |
| linear-search | `search.linear` | ✅ | giữ nguyên |
| binary-search | `search.binary` | ✅ | giữ nguyên |
| **sliding-window** | `[]` | — | **giữ `[]` + cập nhật chú thích** (xem mục 3) |
| bst | `tree.bst-insert`, `tree.bst-search`, `tree.bst-delete`, `tree.bst-inorder` | ✅ | giữ nguyên |
| bfs | `graph.bfs`, `queue.enqueue`, `queue.dequeue` | ✅ | giữ nguyên |
| dfs | `graph.dfs`, `stack.push`, `stack.pop` | ✅ | giữ nguyên |
| dijkstra | `graph.dijkstra` | ✅ | giữ nguyên |

**Kết quả rà (script so khớp tự động):**
- Key dùng trong lessons.ts: **21 unique / 21** — **MISSING = []** (0 key lệch catalog).
- `seedData.spec.ts:70-84` đã assert quy ước này và **đang PASS** → không có bài nào key lệch catalog mà test chưa bắt (không cần sửa key nào).
- 23 key catalog chưa dùng (stack.peek, list.*, tree.bst-preorder/postorder/levelorder, tree.avl-insert, heap.insert, hash.*, structure.*...) — bình thường: catalog là toàn bộ chương trình, lessons.ts mới seed 13 bài.

## 3. Kết luận sliding-window

Catalog 44 key **KHÔNG có** mô phỏng sliding window:
- Không có nhóm `window.*` / `two-pointer` / `prefix-sum`.
- `search.linear` = quét tuần tự từng phần tử (không duy trì cửa sổ) — **không khớp ngữ nghĩa**.
- `structure.array` = mô tả CTDL mảng, không phải kỹ thuật.

→ Theo quy ước `seedData.spec.ts:82` (bài không có simulation phải đánh dấu TODO): **giữ `simulations: []`** + cập nhật chú thích:

```ts
// TODO: catalog 44 key không có mô phỏng sliding window — chờ mở rộng catalog
// (catalog hiện chỉ có sort.*/search.*/stack/queue/list/tree/heap/hash/graph/structure.*;
//  không có window.*/two-pointer/prefix-sum — search.linear là quét tuần tự, không khớp ngữ nghĩa)
simulations: [],
```

KHÔNG tự thêm key gần tương đương (vi phạm ngữ nghĩa) và KHÔNG thêm key mới vào catalog (44 key cố định).

## 4. Thay đổi

- `frontend/src/data/lessons.ts` — duy nhất bài `sliding-window`: cập nhật chú thích TODO sang chuẩn quy ước, giữ `simulations: []`. Không đổi key nào khác, không đổi catalog.

## 5. Verify

- [x] `npm run build` — **0 lỗi** (vue-tsc -b && vite build ✓)
- [x] `npm test` — **78/78 PASS** (gồm `seedData.spec.ts` 7/7 + `catalog.spec.ts` 10/10)
- [x] `npx playwright test` — **13/13 PASS** (26.8s)
- [x] Rà manual bằng script: mọi key trong lessons.ts ∈ catalog 44 key (MISSING = [])

## 6. Commit

- `git commit` (commit-as.ps1 son): hash xem `git log`.
