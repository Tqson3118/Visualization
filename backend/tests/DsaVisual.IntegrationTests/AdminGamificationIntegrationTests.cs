using System.Net;
using System.Net.Http.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DsaVisual.IntegrationTests;

public sealed class AdminGamificationIntegrationTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    public AdminGamificationIntegrationTests(ApiTestFixture fixture) : base(fixture) { }

    [Fact(DisplayName = "Student accessing admin gamification settings returns 403 Forbidden")]
    public async Task GetSettings_AsStudent_ReturnsForbidden()
    {
        var student = await CreateUserAsync(role: UserRole.Student);
        using var client = CreateClientWithToken(student.Id, RoleNames.Student);

        var response = await client.GetAsync("/api/v1/admin/gamification/settings");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName = "Admin GET and PUT gamification settings updates config dynamically")]
    public async Task Admin_GetAndPut_GamificationSettings()
    {
        var admin = await CreateUserAsync(role: UserRole.Admin);
        using var client = CreateClientWithToken(admin.Id, RoleNames.Admin);

        // 1. GET initial settings
        var getRes = await client.GetAsync("/api/v1/admin/gamification/settings");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
        var initial = await ReadJsonAsync<GamificationSettingsDto>(getRes);
        Assert.NotNull(initial);

        // 2. PUT new settings
        var updatePayload = new GamificationSettingsDto
        {
            TheoryBaseXp = 60,
            QuizBaseXp = 55,
            CodelabBaseXp = 120,
            StreakBonusXp = 25,
            HeartsMaxFree = 12,
            HeartsMaxPremium = 35,
            HeartRegenMinutes = 25,
            SessionHours = 40
        };

        var putRes = await client.PutAsJsonAsync("/api/v1/admin/gamification/settings", updatePayload);
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var updated = await ReadJsonAsync<GamificationSettingsDto>(putRes);
        Assert.Equal(60, updated.TheoryBaseXp);
        Assert.Equal(120, updated.CodelabBaseXp);
        Assert.Equal(12, updated.HeartsMaxFree);
        Assert.Equal(35, updated.HeartsMaxPremium);

        // 3. Reset back to defaults
        var resetRes = await client.PostAsync("/api/v1/admin/gamification/settings/reset", null);
        Assert.Equal(HttpStatusCode.OK, resetRes.StatusCode);
        var reset = await ReadJsonAsync<GamificationSettingsDto>(resetRes);
        Assert.Equal(50, reset.TheoryBaseXp);
        Assert.Equal(100, reset.CodelabBaseXp);
    }

    [Fact(DisplayName = "PUT invalid gamification settings returns 400 Validation Error")]
    public async Task PutSettings_InvalidPayload_ReturnsBadRequest()
    {
        var admin = await CreateUserAsync(role: UserRole.Admin);
        using var client = CreateClientWithToken(admin.Id, RoleNames.Admin);

        var invalidPayload = new GamificationSettingsDto
        {
            TheoryBaseXp = -5,
            HeartsMaxFree = 0,
            HeartsMaxPremium = 0
        };

        var response = await client.PutAsJsonAsync("/api/v1/admin/gamification/settings", invalidPayload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
