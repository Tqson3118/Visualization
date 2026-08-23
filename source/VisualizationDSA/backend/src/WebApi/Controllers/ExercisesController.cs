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
    /// <summary>Màn 15 — /api/v1/exercises wrapper on Quiz (Module F).</summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/exercises")]
    [RequireJwtRole]
    public class ExercisesController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;
        public ExercisesController(IApplicationDbContext ctx) { _ctx = ctx; }

        private Guid Uid() => Guid.TryParse(JwtHelper.ExtractSubFromToken(Request), out var g) ? g : Guid.Empty;

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int? lessonId, [FromQuery] string? nodeId, [FromQuery] int? stage, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var quizzes = await _ctx.Quizzes.AsNoTracking().IgnoreQueryFilters().Where(q => !q.IsDeleted).OrderBy(q => q).ToListAsync();
            var itemList = quizzes.Select(q => (object)new
            {
                id = q.Id.ToString(), title = q.Title, description = q.Description, type = "MCQ",
                lessonId = (string?)null, nodeId = (object?)null, stage = (int?)1, durationMinutes = 10,
                maxScore = q.Questions?.Count ?? 0, status = "active", completedByUserCount = 0,
            }).ToList();
            var paged = PagedResponse<object>.Create(itemList, page, pageSize);
            return Ok(new { items = paged.Items, page = paged.Page, pageSize = paged.PageSize, total = paged.Total, totalPages = paged.TotalPages });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(string id)
        {
            var qid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var quiz = await _ctx.Quizzes.AsNoTracking().IgnoreQueryFilters().Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == qid && !q.IsDeleted);
            if (quiz == null) return NotFound();
            return Ok(new
            {
                id = quiz.Id.ToString(), title = quiz.Title, description = quiz.Description, type = "MCQ",
                lessonId = (string?)null, nodeId = (object?)null, stage = 1, durationMinutes = 10,
                maxScore = quiz.Questions.Count, status = "active",
                questions = quiz.Questions.Select(q => new { id = q.Id.ToString(), content = q.Question, type = "SINGLE", options = q.Options, points = 1 }).ToList(),
            });
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit(string id, [FromBody] ExerciseSubmitRequest req) => await Grade(id, req, save: true);

        [HttpPost("{id}/practice")]
        public async Task<IActionResult> Practice(string id, [FromBody] ExerciseSubmitRequest req) => await Grade(id, req, save: false);

        private async Task<IActionResult> Grade(string id, ExerciseSubmitRequest req, bool save)
        {
            var qid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var quiz = await _ctx.Quizzes.AsNoTracking().IgnoreQueryFilters().Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == qid && !q.IsDeleted);
            if (quiz == null) return NotFound();
            var answers = req.Answers ?? new List<ExerciseAnswerStorage>();
            var results = new List<object>();
            var score = 0;
            var max = quiz.Questions.Count;
            foreach (var q in quiz.Questions)
            {
                var sel = answers.FirstOrDefault(a => Guid.TryParse(a.QuestionId, out var qg) && qg == q.Id);
                var correct = sel != null && sel.Selected != null && sel.Selected.Contains(q.CorrectIndex);
                if (correct) score++;
                results.Add(new { questionId = q.Id.ToString(), correct, correctAnswer = new[] { q.CorrectIndex }, explanation = q.Explanation });
            }
            Guid? subId = null;
            if (save)
            {
                var attempt = new QuizAttempt(Uid(), qid, answers.SelectMany(a => a.Selected ?? new int[0]).ToArray(), score, max);
                _ctx.Set<QuizAttempt>().Add(attempt);
                await _ctx.SaveChangesAsync(CancellationToken.None);
                subId = attempt.Id;
            }
            return Ok(new { score, maxScore = max, results, submissionId = subId?.ToString(), submittedAt = DateTime.UtcNow.ToString("o") });
        }

        [HttpGet("{id}/submissions/me")]
        public async Task<IActionResult> MySubmissions(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var qid = Guid.TryParse(id, out var g) ? g : Guid.Empty;
            var attempts = await _ctx.Set<QuizAttempt>().AsNoTracking().Where(a => a.UserId == Uid() && a.QuizId == qid).OrderByDescending(a => a.AttemptedAt).ToListAsync();
            var list = attempts.Select(a => (object)new
            {
                id = a.Id.ToString(), userId = a.UserId.ToString(), userDisplayName = (string?)null, score = a.Score,
                durationSeconds = (int?)null, classAssignmentId = (object?)null, submittedAt = a.AttemptedAt.ToString("o"),
            }).ToList();
            var paged = PagedResponse<object>.Create(list, page, pageSize);
            return Ok(new { items = paged.Items, page = paged.Page, pageSize = paged.PageSize, total = paged.Total, totalPages = paged.TotalPages });
        }

        [HttpGet("{id}/code-submissions/me")]
        public async Task<IActionResult> MyCodeSubmissions(string id) => Ok(new List<object>());

        [HttpPost("{id}/code-submit")]
        public async Task<IActionResult> CodeSubmit(string id, [FromBody] CodeSubmitRequest req)
            => Ok(new { score = 0, passed = 0, total = 0, results = new List<object>() });
    }

    public class ExerciseAnswerStorage { public string? QuestionId { get; set; } public int[]? Selected { get; set; } }
    public class ExerciseSubmitRequest { public List<ExerciseAnswerStorage>? Answers { get; set; } public string? ClassAssignmentId { get; set; } }
    public class CodeSubmitRequest { public string? Code { get; set; } public string? ClassAssignmentId { get; set; } }
}
