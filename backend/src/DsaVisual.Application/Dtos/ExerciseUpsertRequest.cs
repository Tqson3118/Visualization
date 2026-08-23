using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Dtos;

/// <summary>Tạo/sửa bài tập — POST/PUT /exercises (API_REFERENCE.md §4.6, SDD §7.3.9).</summary>
public sealed class ExerciseUpsertRequest
{
    public int LessonId { get; set; }
    public int? NodeId { get; set; }                 // node Ladder (v2.4)
    public int? Stage { get; set; }                  // 1=QUIZ, 2=LAB, 3=CODE (khi NodeId != null)
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ExerciseType Type { get; set; } = ExerciseType.Mcq;
    public int? DurationMinutes { get; set; }        // null = không giới hạn
    public int MaxScore { get; set; }
    public ExerciseStatus Status { get; set; } = ExerciseStatus.Draft;
    public string? ConfigJson { get; set; }          // CODE: { signature, testCases }
    public List<QuestionUpsertDto> Questions { get; set; } = [];
}
