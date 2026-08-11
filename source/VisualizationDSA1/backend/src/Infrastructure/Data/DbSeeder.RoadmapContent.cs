using System;
using System.Collections.Generic;
using System.Text.Json;

namespace VisualizationDSA.Infrastructure.Data
{
    public partial class DbSeeder
    {
        // ══════════════════════════════════════════════════════════════════════
        // S2 — Nội dung giàu (Rich theory), visualizerConfig, mã giả, codelab, lesson
        // ══════════════════════════════════════════════════════════════════════

        private static string BuildVisualizerConfig(string algo, string sampleInput)
        {
            var obj = new { algorithm = algo, sampleInput = sampleInput, speed = 1 };
            return JsonSerializer.Serialize(obj);
        }

        // ContentJson: 4-6 khối markdown (text) — mục tiêu, giải thích + bảng độ phức tạp, mã giả, ví dụ, mẹo.
        private static string BuildTheoryJson(string name, string difficulty, string algo, string sampleInput, string desc)
        {
            var blocks = new List<object>
            {
                new { type = "text", content = $"## 🎯 Mục tiêu bài học\n\n{desc}\n\nBài này thuộc roadmap **trực quan hóa thuật toán** — bạn sẽ thấy thuật toán chạy từng bước trên dữ liệu mẫu `{sampleInput}`." },
                new { type = "text", content = BuildTheoryExplanation(name, algo) },
                new { type = "text", content = BuildTheoryComplexityTable(name) },
                new { type = "text", content = BuildTheoryPseudo(name) },
                new { type = "text", content = $"## 📥 Ví dụ Input / Output\n\n- **Sample input (khớp Visualizer):** `{sampleInput}`\n- Nhập đúng dữ liệu này vào Visualizer để thấy thuật toán chạy từng bước khớp lý thuyết.\n- Sau khi nắm vững, mở **Codelab** bên dưới để tự cài đặt và chạy trên bộ test mẫu." },
                new { type = "text", content = $"## ⚡ Mẹo & Bẫy thường gặp\n\n- **Bẫy:** dễ nhầm chỉ số vòng lặp hoặc quên biên {name} — luôn kiểm tra mảng rỗng và mảng 1 phần tử.\n- **Mẹo:** vẽ nháp 3 bước đầu trên giấy trước khi code.\n- **Ôn luyện:** làm Quiz 10 câu ở cuối bài để ghi nhớ độ phức tạp và ý tưởng cốt lõi." }
            };
            return JsonSerializer.Serialize(blocks);
        }

        private static string BuildTheoryExplanation(string name, string algo)
        {
            return "## 💡 Giải thích trực quan\n\n" +
                   $"**{name}** — thuật toán `{algo}`. Cách dùng bài này:\n\n" +
                   "- Mở **Visualizer** ngay dưới bài giảng để xem thuật toán chạy từng bước.\n" +
                   "- Quan sát dữ liệu thay đổi sau mỗi lượt, đối chiếu với mã giả ở phần sau.\n" +
                   "- Thử đổi **sample input** để hiểu hành vi với dữ liệu khác nhau.\n\n" +
                   "Nguyên tắc: **đọc mã giả trước, chạy Visualizer sau** — vừa xem vừa đối chiếu giúp nhớ lâu hơn.";
        }

