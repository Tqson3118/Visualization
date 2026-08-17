# 3. Big-O & Tốc độ tăng trưởng

Ở Module 1 bạn đã thấy cùng một bài toán có thể có nhiều cách giải. Nhưng làm sao **đo được** cách nào nhanh hơn khi dữ liệu chưa lớn? Câu trả lời chính là **Big-O notation** — một công cụ toán học đơn giản mà cực kỳ mạnh, được dùng ở mọi nơi trong ngành phần mềm để mô tả độ phức tạp thuật toán.

## 3.1. Ý tưởng cốt lõi: đếm theo "tốc độ tăng trưởng"

Khi dữ liệu đầu vào có kích thước **n**, mỗi thuật toán thực hiện một số phép toán **t(n)**. Big-O không quan tâm con số chính xác, mà chỉ quan tâm **t(n) "tăng nhanh" cỡ nào khi n lớn dần**.

Ví dụ, dù thuật toán A thực hiện `2n + 3` phép toán hay `n + 1000` phép toán, khi n đủ lớn thì phần quan trọng nhất là **n** — hằng số không đáng kể. Ta nói cả hai đều là **O(n)**.

> **Các mức Big-O thường gặp (từ nhanh đến chậm):**

<div style="margin: 26px 0;">
  <style>
    @keyframes complexityRow { 0% { transform: translateX(-10px); opacity: 0; } 100% { transform: translateX(0); opacity: 1; } }
    .complexity { display: flex; flex-direction: column; gap: 8px; }
    .c-row { display: flex; align-items: center; gap: 12px; background: rgba(13,12,22,0.6); border: 1px solid rgba(255,255,255,0.08); border-radius: 10px; padding: 10px 14px; animation: complexityRow .5s ease both; }
    .c-row .badge { min-width: 92px; padding: 4px 10px; border-radius: 6px; font-weight: 800; font-size: 13px; text-align: center; font-family: monospace; }
    .c-row .name { flex: 1; font-size: 13px; color: #e2e8f0; }
    .c-row .ex { font-size: 12px; color: #94a3b8; }
    .g1 { background: rgba(34,197,94,0.15); color: #4ade80; }
    .g2 { background: rgba(132,204,22,0.15); color: #a3e635; }
    .g3 { background: rgba(250,204,21,0.15); color: #facc15; }
    .g4 { background: rgba(249,115,22,0.15); color: #fb923c; }
    .g5 { background: rgba(239,68,68,0.18); color: #f87171; }
  </style>
  <div class="complexity">
    <div class="c-row" style="animation-delay:.05s"><div class="badge g1">O(1)</div><div class="name">Hằng số — luôn xong ngay lập tức</div><div class="ex">Truy cập phần tử theo chỉ số</div></div>
    <div class="c-row" style="animation-delay:.12s"><div class="badge g2">O(log n)</div><div class="name">Logarit — cực nhanh, dữ liệu tăng gấp đôi chỉ thêm 1 bước</div><div class="ex">Tìm kiếm nhị phân</div></div>
    <div class="c-row" style="animation-delay:.19s"><div class="badge g3">O(n)</div><div class="name">Tuyến tính — chạy tỉ lệ thuận với dữ liệu</div><div class="ex">Tìm kiếm tuyến tính</div></div>
    <div class="c-row" style="animation-delay:.26s"><div class="badge g4">O(n log n)</div><div class="name">Tuyến tính-logarit — chuẩn vàng cho sắp xếp</div><div class="ex">Merge Sort</div></div>
    <div class="c-row" style="animation-delay:.33s"><div class="badge g5">O(n²)</div><div class="name">Bậc hai — chậm với dữ liệu lớn</div><div class="ex">Vòng lặp lồng nhau</div></div>
  </div>
</div>

## 3.2. Trực quan: đồ thị tăng trưởng

Hãy nhìn vào đồ thị bên dưới — nó sẽ nói lên tất cả. Trục **ngang** là kích thước dữ liệu n, trục **dọc** là số phép toán cần thực hiện:

<div style="margin: 26px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; padding: 8px; background: rgba(10,9,18,0.85);">
  <style>
    @keyframes drawLine { from { stroke-dashoffset: 600; } to { stroke-dashoffset: 0; } }
    .growth-chart { width: 100%; max-width: 640px; display: block; margin: 0 auto; }
    .growth-chart .grid-line { stroke: rgba(255,255,255,0.06); stroke-width: 1; }
    .growth-chart .axis { stroke: rgba(255,255,255,0.25); stroke-width: 1.5; }
    .growth-chart .line { stroke-width: 3; fill: none; stroke-linecap: round; animation: drawLine 1.6s ease both; }
    .growth-chart .label { font-size: 11px; font-weight: 700; }
  </style>
  <svg class="growth-chart" viewBox="0 0 640 400">
    <defs>
      <linearGradient id="areaGrad" x1="0" y1="0" x2="0" y2="1">
        <stop offset="0%" stop-color="#f87171" stop-opacity="0.25"/>
        <stop offset="100%" stop-color="#f87171" stop-opacity="0"/>
      </linearGradient>
    </defs>
    <!-- axes -->
    <line class="axis" x1="40" y1="360" x2="620" y2="360"/>
    <line class="axis" x1="40" y1="20" x2="40" y2="360"/>
    <!-- grid -->
    <line class="grid-line" x1="40" y1="80" x2="620" y2="80"/>
    <line class="grid-line" x1="40" y1="160" x2="620" y2="160"/>
    <line class="grid-line" x1="40" y1="240" x2="620" y2="240"/>
    <line class="grid-line" x1="40" y1="320" x2="620" y2="320"/>
    <!-- axis labels -->
    <text x="320" y="392" fill="#94a3b8" class="label" text-anchor="middle">Kích thước dữ liệu (n)</text>
    <text x="14" y="200" fill="#94a3b8" class="label" transform="rotate(-90 14,200)" text-anchor="middle">Phép toán</text>
    <!-- O(1): horizontal -->
    <line class="line" x1="40" y1="350" x2="600" y2="350" stroke="#4ade80"/>
    <text x="600" y="344" fill="#4ade80" class="label" text-anchor="end">O(1)</text>
    <!-- O(log n) -->
    <path class="line" d="M40 350 C 120 345, 200 330, 600 288" stroke="#a3e635"/>
    <text x="600" y="282" fill="#a3e635" class="label" text-anchor="end">O(log n)</text>
    <!-- O(n) -->
    <line class="line" x1="40" y1="350" x2="600" y2="60" stroke="#facc15"/>
    <text x="600" y="54" fill="#facc15" class="label" text-anchor="end">O(n)</text>
    <!-- O(n log n) -->
    <path class="line" d="M40 350 C 160 340, 300 300, 600 100" stroke="#fb923c"/>
    <text x="600" y="94" fill="#fb923c" class="label" text-anchor="end">O(n log n)</text>
    <!-- O(n^2) -->
    <path class="line" d="M40 350 C 120 360, 200 320, 300 240, 420 120, 600 18" stroke="#f87171"/>
    <text x="600" y="18" fill="#f87171" class="label" text-anchor="end">O(n²)</text>
  </svg>
  <div style="padding: 10px 16px; background: rgba(168,85,247,0.1); color: #c084fc; font-size: 12px; font-weight: 600;">
    Quan sát: khi n tăng, O(n²) "bùng nổ" theo đường cong dựng đứng — chỉ sau vài chục phần tử đã vượt xa O(n log n).
  </div>
</div>

## 3.3. Quy tắc đếm Big-O

Để xác định Big-O của một đoạn code, bạn chỉ cần 4 quy tắc đơn giản:

1. **Bỏ hằng số:** `3n + 5` → `O(n)`, `10` → `O(1)`.
2. **Giữ bậc cao nhất:** `n² + n` → `O(n²)` (n² lấn át n khi n lớn).
3. **Vòng lặp tuần tự:** cộng số lần lặp — `for` chạy n lần + `for` chạy n lần = `O(n) + O(n) = O(n)`.
4. **Vòng lặp lồng nhau:** nhân số lần lặp — `for` chạy n lần chứa `for` chạy n lần = `O(n × n) = O(n²)`.

Hãy xem 3 đoạn code phổ biến:

```javascript
// O(1) — hằng số: làm 1 việc duy nhất, không phụ thuộc n
function getFirst(arr) { return arr[0]; }

// O(n) — tuyến tính: duyệt hết mảng
function sum(arr) {
  let total = 0;
  for (let x of arr) total += x;
  return total;
}

// O(n²) — bậc hai: vòng lặp lồng nhau
function hasDuplicate(arr) {
  for (let i = 0; i < arr.length; i++)
    for (let j = 0; j < arr.length; j++)
      if (i !== j && arr[i] === arr[j]) return true;
  return false;
}
```

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes opCount { 0% { opacity: 0; transform: scale(.92); } 100% { opacity: 1; transform: scale(1); } }
    .ops { display: flex; justify-content: space-around; align-items: flex-end; gap: 10px; padding: 20px; background: rgba(10,9,18,0.8); }
    .op-col { display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .op-bar { width: 56px; border-radius: 6px 6px 0 0; display: flex; align-items: flex-end; justify-content: center; color: #fff; font-size: 11px; font-weight: 700; padding-bottom: 4px; animation: opCount .5s ease both; }
    .op-label { font-size: 12px; color: #94a3b8; font-weight: 600; }
  </style>
  <div class="ops">
    <div class="op-col"><div class="op-bar" style="height:24px; background:linear-gradient(180deg,#4ade80,#22c55e); animation-delay:.1s">1</div><div class="op-label">O(1)</div></div>
    <div class="op-col"><div class="op-bar" style="height:70px; background:linear-gradient(180deg,#a3e635,#65a30d); animation-delay:.2s">n</div><div class="op-label">O(n)</div></div>
    <div class="op-col"><div class="op-bar" style="height:160px; background:linear-gradient(180deg,#fb923c,#ea580c); animation-delay:.3s">n²</div><div class="op-label">O(n²)</div></div>
  </div>
</div>

## 3.4. Trường hợp tốt / xấu / trung bình

Một thuật toán có thể có các chi phí khác nhau tùy dữ liệu:

- **Best case** — dữ liệu "may mắn" nhất (thường không quan trọng).
- **Worst case** — dữ liệu "xui xẻo" nhất (quan trọng nhất, ta luôn chuẩn bị cho kịch bản này).
- **Average case** — mức trung bình trên mọi dữ liệu.

Ví dụ tìm kiếm tuyến tính `search.linear` trên mảng n phần tử: phần tử cần tìm nằm ngay đầu → **O(1)**; nằm cuối hoặc không có → **O(n)**. Khi nói "tìm kiếm tuyến tính là O(n)", ta nói về **worst case** — mức đảm bảo.

> 💡 **Mẹo ghi nhớ:** Với hầu hết thuật toán, cứ nhìn **vòng lặp sâu nhất** — số vòng lặp lồng nhau quyết định bậc chính: 0 vòng = O(1), 1 vòng = O(n), 2 vòng = O(n²), và cứ "chia đôi mỗi lần" (binary search, merge) = O(log n) hoặc O(n log n).

## 3.5. Tóm tắt

- Big-O mô tả **tốc độ tăng trưởng** số phép toán theo kích thước dữ liệu n.
- Thứ tự nhanh→chậm: **O(1) < O(log n) < O(n) < O(n log n) < O(n²)**.
- Quy tắc: bỏ hằng số, giữ bậc cao nhất, lồng nhau thì nhân, tuần tự thì cộng.
- Luôn nói Big-O theo **worst case** khi không ghi chú gì thêm.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Xác định Big-O của đoạn code sau: <code style="color:#a7f3d0">for (i=0; i&lt;n; i++) for (j=0; j&lt;n; j++) console.log(i*j)</code>. Đáp án: O(n²) vì hai vòng lồng nhau, mỗi vòng n lần.
</div>