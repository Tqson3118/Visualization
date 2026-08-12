using System.Collections.Concurrent;

namespace DsaVisual.Application.Services;

/// <summary>
/// Khóa đồng thời nộp bài per (UserId, ExerciseId) — SUBMISSION_IN_PROGRESS (API_REFERENCE.md §6.2).
/// Singleton; single-instance assumption (ghi chú: multi-instance cần row lock DB).
/// </summary>
public sealed class SubmissionLockRegistry
{
    private readonly ConcurrentDictionary<(int UserId, int ExerciseId), SemaphoreSlim> _locks = new();

    /// <summary>Giành khóa; trả null nếu đang có bài nộp đồng thời.</summary>
    public IDisposable? TryAcquire(int userId, int exerciseId)
    {
        var semaphore = _locks.GetOrAdd((userId, exerciseId), static _ => new SemaphoreSlim(1, 1));
        if (semaphore.Wait(0))
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
