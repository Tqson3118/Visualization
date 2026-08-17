# 9. Đệ quy

Đệ quy (recursion) là một trong những khái niệm **khó nhất nhưng cũng quan trọng nhất** cho người mới bắt đầu. Nó xuất hiện khắp nơi: duyệt cây, tìm kiếm đồ thị, chia để trị, backtracking... Bài này giúp bạn nắm tư duy đệ quy một cách trực quan.

## 9.1. Đệ quy là gì?

Một hàm được gọi là **đệ quy** nếu trong thân nó **tự gọi chính nó**. Nghe có vẻ "lặp vô hạn", nhưng đệ quy đúng phải có **điều kiện dừng (base case)** — khi đạt điều kiện này, hàm dừng gọi nữa và bắt đầu "trả về ngược".

> 🪞 **Nói dễ hiểu:** Đệ quy giống như hai chiếc gương đặt đối diện nhau — hình ảnh phản chiếu lồng nhau vô tận. Nhưng trong lập trình, bạn phải đặt một tấm "gương" ở vị trí cuối để vòng phản chiếu dừng lại.

## 9.2. Ví dụ kinh điển: Giai thừa (Factorial)

`n! = n × (n-1) × (n-2) × ... × 1`, và quan trọng: **n! = n × (n-1)!** — giai thừa của n được tính từ giai thừa của n-1. Đây chính là "bài toán nhỏ hơn cùng dạng".

