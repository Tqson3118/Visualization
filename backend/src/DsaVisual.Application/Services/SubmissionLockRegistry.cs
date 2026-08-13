using System.Collections.Concurrent;

namespace DsaVisual.Application.Services;

/// <summary>
/// Khóa đồng bộ nộp bài per (UserId, ExerciseId) — SUBMISSION_IN_PROGRESS (API_REFERENCE.md §6.2).
/// Singleton; single-instance assumption (ghi chú: multi-instance cần row lock DB).
/// <see cref="TryAcquire(int,int,TimeSpan)"/> chờ có giới hạn: request thứ 2 chờ request thứ nhất
/// commit rồi đi vào nhánh idempotent (merge) thay vì trả 422 ngay — cần cho race
/// PASS+FAIL song song (findings-biz #7): pass phải được ghi dù submit bị lock.
/// </summary>
public sealed class SubmissionLockRegistry
{
    private readonly ConcurrentDictionary<(int UserId, int ExerciseId), SemaphoreSlim> _locks = new();

    /// <summary>Giành khóa; trả null nếu đang có bài nộp đồng thời.</summary>
    public IDisposable? TryAcquire(int userId, int exerciseId) =>
        TryAcquire(userId, exerciseId, TimeSpan.Zero);

    /// <summary>Giành khóa, chờ tối đa <paramref name="timeout"/>; trả null nếu vẫn bận.</summary>
    public IDisposable? TryAcquire(int userId, int exerciseId, TimeSpan timeout)
    {
        var semaphore = _locks.GetOrAdd((userId, exerciseId), static _ => new SemaphoreSlim(1, 1));
        if (semaphore.Wait(timeout))
        {
            return new Releaser(semaphore);
        }

        return null;
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }
}
