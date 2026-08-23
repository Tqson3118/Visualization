namespace DsaVisual.Application.Dtos;

/// <summary>Gán nội dung + hạn — POST /classes/{id}/assignments (API_REFERENCE.md §4.11).</summary>
public sealed class ClassAssignmentUpsertRequest
{
    public int? LessonId { get; set; }
    public int? ExerciseId { get; set; }
    public DateTime? DueAt { get; set; }
    public bool AllowLateSubmission { get; set; } = true;   // v2.15
}

/// <summary>Sửa hạn/trạng thái gán — PUT /classes/{id}/assignments/{assignId}.</summary>
public sealed class ClassAssignmentUpdateRequest
{
    public DateTime? DueAt { get; set; }
    public bool? AllowLateSubmission { get; set; }   // v2.15
}
