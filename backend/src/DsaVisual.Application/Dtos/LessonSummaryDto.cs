using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Dtos;

/// <summary>Tiến độ cá nhân trong danh sách (API_REFERENCE.md §4.4 GET /lessons).</summary>
public sealed class LessonSummaryProgressDto
{
    public bool Viewed { get; set; }
    public int? BestScore { get; set; }
    public bool Completed { get; set; }
}

/// <summary>Tóm tắt bài học trong danh sách — API_REFERENCE.md §3.4/§4.4.</summary>
public sealed class LessonSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TopicId { get; set; }
    public int SortOrder { get; set; }
    public string Status { get; set; } = LessonStatus.Draft.ToString();
    public bool IsClassOnly { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int SimulationCount { get; set; }
    public int ExerciseCount { get; set; }
    public LessonSummaryProgressDto? Progress { get; set; }
}
