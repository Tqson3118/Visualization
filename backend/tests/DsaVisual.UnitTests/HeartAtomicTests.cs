using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DsaVisual.UnitTests;

public class HeartAtomicTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(GamificationService Service, AppDbContext Db)> SetupAsync(string dbName)
    {
        var (db, _) = TestServices.CreateSqliteDb();

        db.Users.Add(new User
        {
            Id = 1,
            Email = "student@demo.local",
            PasswordHash = "x",
            DisplayName = "Student",
            Hearts = 3,
            HeartsMax = 5,
            LastHeartAt = _clock.UtcNow,
            CreatedAt = _clock.UtcNow
        });

        db.LearningPaths.Add(new LearningPath
        {
            Id = 1,
            Title = "Lộ trình DSA",
            IsActive = true,
            CreatedBy = 1
        });

        db.LearningPathNodes.Add(new LearningPathNode
        {
            Id = 101,
            PathId = 1,
            Title = "Bubble Sort Node",
            SortOrder = 1
        });

        await db.SaveChangesAsync();

        var service = TestServices.CreateGamificationService(db, _clock);
        return (service, db);
    }

    [Fact]
    public async Task EnterNode_NormalRequest_DeductsOneHeart()
    {
        var (service, db) = await SetupAsync(nameof(EnterNode_NormalRequest_DeductsOneHeart));

        var result = await service.EnterNodeAsync(1, 1, 101, null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.HeartsLeft);

        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(2, user.Hearts);
    }

    [Fact]
    public async Task EnterNode_WhenHeartsZero_ReturnsHeartsEmptyError()
    {
        var (service, db) = await SetupAsync(nameof(EnterNode_WhenHeartsZero_ReturnsHeartsEmptyError));
        var user = await db.Users.FirstAsync(u => u.Id == 1);
        user.Hearts = 0;
        await db.SaveChangesAsync();

        var result = await service.EnterNodeAsync(1, 1, 101, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.HEARTS_EMPTY, result.ErrorCode);
    }

    [Fact]
    public async Task EnterNode_ActiveSession_ResumeWithoutDeducting()
    {
        var (service, db) = await SetupAsync(nameof(EnterNode_ActiveSession_ResumeWithoutDeducting));

        var first = await service.EnterNodeAsync(1, 1, 101, null, CancellationToken.None);
        Assert.True(first.IsSuccess);
        Assert.Equal(2, first.Value!.HeartsLeft);

        // Re-enter within active session (30 minutes)
        var second = await service.EnterNodeAsync(1, 1, 101, null, CancellationToken.None);
        Assert.True(second.IsSuccess);
        Assert.Equal(2, second.Value!.HeartsLeft);

        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(2, user.Hearts);
    }
}
