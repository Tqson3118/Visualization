// engines/benchmark/codeTemplates.ts — code mẫu chạy THẬT cho Benchmark Lab (Màn 17)
//
// Mỗi template là code JS chạy trong sandbox của CompilerStepExecutor (chế độ measure —
// KHÔNG trace, SDD §4.0.3 v2.5). Sandbox cung cấp sẵn: array/arr (proxy), compare(i,j),
// swap(i,j), highlight(i). runMeasure đếm comparisons/swaps/writes + thời gian thật.
//
// Hỗ trợ Đa Chuyên Đề: Sắp xếp (sort), Tìm kiếm (search), Tra cứu CTDL (lookup),
// Đồ thị (graph), Chiến lược Đống (heap_strategy). Timeout 5s/độ đo → N/A.

export type BenchmarkDomain = 'sort' | 'search' | 'lookup' | 'graph' | 'heap_strategy';

export interface BenchmarkAlgorithmDef {
  key: string;
  title: string;
  domain: BenchmarkDomain;
  complexityClass: string;
  spaceComplexity: string;
  bestCase: string;
  worstCase: string;
  /** Có hỗ trợ dữ liệu tốt nhất / xấu nhất không */
  supportsOrderedData: boolean;
  /** Code chạy trong sandbox — biến `array` là dữ liệu đầu vào */
  code: string;
}

const TEMPLATE = (body: string): string => `function solve(a) {
${body}
}
solve(array);
`;

