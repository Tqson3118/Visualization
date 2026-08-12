namespace DsaVisual.Application.Dtos;

/// <summary>Quest hôm nay — API_REFERENCE.md §3.13/§4.14 (FR-10.3).</summary>
public sealed class QuestDto
{
    public int Id { get; set; }              // UserQuest.Id — dùng cho claim
    public int QuestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Type { get; set; }            // 0=E, 1=M, 2=H
    public int Progress { get; set; }
    public int Target { get; set; }
    public bool Claimed { get; set; }
    public QuestRewardDto Reward { get; set; } = new();
}

/// <summary>Phần thưởng quest {gems, xp}.</summary>
public sealed class QuestRewardDto
{
    public int Gems { get; set; }
    public int Xp { get; set; }
}

/// <summary>Kết quả claim quest — API_REFERENCE.md §4.14 example (FR-10.3).</summary>
public sealed class QuestClaimResultDto
{
    public bool Claimed { get; set; }
    public QuestRewardDto Reward { get; set; } = new();
    public int GemsTotal { get; set; }
}

/// <summary>Streak — GET /me/streak (FR-10.4).</summary>
public sealed class StreakDto
{
    public int StreakDays { get; set; }
    public int StreakFreeze { get; set; }
}

/// <summary>Dòng leaderboard — GET /leaderboard (FR-10.6).</summary>
public sealed class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Xp { get; set; }
    public int Level { get; set; }

    /// <summary>
    /// Giá trị xếp hạng theo tab (FE đọc để hiển thị — G-F3E-NEW-1):
    /// week = XP tuần, level = tổng XP, class = điểm trong lớp.
    /// Hiện tại cả 3 tab đều xếp hạng theo tổng Xp (chỉ khác bộ lọc) nên Value = Xp.
    /// </summary>
    public long Value { get; set; }
}
