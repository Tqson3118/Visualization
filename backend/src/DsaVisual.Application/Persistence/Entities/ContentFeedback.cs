namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Phản hồi nội dung — SDD §7.3.21 (FR-7.4). UNIQUE (UserId, LessonId).</summary>
public sealed class ContentFeedback
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }
    public int Rating { get; set; }                                  // 1-5
    public string? Comment { get; set; }                             // ≤ 200 ký tự
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
