using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.Application.Common.Interfaces;
using VisualizationDSA.Application.DTOs;
using VisualizationDSA.Application.Services;
using VisualizationDSA.Domain.Engine;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Infrastructure.Data;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/concepts/auth")]
    public class StatelessAuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;

        public StatelessAuthController(IAuthService authService, ApplicationDbContext dbContext, IEmailService emailService)
        {
            _authService = authService;
            _dbContext = dbContext;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<StatelessAuthResponse>> Register([FromBody] StatelessRegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(new RegisterRequest
                {
                    Email = request.Email,
                    Username = request.Username,
                    Password = request.Password
                });
                return Ok(MapAuthResponse(response));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "REGISTRATION_FAILED", message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<StatelessAuthResponse>> Login([FromBody] StatelessLoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(new LoginRequest
                {
                    Email = request.Email,
                    Password = request.Password
                });
                return Ok(MapAuthResponse(response));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "LOGIN_FAILED", message = ex.Message });
            }
        }

        [HttpPost("google-login")]
        public async Task<ActionResult<StatelessAuthResponse>> GoogleLogin([FromBody] StatelessGoogleLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.IdToken) && string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { error = "INVALID_INPUT", message = "Thiếu idToken hoặc email từ Google." });
                }

                var clientId = Request.HttpContext.RequestServices.GetService(typeof(IConfiguration)) is Microsoft.Extensions.Configuration.IConfiguration cfg
                    ? cfg["Google__ClientId"] ?? cfg["Google:ClientId"]
                    : null;

                // Validate idToken với Google tokeninfo (chỉ khi có client id thật; placeholder → skip validate)
                string? email = request.Email;
                string? name = request.Name;
                string? subject = request.GoogleSubject;
                if (!string.IsNullOrWhiteSpace(request.IdToken) &&
                    clientId != null && !clientId.StartsWith("CHANGE_ME"))
                {
                    using var http = new System.Net.Http.HttpClient();
                    var resp = await http.GetStringAsync(
                        $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(request.IdToken)}");
                    var payload = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(resp);
                    if (payload.TryGetProperty("email", out var e) && e.ValueKind == System.Text.Json.JsonValueKind.String)
                        email = e.GetString();
                    if (payload.TryGetProperty("name", out var n) && n.ValueKind == System.Text.Json.JsonValueKind.String)
                        name = n.GetString();
                    if (payload.TryGetProperty("sub", out var s) && s.ValueKind == System.Text.Json.JsonValueKind.String)
                        subject = s.GetString();
                    if (string.IsNullOrWhiteSpace(email))
                        return Unauthorized(new { error = "GOOGLE_INVALID", message = "Không xác thực được tài khoản Google." });
                }

                var response = await _authService.GoogleLoginAsync(email ?? "", name ?? "", subject ?? "");
                return Ok(MapAuthResponse(response));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "GOOGLE_LOGIN_FAILED", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "GOOGLE_ERROR", message = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<StatelessAuthResponse>> Refresh([FromBody] StatelessRefreshRequest request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(MapAuthResponse(response));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = "REFRESH_FAILED", message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = "USER_NOT_FOUND", message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] StatelessRefreshRequest request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return NoContent();
        }

        [HttpGet("me")]
        public async Task<ActionResult<StatelessUserDto>> GetMe()
        {
            var currentUserId = JwtHelper.ExtractSubFromToken(Request);
            if (string.IsNullOrWhiteSpace(currentUserId) || !Guid.TryParse(currentUserId, out _))
                return Unauthorized(new { error = "UNAUTHORIZED", message = "Yêu cầu đăng nhập để xem hồ sơ." });

            try
            {
                var user = await _authService.GetCurrentUserAsync(currentUserId);
                return Ok(MapUserDto(user));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = "USER_NOT_FOUND", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "INVALID_USER_ID", message = ex.Message });
            }
        }

        [HttpGet("progress")]
        public async Task<ActionResult<StatelessUserProgressDto>> GetProgress()
        {
            var currentUserId = JwtHelper.ExtractSubFromToken(Request);
            if (string.IsNullOrWhiteSpace(currentUserId) || !Guid.TryParse(currentUserId, out _))
                return Unauthorized(new { error = "UNAUTHORIZED", message = "Yêu cầu đăng nhập để xem tiến độ." });

            try
            {
                if (!Guid.TryParse(currentUserId, out var dbUserId))
                    return BadRequest(new { error = "INVALID_USER_ID", message = "ID người dùng không hợp lệ." });

                var user = await _dbContext.Users
                    .Include(u => u.UserBadges).ThenInclude(ub => ub.Badge)
                    .Include(u => u.LearningProgresses)
                    .AsSingleQuery()
                    .FirstOrDefaultAsync(u => u.Id == dbUserId);

                if (user == null)
                    return NotFound(new { error = "USER_NOT_FOUND", message = "Người dùng không tồn tại." });

                var thresholds = new[] { 0, 100, 300, 600, 1000, 1500, 2200, 3000 };
                var currentIdx = Math.Min(user.CurrentLevel - 1, thresholds.Length - 1);
                var nextIdx = Math.Min(user.CurrentLevel, thresholds.Length - 1);
                var currentThreshold = thresholds[currentIdx];
                var nextThreshold = thresholds[nextIdx];
                var xpInLevel = user.TotalXP - currentThreshold;
                var xpForLevel = nextThreshold - currentThreshold;
                var progressPercent = xpForLevel > 0 ? Math.Min(100, (int)(xpInLevel * 100.0 / xpForLevel)) : 100;

                var completedModules = user.LearningProgresses.Select(lp => lp.ModuleId).Where(m => !string.IsNullOrEmpty(m)).ToList();

                return Ok(new StatelessUserProgressDto
                {
                    TotalXP = user.TotalXP,
                    CurrentLevel = user.CurrentLevel,
                    XpToNextLevel = Math.Max(0, nextThreshold - user.TotalXP),
                    LevelProgressPercent = progressPercent,
                    BadgesEarned = user.UserBadges.Count,
                    ModulesCompleted = completedModules.Count,
                    CurrentStreak = user.StreakDays,
                    CompletedModuleIds = completedModules,
                    Badges = user.UserBadges.Select(ub => MapBadge(ub.Badge)).ToList(),
                    IsPremium = user.IsPremium
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "INVALID_USER_ID", message = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<StatelessUserDto>> UpdateProfile([FromBody] StatelessUpdateProfileRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserId) || !Guid.TryParse(request.UserId, out var dbUserId))
                    return BadRequest(new { error = "INVALID_USER_ID", message = "Thiếu userId hợp lệ." });

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == dbUserId);
                if (user == null)
                    return NotFound(new { error = "USER_NOT_FOUND", message = "Người dùng không tồn tại." });

                if (!string.IsNullOrWhiteSpace(request.Username))
                {
                    var dup = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.Id != dbUserId);
                    if (dup != null)
                        return BadRequest(new { error = "UPDATE_FAILED", message = "Username này đã được sử dụng." });
                    user.UpdateUsername(request.Username);
                }

                await _dbContext.SaveChangesAsync();
                var userDto = await _authService.GetCurrentUserAsync(dbUserId.ToString());
                return Ok(MapUserDto(userDto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = "USER_NOT_FOUND", message = ex.Message });
            }
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] StatelessChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { error = "INVALID_INPUT", message = "Mật khẩu hiện tại và mật khẩu mới không được để trống." });

            if (request.NewPassword.Length < 8)
                return BadRequest(new { error = "INVALID_INPUT", message = "Mật khẩu mới phải có ít nhất 8 ký tự." });

            var currentUserId = JwtHelper.ExtractSubFromToken(Request);
            if (string.IsNullOrWhiteSpace(currentUserId) || !Guid.TryParse(currentUserId, out var dbUserId))
                return Unauthorized(new { error = "UNAUTHORIZED", message = "Yêu cầu đăng nhập để đổi mật khẩu." });

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == dbUserId);
            if (user == null)
                return NotFound(new { error = "USER_NOT_FOUND", message = "Không tìm thấy người dùng." });

            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
                return BadRequest(new { error = "INCORRECT_PASSWORD", message = "Mật khẩu hiện tại không chính xác." });

            var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
            user.ChangePassword(newHash);
            user.RecordActivity();
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        // UC-03 — Forgot password: sinh token 15 phút, trả link (chưa có email service → trả token trong Development)
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] StatelessForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { error = "INVALID_INPUT", message = "Email không được để trống." });

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (user == null)
            {
                // Luôn trả success — chống enum email (UC-03)
                return Ok(new { success = true });
            }

            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N").Substring(0, 8);
            user.SetPasswordResetToken(token, TimeSpan.FromMinutes(15));
            await _dbContext.SaveChangesAsync();

            var env = HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
            bool isDev = env != null && env.IsDevelopment();

            var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?token={token}";
            if (_emailService.IsConfigured)
            {
                // Production: gửi email thật (key SMTP trong .env — PM điền)
                await _emailService.SendAsync(
                    user.Email,
                    "Đặt lại mật khẩu — VisualizationDSA",
                    $"<p>Chào {user.Username},</p><p>Bấm link để đặt lại mật khẩu (hiệu lực 15 phút):</p>" +
                    $"<p><a href=\"{resetLink}\">{resetLink}</a></p>" +
                    "<p>Nếu bạn không yêu cầu, bỏ qua email này.</p>");
            }

            var response = new Dictionary<string, object> { ["success"] = true };
            if (isDev && !_emailService.IsConfigured)
            {
                // Dev-only: chưa có SMTP → trả token để test (production gửi email)
                response["resetToken"] = token;
                response["resetLink"] = resetLink;
            }
            return Ok(response);
        }

        // UC-03 — Reset password với token
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] StatelessResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { error = "INVALID_INPUT", message = "Token và mật khẩu mới không được để trống." });

            if (request.NewPassword.Length < 8)
                return BadRequest(new { error = "INVALID_INPUT", message = "Mật khẩu mới phải có ít nhất 8 ký tự." });

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            if (user == null || !user.IsPasswordResetTokenValid(request.Token))
                return BadRequest(new { error = "INVALID_TOKEN", message = "Token không hợp lệ hoặc đã hết hạn." });

            var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
            user.ChangePassword(newHash);
            user.ClearPasswordResetToken();
            user.RecordActivity();
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Mật khẩu đã được đặt lại thành công!" });
        }

        [HttpPost("award-xp")]
        public async Task<ActionResult<StatelessUserDto>> AwardXP([FromBody] StatelessXpAwardRequest request)
        {
            var currentUserId = JwtHelper.ExtractSubFromToken(Request);
            if (string.IsNullOrWhiteSpace(currentUserId) || !Guid.TryParse(currentUserId, out var dbUserId))
                return Unauthorized(new { error = "UNAUTHORIZED", message = "Yêu cầu đăng nhập." });

            try
            {
                if (request.Amount <= 0)
                    return BadRequest(new { error = "INVALID_AMOUNT", message = "Số XP phải lớn hơn 0." });

                var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == dbUserId);
                if (dbUser == null)
                    return NotFound(new { error = "USER_NOT_FOUND", message = "Người dùng không tồn tại." });

                dbUser.AwardXP(request.Amount);
                dbUser.RecordActivity();
                await _dbContext.SaveChangesAsync();

                var userDto = await _authService.GetCurrentUserAsync(dbUserId.ToString());
                return Ok(MapUserDto(userDto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "INVALID_AMOUNT", message = ex.Message });
            }
        }

        [HttpGet("demo-credentials")]
        public ActionResult<object> GetDemoCredentials()
        {
            return Ok(new
            {
                message = "Tài khoản demo để kiểm thử",
                email = "demo@visualizationdsa.dev",
                password = "Demo@2024",
                note = "Dữ liệu được lưu vĩnh viễn vào PostgreSQL qua DbSeeder khi khởi động."
            });
        }

        private static StatelessAuthResponse MapAuthResponse(AuthResponse r) => new()
        {
            AccessToken = r.AccessToken,
            RefreshToken = r.RefreshToken,
            ExpiresIn = r.ExpiresIn,
            User = MapUserDto(r.User)
        };

        private static StatelessUserDto MapUserDto(UserDto u) => new()
        {
            Id = u.Id.ToString(),
            Email = u.Email,
            Username = u.Username,
            TotalXP = u.TotalXP,
            CurrentLevel = u.CurrentLevel,
            StreakDays = u.StreakDays,
            CreatedAt = u.CreatedAt,
            Badges = u.Badges.Select(b => new StatelessBadgeInfoDto
            {
                Id = b.Id.ToString(),
                Name = b.Name,
                Description = b.Description,
                Icon = b.Icon,
                Color = b.Color,
                EarnedAt = b.EarnedAt
            }).ToList(),
            IsPremium = u.IsPremium,
            Role = u.Role,
            Hearts = u.Hearts,
            MaxHearts = u.MaxHearts,
            GemsCount = u.GemsCount,
            TeacherAppStatus = u.TeacherAppStatus,
            AvatarUrl = u.AvatarUrl,
            AvatarFrameType = u.AvatarFrameType,
            CoverUrl = u.CoverUrl
        };

        private static StatelessBadgeInfoDto MapBadge(Badge b) => new()
        {
            Id = b.Id.ToString(),
            Name = b.Name,
            Description = b.Description,
            Icon = b.Icon,
            Color = b.Color,
            EarnedAt = System.DateTime.UtcNow
        };

        private static bool VerifyPassword(string password, string passwordHash)
        {
            if (passwordHash.StartsWith("$2a$") || passwordHash.StartsWith("$2b$") || passwordHash.StartsWith("$2y$"))
            {
                try { return BCrypt.Net.BCrypt.Verify(password, passwordHash); }
                catch { return false; }
            }
            var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password + "visualizationdsa-salt"));
            var sha256Hash = Convert.ToHexString(bytes).ToLowerInvariant();
            return sha256Hash == passwordHash;
        }
    }
}