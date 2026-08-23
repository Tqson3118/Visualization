using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.Application.DTOs;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    /// <summary>Màn 12 (Bảng xếp hạng) — GET /api/v1/leaderboard trả PagedResponse (FE gamification.ts fetchLeaderboard).</summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/leaderboard")]
    public class LeaderboardV2Controller : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;
        public LeaderboardV2Controller(IApplicationDbContext ctx) { _ctx = ctx; }

        private static readonly int[] LevelThresholds = { 0, 100, 300, 600, 1000, 1500, 2200, 3000, 3800, 4700, 5800, 7000, 8400, 10000, 12000, 14500 };
        private static int LevelFromXp(int xp) { var l = 1; for (var i = 0; i < LevelThresholds.Length; i++) if (xp >= LevelThresholds[i]) l = i + 1; else break; return l; }

        [HttpGet]
        [RequireJwtRole]
        public async Task<IActionResult> Get([FromQuery] string? tab, [FromQuery] string? classId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _ctx.Users.AsNoTracking().Where(u => u.Role == "Student").ToListAsync();
            var ordered = users.OrderByDescending(u => u.TotalXP).ToList();
            var itemList = ordered.Select((u, idx) => new
            {
                rank = idx + 1, userId = u.Id.ToString(), displayName = u.Username, avatarUrl = (string?)null,
                value = u.TotalXP, streak = (int?)u.StreakDays, level = (int?)LevelFromXp(u.TotalXP),
            }).Cast<object>().ToList();
            var paged = PagedResponse<object>.Create(itemList, page, pageSize);
            return Ok(new { items = paged.Items, page = paged.Page, pageSize = paged.PageSize, total = paged.Total, totalPages = paged.TotalPages });
        }
    }
}
