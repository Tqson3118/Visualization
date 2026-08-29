using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// CRUD bài tập/câu hỏi, chấm điểm (SINGLE/MULTI/BOOLEAN/Lab), chống nộp trùng, import CSV, nộp code (SDD §5.4/§5.5).
/// </summary>
public interface IExerciseService
{
    Task<Result<PagedResponse<ExerciseSummaryDto>>> GetListAsync(int? lessonId, int? nodeId, int? stage, int? topicId, int? courseId, int? roadmapId, int page, int pageSize, CancellationToken ct);
    Task<Result<ExerciseDto>> GetByIdAsync(int userId, int id, CancellationToken ct);
    Task<Result<ExerciseDto>> CreateAsync(int userId, ExerciseUpsertRequest request, CancellationToken ct);
    Task<Result<ExerciseDto>> UpdateAsync(int userId, string role, int id, ExerciseUpsertRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(int userId, string role, int id, CancellationToken ct);
    Task<Result<SubmitResultDto>> SubmitAsync(int userId, int id, SubmitRequest request, CancellationToken ct);
    Task<Result<ExerciseDto>> PracticeAsync(int userId, int id, CancellationToken ct);
    Task<Result<ImportCsvResultDto>> ImportCsvAsync(int userId, int lessonId, string csvText, CancellationToken ct);
    Task<Result<PagedResponse<SubmissionSummaryDto>>> GetSubmissionsAsync(int userId, string role, int id, int page, int pageSize, CancellationToken ct = default, DateTime? lastSubmittedAt = null, int? lastId = null);
    Task<Result<PagedResponse<SubmissionSummaryDto>>> GetMySubmissionsAsync(int userId, int id, int page, int pageSize, CancellationToken ct = default, DateTime? lastSubmittedAt = null, int? lastId = null);
    Task<Result<CodeSubmitResultDto>> SubmitCodeAsync(int userId, int id, CodeSubmitRequest request, CancellationToken ct);
    Task<Result<PagedResponse<CodeSubmissionSummaryDto>>> GetCodeSubmissionsAsync(int userId, string role, int id, int page, int pageSize, CancellationToken ct = default, DateTime? lastSubmittedAt = null, int? lastId = null);
    Task<Result<PagedResponse<CodeSubmissionSummaryDto>>> GetMyCodeSubmissionsAsync(int userId, int id, int page, int pageSize, CancellationToken ct = default, DateTime? lastSubmittedAt = null, int? lastId = null);
}
