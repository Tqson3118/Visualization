namespace DsaVisual.Application.Dtos;

/// <summary>Tiến độ chi tiết 1 bài học — GET /progress/me/lessons/{lessonId} (API_REFERENCE.md §4.7).</summary>
public sealed class LessonProgressDetailDto
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Viewed { get; set; }
    public int SimulationCount { get; set; }
    public int? BestScore { get; set; }
    public bool Completed { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ExerciseProgressItemDto> Exercises { get; set; } = [];
}

/// <summary>Tiến độ 1 bài tập của người học.</summary>
public sealed class ExerciseProgressItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? BestScore { get; set; }
    public bool Completed { get; set; }
}
