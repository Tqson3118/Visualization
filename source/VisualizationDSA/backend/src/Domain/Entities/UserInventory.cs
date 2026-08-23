using System;

namespace VisualizationDSA.Domain.Entities
{
    /// <summary>Vật phẩm user đã sở hữu (túi đồ).</summary>
    public class UserInventory
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid ItemId { get; private set; }
        public bool Equipped { get; private set; } = false;
        public DateTime AcquiredAt { get; private set; }

        public virtual User User { get; private set; } = null!;
        public virtual ShopItem Item { get; private set; } = null!;

        private UserInventory() { }

        public UserInventory(Guid userId, Guid itemId, bool equipped = false)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            ItemId = itemId;
            Equipped = equipped;
            AcquiredAt = DateTime.UtcNow;
        }

        public void SetEquipped(bool equipped) => Equipped = equipped;
    }
}
