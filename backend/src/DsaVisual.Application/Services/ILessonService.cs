using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// CRUD bài học + sanitize HTML + gắn mô phỏng + quyền sở hữu (SDD §5.4).
/// </summary>
public interface ILessonService
{
    Task<Result<PagedResponse<LessonSummaryDto>>> GetListAsync(
        int userId, string role, int? topicId, string? status, string? q,
        int page, int pageSize, CancellationToken ct);

    Task<Result<LessonDto>> GetByIdAsync(int userId, string role, int id, bool includeContent, CancellationToken ct);

    Task<Result<LessonDto>> CreateAsync(int userId, LessonUpsertRequest request, CancellationToken ct);

    Task<Result<LessonDto>> UpdateAsync(int userId, string role, int id, LessonUpsertRequest request, CancellationToken ct);

    Task<Result> DeleteAsync(int userId, string role, int id, CancellationToken ct);

    /// <summary>Đánh dấu đã học — upsert UserProgress (Viewer=true), không trùng bản ghi (TEST-B-033/034).</summary>
    Task<Result> MarkViewedAsync(int userId, string role, int lessonId, CancellationToken ct);
}
