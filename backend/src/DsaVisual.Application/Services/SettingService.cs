using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// SettingService thật theo SDD §7.3.12 / API_REFERENCE.md §4.10 (Admin).
/// Cache singleton trong bộ nhớ (SettingsCache); PUT → upsert DB + invalidate cache ngay.
/// </summary>
public sealed class SettingService(
    AppDbContext db,
    SettingsCache cache,
    IDateTimeProvider clock,
    ILogger<SettingService> logger) : ISettingService
{
    public async Task<Result<List<SettingDto>>> GetAllAsync(CancellationToken ct)
    {
        await EnsureLoadedAsync(ct);

        return Result<List<SettingDto>>.Ok(
            db.Settings.AsNoTracking()
                .OrderBy(s => s.Key)
                .Select(s => new SettingDto
                {
                    Key = s.Key,
                    Value = s.Value,
                    Description = s.Description
                })
                .ToList());
    }

    public async Task<Result> UpdateAsync(int userId, SettingsUpdateRequest request, CancellationToken ct)
    {
        if (request.Settings.Count == 0)
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED, "Danh sách cấu hình rỗng", new()
            {
                ["settings"] = ["Danh sách cấu hình rỗng"]
            });
        }

        var now = clock.UtcNow;
        var existing = await db.Settings.ToDictionaryAsync(s => s.Key, ct);

        foreach (var item in request.Settings)
        {
            if (string.IsNullOrWhiteSpace(item.Key))
            {
                continue;
            }

            if (existing.TryGetValue(item.Key, out var setting))
            {
                setting.Value = item.Value;
                setting.Description = item.Description ?? setting.Description;
                setting.UpdatedAt = now;
                setting.UpdatedBy = userId;
            }
            else
            {
                db.Settings.Add(new Setting
                {
                    Key = item.Key,
                    Value = item.Value,
                    Description = item.Description,
                    UpdatedAt = now,
                    UpdatedBy = userId
                });
            }

            // Invalidate/upsert cache ngay (SDD §5.3.7)
            cache.Upsert(new Setting
            {
                Key = item.Key,
                Value = item.Value,
                Description = item.Description,
                UpdatedAt = now,
                UpdatedBy = userId
            });
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Settings updated by user {UserId}: {Keys}", userId,
            string.Join(',', request.Settings.Select(s => s.Key)));
        return Result.Ok();
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken ct)
    {
        if (!cache.ContainsKey(key))
        {
            await EnsureLoadedAsync(ct);
        }

        return cache.Get(key);
    }

    // ── Private ───────────────────────────────────────────────

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (cache.Count > 0)
        {
            return;
        }

        var settings = await db.Settings.AsNoTracking().ToListAsync(ct);
        cache.SetAll(settings);
    }
}
