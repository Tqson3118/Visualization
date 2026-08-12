namespace DsaVisual.Application.Dtos;

/// <summary>Nộp bài — POST /exercises/{id}/submit (API_REFERENCE.md §3.8/§4.6).</summary>
public sealed class SubmitRequest
{
    public List<AnswerDto> Answers { get; set; } = [];
    public int? ClassAssignmentId { get; set; }   // nộp qua luồng lớp (v2.8)
    public int? DurationSeconds { get; set; }
}
