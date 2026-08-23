namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Kho vật phẩm người dùng — SDD §7.3.28.</summary>
public sealed class UserInventory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public bool IsEquipped { get; set; }                             // v2.9: equip cùng loại set 0 các dòng khác (SDD §7.3.27)
    public DateTime PurchasedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public byte[]? RowVersion { get; set; }                     // concurrency token (finding #3)
}
