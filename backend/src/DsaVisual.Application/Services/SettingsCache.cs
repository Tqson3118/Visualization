using System.Collections.Concurrent;
using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Services;

/// <summary>
/// Cache Settings trong bộ nhớ (SDD §5.3.7 — Singleton). Invalidate/upsert khi PUT /settings.
/// GIỚI HẠN (finding biz#17b — đã chấp nhận, đồ án chạy 1 instance như SubmissionLockRegistry):
/// cache in-process per-instance → PUT ở instance A không invalidate instance B; multi-instance
/// cần invalidate qua signal (Redis pub/sub / DB version stamp) — xem docs/work/backend-audit/notes.md.
/// </summary>
public sealed class SettingsCache
{
    private readonly ConcurrentDictionary<string, Setting> _settings = new(StringComparer.OrdinalIgnoreCase);

    // Khóa nạp lần đầu — đặt TRONG cache (singleton) vì SettingService là Scoped: mỗi request 1 service,
    // nhưng cache dùng chung mọi request → lock ở đây mới chặn được 2 request cùng load (finding biz#17a).
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public void SetAll(IEnumerable<Setting> settings)
    {
        _settings.Clear();
        foreach (var setting in settings)
        {
            _settings[setting.Key] = setting;
        }
    }

    /// <summary>
    /// Nạp cache từ DB đúng 1 lần (double-checked + async lock): nhiều caller đồng thời khi cache trống
    /// chỉ 1 caller thực sự query DB, các caller còn lại chờ rồi thấy cache đã nạp (finding biz#17a).
    /// </summary>
    public async Task LoadOnceAsync(Func<CancellationToken, Task<IReadOnlyList<Setting>>> loader, CancellationToken ct)
    {
        if (_settings.Count > 0)
        {
            return;
        }

        await _loadLock.WaitAsync(ct);
        try
        {
            if (_settings.Count > 0)
            {
                return; // caller khác đã nạp xong trong lúc chờ lock
            }

            SetAll(await loader(ct));
        }
        finally
        {
            _loadLock.Release();
        }
    }

    public void Upsert(Setting setting) => _settings[setting.Key] = setting;

    public string? Get(string key) =>
        _settings.TryGetValue(key, out var setting) ? setting.Value : null;

    public bool ContainsKey(string key) => _settings.ContainsKey(key);

    public int Count => _settings.Count;
}
