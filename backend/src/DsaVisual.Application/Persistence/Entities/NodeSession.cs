namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Phiên làm bài trong node (hearts/session) — SDD §7.3.26 (v2.4).</summary>
public sealed class NodeSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int NodeId { get; set; }                                  // FK → LearningPathNodes.Id
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int? Stage { get; set; }
    public int? StepIndex { get; set; }
    public byte[]? RowVersion { get; set; }                     // concurrency token (finding #3)
}
