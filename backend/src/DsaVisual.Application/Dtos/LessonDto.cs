using System.Text.Json.Serialization;
using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Dtos;

/// <summary>Tham chiếu mô phỏng gắn với bài học.</summary>
public sealed class SimulationRefDto
{
    public string SimulationKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

/// <summary>Tham chiếu bài tập thuộc bài học.</summary>
public sealed class ExerciseRefDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Type { get; set; }
}

/// <summary>Trạng thái tiến độ cá nhân của người gọi (Student).</summary>
public sealed class LessonProgressDto
{
    public bool Viewed { get; set; }
    public int? BestScore { get; set; }
    public bool Completed { get; set; }
}

/// <summary>Chi tiết bài học — API_REFERENCE.md §3.4.</summary>
public sealed class LessonDto
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Chỉ trả khi quyền Teacher hoặc ?includeContent=true (§3.4, §3.14.3).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentHtml { get; set; }

    public string Status { get; set; } = LessonStatus.Draft.ToString();
    public int SortOrder { get; set; }
    public List<SimulationRefDto> Simulations { get; set; } = [];
    public List<ExerciseRefDto> Exercises { get; set; } = [];
    public LessonProgressDto? Progress { get; set; }
}
