using System.Text.Json;
using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
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
public class ConceptsController(
    AppDbContext db,
    IGamificationConfigService configService,
    Microsoft.Extensions.Caching.Memory.IMemoryCache cache) : ApiControllerBase
{
    // true = bỏ hết khóa node; false = bật lại khóa tuần tự bình thường.
    private static readonly bool DisableNodeLocks = false;

    private readonly AppDbContext _db = db;
    private readonly IGamificationConfigService _configService = configService;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache = cache;

    // ── Courses (list + detail) ────────────────────────────────

    public sealed class ConceptsCourseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "DSA";
        public string Difficulty { get; set; } = "Intermediate";
        public int? TopicId { get; set; }                      // C1: phân nhóm lộ trình theo chủ đề
        public string? TopicName { get; set; }
        public bool IsPremium { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public string Status { get; set; } = "draft";
        public string? RejectionReason { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? AuthorName { get; set; }
        public int CreatedBy { get; set; }
        public int? AuthorId { get; set; }
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

    public sealed class CourseReviewRequest
    {
        public bool Approve { get; set; }
        public string? Reason { get; set; }
    }

    public sealed class CourseContentMetadata
    {
        public string? Category { get; set; }
        public string? Difficulty { get; set; }
        public List<CourseHighlightDto> Highlights { get; set; } = [];
        public List<string> LearningObjectives { get; set; } = [];
        public List<string> KeyOutcomes { get; set; } = [];
    }

    public sealed class CourseUpsertRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Difficulty { get; set; }
        public int? TopicId { get; set; }
        public int? SortOrder { get; set; }               // null = giữ nguyên (update) / 1 (create)
        public bool IsActive { get; set; } = true;
        public string? Scope { get; set; }               // draft | class | public
        public string? Status { get; set; }              // draft | class | pending_review | active
        public List<string> LearningObjectives { get; set; } = [];
        public List<string> KeyOutcomes { get; set; } = [];
        public List<CourseHighlightDto> Highlights { get; set; } = [];
    }

    public sealed class CourseNodeCreateRequest
    {
        public string Title { get; set; } = string.Empty;
        public int? LessonId { get; set; }
        public int SortOrder { get; set; } = 1;
        public int? FinalTestId { get; set; }
    }

    public sealed class CourseReorderNodesRequest
    {
        public List<int> NodeIds { get; set; } = [];
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
        public int? NodeId { get; set; }
        public int? LessonId { get; set; }
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

    private static string? CleanPrefix(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        return System.Text.RegularExpressions.Regex.Replace(text, @"^Module\s*\d+\s*:\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
    }

    private static string DetermineCategory(LearningPath path, CourseContentMetadata? meta = null)
    {
        if (!string.IsNullOrWhiteSpace(meta?.Category))
            return meta.Category;
        var title = path.Title;
        if (title.Contains("Data Structures", StringComparison.OrdinalIgnoreCase) || title.Contains("CTDL", StringComparison.OrdinalIgnoreCase) || title.Contains("Cấu trúc dữ liệu", StringComparison.OrdinalIgnoreCase) || title.Contains("tuyến tính", StringComparison.OrdinalIgnoreCase))
            return "Cấu trúc dữ liệu";
        if (title.Contains("Cây", StringComparison.OrdinalIgnoreCase) || title.Contains("Tree", StringComparison.OrdinalIgnoreCase) || title.Contains("Bảng băm", StringComparison.OrdinalIgnoreCase) || title.Contains("Hash", StringComparison.OrdinalIgnoreCase))
            return "Cây & Bảng băm";
        if (title.Contains("Đồ thị", StringComparison.OrdinalIgnoreCase) || title.Contains("Graph", StringComparison.OrdinalIgnoreCase))
            return "Đồ thị";
        if (title.Contains("Sắp xếp", StringComparison.OrdinalIgnoreCase) || title.Contains("Sorting", StringComparison.OrdinalIgnoreCase) || title.Contains("Tìm kiếm", StringComparison.OrdinalIgnoreCase) || title.Contains("Searching", StringComparison.OrdinalIgnoreCase))
            return "Sắp xếp & Tìm kiếm";
        if (title.Contains("Algorithms", StringComparison.OrdinalIgnoreCase) || title.Contains("Giải thuật", StringComparison.OrdinalIgnoreCase) || title.Contains("Thuật toán", StringComparison.OrdinalIgnoreCase))
            return "Giải thuật";
        return "Cấu trúc dữ liệu";
    }

    private static string DetermineDifficulty(LearningPath path, CourseContentMetadata? meta = null)
    {
        if (!string.IsNullOrWhiteSpace(meta?.Difficulty))
            return meta.Difficulty;
        var title = path.Title;
        if (title.Contains("Cơ bản", StringComparison.OrdinalIgnoreCase) || title.Contains("Data Structures", StringComparison.OrdinalIgnoreCase) || title.Contains("Sắp xếp", StringComparison.OrdinalIgnoreCase) || title.Contains("tuyến tính", StringComparison.OrdinalIgnoreCase) || path.SortOrder <= 1)
            return "Beginner";
        if (title.Contains("Đồ thị", StringComparison.OrdinalIgnoreCase) || title.Contains("Nâng cao", StringComparison.OrdinalIgnoreCase) || path.SortOrder >= 5)
            return "Advanced";
        return "Intermediate";
    }

    private static string FormatLearningPathStatus(LearningPathStatus status) => status switch
    {
        LearningPathStatus.PendingReview => "pending_review",
        LearningPathStatus.Active => "active",
        LearningPathStatus.Rejected => "rejected",
        LearningPathStatus.ClassOnly => "class",
        _ => "draft"
    };

    [HttpGet("courses")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ConceptsCourseDto>>> GetCourses(CancellationToken ct)
    {
        var userId = TryGetCurrentUserId();
        var role = TryGetCurrentRole();

        var query = _db.LearningPaths.AsNoTracking();
        if (role == "ADMIN")
        {
            // Admin thấy tất cả lộ trình (kể cả Draft, PendingReview)
        }
        else if (role == "TEACHER" && userId is not null)
        {
            // Teacher thấy lộ trình Active/ClassOnly + lộ trình do mình tạo (bản nháp của mình)
            query = query.Where(p => p.Status == LearningPathStatus.Active 
                || p.Status == LearningPathStatus.ClassOnly
                || p.CreatedBy == userId.Value 
                || (p.AuthorId != null && p.AuthorId == userId.Value));
        }
        else if (userId is not null)
        {
            // Student chỉ thấy lộ trình Active (Công khai) + lộ trình ClassOnly của các lớp mình tham gia
            var myClassIds = await _db.ClassMembers.AsNoTracking()
                .Where(m => m.UserId == userId.Value)
                .Select(m => m.ClassId)
                .ToListAsync(ct);

            var myClassPathIds = myClassIds.Count > 0
                ? await _db.Classes.AsNoTracking()
                    .Where(c => myClassIds.Contains(c.Id) && c.LearningPathId != null && c.DeletedAt == null)
                    .Select(c => c.LearningPathId!.Value)
                    .Distinct()
                    .ToListAsync(ct)
                : new List<int>();

            query = query.Where(p => p.Status == LearningPathStatus.Active 
                || (p.Status == LearningPathStatus.ClassOnly && myClassPathIds.Contains(p.Id)));
        }
        else
        {
            // Khách chỉ thấy lộ trình Active (Công khai)
            query = query.Where(p => p.Status == LearningPathStatus.Active);
        }

        var paths = await query.OrderBy(p => p.SortOrder).ToListAsync(ct);

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
        var passedNodeIds = allNodeIds.Count == 0 || userId is null
            ? new HashSet<int>()
            : (await _db.UserNodeProgress.AsNoTracking()
                .Where(p => p.UserId == userId.Value && allNodeIds.Contains(p.NodeId) && p.Status == 2)
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

        var allLessonIds = allNodes.Where(n => n.LessonId != null).Select(n => n.LessonId!.Value).Distinct().ToList();
        var allFeedbacks = allLessonIds.Count > 0
            ? await _db.ContentFeedback.AsNoTracking()
                .Where(f => allLessonIds.Contains(f.LessonId))
                .Select(f => new { f.LessonId, f.Rating })
                .ToListAsync(ct)
            : [];
        var feedbackByLesson = allFeedbacks.GroupBy(f => f.LessonId).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<ConceptsCourseDto>();
        var pathTopicIds = paths.Where(p => p.TopicId != null).Select(p => p.TopicId!.Value).Distinct().ToList();
        var topicsMap = pathTopicIds.Count > 0
            ? await _db.Topics.AsNoTracking()
                .Where(t => pathTopicIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Name, ct)
            : new Dictionary<int, string>();
        foreach (var path in paths)
        {
            var nodes = nodesByPath.GetValueOrDefault(path.Id, []);
            var playableNodes = nodes.Where(n => n.ItemType != PathItemType.Folder).ToList();
            var playableNodeIds = playableNodes.Select(n => n.Id).ToList();
            var completed = playableNodes.Count(n => passedNodeIds.Contains(n.Id));
            var totalLessons = playableNodes.Count;
            var lessonIds = nodes.Where(n => n.LessonId != null).Select(n => n.LessonId!.Value).ToList();
            var feedbacks = lessonIds.SelectMany(id => feedbackByLesson.GetValueOrDefault(id, [])).ToList();
            var rating = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 4.8;
            var ratingCount = feedbacks.Count > 0 ? feedbacks.Count : 12;

            var meta = ParseMetadata(path.HighlightsJson, path.Title);
            var author = path.AuthorId.HasValue ? authorsMap.GetValueOrDefault(path.AuthorId.Value) : null;
            result.Add(new ConceptsCourseDto
            {
                Id = path.Id.ToString(),
                Title = path.Title,
                Description = path.Description ?? string.Empty,
                Category = DetermineCategory(path, meta),
                Difficulty = DetermineDifficulty(path, meta),
                TopicId = path.TopicId,
                TopicName = path.TopicId != null ? CleanPrefix(topicsMap.GetValueOrDefault(path.TopicId.Value)) : null,
                IsPremium = false,
                IsPublished = path.Status == LearningPathStatus.Active,
                Status = FormatLearningPathStatus(path.Status),
                RejectionReason = path.RejectionReason,
                ReviewedBy = path.ReviewedBy,
                ReviewedAt = path.ReviewedAt,
                SubmittedAt = path.SubmittedAt,
                AuthorName = author?.Name,
                CreatedBy = path.CreatedBy,
                AuthorId = path.AuthorId,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                TotalLessons = totalLessons,
                CompletedLessons = completed,
                ProgressPercent = totalLessons == 0 ? 0 : (int)Math.Round(completed * 100.0 / totalLessons),
                XpReward = playableNodes.Sum(n => GetNodeXpReward(n)),
                LearningObjectives = meta.LearningObjectives,
                KeyOutcomes = meta.KeyOutcomes,
                Rating = rating,
                RatingCount = ratingCount,
                Highlights = meta.Highlights,
                Testimonials = ParseTestimonials(path.TestimonialsJson),
                Author = author,
                Lessons = []
            });
        }

        return Ok(result);
    }

    [HttpGet("courses/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ConceptsCourseDto>> GetCourse(int id, CancellationToken ct)
    {
        var userId = TryGetCurrentUserId();
        var role = TryGetCurrentRole();
        var path = await _db.LearningPaths.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (path is null)
        {
            return NotFound(new { message = "Khóa học không tồn tại." });
        }

        var isOwnerOrAuthor = userId != null && (path.CreatedBy == userId.Value || (path.AuthorId.HasValue && path.AuthorId.Value == userId.Value));
        var isTeacherOrAdmin = role == "ADMIN" || (role == "TEACHER" && isOwnerOrAuthor);
        var canView = isTeacherOrAdmin || path.Status == LearningPathStatus.Active;
        if (!canView && userId != null)
        {
            canView = await _db.Classes.AsNoTracking()
                .Where(c => c.LearningPathId == id && c.DeletedAt == null)
                .Join(_db.ClassMembers.AsNoTracking(), c => c.Id, m => m.ClassId, (c, m) => m.UserId)
                .AnyAsync(uId => uId == userId.Value, ct);
        }

        if (!canView)
        {
            return NotFound(new { message = "Khóa học không tồn tại hoặc hiện đang ở chế độ bản nháp." });
        }

        var nodes = await _db.LearningPathNodes.AsNoTracking()
            .Where(n => n.PathId == path.Id)
            .OrderBy(n => n.SortOrder)
            .ToListAsync(ct);

        var playableNodes = nodes.Where(n => n.ItemType != PathItemType.Folder).ToList();
        var playableNodeIds = playableNodes.Select(n => n.Id).ToList();
        var passedNodeIds = playableNodeIds.Count == 0 || userId is null
            ? new HashSet<int>()
            : (await _db.UserNodeProgress.AsNoTracking()
                .Where(p => p.UserId == userId.Value && playableNodeIds.Contains(p.NodeId) && p.Status == 2)
                .Select(p => p.NodeId)
                .ToListAsync(ct))
                .ToHashSet();

        var completed = playableNodes.Count(n => passedNodeIds.Contains(n.Id));
        var isOwnerOrTeacher = role == "ADMIN" || (role == "TEACHER" && (path.CreatedBy == userId || path.AuthorId == userId));
        var lessons = await BuildLessonsAsync(userId, role, nodes, isOwnerOrTeacher, ct, passedNodeIds);
        var (rating, ratingCount) = await CourseRatingAsync(nodes, ct);
        var meta = ParseMetadata(path.HighlightsJson, path.Title);
        var author = await CourseAuthorAsync(path.AuthorId, ct);
        return Ok(new ConceptsCourseDto
        {
            Id = path.Id.ToString(),
            Title = path.Title,
            Description = path.Description ?? string.Empty,
            TopicId = path.TopicId,
            TopicName = path.TopicId != null ? CleanPrefix(await _db.Topics.Where(t => t.Id == path.TopicId.Value).Select(t => t.Name).FirstOrDefaultAsync(ct)) : null,
            Category = DetermineCategory(path, meta),
            Difficulty = DetermineDifficulty(path, meta),
            IsPremium = false,
            IsPublished = path.Status == LearningPathStatus.Active,
            Status = FormatLearningPathStatus(path.Status),
            RejectionReason = path.RejectionReason,
            ReviewedBy = path.ReviewedBy,
            ReviewedAt = path.ReviewedAt,
            SubmittedAt = path.SubmittedAt,
            AuthorName = author?.Name,
            CreatedBy = path.CreatedBy,
            AuthorId = path.AuthorId,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            TotalLessons = lessons.Count,
            CompletedLessons = completed,
            ProgressPercent = lessons.Count == 0 ? 0 : (int)Math.Round(completed * 100.0 / lessons.Count),
            XpReward = lessons.Sum(l => l.XpReward),
            LearningObjectives = meta.LearningObjectives,
            KeyOutcomes = meta.KeyOutcomes,
            Rating = rating,
            RatingCount = ratingCount,
            Highlights = meta.Highlights,
            Testimonials = ParseTestimonials(path.TestimonialsJson),
            Author = author,
            Lessons = lessons
        });
    }

    [HttpPost("courses")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ConceptsCourseDto>> CreateCourse([FromBody] CourseUpsertRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var role = CurrentRole();
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Tiêu đề khóa học không được để trống." });
        }

        var meta = new CourseContentMetadata
        {
            Category = request.Category,
            Difficulty = request.Difficulty,
            Highlights = request.Highlights,
            LearningObjectives = request.LearningObjectives,
            KeyOutcomes = request.KeyOutcomes
        };

        var requestedScope = (request.Scope ?? request.Status ?? "draft").ToLowerInvariant();
        LearningPathStatus status;
        if (requestedScope == "draft")
        {
            status = LearningPathStatus.Draft;
        }
        else if (requestedScope == "class" || requestedScope == "classonly")
        {
            status = LearningPathStatus.ClassOnly;
        }
        else if (role == "ADMIN" && (request.IsActive || requestedScope == "active" || requestedScope == "public"))
        {
            status = LearningPathStatus.Active;
        }
        else if (requestedScope == "public" || requestedScope == "pending_review")
        {
            status = LearningPathStatus.PendingReview;
        }
        else
        {
            status = LearningPathStatus.Draft;
        }

        var isActive = status == LearningPathStatus.Active;

        var path = new LearningPath
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            TopicId = request.TopicId,
            SortOrder = request.SortOrder ?? 1,
            IsActive = isActive,
            Status = status,
            CreatedBy = userId,
            AuthorId = userId,
            HighlightsJson = JsonSerializer.Serialize(meta)
        };

        if (status == LearningPathStatus.Active)
        {
            path.ReviewedBy = userId;
            path.ReviewedAt = DateTime.UtcNow;
        }
        else if (status == LearningPathStatus.PendingReview)
        {
            path.SubmittedAt = DateTime.UtcNow;
        }

        _db.LearningPaths.Add(path);
        await _db.SaveChangesAsync(ct);

        return await GetCourse(path.Id, ct);
    }

    [HttpPut("courses/{id:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<ConceptsCourseDto>> UpdateCourse([FromRoute] int id, [FromBody] CourseUpsertRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var role = CurrentRole();
        var path = await _db.LearningPaths.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (path is null)
        {
            return NotFound(new { message = "Khóa học không tồn tại." });
        }

        var canManage = role == "ADMIN" || path.CreatedBy == userId || path.AuthorId == userId || path.CreatedBy <= 1;
        if (!canManage)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Tiêu đề khóa học không được để trống." });
        }

        var meta = new CourseContentMetadata
        {
            Category = request.Category,
            Difficulty = request.Difficulty,
            Highlights = request.Highlights,
            LearningObjectives = request.LearningObjectives,
            KeyOutcomes = request.KeyOutcomes
        };

        path.Title = request.Title.Trim();
        path.Description = request.Description?.Trim();
        // Fix mất phân loại chủ đề khi lưu lộ trình: chỉ gán khi client gửi rõ ràng,
        // tránh mỗi lần PUT không kèm topicId/sortOrder làm TopicId = null, SortOrder = 1.
        if (request.TopicId.HasValue)
        {
            path.TopicId = request.TopicId;
        }
        if (request.SortOrder.HasValue)
        {
            path.SortOrder = request.SortOrder.Value;
        }
        path.HighlightsJson = JsonSerializer.Serialize(meta);

        var requestedScope = (request.Scope ?? request.Status ?? "").ToLowerInvariant();
        if (requestedScope == "class" || requestedScope == "classonly")
        {
            if (path.Status == LearningPathStatus.Active || path.Status == LearningPathStatus.PendingReview)
            {
                return BadRequest(new { message = "Lộ trình đã xuất bản công khai toàn hệ thống, không thể chuyển về phạm vi lớp riêng." });
            }
            path.Status = LearningPathStatus.ClassOnly;
            path.IsActive = true;
        }
        else if (requestedScope == "draft")
        {
            if (path.Status == LearningPathStatus.Active || path.Status == LearningPathStatus.PendingReview || path.Status == LearningPathStatus.ClassOnly)
            {
                return BadRequest(new { message = "Lộ trình đã được phát hành (vào lớp học hoặc công khai), không thể chuyển ngược về Bản nháp." });
            }
            path.Status = LearningPathStatus.Draft;
            path.IsActive = false;
        }
        else if (requestedScope == "public" || requestedScope == "pending_review" || requestedScope == "active")
        {
            if (role == "ADMIN")
            {
                path.Status = LearningPathStatus.Active;
                path.IsActive = true;
                path.RejectionReason = null;
                path.ReviewedBy = userId;
                path.ReviewedAt = DateTime.UtcNow;
                await ActivateAllLessonsInPath(path.Id, userId, ct);
            }
            else
            {
                // Nếu lộ trình đã được Admin phê duyệt (Active), Giảng viên cập nhật metadata
                // thì giữ nguyên Active, không tự động giáng cấp về PendingReview!
                if (path.Status != LearningPathStatus.Active)
                {
                    path.Status = LearningPathStatus.PendingReview;
                    path.SubmittedAt = DateTime.UtcNow;
                    path.RejectionReason = null;
                }
            }
        }
        else if (role == "ADMIN" && string.IsNullOrWhiteSpace(request.Scope) && string.IsNullOrWhiteSpace(request.Status))
        {
            path.IsActive = request.IsActive;
            if (request.IsActive && path.Status != LearningPathStatus.Active)
            {
                path.Status = LearningPathStatus.Active;
                path.RejectionReason = null;
                path.ReviewedBy = userId;
                path.ReviewedAt = DateTime.UtcNow;
                await ActivateAllLessonsInPath(path.Id, userId, ct);
            }
            else if (!request.IsActive && path.Status == LearningPathStatus.Active)
            {
                path.Status = LearningPathStatus.Draft;
            }
        }

        await _db.SaveChangesAsync(ct);
        return await GetCourse(path.Id, ct);
    }

    /// <summary>Gán nhanh lộ trình cho nhiều lớp (Cửa phụ trong Builder lộ trình).</summary>
    [HttpPost("courses/{id:int}/assign-classes")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> AssignCourseToClasses([FromRoute] int id, [FromBody] AssignCourseToClassesRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var role = CurrentRole();
        var path = await _db.LearningPaths.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (path is null)
        {
            return NotFound(new { message = "Khóa học không tồn tại." });
        }

        var canManage = role == "ADMIN" || path.CreatedBy == userId || path.AuthorId == userId || path.CreatedBy <= 1;
        if (!canManage)
        {
            return Forbid();
        }

        // Giáo viên chỉ được gán lộ trình dành cho lớp học (ClassOnly) cho lớp học
        var isClassOnly = path.Status == LearningPathStatus.ClassOnly || path.Visibility == PathVisibility.ClassOnly;
        if (role != "ADMIN" && !isClassOnly)
        {
            return BadRequest(new { message = "Giáo viên chỉ được gán lộ trình dành cho lớp học (Class Only) cho lớp học." });
        }

        if (request.ClassIds == null || request.ClassIds.Count == 0)
        {
            return BadRequest(new { message = "Danh sách lớp không được để trống." });
        }

        var classes = await _db.Classes
            .Where(c => request.ClassIds.Contains(c.Id) && c.DeletedAt == null)
            .ToListAsync(ct);

        int assignedCount = 0;
        foreach (var c in classes)
        {
            if (role != "ADMIN" && c.OwnerId != userId)
            {
                continue; // Chỉ gán những lớp GV sở hữu
            }

            // Không cho phép đổi lộ trình khác khi lớp đã có học viên
            if (c.LearningPathId != id && c.LearningPathId != null)
            {
                var hasStudents = await _db.ClassMembers.AsNoTracking()
                    .AnyAsync(m => m.ClassId == c.Id && m.UserId != c.OwnerId, ct);
                if (hasStudents)
                {
                    continue;
                }
            }

            c.LearningPathId = id;
            c.CurriculumTitle ??= path.Title;
            c.CurriculumDescription ??= path.Description;
            c.CurriculumPublished = true;
            assignedCount++;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = $"Đã gán lộ trình cho {assignedCount} lớp thành công." });
    }

    // ═══ Helper: Active tất cả bài trong lộ trình ═══
    private async Task ActivateAllLessonsInPath(int pathId, int adminUserId, CancellationToken ct)
    {
        var lessonIds = await _db.LearningPathNodes
            .Where(n => n.PathId == pathId && n.LessonId != null)
            .Select(n => n.LessonId!.Value)
            .ToListAsync(ct);

        if (lessonIds.Count == 0) return;

        var lessons = await _db.Lessons
            .Where(l => lessonIds.Contains(l.Id) && l.DeletedAt == null)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var lesson in lessons)
        {
            if (lesson.Status != LessonStatus.Active)
            {
                lesson.Status = LessonStatus.Active;
                lesson.PublishedAt ??= now;
                lesson.UpdatedBy = adminUserId;
                lesson.UpdatedAt = now;
            }
        }
    }

    // ═══ GV gửi duyệt lộ trình ═══
    [HttpPost("courses/{id:int}/submit-review")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> SubmitCourseForReview(int id, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var role = CurrentRole();
        var path = await _db.LearningPaths.FindAsync([id], ct);
        if (path is null) return NotFound(new { message = "Lộ trình không tồn tại." });

        if (role != "ADMIN" && path.CreatedBy != userId && path.AuthorId != userId && path.CreatedBy > 1)
            return Forbid();

        var nodeCount = await _db.LearningPathNodes.CountAsync(n => n.PathId == id && (n.LessonId != null || n.FinalTestId != null || n.LabExerciseId != null || n.ItemType != PathItemType.Folder), ct);
        if (nodeCount == 0)
        {
            nodeCount = await _db.LearningPathNodes.CountAsync(n => n.PathId == id, ct);
        }
        if (nodeCount == 0)
            return BadRequest(new { message = "Lộ trình phải có ít nhất 1 bài học hoặc mục nội dung trước khi gửi duyệt." });

        if (role == "ADMIN")
        {
            path.Status = LearningPathStatus.Active;
            path.IsActive = true;
            path.RejectionReason = null;
            path.ReviewedBy = userId;
            path.ReviewedAt = DateTime.UtcNow;
            await ActivateAllLessonsInPath(id, userId, ct);
        }
        else
        {
            path.Status = LearningPathStatus.PendingReview;
            path.SubmittedAt = DateTime.UtcNow;
            path.RejectionReason = null;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new
        {
            message = role == "ADMIN"
                ? "Lộ trình đã xuất bản thành công!"
                : "Lộ trình đã được gửi đến Admin để duyệt."
        });
    }

    // ═══ Admin duyệt lộ trình ═══
    [HttpPost("courses/{id:int}/review")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult> ReviewCourse(
        int id,
        [FromBody] CourseReviewRequest request,
        CancellationToken ct)
    {
        var path = await _db.LearningPaths.FindAsync([id], ct);
        if (path is null) return NotFound(new { message = "Lộ trình không tồn tại." });

        if (path.Status != LearningPathStatus.PendingReview)
            return BadRequest(new { message = "Lộ trình không ở trạng thái chờ duyệt." });

        var adminId = CurrentUserId();
        var now = DateTime.UtcNow;

        if (request.Approve)
        {
            path.Status = LearningPathStatus.Active;
            path.IsActive = true;
            path.RejectionReason = null;
            path.ReviewedBy = adminId;
            path.ReviewedAt = now;
            await ActivateAllLessonsInPath(id, adminId, ct);
        }
        else
        {
            path.Status = LearningPathStatus.Rejected;
            path.IsActive = false;
            path.RejectionReason = request.Reason?.Trim();
            path.ReviewedBy = adminId;
            path.ReviewedAt = now;
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new
        {
            message = request.Approve
                ? "Lộ trình và tất cả bài giảng đã được duyệt thành công!"
                : "Đã từ chối lộ trình và gửi lý do tới Giảng viên."
        });
    }

    // ═══ Admin lấy danh sách lộ trình chờ duyệt ═══
    [HttpGet("courses/pending")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<List<ConceptsCourseDto>>> GetPendingCourses(CancellationToken ct)
    {
        var paths = await _db.LearningPaths.AsNoTracking()
            .Where(p => p.Status == LearningPathStatus.PendingReview)
            .OrderBy(p => p.SubmittedAt)
            .ToListAsync(ct);

        if (paths.Count == 0) return Ok(new List<ConceptsCourseDto>());

        var pathIds = paths.Select(p => p.Id).ToList();
        var allNodes = await _db.LearningPathNodes.AsNoTracking()
            .Where(n => pathIds.Contains(n.PathId))
            .OrderBy(n => n.SortOrder)
            .ToListAsync(ct);

        var nodesByPath = allNodes.GroupBy(n => n.PathId).ToDictionary(g => g.Key, g => g.ToList());

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
            var meta = ParseMetadata(path.HighlightsJson, path.Title);
            var author = path.AuthorId is { } aId ? authorsMap.GetValueOrDefault(aId) : null;

            result.Add(new ConceptsCourseDto
            {
                Id = path.Id.ToString(),
                Title = path.Title,
                Description = path.Description ?? string.Empty,
                Category = DetermineCategory(path),
                Difficulty = DetermineDifficulty(path),
                IsPremium = false,
                IsPublished = false,
                Status = FormatLearningPathStatus(path.Status),
                RejectionReason = path.RejectionReason,
                ReviewedBy = path.ReviewedBy,
                ReviewedAt = path.ReviewedAt,
                SubmittedAt = path.SubmittedAt,
                AuthorName = author?.Name,
                CreatedBy = path.CreatedBy,
                AuthorId = path.AuthorId,
                CreatedAt = path.SubmittedAt ?? DateTime.UtcNow,
                TotalLessons = nodes.Count,
                CompletedLessons = 0,
                ProgressPercent = 0,
                XpReward = 0,
                LearningObjectives = meta.LearningObjectives,
                KeyOutcomes = meta.KeyOutcomes,
                Rating = 5.0,
                RatingCount = 0,
                Highlights = meta.Highlights,
                Testimonials = ParseTestimonials(path.TestimonialsJson),
                Author = author,
                Lessons = await BuildLessonsAsync(null, null, nodes, isOwnerOrTeacher: false, ct: ct)
            });
        }

        return Ok(result);
    }

    [HttpDelete("courses/{id:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> DeleteCourse([FromRoute] int id, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var role = CurrentRole();
        var path = await _db.LearningPaths.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (path is null)
        {
            return NotFound(new { message = "Khóa học không tồn tại." });
        }

        var canManage = role == "ADMIN" || path.CreatedBy == userId || path.AuthorId == userId || path.CreatedBy <= 1;
        if (!canManage)
        {
            return Forbid();
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var classes = await _db.Classes.Where(c => c.LearningPathId == id).ToListAsync(ct);
            foreach (var cls in classes)
            {
                cls.LearningPathId = null;
            }

            var nodes = await _db.LearningPathNodes.Where(n => n.PathId == id).ToListAsync(ct);
            var nodeIds = nodes.Select(n => n.Id).ToList();

            if (nodeIds.Count > 0)
            {
                // 1. Unlink ClassAssignments
                var assignments = await _db.ClassAssignments
                    .Where(a => a.PathItemId != null && nodeIds.Contains(a.PathItemId.Value))
                    .ToListAsync(ct);
                foreach (var a in assignments)
                {
                    a.PathItemId = null;
                }

                // 2. Unlink Exercises pointing to these nodes
                var exercises = await _db.Exercises
                    .Where(e => e.NodeId != null && nodeIds.Contains(e.NodeId.Value))
                    .ToListAsync(ct);
                foreach (var ex in exercises)
                {
                    ex.NodeId = null;
                }

                // 3. Remove NodeSessions
                var sessions = await _db.NodeSessions
                    .Where(s => nodeIds.Contains(s.NodeId))
                    .ToListAsync(ct);
                if (sessions.Count > 0)
                {
                    _db.NodeSessions.RemoveRange(sessions);
                }

                // 4. Remove UserNodeProgress
                var progresses = await _db.UserNodeProgress
                    .Where(p => nodeIds.Contains(p.NodeId))
                    .ToListAsync(ct);
                if (progresses.Count > 0)
                {
                    _db.UserNodeProgress.RemoveRange(progresses);
                }

                // 5. Break self-referencing hierarchy (ParentId) to avoid SQL Server circular/self-reference restrict constraint
                foreach (var n in nodes)
                {
                    n.ParentId = null;
                }
                await _db.SaveChangesAsync(ct);

                // 6. Remove nodes
                _db.LearningPathNodes.RemoveRange(nodes);
            }

            // 7. Remove course feedbacks
            var feedbacks = await _db.CourseFeedback.Where(f => f.CourseId == id).ToListAsync(ct);
            if (feedbacks.Count > 0)
            {
                _db.CourseFeedback.RemoveRange(feedbacks);
            }

            // 8. Remove LearningPath
            _db.LearningPaths.Remove(path);
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return NoContent();
        }
        catch (Exception)
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    [HttpPost("courses/{id:int}/nodes")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<object>> AddCourseNode([FromRoute] int id, [FromBody] CourseNodeCreateRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var role = CurrentRole();
        var path = await _db.LearningPaths.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (path is null)
        {
            return NotFound(new { message = "Khóa học không tồn tại." });
        }

        var canManage = role == "ADMIN" || path.CreatedBy == userId || path.AuthorId == userId || path.CreatedBy <= 1;
        if (!canManage)
        {
            return Forbid();
        }

        var maxSort = await _db.LearningPathNodes.Where(n => n.PathId == id).MaxAsync(n => (int?)n.SortOrder, ct) ?? 0;
        var node = new LearningPathNode
        {
            PathId = id,
            Title = string.IsNullOrWhiteSpace(request.Title) ? "Bài học mới" : request.Title.Trim(),
            LessonId = request.LessonId,
            SortOrder = request.SortOrder > 0 ? request.SortOrder : maxSort + 1,
            FinalTestId = request.FinalTestId
        };

        _db.LearningPathNodes.Add(node);
        await _db.SaveChangesAsync(ct);

        return Ok(new { id = node.Id, pathId = node.PathId, title = node.Title, lessonId = node.LessonId, sortOrder = node.SortOrder });
    }

    [HttpDelete("courses/{id:int}/nodes/{nodeId:int}")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> DeleteCourseNode([FromRoute] int id, [FromRoute] int nodeId, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var role = CurrentRole();
        var path = await _db.LearningPaths.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (path is null)
        {
            return NotFound(new { message = "Khóa học không tồn tại." });
        }

        var canManage = role == "ADMIN" || path.CreatedBy == userId || path.AuthorId == userId || path.CreatedBy <= 1;
        if (!canManage)
        {
            return Forbid();
        }

        var node = await _db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == nodeId && n.PathId == id, ct);
        if (node is not null)
        {
            _db.LearningPathNodes.Remove(node);
            await _db.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    [HttpPut("courses/{id:int}/reorder")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult> ReorderCourseNodes([FromRoute] int id, [FromBody] CourseReorderNodesRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var role = CurrentRole();
        var path = await _db.LearningPaths.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (path is null)
        {
            return NotFound(new { message = "Khóa học không tồn tại." });
        }

        var canManage = role == "ADMIN" || path.CreatedBy == userId || path.AuthorId == userId || path.CreatedBy <= 1;
        if (!canManage)
        {
            return Forbid();
        }

        var nodes = await _db.LearningPathNodes.Where(n => n.PathId == id).ToListAsync(ct);
        for (var i = 0; i < request.NodeIds.Count; i++)
        {
            var targetNode = nodes.FirstOrDefault(n => n.Id == request.NodeIds[i]);
            if (targetNode is not null)
            {
                targetNode.SortOrder = i + 1;
            }
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
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

    /// <summary>Metadata ("Why choose" + Mục tiêu + Kết quả) — JSON tùy biến theo khóa.</summary>
    private static CourseContentMetadata ParseMetadata(string? json, string courseTitle)
    {
        var meta = new CourseContentMetadata();
        if (string.IsNullOrWhiteSpace(json))
        {
            meta.LearningObjectives = CourseLearningObjectives(courseTitle);
            meta.KeyOutcomes = CourseKeyOutcomes(courseTitle);
            return meta;
        }

        try
        {
            var trimmed = json.Trim();
            if (trimmed.StartsWith('{'))
            {
                var parsed = JsonSerializer.Deserialize<CourseContentMetadata>(trimmed, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed != null)
                {
                    meta.Category = parsed.Category;
                    meta.Difficulty = parsed.Difficulty;
                    meta.Highlights = parsed.Highlights ?? [];
                    meta.LearningObjectives = parsed.LearningObjectives?.Count > 0 ? parsed.LearningObjectives : CourseLearningObjectives(courseTitle);
                    meta.KeyOutcomes = parsed.KeyOutcomes?.Count > 0 ? parsed.KeyOutcomes : CourseKeyOutcomes(courseTitle);
                    return meta;
                }
            }
            else if (trimmed.StartsWith('['))
            {
                meta.Highlights = JsonSerializer.Deserialize<List<CourseHighlightDto>>(trimmed, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            }
        }
        catch
        {
            // fallback
        }

        meta.LearningObjectives = CourseLearningObjectives(courseTitle);
        meta.KeyOutcomes = CourseKeyOutcomes(courseTitle);
        return meta;
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
    /// Khoá node tuần tự (nghiệp vụ lộ trình): node MỞ khi node NGAY TRƯỚC nó (theo thứ tự duyệt DFS
    /// lộ trình) đã hoàn thành (UserNodeProgress Status=2) — hoặc node đó là node đầu tiên của path.
    /// Node đã tự pass thì luôn mở (được xem lại). Nghiệp vụ: học từ trên xuống —
    /// hết bài 1 mới mở bài 2 → xong bài 2 mới mở quiz → xong quiz mới mở assignment → sang module sau.
    /// </summary>
    private async Task<bool> IsNodeLockedAsync(int? userId, LearningPathNode node, CancellationToken ct)
    {
        if (DisableNodeLocks || userId is null)
        {
            return false;
        }

        var role = TryGetCurrentRole();
        if (role == "ADMIN" || role == "TEACHER")
        {
            return false;
        }

        var passed = await _db.UserNodeProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == userId.Value && p.NodeId == node.Id && p.Status == 2, ct);
        if (passed)
        {
            return false;
        }

        var allNodes = await _db.LearningPathNodes.AsNoTracking()
            .Where(n => n.PathId == node.PathId)
            .OrderBy(n => n.SortOrder)
            .ToListAsync(ct);

        if (allNodes.Count == 0)
        {
            return false;
        }

        var childrenByParent = allNodes.ToLookup(n => n.ParentId);
        var orderedPlayableNodes = new List<LearningPathNode>();

        void Traverse(int? parentId)
        {
            var children = childrenByParent[parentId].OrderBy(n => n.SortOrder).ToList();
            foreach (var child in children)
            {
                if (child.ItemType == PathItemType.Folder)
                {
                    Traverse(child.Id);
                }
                else
                {
                    orderedPlayableNodes.Add(child);
                    Traverse(child.Id);
                }
            }
        }

        Traverse(null);

        if (orderedPlayableNodes.Count == 0)
        {
            orderedPlayableNodes = allNodes.Where(n => n.ItemType != PathItemType.Folder).OrderBy(n => n.SortOrder).ToList();
        }

        var currentIndex = orderedPlayableNodes.FindIndex(n => n.Id == node.Id);
        if (currentIndex <= 0)
        {
            return false;
        }

        var prevNode = orderedPlayableNodes[currentIndex - 1];
        var prevPassed = await _db.UserNodeProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == userId.Value && p.NodeId == prevNode.Id && p.Status == 2, ct);

        if (!prevPassed && prevNode.LessonId != null)
        {
            prevPassed = await _db.UserProgress.AsNoTracking()
                .AnyAsync(p => p.UserId == userId.Value && p.LessonId == prevNode.LessonId.Value && (p.CompletedAt != null || (p.BestScore ?? 0) > 0), ct);
        }

        return !prevPassed;
    }

    private async Task<List<ConceptsLessonDto>> BuildLessonsAsync(
        int? userId, string? role, List<LearningPathNode> nodes, bool isOwnerOrTeacher = false, CancellationToken ct = default, HashSet<int>? preloadedPassedNodeIds = null)
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
        var finalTestIds = nodes.Where(n => n.FinalTestId != null).Select(n => n.FinalTestId!.Value).Distinct().ToList();
        var labExerciseIds = nodes.Where(n => n.LabExerciseId != null).Select(n => n.LabExerciseId!.Value).Distinct().ToList();

        var exByNode = nodeIds.Count > 0
            ? await _db.Exercises.AsNoTracking()
                .Where(e => e.NodeId != null && nodeIds.Contains(e.NodeId.Value) && e.DeletedAt == null)
                .ToListAsync(ct)
            : [];
        var exByLesson = lessonIds.Count > 0
            ? await _db.Exercises.AsNoTracking()
                .Where(e => lessonIds.Contains(e.LessonId) && e.DeletedAt == null)
                .ToListAsync(ct)
            : [];
        var otherIds = finalTestIds.Concat(labExerciseIds).Distinct().ToList();
        var exById = otherIds.Count > 0
            ? await _db.Exercises.AsNoTracking()
                .Where(e => otherIds.Contains(e.Id) && e.DeletedAt == null)
                .ToListAsync(ct)
            : [];
        var exercisesList = exByNode.Concat(exByLesson).Concat(exById).DistinctBy(e => e.Id).ToList();
        var exercisesByNode = exercisesList
            .Where(e => e.NodeId != null)
            .GroupBy(e => e.NodeId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
        var exercisesByLesson = exercisesList
            .GroupBy(e => e.LessonId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var lessonSims = lessonIds.Count > 0
            ? await _db.LessonSimulations.AsNoTracking()
                .Where(s => lessonIds.Contains(s.LessonId))
                .ToListAsync(ct)
            : [];
        var simsByLesson = lessonSims.GroupBy(s => s.LessonId)
            .ToDictionary(g => g.Key, g => g.Select(s => s.SimulationKey).ToList());

        var passedNodeIds = preloadedPassedNodeIds ?? (userId is null
            ? new HashSet<int>()
            : (await _db.UserNodeProgress.AsNoTracking()
                .Where(p => p.UserId == userId.Value && nodeIds.Contains(p.NodeId) && p.Status == 2)
                .Select(p => p.NodeId)
                .ToListAsync(ct))
                .ToHashSet());

        // Duyệt cây pre-order depth-first (ToLookup hỗ trợ null/0 key an toàn cho root nodes)
        var childrenByParent = nodes.ToLookup(n => (n.ParentId == null || n.ParentId <= 0) ? (int?)null : n.ParentId);
        var hasFolders = nodes.Any(n => n.ItemType == PathItemType.Folder);

        var orderedPageNodes = new List<(LearningPathNode Node, string ModuleTitle)>();

        void Traverse(int? parentId, string currentModule)
        {
            var children = childrenByParent[parentId].OrderBy(n => n.SortOrder).ToList();
            if (children.Count == 0) return;
            foreach (var child in children)
            {
                if (child.ItemType == PathItemType.Folder)
                {
                    var folderModule = string.IsNullOrEmpty(currentModule) ? child.Title : $"{currentModule} > {child.Title}";
                    Traverse(child.Id, folderModule);
                }
                else
                {
                    var isFinalAssessment = child.FinalTestId != null
                        || child.Title.Contains("Final-Quiz", StringComparison.OrdinalIgnoreCase)
                        || child.Title.Contains("Final-Quizz", StringComparison.OrdinalIgnoreCase)
                        || child.Title.Contains("Kiểm tra cuối", StringComparison.OrdinalIgnoreCase);

                    var mod = !string.IsNullOrEmpty(currentModule) ? currentModule
                        : (!hasFolders && child.LessonId is { } lid && lessonsMap.TryGetValue(lid, out var l) && topicsMap.TryGetValue(l.TopicId, out var t)) ? t.Name
                        : isFinalAssessment ? "Module 9: Đánh giá & Kiểm tra cuối khóa"
                        : "Nội dung bài học";

                    if (isFinalAssessment && topicsMap.Count > 1 && (mod.StartsWith("Module 1", StringComparison.OrdinalIgnoreCase) || mod == "Nội dung bài học"))
                    {
                        mod = "Module 9: Đánh giá & Kiểm tra cuối khóa";
                    }

                    orderedPageNodes.Add((child, mod));
                    Traverse(child.Id, mod);
                }
            }
        }

        Traverse(null, string.Empty);

        // Fallback nếu không có cấu trúc cây chuẩn
        if (orderedPageNodes.Count == 0)
        {
            foreach (var node in nodes.Where(n => n.ItemType != PathItemType.Folder).OrderBy(n => n.SortOrder))
            {
                Lesson? lesson = node.LessonId is { } lessonId ? lessonsMap.GetValueOrDefault(lessonId) : null;
                var isFinalAssessment = node.FinalTestId != null
                    || node.Title.Contains("Final-Quiz", StringComparison.OrdinalIgnoreCase)
                    || node.Title.Contains("Final-Quizz", StringComparison.OrdinalIgnoreCase)
                    || node.Title.Contains("Kiểm tra cuối", StringComparison.OrdinalIgnoreCase);

                string mod = (lesson?.TopicId is { } topicId && topicsMap.TryGetValue(topicId, out var topic)) ? topic.Name
                    : isFinalAssessment ? "Module 9: Đánh giá & Kiểm tra cuối khóa"
                    : "Nội dung bài học";

                if (isFinalAssessment && topicsMap.Count > 1 && (mod.StartsWith("Module 1", StringComparison.OrdinalIgnoreCase) || mod == "Nội dung bài học"))
                {
                    mod = "Module 9: Đánh giá & Kiểm tra cuối khóa";
                }

                orderedPageNodes.Add((node, mod));
            }
        }

        var result = new List<ConceptsLessonDto>();
        var prevPassed = true; // node đầu tiên luôn mở
        int pageOrder = 1;

        foreach (var (node, moduleTitle) in orderedPageNodes)
        {
            Lesson? lesson = node.LessonId is { } lessonId ? lessonsMap.GetValueOrDefault(lessonId) : null;
            var title = !string.IsNullOrWhiteSpace(node.Title) ? node.Title : (lesson?.Title ?? string.Empty);
            var content = lesson?.ContentHtml ?? string.Empty;

            var nodeExercises = exercisesByNode.GetValueOrDefault(node.Id, []);
            if (nodeExercises.Count == 0 && node.LessonId is { } lId)
            {
                nodeExercises = exercisesByLesson.GetValueOrDefault(lId, [])
                    .Where(e => e.NodeId == null || e.NodeId == node.Id)
                    .ToList();
            }

            var quizEx = (node.FinalTestId is { } ftId ? exercisesList.FirstOrDefault(e => e.Id == ftId) : null)
                ?? nodeExercises.FirstOrDefault(e => e.Type == ExerciseType.Mcq);
            var codeEx = (node.LabExerciseId is { } labId ? exercisesList.FirstOrDefault(e => e.Id == labId) : null)
                ?? nodeExercises.FirstOrDefault(e => e.Type == ExerciseType.Code);
            var labEx = nodeExercises.FirstOrDefault(e => e.Type == ExerciseType.SimulationLab);

            string sandboxType;
            string sandboxConfig = string.Empty;

            var titleLower = (title ?? string.Empty).ToLowerInvariant();
            var isQuizNaming = titleLower.Contains("quiz") || titleLower.Contains("quizz") || titleLower.Contains("trắc nghiệm") || titleLower.Contains("kiểm tra");
            var isLabNaming = titleLower.Contains("assignment") || titleLower.Contains("lab") || titleLower.Contains("thực hành") || titleLower.Contains("bài tập");

            if (node.ItemType == PathItemType.Folder)
            {
                sandboxType = "folder";
            }
            else if (node.ItemType == PathItemType.Quiz || (quizEx != null && !isLabNaming) || isQuizNaming)
            {
                sandboxType = "quiz";
                sandboxConfig = quizEx != null ? $"{{\"quizId\": {quizEx.Id}}}" : string.Empty;
            }
            else if (node.ItemType == PathItemType.Lab || (codeEx != null && !isQuizNaming) || isLabNaming)
            {
                sandboxType = "codelab";
                sandboxConfig = codeEx?.ConfigJson ?? (codeEx != null ? $"{{\"exerciseId\": {codeEx.Id}}}" : string.Empty);
            }
            else if (node.ItemType == PathItemType.Theory)
            {
                sandboxType = "dsa";
                var simList = node.LessonId is { } lkId ? simsByLesson.GetValueOrDefault(lkId, []) : [];
                if (simList.Count > 0)
                {
                    sandboxConfig = JsonSerializer.Serialize(new { simulationKeys = simList, simulationKey = simList[0] });
                }
                else if (labEx is not null)
                {
                    sandboxConfig = $"{{\"simulationKey\": \"{(labEx.ConfigJson is not null ? TryReadSimulationKey(labEx.ConfigJson) : string.Empty)}\"}}";
                }
            }
            else if (codeEx is not null)
            {
                sandboxType = "codelab";
                sandboxConfig = codeEx.ConfigJson ?? $"{{\"exerciseId\": {codeEx.Id}}}";
            }
            else if (quizEx is not null)
            {
                sandboxType = "quiz";
                sandboxConfig = $"{{\"quizId\": {quizEx.Id}}}";
            }
            else if (labEx is not null)
            {
                sandboxType = "dsa";
                sandboxConfig = $"{{\"simulationKey\": \"{(labEx.ConfigJson is not null ? TryReadSimulationKey(labEx.ConfigJson) : string.Empty)}\"}}";
            }
            else
            {
                sandboxType = "dsa";
                var simList = node.LessonId is { } lkId ? simsByLesson.GetValueOrDefault(lkId, []) : [];
                if (simList.Count > 0)
                {
                    sandboxConfig = JsonSerializer.Serialize(new { simulationKeys = simList, simulationKey = simList[0] });
                }
            }

            var passed = passedNodeIds.Contains(node.Id);
            var locked = DisableNodeLocks || isOwnerOrTeacher ? false : (!prevPassed && !passed);

            result.Add(new ConceptsLessonDto
            {
                Id = node.Id.ToString(),
                NodeId = node.Id,
                LessonId = node.LessonId,
                Title = title,
                ContentMd = content,
                SandboxType = sandboxType,
                SandboxConfig = sandboxConfig,
                QuizId = quizEx?.Id.ToString(),
                XpReward = GetNodeXpReward(sandboxType, codeEx != null, quizEx != null),
                OrderIndex = pageOrder++,
                Status = passed ? "Completed" : "NotStarted",
                ModuleTitle = moduleTitle,
                Locked = locked
            });
            prevPassed = passed;
        }

        return result;
    }

    private int GetNodeXpReward(string? sandboxType, bool hasCodeEx = false, bool hasQuizEx = false)
    {
        if (sandboxType == "codelab" || hasCodeEx)
        {
            return _configService.GetCodelabBaseXp();
        }
        if (sandboxType == "quiz" || hasQuizEx)
        {
            return _configService.GetQuizBaseXp();
        }
        return _configService.GetTheoryBaseXp();
    }

    private int GetNodeXpReward(LearningPathNode node)
    {
        if (node.ItemType == PathItemType.Lab || node.LabExerciseId != null)
        {
            return _configService.GetCodelabBaseXp();
        }
        if (node.ItemType == PathItemType.Quiz || node.FinalTestId != null)
        {
            return _configService.GetQuizBaseXp();
        }
        return _configService.GetTheoryBaseXp();
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
        public List<SimulationRefDto> Simulations { get; set; } = [];
        public List<string> SimulationKeys { get; set; } = [];
        public int XpReward { get; set; }
        public int OrderIndex { get; set; }
        public string Status { get; set; } = "NotStarted";
        public int LastActiveFrameIndex { get; set; }
        public int LastScrollPercent { get; set; }
        public string? LastSubmittedCode { get; set; }
        public LastQuizSubmissionDto? LastQuizSubmission { get; set; }
    }

    public sealed class LastQuizSubmissionDto
    {
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public bool Passed { get; set; }
        public string? AnswersJson { get; set; }
        public string? ResultJson { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    [HttpGet("lessons/{id:int}")]
    public async Task<ActionResult<LessonDetailResponse>> GetLesson(
        [FromRoute] int id,
        [FromQuery] int? courseId = null,
        CancellationToken ct = default)
    {
        var userId = CurrentUserId();
        var role = TryGetCurrentRole();
        LearningPathNode? node = null;
        if (courseId.HasValue && courseId.Value > 0)
        {
            node = await _db.LearningPathNodes.AsNoTracking()
                .FirstOrDefaultAsync(n => n.PathId == courseId.Value && (n.LessonId == id || n.Id == id), ct);
        }
        if (node is null)
        {
            node = await _db.LearningPathNodes.AsNoTracking()
                .FirstOrDefaultAsync(n => n.LessonId == id, ct);
        }
        if (node is null)
        {
            node = await _db.LearningPathNodes.AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id, ct);
        }
        if (node is null)
        {
            var soloLesson = await _db.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null, ct);
            if (soloLesson != null)
            {
                var fallbackPath = await _db.LearningPaths.AsNoTracking().FirstOrDefaultAsync(p => p.TopicId == soloLesson.TopicId, ct)
                                   ?? await _db.LearningPaths.AsNoTracking().FirstOrDefaultAsync(ct);
                node = new LearningPathNode
                {
                    Id = soloLesson.Id,
                    PathId = fallbackPath?.Id ?? 0,
                    Title = soloLesson.Title,
                    LessonId = soloLesson.Id,
                    ItemType = PathItemType.Theory,
                    SortOrder = soloLesson.SortOrder > 0 ? soloLesson.SortOrder : 1
                };
            }
        }
        if (node is null)
        {
            return NotFound(new { message = "Bài học không tồn tại." });
        }

        var path = await _db.LearningPaths.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == node.PathId, ct);

        var isOwnerOrTeacher = role == "ADMIN" || (role == "TEACHER" && path != null && (path.CreatedBy == userId || path.AuthorId == userId));

        var isEnrolledViaClass = path != null && await _db.Classes.AsNoTracking()
            .Where(c => c.LearningPathId == path.Id && c.DeletedAt == null)
            .Join(_db.ClassMembers.AsNoTracking(), c => c.Id, m => m.ClassId, (c, m) => m.UserId)
            .AnyAsync(uId => uId == userId, ct);

        // Chặn học sinh/khách xem bài học nếu khóa học đang là bản nháp (trừ khi là thành viên lớp được gán lộ trình).
        // Fix A3: chế độ "Lớp học" (ClassOnly) phải học được — chỉ chặn bản nháp/tạm ẩn khi không thuộc lớp.
        if (!isOwnerOrTeacher && !isEnrolledViaClass && path != null && path.Status != LearningPathStatus.Active && path.Status != LearningPathStatus.ClassOnly)
        {
            return NotFound(new { message = "Khóa học hiện đang ở chế độ bản nháp hoặc tạm ẩn." });
        }

        // Khoá tuần tự: node chưa mở → 403 (học xong bài trước mới vào được bài sau); Bypass cho giáo viên/admin/chủ khóa học
        if (!isOwnerOrTeacher && await IsNodeLockedAsync(userId, node, ct))
        {
            return StatusCode(403, new { message = "Bài học chưa được mở khóa — hãy hoàn thành bài học trước." });
        }

        Lesson? lesson = null;
        if (node.LessonId is { } lessonId)
        {
            lesson = await _db.Lessons.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
        }
        else if (node.ItemType == PathItemType.Theory)
        {
            var dbNode = await _db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == node.Id, ct);
            if (dbNode != null && dbNode.LessonId == null)
            {
                var newLesson = new Lesson
                {
                    TopicId = path?.TopicId ?? 1,
                    Title = dbNode.Title,
                    Description = dbNode.Description,
                    ContentHtml = "",
                    Status = LessonStatus.Active,
                    CreatedBy = path?.CreatedBy ?? userId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Lessons.Add(newLesson);
                await _db.SaveChangesAsync(ct);
                dbNode.LessonId = newLesson.Id;
                await _db.SaveChangesAsync(ct);
                lesson = newLesson;
            }
        }

        var exercises = await _db.Exercises.AsNoTracking()
            .Where(e => (e.NodeId == node.Id 
                      || (node.FinalTestId != null && e.Id == node.FinalTestId.Value)
                      || (node.LabExerciseId != null && e.Id == node.LabExerciseId.Value)
                      || (node.LessonId != null && e.LessonId == node.LessonId.Value && (e.NodeId == null || e.NodeId == node.Id)))
                      && e.DeletedAt == null)
            .ToListAsync(ct);

        var quizEx = (node.FinalTestId != null ? exercises.FirstOrDefault(e => e.Id == node.FinalTestId.Value) : null)
            ?? exercises.FirstOrDefault(e => e.Type == ExerciseType.Mcq);
        var codeEx = (node.LabExerciseId != null ? exercises.FirstOrDefault(e => e.Id == node.LabExerciseId.Value) : null)
            ?? exercises.FirstOrDefault(e => e.Type == ExerciseType.Code);
        var labEx = exercises.FirstOrDefault(e => e.Type == ExerciseType.SimulationLab);

        var simulations = lesson != null
            ? await _db.LessonSimulations.AsNoTracking()
                .Where(s => s.LessonId == lesson.Id)
                .OrderBy(s => s.SortOrder)
                .Select(s => new SimulationRefDto { SimulationKey = s.SimulationKey, Title = s.Title })
                .ToListAsync(ct)
            : [];
        var simKeys = simulations.Select(s => s.SimulationKey).ToList();

        string sandboxType = "dsa";
        string sandboxConfig = string.Empty;

        var titleLower = ((lesson?.Title ?? node.Title) ?? string.Empty).ToLowerInvariant();
        var isQuizNaming = titleLower.Contains("quiz") || titleLower.Contains("quizz") || titleLower.Contains("trắc nghiệm") || titleLower.Contains("kiểm tra");
        var isLabNaming = titleLower.Contains("assignment") || titleLower.Contains("lab") || titleLower.Contains("thực hành") || titleLower.Contains("bài tập");

        if (node.ItemType == PathItemType.Quiz || (quizEx != null && !isLabNaming) || isQuizNaming)
        {
            sandboxType = "quiz";
            sandboxConfig = quizEx != null ? $"{{ \"quizId\": {quizEx.Id} }}" : string.Empty;
        }
        else if (node.ItemType == PathItemType.Lab || (codeEx != null && !isQuizNaming) || isLabNaming)
        {
            sandboxType = "codelab";
            sandboxConfig = codeEx?.ConfigJson ?? (codeEx != null ? $"{{\"exerciseId\": {codeEx.Id}}}" : string.Empty);
        }
        else if (node.ItemType == PathItemType.Theory)
        {
            sandboxType = "dsa";
            if (simKeys.Count > 0)
            {
                sandboxConfig = JsonSerializer.Serialize(new { simulationKeys = simKeys, simulationKey = simKeys[0] });
            }
            else if (labEx is not null)
            {
                sandboxConfig = $"{{ \"simulationKey\": \"{TryReadSimulationKey(labEx.ConfigJson ?? string.Empty)}\" }}";
            }
        }
        else if (codeEx is not null)
        {
            sandboxType = "codelab";
            sandboxConfig = codeEx.ConfigJson ?? $"{{\"exerciseId\": {codeEx.Id}}}";
        }
        else if (quizEx is not null)
        {
            sandboxType = "quiz";
            sandboxConfig = $"{{ \"quizId\": {quizEx.Id} }}";
        }
        else if (labEx is not null)
        {
            sandboxType = "dsa";
            sandboxConfig = $"{{ \"simulationKey\": \"{TryReadSimulationKey(labEx.ConfigJson ?? string.Empty)}\" }}";
        }

        if (simKeys.Count > 0 && string.IsNullOrEmpty(sandboxConfig))
        {
            sandboxConfig = JsonSerializer.Serialize(new { simulationKeys = simKeys, simulationKey = simKeys[0] });
        }

        var passed = await _db.UserNodeProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.NodeId == node.Id && p.Status == 2, ct);

        LastQuizSubmissionDto? lastQuizSub = null;
        if (quizEx != null && userId > 0)
        {
            var latestSub = await _db.ExerciseSubmissions.AsNoTracking()
                .Where(s => s.UserId == userId && s.ExerciseId == quizEx.Id)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync(ct);
            if (latestSub != null)
            {
                var qCount = await _db.Questions.AsNoTracking().CountAsync(q => q.ExerciseId == quizEx.Id, ct);
                var maxQ = qCount > 0 ? qCount : Math.Max(1, latestSub.Score);
                lastQuizSub = new LastQuizSubmissionDto
                {
                    Score = latestSub.Score,
                    MaxScore = maxQ,
                    Passed = maxQ > 0 ? (latestSub.Score * 1.0 / maxQ >= 0.7) : (latestSub.Score > 0),
                    AnswersJson = latestSub.AnswersJson,
                    ResultJson = latestSub.ResultJson,
                    SubmittedAt = latestSub.SubmittedAt
                };
            }
        }

        string? lastCode = null;
        if (codeEx != null && userId > 0)
        {
            var latestCodeSub = await _db.CodeSubmissions.AsNoTracking()
                .Where(s => s.UserId == userId && s.ExerciseId == codeEx.Id)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync(ct);
            if (latestCodeSub != null && !string.IsNullOrWhiteSpace(latestCodeSub.Code))
            {
                lastCode = latestCodeSub.Code;
            }
        }

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
            ExerciseId = (codeEx?.Id ?? quizEx?.Id)?.ToString(),
            Simulations = simulations,
            SimulationKeys = simKeys,
            XpReward = GetNodeXpReward(sandboxType, codeEx != null, quizEx != null),
            OrderIndex = node.SortOrder,
            Status = passed ? "Completed" : "NotStarted",
            LastSubmittedCode = lastCode,
            LastQuizSubmission = lastQuizSub
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
        public string Type { get; set; } = "SINGLE";
        public List<string> Options { get; set; } = [];
        public int? CorrectIndex { get; set; }
        public List<int>? CorrectIndices { get; set; }
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

        var role = TryGetCurrentRole();
        var isStaff = role == "ADMIN" || role == "TEACHER";

        var detail = new ConceptsQuizDetail
        {
            Id = exercise.Id.ToString(),
            Title = exercise.Title,
            Topic = "dsa",
            Difficulty = "Medium",
            XpReward = exercise.MaxScore > 0 ? exercise.MaxScore : _configService.GetQuizBaseXp(),
            Questions = exercise.Questions
                .OrderBy(q => q.SortOrder)
                .Select(q =>
                {
                    var correctIndices = DeserializeCorrectIndices(q.AnswerJson);
                    return new ConceptsQuizQuestion
                    {
                        Id = q.Id.ToString(),
                        Text = q.Content,
                        Type = q.Type == QuestionType.Multi ? "MULTIPLE" : "SINGLE",
                        Options = DeserializeOptions(q.OptionsJson),
                        CorrectIndex = isStaff ? (correctIndices.Count > 0 ? correctIndices[0] : 0) : null,
                        CorrectIndices = isStaff ? correctIndices : null,
                        Explanation = isStaff ? (q.Explanation ?? string.Empty) : string.Empty
                    };
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
        var indices = DeserializeCorrectIndices(answerJson);
        return indices.Count > 0 ? indices[0] : 0;
    }

    private static List<int> DeserializeCorrectIndices(string answerJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(answerJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var list = new List<int>();
                foreach (var elem in doc.RootElement.EnumerateArray())
                {
                    if (elem.TryGetInt32(out var idx))
                    {
                        list.Add(idx);
                    }
                }
                return list;
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Number && doc.RootElement.TryGetInt32(out var singleVal))
            {
                return [singleVal];
            }
        }
        catch
        {
            // fallthrough
        }

        return [];
    }

    public sealed class QuizSubmitRequest
    {
        public string QuizId { get; set; } = string.Empty;
        public List<JsonElement> Answers { get; set; } = [];
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
        public List<int> CorrectIndices { get; set; } = [];
        public string Explanation { get; set; } = string.Empty;
    }

    [HttpPost("quiz/submit")]
    public async Task<ActionResult<QuizAttemptResult>> SubmitQuiz([FromBody] QuizSubmitRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var role = TryGetCurrentRole();
        if (role == "TEACHER")
        {
            return StatusCode(403, new { message = "Giảng viên chỉ được xem lộ trình, không được hoàn thành bài thay học viên." });
        }
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
            var correctIndices = DeserializeCorrectIndices(q.AnswerJson);
            var selectedIndices = new List<int>();

            if (request.Answers.Count > i)
            {
                var ansElem = request.Answers[i];
                if (ansElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in ansElem.EnumerateArray())
                    {
                        if (item.TryGetInt32(out var idx))
                        {
                            selectedIndices.Add(idx);
                        }
                    }
                }
                else if (ansElem.ValueKind == JsonValueKind.Number && ansElem.TryGetInt32(out var singleIdx))
                {
                    selectedIndices.Add(singleIdx);
                }
            }

            bool isCorrect;
            if (q.Type == QuestionType.Multi)
            {
                isCorrect = selectedIndices.Count > 0 && selectedIndices.ToHashSet().SetEquals(correctIndices);
            }
            else
            {
                isCorrect = selectedIndices.Count == 1 && (correctIndices.Count == 0 || selectedIndices[0] == correctIndices[0]);
            }

            if (isCorrect)
            {
                score++;
            }

            results.Add(new QuizQuestionResult
            {
                QuestionId = q.Id.ToString(),
                IsCorrect = isCorrect,
                CorrectIndex = correctIndices.Count > 0 ? correctIndices[0] : 0,
                CorrectIndices = correctIndices,
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
            var isFirstPass = existing is null || existing.Status != 2;
            if (existing is null)
            {
                _db.UserNodeProgress.Add(new UserNodeProgress
                {
                    UserId = userId,
                    NodeId = nodeId,
                    Status = 2,
                    Stars = Math.Clamp((int)Math.Ceiling(score * 3.0 / maxScore), 0, 3),
                    NodeScore = score,
                    PassedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else if (existing.Status != 2)
            {
                existing.Status = 2;
                existing.Stars = Math.Clamp((int)Math.Ceiling(score * 3.0 / maxScore), 0, 3);
                existing.NodeScore = score;
                existing.PassedAt = DateTime.UtcNow;
                existing.UpdatedAt = DateTime.UtcNow;
                _db.UserNodeProgress.Update(existing);
            }

            if (isFirstPass)
            {
                var baseQuizXp = _configService.GetQuizBaseXp();
                var xpEarned = maxScore > 0 ? (int)Math.Round((double)score / maxScore * baseQuizXp) : baseQuizXp;
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Users SET Xp = Xp + {xpEarned}, UpdatedAt = {DateTime.UtcNow} WHERE Id = {userId}", ct);
            }
        }

        await _db.SaveChangesAsync(ct);

        var baseQuizXpAward = _configService.GetQuizBaseXp();
        var xpAwarded = passed ? (maxScore > 0 ? (int)Math.Round((double)score / maxScore * baseQuizXpAward) : baseQuizXpAward) : 0;

        return Ok(new QuizAttemptResult
        {
            Score = score,
            MaxScore = maxScore,
            Passed = passed,
            XpAwarded = xpAwarded,
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
                totalXp = GetNodeXpReward(node)
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

        var path = await _db.LearningPaths.AsNoTracking().FirstOrDefaultAsync(p => p.Id == node.PathId, ct);
        var role = TryGetCurrentRole();
        if (role == "TEACHER")
        {
            return StatusCode(403, new { message = "Giảng viên chỉ được xem lộ trình, không được hoàn thành bài thay học viên." });
        }
        var isOwnerOrTeacher = role == "ADMIN";

        var isEnrolledViaClass = path != null && await _db.Classes.AsNoTracking()
            .Where(c => c.LearningPathId == path.Id && c.DeletedAt == null)
            .Join(_db.ClassMembers.AsNoTracking(), c => c.Id, m => m.ClassId, (c, m) => m.UserId)
            .AnyAsync(uId => uId == userId, ct);

        // Fix A3: chế độ "Lớp học" (ClassOnly) cho học viên trong lớp ghi nhận tiến độ —
        // trước đây chỉ Active được ghi → khóa lớp học xong bài nhưng unlock không bao giờ cập nhật.
        if (!isOwnerOrTeacher && !isEnrolledViaClass && path != null && path.Status != LearningPathStatus.Active && path.Status != LearningPathStatus.ClassOnly)
        {
            return StatusCode(403, new { message = "Khóa học hiện đang ở chế độ bản nháp hoặc tạm ẩn, không thể ghi nhận tiến độ." });
        }

        // Khoá tuần tự: không cho ghi tiến độ node CHƯA MỞ (chống bypass — học viên chỉ sync
        // bài đang học, bài đó luôn mở khoá; node đã pass vẫn ghi được bình thường).
        if (!isOwnerOrTeacher && await IsNodeLockedAsync(userId, node, ct))
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
                    CompletedAt = (payload.Completed == true || payload.CodelabCompleted) ? now : null,
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

                if ((payload.Completed == true || payload.CodelabCompleted) && progress.CompletedAt is null)
                {
                    progress.CompletedAt = now;
                }

                progress.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Node pass khi "Hoàn thành bài học" (cờ Completed hoặc CodelabCompleted)
        if (payload.Completed == true || payload.CodelabCompleted == true)
        {
            // Nghiệp vụ Quiz: Không cho phép hoàn thành nếu chưa đạt tối thiểu 70%
            if (node.ItemType == PathItemType.Quiz && node.FinalTestId.HasValue)
            {
                var qCount = await _db.Questions.AsNoTracking().CountAsync(q => q.ExerciseId == node.FinalTestId.Value, ct);
                var bestSub = await _db.ExerciseSubmissions.AsNoTracking()
                    .Where(s => s.UserId == userId && s.ExerciseId == node.FinalTestId.Value)
                    .OrderByDescending(s => s.Score)
                    .FirstOrDefaultAsync(ct);
                var quizPassed = bestSub != null && qCount > 0 && (bestSub.Score * 1.0 / qCount >= 0.7);
                if (!quizPassed)
                {
                    return BadRequest(new { message = "Điểm bài trắc nghiệm chưa đạt yêu cầu (cần tối thiểu 70%) để hoàn thành." });
                }
            }

            // Nghiệp vụ Codelab (fix blind-trust SaveProgress): node có exercise CODE đính kèm →
            // CHỈ được pass khi có bài nộp code PASS toàn bộ test phía MÁY CHỦ (mirror nhánh Quiz 70%
            // ở trên — không tin cờ CodelabCompleted/Completed từ client: POST thẳng API mà chưa từng
            // nộp code vẫn pass node + cộng XP).
            var codelabExerciseId = await _db.Exercises.AsNoTracking()
                .Where(e => e.DeletedAt == null
                    && e.Type == ExerciseType.Code
                    && (e.NodeId == node.Id
                        || (node.LabExerciseId != null && e.Id == node.LabExerciseId.Value)
                        || (node.LessonId != null && e.LessonId == node.LessonId.Value && (e.NodeId == null || e.NodeId == node.Id))))
                .OrderByDescending(e => e.Id)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync(ct);
            if (codelabExerciseId is { } labCodeId)
            {
                var bestCodeSub = await _db.CodeSubmissions.AsNoTracking()
                    .Where(s => s.UserId == userId && s.ExerciseId == labCodeId)
                    .OrderByDescending(s => s.PassedTests)
                    .FirstOrDefaultAsync(ct);
                var codePassed = bestCodeSub is not null
                    && bestCodeSub.TotalTests > 0
                    && bestCodeSub.PassedTests >= bestCodeSub.TotalTests;
                if (!codePassed)
                {
                    return BadRequest(new { message = "Bài thực hành code chưa đạt (cần pass toàn bộ test case trên máy chủ) để hoàn thành." });
                }
            }
            var nodeProgress = await _db.UserNodeProgress
                .FirstOrDefaultAsync(p => p.UserId == userId && p.NodeId == id, ct);
            var isFirstPass = nodeProgress is null || nodeProgress.Status != 2;
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

            if (isFirstPass)
            {
                var xpEarned = GetNodeXpReward(node);
                if (_db.Database.IsRelational())
                {
                    await _db.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE Users SET Xp = Xp + {xpEarned}, UpdatedAt = {DateTime.UtcNow} WHERE Id = {userId}", ct);
                }
                else
                {
                    // InMemory (unit test) không chạy raw SQL — cập nhật qua EF, giống ExerciseService.
                    var user = await _db.Users.FindAsync([userId], ct);
                    if (user is not null)
                    {
                        user.Xp += xpEarned;
                        user.UpdatedAt = DateTime.UtcNow;
                    }
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
    [Authorize]
    public async Task<ActionResult<object>> AwardXp([FromBody] AwardXpRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId();
        if (request.Amount <= 0 || request.Amount > 1000)
        {
            return BadRequest(new { message = "Số XP không hợp lệ." });
        }

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET Xp = Xp + {request.Amount}, UpdatedAt = {DateTime.UtcNow} WHERE Id = {userId}", ct);

        var newXp = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Xp)
            .FirstOrDefaultAsync(ct);

        return Ok(new { success = true, xp = newXp, amount = request.Amount });
    }
}
