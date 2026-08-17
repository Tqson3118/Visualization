# 6. Tìm kiếm nhị phân (Binary Search)

Ở bài trước, tìm kiếm tuyến tính duyệt từng phần tử — chậm khi danh sách lớn. Bài này giới thiệu **tìm kiếm nhị phân (binary search)**: một thuật toán thông minh đạt **O(log n)** — cực nhanh, nhưng **yêu cầu danh sách đã sắp xếp tăng dần**. Đây là một trong những thuật toán quan trọng nhất bạn cần thuộc lòng.

## 6.1. Ý tưởng: "Chia đôi để tiến"

Hãy nghĩ lại cách bạn tra từ điển: không lật từ trang 1, mà mở **giữa quyển**, thấy chữ mình cần nằm **sau** thì lật về phía sau, nằm **trước** thì lật về phía trước. Mỗi lần lật, bạn **loại bỏ một nửa** số trang còn lại. Đó chính xác là binary search.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes midPulse { 0%,100% { box-shadow: 0 0 0 rgba(250,204,21,0); } 50% { box-shadow: 0 0 16px rgba(250,204,21,0.5); } }
    @keyframes shrink { 0% { opacity: 0; } 100% { opacity: 1; } }
    .bs-steps { padding: 18px; background: rgba(10,9,18,0.85); display: flex; flex-direction: column; gap: 14px; }
    .bs-row { display: flex; gap: 4px; }
    .bs-row .cell { flex: 1; height: 36px; border-radius: 5px; display: flex; align-items: center; justify-content: center; font-size: 12px; font-weight: 800; color: #64748b; background: rgba(100,116,139,0.1); animation: shrink .3s ease both; }
    .bs-row .cell.live { color: #e2e8f0; background: rgba(148,163,184,0.18); }
    .bs-row .cell.mid { color: #000; background: #facc15; animation: midPulse 1.6s infinite; }
    .bs-row .cell.found { color: #052e16; background: #4ade80; }
    .bs-row .label { width: 76px; font-size: 11px; font-weight: 700; color: #94a3b8; display: flex; align-items: center; }
  </style>
  <div class="bs-steps">
    <div class="bs-row"><div class="label">Bước 1</div><div class="cell live">1</div><div class="cell live">3</div><div class="cell live">5</div><div class="cell mid">7</div><div class="cell live">9</div><div class="cell live">11</div><div class="cell live">13</div></div>
    <div class="bs-row"><div class="label">Bước 2</div><div class="cell live">1</div><div class="cell live">3</div><div class="cell live">5</div><div class="cell found">7</div><div class="cell">9</div><div class="cell">11</div><div class="cell">13</div></div>
    <div style="font-size: 12px; color: #94a3b8;">Tìm số <b style="color:#4ade80">7</b>. Bước 1: mid = 7, đúng bằng target → dừng ngay. Tổng cộng chỉ <b>2 bước</b>, không cần duyệt hết mảng.</div>
  </div>
</div>

## 6.2. Code JavaScript

```javascript
function binarySearch(arr, target) {
  let lo = 0;
  let hi = arr.length - 1;

  while (lo <= hi) {
    const mid = Math.floor((lo + hi) / 2);   // phần tử giữa
    if (arr[mid] === target) return mid;     // tìm thấy
    else if (arr[mid] < target) lo = mid + 1; // nằm nửa phải
    else hi = mid - 1;                        // nằm nửa trái
  }
  return -1;                                   // không tồn tại
}

console.log(binarySearch([1, 3, 5, 7, 9, 11, 13], 7)); // 3
console.log(binarySearch([1, 3, 5, 7, 9, 11, 13], 2)); // -1
```

## 6.3. Độ phức tạp — vì sao lại nhanh vậy?

Mỗi bước, binary search **loại bỏ một nửa** dữ liệu. Số bước tối đa = số lần chia đôi từ n xuống 1 = **log₂(n)**:

| Kích thước n | Số bước tối đa (log₂ n) |
|---|---|
| 10 | ~4 |
| 1.000 | ~10 |
| 1.000.000 | ~20 |
| 1.000.000.000 | ~30 |

> **Kết luận:** với **1 tỷ phần tử**, binary search chỉ cần **30 phép so sánh** — trong khi linear search phải mất tới 1 tỷ! Đó là sức mạnh của O(log n).

<div style="margin: 22px 0;">
  <style>
    @keyframes growthDots { 0% { transform: translateY(0); } 50% { transform: translateY(-4px); } 100% { transform: translateY(0); } }
    .scale-viz { display: flex; align-items: flex-end; gap: 16px; padding: 18px; border-radius: 14px; background: rgba(13,12,22,0.6); border: 1px solid rgba(255,255,255,0.08); justify-content: center; }
    .scale-col { display: flex; flex-direction: column; align-items: center; gap: 6px; }
    .scale-dots { display: flex; gap: 3px; }
    .dot { width: 8px; height: 8px; border-radius: 50%; background: #a855f7; animation: growthDots 1.2s ease-in-out infinite; }
    .dot.big { width: 10px; height: 10px; }
    .scale-label { font-size: 11px; color: #94a3b8; font-weight: 700; }
  </style>
  <div class="scale-viz">
    <div class="scale-col"><div class="scale-dots">1 3 5 7 9 11 13 15 17 19</div><div class="scale-label">n = 10 · ~4 bước</div></div>
    <div class="scale-col"><div class="scale-dots">1 3 5 7 9 11 13 15 17 19 · 21 … 1000</div><div class="scale-label">n = 1.000 · ~10 bước</div></div>
  </div>
</div>

## 6.4. Điều kiện tiên quyết: mảng phải ĐƯỢC SẮP XẾP

Đây là điểm tối quan trọng: binary search **chỉ đúng khi mảng đã sắp xếp tăng dần**. Nếu mảng chưa sắp xếp, kết quả sẽ sai bất thường — vì thuật toán tin tưởng "phần tử giữa" phân chia đúng hai nửa nhỏ/lớn.

> ⚠️ **Lỗi phổ biến khi viết binary search:**
> 1. **Off-by-one:** quên điều kiện `lo <= hi` (dùng `<` sẽ bỏ sót phần tử cuối).
> 2. **Infinite loop:** cập nhật `lo = mid` thay vì `lo = mid + 1` khiến vòng lặp không bao giờ kết thúc với 2 phần tử.
> 3. **Tràn số:** với ngôn ngữ có giới hạn số nguyên lớn, nên dùng `lo + (hi - lo) / 2` thay vì `(lo + hi) / 2`.

## 6.5. So sánh nhanh hai thuật toán tìm kiếm

| Tiêu chí | Linear Search | Binary Search |
|---|---|---|
| Độ phức tạp | O(n) | **O(log n)** |
| Cần mảng đã sắp xếp | Không | **Có** |
| Dữ liệu chưa sắp xếp | ✅ dùng được | ❌ sai kết quả |
| Dữ liệu lớn, tìm nhiều lần | ❌ chậm | ✅ rất nhanh |

## 6.6. Tóm tắt

- Binary search **chia đôi dữ liệu mỗi bước** → O(log n).
- Yêu cầu mảng **đã sắp xếp** — mua dữ liệu sạch trước khi dùng.
- 3 lỗi hay gặp: off-by-one, infinite loop, tràn số.
- Cực kỳ phổ biến trong phỏng vấn và hệ thống lớn (tra cứu).

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Mảng <code style="color:#a7f3d0">[1, 4, 7, 10, 15, 20, 25]</code>, tìm <code style="color:#a7f3d0">15</code> bằng binary search. Liệt kê các mid bạn ghé qua. Đáp án: mid=7 (index 3, value 10, lo=4) → mid=15 (index 5, value 20, lo=4, hi=4) → mid=15 (index 4) → tìm thấy sau 3 bước.
</div>