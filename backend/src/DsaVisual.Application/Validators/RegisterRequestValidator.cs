using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Đăng ký — API_REFERENCE.md §3.1 (policy mật khẩu chi tiết ở Service qua PasswordPolicy — WEAK_PASSWORD).</summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Tên hiển thị không được để trống")
            .Length(2, 100).WithMessage("Tên hiển thị phải từ 2 đến 100 ký tự");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Định dạng email sai")
            .MaximumLength(256).WithMessage("Email không được vượt quá 256 ký tự");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống")
            .Length(8, 64).WithMessage("Mật khẩu phải từ 8 đến 64 ký tự");
    }
}
