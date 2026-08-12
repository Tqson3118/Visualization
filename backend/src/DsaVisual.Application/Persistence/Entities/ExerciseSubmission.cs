namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Bài nộp — SDD §7.3.10. ResultJson lưu đầy đủ để tái hiện màn kết quả (ADR-010).</summary>
public sealed class ExerciseSubmission
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ExerciseId { get; set; }
    public int? ClassAssignmentId { get; set; }                      // nộp qua luồng lớp (v2.8, FR-8.3)
    public int Score { get; set; }
    public string AnswersJson { get; set; } = string.Empty;
    public string ResultJson { get; set; } = string.Empty;
    public int? DurationSeconds { get; set; }
    public DateTime SubmittedAt { get; set; }
}
