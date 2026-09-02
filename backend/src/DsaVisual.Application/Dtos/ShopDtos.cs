namespace DsaVisual.Application.Dtos;

/// <summary>Vật phẩm shop — API_REFERENCE.md §3.13/§4.14 (FR-10.2).</summary>
public sealed class ShopItemDto
{
    public int Id { get; set; }
    public string ItemKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Slot { get; set; }
    public int PriceGems { get; set; }
    public int MaxStack { get; set; }
    public int Type { get; set; }            // 0=consumable, 1=permanent, 2=timed
    public int Owned { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>Mua vật phẩm — POST /shop/buy {itemId}.</summary>
public sealed class ShopBuyRequest
{
    public int ItemId { get; set; }
}

/// <summary>Kết quả mua — API_REFERENCE.md §4.14 example.</summary>
public sealed class ShopBuyResultDto
{
    public ShopItemDto Item { get; set; } = new();
    public int GemsLeft { get; set; }
    public int Owned { get; set; }
}

/// <summary>Vật phẩm trong kho — GET /me/inventory (v2.9).</summary>
public sealed class InventoryItemDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Type { get; set; }
    public bool IsEquipped { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>Trang bị vật phẩm — PUT /me/inventory/equip {itemId, slot?} (v2.9).</summary>
public sealed class EquipRequest
{
    public int ItemId { get; set; }
    public string? Slot { get; set; }        // null = dùng Type làm nhóm slot
}
