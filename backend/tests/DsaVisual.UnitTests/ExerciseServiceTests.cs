using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Services;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.UnitTests;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test chấm điểm ExerciseService: SINGLE (selected == answer[0]), MULTI (tập hợp bằng),
/// LAB (trạng thái cuối + stepsUsed ≤ maxSteps×1.5) — SDD §5.5, API_REFERENCE §4.6/§4.16.
/// </summary>
public class ExerciseServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private static ExerciseUpsertRequest BuildRequest() => new()
    {
        LessonId = 1,
        Title = "Trắc nghiệm Bubble Sort",
        Type = ExerciseType.Mcq,
        MaxScore = 10,
        Status = ExerciseStatus.Active,
        Questions =
        [
            new QuestionUpsertDto
            {
                Content = "Câu SINGLE: phần tử lớn nhất nằm ở đâu?",
                Type = QuestionType.Single,
                Options = ["Đầu mảng", "Cuối mảng", "Giữa mảng"],
                AnswerJson = "[1]",
                Points = 4,
                SortOrder = 0
            },
            new QuestionUpsertDto
            {
                Content = "Câu MULTI: chọn các thao tác O(1)?",
                Type = QuestionType.Multi,
                Options = ["Push", "Pop", "Tìm kiếm", "Duyệt"],
                AnswerJson = "[0,2]",
                Points = 3,
                SortOrder = 1
            },
            new QuestionUpsertDto
            {
                Content = "Câu BOOLEAN: mảng [1,2,3] đã sắp xếp?",
                Type = QuestionType.Boolean,
                Options = ["Đúng", "Sai"],
                AnswerJson = "[0]",
                Points = 3,
                SortOrder = 2
            }
        ]
    };

    private async Task<(ExerciseService Service, int ExerciseId, AppDbContext Db)> SetupAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        db.Lessons.Add(new Lesson
        {
            Id = 1,
            TopicId = 1,
            Title = "Bubble Sort",
            ContentHtml = "<p>nội dung</p>",
            Status = LessonStatus.Active,
            CreatedBy = 1,
            CreatedAt = _clock.UtcNow
        });
        db.Topics.Add(new Topic { Id = 1, Name = "Sắp xếp", CreatedBy = 1, CreatedAt = _clock.UtcNow });
        db.Users.Add(new User
        {
            Id = 1,
            Email = "student@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Student",
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        var service = TestServices.CreateExerciseService(db, _clock);
        var created = await service.CreateAsync(1, BuildRequest(), CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return (service, created.Value!.Id, db);
    }

    [Fact]
    public async Task Submit_SingleCorrect_GetsPoints()
    {
        var (service, exerciseId, db) = await SetupAsync(nameof(Submit_SingleCorrect_GetsPoints));
        var questions = await LoadQuestionsAsync(db, exerciseId);

        var result = await service.SubmitAsync(1, exerciseId, new SubmitRequest
        {
            Answers = new List<AnswerDto>
            {
                new() { QuestionId = questions[0].Id, Selected = [1] },       // SINGLE đúng
                new() { QuestionId = questions[1].Id, Selected = [0, 2] },    // MULTI đúng (khác thứ tự)
                new() { QuestionId = questions[2].Id, Selected = [0] }        // BOOLEAN đúng
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.Score);
        Assert.Equal(10, result.Value.MaxScore);
        Assert.True(result.Value.Passed);
        Assert.All(result.Value.Results, r => Assert.True(r.Correct));
    }

    [Fact]
    public async Task Submit_SingleWrong_ZeroPointsForThatQuestion()
    {
        var (service, exerciseId, db) = await SetupAsync(nameof(Submit_SingleWrong_ZeroPointsForThatQuestion));
        var questions = await LoadQuestionsAsync(db, exerciseId);

        var result = await service.SubmitAsync(1, exerciseId, new SubmitRequest
        {
            Answers = new List<AnswerDto>
            {
                new() { QuestionId = questions[0].Id, Selected = [0] },       // SINGLE sai
                new() { QuestionId = questions[1].Id, Selected = [0, 2] },
                new() { QuestionId = questions[2].Id, Selected = [0] }
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, result.Value!.Score);   // chỉ MULTI + BOOLEAN
        Assert.False(result.Value.Passed);
        Assert.False(result.Value.Results[0].Correct);
    }

    [Fact]
    public async Task Submit_MultiWrongOrder_StillCorrectBySetEquality()
    {
        var (service, exerciseId, db) = await SetupAsync(nameof(Submit_MultiWrongOrder_StillCorrectBySetEquality));
        var questions = await LoadQuestionsAsync(db, exerciseId);

        var result = await service.SubmitAsync(1, exerciseId, new SubmitRequest
        {
            Answers = new List<AnswerDto>
            {
                new() { QuestionId = questions[0].Id, Selected = [1] },
                new() { QuestionId = questions[1].Id, Selected = [2, 0] },    // đảo thứ tự — vẫn đúng
                new() { QuestionId = questions[2].Id, Selected = [0] }
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.Score);
    }

    [Fact]
    public async Task Submit_MissingAnswer_ReturnsQuestionAnswerMismatch()
    {
        var (service, exerciseId, db) = await SetupAsync(nameof(Submit_MissingAnswer_ReturnsQuestionAnswerMismatch));
        var questions = await LoadQuestionsAsync(db, exerciseId);

        var result = await service.SubmitAsync(1, exerciseId, new SubmitRequest
        {
            Answers = new List<AnswerDto>
            {
                new() { QuestionId = questions[0].Id, Selected = [1] }        // thiếu 2 câu còn lại
            }
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.QUESTION_ANSWER_MISMATCH, result.ErrorCode);
    }

    [Fact]
    public async Task Submit_DuplicateQuestionId_ReturnsValidationFailed()
    {
        // Bug P2 SETUP_TODO §8.4: answers có QuestionId trùng → trước fix ToDictionary ném → 500
        var (service, exerciseId, db) = await SetupAsync(nameof(Submit_DuplicateQuestionId_ReturnsValidationFailed));
        var questions = await LoadQuestionsAsync(db, exerciseId);

        var result = await service.SubmitAsync(1, exerciseId, new SubmitRequest
        {
            Answers = new List<AnswerDto>
            {
                new() { QuestionId = questions[0].Id, Selected = [1] },
                new() { QuestionId = questions[0].Id, Selected = [1] },       // trùng QuestionId
                new() { QuestionId = questions[1].Id, Selected = [0, 2] },
                new() { QuestionId = questions[2].Id, Selected = [0] }
            }
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        // Không tạo bản nộp
        Assert.Equal(0, await db.ExerciseSubmissions.CountAsync());
    }

    // ── SubmitCodeAsync (bug P2 SETUP_TODO §8.3: lock registry + Status Active) ──

    private async Task<(ExerciseService Service, int ExerciseId, AppDbContext Db)> SetupCodeAsync(
        string dbName, ExerciseStatus status = ExerciseStatus.Active, SubmissionLockRegistry? locks = null)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        db.Topics.Add(new Topic { Id = 1, Name = "Sắp xếp", CreatedBy = 1, CreatedAt = _clock.UtcNow });
        db.Lessons.Add(new Lesson
        {
            Id = 1,
            TopicId = 1,
            Title = "Code Bubble Sort",
            ContentHtml = "<p>nội dung</p>",
            Status = LessonStatus.Active,
            CreatedBy = 1,
            CreatedAt = _clock.UtcNow
        });
        db.Users.Add(new User
        {
            Id = 1,
            Email = "student@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Student",
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        var service = locks is null
            ? TestServices.CreateExerciseService(db, _clock)
            : TestServices.CreateExerciseService(db, _clock, locks);
        var created = await service.CreateAsync(1, new ExerciseUpsertRequest
        {
            LessonId = 1,
            Title = "Code Bubble Sort",
            Type = ExerciseType.Code,
            MaxScore = 10,
            Status = status
        }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return (service, created.Value!.Id, db);
    }

    [Fact]
    public async Task SubmitCode_Valid_ReturnsOk()
    {
        var (service, exerciseId, db) = await SetupCodeAsync(nameof(SubmitCode_Valid_ReturnsOk));

        var result = await service.SubmitCodeAsync(1, exerciseId, new CodeSubmitRequest
        {
            Code = "print('ok')",
            Score = 10,
            Passed = 2,
            Total = 2,
            Results =
            [
                new CodeTestCaseResultDto { TestId = "t1", Passed = true, Input = "5", Expected = "5", Output = "5" }
            ]
        }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(10, result.Value!.Score);
        Assert.Equal(1, await db.CodeSubmissions.CountAsync());
        // Upsert UserProgress — Viewed + BestScore
        var progress = await db.UserProgress.AsNoTracking().FirstAsync(p => p.UserId == 1 && p.LessonId == 1);
        Assert.True(progress.Viewed);
        Assert.Equal(10, progress.BestScore);
    }

    [Fact]
    public async Task SubmitCode_ExerciseNotActive_ReturnsExerciseClosed()
    {
        // Bug P2 SETUP_TODO §8.3: trước fix nộp code bài Draft vẫn được → giờ phải chặn
        var (service, exerciseId, db) = await SetupCodeAsync(nameof(SubmitCode_ExerciseNotActive_ReturnsExerciseClosed),
            status: ExerciseStatus.Draft);

        var result = await service.SubmitCodeAsync(1, exerciseId, new CodeSubmitRequest
        {
            Code = "print('ok')",
            Score = 10,
            Passed = 2,
            Total = 2
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.EXERCISE_CLOSED, result.ErrorCode);
        Assert.Equal(0, await db.CodeSubmissions.CountAsync());
    }

    [Fact]
    public async Task SubmitCode_ConcurrentSubmit_ReturnsSubmissionInProgress()
    {
        // Bug P2 SETUP_TODO §8.3: trước fix nộp code không có SubmissionLockRegistry → nộp chồng
        var locks = new SubmissionLockRegistry();
        var (service, exerciseId, _) = await SetupCodeAsync(
            nameof(SubmitCode_ConcurrentSubmit_ReturnsSubmissionInProgress), locks: locks);

        // Giành lock bằng tay (giả lập yêu cầu đang xử lý) → yêu cầu thứ 2 phải bị chặn
        using (var held = locks.TryAcquire(1, exerciseId))
        {
            Assert.NotNull(held);
            var result = await service.SubmitCodeAsync(1, exerciseId, new CodeSubmitRequest
            {
                Code = "print('ok')",
                Score = 10,
                Passed = 2,
                Total = 2
            }, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCodes.SUBMISSION_IN_PROGRESS, result.ErrorCode);
        }
    }

    [Fact]
    public async Task Submit_Lab_CorrectStateAndStepsWithinLimit_Passes()
    {
        var (service, exerciseId, db) = await SetupLabAsync(nameof(Submit_Lab_CorrectStateAndStepsWithinLimit_Passes));

        var labQuestion = await db.Questions.AsNoTracking()
            .FirstAsync(q => q.ExerciseId == exerciseId);

        // Chuẩn maxSteps=4 → giới hạn ceil(4×1.5)=6; dùng 3 bước, đúng trạng thái cuối [1,2,3]
        var result = await service.SubmitAsync(1, exerciseId, new SubmitRequest
        {
            Answers = new List<AnswerDto>
            {
                new()
                {
                    QuestionId = labQuestion.Id,
                    LabAnswer = new LabAnswerDto
                    {
                        FinalState = JsonSerializer.Deserialize<JsonElement>("[1,2,3]"),
                        StepsUsed = 3,
                        MaxSteps = 4
                    }
                }
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Score);
        Assert.True(result.Value.Passed);
        Assert.True(result.Value.Results[0].Correct);
    }

    [Fact]
    public async Task Submit_Lab_TooManySteps_Fails()
    {
        var (service, exerciseId, db) = await SetupLabAsync(nameof(Submit_Lab_TooManySteps_Fails));
        var labQuestion = await db.Questions.AsNoTracking()
            .FirstAsync(q => q.ExerciseId == exerciseId);

        // 7 bước > giới hạn 6 (chuẩn 4 × 1.5) → sai dù trạng thái cuối đúng
        var result = await service.SubmitAsync(1, exerciseId, new SubmitRequest
        {
            Answers = new List<AnswerDto>
            {
                new()
                {
                    QuestionId = labQuestion.Id,
                    LabAnswer = new LabAnswerDto
                    {
                        FinalState = JsonSerializer.Deserialize<JsonElement>("[1,2,3]"),
                        StepsUsed = 7,
                        MaxSteps = 4
                    }
                }
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.Score);
        Assert.False(result.Value.Results[0].Correct);
        Assert.Contains("bước", result.Value.Results[0].Explanation ?? string.Empty);
    }

    [Fact]
    public async Task Submit_Lab_WrongFinalState_Fails()
    {
        var (service, exerciseId, db) = await SetupLabAsync(nameof(Submit_Lab_WrongFinalState_Fails));
        var labQuestion = await db.Questions.AsNoTracking()
            .FirstAsync(q => q.ExerciseId == exerciseId);

        var result = await service.SubmitAsync(1, exerciseId, new SubmitRequest
        {
            Answers = new List<AnswerDto>
            {
                new()
                {
                    QuestionId = labQuestion.Id,
                    LabAnswer = new LabAnswerDto
                    {
                        FinalState = JsonSerializer.Deserialize<JsonElement>("[3,2,1]"),
                        StepsUsed = 3,
                        MaxSteps = 4
                    }
                }
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.Score);
        Assert.False(result.Value.Results[0].Correct);
    }

    [Fact]
    public async Task Submit_UpsertsUserProgressBestScore()
    {
        var (service, exerciseId, db) = await SetupAsync(nameof(Submit_UpsertsUserProgressBestScore));
        var questions = await LoadQuestionsAsync(db, exerciseId);

        // Lần 1: sai câu SINGLE → 6 điểm
        var first = await service.SubmitAsync(1, exerciseId, new SubmitRequest
        {
            Answers = new List<AnswerDto>
            {
                new() { QuestionId = questions[0].Id, Selected = [0] },
                new() { QuestionId = questions[1].Id, Selected = [0, 2] },
                new() { QuestionId = questions[2].Id, Selected = [0] }
            }
        }, CancellationToken.None);
        Assert.True(first.IsSuccess);

        var progress = await db.UserProgress.AsNoTracking()
            .FirstAsync(p => p.UserId == 1 && p.LessonId == 1);
        Assert.Equal(6, progress.BestScore);
        Assert.True(progress.Viewed);
    }

    private async Task<List<Question>> LoadQuestionsAsync(AppDbContext db, int exerciseId) =>
        await db.Questions.AsNoTracking()
            .Where(q => q.ExerciseId == exerciseId)
            .OrderBy(q => q.SortOrder)
            .ToListAsync();

    // ── Nộp qua luồng lớp — hạn nộp + AllowLateSubmission (v2.15, Vấn đề 14) ──

    private async Task SeedClassAssignmentAsync(AppDbContext db, int exerciseId, bool allowLate, DateTime dueAt)
    {
        db.Users.Add(new User
        {
            Id = 2,
            Email = "teacher@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Teacher",
            CreatedAt = _clock.UtcNow
        });
        db.Classes.Add(new Class
        {
            Id = 1,
            Name = "Lớp K17",
            InviteCode = "ABC123",
            OwnerId = 2,
            Status = ClassStatus.Open,
            CreatedAt = _clock.UtcNow
        });
        db.ClassMembers.Add(new ClassMember { ClassId = 1, UserId = 1, JoinedAt = _clock.UtcNow });
        db.ClassAssignments.Add(new ClassAssignment
        {
            Id = 1,
            ClassId = 1,
            ExerciseId = exerciseId,
            DueAt = dueAt,
            AllowLateSubmission = allowLate,
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Submit_AfterDue_AllowLateFalse_ReturnsAssignmentOverdue()
    {
        var (service, exerciseId, db) = await SetupAsync(nameof(Submit_AfterDue_AllowLateFalse_ReturnsAssignmentOverdue));
        await SeedClassAssignmentAsync(db, exerciseId, allowLate: false, dueAt: _clock.UtcNow.AddDays(-1));

        // Quá hạn + không cho nộp muộn → ASSIGNMENT_OVERDUE (chặn trước khi chấm điểm)
        var result = await service.SubmitAsync(1, exerciseId, new SubmitRequest { ClassAssignmentId = 1 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ASSIGNMENT_OVERDUE, result.ErrorCode);
        Assert.Equal(0, await db.ExerciseSubmissions.CountAsync());
    }

    [Fact]
    public async Task Submit_AfterDue_AllowLateTrue_Succeeds()
    {
        var (service, exerciseId, db) = await SetupAsync(nameof(Submit_AfterDue_AllowLateTrue_Succeeds));
        await SeedClassAssignmentAsync(db, exerciseId, allowLate: true, dueAt: _clock.UtcNow.AddDays(-1));
        var questions = await LoadQuestionsAsync(db, exerciseId);

        // Quá hạn nhưng GV cho phép nộp muộn → vẫn nộp + chấm bình thường
        var result = await service.SubmitAsync(1, exerciseId, new SubmitRequest
        {
            ClassAssignmentId = 1,
            Answers = new List<AnswerDto>
            {
                new() { QuestionId = questions[0].Id, Selected = [1] },       // SINGLE đúng
                new() { QuestionId = questions[1].Id, Selected = [0, 2] },    // MULTI đúng
                new() { QuestionId = questions[2].Id, Selected = [0] }        // BOOLEAN đúng
            }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(10, result.Value!.Score);
        Assert.Equal(1, await db.ExerciseSubmissions.CountAsync(s => s.UserId == 1));
    }

    private async Task<(ExerciseService Service, int ExerciseId, AppDbContext Db)> SetupLabAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        db.Lessons.Add(new Lesson
        {
            Id = 1,
            TopicId = 1,
            Title = "Lab Sắp xếp",
            ContentHtml = "<p>nội dung</p>",
            Status = LessonStatus.Active,
            CreatedBy = 1,
            CreatedAt = _clock.UtcNow
        });
        db.Topics.Add(new Topic { Id = 1, Name = "Sắp xếp", CreatedBy = 1, CreatedAt = _clock.UtcNow });
        db.Users.Add(new User
        {
            Id = 1,
            Email = "student@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Student",
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        var service = TestServices.CreateExerciseService(db, _clock);
        var created = await service.CreateAsync(1, new ExerciseUpsertRequest
        {
            LessonId = 1,
            Title = "Lab Bubble Sort",
            Type = ExerciseType.SimulationLab,
            MaxScore = 3,
            Status = ExerciseStatus.Active,
            Questions =
            [
                new QuestionUpsertDto
                {
                    Content = "Sắp xếp mảng [3,1,2]",
                    Type = QuestionType.Lab,
                    AnswerJson = """{"type":"STATE_MATCH","finalState":[1,2,3],"maxSteps":4}""",
                    Points = 3,
                    SortOrder = 0
                }
            ]
        }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return (service, created.Value!.Id, db);
    }

    // ── CompletedByUserCount (GET /exercises) ────────────────
    // Semantics: số user DISTINCT có best score ≥ MaxScore (ExerciseSubmissions + CodeSubmissions).

    private async Task<(ExerciseService Service, int ExerciseId, AppDbContext Db)> SetupSummaryAsync(string dbName)
    {
        var (service, exerciseId, db) = await SetupAsync(dbName);
        db.Users.AddRange(
            new User { Id = 2, Email = "u2@university.edu.vn", PasswordHash = "x", DisplayName = "U2", CreatedAt = _clock.UtcNow },
            new User { Id = 3, Email = "u3@university.edu.vn", PasswordHash = "x", DisplayName = "U3", CreatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();
        return (service, exerciseId, db);
    }

    [Fact]
    public async Task GetList_CompletedByUserCount_UserPassedOnce_CountsOne()
    {
        var (service, exerciseId, db) = await SetupSummaryAsync(nameof(GetList_CompletedByUserCount_UserPassedOnce_CountsOne));
        db.ExerciseSubmissions.Add(new ExerciseSubmission
        {
            UserId = 1, ExerciseId = exerciseId, Score = 10, // = MaxScore → pass
            AnswersJson = "{}", ResultJson = "{}", SubmittedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await service.GetListAsync(null, null, null, 1, 10, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!.Items);
        Assert.Equal(1, dto.CompletedByUserCount);
    }

    [Fact]
    public async Task GetList_CompletedByUserCount_UserNeverPassed_CountsZero()
    {
        var (service, exerciseId, db) = await SetupSummaryAsync(nameof(GetList_CompletedByUserCount_UserNeverPassed_CountsZero));
        db.ExerciseSubmissions.AddRange(
            new ExerciseSubmission { UserId = 1, ExerciseId = exerciseId, Score = 4, AnswersJson = "{}", ResultJson = "{}", SubmittedAt = _clock.UtcNow },
            new ExerciseSubmission { UserId = 1, ExerciseId = exerciseId, Score = 7, AnswersJson = "{}", ResultJson = "{}", SubmittedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.GetListAsync(null, null, null, 1, 10, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!.Items);
        Assert.Equal(0, dto.CompletedByUserCount);
    }

    [Fact]
    public async Task GetList_CompletedByUserCount_MultipleAttemptsOnePass_CountsOnceDistinct()
    {
        var (service, exerciseId, db) = await SetupSummaryAsync(nameof(GetList_CompletedByUserCount_MultipleAttemptsOnePass_CountsOnceDistinct));
        db.ExerciseSubmissions.AddRange(
            new ExerciseSubmission { UserId = 1, ExerciseId = exerciseId, Score = 3, AnswersJson = "{}", ResultJson = "{}", SubmittedAt = _clock.UtcNow },
            new ExerciseSubmission { UserId = 1, ExerciseId = exerciseId, Score = 6, AnswersJson = "{}", ResultJson = "{}", SubmittedAt = _clock.UtcNow },
            new ExerciseSubmission { UserId = 1, ExerciseId = exerciseId, Score = 10, AnswersJson = "{}", ResultJson = "{}", SubmittedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.GetListAsync(null, null, null, 1, 10, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!.Items);
        Assert.Equal(1, dto.CompletedByUserCount);
    }

    [Fact]
    public async Task GetList_CompletedByUserCount_MultipleDistinctUsers_CountsEachOnce()
    {
        var (service, exerciseId, db) = await SetupSummaryAsync(nameof(GetList_CompletedByUserCount_MultipleDistinctUsers_CountsEachOnce));
        db.ExerciseSubmissions.AddRange(
            new ExerciseSubmission { UserId = 1, ExerciseId = exerciseId, Score = 10, AnswersJson = "{}", ResultJson = "{}", SubmittedAt = _clock.UtcNow },
            new ExerciseSubmission { UserId = 1, ExerciseId = exerciseId, Score = 10, AnswersJson = "{}", ResultJson = "{}", SubmittedAt = _clock.UtcNow },
            new ExerciseSubmission { UserId = 2, ExerciseId = exerciseId, Score = 9, AnswersJson = "{}", ResultJson = "{}", SubmittedAt = _clock.UtcNow },
            new ExerciseSubmission { UserId = 3, ExerciseId = exerciseId, Score = 10, AnswersJson = "{}", ResultJson = "{}", SubmittedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.GetListAsync(null, null, null, 1, 10, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!.Items);
        Assert.Equal(2, dto.CompletedByUserCount); // user 1 (2 lần đạt) + user 3; user 2 chưa đạt
    }

    [Fact]
    public async Task GetList_CompletedByUserCount_CodeSubmissions_Counted()
    {
        var (service, exerciseId, db) = await SetupSummaryAsync(nameof(GetList_CompletedByUserCount_CodeSubmissions_Counted));
        db.CodeSubmissions.AddRange(
            new CodeSubmission { UserId = 1, ExerciseId = exerciseId, Score = 10, PassedTests = 10, TotalTests = 10, Code = "x", ResultJson = "{}", SubmittedAt = _clock.UtcNow },
            new CodeSubmission { UserId = 1, ExerciseId = exerciseId, Score = 2, PassedTests = 2, TotalTests = 10, Code = "x", ResultJson = "{}", SubmittedAt = _clock.UtcNow },
            new CodeSubmission { UserId = 2, ExerciseId = exerciseId, Score = 10, PassedTests = 10, TotalTests = 10, Code = "x", ResultJson = "{}", SubmittedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.GetListAsync(null, null, null, 1, 10, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!.Items);
        Assert.Equal(2, dto.CompletedByUserCount); // user 1 (đạt 1/2 lần) + user 2
    }
}
