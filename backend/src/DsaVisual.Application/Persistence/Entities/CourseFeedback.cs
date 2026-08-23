namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Ý kiến học viên gửi giảng viên theo khóa học (lộ trình) — 2 chiều: GV trả lời + cập nhật trạng thái.</summary>
public sealed class CourseFeedback
{
    public int Id { get; set; }
    public int CourseId { get; set; }                                // LearningPath
    public int UserId { get; set; }
    public CourseFeedbackType Type { get; set; } = CourseFeedbackType.Suggestion;
    public string Content { get; set; } = string.Empty;              // ≤ 1000 ký tự
    public CourseFeedbackStatus Status { get; set; } = CourseFeedbackStatus.New;
    public string? ReplyText { get; set; }                           // trả lời của GV
    public int? RepliedById { get; set; }
    public DateTime? RepliedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
