using System;
using System.Collections.Generic;

namespace VisualizationDSA.Domain.Entities
{
    public class LearningPath
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<LearningPathNode> Nodes { get; set; } = new List<LearningPathNode>();
    }
}
