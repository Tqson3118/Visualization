using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Domain.Entities;

namespace VisualizationDSA.Infrastructure.Data
{
    public partial class DbSeeder
    {
        // ══════════════════════════════════════════════════════════════════════
        // S3 — QUIZ nền (mở rộng từ 1-5 câu → 10 câu/bài, tiếng Việt)
        // Mỗi quiz guard theo Title → idempotent, không duplicate khi reseed.
        // ══════════════════════════════════════════════════════════════════════
        private async Task SeedQuizzesAsync()
        {
            var bubble = await EnsureQuizAsync("Bubble Sort Mastery", "Kiểm tra kiến thức thuật toán sắp xếp nổi bọt", "sorting", 1, 50);
            if (bubble.Questions == null) { var f = typeof(Quiz).GetProperty("Questions"); f?.SetValue(bubble, new System.Collections.Generic.List<QuizQuestion>()); }
            if (bubble.Questions.Count < 8) FillBubbleQuiz(bubble);

            var quick = await EnsureQuizAsync("Quick Sort Fundamentals", "Nắm vững chiến lược chia để trị của Quick Sort", "sorting", 2, 75);
            if (quick.Questions.Count < 8) FillQuickQuiz(quick);

            var oop = await EnsureQuizAsync("OOP Concepts", "Kiểm tra hiểu biết về Lập trình hướng đối tượng", "oop", 2, 100);
            if (oop.Questions.Count < 8) FillOopQuiz(oop);

            var solid = await EnsureQuizAsync("SOLID Principles", "Nắm vững 5 nguyên lý SOLID trong thiết kế phần mềm", "solid", 3, 125);
            if (solid.Questions.Count < 8) FillSolidQuiz(solid);

            var patterns = await EnsureQuizAsync("Design Patterns", "Nhận diện các mẫu thiết kế phổ biến", "patterns", 3, 150);
            if (patterns.Questions.Count < 8) FillPatternsQuiz(patterns);

            var dsa = await EnsureQuizAsync("Trắc nghiệm Nền tảng DSA", "Đánh giá kiến thức về Big O và Mảng", "dsa", 1, 40);
            if (dsa.Questions.Count < 8) FillDsaQuiz(dsa);

            await _context.SaveChangesAsync();
        }

