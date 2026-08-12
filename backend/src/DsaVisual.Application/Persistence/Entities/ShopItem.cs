namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Vật phẩm cửa hàng gems — SDD §7.3.28. ItemKey UNIQUE.</summary>
public sealed class ShopItem
{
    public int Id { get; set; }
    public string ItemKey { get; set; } = string.Empty;              // UNIQUE
    public string Name { get; set; } = string.Empty;
    public int PriceGems { get; set; }
    public int MaxStack { get; set; }
    public int Type { get; set; }
    public int? DurationHours { get; set; }
}
