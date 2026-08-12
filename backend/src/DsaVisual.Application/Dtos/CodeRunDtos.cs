using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Dtos;

/// <summary>Lưu lần chạy code — POST /code-runs (API_REFERENCE.md §4.13, ADR-012 — chạy client).</summary>
public sealed class CodeRunRequest
{
    public int? ExerciseId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Input { get; set; }
    public string Status { get; set; } = nameof(CodeRunStatus.Success);   // Success/Error/Timeout
    public int DurationMs { get; set; }
    public CodeRunStatsDto? Stats { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
    public List<TraceEventDto>? Trace { get; set; }
}

/// <summary>Thống kê lần chạy (comparisons/swaps...).</summary>
public sealed class CodeRunStatsDto
{
    public long? Comparisons { get; set; }
    public long? Swaps { get; set; }
    public long? Steps { get; set; }
}

/// <summary>Sự kiện trace (TraceEvent[]) — lưu TraceJson dạng JSON array.</summary>
public sealed class TraceEventDto
{
    public int Index { get; set; }
    public string? Type { get; set; }
    public string? Message { get; set; }
    public string? Data { get; set; }
}
