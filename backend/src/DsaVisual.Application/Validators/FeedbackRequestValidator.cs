using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Gửi phản hồi — API_REFERENCE.md §4.15 (FR-7.4).</summary>
public sealed class FeedbackRequestValidator : AbstractValidator<FeedbackRequest>
{
    public FeedbackRequestValidator()
    {
        RuleFor(x => x.LessonId)
            .GreaterThan(0).WithMessage("Bài học không hợp lệ");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Đánh giá phải từ 1 đến 5 sao");

        RuleFor(x => x.Comment)
            .MaximumLength(200).WithMessage("Nhận xét không được vượt quá 200 ký tự");
    }
}
