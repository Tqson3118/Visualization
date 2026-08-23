using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Tạo/sửa lớp — API_REFERENCE.md §4.11.</summary>
public sealed class ClassUpsertRequestValidator : AbstractValidator<ClassUpsertRequest>
{
    public ClassUpsertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên lớp không được để trống")
            .Length(2, 200).WithMessage("Tên lớp phải từ 2 đến 200 ký tự");

        RuleFor(x => x.Semester)
            .MaximumLength(50).WithMessage("Học kỳ không được vượt quá 50 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

        RuleFor(x => x.Status)
            .Must(s => s is null || s.Equals("open", StringComparison.OrdinalIgnoreCase) ||
                       s.Equals("closed", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Trạng thái lớp phải là open hoặc closed");
    }
}
