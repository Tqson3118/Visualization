using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Validators;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test mẫu cho LessonValidator — ràng buộc theo API_REFERENCE.md §3.5.
/// </summary>
public class LessonValidatorTests
{
    private readonly LessonValidator _validator = new();

    private static LessonUpsertRequest ValidRequest() => new()
    {
        TopicId = 1,
        Title = "Bubble Sort — Sắp xếp nổi bọt",
        Description = "Giải thuật đơn giản nhất",
        ContentHtml = "<h2>Ý tưởng</h2><p>So sánh từng cặp phần tử liền kề</p>",
        Status = LessonStatus.Draft,
        SortOrder = 1
    };

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(ValidRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task EmptyTitle_Fails()
    {
        var request = ValidRequest();
        request.Title = "";

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LessonUpsertRequest.Title));
    }

    [Theory]
    [InlineData(2)]     // < 3 ký tự
    [InlineData(201)]   // > 200 ký tự
    public async Task InvalidTitleLength_Fails(int length)
    {
        var request = ValidRequest();
        request.Title = new string('x', length);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LessonUpsertRequest.Title));
    }

    [Fact]
    public async Task ContentHtmlOver200k_Fails()
    {
        var request = ValidRequest();
        request.ContentHtml = new string('a', 200_001);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LessonUpsertRequest.ContentHtml));
    }

    [Fact]
    public async Task InvalidStatus_Fails()
    {
        var request = ValidRequest();
        request.Status = (LessonStatus)99;

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LessonUpsertRequest.Status));
    }

    [Fact]
    public async Task TopicIdZero_Fails()
    {
        var request = ValidRequest();
        request.TopicId = 0;

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LessonUpsertRequest.TopicId));
    }

    [Fact]
    public async Task NegativeSortOrder_Fails()
    {
        var request = ValidRequest();
        request.SortOrder = -1;

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LessonUpsertRequest.SortOrder));
    }
}

public class ExerciseUpsertRequestValidatorTests
{
    private readonly ExerciseUpsertRequestValidator _validator = new();

    private static ExerciseUpsertRequest ValidMcqRequest() => new()
    {
        LessonId = 1,
        Title = "Quiz Kiểm tra Bubble Sort",
        Description = "Củng cố kiến thức",
        Type = ExerciseType.Mcq,
        Status = ExerciseStatus.Active,
        MaxScore = 10,
        Questions =
        [
            new QuestionUpsertDto
            {
                Content = "Độ phức tạp thời gian tốt nhất của Bubble Sort là gì?",
                Type = QuestionType.Single,
                Points = 2,
                Options = ["O(N)", "O(N^2)", "O(log N)", "O(1)"],
                AnswerJson = "[0]"
            }
        ]
    };

    private static ExerciseUpsertRequest ValidCodeRequest() => new()
    {
        LessonId = 1,
        Title = "Thực hành: Cài đặt Bubble Sort",
        Description = "Cài đặt thuật toán",
        Type = ExerciseType.Code,
        Status = ExerciseStatus.Active,
        MaxScore = 100,
        ConfigJson = "{\"entryFunction\":\"bubbleSort\",\"starterCode\":\"function bubbleSort(arr) { return arr; }\",\"testCases\":[{\"input\":\"[[3, 2, 1]]\",\"expected\":\"[1, 2, 3]\",\"isHidden\":false}]}",
        Questions = []
    };

    [Fact]
    public async Task ValidMcqRequest_Passes()
    {
        var result = await _validator.ValidateAsync(ValidMcqRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidCodeRequest_Passes()
    {
        var result = await _validator.ValidateAsync(ValidCodeRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CodeRequest_EmptyConfigJson_Fails()
    {
        var request = ValidCodeRequest();
        request.ConfigJson = "";

        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExerciseUpsertRequest.ConfigJson));
    }

    [Fact]
    public async Task CodeRequest_InvalidJsonConfig_Fails()
    {
        var request = ValidCodeRequest();
        request.ConfigJson = "not a valid json {";

        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExerciseUpsertRequest.ConfigJson));
    }

    [Fact]
    public async Task CodeRequest_MissingTestCases_Fails()
    {
        var request = ValidCodeRequest();
        request.ConfigJson = "{\"entryFunction\":\"solve\",\"starterCode\":\"function solve() {}\",\"testCases\":[]}";

        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ExerciseUpsertRequest.ConfigJson));
    }
}

