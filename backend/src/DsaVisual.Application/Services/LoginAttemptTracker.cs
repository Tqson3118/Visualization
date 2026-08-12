using System.Collections.Concurrent;
using DsaVisual.Application.Common;
using Microsoft.Extensions.Configuration;

namespace DsaVisual.Application.Services;

/// <summary>
/// Theo dõi đăng nhập thất bại trong bộ nhớ — ACCOUNT_LOCKED sau 5 lần / 15 phút (API_REFERENCE.md §1.5, SDD §5.4).
/// Giới hạn/lockout từ config <c>DSA:Auth:MaxLoginAttempts</c> / <c>DSA:Auth:LockoutMinutes</c>.
/// Ghi chú: single-instance assumption — multi-instance cần lưu Redis/DB (NFR-12).
/// </summary>
public sealed class LoginAttemptTracker(IDateTimeProvider clock, IConfiguration config)
{
    private readonly ConcurrentDictionary<int, List<DateTime>> _failed = new();
    private readonly object _lock = new();

    private int MaxAttempts => config.GetValue("DSA:Auth:MaxLoginAttempts", 5);
    private TimeSpan LockoutWindow => TimeSpan.FromMinutes(config.GetValue("DSA:Auth:LockoutMinutes", 15));

    public bool IsLocked(int userId)
    {
        lock (_lock)
        {
            if (!_failed.TryGetValue(userId, out var attempts))
            {
                return false;
            }

            Prune(attempts);
            return attempts.Count >= MaxAttempts;
        }
    }

    public void RecordFailure(int userId)
    {
        lock (_lock)
        {
            var attempts = _failed.GetOrAdd(userId, static _ => []);
            Prune(attempts);
            attempts.Add(clock.UtcNow);
        }
    }

    public void Reset(int userId)
    {
        lock (_lock)
        {
            _failed.TryRemove(userId, out _);
        }
    }

    private void Prune(List<DateTime> attempts)
    {
        var cutoff = clock.UtcNow - LockoutWindow;
        attempts.RemoveAll(a => a < cutoff);
    }
}
