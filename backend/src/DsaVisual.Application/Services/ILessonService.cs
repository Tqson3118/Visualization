using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// CRUD bài học + sanitize HTML + gắn mô phỏng + quyền sở hữu (SDD §5.4).
/// v2.15: luồng kiểm duyệt (PendingReview → Active) + báo cáo vi phạm.
/// </summary>
public interface ILessonService
{
    Task<Result<PagedResponse<LessonSummaryDto>>> GetListAsync(
        int userId, string role, int? topicId, string? status, string? q,
        int page, int pageSize, CancellationToken ct);

    Task<Result<LessonDto>> GetByIdAsync(int userId, string role, int id, bool includeContent, CancellationToken ct);

    Task<Result<LessonDto>> CreateAsync(int userId, string role, LessonUpsertRequest request, CancellationToken ct);

    Task<Result<LessonDto>> UpdateAsync(int userId, string role, int id, LessonUpsertRequest request, CancellationToken ct);

    Task<Result> DeleteAsync(int userId, string role, int id, CancellationToken ct);

    /// <summary>Admin: danh sách bài học chờ duyệt (Status == PendingReview) — v2.15.</summary>
    Task<Result<List<LessonSummaryDto>>> GetPendingListAsync(CancellationToken ct);

    /// <summary>Admin: duyệt (→ Active + PublishedAt) hoặc từ chối (→ Draft + RejectionReason) — v2.15.</summary>
    Task<Result<LessonDto>> ReviewAsync(int userId, int id, LessonReviewRequest request, CancellationToken ct);

    /// <summary>Sinh viên báo cáo bài học vi phạm — lưu BugReports category CONTENT_VIOLATION (v2.15).</summary>
    Task<Result> ReportAsync(int userId, int lessonId, LessonReportRequest request, CancellationToken ct);

    /// <summary>Đánh dấu đã học — upsert UserProgress (Viewer=true), không trùng bản ghi (TEST-B-033/034).</summary>
    Task<Result> MarkViewedAsync(int userId, string role, int lessonId, CancellationToken ct);

    /// <summary>
    /// Gửi/chỉnh đánh giá bài học — upsert ContentFeedback, 1 bản ghi/1 (User, Lesson),
    /// lần 2 chỉ update Rating/Comment (FR-7.4, TEST-B-076/077).
    /// </summary>
    Task<Result<FeedbackSavedDto>> AddFeedbackAsync(
        int userId, string role, int lessonId, LessonFeedbackRequest request, CancellationToken ct);
}
