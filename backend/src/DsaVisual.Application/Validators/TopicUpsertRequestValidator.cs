using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Tạo/sửa chủ đề — POST/PUT /topics (API_REFERENCE.md §4.3, finding security#12).</summary>
public sealed class TopicUpsertRequestValidator : AbstractValidator<TopicUpsertRequest>
{
    public TopicUpsertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên chủ đề không được để trống")
            .MaximumLength(200).WithMessage("Tên chủ đề không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự")
            .When(x => x.Description is not null);

        RuleFor(x => x.ParentId)
            .GreaterThan(0).WithMessage("Chủ đề cha không hợp lệ")
            .When(x => x.ParentId is not null);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự sắp xếp không hợp lệ");
    }
}
