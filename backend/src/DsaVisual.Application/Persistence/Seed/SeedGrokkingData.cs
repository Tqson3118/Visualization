using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// Import khóa "Grokking Data Structures" (nguồn: VisualizationDSA-main SQLite → backend/seed-data/grokking-course.json).
/// - 4 topics mới (theo 4 module) + 16 lessons (8 theory tiếng Việt + 4 mini-quizz + 4 assignment codelab).
/// - 1 path lớn "Grokking Data Structures" (~18 nodes: Học bài → mini-quizz → assignment → luyện tập → kiểm tra cuối).
/// - Quiz nguồn → Exercise MCQ; Assignment (testCases trong sandboxConfig) → Exercise CODE; Lab → Exercise SIMULATION_LAB.
/// - 5 path cũ set IsActive=false (dữ liệu giữ nguyên — seed activity/test tham chiếu không vỡ).
/// Idempotent: kiểm tra tồn tại trước khi chèn (title/tên path).
/// </summary>
public static partial class SeedGrokkingData
{
    private const string SeedJsonRelative = "backend\\seed-data\\grokking-course.json";

    // ── Map module nguồn → Topic (module order 1000..8000) ──
    private static readonly (int Order, string Name, string Description)[] ModuleTopics =
    [
        (1000, "Cấu trúc Dữ liệu Tuyến tính cốt lõi", "Array (Mảng), Linked List (Danh sách liên kết) — nền tảng tuyến tính của mọi cấu trúc dữ liệu."),
        (2000, "Quản lý luồng dữ liệu (LIFO & FIFO)", "Stack (Ngăn xếp), Queue & Deque (Hàng đợi) — kiểm soát thứ tự xử lý dữ liệu."),
        (3000, "Tra cứu dữ liệu tốc độ cao (Hashing)", "Hash Map (Bảng băm), Hash Set (Tập hợp băm) — truy cập O(1) qua hàm băm."),
        (4000, "Cấu trúc dữ liệu Phân cấp (Cây cơ bản)", "Tree (Cây tổng quát), Binary Search Tree (BST) — tổ chức dữ liệu theo phân cấp."),
        (5000, "Cây nhị phân tìm kiếm & Cân bằng (BST & AVL)", "Binary Search Tree (BST) — tìm kiếm nhanh O(log n); AVL — tự cân bằng sau mỗi thao tác."),
        (6000, "Đống nhị phân (Heap) & Hàng đợi ưu tiên", "Heap (Đống nhị phân) — chèn/trích xuất O(log n); Priority Queue — xử lý theo độ ưu tiên."),
        (7000, "Đồ thị (Graph)", "Đồ thị — mô hình quan hệ; BFS/DFS — duyệt và tìm đường đi ngắn nhất."),
        (8000, "Cấu trúc dữ liệu nâng cao (Trie, Set/Map)", "Trie — tìm kiếm theo tiền tố; Set/Map — tra cứu O(1) và ứng dụng thực tế."),
    ];

    /// <summary>Lesson nguồn → (LessonId nguồn, title, moduleOrder, sandboxType). Lấy từ JSON lúc runtime.</summary>
    private sealed record SourceLesson(string Id, string Title, int ModuleOrder, int OrderIndex, string SandboxType, string ContentMd, string? SandboxConfig);

