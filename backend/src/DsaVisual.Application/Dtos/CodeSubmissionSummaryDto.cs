namespace DsaVisual.Application.Dtos;

/// <summary>Bài nộp code trong lịch sử — GET /exercises/{id}/code-submissions(me) (API_REFERENCE.md §4.13).</summary>
public sealed class CodeSubmissionSummaryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? UserDisplayName { get; set; }
    public string? Code { get; set; }
    public int Score { get; set; }
    public int PassedTests { get; set; }
    public int TotalTests { get; set; }
    public DateTime SubmittedAt { get; set; }
}
