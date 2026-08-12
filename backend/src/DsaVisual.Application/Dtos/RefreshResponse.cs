using System.Text.Json.Serialization;

namespace DsaVisual.Application.Dtos;

/// <summary>Response đăng nhập / refresh — API_REFERENCE.md §3.2: { accessToken, expiresIn, user }.
/// RefreshToken chỉ truyền qua cookie HttpOnly — đánh dấu JsonIgnore để không lộ trong body.</summary>
public sealed class RefreshResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }                 // giây
    public UserSummary User { get; set; } = new();

    [JsonIgnore]
    public string? RefreshToken { get; set; }          // dùng để set cookie ở Controller
}
