using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.Domain.Engine;
using VisualizationDSA.Domain.Entities;
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
        private readonly ApplicationDbContext _dbContext;
        private static readonly (int level, string name, int xpRequired)[] LevelTable =
        {
            (1, "Novice",       0),
            (2, "Explorer",     100),
            (3, "Learner",      300),
            (4, "Practitioner", 600),
            (5, "Expert",       1000),
            (6, "Master",       1500),
            (7, "Grandmaster",  2200),
            (8, "Legend",       3000),
        };

        public StatelessGamificationController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private static string GetLevelName(int level)
        {
            foreach (var entry in LevelTable)
                if (entry.level == level) return entry.name;
            return "Novice";
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await ResolveCurrentUserAsync();
            if (user == null)
                return NotFound(new { error = "USER_NOT_FOUND", message = "Người dùng không tồn tại." });

            var latest = await _dbContext.QuizAttempts
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.AttemptedAt)
                .Take(20)
                .ToListAsync();

            return Ok(new StatelessUserProfile
            {
                UserId = user.Id.ToString(),
                Username = user.Username,
                TotalXp = user.TotalXP,
                CurrentLevel = user.CurrentLevel,
                LevelName = GetLevelName(user.CurrentLevel),
                StreakDays = user.StreakDays,
                EarnedBadges = user.UserBadges.Select(ub => MapBadge(ub.Badge)).ToList(),
                RecentActivity = latest.Select(a => new StatelessXpEvent
                {
                    Type = a.Passed ? "QUIZ_COMPLETE" : "QUIZ_ATTEMPT",
                    Amount = a.Passed ? (a.Quiz?.XPReward ?? 0) : 0,
                    Description = $"Quiz '{(a.Quiz?.Title ?? "unknown")}' hoàn thành: {a.Score}/{a.MaxScore}",
                    Timestamp = a.AttemptedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }).ToList()
            });
        }

        [HttpPost("award-xp")]
        public async Task<IActionResult> AwardXp([FromBody] AwardXpRequest request)
        {
            if (request.Amount <= 0 || request.Amount > 500)
                return BadRequest(new { error = "INVALID_AMOUNT", message = "XP phải trong khoảng 1-500." });

            var user = await ResolveCurrentUserAsync();
            if (user == null)
                return NotFound(new { error = "USER_NOT_FOUND", message = "Người dùng không tồn tại." });

            user.AwardXP(request.Amount);
            user.RecordActivity();
            await _dbContext.SaveChangesAsync();

            var badges = await _dbContext.Badges.ToListAsync();
            foreach (var badge in badges)
            {
                if (user.UserBadges.Any(ub => ub.BadgeId == badge.Id)) continue;
                if (ShouldAwardBadge(user, badge))
                {
                    user.UserBadges.Add(new UserBadge(user.Id, badge.Id));
                }
            }
            await _dbContext.SaveChangesAsync();

            return Ok(new StatelessUserProfile
            {
                UserId = user.Id.ToString(),
                Username = user.Username,
                TotalXp = user.TotalXP,
                CurrentLevel = user.CurrentLevel,
                LevelName = GetLevelName(user.CurrentLevel),
                StreakDays = user.StreakDays,
                EarnedBadges = user.UserBadges.Select(ub => MapBadge(ub.Badge)).ToList(),
                RecentActivity = new List<StatelessXpEvent>()
            });
        }

        [HttpGet("badges")]
        public async Task<IActionResult> GetBadges()
        {
            var badges = await _dbContext.Badges.OrderBy(b => b.Name).ToListAsync();
            var user = await ResolveCurrentUserAsync();
            var earnedSet = user == null ? new HashSet<Guid>() : user.UserBadges.Select(ub => ub.BadgeId).ToHashSet();

            var result = badges.Select(b => new StatelessBadgeDto
            {
                Id = b.Id.ToString(),
                Name = b.Name,
                Description = b.Description,
                Icon = b.Icon,
                Color = b.Color,
                EarnedAt = earnedSet.Contains(b.Id) ? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") : string.Empty
            }).ToList();

            return Ok(result);
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int limit = 10)
        {
            limit = Math.Clamp(limit, 1, 50);
            var dbUsers = await _dbContext.Users
                .OrderByDescending(u => u.TotalXP)
                .Take(limit)
                .Select(u => new
                {
                    u.Username, u.TotalXP, u.CurrentLevel, u.StreakDays,
                    BadgeCount = u.UserBadges.Count
                })
                .ToListAsync();

            var leaderboard = dbUsers.Select((u, index) => new StatelessLeaderboardEntry
            {
                Rank = index + 1,
                Username = u.Username,
                TotalXp = u.TotalXP,
                Level = u.CurrentLevel,
                LevelName = GetLevelName(u.CurrentLevel),
                BadgeCount = u.BadgeCount,
                StreakDays = u.StreakDays
            }).ToList();

            return Ok(leaderboard);
        }

        [HttpGet("config")]
        [ResponseCache(Duration = 86400)]
        public async Task<IActionResult> GetConfig()
        {
            var badges = await _dbContext.Badges.OrderBy(b => b.Name).ToListAsync();
            return Ok(new
            {
                levels = LevelTable.Select(l => new { l.level, l.name, l.xpRequired }),
                badges = badges.Select(b => new { b.Id, b.Name, b.Description, b.Icon, b.Color }),
                xpEvents = new[]
                {
                    new { type = "QUIZ_COMPLETE",  defaultXp = 50,  description = "Hoàn thành một quiz" },
                    new { type = "MODULE_FINISH",  defaultXp = 100, description = "Hoàn thành một module học tập" },
                    new { type = "STREAK_BONUS",   defaultXp = 25,  description = "Bonus streak hàng ngày" },
                    new { type = "ACHIEVEMENT",    defaultXp = 200, description = "Đạt thành tích đặc biệt" },
                }
            });
        }

        private async Task<User?> ResolveCurrentUserAsync()
        {
            var id = JwtHelper.ExtractSubFromToken(Request);
            if (!string.IsNullOrEmpty(id) && Guid.TryParse(id, out var guidId))
            {
                var byToken = await _dbContext.Users
                    .Include(u => u.UserBadges).ThenInclude(ub => ub.Badge)
                    .AsSingleQuery()
                    .FirstOrDefaultAsync(u => u.Id == guidId);
                if (byToken != null) return byToken;
            }
            return await _dbContext.Users
                .Include(u => u.UserBadges).ThenInclude(ub => ub.Badge)
                .AsSingleQuery()
                .FirstOrDefaultAsync(u => u.Email == "demo@visualizationdsa.dev");
        }

        private bool ShouldAwardBadge(User user, Badge badge)
        {
            return badge.Name switch
            {
                "First Steps" => user.QuizAttempts.Count >= 1,
                "Sorting Wizard" => user.LearningProgresses.Any(lp => lp.ModuleId.Contains("sort")),
                "OOP Guru" => user.LearningProgresses.Any(lp => lp.ModuleId.Contains("oop")),
                "SOLID Master" => user.LearningProgresses.Any(lp => lp.ModuleId.Contains("solid")),
                "Pattern Hunter" => user.LearningProgresses.Any(lp => lp.ModuleId.Contains("pattern")),
                "Streak Keeper" => user.StreakDays >= 7,
                "System Architect" => user.LearningProgresses.Any(lp => lp.ModuleId.Contains("system")),
                "DSA Champion" => user.CurrentLevel >= 5,
                _ => false
            };
        }

        private static StatelessBadgeDto MapBadge(Badge b) => new()
        {
            Id = b.Id.ToString(),
            Name = b.Name,
            Description = b.Description,
            Icon = b.Icon,
            Color = b.Color
        };
    }

    public class AwardXpRequest
    {
        public int Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}