export const BENCHMARK_ALGORITHMS: Record<string, BenchmarkAlgorithmDef> = {
  // ════════════════════════════════════════════════════════════════════════
  // 1. SẮP XẾP MẢNG (SORTING)
  // ════════════════════════════════════════════════════════════════════════
  'sort.bubble': {
    key: 'sort.bubble',
    title: 'Bubble Sort',
    domain: 'sort',
    complexityClass: 'O(n²)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(n)',
    worstCase: 'O(n²)',
    supportsOrderedData: true,
    code: TEMPLATE(`  const n = a.length;
  for (let i = 0; i < n - 1; i++) {
    let swapped = false;
    for (let j = 0; j < n - i - 1; j++) {
      compare(j, j + 1);
      if (a[j] > a[j + 1]) {
        swap(j, j + 1);
        swapped = true;
      }
    }
    if (!swapped) break;
  }`),
  },

  'sort.cocktail': {
    key: 'sort.cocktail',
    title: 'Cocktail Shaker Sort',
    domain: 'sort',
    complexityClass: 'O(n²)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(n)',
    worstCase: 'O(n²)',
    supportsOrderedData: true,
    code: TEMPLATE(`  let left = 0;
  let right = a.length - 1;
  let swapped = true;
  while (swapped && left < right) {
    swapped = false;
    for (let i = left; i < right; i++) {
      compare(i, i + 1);
      if (a[i] > a[i + 1]) {
        swap(i, i + 1);
        swapped = true;
      }
    }
    right--;
    if (!swapped) break;
    swapped = false;
    for (let i = right; i > left; i--) {
      compare(i, i - 1);
      if (a[i] < a[i - 1]) {
        swap(i, i - 1);
        swapped = true;
      }
    }
    left++;
  }`),
  },

  'sort.selection': {
    key: 'sort.selection',
    title: 'Selection Sort',
    domain: 'sort',
    complexityClass: 'O(n²)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(n²)',
    worstCase: 'O(n²)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const n = a.length;
  for (let i = 0; i < n - 1; i++) {
    let min = i;
    for (let j = i + 1; j < n; j++) {
      compare(j, min);
      if (a[j] < a[min]) min = j;
    }
    if (min !== i) swap(i, min);
  }`),
  },

  'sort.insertion': {
    key: 'sort.insertion',
    title: 'Insertion Sort',
    domain: 'sort',
    complexityClass: 'O(n²)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(n)',
    worstCase: 'O(n²)',
    supportsOrderedData: true,
    code: TEMPLATE(`  const n = a.length;
  for (let i = 1; i < n; i++) {
    const key = a[i];
    let j = i - 1;
    while (j >= 0) {
      compare(j, i);
      if (a[j] > key) {
        a[j + 1] = a[j];
        j--;
      } else break;
    }
    a[j + 1] = key;
  }`),
  },

  'sort.shell': {
    key: 'sort.shell',
    title: 'Shell Sort (Knuth)',
    domain: 'sort',
    complexityClass: 'O(n^1.3)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(n log n)',
    worstCase: 'O(n²)',
    supportsOrderedData: true,
    code: TEMPLATE(`  const n = a.length;
  let gap = 1;
  while (gap < Math.floor(n / 3)) gap = gap * 3 + 1;
  while (gap >= 1) {
    for (let i = gap; i < n; i++) {
      const temp = a[i];
      let j = i;
      while (j >= gap) {
        compare(j - gap, i);
        if (a[j - gap] > temp) {
          a[j] = a[j - gap];
          j -= gap;
        } else break;
      }
      a[j] = temp;
    }
    gap = Math.floor(gap / 3);
  }`),
  },

  'sort.merge': {
    key: 'sort.merge',
    title: 'Merge Sort',
    domain: 'sort',
    complexityClass: 'O(n log n)',
    spaceComplexity: 'O(n)',
    bestCase: 'O(n log n)',
    worstCase: 'O(n log n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  function merge(lo, mid, hi) {
    const left = a.slice(lo, mid + 1);
    const right = a.slice(mid + 1, hi + 1);
    let i = 0, j = 0, k = lo;
    while (i < left.length && j < right.length) {
      compare(lo + i, mid + 1 + j);
      if (left[i] <= right[j]) {
        a[k++] = left[i++];
      } else {
        a[k++] = right[j++];
      }
    }
    while (i < left.length) a[k++] = left[i++];
    while (j < right.length) a[k++] = right[j++];
  }
  function mergeSort(lo, hi) {
    if (lo >= hi) return;
    const mid = Math.floor((lo + hi) / 2);
    mergeSort(lo, mid);
    mergeSort(mid + 1, hi);
    merge(lo, mid, hi);
  }
  mergeSort(0, a.length - 1);`),
  },

  'sort.quick': {
    key: 'sort.quick',
    title: 'Quick Sort (Lomuto)',
    domain: 'sort',
    complexityClass: 'O(n log n)',
    spaceComplexity: 'O(log n)',
    bestCase: 'O(n log n)',
    worstCase: 'O(n²)',
    supportsOrderedData: true,
    code: TEMPLATE(`  function partition(lo, hi) {
    const pivot = a[hi];
    let i = lo;
    for (let j = lo; j < hi; j++) {
      compare(j, hi);
      if (a[j] < pivot) {
        swap(i, j);
        i++;
      }
    }
    swap(i, hi);
    return i;
  }
  function quickSort(lo, hi) {
    if (lo >= hi) return;
    const p = partition(lo, hi);
    quickSort(lo, p - 1);
    quickSort(p + 1, hi);
  }
  quickSort(0, a.length - 1);`),
  },

  'sort.quick_hoare': {
    key: 'sort.quick_hoare',
    title: 'Quick Sort (Hoare)',
    domain: 'sort',
    complexityClass: 'O(n log n)',
    spaceComplexity: 'O(log n)',
    bestCase: 'O(n log n)',
    worstCase: 'O(n²)',
    supportsOrderedData: true,
    code: TEMPLATE(`  function partition(lo, hi) {
    const pivot = a[Math.floor((lo + hi) / 2)];
    let i = lo - 1;
    let j = hi + 1;
    while (true) {
      do {
        i++;
        compare(i, Math.floor((lo + hi) / 2));
      } while (a[i] < pivot);
      do {
        j--;
        compare(j, Math.floor((lo + hi) / 2));
      } while (a[j] > pivot);
      if (i >= j) return j;
      swap(i, j);
    }
  }
  function quickSort(lo, hi) {
    if (lo < hi) {
      const p = partition(lo, hi);
      quickSort(lo, p);
      quickSort(p + 1, hi);
    }
  }
  quickSort(0, a.length - 1);`),
  },

  'sort.heap': {
    key: 'sort.heap',
    title: 'Heap Sort',
    domain: 'sort',
    complexityClass: 'O(n log n)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(n log n)',
    worstCase: 'O(n log n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  function heapify(n, i) {
    let largest = i;
    const l = 2 * i + 1;
    const r = 2 * i + 2;
    if (l < n) {
      compare(l, largest);
      if (a[l] > a[largest]) largest = l;
    }
    if (r < n) {
      compare(r, largest);
      if (a[r] > a[largest]) largest = r;
    }
    if (largest !== i) {
      swap(i, largest);
      heapify(n, largest);
    }
  }
  const n = a.length;
  for (let i = Math.floor(n / 2) - 1; i >= 0; i--) heapify(n, i);
  for (let i = n - 1; i > 0; i--) {
    swap(0, i);
    heapify(i, 0);
  }`),
  },

  'sort.counting': {
    key: 'sort.counting',
    title: 'Counting Sort (Non-comp)',
    domain: 'sort',
    complexityClass: 'O(n+k)',
    spaceComplexity: 'O(k)',
    bestCase: 'O(n+k)',
    worstCase: 'O(n+k)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const n = a.length;
  if (n <= 1) return;
  let min = a[0], max = a[0];
  for (let i = 1; i < n; i++) {
    compare(i, 0);
    if (a[i] < min) min = a[i];
    if (a[i] > max) max = a[i];
  }
  const range = max - min + 1;
  const count = new Array(range).fill(0);
  for (let i = 0; i < n; i++) count[a[i] - min]++;
  let k = 0;
  for (let i = 0; i < range; i++) {
    while (count[i] > 0) {
      a[k++] = i + min;
      count[i]--;
    }
  }`),
  },

  'sort.radix_lsd': {
    key: 'sort.radix_lsd',
    title: 'Radix Sort (LSD)',
    domain: 'sort',
    complexityClass: 'O(n)',
    spaceComplexity: 'O(n+b)',
    bestCase: 'O(d·n)',
    worstCase: 'O(d·n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const n = a.length;
  if (n <= 1) return;
  let max = a[0];
  for (let i = 1; i < n; i++) {
    compare(i, 0);
    if (a[i] > max) max = a[i];
  }
  for (let exp = 1; Math.floor(max / exp) > 0; exp *= 10) {
    const output = new Array(n);
    const count = new Array(10).fill(0);
    for (let i = 0; i < n; i++) {
      const digit = Math.floor(a[i] / exp) % 10;
      count[digit]++;
    }
    for (let i = 1; i < 10; i++) count[i] += count[i - 1];
    for (let i = n - 1; i >= 0; i--) {
      const digit = Math.floor(a[i] / exp) % 10;
      output[count[digit] - 1] = a[i];
      count[digit]--;
    }
    for (let i = 0; i < n; i++) a[i] = output[i];
  }`),
  },

  // ════════════════════════════════════════════════════════════════════════
  // 2. TÌM KIẾM MẢNG (SEARCHING)
  // ════════════════════════════════════════════════════════════════════════
  'search.linear': {
    key: 'search.linear',
    title: 'Linear Search',
    domain: 'search',
    complexityClass: 'O(n)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(1)',
    worstCase: 'O(n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const target = -1;
  for (let i = 0; i < a.length; i++) {
    compare(i, 0);
    if (a[i] === target) break;
  }`),
  },

  'search.binary': {
    key: 'search.binary',
    title: 'Binary Search',
    domain: 'search',
    complexityClass: 'O(log n)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(1)',
    worstCase: 'O(log n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const target = -1;
  let lo = 0, hi = a.length - 1;
  while (lo <= hi) {
    const mid = Math.floor((lo + hi) / 2);
    compare(mid, 0);
    if (a[mid] === target) break;
    if (a[mid] < target) lo = mid + 1;
    else hi = mid - 1;
  }`),
  },

  'search.ternary': {
    key: 'search.ternary',
    title: 'Ternary Search',
    domain: 'search',
    complexityClass: 'O(log3 n)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(1)',
    worstCase: 'O(log3 n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const target = -1;
  let lo = 0, hi = a.length - 1;
  while (lo <= hi) {
    const mid1 = lo + Math.floor((hi - lo) / 3);
    const mid2 = hi - Math.floor((hi - lo) / 3);
    compare(mid1, 0);
    if (a[mid1] === target) break;
    compare(mid2, 0);
    if (a[mid2] === target) break;
    if (target < a[mid1]) {
      hi = mid1 - 1;
    } else if (target > a[mid2]) {
      lo = mid2 + 1;
    } else {
      lo = mid1 + 1;
      hi = mid2 - 1;
    }
  }`),
  },

  'search.jump': {
    key: 'search.jump',
    title: 'Jump Search',
    domain: 'search',
    complexityClass: 'O(sqrt(n))',
    spaceComplexity: 'O(1)',
    bestCase: 'O(1)',
    worstCase: 'O(sqrt(n))',
    supportsOrderedData: false,
    code: TEMPLATE(`  const target = -1;
  const n = a.length;
  const step = Math.floor(Math.sqrt(n));
  let prev = 0, curr = step;
  while (curr < n) {
    compare(Math.min(curr, n) - 1, 0);
    if (a[Math.min(curr, n) - 1] >= target) break;
    prev = curr;
    curr += step;
  }
  while (prev < Math.min(curr, n)) {
    compare(prev, 0);
    if (a[prev] === target) break;
    prev++;
  } `),
  },

  'search.exponential': {
    key: 'search.exponential',
    title: 'Exponential Search',
    domain: 'search',
    complexityClass: 'O(log n)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(1)',
    worstCase: 'O(log n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const target = -1;
  const n = a.length;
  if (n === 0) return;
  compare(0, 0);
  if (a[0] === target) return;
  let i = 1;
  while (i < n && a[i] <= target) {
    compare(i, 0);
    i *= 2;
  }
  let lo = Math.floor(i / 2), hi = Math.min(i, n - 1);
  while (lo <= hi) {
    const mid = Math.floor((lo + hi) / 2);
    compare(mid, 0);
    if (a[mid] === target) break;
    if (a[mid] < target) lo = mid + 1;
    else hi = mid - 1;
  }`),
  },

  'search.interpolation': {
    key: 'search.interpolation',
    title: 'Interpolation Search',
    domain: 'search',
    complexityClass: 'O(log log n)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(1)',
    worstCase: 'O(n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const target = -1;
  let lo = 0, hi = a.length - 1;
  while (lo <= hi && target >= a[lo] && target <= a[hi]) {
    compare(lo, hi);
    if (lo === hi) {
      if (a[lo] === target) break;
      break;
    }
    const pos = lo + Math.floor(((target - a[lo]) / (a[hi] - a[lo])) * (hi - lo));
    compare(pos, 0);
    if (a[pos] === target) break;
    if (a[pos] < target) lo = pos + 1;
    else hi = pos - 1;
  }`),
  },

  // ════════════════════════════════════════════════════════════════════════
  // 3. TRA CỨU & THAO TÁC CẤU TRÚC DỮ LIỆU (LOOKUP & INSERT)
  // ════════════════════════════════════════════════════════════════════════
  'lookup.array': {
    key: 'lookup.array',
    title: 'Mảng (Array Scan)',
    domain: 'lookup',
    complexityClass: 'O(n)',
    spaceComplexity: 'O(n)',
    bestCase: 'O(1)',
    worstCase: 'O(n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const n = a.length;
  // Quét tra cứu tuần tự n truy vấn
  for (let q = 0; q < n; q++) {
    const target = a[q];
    for (let i = 0; i < n; i++) {
      compare(i, q);
      if (a[i] === target) break;
    }
  }`),
  },

  'lookup.linkedlist': {
    key: 'lookup.linkedlist',
    title: 'DS liên kết (Linked List)',
    domain: 'lookup',
    complexityClass: 'O(n)',
    spaceComplexity: 'O(n)',
    bestCase: 'O(1)',
    worstCase: 'O(n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const n = a.length;
  // Xây dựng danh sách liên kết
  class Node { constructor(v) { this.val = v; this.next = null; } }
  let head = null, tail = null;
  for (let i = 0; i < n; i++) {
    const node = new Node(a[i]);
    if (!head) head = tail = node;
    else { tail.next = node; tail = node; }
  }
  // Tra cứu các phần tử qua con trỏ
  for (let q = 0; q < n; q++) {
    const target = a[q];
    let curr = head;
    while (curr) {
      compare(q, 0);
      if (curr.val === target) break;
      curr = curr.next;
    }
  }`),
  },

  'lookup.bst': {
    key: 'lookup.bst',
    title: 'Cây BST (Binary Search Tree)',
    domain: 'lookup',
    complexityClass: 'O(log n)',
    spaceComplexity: 'O(n)',
    bestCase: 'O(1)',
    worstCase: 'O(n)',
    supportsOrderedData: true,
    code: TEMPLATE(`  const n = a.length;
  class BSTNode { constructor(v) { this.val = v; this.left = null; this.right = null; } }
  function insert(root, val) {
    if (!root) return new BSTNode(val);
    if (val < root.val) root.left = insert(root.left, val);
    else if (val > root.val) root.right = insert(root.right, val);
    return root;
  }
  let root = null;
  for (let i = 0; i < n; i++) root = insert(root, a[i]);
  // Tra cứu n phần tử
  function search(node, val) {
    if (!node) return false;
    compare(0, 0);
    if (node.val === val) return true;
    if (val < node.val) return search(node.left, val);
    return search(node.right, val);
  }
  for (let q = 0; q < n; q++) search(root, a[q]);`),
  },

  'lookup.avl': {
    key: 'lookup.avl',
    title: 'Cây AVL (Self-balancing Tree)',
    domain: 'lookup',
    complexityClass: 'O(log n)',
    spaceComplexity: 'O(n)',
    bestCase: 'O(1)',
    worstCase: 'O(log n)',
    supportsOrderedData: true,
    code: TEMPLATE(`  const n = a.length;
  class AVLNode {
    constructor(v) { this.val = v; this.left = null; this.right = null; this.h = 1; }
  }
  function h(node) { return node ? node.h : 0; }
  function getB(node) { return node ? h(node.left) - h(node.right) : 0; }
  function rR(y) {
    const x = y.left, t = x.right;
    x.right = y; y.left = t;
    y.h = Math.max(h(y.left), h(y.right)) + 1;
    x.h = Math.max(h(x.left), h(x.right)) + 1;
    return x;
  }
  function rL(x) {
    const y = x.right, t = y.left;
    y.left = x; x.right = t;
    x.h = Math.max(h(x.left), h(x.right)) + 1;
    y.h = Math.max(h(y.left), h(y.right)) + 1;
    return y;
  }
  function insert(node, val) {
    if (!node) return new AVLNode(val);
    if (val < node.val) node.left = insert(node.left, val);
    else if (val > node.val) node.right = insert(node.right, val);
    else return node;
    node.h = 1 + Math.max(h(node.left), h(node.right));
    const b = getB(node);
    if (b > 1 && val < node.left.val) return rR(node);
    if (b < -1 && val > node.right.val) return rL(node);
    if (b > 1 && val > node.left.val) { node.left = rL(node.left); return rR(node); }
    if (b < -1 && val < node.right.val) { node.right = rR(node.right); return rL(node); }
    return node;
  }
  let root = null;
  for (let i = 0; i < n; i++) root = insert(root, a[i]);
  // Tra cứu n phần tử
  for (let q = 0; q < n; q++) {
    const target = a[q];
    let curr = root;
    while (curr) {
      compare(0, 0);
      if (curr.val === target) break;
      curr = target < curr.val ? curr.left : curr.right;
    }
  }`),
  },

  'lookup.hashtable': {
    key: 'lookup.hashtable',
    title: 'Bảng băm (Hash Table Chaining)',
    domain: 'lookup',
    complexityClass: 'O(1)',
    spaceComplexity: 'O(n)',
    bestCase: 'O(1)',
    worstCase: 'O(n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const n = a.length;
  const size = Math.max(17, Math.floor(n * 1.5));
  const buckets = Array.from({ length: size }, () => []);
  for (let i = 0; i < n; i++) {
    const key = a[i];
    const idx = ((key % size) + size) % size;
    buckets[idx].push(key);
  }
  // Tra cứu tức thì O(1)
  for (let q = 0; q < n; q++) {
    const key = a[q];
    const idx = ((key % size) + size) % size;
    const bucket = buckets[idx];
    for (let j = 0; j < bucket.length; j++) {
      compare(q, j);
      if (bucket[j] === key) break;
    }
  }`),
  },

  // ════════════════════════════════════════════════════════════════════════
  // 4. THUẬT TOÁN ĐỒ THỊ (GRAPH ALGORITHMS)
  // ════════════════════════════════════════════════════════════════════════
  'graph.bfs': {
    key: 'graph.bfs',
    title: 'BFS Đồ thị (Queue)',
    domain: 'graph',
    complexityClass: 'O(V+E)',
    spaceComplexity: 'O(V)',
    bestCase: 'O(V+E)',
    worstCase: 'O(V+E)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const V = a.length;
  // Dựng đồ thị ngẫu nhiên V đỉnh từ mảng input a
  const adj = Array.from({ length: V }, () => []);
  for (let i = 0; i < V; i++) {
    const target = a[i] % V;
    if (target !== i) { adj[i].push(target); adj[target].push(i); }
  }
  const visited = new Array(V).fill(false);
  const queue = [0];
  visited[0] = true;
  while (queue.length > 0) {
    const u = queue.shift();
    for (const v of adj[u]) {
      compare(u, v);
      if (!visited[v]) {
        visited[v] = true;
        queue.push(v);
      }
    }
  }`),
  },

  'graph.dfs': {
    key: 'graph.dfs',
    title: 'DFS Đồ thị (Stack / Đệ quy)',
    domain: 'graph',
    complexityClass: 'O(V+E)',
    spaceComplexity: 'O(V)',
    bestCase: 'O(V+E)',
    worstCase: 'O(V+E)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const V = a.length;
  const adj = Array.from({ length: V }, () => []);
  for (let i = 0; i < V; i++) {
    const target = a[i] % V;
    if (target !== i) { adj[i].push(target); adj[target].push(i); }
  }
  const visited = new Array(V).fill(false);
  function dfs(u) {
    visited[u] = true;
    for (const v of adj[u]) {
      compare(u, v);
      if (!visited[v]) dfs(v);
    }
  }
  dfs(0);`),
  },

  'graph.dijkstra_heap': {
    key: 'graph.dijkstra_heap',
    title: 'Dijkstra (Min-Heap / Thưa)',
    domain: 'graph',
    complexityClass: 'O((V+E)log V)',
    spaceComplexity: 'O(V)',
    bestCase: 'O((V+E)log V)',
    worstCase: 'O((V+E)log V)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const V = a.length;
  const adj = Array.from({ length: V }, () => []);
  for (let i = 0; i < V; i++) {
    const v = a[i] % V;
    const w = (a[i] % 50) + 1;
    if (v !== i) adj[i].push({ to: v, weight: w });
  }
  const dist = new Array(V).fill(Infinity);
  dist[0] = 0;
  // Giả lập Min-Heap đơn giản
  const pq = [{ u: 0, d: 0 }];
  while (pq.length > 0) {
    pq.sort((x, y) => x.d - y.d);
    const { u, d } = pq.shift();
    if (d > dist[u]) continue;
    for (const edge of adj[u]) {
      compare(u, edge.to);
      if (dist[u] + edge.weight < dist[edge.to]) {
        dist[edge.to] = dist[u] + edge.weight;
        pq.push({ u: edge.to, d: dist[edge.to] });
      }
    }
  }`),
  },

  'graph.dijkstra_matrix': {
    key: 'graph.dijkstra_matrix',
    title: 'Dijkstra (Matrix / Dày)',
    domain: 'graph',
    complexityClass: 'O(V²)',
    spaceComplexity: 'O(V²)',
    bestCase: 'O(V²)',
    worstCase: 'O(V²)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const V = a.length;
  const matrix = Array.from({ length: V }, () => new Array(V).fill(Infinity));
  for (let i = 0; i < V; i++) matrix[i][i] = 0;
  for (let i = 0; i < V; i++) {
    const v = a[i] % V;
    matrix[i][v] = (a[i] % 50) + 1;
  }
  const dist = new Array(V).fill(Infinity);
  const visited = new Array(V).fill(false);
  dist[0] = 0;
  for (let count = 0; count < V - 1; count++) {
    let minD = Infinity, u = -1;
    for (let i = 0; i < V; i++) {
      if (!visited[i] && dist[i] < minD) { minD = dist[i]; u = i; }
    }
    if (u === -1) break;
    visited[u] = true;
    for (let v = 0; v < V; v++) {
      compare(u, v);
      if (!visited[v] && matrix[u][v] !== Infinity && dist[u] + matrix[u][v] < dist[v]) {
        dist[v] = dist[u] + matrix[u][v];
      }
    }
  }`),
  },

  // ════════════════════════════════════════════════════════════════════════
  // 5. CHIẾN LƯỢC ĐỐNG (HEAP CONSTRUCTION STRATEGIES)
  // ════════════════════════════════════════════════════════════════════════
  'heap.floyd': {
    key: 'heap.floyd',
    title: 'Floyd Heapify (Xây 1 lần)',
    domain: 'heap_strategy',
    complexityClass: 'O(n)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(n)',
    worstCase: 'O(n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const n = a.length;
  function siftDown(i, len) {
    let largest = i;
    const l = 2 * i + 1, r = 2 * i + 2;
    if (l < len) { compare(l, largest); if (a[l] > a[largest]) largest = l; }
    if (r < len) { compare(r, largest); if (a[r] > a[largest]) largest = r; }
    if (largest !== i) {
      swap(i, largest);
      siftDown(largest, len);
    }
  }
  // Thuật toán Floyd O(N) từ dưới lên
  for (let i = Math.floor(n / 2) - 1; i >= 0; i--) {
    siftDown(i, n);
  }`),
  },

  'heap.sequential': {
    key: 'heap.sequential',
    title: 'N lần Chèn Heap (Tuần tự)',
    domain: 'heap_strategy',
    complexityClass: 'O(n log n)',
    spaceComplexity: 'O(1)',
    bestCase: 'O(n)',
    worstCase: 'O(n log n)',
    supportsOrderedData: false,
    code: TEMPLATE(`  const n = a.length;
  function siftUp(i) {
    while (i > 0) {
      const parent = Math.floor((i - 1) / 2);
      compare(i, parent);
      if (a[i] > a[parent]) {
        swap(i, parent);
        i = parent;
      } else break;
    }
  }
  // Chèn tuần tự N phần tử O(N log N)
  for (let i = 1; i < n; i++) {
    siftUp(i);
  }`),
  },
};

