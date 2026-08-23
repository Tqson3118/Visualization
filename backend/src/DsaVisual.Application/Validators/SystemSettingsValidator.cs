using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Cấu hình hệ thống — PUT /settings (API_REFERENCE.md §4.10, finding security#12 — validate range).</summary>
public sealed class SystemSettingsValidator : AbstractValidator<SystemSettingsDto>
{
    public SystemSettingsValidator()
    {
        RuleFor(x => x.SiteName)
            .NotEmpty().WithMessage("Tên hệ thống không được để trống")
            .MaximumLength(100).WithMessage("Tên hệ thống không được vượt quá 100 ký tự");

        RuleFor(x => x.AllowedDomains)
            .Must(domains => domains.Count <= 50).WithMessage("Danh sách domain quá lớn");

        RuleForEach(x => x.AllowedDomains)
            .MaximumLength(100).WithMessage("Domain không được vượt quá 100 ký tự");

        RuleFor(x => x.PasswordPolicy.MinLength)
            .InclusiveBetween(6, 64).WithMessage("Độ dài tối thiểu mật khẩu phải từ 6 đến 64");

        RuleFor(x => x.UploadMaxMb)
            .InclusiveBetween(1, 100).WithMessage("UploadMaxMb phải từ 1 đến 100 MB");

        RuleFor(x => x.SandboxSeconds)
            .InclusiveBetween(1, 120).WithMessage("SandboxSeconds phải từ 1 đến 120 giây");

        RuleFor(x => x.SandboxMemoryMb)
            .InclusiveBetween(16, 512).WithMessage("SandboxMemoryMb phải từ 16 đến 512 MB");
    }
}
