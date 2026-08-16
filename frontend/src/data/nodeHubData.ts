/**
 * data/nodeHubData.ts — map node id GLOBAL (backend learning-path, 1..18) → lesson id.
 * Node id trong route /path/:topicId/node/:nodeId là id GLOBAL (topic 2 bắt đầu từ 5, ...).
 * Chỉ node bài học (Học: ...) có lesson; node Luyện tập/Kiểm tra cuối không có lesson.
 * Lesson id thật do backend seed cấp (GET /lessons): 1=Bubble, 2=Binary Search,
 * 3=Stack, 4=Linked List, 5=BST, 6=AVL, 7=Hash Table, 8=BFS.
 */

export const TOPIC_NODE_LESSONS: Record<number, number> = {
  // Topic 1 — Sắp xếp & Tìm kiếm
  1: 1, // Học: Bubble Sort
  2: 2, // Học: Binary Search
  // Topic 2 — CTDL tuyến tính
  5: 3, // Học: Stack
  6: 4, // Học: Linked List
  // Topic 3 — Cây
  9: 5, // Học: BST
  10: 6, // Học: AVL
  // Topic 4 — Bảng băm
  13: 7, // Học: Hash Table
  // Topic 5 — Đồ thị
  16: 8, // Học: BFS
};
