using System;

namespace VisualizationDSA.Domain.Entities
{
    public class GemTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int Amount { get; set; }
        public string Type { get; set; } = "Earn";
        public string? ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
