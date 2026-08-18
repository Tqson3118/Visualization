using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DsaVisual.UnitTests;

public class ShopAtomicTests
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
            Gems = 100,
            Hearts = 5,
            HeartsMax = 5,
            LastHeartAt = _clock.UtcNow,
            CreatedAt = _clock.UtcNow
        });

        db.ShopItems.Add(new ShopItem
        {
            Id = 1,
            ItemKey = "heart_refill",
            Name = "Bình hồi phục tim",
            PriceGems = 50,
            MaxStack = 5,
            Type = 0
        });

        db.ShopItems.Add(new ShopItem
        {
            Id = 2,
            ItemKey = "vip_badge",
            Name = "Huy hiệu VIP",
            PriceGems = 200,
            MaxStack = 1,
            Type = 1
        });

        await db.SaveChangesAsync();

        var service = TestServices.CreateGamificationService(db, _clock);
        return (service, db);
    }

    [Fact]
    public async Task BuyItem_SufficientGems_DeductsGemsAndAddsInventory()
    {
        var (service, db) = await SetupAsync(nameof(BuyItem_SufficientGems_DeductsGemsAndAddsInventory));

        var result = await service.BuyItemAsync(1, 1, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(50, result.Value!.GemsLeft);

        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(50, user.Gems);

        var inventory = await db.UserInventory.AsNoTracking().FirstOrDefaultAsync(i => i.UserId == 1 && i.ItemId == 1);
        Assert.NotNull(inventory);
        Assert.Equal(1, inventory.Quantity);
    }

    [Fact]
    public async Task BuyItem_InsufficientGems_ReturnsErrorAndDoesNotDeduct()
    {
        var (service, db) = await SetupAsync(nameof(BuyItem_InsufficientGems_ReturnsErrorAndDoesNotDeduct));

        var result = await service.BuyItemAsync(1, 2, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.INSUFFICIENT_GEMS, result.ErrorCode);

        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(100, user.Gems);
    }

    [Fact]
    public async Task BuyItem_ItemNotFound_ReturnsNotFound()
    {
        var (service, _) = await SetupAsync(nameof(BuyItem_ItemNotFound_ReturnsNotFound));

        var result = await service.BuyItemAsync(1, 999, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NOT_FOUND, result.ErrorCode);
    }
}
