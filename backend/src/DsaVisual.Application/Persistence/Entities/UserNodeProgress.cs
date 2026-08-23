namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Tiến độ node (Ladder) — SDD §7.3.26.</summary>
public sealed class UserNodeProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int NodeId { get; set; }
    public int Status { get; set; }
    public int Stars { get; set; }
    public int NodeScore { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public DateTime? PassedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[]? RowVersion { get; set; }                     // concurrency token (finding #3)
}
