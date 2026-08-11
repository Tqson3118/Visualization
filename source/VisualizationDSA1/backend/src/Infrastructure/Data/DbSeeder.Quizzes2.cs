using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Domain.Entities;

namespace VisualizationDSA.Infrastructure.Data
{
    public partial class DbSeeder
    {
        // S3 — 6 quiz nền còn lại (mở rộng đủ 10 câu/bài)
        private async Task SeedQuizzesExtendedAsync()
        {
            var ll = await EnsureQuizAsync("Trắc nghiệm Danh sách liên kết", "Đánh giá hiểu biết về Linked List và con trỏ", "dsa", 1, 50);
            if (ll.Questions.Count < 8) FillLinkedListQuiz(ll);

            var sq = await EnsureQuizAsync("Trắc nghiệm Ngăn xếp & Hàng đợi", "Phân biệt nguyên lý LIFO và FIFO", "dsa", 1, 50);
            if (sq.Questions.Count < 8) FillStackQueueQuiz(sq);

            var tree = await EnsureQuizAsync("Trắc nghiệm Duyệt cây Nhị phân", "Xác định thứ tự duyệt cây DFS và BFS", "dsa", 2, 80);
            if (tree.Questions.Count < 8) FillTreeQuiz(tree);

            var graph = await EnsureQuizAsync("Trắc nghiệm Đồ thị & Dijkstra", "Phân tích thuật toán đường đi ngắn nhất", "graph", 3, 120);
            if (graph.Questions.Count < 8) FillGraphQuiz(graph);

            var dp = await EnsureQuizAsync("Trắc nghiệm Quy hoạch động", "Phân biệt Memoization và Tabulation", "dsa", 3, 140);
            if (dp.Questions.Count < 8) FillDpQuiz(dp);

            var sys = await EnsureQuizAsync("Trắc nghiệm System Design & Multithreading", "Phát hiện Race Condition và Deadlock", "system", 3, 150);
            if (sys.Questions.Count < 8) FillSystemQuiz(sys);

            await _context.SaveChangesAsync();
        }

        private static void FillLinkedListQuiz(Quiz q)
        {
            q.AddQuestion("Trường hợp nào dẫn tới Memory Leak trong Linked List?", new[] { "Gán head = head.next mà không giải phóng node cũ", "Duyệt qua danh sách", "Tạo node mới", "Đếm số node" }, 0, "Mất con trỏ tới node mà không deallocate → rò rỉ bộ nhớ.");
            q.AddQuestion("Chèn phần tử vào đầu Linked List có độ phức tạp bao nhiêu?", new[] { "O(1)", "O(n)", "O(log n)", "O(n²)" }, 0, "Chỉ cần đổi con trỏ head → O(1).");
            q.AddQuestion("Truy cập phần tử thứ k trong Linked List có độ phức tạp?", new[] { "O(n)", "O(1)", "O(log n)", "O(k)" }, 0, "Phải duyệt tuần tự từ đầu đến vị trí k → O(n).");
            q.AddQuestion("Doubly Linked List khác Singly Linked List ở điểm gì?", new[] { "Node có con trỏ next và prev", "Chỉ có next", "Lưu theo mảng", "Không có head" }, 0, "Node doubly có hai con trỏ giúp duyệt hai chiều.");
            q.AddQuestion("Ưu điểm của Linked List so với mảng là gì?", new[] { "Chèn/xóa giữa O(1) khi đã có con trỏ", "Random access O(1)", "Ít bộ nhớ hơn", "Tự sắp xếp" }, 0, "Chèn/xóa khi đã có node tham chiếu là O(1), không cần dịch chuyển.");
            q.AddQuestion("Nhược điểm chính của Linked List?", new[] { "Không random access, tốn bộ nhớ con trỏ", "Không lưu được số nguyên", "Chỉ dùng được trong C", "Quá nhanh" }, 0, "Phải duyệt tuần tự và mỗi node tốn thêm bộ nhớ con trỏ.");
            q.AddQuestion("Thao tác tìm node có giá trị x trong Linked List có độ phức tạp?", new[] { "O(n)", "O(1)", "O(log n)", "O(n²)" }, 0, "Tìm kiếm tuần tự từ đầu.");
            q.AddQuestion("Circular Linked List là gì?", new[] { "Node cuối trỏ về head tạo vòng", "Danh sách có chu trình lỗi", "Không có node", "Chỉ có một node" }, 0, "Node cuối trỏ lại node đầu, thường dùng cho round-robin.");
            q.AddQuestion("Để xóa node giữa danh sách, ta cần gì?", new[] { "Con trỏ tới node trước nó (prev)", "Không cần gì", "Đếm tổng số node", "Sắp xếp lại" }, 0, "Cần node trước để nối lại liên kết sau khi xóa.");
            q.AddQuestion("Khi nào nên chọn Linked List thay vì Array?", new[] { "Chèn/xóa thường xuyên, kích thước thay đổi linh hoạt", "Cần truy cập ngẫu nhiên nhanh", "Dữ liệu nhỏ cố định", "Cần sắp xếp sẵn" }, 0, "Danh sách thay đổi kích thước thường xuyên phù hợp Linked List.");
        }

