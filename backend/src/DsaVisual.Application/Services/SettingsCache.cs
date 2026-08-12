using System.Collections.Concurrent;
using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Services;

/// <summary>
/// Cache Settings trong bộ nhớ (SDD §5.3.7 — Singleton). Invalidate/upsert khi PUT /settings.
/// </summary>
public sealed class SettingsCache
{
    private readonly ConcurrentDictionary<string, Setting> _settings = new(StringComparer.OrdinalIgnoreCase);

    public void SetAll(IEnumerable<Setting> settings)
    {
        _settings.Clear();
        foreach (var setting in settings)
        {
            _settings[setting.Key] = setting;
        }
    }

    public void Upsert(Setting setting) => _settings[setting.Key] = setting;

    public string? Get(string key) =>
        _settings.TryGetValue(key, out var setting) ? setting.Value : null;

    public bool ContainsKey(string key) => _settings.ContainsKey(key);

    public int Count => _settings.Count;
}