        private static string BuildTheoryComplexityTable(string name)
        {
            var map = new Dictionary<string, string>
            {
                ["Bubble Sort"] = "| Trường hợp | Độ phức tạp |\n|---|---|\n| Tốt nhất | O(n) |\n| Trung bình | O(n²) |\n| Xấu nhất | O(n²) |\n| Bộ nhớ | O(1) |\n| Ổn định | ✅ Có |",
                ["Selection Sort"] = "| Trường hợp | Độ phức tạp |\n|---|---|\n| Tốt nhất | O(n²) |\n| Trung bình | O(n²) |\n| Xấu nhất | O(n²) |\n| Bộ nhớ | O(1) |\n| Ổn định | ❌ Không |",
                ["Insertion Sort"] = "| Trường hợp | Độ phức tạp |\n|---|---|\n| Tốt nhất | O(n) |\n| Trung bình | O(n²) |\n| Xấu nhất | O(n²) |\n| Bộ nhớ | O(1) |\n| Ổn định | ✅ Có |",
                ["Quick Sort"] = "| Trường hợp | Độ phức tạp |\n|---|---|\n| Tốt nhất | O(n log n) |\n| Trung bình | O(n log n) |\n| Xấu nhất | O(n²) |\n| Bộ nhớ | O(log n) |\n| Ổn định | ❌ Không |",
                ["Merge Sort"] = "| Trường hợp | Độ phức tạp |\n|---|---|\n| Tốt nhất | O(n log n) |\n| Trung bình | O(n log n) |\n| Xấu nhất | O(n log n) |\n| Bộ nhớ | O(n) |\n| Ổn định | ✅ Có |",
                ["Heap Sort"] = "| Trường hợp | Độ phức tạp |\n|---|---|\n| Tốt nhất | O(n log n) |\n| Trung bình | O(n log n) |\n| Xấu nhất | O(n log n) |\n| Bộ nhớ | O(1) |\n| Ổn định | ❌ Không |",
                ["Duyệt BFS"] = "| Chỉ số | Giá trị |\n|---|---|\n| Thời gian | O(V + E) |\n| Không gian | O(V) |\n| Cấu trúc dữ liệu | Queue (FIFO) |\n| Ứng dụng | Đường đi ngắn nhất không trọng số |",
                ["Duyệt DFS"] = "| Chỉ số | Giá trị |\n|---|---|\n| Thời gian | O(V + E) |\n| Không gian | O(V) |\n| Cấu trúc dữ liệu | Stack / đệ quy |\n| Ứng dụng | Phát hiện chu trình, liên thông |",
                ["Dijkstra"] = "| Chỉ số | Giá trị |\n|---|---|\n| Thời gian | O((V+E) log V) |\n| Không gian | O(V) |\n| Điều kiện | Trọng số không âm |\n| Cấu trúc dữ liệu | Priority Queue |",
                ["Bellman-Ford"] = "| Chỉ số | Giá trị |\n|---|---|\n| Thời gian | O(V × E) |\n| Không gian | O(V) |\n| Điều kiện | Cho phép cạnh âm |\n| Đặc biệt | Phát hiện chu trình âm |",
                ["Cây khung nhỏ nhất (Kruskal)"] = "| Chỉ số | Giá trị |\n|---|---|\n| Thời gian | O(E log E) |\n| Không gian | O(V) |\n| Kỹ thuật | DSU (Union-Find) |\n| Đặc biệt | Sắp xếp cạnh tăng dần |"
            };
            return map.TryGetValue(name, out var table)
                ? $"## ⏱️ Độ phức tạp\n\n{table}\n\n*Bảng này nên thuộc lòng trước khi đi phỏng vấn.*"
                : "## ⏱️ Độ phức tạp\n\nTham khảo bảng độ phức tạp trong Visualizer và ghi nhớ mức tốt nhất/xấu nhất.";
        }

