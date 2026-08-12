using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Validators;
using FluentValidation;
using FluentValidation.Results;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// Service mẫu theo SDD §5.7.2: DbContext trực tiếp (KHÔNG Repository), Result&lt;T&gt;,
/// sanitize HTML bằng Ganss.Xss, FluentValidation gọi ở Service (SDD §5.3.4), ILogger.
/// </summary>
public sealed class LessonService(
    AppDbContext db,
    IValidator<LessonUpsertRequest> validator,
    IValidator<LessonFeedbackRequest> feedbackValidator,
    IHtmlSanitizer htmlSanitizer,
    IDateTimeProvider clock,
    ILogger<LessonService> logger) : ILessonService
{
    private const string RoleTeacher = "TEACHER";
    private const string RoleAdmin = "ADMIN";

    public async Task<Result<PagedResponse<LessonSummaryDto>>> GetListAsync(
        int userId, string role, int? topicId, string? status, string? q,
        int page, int pageSize, CancellationToken ct)
    {
        var (safePage, safeSize) = Pagination.Normalize(page, pageSize);
        bool isStudent = !IsTeacherOrAdmin(role);

        var query = db.Lessons
            .AsNoTracking()
            .Where(l => l.DeletedAt == null);

        if (isStudent)
        {
            // Student chỉ nhận bài active (SDD §5.7.1 / API_REFERENCE §3.4)
            query = query.Where(l => l.Status == LessonStatus.Active);
        }
        else if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LessonStatus>(status, true, out var statusFilter))
        {
            query = query.Where(l => l.Status == statusFilter);
        }

        if (topicId is > 0)
        {
            query = query.Where(l => l.TopicId == topicId);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var keyword = q.Trim();
            query = query.Where(l => l.Title.Contains(keyword) || (l.Description != null && l.Description.Contains(keyword)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Id)
            .Skip((safePage - 1) * safeSize).Take(safeSize)
            .Select(l => new LessonSummaryDto
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                TopicId = l.TopicId,
                SortOrder = l.SortOrder,
                Status = l.Status.ToString().ToLowerInvariant(),
                SimulationCount = l.LessonSimulations.Count,
                ExerciseCount = l.Exercises.Count(e => e.DeletedAt == null),
                Progress = isStudent
                    ? db.UserProgress.AsNoTracking()
                        .Where(p => p.UserId == userId && p.LessonId == l.Id)
                        .Select(p => new LessonSummaryProgressDto
                        {
                            Viewed = p.Viewed,
                            BestScore = p.BestScore,
                            Completed = p.CompletedAt != null
                        })
                        .FirstOrDefault()
                    : null
            })
            .ToListAsync(ct);

        return Result<PagedResponse<LessonSummaryDto>>.Ok(
            PagedResponse<LessonSummaryDto>.Create(items, safePage, safeSize, total, Pagination.TotalPages(total, safeSize)));
    }

    public async Task<Result<LessonDto>> GetByIdAsync(int userId, string role, int id, bool includeContent, CancellationToken ct)
    {
        var lesson = await db.Lessons
            .AsNoTracking()
            .Include(l => l.LessonSimulations)
            .Include(l => l.Exercises.Where(e => e.DeletedAt == null))
            .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null, ct);

        if (lesson is null)
        {
            return Result<LessonDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        if (!IsTeacherOrAdmin(role) && lesson.Status != LessonStatus.Active)
        {
            // Student không được xem bản nháp/ẩn
            return Result<LessonDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        return Result<LessonDto>.Ok(ToDto(lesson, includeContent: includeContent || IsTeacherOrAdmin(role)));
    }

    public async Task<Result<LessonDto>> CreateAsync(int userId, LessonUpsertRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<LessonDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Dữ liệu bài học không hợp lệ", ToFieldErrors(validation));
        }

        var topic = await db.Topics.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.TopicId && t.DeletedAt == null, ct);
        if (topic is null)
        {
            return Result<LessonDto>.Fail(ErrorCodes.NOT_FOUND, "Chủ đề không tồn tại");
        }

        var sanitized = htmlSanitizer.Sanitize(request.ContentHtml);
        if (sanitized.Length < 10)
        {
            return Result<LessonDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Nội dung bài học quá ngắn");
        }

        var lesson = new Lesson
        {
            TopicId = request.TopicId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            ContentHtml = sanitized,
            Status = request.Status,
            SortOrder = request.SortOrder,
            CreatedBy = userId,
            CreatedAt = clock.UtcNow
        };

        db.Lessons.Add(lesson);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Lesson {LessonId} created by user {UserId}", lesson.Id, userId);
        return Result<LessonDto>.Ok(ToDto(lesson, includeContent: true));
    }

    public async Task<Result<LessonDto>> UpdateAsync(int userId, string role, int id, LessonUpsertRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<LessonDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Dữ liệu bài học không hợp lệ", ToFieldErrors(validation));
        }

        var lesson = await db.Lessons
            .Include(l => l.LessonSimulations)
            .Include(l => l.Exercises.Where(e => e.DeletedAt == null))
            .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null, ct);

        if (lesson is null)
        {
            return Result<LessonDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        if (!CanManage(userId, role, lesson))
        {
            return Result<LessonDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền sửa bài học này");
        }

        var topic = await db.Topics.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.TopicId && t.DeletedAt == null, ct);
        if (topic is null)
        {
            return Result<LessonDto>.Fail(ErrorCodes.NOT_FOUND, "Chủ đề không tồn tại");
        }

        var sanitized = htmlSanitizer.Sanitize(request.ContentHtml);
        if (sanitized.Length < 10)
        {
            return Result<LessonDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Nội dung bài học quá ngắn");
        }

        lesson.TopicId = request.TopicId;
        lesson.Title = request.Title.Trim();
        lesson.Description = request.Description?.Trim();
        lesson.ContentHtml = sanitized;
        lesson.Status = request.Status;
        lesson.SortOrder = request.SortOrder;
        lesson.UpdatedBy = userId;
        lesson.UpdatedAt = clock.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Lesson {LessonId} updated by user {UserId}", lesson.Id, userId);
        return Result<LessonDto>.Ok(ToDto(lesson, includeContent: true));
    }

    public async Task<Result> DeleteAsync(int userId, string role, int id, CancellationToken ct)
    {
        var lesson = await db.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null, ct);

        if (lesson is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        if (!CanManage(userId, role, lesson))
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xóa bài học này");
        }

        var hasActiveExercises = await db.Exercises
            .AsNoTracking()
            .AnyAsync(e => e.LessonId == id && e.Status == ExerciseStatus.Active && e.DeletedAt == null, ct);

        if (hasActiveExercises)
        {
            return Result.Fail(ErrorCodes.LESSON_HAS_EXERCISES, "Không xóa được bài học còn bài tập đang hoạt động");
        }

        // Xóa mềm (D-5)
        var tracked = await db.Lessons.FirstAsync(l => l.Id == id, ct);
        tracked.DeletedAt = clock.UtcNow;
        tracked.UpdatedAt = clock.UtcNow;
        tracked.UpdatedBy = userId;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Lesson {LessonId} soft-deleted by user {UserId}", id, userId);
        return Result.Ok();
    }

    public async Task<Result> MarkViewedAsync(int userId, string role, int lessonId, CancellationToken ct)
    {
        var lesson = await db.Lessons.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
        if (lesson is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        if (!IsTeacherOrAdmin(role) && lesson.Status != LessonStatus.Active)
        {
            // Student không được đánh dấu bản nháp/ẩn
            return Result.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        var now = clock.UtcNow;
        var progress = await db.UserProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);

        // Upsert UserProgress — 1 bản ghi/1 (User, Lesson); lần 2 chỉ update Viewed/UpdatedAt (TEST-B-033/034)
        if (progress is null)
        {
            db.UserProgress.Add(new UserProgress
            {
                UserId = userId,
                LessonId = lessonId,
                Viewed = true,
                UpdatedAt = now
            });
        }
        else
        {
            progress.Viewed = true;
            progress.UpdatedAt = now;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} marked lesson {LessonId} as viewed", userId, lessonId);
        return Result.Ok();
    }

    public async Task<Result<FeedbackSavedDto>> AddFeedbackAsync(
        int userId, string role, int lessonId, LessonFeedbackRequest request, CancellationToken ct)
    {
        var validation = await feedbackValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return Result<FeedbackSavedDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Dữ liệu đánh giá không hợp lệ", ToFieldErrors(validation));
        }

        var lesson = await db.Lessons.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
        if (lesson is null)
        {
            return Result<FeedbackSavedDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        if (!IsTeacherOrAdmin(role) && lesson.Status != LessonStatus.Active)
        {
            // Student không được đánh giá bản nháp/ẩn
            return Result<FeedbackSavedDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        // FR-7.4 (v2.9 — API_REFERENCE §4.15): chỉ đánh giá sau khi đã "Đánh dấu đã học" bài đó
        var viewed = await db.UserProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.LessonId == lessonId && p.Viewed, ct);
        if (!viewed)
        {
            return Result<FeedbackSavedDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn cần học bài này trước khi đánh giá");
        }

        var comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();
        var now = clock.UtcNow;
        var feedback = await db.ContentFeedback
            .FirstOrDefaultAsync(f => f.UserId == userId && f.LessonId == lessonId, ct);

        // Upsert ContentFeedback — 1 bản ghi/1 (User, Lesson) (UNIQUE IX_ContentFeedback_UserId_LessonId);
        // lần 2 chỉ update Rating/Comment (TEST-B-076/077)
        if (feedback is null)
        {
            db.ContentFeedback.Add(new ContentFeedback
            {
                UserId = userId,
                LessonId = lessonId,
                Rating = request.Rating,
                Comment = comment,
                CreatedAt = now
            });
        }
        else
        {
            feedback.Rating = request.Rating;
            feedback.Comment = comment;
            feedback.UpdatedAt = now;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} submitted feedback for lesson {LessonId} (rating {Rating})", userId, lessonId, request.Rating);
        return Result<FeedbackSavedDto>.Ok(new FeedbackSavedDto { LessonId = lessonId, Rating = request.Rating });
    }

    // ── Private ─────────────────────────────────────────────

    private static bool IsTeacherOrAdmin(string role) =>
        role.Equals(RoleTeacher, StringComparison.OrdinalIgnoreCase) ||
        role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase);

    private static bool CanManage(int userId, string role, Lesson lesson) =>
        role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase) || lesson.CreatedBy == userId;

    private static Dictionary<string, string[]> ToFieldErrors(ValidationResult result) =>
        result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

    private static LessonDto ToDto(Lesson lesson, bool includeContent) => new()
    {
        Id = lesson.Id,
        TopicId = lesson.TopicId,
        Title = lesson.Title,
        Description = lesson.Description,
        ContentHtml = includeContent ? lesson.ContentHtml : null,
        Status = lesson.Status.ToString().ToLowerInvariant(),
        SortOrder = lesson.SortOrder,
        Simulations = lesson.LessonSimulations
            .OrderBy(s => s.SortOrder)
            .Select(s => new SimulationRefDto { SimulationKey = s.SimulationKey, Title = s.Title })
            .ToList(),
        Exercises = lesson.Exercises
            .OrderBy(e => e.Id)
            .Select(e => new ExerciseRefDto { Id = e.Id, Title = e.Title, Type = (int)e.Type })
            .ToList()
    };
}
