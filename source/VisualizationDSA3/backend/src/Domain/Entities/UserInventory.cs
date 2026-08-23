using System;

namespace VisualizationDSA.Domain.Entities
{
    public class UserInventory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid ItemId { get; set; }
        public ShopItem Item { get; set; } = null!;
        public bool Equipped { get; set; }
        public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
    }
}
