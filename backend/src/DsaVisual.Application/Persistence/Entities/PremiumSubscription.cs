namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Đăng ký premium — SDD §7.3.30.</summary>
public sealed class PremiumSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? PlanId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int Status { get; set; }
    public string? OrderRef { get; set; }
    public DateTime CreatedAt { get; set; }
    public byte[]? RowVersion { get; set; }                     // concurrency token (finding #3)
}
