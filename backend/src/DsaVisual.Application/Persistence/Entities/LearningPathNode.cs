namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Node của lộ trình học (Ladder / Tree) — SDD §7.3.25.</summary>
public sealed class LearningPathNode
{
    public int Id { get; set; }
    public int PathId { get; set; }                                  // FK → LearningPaths.Id
    public int? ParentId { get; set; }                               // FK self; null = cấp gốc của cây
    public PathItemType ItemType { get; set; } = PathItemType.Theory;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }                         // mô tả folder (module)
    public int? LessonId { get; set; }                               // node bài học (loại Theory)
    public int? FinalTestId { get; set; }                            // bài kiểm tra cuối node / Quiz (exercise Mcq)
    public int? LabExerciseId { get; set; }                          // FK → Exercises.Id (node loại Lab)
    public int SortOrder { get; set; }

    // Soft delete (D6): node bị xóa chỉ ẩn khỏi truy vấn (global query filter), giữ dòng cho
    // UserNodeProgress / NodeSession / ClassAssignment trỏ tới (FK Restrict) khỏi orphan.
    public DateTime? DeletedAt { get; set; }
}
