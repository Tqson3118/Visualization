namespace DsaVisual.Application.Dtos;

/// <summary>Gửi báo cáo lỗi — POST /bug-reports (API_REFERENCE.md §4.15).</summary>
public sealed class BugReportRequest
{
    public string Description { get; set; } = string.Empty;   // ≤ 2000
    public string? Context { get; set; }                      // URL, browser, bước mô phỏng
}

/// <summary>Báo cáo lỗi — GET /admin/bug-reports.</summary>
public sealed class BugReportDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string Status { get; set; } = "NEW";
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>Cập nhật trạng thái xử lý — PUT /admin/bug-reports/{id}.</summary>
public sealed class BugReportUpdateRequest
{
    public string Status { get; set; } = "NEW";   // NEW/PROCESSING/RESOLVED/CLOSED
    public string? AdminNote { get; set; }        // Phản hồi của Admin (v2.15)
}

/// <summary>Báo cáo bài học vi phạm — POST /lessons/{id}/report (v2.15).</summary>
public sealed class LessonReportRequest
{
    public string Reason { get; set; } = string.Empty;   // Lý do: nội dung không phù hợp, sai kiến thức, 18+, spam
}

/// <summary>Tham gia lớp bằng mã mời — POST /classes/join-by-code (v2.15).</summary>
public sealed class JoinClassByCodeRequest
{
    public string InviteCode { get; set; } = string.Empty;
}

/// <summary>Duyệt/từ chối bài học — POST /admin/lessons/{id}/review (v2.15).</summary>
public sealed class LessonReviewRequest
{
    public bool Approve { get; set; }
    public string? Reason { get; set; }   // Bắt buộc khi từ chối (approve == false)
}
