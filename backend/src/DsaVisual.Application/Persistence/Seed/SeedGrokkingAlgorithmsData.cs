using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// Import khóa "Grokking Algorithms" (nguồn: backend/seed-data/grokking-algorithms.json).
/// - 8 topics mới (theo 8 module) + 32 lessons (16 theory tiếng Việt + 8 mini-quizz + 8 assignment codelab).
/// - 1 path lớn "Grokking Algorithms" (~34 nodes: Học bài → mini-quizz → assignment → Final-Quizz → Kiểm tra cuối).
/// - Quiz nguồn → Exercise MCQ; codelab (testCases trong sandboxConfig) → Exercise CODE;
///   theory có LabSimKeys → Exercise SIMULATION_LAB (dùng 14 engine có sẵn).
/// Idempotent: kiểm tra tồn tại trước khi chèn (title/tên path).
/// </summary>
public static partial class SeedGrokkingAlgorithmsData
{
    private const string SeedJsonRelative = "backend\\seed-data\\grokking-algorithms.json";
    private const string PathTitle = "Grokking Algorithms";

    // ── Map module order → Topic ──
    private static readonly (int Order, string Name, string Description)[] ModuleTopics =
    [
        (1000, "Nhập môn Thuật toán", "Thuật toán là gì, tính chất cơ bản và ứng dụng trong đời sống."),
        (2000, "Độ phức tạp Big-O", "Đo lường và so sánh tốc độ thuật toán bằng ký hiệu Big-O."),
        (3000, "Tìm kiếm", "Tìm kiếm tuyến tính và tìm kiếm nhị phân."),
        (4000, "Sắp xếp cơ bản", "Bubble Sort, Selection Sort và Insertion Sort."),
        (5000, "Đệ quy & Chia để trị", "Tư duy đệ quy, Merge Sort và Quick Sort."),
        (6000, "Heap & Hàng đợi ưu tiên", "Đống nhị phân, Heapify và ứng dụng Top-K."),
        (7000, "Đồ thị", "BFS, DFS và thuật toán Dijkstra."),
        (8000, "Tham lam & Quy hoạch động", "Thuật toán tham lam và giới thiệu quy hoạch động."),
    ];

    /// <summary>Lab simulation key theo bài theory (LAB exercise) — 14 engine sẵn có.</summary>
    private static readonly (string LessonTitle, string[] Keys)[] LabSimKeys =
    [
        ("Bài 5: Tìm kiếm tuyến tính (Linear Search)", ["search.linear", "structure.array"]),
        ("Bài 6: Tìm kiếm nhị phân (Binary Search)", ["search.binary"]),
        ("Bài 7: Bubble Sort & Selection Sort", ["sort.bubble", "sort.selection"]),
        ("Bài 8: Insertion Sort", ["sort.insertion"]),
        ("Bài 10: Merge Sort & Quick Sort", ["sort.merge", "sort.quick"]),
        ("Bài 11: Đống nhị phân (Heap) & Heapify", ["heap.insert", "heap.heapify"]),
        ("Bài 12: Heap Sort & ứng dụng Top-K", ["heap.extract", "sort.heap"]),
        ("Bài 13: Đồ thị — BFS & DFS", ["graph.bfs", "graph.dfs"]),
        ("Bài 14: Thuật toán Dijkstra", ["graph.dijkstra"]),
    ];

    /// <summary>Simulation key minh họa cho Assignment CODE (để gắn LessonSimulation nếu bài codelab có lesson).</summary>
    private static readonly (string LessonTitle, string Key)[] AssignmentSimKeys =
    [
        ("Assignment 3: Tra từ điển (Binary Search)", "search.binary"),
        ("Assignment 4: Xếp hạng thành tích (Insertion Sort)", "sort.insertion"),
        ("Assignment 5: Trộn danh sách (Merge Sort)", "sort.merge"),
        ("Assignment 6: Top-K sản phẩm (Heap)", "heap.insert"),
        ("Assignment 7: Đường đi trong mê cung (BFS)", "graph.bfs"),
    ];

    public static async Task SeedAsync(AppDbContext db, int adminId, DateTime now, ILogger logger, CancellationToken ct)
    {
        var jsonPath = FindSeedJson();
        if (jsonPath is null)
        {
            logger.LogWarning("SeedGrokkingAlgorithms: KHÔNG tìm thấy {Rel} — bỏ qua import khóa Algorithms", SeedJsonRelative);
            return;
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath, new UTF8Encoding(false)));
        var root = doc.RootElement;