        private static void FillStackQueueQuiz(Quiz q)
        {
            q.AddQuestion("Cấu trúc dữ liệu nào tuân theo nguyên lý LIFO?", new[] { "Queue", "Stack", "Array", "Graph" }, 1, "Stack vào sau ra trước (LIFO).");
            q.AddQuestion("Nguyên lý của Queue là gì?", new[] { "FIFO", "LIFO", "LILO", "Random" }, 0, "Queue vào trước ra trước (FIFO).");
            q.AddQuestion("Thao tác Push trong Stack làm gì?", new[] { "Đẩy phần tử vào đỉnh", "Lấy phần tử ra", "Xem phần tử đỉnh", "Xóa toàn bộ" }, 0, "Push thêm phần tử lên đỉnh stack.");
            q.AddQuestion("Thao tác Peek trong Stack?", new[] { "Xem phần tử đỉnh mà không xóa", "Xóa phần tử đỉnh", "Đẩy phần tử vào", "Đếm số phần tử" }, 0, "Peek/Top chỉ đọc, không thay đổi.");
            q.AddQuestion("Ứng dụng nào KHÔNG phù hợp dùng Stack?", new[] { "Quản lý lượt khám bệnh (ai đến trước khám trước)", "Undo/Redo", "Kiểm tra dấu ngoặc hợp lệ", "Back/Forward trình duyệt" }, 0, "Xếp hàng khám bệnh phải dùng Queue (FIFO).");
            q.AddQuestion("Ứng dụng điển hình của Queue?", new[] { "In ấn theo thứ tự yêu cầu", "Undo/Redo", "Gọi hàm đệ quy", "Duyệt DFS" }, 0, "Hàng đợi in/job scheduling dùng FIFO.");
            q.AddQuestion("Độ phức tạp Push/Pop của Stack là?", new[] { "O(1)", "O(n)", "O(log n)", "O(n²)" }, 0, "Chỉ thao tác đỉnh stack → O(1).");
            q.AddQuestion("Kiểm tra dấu ngoặc hợp lệ ( ) [ ] { } nên dùng cấu trúc nào?", new[] { "Stack", "Queue", "Array", "Heap" }, 0, "Stack khớp ngoặc mở/đóng theo thứ tự gần nhất.");
            q.AddQuestion("Deque (Double-ended queue) cho phép gì?", new[] { "Thêm/xóa cả hai đầu", "Chỉ thêm đầu trước", "Chỉ xóa đầu sau", "Truy cập ngẫu nhiên" }, 0, "Deque thao tác được cả front và back.");
            q.AddQuestion("Priority Queue sắp xếp phần tử theo gì?", new[] { "Độ ưu tiên", "Thứ tự chèn", "Thứ tự từ điển", "Ngẫu nhiên" }, 0, "Priority Queue lấy phần tử có độ ưu tiên cao nhất trước.");
        }

