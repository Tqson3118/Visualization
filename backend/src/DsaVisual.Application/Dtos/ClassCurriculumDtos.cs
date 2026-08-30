namespace DsaVisual.Application.Dtos;

// ── Learning Path / Curriculum per class (API bổ sung cho "Teacher tạo Learning Path") ──

/// <summary>Body cho PUT /classes/{id}/learning-path — gán/đổi lộ trình active cho lớp.</summary>
public sealed class SetClassLearningPathRequest
{
    public int? LearningPathId { get; set; }
}

/// <summary>Body cho PUT /classes/{id}/assignments/deadline — cập nhật deadline cho item theo pathItemId.</summary>
public sealed class UpdateAssignmentDeadlineRequest
{
    public int PathItemId { get; set; }
    public DateTime? DueAt { get; set; }
    public bool AllowLateSubmission { get; set; } = true;
}

/// <summary>Body cho PUT /classes/{id}/assignments/lessons/{lessonId}/deadline — cập nhật deadline bài học.</summary>
public sealed class UpdateLessonDeadlineRequest
{
    public DateTime? DueAt { get; set; }
    public bool AllowLateSubmission { get; set; } = true;
}

/// <summary>Body cho POST /courses/{id}/assign-classes — gán nhanh 1 lộ trình cho nhiều lớp (cửa phụ).</summary>
public sealed class AssignCourseToClassesRequest
{
    public List<int> ClassIds { get; set; } = [];
}

/// <summary>Body cho PUT /classes/{id}/curriculum — set meta + publish/unpublish.</summary>
public sealed class ClassCurriculumUpsertRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    /// <summary>true = publish (học viên thấy), false = draft (ẩn với học viên). Null = giữ nguyên.</summary>
    public bool? Published { get; set; }
}

/// <summary>Body cho PUT /classes/{id}/curriculum/reorder — danh sách thứ tự mới.</summary>
public sealed class ClassCurriculumReorderRequest
{
    public List<ClassCurriculumReorderItem> Items { get; set; } = [];
}

/// <summary>Một mục trong lộ trình (bản gốc ClassAssignment).</summary>
public sealed class ClassCurriculumReorderItem
{
    public int AssignmentId { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>Lộ trình học của lớp cho học viên — GET /classes/{id}/curriculum.</summary>
public sealed class ClassCurriculumDto
{
    public int ClassId { get; set; }
    public int? LearningPathId { get; set; }
    public string? LearningPathTitle { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool Published { get; set; }
    /// <summary>Tỷ lệ % items completed (0-100) — dữ liệu thật theo progress của học viên.</summary>
    public int ProgressPct { get; set; }
    public List<ClassCurriculumItemDto> Items { get; set; } = [];
}

/// <summary>Một item trong lộ trình — status: not_started | in_progress | completed.</summary>
public sealed class ClassCurriculumItemDto
{
    public int AssignmentId { get; set; }
    public int? PathItemId { get; set; }
    public int? ParentId { get; set; }
    public int? LessonId { get; set; }
    public int? ExerciseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>folder | theory | quiz | lab | lesson | codelab | exercise</summary>
    public string ItemType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime? DueAt { get; set; }
    public bool AllowLateSubmission { get; set; } = true;
    public string Status { get; set; } = "not_started";
    public int? BestScore { get; set; }
    public int? TopicId { get; set; }
    public string? TopicName { get; set; }
    public int SimulationCount { get; set; }
    public int XpReward { get; set; }
    public List<ClassCurriculumItemDto> Children { get; set; } = [];
}
