/**
 * SEED LESSONS — curriculum DSA
 *
 * Nguồn:
 * - `quick-sort`: bê nguyên từ source/VisualizationDSA3/frontend/src/data/lessons.ts (V3)
 *   (bản đầy đủ nhất — có `entryFunction` + testCases dạng double-bracket;
 *    V1 thiếu `entryFunction` và testCases format cũ → không dùng).
 * - Các bài khác: stub — chỉ có id/title/description/simulations/sortOrder.
 *   Nội dung lý thuyết + quiz thật nằm ở source/VisualizationDSA3/plan/content-drafts/v2/
 *   (40 bài: content.md + quiz.json) — seed chi tiết ở task sau (xem backend Seed/README.md).
 *
 * `simulations`: mảng KEY trong `shared/simulation-catalog.json` — chỉ dùng key tồn tại.
 * Key chưa có trong catalog → giữ nguyên bản text + TODO bên dưới.
 */

/** Quiz trắc nghiệm (giải thích tiếng Việt). */
export interface SeedQuizQuestion {
  id: string;
  questionText: string;
  options: string[];
  correctIndex: number;
  explanation: string;
}

/** Test case code challenge: input/expectedOutput là chuỗi JSON của tham số/giá trị trả về. */
export interface SeedCodeTestCase {
  input: string;
  expectedOutput: string;
  isHidden?: boolean;
}

/** Task code challenge (lab/code challenge — SDD §7.5). */
export interface SeedCodelabTask {
  description: string;
  initialCode: string;
  entryFunction: string;
  solution: string;
  testCases: SeedCodeTestCase[];
}

/** Bài học seed — kiểu TS đơn giản, strict, không `any`. */
export interface SeedLesson {
  id: string;
  /** Course.id trong courses.ts (SEED_COURSES). */
  courseId: string;
  title: string;
  description: string;
  sortOrder: number;
  /** KEY trong shared/simulation-catalog.json (LessonSimulations — SDD §7.3.8). */
  simulations: string[];
  theoryContent?: string;
  quizQuestions?: SeedQuizQuestion[];
  codelabTask?: SeedCodelabTask;
}

