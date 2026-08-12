namespace DsaVisual.Application.Dtos;

/// <summary>Tiến độ tổng hợp của tôi — API_REFERENCE.md §3.9/§4.7.</summary>
public sealed class ProgressOverviewDto
{
    public int LessonsViewed { get; set; }
    public int LessonsTotal { get; set; }
    public int ExercisesCompleted { get; set; }
    public int ExercisesTotal { get; set; }
    public double AvgScore { get; set; }
    public List<TopicProgressDto> Topics { get; set; } = [];
}

/// <summary>Tiến độ theo chủ đề — API_REFERENCE.md §3.9.</summary>
public sealed class TopicProgressDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProgressPct { get; set; }
    public List<LessonProgressItemDto> Lessons { get; set; } = [];
}

/// <summary>Tiến độ 1 bài học — API_REFERENCE.md §3.9.</summary>
public sealed class LessonProgressItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Viewed { get; set; }
    public int? BestScore { get; set; }
    public bool Completed { get; set; }
}
