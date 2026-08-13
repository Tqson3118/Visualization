using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Đặt lại mật khẩu — POST /auth/reset-password (API_REFERENCE.md §4.1, finding security#12).</summary>
public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token không được để trống")
            .MaximumLength(512).WithMessage("Token không hợp lệ");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống")
            .Length(8, 64).WithMessage("Mật khẩu mới phải từ 8 đến 64 ký tự");
    }
}
