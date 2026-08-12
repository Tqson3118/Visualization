namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Bài nộp code chấm điểm (test ẩn, client sandbox) — SDD §7.3.24 (FR-9.3, FR-9.5).</summary>
public sealed class CodeSubmission
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ExerciseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Score { get; set; }
    public int PassedTests { get; set; }
    public int TotalTests { get; set; }
    public string ResultJson { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
