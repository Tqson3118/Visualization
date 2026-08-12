namespace DsaVisual.Application.Dtos;

/// <summary>Nộp bài code — POST /exercises/{id}/code-submit (API_REFERENCE.md §4.13/§4.14).
/// Chấm điểm chạy sandbox client (FR-9.3, ADR-012) — client gửi kết quả test ẩn lên.</summary>
public sealed class CodeSubmitRequest
{
    public string Code { get; set; } = string.Empty;
    public int ExerciseId { get; set; }
    public int Score { get; set; }
    public int Passed { get; set; }
    public int Total { get; set; }
    public List<CodeTestCaseResultDto> Results { get; set; } = [];
}
