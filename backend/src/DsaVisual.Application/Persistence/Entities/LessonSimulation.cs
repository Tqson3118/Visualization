namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Mô phỏng gắn với bài học — SDD §7.3.8. UNIQUE (LessonId, SimulationKey).</summary>
public sealed class LessonSimulation
{
    public int Id { get; set; }
    public int LessonId { get; set; }                                // FK cascade
    public string SimulationKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? DefaultInputJson { get; set; }
    public int SortOrder { get; set; }
}
