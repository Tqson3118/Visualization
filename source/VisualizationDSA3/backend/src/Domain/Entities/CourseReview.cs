using System;

namespace VisualizationDSA.Domain.Entities
{
    public class CourseReview
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
