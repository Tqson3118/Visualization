using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

public sealed class GamificationSettingsValidator : AbstractValidator<GamificationSettingsDto>
{
    public GamificationSettingsValidator()
    {
        RuleFor(x => x.TheoryBaseXp)
            .InclusiveBetween(0, 1000)
            .WithMessage("Điểm XP Lý thuyết phải từ 0 đến 1000.");

        RuleFor(x => x.QuizBaseXp)
            .InclusiveBetween(0, 1000)
            .WithMessage("Điểm XP Trắc nghiệm phải từ 0 đến 1000.");

        RuleFor(x => x.CodelabBaseXp)
            .InclusiveBetween(0, 1000)
            .WithMessage("Điểm XP Code Lab phải từ 0 đến 1000.");

        RuleFor(x => x.StreakBonusXp)
            .InclusiveBetween(0, 1000)
            .WithMessage("Điểm thưởng Streak phải từ 0 đến 1000.");

        RuleFor(x => x.HeartsMaxFree)
            .InclusiveBetween(1, 100)
            .WithMessage("Số tim tối đa (Free) phải từ 1 đến 100.");

        RuleFor(x => x.HeartsMaxPremium)
            .InclusiveBetween(1, 100)
            .WithMessage("Số tim tối đa (Premium) phải từ 1 đến 100.")
            .GreaterThanOrEqualTo(x => x.HeartsMaxFree)
            .WithMessage("Số tim Premium không được nhỏ hơn số tim Free.");

        RuleFor(x => x.HeartRegenMinutes)
            .InclusiveBetween(1, 1440)
            .WithMessage("Thời gian hồi tim phải từ 1 phút đến 1440 phút (24h).");

        RuleFor(x => x.SessionHours)
            .InclusiveBetween(1, 168)
            .WithMessage("Thời hạn session phải từ 1 giờ đến 168 giờ (7 ngày).");
    }
}
