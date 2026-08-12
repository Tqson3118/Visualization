using DsaVisual.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// Fixture cấp test class: mỗi test class có MỘT database riêng (DsaVisualTest_&lt;guid&gt;)
/// trên container dùng chung — cô lập dữ liệu tuyệt đối giữa các class, schema tạo bằng
/// migrations EF (Database.MigrateAsync) nên khớp 100% production.
/// </summary>
public sealed class ApiTestFixture : IAsyncLifetime
{
    public string DatabaseName { get; private set; } = string.Empty;
    public ApiFactory Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await MssqlFixture.EnsureStartedAsync();

        DatabaseName = $"DsaVisualTest_{Guid.NewGuid():N}";
        var connectionString = MssqlFixture.GetConnectionString(DatabaseName);

        // Migrations EF — database mới được tạo tự động nếu chưa tồn tại.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.MigrateAsync();
        }

        Factory = new ApiFactory(connectionString);
        Client = Factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }
}
