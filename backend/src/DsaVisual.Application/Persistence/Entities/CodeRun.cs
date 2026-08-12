namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Lượt chạy code (client sandbox, ADR-012) — SDD §7.3.23 (FR-9.2, FR-9.4).</summary>
public sealed class CodeRun
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ExerciseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string InputJson { get; set; } = string.Empty;
    public CodeRunStatus Status { get; set; } = CodeRunStatus.Pending;
    public string? OutputJson { get; set; }
    public string? ErrorJson { get; set; }
    public string? TraceJson { get; set; }                           // TraceEvent[] nén GZIP
    public int DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }                          // dọn sau 30 ngày
}
