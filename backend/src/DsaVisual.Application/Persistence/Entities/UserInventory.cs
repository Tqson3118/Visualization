namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Kho vật phẩm người dùng — SDD §7.3.28.</summary>
public sealed class UserInventory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
