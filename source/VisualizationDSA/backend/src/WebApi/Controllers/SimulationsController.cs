using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using VisualizationDSA.WebApi.Filters;
using Microsoft.AspNetCore.Authorization;

namespace VisualizationDSA.WebApi.Controllers
{
    /// <summary>Màn 08/09 — /api/v1/simulations catalog (25 key). Engine chạy client-side.</summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/simulations")]
    public class SimulationsController : ControllerBase
    {
        private sealed record Sim(string Key, string Title, string DataStructure, string Category, string Level, string Best, string Avg, string Worst, string Space, string[] Tags);

        private static readonly Sim[] Catalog =
        {
            new("sort.bubble","Bubble Sort — Sắp xếp nổi bọt","Array","algorithm","basic","O(n)","O(n²)","O(n²)","O(1)",new[]{"sorting","array"}),
            new("sort.selection","Selection Sort — Sắp xếp chọn","Array","algorithm","basic","O(n²)","O(n²)","O(n²)","O(1)",new[]{"sorting","array"}),
            new("sort.insertion","Insertion Sort — Sắp xếp chèn","Array","algorithm","basic","O(n)","O(n²)","O(n²)","O(1)",new[]{"sorting","array"}),
            new("sort.merge","Merge Sort — Sắp xếp trộn","Array","algorithm","advanced","O(n log n)","O(n log n)","O(n log n)","O(n)",new[]{"sorting","divide-conquer"}),
            new("sort.quick","Quick Sort — Sắp xếp nhanh","Array","algorithm","advanced","O(n log n)","O(n log n)","O(n²)","O(log n)",new[]{"sorting","divide-conquer"}),
            new("sort.heap","Heap Sort — Sắp xếp vun đống","Heap","algorithm","advanced","O(n log n)","O(n log n)","O(n log n)","O(1)",new[]{"sorting","heap"}),
            new("sort.radix","Radix Sort — Sắp xếp theo cơ số","Array","algorithm","advanced","O(d·n)","O(d·n)","O(d·n)","O(n+k)",new[]{"sorting","linear"}),
            new("search.linear","Linear Search — Tìm kiếm tuyến tính","Array","algorithm","basic","O(1)","O(n)","O(n)","O(1)",new[]{"searching","array"}),
            new("search.binary","Binary Search — Tìm kiếm nhị phân","Sorted Array","algorithm","basic","O(1)","O(log n)","O(log n)","O(1)",new[]{"searching","divide-conquer"}),
            new("search.jump","Jump Search — Tìm kiếm nhảy","Sorted Array","algorithm","advanced","O(1)","O(√n)","O(√n)","O(1)",new[]{"searching"}),
            new("search.interpolation","Interpolation Search — Nội suy","Sorted Array","algorithm","advanced","O(1)","O(log log n)","O(n)","O(1)",new[]{"searching"}),
            new("tree.bst","Binary Search Tree — Cây nhị phân tìm kiếm","Tree","structure","basic","O(log n)","O(log n)","O(n)","O(n)",new[]{"tree","bst"}),
            new("tree.heap-max","Max-Heap — Đống tối đa","Heap","structure","advanced","O(log n)","O(log n)","O(log n)","O(n)",new[]{"heap","priority-queue"}),
            new("tree.heap-min","Min-Heap — Đống tối thiểu","Heap","structure","advanced","O(log n)","O(log n)","O(log n)","O(n)",new[]{"heap","priority-queue"}),
            new("tree.avl","AVL Tree — Cây cân bằng","Tree","structure","advanced","O(log n)","O(log n)","O(log n)","O(n)",new[]{"tree","balanced"}),
            new("graph.bfs","BFS — Duyệt theo chiều rộng","Graph","algorithm","basic","O(V+E)","O(V+E)","O(V+E)","O(V)",new[]{"graph","bfs"}),
            new("graph.dfs","DFS — Duyệt theo chiều sâu","Graph","algorithm","basic","O(V+E)","O(V+E)","O(V+E)","O(V)",new[]{"graph","dfs"}),
            new("graph.dijkstra","Dijkstra — Đường đi ngắn nhất","Graph","algorithm","advanced","O(V²)","O(E log V)","O(E log V)","O(V)",new[]{"graph","shortest-path"}),
            new("graph.bellman-ford","Bellman-Ford — Đường đi ngắn nhất","Graph","algorithm","advanced","O(V·E)","O(V·E)","O(V·E)","O(V)",new[]{"graph","shortest-path"}),
            new("graph.topological","Topological Sort — Thứ tự topo","Graph","algorithm","advanced","O(V+E)","O(V+E)","O(V+E)","O(V)",new[]{"graph","dag"}),
            new("graph.kruskal","Kruskal — Cây khung nhỏ nhất","Graph","algorithm","advanced","O(E log E)","O(E log E)","O(E log E)","O(V+E)",new[]{"graph","mst"}),
            new("graph.prim","Prim — Cây khung nhỏ nhất","Graph","algorithm","advanced","O(V²)","O(E log V)","O(E log V)","O(V)",new[]{"graph","mst"}),
            new("stack-queue","Stack & Queue — Ngăn xếp và hàng đợi","Linear","structure","basic","O(1)","O(1)","O(1)","O(n)",new[]{"linear","stack","queue"}),
            new("hash.chaining","Hash Table — Bảng băm (chaining)","Hash Table","structure","advanced","O(1)","O(1)","O(n)","O(n+k)",new[]{"hash","map"}),
            new("dp.knapsack","Knapsack 0/1 — Bài toán cái túi","Table","algorithm","advanced","O(nW)","O(nW)","O(nW)","O(W)",new[]{"dp","optimization"}),
        };

        [HttpGet]
        public IActionResult List()
        {
            var items = Catalog.Select(s => new
            {
                key = s.Key, title = s.Title, dataStructure = s.DataStructure, category = s.Category, level = s.Level,
                complexity = new { best = s.Best, average = s.Avg, worst = s.Worst, space = s.Space },
                tags = s.Tags, demoAllowed = true,
            }).ToList();
            return Ok(new { items, page = 1, pageSize = 25, total = items.Count, totalPages = 1 });
        }

        [HttpGet("{key}")]
        public IActionResult Detail(string key)
        {
            var s = Catalog.FirstOrDefault(x => x.Key == key.ToLowerInvariant());
            if (s == null) return NotFound();
            return Ok(new
            {
                key = s.Key, title = s.Title, dataStructure = s.DataStructure, category = s.Category, level = s.Level,
                complexity = new { best = s.Best, average = s.Avg, worst = s.Worst, space = s.Space },
                tags = s.Tags, demoAllowed = true,
                inputSchema = new { type = "array", description = "Mảng đầu vào", defaultInput = new int[] { 5, 3, 8, 1, 9, 2 } },
                pseudocode = new string[] { "procedure " + s.Key, "  ..." },
            });
        }

        [HttpGet("{key}/schema")]
        public IActionResult Schema(string key) => Ok(new { type = "array", description = "Mảng đầu vào", defaultInput = new int[] { 5, 3, 8, 1, 9, 2 } });

        [HttpGet("public/{key}/run")]
        [AllowAnonymous]
        public IActionResult PublicRun(string key) => Ok(new { key, frames = new List<object>(), done = true });
    }
}
