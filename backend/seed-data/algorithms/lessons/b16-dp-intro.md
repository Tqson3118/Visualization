# 16. Giới thiệu Quy hoạch động (QHĐ)

Chặng cuối của hành trình — **Quy hoạch động (Dynamic Programming, QHĐ/DP)**. Đây là kỹ thuật giải thuật "đỉnh cao" mà nhiều bài toán tối ưu hóa dựa vào. Nghe có vẻ đáng sợ, nhưng bản chất chỉ là một ý tưởng rất đơn giản bạn đã thấy từ bài 9 (Fibonacci): **nhớ lại kết quả đã tính để không phải tính lại**.

## 16.1. Ý tưởng cốt lõi: "Ghi nhớ để không tính lại"

Một bài toán có **tính chất con lặp lại (overlapping subproblems)** nghĩa là cùng một bài toán con xuất hiện nhiều lần trong quá trình giải. QHĐ giải mỗi bài toán con **một lần**, **lưu kết quả**, rồi **tái sử dụng** — đổi thời gian chạy từ "thảm họa" sang "tuyến tính".

> 🧠 **Nói dễ hiểu:** Đệ quy ngây thơ giống một người quên lời: cứ hỏi đi hỏi lại câu "F(3) bằng mấy?". QHĐ giống người **cầm quyển sổ ghi chú** — tính xong F(3) là ghi lại, lần sau chỉ việc tra sổ.

## 16.2. So sánh Fibonacci: đệ quy thô vs QHĐ

Bạn đã thấy đệ quy Fibonacci ở bài 9 là **O(2ⁿ)** — tính lại cùng một giá trị hàng triệu lần. Nhìn lại cây đệ quy: F(3), F(2) bị lặp liên tục.

**Cách 1 — Memoization (top-down):** vẫn đệ quy, nhưng **nhớ kết quả** vào một bảng (memo):

```javascript
function fibMemo(n, memo = {}) {
  if (n <= 1) return n;
  if (n in memo) return memo[n];         // tra sổ ghi chú!
  memo[n] = fibMemo(n - 1, memo) + fibMemo(n - 2, memo);
  return memo[n];
}

console.log(fibMemo(40)); // 102334155 — chạy tức thì, không phải đợi
```

**Cách 2 — Tabulation (bottom-up):** tính **từ nhỏ đến lớn**, không đệ quy, dùng vòng lặp:

