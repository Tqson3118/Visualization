using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v1/admin/gamification")]
[Authorize(Roles = "ADMIN")]
public class AdminGamificationController(IGamificationConfigService configService) : ApiControllerBase
{
    private readonly IGamificationConfigService _configService = configService;

    /// <summary>Lấy toàn bộ cấu hình Gamification hiện tại (chỉ Admin).</summary>
    [HttpGet("settings")]
    public ActionResult<GamificationSettingsDto> GetSettings()
    {
        return Ok(_configService.GetSettings());
    }

    /// <summary>Cập nhật cấu hình Gamification (chỉ Admin).</summary>
    [HttpPut("settings")]
    public async Task<ActionResult<GamificationSettingsDto>> UpdateSettings(
        [FromBody] GamificationSettingsDto request, CancellationToken ct)
    {
        var updated = await _configService.UpdateSettingsAsync(request, ct);
        return Ok(updated);
    }

    /// <summary>Khôi phục cấu hình Gamification về mặc định ban đầu (chỉ Admin).</summary>
    [HttpPost("settings/reset")]
    public async Task<ActionResult<GamificationSettingsDto>> ResetSettings(CancellationToken ct)
    {
        var reset = await _configService.ResetToDefaultsAsync(ct);
        return Ok(reset);
    }
}
