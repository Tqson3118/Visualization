using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace DsaVisual.UnitTests;

/// <summary>
/// Helper dựng service cho unit test: EF InMemory (phần lớn service) hoặc SQLite in-memory
/// (GamificationService — cần ExecuteSqlInterpolated cho trừ tim/claim atomic; InMemory không hỗ trợ raw SQL).
/// Ghi chú cách chạy: dotnet test backend/tests/DsaVisual.UnitTests
/// </summary>
internal static class TestServices
{
    public sealed class FixedClock : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; } = new(2026, 8, 12, 8, 0, 0, DateTimeKind.Utc);
    }

    private sealed class FakeEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "DsaVisual.UnitTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    public static IConfiguration CreateConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DSA:Jwt:Secret"] = "test-secret-key-0123456789abcdef0123456789abcdef",
            ["DSA:Jwt:Issuer"] = "DsaVisual.Api",
            ["DSA:Jwt:Audience"] = "DsaVisual.Frontend",
            ["DSA:Jwt:AccessTokenMinutes"] = "60",
            ["DSA:Jwt:RefreshTokenDays"] = "7",
            ["DSA:Auth:MaxLoginAttempts"] = "5",
            ["DSA:Auth:LockoutMinutes"] = "15"
        })
        .Build();

    public static AppDbContext CreateInMemoryDb(string name) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    public static (AppDbContext Db, SqliteConnection Connection) CreateSqliteDb()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options);
        db.Database.EnsureCreated();
        return (db, connection);
    }

    public static AuthService CreateAuthService(AppDbContext db, FixedClock clock, string dbName)
    {
        var config = CreateConfig();
        var cache = new SettingsCache();
        var settings = new SettingService(db, cache, clock, NullLogger<SettingService>.Instance);
        return new AuthService(
            db,
            new TokenService(config),
            clock,
            config,
            settings,
            new LoginAttemptTracker(clock, config),
            NullLogger<AuthService>.Instance);
    }

    public static ExerciseService CreateExerciseService(AppDbContext db, FixedClock clock) =>
        new(db, clock, new SubmissionLockRegistry(), NullLogger<ExerciseService>.Instance);

    public static TopicService CreateTopicService(AppDbContext db, FixedClock clock) =>
        new(db, clock, NullLogger<TopicService>.Instance);

    public static GamificationService CreateGamificationService(AppDbContext db, FixedClock clock)
    {
        var config = CreateConfig();
        var catalog = new SimulationCatalogService(config, new FakeEnvironment(), NullLogger<SimulationCatalogService>.Instance);
        return new GamificationService(db, clock, catalog, NullLogger<GamificationService>.Instance);
    }
}
