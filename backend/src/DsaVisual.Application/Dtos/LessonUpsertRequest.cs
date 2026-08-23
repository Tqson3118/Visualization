using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Dtos;

/// <summary>Request tạo/cập nhật bài học — API_REFERENCE.md §3.5.</summary>
public sealed class LessonUpsertRequest
{
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContentHtml { get; set; } = string.Empty;
    public LessonStatus Status { get; set; } = LessonStatus.Draft;
    public bool IsClassOnly { get; set; }                            // Chỉ dùng trong Lớp học riêng
    public int SortOrder { get; set; }
    /// <summary>Simulation keys đính kèm (thay thế toàn bộ danh sách cũ).</summary>
    public List<string> SimulationKeys { get; set; } = [];
}
