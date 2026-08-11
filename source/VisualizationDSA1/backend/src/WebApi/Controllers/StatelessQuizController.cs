using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.Domain.Engine;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Infrastructure.Data;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/concepts/quiz")]
    public class StatelessQuizController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly VisualizationDSA.Application.Services.IHeartService _heartService;

        public StatelessQuizController(ApplicationDbContext dbContext, VisualizationDSA.Application.Services.IHeartService heartService)
        {
            _dbContext = dbContext;
            _heartService = heartService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var quizzes = await _dbContext.Quizzes
                .Select(q => new { q.Id, q.Title, q.Topic, q.Difficulty, q.XPReward })
                .ToListAsync();

            var result = quizzes.Select(q => new StatelessQuizDto
            {
                Id = q.Id.ToString(),
                Title = q.Title,
                Topic = q.Topic,
                Difficulty = MapToDifficulty(q.Difficulty),
                XpReward = q.XPReward
            }).ToList();

            return Ok(result);
        }

        [HttpGet("topics")]
        public async Task<IActionResult> GetTopics()
        {
            var topics = await _dbContext.Quizzes
                .Select(q => q.Topic)
                .Distinct()
                .ToArrayAsync();
            return Ok(topics.ToList());
        }

        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetById(string quizId)
        {
            var quiz = await LoadQuizDto(quizId);
            if (quiz == null)
                return NotFound(new { error = "QUIZ_NOT_FOUND", quizId });
            return Ok(quiz);
        }

        [HttpGet("topic/{topic}")]
        public async Task<IActionResult> GetByTopic(string topic)
        {
            var quizzes = await _dbContext.Quizzes
                .Include(q => q.Questions)
                .Where(q => q.Topic == topic)
                .ToListAsync();
            return Ok(quizzes.Select(MapQuiz).ToList());
        }

        [HttpPost("submit")]
        [RequireJwtRole]
        public async Task<IActionResult> SubmitAttempt([FromBody] StatelessQuizAttemptRequest request)
        {
            try
            {
                var quiz = await LoadQuizEntity(request.QuizId);
                if (quiz == null)
                    throw new KeyNotFoundException($"Quiz '{request.QuizId}' không tồn tại.");

                var questions = quiz.Questions.OrderBy(q => q.Id).ToList();
                if (request.Answers.Count != questions.Count)
                    throw new ArgumentException($"Số câu trả lời ({request.Answers.Count}) không khớp số câu hỏi ({questions.Count}).");

                int score = 0;
                var results = new List<StatelessQuestionResult>();
                for (int i = 0; i < questions.Count; i++)
                {
                    var q = questions[i];
                    var isCorrect = request.Answers[i] == q.CorrectIndex;
                    if (isCorrect) score++;
                    results.Add(new StatelessQuestionResult
                    {
                        QuestionId = q.Id.ToString(),
                        IsCorrect = isCorrect,
                        CorrectIndex = q.CorrectIndex,
                        Explanation = q.Explanation
                    });
                }

                var maxScore = questions.Count;
                var passed = score >= (int)Math.Ceiling(maxScore * 0.7);

                var userIdStr = JwtHelper.ExtractSubFromToken(Request);
                var user = userIdStr != null && Guid.TryParse(userIdStr, out var userIdGuid)
                    ? await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid)
                    : null;

                int xpEarned = 0;
                if (user != null)
                {
                    var attempt = new QuizAttempt(user.Id, quiz.Id, request.Answers.ToArray(), score, maxScore);
                    _dbContext.QuizAttempts.Add(attempt);

                    // G3.3.3 — Trừ 1 tim khi fail (chống spam; hết tim → OUT_OF_HEARTS)
                    if (!passed)
                    {
                        await _heartService.DeductHeartAtomicAsync(user.Id);
                    }

                    if (passed)
                    {
                        var previousAttempts = await _dbContext.QuizAttempts
                            .Where(a => a.UserId == user.Id && a.QuizId == quiz.Id)
                            .OrderBy(a => a.AttemptedAt)
                            .ToListAsync();

                        var priorPasses = previousAttempts
                            .Where(a => a.Passed && a.Id != attempt.Id)
                            .OrderBy(a => a.AttemptedAt)
                            .ToList();

                        if (priorPasses.Count == 0)
                        {
                            xpEarned = quiz.XPReward;
                        }
                        else
                        {
                            int runningMax = priorPasses[0].Score;
                            bool hasEarnedSecondReward = false;
                            for (int i = 1; i < priorPasses.Count; i++)
                            {
                                var p = priorPasses[i];
                                bool isImprovement = p.Score > runningMax;
                                bool meetsUpgrade = (p.Score - runningMax) / (double)maxScore >= 0.20 || (p.Score == maxScore && runningMax < maxScore);
                                if (isImprovement && meetsUpgrade) { hasEarnedSecondReward = true; break; }
                                if (p.Score > runningMax) runningMax = p.Score;
                            }

                            if (!hasEarnedSecondReward)
                            {
                                int overallMaxPrev = priorPasses.Max(a => a.Score);
                                bool isCurrentImprovement = score > overallMaxPrev;
                                bool currentMeetsUpgrade = (score - overallMaxPrev) / (double)maxScore >= 0.20 || (score == maxScore && overallMaxPrev < maxScore);
                                if (isCurrentImprovement && currentMeetsUpgrade) xpEarned = quiz.XPReward;
                            }
                        }
                    }

                    if (xpEarned > 0)
                    {
                        user.AwardXP(xpEarned);
                        user.RecordActivity();
                    }

                    await _dbContext.SaveChangesAsync();
                }

                return Ok(new StatelessQuizAttemptResult
                {
                    Score = score,
                    MaxScore = maxScore,
                    Passed = passed,
                    XpAwarded = xpEarned,
                    QuestionResults = results
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = "QUIZ_NOT_FOUND", message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "INVALID_ANSWERS", message = ex.Message });
            }
        }

        [HttpPost("manage")]
        [RequireJwtRole("Teacher,Admin")]
        public async Task<IActionResult> ManageQuiz([FromBody] StatelessQuizDto quiz)
        {
            if (quiz == null)
                return BadRequest(new { error = "INVALID_QUIZ", message = "Dữ liệu quiz trống." });

            var validationError = ValidateQuiz(quiz);
            if (validationError != null) return validationError;

            var difficultyInt = quiz.Difficulty switch
            {
                "easy" => 1, "medium" => 3, "hard" => 5, _ => 3
            };

            var dbQuiz = new Quiz(NormalizeText(quiz.Title), NormalizeText(quiz.Title), NormalizeText(quiz.Topic), difficultyInt, quiz.XpReward);
            foreach (var q in quiz.Questions)
            {
                dbQuiz.AddQuestion(
                    NormalizeText(q.Text),
                    q.Options.Select(NormalizeText).ToArray(),
                    q.CorrectIndex,
                    NormalizeText(q.Explanation));
            }
            _dbContext.Quizzes.Add(dbQuiz);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Quiz đã được thêm thành công.", quiz = MapQuiz(dbQuiz) });
        }

        [HttpPut("manage/{quizId}")]
        [RequireJwtRole("Teacher,Admin")]
        public async Task<IActionResult> UpdateQuiz(string quizId, [FromBody] StatelessQuizDto quiz)
        {
            if (quiz == null)
                return BadRequest(new { error = "INVALID_QUIZ", message = "Dữ liệu quiz trống." });

            var validationError = ValidateQuiz(quiz);
            if (validationError != null) return validationError;

            var dbQuiz = await LoadQuizEntity(quizId);
            if (dbQuiz == null)
                return NotFound(new { error = "QUIZ_NOT_FOUND", message = $"Không tìm thấy quiz với ID {quizId} để cập nhật." });

            var difficultyInt = quiz.Difficulty switch
            {
                "easy" => 1, "medium" => 3, "hard" => 5, _ => 3
            };

            dbQuiz.Update(NormalizeText(quiz.Title), NormalizeText(quiz.Title), NormalizeText(quiz.Topic), difficultyInt, quiz.XpReward);
            dbQuiz.ClearQuestions();
            foreach (var q in quiz.Questions)
            {
                dbQuiz.AddQuestion(
                    NormalizeText(q.Text),
                    q.Options.Select(NormalizeText).ToArray(),
                    q.CorrectIndex,
                    NormalizeText(q.Explanation));
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Quiz đã được cập nhật thành công.", quiz = MapQuiz(dbQuiz) });
        }

        [HttpDelete("manage/{quizId}")]
        [RequireJwtRole("Teacher,Admin")]
        public async Task<IActionResult> DeleteQuiz(string quizId)
        {
            var dbQuiz = await LoadQuizEntity(quizId);
            if (dbQuiz == null)
                return NotFound(new { error = "QUIZ_NOT_FOUND", message = $"Không tìm thấy quiz với ID {quizId} để xóa." });

            _dbContext.Quizzes.Remove(dbQuiz);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Quiz đã được xóa thành công." });
        }

        [HttpGet("analytics")]
        [RequireJwtRole("Teacher,Admin")]
        public async Task<IActionResult> GetAnalytics()
        {
            var totalQuizzes = await _dbContext.Quizzes.CountAsync();
            var totalQuestionsInBank = await _dbContext.QuizQuestions.CountAsync();

            var topicBreakdown = await _dbContext.Quizzes
                .GroupBy(q => q.Topic)
                .Select(g => new { topic = g.Key, quizCount = g.Count() })
                .OrderByDescending(t => t.quizCount)
                .ToListAsync();

            var totalAttempts = await _dbContext.QuizAttempts.CountAsync();
            var totalPassed = await _dbContext.QuizAttempts.CountAsync(a => a.Passed);
            var passRate = totalAttempts > 0 ? Math.Round((double)totalPassed / totalAttempts * 100, 1) : 0.0;
            var averageScore = totalAttempts > 0 ? Math.Round(await _dbContext.QuizAttempts.AverageAsync(a => (double)a.Score / a.MaxScore * 100), 1) : 0.0;

            var perQuizStats = await _dbContext.Quizzes
                .Select(q => new
                {
                    quizId = q.Id.ToString(),
                    title = q.Title,
                    topic = q.Topic,
                    difficulty = q.Difficulty == 1 ? "easy" : q.Difficulty == 5 ? "hard" : "medium",
                    questionCount = q.Questions.Count,
                    xpReward = q.XPReward,
                    totalAttempts = q.Attempts.Count,
                    passedCount = q.Attempts.Count(a => a.Passed),
                    avgScore = q.Attempts.Count > 0 ? Math.Round(q.Attempts.Average(a => (double)a.Score / a.MaxScore * 100), 1) : 0.0,
                    passRatePercent = q.Attempts.Count > 0 ? Math.Round((double)q.Attempts.Count(a => a.Passed) / q.Attempts.Count * 100, 1) : 0.0
                })
                .OrderByDescending(q => q.totalAttempts)
                .ToListAsync();

            var totalUsers = await _dbContext.Users.CountAsync();
            var premiumUsers = await _dbContext.Users.CountAsync(u => u.IsPremium);

            return Ok(new
            {
                totalQuizzes, totalQuestionsInBank, totalAttempts, totalPassed, passRate, averageScore,
                totalUsers, premiumUsers, topicBreakdown, perQuizStats
            });
        }

        [HttpGet("history")]
        [RequireJwtRole]
        public async Task<IActionResult> GetHistory([FromQuery] string? userId)
        {
            var currentUserId = JwtHelper.ExtractSubFromToken(Request);
            var targetUserId = userId ?? currentUserId;
            if (targetUserId != currentUserId && !JwtHelper.IsTeacherOrAdmin(Request))
            {
                return StatusCode(403, new { error = "FORBIDDEN", message = "Bạn không có quyền xem lịch sử của người khác." });
            }

            if (!Guid.TryParse(targetUserId, out var guidUserId))
                return BadRequest(new { error = "INVALID_USER_ID" });

            var attempts = await _dbContext.QuizAttempts
                .Include(a => a.Quiz)
                .Where(a => a.UserId == guidUserId)
                .OrderByDescending(a => a.AttemptedAt)
                .Select(a => new
                {
                    a.Id, a.QuizId, quizTitle = a.Quiz.Title, quizTopic = a.Quiz.Topic,
                    a.Score, a.MaxScore, a.Passed, a.AttemptedAt, a.Answers
                })
                .ToListAsync();

            return Ok(attempts);
        }

        private async Task<StatelessQuizDto?> LoadQuizDto(string quizId)
        {
            var quiz = await LoadQuizEntity(quizId);
            return quiz == null ? null : MapQuiz(quiz);
        }

        private async Task<Quiz?> LoadQuizEntity(string quizId)
        {
            var query = _dbContext.Quizzes.Include(q => q.Questions).AsQueryable();
            if (Guid.TryParse(quizId, out var guid))
                return await query.FirstOrDefaultAsync(q => q.Id == guid);
            return await query.FirstOrDefaultAsync(q => q.Title == quizId);
        }

        private IActionResult? ValidateQuiz(StatelessQuizDto quiz)
        {
            if (string.IsNullOrWhiteSpace(quiz.Title))
                return BadRequest(new { error = "INVALID_QUIZ", message = "Quiz phải có tiêu đề." });
            if (quiz.Title.Length > 200)
                return BadRequest(new { error = "INVALID_QUIZ", message = "Tiêu đề quiz không được vượt quá 200 ký tự." });
            if (quiz.Questions == null || quiz.Questions.Count == 0)
                return BadRequest(new { error = "INVALID_QUIZ", message = "Quiz phải có ít nhất 1 câu hỏi." });
            if (quiz.Questions.Count > 100)
                return BadRequest(new { error = "INVALID_QUIZ", message = "Số lượng câu hỏi trong một bài quiz tối đa là 100." });

            for (int i = 0; i < quiz.Questions.Count; i++)
            {
                var q = quiz.Questions[i];
                if (string.IsNullOrWhiteSpace(q.Text))
                    return BadRequest(new { error = "INVALID_QUIZ", message = $"Câu hỏi thứ {i + 1} không được để trống nội dung." });
                if (q.Text.Length > 1000)
                    return BadRequest(new { error = "INVALID_QUIZ", message = $"Nội dung câu hỏi thứ {i + 1} không được dài quá 1000 ký tự." });
                if (q.Options == null || q.Options.Count < 2 || q.Options.Count > 10)
                    return BadRequest(new { error = "INVALID_QUIZ", message = $"Câu hỏi thứ {i + 1} phải có từ 2 đến 10 đáp án lựa chọn." });
                if (q.CorrectIndex < 0 || q.CorrectIndex >= q.Options.Count)
                    return BadRequest(new { error = "INVALID_QUIZ", message = $"Đáp án đúng của câu hỏi thứ {i + 1} không hợp lệ." });
            }
            return null;
        }

        private static StatelessQuizDto MapQuiz(Quiz q) => new()
        {
            Id = q.Id.ToString(),
            Title = q.Title,
            Topic = q.Topic,
            Difficulty = MapToDifficulty(q.Difficulty),
            XpReward = q.XPReward,
            Questions = q.Questions.Select(x => new StatelessQuestionDto
            {
                Id = x.Id.ToString(),
                Text = x.Question,
                Options = x.Options.ToList(),
                CorrectIndex = -1, // Ẩn đáp án khỏi payload học viên (chống gian lận — UC-11)
                Explanation = x.Explanation
            }).ToList()
        };

        private static string MapToDifficulty(int d) => d switch { 1 => "easy", 5 => "hard", _ => "medium" };

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(text.Trim(), @"\s+", " ");
        }
    }
}