namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Báo lỗi — SDD §7.3.22.</summary>
public sealed class BugReport
{
    public int Id { get; set; }
    public int? UserId { get; set; }                                 // null = khách
    public string Description { get; set; } = string.Empty;          // ≤ 2000
    public string? ContextJson { get; set; }                         // URL, browser, bước mô phỏng
    public BugReportStatus Status { get; set; } = BugReportStatus.New;
    public int? AssigneeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
