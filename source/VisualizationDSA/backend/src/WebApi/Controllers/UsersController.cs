using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VisualizationDSA.Application.DTOs;
using VisualizationDSA.Application.Services;
using VisualizationDSA.Domain.Interfaces;
using VisualizationDSA.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    
    
    
    
    
    
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [RequireJwtRole]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork           _unitOfWork;
        private readonly IGamificationService  _gamification;
        private readonly IApplicationDbContext _ctx;

        public UsersController(IUnitOfWork unitOfWork, IGamificationService gamification, IApplicationDbContext ctx)
        {
            _unitOfWork   = unitOfWork;
            _gamification = gamification;
            _ctx          = ctx;
        }

        
        
        
        
        [HttpGet("me/progress")]
        public async Task<ActionResult<UserProgressDto>> GetMyProgress()
        {
            var userId = GetCurrentUserId();
            
            
            var progress = await _unitOfWork.Users.GetUserProgressDomainModelAsync(userId);
            if (progress == null) return NotFound();

            
            var stats = _gamification.CalculateUserProgressStats(progress);

            
            return Ok(new UserProgressDto
            {
                TotalXP              = stats.TotalXP,
                CurrentLevel         = stats.CurrentLevel,
                XpToNextLevel        = stats.XpToNextLevel,
                LevelProgressPercent = stats.LevelProgressPercent,
                BadgesEarned         = stats.BadgesEarned,
                IsPremium            = progress.IsPremium,
                ModulesCompleted     = stats.ModulesCompleted,
                CurrentStreak        = stats.CurrentStreak,
                CompletedModuleIds   = progress.CompletedModuleIds,
                Badges               = progress.Badges
                                            .Select(ub => new BadgeDto
                                            {
                                                Id          = ub.BadgeId,
                                                Name        = ub.Name,
                                                Description = ub.Description,
                                                Icon        = ub.Icon,
                                                Color       = ub.Color,
                                                EarnedAt    = ub.EarnedAt,
                                            }).ToList()
            });
        }

        
        
        
        
        [HttpPost("me/xp")]
        public async Task<IActionResult> SyncXP([FromBody] XPAwardRequest request)
        {
            var userId = GetCurrentUserId();
            await _gamification.AwardXPAsync(userId, request.Amount, request.Reason);
            await _gamification.CheckAndAwardBadgesAsync(userId);

            var stats = await _gamification.GetUserProgressAsync(userId);
            return Ok(new
            {
                message      = $"Đã cộng {request.Amount} XP.",
                totalXP      = stats.TotalXP,
                currentLevel = stats.CurrentLevel,
            });
        }

        
        
        
        
        [HttpPost("me/modules/{moduleId}")]
        public async Task<IActionResult> CompleteModule(string moduleId)
        {
            var userId = GetCurrentUserId();
            await _gamification.CompleteModuleAsync(userId, moduleId);
            return NoContent();
        }

        
        
        
        
        [HttpGet("me/badges")]
        public async Task<ActionResult<IEnumerable<BadgeDto>>> GetMyBadges()
        {
            var userId = GetCurrentUserId();
            var user   = await _unitOfWork.Users.GetByIdWithDetailsAsync(userId, track: false);
            if (user == null) return NotFound();

            var badges = user.UserBadges.Select(ub => new BadgeDto
            {
                Id          = ub.BadgeId,
                Name        = ub.Badge?.Name        ?? string.Empty,
                Description = ub.Badge?.Description ?? string.Empty,
                Icon        = ub.Badge?.Icon        ?? string.Empty,
                Color       = ub.Badge?.Color       ?? string.Empty,
                EarnedAt    = ub.EarnedAt,
            });

            return Ok(badges);
        }

        
        [HttpGet("{id}/progress")]
        [VisualizationDSA.WebApi.Filters.RequireJwtRole("Admin")]
        public async Task<ActionResult> GetUserProgress(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdWithDetailsAsync(id, track: false);
            if (user == null) return NotFound();

            return Ok(new
            {
                totalXP          = user.TotalXP,
                currentLevel     = user.CurrentLevel,
                streakDays       = user.StreakDays,
                badgesEarned     = user.UserBadges.Count,
                modulesCompleted = user.LearningProgresses.Count,
                completedModuleIds = user.LearningProgresses.Select(lp => lp.ModuleId).ToList(),
                isPremium        = user.IsPremium,
            });
        }


        // ── Admin user management (FE admin.ts — Màn 21 /admin/users) ──────────────

        private static string RoleDto(string role)
            => role switch
            {
                "Admin" => "ADMIN",
                "Teacher" => "TEACHER",
                "PendingTeacher" => "TEACHER_PENDING",
                _ => "STUDENT",
            };

        /// <summary>GET /users?page&pageSize&role&status&q — PagedResponse&lt;AdminUserDto&gt;</summary>
        [HttpGet]
        [RequireJwtRole("Admin")]
        public async Task<ActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] string? role = null, [FromQuery] string? status = null, [FromQuery] string? q = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _ctx.Users.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(role))
            {
                var r = role.ToUpperInvariant() switch { "ADMIN" => "Admin", "TEACHER" => "Teacher", "TEACHER_PENDING" => "PendingTeacher", _ => "Student" };
                query = query.Where(u => u.Role == r);
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                var active = status.Equals("active", StringComparison.OrdinalIgnoreCase);
                query = query.Where(u => u.IsActive == active);
            }
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(u => u.Username.Contains(q) || u.Email.Contains(q));

            var total = await query.CountAsync();
            var users = await query.OrderBy(u => u.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = users.Select(u => new
            {
                id = u.Id.ToString(),
                displayName = u.Username,
                email = u.Email,
                role = RoleDto(u.Role),
                isActive = u.IsActive,
                createdAt = u.CreatedAt.ToString("o"),
                avatarUrl = (string?)null,
                department = u.Role == "PendingTeacher" ? "Khoa CNTT" : (string?)null,
                staffCode = u.Role == "PendingTeacher" ? "GV" + Convert.ToString(u.CreatedAt.Millisecond) : (string?)null,
            }).ToList();
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
            return Ok(new { items, page, pageSize, total, totalPages });
        }

        /// <summary>GET /users/{id} — AdminUserDto đầy đủ</summary>
        [HttpGet("{id:guid}")]
        [RequireJwtRole("Admin,Teacher")]
        public async Task<ActionResult> GetUser(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdWithDetailsAsync(id, track: false);
            if (user == null) return NotFound();
            var attempts = user.QuizAttempts.Count;
            return Ok(new
            {
                id = user.Id.ToString(),
                displayName = user.Username,
                email = user.Email,
                role = RoleDto(user.Role),
                isActive = user.IsActive,
                createdAt = user.CreatedAt.ToString("o"),
                avatarUrl = (string?)null,
                xp = user.TotalXP,
                level = user.CurrentLevel,
                streakDays = user.StreakDays,
                gems = 0,
                hearts = user.Hearts,
                lessonsCompletedCount = user.UserLessonProgresses.Count(p => p.Status == "Completed"),
                exercisesPassedCount = attempts,
                joinedClassesCount = 0,
            });
        }

        /// <summary>PUT /users/{id}/status — { isActive }</summary>
        [HttpPut("{id}/status")]
        [RequireJwtRole("Admin")]
        public async Task<IActionResult> SetStatus(Guid id, [FromBody] StatusRequest req)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return NotFound();
            user.SetActiveStatus(req.IsActive);
            await _unitOfWork.CommitAsync();
            return NoContent();
        }

        /// <summary>PUT /users/{id}/role — { role: STUDENT | TEACHER }</summary>
        [HttpPut("{id}/role")]
        [RequireJwtRole("Admin")]
        public async Task<IActionResult> SetRole(Guid id, [FromBody] RoleRequest req)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return NotFound();
            var role = req.Role?.ToUpperInvariant() == "TEACHER" ? "Teacher" : "Student";
            user.SetRole(role);
            await _unitOfWork.CommitAsync();
            return NoContent();
        }

        /// <summary>POST /users/{id}/approve-teacher — { approve, reason }</summary>
        [HttpPost("{id}/approve-teacher")]
        [RequireJwtRole("Admin")]
        public async Task<IActionResult> ApproveTeacher(Guid id, [FromBody] ApproveRequest req)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return NotFound();
            user.SetRole(req?.Approve == true ? "Teacher" : "Student");
            await _unitOfWork.CommitAsync();
            return NoContent();
        }

        /// <summary>POST /users/{id}/reset-password</summary>
        [HttpPost("{id}/reset-password")]
        [RequireJwtRole("Admin")]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return NotFound();
            user.ChangePassword(HashPasswordSHA256("Fpt@2026"));
            await _unitOfWork.CommitAsync();
            return NoContent();
        }

        /// <summary>DELETE /users/{id}</summary>
        [HttpDelete("{id:guid}")]
        [RequireJwtRole("Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return NotFound();
            user.SetActiveStatus(false);
            await _unitOfWork.CommitAsync();
            return NoContent();
        }

        private static string HashPasswordSHA256(string password)
        {
            var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password + "visualizationdsa-salt"));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        public class StatusRequest { public bool IsActive { get; set; } }
        public class RoleRequest { public string? Role { get; set; } }
        public class ApproveRequest { public bool Approve { get; set; } public string? Reason { get; set; } }

        private Guid GetCurrentUserId()
        {
            // [RequireJwtRole] không populate HttpContext.User — phải đọc từ token.
            var claim = JwtHelper.ExtractSubFromToken(Request);

            if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            {
                
                throw new UnauthorizedAccessException("User identity claim không hợp lệ hoặc bị thiếu.");
            }
            return userId;
        }
    }
}
