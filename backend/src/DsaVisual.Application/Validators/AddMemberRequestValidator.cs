using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Thêm sinh viên vào lớp — POST /classes/{id}/members (API_REFERENCE.md §4.11, finding security#12).</summary>
public sealed class AddMemberRequestValidator : AbstractValidator<AddMemberRequest>
{
    public AddMemberRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .MaximumLength(256).WithMessage("Email không được vượt quá 256 ký tự");
    }
}
