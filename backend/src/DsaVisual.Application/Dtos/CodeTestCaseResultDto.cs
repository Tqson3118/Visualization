namespace DsaVisual.Application.Dtos;

/// <summary>Kết quả 1 test ẩn — API_REFERENCE.md §4.14 (FR-9.3).</summary>
public sealed class CodeTestCaseResultDto
{
    public string TestId { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? Input { get; set; }
    public string? Expected { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
}