        private async Task<Quiz> EnsureQuizAsync(string title, string description, string topic, int difficulty, int xpReward)
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Title == title);
            if (quiz == null)
            {
                quiz = new Quiz(title, description, topic, difficulty, xpReward);
                _context.Quizzes.Add(quiz);
            }
            return quiz;
        }

        private static void FillBubbleQuiz(Quiz q)
        {
            q.AddQuestion("Độ phức tạp thời gian trong trường hợp xấu nhất của Bubble Sort là gì?", new[] { "O(n)", "O(n log n)", "O(n²)", "O(2^n)" }, 2, "Bubble Sort so sánh từng cặp liền kề trong hai vòng lặp lồng nhau → O(n²).");
            q.AddQuestion("Trường hợp tốt nhất của Bubble Sort (có tối ưu cờ dừng) là bao nhiêu?", new[] { "O(n)", "O(n log n)", "O(n²)", "O(1)" }, 0, "Khi mảng đã sắp xếp, chỉ cần một lượt và phát hiện không hoán đổi → O(n).");
            q.AddQuestion("Bubble Sort có phải là thuật toán ổn định (stable) không?", new[] { "Có", "Không", "Chỉ với số nguyên", "Tùy cài đặt" }, 0, "Bubble Sort chỉ hoán đổi phần tử liền kề khi sai thứ tự nên giữ thứ tự phần tử bằng nhau → stable.");
            q.AddQuestion("Sau lượt duyệt đầu tiên của Bubble Sort, phần tử nào chắc chắn nằm đúng vị trí?", new[] { "Phần tử nhỏ nhất", "Phần tử lớn nhất", "Phần tử ở giữa", "Không phần tử nào" }, 1, "Phần tử lớn nhất 'nổi' về cuối mảng sau lượt đầu tiên.");
            q.AddQuestion("Bubble Sort dùng bao nhiêu bộ nhớ phụ?", new[] { "O(1)", "O(n)", "O(log n)", "O(n²)" }, 0, "Chỉ dùng một biến tạm để hoán đổi → in-place, O(1).");
            q.AddQuestion("Cách tối ưu phổ biến cho Bubble Sort khi mảng gần như đã sắp xếp?", new[] { "Dùng cờ kiểm tra có hoán đổi nào trong lượt không", "Dùng đệ quy", "Tăng tốc CPU", "Chia mảng làm đôi" }, 0, "Nếu một lượt không có hoán đổi nào → mảng đã sắp xếp → dừng sớm.");
            q.AddQuestion("Số lần so sánh tối đa của Bubble Sort với mảng n phần tử?", new[] { "n", "n log n", "n(n-1)/2", "n²" }, 2, "Trường hợp xấu nhất so sánh khoảng n(n-1)/2 lần.");
            q.AddQuestion("Khi nào Bubble Sort phù hợp sử dụng?", new[] { "Mảng rất lớn", "Mảng nhỏ hoặc gần như đã sắp xếp", "Dữ liệu phát trực tuyến", "Luôn tối ưu nhất" }, 1, "O(n) tốt nhất khi gần sắp xếp; phù hợp mảng nhỏ do dễ cài đặt.");
            q.AddQuestion("Bubble Sort thuộc nhóm thuật toán nào?", new[] { "Chia để trị", "So sánh tại chỗ (in-place comparison)", "Quy hoạch động", "Tham lam" }, 1, "Là thuật toán sắp xếp dựa trên so sánh, hoán đổi tại chỗ.");
            q.AddQuestion("Sau lượt thứ i (bắt đầu từ 0), số phần tử cuối đã đúng vị trí?", new[] { "i", "i + 1", "n - i", "n" }, 1, "Sau mỗi lượt, i+1 phần tử lớn nhất đã về đúng vị trí cuối.");
        }

        private static void FillQuickQuiz(Quiz q)
        {
            q.AddQuestion("Độ phức tạp trung bình của Quick Sort là gì?", new[] { "O(n)", "O(n log n)", "O(n²)", "O(log n)" }, 1, "Quick Sort chia mảng và sắp xếp từng phân đoạn → O(n log n) trung bình.");
            q.AddQuestion("Pivot trong Quick Sort là gì?", new[] { "Phần tử đầu tiên", "Phần tử giữa", "Phần tử dùng để phân hoạch mảng", "Phần tử lớn nhất" }, 2, "Pivot là phần tử chốt dùng để chia mảng thành phần nhỏ hơn và lớn hơn nó.");
            q.AddQuestion("Trường hợp xấu nhất của Quick Sort xảy ra khi nào?", new[] { "Mảng đã sắp xếp và chọn pivot là phần tử biên", "Mảng ngẫu nhiên", "Mảng có nhiều phần tử trùng", "Mảng nhỏ" }, 0, "Khi pivot luôn là phần tử nhỏ nhất/lớn nhất, phân hoạch lệch → O(n²).");
            q.AddQuestion("Thuật toán Partition trong Quick Sort có vai trò gì?", new[] { "Trộn hai nửa", "Đặt pivot vào đúng vị trí và chia mảng", "Đếm số phần tử", "Sao chép mảng" }, 1, "Partition sắp xếp pivot vào đúng vị trí và trả về chỉ số phân hoạch.");
            q.AddQuestion("Quick Sort dùng cấu trúc dữ liệu nào trong cài đặt đệ quy?", new[] { "Ngăn xếp (call stack)", "Hàng đợi", "Cây nhị phân", "Bảng băm" }, 0, "Đệ quy dùng call stack; bản lặp dùng stack tường minh.");
            q.AddQuestion("Quick Sort có ổn định (stable) không?", new[] { "Có", "Không", "Chỉ với chuỗi", "Luôn luôn" }, 1, "Phân hoạch thường hoán đổi phần tử xa nhau nên không giữ thứ tự phần tử bằng nhau → không stable.");
            q.AddQuestion("Kỹ thuật nào giảm xác suất rơi vào trường hợp xấu của Quick Sort?", new[] { "Chọn pivot ngẫu nhiên / median-of-three", "Dùng vòng lặp lồng nhau", "Sao chép toàn mảng", "Không thể cải thiện" }, 0, "Pivot ngẫu nhiên hoặc median-of-three giúp phân hoạch cân bằng hơn.");
            q.AddQuestion("Độ phức tạp không gian trung bình của Quick Sort (đệ quy)?", new[] { "O(1)", "O(log n)", "O(n)", "O(n²)" }, 1, "Mỗi lần đệ quy lưu một khung → O(log n) trung bình.");
            q.AddQuestion("Quick Sort thường nhanh hơn Merge Sort trong thực tế vì sao?", new[] { "Cục bộ cache tốt, không cần mảng phụ", "Luôn O(n log n)", "Không dùng so sánh", "Đơn giản hơn" }, 0, "In-place, ít ghi bộ nhớ phụ nên tận dụng cache tốt.");
            q.AddQuestion("So với Merge Sort, Quick Sort có nhược điểm gì nổi bật?", new[] { "Trường hợp xấu O(n²)", "Tốn nhiều bộ nhớ hơn", "Không dùng được với số nguyên", "Không đệ quy được" }, 0, "Trường hợp xấu O(n²) trong khi Merge Sort luôn O(n log n).");
        }

        private static void FillOopQuiz(Quiz q)
        {
            q.AddQuestion("Nguyên lý nào ẩn chi tiết triển khai và chỉ phơi bày chức năng cần thiết?", new[] { "Kế thừa", "Đóng gói", "Đa hình", "Trừu tượng hóa" }, 1, "Đóng gói gom dữ liệu và phương thức, ẩn chi tiết nội bộ.");
            q.AddQuestion("Điều gì cho phép lớp con thừa hưởng thuộc tính từ lớp cha?", new[] { "Kế thừa", "Đóng gói", "Đa hình", "Composition" }, 0, "Kế thừa tái sử dụng code bằng cách cho lớp con nhận thành viên của lớp cha.");
            q.AddQuestion("Bốn trụ cột của OOP là gì?", new[] { "Abstraction, Composition, Inheritance, Polymorphism", "Encapsulation, Inheritance, Polymorphism, Abstraction", "Encapsulation, Composition, Aggregation, Polymorphism", "Inheritance, Interface, Abstract, Virtual" }, 1, "Bốn trụ cột: Đóng gói, Kế thừa, Đa hình, Trừu tượng hóa.");
            q.AddQuestion("Access modifier nào ẩn thành viên chỉ trong phạm vi lớp hiện tại?", new[] { "public", "internal", "private", "protected" }, 2, "private chỉ truy cập được bên trong chính lớp đó.");
            q.AddQuestion("Đa hình (Polymorphism) cho phép điều gì?", new[] { "Cùng một thông điệp nhưng hành vi khác nhau theo kiểu runtime", "Truy cập private field", "Nhiều lớp cùng tên", "Tự động tối ưu code" }, 0, "Đối tượng cùng giao diện nhưng triển khai khác nhau tùy runtime type.");
            q.AddQuestion("Lợi ích chính của Đóng gói là gì?", new[] { "Bảo vệ dữ liệu khỏi truy cập/hiệu chỉnh sai", "Tăng tốc độ chạy", "Giảm số dòng code", "Loại bỏ constructor" }, 0, "Đóng gói kiểm soát truy cập qua getter/setter và bảo toàn bất biến.");
            q.AddQuestion("Trừu tượng hóa (Abstraction) tập trung vào điều gì?", new[] { "Ẩn phần phức tạp, chỉ phơi bày giao diện dùng được", "Chi tiết cấp phát bộ nhớ", "Thứ tự khởi tạo đối tượng", "Tốc độ render" }, 0, "Abstraction che giấu phức tạp, phơi bày mức giao diện cần thiết.");
            q.AddQuestion("Khi dùng composition thay kế thừa, ta đạt được lợi ích gì?", new[] { "Giảm ràng buộc chặt giữa các lớp", "Chắc chắn chạy nhanh hơn", "Tự động sinh code", "Không cần interface" }, 0, "Composition (has-a) linh hoạt hơn, giảm coupling so với inheritance (is-a).");
            q.AddQuestion("Từ khóa virtual trong C# dùng để làm gì?", new[] { "Cho phép lớp con override phương thức", "Ngăn lớp con kế thừa", "Tăng tốc compile", "Khai báo hằng số" }, 0, "Phương thức virtual có thể được override trong lớp kế thừa.");
            q.AddQuestion("Một lớp chỉ nên có một lý do để thay đổi là mô tả nguyên lý nào?", new[] { "OCP", "LSP", "SRP", "DIP" }, 2, "Single Responsibility Principle (SRP).");
        }

        private static void FillSolidQuiz(Quiz q)
        {
            q.AddQuestion("Nguyên lý nào nói một lớp chỉ nên có một lý do để thay đổi?", new[] { "Open/Closed", "Single Responsibility", "Liskov Substitution", "Interface Segregation" }, 1, "SRP: mỗi lớp chỉ đảm nhận một trách nhiệm duy nhất.");
            q.AddQuestion("Nguyên lý nào khuyến khích mở rộng mà không sửa code hiện có?", new[] { "Open/Closed", "Single Responsibility", "Liskov Substitution", "Dependency Inversion" }, 0, "OCP: mở cho mở rộng, đóng cho sửa đổi.");
            q.AddQuestion("Liskov Substitution Principle (LSP) yêu cầu gì?", new[] { "Lớp con phải thay thế được lớp cha mà không phá vỡ hành vi", "Lớp con không được có constructor", "Lớp cha phải abstract", "Chỉ dùng interface" }, 0, "Đối tượng lớp con dùng được ở mọi nơi lớp cha được dùng mà hành vi vẫn đúng.");
            q.AddQuestion("Interface Segregation Principle (ISP) khuyên điều gì?", new[] { "Nhiều interface nhỏ, chuyên biệt thay vì interface khổng lồ", "Chỉ một interface duy nhất", "Interface phải có ít nhất 10 phương thức", "Bỏ interface dùng lớp abstract" }, 0, "Khách hàng không nên bị ép phụ thuộc vào phương thức chúng không dùng.");
            q.AddQuestion("Dependency Inversion Principle (DIP) nói gì?", new[] { "Module cấp cao phụ thuộc trừu tượng, không phụ thuộc chi tiết", "Module cấp thấp điều khiển module cấp cao", "Không dùng interface", "Chỉ dùng singleton" }, 0, "Phụ thuộc vào abstraction (interface) thay vì implementation cụ thể.");
            q.AddQuestion("Vi phạm SRP thường dẫn đến hệ quả gì?", new[] { "Khó bảo trì, thay đổi một chức năng ảnh hưởng chức năng khác", "Chạy nhanh hơn", "Ít class hơn", "Không có lỗi nào" }, 0, "Lớp ôm quá nhiều trách nhiệm → thay đổi dễ gây lỗi lan truyền.");
            q.AddQuestion("Ví dụ điển hình vi phạm LSP?", new[] { "Lớp Square kế thừa Rectangle nhưng thay đổi hành vi setWidth/Height", "Hai lớp không liên quan", "Interface quá nhỏ", "Sử dụng nhiều namespace" }, 0, "Square override setWidth để giữ cạnh bằng nhau → phá vỡ bất biến Rectangle → vi phạm LSP.");
            q.AddQuestion("Để tuân thủ DIP, ta nên inject dependency bằng cách nào?", new[] { "Truyền qua constructor dưới dạng interface", "new trực tiếp trong method", "Dùng biến static", "Không truyền gì" }, 0, "Constructor injection interface giúp dễ swap implementation và test.");
            q.AddQuestion("Một interface có 20 phương thức mà khách hàng chỉ dùng 5 → nên làm gì?", new[] { "Tách interface nhỏ theo ISP", "Giữ nguyên", "Xóa class", "Merge thành một" }, 0, "Tách interface chuyên biệt để khách hàng chỉ phụ thuộc cái cần.");
            q.AddQuestion("OCP thường được cài đặt bằng kỹ thuật nào?", new[] { "Strategy/Polymorphism và interface", "Copy-paste code", "Biến toàn cục", "Enum khổng lồ" }, 0, "Dùng interface + đa hình để thêm hành vi mới mà không sửa lớp cũ.");
        }

        private static void FillPatternsQuiz(Quiz q)
        {
            q.AddQuestion("Mẫu nào định nghĩa quan hệ một-nhiều giữa các đối tượng?", new[] { "Strategy", "Observer", "Factory", "Singleton" }, 1, "Observer: đối tượng đăng ký và được thông báo khi sự kiện xảy ra.");
            q.AddQuestion("Mẫu nào cho phép đổi thuật toán tại runtime?", new[] { "Observer", "Strategy", "Decorator", "Builder" }, 1, "Strategy định nghĩa họ thuật toán và làm chúng hoán đổi được.");
            q.AddQuestion("Singleton đảm bảo điều gì?", new[] { "Chỉ một instance duy nhất toàn cục", "Nhiều instance", "Instance tự hủy", "Instance bất biến" }, 0, "Singleton giới hạn lớp chỉ có một instance và cung cấp điểm truy cập toàn cục.");
            q.AddQuestion("Factory Method dùng để làm gì?", new[] { "Ủy quyền việc tạo đối tượng cho lớp con", "Xóa đối tượng", "Clone đối tượng", "Serialize đối tượng" }, 0, "Factory cho lớp con quyết định tạo instance nào.");
            q.AddQuestion("Decorator pattern giúp gì?", new[] { "Thêm hành vi cho đối tượng một cách động", "Giảm số class", "Tăng tốc độ", "Chống vòng lặp" }, 0, "Decorator bọc đối tượng để bổ sung trách nhiệm linh hoạt.");
            q.AddQuestion("Builder pattern hữu ích khi nào?", new[] { "Đối tượng có nhiều tham số khởi tạo, tạo từng bước", "Đối tượng không có constructor", "Cần singleton", "Cần tốc độ tối đa" }, 0, "Builder tách việc xây dựng đối tượng phức tạp khỏi biểu diễn.");
            q.AddQuestion("Adapter pattern dùng để làm gì?", new[] { "Chuyển giao diện của một lớp sang giao diện khác khách hàng mong đợi", "Tăng tốc độ", "Quản lý bộ nhớ", "Sinh mã tự động" }, 0, "Adapter làm cho lớp không tương thích làm việc cùng nhau.");
            q.AddQuestion("Mẫu nào phù hợp để hủy kết hợp (decouple) giữa các module?", new[] { "Observer", "Singleton", "Flyweight", "Prototype" }, 0, "Observer giúp module phát sự kiện mà không biết ai lắng nghe.");
            q.AddQuestion("Trong Strategy pattern, các thuật toán được đóng gói như thế nào?", new[] { "Mỗi thuật toán một lớp riêng, cùng interface", "Trong một switch lớn", "Trong static method", "Trong database" }, 0, "Mỗi strategy là một lớp cài interface chung, dễ thay thế.");
            q.AddQuestion("Nhược điểm của lạm dụng Singleton là gì?", new[] { "Khó test, tạo trạng thái toàn cục ngầm", "Chạy chậm hơn", "Tốn RAM hơn", "Không có nhược điểm" }, 0, "Singleton gây khó unit test và che giấu phụ thuộc toàn cục.");
        }

        private static void FillDsaQuiz(Quiz q)
        {
            q.AddQuestion("Độ phức tạp O(1) nghĩa là gì?", new[] { "Thời gian tuyến tính", "Thời gian hằng số", "Thời gian bình phương", "Thời gian mũ" }, 1, "O(1) thực thi không phụ thuộc kích thước đầu vào N.");
            q.AddQuestion("Truy cập phần tử mảng theo chỉ số có độ phức tạp bao nhiêu?", new[] { "O(1)", "O(n)", "O(log n)", "O(n²)" }, 0, "Mảng lưu liên tiếp nên tính địa chỉ trực tiếp.");
            q.AddQuestion("Độ phức tạp của Linear Search trong trường hợp xấu nhất?", new[] { "O(1)", "O(n)", "O(log n)", "O(n log n)" }, 1, "Phải duyệt hết mảng nếu không tìm thấy.");
            q.AddQuestion("Binary Search yêu cầu mảng phải như thế nào?", new[] { "Đã được sắp xếp", "Ngẫu nhiên", "Chỉ chứa số dương", "Có độ dài chẵn" }, 0, "Binary Search chỉ đúng trên dãy đã sắp xếp.");
            q.AddQuestion("O(n log n) là độ phức tạp của thuật toán sắp xếp nào?", new[] { "Merge Sort và Quick Sort (trung bình)", "Bubble Sort", "Insertion Sort", "Selection Sort" }, 0, "Merge/Quick/Heap đều O(n log n) (Quick trung bình).");
            q.AddQuestion("Vì sao O(n²) được coi là chậm với n lớn?", new[] { "Tăng trưởng nhanh theo bình phương của n", "Không dùng được", "Cần nhiều RAM", "Không song song hóa được" }, 0, "n=10^5 thì n² = 10^10 thao tác — quá lớn.");
            q.AddQuestion("Độ phức tạp thời gian của thuật toán chia để trị thường có dạng gì?", new[] { "O(n log n) hoặc O(log n)", "O(n²)", "O(n!)", "O(n)" }, 0, "Chia nhỏ bài toán và gộp kết quả thường cho O(n log n).");
            q.AddQuestion("Space complexity đo lường gì?", new[] { "Lượng bộ nhớ bổ sung thuật toán dùng", "Tốc độ CPU", "Số dòng code", "Độ dài tên biến" }, 0, "Space complexity là bộ nhớ phụ theo N.");
            q.AddQuestion("Thuật toán tham lam (greedy) chọn lựa chọn như thế nào?", new[] { "Lựa chọn tốt nhất hiện tại từng bước", "Thử mọi tổ hợp", "Dùng đệ quy toàn phần", "Ngẫu nhiên" }, 0, "Greedy luôn chọn phương án tối ưu cục bộ tại mỗi bước.");
            q.AddQuestion("Quy hoạch động giải quyết bài toán bằng cách nào?", new[] { "Chia bài toán con chồng lấn và lưu kết quả", "Tham lam từng bước", "Sắp xếp trước", "Dùng brute force thuần" }, 0, "DP lưu kết quả bài toán con để tránh tính lại.");
        }
    }
}
