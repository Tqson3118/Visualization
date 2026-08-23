# 12. Heap Sort & ứng dụng Top-K

Bài 11 bạn đã có heap với insert/extract/heapify. Bài này áp dụng heap vào hai bài toán thực tế quan trọng: **Heap Sort** (sắp xếp O(n log n), tại chỗ) và **Top-K** (tìm K phần tử lớn/nhỏ nhất hiệu quả) — câu hỏi phỏng vấn rất phổ biến.

## 12.1. Heap Sort — "extract liên tục"

Ý tưởng Heap Sort cực kỳ đơn giản: **dựng max-heap**, rồi lặp lại n lần: lấy gốc (phần tử lớn nhất còn lại), đưa ra cuối, **sift down** phần tử cuối lên gốc để duy trì heap với phần còn lại.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes extractPop { 0% { transform: scale(1); background: #f59e0b; } 60% { transform: scale(1.12); } 100% { transform: scale(1); } }
    .hs-stage { padding: 18px; background: rgba(10,9,18,0.85); }
    .hs-row { display: flex; align-items: center; gap: 6px; margin-bottom: 8px; }
    .hs-row .lab { width: 70px; font-size: 11px; font-weight: 800; color: #94a3b8; }
    .hs-row .cell { width: 34px; height: 32px; border-radius: 5px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 13px; background: rgba(148,163,184,0.16); color: #e2e8f0; }
    .hs-row .cell.root { background: #f59e0b; color: #000; animation: extractPop 1.4s infinite; }
    .hs-row .cell.done { background: rgba(34,197,94,0.18); color: #4ade80; border: 1px solid rgba(34,197,94,0.35); }
  </style>
  <div class="hs-stage">
    <div class="hs-row"><div class="lab">Heap</div><div class="cell root">9</div><div class="cell">6</div><div class="cell">8</div><div class="cell">1</div><div class="cell">4</div><div class="cell">3</div></div>
    <div class="hs-row"><div class="lab">Lấy 9</div><div class="cell">6</div><div class="cell">3</div><div class="cell">8</div><div class="cell">1</div><div class="cell">4</div><div class="cell done">9</div></div>
    <div class="hs-row"><div class="lab">Lấy 8</div><div class="cell">4</div><div class="cell">3</div><div class="cell">6</div><div class="cell">1</div><div class="cell done">8</div><div class="cell done">9</div></div>
    <div class="hs-row"><div class="lab">Cuối</div><div class="cell done">1</div><div class="cell done">3</div><div class="cell done">4</div><div class="cell done">6</div><div class="cell done">8</div><div class="cell done">9</div></div>
    <div style="padding-top: 4px; font-size: 12px; color: #94a3b8;">Mỗi lượt: đưa gốc (vàng) ra cuối vùng đã sắp xếp (xanh), sift down phần còn lại.</div>
  </div>
</div>

```javascript
function heapSort(arr) {
  const a = [...arr];
  const n = a.length;

  // Bước 1: heapify — dựng max-heap
  for (let i = Math.floor(n / 2) - 1; i >= 0; i--) siftDown(a, i, n);

  // Bước 2: extract gốc n lần
  for (let end = n - 1; end > 0; end--) {
    [a[0], a[end]] = [a[end], a[0]]; // gốc max → cuối
    siftDown(a, 0, end);             // phục hồi heap với phần còn lại
  }
  return a;
}

function siftDown(a, i, size) {
  while (true) {
    const l = 2 * i + 1, r = 2 * i + 2;
    let largest = i;
    if (l < size && a[l] > a[largest]) largest = l;
    if (r < size && a[r] > a[largest]) largest = r;
    if (largest === i) break;
    [a[i], a[largest]] = [a[largest], a[i]];
    i = largest;
  }
}

console.log(heapSort([5, 3, 8, 1, 9, 2, 7, 4])); // [1,2,3,4,5,7,8,9]
```

## 12.2. Độ phức tạp Heap Sort

| Trường hợp | Big-O |
|---|---|
| Best / Average / Worst | **O(n log n)** — không có trường hợp xấu đặc biệt như Quick Sort |
| Bộ nhớ | **O(1)** — sắp tại chỗ |
| Ổn định | ❌ (đổi chỗ gốc/cuối làm mất thứ tự tương đối) |

> 💡 **So sánh nhanh:** Merge Sort luôn O(n log n) nhưng tốn O(n) bộ nhớ. Heap Sort cũng O(n log n) nhưng **O(1) bộ nhớ**. Tuy nhiên trong thực tế, Heap Sort chậm hơn Quick Sort do **bộ nhớ không cache-friendly** (nhảy cóc giữa các vị trí xa nhau) — nên hầu hết thư viện chọn Quick/Merge, dùng Heap Sort khi cần giới hạn bộ nhớ nghiêm ngặt.

## 12.3. Bài toán Top-K — vì sao heap là "vũ khí bí mật"

**Bài toán:** Cho mảng có n phần tử, tìm **K phần tử LỚN NHẤT** (hoặc nhỏ nhất). Ví dụ: tìm 3 sản phẩm bán chạy nhất trong 1 triệu sản phẩm.

Cách "ngây thơ": sắp xếp toàn bộ rồi lấy K phần tử đầu → **O(n log n)**. Nhưng thật lãng phí — bạn chỉ cần K phần tử!

**Cách dùng heap — O(n log K):**
- Duy trì một **min-heap kích thước K** (đối với top-K lớn nhất).
- Với mỗi phần tử x: nếu heap chưa đủ K → push. Ngược lại, nếu x > đỉnh heap (nhỏ nhất trong nhóm hiện tại) → **extract min rồi push x**.
- Kết thúc: heap chứa đúng K phần tử lớn nhất.

```javascript
function topKLargest(arr, k) {
  const heap = [];  // min-heap kích thước k
  for (const x of arr) {
    if (heap.length < k) {
      heapInsert(heap, x);
    } else if (x > heap[0]) {
      heapExtract(heap);   // bỏ phần tử nhỏ nhất hiện tại
      heapInsert(heap, x); // chèn phần tử mới
    }
  }
  return heap.sort((a, b) => b - a);
}
// tái sử dụng heapInsert / heapExtract (min-heap) của bài 11
```

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes topkIn { 0% { transform: translateY(-5px); opacity: 0; } 100% { transform: translateY(0); opacity: 1; } }
    .topk-stage { padding: 16px; background: rgba(10,9,18,0.85); }
    .topk-line { display: flex; align-items: center; gap: 6px; margin-bottom: 7px; animation: topkIn .4s ease both; }
    .topk-line .stream { font-size: 12px; color: #94a3b8; width: 120px; font-weight: 700; }
    .topk-line .pile { display: flex; gap: 4px; }
    .topk-line .cell { width: 30px; height: 28px; border-radius: 5px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 12px; background: rgba(168,85,247,0.22); color: #e9d5ff; }
    .topk-line .cell.out { background: rgba(239,68,68,0.2); color: #f87171; text-decoration: line-through; }
    .topk-line .cell.in { background: rgba(34,197,94,0.25); color: #4ade80; }
  </style>
  <div class="topk-stage">
    <div class="topk-line"><div class="stream">Đọc 5</div><div class="pile"><div class="cell">5</div></div></div>
    <div class="topk-line"><div class="stream">Đọc 1</div><div class="pile"><div class="cell">1</div><div class="cell">5</div></div></div>
    <div class="topk-line"><div class="stream">Đọc 9</div><div class="pile"><div class="cell out">1</div><div class="cell in">9</div><div class="cell">5</div></div></div>
    <div class="topk-line"><div class="stream">Đọc 3</div><div class="pile"><div class="cell">3</div><div class="cell">5</div><div class="cell">9</div></div></div>
    <div class="topk-line"><div class="stream">Đọc 8</div><div class="pile"><div class="cell out">3</div><div class="cell in">8</div><div class="cell">5</div><div class="cell">9</div></div></div>
    <div style="padding-top: 6px; font-size: 12px; color: #94a3b8;">Top-3 lớn nhất trong [5,1,9,3,8,...] = heap [5,8,9] — chỉ giữ 3 phần tử, đỉnh heap luôn là "người nhỏ nhất trong top" để so sánh.</div>
  </div>
</div>

## 12.4. Vì sao O(n log K) lại quan trọng?

Với n = 1 triệu và K = 3:
- **Cách sắp xếp toàn bộ:** 1.000.000 × 20 ≈ **20 triệu phép toán**.
- **Dùng heap:** 1.000.000 × log(3) ≈ 1.000.000 × 2 ≈ **2 triệu phép toán**.

Đặc biệt quan trọng khi K **rất nhỏ** so với n, hoặc khi dữ liệu **đến liên tục** (streaming) — không thể lưu hết để sắp xếp. Đây chính là lý do priority queue xuất hiện trong Dijkstra (bài 14): cần lấy nhanh "đỉnh nhỏ nhất" giữa các ứng viên.

## 12.5. Tóm tắt

- **Heap Sort**: heapify + extract liên tục → **O(n log n)**, bộ nhớ O(1).
- **Top-K**: duy trì **min-heap kích thước K** → **O(n log K)**, tối ưu cho K nhỏ và dữ liệu streaming.
- Heap là nền tảng của **hàng đợi ưu tiên** — sẽ dùng trong Dijkstra ở bài 14.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Cho mảng <code style="color:#a7f3d0">[10, 3, 9, 7, 12, 5]</code>, tìm Top-2 lớn nhất bằng min-heap kích thước 2. Kết quả sau mỗi phần tử: [10] → [3,10] → [9,10] → [9,10] → [10,12] → [10,12]. Đáp án: 12 và 10.
</div>