export type BenchmarkMeasure = {
  durationMs: number;
  comparisons: number;
  swaps: number;
  writes: number;
};

/** Lưới kích thước n theo miền & độ phức tạp */
export function sizesForDomain(domain: BenchmarkDomain, complexity: string): number[] {
  if (domain === 'graph') {
    return [10, 30, 60, 100, 200];
  }
  if (domain === 'lookup') {
    return [100, 500, 1000, 2500, 5000];
  }
  if (complexity.includes('n²') || complexity.includes('V²')) {
    return [10, 50, 100, 200, 500];
  }
  if (complexity.includes('log') || complexity.includes('O(1)')) {
    return [50, 200, 1000, 5000, 10000];
  }
  return [10, 50, 100, 500, 1000];
}

/** Tương thích hàm cũ: Lưới kích thước n theo độ phức tạp */
export function sizesForComplexity(cls: string): number[] {
  return cls === 'O(n²)' ? [10, 50, 100, 200, 500] : [10, 50, 100, 500, 1000];
}

/** Sinh mảng ngẫu nhiên deterministic (seed 42) */
export function randomArray(n: number): number[] {
  let s = 42;
  const rng = (): number => {
    s ^= s << 13;
    s >>>= 0;
    s ^= s >> 17;
    s ^= s << 5;
    s >>>= 0;
    return s / 0xffffffff;
  };
  return Array.from({ length: n }, () => Math.floor(rng() * 1000));
}

