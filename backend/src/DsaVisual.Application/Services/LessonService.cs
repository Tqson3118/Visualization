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
/// v2.15: luồng kiểm duyệt nội dung — Teacher public bài → PendingReview, Admin duyệt → Active
/// (PublishedAt); IsClassOnly → Active trực tiếp (chỉ lớp học). Sinh viên có nút báo cáo vi phạm.
/// </summary>
public sealed class LessonService(
    AppDbContext db,
    IValidator<LessonUpsertRequest> validator,
    IValidator<LessonFeedbackRequest> feedbackValidator,
    IHtmlSanitizer htmlSanitizer,
    ISimulationCatalogService catalog,
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
            // Student chỉ nhận bài active public (SDD §5.7.1 / API_REFERENCE §3.4) — v2.15:
            // loại bài IsClassOnly (nội bộ lớp, truy cập qua ClassAssignment)
            query = query.Where(l => l.Status == LessonStatus.Active && !l.IsClassOnly);
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
                IsClassOnly = l.IsClassOnly,
                PublishedAt = l.PublishedAt,
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

    public async Task<Result<LessonDto>> CreateAsync(int userId, string role, LessonUpsertRequest request, CancellationToken ct)
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

        // v2.15: phân quyền xuất bản — Admin gán Active trực tiếp; Teacher chỉ PendingReview (public)
        // hoặc Active (IsClassOnly)
        var status = ResolveCreateStatus(role, request.Status, request.IsClassOnly);

        var lesson = new Lesson
        {
            TopicId = request.TopicId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            ContentHtml = sanitized,
            Status = status,
            IsClassOnly = request.IsClassOnly,
            SortOrder = request.SortOrder,
            CreatedBy = userId,
            CreatedAt = clock.UtcNow
        };

        if (status == LessonStatus.Active && !request.IsClassOnly)
        {
            lesson.PublishedAt = clock.UtcNow;
        }

        db.Lessons.Add(lesson);

        var simError = await SyncSimulationsAsync(lesson, request.SimulationKeys, ct);
        if (simError is not null)
        {
            return simError;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Lesson {LessonId} created by user {UserId} (status {Status}, classOnly {ClassOnly})",
            lesson.Id, userId, status, request.IsClassOnly);
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

        // v2.15: chuyển sang Class Only → Active ngay; chuyển sang Public → PendingReview
        // (trừ khi Admin duyệt trực tiếp hoặc giữ nguyên trạng thái Active khi sửa nội dung)
        var previousStatus = lesson.Status;
        var newStatus = ResolveUpdateStatus(role, lesson, request.Status, request.IsClassOnly);
        lesson.Status = newStatus;
        lesson.IsClassOnly = request.IsClassOnly;

        if (newStatus == LessonStatus.Active && previousStatus != LessonStatus.Active && !request.IsClassOnly)
        {
            lesson.PublishedAt = clock.UtcNow;
        }
        else if (newStatus != LessonStatus.Active && !request.IsClassOnly)
        {
            lesson.PublishedAt = null;
        }

        lesson.TopicId = request.TopicId;
        lesson.Title = request.Title.Trim();
        lesson.Description = request.Description?.Trim();
        lesson.ContentHtml = sanitized;
        lesson.SortOrder = request.SortOrder;
        lesson.UpdatedBy = userId;
        lesson.UpdatedAt = clock.UtcNow;

        var simError = await SyncSimulationsAsync(lesson, request.SimulationKeys, ct);
        if (simError is not null)
        {
            return simError;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Lesson {LessonId} updated by user {UserId} (status {Status}, classOnly {ClassOnly})",
            lesson.Id, userId, newStatus, request.IsClassOnly);
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

    // ── Kiểm duyệt (v2.15) ────────────────────────────────────

    public async Task<Result<List<LessonSummaryDto>>> GetPendingListAsync(CancellationToken ct)
    {
        var items = await db.Lessons
            .AsNoTracking()
            .Where(l => l.Status == LessonStatus.PendingReview && l.DeletedAt == null)
            .OrderBy(l => l.CreatedAt)
            .Select(l => new LessonSummaryDto
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                TopicId = l.TopicId,
                SortOrder = l.SortOrder,
                Status = l.Status.ToString().ToLowerInvariant(),
                IsClassOnly = l.IsClassOnly,
                PublishedAt = l.PublishedAt,
                SimulationCount = l.LessonSimulations.Count,
                ExerciseCount = l.Exercises.Count(e => e.DeletedAt == null)
            })
            .ToListAsync(ct);

        return Result<List<LessonSummaryDto>>.Ok(items);
    }

    public async Task<Result<LessonDto>> ReviewAsync(int userId, int id, LessonReviewRequest request, CancellationToken ct)
    {
        var lesson = await db.Lessons
            .Include(l => l.LessonSimulations)
            .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null, ct);
        if (lesson is null)
        {
            return Result<LessonDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        if (lesson.Status != LessonStatus.PendingReview)
        {
            return Result<LessonDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Bài học không ở trạng thái chờ duyệt", new() { ["id"] = ["Bài học không ở trạng thái chờ duyệt"] });
        }

        if (request.Approve)
        {
            lesson.Status = LessonStatus.Active;
            lesson.PublishedAt = clock.UtcNow;
            lesson.RejectionReason = null;
        }
        else
        {
            lesson.Status = LessonStatus.Draft;
            lesson.RejectionReason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        }

        lesson.UpdatedBy = userId;
        lesson.UpdatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Lesson {LessonId} reviewed by admin {UserId} (approve {Approve}, reason {Reason})",
            id, userId, request.Approve, request.Reason);
        return Result<LessonDto>.Ok(ToDto(lesson, includeContent: true));
    }

    // ── Báo cáo vi phạm (v2.15) ───────────────────────────────

    public async Task<Result> ReportAsync(int userId, int lessonId, LessonReportRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length < 5)
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED,
                "Vui lòng nhập lý do báo cáo (tối thiểu 5 ký tự)", new() { ["reason"] = ["Lý do quá ngắn"] });
        }

        var lesson = await db.Lessons.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
        if (lesson is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        var reason = request.Reason.Trim();
        if (reason.Length > 2000)
        {
            reason = reason[..2000];
        }

        // Lưu vào BugReports với category CONTENT_VIOLATION (trong ContextJson) để Admin xử lý
        db.BugReports.Add(new BugReport
        {
            UserId = userId,
            Description = $"Báo cáo vi phạm bài học #{lessonId} — {lesson.Title}: {reason}",
            ContextJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                type = "CONTENT_VIOLATION",
                lessonId,
                lessonTitle = lesson.Title
            }),
            Status = BugReportStatus.New,
            CreatedAt = clock.UtcNow
        });
        await db.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} reported lesson {LessonId} (reason {Reason})", userId, lessonId, reason);
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

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng UserProgress đổi (RowVersion) giữa lúc đọc và ghi (2 tab mở bài cùng lúc)
            return Result.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

        // Finding #6 (FR-10.3): xem lesson → tăng quest lesson_viewed (atomic, không chặn luồng chính)
        await QuestProgressWriter.IncrementAsync(db, userId, "lesson_viewed", ct);

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

    /// <summary>
    /// v2.15: trạng thái khi TẠO MỚI — Admin gán tùy ý; Teacher: classOnly → Active ngay,
    /// public → PendingReview (phải qua Admin duyệt).
    /// </summary>
    private static LessonStatus ResolveCreateStatus(string role, LessonStatus requested, bool isClassOnly)
    {
        if (IsAdmin(role))
        {
            return requested;
        }

        if (isClassOnly)
        {
            return LessonStatus.Active;
        }

        return requested is LessonStatus.Active or LessonStatus.PendingReview
            ? LessonStatus.PendingReview
            : LessonStatus.Draft;
    }

    /// <summary>
    /// v2.15: trạng thái khi SỬA — Admin gán tùy ý; Teacher: classOnly → Active ngay;
    /// public: giữ Active nếu đang Active (sửa nội dung không gỡ bài đã duyệt), nếu yêu cầu
    /// chuyển trạng thái khác (kể cả Active từ Draft) → PendingReview.
    /// </summary>
    private static LessonStatus ResolveUpdateStatus(string role, Lesson lesson, LessonStatus requested, bool isClassOnly)
    {
        if (IsAdmin(role))
        {
            return requested;
        }

        if (isClassOnly)
        {
            return LessonStatus.Active;
        }

        if (lesson.Status == LessonStatus.Active && requested == LessonStatus.Active)
        {
            return LessonStatus.Active;
        }

        return requested is LessonStatus.Active or LessonStatus.PendingReview
            ? LessonStatus.PendingReview
            : LessonStatus.Draft;
    }

    /// <summary>
    /// Đồng bộ danh sách Simulation đính kèm (multi-select v2.15): thay thế toàn bộ danh sách
    /// cũ; key phải tồn tại trong catalog (SIMULATION_KEY_INVALID nếu không).
    /// </summary>
    private async Task<Result<LessonDto>?> SyncSimulationsAsync(Lesson lesson, IReadOnlyCollection<string> keys, CancellationToken ct)
    {
        var existing = lesson.LessonSimulations.ToDictionary(s => s.SimulationKey, StringComparer.OrdinalIgnoreCase);
        var newKeys = keys.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        if (newKeys.Count > 0)
        {
            var catalogResult = await catalog.GetListAsync(ct);
            if (!catalogResult.IsSuccess)
            {
                return Result<LessonDto>.Fail(catalogResult.ErrorCode!, catalogResult.ErrorMessage!);
            }

            var catalogByKey = catalogResult.Value!
                .ToDictionary(s => s.Key, s => s.Title, StringComparer.OrdinalIgnoreCase);

            var invalid = newKeys.FirstOrDefault(k => !catalogByKey.ContainsKey(k));
            if (invalid is not null)
            {
                return Result<LessonDto>.Fail(ErrorCodes.SIMULATION_KEY_INVALID,
                    $"Mô phỏng '{invalid}' không tồn tại", new() { ["simulationKeys"] = [$"Mô phỏng '{invalid}' không tồn tại"] });
            }

            for (var i = 0; i < newKeys.Count; i++)
            {
                var key = newKeys[i];
                if (existing.TryGetValue(key, out var row))
                {
                    row.SortOrder = i;
                    existing.Remove(key);
                }
                else
                {
                    lesson.LessonSimulations.Add(new LessonSimulation
                    {
                        SimulationKey = key,
                        Title = catalogByKey[key],
                        SortOrder = i
                    });
                }
            }
        }

        // Xóa các mô phỏng không còn trong danh sách mới
        foreach (var removed in existing.Values.ToList())
        {
            lesson.LessonSimulations.Remove(removed);
        }

        return null;
    }

    private static bool IsTeacherOrAdmin(string role) =>
        role.Equals(RoleTeacher, StringComparison.OrdinalIgnoreCase) ||
        role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase);

    private static bool IsAdmin(string role) =>
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
        IsClassOnly = lesson.IsClassOnly,
        RejectionReason = lesson.RejectionReason,
        PublishedAt = lesson.PublishedAt,
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
