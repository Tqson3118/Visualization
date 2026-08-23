using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Xác nhận mã OTP 2FA — POST /auth/2fa/verify (API_REFERENCE.md §4.12, finding security#12).</summary>
public sealed class Verify2FaRequestValidator : AbstractValidator<Verify2FaRequest>
{
    public Verify2FaRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã xác thực không được để trống")
            .Length(6).WithMessage("Mã xác thực phải là 6 chữ số")
            .Must(c => c.All(char.IsAsciiDigit)).WithMessage("Mã xác thực phải là 6 chữ số");
    }
}
