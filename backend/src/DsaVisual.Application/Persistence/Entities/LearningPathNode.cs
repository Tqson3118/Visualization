namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Node của lộ trình học (Ladder) — SDD §7.3.25.</summary>
public sealed class LearningPathNode
{
    public int Id { get; set; }
    public int PathId { get; set; }                                  // FK → LearningPaths.Id
    public string Title { get; set; } = string.Empty;
    public int? LessonId { get; set; }                               // node bài học (tùy chọn)
    public int SortOrder { get; set; }
    public int? FinalTestId { get; set; }                            // bài kiểm tra cuối node
}
