using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VisualizationDSA.Application.DTOs;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    /// <summary>Màn 22/admin + NodeHub — /api/v1/lessons (FE lessons.ts).</summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/lessons")]
    public class LessonsController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;
        public LessonsController(IApplicationDbContext ctx) { _ctx = ctx; }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int? topicId, [FromQuery] string? status, [FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var lessons = await _ctx.Lessons.AsNoTracking().IgnoreQueryFilters().Where(l => !l.IsDeleted).OrderBy(l => l.CreatedAt).ToListAsync();
            var result = lessons.Select(l => (object)new
            {
                id = l.Id.ToString(), title = l.Title, description = "", topicId = 0, sortOrder = 0,
                status = MapStatus(l.PublishStatus), simulationCount = 0, exerciseCount = 0, progress = (object?)null,
            }).ToList();
            var paged = PagedResponse<object>.Create(result, page, pageSize);
            return Ok(new { items = paged.Items, page = paged.Page, pageSize = paged.PageSize, total = paged.Total, totalPages = paged.TotalPages });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(string id, [FromQuery] bool includeContent = false)
        {
            var lid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var lesson = await _ctx.Lessons.AsNoTracking().IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == lid && !l.IsDeleted);
            if (lesson == null) return NotFound();
            return Ok(new
            {
                id = lesson.Id.ToString(), title = lesson.Title, description = "", topicId = 0, sortOrder = 0,
                status = MapStatus(lesson.PublishStatus), simulationCount = 0, exerciseCount = 0, progress = (object?)null,
                contentHtml = lesson.ContentMd, simulations = new List<object>(), exercises = new List<object>(),
            });
        }

        [HttpPost("{id}/mark-viewed")]
        public async Task<IActionResult> MarkViewed(string id)
        {
            var lid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var uid = Guid.TryParse(JwtHelper.ExtractSubFromToken(Request), out var ug) ? ug : Guid.Empty;
            var exists = await _ctx.Lessons.AsNoTracking().AnyAsync(l => l.Id == lid && !l.IsDeleted);
            if (!exists) return NotFound();
            if (uid != Guid.Empty)
            {
                var prog = await _ctx.Set<UserLessonProgress>().FirstOrDefaultAsync(p => p.UserId == uid && p.LessonId == lid);
                if (prog == null)
                {
                    _ctx.Set<UserLessonProgress>().Add(new UserLessonProgress(uid, lid, "Viewed"));
                    await _ctx.SaveChangesAsync(CancellationToken.None);
                }
            }
            return Ok(new { viewed = true });
        }

        [HttpPost("{id}/progress")]
        public async Task<IActionResult> Progress(string id)
        {
            var lid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var lesson = await _ctx.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lid);
            if (lesson == null) return NotFound();
            var uid = Guid.TryParse(JwtHelper.ExtractSubFromToken(Request), out var ug) ? ug : Guid.Empty;
            if (uid != Guid.Empty)
            {
                var prog = await _ctx.Set<UserLessonProgress>().FirstOrDefaultAsync(p => p.UserId == uid && p.LessonId == lid);
                if (prog == null) { _ctx.Set<UserLessonProgress>().Add(new UserLessonProgress(uid, lid, "InProgress")); await _ctx.SaveChangesAsync(CancellationToken.None); }
            }
            return Ok(new { viewed = true, bestScore = (int?)null, completed = false });
        }

        private static string MapStatus(VisualizationDSA.Domain.Enums.LessonPublishStatus s)
            => s switch
            {
                VisualizationDSA.Domain.Enums.LessonPublishStatus.Published => "active",
                VisualizationDSA.Domain.Enums.LessonPublishStatus.PendingReview => "pendingreview",
                VisualizationDSA.Domain.Enums.LessonPublishStatus.Draft or VisualizationDSA.Domain.Enums.LessonPublishStatus.Rejected => "draft",
                _ => "hidden",
            };
    }
}
