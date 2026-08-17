# 8. Insertion Sort

Bài trước bạn gặp Bubble Sort và Selection Sort — cả hai đều O(n²). Bài này giới thiệu **Insertion Sort**: cũng O(n²) trong worst case, nhưng **cực nhanh với dữ liệu gần như đã sắp xếp** (best case O(n)) và được dùng làm thuật toán "tinh chỉnh cuối" trong các thư viện sắp xếp thực tế.

## 8.1. Ý tưởng: "Xếp bài như người chơi bài"

Hãy nghĩ cách bạn **xếp bài tây trên tay**: bạn giữ phía tay trái đã được sắp xếp. Lấy lá bài mới, bạn **so sánh và dịch chuyển** những lá lớn hơn sang phải, rồi **chèn** lá mới vào đúng chỗ trống. Insertion Sort làm đúng vậy trên mảng.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes cardSlide { 0% { transform: translateX(0); } 100% { transform: translateX(0); } }
    @keyframes cardNew { 0% { transform: translateY(-8px); opacity: 0; } 100% { transform: translateY(0); opacity: 1; } }
    .ins-stage { padding: 18px; background: rgba(10,9,18,0.85); }
    .ins-row { display: flex; align-items: center; gap: 5px; margin-bottom: 8px; }
    .ins-row .cells { display: flex; gap: 4px; flex: 1; }
    .ins-row .cell { flex: 1; height: 36px; border-radius: 5px; display: flex; align-items: center; justify-content: center; font-weight: 800; color: #e2e8f0; background: rgba(148,163,184,0.16); font-size: 13px; }
    .ins-row .cell.sorted { background: rgba(34,197,94,0.18); color: #4ade80; border: 1px solid rgba(34,197,94,0.35); }
    .ins-row .cell.insert { background: #a855f7; color: #fff; animation: cardNew .4s ease both; box-shadow: 0 0 14px rgba(168,85,247,0.5); }
    .ins-row .cell.shift { background: rgba(245,158,11,0.2); color: #fbbf24; }
    .ins-row .label { width: 78px; font-size: 11px; font-weight: 700; color: #94a3b8; }
  </style>
  <div class="ins-stage">
    <div class="ins-row"><div class="label">Bắt đầu</div><div class="cells"><div class="cell sorted">5</div><div class="cell insert">3</div><div class="cell">8</div><div class="cell">1</div></div><span style="color:#a855f7;font-size:11px;font-weight:800">chèn 3</span></div>
    <div class="ins-row"><div class="label">Dịch</div><div class="cells"><div class="cell shift">5</div><div class="cell shift">5</div><div class="cell">8</div><div class="cell">1</div></div><span style="color:#fbbf24;font-size:11px;font-weight:800">5 dịch phải</span></div>
    <div class="ins-row"><div class="label">Chèn</div><div class="cells"><div class="cell insert">3</div><div class="cell sorted">5</div><div class="cell">8</div><div class="cell">1</div></div><span style="color:#a855f7;font-size:11px;font-weight:800">3 vào vị trí</span></div>
    <div class="ins-row"><div class="label">Tiếp</div><div class="cells"><div class="cell sorted">3</div><div class="cell sorted">5</div><div class="cell insert">8</div><div class="cell">1</div></div><span style="color:#a855f7;font-size:11px;font-weight:800">chèn 8</span></div>
    <div style="padding-top: 6px; font-size: 12px; color: #94a3b8;">Vùng xanh = đã sắp xếp. Lá bài mới (tím) được dịch chuyển các lá lớn hơn rồi chèn vào đúng chỗ.</div>
  </div>
</div>

## 8.2. Code JavaScript

```javascript
function insertionSort(arr) {
  const a = [...arr];
  for (let i = 1; i < a.length; i++) {
    const key = a[i];          // lá bài đang cầm
    let j = i - 1;
    while (j >= 0 && a[j] > key) {
      a[j + 1] = a[j];         // dịch lá lớn hơn sang phải
      j--;
    }
    a[j + 1] = key;            // chèn vào đúng chỗ
  }
  return a;
}

console.log(insertionSort([5, 3, 8, 1])); // [1, 3, 5, 8]
```

## 8.3. Độ phức tạp

| Trường hợp | Dữ liệu | Big-O |
|---|---|---|
| **Best** | Mảng đã sắp xếp | **O(n)** — chỉ duyệt, không dịch |
| Average | Ngẫu nhiên | O(n²) |
| Worst | Mảng giảm dần | O(n²) — dịch hết mọi lá |
| Bộ nhớ phụ | — | O(1) — sắp tại chỗ |

Điểm đặc biệt: với dữ liệu **gần như đã sắp xếp** (chỉ vài phần tử lệch chỗ), Insertion Sort cực nhanh vì hầu như không cần dịch chuyển.

> 💡 **Ứng dụng thực tế:** Các thư viện sắp xếp hiện đại (như của JavaScript, Java, Python) **không dùng một thuật toán duy nhất**. Chúng thường dùng Quick/Merge cho dữ liệu lớn, nhưng **chuyển sang Insertion Sort** cho các mảng con nhỏ (n < ~20) vì phần chi phí cố định thấp. Đây gọi là **hybrid sort**.

## 8.4. So sánh tổng kết 3 thuật toán sắp xếp cơ bản

| Thuật toán | Best | Average | Worst | Bộ nhớ | Ổn định | Đặc điểm |
|---|---|---|---|---|---|---|
| Bubble | O(n) | O(n²) | O(n²) | O(1) | ✅ | Dễ hiểu nhất |
| Selection | O(n²) | O(n²) | O(n²) | O(1) | ❌ | Đổi chỗ ít |
| Insertion | **O(n)** | O(n²) | O(n²) | O(1) | ✅ | Nhanh với dữ liệu gần sắp xếp |

## 8.5. Tóm tắt

- Insertion Sort mô phỏng cách **xếp bài tây**: dịch chuyển rồi chèn.
- **O(n)** khi dữ liệu gần như đã sắp xếp — best case tốt nhất trong nhóm cơ bản.
- Được dùng làm thuật toán tinh chỉnh trong các thư viện sắp xếp thực tế.
- Bắt đầu từ Module 5, bạn sẽ học thuật toán **O(n log n)** nhanh hơn hẳn.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Mảng <code style="color:#a7f3d0">[6, 2, 4]</code>. Mô phỏng Insertion Sort: chèn 2 (dịch 6) → [2,6,4]; chèn 4 (dịch 6) → [2,4,6]. Số phép dịch chuyển là bao nhiêu? Đáp án: 2 lần dịch.
</div>