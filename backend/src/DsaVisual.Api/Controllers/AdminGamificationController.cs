using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v1/admin/gamification")]
[Authorize(Roles = "ADMIN")]
public class AdminGamificationController(IGamificationConfigService configService, AppDbContext db) : ApiControllerBase
{
    private readonly IGamificationConfigService _configService = configService;
    private readonly AppDbContext _db = db;

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
        var oldSettings = _configService.GetSettings();
        var updated = await _configService.UpdateSettingsAsync(request, ct);

        // Đồng bộ HeartsMax cho người dùng trong cơ sở dữ liệu:
        // Theo yêu cầu: CHỈ nâng HeartsMax, GIỮ NGUYÊN số tim hiện tại (Hearts); chỉ clamp nếu giảm max tim.
        var now = DateTime.UtcNow;

        if (request.HeartsMaxFree != oldSettings.HeartsMaxFree)
        {
            var newFree = request.HeartsMaxFree;
            await _db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE Users 
                SET HeartsMax = {newFree},
                    LastHeartAt = {now},
                    Hearts = CASE WHEN Hearts > {newFree} THEN {newFree} ELSE Hearts END
                WHERE (PremiumUntil IS NULL OR PremiumUntil <= {now}) AND Role = 0
            """, ct);
        }

        if (request.HeartsMaxPremium != oldSettings.HeartsMaxPremium)
        {
            var newPrem = request.HeartsMaxPremium;
            await _db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE Users 
                SET HeartsMax = {newPrem},
                    LastHeartAt = {now},
                    Hearts = CASE WHEN Hearts > {newPrem} THEN {newPrem} ELSE Hearts END
                WHERE (PremiumUntil > {now} OR Role = 1 OR Role = 3)
            """, ct);
        }

        return Ok(updated);
    }

    /// <summary>Khôi phục cấu hình Gamification về mặc định ban đầu (chỉ Admin).</summary>
    [HttpPost("settings/reset")]
    public async Task<ActionResult<GamificationSettingsDto>> ResetSettings(CancellationToken ct)
    {
        var reset = await _configService.ResetToDefaultsAsync(ct);
        var now = DateTime.UtcNow;

        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE Users 
            SET HeartsMax = {reset.HeartsMaxFree},
                Hearts = CASE WHEN Hearts > {reset.HeartsMaxFree} THEN {reset.HeartsMaxFree} ELSE Hearts END
            WHERE (PremiumUntil IS NULL OR PremiumUntil <= {now}) AND Role = 0
        """, ct);

        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE Users 
            SET HeartsMax = {reset.HeartsMaxPremium},
                Hearts = CASE WHEN Hearts > {reset.HeartsMaxPremium} THEN {reset.HeartsMaxPremium} ELSE Hearts END
            WHERE (PremiumUntil > {now} OR Role = 1 OR Role = 3)
        """, ct);

        return Ok(reset);
    }
}
