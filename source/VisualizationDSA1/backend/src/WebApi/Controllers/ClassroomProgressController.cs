using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.Application.Services;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/classrooms")]
    [Authorize]
    public class ClassroomProgressController : ControllerBase
    {
        private readonly IClassroomProgressService _progressService;
        private readonly IClassroomUnlockRuleEngine _unlockRuleEngine;
        private readonly IApplicationDbContext _context;

        public ClassroomProgressController(
            IClassroomProgressService progressService,
            IClassroomUnlockRuleEngine unlockRuleEngine,
            IApplicationDbContext context)
        {
            _progressService = progressService;
            _unlockRuleEngine = unlockRuleEngine;
            _context = context;
        }

        [HttpGet("{classroomId}/my-progress")]
        public async Task<IActionResult> GetMyProgress(Guid classroomId)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (!Guid.TryParse(userIdStr, out var studentId))
                return Unauthorized();

            var summary = await _progressService.GetProgressSummaryAsync(classroomId, studentId);
            return Ok(summary);
        }

        [HttpGet("{classroomId}/unlocked-items")]
        public async Task<IActionResult> GetUnlockedItems(Guid classroomId)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (!Guid.TryParse(userIdStr, out var studentId))
                return Unauthorized();

            var itemIds = await _progressService.GetUnlockedItemIdsAsync(classroomId, studentId);
            return Ok(new { unlockedItemIds = itemIds });
        }

        [HttpPost("module-items/{moduleItemId}/start")]
        public async Task<IActionResult> StartModuleItem(Guid moduleItemId)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (!Guid.TryParse(userIdStr, out var studentId))
                return Unauthorized();

            var classroomId = await GetClassroomIdForItem(moduleItemId);
            if (classroomId == null)
                return NotFound(new { error = "MODULE_ITEM_NOT_FOUND" });

            var result = await _progressService.StartItemAsync(classroomId.Value, moduleItemId, studentId);
            return Ok(result);
        }

        [HttpPut("module-items/{moduleItemId}/progress")]
        public async Task<IActionResult> UpdateProgress(Guid moduleItemId, [FromBody] UpdateProgressRequest request)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (!Guid.TryParse(userIdStr, out var studentId))
                return Unauthorized();

            var classroomId = await GetClassroomIdForItem(moduleItemId);
            if (classroomId == null)
                return NotFound(new { error = "MODULE_ITEM_NOT_FOUND" });

            var result = await _progressService.UpdateProgressAsync(classroomId.Value, moduleItemId, studentId, request.ActiveFrame, request.ScrollPercent);
            return Ok(result);
        }

        [HttpPost("module-items/{moduleItemId}/complete")]
        public async Task<IActionResult> CompleteModuleItem(Guid moduleItemId, [FromBody] CompleteItemRequest request)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (!Guid.TryParse(userIdStr, out var studentId))
                return Unauthorized();

            var classroomId = await GetClassroomIdForItem(moduleItemId);
            if (classroomId == null)
                return NotFound(new { error = "MODULE_ITEM_NOT_FOUND" });

            var result = await _progressService.CompleteItemAsync(classroomId.Value, moduleItemId, studentId, request.Score);
            return Ok(result);
        }

        [HttpGet("module-items/{moduleItemId}/unlock-status")]
        public async Task<IActionResult> GetUnlockStatus(Guid moduleItemId)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (!Guid.TryParse(userIdStr, out var studentId))
                return Unauthorized();

            var moduleItem = await GetModuleItemWithClassroom(moduleItemId);
            if (moduleItem == null)
                return NotFound(new { error = "MODULE_ITEM_NOT_FOUND" });

            var classroomId = moduleItem.Module?.ClassroomId;
            if (classroomId == null)
                return NotFound(new { error = "MODULE_ITEM_NOT_FOUND" });

            var isUnlocked = await _unlockRuleEngine.IsItemUnlockedAsync(classroomId.Value, moduleItemId, studentId);
            var reason = await _unlockRuleEngine.GetUnlockReasonAsync(classroomId.Value, moduleItemId, studentId);

            return Ok(new { isUnlocked, reason });
        }

        [HttpPost("import-course")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ImportCourse([FromBody] ImportCourseRequest request)
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var teacherId))
                return Unauthorized();

            var course = await _context.Courses.FindAsync(request.CourseId);
            if (course == null)
                return NotFound(new { error = "COURSE_NOT_FOUND", message = "Không tìm thấy khóa học." });

            if (!course.IsPublished)
                return BadRequest(new { error = "COURSE_NOT_PUBLISHED", message = "Chỉ có thể import khóa học đã xuất bản." });

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.OwnerTeacherId == teacherId && c.Id == request.ClassroomId);

            if (classroom == null)
                return NotFound(new { error = "CLASSROOM_NOT_FOUND", message = "Không tìm thấy lớp học." });

            var existingModules = await _context.ClassroomModules
                .Where(m => m.ClassroomId == classroom.Id)
                .Select(m => m.Id)
                .ToListAsync();

            if (existingModules.Any() && !request.OverrideExisting)
                return BadRequest(new { error = "CLASSROOM_NOT_EMPTY", message = "Lớp học đã có module. Hãy bật override để thay thế." });

            if (existingModules.Any())
            {
                _context.ClassroomModules.RemoveRange(await _context.ClassroomModules.Where(m => m.ClassroomId == classroom.Id).ToListAsync());
                await _context.SaveChangesAsync(CancellationToken.None);
            }

            var selectedModuleIds = request.SelectedModuleIds ?? await _context.CourseModules
                .Where(cm => cm.CourseId == request.CourseId)
                .Select(cm => cm.Id)
                .ToListAsync();

            var courseModules = await _context.CourseModules
                .Where(cm => cm.CourseId == request.CourseId && selectedModuleIds.Contains(cm.Id))
                .Include(cm => cm.Items)
                .ToListAsync();

            foreach (var courseModule in courseModules)
            {
                var newModule = new ClassroomModule(
                    classroom.Id,
                    courseModule.Title,
                    courseModule.Description,
                    courseModule.OrderIndex
                );
                _context.ClassroomModules.Add(newModule);
                await _context.SaveChangesAsync(CancellationToken.None);

                foreach (var courseItem in courseModule.Items)
                {
                    _context.ClassroomModuleItems.Add(new ClassroomModuleItem(
                        newModule.Id,
                        courseItem.ItemType,
                        courseItem.LessonId,
                        courseItem.QuizId,
                        courseItem.CodelabId,
                        courseItem.OverrideTitle ?? string.Empty,
                        string.Empty,
                        courseItem.OrderIndex,
                        courseItem.IsRequired
                    ));
                }
            }

            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new { message = "Import khóa học vào lớp thành công!", classroomId = classroom.Id });
        }

        private async Task<Guid?> GetClassroomIdForItem(Guid moduleItemId)
        {
            var item = await _context.ClassroomModuleItems
                .Include(i => i.Module)
                .FirstOrDefaultAsync(i => i.Id == moduleItemId && !i.IsDeleted);

            if (item?.Module == null) return null;

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Modules.Any(m => m.Id == item.ModuleId));

            return classroom?.Id;
        }

        private async Task<ClassroomModuleItem?> GetModuleItemWithClassroom(Guid moduleItemId)
        {
            return await _context.ClassroomModuleItems
                .Include(i => i.Module)
                .FirstOrDefaultAsync(i => i.Id == moduleItemId && !i.IsDeleted);
        }
    }

    public class UpdateProgressRequest
    {
        public int ActiveFrame { get; set; }
        public double ScrollPercent { get; set; }
    }

    public class CompleteItemRequest
    {
        public int? Score { get; set; }
    }

    public class ImportCourseRequest
    {
        public Guid CourseId { get; set; }
        public Guid ClassroomId { get; set; }
        public bool IncludeAllModules { get; set; }
        public List<Guid>? SelectedModuleIds { get; set; }
        public bool OverrideExisting { get; set; }
    }
}