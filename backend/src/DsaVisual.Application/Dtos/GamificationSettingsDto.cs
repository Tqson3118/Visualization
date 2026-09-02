namespace DsaVisual.Application.Dtos;

public sealed class GamificationSettingsDto
{
    public int TheoryBaseXp { get; set; } = 50;
    public int QuizBaseXp { get; set; } = 50;
    public int CodelabBaseXp { get; set; } = 100;
    public int StreakBonusXp { get; set; } = 20;
    public int HeartsMaxFree { get; set; } = 10;
    public int HeartsMaxPremium { get; set; } = 30;
    public int HeartRegenMinutes { get; set; } = 30;
    public int SessionHours { get; set; } = 36;
}
