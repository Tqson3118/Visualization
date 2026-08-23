using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Thanh toán mô phỏng — POST /premium/mock-pay (API_REFERENCE.md §4.14, finding security#12).</summary>
public sealed class PremiumMockPayRequestValidator : AbstractValidator<PremiumMockPayRequest>
{
    public PremiumMockPayRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Đơn hàng không hợp lệ");
    }
}
