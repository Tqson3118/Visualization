using System.Collections.Generic;

namespace VisualizationDSA.Infrastructure.Data
{
    public partial class DbSeeder
    {
        private class QuizQuestionSeed
        {
            public string Question { get; set; } = "";
            public string[] Options { get; set; } = new string[0];
            public int CorrectIndex { get; set; }
            public string Explanation { get; set; } = "";
        }

        private static QuizQuestionSeed Q(string question, string[] options, int correctIndex, string explanation)
            => new QuizQuestionSeed { Question = question, Options = options, CorrectIndex = correctIndex, Explanation = explanation };

        // ── Quiz theo node — Sorting ──
        private static List<QuizQuestionSeed> SortingQuizQuestions(string name)
        {
            if (name == "Bubble Sort")
                return new List<QuizQuestionSeed>
                {
                    Q("Độ phức tạp trường hợp xấu nhất của Bubble Sort?", new[]{"O(n)","O(n log n)","O(n²)","O(2^n)"},2,"Hai vòng lặp lồng nhau → O(n²)."),
                    Q("Sau lượt đầu tiên, phần tử nào nằm đúng vị trí?", new[]{"Phần tử nhỏ nhất","Phần tử lớn nhất","Phần tử giữa","Không phần tử nào"},1,"Phần tử lớn nhất 'nổi' về cuối."),
                    Q("Bubble Sort có ổn định không?", new[]{"Có","Không","Chỉ với chuỗi","Tùy ý"},0,"Chỉ hoán đổi liền kề khi sai thứ tự → stable."),
                    Q("Tối ưu nào giúp Bubble Sort dừng sớm khi mảng đã sắp xếp?", new[]{"Cờ kiểm tra có hoán đổi","Dùng đệ quy","Dùng heap","Không tối ưu được"},0,"Không có hoán đổi trong một lượt → dừng."),
                    Q("Bộ nhớ phụ của Bubble Sort?", new[]{"O(1)","O(n)","O(log n)","O(n²)"},0,"In-place, chỉ 1 biến tạm."),
                    Q("Bubble Sort thuộc nhóm thuật toán nào?", new[]{"Chia để trị","So sánh tại chỗ","Quy hoạch động","Tham lam"},1,"Dựa trên so sánh và hoán đổi."),
                    Q("Với mảng 5 phần tử, số lượt duyệt tối đa cần là?", new[]{"4","5","10","3"},0,"n-1 = 4 lượt (phần tử cuối tự đúng)."),
                    Q("Khi nào Bubble Sort phù hợp?", new[]{"Mảng nhỏ/gần sắp xếp","Mảng triệu phần tử","Luôn luôn","Không bao giờ"},0,"O(n) khi gần sắp xếp, dễ cài đặt."),
                    Q("Trường hợp tốt nhất của Bubble Sort?", new[]{"O(n)","O(n²)","O(log n)","O(1)"},0,"Mảng đã sắp xếp, dừng sau 1 lượt."),
                    Q("Bubble Sort so sánh các phần tử nào?", new[]{"Cặp liền kề","Cặp xa nhau","Ngẫu nhiên","Chỉ đầu-cuối"},0,"So sánh từng cặp a[j] và a[j+1]."),
                };
            if (name == "Selection Sort")
                return new List<QuizQuestionSeed>
                {
                    Q("Selection Sort mỗi lượt tìm gì?", new[]{"Phần tử nhỏ nhất","Phần tử lớn nhất","Phần tử giữa","Ngẫu nhiên"},0,"Tìm min trong phần chưa sắp xếp."),
                    Q("Độ phức tạp trung bình của Selection Sort?", new[]{"O(n)","O(n²)","O(n log n)","O(1)"},1,"Luôn duyệt hai vòng lặp → O(n²)."),
                    Q("Selection Sort có ổn định không?", new[]{"Có","Không","Chỉ với số nguyên","Tùy ý"},1,"Hoán đổi phần tử xa nhau có thể đảo thứ tự phần tử bằng."),
                    Q("Số lần hoán đổi tối đa của Selection Sort?", new[]{"n-1","n²","n log n","n/2"},0,"Mỗi lượt 1 lần hoán đổi → tối đa n-1."),
                    Q("Selection Sort dùng bộ nhớ phụ bao nhiêu?", new[]{"O(1)","O(n)","O(log n)","O(n²)"},0,"In-place."),
                    Q("Tại sao Selection Sort luôn O(n²) kể cả mảng đã sắp xếp?", new[]{"Vẫn phải tìm min từng lượt","Vì dùng đệ quy","Vì hoán đổi liên tục","Không đúng, có thể O(n)"},0,"Không có cờ dừng sớm như Bubble Sort."),
                    Q("Sau lượt i, phần đầu mảng (0..i) như thế nào?", new[]{"Đã sắp xếp đúng vị trí","Chưa sắp","Ngẫu nhiên","Đã xóa"},0,"Các min đã về đúng vị trí đầu."),
                    Q("Selection Sort phù hợp khi nào?", new[]{"Ưu tiên ít hoán đổi","Cần stable","Mảng rất lớn","Cần O(n log n)"},0,"Số hoán đổi tối thiểu O(n)."),
                    Q("Cài đặt Selection Sort cần mấy vòng lặp?", new[]{"2","1","3","0"},0,"Vòng ngoài chọn vị trí, vòng trong tìm min."),
                    Q("Khác Bubble Sort ở điểm cốt lõi?", new[]{"Chỉ hoán đổi 1 lần/lượt","Hoán đổi liền kề","Nhanh hơn mọi trường hợp","Ổn định hơn"},0,"Selection đưa min về đầu, Bubble nổi max về cuối."),
                };
            if (name == "Insertion Sort")
                return new List<QuizQuestionSeed>
                {
                    Q("Insertion Sort chèn phần tử vào đâu?", new[]{"Đúng vị trí trong dãy đã sắp","Về cuối","Về đầu","Ngẫu nhiên"},0,"Chèn vào đúng vị trí trong dãy con đã sắp."),
                    Q("Trường hợp tốt nhất của Insertion Sort?", new[]{"O(n)","O(n²)","O(n log n)","O(1)"},0,"Mảng đã sắp → mỗi phần tử vào ngay vị trí."),
                    Q("Insertion Sort có ổn định không?", new[]{"Có","Không","Tùy ý","Chỉ với chuỗi"},0,"Chỉ dịch khi cần, phần tử bằng giữ thứ tự."),
                    Q("Insertion Sort đặc biệt hiệu quả với dữ liệu nào?", new[]{"Gần như đã sắp xếp","Ngẫu nhiên","Đảo ngược hoàn toàn","Toàn số trùng"},0,"Dịch chuyển ít → gần O(n)."),
                    Q("Vòng lặp chính của Insertion Sort bắt đầu từ đâu?", new[]{"Phần tử thứ 2 (index 1)","Phần tử đầu","Phần tử cuối","Giữa mảng"},0,"Phần tử đầu coi như đã sắp."),
                    Q("Độ phức tạp trung bình của Insertion Sort?", new[]{"O(n²)","O(n log n)","O(n)","O(1)"},0,"Dịch chuyển trung bình n/2 cho mỗi phần tử."),
                    Q("Bộ nhớ phụ của Insertion Sort?", new[]{"O(1)","O(n)","O(log n)","O(n²)"},0,"Chỉ dùng biến key."),
                    Q("Insertion Sort thường dùng để làm gì trong hệ thống lớn?", new[]{"Sort mảng con nhỏ trong Quick Sort","Sort toàn bộ tập lớn","Chỉ để dạy học","Không dùng"},0,"Dùng làm base case cho mảng nhỏ (hybrid sort)."),
                    Q("Nếu mảng giảm dần, Insertion Sort chạy thế nào?", new[]{"Xấu nhất O(n²)","Tốt nhất O(n)","Không hoạt động","O(n log n)"},0,"Mỗi phần tử phải dịch hết phần trước."),
                    Q("Ý tưởng Insertion Sort giống trò chơi gì?", new[]{"Xếp bài trên tay","Chạy đua","Đi tìm kho báu","Xếp ghế"},0,"Nhét từng lá bài mới vào đúng vị trí."),
                };
            if (name == "Quick Sort")
                return new List<QuizQuestionSeed>
                {
                    Q("Độ phức tạp trung bình của Quick Sort?", new[]{"O(n)","O(n log n)","O(n²)","O(log n)"},1,"Phân hoạch + đệ quy hai nửa."),
                    Q("Pivot là gì?", new[]{"Phần tử chốt để phân hoạch","Phần tử đầu mảng","Phần tử nhỏ nhất","Phần tử cuối cùng"},0,"Chia mảng thành < pivot và > pivot."),
                    Q("Trường hợp xấu nhất của Quick Sort?", new[]{"Mảng đã sắp + pivot biên","Mảng ngẫu nhiên","Mảng toàn số trùng","Mảng nhỏ"},0,"Phân hoạch lệch → O(n²)."),
                    Q("Quick Sort có ổn định không?", new[]{"Không","Có","Luôn ổn định","Chỉ với chuỗi"},0,"Hoán đổi xa nhau phá thứ tự phần tử bằng."),
                    Q("Cách giảm xác suất rơi vào trường hợp xấu?", new[]{"Chọn pivot ngẫu nhiên/median-of-three","Dùng nhiều vòng lặp","Sao chép mảng","Không cải thiện được"},0,"Pivot cân bằng giữ phân hoạch đều."),
                    Q("Bộ nhớ phụ trung bình của Quick Sort?", new[]{"O(log n)","O(n)","O(1)","O(n²)"},0,"Đệ quy lưu O(log n) khung nếu cân bằng."),
                    Q("Vì sao Quick Sort thường nhanh hơn Merge Sort?", new[]{"Cache tốt, ít ghi bộ nhớ phụ","Luôn O(n log n)","Không so sánh","Dễ cài đặt"},0,"In-place nên tận dụng cache hiệu quả."),
                    Q("So với Merge Sort, nhược điểm của Quick Sort?", new[]{"Trường hợp xấu O(n²)","Tốn bộ nhớ hơn","Không đệ quy được","Chậm hơn mọi trường hợp"},0,"Merge Sort luôn O(n log n)."),
                    Q("Bước Partition làm gì?", new[]{"Đặt pivot đúng vị trí và chia mảng","Trộn hai nửa","Đếm phần tử","Tìm max"},0,"Trả về chỉ số phân hoạch sau khi đặt pivot."),
                    Q("Quick Sort dùng cấu trúc gì trong cài đặt đệ quy?", new[]{"Call stack","Queue","Heap","HashSet"},0,"Đệ quy dùng call stack."),
                };
            if (name == "Merge Sort")
                return new List<QuizQuestionSeed>
                {
                    Q("Độ phức tạp của Merge Sort ở mọi trường hợp?", new[]{"O(n log n)","O(n²)","O(n)","O(log n)"},0,"Chia đôi + trộn tuyến tính → O(n log n)."),
                    Q("Bộ nhớ phụ của Merge Sort?", new[]{"O(n)","O(1)","O(log n)","O(n²)"},0,"Cần mảng tạm để trộn."),
                    Q("Merge Sort có ổn định không?", new[]{"Có","Không","Tùy ý","Chỉ với chuỗi"},0,"Trộn giữ thứ tự phần tử bằng."),
                    Q("Bước Merge làm gì?", new[]{"Trộn hai nửa đã sắp thành dãy sắp","Chọn pivot","Đảo mảng","Tìm max"},0,"So sánh hai con trỏ và gộp theo thứ tự."),
                    Q("Merge Sort thuộc nhóm thuật toán nào?", new[]{"Chia để trị","Tham lam","Quy hoạch động","Ngẫu nhiên"},0,"Chia đôi đệ quy rồi gộp."),
                    Q("Điều kiện để merge đúng?", new[]{"Hai nửa đã được sắp xếp","Hai nửa bằng nhau","Mảng chẵn","Không cần điều kiện"},0,"Merge chỉ đúng khi hai nửa đã sắp."),
                    Q("Với n = 2^k, số tầng chia của Merge Sort?", new[]{"k = log2(n)","n","2n","n/2"},0,"Chia đôi n lần → log2(n) tầng."),
                    Q("Tại sao Merge Sort tốt cho dữ liệu không truy cập ngẫu nhiên?", new[]{"Duyệt tuần tự, ít nhảy con trỏ","Vì ổn định","Vì nhanh nhất","Vì dùng ít RAM"},0,"Truy cập tuần tự phù hợp linked list, đĩa."),
                    Q("Nhược điểm chính của Merge Sort?", new[]{"Tốn bộ nhớ O(n)","Không ổn định","Không chia để trị","Quá chậm"},0,"Cần mảng phụ O(n)."),
                    Q("Merge Sort thích hợp khi nào?", new[]{"Cần ổn định và đảm bảo O(n log n)","Cần in-place","Dữ liệu rất nhỏ","Không bao giờ"},0,"Stable + worst-case O(n log n) → an toàn."),
                };
            return new List<QuizQuestionSeed>
            {
                Q("Độ phức tạp của Heap Sort ở mọi trường hợp?", new[]{"O(n log n)","O(n²)","O(n)","O(log n)"},0,"Dựng heap O(n) + trích xuất n lần O(log n)."),
                Q("Heap Sort dùng loại heap nào?", new[]{"Max-heap","Min-heap","Không dùng heap","Heap cân bằng"},0,"Trích gốc max về cuối mảng."),
                Q("Bộ nhớ phụ của Heap Sort?", new[]{"O(1)","O(n)","O(log n)","O(n²)"},0,"In-place, heap ngay trên mảng."),
                Q("Heap Sort có ổn định không?", new[]{"Không","Có","Tùy ý","Chỉ với chuỗi"},0,"Hoán đổi xa nhau làm mất thứ tự phần tử bằng."),
                Q("Phép siftDown (heapify down) làm gì?", new[]{"Đẩy node xuống đúng vị trí trong heap","Tăng khóa node","Đếm node","Xóa heap"},0,"Đảm bảo tính chất heap sau khi trích/đổi."),
                Q("Độ phức tạp dựng heap từ mảng?", new[]{"O(n)","O(n log n)","O(n²)","O(1)"},0,"Heapify bottom-up chạy O(n)."),
                Q("Sau khi trích gốc max, ta đặt nó ở đâu?", new[]{"Về cuối mảng chưa sắp","Về đầu mảng","Xóa luôn","Ngẫu nhiên"},0,"Đưa max về cuối, giảm vùng heap."),
                Q("Heap Sort phù hợp khi nào?", new[]{"Cần in-place O(n log n) ổn định thời gian","Cần stable","Cần O(n) trung bình","Chỉ mảng nhỏ"},0,"In-place + luôn O(n log n) nhưng không stable."),
                Q("Tính chất max-heap là gì?", new[]{"Nút cha ≥ nút con","Nút cha ≤ nút con","Nút cha = nút con","Ngẫu nhiên"},0,"Max-heap: gốc là lớn nhất, mọi cha ≥ con."),
                Q("So với Merge Sort, ưu điểm của Heap Sort?", new[]{"In-place O(1) bộ nhớ","Stable","Nhanh hơn mọi trường hợp","Dễ cài đặt"},0,"Heap Sort dùng O(1) bộ nhớ phụ."),
            };
        }

