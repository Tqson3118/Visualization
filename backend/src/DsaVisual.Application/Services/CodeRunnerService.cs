using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// CodeRunnerService thật theo SDD §7.3.23 (FR-9.2/9.4) + API_REFERENCE.md §4.13.
/// Server KHÔNG chạy code — client sandbox chạy rồi gửi kết quả lên; server chỉ lưu CodeRun
/// (status Success/Error/Timeout, stats, durationMs, traceJson) và trả trace phân trang.
/// </summary>
public sealed class CodeRunnerService(
    AppDbContext db,
    IDateTimeProvider clock,
    ILogger<CodeRunnerService> logger) : ICodeRunnerService
{
    public async Task<Result<CodeRunDto>> SaveRunAsync(int userId, CodeRunRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) && string.IsNullOrWhiteSpace(request.Key))
        {
            return Result<CodeRunDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Thiếu mã nguồn hoặc khóa mô phỏng", new() { ["code"] = ["Thiếu mã nguồn hoặc khóa mô phỏng"] });
        }

        if (!Enum.TryParse<CodeRunStatus>(request.Status, true, out var status))
        {
            status = CodeRunStatus.Success;
        }

        var run = new CodeRun
        {
            UserId = userId,
            ExerciseId = request.ExerciseId,
            Code = request.Code,
            InputJson = request.Input ?? "{}",
            Status = status,
            OutputJson = request.Output,
            ErrorJson = request.Error,
            TraceJson = request.Trace is { Count: > 0 } ? JsonSerializer.Serialize(request.Trace) : null,
            DurationMs = request.DurationMs,
            CreatedAt = clock.UtcNow
        };
        db.CodeRuns.Add(run);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("CodeRun {RunId} saved by user {UserId} ({Key}, {Status}, {DurationMs}ms)",
            run.Id, userId, request.Key, status, request.DurationMs);

        return Result<CodeRunDto>.Ok(ToDto(run, request.Key, request.Stats));
    }

    public async Task<Result<CodeRunDto>> GetByIdAsync(int userId, int id, CancellationToken ct)
    {
        var run = await db.CodeRuns.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (run is null)
        {
            return Result<CodeRunDto>.Fail(ErrorCodes.NOT_FOUND, "Lần chạy code không tồn tại");
        }

        if (run.UserId != userId)
        {
            return Result<CodeRunDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem lần chạy này");
        }

        return Result<CodeRunDto>.Ok(ToDto(run, key: null, stats: null));
    }

    public async Task<Result<PagedResponse<TraceEventDto>>> GetTraceAsync(int userId, int id, int page, int pageSize, CancellationToken ct)
    {
        var (safePage, safeSize) = Pagination.Normalize(page, pageSize);

        var run = await db.CodeRuns.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (run is null)
        {
            return Result<PagedResponse<TraceEventDto>>.Fail(ErrorCodes.NOT_FOUND, "Lần chạy code không tồn tại");
        }

        if (run.UserId != userId)
        {
            return Result<PagedResponse<TraceEventDto>>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem lần chạy này");
        }

        var events = ParseTrace(run.TraceJson);
        var total = events.Count;
        var items = events
            .Skip((safePage - 1) * safeSize).Take(safeSize)
            .ToList();

        return Result<PagedResponse<TraceEventDto>>.Ok(
            PagedResponse<TraceEventDto>.Create(items, safePage, safeSize, total, Pagination.TotalPages(total, safeSize)));
    }

    // ── Private ───────────────────────────────────────────────

    private static List<TraceEventDto> ParseTrace(string? traceJson)
    {
        if (string.IsNullOrWhiteSpace(traceJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<TraceEventDto>>(traceJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static CodeRunDto ToDto(CodeRun run, string? key, CodeRunStatsDto? stats) => new()
    {
        Id = run.Id,
        ExerciseId = run.ExerciseId,
        Key = key ?? string.Empty,
        Code = run.Code,
        Status = run.Status.ToString(),
        DurationMs = run.DurationMs,
        Stats = stats,
        Output = run.OutputJson,
        Error = run.ErrorJson,
        CreatedAt = run.CreatedAt
    };
}
