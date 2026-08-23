namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Tiến độ bài học của người học — SDD §7.3.4. UNIQUE (UserId, LessonId).</summary>
public sealed class UserProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }
    public bool Viewed { get; set; }
    public int SimulationCount { get; set; }
    public int? BestScore { get; set; }
    public DateTime? CompletedAt { get; set; }                       // Viewed + BestScore ≠ null
    public DateTime UpdatedAt { get; set; }
    public byte[]? RowVersion { get; set; }                     // concurrency token (finding #3)
}
