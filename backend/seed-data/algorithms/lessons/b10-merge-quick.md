# 10. Merge Sort & Quick Sort

Đã đến lúc thoát khỏi O(n²). Bài này giới thiệu **hai thuật toán sắp xếp O(n log n)** dựa trên tư duy **chia để trị (Divide & Conquer)** đã học ở bài 9: Merge Sort và Quick Sort. Đây là nền tảng mà hầu hết thư viện sắp xếp hiện đại xây dựng trên đó.

## 10.1. Chia để trị là gì?

Chia để trị có 3 bước:
1. **Chia (Divide):** tách bài toán thành các bài toán con nhỏ hơn, cùng dạng.
2. **Trị (Conquer):** giải từng bài toán con (thường đệ quy).
3. **Kết hợp (Combine):** ghép kết quả các bài toán con thành kết quả tổng.

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes divideIn { 0% { opacity: 0; transform: translateY(-4px); } 100% { opacity: 1; transform: translateY(0); } }
    .dc-tree { display: flex; justify-content: center; padding: 16px; background: rgba(10,9,18,0.85); }
    .dc { display: flex; flex-direction: column; align-items: center; gap: 6px; }
    .dc-row { display: flex; gap: 10px; }
    .dc-box { padding: 6px 14px; border-radius: 6px; font-size: 12px; font-weight: 800; animation: divideIn .5s ease both; }
    .dc.d1 .dc-box { background: rgba(168,85,247,0.35); color: #e9d5ff; }
    .dc.d2 .dc-box { background: rgba(99,102,241,0.3); color: #c7d2fe; }
    .dc.d3 .dc-box { background: rgba(34,197,94,0.25); color: #86efac; }
  </style>
  <div class="dc-tree">
    <div class="dc d1">
      <div class="dc-box">[5,3,8,1,9,2,7,4]</div>
      <div class="dc-row">
        <div class="dc d2"><div class="dc-box">[5,3,8,1]</div><div class="dc-row"><div class="dc d3"><div class="dc-box">[5,3]</div></div><div class="dc d3"><div class="dc-box">[8,1]</div></div></div></div>
        <div class="dc d2"><div class="dc-box">[9,2,7,4]</div><div class="dc-row"><div class="dc d3"><div class="dc-box">[9,2]</div></div><div class="dc d3"><div class="dc-box">[7,4]</div></div></div></div>
      </div>
    </div>
  </div>
  <div style="padding: 10px 16px; font-size: 12px; color: #94a3b8;">Chia đôi liên tục cho tới khi mỗi mảng con có 1 phần tử (hiển nhiên đã sắp xếp) → rồi trộn ngược lên.</div>
</div>

## 10.2. Merge Sort — "chia đôi, sắp từng nửa, trộn lại"

Merge Sort chia mảng làm đôi, sắp xếp đệ quy từng nửa, rồi **trộn hai nửa đã sắp xếp** thành một mảng sắp xếp. Bước trộn chính là điểm mấu chốt: so sánh hai đầu và ghi phần tử nhỏ hơn.

```javascript
function mergeSort(arr) {
  if (arr.length <= 1) return arr;             // base case: 1 phần tử

  const mid = Math.floor(arr.length / 2);
  const left = mergeSort(arr.slice(0, mid));   // CHIA + sắp nửa trái
  const right = mergeSort(arr.slice(mid));     // CHIA + sắp nửa phải

  return merge(left, right);                   // KẾT HỢP: trộn 2 nửa
}

function merge(a, b) {
  const result = [];
  let i = 0, j = 0;
  while (i < a.length && j < b.length) {
    result.push(a[i] <= b[j] ? a[i++] : b[j++]); // ghi phần tử nhỏ hơn
  }
  return result.concat(a.slice(i), b.slice(j));  // phần còn lại
}

console.log(mergeSort([5, 3, 8, 1, 9, 2, 7, 4])); // [1,2,3,4,5,7,8,9]
```

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes mergeGlow { 0%,100% { box-shadow: 0 0 0 rgba(34,197,94,0); } 50% { box-shadow: 0 0 12px rgba(34,197,94,0.4); } }
    .merge-stage { padding: 16px; background: rgba(10,9,18,0.85); }
    .merge-row { display: flex; align-items: center; gap: 8px; margin-bottom: 6px; }
    .merge-row .lab { width: 90px; font-size: 11px; font-weight: 800; color: #94a3b8; }
    .merge-row .cell { width: 30px; height: 30px; border-radius: 5px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 12px; background: rgba(99,102,241,0.25); color: #c7d2fe; }
    .merge-row .cell.out { background: rgba(34,197,94,0.2); color: #4ade80; animation: mergeGlow 2s infinite; }
  </style>
  <div class="merge-stage">
    <div class="merge-row"><div class="lab">Trái</div><div class="cell" style="background:rgba(168,85,247,0.3)">1</div><div class="cell" style="background:rgba(168,85,247,0.3)">3</div><div class="cell" style="background:rgba(168,85,247,0.3)">5</div><div class="cell" style="background:rgba(168,85,247,0.3)">8</div></div>
    <div class="merge-row"><div class="lab">Phải</div><div class="cell" style="background:rgba(99,102,241,0.3)">2</div><div class="cell" style="background:rgba(99,102,241,0.3)">4</div><div class="cell" style="background:rgba(99,102,241,0.3)">7</div><div class="cell" style="background:rgba(99,102,241,0.3)">9</div></div>
    <div class="merge-row"><div class="lab">Trộn</div><div class="cell out">1</div><div class="cell out">2</div><div class="cell out">3</div><div class="cell out">4</div><div class="cell out">5</div><div class="cell out">7</div><div class="cell out">8</div><div class="cell out">9</div></div>
    <div style="padding-top: 4px; font-size: 12px; color: #94a3b8;">Trộn 2 mảng con đã sắp xếp: mỗi bước lấy phần tử nhỏ nhất trong hai đầu con trỏ.</div>
  </div>
</div>

## 10.3. Quick Sort — "chọn pivot, phân hoạch, đệ quy"

Quick Sort chọn một phần tử **pivot** (mốc), **phân hoạch** mảng thành: phần nhỏ hơn pivot bên trái, phần lớn hơn bên phải (pivot nằm giữa), rồi đệ quy sắp xếp hai bên.

```javascript
function quickSort(arr) {
  if (arr.length <= 1) return arr;                     // base case

  const pivot = arr[0];                                // chọn pivot (đơn giản: phần tử đầu)
  const less = [], equal = [], greater = [];

  for (const x of arr) {
    if (x < pivot) less.push(x);
    else if (x === pivot) equal.push(x);
    else greater.push(x);
  }

  return [...quickSort(less), ...equal, ...quickSort(greater)]; // ghép kết quả
}

console.log(quickSort([5, 3, 8, 1, 9, 2, 7, 4])); // [1,2,3,4,5,7,8,9]
```

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes pivotRing { 0%,100% { box-shadow: 0 0 0 rgba(245,158,11,0); } 50% { box-shadow: 0 0 14px rgba(245,158,11,0.5); } }
    .quick-stage { padding: 16px; background: rgba(10,9,18,0.85); }
    .quick-row { display: flex; align-items: center; gap: 6px; margin-bottom: 8px; }
    .quick-row .cell { width: 30px; height: 30px; border-radius: 5px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 12px; background: rgba(148,163,184,0.16); color: #e2e8f0; }
    .quick-row .cell.pivot { background: #f59e0b; color: #000; animation: pivotRing 1.6s infinite; }
    .quick-row .cell.less { background: rgba(34,197,94,0.25); color: #4ade80; }
    .quick-row .cell.greater { background: rgba(239,68,68,0.25); color: #f87171; }
    .quick-row .lab { width: 76px; font-size: 11px; font-weight: 800; color: #94a3b8; }
  </style>
  <div class="quick-stage">
    <div class="quick-row"><div class="lab">Mảng</div><div class="cell pivot">5</div><div class="cell">3</div><div class="cell">8</div><div class="cell">1</div><div class="cell">9</div><div class="cell">2</div><div class="cell">7</div><div class="cell">4</div></div>
    <div class="quick-row"><div class="lab">&lt; pivot</div><div class="cell less">3</div><div class="cell less">1</div><div class="cell less">2</div><div class="cell less">4</div></div>
    <div class="quick-row"><div class="lab">pivot</div><div class="cell pivot">5</div></div>
    <div class="quick-row"><div class="lab">&gt; pivot</div><div class="cell greater">8</div><div class="cell greater">9</div><div class="cell greater">7</div></div>
    <div style="padding-top: 4px; font-size: 12px; color: #94a3b8;">Phân hoạch quanh pivot=5 → [1,2,3,4] + [5] + [7,8,9]. Đệ quy sắp xếp 2 bên → [1,2,3,4,5,7,8,9].</div>
  </div>
</div>

## 10.4. So sánh Merge Sort vs Quick Sort

| Tiêu chí | Merge Sort | Quick Sort |
|---|---|---|
| Best / Average | **O(n log n)** | **O(n log n)** |
| Worst | O(n log n) — luôn ổn định | **O(n²)** nếu chọn pivot xấu (mảng đã sắp xếp) |
| Bộ nhớ phụ | **O(n)** — cần mảng tạm khi trộn | **O(log n)** — chỉ đệ quy |
| Ổn định | ✅ | ❌ (bản đơn giản) |
| Ứng dụng | Java Arrays.sort (object), database | C/Python stdlib (pivot ngẫu nhiên) |

> 💡 **Mẹo tránh worst case Quick Sort:** không nên luôn chọn phần tử đầu làm pivot (mảng đã sắp xếp → O(n²)). Cách phổ biến: chọn **pivot ngẫu nhiên**, hoặc chọn **median của 3** (đầu-giữa-cuối).

## 10.5. Tóm tắt

- **Chia để trị**: chia → trị → kết hợp.
- **Merge Sort**: chia đôi + trộn 2 nửa đã sắp — **ổn định O(n log n)**, tốn O(n) bộ nhớ.
- **Quick Sort**: chọn pivot + phân hoạch — nhanh thực tế, tiết kiệm bộ nhớ, nhưng dễ O(n²) nếu pivot xấu.
- Cả hai là nền tảng của mọi thư viện sắp xếp hiện đại.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Với Merge Sort, mảng <code style="color:#a7f3d0">[4,2,7,1]</code>, sau khi chia thành <code style="color:#a7f3d0">[4,2]</code> và <code style="color:#a7f3d0">[7,1]</code>, trộn ngược lên cho kết quả gì? Đáp án: [2,4] + [1,7] → trộn → [1,2,4,7].
</div>