        // ── 1. Topics mới (8 module) ──
        var topics = new Dictionary<int, Topic>();
        foreach (var (order, name, desc) in ModuleTopics)
        {
            var topic = await db.Topics.FirstOrDefaultAsync(t => t.ParentId == null && t.Name == name, ct);
            if (topic is null)
            {
                topic = new Topic { Name = name, Description = desc, SortOrder = 200 + order / 1000, CreatedBy = adminId, CreatedAt = now };
                db.Topics.Add(topic);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("SeedGrokkingAlgorithms: Topics thêm {Name} (Id={Id})", name, topic.Id);
            }
            topics[order] = topic;
        }

        // ── 2. Lessons nguồn (từ JSON) ──
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

        // ── 3. Lessons ──
        var lessonsByTitle = new Dictionary<string, Lesson>(StringComparer.Ordinal);
        var lessonContentUpdated = 0;
        foreach (var src in sources)
        {
            var topic = topics[src.ModuleOrder];
            var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.TopicId == topic.Id && l.Title == src.Title, ct);
            if (lesson is null)
            {
                lesson = new Lesson
                {
                    TopicId = topic.Id,
                    Title = src.Title,
                    Description = MakeDescription(src),
                    ContentHtml = src.ContentMd,
                    SortOrder = sources.IndexOf(src) + 1,
                    Status = LessonStatus.Active,
                    CreatedBy = adminId,
                    CreatedAt = now
                };
                db.Lessons.Add(lesson);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("SeedGrokkingAlgorithms: Lessons thêm {Title} (Id={Id})", src.Title, lesson.Id);
            }
            else if (lesson.ContentHtml != src.ContentMd)
            {
                lesson.ContentHtml = src.ContentMd;
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
            logger.LogInformation("SeedGrokkingAlgorithms: Tự chữa {Count} bài học (cập nhật nội dung theo JSON gốc)", lessonContentUpdated);
        }

        await SeedLessonSimulationsAsync(db, lessonsByTitle, logger, ct);

        // ── 4. Path "Grokking Algorithms" ──
        var path = await db.LearningPaths.FirstOrDefaultAsync(p => p.Title == PathTitle, ct);
        if (path is null)
        {
            path = new LearningPath
            {
                Title = PathTitle,
                Description = "Khóa học Thuật toán từ con số 0 đến trình độ trung cấp: khái niệm cơ bản, Big-O, tìm kiếm, sắp xếp, đệ quy & chia để trị, heap, đồ thị, tham lam và giới thiệu quy hoạch động — lý thuyết trực quan bằng hình ảnh động, mini-quizz, assignment lập trình và kiểm tra cuối.",
                TopicId = topics[1000].Id,
                SortOrder = 2,
                IsActive = true,
                CreatedBy = adminId
            };
            db.LearningPaths.Add(path);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokkingAlgorithms: LearningPaths thêm {Title} (Id={Id})", PathTitle, path.Id);
        }

        // Author + marketing cho khóa
        var teacher = await db.Users
            .FirstOrDefaultAsync(u => u.Email == "teacher@demo.local" && u.DeletedAt == null, ct);
        var highlightsJson = JsonSerializer.Serialize(new[]
        {
            new { title = "Đi từ con số 0", description = "Không cần kiến thức nền — mở đầu bằng khái niệm thuật toán là gì, rồi từng bước lên Big-O, tìm kiếm, sắp xếp cho tới đồ thị và quy hoạch động." },
            new { title = "Minh họa động đa dạng", description = "Biểu đồ Big-O animate, thanh sắp xếp đổi màu từng bước, cây heap, đồ thị BFS/DFS, bảng Dijkstra — mỗi khái niệm một cách trực quan riêng." },
            new { title = "Đệ quy & chia để trị", description = "Module riêng dành cho tư duy đệ quy — rào cản lớn nhất của người mới — trước khi chạm tới Merge/Quick Sort." },
            new { title = "Từ trung cấp trở lên", description = "Kết thúc bằng Tham lam và Quy hoạch động — nền tảng cho các chủ đề nâng cao: backtracking, cây nâng cao, đồ thị nâng cao, xử lý chuỗi." }
        });
        var testimonialsJson = JsonSerializer.Serialize(new[]
        {
            new { name = "Lan Anh", role = "Fresher Frontend", quote = "Mình chưa từng học thuật toán bao giờ. Khóa này đi từ khái niệm 'thuật toán là gì' nên mình không bị ngợp, hết khóa tự tin đi phỏng vấn." },
            new { name = "Minh Khoa", role = "Sinh viên năm 2", quote = "Phần Big-O có đồ thị animate nên mình hiểu ngay tại sao O(n²) chậm. Đệ quy trước đây mình sợ lắm, giờ thấy dễ!" },
            new { name = "Thu Trang", role = "Junior Backend", quote = "Assignment chấm tự động rất đã — nhất là bài Top-K heap và mê cung BFS. Làm xong mình hiểu sâu hơn hẳn đọc lý thuyết suông." }
        });