    /// <summary>Quiz nguồn chọn theo chủ đề — (title lesson đích, quiz title nguồn).</summary>
    private static readonly (string LessonTitle, string QuizTitle)[] QuizMap =
    [
        ("Bài 1: Array (Mảng)", "Trắc nghiệm Mảng cơ bản"),
        ("Bài 2: Linked List (Danh sách liên kết)", "Trắc nghiệm Linked List"),
        ("Bài 3: Stack (Ngăn xếp)", "Trắc nghiệm Stack"),
        ("Bài 4: Queue & Deque", "Trắc nghiệm Queue & Deque"),
        ("Bài 5: Hash Map (Bảng băm)", "Trắc nghiệm Hash Table & Set"),
        ("Bài 6: Hash Set (Tập hợp băm)", "Trắc nghiệm Tổng Hợp: Hash Map & Hash Set"),
        ("Bài 7: Tree (Cây tổng quát)", "Trắc nghiệm Duyệt cây"),
        ("Bài 8: Binary Search Tree", "Trắc nghiệm BST"),
        ("Mini-Quizz 1: Array vs Linked List", "Trắc nghiệm Tổng Hợp: Array & Linked List"),
        ("Mini-Quizz 2: LIFO hay FIFO?", "Trắc nghiệm Tổng Hợp: Stack & Queue"),
        ("Mini-Quizz 3: Collision", "Trắc nghiệm Cấu trúc dữ liệu nâng cao"),
        ("Mini-Quizz 4: Duyệt cây BST", "Trắc nghiệm Tổng Hợp: Tree & Binary Search Tree"),
        ("Bài 9: Binary Search Tree — Thao tác", "Trắc nghiệm Tổng Hợp: BST & AVL"),
        ("Bài 10: AVL — Cây cân bằng & Xoay", "Trắc nghiệm Tổng Hợp: BST & AVL"),
        ("Mini-Quizz 5: BST & AVL", "Trắc nghiệm Tổng Hợp: BST & AVL"),
        ("Bài 11: Heap — Đống nhị phân", "Trắc nghiệm Tổng Hợp: Heap & Priority Queue"),
        ("Bài 12: Priority Queue & Ứng dụng", "Trắc nghiệm Tổng Hợp: Heap & Priority Queue"),
        ("Mini-Quizz 6: Heap & Priority Queue", "Trắc nghiệm Tổng Hợp: Heap & Priority Queue"),
        ("Bài 13: Đồ thị & Cách biểu diễn", "Trắc nghiệm Tổng Hợp: Đồ thị"),
        ("Bài 14: Duyệt BFS & DFS", "Trắc nghiệm Tổng Hợp: Đồ thị"),
        ("Mini-Quizz 7: Đồ thị (Graph)", "Trắc nghiệm Tổng Hợp: Đồ thị"),
        ("Bài 15: Trie — Cây tiền tố", "Trắc nghiệm Tổng Hợp: Trie & Set/Map"),
        ("Bài 16: Set & Map — Ứng dụng thực tế", "Trắc nghiệm Tổng Hợp: Trie & Set/Map"),
        ("Mini-Quizz 8: Trie & Set/Map", "Trắc nghiệm Tổng Hợp: Trie & Set/Map"),
    ];

    /// <summary>Lab simulation key theo bài (LAB exercise) — catalog shared/simulation-catalog.json.</summary>
    private static readonly (string LessonTitle, string[] Keys)[] LabSimKeys =
    [
        ("Bài 1: Array (Mảng)", ["structure.array"]),
        ("Bài 2: Linked List (Danh sách liên kết)", ["structure.linkedlist", "list.insert", "list.traverse"]),
        ("Bài 3: Stack (Ngăn xếp)", ["stack.push", "stack.pop", "stack.peek"]),
        ("Bài 4: Queue & Deque", ["queue.enqueue", "queue.dequeue"]),
        ("Bài 5: Hash Map (Bảng băm)", ["hash.insert", "hash.search"]),
        ("Bài 6: Hash Set (Tập hợp băm)", ["hash.search", "hash.delete"]),
        ("Bài 7: Tree (Cây tổng quát)", ["structure.binarytree", "tree.bst-preorder"]),
        ("Bài 8: Binary Search Tree", ["tree.bst-insert", "tree.bst-inorder", "tree.bst-search"]),
        ("Bài 9: Binary Search Tree — Thao tác", ["tree.bst-insert", "tree.bst-search", "tree.bst-delete", "structure.bst"]),
        ("Bài 10: AVL — Cây cân bằng & Xoay", ["tree.avl-insert"]),
        ("Bài 11: Heap — Đống nhị phân", ["heap.insert", "heap.extract", "heap.heapify"]),
        ("Bài 12: Priority Queue & Ứng dụng", ["heap.extract", "sort.heap"]),
        ("Bài 13: Đồ thị & Cách biểu diễn", ["structure.graph"]),
        ("Bài 14: Duyệt BFS & DFS", ["graph.bfs", "graph.dfs", "graph.dijkstra"]),
        ("Bài 16: Set & Map — Ứng dụng thực tế", ["hash.search", "hash.insert", "structure.hashtable"]),
    ];

    /// <summary>Simulation key cho Assignment CODE (template theo module — client sandbox).</summary>
    private static readonly (string LessonTitle, string Key)[] AssignmentSimKeys =
    [
        ("Assignment 1: Quản lý sinh viên", "structure.linkedlist"),
        ("Assignment 2: Điều phối vé xem phim", "structure.stack"),
        ("Assignment 3: Giỏ hàng tốc độ cao", "structure.hashtable"),
        ("Assignment 4: Product Catalog", "structure.bst"),
        ("Assignment 5: Quản lý kho hàng (BST)", "structure.bst"),
        ("Assignment 6: Tuyến phòng khám (Priority Queue)", "structure.heap"),
        ("Assignment 7: Mạng lưới giao hàng (BFS)", "structure.graph"),
        ("Assignment 8: Tự động gợi ý từ khóa (Trie)", "structure.hashtable"),
    ];

