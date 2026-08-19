using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Asp.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.Domain.Engine;
using VisualizationDSA.Domain.Strategies;
using VisualizationDSA.Infrastructure.Data;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/concepts/gamification")]
    [EnableRateLimiting("api")]
    public class StatelessGamificationController : ControllerBase
    {
        private readonly GamificationStrategy _gamification;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMemoryCache _cache;

        public StatelessGamificationController(
            GamificationStrategy gamification, 
            ApplicationDbContext dbContext,
            IMemoryCache cache)
        {
            _gamification = gamification;
            _dbContext = dbContext;
            _cache = cache;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = JwtHelper.ExtractSubFromToken(Request);
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id))
            {
                var user = await _dbContext.Users
                    .Include(u => u.UserBadges).ThenInclude(ub => ub.Badge)
                    .FirstOrDefaultAsync(u => u.Id == id);
                if (user != null)
                {
                    return Ok(new StatelessUserProfile
                    {
                        UserId = user.Id.ToString(),
                        Username = user.Username,
                        TotalXp = user.TotalXP,
                        CurrentLevel = user.CurrentLevel,
                        LevelName = GamificationStrategy.GetLevelName(user.CurrentLevel),
                        StreakDays = user.StreakDays,
                        EarnedBadges = user.UserBadges.Select(ub => new StatelessBadgeDto
                        {
                            Id = ub.Badge.Id.ToString(),
                            Name = ub.Badge.Name,
                            Description = ub.Badge.Description,
                            Icon = ub.Badge.Icon,
                            Color = ub.Badge.Color,
                            EarnedAt = ub.EarnedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                        }).ToList()
                    });
                }
            }

            var topUser = await _dbContext.Users
                .Include(u => u.UserBadges).ThenInclude(ub => ub.Badge)
                .OrderByDescending(u => u.TotalXP)
                .FirstOrDefaultAsync();

            if (topUser != null)
            {
                return Ok(new StatelessUserProfile
                {
                    UserId = topUser.Id.ToString(),
                    Username = topUser.Username,
                    TotalXp = topUser.TotalXP,
                    CurrentLevel = topUser.CurrentLevel,
                    LevelName = GamificationStrategy.GetLevelName(topUser.CurrentLevel),
                    StreakDays = topUser.StreakDays,
                    EarnedBadges = topUser.UserBadges.Select(ub => new StatelessBadgeDto
                    {
                        Id = ub.Badge.Id.ToString(),
                        Name = ub.Badge.Name,
                        Description = ub.Badge.Description,
                        Icon = ub.Badge.Icon,
                        Color = ub.Badge.Color,
                        EarnedAt = ub.EarnedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    }).ToList()
                });
            }

            return Ok(new StatelessUserProfile());
        }

        [HttpPost("award-xp")]
        [RequireJwtRole("Teacher,Admin")]
        public async Task<IActionResult> AwardXp([FromBody] AwardXpRequest request)
        {
            if (request.Amount <= 0 || request.Amount > 500)
                return BadRequest(new { error = "INVALID_AMOUNT", message = "XP phải trong khoảng 1-500." });

            var userId = JwtHelper.ExtractSubFromToken(Request);
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id))
            {
                var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (dbUser != null)
                {
                    dbUser.AwardXP(request.Amount);
                    dbUser.RecordActivity();
                    await _dbContext.SaveChangesAsync();
                }
            }

            return Ok(new { success = true, amount = request.Amount, reason = request.Reason });
        }

        [HttpGet("badges")]
        public IActionResult GetBadges()
        {
            return Ok(_gamification.GetAllBadges());
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int limit = 10)
        {
            limit = Math.Clamp(limit, 1, 50);
            var cacheKey = $"StatelessGamification_Leaderboard_{limit}";

            if (!_cache.TryGetValue(cacheKey, out List<StatelessLeaderboardEntry>? leaderboard))
            {
                var dbUsers = await _dbContext.Users
                    .OrderByDescending(u => u.TotalXP)
                    .Take(limit)
                    .Select(u => new
                    {
                        u.Username,
                        u.TotalXP,
                        u.CurrentLevel,
                        u.StreakDays,
                        BadgeCount = u.UserBadges.Count
                    })
                    .ToListAsync();

                leaderboard = dbUsers.Select((u, index) => new StatelessLeaderboardEntry
                {
                    Rank = index + 1,
                    Username = u.Username,
                    TotalXp = u.TotalXP,
                    Level = u.CurrentLevel,
                    LevelName = GamificationStrategy.GetLevelName(u.CurrentLevel),
                    BadgeCount = u.BadgeCount,
                    StreakDays = u.StreakDays
                }).ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(15))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

                _cache.Set(cacheKey, leaderboard, cacheOptions);
            }

            return Ok(leaderboard);
        }

        [HttpGet("config")]
        [ResponseCache(Duration = 86400)]
        public IActionResult GetConfig()
        {
            return Ok(_gamification.GetConfig());
        }
    }

    public class AwardXpRequest
    {
        public int Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