        var teacherId = teacher?.Id;
        var pathChanged = false;
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
            logger.LogInformation("SeedGrokkingAlgorithms: Cập nhật tác giả + marketing cho {Title}", PathTitle);
        }

        // ── 5. Nodes: Học bài → Final-Quizz → Kiểm tra cuối ──
        var sortOrder = 1;
        var nodesByLessonTitle = new Dictionary<string, LearningPathNode>(StringComparer.Ordinal);
        foreach (var src in sources.OrderBy(s => s.ModuleOrder).ThenBy(s => s.OrderIndex))
        {
            var nodeTitle = $"Học: {src.Title}";
            var exists = await db.LearningPathNodes.AnyAsync(n => n.PathId == path.Id && n.Title == nodeTitle, ct);
            LearningPathNode? node = null;
            if (!exists)
            {
                node = new LearningPathNode
                {
                    PathId = path.Id,
                    Title = nodeTitle,
                    LessonId = lessonsByTitle.TryGetValue(src.Title, out var l) ? l.Id : null,
                    SortOrder = sortOrder
                };
                db.LearningPathNodes.Add(node);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("SeedGrokkingAlgorithms: Nodes thêm {Node} (path={Path})", nodeTitle, PathTitle);
            }
            else
            {
                node = await db.LearningPathNodes.FirstAsync(n => n.PathId == path.Id && n.Title == nodeTitle, ct);
            }
            nodesByLessonTitle[src.Title] = node;
            sortOrder++;
        }

        const string practiceNodeTitle = "Final-Quizz";
        var practiceNode = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.PathId == path.Id && n.Title == practiceNodeTitle, ct);
        if (practiceNode is null)
        {
            practiceNode = new LearningPathNode { PathId = path.Id, Title = practiceNodeTitle, LessonId = null, SortOrder = sortOrder };
            db.LearningPathNodes.Add(practiceNode);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SeedGrokkingAlgorithms: Nodes thêm {Node} (path={Path})", practiceNodeTitle, PathTitle);
        }
        sortOrder++;

        const string finalNodeTitle = "Kiểm tra cuối lộ trình";
        var finalNode = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.PathId == path.Id && n.Title == finalNodeTitle, ct);
        if (finalNode is null)
        {
            finalNode = new LearningPathNode { PathId = path.Id, Title = finalNodeTitle, LessonId = null, SortOrder = sortOrder };
            db.LearningPathNodes.Add(finalNode);
            await db.SaveChangesAsync(ct);
        }

        // ── 6. Exercises: Quiz + Lab + Assignment ──
        var quizByTitle = LoadQuizzes(root);
        await SeedQuizExercisesAsync(db, lessonsByTitle, nodesByLessonTitle, quizByTitle, adminId, now, logger, ct);
        await SeedLabExercisesAsync(db, lessonsByTitle, nodesByLessonTitle, adminId, now, logger, ct);
        await SeedAssignmentExercisesAsync(db, lessonsByTitle, nodesByLessonTitle, adminId, now, logger, ct);

        // ── 7. Final-Quizz (20 câu tổng hợp — theo manifest: 2-3 câu/module) ──
        await SeedFinalQuizzAsync(db, path, practiceNode, lessonsByTitle, quizByTitle, adminId, now, logger, ct);

        // ── 8. Kiểm tra cuối (3 bài code phong cách Assignment) ──
        await SeedFinalCodingTestAsync(db, path, finalNode, lessonsByTitle, adminId, now, logger, ct);

        logger.LogInformation("SeedGrokkingAlgorithms hoàn tất: Topics={Topics}, Lessons={Lessons}, Path={Path} nodes={Nodes}",
            await db.Topics.CountAsync(ct), await db.Lessons.CountAsync(ct), PathTitle,
            await db.LearningPathNodes.CountAsync(n => n.PathId == path.Id, ct));
    }
}