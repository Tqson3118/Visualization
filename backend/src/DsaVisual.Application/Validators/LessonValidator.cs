using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>
/// Validator mẫu cho LessonUpsertRequest — ràng buộc theo API_REFERENCE.md §3.5.
/// FluentValidation gọi ở Service (SDD §5.3.4).
/// </summary>
public sealed class LessonValidator : AbstractValidator<LessonUpsertRequest>
{
    public LessonValidator()
    {
        RuleFor(x => x.TopicId)
            .GreaterThan(0)
            .WithMessage("Chủ đề không hợp lệ");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .Length(3, 200).WithMessage("Tiêu đề phải từ 3 đến 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

        RuleFor(x => x.ContentHtml)
            .NotEmpty().WithMessage("Nội dung bài học không được để trống")
            .MaximumLength(200_000).WithMessage("Nội dung bài học không được vượt quá 200.000 ký tự");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái bài học không hợp lệ");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự sắp xếp phải ≥ 0");
    }
}
