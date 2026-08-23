using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Mua vật phẩm shop — POST /shop/buy (API_REFERENCE.md §4.14, finding security#12).</summary>
public sealed class ShopBuyRequestValidator : AbstractValidator<ShopBuyRequest>
{
    public ShopBuyRequestValidator()
    {
        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("Vật phẩm không hợp lệ");
    }
}
