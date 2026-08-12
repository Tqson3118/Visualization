namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Giao dịch gems — SDD §7.3.29 (append-only).</summary>
public sealed class GemTransaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Type { get; set; }
    public int Amount { get; set; }
    public string? RefType { get; set; }
    public string? RefId { get; set; }
    public DateTime CreatedAt { get; set; }
}
