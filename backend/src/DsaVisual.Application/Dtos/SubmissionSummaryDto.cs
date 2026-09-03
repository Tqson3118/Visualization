namespace DsaVisual.Application.Dtos;

/// <summary>Bài nộp trong lịch sử — GET /exercises/{id}/submissions(me) (API_REFERENCE.md §4.6).</summary>
public sealed class SubmissionSummaryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserDisplayName { get; set; }
    public int Score { get; set; }
    public int? DurationSeconds { get; set; }
    public int? ClassAssignmentId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? AnswersJson { get; set; }
    public string? ResultJson { get; set; }
}
