using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/progress")]
    [RequireJwtRole]
    public class ProgressController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;
        public ProgressController(IApplicationDbContext ctx) { _ctx = ctx; }

        /// <summary>GET /progress/me — ProgressOverviewDto (ProfileView + PathRedirectView)</summary>
        [HttpGet("me")]
        public async Task<IActionResult> Overview()
        {
            var uid = Guid.TryParse(JwtHelper.ExtractSubFromToken(Request), out var g) ? g : Guid.Empty;
            var lessons = await _ctx.Lessons.AsNoTracking().Where(l => !l.IsDeleted).ToListAsync();
            var viewed = await _ctx.Set<UserLessonProgress>().AsNoTracking()
                .Where(p => p.UserId == uid && (p.Status == "Completed" || p.Status == "InProgress" || p.Status == "Viewed"))
                .ToListAsync();
            var completed = viewed.Where(p => p.Status == "Completed").Select(p => p.LessonId).ToHashSet();
            var courses = await _ctx.Courses.AsNoTracking().Where(c => !c.IsDeleted).ToListAsync();

            // Nhóm bài theo module → topic (id = module index, gần gũi FE)
            var topics = new List<object>();
            var coursesWithModules = await _ctx.CourseModules.AsNoTracking().OrderBy(m => m.OrderIndex).ToListAsync();
            var modulesByCourse = coursesWithModules.GroupBy(m => m.CourseId).ToDictionary(g => g.Key, g => g.ToList());
            var courseTitles = courses.ToDictionary(c => c.Id, c => c.Title);
            var idx = 0;
            foreach (var kv in modulesByCourse)
            {
                idx++;
                var course = courses.FirstOrDefault(c => c.Id == kv.Key);
                var moduleLessons = new List<object>();
                var moduleItems = await _ctx.Set<ModuleItem>().AsNoTracking().Where(mi => kv.Value.Select(m => m.Id).Contains(mi.ModuleId)).ToListAsync();
                foreach (var item in moduleItems.Where(mi => mi.LessonId.HasValue))
                {
                    var lesson = lessons.FirstOrDefault(l => l.Id == item.LessonId.Value);
                    if (lesson == null) continue;
                    var best = await _ctx.Set<QuizAttempt>().AsNoTracking()
                        .Where(a => a.UserId == uid && a.QuizId == (item.QuizId ?? Guid.Empty))
                        .Select(a => (int?)a.Score).FirstOrDefaultAsync();
                    moduleLessons.Add(new
                    {
                        id = lesson.Id.ToString(),
                        title = lesson.Title,
                        viewed = viewed.Any(v => v.LessonId == lesson.Id),
                        bestScore = best,
                        completed = completed.Contains(lesson.Id),
                    });
                }
                topics.Add(new
                {
                    id = $"{kv.Key}",
                    name = courseTitles.TryGetValue(kv.Key, out var t) ? t : "Khóa học",
                    progressPct = 0,
                    lessons = moduleLessons,
                });
            }

            var quizzes = await _ctx.Quizzes.AsNoTracking().CountAsync();
            var attemptsCount = await _ctx.Set<QuizAttempt>().AsNoTracking().CountAsync(a => a.UserId == uid);
            return Ok(new
            {
                lessonsViewed = viewed.Count,
                lessonsTotal = lessons.Count,
                exercisesCompleted = attemptsCount,
                exercisesTotal = quizzes,
                avgScore = (object?)null,
                topics,
            });
        }
    }
}
