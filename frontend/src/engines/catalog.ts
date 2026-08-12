// engines/catalog.ts — Danh mục mô phỏng DUY NHẤT (SDD §4.5, §9.9)
//
// - Danh sách KHỚP 100% key với shared/simulation-catalog.json (44 mô phỏng).
//   CI so sánh 2 danh sách key → khác → fail build (test: engines/__tests__/catalog.spec.ts).
// - Generator hiện tại là STUB: generate() ném "implement in task tiếp" — đủ type
//   SimulationGenerator để build; generator thật triển khai ở task sau của dự án.
// - Mọi khai báo mô phỏng tập trung tại file này (KHÔNG đăng ký rải rác nơi khác).

import type { SimulationGenerator } from './core/types';
import { registerSimulation } from './registry';

/** Metadata một mô phỏng — khớp từng field với shared/simulation-catalog.json. */
export interface CatalogMeta {
  key: string;
  title: string;
  dataStructure: string;
  category: 'algorithm' | 'structure';
  level: 'basic' | 'advanced';
  complexity: { best: string; average: string; worst: string; space: string };
  tags: string[];
  demoAllowed: boolean;
}

// ── Danh mục (nguồn dữ liệu: shared/simulation-catalog.json — đọc CHỈ ĐỌC) ──
export const CATALOG: CatalogMeta[] = [
  { key: 'sort.bubble', title: 'Sắp xếp nổi bọt (Bubble Sort)', dataStructure: 'Mảng', category: 'algorithm', level: 'basic', complexity: { best: 'O(n)', average: 'O(n²)', worst: 'O(n²)', space: 'O(1)' }, tags: ['sắp xếp', 'so sánh'], demoAllowed: true },
  { key: 'sort.selection', title: 'Sắp xếp chọn (Selection Sort)', dataStructure: 'Mảng', category: 'algorithm', level: 'basic', complexity: { best: 'O(n²)', average: 'O(n²)', worst: 'O(n²)', space: 'O(1)' }, tags: ['sắp xếp', 'chọn'], demoAllowed: false },
  { key: 'sort.insertion', title: 'Sắp xếp chèn (Insertion Sort)', dataStructure: 'Mảng', category: 'algorithm', level: 'basic', complexity: { best: 'O(n)', average: 'O(n²)', worst: 'O(n²)', space: 'O(1)' }, tags: ['sắp xếp', 'chèn'], demoAllowed: false },
  { key: 'sort.merge', title: 'Sắp xếp trộn (Merge Sort)', dataStructure: 'Mảng', category: 'algorithm', level: 'advanced', complexity: { best: 'O(n log n)', average: 'O(n log n)', worst: 'O(n log n)', space: 'O(n)' }, tags: ['sắp xếp', 'chia để trị', 'đệ quy'], demoAllowed: false },
  { key: 'sort.quick', title: 'Sắp xếp nhanh (Quick Sort — Lomuto)', dataStructure: 'Mảng', category: 'algorithm', level: 'advanced', complexity: { best: 'O(n log n)', average: 'O(n log n)', worst: 'O(n²)', space: 'O(log n)' }, tags: ['sắp xếp', 'chia để trị', 'pivot'], demoAllowed: false },
  { key: 'sort.heap', title: 'Sắp xếp vun đống (Heap Sort)', dataStructure: 'Đống nhị phân', category: 'algorithm', level: 'advanced', complexity: { best: 'O(n log n)', average: 'O(n log n)', worst: 'O(n log n)', space: 'O(1)' }, tags: ['sắp xếp', 'heap'], demoAllowed: false },
  { key: 'search.linear', title: 'Tìm kiếm tuyến tính (Linear Search)', dataStructure: 'Mảng', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(n)', worst: 'O(n)', space: 'O(1)' }, tags: ['tìm kiếm'], demoAllowed: false },
  { key: 'search.binary', title: 'Tìm kiếm nhị phân (Binary Search)', dataStructure: 'Mảng', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(log n)', worst: 'O(log n)', space: 'O(1)' }, tags: ['tìm kiếm', 'chia đôi'], demoAllowed: true },
  { key: 'stack.push', title: 'Ngăn xếp — Push', dataStructure: 'Ngăn xếp', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(1)', space: 'O(1)' }, tags: ['stack', 'LIFO'], demoAllowed: false },
  { key: 'stack.pop', title: 'Ngăn xếp — Pop', dataStructure: 'Ngăn xếp', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(1)', space: 'O(1)' }, tags: ['stack', 'LIFO'], demoAllowed: false },
  { key: 'stack.peek', title: 'Ngăn xếp — Peek', dataStructure: 'Ngăn xếp', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(1)', space: 'O(1)' }, tags: ['stack', 'LIFO'], demoAllowed: false },
  { key: 'queue.enqueue', title: 'Hàng đợi — Enqueue', dataStructure: 'Hàng đợi', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(1)', space: 'O(1)' }, tags: ['queue', 'FIFO'], demoAllowed: false },
  { key: 'queue.dequeue', title: 'Hàng đợi — Dequeue', dataStructure: 'Hàng đợi', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(1)', space: 'O(1)' }, tags: ['queue', 'FIFO'], demoAllowed: false },
  { key: 'list.insert', title: 'Danh sách liên kết — Chèn', dataStructure: 'Danh sách liên kết đơn', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(n)', worst: 'O(n)', space: 'O(1)' }, tags: ['list', 'con trỏ'], demoAllowed: false },
  { key: 'list.delete', title: 'Danh sách liên kết — Xóa', dataStructure: 'Danh sách liên kết đơn', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(n)', worst: 'O(n)', space: 'O(1)' }, tags: ['list', 'con trỏ'], demoAllowed: false },
  { key: 'list.search', title: 'Danh sách liên kết — Tìm kiếm', dataStructure: 'Danh sách liên kết đơn', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(n)', worst: 'O(n)', space: 'O(1)' }, tags: ['list', 'con trỏ'], demoAllowed: false },
  { key: 'list.traverse', title: 'Danh sách liên kết — Duyệt', dataStructure: 'Danh sách liên kết đơn', category: 'algorithm', level: 'basic', complexity: { best: 'O(n)', average: 'O(n)', worst: 'O(n)', space: 'O(1)' }, tags: ['list', 'con trỏ'], demoAllowed: false },
  { key: 'tree.bst-insert', title: 'BST — Chèn', dataStructure: 'Cây nhị phân tìm kiếm', category: 'algorithm', level: 'basic', complexity: { best: 'O(log n)', average: 'O(log n)', worst: 'O(n)', space: 'O(log n)' }, tags: ['cây', 'BST'], demoAllowed: false },
  { key: 'tree.bst-delete', title: 'BST — Xóa', dataStructure: 'Cây nhị phân tìm kiếm', category: 'algorithm', level: 'advanced', complexity: { best: 'O(log n)', average: 'O(log n)', worst: 'O(n)', space: 'O(log n)' }, tags: ['cây', 'BST'], demoAllowed: false },
  { key: 'tree.bst-search', title: 'BST — Tìm kiếm', dataStructure: 'Cây nhị phân tìm kiếm', category: 'algorithm', level: 'basic', complexity: { best: 'O(log n)', average: 'O(log n)', worst: 'O(n)', space: 'O(log n)' }, tags: ['cây', 'BST'], demoAllowed: false },
  { key: 'tree.bst-preorder', title: 'BST — Duyệt Preorder', dataStructure: 'Cây nhị phân tìm kiếm', category: 'algorithm', level: 'basic', complexity: { best: 'O(n)', average: 'O(n)', worst: 'O(n)', space: 'O(n)' }, tags: ['cây', 'duyệt'], demoAllowed: false },
  { key: 'tree.bst-inorder', title: 'BST — Duyệt Inorder', dataStructure: 'Cây nhị phân tìm kiếm', category: 'algorithm', level: 'basic', complexity: { best: 'O(n)', average: 'O(n)', worst: 'O(n)', space: 'O(n)' }, tags: ['cây', 'duyệt'], demoAllowed: false },
  { key: 'tree.bst-postorder', title: 'BST — Duyệt Postorder', dataStructure: 'Cây nhị phân tìm kiếm', category: 'algorithm', level: 'basic', complexity: { best: 'O(n)', average: 'O(n)', worst: 'O(n)', space: 'O(n)' }, tags: ['cây', 'duyệt'], demoAllowed: false },
  { key: 'tree.bst-levelorder', title: 'BST — Duyệt Level-order', dataStructure: 'Cây nhị phân tìm kiếm', category: 'algorithm', level: 'basic', complexity: { best: 'O(n)', average: 'O(n)', worst: 'O(n)', space: 'O(n)' }, tags: ['cây', 'duyệt', 'BFS'], demoAllowed: false },
  { key: 'tree.avl-insert', title: 'Cây AVL — Chèn kèm xoay (LL/RR/LR/RL)', dataStructure: 'Cây AVL', category: 'algorithm', level: 'advanced', complexity: { best: 'O(log n)', average: 'O(log n)', worst: 'O(log n)', space: 'O(log n)' }, tags: ['cây', 'cân bằng', 'xoay'], demoAllowed: false },
  { key: 'heap.insert', title: 'Đống nhị phân — Chèn (bubble up)', dataStructure: 'Đống nhị phân', category: 'algorithm', level: 'advanced', complexity: { best: 'O(log n)', average: 'O(log n)', worst: 'O(log n)', space: 'O(1)' }, tags: ['heap', 'bubble up'], demoAllowed: false },
  { key: 'heap.extract', title: 'Đống nhị phân — Trích xuất max (sift down)', dataStructure: 'Đống nhị phân', category: 'algorithm', level: 'advanced', complexity: { best: 'O(log n)', average: 'O(log n)', worst: 'O(log n)', space: 'O(1)' }, tags: ['heap', 'sift down'], demoAllowed: false },
  { key: 'heap.heapify', title: 'Đống nhị phân — Heapify', dataStructure: 'Đống nhị phân', category: 'algorithm', level: 'advanced', complexity: { best: 'O(n)', average: 'O(n)', worst: 'O(n)', space: 'O(1)' }, tags: ['heap', 'heapify'], demoAllowed: false },
  { key: 'hash.insert', title: 'Bảng băm — Chèn (chuỗi nối kết)', dataStructure: 'Bảng băm', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(n)', space: 'O(n)' }, tags: ['bảng băm', 'hàm băm'], demoAllowed: false },
  { key: 'hash.search', title: 'Bảng băm — Tìm kiếm', dataStructure: 'Bảng băm', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(n)', space: 'O(n)' }, tags: ['bảng băm', 'hàm băm'], demoAllowed: false },
  { key: 'hash.delete', title: 'Bảng băm — Xóa', dataStructure: 'Bảng băm', category: 'algorithm', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(n)', space: 'O(n)' }, tags: ['bảng băm', 'hàm băm'], demoAllowed: false },
  { key: 'graph.bfs', title: 'Đồ thị — Duyệt BFS', dataStructure: 'Đồ thị', category: 'algorithm', level: 'basic', complexity: { best: 'O(V+E)', average: 'O(V+E)', worst: 'O(V+E)', space: 'O(V)' }, tags: ['đồ thị', 'BFS', 'hàng đợi'], demoAllowed: true },
  { key: 'graph.dfs', title: 'Đồ thị — Duyệt DFS', dataStructure: 'Đồ thị', category: 'algorithm', level: 'basic', complexity: { best: 'O(V+E)', average: 'O(V+E)', worst: 'O(V+E)', space: 'O(V)' }, tags: ['đồ thị', 'DFS', 'ngăn xếp'], demoAllowed: false },
  { key: 'graph.dijkstra', title: 'Đồ thị — Dijkstra (đường đi ngắn nhất)', dataStructure: 'Đồ thị', category: 'algorithm', level: 'advanced', complexity: { best: 'O((V+E) log V)', average: 'O((V+E) log V)', worst: 'O((V+E) log V)', space: 'O(V)' }, tags: ['đồ thị', 'Dijkstra', 'hàng đợi ưu tiên'], demoAllowed: false },
  { key: 'structure.array', title: 'Mảng (Array)', dataStructure: 'Mảng', category: 'structure', level: 'basic', complexity: { best: 'O(1)', average: 'O(n)', worst: 'O(n)', space: 'O(n)' }, tags: ['CTDL', 'tuần tự'], demoAllowed: false },
  { key: 'structure.linkedlist', title: 'Danh sách liên kết đơn (Singly Linked List)', dataStructure: 'Danh sách liên kết đơn', category: 'structure', level: 'basic', complexity: { best: 'O(1)', average: 'O(n)', worst: 'O(n)', space: 'O(n)' }, tags: ['CTDL', 'con trỏ'], demoAllowed: false },
  { key: 'structure.stack', title: 'Ngăn xếp (Stack — LIFO)', dataStructure: 'Ngăn xếp', category: 'structure', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(1)', space: 'O(n)' }, tags: ['CTDL', 'LIFO'], demoAllowed: false },
  { key: 'structure.queue', title: 'Hàng đợi (Queue — FIFO)', dataStructure: 'Hàng đợi', category: 'structure', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(1)', space: 'O(n)' }, tags: ['CTDL', 'FIFO'], demoAllowed: false },
  { key: 'structure.binarytree', title: 'Cây nhị phân (Binary Tree)', dataStructure: 'Cây nhị phân', category: 'structure', level: 'basic', complexity: { best: 'O(log n)', average: 'O(n)', worst: 'O(n)', space: 'O(n)' }, tags: ['CTDL', 'cây'], demoAllowed: false },
  { key: 'structure.bst', title: 'Cây nhị phân tìm kiếm (BST)', dataStructure: 'Cây nhị phân tìm kiếm', category: 'structure', level: 'basic', complexity: { best: 'O(log n)', average: 'O(log n)', worst: 'O(n)', space: 'O(n)' }, tags: ['CTDL', 'cây', 'BST'], demoAllowed: false },
  { key: 'structure.avl', title: 'Cây AVL (cân bằng)', dataStructure: 'Cây AVL', category: 'structure', level: 'advanced', complexity: { best: 'O(log n)', average: 'O(log n)', worst: 'O(log n)', space: 'O(n)' }, tags: ['CTDL', 'cây', 'cân bằng'], demoAllowed: false },
  { key: 'structure.heap', title: 'Đống nhị phân (Binary Heap — max-heap)', dataStructure: 'Đống nhị phân', category: 'structure', level: 'advanced', complexity: { best: 'O(1)', average: 'O(log n)', worst: 'O(log n)', space: 'O(n)' }, tags: ['CTDL', 'heap'], demoAllowed: false },
  { key: 'structure.hashtable', title: 'Bảng băm (Hash Table — địa chỉ mở: chuỗi nối kết)', dataStructure: 'Bảng băm', category: 'structure', level: 'basic', complexity: { best: 'O(1)', average: 'O(1)', worst: 'O(n)', space: 'O(n)' }, tags: ['CTDL', 'bảng băm'], demoAllowed: false },
  { key: 'structure.graph', title: 'Đồ thị (Graph — có hướng/vô hướng, trọng số)', dataStructure: 'Đồ thị', category: 'structure', level: 'advanced', complexity: { best: 'O(V+E)', average: 'O(V+E)', worst: 'O(V+E)', space: 'O(V+E)' }, tags: ['CTDL', 'đồ thị'], demoAllowed: false },
];

const NOT_IMPLEMENTED = 'implement in task tiếp';

/** Tạo generator STUB từ metadata — đủ type SimulationGenerator (SDD §4.2) để build. */
export function stubSimulation(meta: CatalogMeta): SimulationGenerator {
  return {
    key: meta.key,
    title: meta.title,
    category: meta.category,
    dataStructure: meta.dataStructure,
    level: meta.level,
    complexity: meta.complexity,
    // TODO (task generator): inputSchema/pseudocode thật theo từng GT (SDD §4.2, §4.7).
    inputSchema: { kind: 'array', fields: [] },
    pseudocode: [],
    generate: () => {
      throw new Error(`${NOT_IMPLEMENTED}: ${meta.key}`);
    },
    validate: () => ({ ok: true, errors: [] }),
  };
}

// Đăng ký toàn bộ danh mục vào registry (SDD §4.5) — import catalog là đủ để kích hoạt.
for (const meta of CATALOG) {
  registerSimulation(meta.key, () => stubSimulation(meta));
}

/** Truy vấn metadata theo key (helper cho UI danh sách mô phỏng). */
export function getCatalogMeta(key: string): CatalogMeta | undefined {
  return CATALOG.find((meta) => meta.key === key);
}
