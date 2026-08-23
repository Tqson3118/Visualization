using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Domain.Enums;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    /// <summary>Màn 10/11 — /api/v1/classes alias cho Classroom (Module H).</summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/classes")]
    public class ClassesController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;
        public ClassesController(IApplicationDbContext ctx) { _ctx = ctx; }

        private Guid? Uid() => Guid.TryParse(JwtHelper.ExtractSubFromToken(Request), out var g) ? g : (Guid?)null;

        /// <summary>GET /classes — ClassDto[]</summary>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var uid = Uid();
            var classrooms = await _ctx.Classrooms.AsNoTracking()
                .Where(c => !c.IsArchived).OrderBy(c => c.Name).ToListAsync();
            var enrollments = await _ctx.Set<ClassroomEnrollment>().AsNoTracking()
                .Where(e => e.Status == EnrollmentStatus.Active).ToListAsync();
            var result = classrooms.Select(c =>
            {
                var members = enrollments.Count(e => e.ClassroomId == c.Id);
                string role = "STUDENT";
                if (uid.HasValue)
                {
                    if (c.OwnerTeacherId == uid.Value) role = "OWNER";
                    else if (enrollments.Any(e => e.ClassroomId == c.Id && e.StudentId == uid.Value)) role = "STUDENT";
                    else role = "TEACHER";
                }
                return new
                {
                    id = c.Id.ToString(), name = c.Name, description = (string?)c.Description, inviteCode = c.InviteCode,
                    ownerId = c.OwnerTeacherId.ToString(), memberCount = members, createdAt = c.CreatedAt.ToString("o"), role,
                };
            }).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            var cid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var c = await _ctx.Classrooms.AsNoTracking().Include(x => x.Enrollments).FirstOrDefaultAsync(x => x.Id == cid && !x.IsArchived);
            if (c == null) return NotFound();
            var active = c.Enrollments.Where(e => e.Status == EnrollmentStatus.Active).ToList();
            return Ok(new
            {
                id = c.Id.ToString(), name = c.Name, description = (string?)c.Description, inviteCode = c.InviteCode,
                ownerId = c.OwnerTeacherId.ToString(), memberCount = active.Count, createdAt = c.CreatedAt.ToString("o"),
                role = Uid() == c.OwnerTeacherId ? "OWNER" : "STUDENT",
            });
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> Members(string id)
        {
            var cid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var rows = await _ctx.Set<ClassroomEnrollment>().AsNoTracking()
                .Include(e => e.Student)
                .Where(e => e.ClassroomId == cid && e.Status == EnrollmentStatus.Active)
                .OrderBy(e => e.JoinedAt).ToListAsync();
            return Ok(rows.Select(e => new
            {
                userId = e.StudentId.ToString(), displayName = e.Student.Username, email = e.Student.Email, joinedAt = e.JoinedAt.ToString("o"),
            }).ToList());
        }

        [HttpPost("join-by-code")]
        public async Task<IActionResult> JoinByCode([FromBody] JoinByCodeRequest req)
        {
            var uid = Uid();
            if (!uid.HasValue) return Unauthorized();
            var code = (req.InviteCode ?? "").Trim();
            var c = await _ctx.Classrooms.AsNoTracking().FirstOrDefaultAsync(x => x.InviteCode == code && !x.IsArchived);
            if (c == null) return NotFound(new { message = "Mã mời không hợp lệ." });
            var exists = await _ctx.Set<ClassroomEnrollment>().AsNoTracking()
                .AnyAsync(e => e.ClassroomId == c.Id && e.StudentId == uid.Value && e.Status == EnrollmentStatus.Active);
            if (!exists) { _ctx.Set<ClassroomEnrollment>().Add(new ClassroomEnrollment(c.Id, uid.Value)); await _ctx.SaveChangesAsync(CancellationToken.None); }
            var active = await _ctx.Set<ClassroomEnrollment>().AsNoTracking().CountAsync(e => e.ClassroomId == c.Id && e.Status == EnrollmentStatus.Active);
            return Ok(new
            {
                id = c.Id.ToString(), name = c.Name, description = (string?)c.Description, inviteCode = c.InviteCode,
                ownerId = c.OwnerTeacherId.ToString(), memberCount = active, createdAt = c.CreatedAt.ToString("o"), role = "STUDENT",
            });
        }

        [HttpGet("{id}/assignments")]
        public async Task<IActionResult> Assignments(string id)
        {
            var cid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var lessons = await _ctx.Set<ClassroomLesson>().AsNoTracking().Where(l => l.ClassroomId == cid).OrderBy(l => l).ToListAsync();
            var list = new List<object>();
            var sort = 1;
            foreach (var l in lessons)
            {
                var lesson = await _ctx.Lessons.AsNoTracking().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == l.LessonId);
                list.Add(new { id = l.Id.ToString(), lessonId = l.LessonId.ToString(), exerciseId = (string?)null, title = lesson?.Title,
                    dueAt = (string?)null, allowLateSubmission = false, sortOrder = sort++, createdAt = DateTime.UtcNow.ToString("o") });
            }
            return Ok(list);
        }

        [HttpGet("{id}/curriculum")]
        public async Task<IActionResult> Curriculum(string id)
        {
            var cid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var c = await _ctx.Classrooms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cid);
            if (c == null) return NotFound();
            var lessons = await _ctx.Set<ClassroomLesson>().AsNoTracking().Where(l => l.ClassroomId == cid).OrderBy(l => l).ToListAsync();
            var items = lessons.Select(l => new
            {
                assignmentId = l.Id.ToString(), lessonId = l.LessonId.ToString(), exerciseId = (string?)null,
                title = "Bài học", itemType = "lesson", sortOrder = 1, dueAt = (string?)null, status = "not_started", bestScore = (int?)null,
            }).ToList();
            return Ok(new { classId = cid.ToString(), title = (string?)null, description = (string?)null, published = true, progressPct = 0, items });
        }

        [HttpGet("{id}/report")]
        public async Task<IActionResult> Report(string id)
        {
            var cid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var c = await _ctx.Classrooms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cid);
            if (c == null) return NotFound();
            var members = await _ctx.Set<ClassroomEnrollment>().AsNoTracking().CountAsync(e => e.ClassroomId == cid && e.Status == EnrollmentStatus.Active);
            return Ok(new { classId = cid.ToString(), className = c.Name, totalMembers = members, assignments = new List<object>(), laggingLearners = new List<object>() });
        }
    }

    public class JoinByCodeRequest { public string? InviteCode { get; set; } }
}
