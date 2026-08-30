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
    public string Type { get; set; } = "MCQ";
    public int? DurationMinutes { get; set; }
    public int MaxScore { get; set; }
    public string Status { get; set; } = ExerciseStatus.Draft.ToString();
    public int CreatedBy { get; set; }

    /// <summary>
    /// Số user distinct đã PASS bài này — best score ≥ MaxScore (gộp ExerciseSubmissions
    /// cho MCQ/SIMULATION_LAB + CodeSubmissions cho CODE; 1 user nộp n lần chỉ tính 1).
    /// </summary>
    public int CompletedByUserCount { get; set; }
}
