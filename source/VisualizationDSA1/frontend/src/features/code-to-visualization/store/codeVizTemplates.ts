import { DEFAULT_SOURCE_CODE, DEFAULT_INPUT_ARRAY } from './liveCompilerDefaults';

/**
 * Mảng các key chuẩn hoá cho thuật toán MẢNG được phép bật bước "Code-to-Viz".
 * Graph / BST / OOP / SOLID / Pattern ... KHÔNG nằm trong danh sách này.
 */
export const ARRAY_ALGORITHM_KEYS = new Set<string>([
  'bubble-sort',
  'selection-sort',
  'insertion-sort',
  'quick-sort',
  'merge-sort',
  'heap-sort',
  'radix-sort',
  'counting-sort',
  'bucket-sort',
  'linear-search',
  'binary-search',
  'sliding-window',
]);

/**
 * Alias thường gặp (không dấu, rút gọn, kiểu sandboxType) → key chuẩn hoá.
 */
const ARRAY_ALGORITHM_ALIASES: Record<string, string> = {
  sorting: 'bubble-sort',
  bubble: 'bubble-sort',
  bubblesort: 'bubble-sort',
  bubble_sort: 'bubble-sort',
  'bubble sort': 'bubble-sort',
  selection: 'selection-sort',
  selectionsort: 'selection-sort',
  selection_sort: 'selection-sort',
  'selection sort': 'selection-sort',
  insertion: 'insertion-sort',
  insertionsort: 'insertion-sort',
  insertion_sort: 'insertion-sort',
  'insertion sort': 'insertion-sort',
  quick: 'quick-sort',
  quicksort: 'quick-sort',
  quick_sort: 'quick-sort',
  'quick sort': 'quick-sort',
  merge: 'merge-sort',
  mergesort: 'merge-sort',
  merge_sort: 'merge-sort',
  'merge sort': 'merge-sort',
  heap: 'heap-sort',
  heapsort: 'heap-sort',
  heap_sort: 'heap-sort',
  'heap sort': 'heap-sort',
  radix: 'radix-sort',
  radixsort: 'radix-sort',
  radix_sort: 'radix-sort',
  'radix sort': 'radix-sort',
  counting: 'counting-sort',
  countingsort: 'counting-sort',
  counting_sort: 'counting-sort',
  'counting sort': 'counting-sort',
  bucket: 'bucket-sort',
  bucketsort: 'bucket-sort',
  bucket_sort: 'bucket-sort',
  'bucket sort': 'bucket-sort',
  linearsearch: 'linear-search',
  linear_search: 'linear-search',
  'linear search': 'linear-search',
  'linear-search': 'linear-search',
  binarysearch: 'binary-search',
  binary_search: 'binary-search',
  'binary search': 'binary-search',
  'binary-search': 'binary-search',
  slidingwindow: 'sliding-window',
  sliding_window: 'sliding-window',
  'sliding window': 'sliding-window',
  'sliding-window': 'sliding-window',
};

/**
 * Chuẩn hoá một chuỗi algorithm/sandboxType → key thuật toán mảng, hoặc null
 * nếu không thuộc tập cho phép (graph/bst/stack/queue/oop/solid/pattern...).
 */
export function normalizeArrayAlgorithmKey(raw: string | null | undefined): string | null {
  const key = (raw ?? '').trim().toLowerCase();
  if (!key) return null;
  if (ARRAY_ALGORITHM_KEYS.has(key)) return key;
  const alias = ARRAY_ALGORITHM_ALIASES[key];
  if (alias) return alias;
  const stripped = key.replace(/[\s_]+/g, '-');
  if (ARRAY_ALGORITHM_KEYS.has(stripped)) return stripped;
  const strippedAlias = ARRAY_ALGORITHM_ALIASES[stripped];
  if (strippedAlias) return strippedAlias;
  return null;
}

/** Kiểm tra một key/alias có thuộc tập thuật toán MẢNG hay không. */
export function isArrayAlgorithmKey(raw: string | null | undefined): boolean {
  return normalizeArrayAlgorithmKey(raw) !== null;
}

/** Parse chuỗi "1, 2, 3" → number[], trả về null nếu không hợp lệ. */
export function parseNumberArray(raw: string | null | undefined): number[] | null {
  if (!raw) return null;
  const trimmed = raw.trim();
  if (!trimmed) return null;
  const parts = trimmed.split(',').map(s => s.trim()).filter(Boolean);
  if (parts.length < 2) return null;
  const nums: number[] = [];
  for (const part of parts) {
    const num = Number(part);
    if (Number.isNaN(num) || !Number.isFinite(num)) return null;
    nums.push(num);
  }
  return nums;
}

export interface CodeVizTemplate {
  code: string;
  array: number[];
}

