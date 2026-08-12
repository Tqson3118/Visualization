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
}
