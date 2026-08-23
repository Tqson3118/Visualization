using System;

namespace VisualizationDSA.Domain.Entities
{
    public class StageProgress
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int StageIndex { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
