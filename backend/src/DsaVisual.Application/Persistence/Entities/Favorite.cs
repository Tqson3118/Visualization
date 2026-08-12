namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Yêu thích mô phỏng — SDD §7.3.11. UNIQUE (UserId, SimulationKey).</summary>
public sealed class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SimulationKey { get; set; } = string.Empty;
    public string? InputJson { get; set; }
    public DateTime CreatedAt { get; set; }
}
