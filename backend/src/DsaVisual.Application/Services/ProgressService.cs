using System.Globalization;
using System.Text;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Application.Services;

/// <summary>
/// ProgressService thật theo SDD §7.3.4/§7.6 + API_REFERENCE.md §4.7.
/// Báo cáo giảng viên theo mẫu SQL SDD §7.6.1; CSV UTF-8 BOM tên file report_lessons_{id}_{date}.csv.
/// </summary>
public sealed class ProgressService(
    AppDbContext db,
    IDateTimeProvider clock) : IProgressService
{
    private const string RoleTeacher = "TEACHER";
    private const string RoleAdmin = "ADMIN";

    public async Task<Result<ProgressOverviewDto>> GetMyOverviewAsync(int userId, CancellationToken ct)
    {
        var (lessonsTotal, exercisesTotal, topics) = await LoadCountsAsync(ct);

        var viewed = await db.UserProgress.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new { p.LessonId, p.Viewed, p.BestScore, p.CompletedAt })
            .ToListAsync(ct);

        var lessonsViewed = viewed.Count(p => p.Viewed);
        var exercisesCompleted = await db.ExerciseSubmissions.AsNoTracking()
            .Where(s => s.UserId == userId && s.Score > 0)
            .Select(s => s.ExerciseId)
            .Distinct()
            .CountAsync(ct);

        var bestScores = viewed.Where(p => p.BestScore != null).Select(p => p.BestScore!.Value).ToList();
        var avgScore = bestScores.Count > 0 ? bestScores.Average() : 0;

        var topicProgress = topics.Select(t => new TopicProgressDto
        {
            Id = t.Id,
            Name = t.Name,
            ProgressPct = t.TotalLessons == 0 ? 0 : (int)Math.Round(t.ViewedLessons * 100.0 / t.TotalLessons),
            Lessons = t.Lessons.Select(l => new LessonProgressItemDto
            {
                Id = l.Id,
                Title = l.Title,
                Viewed = l.Viewed,
                BestScore = l.BestScore,
                Completed = l.Completed
            }).ToList()
        }).ToList();

        return Result<ProgressOverviewDto>.Ok(new ProgressOverviewDto
        {
            LessonsViewed = lessonsViewed,
            LessonsTotal = lessonsTotal,
            ExercisesCompleted = exercisesCompleted,
            ExercisesTotal = exercisesTotal,
            AvgScore = Math.Round(avgScore, 1),
            Topics = topicProgress
        });
    }

    public async Task<Result<LessonProgressDetailDto>> GetMyLessonAsync(int userId, int lessonId, CancellationToken ct)
    {
        var lesson = await db.Lessons.AsNoTracking()
            .Include(l => l.Exercises.Where(e => e.DeletedAt == null))
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
        if (lesson is null)
        {
            return Result<LessonProgressDetailDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        var progress = await db.UserProgress.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);

        var exerciseIds = lesson.Exercises.Select(e => e.Id).ToList();
        var bestByExercise = await db.ExerciseSubmissions.AsNoTracking()
            .Where(s => s.UserId == userId && exerciseIds.Contains(s.ExerciseId))
            .GroupBy(s => s.ExerciseId)
            .Select(g => new { ExerciseId = g.Key, Best = g.Max(s => s.Score) })
            .ToDictionaryAsync(x => x.ExerciseId, x => x.Best, ct);

        return Result<LessonProgressDetailDto>.Ok(new LessonProgressDetailDto
        {
            LessonId = lesson.Id,
            Title = lesson.Title,
            Viewed = progress?.Viewed ?? false,
            SimulationCount = progress?.SimulationCount ?? 0,
            BestScore = progress?.BestScore,
            Completed = progress?.CompletedAt != null,
            UpdatedAt = progress?.UpdatedAt ?? lesson.CreatedAt,
            Exercises = lesson.Exercises.Select(e => new ExerciseProgressItemDto
            {
                Id = e.Id,
                Title = e.Title,
                BestScore = bestByExercise.GetValueOrDefault(e.Id),
                Completed = bestByExercise.ContainsKey(e.Id)
            }).ToList()
        });
    }

    public async Task<Result<TeacherReportDto>> GetReportAsync(int userId, string role, int lessonId, CancellationToken ct)
    {
        var lesson = await db.Lessons.AsNoTracking()
            .Include(l => l.Exercises.Where(e => e.DeletedAt == null))
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
        if (lesson is null)
        {
            return Result<TeacherReportDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        if (!IsTeacherOrAdmin(role))
        {
            return Result<TeacherReportDto>.Fail(ErrorCodes.FORBIDDEN, "Chỉ giảng viên/Admin xem được báo cáo");
        }

        if (IsTeacher(role) && lesson.CreatedBy != userId)
        {
            return Result<TeacherReportDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem báo cáo bài học này");
        }

        // Mẫu SQL SDD §7.6.1: learners từ UserProgress JOIN Users (Role=Student, chưa xóa)
        var learners = await db.UserProgress.AsNoTracking()
            .Where(p => p.LessonId == lessonId)
            .Join(db.Users.AsNoTracking().Where(u => u.Role == UserRole.Student && u.DeletedAt == null),
                p => p.UserId, u => u.Id, (p, u) => new { p.UserId, p.Viewed, p.BestScore, u.DisplayName, u.Email })
            .ToListAsync(ct);

        var totalLearners = learners.Count;
        var learnersViewed = learners.Count(l => l.Viewed);
        var avgScore = learners.Where(l => l.BestScore != null).Select(l => l.BestScore!.Value).ToList();
        var average = avgScore.Count > 0 ? avgScore.Average() : 0;

        var exerciseIds = lesson.Exercises.Select(e => e.Id).ToList();
        var submissions = await db.ExerciseSubmissions.AsNoTracking()
            .Where(s => exerciseIds.Contains(s.ExerciseId))
            .GroupBy(s => s.ExerciseId)
            .Select(g => new { ExerciseId = g.Key, Count = g.Count(), Avg = g.Average(s => (double)s.Score) })
            .ToListAsync(ct);
        var submissionMap = submissions.ToDictionary(s => s.ExerciseId);

        var inactive = learners.Where(l => !l.Viewed)
            .Select(l => new UserSummary
            {
                Id = l.UserId,
                DisplayName = l.DisplayName,
                Email = IsTeacher(role) ? EmailMasker.Mask(l.Email) : l.Email,
                Role = RoleNames.Student,
                CreatedAt = default
            })
            .ToList();

        return Result<TeacherReportDto>.Ok(new TeacherReportDto
        {
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            TotalLearners = totalLearners,
            LearnersViewed = learnersViewed,
            CompletionPct = totalLearners == 0 ? 0 : Math.Round(learnersViewed * 100.0 / totalLearners, 1),
            AvgScore = Math.Round(average, 1),
            Exercises = lesson.Exercises.Select(e => new ExerciseReportDto
            {
                Id = e.Id,
                Title = e.Title,
                AvgScore = submissionMap.TryGetValue(e.Id, out var s) ? Math.Round(s.Avg, 1) : 0,
                SubmissionCount = s?.Count ?? 0
            }).ToList(),
            InactiveLearners = inactive
        });
    }

    public async Task<Result<CsvFileDto>> ExportReportCsvAsync(int userId, string role, int lessonId, CancellationToken ct)
    {
        var report = await GetReportAsync(userId, role, lessonId, ct);
        if (!report.IsSuccess)
        {
            return Result<CsvFileDto>.Fail(report.ErrorCode!, report.ErrorMessage!, report.FieldErrors!);
        }

        var data = report.Value!;
        var sb = new StringBuilder();
        sb.AppendLine("LessonId,LessonTitle");
        sb.AppendLine($"{data.LessonId},{Csv(data.LessonTitle)}");
        sb.AppendLine("TotalLearners,LearnersViewed,CompletionPct,AvgScore");
        sb.AppendLine($"{data.TotalLearners},{data.LearnersViewed},{data.CompletionPct.ToString(CultureInfo.InvariantCulture)},{data.AvgScore.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine("ExerciseId,ExerciseTitle,AvgScore,SubmissionCount");
        foreach (var exercise in data.Exercises)
        {
            sb.AppendLine($"{exercise.Id},{Csv(exercise.Title)},{exercise.AvgScore.ToString(CultureInfo.InvariantCulture)},{exercise.SubmissionCount}");
        }

        // UTF-8 BOM (API_REFERENCE.md §6.3)
        var content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return Result<CsvFileDto>.Ok(new CsvFileDto
        {
            FileName = $"report_lessons_{lessonId}_{clock.UtcNow:yyyyMMdd}.csv",
            Content = content
        });
    }

    // ── Private ───────────────────────────────────────────────

    private async Task<(int LessonsTotal, int ExercisesTotal, List<TopicAggregate> Topics)> LoadCountsAsync(CancellationToken ct)
    {
        var topics = await db.Topics.AsNoTracking()
            .Where(t => t.DeletedAt == null)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Id)
            .ToListAsync(ct);

        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null && l.Status == LessonStatus.Active)
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Id)
            .ToListAsync(ct);

        var lessonIds = lessons.Select(l => l.Id).ToList();
        var progress = await db.UserProgress.AsNoTracking()
            .Where(p => lessonIds.Contains(p.LessonId))
            .ToListAsync(ct);
        var progressByLesson = progress.ToDictionary(p => p.LessonId);

        var exercisesTotal = await db.Exercises.AsNoTracking()
            .CountAsync(e => e.DeletedAt == null && e.Status == ExerciseStatus.Active, ct);

        var aggregates = topics.Select(topic =>
        {
            var topicLessons = lessons.Where(l => l.TopicId == topic.Id).ToList();
            var viewedCount = topicLessons.Count(l =>
                progressByLesson.TryGetValue(l.Id, out var p) && p.Viewed);
            return new TopicAggregate
            {
                Id = topic.Id,
                Name = topic.Name,
                TotalLessons = topicLessons.Count,
                ViewedLessons = viewedCount,
                Lessons = topicLessons.Select(l =>
                {
                    progressByLesson.TryGetValue(l.Id, out var p);
                    return new LessonProgressItemDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Viewed = p?.Viewed ?? false,
                        BestScore = p?.BestScore,
                        Completed = p?.CompletedAt != null
                    };
                }).ToList()
            };
        }).ToList();

        return (lessons.Count, exercisesTotal, aggregates);
    }

    private static bool IsTeacherOrAdmin(string role) =>
        role.Equals(RoleTeacher, StringComparison.OrdinalIgnoreCase) ||
        role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase);

    private static bool IsTeacher(string role) =>
        role.Equals(RoleTeacher, StringComparison.OrdinalIgnoreCase);

    private static string Csv(string value) =>
        value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;

    private sealed class TopicAggregate
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int TotalLessons { get; init; }
        public int ViewedLessons { get; init; }
        public List<LessonProgressItemDto> Lessons { get; init; } = [];
    }
}