        // ── Quiz theo node — Graph ──
        private static List<QuizQuestionSeed> GraphQuizQuestions(string name)
        {
            if (name == "Giới thiệu Đồ thị")
                return new List<QuizQuestionSeed>
                {
                    Q("Danh sách kề (adjacency list) lưu đồ thị bằng gì?", new[]{"Mỗi đỉnh giữ danh sách đỉnh kề","Ma trận n×n","Danh sách cạnh thuần","Bảng băm đỉnh"},0,"Mỗi đỉnh có list đỉnh kề → tiết kiệm cho đồ thị thưa."),
                    Q("Ma trận kề phù hợp đồ thị nào?", new[]{"Dày đặc (dense)","Thưa","Vô hướng thuần","Không trọng số thuần"},0,"Ma trận n² bộ nhớ, tốt cho đồ thị dày."),
                    Q("Đồ thị vô hướng mỗi cạnh (u,v) xuất hiện thế nào?", new[]{"u-v và v-u đều có","Chỉ u-v","Không có","Chỉ khi v<u"},0,"Vô hướng: cạnh hai chiều."),
                    Q("Bậc (degree) của đỉnh là gì?", new[]{"Số cạnh nối với đỉnh đó","Số đỉnh trong đồ thị","Trọng số lớn nhất","Số chu trình"},0,"Bậc = số đỉnh kề (vô hướng)."),
                    Q("Đồ thị có hướng mỗi cạnh có ý nghĩa gì?", new[]{"Đi một chiều từ u đến v","Đi hai chiều","Không đi được","Chỉ v đến u"},0,"Cạnh có hướng u→v chỉ đi theo chiều đó."),
                    Q("Độ phức tạp bộ nhớ adjacency list với V đỉnh, E cạnh?", new[]{"O(V + E)","O(V²)","O(E)","O(V)"},0,"Mỗi đỉnh + mỗi cạnh lưu 1 lần."),
                    Q("Độ phức tạp bộ nhớ ma trận kề?", new[]{"O(V²)","O(V + E)","O(E log V)","O(V)"},0,"Ma trận V×V."),
                    Q("Self-loop là gì?", new[]{"Cạnh từ đỉnh tới chính nó","Cạnh giữa hai đỉnh khác nhau","Chu trình dài","Đồ thị rỗng"},0,"Self-loop: cạnh (u,u)."),
                    Q("Trong đồ thị có trọng số, cạnh lưu thêm gì?", new[]{"Trọng số w","Màu sắc","Thời gian","Không gì"},0,"Cạnh có thêm trọng số để bài toán đường đi."),
                    Q("Connected graph (đồ thị liên thông) là gì?", new[]{"Mọi đỉnh nối được với nhau","Có đúng 1 cạnh","Không có chu trình","Mọi đỉnh bậc 1"},0,"Liên thông: luôn có đường đi giữa mọi cặp đỉnh."),
                };
            if (name == "Duyệt BFS")
                return new List<QuizQuestionSeed>
                {
                    Q("BFS dùng cấu trúc dữ liệu nào?", new[]{"Queue","Stack","Heap","Mảng"},0,"BFS duyệt theo tầng bằng FIFO."),
                    Q("BFS tìm đường đi ngắn nhất (ít cạnh) trên đồ thị nào?", new[]{"Không trọng số","Có trọng số âm","Có trọng số dương","Có chu trình âm"},0,"BFS theo tầng cho đường ngắn nhất theo số cạnh."),
                    Q("Độ phức tạp BFS trên adjacency list?", new[]{"O(V + E)","O(V)","O(E)","O(V×E)"},0,"Mỗi đỉnh vào queue 1 lần, mỗi cạnh duyệt 1 lần."),
                    Q("BFS duyệt theo thứ tự nào?", new[]{"Tầng (level) từ gần đến xa","Chiều sâu trước","Ngẫu nhiên","Theo trọng số"},0,"BFS mở rộng theo từng tầng."),
                    Q("Để tránh lặp vô hạn trong BFS, ta cần gì?", new[]{"Mảng visited","Sắp xếp cạnh","Đếm đỉnh","Không cần"},0,"Đánh dấu visited khi đưa vào queue."),
                    Q("Ứng dụng điển hình của BFS?", new[]{"Đường đi ngắn nhất không trọng số","Cây khung nhỏ nhất","Sắp xếp tô pô","Detect cycle bằng DFS thuần"},0,"BFS phù hợp tìm đường ngắn nhất số cạnh."),
                    Q("BFS trên cây (tree) còn gọi là gì?", new[]{"Level-order traversal","Pre-order","Post-order","In-order"},0,"Level-order duyệt theo tầng giống BFS."),
                    Q("Nếu đồ thị không liên thông, BFS từ 1 đỉnh xử lý?", new[]{"Chỉ duyệt thành phần liên thông của đỉnh đó","Duyệt hết đồ thị","Lỗi","Không duyệt"},0,"BFS chỉ đi trong thành phần liên thông chứa đỉnh xuất phát."),
                    Q("Độ phức tạp không gian BFS?", new[]{"O(V)","O(E)","O(V²)","O(1)"},0,"Queue tối đa chứa O(V) đỉnh."),
                    Q("BFS dùng để kiểm tra tính chất nào của đồ thị?", new[]{"Liên thông","Có trọng số âm","Số cạnh tối đa","Tính cân bằng"},0,"BFS giúp xác định thành phần liên thông."),
                };
            if (name == "Duyệt DFS")
                return new List<QuizQuestionSeed>
                {
                    Q("DFS dùng cấu trúc dữ liệu nào (bản lặp)?", new[]{"Stack","Queue","Priority Queue","HashSet"},0,"DFS đi sâu bằng stack/đệ quy."),
                    Q("Độ phức tạp DFS trên adjacency list?", new[]{"O(V + E)","O(V)","O(E)","O(V²)"},0,"Duyệt mọi đỉnh và mọi cạnh 1 lần."),
                    Q("DFS có thể phát hiện gì?", new[]{"Chu trình","Trọng số âm","Đường đi dài nhất","Tính đối xứng"},0,"Back-edge khi gặp đỉnh đang xét → chu trình."),
                    Q("DFS ứng dụng trong bài toán nào?", new[]{"Đếm thành phần liên thông","Đường đi ngắn nhất có trọng số","Tìm cây khung tối ưu","Sort số nguyên"},0,"DFS lan truyền đánh dấu connected components."),
                    Q("Khi nào cần dùng DFS thay vì BFS?", new[]{"Cần khám phá sâu như maze, backtracking","Cần đường ngắn nhất","Cần duyệt theo tầng","Cần tối ưu chi phí"},0,"DFS phù hợp backtracking, duyệt sâu."),
                    Q("Back-edge trong DFS là gì?", new[]{"Cạnh nối tới đỉnh đang trong stack duyệt","Cạnh tới đỉnh đã xong","Cạnh song song","Không có"},0,"Back-edge chỉ tới tổ tiên trong cây DFS → cycle."),
                    Q("Độ phức tạp không gian DFS (đệ quy) trên cây cao n?", new[]{"O(n)","O(1)","O(n²)","O(log n)"},0,"Call stack sâu bằng chiều cao cây DFS."),
                    Q("Pre-order traversal tương đương với?", new[]{"DFS ghé thăm trước khi xuống con","BFS","Post-order","In-order"},0,"Pre-order = DFS duyệt gốc trước."),
                    Q("DFS có tìm được đường đi ngắn nhất trên đồ thị có trọng số không?", new[]{"Không đảm bảo","Luôn đúng","Chỉ khi trọng số bằng nhau","Chỉ đồ thị cây"},0,"DFS không đảm bảo đường ngắn nhất; cần BFS/Dijkstra."),
                    Q("Để tránh vòng lặp vô hạn DFS cần gì?", new[]{"Mảng visited","Sắp xếp","Đếm cạnh","Không cần"},0,"visited chặn duyệt lại đỉnh đã thăm."),
                };
            if (name == "Dijkstra")
                return new List<QuizQuestionSeed>
                {
                    Q("Dijkstra tìm gì?", new[]{"Đường đi ngắn nhất từ 1 nguồn","Cây khung nhỏ nhất","Chu trình âm","Thành phần liên thông"},0,"Single-source shortest path trên trọng số không âm."),
                    Q("Điều kiện áp dụng Dijkstra?", new[]{"Trọng số không âm","Trọng số âm","Đồ thị có hướng thuần","Đồ thị không trọng số thuần"},0,"Cạnh âm làm sai kết quả."),
                    Q("Dijkstra dùng cấu trúc dữ liệu nào?", new[]{"Priority Queue (min-heap)","Queue","Stack","Bảng băm"},0,"Min-heap luôn lấy đỉnh dist nhỏ nhất."),
                    Q("Độ phức tạp Dijkstra với heap?", new[]{"O((V+E) log V)","O(V²)","O(E)","O(V)"},0,"Mỗi thao tác heap O(log V)."),
                    Q("Khi dist[u] cũ > dist mới tìm được, ta làm gì?", new[]{"Relax: cập nhật dist nhỏ hơn","Bỏ qua","Tăng lên","Dừng thuật toán"},0,"Relaxation cập nhật đường đi tốt hơn."),
                    Q("Nếu đồ thị có cạnh trọng số âm, Dijkstra?", new[]{"Cho kết quả sai","Vẫn đúng","Chạy nhanh hơn","Lỗi biên dịch"},0,"Tham ăn không xử lý đúng cạnh âm."),
                    Q("Ứng dụng thực tế của Dijkstra?", new[]{"Google Maps định tuyến","Sắp xếp mảng","Nén dữ liệu","Mã hóa"},0,"Định tuyến đường đi ngắn nhất."),
                    Q("Relaxation là gì?", new[]{"Cập nhật dist[v] = min(dist[v], dist[u]+w)","Tăng dist","Xóa cạnh","Sắp xếp đỉnh"},0,"Nếu đi qua u tốt hơn thì cập nhật."),
                    Q("Dijkstra có hoạt động trên đồ thị vô hướng không?", new[]{"Có","Không","Chỉ có hướng","Chỉ cây"},0,"Vô hướng biểu diễn như 2 cạnh có hướng."),
                    Q("Sau khi thuật toán chạy xong, dist[v] là gì?", new[]{"Đường đi ngắn nhất từ nguồn tới v","Đường đi dài nhất","Số bậc của v","Trọng số cây khung"},0,"dist[v] = tổng trọng số đường đi tối ưu."),
                };
            if (name == "Bellman-Ford")
                return new List<QuizQuestionSeed>
                {
                    Q("Bellman-Ford cho phép điều gì mà Dijkstra không có?", new[]{"Cạnh trọng số âm","Cạnh không âm","Đồ thị có hướng","Đồ thị vô hướng"},0,"Bellman-Ford xử lý cạnh âm."),
                    Q("Bellman-Ford lặp tối đa bao nhiêu lần?", new[]{"V-1 lần relax","V lần","E lần","V+E lần"},0,"Sau V-1 lần, mọi đường đi tối ưu đã hội tụ."),
                    Q("Làm sao Bellman-Ford phát hiện chu trình âm?", new[]{"Relax thêm 1 vòng nữa vẫn còn cải thiện","Đếm cạnh","So sánh bậc","Không phát hiện được"},0,"Vòng thứ V vẫn relax được → chu trình âm."),
                    Q("Độ phức tạp Bellman-Ford?", new[]{"O(V × E)","O((V+E) log V)","O(V²)","O(E log V)"},0,"V-1 vòng, mỗi vòng duyệt E cạnh."),
                    Q("Bellman-Ford dùng chiến lược gì?", new[]{"Dynamic programming / relaxation lặp","Tham ăn","Chia để trị","Backtracking"},0,"Lặp cập nhật dist dần theo từng vòng."),
                    Q("Khi có chu trình âm, dist có tính chất gì?", new[]{"Giảm mãi, không hội tụ","Tăng mãi","Ổn định","Bằng 0"},0,"Vòng âm làm dist giảm vô hạn."),
                    Q("Bellman-Ford phù hợp khi nào?", new[]{"Cần phát hiện chu trình âm","Đồ thị nhỏ, cạnh âm","Cần nhanh nhất","Đồ thị không trọng số"},0,"Khi có cạnh âm và cần detect negative cycle."),
                    Q("Với V đỉnh và đường đi tối ưu tối đa mấy cạnh?", new[]{"V-1 cạnh","V cạnh","V² cạnh","E cạnh"},0,"Đường đi không chu trình tối đa V-1 cạnh."),
                    Q("Ban đầu dist[source] = ?", new[]{"0","∞","1","-1"},0,"Nguồn có dist = 0."),
                    Q("Điều kiện relax trong Bellman-Ford?", new[]{"dist[u] + w < dist[v]","dist[u] + w > dist[v]","dist[u] = dist[v]","Luôn relax"},0,"Cập nhật khi tìm được đường tốt hơn."),
                };
            return new List<QuizQuestionSeed>
            {
                Q("Cây khung nhỏ nhất (MST) nối tất cả đỉnh với điều kiện gì?", new[]{"Tổng trọng số nhỏ nhất, không chu trình","Nhiều cạnh nhất","Số đỉnh lớn nhất","Tổng trọng số lớn nhất"},0,"MST tối ưu tổng trọng số, không vòng."),
                Q("Kruskal xử lý cạnh theo thứ tự nào?", new[]{"Tăng dần trọng số","Ngẫu nhiên","Giảm dần","Theo thứ tự nhập"},0,"Sort cạnh tăng dần rồi chọn."),
                Q("Kruskal dùng cấu trúc gì để tránh chu trình?", new[]{"DSU (Union-Find)","Heap","Queue","Bảng băm"},0,"DSU kiểm tra hai đỉnh có cùng tập hay không."),
                Q("Độ phức tạp Kruskal?", new[]{"O(E log E)","O(V×E)","O(V²)","O(E)"},0,"Chủ yếu do sort cạnh."),
                Q("Prim khác Kruskal ở điểm nào?", new[]{"Prim mở rộng từ 1 đỉnh bằng heap","Prim sort cạnh","Prim không dùng heap","Giống hệt"},0,"Prim tăng dần từ đỉnh nguồn, Kruskal chọn cạnh toàn cục."),
                Q("MST của đồ thị n đỉnh có bao nhiêu cạnh?", new[]{"n-1","n","n+1","2n"},0,"Cây n đỉnh có n-1 cạnh."),
                Q("Nếu hai cạnh cùng trọng số, MST có duy nhất không?", new[]{"Có thể không duy nhất","Luôn duy nhất","Không bao giờ tồn tại","Chỉ cây có"},0,"Trọng số trùng nhau có thể cho nhiều MST."),
                Q("Điều kiện để add cạnh (u,v) vào Kruskal?", new[]{"find(u) != find(v)","find(u) == find(v)","u < v","w > 0"},0,"Hai đỉnh khác tập mới không tạo chu trình."),
                Q("Kruskal trên đồ thị không liên thông cho kết quả gì?", new[]{"Rừng khung (forest)","Một cây duy nhất","Lỗi","Rỗng"},0,"Mỗi thành phần liên thông một cây khung."),
                Q("Ứng dụng của MST?", new[]{"Mạng lưới cáp/điện chi phí thấp","Đường đi ngắn nhất","Sắp xếp","Nén ảnh"},0,"Nối các node với chi phí tối thiểu."),
            };
        }
    }
}
