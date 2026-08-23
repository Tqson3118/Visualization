using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Nâng cấp Premium — POST /premium/upgrade (API_REFERENCE.md §4.14, finding security#12).</summary>
public sealed class PremiumUpgradeRequestValidator : AbstractValidator<PremiumUpgradeRequest>
{
    private static readonly string[] ValidPlans = ["1m", "3m", "12m"];

    public PremiumUpgradeRequestValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("Gói Premium không được để trống")
            .Must(p => ValidPlans.Contains(p)).WithMessage("Gói Premium không hợp lệ (1m/3m/12m)");
    }
}
