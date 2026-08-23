using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Gửi đánh giá bài học — POST /lessons/{id}/feedback (FR-7.4).</summary>
public sealed class LessonFeedbackRequestValidator : AbstractValidator<LessonFeedbackRequest>
{
    public LessonFeedbackRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Đánh giá phải từ 1 đến 5 sao");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Nhận xét không được vượt quá 1000 ký tự");
    }
}
