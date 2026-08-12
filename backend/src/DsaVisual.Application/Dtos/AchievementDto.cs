namespace DsaVisual.Application.Dtos;

/// <summary>Huy hiệu — GET /achievements (API_REFERENCE.md §4.12, FR-5.5).
/// EarnedAt != null = đã mở; null = còn ẩn.</summary>
public sealed class AchievementDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public DateTime? EarnedAt { get; set; }
}