const SELECTION_SORT_CODE = `function selectionSort(arr) {
  let n = arr.length;
  for (let i = 0; i < n - 1; i++) {
    let minIdx = i;
    for (let j = i + 1; j < n; j++) {
      if (arr[j] < arr[minIdx]) {
        minIdx = j;
      }
    }
    if (minIdx !== i) {
      let temp = arr[i];
      arr[i] = arr[minIdx];
      arr[minIdx] = temp;
    }
  }
}`;

const INSERTION_SORT_CODE = `function insertionSort(arr) {
  let n = arr.length;
  for (let i = 1; i < n; i++) {
    let j = i;
    while (j > 0 && arr[j] < arr[j - 1]) {
      let temp = arr[j];
      arr[j] = arr[j - 1];
      arr[j - 1] = temp;
      j--;
    }
  }
}`;

const QUICK_SORT_CODE = `function quickSort(arr, low = 0, high = arr.length - 1) {
  if (low >= high) return;
  let pivot = high;
  let store = low;
  for (let i = low; i < high; i++) {
    if (arr[i] <= arr[pivot]) {
      if (i !== store) {
        let temp = arr[i];
        arr[i] = arr[store];
        arr[store] = temp;
      }
      store++;
    }
  }
  if (store !== pivot) {
    let temp = arr[store];
    arr[store] = arr[pivot];
    arr[pivot] = temp;
  }
  quickSort(arr, low, store - 1);
  quickSort(arr, store + 1, high);
}`;

const MERGE_SORT_CODE = `function mergeSort(arr, start = 0, end = arr.length - 1) {
  if (start >= end) return;
  let mid = Math.floor((start + end) / 2);
  mergeSort(arr, start, mid);
  mergeSort(arr, mid + 1, end);
  merge(arr, start, mid, end);
}

function merge(arr, start, mid, end) {
  let start2 = mid + 1;
  if (start2 > end) return;
  if (arr[mid] <= arr[start2]) return;
  while (start <= mid && start2 <= end) {
    if (arr[start] <= arr[start2]) {
      start++;
    } else {
      let value = arr[start2];
      let index = start2;
      while (index !== start) {
        arr[index] = arr[index - 1];
        index--;
      }
      arr[start] = value;
      start++;
      mid++;
      start2++;
    }
  }
}`;

const HEAP_SORT_CODE = `function heapSort(arr) {
  let n = arr.length;
  for (let i = Math.floor(n / 2) - 1; i >= 0; i--) {
    siftDown(arr, n, i);
  }
  for (let i = n - 1; i > 0; i--) {
    let temp = arr[0];
    arr[0] = arr[i];
    arr[i] = temp;
    siftDown(arr, i, 0);
  }
}

function siftDown(arr, n, i) {
  let largest = i;
  let left = 2 * i + 1;
  let right = 2 * i + 2;
  if (left < n && arr[left] > arr[largest]) {
    largest = left;
  }
  if (right < n && arr[right] > arr[largest]) {
    largest = right;
  }
  if (largest !== i) {
    let temp = arr[i];
    arr[i] = arr[largest];
    arr[largest] = temp;
    siftDown(arr, n, largest);
  }
}`;

const LINEAR_SEARCH_CODE = `function linearSearch(arr, target = 42) {
  let n = arr.length;
  for (let i = 0; i < n; i++) {
    if (arr[i] === target) {
      return i;
    }
  }
  return -1;
}`;

const BINARY_SEARCH_CODE = `function binarySearch(arr, target = 23) {
  let low = 0;
  let high = arr.length - 1;
  while (low <= high) {
    let mid = Math.floor((low + high) / 2);
    if (arr[mid] === target) {
      return mid;
    } else if (arr[mid] > target) {
      high = mid - 1;
    } else {
      low = mid + 1;
    }
  }
  return -1;
}`;

const SLIDING_WINDOW_CODE = `function maxSlidingWindowSum(arr, k = 3) {
  let n = arr.length;
  let maxSum = 0;
  for (let i = 0; i < k && i < n; i++) {
    maxSum += arr[i];
  }
  let windowSum = maxSum;
  for (let i = k; i < n; i++) {
    windowSum = windowSum - arr[i - k] + arr[i];
    if (windowSum > maxSum) {
      maxSum = windowSum;
    }
  }
  return maxSum;
}`;

const COUNTING_SORT_CODE = `function countingSort(arr) {
  let n = arr.length;
  let maxVal = 0;
  for (let i = 0; i < n; i++) {
    if (arr[i] > maxVal) {
      maxVal = arr[i];
    }
  }
  let count = new Array(maxVal + 1).fill(0);
  for (let i = 0; i < n; i++) {
    count[arr[i]]++;
  }
  let k = 0;
  for (let v = 0; v <= maxVal; v++) {
    while (count[v] > 0) {
      arr[k] = v;
      k++;
      count[v]--;
    }
  }
}`;

