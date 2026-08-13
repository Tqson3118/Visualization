using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Cập nhật hồ sơ — PUT /auth/me (API_REFERENCE.md §4.1, finding security#12).</summary>
public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .Length(2, 100).WithMessage("Tên hiển thị phải từ 2 đến 100 ký tự")
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("AvatarUrl không được vượt quá 500 ký tự")
            .When(x => x.AvatarUrl is not null);
    }
}
