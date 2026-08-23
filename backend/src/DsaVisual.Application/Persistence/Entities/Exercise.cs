namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Bài tập — SDD §7.3.9.</summary>
public sealed class Exercise
{
    public int Id { get; set; }
    public int LessonId { get; set; }                                // FK NOT NULL
    public int? NodeId { get; set; }                                 // FK → LearningPathNodes.Id (Ladder, v2.4)
    public int? Stage { get; set; }                                  // 1=QUIZ, 2=LAB, 3=CODE (chỉ khi NodeId ≠ null)
    public string? ConfigJson { get; set; }                          // cấu hình SIMULATION_LAB/CODE
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ExerciseType Type { get; set; } = ExerciseType.Mcq;
    public int? DurationMinutes { get; set; }                        // null = không giới hạn
    public int MaxScore { get; set; }
    public ExerciseStatus Status { get; set; } = ExerciseStatus.Draft;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Question> Questions { get; set; } = [];
}
