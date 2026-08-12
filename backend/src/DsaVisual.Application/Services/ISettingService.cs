using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// Cấu hình hệ thống + cache (SDD §5.4/§5.3.7 — SettingsCache Singleton, invalidation khi PUT).
/// </summary>
public interface ISettingService
{
    Task<Result<List<SettingDto>>> GetAllAsync(CancellationToken ct);
    Task<Result> UpdateAsync(int userId, SettingsUpdateRequest request, CancellationToken ct);
    Task<string?> GetValueAsync(string key, CancellationToken ct);
}
