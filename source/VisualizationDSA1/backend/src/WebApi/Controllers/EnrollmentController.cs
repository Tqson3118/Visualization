using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Infrastructure.Data;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/enrollments")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private const int MaxActiveEnrollments = 3;

        private readonly ApplicationDbContext _dbContext;

        public EnrollmentController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private Guid? GetUserId()
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            return userIdStr != null && Guid.TryParse(userIdStr, out var id) ? id : null;
        }

        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollRequestDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (dto.RoadmapId == Guid.Empty)
                return BadRequest(new { error = "INVALID_ROADMAP", message = "RoadmapId không hợp lệ." });

            var roadmap = await _dbContext.CustomRoadmaps.FirstOrDefaultAsync(r => r.Id == dto.RoadmapId);
            if (roadmap == null)
            {
                // Fallback: cho phép đăng ký theo Course (roadmap của frontend "Thư viện Lộ trình" dùng Course entity)
                var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.Id == dto.RoadmapId);
                if (course == null)
                    return NotFound(new { error = "ROADMAP_NOT_FOUND", message = "Không tìm thấy lộ trình." });
            }

            var existing = await _dbContext.RoadmapEnrollments
                .FirstOrDefaultAsync(e => e.UserId == userId.Value && e.RoadmapId == dto.RoadmapId);
            if (existing != null)
            {
                if (existing.Status == "Active")
                    return Conflict(new { error = "ALREADY_ENROLLED", message = "Bạn đã đăng ký roadmap này." });
                if (existing.Status == "Dropped")
                {
                    // Đăng ký lại roadmap đã rời bỏ
                    _dbContext.RoadmapEnrollments.Remove(existing);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    return Conflict(new { error = "ALREADY_ENROLLED", message = "Bạn đã hoàn thành roadmap này." });
                }
            }

            var activeCount = await _dbContext.RoadmapEnrollments
                .CountAsync(e => e.UserId == userId.Value && e.Status == "Active");
            if (activeCount >= MaxActiveEnrollments)
            {
                return BadRequest(new
                {
                    error = "MAX_ENROLLMENTS",
                    message = $"Bạn đã đạt tối đa {MaxActiveEnrollments} roadmap đang học. Hoàn thành hoặc rời bỏ 1 roadmap để đăng ký mới."
                });
            }

            var enrollment = new RoadmapEnrollment(userId.Value, dto.RoadmapId);
            _dbContext.RoadmapEnrollments.Add(enrollment);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyEnrollments), new { id = enrollment.Id }, new
            {
                id = enrollment.Id,
                roadmapId = enrollment.RoadmapId,
                status = enrollment.Status,
                enrolledAt = enrollment.EnrolledAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DropEnrollment(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var enrollment = await _dbContext.RoadmapEnrollments
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId.Value);
            if (enrollment == null)
                return NotFound(new { error = "ENROLLMENT_NOT_FOUND", message = "Không tìm thấy đăng ký." });

            if (enrollment.Status == "Active")
                enrollment.Drop();
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyEnrollments([FromQuery] string? status = null)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var query = _dbContext.RoadmapEnrollments
                .Where(e => e.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.Status == status);

            var items = await query
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            var result = new List<object>();
            foreach (var e in items)
            {
                var roadmapId = e.RoadmapId;
                int totalNodes = await _dbContext.CustomNodes.CountAsync(n => n.RoadmapId == roadmapId);
                int completedNodes = 0;
                var nodes = await _dbContext.CustomNodes.Where(n => n.RoadmapId == roadmapId).ToListAsync();
                foreach (var node in nodes)
                {
                    if (node.QuizId == null && node.LabId == null && node.LeetCodeId == null) continue;
                    var lessonLink = await _dbContext.ModuleItems.FirstOrDefaultAsync(m => m.CodelabId == node.LabId);
                    if (lessonLink != null)
                    {
                        var lp = await _dbContext.UserLessonProgresses
                            .FirstOrDefaultAsync(p => p.UserId == userId.Value && p.LessonId == lessonLink.LessonId);
                        if (lp != null && lp.Status == "Completed") completedNodes++;
                    }
                }
                int progressPercent = totalNodes == 0 ? 0 : (int)Math.Round((double)completedNodes / totalNodes * 100);

                result.Add(new
                {
                    id = e.Id,
                    roadmapId = e.RoadmapId,
                    status = e.Status,
                    enrolledAt = e.EnrolledAt,
                    progressPercent = Math.Clamp(progressPercent, 0, 100)
                });
            }

            return Ok(new { items = result });
        }
    }

    public class EnrollRequestDto
    {
        public Guid RoadmapId { get; set; }
    }
}