        private static void FillTreeQuiz(Quiz q)
        {
            q.AddQuestion("In-order traversal trên BST cho kết quả gì?", new[] { "Giảm dần", "Tăng dần", "Ngẫu nhiên", "Chỉ số lẻ" }, 1, "In-order trên BST duyệt trái→gốc→phải nên ra dãy tăng dần.");
            q.AddQuestion("Pre-order traversal duyệt theo thứ tự nào?", new[] { "Gốc → trái → phải", "Trái → gốc → phải", "Trái → phải → gốc", "Phải → gốc → trái" }, 0, "Pre-order: N-L-R.");
            q.AddQuestion("Post-order traversal duyệt theo thứ tự nào?", new[] { "Trái → phải → gốc", "Gốc → trái → phải", "Trái → gốc → phải", "Phải → trái → gốc" }, 0, "Post-order: L-R-N.");
            q.AddQuestion("Chiều cao của cây nhị phân cân bằng với n node?", new[] { "O(log n)", "O(n)", "O(1)", "O(n²)" }, 0, "Cân bằng giữ chiều cao ~log n.");
            q.AddQuestion("BST bị lệch (skewed) tìm kiếm O(n) — vì sao?", new[] { "Chiều cao bằng n khi toàn bộ node về một phía", "Không có con trỏ", "Phải duyệt theo BFS", "Không lưu giá trị" }, 0, "Nếu chèn tăng dần, cây thành linked list → O(n).");
            q.AddQuestion("Level-order traversal dùng cấu trúc dữ liệu nào?", new[] { "Queue", "Stack", "Heap", "HashSet" }, 0, "BFS duyệt theo tầng dùng queue.");
            q.AddQuestion("Trong BST, node bên trái luôn có giá trị thế nào?", new[] { "Nhỏ hơn node gốc", "Lớn hơn node gốc", "Bằng node gốc", "Ngẫu nhiên" }, 0, "Nhánh trái nhỏ hơn gốc, nhánh phải lớn hơn gốc.");
            q.AddQuestion("Tìm kiếm trên BST cân bằng có độ phức tạp?", new[] { "O(log n)", "O(n)", "O(1)", "O(n²)" }, 0, "Mỗi bước giảm một nửa không gian tìm kiếm.");
            q.AddQuestion("Cây nhị phân hoàn chỉnh (complete) phù hợp lưu bằng gì?", new[] { "Mảng liên tiếp", "Linked list", "Graph", "Hash table" }, 0, "Heap thường lưu bằng mảng vì tính chất complete tree.");
            q.AddQuestion("Độ phức tạp chèn vào BST cân bằng?", new[] { "O(log n)", "O(n)", "O(1)", "O(n log n)" }, 0, "Chèn xuống theo nhánh đúng → O(log n) khi cân bằng.");
        }

