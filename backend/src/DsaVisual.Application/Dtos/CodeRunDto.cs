using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Dtos;

/// <summary>Lần chạy code — GET /code-runs/{id} (API_REFERENCE.md §4.13).</summary>
public sealed class CodeRunDto
{
    public int Id { get; set; }
    public int? ExerciseId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string Status { get; set; } = nameof(CodeRunStatus.Success);
    public int DurationMs { get; set; }
    public CodeRunStatsDto? Stats { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
}
