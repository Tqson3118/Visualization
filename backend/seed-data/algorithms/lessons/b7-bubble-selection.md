# 7. Bubble Sort & Selection Sort

Sắp xếp là bài toán kinh điển nhất trong khoa học máy tính. Bài này giới thiệu 2 thuật toán sắp xếp **đơn giản nhất**: Bubble Sort và Selection Sort. Cả hai đều **O(n²)** — chậm với dữ liệu lớn, nhưng cực kỳ quan trọng để hiểu tư duy và so sánh về sau.

## 7.1. Bubble Sort — "nổi bọt"

Bubble Sort duyệt mảng nhiều lượt. Mỗi lượt, nó **so sánh từng cặp phần tử liền kề**; nếu sai thứ tự thì **đổi chỗ**. Phần tử lớn nhất "nổi bọt" lên cuối mảng sau mỗi lượt.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes bubbleSwap { 0% { transform: translateY(0); background:#ef4444; } 30% { transform: translateY(-6px); } 100% { transform: translateY(0); } }
    @keyframes bubbleRise { 0% { opacity: 0; } 100% { opacity: 1; } }
    .bubble-stage { padding: 18px; background: rgba(10,9,18,0.85); }
    .bubble-row { display: flex; align-items: center; gap: 5px; margin-bottom: 8px; animation: bubbleRise .3s ease both; }
    .bubble-row .cells { display: flex; gap: 4px; flex: 1; }
    .bubble-row .cell { flex: 1; height: 34px; border-radius: 5px; display: flex; align-items: center; justify-content: center; font-weight: 800; color: #e2e8f0; background: rgba(148,163,184,0.16); font-size: 13px; }
    .bubble-row .cell.swap { background: #ef4444; color: #fff; animation: bubbleSwap .5s ease; }
    .bubble-row .cell.done { background: rgba(34,197,94,0.2); color: #4ade80; border: 1px solid rgba(34,197,94,0.4); }
    .bubble-row .label { width: 70px; font-size: 11px; font-weight: 700; color: #94a3b8; }
  </style>
  <div class="bubble-stage">
    <div class="bubble-row"><div class="label">Đầu</div><div class="cells"><div class="cell">5</div><div class="cell">3</div><div class="cell">8</div><div class="cell">1</div></div></div>
    <div class="bubble-row" style="animation-delay:.1s"><div class="label">Lượt 1</div><div class="cells"><div class="cell swap">5</div><div class="cell swap">3</div><div class="cell">8</div><div class="cell">1</div><span style="color:#ef4444;font-size:11px;font-weight:800">5↔3</span></div></div>
    <div class="bubble-row" style="animation-delay:.2s"><div class="label"></div><div class="cells"><div class="cell">3</div><div class="cell swap">5</div><div class="cell swap">8</div><div class="cell">1</div><span style="color:#ef4444;font-size:11px;font-weight:800">5↔8</span></div></div>
    <div class="bubble-row" style="animation-delay:.3s"><div class="label"></div><div class="cells"><div class="cell">3</div><div class="cell">5</div><div class="cell swap">8</div><div class="cell swap">1</div><span style="color:#ef4444;font-size:11px;font-weight:800">8↔1</span></div></div>
    <div class="bubble-row" style="animation-delay:.4s"><div class="label">Lượt 2</div><div class="cells"><div class="cell swap">3</div><div class="cell swap">5</div><div class="cell">1</div><div class="cell done">8</div><span style="color:#4ade80;font-size:11px;font-weight:800">8 nổi lên ✓</span></div></div>
    <div style="padding-top: 6px; font-size: 12px; color: #94a3b8;">Sau mỗi lượt, phần tử lớn nhất còn lại "nổi" lên cuối (ô xanh = đã cố định).</div>
  </div>
</div>

```javascript
function bubbleSort(arr) {
  const a = [...arr];
  for (let i = 0; i < a.length - 1; i++) {
    for (let j = 0; j < a.length - 1 - i; j++) {
      if (a[j] > a[j + 1]) {
        [a[j], a[j + 1]] = [a[j + 1], a[j]]; // đổi chỗ
      }
    }
  }
  return a;
}
```

**Tối ưu nhỏ:** nếu một lượt không có phép đổi chỗ nào → mảng đã sắp xếp, dừng sớm (giúp best case = O(n)).

## 7.2. Selection Sort — "chọn phần tử nhỏ nhất"

Selection Sort chia mảng thành hai vùng: **đã sắp xếp** (bên trái) và **chưa sắp xếp** (bên phải). Mỗi lượt, nó **tìm phần tử nhỏ nhất** trong vùng chưa sắp xếp rồi **đưa sang cuối vùng đã sắp xếp**.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes selectPick { 0% { transform: scale(1); } 50% { transform: scale(1.15); } 100% { transform: scale(1); } }
    .sel-stage { padding: 18px; background: rgba(10,9,18,0.85); }
    .sel-row { display: flex; align-items: center; gap: 5px; margin-bottom: 8px; }
    .sel-row .cells { display: flex; gap: 4px; flex: 1; }
    .sel-row .cell { flex: 1; height: 34px; border-radius: 5px; display: flex; align-items: center; justify-content: center; font-weight: 800; color: #e2e8f0; background: rgba(148,163,184,0.16); font-size: 13px; }
    .sel-row .cell.sorted { background: rgba(34,197,94,0.2); color: #4ade80; border: 1px solid rgba(34,197,94,0.4); }
    .sel-row .cell.pick { background: #facc15; color: #000; animation: selectPick .5s ease; }
    .sel-row .label { width: 70px; font-size: 11px; font-weight: 700; color: #94a3b8; }
  </style>
  <div class="sel-stage">
    <div class="sel-row"><div class="label">Đầu</div><div class="cells"><div class="cell">5</div><div class="cell">3</div><div class="cell">8</div><div class="cell">1</div></div></div>
    <div class="sel-row"><div class="label">Chọn</div><div class="cells"><div class="cell">5</div><div class="cell">3</div><div class="cell">8</div><div class="cell pick">1</div></div><span style="color:#facc15;font-size:11px;font-weight:800">nhỏ nhất</span></div>
    <div class="sel-row"><div class="label">Đổi</div><div class="cells"><div class="cell">5</div><div class="cell">3</div><div class="cell">8</div><div class="cell">1</div><span style="color:#ef4444;font-size:11px;font-weight:800">1↔5</span></div></div>
    <div class="sel-row"><div class="label">Lượt 2</div><div class="cells"><div class="cell sorted">1</div><div class="cell">5</div><div class="cell">8</div><div class="cell">3</div></div><span style="color:#4ade80;font-size:11px;font-weight:800">xong vị trí 0</span></div>
    <div style="padding-top: 6px; font-size: 12px; color: #94a3b8;">Mỗi lượt chọn min rồi đưa lên đầu vùng chưa sắp xếp — ô vàng = phần tử đang được chọn.</div>
  </div>
</div>

```javascript
function selectionSort(arr) {
  const a = [...arr];
  for (let i = 0; i < a.length - 1; i++) {
    let minIdx = i;
    for (let j = i + 1; j < a.length; j++) {
      if (a[j] < a[minIdx]) minIdx = j;  // tìm min
    }
    [a[i], a[minIdx]] = [a[minIdx], a[i]]; // đưa min lên đầu
  }
  return a;
}
```

## 7.3. So sánh hai thuật toán

| Tiêu chí | Bubble Sort | Selection Sort |
|---|---|---|
| Ý tưởng | Đổi chỗ cặp liền kề | Chọn min đưa lên đầu |
| Số phép **so sánh** | O(n²) | O(n²) |
| Số phép **đổi chỗ** | O(n²) — nhiều | **O(n)** — ít |
| Best case | O(n) (nếu tối ưu cờ) | O(n²) |
| Bộ nhớ | O(1) | O(1) |
| Ổn định | ✅ | ❌ |

> 💡 **Ghi nhớ:** Selection Sort dù vẫn O(n²) nhưng **đổi chỗ ít hơn hẳn** so với Bubble — quan trọng khi thao tác "đổi chỗ" đắt (ví dụ đổi chỗ bản ghi nặng). Còn Bubble dễ hiểu nhất để bắt đầu.

## 7.4. Tóm tắt

- Bubble Sort: **đổi chỗ cặp liền kề**, phần tử lớn "nổi" lên cuối.
- Selection Sort: **chọn min** đưa lên đầu vùng chưa sắp xếp.
- Cả hai O(n²) — phù hợp dữ liệu nhỏ, dạy tư duy, không dùng cho dữ liệu lớn.
- Từ bài sau, bạn sẽ gặp thuật toán O(n log n) nhanh hơn hẳn.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Mảng <code style="color:#a7f3d0">[4, 2, 9, 1]</code>. Viết các lượt của Bubble Sort đến khi sắp xếp xong. Đáp án: L1: 2,4,1,9 · L2: 2,1,4,9 · L3: 1,2,4,9.
</div>