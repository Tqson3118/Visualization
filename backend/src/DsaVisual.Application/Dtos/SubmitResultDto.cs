namespace DsaVisual.Application.Dtos;

/// <summary>Kết quả nộp bài — API_REFERENCE.md §3.8/§4.6/§4.16.</summary>
public sealed class SubmitResultDto
{
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public bool Passed { get; set; }
    public List<QuestionResultDto> Results { get; set; } = [];
    public int SubmissionId { get; set; }
    public DateTime SubmittedAt { get; set; }
}
