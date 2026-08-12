/**
 * SEED COURSES — nguồn: source/VisualizationDSA/frontend/src/data/courses.ts (V0)
 *
 * Lựa chọn:
 * - Bê 4 course DSA (sort/search/tree) khớp 5 chủ đề SDD §7.5.
 * - KHÔNG bê oop-101 / solid-101 (V0/V1): ngoài phạm vi curriculum DSA của SDD §7.5
 *   (5 chủ đề: Sắp xếp & Tìm kiếm, CTDL tuyến tính, Cây, Bảng băm, Đồ thị).
 * - `topicId` map sang Topics seed backend theo SDD §7.5 (xem backend Seed/README.md).
 */

/** Bài học tham chiếu trong course (id trỏ tới LESSONS trong lessons.ts). */
export interface SeedCourseLessonRef {
  id: string;
  title: string;
  order: number;
}

/** Course seed — kiểu TS đơn giản, strict, không `any`. */
export interface SeedCourse {
  id: string;
  /** Topics.Id theo SDD §7.5: 1=Sắp xếp & Tìm kiếm, 2=CTDL tuyến tính, 3=Cây, 4=Bảng băm, 5=Đồ thị. */
  topicId: number;
  title: string;
  description: string;
  category: string;
  difficulty: 'Easy' | 'Medium' | 'Hard';
  xpReward: number;
  totalLessons: number;
  lessons: SeedCourseLessonRef[];
  isPublished: boolean;
  isPremium: boolean;
  coverImage?: string;
}

export const SEED_COURSES: SeedCourse[] = [
  {
    id: 'sorting-101',
    topicId: 1, // Sắp xếp & Tìm kiếm
    title: 'Thuật toán Sắp xếp Cơ bản',
    description: 'Làm chủ các thuật toán sắp xếp cơ bản: Bubble, Selection, Insertion Sort.',
    category: 'Sorting',
    difficulty: 'Easy',
    xpReward: 300,
    totalLessons: 3,
    lessons: [
      { id: 'bubble-sort', title: 'Bubble Sort - Sắp xếp nổi bọt', order: 1 },
      { id: 'selection-sort', title: 'Selection Sort - Sắp xếp chọn', order: 2 },
      { id: 'insertion-sort', title: 'Insertion Sort - Sắp xếp chèn', order: 3 },
    ],
    isPublished: true,
    isPremium: false,
    coverImage: 'https://images.unsplash.com/photo-1618401471353-b98aedd07871?w=300&q=80',
  },
  {
    id: 'sorting-advanced',
    topicId: 1, // Sắp xếp & Tìm kiếm
    title: 'Sắp xếp Nâng cao',
    description: 'Tối ưu hóa hiệu suất với Quick, Merge, Heap Sort.',
    category: 'Sorting',
    difficulty: 'Hard',
    xpReward: 500,
    totalLessons: 3,
    lessons: [
      { id: 'quick-sort', title: 'Quick Sort - Sắp xếp nhanh', order: 1 },
      { id: 'merge-sort', title: 'Merge Sort - Sắp xếp trộn', order: 2 },
      { id: 'heap-sort', title: 'Heap Sort - Sắp xếp đống', order: 3 },
    ],
    isPublished: true,
    isPremium: false,
  },
  {
    id: 'searching-101',
    topicId: 1, // Sắp xếp & Tìm kiếm
    title: 'Tìm kiếm và Cửa sổ trượt',
    description: 'Tìm kiếm tuần tự, nhị phân và kỹ thuật cửa sổ trượt.',
    category: 'Searching',
    difficulty: 'Medium',
    xpReward: 400,
    totalLessons: 3,
    lessons: [
      { id: 'linear-search', title: 'Linear Search - Tìm kiếm tuần tự', order: 1 },
      { id: 'binary-search', title: 'Binary Search - Tìm kiếm nhị phân', order: 2 },
      { id: 'sliding-window', title: 'Sliding Window - Cửa sổ trượt', order: 3 },
    ],
    isPublished: true,
    isPremium: false,
  },
  {
    id: 'tree-101',
    topicId: 3, // Cây
    title: 'Cây và Đồ thị Cơ bản',
    description: 'Cấu trúc cây, duyệt BFS, DFS, và tìm đường đi ngắn nhất Dijkstra.',
    category: 'Tree/Graph',
    difficulty: 'Medium',
    xpReward: 450,
    totalLessons: 4,
    lessons: [
      { id: 'bst', title: 'Binary Search Tree - Cây tìm kiếm nhị phân', order: 1 },
      { id: 'bfs', title: 'BFS - Duyệt theo chiều rộng', order: 2 },
      { id: 'dfs', title: 'DFS - Duyệt theo chiều sâu', order: 3 },
      { id: 'dijkstra', title: 'Dijkstra - Đường đi ngắn nhất', order: 4 },
    ],
    isPublished: true,
    isPremium: false,
  },
];
