// engines/benchmark/codeTemplates.ts — code mẫu chạy THẬT cho Benchmark Lab (Màn 17)
//
// Mỗi template là code JS chạy trong sandbox của CompilerStepExecutor (chế độ measure —
// KHÔNG trace, SDD §4.0.3 v2.5). Sandbox cung cấp sẵn: array/arr (proxy), compare(i,j),
// swap(i,j), highlight(i). runMeasure đếm comparisons/swaps/writes + thời gian thật.
//
// Chỉ đăng ký GT có thể đo theo n (sắp xếp + tìm kiếm). Timeout 5s/độ đo → N/A.

export interface BenchmarkAlgorithmDef {
  key: string;
  title: string;
  complexityClass: 'O(n²)' | 'O(n log n)' | 'O(n)';
  /** Có hỗ trợ dữ liệu tốt nhất / xấu nhất không */
  supportsOrderedData: boolean;
  /** Code chạy trong sandbox — biến `array` là dữ liệu đầu vào */
  code: string;
}

const SORT_TEMPLATE = (body: string): string => `function solve(a) {
${body}
}
solve(array);
`;

export const BENCHMARK_ALGORITHMS: Record<string, BenchmarkAlgorithmDef> = {
  'sort.bubble': {
    key: 'sort.bubble',
    title: 'Bubble Sort',
    complexityClass: 'O(n²)',
    supportsOrderedData: true,
    code: SORT_TEMPLATE(`  const n = a.length;
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
  'sort.selection': {
    key: 'sort.selection',
    title: 'Selection Sort',
    complexityClass: 'O(n²)',
    supportsOrderedData: false,
    code: SORT_TEMPLATE(`  const n = a.length;
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
    complexityClass: 'O(n²)',
    supportsOrderedData: true,
    code: SORT_TEMPLATE(`  const n = a.length;
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
  'sort.merge': {
    key: 'sort.merge',
    title: 'Merge Sort',
    complexityClass: 'O(n log n)',
    supportsOrderedData: false,
    code: SORT_TEMPLATE(`  function merge(lo, mid, hi) {
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
    complexityClass: 'O(n log n)',
    supportsOrderedData: true,
    code: SORT_TEMPLATE(`  function partition(lo, hi) {
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
  'sort.heap': {
    key: 'sort.heap',
    title: 'Heap Sort',
    complexityClass: 'O(n log n)',
    supportsOrderedData: false,
    code: SORT_TEMPLATE(`  function heapify(n, i) {
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
  'search.linear': {
    key: 'search.linear',
    title: 'Linear Search',
    complexityClass: 'O(n)',
    supportsOrderedData: false,
    code: SORT_TEMPLATE(`  const target = -1;
  for (let i = 0; i < a.length; i++) {
    compare(i, 0);
    if (a[i] === target) break;
  }`),
  },
  'search.binary': {
    key: 'search.binary',
    title: 'Binary Search',
    complexityClass: 'O(n)',
    supportsOrderedData: false,
    code: SORT_TEMPLATE(`  const target = -1;
  let lo = 0, hi = a.length - 1;
  while (lo <= hi) {
    const mid = Math.floor((lo + hi) / 2);
    compare(mid, 0);
    if (a[mid] === target) break;
    if (a[mid] < target) lo = mid + 1;
    else hi = mid - 1;
  }`),
  },
};

export type BenchmarkMeasure = {
  durationMs: number;
  comparisons: number;
  swaps: number;
  writes: number;
};

/** Lưới kích thước n theo độ phức tạp (Màn 17 — O(n²) tối đa 500, O(n log n) tối đa 1000) */
export function sizesForComplexity(cls: BenchmarkAlgorithmDef['complexityClass']): number[] {
  return cls === 'O(n²)' ? [10, 50, 100, 200, 500] : [10, 50, 100, 500, 1000];
}

/** Sinh mảng ngẫu nhiên deterministic (seed 42) — cùng dãy cho mọi GT ở cùng n. */
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