```javascript
function fibTab(n) {
  if (n <= 1) return n;
  const dp = [0, 1];
  for (let i = 2; i <= n; i++) {
    dp[i] = dp[i - 1] + dp[i - 2];   // dùng 2 kết quả trước
  }
  return dp[n];
}
```

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes dpFill { 0% { opacity: 0; transform: scale(.9); } 100% { opacity: 1; transform: scale(1); } }
    .dp-row { display: flex; gap: 4px; justify-content: center; padding: 16px; background: rgba(10,9,18,0.85); }
    .dp-cell { width: 52px; height: 40px; border-radius: 6px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 13px; background: rgba(148,163,184,0.14); color: #e2e8f0; animation: dpFill .4s ease both; }
    .dp-cell.filled { background: rgba(168,85,247,0.3); color: #e9d5ff; border: 1px solid rgba(168,85,247,0.5); }
    .dp-cell.final { background: rgba(34,197,94,0.3); color: #4ade80; border: 1px solid rgba(34,197,94,0.5); }
  </style>
  <div class="dp-row">
    <div class="dp-cell filled" style="animation-delay:.05s">F0=0</div>
    <div class="dp-cell filled" style="animation-delay:.1s">F1=1</div>
    <div class="dp-cell" style="animation-delay:.2s">F2=1</div>
    <div class="dp-cell" style="animation-delay:.3s">F3=2</div>
    <div class="dp-cell" style="animation-delay:.4s">F4=3</div>
    <div class="dp-cell" style="animation-delay:.5s">F5=5</div>
    <div class="dp-cell" style="animation-delay:.6s">F6=8</div>
    <div class="dp-cell final" style="animation-delay:.7s">F7=13</div>
  </div>
  <div style="padding: 8px 16px; background: rgba(168,85,247,0.1); color: #c084fc; font-size: 12px; font-weight: 600;">Tabulation: điền bảng từ F0 → F7, mỗi ô dùng 2 ô trước. Không có phép tính nào bị lặp lại.</div>
</div>

## 16.3. Hai đặc tính để nhận diện bài toán QHĐ

1. **Overlapping subproblems:** cùng bài toán con xuất hiện nhiều lần (Fibonacci, coin change).
2. **Optimal substructure:** lời giải tối ưu của bài toán lớn = **kết hợp** lời giải tối ưu của bài toán con (tìm đường ngắn nhất qua 1 đỉnh = ngắn nhất tới đỉnh đó + cạnh cuối).

Nếu bài toán có **đủ cả hai** → có thể áp dụng QHĐ.

## 16.4. Quy trình 4 bước giải QHĐ

1. **Xác định trạng thái (state):** bài toán con được mô tả bằng tham số nào? (vd `dp[i]` = số cách/kết quả cho kích thước i).
2. **Tìm công thức truy hồi (recurrence):** `dp[i]` được tính từ các `dp` nhỏ hơn thế nào?
3. **Xác định base case:** `dp[0]`, `dp[1]` bằng bao nhiêu?
4. **Chọn cách triển khai:** top-down (memo) hay bottom-up (tabulation).

Ví dụ coin change hệ {1,3,4} — bài mà **greedy sai** (bài 15). QHĐ giải đúng:

```javascript
// dp[i] = số đồng ít nhất để thối i xu
function coinChangeDP(coins, amount) {
  const dp = new Array(amount + 1).fill(Infinity);
  dp[0] = 0;                                   // base case: 0 đồng để thối 0
  for (let i = 1; i <= amount; i++) {
    for (const c of coins) {
      if (i >= c) dp[i] = Math.min(dp[i], dp[i - c] + 1); // truy hồi
    }
  }
  return dp[amount] === Infinity ? -1 : dp[amount];
}

console.log(coinChangeDP([1, 3, 4], 6)); // 2 (3+3) — greedy sai, QHĐ đúng!
```

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes coinDp { 0% { opacity: 0; transform: translateY(-4px); } 100% { opacity: 1; transform: translateY(0); } }
    .coin-row { display: flex; gap: 4px; justify-content: center; padding: 16px; background: rgba(10,9,18,0.85); flex-wrap: wrap; }
    .coin-cell { width: 46px; height: 42px; border-radius: 6px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 13px; background: rgba(148,163,184,0.14); color: #e2e8f0; animation: coinDp .4s ease both; }
    .coin-cell.base { background: rgba(34,197,94,0.25); color: #4ade80; }
    .coin-cell.hot { background: rgba(168,85,247,0.3); color: #e9d5ff; border: 1px solid rgba(168,85,247,0.5); }
    .coin-cell.final { background: rgba(245,158,11,0.3); color: #fde68a; border: 1px solid rgba(245,158,11,0.5); }
  </style>
  <div class="coin-row">
    <div class="coin-cell base" style="animation-delay:.05s">0</div>
    <div class="coin-cell hot" style="animation-delay:.1s">1</div>
    <div class="coin-cell hot" style="animation-delay:.15s">1</div>
    <div class="coin-cell hot" style="animation-delay:.2s">1</div>
    <div class="coin-cell hot" style="animation-delay:.25s">2</div>
    <div class="coin-cell hot" style="animation-delay:.3s">2</div>
    <div class="coin-cell final" style="animation-delay:.35s">2</div>
    <div style="flex-basis:100%; text-align:center; padding-top:6px; font-size:12px; color:#94a3b8;">dp[0..6] với xu {1,3,4}. dp[6]=2 (3+3) — nhỏ nhất có thể. Các ô màu tím = số đồng tối ưu cho từng mức tiền.</div>
  </div>
</div>

## 16.5. Độ phức tạp

Với Fibonacci: từ **O(2ⁿ)** (đệ quy thô) → **O(n)** (QHĐ). Với coin change `amount` và `k` loại xu: **O(amount × k)**. QHĐ thường "mua" thời gian bằng **không gian bảng nhớ** (O(amount)), và thường có thể tối ưu còn O(1) nếu chỉ cần vài giá trị gần nhất.

## 16.6. Tổng kết toàn khóa — bạn đã đi được bao xa!

Chúc mừng! Bạn vừa hoàn thành lộ trình **từ con số 0 đến trung cấp**:

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes roadDone { 0% { opacity: 0; transform: translateX(-8px); } 100% { opacity: 1; transform: translateX(0); } }
    .road { padding: 16px 18px; background: rgba(10,9,18,0.85); }
    .road-row { display: flex; align-items: center; gap: 8px; padding: 7px 0; border-bottom: 1px dashed rgba(255,255,255,0.07); animation: roadDone .4s ease both; }
    .road-row:last-child { border-bottom: none; }
    .road-row .mod { width: 150px; font-size: 12px; font-weight: 800; color: #c084fc; }
    .road-row .desc { flex: 1; font-size: 12.5px; color: #94a3b8; }
    .road-row .check { color: #4ade80; font-weight: 900; }
  </style>
  <div class="road">
    <div class="road-row"><span class="check">✓</span><div class="mod">Module 1</div><div class="desc">Thuật toán là gì — khái niệm & ứng dụng</div></div>
    <div class="road-row"><span class="check">✓</span><div class="mod">Module 2</div><div class="desc">Big-O — đo lường tốc độ thuật toán</div></div>
    <div class="road-row"><span class="check">✓</span><div class="mod">Module 3</div><div class="desc">Tìm kiếm tuyến tính & nhị phân</div></div>
    <div class="road-row"><span class="check">✓</span><div class="mod">Module 4</div><div class="desc">Bubble / Selection / Insertion Sort</div></div>
    <div class="road-row"><span class="check">✓</span><div class="mod">Module 5</div><div class="desc">Đệ quy, Merge Sort, Quick Sort</div></div>
    <div class="road-row"><span class="check">✓</span><div class="mod">Module 6</div><div class="desc">Heap, Heapify, Heap Sort, Top-K</div></div>
    <div class="road-row"><span class="check">✓</span><div class="mod">Module 7</div><div class="desc">BFS, DFS, Dijkstra</div></div>
    <div class="road-row"><span class="check">✓</span><div class="mod">Module 8</div><div class="desc">Tham lam & Quy hoạch động</div></div>
  </div>
</div>

Từ đây, bạn có đủ nền tảng để học tiếp các chủ đề trung-cao cấp: **backtracking, cây nâng cao (AVL, Trie, Segment Tree), Union-Find, đồ thị nâng cao (MST, Topological), chuỗi (KMP, Rabin-Karp)**... Mỗi chủ đề đó đều dựa trên đúng các khối kiến thức bạn vừa xây dựng.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Cho mảng <code style="color:#a7f3d0">[3, 1, 4, 1, 5]</code>. Hãy gọi tên độ phức tạp của: (a) tìm phần tử 4 bằng linear search; (b) sắp xếp bằng merge sort; (c) tìm phần tử lớn nhất. Đáp án: (a) O(n), (b) O(n log n), (c) O(n). Nếu bạn trả lời đúng cả 3 — bạn đã nắm vững lộ trình này!
</div>