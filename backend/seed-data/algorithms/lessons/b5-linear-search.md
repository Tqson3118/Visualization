# 5. Tìm kiếm tuyến tính (Linear Search)

Bắt đầu hành trình vào các thuật toán cụ thể với bài toán phổ biến nhất: **tìm một phần tử trong danh sách**. Cách đầu tiên và tự nhiên nhất chính là **tìm kiếm tuyến tính** — bạn đã dùng nó từ nhỏ khi rà tìm đồ vật.

## 5.1. Ý tưởng

Tìm kiếm tuyến tính (linear search) duyệt **từng phần tử một, từ đầu đến cuối** danh sách, so sánh mỗi phần tử với giá trị cần tìm. Nếu thấy → trả về vị trí; duyệt hết mà không thấy → trả về -1 (không tồn tại).

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes scanX { 0% { left: 0; opacity: 0; } 10% { opacity: 1; } 90% { opacity: 1; } 100% { left: 100%; opacity: 0; } }
    @keyframes scanBlink { 0%,100% { opacity: .35; } 50% { opacity: 1; } }
    .scan-wrap { position: relative; padding: 28px 18px 20px; background: rgba(10,9,18,0.85); }
    .scan-box { position: relative; display: flex; gap: 6px; }
    .scan-box .el { flex: 1; height: 44px; border-radius: 6px; background: rgba(148,163,184,0.14); border: 1px solid rgba(148,163,184,0.3); display: flex; align-items: center; justify-content: center; font-weight: 800; color: #e2e8f0; font-size: 14px; position: relative; z-index: 1; }
    .scan-box .el.found { background: rgba(34,197,94,0.2); border-color: #22c55e; color: #4ade80; box-shadow: 0 0 14px rgba(34,197,94,0.35); }
    .scanner { position: absolute; top: 0; bottom: 0; width: 40px; border: 2px solid #a855f7; border-radius: 8px; box-shadow: 0 0 16px rgba(168,85,247,0.5); animation: scanX 3.2s linear infinite; }
    .scan-hint { position: absolute; top: -2px; left: 0; right: 0; text-align: center; color: #c084fc; font-size: 11px; font-weight: 700; animation: scanBlink 1s infinite; }
  </style>
  <div class="scan-wrap">
    <div class="scan-box">
      <div class="el">5</div>
      <div class="el">8</div>
      <div class="el">3</div>
      <div class="el found">7</div>
      <div class="el">9</div>
      <div class="el">2</div>
      <div class="scanner"></div>
    </div>
    <div style="padding-top: 12px; text-align: center; color: #94a3b8; font-size: 12px;">Máy quét rà lần lượt: 5 → 8 → 3 → <span style="color:#4ade80;font-weight:800">7 ✓</span> — tìm thấy ở vị trí thứ 4.</div>
  </div>
</div>

## 5.2. Code JavaScript

```javascript
function linearSearch(arr, target) {
  for (let i = 0; i < arr.length; i++) {
    if (arr[i] === target) return i;  // thấy → trả về vị trí
  }
  return -1;                           // duyệt hết, không thấy
}

console.log(linearSearch([5, 8, 3, 7, 9], 7)); // 3
console.log(linearSearch([5, 8, 3, 7, 9], 1)); // -1
```

## 5.3. Độ phức tạp

| Trường hợp | Số phép so sánh | Big-O |
|---|---|---|
| Best (phần tử ở đầu) | 1 | O(1) |
| Average | ~n/2 | O(n) |
| **Worst (ở cuối hoặc không có)** | n | **O(n)** |
| Bộ nhớ phụ | — | O(1) |

Điểm mạnh của linear search: **không cần mảng đã sắp xếp**, đơn giản, chạy trên mọi cấu trúc (mảng, danh sách liên kết). Điểm yếu: chậm với danh sách lớn — với 1 triệu phần tử phải duyệt tới 1 triệu lần trong worst case.

> 💡 **Khi nào dùng?** Danh sách nhỏ (vài trăm phần tử), dữ liệu **chưa sắp xếp**, hoặc chỉ tìm một lần duy nhất. Nếu phải tìm nhiều lần trên dữ liệu lớn, bạn nên **sắp xếp trước rồi dùng binary search** (bài sau).

## 5.4. Ứng dụng thực tế

- **Tìm kiếm trong danh sách nhỏ:** tìm tên trong danh bạ điện thoại ngắn.
- **Kiểm tra tồn tại:** kiểm tra mã khách hàng trong mảng đơn giản.
- **Làm nền cho tìm kiếm nhị phân:** nhiều thuật toán duyệt để tìm điểm "neo" trước khi chia đôi.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Mảng <code style="color:#a7f3d0">[10, 20, 30, 40, 50]</code>, tìm <code style="color:#a7f3d0">30</code>. Linear search duyệt bao nhiêu phần tử? Đáp án: 3 (10 → 20 → 30).
</div>