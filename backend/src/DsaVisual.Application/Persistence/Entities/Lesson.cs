namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Bài học — SDD §7.3.2. ContentHtml đã sanitize (Ganss.Xss) trước khi lưu.</summary>
public sealed class Lesson
{
    public int Id { get; set; }
    public int TopicId { get; set; }                                 // FK → Topics.Id
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContentHtml { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public LessonStatus Status { get; set; } = LessonStatus.Draft;
    public int CreatedBy { get; set; }                               // FK → Users.Id — quyền sở hữu Teacher
    public int? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }                         // xóa mềm

    public ICollection<LessonSimulation> LessonSimulations { get; set; } = [];
    public ICollection<Exercise> Exercises { get; set; } = [];
}
