using System;

namespace VisualizationDSA.Domain.Entities
{
    /// <summary>Vật phẩm trong Shop (shop-gem).</summary>
    public class ShopItem
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        /// <summary>avatar | frame | theme | misc</summary>
        public string Slot { get; private set; } = "misc";
        public int PriceGems { get; private set; }
        public string IconUrl { get; private set; } = string.Empty;
        public int SortOrder { get; private set; }
        public bool IsActive { get; private set; } = true;

        private ShopItem() { }

        public ShopItem(string name, string description, string slot, int priceGems, string iconUrl, int sortOrder)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description ?? string.Empty;
            Slot = slot;
            PriceGems = priceGems;
            IconUrl = iconUrl ?? string.Empty;
            SortOrder = sortOrder;
        }
    }
}
