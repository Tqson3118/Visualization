# 14. Thuật toán Dijkstra

BFS tìm đường ngắn nhất theo **số cạnh**. Nhưng thực tế các cạnh thường có **chi phí khác nhau** (km, phút, tiền). Khi đó cần **thuật toán Dijkstra** — một trong những thuật toán quan trọng nhất cho bài toán **đường đi ngắn nhất trên đồ thị có trọng số không âm**, dùng trong Google Maps, định tuyến mạng, game pathfinding.

## 14.1. Bài toán

Cho đồ thị có trọng số (không âm) và một đỉnh nguồn. Tìm **đường đi với tổng chi phí nhỏ nhất** từ nguồn tới **mọi đỉnh khác**.

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes djIn { 0% { opacity: 0; transform: scale(.85); } 100% { opacity: 1; transform: scale(1); } }
    .dj-grid { padding: 18px; background: rgba(10,9,18,0.85); }
    .dj-graph { position: relative; height: 170px; }
    .dj-node { position: absolute; width: 46px; height: 46px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 800; color: #e9d5ff; background: rgba(168,85,247,0.25); border: 2px solid #a855f7; animation: djIn .4s ease both; }
    .dj-node.src { background: rgba(34,197,94,0.35); border-color: #22c55e; color: #4ade80; }
    .dj-edge { stroke: rgba(148,163,184,0.45); stroke-width: 2; }
    .dj-w { fill: #fbbf24; font-size: 11px; font-weight: 800; }
  </style>
  <div class="dj-grid">
    <svg class="dj-graph" viewBox="0 0 400 170">
      <line class="dj-edge" x1="120" y1="30" x2="270" y2="30"/>
      <text class="dj-w" x="185" y="24">4</text>
      <line class="dj-edge" x1="130" y1="55" x2="180" y2="120"/>
      <text class="dj-w" x="140" y="92">2</text>
      <line class="dj-edge" x1="285" y1="52" x2="235" y2="118"/>
      <text class="dj-w" x="250" y="92">3</text>
      <line class="dj-edge" x1="195" y1="130" x2="290" y2="130"/>
      <text class="dj-w" x="230" y="148">5</text>
      <text x="60" y="24" fill="#94a3b8" font-size="11">Bắt đầu</text>
    </svg>
    <div class="dj-node src" style="left:100px; top:8px">A</div>
    <div class="dj-node" style="left:250px; top:8px">B</div>
    <div class="dj-node" style="left:80px; top:90px">C</div>
    <div class="dj-node" style="left:190px; top:105px">D</div>
    <div class="dj-node" style="left:270px; top:105px">E</div>
    <div style="padding-top:8px; text-align:center; font-size:12px; color:#94a3b8;">Đường ngắn nhất A→E: A→B(4)→E(3) = 7, thay vì A→C(2)→D(5)→E = 10.</div>
  </div>
</div>

## 14.2. Ý tưởng: "mở rộng vòng tròn từ nguồn"

Dijkstra duy trì **khoảng cách tốt nhất đã biết** từ nguồn tới từng đỉnh. Mỗi bước:
1. Lấy đỉnh **chưa "chốt" có khoảng cách nhỏ nhất** (đây chính là lúc cần **priority queue / heap**!).
2. **"Chốt"** đỉnh đó — khoảng cách của nó không thể tốt hơn nữa.
3. **"Relax"** các cạnh đi ra: nếu đi qua đỉnh này cho khoảng cách nhỏ hơn → cập nhật.

<div style="margin: 22px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes relax { 0% { background-color: rgba(245,158,11,0.4); } 100% { background-color: rgba(34,197,94,0.3); } }
    .dj-table { width: 100%; border-collapse: collapse; font-size: 12px; }
    .dj-table th { color: #94a3b8; font-weight: 700; padding: 8px; text-align: center; border-bottom: 1px solid rgba(255,255,255,0.1); }
    .dj-table td { padding: 8px; text-align: center; color: #e2e8f0; border-bottom: 1px solid rgba(255,255,255,0.05); }
    .dj-table td.fixed { color: #4ade80; font-weight: 800; }
    .dj-table td.hot { background: rgba(34,197,94,0.2); font-weight: 800; color: #86efac; }
  </style>
  <table class="dj-table">
    <tr><th>Bước</th><th>Chốt</th><th>dist(A)</th><th>dist(B)</th><th>dist(C)</th><th>dist(D)</th><th>dist(E)</th></tr>
    <tr><td>0</td><td>—</td><td class="fixed">0</td><td>∞</td><td>∞</td><td>∞</td><td>∞</td></tr>
    <tr><td>1</td><td class="hot">A</td><td class="fixed">0</td><td class="hot">4</td><td class="hot">2</td><td>∞</td><td>∞</td></tr>
    <tr><td>2</td><td class="hot">C (2)</td><td class="fixed">0</td><td class="hot">4</td><td class="fixed">2</td><td class="hot">7</td><td>∞</td></tr>
    <tr><td>3</td><td class="hot">B (4)</td><td class="fixed">0</td><td class="fixed">4</td><td class="fixed">2</td><td class="hot">7</td><td class="hot">7</td></tr>
    <tr><td>4</td><td class="hot">D (7)</td><td class="fixed">0</td><td class="fixed">4</td><td class="fixed">2</td><td class="fixed">7</td><td class="hot">7</td></tr>
    <tr><td>5</td><td class="hot">E (7)</td><td class="fixed">0</td><td class="fixed">4</td><td class="fixed">2</td><td class="fixed">7</td><td class="fixed">7</td></tr>
  </table>
  <div style="padding: 10px 16px; background: rgba(34,197,94,0.08); color: #86efac; font-size: 12px; font-weight: 600;">Kết quả: dist từ A tới mọi đỉnh: B=4, C=2, D=7, E=7.</div>
</div>

## 14.3. Code JavaScript (dùng heap làm priority queue)

```javascript
function dijkstra(graph, start) {
  const dist = {};
  const visited = new Set();
  const heap = [];          // min-heap (dùng heapInsert/heapExtract bài 11)

  for (const node of Object.keys(graph)) dist[node] = Infinity;
  dist[start] = 0;
  heapInsert(heap, [0, start]);   // [distance, node]

  while (heap.length > 0) {
    const [d, node] = heapExtract(heap);
    if (visited.has(node)) continue;  // đã chốt (bản ghi cũ trong heap)
    visited.add(node);

    for (const [neighbor, weight] of graph[node] || []) {
      const newDist = d + weight;
      if (newDist < dist[neighbor]) {
        dist[neighbor] = newDist;
        heapInsert(heap, [newDist, neighbor]);  // relax: cập nhật tốt hơn
      }
    }
  }
  return dist;
}

// graph: { A: [['B',4],['C',2]], ... }
```

## 14.4. Vì sao Dijkstra cần heap?

Ở mỗi bước, ta cần **lấy nhanh đỉnh có khoảng cách nhỏ nhất** giữa các ứng viên. Quét mảng tuyến tính là O(V) mỗi lần → tổng O(V²). Dùng **min-heap** → mỗi lần lấy chỉ O(log V) → tổng **O((V + E) log V)** — nhanh hơn hẳn với đồ thị lớn.

| Cách chọn đỉnh nhỏ nhất | Tổng độ phức tạp |
|---|---|
| Quét tuyến tính | O(V²) |
| **Min-heap (priority queue)** | **O((V + E) log V)** |

> ⚠️ **Giới hạn quan trọng:** Dijkstra **chỉ đúng khi trọng số không âm**. Nếu có cạnh âm, thuật toán có thể "chốt" sai sớm — khi đó cần thuật toán khác (Bellman-Ford). Đây là câu hỏi kinh điển trong phỏng vấn.

## 14.5. Ứng dụng thực tế

- **Google Maps:** tìm đường nhanh nhất giữa 2 điểm (đồ thị giao thông, trọng số = thời gian).
- **Định tuyến mạng:** gói tin đi qua ít "chặng" nhất (OSPF dùng biến thể Dijkstra).
- **Game AI:** NPC tìm đường đến mục tiêu trên bản đồ.
- **Hệ thống logistics:** tối ưu chi phí vận chuyển.

## 14.6. Tóm tắt

- Dijkstra tìm **đường đi ngắn nhất** trên đồ thị **trọng số không âm**.
- Lặp: lấy đỉnh nhỏ nhất (heap) → chốt → relax các cạnh đi ra.
- Độ phức tạp **O((V + E) log V)** khi dùng min-heap.
- Không đúng với cạnh âm.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Đồ thị A→B (cạnh 1), A→C (cạnh 10), B→C (cạnh 1). Dijkstra từ A cho dist(C) bằng bao nhiêu? Đáp án: 2 (A→B→C), không phải 10 trực tiếp — vì relax cập nhật C qua B trước khi C bị chốt.
</div>