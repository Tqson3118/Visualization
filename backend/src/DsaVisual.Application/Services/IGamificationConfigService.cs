using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

public interface IGamificationConfigService
{
    GamificationSettingsDto GetSettings();
    Task<GamificationSettingsDto> UpdateSettingsAsync(GamificationSettingsDto newSettings, CancellationToken ct = default);
    Task<GamificationSettingsDto> ResetToDefaultsAsync(CancellationToken ct = default);
    int GetTheoryBaseXp();
    int GetQuizBaseXp();
    int GetCodelabBaseXp();
    int GetStreakBonusXp();
    int GetHeartsMaxFree();
    int GetHeartsMaxPremium();
    int GetHeartRegenMinutes();
    int GetSessionHours();
}
