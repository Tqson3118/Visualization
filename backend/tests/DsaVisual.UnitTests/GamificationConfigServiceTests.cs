using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using DsaVisual.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace DsaVisual.UnitTests;

public sealed class GamificationConfigServiceTests : IDisposable
{
    private readonly string _tempFile;

    public GamificationConfigServiceTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"gamification_test_{Guid.NewGuid():N}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
        {
            try { File.Delete(_tempFile); } catch { }
        }
    }

    [Fact]
    public void Constructor_WhenFileDoesNotExist_CreatesDefaultSettingsAndWritesFile()
    {
        // Act
        var service = new GamificationConfigService(_tempFile);
        var settings = service.GetSettings();

        // Assert
        Assert.True(File.Exists(_tempFile));
        Assert.Equal(50, settings.TheoryBaseXp);
        Assert.Equal(50, settings.QuizBaseXp);
        Assert.Equal(100, settings.CodelabBaseXp);
        Assert.Equal(20, settings.StreakBonusXp);
        Assert.Equal(10, settings.HeartsMaxFree);
        Assert.Equal(30, settings.HeartsMaxPremium);
        Assert.Equal(30, settings.HeartRegenMinutes);
        Assert.Equal(36, settings.SessionHours);

        Assert.Equal(50, service.GetTheoryBaseXp());
        Assert.Equal(50, service.GetQuizBaseXp());
        Assert.Equal(100, service.GetCodelabBaseXp());
        Assert.Equal(20, service.GetStreakBonusXp());
        Assert.Equal(10, service.GetHeartsMaxFree());
        Assert.Equal(30, service.GetHeartsMaxPremium());
        Assert.Equal(30, service.GetHeartRegenMinutes());
        Assert.Equal(36, service.GetSessionHours());
    }

    [Fact]
    public async Task UpdateSettingsAsync_UpdatesInMemoryAndPersistsToFile()
    {
        // Arrange
        var service = new GamificationConfigService(_tempFile);
        var newSettings = new GamificationSettingsDto
        {
            TheoryBaseXp = 60,
            QuizBaseXp = 75,
            CodelabBaseXp = 120,
            StreakBonusXp = 25,
            HeartsMaxFree = 15,
            HeartsMaxPremium = 40,
            HeartRegenMinutes = 20,
            SessionHours = 48
        };

        // Act
        var updated = await service.UpdateSettingsAsync(newSettings);

        // Assert in-memory
        Assert.Equal(60, service.GetTheoryBaseXp());
        Assert.Equal(75, service.GetQuizBaseXp());
        Assert.Equal(120, service.GetCodelabBaseXp());
        Assert.Equal(25, service.GetStreakBonusXp());
        Assert.Equal(15, service.GetHeartsMaxFree());
        Assert.Equal(40, service.GetHeartsMaxPremium());
        Assert.Equal(20, service.GetHeartRegenMinutes());
        Assert.Equal(48, service.GetSessionHours());

        // Assert disk reload on a new service instance
        var reloadedService = new GamificationConfigService(_tempFile);
        var reloadedSettings = reloadedService.GetSettings();
        Assert.Equal(60, reloadedSettings.TheoryBaseXp);
        Assert.Equal(75, reloadedSettings.QuizBaseXp);
        Assert.Equal(120, reloadedSettings.CodelabBaseXp);
        Assert.Equal(25, reloadedSettings.StreakBonusXp);
        Assert.Equal(15, reloadedSettings.HeartsMaxFree);
        Assert.Equal(40, reloadedSettings.HeartsMaxPremium);
        Assert.Equal(20, reloadedSettings.HeartRegenMinutes);
        Assert.Equal(48, reloadedSettings.SessionHours);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_RestoresOriginalValues()
    {
        // Arrange
        var service = new GamificationConfigService(_tempFile);
        await service.UpdateSettingsAsync(new GamificationSettingsDto
        {
            TheoryBaseXp = 200,
            HeartsMaxFree = 5
        });
        Assert.Equal(200, service.GetTheoryBaseXp());

        // Act
        var reset = await service.ResetToDefaultsAsync();

        // Assert
        Assert.Equal(50, reset.TheoryBaseXp);
        Assert.Equal(10, reset.HeartsMaxFree);
        Assert.Equal(50, service.GetTheoryBaseXp());
        Assert.Equal(10, service.GetHeartsMaxFree());
    }

    [Fact]
    public void Validator_ValidSettings_PassesValidation()
    {
        var validator = new GamificationSettingsValidator();
        var valid = new GamificationSettingsDto
        {
            TheoryBaseXp = 50,
            QuizBaseXp = 50,
            CodelabBaseXp = 100,
            StreakBonusXp = 20,
            HeartsMaxFree = 10,
            HeartsMaxPremium = 30,
            HeartRegenMinutes = 30,
            SessionHours = 36
        };

        var result = validator.TestValidate(valid);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_InvalidSettings_FailsValidation()
    {
        var validator = new GamificationSettingsValidator();
        var invalid = new GamificationSettingsDto
        {
            TheoryBaseXp = -10,
            QuizBaseXp = 2000,
            CodelabBaseXp = -1,
            StreakBonusXp = 1500,
            HeartsMaxFree = 0,
            HeartsMaxPremium = 0,
            HeartRegenMinutes = 0,
            SessionHours = 500
        };

        var result = validator.TestValidate(invalid);
        result.ShouldHaveValidationErrorFor(x => x.TheoryBaseXp);
        result.ShouldHaveValidationErrorFor(x => x.QuizBaseXp);
        result.ShouldHaveValidationErrorFor(x => x.CodelabBaseXp);
        result.ShouldHaveValidationErrorFor(x => x.StreakBonusXp);
        result.ShouldHaveValidationErrorFor(x => x.HeartsMaxFree);
        result.ShouldHaveValidationErrorFor(x => x.HeartsMaxPremium);
        result.ShouldHaveValidationErrorFor(x => x.HeartRegenMinutes);
        result.ShouldHaveValidationErrorFor(x => x.SessionHours);
    }
}
