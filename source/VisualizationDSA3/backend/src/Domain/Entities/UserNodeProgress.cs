using System;

namespace VisualizationDSA.Domain.Entities
{
    public class UserNodeProgress
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid NodeId { get; set; }
        public LearningPathNode Node { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Status { get; set; } = "NotStarted";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
