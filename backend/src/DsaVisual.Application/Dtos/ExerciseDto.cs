using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Dtos;

/// <summary>Chi tiết bài tập (không đáp án) — API_REFERENCE.md §3.7/§4.6.</summary>
public sealed class ExerciseDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public int? NodeId { get; set; }
    public int? Stage { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = ExerciseType.Mcq.ToString();
    public int? DurationMinutes { get; set; }
    public int MaxScore { get; set; }
    public string Status { get; set; } = ExerciseStatus.Draft.ToString();
    public List<QuestionDto> Questions { get; set; } = [];
    public int? BestScore { get; set; }   // của người gọi (Student)
}
