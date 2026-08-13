using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// TEST TÁI HIỆN #13 (THAP): JWT secret &lt; 32 ký tự phải fail-fast khi khởi động
/// (docs/work/backend-audit/findings-security.md #13 — Program.cs:39-45 chỉ check IsNullOrWhiteSpace).
/// Không cần DB → không nằm trong collection "Mssql".
/// </summary>
public sealed class JwtSecretStartupTests
{
    [Fact(DisplayName = "REPRO #13: JWT secret ngắn (<32 ký tự) → khởi động app phải throw (hiện khởi động OK)")]
    public void Startup_ShortJwtSecret_Throws()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("DSA:Jwt:Secret", "too-short");
                builder.UseSetting("Serilog:MinimumLevel:Default", "Warning");
                builder.UseSetting("Serilog:MinimumLevel:Override:Microsoft.AspNetCore", "Warning");
            });

        // Hiện tại: Program.cs chỉ throw khi secret rỗng → secret 9 ký tự khởi động OK → CreateClient
        // không throw → test FAIL.
        // Sau fix (#13): check jwtSecret.Length < 32 → throw InvalidOperationException tại host build → PASS.
        Assert.ThrowsAny<Exception>(() => factory.CreateClient());
    }
}
