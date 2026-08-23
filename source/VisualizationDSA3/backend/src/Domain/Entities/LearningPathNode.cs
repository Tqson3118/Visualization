using System;

namespace VisualizationDSA.Domain.Entities
{
    public class LearningPathNode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LearningPathId { get; set; }
        public LearningPath LearningPath { get; set; } = null!;
        public int OrderIndex { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid? LessonId { get; set; }
        public Lesson? Lesson { get; set; }
    }
}
