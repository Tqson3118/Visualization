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
/// GET/PUT /settings trao đổi SystemSettingsDto (object — shape FE), map từ bảng Settings key-value.
/// </summary>
public sealed class SettingService(
    AppDbContext db,
    SettingsCache cache,
    IDateTimeProvider clock,
    ILogger<SettingService> logger) : ISettingService
{
    // Key cấu hình thật trong bảng Settings (SDD §7.5) — map sang SystemSettingsDto.
    private const string KeySiteName = "site.name";
    private const string KeyAllowedDomains = "allowed.email.domains";
    private const string KeyPasswordMinLength = "password.policy.minLength";
    private const string KeyUploadMaxSizeMb = "upload.maxSizeMb";

    public async Task<Result<SystemSettingsDto>> GetAllAsync(CancellationToken ct)
    {
        var rows = await db.Settings.AsNoTracking().ToListAsync(ct);
        var byKey = rows.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);

        var settings = new SystemSettingsDto
        {
            SiteName = GetValue(byKey, KeySiteName) ?? "DSA Visual",
            AllowedDomains = (GetValue(byKey, KeyAllowedDomains) ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList(),
            PasswordPolicy = new PasswordPolicySettingsDto
            {
                MinLength = ParseInt(GetValue(byKey, KeyPasswordMinLength), 8)
                // RequireUppercase/RequireDigit/RequireSpecial: PasswordPolicy áp dụng cố định → giữ default true
            },
            UploadMaxMb = ParseInt(GetValue(byKey, KeyUploadMaxSizeMb), 5)
            // SandboxSeconds/SandboxMemoryMb: chạy client-side (ADR-012), backend không có key → default FE
        };

        return Result<SystemSettingsDto>.Ok(settings);
    }

    public async Task<Result> UpdateAsync(int userId, SystemSettingsDto request, CancellationToken ct)
    {
        if (request is null)
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED, "Cấu hình không hợp lệ", new()
            {
                ["settings"] = ["Body phải là đối tượng SystemSettingsDto"]
            });
        }

        var now = clock.UtcNow;
        var existing = await db.Settings.ToDictionaryAsync(s => s.Key, StringComparer.OrdinalIgnoreCase, ct);

        // Chỉ upsert các key có ý nghĩa backend thật; sandboxSeconds/sandboxMemoryMb/passwordPolicy
        // boolean là mặc định client (không có key lưu) → bỏ qua khi PUT.
        var updates = new Dictionary<string, string>
        {
            [KeySiteName] = request.SiteName ?? string.Empty,
            [KeyAllowedDomains] = string.Join(',', request.AllowedDomains ?? []),
            [KeyPasswordMinLength] = (request.PasswordPolicy?.MinLength ?? 8).ToString(),
            [KeyUploadMaxSizeMb] = request.UploadMaxMb.ToString()
        };

        foreach (var (key, value) in updates)
        {
            if (existing.TryGetValue(key, out var setting))
            {
                setting.Value = value;
                setting.UpdatedAt = now;
                setting.UpdatedBy = userId;
            }
            else
            {
                db.Settings.Add(new Setting
                {
                    Key = key,
                    Value = value,
                    UpdatedAt = now,
                    UpdatedBy = userId
                });
            }

            // Invalidate/upsert cache ngay (SDD §5.3.7)
            cache.Upsert(new Setting
            {
                Key = key,
                Value = value,
                UpdatedAt = now,
                UpdatedBy = userId
            });
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Settings updated by user {UserId}: {Keys}", userId,
            string.Join(',', updates.Keys));
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

    private static string? GetValue(Dictionary<string, Setting> byKey, string key) =>
        byKey.TryGetValue(key, out var setting) ? setting.Value : null;

    private static int ParseInt(string? raw, int fallback) =>
        int.TryParse(raw, out var value) ? value : fallback;

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
