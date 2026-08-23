namespace DsaVisual.Application.Dtos;

/// <summary>Cập nhật hồ sơ bản thân — PUT /auth/me (API_REFERENCE.md §4.1).</summary>
public sealed class UpdateProfileRequest
{
    public string? DisplayName { get; set; }    // 2-100
    public string? AvatarUrl { get; set; }      // ≤ 500
}
