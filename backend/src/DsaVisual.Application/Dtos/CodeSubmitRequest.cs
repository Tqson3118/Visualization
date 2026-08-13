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

    /// <summary>Nộp qua luồng lớp (v2.8, FR-8.3) — optional, backward compatible (biz#6).</summary>
    public int? ClassAssignmentId { get; set; }

    /// <summary>
    /// Idempotency key OPTIONAL (fix Đợt D — review Major #1): cùng (User, Exercise, ClientRequestId)
    /// → chỉ 1 CodeSubmission được tạo (retry/double-click an toàn, trả submission cũ).
    /// NULL (FE hiện không gửi) → MỌI lần nộp là lần mới — lịch sử nhiều lần nộp + so sánh 2 lần (FR-9.5).
    /// </summary>
    public string? ClientRequestId { get; set; }

    public List<CodeTestCaseResultDto> Results { get; set; } = [];
}
