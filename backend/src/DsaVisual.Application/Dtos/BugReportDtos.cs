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
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>Cập nhật trạng thái xử lý — PUT /admin/bug-reports/{id}.</summary>
public sealed class BugReportUpdateRequest
{
    public string Status { get; set; } = "NEW";   // NEW/PROCESSING/RESOLVED/CLOSED
}
