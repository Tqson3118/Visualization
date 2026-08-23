using System;

namespace VisualizationDSA.Domain.Entities
{
    public class ShopItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Slot { get; set; } = "avatar";
        public int PriceGems { get; set; }
        public string? IconUrl { get; set; }
        public int SortOrder { get; set; }
    }
}
