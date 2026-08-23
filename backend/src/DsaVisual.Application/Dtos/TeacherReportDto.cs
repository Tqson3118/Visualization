namespace DsaVisual.Application.Dtos;

/// <summary>Báo cáo giảng viên theo bài học — API_REFERENCE.md §3.10/§4.7.</summary>
public sealed class TeacherReportDto
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int TotalLearners { get; set; }
    public int LearnersViewed { get; set; }
    public double CompletionPct { get; set; }
    public double AvgScore { get; set; }
    public List<ExerciseReportDto> Exercises { get; set; } = [];
    public List<UserSummary> InactiveLearners { get; set; } = [];
}

/// <summary>Báo cáo 1 bài tập — API_REFERENCE.md §3.10.</summary>
public sealed class ExerciseReportDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public double AvgScore { get; set; }
    public int SubmissionCount { get; set; }
}
