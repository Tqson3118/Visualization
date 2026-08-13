using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Tham gia lớp — POST /classes/{id}/join (API_REFERENCE.md §4.11, finding security#12).</summary>
public sealed class JoinClassRequestValidator : AbstractValidator<JoinClassRequest>
{
    public JoinClassRequestValidator()
    {
        RuleFor(x => x.InviteCode)
            .NotEmpty().WithMessage("Mã mời không được để trống")
            .MaximumLength(10).WithMessage("Mã mời không hợp lệ");
    }
}
