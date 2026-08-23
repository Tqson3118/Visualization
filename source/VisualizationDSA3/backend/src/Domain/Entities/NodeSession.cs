using System;

namespace VisualizationDSA.Domain.Entities
{
    public class NodeSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid NodeId { get; set; }
        public LearningPathNode Node { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
