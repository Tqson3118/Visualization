namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Cấu hình hệ thống — SDD §7.3.12. Key UNIQUE (site.name, auth.maxLoginAttempts...).</summary>
public sealed class Setting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UpdatedBy { get; set; }                               // FK → Users.Id
}
