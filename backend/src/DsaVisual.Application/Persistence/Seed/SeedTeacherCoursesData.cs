using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// Seed các khóa học & bài giảng chuẩn chỉ, bài bản cho tài khoản Giảng viên (teacher@demo.local),
/// đồng thời dọn dẹp các bản ghi rác/orphan trong DB để môi trường demo chuyên nghiệp.
/// </summary>
public static class SeedTeacherCoursesData
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // 1. Cập nhật thông tin profile chuyên nghiệp cho teacher@demo.local
        var teacher = await db.Users.FirstOrDefaultAsync(u => u.Email == "teacher@demo.local", ct);
        if (teacher is null)
        {
            teacher = new User
            {
                Email = "teacher@demo.local",
                PasswordHash = PasswordHasher.Hash("Teacher@123"),
                DisplayName = "ThS. Hoàng Minh Trí",
                Role = UserRole.Teacher,
                AcademicDegree = "Thạc sĩ Khoa học Máy tính",
                TeacherBio = "Giảng viên bộ môn Cấu trúc Dữ liệu & Giải thuật. 8 năm kinh nghiệm giảng dạy đại học và huấn luyện đội tuyển lập trình thi đấu (ICPC).",
                ProfileLink = "https://github.com/hoangminhtri-dsa",
                AvatarUrl = "/assets/avatars/cyber-hacker.svg",
                IsActive = true,
                Gems = 2500,
                Hearts = 10,
                HeartsMax = 10,
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now
            };
            db.Users.Add(teacher);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            teacher.DisplayName = "ThS. Hoàng Minh Trí";
            teacher.Role = UserRole.Teacher;
            teacher.AcademicDegree = "Thạc sĩ Khoa học Máy tính";
            teacher.TeacherBio = "Giảng viên bộ môn Cấu trúc Dữ liệu & Giải thuật. 8 năm kinh nghiệm giảng dạy đại học và huấn luyện đội tuyển lập trình thi đấu (ICPC).";
            teacher.ProfileLink = "https://github.com/hoangminhtri-dsa";
            teacher.AvatarUrl = "/assets/avatars/cyber-hacker.svg";
            teacher.Gems = Math.Max(teacher.Gems, 2500);
            teacher.Hearts = 10;
            teacher.HeartsMax = 10;
            teacher.IsActive = true;
            teacher.DeletedAt = null;
            teacher.UpdatedAt = now;
            await db.SaveChangesAsync(ct);
        }

        var teacherId = teacher.Id;

        // 2. Dọn dẹp dữ liệu rác (junk / test courses / orphan lessons do test tay trước đây tạo)
        var allCourses = await db.LearningPaths.ToListAsync(ct);
        var junkCourses = allCourses
            .Where(p => p.Title.StartsWith("test", StringComparison.OrdinalIgnoreCase) 
                     || p.Title.StartsWith("demo123", StringComparison.OrdinalIgnoreCase)
                     || p.Title.StartsWith("DIAG-", StringComparison.OrdinalIgnoreCase)
                     || p.Title.StartsWith("này này", StringComparison.OrdinalIgnoreCase)
                     || p.Title.Contains("Playwright", StringComparison.OrdinalIgnoreCase)
                     || p.Title.Equals("Lộ trình mới", StringComparison.OrdinalIgnoreCase)
                     || p.Title.Contains("Untitled", StringComparison.OrdinalIgnoreCase)
                     || string.IsNullOrWhiteSpace(p.Title))
            .ToList();

        try
        {
            if (junkCourses.Count > 0)
            {
                var junkIds = junkCourses.Select(j => j.Id).ToList();
                var junkNodes = await db.LearningPathNodes.Where(n => junkIds.Contains(n.PathId)).ToListAsync(ct);
                var junkNodeIds = junkNodes.Select(n => n.Id).ToList();

                var attachedExercises = await db.Exercises
                    .Where(e => e.NodeId != null && junkNodeIds.Contains(e.NodeId.Value))
                    .ToListAsync(ct);
                foreach (var ex in attachedExercises)
                {
                    ex.NodeId = null;
                }
                if (attachedExercises.Count > 0)
                {
                    await db.SaveChangesAsync(ct);
                }

                var attachedAssignments = await db.ClassAssignments
                    .Where(a => a.PathItemId != null && junkNodeIds.Contains(a.PathItemId.Value))
                    .ToListAsync(ct);
                if (attachedAssignments.Count > 0)
                {
                    db.ClassAssignments.RemoveRange(attachedAssignments);
                    await db.SaveChangesAsync(ct);
                }

                var attachedProgress = await db.UserNodeProgress
                    .Where(p => junkNodeIds.Contains(p.NodeId))
                    .ToListAsync(ct);
                if (attachedProgress.Count > 0)
                {
                    db.UserNodeProgress.RemoveRange(attachedProgress);
                    await db.SaveChangesAsync(ct);
                }

                var attachedSessions = await db.NodeSessions
                    .Where(s => junkNodeIds.Contains(s.NodeId))
                    .ToListAsync(ct);
                if (attachedSessions.Count > 0)
                {
                    db.NodeSessions.RemoveRange(attachedSessions);
                    await db.SaveChangesAsync(ct);
                }

                db.LearningPathNodes.RemoveRange(junkNodes);
                db.LearningPaths.RemoveRange(junkCourses);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("SeedTeacher: Đã dọn dẹp {Count} khóa học rác/test cũ", junkCourses.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("SeedTeacher: Bỏ qua dọn dẹp khóa học cũ do ràng buộc khóa ngoại: {Msg}", ex.Message);
        }

        // 3. Tạo các Topic thuộc về Giảng viên
        var topicLinear = await EnsureTopicAsync(db, "Cấu trúc Dữ liệu Tuyến tính Nâng cao", 
            "Nghiên cứu chuyên sâu Mảng động, Danh sách liên kết, Ngăn xếp & Hàng đợi", 1, teacherId, now, ct);
        var topicTree = await EnsureTopicAsync(db, "Cây Nhị phân & Tự cân bằng", 
            "Làm chủ BST, Cây AVL, Heap và các ứng dụng tối ưu tra cứu", 2, teacherId, now, ct);
        var topicAlgo = await EnsureTopicAsync(db, "Kỹ thuật Thiết kế Thuật toán", 
            "Chia để trị, Quy hoạch động và Tối ưu độ phức tạp thời gian/không gian", 3, teacherId, now, ct);

        // 4. Seed Khóa học 1 (ACTIVE / PUBLIC): Cấu trúc Dữ liệu Tuyến tính & Ứng dụng Thực tế
        await SeedLinearCourseAsync(db, teacherId, topicLinear.Id, now, logger, ct);

        // 5. Seed Khóa học 2 (ACTIVE / PUBLIC): Cây Nhị phân & Cây Tìm kiếm Cân bằng (BST / AVL)
        await SeedTreeCourseAsync(db, teacherId, topicTree.Id, now, logger, ct);

        // 6. Seed Khóa học 3 (DRAFT / PRIVATE): Thiết kế Thuật toán & Tối ưu hóa Nâng cao
        await SeedDraftAlgoCourseAsync(db, teacherId, topicAlgo.Id, now, logger, ct);

        logger.LogInformation("SeedTeacher: Đã hoàn tất khởi tạo các khóa học mẫu bài bản cho {TeacherName}", teacher.DisplayName);
    }

    private static async Task<Topic> EnsureTopicAsync(AppDbContext db, string name, string desc, int sort, int userId, DateTime now, CancellationToken ct)
    {
        var existing = await db.Topics.FirstOrDefaultAsync(t => t.Name == name, ct);
        if (existing is not null)
        {
            return existing;
        }

        var topic = new Topic
        {
            Name = name,
            Description = desc,
            SortOrder = sort,
            CreatedBy = userId,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Topics.Add(topic);
        await db.SaveChangesAsync(ct);
        return topic;
    }

    private static async Task SeedLinearCourseAsync(AppDbContext db, int teacherId, int topicId, DateTime now, ILogger logger, CancellationToken ct)
    {
        const string courseTitle = "Cấu trúc Dữ liệu Tuyến tính & Ứng dụng Thực tế";
        var course = await db.LearningPaths.FirstOrDefaultAsync(p => p.Title == courseTitle, ct);
        if (course is null)
        {
            course = new LearningPath
            {
                Title = courseTitle,
                Description = "Nắm vững bản chất của Mảng động, Danh sách liên kết, Ngăn xếp & Hàng đợi. Rèn luyện tư duy tối ưu hóa bộ nhớ và độ phức tạp qua các bài toán thực tế.",
                TopicId = topicId,
                SortOrder = 3,
                Status = LearningPathStatus.Active,
                Visibility = PathVisibility.Public,
                IsActive = true,
                CreatedBy = teacherId,
                AuthorId = teacherId,
                HighlightsJson = JsonSerializer.Serialize(new
                {
                    highlights = new[]
                    {
                        new { title = "Trực quan hóa sinh động", description = "Mô phỏng từng bước hoạt động của con trỏ và bộ nhớ RAM khi thao tác danh sách liên kết." },
                        new { title = "Bài tập thực chiến", description = "Luyện tập các bài toán phỏng vấn kinh điển: Two Pointers, Ngoặc hợp lệ, Đảo ngược danh sách." },
                        new { title = "Chấm code tự động", description = "Hệ thống Codelab kiểm tra tính đúng đắn với bộ test case đầy đủ." }
                    },
                    learningObjectives = new[]
                    {
                        "Hiểu cơ chế cấp phát bộ nhớ động của Mảng (Dynamic Array) và chi phí tái phân bổ",
                        "Thành thạo con trỏ và cấu trúc Node trong Danh sách liên kết đơn & đôi",
                        "Ứng dụng Ngăn xếp (Stack) để giải quyết các bài toán biểu thức và khử đệ quy",
                        "Sử dụng Hàng đợi (Queue) & Deque trong quản lý tác vụ và thuật toán BFS"
                    },
                    keyOutcomes = new[]
                    {
                        "Tự tin cài đặt các cấu trúc dữ liệu tuyến tính từ con số 0",
                        "Phân tích chính xác Big-O thời gian và không gian của từng thao tác",
                        "Vượt qua các câu hỏi phỏng vấn kỹ thuật về Mảng và Danh sách liên kết"
                    }
                })
            };
            db.LearningPaths.Add(course);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            course.AuthorId = teacherId;
            course.CreatedBy = teacherId;
            course.Status = LearningPathStatus.Active;
            course.Visibility = PathVisibility.Public;
            course.IsActive = true;
            await db.SaveChangesAsync(ct);
        }

        // Tạo các bài học & nodes
        var lessonsData = new[]
        {
            ("Mảng động (Dynamic Array) & Kỹ thuật Two Pointers", 
             "# Mảng động (Dynamic Array) & Kỹ thuật Two Pointers\n\n## 1. Bản chất của Mảng\nMảng (Array) là cấu trúc dữ liệu tuyến tính lưu trữ các phần tử cùng kiểu tại các **ô nhớ liên tiếp** trong RAM.\n\n### Đặc điểm chính:\n- **Truy cập ngẫu nhiên (Random Access)**: Truy xuất phần tử thứ `i` với độ phức tạp $O(1)$ thông qua công thức `Address = Base + i * Size`.\n- **Cấp phát cố định vs Mảng động**: Khi mảng động vượt quá dung lượng (capacity), hệ thống sẽ cấp phát một vùng nhớ mới có kích thước gấp đôi (Growth Factor = 2) và sao chép toàn bộ dữ liệu sang, cho chi phí trung bình (Amortized) là $O(1)$ mỗi phép chèn cuối.\n\n## 2. Kỹ thuật Hai Con Trỏ (Two Pointers)\nKỹ thuật kinh điển giúp giảm độ phức tạp từ $O(N^2)$ xuống $O(N)$ đối với các bài toán mảng đã sắp xếp:\n- **Con trỏ đối đầu (Opposite-direction)**: Một con trỏ từ đầu, một con trỏ từ cuối mảng (ví dụ: Tìm cặp số có tổng bằng Target, Đảo ngược mảng).\n- **Con trỏ cùng chiều (Fast & Slow)**: Hai con trỏ di chuyển cùng chiều với tốc độ khác nhau (ví dụ: Xóa phần tử trùng lặp trong mảng đã sắp xếp).",
             "structure.array", PathItemType.Theory, 1),

            ("Danh sách liên kết đơn (Singly Linked List)",
             "# Danh sách liên kết đơn (Singly Linked List)\n\n## 1. Cấu tạo Node\nMỗi phần tử trong Danh sách liên kết là một `Node` gồm 2 thành phần:\n1. **Data**: Dữ liệu cần lưu trữ.\n2. **Next**: Con trỏ (tham chiếu) trỏ đến địa chỉ của Node kế tiếp trong bộ nhớ.\n\n```cpp\nstruct Node {\n    int data;\n    Node* next;\n    Node(int val) : data(val), next(nullptr) {}\n};\n```\n\n## 2. Ưu và Nhược điểm so với Mảng\n- **Ưu điểm**: Chèn và xóa ở đầu danh sách cực nhanh với $O(1)$, không cần dịch chuyển các phần tử kế tiếp như Mảng.\n- **Nhược điểm**: Không thể truy cập ngẫu nhiên qua chỉ số $O(1)$, phải duyệt tuần tự từ `Head` tốn $O(N)$.",
             "structure.linkedlist", PathItemType.Theory, 2),

            ("Ngăn xếp (Stack) & Ứng dụng xử lý biểu thức",
             "# Ngăn xếp (Stack) & Ứng dụng xử lý biểu thức\n\n## 1. Nguyên lý LIFO (Last In, First Out)\nPhần tử được đưa vào cuối cùng sẽ là phần tử đầu tiên được lấy ra.\n\n### Các thao tác cốt lõi:\n- `push(x)`: Đẩy phần tử vào đỉnh ngăn xếp ($O(1)$)\n- `pop()`: Lấy và xóa phần tử ở đỉnh ngăn xếp ($O(1)$)\n- `peek() / top()`: Xem giá trị ở đỉnh mà không xóa ($O(1)$)\n\n## 2. Các ứng dụng kinh điển:\n- **Kiểm tra ngoặc hợp lệ**: Duyệt chuỗi, gặp mở ngoặc thì push, gặp đóng ngoặc thì pop và so khớp.\n- **Đánh giá biểu thức Ba Lan ngược (RPN)**.\n- **Ngăn xếp cuộc gọi (Call Stack)** trong hệ thống đệ quy.",
             "stack.push", PathItemType.Theory, 3),

            ("Hàng đợi (Queue) & Hàng đợi 2 đầu (Deque)",
             "# Hàng đợi (Queue) & Hàng đợi 2 đầu (Deque)\n\n## 1. Nguyên lý FIFO (First In, First Out)\nPhần tử vào trước sẽ được phục vụ trước (giống xếp hàng thanh toán siêu thị).\n\n### Các thao tác cốt lõi:\n- `enqueue(x)`: Thêm phần tử vào cuối hàng đợi (Rear/Tail) ($O(1)$)\n- `dequeue()`: Lấy phần tử ra khỏi đầu hàng đợi (Front/Head) ($O(1)$)\n\n## 2. Deque (Double-Ended Queue)\nCho phép chèn và xóa ở cả hai đầu (Front & Back) với độ phức tạp $O(1)$, là nền tảng tối ưu cho thuật toán Sliding Window Maximum.",
             "queue.enqueue", PathItemType.Theory, 4)
        };

        Lesson? lastLesson = null;
        foreach (var (title, content, simKey, itemType, sortOrder) in lessonsData)
        {
            var lesson = await EnsureLessonAsync(db, title, content, topicId, teacherId, now, ct);
            await EnsurePathNodeAsync(db, course.Id, title, lesson.Id, itemType, sortOrder, ct);
            await EnsureSimulationAsync(db, lesson.Id, simKey, title, ct);
            lastLesson = lesson;
        }

        // Tạo bài Quiz kiểm tra đánh giá năng lực
        var quizExercise = await EnsureQuizExerciseAsync(db, course.Id, lastLesson!.Id, "Quiz: Đánh giá hiểu biết Cấu trúc Tuyến tính", teacherId, now, [
            ("Mảng động nhân đôi kích thước khi đầy. Độ phức tạp khấu hao (Amortized time) của mỗi thao tác push_back là gì?", 
             new[] { "O(1)", "O(N)", "O(log N)", "O(N^2)" }, 0, "Khi nhân đôi kích thước, hầu hết các lần thêm chỉ tốn O(1), chi phí copy O(N) được chia đều cho N lần thêm trước đó nên thời gian khấu hao trung bình là O(1)."),
            ("Thao tác nào sau đây có độ phức tạp O(1) trên Danh sách liên kết đơn khi chỉ biết con trỏ Head?",
             new[] { "Chèn vào đầu danh sách (Push Front)", "Truy xuất phần tử thứ K", "Xóa phần tử ở cuối danh sách", "Tìm kiếm phần tử X" }, 0, "Chèn đầu chỉ cần tạo node mới trỏ next vào Head cũ và gán lại Head, hoàn toàn tốn O(1)."),
            ("Cấu trúc dữ liệu nào phù hợp nhất để hiện thực chức năng Undo/Redo trong trình soạn thảo văn bản?",
             new[] { "Stack", "Queue", "Binary Tree", "Hash Map" }, 0, "Thao tác Undo cần lấy lại trạng thái gần nhất vừa thực hiện (LIFO), hoàn toàn phù hợp với cấu trúc Stack."),
            ("Trong thuật toán duyệt đồ thị theo chiều rộng (BFS), cấu trúc dữ liệu nào được sử dụng để lưu các đỉnh chờ duyệt?",
             new[] { "Queue (Hàng đợi)", "Stack (Ngăn xếp)", "Priority Queue", "Array tĩnh" }, 0, "BFS duyệt theo từng tầng từ gần đến xa theo thứ tự đến trước duyệt trước (FIFO), nên sử dụng Queue.")
        ], ct);

        // Gắn quiz node vào lộ trình
        await EnsurePathNodeWithFinalTestAsync(db, course.Id, "Quiz Tổng hợp: Cấu trúc Tuyến tính", quizExercise.Id, 5, ct);
    }

    private static async Task SeedTreeCourseAsync(AppDbContext db, int teacherId, int topicId, DateTime now, ILogger logger, CancellationToken ct)
    {
        const string courseTitle = "Cây Nhị phân & Cây Tìm kiếm Cân bằng (BST / AVL)";
        var course = await db.LearningPaths.FirstOrDefaultAsync(p => p.Title == courseTitle, ct);
        if (course is null)
        {
            course = new LearningPath
            {
                Title = courseTitle,
                Description = "Làm chủ các cấu trúc dữ liệu dạng cây: Binary Search Tree, AVL Tree, Heap và Priority Queue. Hiểu sâu cơ chế xoay cây và cân bằng dữ liệu.",
                TopicId = topicId,
                SortOrder = 4,
                Status = LearningPathStatus.Active,
                Visibility = PathVisibility.Public,
                IsActive = true,
                CreatedBy = teacherId,
                AuthorId = teacherId,
                HighlightsJson = JsonSerializer.Serialize(new
                {
                    highlights = new[]
                    {
                        new { title = "Mô phỏng 4 phép xoay AVL", description = "Quan sát trực tiếp cơ chế tự cân bằng và các phép xoay LL, RR, LR, RL theo thời gian thực." },
                        new { title = "Duyệt cây toàn diện", description = "Nắm vững 3 phương pháp duyệt Inorder, Preorder, Postorder và ứng dụng trong dựng cây." },
                        new { title = "Đống nhị phân & Heapify", description = "Cài đặt Max-Heap/Min-Heap phục vụ sắp xếp Heap Sort và thuật toán Dijkstra." }
                    },
                    learningObjectives = new[]
                    {
                        "Hiểu cấu trúc phân cấp cây nhị phân và các khái niệm: Độ cao, Bậc, Node lá, Node gốc",
                        "Thành thạo tính chất của BST: Cây con trái < Gốc < Cây con phải",
                        "Hiểu hệ số cân bằng (Balance Factor) và các trường hợp mất cân bằng trong cây AVL",
                        "Xây dựng Hàng đợi ưu tiên (Priority Queue) bằng cấu trúc Heap"
                    },
                    keyOutcomes = new[]
                    {
                        "Cài đặt hoàn chỉnh cây BST và AVL với đầy đủ thao tác Chèn, Xóa, Tìm kiếm O(log N)",
                        "Áp dụng thành thạo Heap để giải quyết bài toán Top K phần tử lớn/nhỏ nhất",
                        "Giải quyết các bài toán phỏng vấn thuật toán về Cây trên LeetCode"
                    }
                })
            };
            db.LearningPaths.Add(course);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            course.AuthorId = teacherId;
            course.CreatedBy = teacherId;
            course.Status = LearningPathStatus.Active;
            course.Visibility = PathVisibility.Public;
            course.IsActive = true;
            await db.SaveChangesAsync(ct);
        }

        var lessonsData = new[]
        {
            ("Cây nhị phân & 3 phương pháp duyệt (Inorder, Preorder, Postorder)",
             "# Cây nhị phân (Binary Tree) & Các phương pháp duyệt cây\n\n## 1. Định nghĩa Cây Nhị phân\nCây nhị phân là cấu trúc dữ liệu phân cấp phi tuyến tính, trong đó mỗi node có **tối đa 2 node con** (thường gọi là con Trái và con Phải).\n\n## 2. Ba phương pháp duyệt cây Depth-First Search (DFS):\n1. **Pre-order (Gốc - Trái - Phải)**: Dùng để sao chép cây hoặc sinh biểu thức tiền tố.\n2. **In-order (Trái - Gốc - Phải)**: Cho thứ tự tăng dần khi duyệt trên Cây Tìm kiếm Nhị phân (BST).\n3. **Post-order (Trái - Phải - Gốc)**: Dùng để xóa cây hoặc tính toán kích thước thư mục từ dưới lên.",
             "tree.bst-preorder", PathItemType.Theory, 1),

            ("Cây tìm kiếm nhị phân (BST) — Chèn, Tìm kiếm & Xóa",
             "# Cây tìm kiếm nhị phân (Binary Search Tree - BST)\n\n## 1. Quy tắc BST:\nVới mọi node $X$ trên cây:\n- Mọi node thuộc cây con **Trái** có giá trị $< X.value$\n- Mọi node thuộc cây con **Phải** có giá trị $> X.value$\n\n## 2. Độ phức tạp thời gian:\n- **Trường hợp tốt/trung bình (Cây cân bằng)**: $O(\\log N)$ cho thao tác Tìm kiếm, Chèn và Xóa.\n- **Trường hợp xấu nhất (Cây thoái hóa thành đường thẳng)**: $O(N)$ khi các phần tử được chèn theo thứ tự tăng/giảm dần.",
             "tree.bst-insert", PathItemType.Theory, 2),

            ("Cây tự cân bằng AVL — Cơ chế xoay LL, RR, LR, RL",
             "# Cây tự cân bằng AVL\n\n## 1. Hệ số cân bằng (Balance Factor - BF)\n$$\\text{BF}(Node) = \\text{Height}(\\text{LeftSubtree}) - \\text{Height}(\\text{RightSubtree})$$\nCây AVL đảm bảo với mọi node, $\\text{BF} \\in \\{-1, 0, 1\\}$. Khi $\\text{BF} \\ge 2$ hoặc $\\text{BF} \\le -2$, cây bị mất cân bằng và cần xoay.\n\n## 2. Bốn trường hợp xoay cây:\n- **Trường hợp Left-Left (LL)**: Xoay Phải (Right Rotation)\n- **Trường hợp Right-Right (RR)**: Xoay Trái (Left Rotation)\n- **Trường hợp Left-Right (LR)**: Xoay Trái node con trước, sau đó Xoay Phải node cha\n- **Trường hợp Right-Left (RL)**: Xoay Phải node con trước, sau đó Xoay Trái node cha",
             "tree.avl-insert", PathItemType.Theory, 3),

            ("Đống nhị phân (Heap) & Hàng đợi ưu tiên (Priority Queue)",
             "# Đống nhị phân (Binary Heap) & Hàng đợi ưu tiên\n\n## 1. Định nghĩa Heap\nHeap là một cây nhị phân gần hoàn chỉnh (Complete Binary Tree):\n- **Max-Heap**: Giá trị của node cha luôn $\\ge$ các node con. Node gốc (Root) luôn là phần tử lớn nhất.\n- **Min-Heap**: Giá trị của node cha luôn $\\le$ các node con. Node gốc (Root) luôn là phần tử nhỏ nhất.\n\n## 2. Biểu diễn Heap bằng Mảng:\nVới node ở chỉ số `i` (0-indexed):\n- Con trái: `2*i + 1`\n- Con phải: `2*i + 2`\n- Node cha: `(i - 1) / 2`",
             "heap.insert", PathItemType.Theory, 4)
        };

        Lesson? lastLesson = null;
        foreach (var (title, content, simKey, itemType, sortOrder) in lessonsData)
        {
            var lesson = await EnsureLessonAsync(db, title, content, topicId, teacherId, now, ct);
            await EnsurePathNodeAsync(db, course.Id, title, lesson.Id, itemType, sortOrder, ct);
            await EnsureSimulationAsync(db, lesson.Id, simKey, title, ct);
            lastLesson = lesson;
        }

        // Quiz BST & AVL
        var quizExercise = await EnsureQuizExerciseAsync(db, course.Id, lastLesson!.Id, "Quiz: Chuyên sâu Cây BST & AVL", teacherId, now, [
            ("Duyệt In-order trên Cây tìm kiếm nhị phân (BST) cho ra kết quả dãy số có tính chất gì?",
             new[] { "Dãy số có thứ tự tăng dần", "Dãy số có thứ tự giảm dần", "Dãy số ngẫu nhiên", "Dãy số theo thứ tự tầng từ trên xuống" }, 0, "Tính chất cốt lõi của BST là cây con trái < gốc < cây con phải. Khi duyệt In-order (Trái - Gốc - Phải), các phần tử sẽ được xuất ra theo đúng thứ tự tăng dần."),
            ("Độ cao tối đa của cây AVL chứa N phần tử xấp xỉ bằng bao nhiêu?",
             new[] { "1.44 * log2(N)", "N / 2", "log10(N)", "N^2" }, 0, "Do cây AVL luôn giữ hệ số cân bằng trong khoảng [-1, 1], độ cao luôn bị giới hạn trên bởi khoảng 1.44 * log2(N), đảm bảo tra cứu O(log N)."),
            ("Khi chèn một node mới vào cây con Trái của node con Phải gây mất cân bằng, ta cần áp dụng phép xoay nào?",
             new[] { "Xoay Right-Left (RL)", "Xoay Left-Left (LL)", "Xoay Right-Right (RR)", "Xoay Left-Right (LR)" }, 0, "Lệch Phải rồi Trái (RL) cần xoay Phải tại con rồi xoay Trái tại gốc.")
        ], ct);

        await EnsurePathNodeWithFinalTestAsync(db, course.Id, "Quiz Tổng kết: Cây Nhị phân & Cây Tự cân bằng", quizExercise.Id, 5, ct);
    }

    private static async Task SeedDraftAlgoCourseAsync(AppDbContext db, int teacherId, int topicId, DateTime now, ILogger logger, CancellationToken ct)
    {
        const string courseTitle = "Thiết kế Thuật toán & Tối ưu hóa Nâng cao (Bản nháp)";
        var course = await db.LearningPaths.FirstOrDefaultAsync(p => p.Title == courseTitle, ct);
        if (course is null)
        {
            course = new LearningPath
            {
                Title = courseTitle,
                Description = "Khóa học nâng cao về kỹ thuật Chia để trị, Quy hoạch động và Tối ưu độ phức tạp (Đang được Giảng viên soạn thảo nội dung).",
                TopicId = topicId,
                SortOrder = 5,
                Status = LearningPathStatus.Draft,
                Visibility = PathVisibility.Private,
                IsActive = false,
                CreatedBy = teacherId,
                AuthorId = teacherId,
                HighlightsJson = JsonSerializer.Serialize(new
                {
                    highlights = new[]
                    {
                        new { title = "Chiến lược Chia để trị", description = "Phân tích định lý thợ Master Theorem và bài toán đếm nghịch thế." },
                        new { title = "Quy hoạch động từ dễ đến khó", description = "Xây dựng phương trình trạng thái và bảng phương án tối ưu." }
                    },
                    learningObjectives = new[]
                    {
                        "Nắm vững kỹ thuật đệ quy và phân rã bài toán lớn thành các bài toán con",
                        "Làm chủ bảng phương án quy hoạch động 1 chiều và 2 chiều"
                    },
                    keyOutcomes = new[]
                    {
                        "Giải quyết các bài toán tối ưu hóa phức tạp trong lập trình thi đấu"
                    }
                })
            };
            db.LearningPaths.Add(course);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            course.AuthorId = teacherId;
            course.CreatedBy = teacherId;
            course.Status = LearningPathStatus.Draft;
            course.Visibility = PathVisibility.Private;
            course.IsActive = false;
            await db.SaveChangesAsync(ct);
        }

        var lessonDraft1 = await EnsureLessonAsync(db, "Kỹ thuật Chia để trị (Divide and Conquer)", 
            "# Kỹ thuật Chia để trị (Divide and Conquer)\n\nNội dung đang được Giảng viên biên soạn...", topicId, teacherId, now, ct, LessonStatus.Draft);
        var lessonDraft2 = await EnsureLessonAsync(db, "Quy hoạch động 1 chiều & Bảng phương án", 
            "# Quy hoạch động (Dynamic Programming)\n\nNội dung đang được Giảng viên biên soạn...", topicId, teacherId, now, ct, LessonStatus.Draft);
        var lessonDraft3 = await EnsureLessonAsync(db, "Quy hoạch động 2 chiều & Tối ưu hóa", 
            "# Quy hoạch động 2 chiều (2D Dynamic Programming)\n\nNội dung đang được Giảng viên biên soạn...", topicId, teacherId, now, ct, LessonStatus.Draft);
        var lessonDraft4 = await EnsureLessonAsync(db, "Tối ưu hóa Độ phức tạp Thuật toán", 
            "# Tối ưu hóa Độ phức tạp Thuật toán (Optimization)\n\nNội dung đang được Giảng viên biên soạn...", topicId, teacherId, now, ct, LessonStatus.Draft);

        await EnsurePathNodeAsync(db, course.Id, "Bài 1: Kỹ thuật Chia để trị (Draft)", lessonDraft1.Id, PathItemType.Theory, 1, ct);
        await EnsurePathNodeAsync(db, course.Id, "Bài 2: Quy hoạch động 1 chiều (Draft)", lessonDraft2.Id, PathItemType.Theory, 2, ct);
        await EnsurePathNodeAsync(db, course.Id, "Bài 3: Quy hoạch động 2 chiều (Draft)", lessonDraft3.Id, PathItemType.Theory, 3, ct);
        await EnsurePathNodeAsync(db, course.Id, "Bài 4: Tối ưu độ phức tạp (Draft)", lessonDraft4.Id, PathItemType.Theory, 4, ct);
    }

    private static async Task<Lesson> EnsureLessonAsync(AppDbContext db, string title, string content, int topicId, int teacherId, DateTime now, CancellationToken ct, LessonStatus status = LessonStatus.Active)
    {
        var existing = await db.Lessons.FirstOrDefaultAsync(l => l.Title == title, ct);
        if (existing is not null)
        {
            existing.ContentHtml = content;
            existing.CreatedBy = teacherId;
            existing.TopicId = topicId;
            existing.Status = status;
            await db.SaveChangesAsync(ct);
            return existing;
        }

        var lesson = new Lesson
        {
            Title = title,
            Description = title,
            ContentHtml = content,
            TopicId = topicId,
            SortOrder = 1,
            Status = status,
            CreatedBy = teacherId,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(ct);
        return lesson;
    }

    private static async Task EnsurePathNodeAsync(AppDbContext db, int pathId, string title, int lessonId, PathItemType itemType, int sortOrder, CancellationToken ct)
    {
        var node = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.PathId == pathId && n.LessonId == lessonId, ct);
        if (node is null)
        {
            node = new LearningPathNode
            {
                PathId = pathId,
                Title = title,
                LessonId = lessonId,
                ItemType = itemType,
                SortOrder = sortOrder
            };
            db.LearningPathNodes.Add(node);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            node.Title = title;
            node.ItemType = itemType;
            node.SortOrder = sortOrder;
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task EnsurePathNodeWithFinalTestAsync(AppDbContext db, int pathId, string title, int exerciseId, int sortOrder, CancellationToken ct)
    {
        var node = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.PathId == pathId && n.FinalTestId == exerciseId, ct);
        if (node is null)
        {
            node = new LearningPathNode
            {
                PathId = pathId,
                Title = title,
                FinalTestId = exerciseId,
                ItemType = PathItemType.Quiz,
                SortOrder = sortOrder
            };
            db.LearningPathNodes.Add(node);
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task EnsureSimulationAsync(AppDbContext db, int lessonId, string simKey, string title, CancellationToken ct)
    {
        var existing = await db.LessonSimulations.FirstOrDefaultAsync(s => s.LessonId == lessonId && s.SimulationKey == simKey, ct);
        if (existing is null)
        {
            db.LessonSimulations.Add(new LessonSimulation
            {
                LessonId = lessonId,
                SimulationKey = simKey,
                Title = title,
                SortOrder = 1
            });
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task<Exercise> EnsureQuizExerciseAsync(AppDbContext db, int pathId, int lessonId, string title, int teacherId, DateTime now, (string Text, string[] Options, int Correct, string Explanation)[] questions, CancellationToken ct)
    {
        var existing = await db.Exercises.Include(e => e.Questions).FirstOrDefaultAsync(e => e.Title == title && e.DeletedAt == null, ct);
        if (existing is not null)
        {
            return existing;
        }

        var exercise = new Exercise
        {
            LessonId = lessonId,
            Title = title,
            Description = $"Trắc nghiệm đánh giá kiến thức — {questions.Length} câu hỏi.",
            Type = ExerciseType.Mcq,
            MaxScore = questions.Length,
            Status = ExerciseStatus.Active,
            CreatedBy = teacherId,
            CreatedAt = now
        };
        db.Exercises.Add(exercise);
        await db.SaveChangesAsync(ct);

        var sort = 1;
        foreach (var (text, opts, correct, exp) in questions)
        {
            db.Questions.Add(new Question
            {
                ExerciseId = exercise.Id,
                Type = QuestionType.Single,
                Content = text,
                OptionsJson = JsonSerializer.Serialize(opts),
                AnswerJson = $"[{correct}]",
                Explanation = exp,
                KeepOrder = false,
                Points = 1,
                SortOrder = sort++
            });
        }
        await db.SaveChangesAsync(ct);
        return exercise;
    }
}
