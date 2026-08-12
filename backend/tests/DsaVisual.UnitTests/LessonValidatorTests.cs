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
