using System;

namespace VisualizationDSA.Domain.Entities
{
    /// <summary>Đánh giá khóa học (testimonial) từ học viên đã hoàn thành.</summary>
    public class CourseReview
    {
        public Guid Id { get; private set; }
        public Guid CourseId { get; private set; }
        public Guid UserId { get; private set; }
        public int Rating { get; private set; }
        public string Comment { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

        public virtual Course Course { get; private set; } = null!;
        public virtual User User { get; private set; } = null!;

        private CourseReview() { }

        public CourseReview(Guid courseId, Guid userId, int rating, string comment)
        {
            Id = Guid.NewGuid();
            CourseId = courseId;
            UserId = userId;
            Rating = Math.Clamp(rating, 1, 5);
            Comment = comment ?? string.Empty;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
