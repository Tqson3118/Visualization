namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Gán nội dung cho lớp + hạn nộp — SDD §7.3.18. CHECK (LessonId IS NOT NULL OR ExerciseId IS NOT NULL).</summary>
public sealed class ClassAssignment
{
    public int Id { get; set; }
    public int ClassId { get; set; }                                 // FK cascade
    public int? LessonId { get; set; }
    public int? ExerciseId { get; set; }
    public DateTime? DueAt { get; set; }
    public bool AllowLateSubmission { get; set; } = true;            // v2.15: false → chặn nộp sau deadline
    /// <summary>Thứ tự trong lộ trình học của lớp (sort curriculum; -1/0 = chưa vào lộ trình).</summary>
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