export const LESSONS: Record<string, SeedLesson> = {
  'bubble-sort': {
    id: 'bubble-sort',
    courseId: 'sorting-101',
    title: 'Bubble Sort - Sắp xếp nổi bọt',
    description: 'Sắp xếp nổi bọt: so sánh và đổi chỗ các cặp liền kề cho tới khi mảng được sắp xếp.',
    sortOrder: 1,
    simulations: ['sort.bubble'],
  },
  'selection-sort': {
    id: 'selection-sort',
    courseId: 'sorting-101',
    title: 'Selection Sort - Sắp xếp chọn',
    description: 'Sắp xếp chọn: mỗi lượt tìm phần tử nhỏ nhất đưa về đầu mảng.',
    sortOrder: 2,
    simulations: ['sort.selection'],
  },
  'insertion-sort': {
    id: 'insertion-sort',
    courseId: 'sorting-101',
    title: 'Insertion Sort - Sắp xếp chèn',
    description: 'Sắp xếp chèn: chèn từng phần tử vào đúng vị trí trong đoạn đã sắp xếp.',
    sortOrder: 3,
    simulations: ['sort.insertion'],
  },
  'quick-sort': {
    id: 'quick-sort',
    courseId: 'sorting-advanced',
    title: 'Quick Sort - Sắp xếp nhanh',
    description: 'Sắp xếp nhanh theo chiến lược Chia để Trị với phân hoạch Lomuto.',
    sortOrder: 1,
    simulations: ['sort.quick'],
    theoryContent: `
# 🚀 Quick Sort – Sắp xếp nhanh

**Quick Sort** là thuật toán sắp xếp theo chiến lược **Chia để Trị** (Divide and Conquer).

## Ý tưởng chính
1. **Chọn Pivot**: Chọn một phần tử làm chốt (thường là phần tử cuối cùng).
2. **Phân hoạch (Partition)**: Sắp xếp lại mảng sao cho:
   - Các phần tử ≤ Pivot nằm bên trái.
   - Các phần tử > Pivot nằm bên phải.
3. **Đệ quy**: Áp dụng đệ quy cho mảng con bên trái và bên phải.

## Độ phức tạp
- **Thời gian**: O(n log n) trung bình, O(n²) trong trường hợp xấu nhất.
- **Bộ nhớ**: O(log n) do stack đệ quy.

## Mã giả
\`\`\`javascript
function quickSort(arr, low, high) {
  if (low < high) {
    const pi = partition(arr, low, high);
    quickSort(arr, low, pi - 1);
    quickSort(arr, pi + 1, high);
  }
}

function partition(arr, low, high) {
  const pivot = arr[high];
  let i = low - 1;
  for (let j = low; j < high; j++) {
    if (arr[j] <= pivot) {
      i++;
      swap(arr, i, j);
    }
  }
  swap(arr, i + 1, high);
  return i + 1;
}
\`\`\`
    `,
    quizQuestions: [
      {
        id: 'q1',
        questionText: 'Trong thuật toán Quick Sort, Pivot thường được chọn là phần tử nào?',
        options: ['Phần tử đầu tiên', 'Phần tử cuối cùng', 'Phần tử giữa', 'Bất kỳ phần tử nào cũng được'],
        correctIndex: 3,
        explanation: 'Mặc dù cài đặt Lomuto thường chọn phần tử cuối cùng, nhưng về mặt lý thuyết, bất kỳ phần tử nào cũng có thể được chọn làm Pivot.',
      },
      {
        id: 'q2',
        questionText: 'Độ phức tạp thời gian trung bình của Quick Sort là bao nhiêu?',
        options: ['O(1)', 'O(N)', 'O(N log N)', 'O(N²)'],
        correctIndex: 2,
        explanation: 'Trung bình Quick Sort chạy trong thời gian O(N log N), tuy nhiên trường hợp xấu nhất có thể lên tới O(N²).',
      },
      {
        id: 'q3',
        questionText: 'Quick Sort là một thuật toán sắp xếp ổn định (Stable Sort). Đúng hay sai?',
        options: ['Đúng', 'Sai'],
        correctIndex: 1,
        explanation: 'Quick Sort không ổn định (Unstable) vì quá trình hoán đổi có thể làm thay đổi thứ tự tương đối của các phần tử có giá trị bằng nhau.',
      },
      {
        id: 'q4',
        questionText: 'Trong quá trình Phân hoạch (Partition), các phần tử nằm bên trái Pivot có đặc điểm gì?',
        options: ['Lớn hơn Pivot', 'Nhỏ hơn hoặc bằng Pivot', 'Đã được sắp xếp', 'Bằng Pivot'],
        correctIndex: 1,
        explanation: 'Tất cả các phần tử nhỏ hơn hoặc bằng Pivot sẽ được đẩy về phía bên trái của Pivot.',
      },
      {
        id: 'q5',
        questionText: 'Điều gì xảy ra nếu mảng đầu vào đã được sắp xếp sẵn và ta luôn chọn phần tử cuối làm Pivot?',
        options: ['Quick Sort sẽ chạy rất nhanh (O(N))', 'Quick Sort sẽ rơi vào trường hợp xấu nhất O(N²)', 'Quick Sort sẽ báo lỗi đệ quy vô hạn', 'Không có vấn đề gì, vẫn là O(N log N)'],
        correctIndex: 1,
        explanation: 'Nếu mảng đã sắp xếp và luôn chọn phần tử cuối làm Pivot, mỗi lần phân hoạch sẽ tạo ra một mảng có N-1 phần tử, dẫn đến độ phức tạp O(N²).',
      },
    ],
    codelabTask: {
      description: 'Hoàn thiện hàm quickSort dưới đây bằng JavaScript (sử dụng Lomuto partition scheme).',
      initialCode: `function quickSort(arr, low = 0, high = arr.length - 1) {
  // TODO: Viết code tại đây
  
  return arr;
}`,
      entryFunction: 'quickSort',
      solution: `function quickSort(arr, low = 0, high = arr.length - 1) {
  if (low < high) {
    const pi = partition(arr, low, high);
    quickSort(arr, low, pi - 1);
    quickSort(arr, pi + 1, high);
  }
  return arr;
}

function partition(arr, low, high) {
  const pivot = arr[high];
  let i = low - 1;
  for (let j = low; j < high; j++) {
    if (arr[j] <= pivot) {
      i++;
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
  }
  [arr[i + 1], arr[high]] = [arr[high], arr[i + 1]];
  return i + 1;
}`,
      testCases: [
        { input: '[[3, 6, 8, 10, 1, 2, 1]]', expectedOutput: '[1, 1, 2, 3, 6, 8, 10]' },
        { input: '[[5, 2, 9, 1, 5, 6]]', expectedOutput: '[1, 2, 5, 5, 6, 9]' },
        { input: '[[10, -2, 4, 0]]', expectedOutput: '[-2, 0, 4, 10]' },
        { input: '[[]]', expectedOutput: '[]', isHidden: true },
      ],
    },
  },
  'merge-sort': {
    id: 'merge-sort',
    courseId: 'sorting-advanced',
    title: 'Merge Sort - Sắp xếp trộn',
    description: 'Sắp xếp trộn: chia đôi mảng, sắp xếp đệ quy rồi trộn hai nửa.',
    sortOrder: 2,
    simulations: ['sort.merge'],
  },
  'heap-sort': {
    id: 'heap-sort',
    courseId: 'sorting-advanced',
    title: 'Heap Sort - Sắp xếp đống',
    description: 'Sắp xếp đống: dựng max-heap rồi trích xuất lần lượt phần tử lớn nhất.',
    sortOrder: 3,
    simulations: ['sort.heap', 'heap.heapify', 'heap.extract'],
  },
  'linear-search': {
    id: 'linear-search',
    courseId: 'searching-101',
    title: 'Linear Search - Tìm kiếm tuần tự',
    description: 'Tìm kiếm tuần tự: duyệt từng phần tử tới khi tìm thấy hoặc hết mảng.',
    sortOrder: 1,
    simulations: ['search.linear'],
  },
  'binary-search': {
    id: 'binary-search',
    courseId: 'searching-101',
    title: 'Binary Search - Tìm kiếm nhị phân',
    description: 'Tìm kiếm nhị phân: chia đôi không gian tìm kiếm trên mảng đã sắp xếp.',
    sortOrder: 2,
    simulations: ['search.binary'],
  },
  'sliding-window': {
    id: 'sliding-window',
    courseId: 'searching-101',
    title: 'Sliding Window - Cửa sổ trượt',
    description: 'Kỹ thuật cửa sổ trượt: duy trì một đoạn con trượt qua mảng để giảm độ phức tạp.',
    sortOrder: 3,
    // TODO: chưa có simulation key phù hợp trong shared/simulation-catalog.json
    // (catalog hiện chỉ có sort.*/search.*/stack/queue/list/tree/heap/hash/graph/structure.*)
    simulations: [],
  },
  bst: {
    id: 'bst',
    courseId: 'tree-101',
    title: 'Binary Search Tree - Cây tìm kiếm nhị phân',
    description: 'BST: mọi nút trái nhỏ hơn, nút phải lớn hơn — tìm kiếm O(log n) trung bình.',
    sortOrder: 1,
    simulations: ['tree.bst-insert', 'tree.bst-search', 'tree.bst-delete', 'tree.bst-inorder'],
  },
  bfs: {
    id: 'bfs',
    courseId: 'tree-101',
    title: 'BFS - Duyệt theo chiều rộng',
    description: 'Duyệt đồ thị theo chiều rộng dùng hàng đợi (queue).',
    sortOrder: 2,
    simulations: ['graph.bfs', 'queue.enqueue', 'queue.dequeue'],
  },
  dfs: {
    id: 'dfs',
    courseId: 'tree-101',
    title: 'DFS - Duyệt theo chiều sâu',
    description: 'Duyệt đồ thị theo chiều sâu dùng ngăn xếp hoặc đệ quy.',
    sortOrder: 3,
    simulations: ['graph.dfs', 'stack.push', 'stack.pop'],
  },
  dijkstra: {
    id: 'dijkstra',
    courseId: 'tree-101',
    title: 'Dijkstra - Đường đi ngắn nhất',
    description: 'Dijkstra tìm đường đi ngắn nhất từ một đỉnh nguồn trên đồ thị có trọng số.',
    sortOrder: 4,
    simulations: ['graph.dijkstra'],
  },
};
