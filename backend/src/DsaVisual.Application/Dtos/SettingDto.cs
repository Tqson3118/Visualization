namespace DsaVisual.Application.Dtos;

/// <summary>Cấu hình hệ thống — GET/PUT /settings (API_REFERENCE.md §4.10).</summary>
public sealed class SettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>Cập nhật nhiều cấu hình — PUT /settings body: [{key, value}, ...].</summary>
public sealed class SettingsUpdateRequest
{
    public List<SettingDto> Settings { get; set; } = [];
}
