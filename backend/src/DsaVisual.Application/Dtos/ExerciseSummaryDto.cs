using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Dtos;

/// <summary>Bài tập trong danh sách — GET /exercises (API_REFERENCE.md §4.6).</summary>
public sealed class ExerciseSummaryDto
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
}
