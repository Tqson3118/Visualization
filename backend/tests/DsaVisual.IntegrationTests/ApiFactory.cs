using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// WebApplicationFactory&lt;Program&gt; với cấu hình test: connection string trỏ container test,
/// JWT secret test (≥ 32 ký tự), Serilog tối thiểu Warning (giảm ồn khi chạy test).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ApiFactory(string connectionString) => _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // UseSetting ở host-level — áp dụng TRƯỚC khi thân Program.cs chạy tiếp sau CreateBuilder
        // (ConfigureAppConfiguration có thể chạy trễ hơn việc đăng ký JwtBearer trong Program.cs).
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.UseSetting("DSA:Jwt:Secret", IntegrationTestBase.TestJwtSecret);
        builder.UseSetting("DSA:Cors:AllowedOrigins:0", "http://localhost:5173");
        builder.UseSetting("Serilog:MinimumLevel:Default", "Warning");
        builder.UseSetting("Serilog:MinimumLevel:Override:Microsoft.AspNetCore", "Warning");
        builder.UseSetting("Serilog:MinimumLevel:Override:Microsoft.EntityFrameworkCore", "Warning");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _connectionString,
                ["DSA:Jwt:Secret"] = IntegrationTestBase.TestJwtSecret,
                ["DSA:Cors:AllowedOrigins:0"] = "http://localhost:5173",
                // B0: mã OTP đăng ký cố định cho integration test (AuthService DevOtpCode)
                ["DSA:Auth:DevOtpCode"] = "000000",
                ["Serilog:MinimumLevel:Default"] = "Warning",
                ["Serilog:MinimumLevel:Override:Microsoft.AspNetCore"] = "Warning",
                ["Serilog:MinimumLevel:Override:Microsoft.EntityFrameworkCore"] = "Warning"
            });
        });
    }
}
