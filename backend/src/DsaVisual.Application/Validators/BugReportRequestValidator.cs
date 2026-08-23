using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Gửi báo cáo lỗi — POST /bug-reports (API_REFERENCE.md §4.15, finding security#12).</summary>
public sealed class BugReportRequestValidator : AbstractValidator<BugReportRequest>
{
    public BugReportRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả bắt buộc")
            .MaximumLength(2000).WithMessage("Mô tả không được vượt quá 2000 ký tự");

        RuleFor(x => x.Context)
            .MaximumLength(4000).WithMessage("Ngữ cảnh không được vượt quá 4000 ký tự")
            .When(x => x.Context is not null);
    }
}
