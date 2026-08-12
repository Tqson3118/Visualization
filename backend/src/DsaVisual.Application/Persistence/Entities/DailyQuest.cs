namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Quest hằng ngày — SDD §7.3.27. QuestKey UNIQUE.</summary>
public sealed class DailyQuest
{
    public int Id { get; set; }
    public string QuestKey { get; set; } = string.Empty;             // UNIQUE
    public string Title { get; set; } = string.Empty;
    public int Type { get; set; }
    public string ConditionJson { get; set; } = string.Empty;
    public string RewardJson { get; set; } = string.Empty;
    public bool PoolEnabled { get; set; }
}
