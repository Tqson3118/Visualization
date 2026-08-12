using System.Text;
using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// ExerciseService thật theo SDD §5.4/§5.5 + API_REFERENCE.md §4.6.
/// Chấm điểm: SINGLE selected==answer[0]; MULTI tập hợp bằng; BOOLEAN selected[0]==answer[0];
/// LAB chấm trạng thái cuối + stepsUsed ≤ maxSteps×1.5 (quyết định G-5); Score = Σ Points.
/// Chống nộp trùng: SubmissionLockRegistry (per user+exercise) → SUBMISSION_IN_PROGRESS.
/// Câu hỏi LAB lưu AnswerJson dạng <c>{"type":"STATE_MATCH","finalState":[...],"maxSteps":n}</c>.
/// CSV: <c>content,type,options,points,answer,explanation</c> (options/answer là JSON array).
/// </summary>
public sealed class ExerciseService(
    AppDbContext db,
    IDateTimeProvider clock,
    SubmissionLockRegistry locks,
    ILogger<ExerciseService> logger) : IExerciseService
{
    private const string RoleTeacher = "TEACHER";
    private const string RoleAdmin = "ADMIN";

    // ── Danh sách / chi tiết ──────────────────────────────────

    public async Task<Result<PagedResponse<ExerciseSummaryDto>>> GetListAsync(
        int? lessonId, int? nodeId, int? stage, int page, int pageSize, CancellationToken ct)
    {
        var (safePage, safeSize) = Pagination.Normalize(page, pageSize);

        var query = db.Exercises.AsNoTracking().Where(e => e.DeletedAt == null);
        if (lessonId is > 0)
        {
            query = query.Where(e => e.LessonId == lessonId);
        }

        if (nodeId is > 0)
        {
            query = query.Where(e => e.NodeId == nodeId);
        }

        if (stage is > 0)
        {
            query = query.Where(e => e.Stage == stage);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.Id)
            .Skip((safePage - 1) * safeSize).Take(safeSize)
            .Select(e => new ExerciseSummaryDto
            {
                Id = e.Id,
                LessonId = e.LessonId,
                NodeId = e.NodeId,
                Stage = e.Stage,
                Title = e.Title,
                Description = e.Description,
                Type = e.Type.ToString(),
                DurationMinutes = e.DurationMinutes,
                MaxScore = e.MaxScore,
                Status = e.Status.ToString()
            })
            .ToListAsync(ct);

        return Result<PagedResponse<ExerciseSummaryDto>>.Ok(
            PagedResponse<ExerciseSummaryDto>.Create(items, safePage, safeSize, total, Pagination.TotalPages(total, safeSize)));
    }

    public async Task<Result<ExerciseDto>> GetByIdAsync(int userId, int id, CancellationToken ct)
    {
        var exercise = await db.Exercises.AsNoTracking()
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);

        if (exercise is null)
        {
            return Result<ExerciseDto>.Fail(ErrorCodes.NOT_FOUND, "Bài tập không tồn tại");
        }

        var dto = ToDto(exercise);

        // API_REFERENCE §3.7: bestScore của người gọi (Student)
        var best = await db.ExerciseSubmissions.AsNoTracking()
            .Where(s => s.UserId == userId && s.ExerciseId == id)
            .OrderByDescending(s => s.Score)
            .Select(s => (int?)s.Score)
            .FirstOrDefaultAsync(ct);
        dto.BestScore = best;

        return Result<ExerciseDto>.Ok(dto);
    }

    // ── CRUD ──────────────────────────────────────────────────

    public async Task<Result<ExerciseDto>> CreateAsync(int userId, ExerciseUpsertRequest request, CancellationToken ct)
    {
        var lesson = await db.Lessons.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.DeletedAt == null, ct);
        if (lesson is null)
        {
            return Result<ExerciseDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        if (request.NodeId is { } nodeId)
        {
            var node = await db.LearningPathNodes.AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == nodeId, ct);
            if (node is null)
            {
                return Result<ExerciseDto>.Fail(ErrorCodes.NOT_FOUND, "Node không tồn tại");
            }
        }

        var exercise = new Exercise
        {
            LessonId = request.LessonId,
            NodeId = request.NodeId,
            Stage = request.NodeId is null ? null : request.Stage,
            ConfigJson = request.ConfigJson,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Type = request.Type,
            DurationMinutes = request.DurationMinutes,
            MaxScore = request.MaxScore,
            Status = request.Status,
            CreatedBy = userId,
            CreatedAt = clock.UtcNow
        };

        foreach (var q in request.Questions)
        {
            exercise.Questions.Add(ToQuestion(q));
        }

        db.Exercises.Add(exercise);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Exercise {ExerciseId} created by user {UserId}", exercise.Id, userId);
        return Result<ExerciseDto>.Ok(ToDto(exercise));
    }

    public async Task<Result<ExerciseDto>> UpdateAsync(int userId, string role, int id, ExerciseUpsertRequest request, CancellationToken ct)
    {
        var exercise = await db.Exercises
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);
        if (exercise is null)
        {
            return Result<ExerciseDto>.Fail(ErrorCodes.NOT_FOUND, "Bài tập không tồn tại");
        }

        if (!CanManage(userId, role, exercise))
        {
            return Result<ExerciseDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền sửa bài tập này");
        }

        var lesson = await db.Lessons.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.LessonId && l.DeletedAt == null, ct);
        if (lesson is null)
        {
            return Result<ExerciseDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        exercise.LessonId = request.LessonId;
        exercise.NodeId = request.NodeId;
        exercise.Stage = request.NodeId is null ? null : request.Stage;
        exercise.ConfigJson = request.ConfigJson;
        exercise.Title = request.Title.Trim();
        exercise.Description = request.Description?.Trim();
        exercise.Type = request.Type;
        exercise.DurationMinutes = request.DurationMinutes;
        exercise.MaxScore = request.MaxScore;
        exercise.Status = request.Status;
        exercise.UpdatedAt = clock.UtcNow;

        // Thay thế toàn bộ câu hỏi
        db.Questions.RemoveRange(exercise.Questions);
        exercise.Questions.Clear();
        foreach (var q in request.Questions)
        {
            exercise.Questions.Add(ToQuestion(q));
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Exercise {ExerciseId} updated by user {UserId}", id, userId);
        return Result<ExerciseDto>.Ok(ToDto(exercise));
    }

    public async Task<Result> DeleteAsync(int userId, string role, int id, CancellationToken ct)
    {
        var exercise = await db.Exercises.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);
        if (exercise is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Bài tập không tồn tại");
        }

        if (!CanManage(userId, role, exercise))
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xóa bài tập này");
        }

        var tracked = await db.Exercises.FirstAsync(e => e.Id == id, ct);
        tracked.DeletedAt = clock.UtcNow;
        tracked.UpdatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Exercise {ExerciseId} soft-deleted by user {UserId}", id, userId);
        return Result.Ok();
    }

    // ── Nộp bài / chấm điểm ───────────────────────────────────

    public async Task<Result<SubmitResultDto>> SubmitAsync(int userId, int id, SubmitRequest request, CancellationToken ct)
    {
        var exercise = await db.Exercises.AsNoTracking()
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);
        if (exercise is null)
        {
            return Result<SubmitResultDto>.Fail(ErrorCodes.NOT_FOUND, "Bài tập không tồn tại");
        }

        if (exercise.Status != ExerciseStatus.Active)
        {
            return Result<SubmitResultDto>.Fail(ErrorCodes.EXERCISE_CLOSED, "Bài tập không còn nhận bài nộp");
        }

        using var submissionLock = locks.TryAcquire(userId, id);
        if (submissionLock is null)
        {
            return Result<SubmitResultDto>.Fail(ErrorCodes.SUBMISSION_IN_PROGRESS, "Đang có bài nộp đồng thời");
        }

        // Nộp qua luồng lớp (v2.8): validate thành viên hiện tại + lớp Mở (SDD §7.3.16)
        if (request.ClassAssignmentId is { } assignmentId)
        {
            var assignment = await db.ClassAssignments.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == assignmentId, ct);
            if (assignment is null || assignment.ExerciseId != id)
            {
                return Result<SubmitResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "Bài gán không hợp lệ", new() { ["classAssignmentId"] = ["Bài gán không hợp lệ"] });
            }

            var classRoom = await db.Classes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == assignment.ClassId && c.DeletedAt == null, ct);
            var isMember = await db.ClassMembers.AsNoTracking()
                .AnyAsync(m => m.ClassId == assignment.ClassId && m.UserId == userId, ct);
            if (classRoom is null || !isMember)
            {
                return Result<SubmitResultDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không thuộc lớp được gán bài tập này");
            }

            if (classRoom.Status != ClassStatus.Open)
            {
                return Result<SubmitResultDto>.Fail(ErrorCodes.FORBIDDEN, "Lớp đã đóng, không nhận bài nộp");
            }
        }

        // Ladder (FR-4.11): bài tập thuộc node — phải pass node trước
        if (exercise.NodeId is { } nodeId && !await IsPreviousNodePassedAsync(userId, nodeId, ct))
        {
            return Result<SubmitResultDto>.Fail(ErrorCodes.LADDER_LOCKED, "Chưa pass bậc trước — không mở bậc sau");
        }

        var questions = exercise.Questions.OrderBy(q => q.SortOrder).ToList();
        var answers = request.Answers.ToDictionary(a => a.QuestionId);

        // Validate đáp án khớp câu hỏi (QUESTION_ANSWER_MISMATCH)
        foreach (var question in questions)
        {
            if (!answers.ContainsKey(question.Id))
            {
                return Result<SubmitResultDto>.Fail(ErrorCodes.QUESTION_ANSWER_MISMATCH,
                    "Đáp án gửi lên không khớp câu hỏi", new() { ["questionId"] = [$"Thiếu đáp án câu {question.Id}"] });
            }
        }

        if (answers.Count != questions.Count)
        {
            return Result<SubmitResultDto>.Fail(ErrorCodes.QUESTION_ANSWER_MISMATCH,
                "Đáp án gửi lên không khớp câu hỏi");
        }

        var now = clock.UtcNow;
        var score = 0;
        var results = new List<QuestionResultDto>();

        foreach (var question in questions)
        {
            var answer = answers[question.Id];
            var (correct, correctAnswer, explanation) = Grade(question, answer);
            if (correct)
            {
                score += question.Points;
            }

            results.Add(new QuestionResultDto
            {
                QuestionId = question.Id,
                Correct = correct,
                CorrectAnswer = correctAnswer,
                Explanation = explanation ?? question.Explanation
            });
        }

        var resultJson = JsonSerializer.Serialize(results);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var submission = new ExerciseSubmission
        {
            UserId = userId,
            ExerciseId = id,
            ClassAssignmentId = request.ClassAssignmentId,
            Score = score,
            AnswersJson = JsonSerializer.Serialize(request.Answers),
            ResultJson = resultJson,
            DurationSeconds = request.DurationSeconds,
            SubmittedAt = now
        };
        db.ExerciseSubmissions.Add(submission);

        // Upsert UserProgress (BestScore = max) — SDD §7.3.4
        await UpsertUserProgressAsync(userId, exercise.LessonId, score, now, ct);

        // Cập nhật UserNodeProgress trong CÙNG transaction (SDD §7.3.30 — v2.9)
        if (exercise.NodeId is { } nodeId2)
        {
            await UpsertNodeProgressAsync(userId, nodeId2, score, exercise.Questions.Sum(q => q.Points), now, ct);
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation("Submission {SubmissionId} for exercise {ExerciseId} by user {UserId} score {Score}",
            submission.Id, id, userId, score);

        return Result<SubmitResultDto>.Ok(new SubmitResultDto
        {
            Score = score,
            MaxScore = exercise.Questions.Sum(q => q.Points),
            Passed = score == exercise.Questions.Sum(q => q.Points),
            Results = results,
            SubmissionId = submission.Id,
            SubmittedAt = now
        });
    }

    public async Task<Result<ExerciseDto>> PracticeAsync(int userId, int id, CancellationToken ct)
    {
        // Luyện tập không chấm điểm (FR-4.6): trả câu hỏi, KHÔNG tạo ExerciseSubmission
        var exercise = await db.Exercises.AsNoTracking()
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);

        if (exercise is null)
        {
            return Result<ExerciseDto>.Fail(ErrorCodes.NOT_FOUND, "Bài tập không tồn tại");
        }

        return Result<ExerciseDto>.Ok(ToDto(exercise));
    }

    // ── Import CSV ────────────────────────────────────────────

    public async Task<Result<ImportCsvResultDto>> ImportCsvAsync(int userId, int lessonId, string csvText, CancellationToken ct)
    {
        var lesson = await db.Lessons.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lessonId && l.DeletedAt == null, ct);
        if (lesson is null)
        {
            return Result<ImportCsvResultDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học không tồn tại");
        }

        var result = new ImportCsvResultDto();
        var questions = new List<Question>();
        var lines = csvText.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Bỏ header nếu có
        var start = lines.Length > 0 && lines[0].Contains("content", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

        for (var i = start; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var parts = SplitCsvLine(lines[i]);
            if (parts.Count < 5)
            {
                result.Errors.Add($"Dòng {lineNumber}: thiếu cột (cần: content,type,options,points,answer,explanation)");
                result.Skipped++;
                continue;
            }

            try
            {
                if (!Enum.TryParse<QuestionType>(parts[1].Trim(), true, out var type))
                {
                    result.Errors.Add($"Dòng {lineNumber}: type không hợp lệ '{parts[1]}' (SINGLE/MULTI/BOOLEAN/LAB)");
                    result.Skipped++;
                    continue;
                }

                if (!int.TryParse(parts[3].Trim(), out var points) || points is < 1 or > 10)
                {
                    result.Errors.Add($"Dòng {lineNumber}: points phải là số 1-10");
                    result.Skipped++;
                    continue;
                }

                questions.Add(new Question
                {
                    Type = type,
                    Content = parts[0].Trim(),
                    OptionsJson = parts[2].Trim(),
                    AnswerJson = parts[4].Trim(),
                    Explanation = parts.Count > 5 ? parts[5].Trim() : null,
                    Points = points,
                    SortOrder = questions.Count
                });
            }
            catch (Exception)
            {
                result.Errors.Add($"Dòng {lineNumber}: dữ liệu không hợp lệ");
                result.Skipped++;
            }
        }

        if (questions.Count == 0)
        {
            return Result<ImportCsvResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "CSV không có câu hỏi hợp lệ", new() { ["file"] = result.Errors.ToArray() });
        }

        var exercise = new Exercise
        {
            LessonId = lessonId,
            Title = $"Nhập từ CSV {clock.UtcNow:yyyyMMddHHmmss}",
            Type = ExerciseType.Mcq,
            MaxScore = questions.Sum(q => q.Points),
            Status = ExerciseStatus.Draft,
            CreatedBy = userId,
            CreatedAt = clock.UtcNow,
            Questions = questions
        };
        db.Exercises.Add(exercise);
        await db.SaveChangesAsync(ct);

        result.Created = questions.Count;
        logger.LogInformation("Exercise {ExerciseId} imported from CSV by user {UserId}", exercise.Id, userId);
        return Result<ImportCsvResultDto>.Ok(result);
    }

    // ── Lịch sử bài nộp ───────────────────────────────────────

    public async Task<Result<PagedResponse<SubmissionSummaryDto>>> GetSubmissionsAsync(
        int userId, string role, int id, int page, int pageSize, CancellationToken ct)
    {
        var exercise = await db.Exercises.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);
        if (exercise is null)
        {
            return Result<PagedResponse<SubmissionSummaryDto>>.Fail(ErrorCodes.NOT_FOUND, "Bài tập không tồn tại");
        }

        if (!CanManage(userId, role, exercise))
        {
            return Result<PagedResponse<SubmissionSummaryDto>>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem bài nộp của bài tập này");
        }

        return await QuerySubmissionsAsync(id, page, pageSize, includeUser: true, ct: ct);
    }

    public async Task<Result<PagedResponse<SubmissionSummaryDto>>> GetMySubmissionsAsync(
        int userId, int id, int page, int pageSize, CancellationToken ct)
    {
        return await QuerySubmissionsAsync(id, page, pageSize, includeUser: false, userId: userId, ct);
    }

    // ── Nộp code (FR-9.3, ADR-012 — chấm client) ──────────────

    public async Task<Result<CodeSubmitResultDto>> SubmitCodeAsync(int userId, int id, CodeSubmitRequest request, CancellationToken ct)
    {
        var exercise = await db.Exercises.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);
        if (exercise is null)
        {
            return Result<CodeSubmitResultDto>.Fail(ErrorCodes.NOT_FOUND, "Bài tập không tồn tại");
        }

        if (exercise.Type != ExerciseType.Code)
        {
            return Result<CodeSubmitResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Bài tập không phải loại CODE", new() { ["exerciseId"] = ["Bài tập không phải loại CODE"] });
        }

        var now = clock.UtcNow;
        var resultJson = JsonSerializer.Serialize(request.Results);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var submission = new CodeSubmission
        {
            UserId = userId,
            ExerciseId = id,
            Code = request.Code,
            Score = request.Score,
            PassedTests = request.Passed,
            TotalTests = request.Total,
            ResultJson = resultJson,
            SubmittedAt = now
        };
        db.CodeSubmissions.Add(submission);

        await UpsertUserProgressAsync(userId, exercise.LessonId, request.Score, now, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation("Code submission {SubmissionId} for exercise {ExerciseId} by user {UserId}",
            submission.Id, id, userId);

        return Result<CodeSubmitResultDto>.Ok(new CodeSubmitResultDto
        {
            Score = request.Score,
            Passed = request.Passed,
            Total = request.Total,
            Results = request.Results,
            SubmissionId = submission.Id,
            SubmittedAt = now
        });
    }

    public async Task<Result<PagedResponse<CodeSubmissionSummaryDto>>> GetCodeSubmissionsAsync(
        int userId, string role, int id, int page, int pageSize, CancellationToken ct)
    {
        var exercise = await db.Exercises.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);
        if (exercise is null)
        {
            return Result<PagedResponse<CodeSubmissionSummaryDto>>.Fail(ErrorCodes.NOT_FOUND, "Bài tập không tồn tại");
        }

        if (!CanManage(userId, role, exercise))
        {
            return Result<PagedResponse<CodeSubmissionSummaryDto>>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem bài nộp của bài tập này");
        }

        return await QueryCodeSubmissionsAsync(id, page, pageSize, includeUser: true, ct: ct);
    }

    public async Task<Result<PagedResponse<CodeSubmissionSummaryDto>>> GetMyCodeSubmissionsAsync(
        int userId, int id, int page, int pageSize, CancellationToken ct)
    {
        return await QueryCodeSubmissionsAsync(id, page, pageSize, includeUser: false, userId: userId, ct);
    }

    // ── Private ───────────────────────────────────────────────

    private static (bool Correct, JsonElement CorrectAnswer, string? Explanation) Grade(Question question, AnswerDto answer)
    {
        var answerElement = JsonSerializer.Deserialize<JsonElement>(question.AnswerJson);
        var correctAnswer = answerElement.Clone();

        return question.Type switch
        {
            QuestionType.Single => (
                answer.Selected.Count == 1 && answer.Selected[0] == GetAnswerIndex(answerElement, 0),
                correctAnswer,
                null),
            QuestionType.Multi => (
                IsSameSet(answer.Selected, GetAnswerIndices(answerElement)),
                correctAnswer,
                null),
            QuestionType.Boolean => (
                answer.Selected.Count == 1 && answer.Selected[0] == GetAnswerIndex(answerElement, 0),
                correctAnswer,
                null),
            QuestionType.Lab => GradeLab(answerElement, answer.LabAnswer, out var labExplanation),
            _ => (false, correctAnswer, null)
        };
    }

    private static (bool, JsonElement, string?) GradeLab(JsonElement answerElement, LabAnswerDto? labAnswer, out string? explanation)
    {
        explanation = null;
        if (labAnswer is null)
        {
            return (false, answerElement, "Thiếu labAnswer");
        }

        // Chuẩn từ StepExecutor: { "type":"STATE_MATCH", "finalState":[...], "maxSteps": n }
        var expectedState = answerElement.TryGetProperty("finalState", out var finalState)
            ? finalState
            : answerElement;

        var actualState = labAnswer.FinalState;
        var stateMatch = JsonElementEquals(expectedState, actualState);
        var maxSteps = labAnswer.MaxSteps > 0
            ? labAnswer.MaxSteps
            : (answerElement.TryGetProperty("maxSteps", out var ms) && ms.TryGetInt32(out var m) ? m : int.MaxValue);

        // Quyết định G-5: số bước ≤ chuẩn × 1.5 (API_REFERENCE.md §4.16)
        var stepLimit = (int)Math.Ceiling(maxSteps * 1.5);
        var stepsOk = labAnswer.StepsUsed <= stepLimit;

        explanation = stateMatch && stepsOk
            ? $"Trạng thái cuối khớp chuẩn; {labAnswer.StepsUsed} bước ≤ giới hạn {stepLimit} (chuẩn {maxSteps} × 1.5)"
            : stateMatch
                ? $"Trạng thái cuối khớp nhưng {labAnswer.StepsUsed} bước > giới hạn {stepLimit} (chuẩn {maxSteps} × 1.5)"
                : "Trạng thái cuối không khớp chuẩn StepExecutor";

        return (stateMatch && stepsOk, answerElement, explanation);
    }

    private static int GetAnswerIndex(JsonElement element, int fallback)
    {
        if (element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0 && element[0].TryGetInt32(out var value))
        {
            return value;
        }

        return fallback;
    }

    private static List<int> GetAnswerIndices(JsonElement element)
    {
        var result = new List<int>();
        if (element.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var item in element.EnumerateArray())
        {
            if (item.TryGetInt32(out var value))
            {
                result.Add(value);
            }
        }

        return result;
    }

    private static bool IsSameSet(List<int> selected, List<int> expected) =>
        selected.Count == expected.Count && selected.OrderBy(x => x).SequenceEqual(expected.OrderBy(x => x));

    private static bool JsonElementEquals(JsonElement a, JsonElement b) =>
        JsonElement.DeepEquals(a, b);

    private async Task<bool> IsPreviousNodePassedAsync(int userId, int nodeId, CancellationToken ct)
    {
        var node = await db.LearningPathNodes.AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == nodeId, ct);
        if (node is null)
        {
            return false;
        }

        // Node đầu tiên của path → luôn mở
        var isFirst = !await db.LearningPathNodes.AsNoTracking()
            .AnyAsync(n => n.PathId == node.PathId && n.SortOrder < node.SortOrder, ct);
        if (isFirst)
        {
            return true;
        }

        var previous = await db.LearningPathNodes.AsNoTracking()
            .Where(n => n.PathId == node.PathId && n.SortOrder < node.SortOrder)
            .OrderByDescending(n => n.SortOrder)
            .FirstOrDefaultAsync(ct);
        if (previous is null)
        {
            return true;
        }

        return await db.UserNodeProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.NodeId == previous.Id && p.Status == 2, ct);   // 2 = Passed
    }

    private async Task UpsertUserProgressAsync(int userId, int lessonId, int score, DateTime now, CancellationToken ct)
    {
        var progress = await db.UserProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);

        if (progress is null)
        {
            db.UserProgress.Add(new UserProgress
            {
                UserId = userId,
                LessonId = lessonId,
                Viewed = true,
                BestScore = score,
                CompletedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            progress.Viewed = true;
            if (score > (progress.BestScore ?? 0))
            {
                progress.BestScore = score;
            }

            progress.CompletedAt ??= now;
            progress.UpdatedAt = now;
        }
    }

    private async Task UpsertNodeProgressAsync(int userId, int nodeId, int score, int maxScore, DateTime now, CancellationToken ct)
    {
        var progress = await db.UserNodeProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.NodeId == nodeId, ct);

        var stars = maxScore > 0 ? (int)Math.Ceiling(score * 3.0 / maxScore) : 0;
        stars = Math.Clamp(stars, 0, 3);
        var passed = score >= maxScore;

        if (progress is null)
        {
            db.UserNodeProgress.Add(new UserNodeProgress
            {
                UserId = userId,
                NodeId = nodeId,
                Status = passed ? 2 : 1,
                Stars = stars,
                NodeScore = score,
                UnlockedAt = now,
                PassedAt = passed ? now : null,
                UpdatedAt = now
            });
        }
        else
        {
            progress.Status = passed || progress.Status == 2 ? 2 : 1;
            if (stars > progress.Stars)
            {
                progress.Stars = stars;
            }

            if (score > progress.NodeScore)
            {
                progress.NodeScore = score;
            }

            progress.PassedAt ??= passed ? now : null;
            progress.UpdatedAt = now;
        }
    }

    private async Task<Result<PagedResponse<SubmissionSummaryDto>>> QuerySubmissionsAsync(
        int exerciseId, int page, int pageSize, bool includeUser, int? userId = null, CancellationToken ct = default)
    {
        var (safePage, safeSize) = Pagination.Normalize(page, pageSize);

        var query = db.ExerciseSubmissions.AsNoTracking().Where(s => s.ExerciseId == exerciseId);
        if (userId is not null)
        {
            query = query.Where(s => s.UserId == userId);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.SubmittedAt)
            .Skip((safePage - 1) * safeSize).Take(safeSize)
            .Select(s => new SubmissionSummaryDto
            {
                Id = s.Id,
                UserId = s.UserId,
                Score = s.Score,
                DurationSeconds = s.DurationSeconds,
                ClassAssignmentId = s.ClassAssignmentId,
                SubmittedAt = s.SubmittedAt
            })
            .ToListAsync(ct);

        if (includeUser)
        {
            var userIds = items.Select(i => i.UserId).Distinct().ToList();
            var users = await db.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);
            foreach (var item in items)
            {
                item.UserDisplayName = users.GetValueOrDefault(item.UserId);
            }
        }

        return Result<PagedResponse<SubmissionSummaryDto>>.Ok(
            PagedResponse<SubmissionSummaryDto>.Create(items, safePage, safeSize, total, Pagination.TotalPages(total, safeSize)));
    }

    private async Task<Result<PagedResponse<CodeSubmissionSummaryDto>>> QueryCodeSubmissionsAsync(
        int exerciseId, int page, int pageSize, bool includeUser, int? userId = null, CancellationToken ct = default)
    {
        var (safePage, safeSize) = Pagination.Normalize(page, pageSize);

        var query = db.CodeSubmissions.AsNoTracking().Where(s => s.ExerciseId == exerciseId);
        if (userId is not null)
        {
            query = query.Where(s => s.UserId == userId);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.SubmittedAt)
            .Skip((safePage - 1) * safeSize).Take(safeSize)
            .Select(s => new CodeSubmissionSummaryDto
            {
                Id = s.Id,
                UserId = s.UserId,
                Score = s.Score,
                PassedTests = s.PassedTests,
                TotalTests = s.TotalTests,
                SubmittedAt = s.SubmittedAt
            })
            .ToListAsync(ct);

        if (includeUser)
        {
            var userIds = items.Select(i => i.UserId).Distinct().ToList();
            var users = await db.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);
            foreach (var item in items)
            {
                item.UserDisplayName = users.GetValueOrDefault(item.UserId);
            }
        }

        return Result<PagedResponse<CodeSubmissionSummaryDto>>.Ok(
            PagedResponse<CodeSubmissionSummaryDto>.Create(items, safePage, safeSize, total, Pagination.TotalPages(total, safeSize)));
    }

    private static Question ToQuestion(QuestionUpsertDto dto) => new()
    {
        Type = dto.Type,
        Content = dto.Content,
        OptionsJson = JsonSerializer.Serialize(dto.Options),
        AnswerJson = string.IsNullOrWhiteSpace(dto.AnswerJson) ? "[]" : dto.AnswerJson,
        Explanation = dto.Explanation,
        Hint1 = dto.Hint1,
        Hint2 = dto.Hint2,
        Hint3 = dto.Hint3,
        KeepOrder = dto.KeepOrder,
        Points = Math.Clamp(dto.Points, 1, 10),
        SortOrder = dto.SortOrder
    };

    private static ExerciseDto ToDto(Exercise exercise) => new()
    {
        Id = exercise.Id,
        LessonId = exercise.LessonId,
        NodeId = exercise.NodeId,
        Stage = exercise.Stage,
        Title = exercise.Title,
        Description = exercise.Description,
        Type = exercise.Type.ToString(),
        DurationMinutes = exercise.DurationMinutes,
        MaxScore = exercise.MaxScore,
        Status = exercise.Status.ToString(),
        Questions = exercise.Questions
            .OrderBy(q => q.SortOrder)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                Content = q.Content,
                Type = q.Type.ToString().ToUpperInvariant(),
                Options = DeserializeOptions(q.OptionsJson),
                Points = q.Points
            })
            .ToList()
    };

    private static List<string> DeserializeOptions(string optionsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(optionsJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static bool CanManage(int userId, string role, Exercise exercise) =>
        role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase) || exercise.CreatedBy == userId;

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (ch == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        result.Add(current.ToString());
        return result;
    }
}
