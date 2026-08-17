using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test GET /me/gamification (GamificationService.GetGamificationSummaryAsync): level + XP
/// tính từ Users.Xp theo công thức ComputeLevel = 1 + floor(sqrt(xp/100)).
/// </summary>
public sealed class GamificationSummaryTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(GamificationService Service, AppDbContext Db)> SetupAsync(string dbName, int xp)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        db.Users.Add(new User
        {
            Id = 1,
            Email = $"g{dbName}@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Student",
            Xp = xp,
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();
        return (TestServices.CreateGamificationService(db, _clock), db);
    }

    [Fact]
    public async Task Summary_XpZero_LevelOne_ZeroIntoFull()
    {
        var (service, _) = await SetupAsync(nameof(Summary_XpZero_LevelOne_ZeroIntoFull), 0);
        var result = await service.GetGamificationSummaryAsync(1, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.Level);
        Assert.Equal(0, result.Value.XpIntoLevel);
        Assert.Equal(100, result.Value.XpForNextLevel);
        Assert.Equal(0, result.Value.LevelProgressPct);
    }

    [Fact]
    public async Task Summary_Xp250_Level2_SixtyPercent()
    {
        // level = 1 + floor(sqrt(2.5)) = 2; floor = 100; into = 150; span = 300 → 50%
        var (service, _) = await SetupAsync(nameof(Summary_Xp250_Level2_SixtyPercent), 250);
        var result = await service.GetGamificationSummaryAsync(1, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.Level);
        Assert.Equal(150, result.Value.XpIntoLevel);
        Assert.Equal(300, result.Value.XpForNextLevel);
        Assert.Equal(50, result.Value.LevelProgressPct);
    }

    [Fact]
    public async Task Summary_Xp400_Level3_ZeroInto()
    {
        // level = 1 + floor(sqrt(4)) = 3; floor = 400; into = 0; span = 500
        var (service, _) = await SetupAsync(nameof(Summary_Xp400_Level3_ZeroInto), 400);
        var result = await service.GetGamificationSummaryAsync(1, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, result.Value!.Level);
        Assert.Equal(0, result.Value.XpIntoLevel);
        Assert.Equal(500, result.Value.XpForNextLevel);
        Assert.Equal(0, result.Value.LevelProgressPct);
    }

    [Fact]
    public async Task Summary_Xp999_Level4_HighInto()
    {
        // level = 1 + floor(sqrt(9.99)) = 4; floor = 900; into = 99; span = 700 → 14%
        var (service, _) = await SetupAsync(nameof(Summary_Xp999_Level4_HighInto), 999);
        var result = await service.GetGamificationSummaryAsync(1, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(999, result.Value!.Xp);
        Assert.Equal(4, result.Value!.Level);
        Assert.Equal(99, result.Value.XpIntoLevel);
        Assert.Equal(700, result.Value.XpForNextLevel);
        Assert.Equal(14, result.Value.LevelProgressPct);
    }

    [Fact]
    public async Task Summary_UnknownUser_XpZero_LevelOne()
    {
        var (service, _) = await SetupAsync(nameof(Summary_UnknownUser_XpZero_LevelOne), 10);
        var result = await service.GetGamificationSummaryAsync(1234, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, result.Value!.Xp);
        Assert.Equal(1, result.Value.Level);
    }
}
