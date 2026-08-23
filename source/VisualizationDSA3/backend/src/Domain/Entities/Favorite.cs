using System;

namespace VisualizationDSA.Domain.Entities
{
    public class Favorite
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string SimulationKey { get; set; } = string.Empty;
        public string? InputJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
