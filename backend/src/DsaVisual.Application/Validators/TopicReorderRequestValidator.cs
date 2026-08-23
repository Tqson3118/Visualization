using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Đổi thứ tự chủ đề — PUT /topics/reorder (API_REFERENCE.md §4.3, finding security#12).</summary>
public sealed class TopicReorderRequestValidator : AbstractValidator<TopicReorderRequest>
{
    public TopicReorderRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty().WithMessage("Danh sách chủ đề không được để trống")
            .Must(ids => ids.Count <= 200).WithMessage("Danh sách chủ đề quá lớn");

        RuleForEach(x => x.Ids)
            .GreaterThan(0).WithMessage("Id chủ đề không hợp lệ");
    }
}
