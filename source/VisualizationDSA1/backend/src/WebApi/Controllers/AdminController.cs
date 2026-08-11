using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Infrastructure.Data;
using VisualizationDSA.WebApi.Filters;
using VisualizationDSA.Application.Common.Interfaces;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/concepts/admin")]
    [RequireJwtRole("Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IRoadmapAuditService _auditService;
        private readonly IContentModerationService _moderationService;

        public AdminController(
            ApplicationDbContext dbContext,
            IRoadmapAuditService auditService,
            IContentModerationService moderationService)
        {
            _dbContext = dbContext;
            _auditService = auditService;
            _moderationService = moderationService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var totalUsers   = await _dbContext.Users.CountAsync();
            var totalStudents = await _dbContext.Users.CountAsync(u => u.Role == "Student");
            var totalTeachers = await _dbContext.Users.CountAsync(u => u.Role == "Teacher");
            var totalAdmins  = await _dbContext.Users.CountAsync(u => u.Role == "Admin");
            var premiumUsers = await _dbContext.Users.CountAsync(u => u.IsPremium);
            var totalQuizzes = await _dbContext.Quizzes.CountAsync();
            var totalOrders  = await _dbContext.Orders.CountAsync();
            var paidOrders   = await _dbContext.Orders.CountAsync(o => o.Status == "Completed" || o.Status == "paid");

            var topUsers = await _dbContext.Users
                .OrderByDescending(u => u.TotalXP)
                .Take(5)
                .Select(u => new { u.Email, u.Username, u.TotalXP, u.CurrentLevel, u.Role })
                .ToListAsync();

            var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-6);
            var registrationList = await _dbContext.Users
                .Where(u => u.CreatedAt >= sevenDaysAgo)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var registrationsLast7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.UtcNow.Date.AddDays(-6 + i))
                .Select(date => new
                {
                    date = date.ToString("yyyy-MM-dd"),
                    count = registrationList.FirstOrDefault(r => r.Date == date)?.Count ?? 0
                })
                .ToList();

            var popularCourses = await _dbContext.UserLessonProgresses
                .Join(_dbContext.ModuleItems, p => p.LessonId, m => m.LessonId, (p, m) => new { Progress = p, ModuleItem = m })
                .Join(_dbContext.CourseModules, pm => pm.ModuleItem.ModuleId, cm => cm.Id, (pm, cm) => new { pm.Progress, cm.Course })
                .Where(x => x.Course != null)
                .GroupBy(x => x.Course!.Id)
                .Select(g => new
                {
                    courseId = g.Key,
                    title = g.First().Course!.Title,
                    enrollmentsCount = g.Select(x => x.Progress.UserId).Distinct().Count()
                })
                .OrderByDescending(c => c.enrollmentsCount)
                .Take(3)
                .ToListAsync();

            return Ok(new
            {
                users = new { total = totalUsers, students = totalStudents, teachers = totalTeachers, admins = totalAdmins, premium = premiumUsers },
                quizzes = new { total = totalQuizzes },
                orders  = new { total = totalOrders, paid = paidOrders },
                topUsers,
                registrationsLast7Days,
                popularCourses
            });
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateRoleRequest request)
        {
            if (request.Role != "Student" && request.Role != "Teacher" && request.Role != "Admin")
                return BadRequest(new { error = "INVALID_ROLE", message = "Role phải là Student, Teacher hoặc Admin." });

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);
            if (user == null)
                return NotFound(new { error = "USER_NOT_FOUND" });

            var currentAdminIdStr = JwtHelper.ExtractSubFromToken(Request);
            var isSelf = Guid.TryParse(id, out var targetGuid) && Guid.TryParse(currentAdminIdStr, out var currentAdminId) && targetGuid == currentAdminId;

            // Không cho admin tự hạ quyền của chính mình
            if (isSelf && request.Role != "Admin")
                return StatusCode(403, new { error = "CANNOT_DEMOTE_SELF", message = "Không thể tự hạ quyền Admin của chính mình." });

            // Không cho hạ quyền admin cuối cùng
            if (user.Role == "Admin" && request.Role != "Admin")
            {
                var adminCount = await _dbContext.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                    return StatusCode(403, new { error = "LAST_ADMIN", message = "Không thể hạ quyền admin cuối cùng của hệ thống." });
            }

            var oldRole = user.Role;
            user.SetRole(request.Role);
            await _dbContext.SaveChangesAsync();

            if (Guid.TryParse(id, out targetGuid))
            {
                await LogAdminAction("UpdateUserRole", targetGuid, $"Đổi vai trò của {user.Username} từ {oldRole} sang {request.Role}.");
            }

            return Ok(new { message = $"Đã đổi role của {user.Email} thành {request.Role}.", userId = id, newRole = request.Role });
        }

        [HttpPut("users/{id}/premium")]
        public async Task<IActionResult> TogglePremium(string id, [FromBody] TogglePremiumRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);
            if (user == null)
                return NotFound(new { error = "USER_NOT_FOUND" });

            var oldStatus = user.IsPremium;

            if (request.IsPremium)
                user.SetPremium(DateTime.UtcNow.AddDays(30));
            else
                user.DowngradeFromPremium();

            await _dbContext.SaveChangesAsync();

            if (Guid.TryParse(id, out var targetGuid))
            {
                await LogAdminAction("TogglePremium", targetGuid, $"Thay đổi trạng thái Premium của {user.Username} từ {oldStatus} sang {request.IsPremium}.");
            }

            return Ok(new { message = $"Đã {(request.IsPremium ? "bật" : "tắt")} Premium cho {user.Email}.", userId = id, isPremium = request.IsPremium });
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "INVALID_INPUT", message = "Email, username và mật khẩu không được để trống." });
            }

            var existingUser = await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() || u.Username.ToLower() == request.Username.ToLower());
            if (existingUser)
            {
                return BadRequest(new { error = "USER_EXISTS", message = "Email hoặc Username đã được sử dụng." });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
            var newUser = new User(request.Email, request.Username, passwordHash);

            newUser.SetRole(request.Role);
            if (request.IsPremium)
                newUser.SetPremium(DateTime.UtcNow.AddDays(30));

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            await LogAdminAction("CreateUser", newUser.Id, $"Tạo người dùng mới: {newUser.Username} ({newUser.Email}), vai trò: {newUser.Role}, Premium: {newUser.IsPremium}.");

            return Ok(new
            {
                message = "Tạo người dùng mới thành công.",
                user = new
                {
                    id = newUser.Id.ToString(),
                    newUser.Email,
                    newUser.Username,
                    newUser.Role,
                    newUser.IsPremium,
                    newUser.CurrentLevel,
                    newUser.TotalXP,
                    newUser.StreakDays,
                    newUser.CreatedAt
                }
            });
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);
            if (user == null)
            {
                return NotFound(new { error = "USER_NOT_FOUND", message = "Không tìm thấy người dùng." });
            }

            var currentAdminIdStr = JwtHelper.ExtractSubFromToken(Request);
            var isSelf = Guid.TryParse(id, out var targetGuid) && Guid.TryParse(currentAdminIdStr, out var currentAdminId) && targetGuid == currentAdminId;

            if (isSelf)
                return StatusCode(403, new { error = "CANNOT_DEMOTE_SELF", message = "Không thể xóa tài khoản của chính mình." });

            if (user.Role == "Admin")
            {
                var adminCount = await _dbContext.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                    return StatusCode(403, new { error = "LAST_ADMIN", message = "Không thể xóa admin cuối cùng của hệ thống." });
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            if (Guid.TryParse(id, out targetGuid))
            {
                await LogAdminAction("DeleteUser", targetGuid, $"Xóa người dùng {user.Username} ({user.Email}) khỏi hệ thống.");
            }

            return Ok(new { message = $"Đã xóa người dùng {user.Username} ({user.Email}) thành công." });
        }

        [HttpPut("users/{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            {
                return BadRequest(new { error = "INVALID_PASSWORD", message = "Mật khẩu mới phải có tối thiểu 8 ký tự." });
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);
            if (user == null)
            {
                return NotFound(new { error = "USER_NOT_FOUND", message = "Không tìm thấy người dùng." });
            }

            var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
            user.ChangePassword(newHash);
            await _dbContext.SaveChangesAsync();

            if (Guid.TryParse(id, out var targetGuid))
            {
                await LogAdminAction("ResetPassword", targetGuid, $"Đặt lại mật khẩu của người dùng {user.Username}.");
            }

            return Ok(new { message = $"Đã đặt lại mật khẩu cho người dùng {user.Username} thành công." });
        }

        [HttpGet("quizzes")]
        [RequireJwtRole("Teacher,Admin")]
        public async Task<IActionResult> GetQuizzes()
        {
            var quizzes = await _dbContext.Quizzes
                .OrderBy(q => q.Title)
                .Select(q => new
                {
                    id            = q.Id.ToString(),
                    title         = q.Title,
                    topic         = q.Topic,
                    difficulty    = q.Difficulty == 1 ? "easy" : q.Difficulty == 5 ? "hard" : "medium",
                    xpReward      = q.XPReward,
                    questionCount = q.Questions.Count,
                    createdAt     = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                })
                .ToListAsync();

            return Ok(quizzes);
        }

        [HttpDelete("quizzes/{id}")]
        [RequireJwtRole("Teacher,Admin")]
        public async Task<IActionResult> DeleteQuiz(string id)
        {
            var quiz = await _dbContext.Quizzes.FirstOrDefaultAsync(q => q.Id.ToString() == id);
            if (quiz == null)
                return NotFound(new { error = "QUIZ_NOT_FOUND" });

            _dbContext.Quizzes.Remove(quiz);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = $"Đã xóa quiz \"{quiz.Title}\"." });
        }

        [HttpGet("analytics/quiz")]
        [RequireJwtRole("Teacher,Admin")]
        public async Task<IActionResult> GetQuizAnalytics()
        {
            var quizAttempts = await _dbContext.Quizzes
                .OrderBy(q => q.Title)
                .Select(q => new
                {
                    id            = q.Id.ToString(),
                    title         = q.Title,
                    topic         = q.Topic,
                    difficulty    = q.Difficulty == 1 ? "easy" : q.Difficulty == 5 ? "hard" : "medium",
                    questionCount = q.Questions.Count,
                    xpReward      = q.XPReward
                })
                .ToListAsync();

            var totalUsers   = await _dbContext.Users.CountAsync();
            var premiumCount = await _dbContext.Users.CountAsync(u => u.IsPremium);

            return Ok(new
            {
                totalQuizzes  = quizAttempts.Count,
                totalUsers,
                premiumCount,
                quizzes       = quizAttempts
            });
        }

        [HttpPut("users/{id}/ban")]
        public async Task<IActionResult> BanUser(string id, [FromBody] BanUserRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);
            if (user == null)
                return NotFound(new { error = "USER_NOT_FOUND" });

            var currentAdminIdStr = JwtHelper.ExtractSubFromToken(Request);
            var isSelf = Guid.TryParse(id, out var targetGuid) && Guid.TryParse(currentAdminIdStr, out var currentAdminId) && targetGuid == currentAdminId;

            if (isSelf && !request.IsActive)
                return StatusCode(403, new { error = "CANNOT_DEMOTE_SELF", message = "Không thể khóa tài khoản của chính mình." });

            if (user.Role == "Admin" && !request.IsActive)
            {
                var adminCount = await _dbContext.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                    return StatusCode(403, new { error = "LAST_ADMIN", message = "Không thể khóa admin cuối cùng của hệ thống." });
            }

            user.SetActiveStatus(request.IsActive);
            await _dbContext.SaveChangesAsync();

            var action = request.IsActive ? "mở khóa" : "khóa";
            return Ok(new { message = $"Đã {action} tài khoản {user.Email}.", userId = id, isActive = request.IsActive });
        }

        [HttpPost("users/{id}/impersonate")]
        public async Task<IActionResult> ImpersonateUser(string id)
        {
            var adminId = JwtHelper.ExtractSubFromToken(Request) ?? "unknown-admin";

            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);
            if (dbUser == null)
                return NotFound(new { error = "USER_NOT_FOUND", message = "Không tìm thấy người dùng để đóng vai." });

            var impersonatedToken = GenerateImpersonatedJwt(
                dbUser.Id.ToString(), dbUser.Email, dbUser.Username,
                dbUser.Role, dbUser.CurrentLevel, adminId);

            var impersonatedRefreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

            if (Guid.TryParse(id, out var targetGuid))
            {
                await LogAdminAction("ImpersonateUser", targetGuid, $"Đóng vai (Impersonate) tài khoản học viên {dbUser.Username} ({dbUser.Email}).");
            }

            return Ok(new
            {
                accessToken = impersonatedToken,
                refreshToken = impersonatedRefreshToken,
                expiresIn = 900,
                user = new
                {
                    id,
                    email = dbUser.Email,
                    username = dbUser.Username,
                    role = dbUser.Role,
                    level = dbUser.CurrentLevel,
                    isPremium = dbUser.IsPremium
                }
            });
        }

        private static string GenerateImpersonatedJwt(string userId, string email, string username, string role, int level, string adminId)
        {
            var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{{\"sub\":\"{userId}\",\"email\":\"{email}\",\"name\":\"{username}\"," +
                $"\"role\":\"{role}\"," +
                $"\"level\":{level},\"exp\":{DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()}," +
                $"\"jti\":\"{Guid.NewGuid()}\",\"isImpersonated\":true,\"originalAdminId\":\"{adminId}\"}}"
            ));
            var signature = Convert.ToBase64String(
                HMACSHA256.HashData(JwtHelper.GetSigningKey(), Encoding.UTF8.GetBytes($"{header}.{payload}"))
            );
            return $"{header}.{payload}.{signature}";
        }

        [HttpGet("roadmaps/{id}/edit-history")]
        public async Task<IActionResult> GetRoadmapEditHistory(Guid id)
        {
            var history = await _auditService.GetEditHistoryAsync(id);
            return Ok(history.Select(h => new {
                h.Id,
                h.EditorId,
                EditorName = h.Editor?.Username,
                h.ChangeType,
                h.Note,
                h.ChangedAt
            }));
        }

        [HttpPost("roadmaps/{id}/unpublish")]
        public async Task<IActionResult> UnpublishRoadmap(Guid id, [FromBody] JsonElement payload)
        {
            var roadmap = await _dbContext.CustomRoadmaps.FindAsync(id);
            if (roadmap == null) return NotFound("Roadmap not found");

            if (roadmap.Status != "Published")
                return BadRequest("Roadmap is not currently published");

            roadmap.Publish("Private");

            string reason = payload.TryGetProperty("reason", out var p) ? p.GetString() ?? "No reason provided" : "No reason provided";

            var adminIdStr = JwtHelper.ExtractSubFromToken(Request);
            var adminId = Guid.TryParse(adminIdStr, out var aid) ? aid : Guid.Empty;
            await _auditService.LogEditAsync(roadmap.Id, adminId, "Unpublish", $"Admin unpublished. Reason: {reason}");
            await LogAdminAction("UNPUBLISH_ROADMAP", id, reason);

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Đã gỡ roadmap thành Private" });
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var query = _dbContext.AuditLogs.AsQueryable();
            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, logs });
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetPendingReports()
        {
            var reports = await _moderationService.GetPendingReportsAsync();
            return Ok(reports.Select(r => new {
                r.Id,
                r.NodeId,
                NodeName = r.Node?.Name,
                IsNodeHidden = r.Node?.IsHidden ?? false,
                r.Reason,
                r.Detail,
                r.Status,
                ReporterName = r.Reporter?.Username,
                r.CreatedAt
            }));
        }

        [HttpPatch("reports/{id}/resolve")]
        public async Task<IActionResult> ResolveReport(Guid id, [FromBody] JsonElement payload)
        {
            var action = payload.TryGetProperty("action", out var p) ? p.GetString() : null;
            if (string.IsNullOrEmpty(action)) return BadRequest("Action is required (dismiss, remove, warn_teacher)");

            await _moderationService.ResolveReportAsync(id, action);
            await LogAdminAction("RESOLVE_REPORT", id, $"Action: {action}");

            return Ok(new { message = "Báo cáo đã được xử lý" });
        }

        // E — ẩn/hiện node vi phạm (moderation)
        [HttpPatch("nodes/{id:guid}/hide")]
        public async Task<IActionResult> HideNode(Guid id, [FromBody] JsonElement payload)
        {
            var node = await _dbContext.CustomNodes.FirstOrDefaultAsync(n => n.Id == id);
            if (node == null)
                return NotFound(new { error = "NODE_NOT_FOUND", message = "Không tìm thấy bài học." });

            var hidden = payload.TryGetProperty("hidden", out var h) ? h.GetBoolean() : true;
            if (hidden) node.Hide();
            else node.Unhide();

            await _dbContext.SaveChangesAsync();
            await LogAdminAction(hidden ? "HIDE_NODE" : "UNHIDE_NODE", id, $"Node: {node.Name}");

            return Ok(new { message = hidden ? "Đã ẩn bài học." : "Đã hiện lại bài học.", isHidden = node.IsHidden });
        }

        [HttpGet("blacklist")]
        public async Task<IActionResult> GetBlacklist([FromQuery] string? category)
        {
            var query = _dbContext.KeywordBlacklists.AsQueryable();
            if (!string.IsNullOrEmpty(category))
                query = query.Where(b => b.Category == category);

            var list = await query.ToListAsync();
            return Ok(list);
        }

        [HttpPost("blacklist")]
        public async Task<IActionResult> AddBlacklistKeyword([FromBody] JsonElement payload)
        {
            var keyword = payload.TryGetProperty("keyword", out var p) ? p.GetString() : null;
            var category = payload.TryGetProperty("category", out var c) ? c.GetString() : "general";

            if (string.IsNullOrWhiteSpace(keyword)) return BadRequest("Keyword is required");

            var adminIdStr = JwtHelper.ExtractSubFromToken(Request);
            var adminId = Guid.TryParse(adminIdStr, out var aid) ? aid : Guid.Empty;

            var entry = new KeywordBlacklist(keyword, category!, adminId);
            _dbContext.KeywordBlacklists.Add(entry);
            await _dbContext.SaveChangesAsync();

            await LogAdminAction("ADD_BLACKLIST", entry.Id, $"Keyword: {keyword}");
            return Ok(entry);
        }

        [HttpDelete("blacklist/{id}")]
        public async Task<IActionResult> RemoveBlacklistKeyword(Guid id)
        {
            var entry = await _dbContext.KeywordBlacklists.FindAsync(id);
            if (entry == null) return NotFound();

            _dbContext.KeywordBlacklists.Remove(entry);
            await _dbContext.SaveChangesAsync();

            await LogAdminAction("REMOVE_BLACKLIST", id, $"Keyword: {entry.Keyword}");
            return Ok(new { message = "Đã xóa từ khóa khỏi blacklist" });
        }

        private async Task LogAdminAction(string action, Guid? targetId, string details)
        {
            var adminIdStr = JwtHelper.ExtractSubFromToken(Request);
            var adminName = "SystemAdmin";
            Guid adminId = Guid.Empty;
            if (adminIdStr != null && Guid.TryParse(adminIdStr, out var parsedId))
            {
                adminId = parsedId;
                var adminUser = await _dbContext.Users.FindAsync(adminId);
                if (adminUser != null)
                {
                    adminName = adminUser.Username;
                }
            }

            var log = new AuditLog(action, adminId, adminName, targetId, details);
            _dbContext.AuditLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }
    }

    public record UpdateRoleRequest(string Role);
    public record TogglePremiumRequest(bool IsPremium);
    public record BanUserRequest(bool IsActive);
    public record CreateUserRequest(string Email, string Username, string Password, string Role, bool IsPremium);
    public record ResetPasswordRequest(string NewPassword);
}