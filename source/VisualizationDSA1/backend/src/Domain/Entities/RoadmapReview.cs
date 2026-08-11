using System;

namespace VisualizationDSA.Domain.Entities
{
    public class RoadmapReview
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid RoadmapId { get; private set; }
        public int Rating { get; private set; } // 1..5
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        public virtual User User { get; private set; } = null!;
        public virtual CustomRoadmap Roadmap { get; private set; } = null!;

        private RoadmapReview() { } // EF

        public RoadmapReview(Guid userId, Guid roadmapId, int rating)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            RoadmapId = roadmapId;
            Rating = rating;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
