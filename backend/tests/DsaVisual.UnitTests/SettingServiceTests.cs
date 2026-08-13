using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test SettingService: GET map bảng Settings key-value → SystemSettingsDto (shape FE AdminSettingsView),
/// default khi thiếu key, PUT upsert + invalidate cache ngay (GetValueAsync đọc giá trị mới).
/// </summary>
public class SettingServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private static SettingService CreateService(AppDbContext db, SettingsCache? cache = null) =>
        new(db, cache ?? new SettingsCache(), new TestServices.FixedClock(), NullLogger<SettingService>.Instance);

    [Fact]
    public async Task GetAll_MapsKeysToSystemSettingsDto()
    {
        var db = TestServices.CreateInMemoryDb(nameof(GetAll_MapsKeysToSystemSettingsDto));
        db.Settings.AddRange(
            new Setting { Key = "site.name", Value = "DSA Visual" },
            new Setting { Key = "allowed.email.domains", Value = "university.edu.vn, fpt.edu.vn" },
            new Setting { Key = "password.policy.minLength", Value = "10" },
            new Setting { Key = "upload.maxSizeMb", Value = "20" });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        var settings = result.Value!;
        Assert.Equal("DSA Visual", settings.SiteName);
        Assert.Equal(["university.edu.vn", "fpt.edu.vn"], settings.AllowedDomains);
        Assert.Equal(10, settings.PasswordPolicy.MinLength);
        Assert.Equal(20, settings.UploadMaxMb);
        // Mặc định FE — không có key lưu phía backend
        Assert.Equal(10, settings.SandboxSeconds);
        Assert.Equal(64, settings.SandboxMemoryMb);
        Assert.True(settings.PasswordPolicy.RequireUppercase);
        Assert.True(settings.PasswordPolicy.RequireDigit);
        Assert.True(settings.PasswordPolicy.RequireSpecial);
    }

    [Fact]
    public async Task GetAll_MissingKeys_ReturnsDefaults()
    {
        var db = TestServices.CreateInMemoryDb(nameof(GetAll_MissingKeys_ReturnsDefaults));
        var service = CreateService(db);

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        var settings = result.Value!;
        Assert.Equal("DSA Visual", settings.SiteName);
        Assert.Empty(settings.AllowedDomains);
        Assert.Equal(8, settings.PasswordPolicy.MinLength);
        Assert.Equal(5, settings.UploadMaxMb);
    }

    [Fact]
    public async Task Update_UpsertsKeysAndInvalidatesCache()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Update_UpsertsKeysAndInvalidatesCache));
        db.Settings.Add(new Setting { Key = "site.name", Value = "Cũ" });
        await db.SaveChangesAsync();
        var cache = new SettingsCache();
        var service = CreateService(db, cache);

        var result = await service.UpdateAsync(1, new SystemSettingsDto
        {
            SiteName = "Mới",
            AllowedDomains = ["fpt.edu.vn"],
            PasswordPolicy = new PasswordPolicySettingsDto { MinLength = 12 },
            UploadMaxMb = 30
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);

        // DB upsert: key cũ cập nhật, key mới thêm
        var siteRow = await db.Settings.SingleAsync(s => s.Key == "site.name");
        Assert.Equal("Mới", siteRow.Value);
        Assert.Equal(1, siteRow.UpdatedBy);
        Assert.Equal("fpt.edu.vn", (await db.Settings.SingleAsync(s => s.Key == "allowed.email.domains")).Value);
        Assert.Equal("12", (await db.Settings.SingleAsync(s => s.Key == "password.policy.minLength")).Value);
        Assert.Equal("30", (await db.Settings.SingleAsync(s => s.Key == "upload.maxSizeMb")).Value);

        // Cache đã upsert — GetValueAsync trả giá trị mới ngay (không cần load lại DB)
        Assert.Equal("Mới", await service.GetValueAsync("site.name", CancellationToken.None));
        Assert.Equal("fpt.edu.vn", await service.GetValueAsync("allowed.email.domains", CancellationToken.None));
    }

    [Fact]
    public async Task Update_SandboxFieldsNotPersisted_DefaultsKeptOnGet()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Update_SandboxFieldsNotPersisted_DefaultsKeptOnGet));
        var service = CreateService(db);

        var result = await service.UpdateAsync(1, new SystemSettingsDto
        {
            SiteName = "DSA Visual",
            SandboxSeconds = 30,
            SandboxMemoryMb = 256
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var settings = (await service.GetAllAsync(CancellationToken.None)).Value!;
        Assert.Equal(10, settings.SandboxSeconds);
        Assert.Equal(64, settings.SandboxMemoryMb);
    }
}
