using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Quên mật khẩu — POST /auth/forgot-password (API_REFERENCE.md §4.1, finding security#12).</summary>
public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Định dạng email sai")
            .MaximumLength(256).WithMessage("Email không được vượt quá 256 ký tự");
    }
}
