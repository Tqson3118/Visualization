using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [RequireJwtRole]
    public class AchievementsController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;
        public AchievementsController(IApplicationDbContext ctx) { _ctx = ctx; }

        /// <summary>GET /achievements — AchievementDto[]: tất cả huy hiệu + earnedAt (null = chưa đạt)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var uidStr = JwtHelper.ExtractSubFromToken(Request);
            var uid = Guid.TryParse(uidStr, out var g) ? g : Guid.Empty;
            var badges = await _ctx.Set<Badge>().OrderBy(b => b.Name).ToListAsync();
            var earned = await _ctx.Set<UserBadge>()
                .Where(ub => ub.UserId == uid)
                .Select(ub => new { ub.BadgeId, ub.EarnedAt })
                .ToListAsync();
            var result = badges.Select(b => new
            {
                id = b.Id.ToString(),
                code = Slug(b.Name),
                name = b.Name,
                description = b.Description,
                iconUrl = (string?)null,
                earnedAt = earned.FirstOrDefault(e => e.BadgeId == b.Id)?.EarnedAt.ToString("o"),
            }).ToList();
            return Ok(result);
        }

        private static string Slug(string s)
            => string.Concat(s.ToLowerInvariant().Split(' ')).Replace("-", "").Replace("đ", "d");
    }
}
