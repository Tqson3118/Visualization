namespace DsaVisual.Application.Dtos;

/// <summary>Người dùng trong quản trị — GET /users (API_REFERENCE.md §4.8, kèm isActive).</summary>
public sealed class AdminUserDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
