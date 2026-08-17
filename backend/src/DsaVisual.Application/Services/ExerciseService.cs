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

    /// <summary>Chấm code bài ASM phía máy chủ (Jint sandbox) — bài chỉ PASS khi code chạy ĐÚNG trên server.</summary>
    private readonly CodelabJudgeService _judge = new();

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

        await PopulateCompletedByUserCountAsync(items, ct);

        return Result<PagedResponse<ExerciseSummaryDto>>.Ok(
            PagedResponse<ExerciseSummaryDto>.Create(items, safePage, safeSize, total, Pagination.TotalPages(total, safeSize)));
    }

    /// <summary>
    /// Cách A (pre-load 2 query): đếm số user distinct đã PASS từng bài trong <paramref name="items"/>.
    /// Pass = best score ≥ MaxScore. Bài MCQ/SIMULATION_LAB nộp qua ExerciseSubmissions, bài CODE qua
    /// CodeSubmissions; mỗi exercise chỉ có 1 type → gộp 2 nguồn không double-count. GroupBy (ExerciseId,
    /// UserId) → 1 user nộp n lần (dù có lần đạt) chỉ tính 1. Dùng ≥ thay vì == (SubmitAsync dùng
    /// score == maxScore; giả định server clamp Score ≤ MaxScore nên 2 cách tương đương, ≥ an toàn hơn).
    /// </summary>
    private async Task PopulateCompletedByUserCountAsync(List<ExerciseSummaryDto> items, CancellationToken ct)
    {
        if (items.Count == 0)
        {
            return;
        }

        var ids = items.Select(e => e.Id).ToList();
        var maxScoreById = items.ToDictionary(e => e.Id, e => e.MaxScore);

        var mcqBest = await db.ExerciseSubmissions.AsNoTracking()
            .Where(s => ids.Contains(s.ExerciseId))
            .GroupBy(s => new { s.ExerciseId, s.UserId })
            .Select(g => new { g.Key.ExerciseId, g.Key.UserId, Best = g.Max(s => s.Score) })
            .ToListAsync(ct);

        var codeBest = await db.CodeSubmissions.AsNoTracking()
            .Where(s => ids.Contains(s.ExerciseId))
            .GroupBy(s => new { s.ExerciseId, s.UserId })
            .Select(g => new { g.Key.ExerciseId, g.Key.UserId, Best = g.Max(s => s.Score) })
            .ToListAsync(ct);

        var passedByExercise = new Dictionary<int, int>();
        foreach (var row in mcqBest.Concat(codeBest))
        {
            if (row.Best >= maxScoreById[row.ExerciseId])
            {
                passedByExercise[row.ExerciseId] = passedByExercise.GetValueOrDefault(row.ExerciseId) + 1;
            }
        }

        foreach (var item in items)
        {
            item.CompletedByUserCount = passedByExercise.GetValueOrDefault(item.Id);
        }
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

        // findings-security #11: ownership lesson — chỉ chủ bài học (hoặc ADMIN) tạo bài tập trong bài học
        if (lesson.CreatedBy != userId && !await IsAdminUserAsync(userId, ct))
        {
            return Result<ExerciseDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền tạo bài tập trong bài học này");
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

        // findings-security #11: không cho đổi LessonId sang bài học không sở hữu (trừ ADMIN)
        if (!CanManageLesson(userId, role, lesson))
        {
            return Result<ExerciseDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền tạo bài tập trong bài học này");
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

        using var submissionLock = locks.TryAcquire(userId, id, SubmissionLockWait);
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

            // v2.15 (Vấn đề 14): AllowLateSubmission == false → chặn nộp sau deadline (ASSIGNMENT_OVERDUE)
            if (!assignment.AllowLateSubmission && assignment.DueAt is { } dueAt && clock.UtcNow > dueAt)
            {
                return Result<SubmitResultDto>.Fail(ErrorCodes.ASSIGNMENT_OVERDUE,
                    "Đã quá hạn nộp bài tập này", new() { ["classAssignmentId"] = ["Đã quá hạn nộp bài tập này"] });
            }
        }

        // Ladder (FR-4.11): bài tập thuộc node — phải pass node trước
        if (exercise.NodeId is { } nodeId && !await IsPreviousNodePassedAsync(userId, nodeId, ct))
        {
            return Result<SubmitResultDto>.Fail(ErrorCodes.LADDER_LOCKED, "Chưa pass bậc trước — không mở bậc sau");
        }

        var questions = exercise.Questions.OrderBy(q => q.SortOrder).ToList();

        // Duplicate QuestionId → 400 thay vì 500 (ToDictionary ném ArgumentException — F5-Minor)
        if (request.Answers.Select(a => a.QuestionId).Distinct().Count() != request.Answers.Count)
        {
            return Result<SubmitResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Đáp án gửi lên chứa câu hỏi trùng lặp", new() { ["questionId"] = ["Không được gửi trùng đáp án cho cùng câu hỏi"] });
        }

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
        var maxScore = exercise.Questions.Sum(q => q.Points);
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

        // Fix Đợt D (review Major #1): idempotency key OPTIONAL — pre-check CHỈ khi ClientRequestId != null
        // (retry cùng key → trả submission cũ, idempotent). ClientRequestId == null → MỌI lần nộp là lần mới
        // (re-attempt cải thiện điểm hợp lệ FR-4.4/FR-9.5); double-submit single-instance do
        // SubmissionLockRegistry chống (multi-instance không key: ghi chú NFR-12).
        if (!string.IsNullOrWhiteSpace(request.ClientRequestId))
        {
            var idempotent = await db.ExerciseSubmissions.AsNoTracking()
                .Where(s => s.UserId == userId && s.ExerciseId == id
                    && s.ClassAssignmentId == request.ClassAssignmentId
                    && s.ClientRequestId == request.ClientRequestId)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync(ct);
            if (idempotent is not null)
            {
                logger.LogInformation("Idempotent submit (clientRequestId {ClientRequestId}, user {UserId}, exercise {ExerciseId}) — returning submission {SubmissionId}",
                    request.ClientRequestId, userId, id, idempotent.Id);
                return Result<SubmitResultDto>.Ok(new SubmitResultDto
                {
                    Score = idempotent.Score,
                    MaxScore = maxScore,
                    Passed = idempotent.Score == maxScore,
                    Results = DeserializeResults(idempotent.ResultJson, results),
                    SubmissionId = idempotent.Id,
                    SubmittedAt = idempotent.SubmittedAt
                });
            }
        }

        var submission = new ExerciseSubmission
        {
            UserId = userId,
            ExerciseId = id,
            ClassAssignmentId = request.ClassAssignmentId,
            ClientRequestId = request.ClientRequestId,
            Score = score,
            AnswersJson = JsonSerializer.Serialize(request.Answers),
            ResultJson = resultJson,
            DurationSeconds = request.DurationSeconds,
            SubmittedAt = now
        };
        db.ExerciseSubmissions.Add(submission);

        ExerciseSubmission stored;
        try
        {
            await db.SaveChangesAsync(ct);
            stored = submission;
        }
        catch (DbUpdateException ex) when (request.ClientRequestId is not null && IsUniqueViolation(ex))
        {
            // Chỉ có thể unique violation khi có ClientRequestId (index filtered IS NOT NULL — không có key
            // thì không unique → mọi lỗi khác lan ra như bình thường). Multi-instance race CÙNG key:
            // instance khác vừa commit — rollback + dùng bản ghi đó (idempotent).
            await tx.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            var winner = await db.ExerciseSubmissions.AsNoTracking()
                .Where(s => s.UserId == userId && s.ExerciseId == id
                    && s.ClassAssignmentId == request.ClassAssignmentId
                    && s.ClientRequestId == request.ClientRequestId)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync(ct);
            if (winner is null)
            {
                return Result<SubmitResultDto>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
            }

            logger.LogWarning("Unique violation on idempotent submit (clientRequestId {ClientRequestId}, user {UserId}, exercise {ExerciseId}) — reusing submission {SubmissionId}",
                request.ClientRequestId, userId, id, winner.Id);
            return await MergeDuplicateProgressAndReturnAsync(userId, exercise, score, now, winner, resultJson, results, ct);
        }

        // Upsert UserProgress (BestScore = max) — SDD §7.3.4
        await UpsertUserProgressAsync(userId, exercise.LessonId, score, now, ct);

        // Cập nhật UserNodeProgress trong CÙNG transaction (SDD §7.3.30 — v2.9)
        var nodeJustPassed = false;
        if (exercise.NodeId is { } nodeId2)
        {
            // Finding #6: chỉ tính quest pass_node khi node VỪA chuyển sang passed (Status 1→2) —
            // nộp lại bài của node đã pass không tăng thêm (anti double-count)
            nodeJustPassed = !await db.UserNodeProgress.AsNoTracking()
                .AnyAsync(p => p.UserId == userId && p.NodeId == nodeId2 && p.Status == 2, ct);
            await UpsertNodeProgressAsync(userId, nodeId2, score, exercise.Questions.Sum(q => q.Points), now, ct);
        }

        // findings-biz #7: race RowVersion (hoặc unique) trên UserProgress/UserNodeProgress —
        // reload dữ liệu mới + merge (Status pass giữ 2, Stars/NodeScore lấy max) rồi lưu lại,
        // KHÔNG rollback submission (pass không được mất vì lỗi concurrency)
        if (!await SaveProgressWithRetryAsync(userId, exercise.LessonId, exercise.NodeId, score, maxScore, now, ct))
        {
            await tx.RollbackAsync(ct);
            return Result<SubmitResultDto>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

        // Finding #6 (FR-10.3): tăng Progress quest CÙNG transaction với hành động học (raw SQL trong ambient tx)
        var passed = score == maxScore;
        if (exercise.Type == ExerciseType.Mcq)
        {
            await QuestProgressWriter.IncrementAsync(db, userId, "pass_quiz", ct);
        }
        else if (exercise.Type == ExerciseType.SimulationLab)
        {
            await QuestProgressWriter.IncrementAsync(db, userId, "pass_lab", ct);
        }

        if (passed && nodeJustPassed && exercise.NodeId is { })
        {
            await QuestProgressWriter.IncrementAsync(db, userId, "pass_node", ct);
        }

        await tx.CommitAsync(ct);

        logger.LogInformation("Submission {SubmissionId} for exercise {ExerciseId} by user {UserId} score {Score}",
            stored.Id, id, userId, stored.Score);

        return Result<SubmitResultDto>.Ok(new SubmitResultDto
        {
            Score = stored.Score,
            MaxScore = maxScore,
            Passed = stored.Score == maxScore,
            Results = DeserializeResults(stored.ResultJson, results),
            SubmissionId = stored.Id,
            SubmittedAt = stored.SubmittedAt
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

        // findings-security #11: ownership lesson — chỉ chủ bài học (hoặc ADMIN) import vào bài học
        if (lesson.CreatedBy != userId && !await IsAdminUserAsync(userId, ct))
        {
            return Result<ImportCsvResultDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền tạo bài tập trong bài học này");
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
        int userId, string role, int id, int page, int pageSize, CancellationToken ct = default,
        DateTime? lastSubmittedAt = null, int? lastId = null)
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

        return await QuerySubmissionsAsync(id, page, pageSize, includeUser: true, ct: ct, lastSubmittedAt: lastSubmittedAt, lastId: lastId);
    }

    public async Task<Result<PagedResponse<SubmissionSummaryDto>>> GetMySubmissionsAsync(
        int userId, int id, int page, int pageSize, CancellationToken ct = default,
        DateTime? lastSubmittedAt = null, int? lastId = null)
    {
        return await QuerySubmissionsAsync(id, page, pageSize, includeUser: false, userId: userId, ct,
            lastSubmittedAt: lastSubmittedAt, lastId: lastId);
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

        // Chỉ nhận nộp khi bài tập còn Active (F5-Minor — đồng bộ với SubmitAsync)
        if (exercise.Status != ExerciseStatus.Active)
        {
            return Result<CodeSubmitResultDto>.Fail(ErrorCodes.EXERCISE_CLOSED, "Bài tập không còn nhận bài nộp");
        }

        // Chống nộp trùng per (user, exercise) — SUBMISSION_IN_PROGRESS (F5-Minor — đồng bộ với SubmitAsync).
        // Chờ có giới hạn: nếu request trước đang chạy, chờ nó commit rồi vào nhánh idempotent (merge)
        // thay vì trả 422 ngay — cần cho race PASS+FAIL song song (findings-biz #7).
        using var submissionLock = locks.TryAcquire(userId, id, SubmissionLockWait);
        if (submissionLock is null)
        {
            return Result<CodeSubmitResultDto>.Fail(ErrorCodes.SUBMISSION_IN_PROGRESS, "Đang có bài nộp đồng thời");
        }

        // findings-biz #6: nộp qua luồng lớp — validate assignment/membership/class Open (đồng bộ SubmitAsync)
        if (request.ClassAssignmentId is { } assignmentId)
        {
            var assignment = await db.ClassAssignments.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == assignmentId, ct);
            if (assignment is null || assignment.ExerciseId != id)
            {
                return Result<CodeSubmitResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "Bài gán không hợp lệ", new() { ["classAssignmentId"] = ["Bài gán không hợp lệ"] });
            }

            var classRoom = await db.Classes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == assignment.ClassId && c.DeletedAt == null, ct);
            var isMember = await db.ClassMembers.AsNoTracking()
                .AnyAsync(m => m.ClassId == assignment.ClassId && m.UserId == userId, ct);
            if (classRoom is null || !isMember)
            {
                return Result<CodeSubmitResultDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không thuộc lớp được gán bài tập này");
            }

            if (classRoom.Status != ClassStatus.Open)
            {
                return Result<CodeSubmitResultDto>.Fail(ErrorCodes.FORBIDDEN, "Lớp đã đóng, không nhận bài nộp");
            }

            // v2.15 (Vấn đề 14): AllowLateSubmission == false → chặn nộp sau deadline (ASSIGNMENT_OVERDUE)
            if (!assignment.AllowLateSubmission && assignment.DueAt is { } dueAt && clock.UtcNow > dueAt)
            {
                return Result<CodeSubmitResultDto>.Fail(ErrorCodes.ASSIGNMENT_OVERDUE,
                    "Đã quá hạn nộp bài tập này", new() { ["classAssignmentId"] = ["Đã quá hạn nộp bài tập này"] });
            }
        }

        // findings-biz #6: ladder — bài code thuộc node phải pass node trước (đồng bộ SubmitAsync)
        if (exercise.NodeId is { } ladderNodeId && !await IsPreviousNodePassedAsync(userId, ladderNodeId, ct))
        {
            return Result<CodeSubmitResultDto>.Fail(ErrorCodes.LADDER_LOCKED, "Chưa pass bậc trước — không mở bậc sau");
        }

        var now = clock.UtcNow;

        // ── Chấm code PHÍA MÁY CHỦ (nghiệp vụ 15/08): bài ASM/kiểm tra cuối có ConfigJson dạng
        // array tasks → server tự chạy code học viên bằng Jint (sandbox: timeout/statements/memory/
        // stack) so với testcase của task; KHÔNG tin Score/Passed/Total client khai.
        var tasks = CodelabJudgeService.TryParseTasks(exercise.ConfigJson);
        var serverJudged = false;
        var judgedTaskEntry = (string?)null;
        var judgeCases = new List<CodelabCaseResult>();
        var judgeError = (string?)null;
        if (tasks is not null)
        {
            if (string.IsNullOrWhiteSpace(request.TaskId))
            {
                return Result<CodeSubmitResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "Bài tập này yêu cầu chấm code phía máy chủ — thiếu TaskId của bài con");
            }

            var task = tasks.FirstOrDefault(t => t.Id == request.TaskId)
                       ?? tasks.FirstOrDefault(t => t.EntryFunction == request.TaskId);
            if (task is null)
            {
                return Result<CodeSubmitResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "TaskId không tồn tại trong đề bài");
            }

            var judged = _judge.Judge(request.Code, task);
            serverJudged = true;
            judgedTaskEntry = task.EntryFunction;
            judgeCases = [.. judged.Cases];
            judgeError = judged.CompileError ? judged.CompileErrorText : judged.TimedOut || !string.IsNullOrWhiteSpace(judged.TimeoutError) ? judged.TimeoutError : null;
        }

        // findings-security #1: clamp Score vào [0, MaxScore] — không tin điểm client mù;
        // nếu bài không có MaxScore → clamp theo Total client khai.
        // Khi serverJudged: Score = số testcase pass do MÁY CHỦ chấm.
        var maxScore = exercise.MaxScore > 0 ? exercise.MaxScore : Math.Max(request.Total, 0);
        var passedTests = serverJudged ? judgeCases.Count(c => c.Passed) : request.Passed;
        var totalTests = serverJudged ? judgeCases.Count : request.Total;
        var score = serverJudged ? passedTests : Math.Clamp(request.Score, 0, maxScore);
        var resultJson = serverJudged
            ? JsonSerializer.Serialize(new
            {
                taskId = request.TaskId,
                entryFunction = judgedTaskEntry,
                judged = true,
                error = judgeError,
                results = judgeCases.Select(c => new { passed = c.Passed, error = c.Error }).ToList()
            })
            : JsonSerializer.Serialize(request.Results);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Fix Đợt D (review Major #1): idempotency key OPTIONAL — pre-check CHỈ khi ClientRequestId != null
        // (retry cùng key → trả submission cũ, idempotent). ClientRequestId == null → MỌI lần nộp là lần mới
        // (lịch sử + so sánh 2 lần nộp hợp lệ FR-9.5); double-submit single-instance do SubmissionLockRegistry
        // chống (multi-instance không key: ghi chú NFR-12).
        if (!string.IsNullOrWhiteSpace(request.ClientRequestId))
        {
            var idempotent = await db.CodeSubmissions.AsNoTracking()
                .Where(s => s.UserId == userId && s.ExerciseId == id && s.ClientRequestId == request.ClientRequestId)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync(ct);
            if (idempotent is not null)
            {
                logger.LogInformation("Idempotent code submit (clientRequestId {ClientRequestId}, user {UserId}, exercise {ExerciseId}) — returning submission {SubmissionId}",
                    request.ClientRequestId, userId, id, idempotent.Id);
                return Result<CodeSubmitResultDto>.Ok(new CodeSubmitResultDto
                {
                    Score = idempotent.Score,
                    Passed = idempotent.PassedTests,
                    Total = idempotent.TotalTests,
                    Results = DeserializeCodeResults(idempotent.ResultJson, request.Results),
                    SubmissionId = idempotent.Id,
                    SubmittedAt = idempotent.SubmittedAt
                });
            }
        }

        var submission = new CodeSubmission
        {
            UserId = userId,
            ExerciseId = id,
            ClientRequestId = request.ClientRequestId,
            Code = request.Code,
            Score = score,
            PassedTests = passedTests,
            TotalTests = totalTests,
            ResultJson = resultJson,
            IsClientDeclared = !serverJudged,
            SubmittedAt = now
        };
        db.CodeSubmissions.Add(submission);

        CodeSubmission stored;
        try
        {
            await db.SaveChangesAsync(ct);
            stored = submission;
        }
        catch (DbUpdateException ex) when (request.ClientRequestId is not null && IsUniqueViolation(ex))
        {
            // Chỉ có thể unique violation khi có ClientRequestId (index filtered IS NOT NULL — không có key
            // thì không unique → mọi lỗi khác lan ra như bình thường). Multi-instance race CÙNG key:
            // instance khác vừa commit — rollback + dùng bản ghi đó (idempotent).
            await tx.RollbackAsync(ct);
            db.ChangeTracker.Clear();
            var winner = await db.CodeSubmissions.AsNoTracking()
                .Where(s => s.UserId == userId && s.ExerciseId == id && s.ClientRequestId == request.ClientRequestId)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync(ct);
            if (winner is null)
            {
                return Result<CodeSubmitResultDto>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
            }

            logger.LogWarning("Unique violation on idempotent code submit (clientRequestId {ClientRequestId}, user {UserId}, exercise {ExerciseId}) — reusing submission {SubmissionId}",
                request.ClientRequestId, userId, id, winner.Id);
            return await MergeCodeDuplicateProgressAndReturnAsync(userId, exercise, score, maxScore, now, winner, resultJson, request, ct);
        }

        // findings-biz #5: đồng bộ UserProgress + UserNodeProgress (exercise có NodeId) trong CÙNG transaction
        var nodeJustPassed = false;
        if (exercise.NodeId is { } nodeId)
        {
            nodeJustPassed = !await db.UserNodeProgress.AsNoTracking()
                .AnyAsync(p => p.UserId == userId && p.NodeId == nodeId && p.Status == 2, ct);
        }

        // Nghiệp vụ ASM: node pass khi MÁY CHỦ xác nhận TẤT CẢ task con đã có bài nộp full-pass.
        var allTasksPassed = false;
        if (serverJudged && tasks is not null)
        {
            var pastSubs = await db.CodeSubmissions.AsNoTracking()
                .Where(s => s.UserId == userId && s.ExerciseId == id && s.PassedTests > 0)
                .ToListAsync(ct);
            var coveredEntries = new HashSet<string>(StringComparer.Ordinal);
            foreach (var s in pastSubs)
            {
                // entryFunction bền vững hơn id (seed tạo id mới mỗi lần chạy)
                var entry = TryReadJudgedEntry(s.ResultJson);
                if (entry is not null && s.PassedTests >= s.TotalTests)
                {
                    coveredEntries.Add(entry);
                }
            }

            allTasksPassed = tasks.All(t => coveredEntries.Contains(t.EntryFunction));
        }

        var nodePassScore = serverJudged ? (allTasksPassed ? maxScore : score) : score;
        await UpsertUserProgressAsync(userId, exercise.LessonId, score, now, ct);
        if (exercise.NodeId is { } nid)
        {
            await UpsertNodeProgressAsync(userId, nid, nodePassScore, maxScore, now, ct);
        }

        // findings-biz #7: retry khi race RowVersion/unique trên progress — pass không được mất
        if (!await SaveProgressWithRetryAsync(userId, exercise.LessonId, exercise.NodeId, nodePassScore, maxScore, now, ct))
        {
            await tx.RollbackAsync(ct);
            return Result<CodeSubmitResultDto>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

        // Finding #6 (FR-10.3): chạy code thành công → tăng quest code_run (cùng transaction)
        await QuestProgressWriter.IncrementAsync(db, userId, "code_run", ct);

        // findings-biz #5: node VỪA pass nhờ code-submit → quest pass_node (anti double-count)
        var passed = serverJudged ? allTasksPassed : score >= maxScore;
        if (passed && nodeJustPassed && exercise.NodeId is { })
        {
            await QuestProgressWriter.IncrementAsync(db, userId, "pass_node", ct);
        }

        await tx.CommitAsync(ct);

        logger.LogInformation("Code submission {SubmissionId} for exercise {ExerciseId} by user {UserId} (serverJudged={ServerJudged}, {Passed}/{Total} passed)",
            stored.Id, id, userId, serverJudged, passedTests, totalTests);

        return Result<CodeSubmitResultDto>.Ok(new CodeSubmitResultDto
        {
            Score = stored.Score,
            Passed = stored.PassedTests,
            Total = stored.TotalTests,
            Results = BuildCodeSubmitResults(serverJudged, judgeCases, request),
            SubmissionId = stored.Id,
            SubmittedAt = stored.SubmittedAt,
            Error = judgeError
        });
    }

    public async Task<Result<PagedResponse<CodeSubmissionSummaryDto>>> GetCodeSubmissionsAsync(
        int userId, string role, int id, int page, int pageSize, CancellationToken ct = default,
        DateTime? lastSubmittedAt = null, int? lastId = null)
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

        return await QueryCodeSubmissionsAsync(id, page, pageSize, includeUser: true, ct: ct, lastSubmittedAt: lastSubmittedAt, lastId: lastId);
    }

    public async Task<Result<PagedResponse<CodeSubmissionSummaryDto>>> GetMyCodeSubmissionsAsync(
        int userId, int id, int page, int pageSize, CancellationToken ct = default,
        DateTime? lastSubmittedAt = null, int? lastId = null)
    {
        return await QueryCodeSubmissionsAsync(id, page, pageSize, includeUser: false, userId: userId, ct,
            lastSubmittedAt: lastSubmittedAt, lastId: lastId);
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

    // ── findings-biz #1/#7 + findings-security #1/#11 — helpers ──

    private const int MaxSaveAttempts = 3;

    /// <summary>
    /// Chờ tối đa cho lock nộp bài của request trước commit (rồi đi vào nhánh idempotent/merge).
    /// Ngắn đủ để race test không trễ, dài đủ để request trước (~100-300ms) kịp commit.
    /// </summary>
    private static readonly TimeSpan SubmissionLockWait = TimeSpan.FromSeconds(2);

    /// <summary>Phát hiện unique constraint violation (SQL Server: 2601 = duplicate key, 2627 = unique index).</summary>
    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        for (var current = ex.InnerException; current is not null; current = current.InnerException)
        {
            if (current is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> IsAdminUserAsync(int userId, CancellationToken ct) =>
        await db.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.Role == UserRole.Admin, ct);

    private static bool CanManageLesson(int userId, string role, Lesson lesson) =>
        role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase) || lesson.CreatedBy == userId;

    /// <summary>
    /// findings-biz #7: SaveChanges upsert progress với retry — DbUpdateConcurrencyException (RowVersion đổi
    /// giữa đọc/ghi) hoặc unique violation (2 request cùng Add UserProgress/UserNodeProgress) → reload dữ liệu
    /// mới + merge lại (Status=2 giữ pass; BestScore/Stars/NodeScore lấy max) rồi lưu lại. Không rollback
    /// submission đã insert — pass không được mất vì lỗi concurrency.
    /// </summary>
    private async Task<bool> SaveProgressWithRetryAsync(
        int userId, int lessonId, int? nodeId, int score, int nodeMaxScore, DateTime now, CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (attempt >= MaxSaveAttempts)
                {
                    return false;
                }

                await ReloadAndReapplyProgressAsync(userId, lessonId, nodeId, score, nodeMaxScore, now, ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                if (attempt >= MaxSaveAttempts)
                {
                    return false;
                }

                await ReloadAndReapplyProgressAsync(userId, lessonId, nodeId, score, nodeMaxScore, now, ct);
            }
        }
    }

    /// <summary>
    /// Reload các entity progress bị đụng độ (Added → detach rồi đọc lại từ store; Modified → ReloadAsync)
    /// rồi chạy lại upsert merge với dữ liệu MỚI.
    /// </summary>
    private async Task ReloadAndReapplyProgressAsync(
        int userId, int lessonId, int? nodeId, int score, int nodeMaxScore, DateTime now, CancellationToken ct)
    {
        foreach (var entry in db.ChangeTracker.Entries<UserProgress>().Where(e => e.State == EntityState.Added).ToList())
        {
            entry.State = EntityState.Detached;
        }

        foreach (var entry in db.ChangeTracker.Entries<UserNodeProgress>().Where(e => e.State == EntityState.Added).ToList())
        {
            entry.State = EntityState.Detached;
        }

        foreach (var entry in db.ChangeTracker.Entries<UserProgress>().Where(e => e.State == EntityState.Modified).ToList())
        {
            await entry.ReloadAsync(ct);
        }

        foreach (var entry in db.ChangeTracker.Entries<UserNodeProgress>().Where(e => e.State == EntityState.Modified).ToList())
        {
            await entry.ReloadAsync(ct);
        }

        await UpsertUserProgressAsync(userId, lessonId, score, now, ct);
        if (nodeId is { } nid)
        {
            await UpsertNodeProgressAsync(userId, nid, score, nodeMaxScore, now, ct);
        }
    }

    /// <summary>
    /// Multi-instance race CÙNG idempotency key: request khác vừa commit submission trước (unique violation).
    /// Merge progress với điểm THỰC CUỐI = max(winner.Score, score hiện tại) (pass/điểm cao hơn thắng);
    /// quest pass_node chỉ tăng khi CHÍNH request này làm node pass (winner chưa pass — anti double-count);
    /// response build từ dữ liệu MERGED (Major #2: request thua race nhận đúng kết quả cuối — score cao nhất,
    /// passed đúng — KHÔNG trả "không đạt" dù node đã pass).
    /// </summary>
    private async Task<Result<SubmitResultDto>> MergeDuplicateProgressAndReturnAsync(
        int userId, Exercise exercise, int score, DateTime now, ExerciseSubmission winner,
        string resultJson, List<QuestionResultDto> results, CancellationToken ct)
    {
        var maxScore = exercise.Questions.Sum(q => q.Points);
        var mergedScore = Math.Max(winner.Score, score);   // điểm thực cuối sau merge (max — upsert cũng max)
        var mergedPassed = mergedScore == maxScore;

        // Quest pass_node: nodeJustPassed đọc TRƯỚC khi merge (sau merge Status đã = 2 nên không phát hiện được
        // "vừa pass"). Chỉ tăng khi request này mang điểm CAO HƠN winner (mergedScore > winner.Score ⟹ winner
        // không pass ⟹ winner không tăng quest) → không double-count; winner pass (mergedScore == winner.Score)
        // → winner tự tăng trong luồng của nó. pass_quiz/pass_lab do winner tăng (mọi submission — R3 Đợt A).
        var nodeJustPassed = false;
        if (exercise.NodeId is { } nodeId && mergedPassed)
        {
            nodeJustPassed = !await db.UserNodeProgress.AsNoTracking()
                .AnyAsync(p => p.UserId == userId && p.NodeId == nodeId && p.Status == 2, ct);
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await UpsertUserProgressAsync(userId, exercise.LessonId, mergedScore, now, ct);
        if (exercise.NodeId is { } nodeId2)
        {
            await UpsertNodeProgressAsync(userId, nodeId2, mergedScore, maxScore, now, ct);
        }

        if (!await SaveProgressWithRetryAsync(userId, exercise.LessonId, exercise.NodeId, mergedScore, maxScore, now, ct))
        {
            await tx.RollbackAsync(ct);
            return Result<SubmitResultDto>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

        if (mergedPassed && nodeJustPassed && exercise.NodeId is { })
        {
            await QuestProgressWriter.IncrementAsync(db, userId, "pass_node", ct);
        }

        await tx.CommitAsync(ct);

        // Major #2: reload trạng thái cuối (submission winner + điểm merged) → response phản ánh kết quả
        // THỰC CUỐI. Results lấy theo submission có điểm cao nhất để response tự nhất quán.
        var final = await db.ExerciseSubmissions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == winner.Id, ct) ?? winner;
        var finalScore = Math.Max(final.Score, mergedScore);
        var bestResults = finalScore == score
            ? DeserializeResults(resultJson, results)
            : DeserializeResults(final.ResultJson, []);

        return Result<SubmitResultDto>.Ok(new SubmitResultDto
        {
            Score = finalScore,
            MaxScore = maxScore,
            Passed = finalScore == maxScore,
            Results = bestResults,
            SubmissionId = final.Id,
            SubmittedAt = final.SubmittedAt
        });
    }

    /// <summary>
    /// Multi-instance race CÙNG idempotency key cho code-submit: merge progress với điểm THỰC CUỐI
    /// (max — pass/điểm cao hơn thắng), quest pass_node chỉ tăng khi chính request này làm node pass
    /// (winner chưa pass — anti double-count; code_run do winner tăng trong luồng của nó), response build
    /// từ dữ liệu MERGED (Major #2: request thua race nhận đúng kết quả cuối, không trả "không đạt").
    /// </summary>
    private async Task<Result<CodeSubmitResultDto>> MergeCodeDuplicateProgressAndReturnAsync(
        int userId, Exercise exercise, int score, int maxScore, DateTime now, CodeSubmission winner,
        string resultJson, CodeSubmitRequest request, CancellationToken ct)
    {
        var mergedScore = Math.Max(winner.Score, score);   // điểm thực cuối sau merge (max)
        var mergedPassed = mergedScore >= maxScore;

        var nodeJustPassed = false;
        if (exercise.NodeId is { } nodeId && mergedPassed)
        {
            nodeJustPassed = !await db.UserNodeProgress.AsNoTracking()
                .AnyAsync(p => p.UserId == userId && p.NodeId == nodeId && p.Status == 2, ct);
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await UpsertUserProgressAsync(userId, exercise.LessonId, mergedScore, now, ct);
        if (exercise.NodeId is { } nodeId2)
        {
            await UpsertNodeProgressAsync(userId, nodeId2, mergedScore, maxScore, now, ct);
        }

        if (!await SaveProgressWithRetryAsync(userId, exercise.LessonId, exercise.NodeId, mergedScore, maxScore, now, ct))
        {
            await tx.RollbackAsync(ct);
            return Result<CodeSubmitResultDto>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

        // Quest pass_node: chỉ khi request này mang điểm CAO HƠN winner (winner chưa pass → chưa tăng quest);
        // code_run do winner tăng (mọi code submit) → không tăng lại (không double-count).
        if (mergedPassed && nodeJustPassed && exercise.NodeId is { })
        {
            await QuestProgressWriter.IncrementAsync(db, userId, "pass_node", ct);
        }

        await tx.CommitAsync(ct);

        // Major #2: reload trạng thái cuối + response từ dữ liệu merged (score cao nhất, passed đúng).
        var final = await db.CodeSubmissions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == winner.Id, ct) ?? winner;
        var finalScore = Math.Max(final.Score, mergedScore);
        if (finalScore == score)
        {
            return Result<CodeSubmitResultDto>.Ok(new CodeSubmitResultDto
            {
                Score = score,
                Passed = request.Passed,
                Total = request.Total,
                Results = DeserializeCodeResults(resultJson, request.Results),
                SubmissionId = final.Id,
                SubmittedAt = final.SubmittedAt
            });
        }

        return Result<CodeSubmitResultDto>.Ok(new CodeSubmitResultDto
        {
            Score = final.Score,
            Passed = final.PassedTests,
            Total = final.TotalTests,
            Results = DeserializeCodeResults(final.ResultJson, []),
            SubmissionId = final.Id,
            SubmittedAt = final.SubmittedAt
        });
    }

    private static List<QuestionResultDto> DeserializeResults(string resultJson, List<QuestionResultDto> fallback)
    {
        try
        {
            return JsonSerializer.Deserialize<List<QuestionResultDto>>(resultJson) ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    private static List<CodeTestCaseResultDto> DeserializeCodeResults(string resultJson, List<CodeTestCaseResultDto> fallback)
    {
        try
        {
            return JsonSerializer.Deserialize<List<CodeTestCaseResultDto>>(resultJson) ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    /// <summary>Đọc entryFunction từ ResultJson do MÁY CHỦ chấm (wrapper { judged, entryFunction, ... }) —
    /// dùng để xác định task nào đã có bài nộp full-pass (node pass ASM = đủ TẤT CẢ task).</summary>
    private static string? TryReadJudgedEntry(string resultJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(resultJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object
                || !doc.RootElement.TryGetProperty("judged", out var judgedProp)
                || judgedProp.ValueKind != JsonValueKind.True
                || !doc.RootElement.TryGetProperty("entryFunction", out var entryProp)
                || entryProp.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return entryProp.GetString();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Kết quả trả về client: MÁY CHỦ chấm → map case kết quả (kèm input/expected từ đề);
    /// không phải server chấm → giữ hành vi cũ (đọc từ ResultJson, fallback request.Results).</summary>
    private static List<CodeTestCaseResultDto> BuildCodeSubmitResults(
        bool serverJudged, List<CodelabCaseResult> judgeCases, CodeSubmitRequest request)
    {
        if (!serverJudged)
        {
            return request.Results;
        }

        return judgeCases
            .Select((c, i) => new CodeTestCaseResultDto
            {
                TestId = $"case-{i + 1}",
                Passed = c.Passed,
                Error = c.Error
            })
            .ToList();
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
        int exerciseId, int page, int pageSize, bool includeUser, int? userId = null,
        CancellationToken ct = default, DateTime? lastSubmittedAt = null, int? lastId = null)
    {
        var (safePage, safeSize) = Pagination.Normalize(page, pageSize);

        var query = db.ExerciseSubmissions.AsNoTracking().Where(s => s.ExerciseId == exerciseId);
        if (userId is not null)
        {
            query = query.Where(s => s.UserId == userId);
        }

        var total = await query.CountAsync(ct);

        // perf#6: keyset/cursor — (SubmittedAt < @last) OR (SubmittedAt = @last AND Id < @lastId)
        // ORDER BY SubmittedAt DESC, Id DESC (append-only, trang sâu không OFFSET lại toàn bộ).
        // Client truyền lastSubmittedAt+lastId = phần tử cuối trang trước; không có → fallback offset
        // (contract page/size cũ giữ nguyên). ThenByDescending(Id) chốt tie SubmittedAt deterministic.
        var ordered = query.OrderByDescending(s => s.SubmittedAt).ThenByDescending(s => s.Id);
        IQueryable<ExerciseSubmission> paged;
        if (lastSubmittedAt is not null && lastId is not null)
        {
            paged = ordered.Where(s => s.SubmittedAt < lastSubmittedAt.Value
                || (s.SubmittedAt == lastSubmittedAt.Value && s.Id < lastId.Value));
        }
        else
        {
            paged = ordered.Skip((safePage - 1) * safeSize);
        }

        var items = await paged
            .Take(safeSize)
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
        int exerciseId, int page, int pageSize, bool includeUser, int? userId = null,
        CancellationToken ct = default, DateTime? lastSubmittedAt = null, int? lastId = null)
    {
        var (safePage, safeSize) = Pagination.Normalize(page, pageSize);

        var query = db.CodeSubmissions.AsNoTracking().Where(s => s.ExerciseId == exerciseId);
        if (userId is not null)
        {
            query = query.Where(s => s.UserId == userId);
        }

        var total = await query.CountAsync(ct);

        // perf#7: keyset/cursor như #6 (append-only) — fallback offset khi không có cursor.
        var ordered = query.OrderByDescending(s => s.SubmittedAt).ThenByDescending(s => s.Id);
        IQueryable<CodeSubmission> paged;
        if (lastSubmittedAt is not null && lastId is not null)
        {
            paged = ordered.Where(s => s.SubmittedAt < lastSubmittedAt.Value
                || (s.SubmittedAt == lastSubmittedAt.Value && s.Id < lastId.Value));
        }
        else
        {
            paged = ordered.Skip((safePage - 1) * safeSize);
        }

        var items = await paged
            .Take(safeSize)
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
