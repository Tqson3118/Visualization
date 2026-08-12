namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Người dùng đạt thành tích — SDD §7.3.20. UNIQUE (UserId, AchievementId) chống trao 2 lần.</summary>
public sealed class UserAchievement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AchievementId { get; set; }
    public DateTime EarnedAt { get; set; }
}
