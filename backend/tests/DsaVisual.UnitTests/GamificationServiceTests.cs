using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using DsaVisual.UnitTests;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test GamificationService: trừ tim atomic (enter node), resume session không trừ tim,
/// HEARTS_EMPTY, claim quest (atomic, chống claim trùng).
/// Dùng SQLite in-memory (InMemory provider không hỗ trợ ExecuteSqlInterpolated).
/// </summary>
public class GamificationServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(GamificationService Service, AppDbContext Db)> SetupAsync(string dbName)
    {
        var (db, connection) = TestServices.CreateSqliteDb();

        db.Users.Add(new User
        {
            Id = 1,
            Email = "student@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Student",
            Hearts = 10,
            HeartsMax = 10,
            LastHeartAt = _clock.UtcNow,
            CreatedAt = _clock.UtcNow
        });
        db.LearningPaths.Add(new LearningPath
        {
            Id = 1,
            Title = "Sắp xếp & Tìm kiếm",
            IsActive = true,
            CreatedBy = 1
        });
        db.LearningPathNodes.Add(new LearningPathNode
        {
            Id = 10,
            PathId = 1,
            Title = "Bubble Sort",
            SortOrder = 1
        });
        db.LearningPathNodes.Add(new LearningPathNode
        {
            Id = 11,
            PathId = 1,
            Title = "Binary Search",
            SortOrder = 2
        });
        await db.SaveChangesAsync();

        var service = TestServices.CreateGamificationService(db, _clock);
        return (service, db);
    }

    [Fact]
    public async Task EnterNode_DeductsOneHeart_AndCreatesSession()
    {
        var (service, db) = await SetupAsync(nameof(EnterNode_DeductsOneHeart_AndCreatesSession));

        var result = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(9, result.Value!.HeartsLeft);
        Assert.NotNull(result.Value.Session);
        Assert.Equal(10, result.Value.Session.NodeId);

        var sessionCount = await db.NodeSessions.CountAsync();
        Assert.Equal(1, sessionCount);
    }

    [Fact]
    public async Task EnterNode_ActiveSession_ResumeWithoutDeducting()
    {
        var (service, db) = await SetupAsync(nameof(EnterNode_ActiveSession_ResumeWithoutDeducting));

        var first = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);
        Assert.True(first.IsSuccess);
        Assert.Equal(9, first.Value!.HeartsLeft);

        // Vào lại node trong session 30 phút → KHÔNG trừ tim (AC-10.1.2, v2.5)
        var second = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);

        Assert.True(second.IsSuccess);
        Assert.Equal(9, second.Value!.HeartsLeft);
        Assert.Equal(1, await db.NodeSessions.CountAsync());
    }

    [Fact]
    public async Task EnterNode_NoHearts_ReturnsHeartsEmpty()
    {
        var (service, db) = await SetupAsync(nameof(EnterNode_NoHearts_ReturnsHeartsEmpty));
        var user = await db.Users.FirstAsync(u => u.Id == 1);
        user.Hearts = 0;
        await db.SaveChangesAsync();

        var result = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.HEARTS_EMPTY, result.ErrorCode);
        // Rollback → không tạo session
        Assert.Equal(0, await db.NodeSessions.CountAsync());
    }

    [Fact]
    public async Task EnterNode_LockedLadder_ReturnsLadderLocked()
    {
        var (service, db) = await SetupAsync(nameof(EnterNode_LockedLadder_ReturnsLadderLocked));

        // Node 2 chưa pass node 1 → LADDER_LOCKED
        var result = await service.EnterNodeAsync(1, 1, 11, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.LADDER_LOCKED, result.ErrorCode);
    }

    [Fact]
    public async Task EnterNode_PassedNode_ReEntryFree()
    {
        var (service, db) = await SetupAsync(nameof(EnterNode_PassedNode_ReEntryFree));

        // Pass node 10 (UserNodeProgress status=2)
        db.UserNodeProgress.Add(new UserNodeProgress
        {
            UserId = 1,
            NodeId = 10,
            Status = 2,
            Stars = 3,
            NodeScore = 10,
            PassedAt = _clock.UtcNow,
            UpdatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);

        // AC-10.1.3: xem lại node đã pass MIỄN PHÍ — không trừ tim
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.HeartsLeft);
    }

    // ── F5-Major: heart regen persist (SETUP_TODO §8.2, FR-10.1) ──────────────

    [Fact]
    public async Task GetHearts_RegenElapsed_PersistsToDb()
    {
        var (service, db) = await SetupAsync(nameof(GetHearts_RegenElapsed_PersistsToDb));

        // Hết tim từ 90 phút trước (Free 30p/tim) → regen 3 tim
        var user = await db.Users.FirstAsync(u => u.Id == 1);
        user.Hearts = 0;
        user.LastHeartAt = _clock.UtcNow.AddMinutes(-90);
        await db.SaveChangesAsync();

        var result = await service.GetHeartsAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, result.Value!.Hearts);

        // F5-Major: regen phải được GHI xuống DB — truy vấn sau không còn "tim ảo"
        var persisted = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(3, persisted.Hearts);
        Assert.Equal(_clock.UtcNow, persisted.LastHeartAt);
    }

    [Fact]
    public async Task GetHearts_AtMax_DoesNotMoveLastHeartAt()
    {
        var (service, db) = await SetupAsync(nameof(GetHearts_AtMax_DoesNotMoveLastHeartAt));

        // Tim đã đầy từ lâu — không cần regen, không được dời LastHeartAt (giữ NextHeartInSeconds đúng)
        var user = await db.Users.FirstAsync(u => u.Id == 1);
        var original = _clock.UtcNow.AddHours(-5);
        user.Hearts = 10;
        user.LastHeartAt = original;
        await db.SaveChangesAsync();

        var result = await service.GetHeartsAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.Hearts);
        Assert.Equal(0, result.Value.NextHeartInSeconds);
        var persisted = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(original, persisted.LastHeartAt);
    }

    [Fact]
    public async Task GetHearts_Premium_RegenEvery10Minutes()
    {
        var (service, db) = await SetupAsync(nameof(GetHearts_Premium_RegenEvery10Minutes));

        // Premium: 10 phút/tim, max 30 (FR-10.1/FR-10.7)
        var user = await db.Users.FirstAsync(u => u.Id == 1);
        user.Hearts = 29;
        user.HeartsMax = 30;
        user.PremiumUntil = _clock.UtcNow.AddMonths(1);
        user.LastHeartAt = _clock.UtcNow.AddMinutes(-10);
        await db.SaveChangesAsync();

        var result = await service.GetHeartsAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(30, result.Value!.Hearts);
        Assert.Equal(30, result.Value.HeartsMax);

        var persisted = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(30, persisted.Hearts);
    }

    [Fact]
    public async Task EnterNode_RegenElapsed_SpendSucceeds_NotHeartsEmpty()
    {
        var (service, db) = await SetupAsync(nameof(EnterNode_RegenElapsed_SpendSucceeds_NotHeartsEmpty));

        // F5-Major: DB=0 nhưng đã qua 90 phút (Free 30p/tim) → 3 tim regen; vào node phải TRỪ được
        var user = await db.Users.FirstAsync(u => u.Id == 1);
        user.Hearts = 0;
        user.LastHeartAt = _clock.UtcNow.AddMinutes(-90);
        await db.SaveChangesAsync();

        var result = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);

        // Trước fix: UPDATE Hearts > 0 fail (DB=0) → HEARTS_EMPTY dù đã qua chu kỳ regen
        Assert.True(result.IsSuccess, $"Expected success, got {result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(2, result.Value!.HeartsLeft);   // 3 regen − 1 spend

        var persisted = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(2, persisted.Hearts);
        Assert.Equal(_clock.UtcNow, persisted.LastHeartAt);
    }

    [Fact]
    public async Task ClaimQuest_AddsReward_AndSecondClaimFails()
    {
        var (service, db) = await SetupAsync(nameof(ClaimQuest_AddsReward_AndSecondClaimFails));

        db.DailyQuests.Add(new DailyQuest
        {
            Id = 1,
            QuestKey = "learn-1-node",
            Title = "Học 1 node",
            Type = 0,
            ConditionJson = """{"activity":"pass_node","count":1}""",
            RewardJson = """{"gems":3,"xp":20}""",
            PoolEnabled = true
        });
        db.DailyQuests.Add(new DailyQuest
        {
            Id = 2,
            QuestKey = "pass-1-lab",
            Title = "Pass 1 lab",
            Type = 0,
            ConditionJson = """{"activity":"pass_lab","count":1}""",
            RewardJson = """{"gems":2,"xp":10}""",
            PoolEnabled = true
        });
        await db.SaveChangesAsync();

        var quests = await service.GetQuestsAsync(1, CancellationToken.None);
        Assert.True(quests.IsSuccess);
        Assert.True(quests.Value!.Count > 0);

        var questId = quests.Value![0].Id;
        // Hoàn thành quest để claim được
        var uq = await db.UserQuests.FirstAsync(q => q.Id == questId);
        uq.Progress = 1;
        await db.SaveChangesAsync();

        var first = await service.ClaimQuestAsync(1, questId, CancellationToken.None);
        Assert.True(first.IsSuccess);
        Assert.True(first.Value!.Claimed);
        Assert.Equal(3, first.Value.Reward.Gems);
        Assert.Equal(3, first.Value.GemsTotal);

        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(3, user.Gems);
        Assert.Equal(20, user.Xp);

        // Claim lần 2 → QUEST_ALREADY_CLAIMED (atomic, không cộng thêm)
        var second = await service.ClaimQuestAsync(1, questId, CancellationToken.None);
        Assert.False(second.IsSuccess);
        Assert.Equal(ErrorCodes.QUEST_ALREADY_CLAIMED, second.ErrorCode);

        var userAfter = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(3, userAfter.Gems);
    }

    // ── G-F3E-NEW-1: GetLeaderboardAsync map Value theo tab (FE đọc row.value) ──

    [Fact]
    public async Task GetLeaderboard_LevelTab_MapsValueToTotalXp()
    {
        var (service, db) = await SetupAsync(nameof(GetLeaderboard_LevelTab_MapsValueToTotalXp));

        db.Users.AddRange(
            new User { Id = 2, Email = "b@university.edu.vn", PasswordHash = "x", DisplayName = "B", Xp = 500, CreatedAt = _clock.UtcNow },
            new User { Id = 3, Email = "c@university.edu.vn", PasswordHash = "x", DisplayName = "C", Xp = 1200, CreatedAt = _clock.UtcNow },
            new User { Id = 4, Email = "d@university.edu.vn", PasswordHash = "x", DisplayName = "D", Xp = 300, CreatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.GetLeaderboardAsync("level", null, 1, 20, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var items = result.Value!.Items;
        Assert.Equal(4, items.Count);
        // Xếp theo Xp giảm dần; Value = tổng XP cho mọi tab (cách tính hiện có)
        Assert.Equal(1200, items[0].Value);
        Assert.Equal(1200, items[0].Xp);
        Assert.Equal(1, items[0].Rank);
        Assert.Equal(500, items[1].Value);
        Assert.Equal(300, items[2].Value);
        // Level = ComputeLevel(Xp): 1 + floor(sqrt(Xp/100))
        Assert.Equal(4, items[0].Level);   // 1 + floor(sqrt(12)) = 4
        Assert.Equal(3, items[1].Level);   // 1 + floor(sqrt(5)) = 3
    }

    [Fact]
    public async Task GetLeaderboard_WeekTab_MapsValue_OnlyActiveThisWeek()
    {
        var (service, db) = await SetupAsync(nameof(GetLeaderboard_WeekTab_MapsValue_OnlyActiveThisWeek));

        // clock = 2026-08-12 08:00 UTC (= thứ Tư 15:00 UTC+7) → tuần bắt đầu thứ Hai 00:00 UTC+7
        db.Users.Add(new User
        {
            Id = 2, Email = "b@university.edu.vn", PasswordHash = "x", DisplayName = "B",
            Xp = 640, LastActivityDate = _clock.UtcNow.AddHours(-6), CreatedAt = _clock.UtcNow
        });
        db.Users.Add(new User
        {
            Id = 3, Email = "c@university.edu.vn", PasswordHash = "x", DisplayName = "C",
            Xp = 999, LastActivityDate = new DateTime(2026, 8, 5, 0, 0, 0, DateTimeKind.Utc), CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await service.GetLeaderboardAsync("week", null, 1, 20, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var items = result.Value!.Items;
        // User 1 (không LastActivityDate) và user 3 (hoạt động tuần trước) bị loại
        Assert.Single(items);
        Assert.Equal(2, items[0].UserId);
        Assert.Equal(640, items[0].Value);
        Assert.Equal(640, items[0].Xp);
    }

    [Fact]
    public async Task GetLeaderboard_ClassTab_RequiresClassId_ReturnsValidationFailed()
    {
        var (service, db) = await SetupAsync(nameof(GetLeaderboard_ClassTab_RequiresClassId_ReturnsValidationFailed));

        var result = await service.GetLeaderboardAsync("class", null, 1, 20, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
    }

    [Fact]
    public async Task GetLeaderboard_ClassTab_MapsValue_OnlyClassMembers()
    {
        var (service, db) = await SetupAsync(nameof(GetLeaderboard_ClassTab_MapsValue_OnlyClassMembers));

        db.Users.AddRange(
            new User { Id = 2, Email = "b@university.edu.vn", PasswordHash = "x", DisplayName = "B", Xp = 800, CreatedAt = _clock.UtcNow },
            new User { Id = 3, Email = "c@university.edu.vn", PasswordHash = "x", DisplayName = "C", Xp = 600, CreatedAt = _clock.UtcNow });
        db.Classes.Add(new Class { Id = 1, Name = "Lớp DSA 01", InviteCode = "ABC123", OwnerId = 1, CreatedAt = _clock.UtcNow });
        db.ClassMembers.AddRange(
            new ClassMember { ClassId = 1, UserId = 2, JoinedAt = _clock.UtcNow },
            new ClassMember { ClassId = 1, UserId = 3, JoinedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.GetLeaderboardAsync("class", 1, 1, 20, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var items = result.Value!.Items;
        Assert.Equal(2, items.Count);
        Assert.Equal(2, items[0].UserId);     // Xp 800 > 600
        Assert.Equal(800, items[0].Value);
        Assert.Equal(3, items[1].UserId);
        Assert.Equal(600, items[1].Value);
        Assert.DoesNotContain(items, i => i.UserId == 1);   // user 1 không phải thành viên
    }

    // ── GP-T7: OrderRef/ContentRef = DSV{userId}T{months} (QR MB Bank) ──────────

    [Fact]
    public async Task UpgradePremium_CreatesOrder_WithDsvOrderRef()
    {
        var (service, db) = await SetupAsync(nameof(UpgradePremium_CreatesOrder_WithDsvOrderRef));

        var result = await service.UpgradePremiumAsync(1, new PremiumUpgradeRequest { PlanId = "3m" }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("DSV1T3", result.Value!.ContentRef);
        Assert.True(result.Value.OrderId > 0, "OrderId phải > 0 (đã lưu DB)");

        var order = await db.PremiumSubscriptions.AsNoTracking().FirstAsync(s => s.Id == result.Value.OrderId);
        Assert.Equal("DSV1T3", order.OrderRef);      // log giao dịch map được mã CK
        Assert.Equal(2, order.Status);               // chờ xác nhận chuyển khoản (GP-T7)
        Assert.Equal("3m", order.PlanId);
        Assert.Equal(_clock.UtcNow.AddMonths(3), result.Value.ExpiresAt);
    }

    [Theory]
    [InlineData("1m", "DSV1T1")]
    [InlineData("3m", "DSV1T3")]
    [InlineData("12m", "DSV1T12")]
    public async Task UpgradePremium_OrderRefFormat_MatchesPlan(string planId, string expectedRef)
    {
        var (service, db) = await SetupAsync($"UpgradePremium_OrderRefFormat_{planId}");

        var result = await service.UpgradePremiumAsync(1, new PremiumUpgradeRequest { PlanId = planId }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(expectedRef, result.Value!.ContentRef);

        var order = await db.PremiumSubscriptions.AsNoTracking().FirstAsync(s => s.Id == result.Value.OrderId);
        Assert.Equal(expectedRef, order.OrderRef);
    }
}
