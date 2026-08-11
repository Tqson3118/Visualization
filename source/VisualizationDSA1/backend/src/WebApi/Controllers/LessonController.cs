using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Infrastructure.Data;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/concepts/lessons")]
    public class LessonController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly VisualizationDSA.Application.Services.IProgressRuleEngine _progressRuleEngine;
        private readonly VisualizationDSA.Application.Common.Interfaces.IRoadmapReviewService _roadmapReviewService;

        public LessonController(ApplicationDbContext dbContext, VisualizationDSA.Application.Services.IProgressRuleEngine progressRuleEngine, VisualizationDSA.Application.Common.Interfaces.IRoadmapReviewService roadmapReviewService)
        {
            _dbContext = dbContext;
            _progressRuleEngine = progressRuleEngine;
            _roadmapReviewService = roadmapReviewService;
        }

        private bool IsOwnerOrAdmin(Course course, Guid currentUserId)
        {
            if (JwtHelper.IsAdmin(Request)) return true;
            return course?.TeacherId == currentUserId;
        }

        [HttpGet("{id}")]
        [RequireJwtRole]
        public async Task<IActionResult> GetLessonById(Guid id)
        {
            var moduleItem = await _dbContext.ModuleItems.Include(m => m.Module).ThenInclude(m => m.Course).Include(m => m.Lesson).FirstOrDefaultAsync(i => i.LessonId == id);
            if (moduleItem == null || moduleItem.Lesson == null) return NotFound(new { error = "LESSON_NOT_FOUND", message = "Không tìm thấy bài học." });
            var lesson = moduleItem.Lesson;
            var course = moduleItem.Module?.Course;
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (course != null && course.IsPremium && userIdStr != null && Guid.TryParse(userIdStr, out var parsedId))
            {
                var user = await _dbContext.Users.FindAsync(parsedId);
                if (user != null && !user.IsPremium && user.Role == "Student")
                {
                    return StatusCode(403, new { error = "PREMIUM_REQUIRED", message = "Khóa học này yêu cầu tài khoản Premium để truy cập." });
                }
            }

            var status = "NotStarted";
            var lastActiveFrameIndex = 0;
            var lastScrollPercent = 0.0;

            if (userIdStr != null && Guid.TryParse(userIdStr, out var currentUserId))
            {
                var progress = await _dbContext.UserLessonProgresses
                    .FirstOrDefaultAsync(p => p.UserId == currentUserId && p.LessonId == id);
                if (progress != null)
                {
                    status = progress.Status;
                    lastActiveFrameIndex = progress.LastActiveFrameIndex;
                    lastScrollPercent = progress.LastScrollPercent;
                }
            }

            var nextItems = await _dbContext.ModuleItems
                .Include(i => i.Codelab)
                .ThenInclude(c => c.TestCases)
                .Where(i => i.ModuleId == moduleItem.ModuleId && i.OrderIndex > moduleItem.OrderIndex && i.OrderIndex < moduleItem.OrderIndex + 1000)
                .ToListAsync();

            var quizItem = nextItems.FirstOrDefault(i => i.ItemType == VisualizationDSA.Domain.Enums.ModuleItemType.Quiz);
            var codelabItem = nextItems.FirstOrDefault(i => i.ItemType == VisualizationDSA.Domain.Enums.ModuleItemType.Codelab);

            string? leetCodeId = null;
            try {
                if (!string.IsNullOrEmpty(lesson.SandboxConfig)) {
                    var configObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(lesson.SandboxConfig);
                    if (configObj != null && configObj.ContainsKey("leetCodeId")) {
                        leetCodeId = configObj["leetCodeId"]?.ToString();
                    }
                }
            } catch { }
            


            return Ok(new
            {
                lesson.Id,
                courseId = (course?.Id ?? Guid.Empty),
                courseTitle = course?.Title,
                lesson.Title,
                lesson.ContentMd,
                lesson.SandboxType,
                lesson.SandboxConfig,
                QuizId = quizItem?.QuizId,
                Codelab = codelabItem?.Codelab == null ? null : new {
                    codelabItem.Codelab.Id,
                    codelabItem.Codelab.Title,
                    codelabItem.Codelab.Description,
                    codelabItem.Codelab.InitialCode,
                    codelabItem.Codelab.AllowedLanguages,
                    TestCases = codelabItem.Codelab.TestCases.OrderBy(tc => tc.OrderIndex).Select(tc => new {
                        tc.Id,
                        tc.Input,
                        tc.ExpectedOutput,
                        tc.IsHidden,
                        tc.OrderIndex
                    })
                },
                leetCodeId = leetCodeId,
                lesson.XPReward,
                moduleItem.OrderIndex,
                status,
                lastActiveFrameIndex,
                lastScrollPercent
            });
        }

        [HttpPost("{lessonId}/complete")]
        [RequireJwtRole]
        public async Task<IActionResult> CompleteLesson(Guid lessonId)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null) return NotFound(new { error = "USER_NOT_FOUND" });

            var lesson = await _dbContext.Lessons.FindAsync(lessonId);

            // G3.9.2 mở rộng — lessonId có thể là CustomNode.Id (luồng Thư viện Lộ trình học node trực tiếp)
            if (lesson == null)
            {
                var node = await _dbContext.CustomNodes.FindAsync(lessonId);
                if (node == null)
                    return NotFound(new { error = "LESSON_NOT_FOUND", message = "Không tìm thấy bài học." });

                return await CompleteRoadmapNodeAsync(userId, user, node);
            }

            // G3.1.1 — Enforce step lock server-side (chống gian lận UC-12):
            // nếu lesson có module item Quiz/Codelab liên kết, user phải đã pass trước khi complete.
            var linkedItems = await _dbContext.ModuleItems
                .Where(m => m.LessonId == lessonId && !m.IsDeleted)
                .ToListAsync();

            foreach (var item in linkedItems)
            {
                if (item.QuizId.HasValue || item.CodelabId.HasValue)
                {
                    var itemProgress = await _dbContext.UserModuleItemProgresses
                        .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleItemId == item.Id);
                    if (itemProgress == null || itemProgress.Status != "Completed")
                    {
                        return StatusCode(403, new
                        {
                            error = "STEP_LOCKED",
                            message = "Bạn phải hoàn thành bài trắc nghiệm/thực hành liên kết trước khi đánh dấu hoàn thành bài học."
                        });
                    }
                }
            }

            var progress = await _dbContext.UserLessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            bool firstTime = false;
            if (progress == null)
            {
                progress = new UserLessonProgress(userId, lessonId, "Completed");
                progress.MarkAsCompleted(lesson.XPReward);
                _dbContext.UserLessonProgresses.Add(progress);
                firstTime = true;
            }
            else if (progress.Status != "Completed")
            {
                progress.MarkAsCompleted(lesson.XPReward);
                firstTime = true;
            }

            if (firstTime)
            {
                user.AwardXP(lesson.XPReward);
                user.RecordActivity();
            }

            
            var moduleItems = await _dbContext.ModuleItems
                .Where(m => m.LessonId == lessonId && !m.IsDeleted)
                .ToListAsync();

            foreach (var moduleItem in moduleItems)
            {
                var itemProgress = await _dbContext.UserModuleItemProgresses
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleItemId == moduleItem.Id);
                    
                if (itemProgress == null)
                {
                    itemProgress = new UserModuleItemProgress(userId, moduleItem.Id);
                    _dbContext.UserModuleItemProgresses.Add(itemProgress);
                }
                
                itemProgress.UpdateProgress(activeFrame: 0, scrollPercent: 100, isCompleted: true, score: null);
                await _progressRuleEngine.ProcessCompletionAsync(userId, moduleItem.Id);
            }

            await _dbContext.SaveChangesAsync();

            // G3.9.2 — Nếu bài vừa hoàn thành là bài CUỐI của roadmap → set enrollment Completed
            await _roadmapReviewService.MarkRoadmapCompletedIfLastLessonAsync(userId, lessonId);

            return Ok(new
            {
                message = "Đã hoàn thành bài học thành công!",
                xpAwarded = firstTime ? lesson.XPReward : 0,
                totalXp = user.TotalXP,
                currentLevel = user.CurrentLevel
            });
        }

        // G3.9.2 — Hoàn thành roadmap node (CustomNode) khi lessonId là node.Id:
        //  - node có LabId → resolve ModuleItem(CodelabId==LabId)→Lesson → ghi UserLessonProgresses + XP cho lesson đó
        //    (giữ nguyên luồng cũ → teacher analytics có dữ liệu).
        //  - node không LabId → chỉ cộng XP theo difficulty (30/50/80) + RecordActivity.
        //  - mọi trường hợp → MarkRoadmapCompletedIfLastLessonAsync(userId, node.Id) (service lo last-node check).
        private async Task<IActionResult> CompleteRoadmapNodeAsync(Guid userId, User user, CustomNode node)
        {
            Lesson? lesson = null;
            if (node.LabId.HasValue)
            {
                var codelabItem = await _dbContext.ModuleItems
                    .FirstOrDefaultAsync(m => m.CodelabId == node.LabId.Value && !m.IsDeleted);
                if (codelabItem?.LessonId != null)
                    lesson = await _dbContext.Lessons.FindAsync(codelabItem.LessonId.Value);
            }

            int xpReward = node.Difficulty switch
            {
                "Easy" => 30,
                "Hard" => 80,
                _ => 50
            };
            bool firstTime = true;

            if (lesson != null)
            {
                var linkedItems = await _dbContext.ModuleItems
                    .Where(m => m.LessonId == lesson.Id && !m.IsDeleted)
                    .ToListAsync();

                foreach (var item in linkedItems)
                {
                    if (item.QuizId.HasValue || item.CodelabId.HasValue)
                    {
                        var itemProgress = await _dbContext.UserModuleItemProgresses
                            .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleItemId == item.Id);
                        if (itemProgress == null || itemProgress.Status != "Completed")
                        {
                            return StatusCode(403, new
                            {
                                error = "STEP_LOCKED",
                                message = "Bạn phải hoàn thành bài trắc nghiệm/thực hành liên kết trước khi đánh dấu hoàn thành bài học."
                            });
                        }
                    }
                }

                var progress = await _dbContext.UserLessonProgresses
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lesson.Id);

                if (progress == null)
                {
                    progress = new UserLessonProgress(userId, lesson.Id, "Completed");
                    progress.MarkAsCompleted(lesson.XPReward);
                    _dbContext.UserLessonProgresses.Add(progress);
                }
                else if (progress.Status != "Completed")
                {
                    progress.MarkAsCompleted(lesson.XPReward);
                }
                else
                {
                    firstTime = false;
                }

                if (firstTime)
                {
                    user.AwardXP(lesson.XPReward);
                    user.RecordActivity();
                }
                xpReward = lesson.XPReward;

                var moduleItems = await _dbContext.ModuleItems
                    .Where(m => m.LessonId == lesson.Id && !m.IsDeleted)
                    .ToListAsync();

                foreach (var moduleItem in moduleItems)
                {
                    var itemProgress = await _dbContext.UserModuleItemProgresses
                        .FirstOrDefaultAsync(p => p.UserId == userId && p.ModuleItemId == moduleItem.Id);

                    if (itemProgress == null)
                    {
                        itemProgress = new UserModuleItemProgress(userId, moduleItem.Id);
                        _dbContext.UserModuleItemProgresses.Add(itemProgress);
                    }

                    itemProgress.UpdateProgress(activeFrame: 0, scrollPercent: 100, isCompleted: true, score: null);
                    await _progressRuleEngine.ProcessCompletionAsync(userId, moduleItem.Id);
                }
            }
            else
            {
                user.AwardXP(xpReward);
                user.RecordActivity();
            }

            await _dbContext.SaveChangesAsync();

            // Roadmap completion — service đã lo last-node check + yêu cầu mọi node Lab-linked Completed
            await _roadmapReviewService.MarkRoadmapCompletedIfLastLessonAsync(userId, node.Id);

            return Ok(new
            {
                message = "Đã hoàn thành bài học thành công!",
                xpAwarded = firstTime ? xpReward : 0,
                totalXp = user.TotalXP,
                currentLevel = user.CurrentLevel
            });
        }

        [HttpPut("{lessonId}")]
        [RequireJwtRole("Teacher,Admin")]
        public async Task<IActionResult> UpdateLesson(Guid lessonId, [FromBody] CreateLessonDto dto)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var currentUserId)) return Unauthorized();

            var moduleItem = await _dbContext.ModuleItems.Include(m => m.Module).ThenInclude(m => m.Course).Include(m => m.Lesson).FirstOrDefaultAsync(i => i.LessonId == lessonId);
            if (moduleItem == null || moduleItem.Lesson == null) return NotFound(new { error = "LESSON_NOT_FOUND", message = "Không tìm thấy bài học." });
            var lesson = moduleItem.Lesson;
            var course = moduleItem.Module?.Course;

            if (course != null && !IsOwnerOrAdmin(course, currentUserId))
            {
                return StatusCode(403, new { error = "FORBIDDEN", message = "Bạn không có quyền chỉnh sửa bài học này." });
            }

            lesson.Update(dto.Title, dto.ContentMd, dto.SandboxType, dto.SandboxConfig, dto.XPReward);
            moduleItem.UpdateQuizId(dto.QuizId);
            moduleItem.Update(moduleItem.OverrideTitle, dto.OrderIndex, moduleItem.IsRequired);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Cập nhật bài học thành công!", lesson });
        }

        [HttpDelete("{lessonId}")]
        [RequireJwtRole("Teacher,Admin")]
        public async Task<IActionResult> DeleteLesson(Guid lessonId)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var currentUserId)) return Unauthorized();

            var moduleItem = await _dbContext.ModuleItems.Include(m => m.Module).ThenInclude(m => m.Course).Include(m => m.Lesson).FirstOrDefaultAsync(i => i.LessonId == lessonId);
            if (moduleItem == null || moduleItem.Lesson == null) return NotFound(new { error = "LESSON_NOT_FOUND", message = "Không tìm thấy bài học." });
            var lesson = moduleItem.Lesson;
            var course = moduleItem.Module?.Course;

            if (course != null && !IsOwnerOrAdmin(course, currentUserId))
            {
                return StatusCode(403, new { error = "FORBIDDEN", message = "Bạn không có quyền xóa bài học này." });
            }

            _dbContext.Lessons.Remove(lesson);
            _dbContext.ModuleItems.Remove(moduleItem);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Xóa bài học thành công!" });
        }

        [HttpPost("{lessonId}/progress")]
        [RequireJwtRole]
        public async Task<IActionResult> UpdateLessonProgress(Guid lessonId, [FromBody] UpdateProgressDto dto)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var progress = await _dbContext.UserLessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (progress == null)
            {
                progress = new UserLessonProgress(userId, lessonId, "InProgress");
                _dbContext.UserLessonProgresses.Add(progress);
            }

            progress.UpdateProgress(dto.LastActiveFrameIndex, dto.LastScrollPercent);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Cập nhật tiến trình thành công!", progress.Status, progress.LastActiveFrameIndex, progress.LastScrollPercent });
        }

        [HttpGet("{lessonId}/comments")]
        [RequireJwtRole]
        public async Task<IActionResult> GetLessonComments(Guid lessonId, [FromQuery] string? search = null)
        {
            var query = _dbContext.LessonComments.Where(c => c.LessonId == lessonId && !c.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var cleanSearch = search.Trim().ToLower();
                query = query.Where(c => c.Content.ToLower().Contains(cleanSearch));
            }

            var comments = await query
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.LessonId,
                    c.UserId,
                    username = c.User.Username,
                    role = c.User.Role,
                    isPremium = c.User.IsPremium,
                    c.Content,
                    c.CreatedAt,
                    c.ParentId,
                    c.IsEdited,
                    c.EditedAt
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost("{lessonId}/comments")]
        [RequireJwtRole]
        public async Task<IActionResult> CreateLessonComment(Guid lessonId, [FromBody] CreateCommentDto dto)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Length > 2000)
                return BadRequest(new { error = "INVALID_CONTENT", message = "Nội dung bình luận không hợp lệ." });

            if (dto.ParentId.HasValue)
            {
                var parentExists = await _dbContext.LessonComments.AnyAsync(c => c.Id == dto.ParentId.Value);
                if (!parentExists)
                    return NotFound(new { error = "PARENT_COMMENT_NOT_FOUND", message = "Không tìm thấy bình luận gốc." });
            }

            var comment = new LessonComment(lessonId, userId, dto.Content, dto.ParentId);
            _dbContext.LessonComments.Add(comment);
            await _dbContext.SaveChangesAsync();

            if (dto.ParentId.HasValue)
            {
                var parentComment = await _dbContext.LessonComments.FindAsync(dto.ParentId.Value);
                if (parentComment != null && parentComment.UserId != userId)
                {
                    var lesson = await _dbContext.Lessons.FindAsync(lessonId);
                    var sender = await _dbContext.Users.FindAsync(userId);
                    var notificationContent = $"{sender?.Username} đã trả lời bình luận của bạn trong bài học '{lesson?.Title}'.";
                    var notification = new Notification(parentComment.UserId, notificationContent, $"/lessons/{lessonId}?tab=discussion");
                    _dbContext.Notifications.Add(notification);
                    await _dbContext.SaveChangesAsync();
                }
            }

            var user = await _dbContext.Users.FindAsync(userId);
            return Ok(new
            {
                message = "Đăng bình luận thành công!",
                comment = new
                {
                    comment.Id,
                    comment.LessonId,
                    comment.UserId,
                    username = user?.Username ?? "Unknown",
                    role = user?.Role ?? "Student",
                    isPremium = user?.IsPremium ?? false,
                    comment.Content,
                    comment.CreatedAt,
                    comment.ParentId,
                    comment.IsEdited
                }
            });
        }
    }
}
