// data/referenceLinks.ts — Liên kết tài liệu tham khảo cho từng mô phỏng
// (Wikipedia EN + GeeksforGeeks — URL ổn định, article gốc).
//
// - KEY khớp 100% với engines/catalog.ts (44 key) — xem engines/__tests__/catalog.spec.ts
//   để biết quy ước {nhóm}.{tên}. Khi thêm mô phỏng mới vào catalog → bổ sung key tại đây.
// - Các thao tác cùng một cấu trúc (stack.push/pop/peek...) trỏ về chung một article
//   chuyên đề (mô tả toàn bộ thao tác của cấu trúc đó).
// - 'Đọc tài liệu' trên thẻ mô phỏng ưu tiên wikipedia, fallback geeksforgeeks.

/** Liên kết tài liệu của một mô phỏng — cả 2 field đều tùy chọn. */
export interface AlgorithmReference {
  wikipedia?: string;
  geeksforgeeks?: string;
}

/** Bản đồ key mô phỏng → tài liệu tham khảo (nguồn duy nhất cho SimulationsView). */
export const REFERENCE_LINKS: Record<string, AlgorithmReference> = {
  // ── Sắp xếp ──
  'sort.bubble': {
    wikipedia: 'https://en.wikipedia.org/wiki/Bubble_sort',
    geeksforgeeks: 'https://www.geeksforgeeks.org/bubble-sort-algorithm/',
  },
  'sort.selection': {
    wikipedia: 'https://en.wikipedia.org/wiki/Selection_sort',
    geeksforgeeks: 'https://www.geeksforgeeks.org/selection-sort-algorithm-2/',
  },
  'sort.insertion': {
    wikipedia: 'https://en.wikipedia.org/wiki/Insertion_sort',
    geeksforgeeks: 'https://www.geeksforgeeks.org/insertion-sort-algorithm/',
  },
  'sort.merge': {
    wikipedia: 'https://en.wikipedia.org/wiki/Merge_sort',
    geeksforgeeks: 'https://www.geeksforgeeks.org/merge-sort/',
  },
  'sort.quick': {
    wikipedia: 'https://en.wikipedia.org/wiki/Quicksort',
    geeksforgeeks: 'https://www.geeksforgeeks.org/quick-sort-algorithm/',
  },
  'sort.heap': {
    wikipedia: 'https://en.wikipedia.org/wiki/Heapsort',
    geeksforgeeks: 'https://www.geeksforgeeks.org/heap-sort/',
  },

  // ── Tìm kiếm ──
  'search.linear': {
    wikipedia: 'https://en.wikipedia.org/wiki/Linear_search',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linear-search/',
  },
  'search.binary': {
    wikipedia: 'https://en.wikipedia.org/wiki/Binary_search_algorithm',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-search/',
  },

  // ── Ngăn xếp ──
  'stack.push': {
    wikipedia: 'https://en.wikipedia.org/wiki/Stack_(abstract_data_type)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/stack-data-structure/',
  },
  'stack.pop': {
    wikipedia: 'https://en.wikipedia.org/wiki/Stack_(abstract_data_type)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/stack-data-structure/',
  },
  'stack.peek': {
    wikipedia: 'https://en.wikipedia.org/wiki/Stack_(abstract_data_type)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/stack-data-structure/',
  },

  // ── Hàng đợi ──
  'queue.enqueue': {
    wikipedia: 'https://en.wikipedia.org/wiki/Queue_(abstract_data_type)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/queue-data-structure/',
  },
  'queue.dequeue': {
    wikipedia: 'https://en.wikipedia.org/wiki/Queue_(abstract_data_type)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/queue-data-structure/',
  },

  // ── Danh sách liên kết ──
  'list.insert': {
    wikipedia: 'https://en.wikipedia.org/wiki/Linked_list',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linked-list-data-structure/',
  },
  'list.delete': {
    wikipedia: 'https://en.wikipedia.org/wiki/Linked_list',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linked-list-data-structure/',
  },
  'list.search': {
    wikipedia: 'https://en.wikipedia.org/wiki/Linked_list',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linked-list-data-structure/',
  },
  'list.traverse': {
    wikipedia: 'https://en.wikipedia.org/wiki/Linked_list',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linked-list-data-structure/',
  },

  // ── Cây nhị phân tìm kiếm (BST) ──
  'tree.bst-insert': {
    wikipedia: 'https://en.wikipedia.org/wiki/Binary_search_tree',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-search-tree-data-structure/',
  },
  'tree.bst-delete': {
    wikipedia: 'https://en.wikipedia.org/wiki/Binary_search_tree',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-search-tree-data-structure/',
  },
  'tree.bst-search': {
    wikipedia: 'https://en.wikipedia.org/wiki/Binary_search_tree',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-search-tree-data-structure/',
  },
  'tree.bst-preorder': {
    wikipedia: 'https://en.wikipedia.org/wiki/Tree_traversal',
    geeksforgeeks: 'https://www.geeksforgeeks.org/tree-traversals-inorder-preorder-and-postorder/',
  },
  'tree.bst-inorder': {
    wikipedia: 'https://en.wikipedia.org/wiki/Tree_traversal',
    geeksforgeeks: 'https://www.geeksforgeeks.org/tree-traversals-inorder-preorder-and-postorder/',
  },
  'tree.bst-postorder': {
    wikipedia: 'https://en.wikipedia.org/wiki/Tree_traversal',
    geeksforgeeks: 'https://www.geeksforgeeks.org/tree-traversals-inorder-preorder-and-postorder/',
  },
  'tree.bst-levelorder': {
    wikipedia: 'https://en.wikipedia.org/wiki/Tree_traversal',
    geeksforgeeks: 'https://www.geeksforgeeks.org/level-order-tree-traversal/',
  },

  // ── Cây AVL ──
  'tree.avl-insert': {
    wikipedia: 'https://en.wikipedia.org/wiki/AVL_tree',
    geeksforgeeks: 'https://www.geeksforgeeks.org/avl-tree-set-1-insertion/',
  },

  // ── Đống nhị phân ──
  'heap.insert': {
    wikipedia: 'https://en.wikipedia.org/wiki/Heap_(data_structure)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-heap/',
  },
  'heap.extract': {
    wikipedia: 'https://en.wikipedia.org/wiki/Heap_(data_structure)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-heap/',
  },
  'heap.heapify': {
    wikipedia: 'https://en.wikipedia.org/wiki/Heap_(data_structure)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/building-heap-from-array/',
  },

  // ── Bảng băm ──
  'hash.insert': {
    wikipedia: 'https://en.wikipedia.org/wiki/Hash_table',
    geeksforgeeks: 'https://www.geeksforgeeks.org/hashing-data-structure/',
  },
  'hash.search': {
    wikipedia: 'https://en.wikipedia.org/wiki/Hash_table',
    geeksforgeeks: 'https://www.geeksforgeeks.org/hashing-data-structure/',
  },
  'hash.delete': {
    wikipedia: 'https://en.wikipedia.org/wiki/Hash_table',
    geeksforgeeks: 'https://www.geeksforgeeks.org/hashing-data-structure/',
  },

  // ── Đồ thị ──
  'graph.bfs': {
    wikipedia: 'https://en.wikipedia.org/wiki/Breadth-first_search',
    geeksforgeeks: 'https://www.geeksforgeeks.org/breadth-first-search-or-bfs-for-a-graph/',
  },
  'graph.dfs': {
    wikipedia: 'https://en.wikipedia.org/wiki/Depth-first_search',
    geeksforgeeks: 'https://www.geeksforgeeks.org/depth-first-search-or-dfs-for-a-graph/',
  },
  'graph.dijkstra': {
    wikipedia: 'https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm',
    geeksforgeeks: 'https://www.geeksforgeeks.org/dijkstras-shortest-path-algorithm-greedy-algo-7/',
  },

  // ── Cấu trúc dữ liệu ──
  'structure.array': {
    wikipedia: 'https://en.wikipedia.org/wiki/Array_(data_structure)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/array-data-structure/',
  },
  'structure.linkedlist': {
    wikipedia: 'https://en.wikipedia.org/wiki/Linked_list',
    geeksforgeeks: 'https://www.geeksforgeeks.org/linked-list-data-structure/',
  },
  'structure.stack': {
    wikipedia: 'https://en.wikipedia.org/wiki/Stack_(abstract_data_type)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/stack-data-structure/',
  },
  'structure.queue': {
    wikipedia: 'https://en.wikipedia.org/wiki/Queue_(abstract_data_type)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/queue-data-structure/',
  },
  'structure.binarytree': {
    wikipedia: 'https://en.wikipedia.org/wiki/Binary_tree',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-tree-data-structure/',
  },
  'structure.bst': {
    wikipedia: 'https://en.wikipedia.org/wiki/Binary_search_tree',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-search-tree-data-structure/',
  },
  'structure.avl': {
    wikipedia: 'https://en.wikipedia.org/wiki/AVL_tree',
    geeksforgeeks: 'https://www.geeksforgeeks.org/introduction-to-avl-tree/',
  },
  'structure.heap': {
    wikipedia: 'https://en.wikipedia.org/wiki/Heap_(data_structure)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/binary-heap/',
  },
  'structure.hashtable': {
    wikipedia: 'https://en.wikipedia.org/wiki/Hash_table',
    geeksforgeeks: 'https://www.geeksforgeeks.org/hashing-data-structure/',
  },
  'structure.graph': {
    wikipedia: 'https://en.wikipedia.org/wiki/Graph_(abstract_data_type)',
    geeksforgeeks: 'https://www.geeksforgeeks.org/graph-data-structure-and-algorithms/',
  },
};

/** Tra cứu tài liệu theo key mô phỏng (undefined khi không có — UI sẽ ẩn link). */
export function getReference(key: string): AlgorithmReference | undefined {
  return REFERENCE_LINKS[key];
}