const RADIX_SORT_CODE = `function radixSort(arr) {
  let maxVal = 0;
  for (let i = 0; i < arr.length; i++) {
    if (arr[i] > maxVal) {
      maxVal = arr[i];
    }
  }
  for (let exp = 1; Math.floor(maxVal / exp) > 0; exp *= 10) {
    let output = new Array(arr.length).fill(0);
    let count = new Array(10).fill(0);
    for (let i = 0; i < arr.length; i++) {
      count[Math.floor(arr[i] / exp) % 10]++;
    }
    for (let i = 1; i < 10; i++) {
      count[i] += count[i - 1];
    }
    for (let i = arr.length - 1; i >= 0; i--) {
      let digit = Math.floor(arr[i] / exp) % 10;
      output[count[digit] - 1] = arr[i];
      count[digit]--;
    }
    for (let i = 0; i < arr.length; i++) {
      arr[i] = output[i];
    }
  }
}`;

const BUCKET_SORT_CODE = `function bucketSort(arr, bucketSize = 4) {
  let minVal = arr[0];
  let maxVal = arr[0];
  for (let i = 1; i < arr.length; i++) {
    if (arr[i] < minVal) {
      minVal = arr[i];
    }
    if (arr[i] > maxVal) {
      maxVal = arr[i];
    }
  }
  let bucketCount = Math.floor((maxVal - minVal) / bucketSize) + 1;
  let buckets = new Array(bucketCount);
  for (let i = 0; i < bucketCount; i++) {
    buckets[i] = [];
  }
  for (let i = 0; i < arr.length; i++) {
    let bi = Math.floor((arr[i] - minVal) / bucketSize);
    buckets[bi].push(arr[i]);
  }
  for (let i = 0; i < bucketCount; i++) {
    buckets[i].sort((a, b) => a - b);
  }
  let idx = 0;
  for (let i = 0; i < bucketCount; i++) {
    for (let j = 0; j < buckets[i].length; j++) {
      arr[idx] = buckets[i][j];
      idx++;
    }
  }
}`;

/**
 * Bảng starter template theo key chuẩn hoá. Nếu một bài không có template riêng
 * (engine chỉ instrument so sánh `arr[i] op arr[j]` + gán mảng) thì rơi về
 * DEFAULT_SOURCE_CODE (bubble sort) + input phù hợp bài.
 */
const ARRAY_ALGO_TEMPLATES: Record<string, { code: string; array: number[] }> = {
  'bubble-sort': { code: DEFAULT_SOURCE_CODE, array: [...DEFAULT_INPUT_ARRAY] },
  'selection-sort': { code: SELECTION_SORT_CODE, array: [5, 3, 8, 1, 9, 2, 7] },
  'insertion-sort': { code: INSERTION_SORT_CODE, array: [5, 3, 8, 1, 9, 2, 7] },
  'quick-sort': { code: QUICK_SORT_CODE, array: [5, 3, 8, 1, 9, 2, 7] },
  'merge-sort': { code: MERGE_SORT_CODE, array: [5, 3, 8, 1, 9, 2, 7, 4] },
  'heap-sort': { code: HEAP_SORT_CODE, array: [5, 3, 8, 1, 9, 2, 7] },
  'radix-sort': { code: RADIX_SORT_CODE, array: [5, 3, 8, 1, 9, 2, 7] },
  'counting-sort': { code: COUNTING_SORT_CODE, array: [5, 3, 8, 1, 9, 2, 7] },
  'bucket-sort': { code: BUCKET_SORT_CODE, array: [5, 3, 8, 1, 9, 2, 7] },
  'linear-search': { code: LINEAR_SEARCH_CODE, array: [12, 5, 42, 8, 7, 23] },
  'binary-search': { code: BINARY_SEARCH_CODE, array: [2, 5, 8, 12, 16, 23, 38, 56, 72, 91] },
  'sliding-window': { code: SLIDING_WINDOW_CODE, array: [1, 4, 2, 10, 2, 3, 1, 0] },
};

/**
 * Lấy template (code + mảng đầu vào) cho một key thuật toán mảng.
 * `sampleInput` từ lesson (nếu hợp lệ) sẽ ưu tiên hơn mảng mặc định.
 */
export function getCodeVizTemplate(
  algorithmKey: string | null,
  sampleInput?: string | null,
): CodeVizTemplate {
  const key = normalizeArrayAlgorithmKey(algorithmKey);
  const fallback: CodeVizTemplate = {
    code: DEFAULT_SOURCE_CODE,
    array: [...DEFAULT_INPUT_ARRAY],
  };
  if (!key) return fallback;
  const tpl = ARRAY_ALGO_TEMPLATES[key];
  if (!tpl) return fallback;
  const parsed = parseNumberArray(sampleInput);
  return {
    code: tpl.code,
    array: parsed && parsed.length >= 2 ? parsed : [...tpl.array],
  };
}
