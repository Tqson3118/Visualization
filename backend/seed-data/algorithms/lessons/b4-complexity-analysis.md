# 4. Phân tích thuật toán

Bài trước bạn đã biết Big-O là gì. Bài này sẽ dạy bạn **đặt tay vào code thật** để phân tích — xác định Big-O của các đoạn chương trình thực tế, đồng thời biết thêm một chiều nữa: **độ phức tạp không gian (bộ nhớ)**, không chỉ thời gian.

## 4.1. Vì sao "chạy thử" không đủ để đo thuật toán?

Bạn có thể nghĩ: *"Cứ chạy thử với đồng hồ bấm giờ là biết thuật toán nào nhanh hơn chứ?"*. Đúng một phần, nhưng chạy thử (benchmark) có 3 hạn chế:

1. **Phụ thuộc máy:** cùng code, máy nhanh chạy nhanh hơn máy chậm → con số không khách quan.
2. **Phụ thuộc dữ liệu:** một bộ test may mắn có thể che giấu điểm yếu của thuật toán.
3. **Không mở rộng:** không thể chạy thử với dữ liệu "1 tỷ phần tử" để xem thuật toán có trụ được không.

Big-O giải quyết cả 3 vấn đề: nó là **mô hình toán học** độc lập với máy và dữ liệu, cho biết hành vi khi n **tiến tới vô cùng**.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes benchRow { 0% { transform: translateY(-6px); opacity: 0; } 100% { transform: translateY(0); opacity: 1; } }
    .bench { padding: 16px 18px; background: rgba(10,9,18,0.8); }
    .bench-row { display: flex; gap: 10px; margin-bottom: 8px; animation: benchRow .4s ease both; }
    .bench-row .cells { display: flex; gap: 4px; flex: 1; }
    .bench-row .cell { flex: 1; height: 30px; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: rgba(255,255,255,0.85); background: rgba(148,163,184,0.25); }
    .bench-row .bench { font-weight: 800; font-size: 12px; color: #e9d5ff; }
    .bench-row .cell.ok { background: linear-gradient(135deg, #22c55e, #16a34a); }
    .bench-row .cell.warn { background: linear-gradient(135deg, #f59e0b, #d97706); }
    .bench-row .cell.bad { background: linear-gradient(135deg, #ef4444, #dc2626); }
  </style>
  <div class="bench">
    <div class="bench-row" style="animation-delay:.05s"><div class="bench">n = 10</div><div class="cells"><div class="cell ok">10</div><div class="cell ok">100</div></div></div>
    <div class="bench-row" style="animation-delay:.15s"><div class="bench">n = 100</div><div class="cells"><div class="cell warn">100</div><div class="cell bad">10.000</div></div></div>
    <div class="bench-row" style="animation-delay:.25s"><div class="bench">n = 1.000</div><div class="cells"><div class="cell bad">1.000</div><div class="cell bad">1.000.000</div></div></div>
    <div style="padding-top: 8px; font-size: 12px; color: #94a3b8;">Ô trái: số phép toán của O(n) · Ô phải: số phép toán của O(n²) — khoảng cách nứt ra khi n tăng.</div>
  </div>
</div>

## 4.2. Các mẫu code kinh điển và Big-O của chúng

Hãy phân tích một loạt mẫu phổ biến — đây chính là "bài tập vận động não" mà phỏng vấn hay hỏi:

```javascript
// Mẫu 1 — O(1): không có vòng lặp
function constant(arr) { return arr[0] + arr[1]; }

// Mẫu 2 — O(n): 1 vòng lặp duyệt hết
function linear(arr) { let s = 0; for (let x of arr) s += x; return s; }

// Mẫu 3 — O(n): dù có 2 vòng tuần tự, vẫn là O(n)
function twoLoops(arr) {
  for (let x of arr) console.log(x);
  for (let x of arr) console.log(x * 2);
}

// Mẫu 4 — O(n²): vòng lồng vòng
function quadratic(arr) {
  for (let i = 0; i < arr.length; i++)
    for (let j = 0; j < arr.length; j++)
      console.log(arr[i], arr[j]);
}

// Mẫu 5 — O(log n): mỗi bước chia đôi
function countHalves(n) {
  let count = 0;
  while (n > 1) { n = Math.floor(n / 2); count++; }
  return count;
}
```

**Giải thích Mẫu 5 (O(log n)):** bắt đầu từ n, mỗi vòng lặp n giảm một nửa. Số lần lặp là số lần "chia đôi" cho tới khi n = 1. Với n = 8: 8 → 4 → 2 → 1 (3 bước) = log₂(8). Với n = 1024: chỉ 10 bước. Đó là vì sao **binary search** cực kỳ nhanh!

<div style="margin: 22px 0;">
  <style>
    @keyframes splitIn { 0% { opacity: 0; transform: scale(.9); } 100% { opacity: 1; transform: scale(1); } }
    .halves { display: flex; align-items: center; justify-content: center; gap: 10px; flex-wrap: wrap; }
    .half { width: 46px; height: 46px; border-radius: 10px; background: rgba(132,204,22,0.15); border: 2px solid #84cc16; display: flex; align-items: center; justify-content: center; color: #d9f99d; font-weight: 800; font-size: 15px; animation: splitIn .4s ease both; }
    .half.minus { width: 28px; height: 28px; border-radius: 8px; background: rgba(148,163,184,0.12); border: 1px dashed rgba(148,163,184,0.4); font-size: 12px; color: #94a3b8; }
    .half.arrow { background: none; border: none; color: #84cc16; width: auto; font-size: 18px; }
  </style>
  <div class="halves">
    <div class="half" style="animation-delay:.05s">8</div><div class="half arrow">→</div>
    <div class="half" style="animation-delay:.15s">4</div><div class="half arrow">→</div>
    <div class="half" style="animation-delay:.25s">2</div><div class="half arrow">→</div>
    <div class="half" style="animation-delay:.35s">1</div>
    <div class="half minus" style="animation-delay:.45s">3 bước = log₂8</div>
  </div>
</div>

## 4.3. Độ phức tạp không gian (Space Complexity)

Ngoài thời gian, thuật toán còn tốn **bộ nhớ** — và bộ nhớ cũng có Big-O. Độ phức tạp không gian đếm **bộ nhớ phụ** (extra memory) mà thuật toán cần thêm ngoài dữ liệu đầu vào.

```javascript
// O(1) không gian — chỉ dùng vài biến, không phụ thuộc n
function sumInPlace(arr) {
  let total = 0;
  for (let x of arr) total += x;
  return total;
}

// O(n) không gian — tạo mảng mới có kích thước n
function copyArray(arr) {
  const result = [];
  for (let x of arr) result.push(x);
  return result;
}

// O(n²) không gian — ma trận n×n
function identityMatrix(n) {
  const m = [];
  for (let i = 0; i < n; i++) {
    m[i] = [];
    for (let j = 0; j < n; j++) m[i][j] = i === j ? 1 : 0;
  }
  return m;
}
```

> 💡 **Ví dụ hay để nhớ:** Merge Sort chạy nhanh (O(n log n)) nhưng cần **O(n) bộ nhớ phụ** để lưu mảng tạm khi trộn. Còn Insertion Sort chạy chậm hơn (O(n²)) nhưng chỉ cần **O(1)** — sắp ngay trên mảng gốc (in-place). Đây là ví dụ kinh điển của đánh đổi **thời gian ↔ bộ nhớ**.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    .trade { display: flex; gap: 14px; padding: 18px; background: rgba(10,9,18,0.8); flex-wrap: wrap; }
    .trade-card { flex: 1; min-width: 220px; border-radius: 12px; padding: 14px; }
    .trade-card.time { background: rgba(239,68,68,0.08); border: 1px solid rgba(239,68,68,0.3); }
    .trade-card.mem { background: rgba(59,130,246,0.08); border: 1px solid rgba(59,130,246,0.3); }
    .trade-card h5 { margin: 0 0 6px; font-size: 13px; font-weight: 800; }
    .trade-card.time h5 { color: #f87171; }
    .trade-card.mem h5 { color: #60a5fa; }
    .trade-card p { margin: 0; font-size: 12.5px; color: #94a3b8; line-height: 1.6; }
  </style>
  <div class="trade">
    <div class="trade-card time"><h5>⏱️ Thời gian</h5><p>Số phép toán cần thực hiện. Muốn nhanh hơn thường phải "trả giá" bằng bộ nhớ.</p></div>
    <div class="trade-card mem"><h5>💾 Bộ nhớ</h5><p>Bộ nhớ phụ cần dùng. Muốn tiết kiệm RAM thường phải chấp nhận chạy chậm hơn.</p></div>
  </div>
</div>

## 4.4. Quy trình phân tích một thuật toán bất kỳ

Khi gặp một thuật toán mới, hãy áp dụng 4 bước sau:

1. **Đọc hiểu:** bài toán giải gì, vòng lặp chính ở đâu.
2. **Tìm vòng lặp sâu nhất:** số vòng lồng nhau = bậc cơ bản.
3. **Ước lượng số lần lặp mỗi vòng:** đầy đủ n, hay giảm nửa (log n)?
4. **Cộng thêm chi phí khác:** đệ quy (đếm số lần gọi), bộ nhớ phụ (mảng/đệ quy).

## 4.5. Tóm tắt

- Benchmark (chạy thử) không khách quan bằng **phân tích Big-O**.
- Ngoài **thời gian**, thuật toán còn có độ phức tạp **không gian**.
- Có sự đánh đổi kinh điển: **nhanh ↔ tốn bộ nhớ** (Merge Sort vs Insertion Sort).
- Quy trình phân tích: tìm vòng lặp sâu nhất → ước lượng số lần lặp → thêm chi phí phụ.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Đoạn code sau có độ phức tạp thời gian và không gian là bao nhiêu? <code style="color:#a7f3d0">function f(n){ let r=[]; for(let i=0;i&lt;n;i++) r.push(i*i); return r; }</code>. Đáp án: thời gian O(n), không gian O(n) vì tạo mảng r có n phần tử.
</div>