```javascript
function factorial(n) {
  if (n <= 1) return 1;        // BASE CASE: 0! = 1! = 1
  return n * factorial(n - 1); // RECURSIVE CASE: n! = n × (n-1)!
}

console.log(factorial(5)); // 120
```

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes stackIn { 0% { opacity: 0; transform: translateY(-6px); } 100% { opacity: 1; transform: translateY(0); } }
    .stack-viz { padding: 18px; background: rgba(10,9,18,0.85); }
    .stack-viz .bar { display: flex; gap: 4px; align-items: flex-end; height: 200px; }
    .stack-viz .fr { flex: 1; border-radius: 6px 6px 0 0; display: flex; align-items: flex-start; justify-content: center; padding-top: 6px; font-size: 12px; font-weight: 800; color: #fff; animation: stackIn .5s ease both; }
    .stack-viz .lbl { display: flex; gap: 4px; margin-top: 8px; }
    .stack-viz .fl { flex: 1; text-align: center; font-size: 11px; color: #94a3b8; font-weight: 700; }
  </style>
  <div class="stack-viz">
    <div class="bar">
      <div class="fr" style="height:30px; background:rgba(148,163,184,0.5); animation-delay:.05s">f(1)=1</div>
      <div class="fr" style="height:48px; background:rgba(148,163,184,0.6); animation-delay:.15s">f(2)</div>
      <div class="fr" style="height:68px; background:rgba(168,85,247,0.6); animation-delay:.25s">f(3)</div>
      <div class="fr" style="height:88px; background:rgba(168,85,247,0.75); animation-delay:.35s">f(4)</div>
      <div class="fr" style="height:110px; background:rgba(192,132,252,0.9); animation-delay:.45s">f(5)</div>
    </div>
    <div class="lbl">
      <div class="fl">base</div><div class="fl">f(2)</div><div class="fl">f(3)</div><div class="fl">f(4)</div><div class="fl">f(5)</div>
    </div>
    <div style="padding-top: 10px; font-size: 12px; color: #94a3b8;">Mỗi lời gọi được xếp vào <b>call stack</b>. Gọi xuống tới f(1) (base case), rồi "trả về ngược": 1 → 2 → 6 → 24 → 120.</div>
  </div>
</div>

## 9.3. Hai thành phần bắt buộc của đệ quy

1. **Base case (trường hợp dừng):** điều kiện để hàm trả về ngay, không gọi lại chính nó. *Thiếu base case = vòng lặp vô tận → stack overflow (tràn bộ nhớ ngăn xếp).*
2. **Recursive case (trường hợp đệ quy):** gọi chính nó với bài toán **nhỏ hơn**, tiến dần về base case.

> ⚠️ **Lỗi phổ biến:** Quên base case, hoặc recursive case không thu nhỏ bài toán → `RangeError: Maximum call stack size exceeded`. Luôn tự hỏi: "Lời gọi này có nhỏ hơn không? Khi nào nó dừng?"

## 9.4. Đệ quy vs Vòng lặp

Mọi thứ làm được bằng đệ quy đều làm được bằng vòng lặp (và ngược lại). Điểm khác biệt:

| Tiêu chí | Vòng lặp | Đệ quy |
|---|---|---|
| Tư duy | Quen thuộc, tuần tự | Trừu tượng hơn, tự nhiên với bài toán phân cấp |
| Bộ nhớ | O(1) | **O(độ sâu)** — mỗi lời gọi tốn một khung stack |
| Rủi ro | Ít | Dễ stack overflow nếu không có base case |
| Dùng khi | Lặp đơn giản | Cây, đồ thị, chia để trị, backtracking |

```javascript
// Giai thừa bằng vòng lặp — tương đương, không tốn stack sâu
function factorialLoop(n) {
  let result = 1;
  for (let i = 2; i <= n; i++) result *= i;
  return result;
}
```

## 9.5. Ví dụ khác: Fibonacci (dọn đường cho QHĐ)

Dãy Fibonacci: `F(0)=0, F(1)=1, F(n)=F(n-1)+F(n-2)`. Đệ quy "ngây thơ" rất đơn giản nhưng **chậm kinh khủng** (O(2ⁿ)) vì tính lặp lại cùng một bài toán con — đây chính là nguồn gốc của Quy hoạch động bạn sẽ học ở Module 8:

```javascript
function fib(n) {
  if (n <= 1) return n;          // base case
  return fib(n - 1) + fib(n - 2); // 2 bài toán con cùng dạng
}

console.log(fib(10)); // 55
```

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes treeNodeIn { 0% { opacity: 0; transform: scale(.9); } 100% { opacity: 1; transform: scale(1); } }
    .fib-tree { display: flex; justify-content: center; padding: 18px; background: rgba(10,9,18,0.85); }
    .fib-tree .node { min-width: 34px; height: 30px; padding: 0 6px; border-radius: 6px; display: flex; align-items: center; justify-content: center; font-weight: 800; color: #e2e8f0; background: rgba(168,85,247,0.2); border: 1px solid rgba(168,85,247,0.4); font-size: 13px; animation: treeNodeIn .4s ease both; }
    .fib-tree .tree { display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .fib-tree .kids { display: flex; gap: 30px; }
    .fib-tree .dup { color: #fbbf24; }
  </style>
  <div class="fib-tree">
    <div class="tree">
      <div class="node">F(5)</div>
      <div class="kids">
        <div class="tree"><div class="node">F(4)</div><div class="kids"><div class="tree"><div class="node">F(3)</div></div><div class="tree"><div class="node">F(2)</div></div></div></div>
        <div class="tree"><div class="node">F(3)</div><div class="kids"><div class="tree"><div class="node">F(2)</div></div><div class="tree"><div class="node">F(1)</div></div></div></div>
      </div>
    </div>
  </div>
  <div style="padding: 10px 18px; background: rgba(245,158,11,0.08); color: #fbbf24; font-size: 12px; font-weight: 600;">⚠️ F(3) và F(2) bị tính LẠI nhiều lần — lãng phí. Module 8 (QHĐ) sẽ fix bằng cách nhớ kết quả (memoization).</div>
</div>

## 9.6. Tóm tắt

- Đệ quy = hàm **tự gọi chính nó**, gồm **base case** (dừng) + **recursive case** (thu nhỏ bài toán).
- Cần base case để tránh **stack overflow**.
- Tự nhiên cho bài toán **phân cấp/lồng nhau** (cây, đồ thị, chia để trị).
- Fibonacci đệ quy ngây thơ = O(2ⁿ) — tiền đề cho QHĐ.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Viết hàm đệ quy tính tổng <code style="color:#a7f3d0">1 + 2 + ... + n</code>. Gợi ý: <code style="color:#a7f3d0">sum(n) = n + sum(n-1)</code>, base case <code style="color:#a7f3d0">sum(0)=0</code>.
</div>