namespace DsaVisual.Application.Dtos;

/// <summary>Kết quả nộp code — API_REFERENCE.md §4.14 example.</summary>
public sealed class CodeSubmitResultDto
{
    public int Score { get; set; }
    public int Passed { get; set; }
    public int Total { get; set; }
    public List<CodeTestCaseResultDto> Results { get; set; } = [];
    public int SubmissionId { get; set; }
    public DateTime SubmittedAt { get; set; }
}
