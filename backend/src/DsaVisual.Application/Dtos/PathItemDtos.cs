using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Dtos;

/// <summary>DTO biểu diễn 1 node trong cây outline của lộ trình học.</summary>
public sealed class PathItemDto
{
    public int Id { get; set; }
    public int PathId { get; set; }
    public int? ParentId { get; set; }
    public PathItemType ItemType { get; set; } = PathItemType.Theory;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? LessonId { get; set; }
    public int? FinalTestId { get; set; }
    public int? LabExerciseId { get; set; }
    public int SortOrder { get; set; }

    /// <summary>Exercise ID tiện ích cho Quiz hoặc Lab.</summary>
    public int? ExerciseId => FinalTestId ?? LabExerciseId;

    /// <summary>Danh sách con lồng nhau.</summary>
    public List<PathItemDto> Children { get; set; } = [];

    /// <summary>Payload bài tập (cho Quiz/Lab khi xem chi tiết hoặc sau khi tạo).</summary>
    public ExerciseDto? Exercise { get; set; }

    /// <summary>Payload bài học (cho Theory khi xem chi tiết).</summary>
    public LessonDto? Lesson { get; set; }
}

public sealed class PathItemCreateRequest
{
    public PathItemType ItemType { get; set; } = PathItemType.Theory;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public int? LessonId { get; set; }
    public int? SortOrder { get; set; }
}

public sealed class PathItemUpdateRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class PathItemMoveRequest
{
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
}
