using DotNet.Testcontainers.Builders;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// SQL Server test container — MỘT container dùng chung cho toàn bộ collection "Mssql"
/// (mỗi test class tự tạo database riêng qua <see cref="GetConnectionString"/>).
/// Image đã có local: mcr.microsoft.com/mssql/server:2022-latest.
/// </summary>
public sealed class MssqlFixture : IAsyncLifetime
{
    public const string Image = "mcr.microsoft.com/mssql/server:2022-latest";
    public const string SaPassword = "DsaVisual@Test123";

    private static readonly Lazy<MsSqlContainer> ContainerLazy = new(() =>
        new MsSqlBuilder(Image)
            .WithPassword(SaPassword)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("SQL Server is now ready for client connections"))
            .Build());

    private static MsSqlContainer Container => ContainerLazy.Value;

    public Task InitializeAsync() => EnsureStartedAsync();

    public Task DisposeAsync() =>
        ContainerLazy.IsValueCreated ? Container.DisposeAsync().AsTask() : Task.CompletedTask;

    /// <summary>Chờ container sẵn sàng (kết nối SA thành công) trước khi test tạo database.</summary>
    public static async Task EnsureStartedAsync()
    {
        if (ContainerLazy.IsValueCreated)
        {
            return;
        }

        await Container.StartAsync();
        await WaitUntilSqlReadyAsync();
    }

    /// <summary>Connection string tới container với database riêng cho test class.</summary>
    public static string GetConnectionString(string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(Container.GetConnectionString())
        {
            InitialCatalog = databaseName,
            TrustServerCertificate = true,
            ConnectTimeout = 10
        };
        return builder.ConnectionString;
    }

    private static async Task WaitUntilSqlReadyAsync()
    {
        var builder = new SqlConnectionStringBuilder(Container.GetConnectionString())
        {
            InitialCatalog = "master",
            TrustServerCertificate = true,
            ConnectTimeout = 3
        };

        for (var attempt = 0; attempt < 60; attempt++)
        {
            try
            {
                await using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();
                return;
            }
            catch (SqlException)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        throw new TimeoutException("SQL Server container chưa sẵn sàng sau 120 giây");
    }
}

/// <summary>Collection dùng chung — 1 container, các test class trong collection chạy tuần tự.</summary>
[CollectionDefinition("Mssql")]
public sealed class MssqlCollection : ICollectionFixture<MssqlFixture>;
