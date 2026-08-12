using System.Net;
using System.Text.Json;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// Integration tests Public API (TEST_PLAN §4.2 / TEST-API) — /api/v1/public/* KHÔNG cần token.
/// </summary>
public sealed class PublicApiTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    private const string BaseUrl = "/api/v1/public";

    public PublicApiTests(ApiTestFixture fixture) : base(fixture) { }

    [Fact(DisplayName = "GET /api/v1/public/site-info không token → 200 + contract")]
    public async Task GetSiteInfo_WithoutToken_Returns200()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/site-info");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("structures", out var structures) && structures.ValueKind == JsonValueKind.Number);
        Assert.True(root.TryGetProperty("algorithms", out var algorithms) && algorithms.ValueKind == JsonValueKind.Number);
        Assert.True(root.TryGetProperty("lessons", out var lessons) && lessons.ValueKind == JsonValueKind.Number);
    }

    [Fact(DisplayName = "GET /api/v1/public/faqs không token → 200 + mảng FAQ")]
    public async Task GetFaqs_WithoutToken_Returns200()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/faqs");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var array = doc.RootElement;
        Assert.Equal(JsonValueKind.Array, array.ValueKind);
        Assert.True(array.GetArrayLength() >= 1);
        Assert.True(array[0].TryGetProperty("question", out _));
        Assert.True(array[0].TryGetProperty("answer", out _));
    }
}
