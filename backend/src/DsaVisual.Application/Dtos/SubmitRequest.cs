namespace DsaVisual.Application.Dtos;

/// <summary>Nộp bài — POST /exercises/{id}/submit (API_REFERENCE.md §3.8/§4.6).</summary>
public sealed class SubmitRequest
{
    public List<AnswerDto> Answers { get; set; } = [];
    public int? ClassAssignmentId { get; set; }   // nộp qua luồng lớp (v2.8)
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Idempotency key OPTIONAL (fix Đợt D — review Major #1): cùng (User, Exercise, ClassAssignment,
    /// ClientRequestId) → chỉ 1 ExerciseSubmission được tạo (retry/double-click an toàn, trả submission cũ).
    /// NULL (FE hiện không gửi) → MỌI lần nộp là lần mới — re-attempt cải thiện điểm hợp lệ (FR-4.4/FR-9.5).
    /// </summary>
    public string? ClientRequestId { get; set; }
}
