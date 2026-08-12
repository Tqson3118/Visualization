/**
 * data/nodeHubData.ts — map cục bộ topic×node → lesson id (fallback khi backend chưa gắn)
 * Lesson id thật do backend seed cấp; bản đồ này giúp Node Hub hiển thị lý thuyết
 * mẫu ngay cả khi backend chưa khả dụng (ghi chú: không phải dữ liệu chính thức).
 */

export const TOPIC_NODE_LESSONS: Record<number, Record<number, number>> = {
  // Topic 1 — Sắp xếp & Tìm kiếm: node 1..8 → lesson 1..8 (seed)
  1: { 1: 1, 2: 2, 3: 3, 4: 4, 5: 5, 6: 6, 7: 7, 8: 8 },
  // Topic 2 — CTDL tuyến tính
  2: { 1: 9, 2: 10, 3: 11, 4: 12, 5: 13, 6: 14, 7: 15, 8: 16 },
  // Topic 3 — Cây
  3: { 1: 17, 2: 18, 3: 19, 4: 20, 5: 21, 6: 22, 7: 23 },
  // Topic 4 — Bảng băm
  4: { 1: 24, 2: 25, 3: 26, 4: 27 },
  // Topic 5 — Đồ thị
  5: { 1: 28, 2: 29, 3: 30, 4: 31 },
};