        private static void FillGraphQuiz(Quiz q)
        {
            q.AddQuestion("Thuật toán Dijkstra không hoạt động chính xác khi nào?", new[] { "Đồ thị có hướng", "Đồ thị vô hướng", "Đồ thị có cạnh trọng số âm", "Đồ thị dày" }, 2, "Dijkstra tham ăn không xử lý đúng cạnh âm.");
            q.AddQuestion("BFS tìm đường đi ngắn nhất (ít cạnh) trên đồ thị nào?", new[] { "Đồ thị không trọng số", "Đồ thị có trọng số âm", "Đồ thị có trọng số dương", "Đồ thị có chu trình âm" }, 0, "BFS duyệt theo tầng nên đường ngắn nhất theo số cạnh.");
            q.AddQuestion("Cấu trúc dữ liệu chính của BFS?", new[] { "Queue", "Stack", "Heap", "Mảng" }, 0, "BFS dùng hàng đợi (FIFO).");
            q.AddQuestion("Cấu trúc dữ liệu chính của DFS (bản lặp)?", new[] { "Stack", "Queue", "Priority Queue", "HashSet" }, 0, "DFS dùng ngăn xếp (hoặc đệ quy).");
            q.AddQuestion("Độ phức tạp BFS/DFS trên danh sách kề?", new[] { "O(V + E)", "O(V)", "O(E)", "O(V × E)" }, 0, "Duyệt mọi đỉnh và mọi cạnh đúng một lần.");
            q.AddQuestion("Độ phức tạp Dijkstra với heap?", new[] { "O((V + E) log V)", "O(V²)", "O(E)", "O(V)" }, 0, "Với priority queue, mỗi thao tác O(log V).");
            q.AddQuestion("Đồ thị có chu trình âm thì Bellman-Ford làm gì?", new[] { "Phát hiện và báo lỗi", "Trả kết quả vẫn đúng", "Chạy vô hạn", "Đổi sang BFS" }, 0, "Bellman-Ford phát hiện chu trình âm sau V-1 lần lặp.");
            q.AddQuestion("Cây khung nhỏ nhất nối tất cả đỉnh với điều kiện gì?", new[] { "Tổng trọng số nhỏ nhất, không chu trình", "Số đỉnh lớn nhất", "Nhiều cạnh nhất", "Tổng trọng số lớn nhất" }, 0, "MST nối mọi đỉnh với tổng trọng số tối thiểu.");
            q.AddQuestion("Thuật toán Kruskal xử lý cạnh theo thứ tự nào?", new[] { "Sắp xếp cạnh tăng dần trọng số", "Ngẫu nhiên", "Theo thứ tự nhập", "Giảm dần trọng số" }, 0, "Kruskal sort cạnh tăng dần và dùng DSU để tránh chu trình.");
            q.AddQuestion("Adjacency list phù hợp đồ thị nào?", new[] { "Đồ thị thưa (sparse)", "Đồ thị dày đặc", "Chỉ đồ thị vô hướng", "Chỉ đồ thị không trọng số" }, 0, "Adjacency list tiết kiệm bộ nhớ cho đồ thị thưa.");
        }

        private static void FillDpQuiz(Quiz q)
        {
            q.AddQuestion("Tabulation trong Quy hoạch động là phương pháp gì?", new[] { "Top-down đệ quy", "Bottom-up điền bảng", "Greedy tham ăn", "Brute force" }, 1, "Tabulation điền bảng từ bài toán nhỏ lên lớn (bottom-up).");
            q.AddQuestion("Memoization là gì?", new[] { "Lưu kết quả bài toán con khi đệ quy (top-down)", "Xóa cache", "Sắp xếp mảng", "Tăng tốc CPU" }, 0, "Memoization cache kết quả đệ quy để tránh tính lại.");
            q.AddQuestion("Bài toán Fibonacci bằng DP tối ưu thời gian thế nào?", new[] { "O(n) thay vì O(2^n)", "O(1)", "O(n²)", "O(log n) luôn" }, 0, "Lưu kết quả từng bước → O(n) thay vì đệ quy mũ.");
            q.AddQuestion("Điều kiện tiên quyết để áp dụng DP?", new[] { "Bài toán con chồng lấn + cấu trúc con tối ưu", "Đồ thị có trọng số âm", "Dữ liệu đã sắp xếp", "Không có ràng buộc" }, 0, "DP cần overlapping subproblems và optimal substructure.");
            q.AddQuestion("Bài toán Knapsack điển hình giải bằng?", new[] { "DP 2 chiều chọn/không chọn món đồ", "Chỉ greedy", "Chỉ brute force", "Chỉ BFS" }, 0, "Knapsack dùng bảng dp[i][w] chọn hoặc bỏ vật i.");
            q.AddQuestion("LIS (Longest Increasing Subsequence) có lời giải DP O(n²) — tối ưu hơn được không?", new[] { "Có, O(n log n) với binary search + patience", "Không", "Chỉ O(n³)", "Chỉ O(1)" }, 0, "Dùng mảng tails + binary search cho O(n log n).");
            q.AddQuestion("Độ phức tạp thời gian giải DP bài toán Edit Distance?", new[] { "O(m×n)", "O(n)", "O(log n)", "O(n²) luôn" }, 0, "Bảng m×n cho hai chuỗi độ dài m, n.");
            q.AddQuestion("Khi n rất nhỏ (≤ 20), có thể dùng phương pháp nào?", new[] { "DP bitmask (2^n)", "Chỉ đệ quy mũ", "Không giải được", "Chỉ greedy" }, 0, "Bitmask DP phù hợp tập trạng thái ≤ 2^20.");
            q.AddQuestion("Vì sao DP giúp tránh StackOverflow khi n lớn?", new[] { "Bản bottom-up không đệ quy sâu", "Tăng stack size tự động", "Dùng nhiều RAM", "Không cần vòng lặp" }, 0, "Tabulation dùng vòng lặp, không phụ thuộc độ sâu đệ quy.");
            q.AddQuestion("State trong bài toán Coin Change là gì?", new[] { "Số tiền cần đổi — dp[amount]", "Số coin tối đa", "Thứ tự coin", "Màu sắc coin" }, 0, "dp[amount] = số coin tối thiểu để đạt amount.");
        }

