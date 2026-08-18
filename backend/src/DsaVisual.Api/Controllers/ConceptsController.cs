using System.Text.Json;
using Asp.Versioning;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Adapter "Concepts" — trả ĐÚNG format API của VisualizationDSA-main (/api/v1/concepts/*)
/// để FE CoursesListView/CourseDetailView/LessonStudyView (bê nguyên từ project kia)
/// chạy trên backend DsaVisual. Map: LearningPath → Course, Node → Lesson (trong path),
/// Exercise MCQ → Quiz, Exercise CODE → Codelab. KHÔNG đổi schema.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v1/concepts")]
[Authorize]
public class ConceptsController(AppDbContext db) : ApiControllerBase
{
    // true = bỏ hết khóa node; false = bật lại khóa tuần tự bình thường.
    private static readonly bool DisableNodeLocks = false;

    private readonly AppDbContext _db = db;

    // ── Courses (list + detail) ────────────────────────────────

    public sealed class ConceptsCourseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "DSA";
        public string Difficulty { get; set; } = "Intermediate";
        public bool IsPremium { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int ProgressPercent { get; set; }
        public int XpReward { get; set; }
        public List<string> LearningObjectives { get; set; } = [];
        public List<string> KeyOutcomes { get; set; } = [];
        public double Rating { get; set; }
        public int RatingCount { get; set; }
        public List<CourseHighlightDto> Highlights { get; set; } = [];
        public List<CourseTestimonialDto> Testimonials { get; set; } = [];
        public CourseAuthorDto? Author { get; set; }
        public List<ConceptsLessonDto> Lessons { get; set; } = [];
    }

    public sealed class CourseHighlightDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public sealed class CourseTestimonialDto
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Quote { get; set; } = string.Empty;
    }

    public sealed class CourseAuthorDto
    {
        public string Name { get; set; } = string.Empty;
        public string? AcademicDegree { get; set; }
        public string? Bio { get; set; }
        public string? ProfileLink { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public sealed class ConceptsLessonDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ContentMd { get; set; } = string.Empty;
        public string SandboxType { get; set; } = "dsa";
        public string SandboxConfig { get; set; } = string.Empty;
        public string? QuizId { get; set; }
        public int XpReward { get; set; }
        public int OrderIndex { get; set; }
        public string Status { get; set; } = "NotStarted";
        public string ModuleTitle { get; set; } = string.Empty;
        public bool Locked { get; set; }
    }

    [HttpGet("courses")]
    public async Task<ActionResult<List<ConceptsCourseDto>>> GetCourses(CancellationToken ct)
    {
        var userId = CurrentUserId();
        var paths = await _db.LearningPaths.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        if (paths.Count == 0)
        {
            return Ok(new List<ConceptsCourseDto>());
        }

        var pathIds = paths.Select(p => p.Id).ToList();
        var allNodes = await _db.LearningPathNodes.AsNoTracking()
            .Where(n => pathIds.Contains(n.PathId))
            .OrderBy(n => n.SortOrder)
            .ToListAsync(ct);

        var nodesByPath = allNodes.GroupBy(n => n.PathId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var allNodeIds = allNodes.Select(n => n.Id).ToList();
        var passedNodeIds = allNodeIds.Count == 0
            ? new HashSet<int>()
            : (await _db.UserNodeProgress.AsNoTracking()
                .Where(p => p.UserId == userId && allNodeIds.Contains(p.NodeId) && p.Status == 2)
                .Select(p => p.NodeId)
                .ToListAsync(ct))
                .ToHashSet();

        var authorIds = paths.Where(p => p.AuthorId != null).Select(p => p.AuthorId!.Value).Distinct().ToList();
        var authorsMap = authorIds.Count > 0
            ? await _db.Users.AsNoTracking()
                .Where(u => authorIds.Contains(u.Id) && u.DeletedAt == null)
                .ToDictionaryAsync(u => u.Id, u => new CourseAuthorDto
                {
                    Name = u.DisplayName,
                    AcademicDegree = u.AcademicDegree,
                    Bio = u.TeacherBio,
                    ProfileLink = u.ProfileLink,
                    AvatarUrl = u.AvatarUrl
                }, ct)
            : new Dictionary<int, CourseAuthorDto>();

        var result = new List<ConceptsCourseDto>();
        foreach (var path in paths)
        {
            var nodes = nodesByPath.GetValueOrDefault(path.Id, []);
            var completed = nodes.Count(n => passedNodeIds.Contains(n.Id));

            var xp = nodes.Sum(n => LessonXpByTitle.GetValueOrDefault(n.LessonId ?? 0, 100));
            var (rating, ratingCount) = await CourseRatingAsync(nodes, ct);
            var highlights = ParseHighlights(path.HighlightsJson);
            var testimonials = ParseTestimonials(path.TestimonialsJson);
            var author = path.AuthorId is { } aId ? authorsMap.GetValueOrDefault(aId) : null;

            result.Add(new ConceptsCourseDto
            {
                Id = path.Id.ToString(),
                Title = path.Title,
                Description = path.Description ?? string.Empty,
                Category = "DSA",
                Difficulty = "Intermediate",
                IsPremium = false,
                IsPublished = true,
                CreatedAt = path.CreatedBy > 0 ? DateTime.UtcNow.AddDays(-30) : DateTime.UtcNow,
                TotalLessons = nodes.Count,
                CompletedLessons = completed,
                ProgressPercent = nodes.Count == 0 ? 0 : (int)Math.Round(completed * 100.0 / nodes.Count),
                XpReward = xp,
                LearningObjectives = CourseLearningObjectives(path.Title),
                KeyOutcomes = CourseKeyOutcomes(path.Title),
                Rating = rating,
                RatingCount = ratingCount,
                Highlights = highlights,
                Testimonials = testimonials,
                Author = author,
                Lessons = await BuildLessonsAsync(userId, nodes, ct)
            });
        }

        return Ok(result);
    }

    [HttpGet("courses/{id:int}")]
    public async Task<ActionResult<ConceptsCourseDto>> GetCourse(int id, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var path = await _db.LearningPaths.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);
        if (path is null)
        {
            return NotFound(new { message = "Khóa học không tồn tại." });
        }

        var nodes = await _db.LearningPathNodes.AsNoTracking()
            .Where(n => n.PathId == path.Id)
            .OrderBy(n => n.SortOrder)
            .ToListAsync(ct);

        var nodeIds = nodes.Select(n => n.Id).ToList();
        var passedNodeIds = nodeIds.Count == 0
            ? new HashSet<int>()
            : (await _db.UserNodeProgress.AsNoTracking()
                .Where(p => p.UserId == userId && nodeIds.Contains(p.NodeId) && p.Status == 2)
                .Select(p => p.NodeId)
                .ToListAsync(ct))
                .ToHashSet();

        var completed = nodes.Count(n => passedNodeIds.Contains(n.Id));
        var lessons = await BuildLessonsAsync(userId, nodes, ct);
        var (rating, ratingCount) = await CourseRatingAsync(nodes, ct);
        return Ok(new ConceptsCourseDto
        {
            Id = path.Id.ToString(),
            Title = path.Title,
            Description = path.Description ?? string.Empty,
            Category = "DSA",
            Difficulty = "Intermediate",
            IsPremium = false,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            TotalLessons = nodes.Count,
            CompletedLessons = completed,
            ProgressPercent = nodes.Count == 0 ? 0 : (int)Math.Round(completed * 100.0 / nodes.Count),
            XpReward = lessons.Sum(l => l.XpReward),
            LearningObjectives = CourseLearningObjectives(path.Title),
            KeyOutcomes = CourseKeyOutcomes(path.Title),
            Rating = rating,
            RatingCount = ratingCount,
            Highlights = ParseHighlights(path.HighlightsJson),
            Testimonials = ParseTestimonials(path.TestimonialsJson),
            Author = await CourseAuthorAsync(path.AuthorId, ct),
            Lessons = lessons
        });
    }

    /// <summary>Rating trung bình của khóa = AVG/COUNT ContentFeedback của các bài học trong path (dữ liệu thật).</summary>
    private async Task<(double Rating, int Count)> CourseRatingAsync(List<LearningPathNode> nodes, CancellationToken ct)
    {
        var lessonIds = nodes.Where(n => n.LessonId is not null).Select(n => n.LessonId!.Value).ToList();
        if (lessonIds.Count == 0)
        {
            return (0, 0);
        }

        var aggregate = await _db.ContentFeedback.AsNoTracking()
            .Where(f => lessonIds.Contains(f.LessonId))
            .GroupBy(f => 1)
            .Select(g => new { Avg = g.Average(f => f.Rating), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        return aggregate is null ? (0, 0) : (Math.Round(aggregate.Avg, 1), aggregate.Count);
    }

    /// <summary>Highlights ("Why choose") — JSON tùy biến theo khóa, seed từ backend (không hardcode FE).</summary>
    private static List<CourseHighlightDto> ParseHighlights(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<CourseHighlightDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>Testimonials — JSON tùy biến theo khóa, seed từ backend (không hardcode FE).</summary>
    private static List<CourseTestimonialDto> ParseTestimonials(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<CourseTestimonialDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>Tác giả khóa từ AuthorId (User role Teacher) — không có thì trả null (FE tự ẩn).</summary>
    private async Task<CourseAuthorDto?> CourseAuthorAsync(int? authorId, CancellationToken ct)
    {
        if (authorId is not { } id)
        {
            return null;
        }

        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, ct);
        if (user is null)
        {
            return null;
        }

        return new CourseAuthorDto
        {
            Name = user.DisplayName,
            AcademicDegree = user.AcademicDegree,
            Bio = user.TeacherBio,
            ProfileLink = user.ProfileLink,
            AvatarUrl = user.AvatarUrl
        };
    }

    /// <summary>Mục tiêu học tập của khóa (Grokking) — hiển thị ở trang chi tiết khóa học.</summary>
    private static List<string> CourseLearningObjectives(string courseTitle)
    {
        if (courseTitle.Contains("Grokking", StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                "Nắm vững các cấu trúc dữ liệu tuyến tính cốt lõi: Array và Linked List",
                "Hiểu cách quản lý luồng dữ liệu với Stack (LIFO) và Queue/Deque (FIFO)",
                "Làm chủ bảng băm (Hash Map / Hash Set) để tra cứu O(1)",
                "Xây dựng nền tảng cây phân cấp: Tree tổng quát và Binary Search Tree",
                "Biết chọn đúng cấu trúc dữ liệu cho từng bài toán thực tế",
            ];
        }

        return
        [
            "Nắm vững kiến thức nền tảng về cấu trúc dữ liệu và giải thuật",
            "Thực hành qua bài học trực quan và bài tập trắc nghiệm",
        ];
    }

    /// <summary>Kết quả đạt được sau khóa học — hiển thị ở trang chi tiết khóa học.</summary>
    private static List<string> CourseKeyOutcomes(string courseTitle)
    {
        if (courseTitle.Contains("Grokking", StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                "Cài đặt thành thạo các thao tác trên Array, Linked List, Stack, Queue, Hash Map, Tree và BST",
                "Giải được các bài toán phỏng vấn kinh điển: đảo ngược list, kiểm tra ngoặc, tìm kiếm BST, xử lý va chạm băm",
                "Đạt ≥ 70% các bài trắc nghiệm và hoàn thành 4 Assignment lập trình thực tế",
                "Phân tích độ phức tạp thời gian/không gian của từng cấu trúc dữ liệu",
                "Tự tin chuẩn bị cho phỏng vấn DSA cấp độ trung cấp",
            ];
        }

        return
        [
            "Hoàn thành các bài học và bài tập trong khóa",
            "Tích lũy XP và xây dựng lộ trình học cá nhân",
        ];
    }

    /// <summary>
    /// Khoá node tuần tự (nghiệp vụ lộ trình): node MỞ khi node NGAY TRƯỚC nó (theo SortOrder cùng
    /// path) đã hoàn thành (UserNodeProgress Status=2) — hoặc node đó là node đầu tiên của path.
    /// Node đã tự pass thì luôn mở (được xem lại). Nghiệp vụ: học từ trên xuống —
    /// hết bài 1 mới mở bài 2 → xong bài 2 mới mở quiz → xong quiz mới mở assignment → sang module sau.
    /// </summary>
    private async Task<bool> IsNodeLockedAsync(int userId, LearningPathNode node, CancellationToken ct)
    {
        if (DisableNodeLocks)
        {
            return false;
        }

        var passed = await _db.UserNodeProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.NodeId == node.Id && p.Status == 2, ct);
        if (passed)
        {
            return false;
        }

        var prev = await _db.LearningPathNodes.AsNoTracking()
            .Where(n => n.PathId == node.PathId && n.SortOrder < node.SortOrder)
            .OrderByDescending(n => n.SortOrder)
            .FirstOrDefaultAsync(ct);
        if (prev is null)
        {
            return false;
        }

        var prevPassed = await _db.UserNodeProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.NodeId == prev.Id && p.Status == 2, ct);
        return !prevPassed;
    }

    private async Task<List<ConceptsLessonDto>> BuildLessonsAsync(
        int userId, List<LearningPathNode> nodes, CancellationToken ct)
    {
        if (nodes.Count == 0)
        {
            return [];
        }

        var lessonIds = nodes.Where(n => n.LessonId != null).Select(n => n.LessonId!.Value).Distinct().ToList();
        var lessonsMap = lessonIds.Count > 0
            ? await _db.Lessons.AsNoTracking()
                .Where(l => lessonIds.Contains(l.Id) && l.DeletedAt == null)
                .ToDictionaryAsync(l => l.Id, ct)
            : new Dictionary<int, Lesson>();

        var topicIds = lessonsMap.Values.Select(l => l.TopicId).Distinct().ToList();
        var topicsMap = topicIds.Count > 0
            ? await _db.Topics.AsNoTracking()
                .Where(t => topicIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, ct)
            : new Dictionary<int, Topic>();

        var nodeIds = nodes.Select(n => n.Id).ToList();
        var exercisesList = await _db.Exercises.AsNoTracking()
            .Where(e => e.NodeId != null && nodeIds.Contains(e.NodeId.Value) && e.DeletedAt == null)
            .ToListAsync(ct);
        var exercisesByNode = exercisesList
            .GroupBy(e => e.NodeId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var passedNodeIds = (await _db.UserNodeProgress.AsNoTracking()
            .Where(p => p.UserId == userId && nodeIds.Contains(p.NodeId) && p.Status == 2)
            .Select(p => p.NodeId)
            .ToListAsync(ct))
            .ToHashSet();

        var result = new List<ConceptsLessonDto>();
        var prevPassed = true; // node đầu tiên luôn mở
        foreach (var node in nodes)
        {
            Lesson? lesson = node.LessonId is { } lessonId ? lessonsMap.GetValueOrDefault(lessonId) : null;
            var title = lesson?.Title ?? node.Title;
            var content = lesson?.ContentHtml ?? string.Empty;

            // Module title theo Topic của lesson (Module 1-4 của Grokking);
            // node KHÔNG có bài học (luyện tập tổng hợp / kiểm tra cuối) → nhóm module 5 "Kiểm tra cuối lộ trình"
            string moduleTitle = "Kiểm tra cuối lộ trình";
            if (lesson?.TopicId is { } topicId && topicsMap.TryGetValue(topicId, out var topic))
            {
                moduleTitle = topic.Name;
            }

            // Xác định loại node: Quiz exercise (MCQ) / Code exercise / Lab
            var exercises = exercisesByNode.GetValueOrDefault(node.Id, []);
            var quizEx = exercises.FirstOrDefault(e => e.Type == ExerciseType.Mcq);
            var codeEx = exercises.FirstOrDefault(e => e.Type == ExerciseType.Code);
            var labEx = exercises.FirstOrDefault(e => e.Type == ExerciseType.SimulationLab);

            string sandboxType;
            string sandboxConfig = string.Empty;
            if (codeEx is not null)
            {
                sandboxType = "codelab";
                sandboxConfig = codeEx.ConfigJson ?? string.Empty;
            }
            else if (quizEx is not null)
            {
                // Theory (content dài) → 'dsa' (hiện Lý thuyết rồi Quiz); mini-quizz (content ngắn) → 'quiz'
                sandboxType = content.Length >= 300 ? "dsa" : "quiz";
                sandboxConfig = $"{{ \"quizId\": {quizEx.Id} }}";
            }
            else if (labEx is not null)
            {
                sandboxType = "dsa";
                sandboxConfig = $"{{ \"simulationKey\": \"{(labEx.ConfigJson is not null ? TryReadSimulationKey(labEx.ConfigJson) : string.Empty)}\" }}";
            }
            else
            {
                sandboxType = "dsa";
            }

            var passed = passedNodeIds.Contains(node.Id);

            result.Add(new ConceptsLessonDto
            {
                Id = node.Id.ToString(),
                Title = title,
                ContentMd = content,
                SandboxType = sandboxType,
                SandboxConfig = sandboxConfig,
                QuizId = quizEx?.Id.ToString(),
                XpReward = LessonXpByTitle.GetValueOrDefault(node.LessonId ?? 0, 100),
                OrderIndex = node.SortOrder,
                Status = passed ? "Completed" : "NotStarted",
                ModuleTitle = moduleTitle,
                Locked = DisableNodeLocks ? false : !prevPassed && !passed
            });
            prevPassed = passed;
        }

        return result;
    }

    private static string TryReadSimulationKey(string configJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(configJson);
            return doc.RootElement.TryGetProperty("simulationKey", out var k) ? k.GetString() ?? string.Empty : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static readonly Dictionary<int, int> LessonXpByTitle = new()
    {
        // SeedGrokking: theory 100-150, mini-quizz 50-80, assignment 200
    };

    // ── Lesson detail ──────────────────────────────────────────

    public sealed class LessonDetailResponse
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ContentMd { get; set; } = string.Empty;
        public string SandboxType { get; set; } = "dsa";
        public string SandboxConfig { get; set; } = string.Empty;
        public string? QuizId { get; set; }
        public string? ExerciseId { get; set; }
        public int XpReward { get; set; }
        public int OrderIndex { get; set; }
        public string Status { get; set; } = "NotStarted";
        public int LastActiveFrameIndex { get; set; }
        public int LastScrollPercent { get; set; }
    }

    [HttpGet("lessons/{id:int}")]
    public async Task<ActionResult<LessonDetailResponse>> GetLesson(int id, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var node = await _db.LearningPathNodes.AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id, ct);
        if (node is null)
        {
            return NotFound(new { message = "Bài học không tồn tại." });
        }

        // Khoá tuần tự: node chưa mở → 403 (học xong bài trước mới vào được bài sau)
        if (await IsNodeLockedAsync(userId, node, ct))
        {
            return StatusCode(403, new { message = "Bài học chưa được mở khóa — hãy hoàn thành bài học trước." });
        }

        var path = await _db.LearningPaths.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == node.PathId, ct);

        Lesson? lesson = null;
        if (node.LessonId is { } lessonId)
        {
            lesson = await _db.Lessons.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
        }

        var exercises = await _db.Exercises.AsNoTracking()
            .Where(e => e.NodeId == node.Id && e.DeletedAt == null)
            .ToListAsync(ct);

        var quizEx = exercises.FirstOrDefault(e => e.Type == ExerciseType.Mcq);
        var codeEx = exercises.FirstOrDefault(e => e.Type == ExerciseType.Code);
        var labEx = exercises.FirstOrDefault(e => e.Type == ExerciseType.SimulationLab);

        string sandboxType = "dsa";
        string sandboxConfig = string.Empty;
        if (codeEx is not null)
        {
            sandboxType = "codelab";
            sandboxConfig = codeEx.ConfigJson ?? string.Empty;
        }
        else if (quizEx is not null)
        {
            sandboxType = (lesson?.ContentHtml?.Length ?? 0) >= 300 ? "dsa" : "quiz";
            sandboxConfig = $"{{ \"quizId\": {quizEx.Id} }}";
        }
        else if (labEx is not null)
        {
            sandboxType = "dsa";
            sandboxConfig = $"{{ \"simulationKey\": \"{TryReadSimulationKey(labEx.ConfigJson ?? string.Empty)}\" }}";
        }

        var passed = await _db.UserNodeProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.NodeId == node.Id && p.Status == 2, ct);

        return Ok(new LessonDetailResponse
        {
            Id = node.Id.ToString(),
            CourseId = path?.Id.ToString() ?? string.Empty,
            CourseTitle = path?.Title ?? string.Empty,
            Title = lesson?.Title ?? node.Title,
            ContentMd = lesson?.ContentHtml ?? string.Empty,
            SandboxType = sandboxType,
            SandboxConfig = sandboxConfig,
            QuizId = quizEx?.Id.ToString(),
            ExerciseId = codeEx?.Id.ToString(),
            XpReward = LessonXpByTitle.GetValueOrDefault(node.LessonId ?? 0, 100),
            OrderIndex = node.SortOrder,
            Status = passed ? "Completed" : "NotStarted"
        });
    }

    // ── Quiz (stateless — đúng format statelessQuizApi bên kia) ──

    public sealed class ConceptsQuizDetail
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Topic { get; set; } = "dsa";
        public string Difficulty { get; set; } = "Medium";
        public int XpReward { get; set; }
        public List<ConceptsQuizQuestion> Questions { get; set; } = [];
    }

    public sealed class ConceptsQuizQuestion
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<string> Options { get; set; } = [];
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }

    [HttpGet("quiz/{id:int}")]
    public async Task<ActionResult<ConceptsQuizDetail>> GetQuiz(int id, CancellationToken ct)
    {
        var exercise = await _db.Exercises.AsNoTracking()
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);
        if (exercise is null)
        {
            return NotFound(new { message = "Bài trắc nghiệm không tồn tại." });
        }

        var detail = new ConceptsQuizDetail
        {
            Id = exercise.Id.ToString(),
            Title = exercise.Title,
            Topic = "dsa",
            Difficulty = "Medium",
            XpReward = exercise.MaxScore,
            Questions = exercise.Questions
                .OrderBy(q => q.SortOrder)
                .Select(q => new ConceptsQuizQuestion
                {
                    Id = q.Id.ToString(),
                    Text = q.Content,
                    Options = DeserializeOptions(q.OptionsJson),
                    CorrectIndex = DeserializeCorrectIndex(q.AnswerJson),
                    Explanation = q.Explanation ?? string.Empty
                })
                .ToList()
        };
        return Ok(detail);
    }

    private static List<string> DeserializeOptions(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static int DeserializeCorrectIndex(string answerJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(answerJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
            {
                return doc.RootElement[0].GetInt32();
            }
        }
        catch
        {
            // fallthrough
        }

        return 0;
    }

    public sealed class QuizSubmitRequest
    {
        public string QuizId { get; set; } = string.Empty;
        public List<int> Answers { get; set; } = [];
    }

    public sealed class QuizAttemptResult
    {
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public bool Passed { get; set; }
        public int XpAwarded { get; set; }
        public List<QuizQuestionResult> QuestionResults { get; set; } = [];
    }

    public sealed class QuizQuestionResult
    {
        public string QuestionId { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }

    [HttpPost("quiz/submit")]
    public async Task<ActionResult<QuizAttemptResult>> SubmitQuiz([FromBody] QuizSubmitRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId();
        if (!int.TryParse(request.QuizId, out var quizId))
        {
            return BadRequest(new { message = "QuizId không hợp lệ." });
        }

        var exercise = await _db.Exercises.AsNoTracking()
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == quizId && e.DeletedAt == null && e.Type == ExerciseType.Mcq, ct);
        if (exercise is null)
        {
            return NotFound(new { message = "Bài trắc nghiệm không tồn tại." });
        }

        // Khoá tuần tự: quiz của node chưa mở → 403 (chặn nộp bài thẳng bằng API)
        if (exercise.NodeId is { } quizNodeId)
        {
            var quizNode = await _db.LearningPathNodes.AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == quizNodeId, ct);
            if (quizNode is not null && await IsNodeLockedAsync(userId, quizNode, ct))
            {
                return StatusCode(403, new { message = "Bài học chưa được mở khóa — hãy hoàn thành bài học trước." });
            }
        }

        var questions = exercise.Questions.OrderBy(q => q.SortOrder).ToList();
        var score = 0;
        var results = new List<QuizQuestionResult>();
        for (var i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            var correct = DeserializeCorrectIndex(q.AnswerJson);
            var selected = request.Answers.Count > i ? request.Answers[i] : -1;
            var isCorrect = selected == correct;
            if (isCorrect)
            {
                score++;
            }

            results.Add(new QuizQuestionResult
            {
                QuestionId = q.Id.ToString(),
                IsCorrect = isCorrect,
                CorrectIndex = correct,
                Explanation = q.Explanation ?? string.Empty
            });
        }

        var maxScore = questions.Count;
        var passed = maxScore > 0 && score * 1.0 / maxScore >= 0.7;

        // Ghi ExerciseSubmission (Score theo % — khớp chấm điểm của DsaVisual)
        var now = DateTime.UtcNow;
        _db.ExerciseSubmissions.Add(new ExerciseSubmission
        {
            UserId = userId,
            ExerciseId = exercise.Id,
            Score = score,
            AnswersJson = JsonSerializer.Serialize(request.Answers),
            ResultJson = JsonSerializer.Serialize(results),
            DurationSeconds = null,
            SubmittedAt = now
        });

        // Ghi UserNodeProgress pass (nếu quiz thuộc node)
        if (exercise.NodeId is { } nodeId && passed)
        {
            var existing = await _db.UserNodeProgress.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId && p.NodeId == nodeId, ct);
            if (existing is null)
            {
                _db.UserNodeProgress.Add(new UserNodeProgress
                {
                    UserId = userId,
                    NodeId = nodeId,
                    Status = 2,
                    Stars = Math.Clamp((int)Math.Ceiling(score * 3.0 / maxScore), 0, 3),
                    NodeScore = score,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else if (existing.Status != 2)
            {
                existing.Status = 2;
                existing.Stars = Math.Clamp((int)Math.Ceiling(score * 3.0 / maxScore), 0, 3);
                existing.NodeScore = score;
                existing.UpdatedAt = DateTime.UtcNow;
                _db.UserNodeProgress.Update(existing);
            }
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new QuizAttemptResult
        {
            Score = score,
            MaxScore = maxScore,
            Passed = passed,
            XpAwarded = passed ? exercise.MaxScore : 0,
            QuestionResults = results
        });
    }

    // ── Progress + XP ──────────────────────────────────────────

    public sealed class LessonProgressPayload
    {
        public bool HasWatchedVisualizer { get; set; }
        public int? QuizScore { get; set; }
        public int BestScore { get; set; }
        public bool CodelabCompleted { get; set; }
        public int XpAwarded { get; set; }
        /// <summary>Cờ FE gửi khi học viên bấm "Hoàn thành bài học" (bài lý thuyết không có quiz/codelab) → node pass.</summary>
        public bool? Completed { get; set; }
    }

    [HttpGet("auth/progress/{id:int}")]
    public async Task<ActionResult<object>> GetProgress(int id, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var node = await _db.LearningPathNodes.AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id, ct);
        if (node is null)
        {
            return NotFound(new { message = "Bài học không tồn tại." });
        }

        var nodeProgress = await _db.UserNodeProgress.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.NodeId == id, ct);

        return Ok(new
        {
            success = true,
            progress = new
            {
                hasWatchedVisualizer = false,
                quizScore = nodeProgress?.NodeScore,
                codelabCompleted = nodeProgress?.Status == 2,
                xpAwarded = nodeProgress?.NodeScore ?? 0,
                totalXp = 100
            }
        });
    }

    [HttpPost("auth/progress/{id:int}")]
    public async Task<ActionResult<object>> SaveProgress(int id, [FromBody] LessonProgressPayload payload, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var node = await _db.LearningPathNodes.AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id, ct);
        if (node is null)
        {
            return NotFound(new { message = "Bài học không tồn tại." });
        }

        // Khoá tuần tự: không cho ghi tiến độ node CHƯA MỞ (chống bypass — học viên chỉ sync
        // bài đang học, bài đó luôn mở khoá; node đã pass vẫn ghi được bình thường).
        if (await IsNodeLockedAsync(userId, node, ct))
        {
            return StatusCode(403, new { message = "Bài học chưa được mở khóa — hãy hoàn thành bài học trước." });
        }

        // Ghi UserProgress nếu node có lesson
        if (node.LessonId is { } lessonId)
        {
            var progress = await _db.UserProgress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);
            var now = DateTime.UtcNow;
            if (progress is null)
            {
                _db.UserProgress.Add(new UserProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    Viewed = true,
                    BestScore = payload.BestScore > 0 ? payload.BestScore : null,
                    CompletedAt = payload.CodelabCompleted ? now : null,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                progress.Viewed = true;
                if (payload.BestScore > 0)
                {
                    progress.BestScore = payload.BestScore;
                }

                if (payload.CodelabCompleted && progress.CompletedAt is null)
                {
                    progress.CompletedAt = now;
                }

                progress.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Node pass khi "Hoàn thành bài học" (cờ Completed — bài lý thuyết / quiz node KHÔNG có bài
        // code): node pass để mở khoá bài sau. NGƯỢC LẠI node có bài CODE (ASM / kiểm tra cuối) KHÔNG
        // được pass qua đây — bắt buộc nộp qua /exercises/{id}/code-submit để MÁY CHỦ chấm code thật
        // (Jint) mới pass (nghiệp vụ 15/08: chạy đúng mới pass, chạy sai không pass).
        if (payload.Completed == true)
        {
            var hasCodeExercise = await _db.Exercises.AsNoTracking()
                .AnyAsync(e => e.NodeId == id && e.Type == ExerciseType.Code && e.DeletedAt == null, ct);
            if (!hasCodeExercise)
            {
                var nodeProgress = await _db.UserNodeProgress
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NodeId == id, ct);
                if (nodeProgress is null)
                {
                    _db.UserNodeProgress.Add(new UserNodeProgress
                    {
                        UserId = userId,
                        NodeId = id,
                        Status = 2,
                        Stars = 3,
                        NodeScore = 100,
                        PassedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else if (nodeProgress.Status != 2)
                {
                    nodeProgress.Status = 2;
                    nodeProgress.Stars = 3;
                    nodeProgress.NodeScore = 100;
                    nodeProgress.PassedAt = DateTime.UtcNow;
                    nodeProgress.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { success = true });
    }

    public sealed class AwardXpRequest
    {
        public int Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    [HttpPost("auth/award-xp")]
    public async Task<ActionResult<object>> AwardXp([FromBody] AwardXpRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId();
        if (request.Amount <= 0 || request.Amount > 1000)
        {
            return BadRequest(new { message = "Số XP không hợp lệ." });
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return NotFound(new { message = "Tài khoản không tồn tại." });
        }

        user.Xp += request.Amount;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { success = true, xp = user.Xp, amount = request.Amount });
    }
}