    public static async Task SeedAsync(AppDbContext db, int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        var jsonPath = FindSeedJson();
        if (jsonPath is null)
        {
            logger.LogWarning("SeedGrokking: KHÔNG tìm thấy {Rel} — bỏ qua import khóa Grokking", SeedJsonRelative);
            return;
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath, new UTF8Encoding(false)));
        var root = doc.RootElement;

        // ── 1. Topics mới (4 module) ──
        var topics = new Dictionary<int, Topic>(); // moduleOrder → Topic
        foreach (var (order, name, desc) in ModuleTopics)
        {
            var topic = await db.Topics.FirstOrDefaultAsync(t => t.ParentId == null && t.Name == name, ct);
            if (topic is null)
            {
                topic = new Topic { Name = name, Description = desc, SortOrder = 100 + order / 1000, CreatedBy = adminId, CreatedAt = now };
                db.Topics.Add(topic);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("SeedGrokking: Topics thêm {Name} (Id={Id})", name, topic.Id);
            }
            topics[order] = topic;
        }

        // ── 2. Lessons nguồn (từ JSON — lessons là mảng phẳng, mỗi lesson có moduleId) ──
        var moduleOrderById = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var mod in root.GetProperty("modules").EnumerateArray())
        {
            moduleOrderById[mod.GetProperty("id").GetString()!] = mod.GetProperty("orderIndex").GetInt32();
        }

        var sources = new List<SourceLesson>();
        foreach (var l in root.GetProperty("lessons").EnumerateArray())
        {
            var moduleId = l.GetProperty("moduleId").GetString();
            if (moduleId is null || !moduleOrderById.TryGetValue(moduleId, out var modOrder))
            {
                continue;
            }

            sources.Add(new SourceLesson(
                l.GetProperty("id").GetString()!,
                l.GetProperty("title").GetString()!,
                modOrder,
                l.TryGetProperty("orderIndex", out var oi) && oi.TryGetInt32(out var o) ? o : 0,
                l.GetProperty("sandboxType").GetString()!,
                l.GetProperty("contentMd").GetString() ?? string.Empty,
                l.TryGetProperty("sandboxConfig", out var sc) && sc.ValueKind == JsonValueKind.String ? sc.GetString() : null));
        }
        sources = sources.OrderBy(s => s.ModuleOrder).ThenBy(s => s.OrderIndex).ToList();

        // ── 3. Lessons + LessonSimulations ──
        // LƯU NGUYÊN contentMd (MARKDOWN + HTML diagram): FE LessonStepTheory tự render
        // markdown (headings/code fence/bảng/inline code) + HTML diagram. KHÔNG sanitize
        // như HTML — sanitize markdown như HTML làm AngleSharp parse nhầm `Stack<T>`
        // (nuốt toàn bộ phần sau) → mất ~74% nội dung (audit 14/08: bài 3-6).
        var lessonsByTitle = new Dictionary<string, Lesson>(StringComparer.Ordinal);
        var lessonContentUpdated = 0;
        foreach (var src in sources)
        {
            var topic = topics[src.ModuleOrder];
            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.TopicId == topic.Id && l.Title == src.Title, ct);
            var contentMd = src.ContentMd;
            if (lesson is null)
            {
                lesson = new Lesson
                {
                    TopicId = topic.Id,
                    Title = src.Title,
                    Description = MakeDescription(src),
                    ContentHtml = contentMd,
                    SortOrder = sources.IndexOf(src) + 1,
                    Status = LessonStatus.Active,
                    CreatedBy = adminId,
                    CreatedAt = now
                };
                db.Lessons.Add(lesson);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("SeedGrokking: Lessons thêm {Title} (Id={Id})", src.Title, lesson.Id);
            }
            else if (lesson.ContentHtml != contentMd)
            {
                // Tự chữa (self-healing): lần seed đầu lưu content đã bị sanitize cắt cụt;
                // cập nhật lại theo JSON gốc để không "rớt miếng" nào so với nguồn.
                lesson.ContentHtml = contentMd;
                if (lesson.Description != MakeDescription(src))
                {
                    lesson.Description = MakeDescription(src);
                }
                lessonContentUpdated++;
            }
            lessonsByTitle[src.Title] = lesson;
        }
        if (lessonContentUpdated > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokking: Tự chữa {Count} bài học (cập nhật nội dung theo JSON gốc)", lessonContentUpdated);
        }

        await SeedLessonSimulationsAsync(db, lessonsByTitle, logger, ct);

        // ── 3b. Đánh giá mẫu (ContentFeedback) cho bài Grokking — rating khóa hiển thị thật ──
        // Dùng 3 user base (admin/teacher/student@demo — tạo ở SeedUsersAsync, luôn tồn tại TRƯỚC
        // bước này) để idempotent: "first 6 users" đổi giữa các lần seed (V2 tạo thêm 69 user
        // ở bước sau) → lần chạy thứ 2 tạo cặp mới → vỡ test Seed_SecondRun_DoesNotChangeAnyCount.
        var grokkingLessons = lessonsByTitle.Values
            .OrderBy(l => l.SortOrder)
            .Take(8)
            .ToList();
        var ratingEmails = new[] { "admin@system.local", "teacher@demo.local", "student@demo.local" };
        var ratingUsers = new List<User>();
        foreach (var email in ratingEmails)
        {
            var u = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email && x.DeletedAt == null, ct);
            if (u is not null)
            {
                ratingUsers.Add(u);
            }
        }
        var ratingPattern = new[] { 5, 4, 5, 4, 5, 3, 4, 4 };
        var ratingAdded = 0;
        for (var i = 0; i < grokkingLessons.Count && ratingUsers.Count > 0; i++)
        {
            var lesson = grokkingLessons[i];
            var user = ratingUsers[i % ratingUsers.Count];
            var exists = await db.ContentFeedback.AsNoTracking()
                .AnyAsync(f => f.UserId == user.Id && f.LessonId == lesson.Id, ct);
            if (!exists)
            {
                db.ContentFeedback.Add(new ContentFeedback
                {
                    UserId = user.Id,
                    LessonId = lesson.Id,
                    Rating = ratingPattern[i % ratingPattern.Length],
                    CreatedAt = now.AddDays(-10 + i)
                });
                ratingAdded++;
            }
        }
        if (ratingAdded > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokking: Thêm {Count} đánh giá mẫu cho bài học Grokking", ratingAdded);
        }

        // ── 4. Path lớn "Grokking Data Structures" + nodes ──
        const string pathTitle = "Grokking Data Structures";
        var path = await db.LearningPaths.FirstOrDefaultAsync(p => p.Title == pathTitle, ct);
        if (path is null)
        {
            path = new LearningPath
            {
                Title = pathTitle,
                Description = "Khóa học Cấu trúc dữ liệu hoàn chỉnh: Array, Linked List, Stack, Queue, Hash Map/Hash Set, Tree, BST & AVL, Heap & Priority Queue, Đồ thị (BFS/DFS), Trie và Set/Map — lý thuyết trực quan, mini-quizz, assignment thực hành và kiểm tra cuối.",
                TopicId = topics[1000].Id,
                SortOrder = 1,
                IsActive = true,
                Status = LearningPathStatus.Active,
                Visibility = PathVisibility.Public,
                CreatedBy = adminId
            };
            db.LearningPaths.Add(path);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokking: LearningPaths thêm {Title} (Id={Id})", pathTitle, path.Id);
        }

        // Reconcile cả record cũ: seed trước đây bỏ sót Status/Visibility nên fresh DB cũ có thể là Draft/Private.
        if (path.Status != LearningPathStatus.Active || path.Visibility != PathVisibility.Public || !path.IsActive)
        {
            path.Status = LearningPathStatus.Active;
            path.Visibility = PathVisibility.Public;
            path.IsActive = true;
            await db.SaveChangesAsync(ct);
        }

        // ── 4b. Tác giả (AuthorId = teacher@demo.local) + nội dung marketing tùy biến theo khóa ──
        var teacher = await db.Users
            .FirstOrDefaultAsync(u => u.Email == "teacher@demo.local" && u.DeletedAt == null, ct);

        // Backfill hồ sơ giảng viên (phần ABOUT THE AUTHOR ở trang khóa) — idempotent
        if (teacher is not null &&
            (string.IsNullOrWhiteSpace(teacher.TeacherBio) || string.IsNullOrWhiteSpace(teacher.AcademicDegree)))
        {
            teacher.TeacherBio = "Giảng viên Cấu trúc Dữ liệu & Giải thuật tại DsaVisual — đồng hành cùng hàng nghìn sinh viên và fresher ôn luyện phỏng vấn kỹ thuật, chuyên đào tạo tư duy giải thuật qua hình ảnh trực quan và thực hành từng bước.";
            teacher.AcademicDegree = "Thạc sĩ Khoa học Máy tính";
            teacher.ProfileLink = "https://www.linkedin.com";
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokking: Cập nhật hồ sơ giảng viên (bio/degree/link)");
        }

        var highlightsJson = JsonSerializer.Serialize(new[]
        {
            new { title = "Học đúng trọng tâm phỏng vấn", description = "Các cấu trúc dữ liệu phỏng vấn hay hỏi nhất được trình bày gọn gàng, kèm hình ảnh trực quan từng bước — hiểu bản chất thay vì học thuộc." },
            new { title = "Lý thuyết + thực hành đan xen", description = "Mỗi module gồm bài giảng trực quan, mini-quizz kiểm tra nhanh và assignment lập trình ngay trên trình soạn thảo để củng cố ngay sau khi học." },
            new { title = "Mô phỏng từng bước (Step-by-step)", description = "Trực quan hóa thao tác chèn, xóa, duyệt dữ liệu trên từng cấu trúc — nhìn thấy thuật toán chạy bằng mắt, dễ nhớ dễ hiểu." },
            new { title = "Chấm điểm tự động + phản hồi", description = "Bài tập code được chạy thử và chấm tự động ngay trong trình duyệt; sai chỗ nào biết ngay chỗ đó, không chờ ai sửa bài." }
        });
        var testimonialsJson = JsonSerializer.Serialize(new[]
        {
            new { name = "Minh Quân", role = "Sinh viên năm 3 CNTT", quote = "Trước giờ mình chỉ học thuộc code mẫu, chứ chưa từng THẤY Linked List chạy. Xem mô phỏng từng bước xong, bài phỏng vấn đầu tiên mình tự tin trả lời rõ ràng." },
            new { name = "Thu Hà", role = "Junior Developer", quote = "Phần Stack/Queue được ví dụ rất dễ hiểu. Mình ôn lại toàn bộ kiến thức cơ bản trong 2 tuần trước buổi phỏng vấn và đã đậu." },
            new { name = "Đức Anh", role = "Fresher Backend", quote = "Assignment code chấm tự động cực kỳ hữu ích — làm xong biết ngay mình sai case nào, tiết kiệm rất nhiều thời gian tự kiểm tra." }
        });

        var pathChanged = false;
        var teacherId = teacher?.Id;
        if (path.AuthorId != teacherId || path.HighlightsJson != highlightsJson || path.TestimonialsJson != testimonialsJson)
        {
            path.AuthorId = teacherId;
            path.HighlightsJson = highlightsJson;
            path.TestimonialsJson = testimonialsJson;
            pathChanged = true;
        }
        if (pathChanged)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokking: Cập nhật tác giả + marketing cho {Title} (AuthorId={AuthorId})", pathTitle, teacherId);
        }

        // Nodes: Học bài → Mini-Quizz → Assignment (theo module order) + Luyện tập + Kiểm tra cuối
        var nodeTitles = new List<(string Title, string? LessonTitle)>();
        foreach (var src in sources.OrderBy(s => s.ModuleOrder))
        {
            nodeTitles.Add(($"Học: {src.Title}", src.Title));
        }

        // SortOrder bắt đầu SAU node cao nhất hiện có của path — seed chạy lại (thêm module mới
        // vào path đã tồn tại) không được đè SortOrder gây vi phạm unique (PathId, SortOrder).
        var sortOrder = (await db.LearningPathNodes
            .Where(n => n.PathId == path.Id)
            .Select(n => (int?)n.SortOrder)
            .MaxAsync(ct) ?? 0) + 1;
        var nodesByLessonTitle = new Dictionary<string, LearningPathNode>(StringComparer.Ordinal);
        foreach (var (nodeTitle, lessonTitle) in nodeTitles)
        {
            var exists = await db.LearningPathNodes.AnyAsync(n => n.PathId == path.Id && n.Title == nodeTitle, ct);
            if (!exists)
            {
                var node = new LearningPathNode
                {
                    PathId = path.Id,
                    Title = nodeTitle,
                    LessonId = lessonTitle is not null && lessonsByTitle.TryGetValue(lessonTitle, out var l) ? l.Id : null,
                    SortOrder = sortOrder
                };
                db.LearningPathNodes.Add(node);
                await db.SaveChangesAsync(ct);
                if (lessonTitle is not null)
                {
                    nodesByLessonTitle[lessonTitle] = node;
                }
                logger.LogInformation("SeedGrokking: Nodes thêm {Node} (path={Path})", nodeTitle, pathTitle);
            }
            else
            {
                var node = await db.LearningPathNodes.FirstAsync(n => n.PathId == path.Id && n.Title == nodeTitle, ct);
                if (lessonTitle is not null)
                {
                    nodesByLessonTitle[lessonTitle] = node;
                }
            }
            sortOrder++;
        }

        // Node Final-Quizz (trước đây là "Luyện tập tổng hợp" — đổi tên idempotent: DB cũ
        // giữ tên cũ thì rename, chưa có thì tạo mới)
        const string practiceNodeTitle = "Final-Quizz";
        var practiceNode = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.PathId == path.Id && n.Title == practiceNodeTitle, ct);
        if (practiceNode is null)
        {
            const string oldPracticeNodeTitle = "Luyện tập tổng hợp";
            practiceNode = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.PathId == path.Id && n.Title == oldPracticeNodeTitle, ct);
            if (practiceNode is not null)
            {
                practiceNode.Title = practiceNodeTitle;
                await db.SaveChangesAsync(ct);
                logger.LogInformation("SeedGrokking: Nodes đổi tên {Old} → {New} (path={Path})", oldPracticeNodeTitle, practiceNodeTitle, pathTitle);
            }
            else
            {
                practiceNode = new LearningPathNode { PathId = path.Id, Title = practiceNodeTitle, LessonId = null, ItemType = PathItemType.Quiz, SortOrder = sortOrder };
                db.LearningPathNodes.Add(practiceNode);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("SeedGrokking: Nodes thêm {Node} (path={Path})", practiceNodeTitle, pathTitle);
            }
        }
        sortOrder++;

        // Node kiểm tra cuối
        const string finalNodeTitle = "Kiểm tra cuối lộ trình";
        var finalNode = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.PathId == path.Id && n.Title == finalNodeTitle, ct);
        if (finalNode is null)
        {
            finalNode = new LearningPathNode { PathId = path.Id, Title = finalNodeTitle, LessonId = null, ItemType = PathItemType.Quiz, SortOrder = sortOrder };
            db.LearningPathNodes.Add(finalNode);
            await db.SaveChangesAsync(ct);
        }

        // ── 5. Exercises: Quiz (MCQ từ nguồn) + Lab (simulation) + Assignment (CODE) ──
        var quizByTitle = LoadQuizzes(root);
        await SeedQuizExercisesAsync(db, lessonsByTitle, nodesByLessonTitle, quizByTitle, adminId, now, logger, ct);
        await SeedLabExercisesAsync(db, lessonsByTitle, nodesByLessonTitle, adminId, now, logger, ct);
        await SeedAssignmentExercisesAsync(db, lessonsByTitle, nodesByLessonTitle, adminId, now, logger, ct);

        // ── 6. Final-Quizz (20 câu trắc nghiệm tổng hợp — 5 câu/module) ──
        await SeedFinalQuizzAsync(db, path, practiceNode, lessonsByTitle, quizByTitle, adminId, now, logger, ct);

        // ── 6b. Kiểm tra cuối lộ trình (3 bài code theo phong cách Assignment — Monaco + testcase) ──
        await SeedFinalCodingTestAsync(db, path, finalNode, lessonsByTitle, adminId, now, logger, ct);

        // ── 7. Ẩn đúng các path legacy do seed cũ sở hữu; không chạm course giáo viên ──
        var legacyTitles = new[] { "Cấu trúc dữ liệu", "Giải thuật", "Sắp xếp & Tìm kiếm", "CTDL tuyến tính", "Đồ thị" };
        var oldPaths = await db.LearningPaths
            .Where(p => p.IsActive && legacyTitles.Contains(p.Title))
            .ToListAsync(ct);
        foreach (var oldPath in oldPaths)
        {
            oldPath.IsActive = false;
        }
        if (oldPaths.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokking: Ẩn {Count} path cũ (IsActive=false)", oldPaths.Count);
        }

        logger.LogInformation("SeedGrokking hoàn tất: Topics={Topics}, Lessons={Lessons}, Path={Path} nodes={Nodes}",
            await db.Topics.CountAsync(ct), await db.Lessons.CountAsync(ct), pathTitle,
            await db.LearningPathNodes.CountAsync(n => n.PathId == path.Id, ct));
    }
}