        private static void FillSystemQuiz(Quiz q)
        {
            q.AddQuestion("Race Condition xảy ra khi nào?", new[] { "Nhiều thread đọc/ghi tài nguyên dùng chung không đồng bộ", "Chỉ một thread truy cập", "Server quá tải", "Hết RAM" }, 0, "Kết quả phụ thuộc thứ tự thực thi ngẫu nhiên của các thread.");
            q.AddQuestion("Deadlock xảy ra khi có điều kiện gì?", new[] { "4 điều kiện Coffman cùng xảy ra", "Chỉ một tài nguyên", "Thread chạy nhanh", "Ít thread" }, 0, "Mutual exclusion, hold-and-wait, no preemption, circular wait.");
            q.AddQuestion("Mutex khác Semaphore như thế nào?", new[] { "Mutex khóa 1 owner; Semaphore đếm số permit", "Giống hệt nhau", "Semaphore không dùng được", "Mutex nhanh hơn" }, 0, "Mutex là binary lock; Semaphore có thể cấp nhiều permit.");
            q.AddQuestion("Load Balancer dùng để làm gì?", new[] { "Phân phối yêu cầu giữa nhiều server", "Nén dữ liệu", "Mã hóa", "Lưu cache" }, 0, "Phân tải yêu cầu tránh một server quá tải.");
            q.AddQuestion("SQL phù hợp dữ liệu nào?", new[] { "Dữ liệu quan hệ, giao dịch ACID", "Dữ liệu không cấu trúc lớn", "Chỉ dữ liệu tạm", "File nhị phân" }, 0, "SQL tối ưu quan hệ và giao dịch; NoSQL cho scale-out linh hoạt.");
            q.AddQuestion("Cache thường đặt ở lớp nào để giảm tải DB?", new[] { "Giữa client và DB (Redis/CDN)", "Trong CPU", "Ở database bên trong", "Không có nơi nào" }, 0, "Redis/CDN cache các truy vấn nóng giảm tải DB.");
            q.AddQuestion("Round-robin load balancing hoạt động thế nào?", new[] { "Luân phiên gửi yêu cầu cho từng server", "Gửi cho server rảnh nhất", "Ngẫu nhiên", "Gửi cho server đầu tiên" }, 0, "Round-robin xoay vòng đều giữa các server.");
            q.AddQuestion("Một hệ thống cần mở rộng ngang (scale out) nghĩa là gì?", new[] { "Thêm nhiều máy/server hơn", "Nâng cấp CPU 1 máy", "Tối ưu code", "Giảm người dùng" }, 0, "Scale out thêm node thay vì nâng cấp phần cứng đơn.");
            q.AddQuestion("Consistency trong CAP nghĩa là gì?", new[] { "Mọi node trả cùng dữ liệu mới nhất", "Luôn sẵn sàng", "Chịu phân vùng", "Tốc độ nhanh" }, 0, "Tính nhất quán: mọi replica nhìn thấy cùng một trạng thái.");
            q.AddQuestion("Để tránh deadlock, một cách phổ biến là gì?", new[] { "Khóa tài nguyên theo thứ tự cố định", "Khóa ngẫu nhiên", "Không dùng khóa", "Tăng số thread" }, 0, "Thứ tự khóa nhất quán tránh circular wait.");
        }
    }
}
