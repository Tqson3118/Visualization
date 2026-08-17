namespace DsaVisual.Application.Dtos;

/// <summary>Thông tin cấp độ/XP của người dùng — GET /me/gamification (feature port gamification UI).</summary>
public sealed class GamificationSummaryDto
{
    public int Xp { get; set; }
    public int Level { get; set; }
    /// <summary>XP đã tích lũy TRONG level hiện tại.</summary>
    public int XpIntoLevel { get; set; }
    /// <summary>XP cần để tăng từ level này lên level tiếp theo (bề rộng level).</summary>
    public int XpForNextLevel { get; set; }
    /// <summary>Phần trăm tiến tới level tiếp theo (0-100).</summary>
    public int LevelProgressPct { get; set; }
}
