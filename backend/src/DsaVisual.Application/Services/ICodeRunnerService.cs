using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// Lưu CodeRuns, lịch sử nộp + trace (chấm điểm chạy client sandbox — ADR-012, SDD §5.4).
/// </summary>
public interface ICodeRunnerService
{
    Task<Result<CodeRunDto>> SaveRunAsync(int userId, CodeRunRequest request, CancellationToken ct);
    Task<Result<CodeRunDto>> GetByIdAsync(int userId, int id, CancellationToken ct);
    Task<Result<PagedResponse<TraceEventDto>>> GetTraceAsync(int userId, int id, int page, int pageSize, CancellationToken ct);
}
