using System;

namespace VisualizationDSA.Domain.Entities
{
    /// <summary>Lịch sử biến động gem (Earn/Spend).</summary>
    public class GemTransaction
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public int Amount { get; private set; }
        /// <summary>Earn | Spend</summary>
        public string Type { get; private set; } = "Earn";
        public string? ReferenceId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public virtual User User { get; private set; } = null!;

        private GemTransaction() { }

        public GemTransaction(Guid userId, int amount, string type, string? referenceId = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Amount = amount;
            Type = type == "Spend" ? "Spend" : "Earn";
            ReferenceId = referenceId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
