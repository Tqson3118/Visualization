namespace DsaVisual.Application.Dtos;

/// <summary>Yêu thích mô phỏng — API_REFERENCE.md §4.9.</summary>
public sealed class FavoriteDto
{
    public int Id { get; set; }
    public string SimulationKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? DataStructure { get; set; }
    public string? Input { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Thêm yêu thích — POST /favorites {simKey, input?} (API_REFERENCE.md §4.9).</summary>
public sealed class FavoriteUpsertRequest
{
    public string SimKey { get; set; } = string.Empty;
    public string? Input { get; set; }
}
