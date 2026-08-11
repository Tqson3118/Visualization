using System;

namespace VisualizationDSA.Domain.Entities
{
    public class RoadmapEnrollment
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid RoadmapId { get; private set; }
        public string Status { get; private set; } = "Active"; // Active | Completed | Dropped
        public DateTime EnrolledAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        public virtual User User { get; private set; } = null!;
        public virtual CustomRoadmap Roadmap { get; private set; } = null!;

        private RoadmapEnrollment() { }

        public RoadmapEnrollment(Guid userId, Guid roadmapId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            RoadmapId = roadmapId;
            Status = "Active";
            EnrolledAt = DateTime.UtcNow;
        }

        public void MarkCompleted()
        {
            Status = "Completed";
            CompletedAt = DateTime.UtcNow;
        }

        public void Drop()
        {
            Status = "Dropped";
        }
    }
}
