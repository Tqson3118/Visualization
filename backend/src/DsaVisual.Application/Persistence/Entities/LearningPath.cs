namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Lộ trình học (Learning Path) — SDD §7.3.25 (FR-2.10).</summary>
public sealed class LearningPath
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TopicId { get; set; }
    public int SortOrder { get; set; }                               // mở khóa tuần tự 1→5
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public int? AuthorId { get; set; }                               // tác giả khóa (User role Teacher) — trang chi tiết khóa
    public string? HighlightsJson { get; set; }                      // "Why choose" — [{title, description}], tùy biến theo khóa
    public string? TestimonialsJson { get; set; }                    // [{name, role, quote}]
}
