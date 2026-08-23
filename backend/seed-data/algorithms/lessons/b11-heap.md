# 11. Đống nhị phân (Heap) & Heapify

Bạn đã gặp cây và sắp xếp. Giờ đến một cấu trúc "lai" giữa hai thứ đó: **đống nhị phân (binary heap)** — một cây gần đầy đủ được dùng làm **hàng đợi ưu tiên (priority queue)**, nền tảng cho Dijkstra, Heap Sort, và vô số hệ thống lập lịch.

## 11.1. Đống nhị phân là gì?

Đống nhị phân là một **cây nhị phân gần đầy đủ** (mọi mức đều đầy trừ mức cuối, các node mức cuối nằm bên trái) thỏa **tính chất đống**:
- **Max-heap:** mỗi node ≥ cả hai con → **phần tử lớn nhất luôn ở gốc**.
- **Min-heap:** mỗi node ≤ cả hai con → **phần tử nhỏ nhất luôn ở gốc**.

Vì gần đầy đủ, heap được lưu gọn trong **mảng**: node tại chỉ số `i` có con trái tại `2i+1`, con phải tại `2i+2`, cha tại `floor((i-1)/2)`.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes heapGlow { 0%,100% { box-shadow: 0 0 0 rgba(34,197,94,0); } 50% { box-shadow: 0 0 14px rgba(34,197,94,0.45); } }
    .heap-stage { padding: 18px; background: rgba(10,9,18,0.85); }
    .heap-tree { display: flex; justify-content: center; }
    .heap-tree .node { min-width: 38px; height: 32px; padding: 0 8px; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-weight: 800; color: #e2e8f0; background: rgba(148,163,184,0.2); border: 1px solid rgba(148,163,184,0.35); font-size: 14px; }
    .heap-tree .node.root { background: rgba(34,197,94,0.25); border-color: #22c55e; color: #4ade80; animation: heapGlow 2.2s infinite; }
    .heap-tree .node.child { background: rgba(168,85,247,0.2); border-color: rgba(168,85,247,0.45); color: #c084fc; }
    .heap-tree .node.leaf { background: rgba(99,102,241,0.2); border-color: rgba(99,102,241,0.4); color: #a5b4fc; }
    .heap-tree .lvl { display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .heap-tree .kids { display: flex; gap: 20px; }
    .heap-arr { display: flex; gap: 4px; justify-content: center; padding-top: 14px; }
    .heap-arr .cell { width: 34px; height: 30px; border-radius: 5px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 12px; background: rgba(148,163,184,0.14); color: #e2e8f0; }
    .heap-arr .cell.i { background: rgba(34,197,94,0.15); color: #4ade80; border: 1px dashed rgba(34,197,94,0.5); }
  </style>
  <div class="heap-stage">
    <div class="heap-tree">
      <div class="lvl">
        <div class="node root">90</div>
        <div class="kids">
          <div class="lvl"><div class="node child">70</div><div class="kids"><div class="node leaf">45</div><div class="node leaf">40</div></div></div>
          <div class="lvl"><div class="node child">60</div><div class="kids"><div class="node leaf">30</div><div class="node leaf">25</div></div></div>
        </div>
      </div>
    </div>
    <div class="heap-arr">
      <div class="cell i">0</div><div class="cell i">1</div><div class="cell i">2</div><div class="cell i">3</div><div class="cell i">4</div><div class="cell i">5</div><div class="cell i">6</div>
      <div class="cell">90</div><div class="cell">70</div><div class="cell">60</div><div class="cell">45</div><div class="cell">40</div><div class="cell">30</div><div class="cell">25</div>
    </div>
    <div style="padding-top: 8px; font-size: 12px; color: #94a3b8; text-align: center;">Max-heap: gốc = 90 (lớn nhất). Lưu trữ tuần tự: con trái của 70 (chỉ số 1) là 45 (chỉ số 3), con phải là 40 (chỉ số 4).</div>
  </div>
</div>

## 11.2. Chèn (Insert) — "bubble up"

Chèn phần tử mới **vào cuối mảng**, rồi **đẩy ngược lên** (bubble up): so sánh với cha, nếu lớn hơn cha (max-heap) thì **đổi chỗ** cho tới khi đúng vị trí.

```javascript
// Min-heap: chèn x vào heap mảng
function heapInsert(heap, x) {
  heap.push(x);
  let i = heap.length - 1;
  while (i > 0) {
    const parent = Math.floor((i - 1) / 2);
    if (heap[parent] <= heap[i]) break;   // đúng vị trí
    [heap[parent], heap[i]] = [heap[i], heap[parent]]; // đổi chỗ với cha
    i = parent;
  }
}
```

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes upBubble { 0% { transform: translateY(0); background: #f59e0b; } 40% { transform: translateY(-6px); } 100% { transform: translateY(0); } }
    .bubble-viz { padding: 16px; background: rgba(10,9,18,0.85); }
    .bubble-viz .row { display: flex; gap: 6px; margin-bottom: 6px; }
    .bubble-viz .cell { flex: 1; height: 32px; border-radius: 5px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 13px; color: #e2e8f0; background: rgba(148,163,184,0.16); }
    .bubble-viz .cell.hot { background: #f59e0b; color: #000; animation: upBubble .6s ease; }
    .bubble-viz .lab { font-size: 11px; font-weight: 700; color: #94a3b8; }
  </style>
  <div class="bubble-viz">
    <div class="row"><div class="lab" style="width:70px">Chèn 5</div><div class="cell">2</div><div class="cell">6</div><div class="cell">8</div><div class="cell hot">5</div></div>
    <div class="row"><div class="lab" style="width:70px">Bubble</div><div class="cell">2</div><div class="cell hot">5</div><div class="cell">8</div><div class="cell">6</div></div>
    <div class="row"><div class="lab" style="width:70px">Xong</div><div class="cell hot">2</div><div class="cell">5</div><div class="cell">8</div><div class="cell">6</div></div>
    <div style="padding-top: 4px; font-size: 12px; color: #94a3b8;">Min-heap: chèn 5 vào cuối (vị trí 3), đổi chỗ lên với cha 6 → heap hợp lệ. Độ phức tạp O(log n).</div>
  </div>
</div>

## 11.3. Trích xuất phần tử gốc (Extract) — "sift down"

Lấy phần tử gốc (lớn nhất/nhỏ nhất), thay gốc bằng **phần tử cuối**, rồi **đẩy xuống** (sift down): so sánh với con, đổi chỗ với con nhỏ hơn (min-heap) cho tới khi đúng vị trí.

```javascript
// Min-heap: lấy min và giữ nguyên tính chất đống
function heapExtract(heap) {
  const min = heap[0];
  const last = heap.pop();
  if (heap.length > 0) {
    heap[0] = last;
    let i = 0;
    while (true) {
      const l = 2 * i + 1, r = 2 * i + 2;
      let smallest = i;
      if (l < heap.length && heap[l] < heap[smallest]) smallest = l;
      if (r < heap.length && heap[r] < heap[smallest]) smallest = r;
      if (smallest === i) break;
      [heap[i], heap[smallest]] = [heap[smallest], heap[i]];
      i = smallest;
    }
  }
  return min;
}
```

## 11.4. Heapify — dựng heap từ mảng trong O(n)

Thay vì chèn từng phần tử O(n log n), **heapify** dựng heap từ mảng bất kỳ chỉ trong **O(n)**: duyệt từ giữa mảng ngược về đầu, mỗi node **sift down** xuống vị trí đúng.

```javascript
function heapify(arr) {
  const a = [...arr];
  const n = a.length;
  for (let i = Math.floor(n / 2) - 1; i >= 0; i--) {
    // sift down tại i
    let k = i;
    while (true) {
      const l = 2 * k + 1, r = 2 * k + 2;
      let smallest = k;
      if (l < n && a[l] < a[smallest]) smallest = l;
      if (r < n && a[r] < a[smallest]) smallest = r;
      if (smallest === k) break;
      [a[k], a[smallest]] = [a[smallest], a[k]];
      k = smallest;
    }
  }
  return a;
}
```

> 💡 **Vì sao heapify nhanh?** Vì hầu hết node (lá) **không cần di chuyển** — chúng ở đáy cây. Chỉ các node gần gốc mới phải sift xa, và số đó rất ít. Tổng công việc tuyến tính theo n → **O(n)**, không phải O(n log n).

<div style="margin: 22px 0;">
  <style>
    @keyframes heapifyIn { 0% { opacity: 0; transform: scale(.9); } 100% { opacity: 1; transform: scale(1); } }
    .heapify-bars { display: flex; align-items: flex-end; gap: 5px; padding: 14px; border-radius: 12px; background: rgba(13,12,22,0.6); border: 1px solid rgba(255,255,255,0.08); }
    .heapify-bars .h { flex: 1; border-radius: 4px 4px 0 0; display: flex; align-items: flex-end; justify-content: center; padding-bottom: 3px; color: #fff; font-size: 11px; font-weight: 800; animation: heapifyIn .4s ease both; }
  </style>
  <div class="heapify-bars">
    <div class="h" style="height:90px; background:linear-gradient(180deg,#22c55e,#15803d); animation-delay:.05s">9</div>
    <div class="h" style="height:70px; background:linear-gradient(180deg,#4ade80,#16a34a); animation-delay:.1s">7</div>
    <div class="h" style="height:100px; background:linear-gradient(180deg,#a855f7,#7c3aed); animation-delay:.15s">10</div>
    <div class="h" style="height:50px; background:linear-gradient(180deg,#94a3b8,#64748b); animation-delay:.2s">5</div>
    <div class="h" style="height:40px; background:linear-gradient(180deg,#94a3b8,#64748b); animation-delay:.25s">3</div>
    <div class="h" style="height:30px; background:linear-gradient(180deg,#94a3b8,#64748b); animation-delay:.3s">1</div>
    <div class="h" style="height:80px; background:linear-gradient(180deg,#f59e0b,#d97706); animation-delay:.35s">8</div>
  </div>
  <div style="text-align:center; padding-top:8px; font-size:12px; color:#94a3b8;">Sau heapify → max-heap hợp lệ: 10, 7, 9, 5, 3, 1, 8 (gốc = 10 là lớn nhất).</div>
</div>

## 11.5. Độ phức tạp tổng kết

| Thao tác | Big-O |
|---|---|
| Peek (xem gốc) | O(1) |
| Insert (bubble up) | O(log n) |
| Extract (sift down) | O(log n) |
| Heapify (dựng từ mảng) | **O(n)** |
| Bộ nhớ | O(n) |

## 11.6. Tóm tắt

- Heap = cây nhị phân gần đầy đủ + tính chất **max/min heap** → gốc luôn là phần tử cực trị.
- Lưu trữ gọn trong **mảng**: con trái `2i+1`, con phải `2i+2`, cha `(i-1)/2`.
- **Insert** = push + bubble up (O(log n)); **Extract** = thay gốc + sift down (O(log n)).
- **Heapify** dựng heap O(n) — nhanh hơn chèn từng phần tử.
- Nền tảng cho Heap Sort và Priority Queue (bài 12, bài 14).

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Mảng <code style="color:#a7f3d0">[10, 7, 8, 5]</code> là max-heap hợp lệ không? Gốc 10 ≥ 7, 8; 7 ≥ 5 ✅ → hợp lệ. Thêm 12 vào: đặt cuối, bubble up lên gốc → [12,10,8,5,7]. Bạn kiểm tra lại nhé.
</div>