        private static string BuildTheoryPseudo(string name)
        {
            if (name.Contains("Bubble"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nfor i = 0 to n-2:\n    swapped = false\n    for j = 0 to n-2-i:\n        if a[j] > a[j+1]:\n            swap(a[j], a[j+1])\n            swapped = true\n    if not swapped: break\n```";
            if (name.Contains("Selection"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nfor i = 0 to n-2:\n    minIdx = i\n    for j = i+1 to n-1:\n        if a[j] < a[minIdx]: minIdx = j\n    swap(a[i], a[minIdx])\n```";
            if (name.Contains("Insertion"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nfor i = 1 to n-1:\n    key = a[i]; j = i - 1\n    while j >= 0 and a[j] > key:\n        a[j+1] = a[j]; j = j - 1\n    a[j+1] = key\n```";
            if (name.Contains("Quick"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nquickSort(a, lo, hi):\n    if lo < hi:\n        p = partition(a, lo, hi)\n        quickSort(a, lo, p-1)\n        quickSort(a, p+1, hi)\n\npartition(a, lo, hi):\n    pivot = a[hi]; i = lo\n    for j = lo to hi-1:\n        if a[j] <= pivot: swap(a[i++], a[j])\n    swap(a[i], a[hi]); return i\n```";
            if (name.Contains("Merge"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nmergeSort(a, lo, hi):\n    if lo < hi:\n        mid = (lo + hi) / 2\n        mergeSort(a, lo, mid)\n        mergeSort(a, mid+1, hi)\n        merge(a, lo, mid, hi)\n\nmerge: trộn hai nửa đã sắp vào mảng tạm rồi copy về\n```";
            if (name.Contains("Heap"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nheapSort(a):\n    buildMaxHeap(a)\n    for i = n-1 downto 1:\n        swap(a[0], a[i])\n        siftDown(a, 0, i-1)\n```";
            if (name.Contains("BFS"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nBFS(start):\n    queue = [start]; visited[start] = true\n    while queue not empty:\n        u = queue.pop_front(); visit(u)\n        for v in neighbors[u]:\n            if not visited[v]: visited[v]=true; queue.push(v)\n```";
            if (name.Contains("DFS"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nDFS(u):\n    visited[u] = true; visit(u)\n    for v in neighbors[u]:\n        if not visited[v]: DFS(v)\n```";
            if (name.Contains("Dijkstra"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nDijkstra(start):\n    dist[*] = ∞; dist[start] = 0\n    pq = [(0, start)]\n    while pq not empty:\n        (d, u) = pq.pop_min()\n        if d > dist[u]: continue\n        for (v, w) in edges[u]:\n            if dist[u] + w < dist[v]:\n                dist[v] = dist[u] + w\n                pq.push((dist[v], v))\n```";
            if (name.Contains("Bellman"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nBellmanFord(start):\n    dist[*] = ∞; dist[start] = 0\n    for i = 1 to V-1:\n        for each edge (u,v,w):\n            if dist[u] + w < dist[v]: dist[v] = dist[u] + w\n    // vòng thứ V: nếu còn relax được → chu trình âm\n```";
            if (name.Contains("Cây khung"))
                return "## 🧠 Mã giả (Pseudo-code)\n\n```text\nKruskal():\n    sort edges by weight asc\n    dsu.init(n); total = 0\n    for (u,v,w) in edges:\n        if dsu.find(u) != dsu.find(v):\n            dsu.union(u,v); total += w\n    return total\n```";
            return "## 🧠 Mã giả (Pseudo-code)\n\nXem phần giải thích và Visualizer để nắm ý tưởng cài đặt.";
        }

        // ── Lesson markdown (S5) — full theory tiếng Việt ──
        private static string SortingLessonMd(string name, string desc, string sample)
        {
            return $"# {name}\n\n{desc}\n\n## 🎯 Mục tiêu\n- Hiểu ý tưởng và các bước chạy của {name}.\n- Cài đặt được {name} bằng C#.\n- Phân tích được độ phức tạp thời gian và không gian.\n\n## 💡 Giải thích trực quan\n\nMở **Visualizer** và nhập dữ liệu mẫu `{sample}` để quan sát thuật toán chạy từng bước. Chú ý cặp phần tử đang so sánh và vùng đã sắp xếp.\n\n### Các bước chính\n1. Duyệt các phần tử theo chiến lược của thuật toán.\n2. So sánh và hoán đổi khi vi phạm thứ tự.\n3. Lặp lại đến khi không còn hoán đổi/cần xử lý.\n\n## ⏱️ Độ phức tạp\n\n{SortingComplexityNote(name)}\n\n## 🧠 Mã giả\n\nXem mã giả trong phần lý thuyết; tự viết lại trên giấy trước khi mở lời giải.\n\n## 📥 Ví dụ\n\n- **Input:** `{sample}`\n- **Output:** `1,3,5,7,9`\n\n## ⚡ Mẹo\n- Kiểm tra biên: mảng rỗng, mảng 1 phần tử, mảng đã sắp xếp, mảng giảm dần.\n- Sau khi code xong, chạy thử trong **Codelab** bên dưới với bộ test mẫu.";
        }

        private static string SortingComplexityNote(string name)
        {
            var map = new Dictionary<string, string>
            {
                ["Bubble Sort"] = "Best O(n), Average/Worst O(n²), Space O(1), stable",
                ["Selection Sort"] = "Best/Average/Worst O(n²), Space O(1), not stable",
                ["Insertion Sort"] = "Best O(n), Average/Worst O(n²), Space O(1), stable",
                ["Quick Sort"] = "Average O(n log n), Worst O(n²), Space O(log n), not stable",
                ["Merge Sort"] = "Best/Average/Worst O(n log n), Space O(n), stable",
                ["Heap Sort"] = "Best/Average/Worst O(n log n), Space O(1), not stable"
            };
            return map.TryGetValue(name, out var v) ? v : "Tham khảo bảng độ phức tạp trong lý thuyết.";
        }

        private static string GraphLessonMd(string name, string desc, string sample)
        {
            return $"# {name}\n\n{desc}\n\n## 🎯 Mục tiêu\n- Hiểu cách biểu diễn và duyệt/giải bài toán trên đồ thị.\n- Cài đặt được {name} bằng C#.\n- Phân tích độ phức tạp thời gian và không gian.\n\n## 💡 Giải thích trực quan\n\nMở **Visualizer**, nhập đồ thị mẫu `{sample}` và chạy thuật toán từng bước. Quan sát thứ tự ghé thăm đỉnh và màu trạng thái (chưa thăm/đang xét/đã xong).\n\n### Các bước chính\n1. Khởi tạo cấu trúc phụ (queue/stack/dist).\n2. Lặp chọn đỉnh kế tiếp theo chiến lược thuật toán.\n3. Cập nhật trạng thái và kết quả cho đến khi xong.\n\n## ⏱️ Độ phức tạp\n\nXem bảng độ phức tạp trong phần lý thuyết.\n\n## 🧠 Mã giả\n\nXem mã giả trong phần lý thuyết; tự viết lại trước khi mở lời giải.\n\n## 📥 Ví dụ\n\n- **Input:** `{sample}`\n- Kết quả được hiển thị tương ứng trong Visualizer.\n\n## ⚡ Mẹo\n- Luôn đánh dấu visited để tránh lặp vô hạn trên đồ thị có chu trình.\n- Chạy thử trong **Codelab** bên dưới với bộ test mẫu.";
        }

        private static string OopLessonMd(string name, string desc)
        {
            return $"# {name}\n\n{desc}\n\n## 🎯 Mục tiêu\n- Nắm vững khái niệm {name} và vì sao nó quan trọng.\n- Nhận diện code vi phạm / tuân thủ.\n- Áp dụng trong thiết kế lớp C#.\n\n## 💡 Giải thích trực quan\n\nMở **Visualizer** để xem mô phỏng trực quan khái niệm này trên ví dụ cụ thể, quan sát mối quan hệ giữa các đối tượng.\n\n### Nội dung chính\n1. Định nghĩa và vai trò của {name}.\n2. Ví dụ mã C# minh họa.\n3. Bẫy thường gặp và cách tránh.\n\n## 🧠 Ví dụ mã\n\nTham khảo mã giả/mã C# trong phần lý thuyết.\n\n## 📥 Thực hành\n\nSau khi đọc lý thuyết, làm **Quiz** và thử sức trong **Codelab** bên dưới.\n\n## ⚡ Mẹo\n- Luôn đối chiếu code với mô phỏng Visualizer để hiểu bản chất.\n- Tự viết lại ví dụ trên giấy trước khi code.";
        }

        // ── Initial code cho codelab ──
        private static string SortingInitialCode()
        {
            return "using System;\n\npublic class Solution {\n    public int[] Sort(int[] arr) {\n        // Viết code thuật toán sắp xếp của bạn tại đây\n        return arr;\n    }\n}";
        }

        private static string GraphBfsInitialCode()
        {
            return "using System;\nusing System.Collections.Generic;\n\npublic class Solution {\n    // edges dạng \"u-v,u-v\" — trả về thứ tự duyệt BFS từ đỉnh 0\n    public List<int> Bfs(string edges, int n) {\n        // Viết code của bạn tại đây\n        return new List<int>();\n    }\n}";
        }

        private static string GraphDfsInitialCode()
        {
            return "using System;\nusing System.Collections.Generic;\n\npublic class Solution {\n    // edges dạng \"u-v,u-v\" — trả về thứ tự duyệt DFS từ đỉnh 0\n    public List<int> Dfs(string edges, int n) {\n        // Viết code của bạn tại đây\n        return new List<int>();\n    }\n}";
        }

        private static string DijkstraInitialCode()
        {
            return "using System;\nusing System.Collections.Generic;\n\npublic class Solution {\n    // edges dạng \"u-v-w,u-v-w\" — trả về mảng dist[] từ đỉnh 0\n    public int[] Dijkstra(string edges, int n) {\n        // Viết code của bạn tại đây\n        return new int[n];\n    }\n}";
        }

        private static string KruskalInitialCode()
        {
            return "using System;\nusing System.Collections.Generic;\n\npublic class Solution {\n    // edges dạng \"u-v-w,u-v-w\" — trả về tổng trọng số cây khung nhỏ nhất\n    public int MstWeight(string edges, int n) {\n        // Viết code của bạn tại đây\n        return 0;\n    }\n}";
        }

        private static string OopEncapsulationInitialCode()
        {
            return "using System;\n\npublic class BankAccount {\n    private int balance;\n\n    public BankAccount(int initial) { balance = initial; }\n\n    public void Deposit(int amount) { /* Viết code */ }\n\n    public void Withdraw(int amount) { /* Viết code (balance không âm) */ }\n\n    public int GetBalance() { return balance; }\n}";
        }

        private static string OopPolymorphismInitialCode()
        {
            return "using System;\n\npublic abstract class Shape {\n    public abstract string Draw();\n}\n\npublic class Circle : Shape {\n    public override string Draw() { return \"Drawing Circle\"; }\n}\n\n// Thêm Rectangle, Triangle kế thừa Shape\n\npublic class Solution {\n    public string Run(string shapeName) {\n        Shape s = shapeName == \"Circle\" ? new Circle() : null;\n        return s == null ? \"Unknown\" : s.Draw();\n    }\n}";
        }

        private static string OopGenericInitialCode()
        {
            return "using System;\n\n// Viết lớp tuân thủ yêu cầu trong mô tả\npublic class Solution {\n    public string Execute(string input) {\n        // Viết code của bạn tại đây\n        return \"\";\n    }\n}";
        }
    }
}
