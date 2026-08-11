using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VisualizationDSA.Application.DTOs;
using VisualizationDSA.Application.DTOs.GemsShop;
using VisualizationDSA.Application.Services;
using VisualizationDSA.Application.Common.Interfaces;
using VisualizationDSA.Domain.Interfaces;
using VisualizationDSA.Infrastructure.Data;

namespace VisualizationDSA.WebApi.Controllers
{
    
    
    
    
    
    
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork           _unitOfWork;
        private readonly IGamificationService  _gamification;
        private readonly IGemsShopService      _gemsShopService;
        private readonly IWebHostEnvironment  _env;
        private readonly ApplicationDbContext  _dbContext;

        public UsersController(IUnitOfWork unitOfWork, IGamificationService gamification, IGemsShopService gemsShopService, IWebHostEnvironment env, ApplicationDbContext dbContext)
        {
            _unitOfWork       = unitOfWork;
            _gamification     = gamification;
            _gemsShopService  = gemsShopService;
            _env              = env;
            _dbContext        = dbContext;
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

        /// <summary>
        /// Trang bị Avatar Frame.
        /// PATCH /api/v1/users/me/avatar-frame
        /// </summary>
        [HttpPatch("me/avatar-frame")]
        public async Task<IActionResult> EquipAvatarFrame([FromBody] EquipAvatarFrameRequest request)
        {
            var userId = GetCurrentUserId();
            var success = await _gemsShopService.EquipAvatarFrameAsync(userId, request.FrameType);
            
            if (!success)
            {
                return BadRequest(new { success = false, error = "FRAME_NOT_OWNED_OR_USER_NOT_FOUND", message = "Bạn không sở hữu khung này hoặc có lỗi xảy ra." });
            }

            return Ok(new { success = true, avatarFrameType = request.FrameType });
        }

        /// <summary>
        /// Upload ảnh đại diện.
        /// POST /api/v1/users/me/avatar
        /// </summary>
        [HttpPost("me/avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn một file ảnh." });

            var url = await SaveUploadedImage(avatar);
            if (url == null)
                return BadRequest(new { message = "Chỉ chấp nhận file ảnh (.jpg, .png, .gif, .webp) tối đa 5MB." });

            var userId = GetCurrentUserId();
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

            user.SetAvatarUrl(url);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Cập nhật avatar thành công.", url = url });
        }

        /// <summary>
        /// Upload ảnh bìa hồ sơ.
        /// POST /api/v1/users/me/cover
        /// </summary>
        [HttpPost("me/cover")]
        public async Task<IActionResult> UploadCover([FromForm] IFormFile cover)
        {
            if (cover == null || cover.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn một file ảnh." });

            var url = await SaveUploadedImage(cover);
            if (url == null)
                return BadRequest(new { message = "Chỉ chấp nhận file ảnh (.jpg, .png, .gif, .webp) tối đa 5MB." });

            var userId = GetCurrentUserId();
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

            user.SetCoverUrl(url);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Cập nhật ảnh bìa thành công.", url = url });
        }

        /// <summary>
        /// Hoạt động gần đây của user (Dashboard).
        /// GET /api/v1/users/me/activity/recent?limit=10
        /// </summary>
        [HttpGet("me/activity/recent")]
        public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 10)
        {
            var userId = GetCurrentUserId();
            limit = Math.Clamp(limit, 1, 50);

            var activities = new List<object>();

            var lessonProgresses = await _dbContext.UserLessonProgresses
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CompletedAt)
                .Take(limit)
                .Select(p => new
                {
                    lessonId = p.LessonId,
                    status = p.Status,
                    completedAt = p.CompletedAt,
                })
                .ToListAsync();

            foreach (var lp in lessonProgresses)
            {
                if (lp.completedAt == null) continue;
                activities.Add(new
                {
                    id = $"lesson-{lp.lessonId}",
                    type = lp.status == "Completed" ? "lesson" : "progress",
                    title = lp.status == "Completed" ? "Hoàn thành bài học" : "Đang học bài",
                    description = "Cập nhật tiến độ bài học",
                    timestamp = lp.completedAt,
                });
            }

            var enrollments = await _dbContext.RoadmapEnrollments
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .Take(limit)
                .Select(e => new { e.RoadmapId, e.Status, e.EnrolledAt, e.CompletedAt })
                .ToListAsync();

            foreach (var enr in enrollments)
            {
                activities.Add(new
                {
                    id = $"enroll-{enr.RoadmapId}",
                    type = "enrollment",
                    title = enr.Status == "Completed" ? "Hoàn thành lộ trình" : "Đăng ký lộ trình",
                    description = "Roadmap",
                    timestamp = enr.CompletedAt ?? enr.EnrolledAt,
                });
            }

            return Ok(activities
                .OrderByDescending(a => ((dynamic)a).timestamp)
                .Take(limit));
        }

        private async Task<string?> SaveUploadedImage(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif" && ext != ".webp")
                return null;

            if (file.Length > 5 * 1024 * 1024)
                return null;

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{uniqueFileName}";
        }

        private Guid GetCurrentUserId()
        {
            
            
            
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                        ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            {
                
                throw new UnauthorizedAccessException("User identity claim không hợp lệ hoặc bị thiếu.");
            }
            return userId;
        }
    }
}
