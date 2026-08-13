namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Bài nộp code chấm điểm (test ẩn, client sandbox) — SDD §7.3.24 (FR-9.3, FR-9.5).</summary>
public sealed class CodeSubmission
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ExerciseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Score { get; set; }
    public int PassedTests { get; set; }
    public int TotalTests { get; set; }
    public string ResultJson { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

    /// <summary>
    /// Điểm/Passed/Total do CLIENT tự khai (ADR-012 chấm client — findings-security #1).
    /// Server clamp Score vào [0, MaxScore] nhưng KHÔNG thể tin tuyệt đối → đánh dấu bản ghi
    /// để báo cáo/điều tra không nhầm với chấm điểm chính thức phía server.
    /// </summary>
    public bool IsClientDeclared { get; set; } = true;

    /// <summary>
    /// Idempotency key OPTIONAL (fix Đợt D — review Major #1): NULL → mọi lần nộp là lần mới
    /// (lịch sử + so sánh 2 lần nộp hợp lệ FR-9.5); có giá trị → unique filtered
    /// (UserId, ExerciseId, ClientRequestId) chống nộp trùng cùng key. FE hiện không gửi.
    /// </summary>
    public string? ClientRequestId { get; set; }
}
