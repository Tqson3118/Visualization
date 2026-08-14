// data/referenceLinks.ts — Liên kết tài liệu tham khảo cho từng mô phỏng
// (Wikipedia VI + GeeksforGeeks — URL ổn định, article gốc).
//
// - KEY khớp 100% với engines/catalog.ts (44 key) — xem engines/__tests__/catalog.spec.ts
//   để biết quy ước {nhóm}.{tên}. Khi thêm mô phỏng mới vào catalog → bổ sung key tại đây.
// - Các thao tác cùng một cấu trúc (stack.push/pop/peek...) trỏ về chung một article
//   chuyên đề (mô tả toàn bộ thao tác của cấu trúc đó).
// - 'Đọc tài liệu' trên thẻ mô phỏng ưu tiên wikipedia, fallback geeksforgeeks.

/** Liên kết tài liệu của một mô phỏng — cả 3 field đều tùy chọn. */
export interface AlgorithmReference {
  wikipedia?: string;
  geeksforgeeks?: string;
  note?: string;
}

/** Bản đồ key mô phỏng → tài liệu tham khảo (nguồn duy nhất cho SimulationsView). */
export const REFERENCE_LINKS: Record<string, AlgorithmReference> = {
  // ── Sắp xếp ──
  'sort.bubble': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Sắp_xếp_nổi_bọt',
    geeksforgeeks: 'https://www.geeksforgeeks.org/bubble-sort-algorithm/',
  },
  'sort.selection': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Sắp_xếp_chọn',
    geeksforgeeks: 'https://www.geeksforgeeks.org/selection-sort-algorithm-2/',
  },
  'sort.insertion': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Sắp_xếp_chèn',
    geeksforgeeks: 'https://www.geeksforgeeks.org/insertion-sort-algorithm/',
  },
  'sort.merge': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Sắp_xếp_trộn',
    geeksforgeeks: 'https://www.geeksforgeeks.org/merge-sort/',
  },
  'sort.quick': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Sắp_xếp_nhanh',
    geeksforgeeks: 'https://www.geeksforgeeks.org/quick-sort-algorithm/',
    note: 'Mô phỏng dùng biến thể Lomuto — tài liệu có thêm phân hoạch Hoare.',
  },
  'sort.heap': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Sắp_xếp_vun_đống',
    geeksforgeeks: 'https://www.geeksforgeeks.org/heap-sort/',
  },

  // ── Tìm kiếm ──
  'search.linear': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Tìm_kiếm_tuần_tự',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linear-search/',
  },
  'search.binary': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Tìm_kiếm_nhị_phân',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-search/',
    note: 'Yêu cầu mảng đã sắp xếp — mô phỏng tự sắp xếp nếu dữ liệu chưa sắp xếp.',
  },

  // ── Ngăn xếp ──
  'stack.push': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Ngăn_xếp',
    geeksforgeeks: 'https://www.geeksforgeeks.org/stack-data-structure/',
  },
  'stack.pop': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Ngăn_xếp',
    geeksforgeeks: 'https://www.geeksforgeeks.org/stack-data-structure/',
  },
  'stack.peek': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Ngăn_xếp',
    geeksforgeeks: 'https://www.geeksforgeeks.org/stack-data-structure/',
  },

  // ── Hàng đợi ──
  'queue.enqueue': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Hàng_đợi',
    geeksforgeeks: 'https://www.geeksforgeeks.org/queue-data-structure/',
  },
  'queue.dequeue': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Hàng_đợi',
    geeksforgeeks: 'https://www.geeksforgeeks.org/queue-data-structure/',
  },

  // ── Danh sách liên kết ──
  'list.insert': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Danh_sách_liên_kết',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linked-list-data-structure/',
  },
  'list.delete': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Danh_sách_liên_kết',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linked-list-data-structure/',
  },
  'list.search': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Danh_sách_liên_kết',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linked-list-data-structure/',
  },
  'list.traverse': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Danh_sách_liên_kết',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linked-list-data-structure/',
  },

  // ── Cây nhị phân tìm kiếm (BST) ──
  'tree.bst-insert': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Cây_tìm_kiếm_nhị_phân',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-search-tree-data-structure/',
  },
  'tree.bst-delete': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Cây_tìm_kiếm_nhị_phân',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-search-tree-data-structure/',
  },
  'tree.bst-search': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Cây_tìm_kiếm_nhị_phân',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-search-tree-data-structure/',
  },
  'tree.bst-preorder': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Duyệt_cây',
    geeksforgeeks: 'https://www.geeksforgeeks.org/tree-traversals-inorder-preorder-and-postorder/',
  },
  'tree.bst-inorder': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Duyệt_cây',
    geeksforgeeks: 'https://www.geeksforgeeks.org/tree-traversals-inorder-preorder-and-postorder/',
  },
  'tree.bst-postorder': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Duyệt_cây',
    geeksforgeeks: 'https://www.geeksforgeeks.org/tree-traversals-inorder-preorder-and-postorder/',
  },
  'tree.bst-levelorder': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Duyệt_cây',
    geeksforgeeks: 'https://www.geeksforgeeks.org/level-order-tree-traversal/',
    note: 'Duyệt theo tầng — dùng hàng đợi (BFS trên cây).',
  },

  // ── Cây AVL ──
  'tree.avl-insert': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Cây_AVL',
    geeksforgeeks: 'https://www.geeksforgeeks.org/avl-tree-set-1-insertion/',
  },

  // ── Đống nhị phân ──
  'heap.insert': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Đống_(cấu_trúc_dữ_liệu)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-heap/',
  },
  'heap.extract': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Đống_(cấu_trúc_dữ_liệu)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-heap/',
    note: 'Trích xuất max — sift down trên max-heap.',
  },
  'heap.heapify': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Đống_(cấu_trúc_dữ_liệu)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/building-heap-from-array/',
  },

  // ── Bảng băm ──
  'hash.insert': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Bảng_băm',
    geeksforgeeks: 'https://www.geeksforgeeks.org/hashing-data-structure/',
  },
  'hash.search': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Bảng_băm',
    geeksforgeeks: 'https://www.geeksforgeeks.org/hashing-data-structure/',
  },
  'hash.delete': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Bảng_băm',
    geeksforgeeks: 'https://www.geeksforgeeks.org/hashing-data-structure/',
  },

  // ── Đồ thị ──
  'graph.bfs': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Tìm_kiếm_theo_chiều_rộng',
    geeksforgeeks: 'https://www.geeksforgeeks.org/breadth-first-search-or-bfs-for-a-graph/',
  },
  'graph.dfs': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Tìm_kiếm_theo_chiều_sâu',
    geeksforgeeks: 'https://www.geeksforgeeks.org/depth-first-search-or-dfs-for-a-graph/',
  },
  'graph.dijkstra': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Thuật_toán_Dijkstra',
    geeksforgeeks: 'https://www.geeksforgeeks.org/dijkstras-shortest-path-algorithm-greedy-algo-7/',
    note: 'Yêu cầu trọng số cạnh không âm.',
  },

  // ── Cấu trúc dữ liệu ──
  'structure.array': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Mảng_(cấu_trúc_dữ_liệu)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/array-data-structure/',
  },
  'structure.linkedlist': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Danh_sách_liên_kết',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linked-list-data-structure/',
  },
  'structure.stack': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Ngăn_xếp',
    geeksforgeeks: 'https://www.geeksforgeeks.org/stack-data-structure/',
  },
  'structure.queue': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Hàng_đợi',
    geeksforgeeks: 'https://www.geeksforgeeks.org/queue-data-structure/',
  },
  'structure.binarytree': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Cây_nhị_phân',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-tree-data-structure/',
  },
  'structure.bst': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Cây_tìm_kiếm_nhị_phân',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-search-tree-data-structure/',
  },
  'structure.avl': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Cây_AVL',
    geeksforgeeks: 'https://www.geeksforgeeks.org/introduction-to-avl-tree/',
  },
  'structure.heap': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Đống_(cấu_trúc_dữ_liệu)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-heap/',
  },
  'structure.hashtable': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Bảng_băm',
    geeksforgeeks: 'https://www.geeksforgeeks.org/hashing-data-structure/',
    note: 'Mô phỏng dùng chuỗi nối kết (separate chaining).',
  },
  'structure.graph': {
    wikipedia: 'https://vi.wikipedia.org/wiki/Đồ_thị_(lý_thuyết_đồ_thị)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/graph-data-structure-and-algorithms/',
  },
};

/** Tra cứu tài liệu theo key mô phỏng (undefined khi không có — UI sẽ ẩn link). */
export function getReference(key: string): AlgorithmReference | undefined {
  return REFERENCE_LINKS[key];
}
