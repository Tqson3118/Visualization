using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Đổi mật khẩu — PUT /auth/me/password (API_REFERENCE.md §4.1, finding security#12).</summary>
public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mật khẩu hiện tại không được để trống");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống")
            .Length(8, 64).WithMessage("Mật khẩu mới phải từ 8 đến 64 ký tự");
    }
}
