namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Ghi chú cá nhân của người học — SDD §7.3.15 (FR-2.6). UNIQUE (UserId, LessonId).</summary>
public sealed class LessonNote
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }
    public string ContentHtml { get; set; } = string.Empty;          // sanitize
    public DateTime UpdatedAt { get; set; }
}
