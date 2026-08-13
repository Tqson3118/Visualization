namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Quest của người dùng theo ngày — SDD §7.3.27.</summary>
public sealed class UserQuest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int QuestId { get; set; }
    public DateTime QuestDate { get; set; }                          // date
    public int Progress { get; set; }
    public bool Claimed { get; set; }
    public byte[]? RowVersion { get; set; }                     // concurrency token (finding #3)
}
