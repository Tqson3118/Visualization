using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// Upsert tiến độ, dashboard, báo cáo giảng viên + CSV (SDD §5.4).
/// </summary>
public interface IProgressService
{
    Task<Result<ProgressOverviewDto>> GetMyOverviewAsync(int userId, CancellationToken ct);
    Task<Result<LessonProgressDetailDto>> GetMyLessonAsync(int userId, int lessonId, CancellationToken ct);
    Task<Result<TeacherReportDto>> GetReportAsync(int userId, string role, int lessonId, CancellationToken ct);
    Task<Result<CsvFileDto>> ExportReportCsvAsync(int userId, string role, int lessonId, CancellationToken ct);
}