export function worstArray(n: number): number[] {
  return Array.from({ length: n }, (_, i) => n - i);
}

export function bestArray(n: number): number[] {
  return Array.from({ length: n }, (_, i) => i);
}

/** Mảng gần như đã sắp xếp (95% sorted) */
export function nearlySortedArray(n: number): number[] {
  const arr = Array.from({ length: n }, (_, i) => i);
  const swapCount = Math.max(1, Math.floor(n * 0.05));
  let s = 101;
  const rng = (): number => {
    s ^= s << 13;
    s >>>= 0;
    s ^= s >> 17;
    s ^= s << 5;
    s >>>= 0;
    return s / 0xffffffff;
  };
  for (let k = 0; k < swapCount; k++) {
    const i = Math.floor(rng() * n);
    const j = Math.floor(rng() * n);
    const tmp = arr[i];
    arr[i] = arr[j];
    arr[j] = tmp;
  }
  return arr;
}

/** Mảng nhiều phần tử trùng lặp (chỉ 5 giá trị) */
export function duplicatesArray(n: number): number[] {
  let s = 77;
  const rng = (): number => {
    s ^= s << 13;
    s >>>= 0;
    s ^= s >> 17;
    s ^= s << 5;
    s >>>= 0;
    return s / 0xffffffff;
  };
  return Array.from({ length: n }, () => Math.floor(rng() * 5));
}
