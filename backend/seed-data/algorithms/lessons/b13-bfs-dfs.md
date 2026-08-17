# 13. Đồ thị — BFS & DFS

Đồ thị (graph) là mô hình hóa mạnh nhất cho các mối quan hệ: mạng xã hội, bản đồ giao thông, mạng máy tính, dependencies... Bài này giới thiệu đồ thị và **hai cách duyệt cơ bản nhất**: BFS (theo chiều rộng) và DFS (theo chiều sâu).

## 13.1. Đồ thị là gì?

Đồ thị gồm **đỉnh (vertex/node)** và **cạnh (edge)** nối chúng. Cạnh có thể **có hướng** (mũi tên, như follow trên mạng xã hội) hoặc **vô hướng** (như tình bạn), và có thể có **trọng số** (khoảng cách, chi phí).

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes graphNode { 0% { opacity: 0; transform: scale(.8); } 100% { opacity: 1; transform: scale(1); } }
    .graph-grid { display: flex; gap: 16px; padding: 18px; background: rgba(10,9,18,0.85); justify-content: center; flex-wrap: wrap; }
    .graph-box { text-align: center; }
    .graph-box h5 { margin: 0 0 8px; font-size: 12px; color: #94a3b8; font-weight: 700; }
    .gn { display: inline-flex; width: 34px; height: 34px; border-radius: 50%; align-items: center; justify-content: center; font-weight: 800; font-size: 13px; color: #e9d5ff; background: rgba(168,85,247,0.25); border: 2px solid #a855f7; animation: graphNode .4s ease both; }
    .gn.small { width: 26px; height: 26px; font-size: 11px; }
  </style>
  <div class="graph-grid">
    <div style="text-align:center">
      <h5>Vô hướng</h5>
      <div style="position:relative; width:220px; height:90px;">
        <div style="position:absolute; left:10px; top:10px"><div class="gn">A</div></div>
        <div style="position:absolute; left:170px; top:10px"><div class="gn">B</div></div>
        <div style="position:absolute; left:90px; top:55px"><div class="gn small">C</div></div>
        <svg style="position:absolute; inset:0; width:100%; height:100%">
          <line x1="44" y1="27" x2="184" y2="27" stroke="#a855f7" stroke-width="2"/>
          <line x1="60" y1="35" x2="105" y2="60" stroke="#a855f7" stroke-width="2"/>
          <line x1="185" y1="35" x2="125" y2="60" stroke="#a855f7" stroke-width="2"/>
        </svg>
      </div>
    </div>
    <div style="text-align:center">
      <h5>Có hướng + trọng số</h5>
      <div style="position:relative; width:220px; height:90px;">
        <div style="position:absolute; left:10px; top:10px"><div class="gn">Hà Nội</div></div>
        <div style="position:absolute; left:150px; top:10px"><div class="gn">Đà Nẵng</div></div>
        <svg style="position:absolute; inset:0; width:100%; height:100%">
          <defs><marker id="arr" markerWidth="6" markerHeight="6" refX="5" refY="3" orient="auto"><path d="M0,0 L6,3 L0,6 z" fill="#a855f7"/></marker></defs>
          <line x1="75" y1="30" x2="155" y2="30" stroke="#a855f7" stroke-width="2" marker-end="url(#arr)"/>
        </svg>
        <div style="position:absolute; left:95px; top:6px; color:#fbbf24; font-size:11px; font-weight:800">~765km</div>
      </div>
    </div>
  </div>
</div>

Trong code, đồ thị thường lưu bằng **danh sách kề (adjacency list)**: mỗi đỉnh → mảng các đỉnh kề.

```javascript
// Đồ thị dạng adjacency list
const graph = {
  A: ['B', 'C'],
  B: ['A', 'D'],
  C: ['A', 'E'],
  D: ['B'],
  E: ['C'],
};
```

## 13.2. BFS — Duyệt theo chiều rộng (hàng đợi)

BFS duyệt **theo từng "tầng"**: bắt đầu từ đỉnh nguồn, rồi tất cả hàng xóm của nó, rồi hàng xóm của hàng xóm... Dùng **hàng đợi (Queue — FIFO)**. BFS cho ta **đường đi ngắn nhất theo số cạnh** (trên đồ thị không trọng số).

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes bfsVisit { 0% { opacity: 0; transform: scale(.7); } 100% { opacity: 1; transform: scale(1); } }
    .bfs-stage { padding: 18px; background: rgba(10,9,18,0.85); }
    .bfs-grid { display: flex; gap: 12px; justify-content: center; align-items: center; flex-wrap: wrap; }
    .bfs-node { width: 44px; height: 44px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 800; background: rgba(148,163,184,0.2); color: #e2e8f0; border: 2px solid rgba(148,163,184,0.4); animation: bfsVisit .4s ease both; }
    .bfs-node.t0 { background: rgba(245,158,11,0.3); border-color: #f59e0b; color: #fde68a; }
    .bfs-node.t1 { background: rgba(34,197,94,0.3); border-color: #22c55e; color: #86efac; }
    .bfs-node.t2 { background: rgba(59,130,246,0.3); border-color: #3b82f6; color: #93c5fd; }
    .bfs-legend { display: flex; gap: 14px; justify-content: center; padding-top: 12px; font-size: 11px; font-weight: 700; }
  </style>
  <div class="bfs-stage">
    <div class="bfs-grid">
      <div class="bfs-node t0">A</div>
      <div class="bfs-node t1">B</div>
      <div class="bfs-node t1">C</div>
      <div class="bfs-node t2">D</div>
      <div class="bfs-node t2">E</div>
    </div>
    <div class="bfs-legend">
      <span style="color:#f59e0b">Tầng 0 (A)</span>
      <span style="color:#22c55e">Tầng 1 (B, C)</span>
      <span style="color:#3b82f6">Tầng 2 (D, E)</span>
    </div>
    <div style="padding-top:8px; text-align:center; font-size:12px; color:#94a3b8;">Thứ tự BFS: A → B → C → D → E. Duyệt theo tầng, dùng hàng đợi.</div>
  </div>
</div>

```javascript
function bfs(graph, start) {
  const visited = new Set();
  const queue = [start];          // hàng đợi FIFO
  const order = [];
  visited.add(start);

  while (queue.length > 0) {
    const node = queue.shift();
    order.push(node);
    for (const neighbor of graph[node] || []) {
      if (!visited.has(neighbor)) {
        visited.add(neighbor);
        queue.push(neighbor);
      }
    }
  }
  return order;
}

console.log(bfs(graph, 'A')); // ['A', 'B', 'C', 'D', 'E']
```

## 13.3. DFS — Duyệt theo chiều sâu (ngăn xếp)

DFS duyệt **"càng sâu càng tốt"** trước khi quay lui: đi xuống một nhánh tới tận cùng, rồi lùi lại đi nhánh khác. Dùng **ngăn xếp (Stack — LIFO)**, hoặc viết bằng **đệ quy** (mượn call stack).

<div style="margin: 24px 0; border: 1px solid rgba(168,85,247,0.2); border-radius: 14px; overflow: hidden;">
  <style>
    @keyframes dfsDeep { 0% { opacity: 0; transform: translateY(-4px); } 100% { opacity: 1; transform: translateY(0); } }
    .dfs-stage { padding: 18px; background: rgba(10,9,18,0.85); }
    .dfs-path { display: flex; flex-wrap: wrap; gap: 6px; justify-content: center; align-items: center; }
    .dfs-cell { padding: 8px 14px; border-radius: 8px; font-weight: 800; font-size: 13px; background: rgba(168,85,247,0.22); color: #e9d5ff; border: 1px solid rgba(168,85,247,0.4); animation: dfsDeep .5s ease both; }
    .dfs-cell.back { background: rgba(148,163,184,0.15); color: #94a3b8; border-style: dashed; }
  </style>
  <div class="dfs-stage">
    <div class="dfs-path">
      <div class="dfs-cell" style="animation-delay:.05s">A</div>
      <div class="dfs-cell" style="animation-delay:.15s">B</div>
      <div class="dfs-cell" style="animation-delay:.25s">D</div>
      <div class="dfs-cell back" style="animation-delay:.35s">← quay lui B</div>
      <div class="dfs-cell" style="animation-delay:.45s">← quay lui A → C</div>
      <div class="dfs-cell" style="animation-delay:.55s">E</div>
    </div>
    <div style="padding-top:10px; text-align:center; font-size:12px; color:#94a3b8;">Thứ tự DFS: A → B → D (tận cùng) → quay lui → C → E. Đi sâu tối đa trước, dùng ngăn xếp/đệ quy.</div>
  </div>
</div>

```javascript
function dfs(graph, start) {
  const visited = new Set();
  const order = [];

  function visit(node) {
    visited.add(node);
    order.push(node);
    for (const neighbor of graph[node] || []) {
      if (!visited.has(neighbor)) visit(neighbor); // đệ quy đi sâu
    }
  }
  visit(start);
  return order;
}

console.log(dfs(graph, 'A')); // ['A', 'B', 'D', 'C', 'E']
```

## 13.4. So sánh BFS và DFS

| Tiêu chí | BFS | DFS |
|---|---|---|
| Cấu trúc | **Hàng đợi** (hoặc duyệt theo tầng) | **Ngăn xếp** (hoặc đệ quy) |
| Đường đi ngắn nhất (số cạnh) | ✅ tìm được | ❌ không đảm bảo |
| Bộ nhớ | O(chiều rộng) — nhiều khi đồ thị rộng | O(chiều sâu) — ít hơn thường |
| Dùng khi | Tìm đường ngắn nhất, mức gần nhất | Duyệt hết, topological, backtracking |
| Đệ quy | Khó hơn | **Tự nhiên** |

> 💡 **Mẹo ghi nhớ:** BFS = "B" như **B**readth (rộng) — mở rộng theo tầng. DFS = "D" như **D**epth (sâu) — chui xuống tận cùng. BFS dùng **hàng đợi** (vào trước ra trước — giống xếp hàng), DFS dùng **ngăn xếp/đệ quy** (vào sau ra trước — giống chồng đĩa).

## 13.5. Tóm tắt

- Đồ thị = đỉnh + cạnh (có hướng/vô hướng, có/không trọng số); lưu bằng **adjacency list**.
- **BFS** duyệt theo tầng (Queue) → tìm **đường đi ngắn nhất theo số cạnh**.
- **DFS** duyệt sâu tối đa (Stack/đệ quy) → duyệt toàn bộ, nền tảng cho nhiều thuật toán nâng cao.
- Cả hai đều **O(V + E)** thời gian, cần mảng `visited` để tránh lặp vô hạn.

<div style="margin: 24px 0; padding: 16px 18px; background: rgba(34,197,94,0.08); border-left: 4px solid #22c55e; border-radius: 8px; color: #86efac; font-size: 14px;">
  <strong>Bài tập tự kiểm tra:</strong> Đồ thị A→(B,C), B→(D), C→(E). BFS từ A ghé thăm theo thứ tự nào? Đáp án: A, B, C, D, E (theo tầng: tầng 0 A, tầng 1 B,C, tầng 2 D,E).
</div>